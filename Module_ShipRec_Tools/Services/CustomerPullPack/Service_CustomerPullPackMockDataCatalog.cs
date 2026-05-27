using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

/// <summary>
/// Loads Customer Pull n' Pack-specific mock data from module-owned JSON assets.
/// </summary>
public sealed class Service_CustomerPullPackMockDataCatalog
    : IService_CustomerPullPackMockDataCatalog
{
    private const string DefaultCatalogPath =
        "Module_ShipRec_Tools/Defaults/customer-pull-pack-mock-data.json";
    private const string DefaultRuntimeCatalogPath =
        "Module_ShipRec_Tools/Defaults/customer-pull-pack-runtime.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly string _catalogPath;
    private readonly string _runtimeCatalogPath;
    private readonly IService_LoggingUtility? _logger;
    private readonly Lazy<Model_CustomerPullPack_MockDataCatalog> _catalog;
    private readonly Lazy<Model_CustomerPullPack_MockDataCatalog> _runtimeCatalog;

    public Service_CustomerPullPackMockDataCatalog(IService_LoggingUtility? logger = null)
        : this(DefaultCatalogPath, DefaultRuntimeCatalogPath, logger) { }

    public Service_CustomerPullPackMockDataCatalog(
        string catalogPath,
        string runtimeCatalogPath,
        IService_LoggingUtility? logger = null
    )
    {
        _catalogPath = catalogPath;
        _runtimeCatalogPath = runtimeCatalogPath;
        _logger = logger;
        _catalog = new Lazy<Model_CustomerPullPack_MockDataCatalog>(LoadCatalog);
        _runtimeCatalog = new Lazy<Model_CustomerPullPack_MockDataCatalog>(LoadRuntimeCatalog);
    }

    public Model_CustomerPullPack_MockDataCatalog GetCatalog()
    {
        var demandRows = GetDemandRows().ToList();
        var locationRows = GetLocationRows().ToList();

        return new Model_CustomerPullPack_MockDataCatalog
        {
            DemandRows = demandRows,
            LocationRows = locationRows,
        };
    }

    public IReadOnlyList<Model_InforVisualCustomerPullPackDemandRow> GetDemandRows()
    {
        var mergedRows = new Dictionary<string, Model_InforVisualCustomerPullPackDemandRow>(
            StringComparer.OrdinalIgnoreCase
        );

        void Merge(IEnumerable<Model_InforVisualCustomerPullPackDemandRow> rows)
        {
            foreach (var row in rows.Select(NormalizeDemandRow))
            {
                if (string.IsNullOrWhiteSpace(row.SourceLineKey))
                {
                    continue;
                }

                mergedRows[row.SourceLineKey] = row;
            }
        }

        Merge(_catalog.Value.DemandRows);
        Merge(_runtimeCatalog.Value.DemandRows);

        return mergedRows
            .Values.OrderBy(row => row.CustomerId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.PullDate)
            .ThenBy(row => row.CustomerOrderId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ParentPartId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> GetLocationRows()
    {
        var mergedRows = new Dictionary<string, Model_InforVisualCustomerPullPackLocationRow>(
            StringComparer.OrdinalIgnoreCase
        );

        void Merge(IEnumerable<Model_InforVisualCustomerPullPackLocationRow> rows)
        {
            foreach (var row in rows.Select(NormalizeLocationRow))
            {
                var key = string.IsNullOrWhiteSpace(row.LocationKey)
                    ? $"{row.SourceLineKey}|{row.LocationId}"
                    : row.LocationKey;

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                mergedRows[key] = row;
            }
        }

        Merge(_catalog.Value.LocationRows);
        Merge(_runtimeCatalog.Value.LocationRows);

        return mergedRows
            .Values.OrderBy(row => row.SourceLineKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private Model_CustomerPullPack_MockDataCatalog LoadCatalog()
    {
        return LoadCatalog(_catalogPath, logWhenMissing: true);
    }

    private Model_CustomerPullPack_MockDataCatalog LoadRuntimeCatalog()
    {
        return LoadCatalog(_runtimeCatalogPath, logWhenMissing: false);
    }

    private Model_CustomerPullPack_MockDataCatalog LoadCatalog(string configuredPath, bool logWhenMissing)
    {
        var resolvedPath = ResolveReadablePath(configuredPath);
        if (resolvedPath is null)
        {
            if (logWhenMissing)
            {
                _logger?.LogWarning(
                    $"Customer Pull n' Pack mock catalog not found for '{configuredPath}'."
                );
            }

            return new Model_CustomerPullPack_MockDataCatalog();
        }

        try
        {
            var json = File.ReadAllText(resolvedPath);
            var catalog = JsonSerializer.Deserialize<Model_CustomerPullPack_MockDataCatalog>(
                json,
                SerializerOptions
            );

            if (catalog is null)
            {
                _logger?.LogWarning(
                    $"Customer Pull n' Pack mock catalog at '{resolvedPath}' was empty or invalid."
                );
                return new Model_CustomerPullPack_MockDataCatalog();
            }

            NormalizeCatalog(catalog);
            return catalog;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Failed to load Customer Pull n' Pack mock catalog '{resolvedPath}': {ex.Message}",
                ex
            );
            return new Model_CustomerPullPack_MockDataCatalog();
        }
    }

    private static void NormalizeCatalog(Model_CustomerPullPack_MockDataCatalog catalog)
    {
        catalog.DemandRows = catalog
            .DemandRows.Select(NormalizeDemandRow)
            .OrderBy(row => row.CustomerId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.PullDate)
            .ThenBy(row => row.CustomerOrderId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ParentPartId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        catalog.LocationRows = catalog
            .LocationRows.Select(NormalizeLocationRow)
            .OrderBy(row => row.SourceLineKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Model_InforVisualCustomerPullPackDemandRow NormalizeDemandRow(
        Model_InforVisualCustomerPullPackDemandRow row
    )
    {
        return new Model_InforVisualCustomerPullPackDemandRow
        {
            SourceLineKey = row.SourceLineKey?.Trim() ?? string.Empty,
            CustomerId = row.CustomerId?.Trim().ToUpperInvariant() ?? string.Empty,
            CustomerName = row.CustomerName?.Trim() ?? string.Empty,
            CustomerOrderId = row.CustomerOrderId?.Trim().ToUpperInvariant() ?? string.Empty,
            ParentPartId = row.ParentPartId?.Trim().ToUpperInvariant() ?? string.Empty,
            SourceLocationId = row.SourceLocationId?.Trim().ToUpperInvariant() ?? string.Empty,
            ShipQuantity = row.ShipQuantity,
            PullDate = row.PullDate,
            QuantityToPack = row.QuantityToPack,
            FgOnHandQuantity = row.FgOnHandQuantity,
            FgLocationId = row.FgLocationId?.Trim().ToUpperInvariant() ?? string.Empty,
            ShortageFlag = row.ShortageFlag,
            LateOrderFlag = row.LateOrderFlag,
            PulledFlag = row.PulledFlag,
            HasLinkedWaitlist = row.HasLinkedWaitlist,
            LinkedWaitlistId = row.LinkedWaitlistId?.Trim() ?? string.Empty,
            LinkedWaitlistStatus = row.LinkedWaitlistStatus?.Trim() ?? string.Empty,
            RequesterNote = row.RequesterNote?.Trim() ?? string.Empty,
            RecheckIndicator = row.RecheckIndicator,
        };
    }

    private static Model_InforVisualCustomerPullPackLocationRow NormalizeLocationRow(
        Model_InforVisualCustomerPullPackLocationRow row
    )
    {
        return new Model_InforVisualCustomerPullPackLocationRow
        {
            LocationKey = row.LocationKey?.Trim() ?? string.Empty,
            SourceLineKey = row.SourceLineKey?.Trim() ?? string.Empty,
            ParentPartId = row.ParentPartId?.Trim().ToUpperInvariant() ?? string.Empty,
            LocationId = row.LocationId?.Trim().ToUpperInvariant() ?? string.Empty,
            DisplayLabel = row.DisplayLabel?.Trim() ?? string.Empty,
            OnHandQuantity = row.OnHandQuantity,
            SourceType = row.SourceType?.Trim() ?? string.Empty,
            InitiallySelected = row.InitiallySelected,
        };
    }

    private static string? ResolveReadablePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return File.Exists(configuredPath) ? configuredPath : null;
        }

        var normalizedRelativePath = configuredPath.Replace('/', Path.DirectorySeparatorChar);
        var directPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, normalizedRelativePath));
        if (File.Exists(directPath))
        {
            return directPath;
        }

        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);
        while (currentDirectory != null)
        {
            var candidate = Path.Combine(currentDirectory.FullName, normalizedRelativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
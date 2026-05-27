using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;

/// <summary>
/// Read-only data access for Customer Pull n' Pack demand.
/// Loads the live Infor Visual demand query and falls back to the JSON-backed mock catalog when mock mode is enabled.
/// </summary>
public class Dao_CustomerPullPackDemand
{
    private const string DemandQueryPath = "CustomerPullPack/01_GetCustomerPullPackDemand.sql";
    private const string DefaultSiteId = "002";
    private const string DefaultWarehouseCode = "002";

    private readonly string _inforVisualConnectionString;
    private readonly IService_AppSettings _appSettings;
    private readonly IService_LoggingUtility? _logger;
    private readonly IService_InforVisualMockDataCatalog _mockDataCatalog;

    private bool UseMockData => _appSettings.GetUseInforVisualMockData();

    /// <summary>
    /// Initializes a new instance of the <see cref="Dao_CustomerPullPackDemand"/> class.
    /// </summary>
    /// <param name="inforVisualConnectionString">Read-only Infor Visual SQL Server connection string.</param>
    /// <param name="appSettings">Application settings service used to detect mock mode.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="mockDataCatalog">Optional mock catalog provider.</param>
    public Dao_CustomerPullPackDemand(
        string inforVisualConnectionString,
        IService_AppSettings appSettings,
        IService_LoggingUtility? logger = null,
        IService_InforVisualMockDataCatalog? mockDataCatalog = null
    )
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        ArgumentNullException.ThrowIfNull(appSettings);

        _inforVisualConnectionString = inforVisualConnectionString;
        _appSettings = appSettings;
        _logger = logger;
        _mockDataCatalog = mockDataCatalog ?? new Service_InforVisualMockDataCatalog(logger);
    }

    /// <summary>
    /// Implements Workflow 2.1: load one-customer demand lines for the Customer Pull n' Pack report surface.
    /// </summary>
    /// <param name="filter">Demand filter defining the selected customer and report constraints.</param>
    /// <returns>Demand lines already mapped into the feature model.</returns>
    public virtual async Task<
        Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>
    > GetDemandAsync(Model_CustomerPullPack_DemandFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (string.IsNullOrWhiteSpace(filter.CustomerId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                "Customer ID is required."
            );
        }

        if (filter.DateTo < filter.DateFrom)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                "DateTo cannot be earlier than DateFrom."
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Loading Customer Pull n' Pack demand for customer '{filter.CustomerId}'."
            );
            return Model_Dao_Result_Factory.Success(CreateMockDemand(filter));
        }

        try
        {
            _logger?.LogInfo(
                $"Loading Customer Pull n' Pack demand for customer '{filter.CustomerId}' from Infor Visual."
            );

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(DemandQueryPath);

            await using var connection = new SqlConnection(_inforVisualConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue(
                "@CustomerId",
                filter.CustomerId.Trim().ToUpperInvariant()
            );
            command.Parameters.AddWithValue("@DateFrom", filter.DateFrom.Date);
            command.Parameters.AddWithValue("@DateTo", filter.DateTo.Date);
            command.Parameters.AddWithValue("@SiteId", DefaultSiteId);
            command.Parameters.AddWithValue("@WarehouseCode", DefaultWarehouseCode);
            command.Parameters.AddWithValue("@PartId", DBNull.Value);
            command.Parameters.AddWithValue("@LocationId", DBNull.Value);
            command.Parameters.AddWithValue("@ShortagesOnly", filter.ShortagesOnly);
            command.Parameters.AddWithValue("@LateOrdersOnly", filter.LateOrdersOnly);
            command.Parameters.AddWithValue("@UnpulledOnly", filter.UnpulledOnly);
            command.Parameters.AddWithValue("@MaxResults", 500);

            var demandLines = new List<Model_CustomerPullPack_DemandLine>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var demandLine = new Model_CustomerPullPack_DemandLine
                {
                    SourceLineKey = ReadString(reader, "SourceLineKey"),
                    CustomerId = ReadString(reader, "CustomerId"),
                    CustomerName = ReadString(reader, "CustomerName"),
                    CustomerOrderId = ReadString(reader, "CustomerOrderId"),
                    ParentPartId = ReadString(reader, "ParentPartId"),
                    SourceLocationId = ReadString(reader, "SourceLocationId"),
                    ShipQuantity = ReadDecimal(reader, "ShipQuantity"),
                    PullDate = ReadDateTime(reader, "PullDate"),
                    QuantityToPack = ReadDecimal(reader, "QtyToPack"),
                    FgOnHandQuantity = ReadDecimal(reader, "FgOnHandQuantity"),
                    FgLocationId = ReadString(reader, "FgLocationId"),
                    ShortageFlag = ReadBoolean(reader, "ShortageFlag"),
                    LateOrderFlag = ReadBoolean(reader, "LateOrderFlag"),
                    PulledFlag = ReadBoolean(reader, "PulledFlag"),
                    HasLinkedWaitlist = ReadBoolean(reader, "HasLinkedWaitlist"),
                    LinkedWaitlistId = ReadString(reader, "LinkedWaitlistId"),
                    RecheckIndicator = false,
                    WaitlistStateDisplay = BuildWaitlistStateDisplay(
                        ReadBoolean(reader, "HasLinkedWaitlist"),
                        ReadString(reader, "LinkedWaitlistStatus"),
                        ReadString(reader, "LinkedWaitlistId")
                    ),
                    SubPartAvailabilitySummary = ReadString(reader, "SubPartAvailabilitySummary"),
                    RequesterNote = string.Empty,
                    LocationOptions = BuildLiveLocationOptions(
                        ReadString(reader, "SourceLineKey"),
                        ReadString(reader, "ParentPartId"),
                        ReadString(reader, "FgLocationId"),
                        ReadDecimal(reader, "FgOnHandQuantity")
                    ),
                };

                demandLines.Add(demandLine);
            }

            return Model_Dao_Result_Factory.Success(
                ApplyLocalFiltersAndSorting(demandLines, filter)
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Failed to load Customer Pull n' Pack demand for customer '{filter.CustomerId}': {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                $"Failed to load Customer Pull n' Pack demand: {ex.Message}",
                ex
            );
        }
    }

    private static void ValidateReadOnlyConnection(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. Current ApplicationIntent: {builder.ApplicationIntent}."
                );
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                "CONSTITUTIONAL VIOLATION: Invalid Infor Visual connection string provided.",
                ex
            );
        }
    }

    private List<Model_CustomerPullPack_DemandLine> CreateMockDemand(
        Model_CustomerPullPack_DemandFilter filter
    )
    {
        var normalizedCustomerId = filter.CustomerId.Trim().ToUpperInvariant();
        var locationRows = _mockDataCatalog.GetCustomerPullPackLocationRows();

        var demandLines = _mockDataCatalog
            .GetCustomerPullPackDemandRows()
            .Where(row =>
                string.Equals(
                    row.CustomerId,
                    normalizedCustomerId,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Where(row =>
                row.PullDate.Date >= filter.DateFrom.Date && row.PullDate.Date <= filter.DateTo.Date
            )
            .Select(row => new Model_CustomerPullPack_DemandLine
            {
                SourceLineKey = row.SourceLineKey,
                CustomerId = row.CustomerId,
                CustomerName = row.CustomerName,
                CustomerOrderId = row.CustomerOrderId,
                ParentPartId = row.ParentPartId,
                SourceLocationId = row.SourceLocationId,
                ShipQuantity = row.ShipQuantity,
                PullDate = row.PullDate,
                QuantityToPack = row.QuantityToPack,
                FgOnHandQuantity = row.FgOnHandQuantity,
                FgLocationId = row.FgLocationId,
                ShortageFlag = row.ShortageFlag,
                LateOrderFlag = row.LateOrderFlag,
                PulledFlag = row.PulledFlag,
                HasLinkedWaitlist = row.HasLinkedWaitlist,
                LinkedWaitlistId = row.LinkedWaitlistId,
                RecheckIndicator = row.RecheckIndicator,
                WaitlistStateDisplay = BuildWaitlistStateDisplay(
                    row.HasLinkedWaitlist,
                    row.LinkedWaitlistStatus,
                    row.LinkedWaitlistId
                ),
                SubPartAvailabilitySummary = BuildMockAvailabilitySummary(
                    row.SourceLineKey,
                    row.ParentPartId,
                    row.FgOnHandQuantity,
                    locationRows
                ),
                RequesterNote = row.RequesterNote,
                LocationOptions = BuildMockLocationOptions(
                    row.SourceLineKey,
                    row.ParentPartId,
                    locationRows
                ),
            })
            .ToList();

        return ApplyLocalFiltersAndSorting(demandLines, filter);
    }

    private List<Model_CustomerPullPack_DemandLine> ApplyLocalFiltersAndSorting(
        IEnumerable<Model_CustomerPullPack_DemandLine> demandLines,
        Model_CustomerPullPack_DemandFilter filter
    )
    {
        IEnumerable<Model_CustomerPullPack_DemandLine> filtered = demandLines;

        if (filter.LinkedWaitlistOnly)
        {
            filtered = filtered.Where(line => line.HasLinkedWaitlist);
        }

        if (filter.RequesterWorkOnly)
        {
            _logger?.LogWarning(
                "Customer Pull n' Pack requester-only filtering is not yet available in the demand DAO; returning the full result set for this flag."
            );
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var normalizedSearchText = filter.SearchText.Trim();
            filtered = filtered.Where(line => MatchesSearch(line, normalizedSearchText));
        }

        filtered = filter.SortMode switch
        {
            Enum_CustomerPullPackSortMode.ShortageFirst => filtered
                .OrderByDescending(line => line.ShortageFlag)
                .ThenBy(line => line.PullDate)
                .ThenBy(line => line.CustomerOrderId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.ParentPartId, StringComparer.OrdinalIgnoreCase),
            Enum_CustomerPullPackSortMode.Part => filtered
                .OrderBy(line => line.ParentPartId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.PullDate)
                .ThenBy(line => line.CustomerOrderId, StringComparer.OrdinalIgnoreCase),
            _ => filtered
                .OrderBy(line => line.PullDate)
                .ThenBy(line => line.CustomerOrderId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.ParentPartId, StringComparer.OrdinalIgnoreCase),
        };

        return filtered.ToList();
    }

    private static bool MatchesSearch(Model_CustomerPullPack_DemandLine line, string searchText)
    {
        return line.CustomerId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.CustomerOrderId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.ParentPartId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.SourceLocationId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.FgLocationId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.WaitlistStateDisplay.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.SubPartAvailabilitySummary.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase
            )
            || line.RequesterNote.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || line.LocationOptions.Any(option =>
                option.LocationId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || option.DisplayLabel.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            );
    }

    private static List<Model_CustomerPullPack_LocationOption> BuildLiveLocationOptions(
        string sourceLineKey,
        string parentPartId,
        string fgLocationId,
        decimal fgOnHandQuantity
    )
    {
        if (string.IsNullOrWhiteSpace(fgLocationId) || fgOnHandQuantity <= 0)
        {
            return [];
        }

        return
        [
            new Model_CustomerPullPack_LocationOption
            {
                LocationKey = $"{sourceLineKey}|{fgLocationId}",
                SourceLineKey = sourceLineKey,
                ParentPartId = parentPartId,
                LocationId = fgLocationId,
                DisplayLabel = $"{fgLocationId} ({fgOnHandQuantity:0.##})",
                OnHandQuantity = fgOnHandQuantity,
                SourceType = Enum_CustomerPullPackLocationSourceType.FinishedGoodsFallback,
                Selected = false,
            },
        ];
    }

    private static List<Model_CustomerPullPack_LocationOption> BuildMockLocationOptions(
        string sourceLineKey,
        string parentPartId,
        IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> locationRows
    )
    {
        return ResolveMockLocationRows(sourceLineKey, parentPartId, locationRows)
            .OrderBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
            .Select(row => new Model_CustomerPullPack_LocationOption
            {
                LocationKey = row.LocationKey,
                SourceLineKey = sourceLineKey,
                ParentPartId = string.IsNullOrWhiteSpace(row.ParentPartId)
                    ? parentPartId
                    : row.ParentPartId,
                LocationId = row.LocationId,
                DisplayLabel = row.DisplayLabel,
                OnHandQuantity = row.OnHandQuantity,
                SourceType = ParseLocationSourceType(row.SourceType),
                Selected = row.InitiallySelected,
            })
            .ToList();
    }

    private static string BuildMockAvailabilitySummary(
        string sourceLineKey,
        string parentPartId,
        decimal fgOnHandQuantity,
        IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> locationRows
    )
    {
        var matchingLocationCount = ResolveMockLocationRows(
            sourceLineKey,
            parentPartId,
            locationRows
        ).Count;

        return $"{matchingLocationCount} selectable locations / {fgOnHandQuantity:0.##} on hand";
    }

    private static List<Model_InforVisualCustomerPullPackLocationRow> ResolveMockLocationRows(
        string sourceLineKey,
        string parentPartId,
        IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> locationRows
    )
    {
        var sourceLineRows = locationRows
            .Where(row =>
                string.Equals(row.SourceLineKey, sourceLineKey, StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        if (sourceLineRows.Count > 0)
        {
            return sourceLineRows;
        }

        return locationRows
            .Where(row => string.IsNullOrWhiteSpace(row.SourceLineKey))
            .Where(row =>
                string.Equals(row.ParentPartId, parentPartId, StringComparison.OrdinalIgnoreCase)
            )
            .ToList();
    }

    private static Enum_CustomerPullPackLocationSourceType ParseLocationSourceType(
        string sourceType
    )
    {
        return sourceType.Trim().ToUpperInvariant() switch
        {
            "FINISHEDGOODSFALLBACK" =>
                Enum_CustomerPullPackLocationSourceType.FinishedGoodsFallback,
            _ => Enum_CustomerPullPackLocationSourceType.SubPartOnHand,
        };
    }

    private static string BuildWaitlistStateDisplay(
        bool hasLinkedWaitlist,
        string linkedWaitlistStatus,
        string linkedWaitlistId
    )
    {
        if (!hasLinkedWaitlist)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(linkedWaitlistId))
        {
            return linkedWaitlistStatus?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(linkedWaitlistStatus))
        {
            return linkedWaitlistId.Trim();
        }

        return $"{linkedWaitlistStatus.Trim()} ({linkedWaitlistId.Trim()})";
    }

    private static string ReadString(SqlDataReader reader, string columnName)
    {
        var value = reader[columnName];
        return value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;
    }

    private static decimal ReadDecimal(SqlDataReader reader, string columnName)
    {
        var value = reader[columnName];
        return value == DBNull.Value ? 0 : Convert.ToDecimal(value);
    }

    private static bool ReadBoolean(SqlDataReader reader, string columnName)
    {
        var value = reader[columnName];
        if (value == DBNull.Value)
        {
            return false;
        }

        return value switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            string stringValue => bool.TryParse(stringValue, out var parsedBool)
                ? parsedBool
                : string.Equals(stringValue, "1", StringComparison.OrdinalIgnoreCase),
            _ => Convert.ToBoolean(value),
        };
    }

    private static DateTime ReadDateTime(SqlDataReader reader, string columnName)
    {
        var value = reader[columnName];
        return value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
    }
}

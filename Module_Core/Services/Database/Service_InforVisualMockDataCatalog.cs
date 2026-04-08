using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Services.Database;

/// <summary>
/// Loads JSON-backed mock data used by Infor Visual integration paths.
/// </summary>
public class Service_InforVisualMockDataCatalog : IService_InforVisualMockDataCatalog
{
    private const string CatalogPath = "Module_Settings.Core/Defaults/inforvisual-mock-data.json";
    private const string RuntimeCatalogPath =
        "Module_Settings.Core/Defaults/inforvisual.mock-runtime.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly IService_LoggingUtility? _logger;
    private readonly Lazy<Model_InforVisualMockDataCatalog> _catalog;
    private readonly Lazy<Model_InforVisualMockDataCatalog> _runtimeCatalog;

    public Service_InforVisualMockDataCatalog(IService_LoggingUtility? logger = null)
    {
        _logger = logger;
        _catalog = new Lazy<Model_InforVisualMockDataCatalog>(LoadCatalog);
        _runtimeCatalog = new Lazy<Model_InforVisualMockDataCatalog>(LoadRuntimeCatalog);
    }

    public Model_InforVisualMockDataCatalog GetCatalog()
    {
        return _catalog.Value;
    }

    public string? GetDefaultPurchaseOrderNumber()
    {
        var catalog = GetCatalog();
        if (!string.IsNullOrWhiteSpace(catalog.DefaultPurchaseOrderNumber))
        {
            return catalog.DefaultPurchaseOrderNumber;
        }

        return catalog.PurchaseOrders.FirstOrDefault()?.PONumber;
    }

    public IReadOnlyList<string> GetLocations()
    {
        return GetCatalog()
            .Locations.Concat(_runtimeCatalog.Value.Locations)
            .Where(location => !string.IsNullOrWhiteSpace(location))
            .Select(location => location.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(location => location, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<Model_InforVisualMockReceivingTransaction> GetReceivingTransactions()
    {
        var mergedTransactions = new Dictionary<string, Model_InforVisualMockReceivingTransaction>(
            StringComparer.OrdinalIgnoreCase
        );
        var anonymousTransactions = new List<Model_InforVisualMockReceivingTransaction>();

        void Merge(IEnumerable<Model_InforVisualMockReceivingTransaction> transactions)
        {
            foreach (var transaction in transactions.Select(NormalizeTransaction))
            {
                if (string.IsNullOrWhiteSpace(transaction.SourceLoadId))
                {
                    anonymousTransactions.Add(transaction);
                    continue;
                }

                mergedTransactions[transaction.SourceLoadId] = transaction;
            }
        }

        Merge(GetCatalog().ReceivingTransactions);
        Merge(_runtimeCatalog.Value.ReceivingTransactions);

        return mergedTransactions
            .Values.Concat(anonymousTransactions)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenBy(transaction => transaction.PartID, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<Model_Dao_Result<int>> AppendReceivingTransactionsAsync(
        IEnumerable<Model_InforVisualMockReceivingTransaction> transactions
    )
    {
        ArgumentNullException.ThrowIfNull(transactions);

        try
        {
            var normalizedTransactions = transactions
                .Where(transaction =>
                    !string.IsNullOrWhiteSpace(transaction.PartID)
                    && !string.IsNullOrWhiteSpace(transaction.PONumber)
                    && !string.IsNullOrWhiteSpace(transaction.CurrentLocationId)
                )
                .Select(NormalizeTransaction)
                .ToList();

            if (normalizedTransactions.Count == 0)
            {
                return Model_Dao_Result_Factory.Success(0, 0);
            }

            var runtimeCatalog = _runtimeCatalog.Value;

            foreach (var transaction in normalizedTransactions)
            {
                var existing = runtimeCatalog.ReceivingTransactions.FirstOrDefault(
                    existingTransaction =>
                        string.Equals(
                            existingTransaction.SourceLoadId,
                            transaction.SourceLoadId,
                            StringComparison.OrdinalIgnoreCase
                        )
                );

                if (existing == null)
                {
                    runtimeCatalog.ReceivingTransactions.Add(transaction);
                }
                else
                {
                    existing.PONumber = transaction.PONumber;
                    existing.PartID = transaction.PartID;
                    existing.POLineNumber = transaction.POLineNumber;
                    existing.Quantity = transaction.Quantity;
                    existing.UnitOfMeasure = transaction.UnitOfMeasure;
                    existing.ReceivedDate = transaction.ReceivedDate;
                    existing.TransactionDate = transaction.TransactionDate;
                    existing.ReceiptWarehouseId = transaction.ReceiptWarehouseId;
                    existing.ReceiptLocationId = transaction.ReceiptLocationId;
                    existing.CurrentWarehouseId = transaction.CurrentWarehouseId;
                    existing.CurrentLocationId = transaction.CurrentLocationId;
                    existing.EmployeeNumber = transaction.EmployeeNumber;
                    existing.UserId = transaction.UserId;
                }

                if (
                    !runtimeCatalog.Locations.Any(location =>
                        string.Equals(
                            location,
                            transaction.CurrentLocationId,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                {
                    runtimeCatalog.Locations.Add(transaction.CurrentLocationId);
                }
            }

            NormalizeCatalog(runtimeCatalog);

            var serializedCatalog = JsonSerializer.Serialize(runtimeCatalog, SerializerOptions);
            foreach (var path in ResolveWritableCatalogPaths(RuntimeCatalogPath))
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(path, serializedCatalog);
            }

            _logger?.LogInfo(
                $"Appended {normalizedTransactions.Count} mock receiving transaction(s) to InforVisual mock catalog."
            );
            return Model_Dao_Result_Factory.Success(
                normalizedTransactions.Count,
                normalizedTransactions.Count
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to append mock receiving transactions: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to append mock receiving transactions: {ex.Message}",
                ex
            );
        }
    }

    private Model_InforVisualMockDataCatalog LoadCatalog()
    {
        return LoadCatalog(CatalogPath, logWhenMissing: true);
    }

    private Model_InforVisualMockDataCatalog LoadRuntimeCatalog()
    {
        return LoadCatalog(RuntimeCatalogPath, logWhenMissing: false);
    }

    private Model_InforVisualMockDataCatalog LoadCatalog(string relativePath, bool logWhenMissing)
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(absolutePath))
        {
            if (logWhenMissing)
            {
                _logger?.LogWarning($"Infor Visual mock catalog not found at '{absolutePath}'.");
            }

            return new Model_InforVisualMockDataCatalog();
        }

        try
        {
            var json = File.ReadAllText(absolutePath);
            var catalog = JsonSerializer.Deserialize<Model_InforVisualMockDataCatalog>(
                json,
                SerializerOptions
            );

            if (catalog is null)
            {
                _logger?.LogWarning(
                    $"Infor Visual mock catalog at '{absolutePath}' was empty or invalid."
                );
                return new Model_InforVisualMockDataCatalog();
            }

            NormalizeCatalog(catalog);

            return catalog;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to load Infor Visual mock catalog: {ex.Message}", ex);
            return new Model_InforVisualMockDataCatalog();
        }
    }

    private static void NormalizeCatalog(Model_InforVisualMockDataCatalog catalog)
    {
        catalog.Locations = catalog
            .Locations.Where(location => !string.IsNullOrWhiteSpace(location))
            .Select(location => location.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(location => location, StringComparer.OrdinalIgnoreCase)
            .ToList();

        catalog.ReceivingTransactions = catalog
            .ReceivingTransactions.Select(NormalizeTransaction)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenBy(transaction => transaction.PartID, StringComparer.OrdinalIgnoreCase)
            .ToList();

        catalog.AssociatedPartRuns = catalog
            .AssociatedPartRuns.Select(NormalizeAssociatedPartRun)
            .OrderBy(run => run.InputPartNumber, StringComparer.OrdinalIgnoreCase)
            .ThenBy(run => run.IsFutureOrTodayRun ? 0 : 1)
            .ThenBy(run => run.NextDueToRunDate ?? DateTime.MaxValue)
            .ThenBy(run => run.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Model_InforVisualMockReceivingTransaction NormalizeTransaction(
        Model_InforVisualMockReceivingTransaction transaction
    )
    {
        return new Model_InforVisualMockReceivingTransaction
        {
            SourceLoadId = transaction.SourceLoadId?.Trim() ?? string.Empty,
            PONumber = transaction.PONumber?.Trim().ToUpperInvariant() ?? string.Empty,
            PartID = transaction.PartID?.Trim().ToUpperInvariant() ?? string.Empty,
            POLineNumber = transaction.POLineNumber?.Trim() ?? string.Empty,
            Quantity = transaction.Quantity,
            UnitOfMeasure = string.IsNullOrWhiteSpace(transaction.UnitOfMeasure)
                ? "EA"
                : transaction.UnitOfMeasure.Trim().ToUpperInvariant(),
            ReceivedDate = transaction.ReceivedDate,
            TransactionDate =
                transaction.TransactionDate == default
                    ? transaction.ReceivedDate
                    : transaction.TransactionDate,
            ReceiptWarehouseId = string.IsNullOrWhiteSpace(transaction.ReceiptWarehouseId)
                ? "002"
                : transaction.ReceiptWarehouseId.Trim().ToUpperInvariant(),
            ReceiptLocationId =
                transaction.ReceiptLocationId?.Trim().ToUpperInvariant() ?? string.Empty,
            CurrentWarehouseId = string.IsNullOrWhiteSpace(transaction.CurrentWarehouseId)
                ? "002"
                : transaction.CurrentWarehouseId.Trim().ToUpperInvariant(),
            CurrentLocationId =
                transaction.CurrentLocationId?.Trim().ToUpperInvariant() ?? string.Empty,
            EmployeeNumber = transaction.EmployeeNumber,
            UserId = transaction.UserId?.Trim() ?? string.Empty,
        };
    }

    private static Model_InforVisualAssociatedPartRunRow NormalizeAssociatedPartRun(
        Model_InforVisualAssociatedPartRunRow row
    )
    {
        return new Model_InforVisualAssociatedPartRunRow
        {
            InputPartNumber = row.InputPartNumber?.Trim().ToUpperInvariant() ?? string.Empty,
            InputPartDescription = row.InputPartDescription?.Trim() ?? string.Empty,
            AssociatedPartNumber =
                row.AssociatedPartNumber?.Trim().ToUpperInvariant() ?? string.Empty,
            AssociatedPartDescription = row.AssociatedPartDescription?.Trim() ?? string.Empty,
            NextDueToRunDate = row.NextDueToRunDate,
            IsFutureOrTodayRun = row.IsFutureOrTodayRun,
            NextDueDateSource = row.NextDueDateSource?.Trim() ?? string.Empty,
            WorkOrderType = row.WorkOrderType?.Trim().ToUpperInvariant() ?? string.Empty,
            WorkOrderBaseId = row.WorkOrderBaseId?.Trim() ?? string.Empty,
            WorkOrderLotId = row.WorkOrderLotId?.Trim() ?? string.Empty,
            WorkOrderSplitId = row.WorkOrderSplitId?.Trim() ?? string.Empty,
            WorkOrderSubId = row.WorkOrderSubId?.Trim() ?? string.Empty,
            OperationSeqNo = row.OperationSeqNo,
            RequirementPieceNo = row.RequirementPieceNo,
            WorkOrderStatus = row.WorkOrderStatus?.Trim().ToUpperInvariant() ?? string.Empty,
            RequirementStatus = row.RequirementStatus?.Trim().ToUpperInvariant() ?? string.Empty,
        };
    }

    private static IReadOnlyList<string> ResolveWritableCatalogPaths(string relativePath)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var outputPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, normalizedRelativePath)
        );
        candidates.Add(outputPath);

        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);
        while (currentDirectory != null)
        {
            var candidate = Path.Combine(currentDirectory.FullName, normalizedRelativePath);
            if (File.Exists(candidate))
            {
                candidates.Add(candidate);
            }

            currentDirectory = currentDirectory.Parent;
        }

        if (candidates.Count == 1)
        {
            var workspaceCandidate = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "..",
                    normalizedRelativePath
                )
            );
            candidates.Add(workspaceCandidate);
        }

        return candidates.ToList();
    }
}

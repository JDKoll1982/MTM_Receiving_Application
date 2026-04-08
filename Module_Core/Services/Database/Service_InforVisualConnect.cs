using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Services.Database;

/// <summary>
/// Service for connecting to and querying Infor Visual database (SQL Server)
/// ⚠️ STRICTLY READ ONLY - NO WRITES ALLOWED TO INFOR VISUAL ⚠️
/// Server: VISUAL, Database: MTMFG, Warehouse: 002
/// </summary>
public class Service_InforVisualConnect : IService_InforVisual
{
    private readonly Dao_InforVisualConnection _dao;
    private readonly IService_AppSettings _appSettings;
    private readonly IService_LoggingUtility? _logger;
    private readonly IService_InforVisualMockDataCatalog _mockDataCatalog;

    private bool UseMockData => _appSettings.GetUseInforVisualMockData();

    public Service_InforVisualConnect(
        Dao_InforVisualConnection dao,
        IService_AppSettings appSettings,
        IService_LoggingUtility? logger = null,
        IService_InforVisualMockDataCatalog? mockDataCatalog = null
    )
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _logger = logger;
        _mockDataCatalog = mockDataCatalog ?? new Service_InforVisualMockDataCatalog(logger);
    }

    #region Connection Testing

    public async Task<bool> TestConnectionAsync()
    {
        if (UseMockData)
        {
            _logger?.LogInfo("[MOCK DATA MODE] Simulating successful Infor Visual connection test");
            return true;
        }

        try
        {
            _logger?.LogInfo("Testing Infor Visual connection...");
            var result = await _dao.TestConnectionAsync();

            if (result.IsSuccess)
            {
                _logger?.LogInfo(
                    $"Infor Visual connection test: {(result.Data ? "SUCCESS" : "FAILED")}"
                );
                return result.Data;
            }

            _logger?.LogError($"Connection test failed: {result.ErrorMessage}");
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Connection test exception: {ex.Message}", ex);
            return false;
        }
    }

    #endregion

    #region Purchase Order Operations

    public async Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(
                "PO number cannot be null or empty"
            );
        }

        // Use the PO number as provided - Infor Visual IDs include the prefix (e.g. "PO-123456")
        string cleanPoNumber = poNumber;

        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock data for PO: {cleanPoNumber}");
            return CreateMockPO(cleanPoNumber);
        }

        try
        {
            _logger?.LogInfo($"Querying Infor Visual for PO: {cleanPoNumber}");

            var result = await _dao.GetPOWithPartsAsync(cleanPoNumber);

            if (!result.IsSuccess)
            {
                _logger?.LogError($"Failed to retrieve PO {cleanPoNumber}: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(result.ErrorMessage);
            }

            if (result.Data == null || result.Data.Count == 0)
            {
                _logger?.LogWarning($"PO {cleanPoNumber} not found");
                return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(null);
            }

            // Convert flat DAO model to hierarchical service model
            var po = ConvertToServiceModel(result.Data);
            _logger?.LogInfo(
                $"Successfully retrieved PO {cleanPoNumber} with {po.Parts.Count} line items"
            );

            return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(po);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error querying PO {cleanPoNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(
                $"Unexpected error: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Part Operations

    public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
    {
        if (string.IsNullOrWhiteSpace(partID))
        {
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(
                "Part ID cannot be null or empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock data for Part: {partID}");
            return CreateMockPart(partID);
        }

        try
        {
            _logger?.LogInfo($"Querying Infor Visual for Part: {partID}");

            var result = await _dao.GetPartByNumberAsync(partID);

            if (!result.IsSuccess)
            {
                _logger?.LogError($"Failed to retrieve Part {partID}: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(
                    result.ErrorMessage
                );
            }

            if (result.Data == null)
            {
                _logger?.LogWarning($"Part {partID} not found");
                return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(null);
            }

            // Convert DAO model to service model
            var part = ConvertPartToServiceModel(result.Data);
            _logger?.LogInfo($"Successfully retrieved Part {partID}");

            return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(part);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error querying Part {partID}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(
                $"Unexpected error: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Quantity Calculations

    public async Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(
        string poNumber,
        string partID,
        DateTime date
    )
    {
        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning 0 for same-day receiving");
            return Model_Dao_Result_Factory.Success<decimal>(0);
        }

        // This functionality requires custom query/stored procedure in Infor Visual
        // Not implemented in current SQL files
        _logger?.LogWarning("Same-day receiving quantity not implemented - returning 0");
        return Model_Dao_Result_Factory.Success<decimal>(0);
    }

    public async Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(
        string poNumber,
        string partID
    )
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return Model_Dao_Result_Factory.Failure<int>("PO number cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(partID))
        {
            return Model_Dao_Result_Factory.Failure<int>("Part ID cannot be null or empty");
        }

        if (UseMockData)
        {
            var matchingLine = _mockDataCatalog
                .GetCatalog()
                .PurchaseOrders.FirstOrDefault(po =>
                    string.Equals(po.PONumber, poNumber, StringComparison.OrdinalIgnoreCase)
                )
                ?.Parts.FirstOrDefault(part =>
                    string.Equals(part.PartID, partID, StringComparison.OrdinalIgnoreCase)
                );

            if (matchingLine == null)
            {
                _logger?.LogWarning($"Mock part {partID} not found on mock PO {poNumber}");
                return Model_Dao_Result_Factory.Failure<int>("Part not found on PO");
            }

            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning mock remaining quantity: {matchingLine.RemainingQuantity}"
            );
            return Model_Dao_Result_Factory.Success<int>(matchingLine.RemainingQuantity);
        }

        try
        {
            _logger?.LogInfo($"Calculating remaining quantity for PO: {poNumber}, Part: {partID}");

            var result = await _dao.GetPOWithPartsAsync(poNumber);

            if (!result.IsSuccess || result.Data == null || result.Data.Count == 0)
            {
                _logger?.LogWarning($"Failed to retrieve PO {poNumber}");
                return Model_Dao_Result_Factory.Failure<int>("PO not found");
            }

            var matchingLine = result.Data.FirstOrDefault(line =>
                line.PartNumber.Equals(partID, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingLine == null)
            {
                _logger?.LogWarning($"Part {partID} not found on PO {poNumber}");
                return Model_Dao_Result_Factory.Failure<int>("Part not found on PO");
            }

            int remaining = (int)matchingLine.RemainingQty;
            _logger?.LogInfo($"Remaining quantity: {remaining}");

            return Model_Dao_Result_Factory.Success<int>(remaining);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Unexpected error: {ex.Message}", ex);
        }
    }

    public async Task<
        Model_Dao_Result<List<Model_InforVisualLocationEvidence>>
    > GetReceivingLocationEvidenceAsync(
        string poNumber,
        string partID,
        string? poLineNumber,
        DateTime? receivedDate
    )
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationEvidence>>(
                "PO number cannot be null or empty"
            );
        }

        if (string.IsNullOrWhiteSpace(partID))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationEvidence>>(
                "Part ID cannot be null or empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning mock receiving location evidence for PO {poNumber}, part {partID}"
            );

            var normalizedPo = poNumber.Trim().ToUpperInvariant();
            var normalizedPart = partID.Trim().ToUpperInvariant();
            var normalizedLine = string.IsNullOrWhiteSpace(poLineNumber)
                ? string.Empty
                : poLineNumber.Trim();
            var matchingTransactions = (_mockDataCatalog.GetReceivingTransactions() ?? [])
                .Where(transaction =>
                    string.Equals(
                        transaction.PONumber,
                        normalizedPo,
                        StringComparison.OrdinalIgnoreCase
                    )
                    && string.Equals(
                        transaction.PartID,
                        normalizedPart,
                        StringComparison.OrdinalIgnoreCase
                    )
                    && (
                        string.IsNullOrWhiteSpace(normalizedLine)
                        || string.Equals(
                            transaction.POLineNumber,
                            normalizedLine,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    && (
                        !receivedDate.HasValue
                        || transaction.ReceivedDate.Date >= receivedDate.Value.Date.AddDays(-2)
                            && transaction.ReceivedDate.Date <= receivedDate.Value.Date.AddDays(2)
                    )
                )
                .OrderByDescending(transaction => transaction.TransactionDate)
                .ToList();

            if (matchingTransactions.Count == 0)
            {
                return Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>()
                );
            }

            var latestTransaction = matchingTransactions[0];
            var receiptCount = matchingTransactions.Count;
            var firstReceivedDate = matchingTransactions.Min(transaction =>
                transaction.ReceivedDate
            );
            var lastReceivedDate = matchingTransactions.Max(transaction =>
                transaction.ReceivedDate
            );

            var evidenceRows = matchingTransactions
                .GroupBy(transaction => new
                {
                    Warehouse = transaction.CurrentWarehouseId,
                    Location = transaction.CurrentLocationId,
                })
                .Select(group =>
                {
                    var latestLocationTransaction = group
                        .OrderByDescending(transaction => transaction.TransactionDate)
                        .ThenByDescending(transaction => transaction.ReceivedDate)
                        .First();

                    return new Model_InforVisualLocationEvidence
                    {
                        CurrentWarehouseId = group.Key.Warehouse,
                        CurrentLocationId = group.Key.Location,
                        CurrentQuantity = group.Sum(transaction => transaction.Quantity),
                        MatchedTransactionQuantity = group.Sum(transaction => transaction.Quantity),
                        MatchedTransactionCount = group.Count(),
                        MatchedTransactionDate = latestLocationTransaction.TransactionDate,
                        MatchedTransactionUserId = latestLocationTransaction.UserId,
                        MatchedTransactionId = null,
                        ReceiptCount = receiptCount,
                        FirstReceivedDate = firstReceivedDate,
                        LastReceivedDate = lastReceivedDate,
                        LatestReceiptWarehouseId = latestTransaction.ReceiptWarehouseId,
                        LatestReceiptLocationId = latestTransaction.ReceiptLocationId,
                        LatestReceiptEvidenceDate = latestTransaction.ReceivedDate,
                        LatestTransactionWarehouseId = latestTransaction.CurrentWarehouseId,
                        LatestTransactionLocationId = latestTransaction.CurrentLocationId,
                        LatestTransactionQuantity = latestTransaction.Quantity,
                        LatestTransactionDate = latestTransaction.TransactionDate,
                        LatestTransactionUserId = latestTransaction.UserId,
                        LatestTransactionId = null,
                    };
                })
                .ToList();

            return Model_Dao_Result_Factory.Success(evidenceRows);
        }

        try
        {
            return await _dao.GetReceivingLocationEvidenceAsync(
                poNumber,
                partID,
                poLineNumber,
                receivedDate
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Unexpected error retrieving receiving location evidence for PO {poNumber}, part {partID}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationEvidence>>(
                $"Unexpected error: {ex.Message}",
                ex
            );
        }
    }

    public async Task<
        Model_Dao_Result<List<Model_InforVisualLocationTransaction>>
    > GetReceivingLocationTransactionHistoryAsync(
        string poNumber,
        string partID,
        string? poLineNumber,
        DateTime? receivedDate
    )
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationTransaction>>(
                "PO number cannot be null or empty"
            );
        }

        if (string.IsNullOrWhiteSpace(partID))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationTransaction>>(
                "Part ID cannot be null or empty"
            );
        }

        if (UseMockData)
        {
            var normalizedPo = poNumber.Trim().ToUpperInvariant();
            var normalizedPart = partID.Trim().ToUpperInvariant();
            var normalizedLine = string.IsNullOrWhiteSpace(poLineNumber)
                ? string.Empty
                : poLineNumber.Trim();

            var transactions = (_mockDataCatalog.GetReceivingTransactions() ?? [])
                .Where(transaction =>
                    string.Equals(
                        transaction.PONumber,
                        normalizedPo,
                        StringComparison.OrdinalIgnoreCase
                    )
                    && string.Equals(
                        transaction.PartID,
                        normalizedPart,
                        StringComparison.OrdinalIgnoreCase
                    )
                    && (
                        string.IsNullOrWhiteSpace(normalizedLine)
                        || string.Equals(
                            transaction.POLineNumber,
                            normalizedLine,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    && (
                        !receivedDate.HasValue
                        || transaction.ReceivedDate.Date >= receivedDate.Value.Date.AddDays(-2)
                            && transaction.ReceivedDate.Date <= receivedDate.Value.Date.AddDays(2)
                    )
                )
                .OrderByDescending(transaction => transaction.TransactionDate)
                .ThenByDescending(transaction => transaction.Quantity)
                .Select(transaction => new Model_InforVisualLocationTransaction
                {
                    SourceLoadId = transaction.SourceLoadId,
                    WarehouseId = transaction.CurrentWarehouseId,
                    LocationId = transaction.CurrentLocationId,
                    Quantity = transaction.Quantity,
                    TransactionDate = transaction.TransactionDate,
                    UserId = transaction.UserId,
                    TransactionId = null,
                    ReceiptWarehouseId = transaction.ReceiptWarehouseId,
                    ReceiptLocationId = transaction.ReceiptLocationId,
                })
                .ToList();

            return Model_Dao_Result_Factory.Success(transactions);
        }

        try
        {
            return await _dao.GetReceivingLocationTransactionHistoryAsync(
                poNumber,
                partID,
                poLineNumber,
                receivedDate
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Unexpected error retrieving receiving location transaction history for PO {poNumber}, part {partID}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationTransaction>>(
                $"Unexpected error: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Helper Methods - Model Conversion

    /// <summary>
    /// Converts flat DAO PO lines to hierarchical service model
    /// </summary>
    /// <param name="poLines"></param>
    private Model_InforVisualPO ConvertToServiceModel(List<Model_InforVisualPOLine> poLines)
    {
        var firstLine = poLines[0];

        return new Model_InforVisualPO
        {
            PONumber = firstLine.PoNumber,
            Vendor = firstLine.VendorName,
            Status = firstLine.PoStatus,
            HeaderPromiseDate = firstLine.HeaderPromiseDate,
            HeaderDesiredReceiveDate = firstLine.HeaderDesiredReceiveDate,
            FreeOnBoard = firstLine.FreeOnBoard,
            Parts = poLines.ConvertAll(line => new Model_InforVisualPart
            {
                PartID = line.PartNumber,
                Description = line.PartDescription,
                POLineNumber = line.PoLine.ToString(),
                PartType = "FG", // Default - could be enhanced with additional query
                DefaultLocationId = line.DefaultLocationId,
                QtyOrdered = (int)line.OrderedQty,
                RemainingQuantity = (int)line.RemainingQty,
                UnitOfMeasure = line.UnitOfMeasure,
                DueDate = line.DueDate,
            }),
        };
    }

    /// <summary>
    /// Converts DAO part model to service model
    /// </summary>
    /// <param name="daoPart"></param>
    private Model_InforVisualPart ConvertPartToServiceModel(Model_InforVisualPartInfo daoPart)
    {
        return new Model_InforVisualPart
        {
            PartID = daoPart.PartNumber,
            Description = daoPart.Description,
            POLineNumber = "N/A",
            PartType = "FG", // Default
            DefaultLocationId = daoPart.DefaultLocationId,
            QtyOrdered = 0,
            RemainingQuantity = (int)daoPart.AvailableQty,
            UnitOfMeasure = daoPart.PrimaryUom,
            RequiresQualityHold = false,
            QualityHoldRestrictionType = string.Empty,
        };
    }

    #endregion

    #region Mock Data Generation

    private Model_Dao_Result<Model_InforVisualPO?> CreateMockPO(string poNumber)
    {
        var match = _mockDataCatalog
            .GetCatalog()
            .PurchaseOrders.FirstOrDefault(po =>
                string.Equals(po.PONumber, poNumber, StringComparison.OrdinalIgnoreCase)
            );

        return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(
            match is null ? null : ClonePurchaseOrder(match)
        );
    }

    private Model_Dao_Result<Model_InforVisualPart?> CreateMockPart(string partID)
    {
        var match = _mockDataCatalog
            .GetCatalog()
            .Parts.FirstOrDefault(part =>
                string.Equals(part.PartID, partID, StringComparison.OrdinalIgnoreCase)
            );

        return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
            match is null ? null : ClonePart(match)
        );
    }

    private Model_Dao_Result<List<Model_OutsideServiceHistory>> CreateMockOutsideServiceHistory(
        string partNumber
    )
    {
        var records = _mockDataCatalog
            .GetCatalog()
            .OutsideServiceHistory.Where(record =>
                string.Equals(record.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase)
            )
            .Select(CloneOutsideServiceHistory)
            .ToList();

        return Model_Dao_Result_Factory.Success(records);
    }

    private Model_Dao_Result<
        List<Model_OutsideServiceHistory>
    > CreateMockOutsideServiceHistoryByVendor(string vendorId)
    {
        var records = _mockDataCatalog
            .GetCatalog()
            .OutsideServiceHistory.Where(record =>
                string.Equals(record.VendorID, vendorId, StringComparison.OrdinalIgnoreCase)
            )
            .Select(CloneOutsideServiceHistory)
            .ToList();

        return Model_Dao_Result_Factory.Success(records);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockFuzzyParts(string term)
    {
        var normalizedTerm = term.Trim();
        var results = _mockDataCatalog
            .GetCatalog()
            .Parts.Where(part =>
                part.PartID.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
                || part.Description.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(part => part.PartID, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .Select(part => new Model_FuzzySearchResult
            {
                Key = part.PartID,
                Label = part.PartID,
                Detail = part.Description,
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(results);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockFuzzyVendors(string term)
    {
        var normalizedTerm = term.Trim();
        var results = _mockDataCatalog
            .GetCatalog()
            .OutsideServiceHistory.Where(record =>
                record.VendorID.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
                || record.VendorName.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            )
            .GroupBy(record => new
            {
                record.VendorID,
                record.VendorName,
                record.VendorCity,
                record.VendorState,
            })
            .OrderBy(group => group.Key.VendorName, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .Select(group => new Model_FuzzySearchResult
            {
                Key = group.Key.VendorID,
                Label = group.Key.VendorName,
                Detail = string.Join(
                    ", ",
                    new[] { group.Key.VendorCity, group.Key.VendorState }.Where(value =>
                        !string.IsNullOrWhiteSpace(value)
                    )
                ),
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(results);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockPartsByVendor(string vendorId)
    {
        var results = _mockDataCatalog
            .GetCatalog()
            .OutsideServiceHistory.Where(record =>
                string.Equals(record.VendorID, vendorId, StringComparison.OrdinalIgnoreCase)
            )
            .GroupBy(record => record.PartNumber ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new Model_FuzzySearchResult
            {
                Key = group.Key,
                Label = group.Key,
                Detail =
                    $"{group.Count()} dispatch(es) — last {group.Max(record => record.DispatchDate):MM/dd/yyyy}",
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(results);
    }

    #endregion

    #region Outside Service Operations

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetOutsideServiceHistoryByPartAsync(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                "Part number cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning mock outside service history for part: {partNumber}"
            );
            return CreateMockOutsideServiceHistory(partNumber);
        }

        return await _dao.GetOutsideServiceHistoryByPartAsync(partNumber);
    }

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetOutsideServiceHistoryByVendorAsync(string vendorId)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                "Vendor ID cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning mock outside service history for vendor: {vendorId}"
            );
            return CreateMockOutsideServiceHistoryByVendor(vendorId);
        }

        return await _dao.GetOutsideServiceHistoryByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(
        string term
    )
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                "Search term cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock fuzzy part results for: {term}");
            return CreateMockFuzzyParts(term);
        }

        return await _dao.FuzzySearchPartsByIdAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPurchaseOrdersByPartAsync(
        string partId
    )
    {
        if (string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                "Part ID cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock purchase orders for part: {partId}");
            return CreateMockPurchaseOrdersByPart(partId);
        }

        return await _dao.GetPurchaseOrdersByPartAsync(partId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchVendorsAsync(
        string term
    )
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                "Search term cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock fuzzy vendor results for: {term}");
            return CreateMockFuzzyVendors(term);
        }

        return await _dao.FuzzySearchVendorsByNameAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(
        string vendorId
    )
    {
        if (string.IsNullOrWhiteSpace(vendorId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                "Vendor ID cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock parts for vendor: {vendorId}");
            return CreateMockPartsByVendor(vendorId);
        }

        return await _dao.GetPartsByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetOutsideServiceHistoryByVendorAndPartAsync(string vendorId, string partNumber)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                "Vendor ID cannot be empty"
            );
        }

        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                "Part number cannot be empty"
            );
        }

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning mock history for vendor {vendorId}, part {partNumber}"
            );
            return CreateMockOutsideServiceHistory(partNumber);
        }

        return await _dao.GetOutsideServiceHistoryByVendorAndPartAsync(vendorId, partNumber);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchLocationsAsync(
        string term,
        string warehouseCode
    )
    {
        if (string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                "Warehouse code cannot be empty"
            );
        }

        var normalizedTerm = term?.Trim() ?? string.Empty;

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning mock location results for term '{normalizedTerm}' in warehouse '{warehouseCode}'"
            );
            return CreateMockFuzzyLocations(normalizedTerm, warehouseCode);
        }

        return await _dao.FuzzySearchLocationsByWarehouseAsync(normalizedTerm, warehouseCode);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<bool>> PartExistsAsync(string partId)
    {
        if (string.IsNullOrWhiteSpace(partId))
            return Model_Dao_Result_Factory.Failure<bool>("Part ID cannot be empty");

        if (UseMockData)
            return Model_Dao_Result_Factory.Success(
                _mockDataCatalog
                    .GetCatalog()
                    .Parts.Any(part =>
                        string.Equals(part.PartID, partId, StringComparison.OrdinalIgnoreCase)
                    )
            );

        return await _dao.PartExistsAsync(partId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<bool>> LocationExistsAsync(
        string locationId,
        string warehouseCode
    )
    {
        if (string.IsNullOrWhiteSpace(locationId))
            return Model_Dao_Result_Factory.Failure<bool>("Location ID cannot be empty");

        if (string.IsNullOrWhiteSpace(warehouseCode))
            return Model_Dao_Result_Factory.Failure<bool>("Warehouse code cannot be empty");

        if (UseMockData)
            return Model_Dao_Result_Factory.Success(
                _mockDataCatalog
                    .GetLocations()
                    .Any(location =>
                        string.Equals(location, locationId, StringComparison.OrdinalIgnoreCase)
                    )
            );

        return await _dao.LocationExistsAsync(locationId, warehouseCode);
    }

    public async Task<
        Model_Dao_Result<List<Model_InforVisualMaterialLocationRow>>
    > GetMaterialAvailabilityCurrentStockAsync(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        if (string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualMaterialLocationRow>>(
                "Warehouse code cannot be empty"
            );
        }

        if (string.IsNullOrWhiteSpace(locationId) && string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualMaterialLocationRow>>(
                "Either a location or part ID is required"
            );
        }

        var normalizedWarehouseCode = warehouseCode.Trim().ToUpperInvariant();
        var normalizedLocationId = NormalizeOptionalInput(locationId);
        var normalizedPartId = NormalizeOptionalInput(partId);

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning material availability current stock for warehouse '{normalizedWarehouseCode}', location '{normalizedLocationId}', part '{normalizedPartId}'"
            );
            return CreateMockMaterialAvailabilityCurrentStock(
                normalizedLocationId,
                normalizedPartId,
                normalizedWarehouseCode
            );
        }

        return await _dao.GetMaterialAvailabilityCurrentStockAsync(
            normalizedLocationId,
            normalizedPartId,
            normalizedWarehouseCode
        );
    }

    public async Task<
        Model_Dao_Result<List<Model_InforVisualIncomingSupplyRow>>
    > GetMaterialAvailabilityIncomingSupplyAsync(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        if (string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualIncomingSupplyRow>>(
                "Warehouse code cannot be empty"
            );
        }

        if (string.IsNullOrWhiteSpace(locationId) && string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualIncomingSupplyRow>>(
                "Either a location or part ID is required"
            );
        }

        var normalizedWarehouseCode = warehouseCode.Trim().ToUpperInvariant();
        var normalizedLocationId = NormalizeOptionalInput(locationId);
        var normalizedPartId = NormalizeOptionalInput(partId);

        if (UseMockData)
        {
            _logger?.LogInfo(
                $"[MOCK DATA MODE] Returning material availability incoming supply for warehouse '{normalizedWarehouseCode}', location '{normalizedLocationId}', part '{normalizedPartId}'"
            );
            return CreateMockMaterialAvailabilityIncomingSupply(
                normalizedLocationId,
                normalizedPartId,
                normalizedWarehouseCode
            );
        }

        return await _dao.GetMaterialAvailabilityIncomingSupplyAsync(
            normalizedLocationId,
            normalizedPartId,
            normalizedWarehouseCode
        );
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockFuzzyLocations(
        string term,
        string warehouseCode
    )
    {
        var normalizedTerm = term.Trim();
        var filteredResults = _mockDataCatalog
            .GetLocations()
            .Where(location =>
                string.IsNullOrWhiteSpace(normalizedTerm)
                || location.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(location => location, StringComparer.OrdinalIgnoreCase)
            .Select(location => new Model_FuzzySearchResult
            {
                Key = location,
                Label = location,
                Detail = $"Warehouse {warehouseCode} — Mock location",
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(filteredResults);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockPurchaseOrdersByPart(
        string partId
    )
    {
        var normalizedPartId = partId.Trim().ToUpperInvariant();
        var results = _mockDataCatalog
            .GetCatalog()
            .PurchaseOrders.Where(po =>
                po.Parts.Any(part =>
                    string.Equals(part.PartID, normalizedPartId, StringComparison.OrdinalIgnoreCase)
                )
            )
            .OrderBy(po => po.PONumber, StringComparer.OrdinalIgnoreCase)
            .Select(po => new Model_FuzzySearchResult
            {
                Key = po.PONumber,
                Label = po.PONumber,
                Detail =
                    $"Vendor: {po.Vendor} | Status: {po.StatusDescription} | Contains {normalizedPartId}",
            })
            .ToList();

        return Model_Dao_Result_Factory.Success(results);
    }

    private Model_Dao_Result<
        List<Model_InforVisualMaterialLocationRow>
    > CreateMockMaterialAvailabilityCurrentStock(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        var catalog = _mockDataCatalog.GetCatalog();
        var transactions = _mockDataCatalog
            .GetReceivingTransactions()
            .Where(transaction =>
                string.Equals(
                    transaction.CurrentWarehouseId,
                    warehouseCode,
                    StringComparison.OrdinalIgnoreCase
                ) && string.IsNullOrWhiteSpace(transaction.CurrentLocationId) is false
            )
            .ToList();

        var requestedPartIds = partId is not null
            ? new[] { partId }
            : transactions
                .Where(transaction =>
                    string.Equals(
                        transaction.CurrentLocationId,
                        locationId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Select(transaction => transaction.PartID)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (requestedPartIds.Length == 0)
        {
            return Model_Dao_Result_Factory.Success(
                new List<Model_InforVisualMaterialLocationRow>()
            );
        }

        var partDescriptions = catalog.Parts.ToDictionary(
            part => part.PartID,
            part => part.Description,
            StringComparer.OrdinalIgnoreCase
        );

        var rows = transactions
            .Where(transaction =>
                requestedPartIds.Contains(transaction.PartID, StringComparer.OrdinalIgnoreCase)
            )
            .GroupBy(transaction => new
            {
                PartID = (transaction.PartID ?? string.Empty).Trim().ToUpperInvariant(),
                CurrentLocationId = (transaction.CurrentLocationId ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant(),
            })
            .Select(group => new Model_InforVisualMaterialLocationRow
            {
                PartId = group.Key.PartID,
                PartDescription = partDescriptions.TryGetValue(
                    group.Key.PartID,
                    out var description
                )
                    ? description
                    : string.Empty,
                WarehouseCode = warehouseCode,
                LocationId = group.Key.CurrentLocationId,
                Quantity = group.Sum(transaction => transaction.Quantity),
                CommittedQuantity = 0,
            })
            .Where(row => row.Quantity > 0)
            .OrderBy(row => row.PartId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Model_Dao_Result_Factory.Success(rows);
    }

    private Model_Dao_Result<
        List<Model_InforVisualIncomingSupplyRow>
    > CreateMockMaterialAvailabilityIncomingSupply(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        var catalog = _mockDataCatalog.GetCatalog();
        var transactions = _mockDataCatalog
            .GetReceivingTransactions()
            .Where(transaction =>
                string.Equals(
                    transaction.CurrentWarehouseId,
                    warehouseCode,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToList();

        var requestedPartIds = partId is not null
            ? new[] { partId }
            : transactions
                .Where(transaction =>
                    string.Equals(
                        transaction.CurrentLocationId,
                        locationId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Select(transaction => transaction.PartID)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (requestedPartIds.Length == 0)
        {
            return Model_Dao_Result_Factory.Success(new List<Model_InforVisualIncomingSupplyRow>());
        }

        var rows = catalog
            .PurchaseOrders.Where(po =>
                string.IsNullOrWhiteSpace(po.Status) is false
                && new[] { "O", "P", "R", "F" }.Contains(
                    po.Status,
                    StringComparer.OrdinalIgnoreCase
                )
            )
            .SelectMany(po => po.Parts, (po, part) => new { PurchaseOrder = po, Part = part })
            .Where(item =>
                requestedPartIds.Contains(item.Part.PartID, StringComparer.OrdinalIgnoreCase)
            )
            .Select(item => new Model_InforVisualIncomingSupplyRow
            {
                PartId = item.Part.PartID,
                PartDescription = item.Part.Description,
                WarehouseCode = warehouseCode,
                PONumber = item.PurchaseOrder.PONumber,
                POLineNumber = item.Part.POLineNumber,
                VendorName = item.PurchaseOrder.Vendor,
                PoStatus = item.PurchaseOrder.Status,
                OrderedQty = item.Part.QtyOrdered,
                ReceivedQty = item.Part.QtyOrdered - item.Part.RemainingQuantity,
                RemainingQty = item.Part.RemainingQuantity,
                LineDesiredReceiveDate = item.Part.DueDate,
                LinePromiseDate = null,
                LineLastReceivedDate = item.PurchaseOrder.IsBlanketOrder ? item.Part.DueDate : null,
                HeaderPromiseDate = item.PurchaseOrder.HeaderPromiseDate,
                HeaderDesiredReceiveDate = item.PurchaseOrder.HeaderDesiredReceiveDate,
                FreeOnBoard = item.PurchaseOrder.FreeOnBoard,
                IsBlanketOrder = item.PurchaseOrder.IsBlanketOrder,
            })
            .OrderBy(row => row.PartId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.PONumber, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.POLineNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Model_Dao_Result_Factory.Success(rows);
    }

    private static string? NormalizeOptionalInput(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }

    private static Model_InforVisualPO ClonePurchaseOrder(Model_InforVisualPO source)
    {
        return new Model_InforVisualPO
        {
            PONumber = source.PONumber,
            Vendor = source.Vendor,
            Status = source.Status,
            HeaderPromiseDate = source.HeaderPromiseDate,
            HeaderDesiredReceiveDate = source.HeaderDesiredReceiveDate,
            FreeOnBoard = source.FreeOnBoard,
            Parts = source.Parts.ConvertAll(ClonePart),
        };
    }

    private static Model_InforVisualPart ClonePart(Model_InforVisualPart source)
    {
        return new Model_InforVisualPart
        {
            PartID = source.PartID,
            POLineNumber = source.POLineNumber,
            PartType = source.PartType,
            QtyOrdered = source.QtyOrdered,
            UnitOfMeasure = source.UnitOfMeasure,
            Description = source.Description,
            DefaultLocationId = source.DefaultLocationId,
            RemainingQuantity = source.RemainingQuantity,
            DueDate = source.DueDate,
            RequiresQualityHold = source.RequiresQualityHold,
            QualityHoldRestrictionType = source.QualityHoldRestrictionType,
        };
    }

    private static Model_OutsideServiceHistory CloneOutsideServiceHistory(
        Model_OutsideServiceHistory source
    )
    {
        return new Model_OutsideServiceHistory
        {
            VendorID = source.VendorID,
            VendorName = source.VendorName,
            VendorCity = source.VendorCity,
            VendorState = source.VendorState,
            DispatchID = source.DispatchID,
            DispatchDate = source.DispatchDate,
            PartNumber = source.PartNumber,
            QuantitySent = source.QuantitySent,
            DispatchStatus = source.DispatchStatus,
        };
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Data.InforVisual;

/// <summary>
/// Data Access Object for Infor Visual database connections and queries
/// WARNING: READ-ONLY ACCESS ONLY - NO WRITES PERMITTED
/// </summary>
public class Dao_InforVisualConnection
{
    private readonly string _connectionString;
    private readonly string _siteId;
    private readonly IService_LoggingUtility? _logger;

    public Dao_InforVisualConnection(
        string connectionString,
        IService_LoggingUtility? logger = null,
        string siteId = "002"
    )
    {
        ValidateReadOnlyConnection(connectionString);
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _siteId = siteId;
        _logger = logger;
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
                    $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. "
                        + $"Current connection string ApplicationIntent is '{builder.ApplicationIntent}'."
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

    #region Connection Management

    /// <summary>
    /// Tests connection to Infor Visual database
    /// </summary>
    public async Task<Model_Dao_Result<bool>> TestConnectionAsync()
    {
        try
        {
            _logger?.LogInfo("Testing Infor Visual connection...");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            bool isConnected = connection.State == ConnectionState.Open;
            _logger?.LogInfo($"Infor Visual connection test result: {isConnected}");
            return Model_Dao_Result_Factory.Success(isConnected);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Connection test failed: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Connection test failed: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Purchase Order Queries

    /// <summary>
    /// Retrieves PO with all line items and parts
    /// Uses: 01_GetPOWithParts.sql
    /// </summary>
    /// <param name="poNumber"></param>
    public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPOWithPartsAsync(
        string poNumber
    )
    {
        try
        {
            _logger?.LogInfo($"Retrieving PO with parts: {poNumber}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("01_GetPOWithParts.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var poLines = new List<Model_InforVisualPOLine>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                poLines.Add(
                    new Model_InforVisualPOLine
                    {
                        PoNumber = reader["PoNumber"].ToString() ?? string.Empty,
                        PoLine = Convert.ToInt32(reader["PoLine"]),
                        PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                        PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                        DefaultLocationId = reader["DefaultLocationId"].ToString() ?? string.Empty,
                        OrderedQty = Convert.ToDecimal(reader["OrderedQty"]),
                        ReceivedQty = Convert.ToDecimal(reader["ReceivedQty"]),
                        RemainingQty = Convert.ToDecimal(reader["RemainingQty"]),
                        UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
                        DueDate = reader["DueDate"] as DateTime?,
                        VendorCode = reader["VendorCode"].ToString() ?? string.Empty,
                        VendorName = reader["VendorName"].ToString() ?? string.Empty,
                        PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
                        SiteId = reader["SiteId"].ToString() ?? "002",
                    }
                );
            }

            _logger?.LogInfo($"Retrieved {poLines.Count} lines for PO {poNumber}");
            return Model_Dao_Result_Factory.Success(poLines);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPOLine>>(
                $"Error retrieving PO {poNumber}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Validates if a PO number exists
    /// Uses: 02_ValidatePONumber.sql
    /// </summary>
    /// <param name="poNumber"></param>
    public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
    {
        try
        {
            _logger?.LogInfo($"Validating PO number: {poNumber}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("02_ValidatePONumber.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var count = (int?)await command.ExecuteScalarAsync() ?? 0;
            bool isValid = count > 0;
            _logger?.LogInfo($"PO validation result for {poNumber}: {isValid}");
            return Model_Dao_Result_Factory.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error validating PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Error validating PO {poNumber}: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Receiving Location Reconciliation Queries

    /// <summary>
    /// Retrieves receipt-history and current-inventory evidence used to infer the current
    /// location for a saved receiving row.
    /// Uses: 16_GetReceivingLocationEvidence.sql
    /// </summary>
    /// <param name="poNumber"></param>
    /// <param name="partNumber"></param>
    /// <param name="poLineNumber"></param>
    /// <param name="receivedDate"></param>
    public async Task<
        Model_Dao_Result<List<Model_InforVisualLocationEvidence>>
    > GetReceivingLocationEvidenceAsync(
        string poNumber,
        string partNumber,
        string? poLineNumber,
        DateTime? receivedDate
    )
    {
        try
        {
            _logger?.LogInfo(
                $"Retrieving receiving location evidence for PO {poNumber}, part {partNumber}, line {poLineNumber ?? "(any)"}"
            );

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "16_GetReceivingLocationEvidence.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);
            command.Parameters.AddWithValue("@PartId", partNumber);
            command.Parameters.AddWithValue(
                "@PoLineNumber",
                string.IsNullOrWhiteSpace(poLineNumber) ? DBNull.Value : poLineNumber
            );
            command.Parameters.AddWithValue("@ReceivedDate", receivedDate ?? (object)DBNull.Value);

            var evidenceRows = new List<Model_InforVisualLocationEvidence>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                evidenceRows.Add(
                    new Model_InforVisualLocationEvidence
                    {
                        CurrentWarehouseId =
                            reader["CurrentWarehouseId"].ToString() ?? string.Empty,
                        CurrentLocationId = reader["CurrentLocationId"].ToString() ?? string.Empty,
                        CurrentQuantity =
                            reader["CurrentQuantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["CurrentQuantity"]),
                        MatchedTransactionQuantity =
                            reader["MatchedTransactionQuantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["MatchedTransactionQuantity"]),
                        MatchedTransactionCount =
                            reader["MatchedTransactionCount"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["MatchedTransactionCount"]),
                        MatchedTransactionDate = reader["MatchedTransactionDate"] as DateTime?,
                        MatchedTransactionUserId =
                            reader["MatchedTransactionUserId"].ToString() ?? string.Empty,
                        MatchedTransactionId =
                            reader["MatchedTransactionId"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["MatchedTransactionId"]),
                        TotalMatchedTransactionQuantity =
                            reader["TotalMatchedTransactionQuantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["TotalMatchedTransactionQuantity"]),
                        TotalMatchedTransactionCount =
                            reader["TotalMatchedTransactionCount"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["TotalMatchedTransactionCount"]),
                        ReceiptCount =
                            reader["ReceiptCount"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["ReceiptCount"]),
                        FirstReceivedDate = reader["FirstReceivedDate"] as DateTime?,
                        LastReceivedDate = reader["LastReceivedDate"] as DateTime?,
                        LatestReceiptWarehouseId =
                            reader["LatestReceiptWarehouseId"].ToString() ?? string.Empty,
                        LatestReceiptLocationId =
                            reader["LatestReceiptLocationId"].ToString() ?? string.Empty,
                        LatestReceiptEvidenceDate =
                            reader["LatestReceiptEvidenceDate"] as DateTime?,
                        LatestTransactionWarehouseId =
                            reader["LatestTransactionWarehouseId"].ToString() ?? string.Empty,
                        LatestTransactionLocationId =
                            reader["LatestTransactionLocationId"].ToString() ?? string.Empty,
                        LatestTransactionQuantity =
                            reader["LatestTransactionQuantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["LatestTransactionQuantity"]),
                        LatestTransactionDate = reader["LatestTransactionDate"] as DateTime?,
                        LatestTransactionUserId =
                            reader["LatestTransactionUserId"].ToString() ?? string.Empty,
                        LatestTransactionId =
                            reader["LatestTransactionId"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["LatestTransactionId"]),
                    }
                );
            }

            return Model_Dao_Result_Factory.Success(evidenceRows);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error retrieving receiving location evidence for PO {poNumber}, part {partNumber}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationEvidence>>(
                $"Error retrieving receiving location evidence: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves ordered PO transaction-history rows used during smart-fix reconciliation.
    /// Uses: 17_GetReceivingLocationTransactionHistory.sql
    /// </summary>
    /// <param name="poNumber"></param>
    /// <param name="partNumber"></param>
    /// <param name="poLineNumber"></param>
    /// <param name="receivedDate"></param>
    public async Task<
        Model_Dao_Result<List<Model_InforVisualLocationTransaction>>
    > GetReceivingLocationTransactionHistoryAsync(
        string poNumber,
        string partNumber,
        string? poLineNumber,
        DateTime? receivedDate
    )
    {
        try
        {
            _logger?.LogInfo(
                $"Retrieving receiving location transaction history for PO {poNumber}, part {partNumber}, line {poLineNumber ?? "(any)"}"
            );

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "17_GetReceivingLocationTransactionHistory.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);
            command.Parameters.AddWithValue("@PartId", partNumber);
            command.Parameters.AddWithValue(
                "@PoLineNumber",
                string.IsNullOrWhiteSpace(poLineNumber) ? DBNull.Value : poLineNumber
            );
            command.Parameters.AddWithValue("@ReceivedDate", receivedDate ?? (object)DBNull.Value);

            var transactions = new List<Model_InforVisualLocationTransaction>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                transactions.Add(
                    new Model_InforVisualLocationTransaction
                    {
                        WarehouseId = reader["WarehouseId"].ToString() ?? string.Empty,
                        LocationId = reader["LocationId"].ToString() ?? string.Empty,
                        Quantity =
                            reader["Quantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["Quantity"]),
                        TransactionDate =
                            reader["TransactionDate"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["TransactionDate"]),
                        UserId = reader["TransactionUserId"].ToString() ?? string.Empty,
                        TransactionId =
                            reader["TransactionId"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["TransactionId"]),
                        ReceiptWarehouseId =
                            reader["ReceiptWarehouseId"].ToString() ?? string.Empty,
                        ReceiptLocationId = reader["ReceiptLocationId"].ToString() ?? string.Empty,
                    }
                );
            }

            return Model_Dao_Result_Factory.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error retrieving receiving location transaction history for PO {poNumber}, part {partNumber}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationTransaction>>(
                $"Error retrieving receiving location transaction history: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Part Queries

    /// <summary>
    /// Retrieves part details by part number
    /// Uses: 03_GetPartByNumber.sql
    /// </summary>
    /// <param name="partNumber"></param>
    public async Task<Model_Dao_Result<Model_InforVisualPartInfo?>> GetPartByNumberAsync(
        string partNumber
    )
    {
        try
        {
            _logger?.LogInfo($"Retrieving part by number: {partNumber}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("03_GetPartByNumber.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartNumber", partNumber);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var part = new Model_InforVisualPartInfo
                {
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    Description = reader["Description"].ToString() ?? string.Empty,
                    UnitCost =
                        reader["UnitCost"] != DBNull.Value
                            ? Convert.ToDecimal(reader["UnitCost"])
                            : 0,
                    PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
                    OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
                    AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
                    AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
                    DefaultSite = reader["DefaultSite"].ToString() ?? string.Empty,
                    DefaultLocationId = reader["DefaultLocationId"].ToString() ?? string.Empty,
                    PartStatus = reader["PartStatus"].ToString() ?? string.Empty,
                    ProductLine = reader["ProductLine"].ToString() ?? string.Empty,
                };
                _logger?.LogInfo($"Found part: {partNumber}");
                return Model_Dao_Result_Factory.Success<Model_InforVisualPartInfo?>(part);
            }

            _logger?.LogWarning($"Part not found: {partNumber}");
            return Model_Dao_Result_Factory.Success<Model_InforVisualPartInfo?>(null);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPartInfo?>(
                $"Error retrieving part {partNumber}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Searches for parts by description pattern
    /// Uses: 04_SearchPartsByDescription.sql
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="maxResults"></param>
    public async Task<
        Model_Dao_Result<List<Model_InforVisualPartInfo>>
    > SearchPartsByDescriptionAsync(string searchTerm, int maxResults = 50)
    {
        try
        {
            _logger?.LogInfo($"Searching parts by description: '{searchTerm}' (Max: {maxResults})");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "04_SearchPartsByDescription.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", searchTerm);
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var parts = new List<Model_InforVisualPartInfo>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                parts.Add(
                    new Model_InforVisualPartInfo
                    {
                        PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                        Description = reader["Description"].ToString() ?? string.Empty,
                        UnitCost =
                            reader["UnitCost"] != DBNull.Value
                                ? Convert.ToDecimal(reader["UnitCost"])
                                : 0,
                        PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
                        OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
                        AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
                        AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
                        DefaultSite = reader["DefaultSite"].ToString() ?? string.Empty,
                        PartStatus = reader["PartStatus"].ToString() ?? string.Empty,
                        ProductLine = reader["ProductLine"].ToString() ?? string.Empty,
                    }
                );
            }

            _logger?.LogInfo($"Found {parts.Count} parts matching '{searchTerm}'");
            return Model_Dao_Result_Factory.Success(parts);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error searching parts: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPartInfo>>(
                $"Error searching parts: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Outside Service Queries

    /// <summary>
    /// Retrieves service dispatch history for a given part number.
    /// Uses: 05_GetOutsideServiceHistoryByPart.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="partNumber">The part ID to search for in SERVICE_DISP_LINE.</param>
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetOutsideServiceHistoryByPartAsync(string partNumber)
    {
        try
        {
            _logger?.LogInfo($"Querying outside service history for part: {partNumber}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "05_GetOutsideServiceHistoryByPart.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartNumber", partNumber);

            var records = new List<Model_OutsideServiceHistory>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(
                    new Model_OutsideServiceHistory
                    {
                        VendorID = reader["VendorID"].ToString() ?? string.Empty,
                        VendorName = reader["VendorName"].ToString() ?? string.Empty,
                        VendorCity = reader["VendorCity"] as string,
                        VendorState = reader["VendorState"] as string,
                        DispatchID = reader["DispatchID"]?.ToString(),
                        DispatchDate = reader["DispatchDate"] as DateTime?,
                        PartNumber = reader["PartNumber"] as string,
                        QuantitySent =
                            reader["QuantitySent"] != DBNull.Value
                                ? Convert.ToDecimal(reader["QuantitySent"])
                                : null,
                        DispatchStatus = reader["DispatchStatus"]?.ToString()?.Trim(),
                    }
                );
            }

            _logger?.LogInfo(
                $"Retrieved {records.Count} outside service records for part {partNumber}"
            );
            return Model_Dao_Result_Factory.Success(records);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error querying outside service history for part {partNumber}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                $"Error querying outside service history: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Fuzzy search for parts whose ID contains <paramref name="term"/> (LIKE '%term%').
    /// Uses: 06_FuzzySearchPartsByID.sql
    /// </summary>
    /// <param name="term">Partial part ID to search for.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsByIdAsync(
        string term,
        int maxResults = 50
    )
    {
        try
        {
            _logger?.LogInfo($"Fuzzy-searching parts by ID: '{term}' (max {maxResults})");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("06_FuzzySearchPartsByID.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Term", $"%{term}%");
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var results = new List<Model_FuzzySearchResult>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var partNumber = reader["PartNumber"].ToString() ?? string.Empty;
                var description = reader["Description"].ToString() ?? string.Empty;
                results.Add(
                    new Model_FuzzySearchResult
                    {
                        Key = partNumber,
                        Label = partNumber,
                        Detail = description,
                    }
                );
            }

            _logger?.LogInfo($"Fuzzy part search found {results.Count} results for '{term}'");
            return Model_Dao_Result_Factory.Success(results);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error fuzzy-searching parts: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                $"Error searching parts: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns purchase orders that contain the specified part number.
    /// Uses: 14_FuzzySearchPOsByPart.sql
    /// </summary>
    /// <param name="partId">Exact part ID to search for.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPurchaseOrdersByPartAsync(
        string partId,
        int maxResults = 50
    )
    {
        try
        {
            _logger?.LogInfo(
                $"Retrieving purchase orders containing part '{partId}' (max {maxResults})"
            );
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("14_FuzzySearchPOsByPart.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartId", partId);
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var results = new List<Model_FuzzySearchResult>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var poNumber = reader["PoNumber"].ToString() ?? string.Empty;
                var vendorName = reader["VendorName"].ToString() ?? string.Empty;
                var poStatus = reader["PoStatus"].ToString() ?? string.Empty;

                results.Add(
                    new Model_FuzzySearchResult
                    {
                        Key = poNumber,
                        Label = poNumber,
                        Detail = $"Vendor: {vendorName} | Status: {poStatus}",
                    }
                );
            }

            _logger?.LogInfo($"Found {results.Count} purchase order matches for part '{partId}'");
            return Model_Dao_Result_Factory.Success(results);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error retrieving purchase orders for part '{partId}': {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                $"Error retrieving purchase orders for part: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Fuzzy search for vendors whose NAME contains <paramref name="term"/> (LIKE '%term%').
    /// Uses: 07_FuzzySearchVendorsByName.sql
    /// </summary>
    /// <param name="term">Partial vendor name to search for.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    public async Task<
        Model_Dao_Result<List<Model_FuzzySearchResult>>
    > FuzzySearchVendorsByNameAsync(string term, int maxResults = 50)
    {
        try
        {
            _logger?.LogInfo($"Fuzzy-searching vendors by name: '{term}' (max {maxResults})");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "07_FuzzySearchVendorsByName.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Term", $"%{term}%");
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var results = new List<Model_FuzzySearchResult>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var vendorId = reader["VendorID"].ToString() ?? string.Empty;
                var vendorName = reader["VendorName"].ToString() ?? string.Empty;
                var city = reader["VendorCity"] as string;
                var state = reader["VendorState"] as string;
                var detail = (city, state) switch
                {
                    ({ } c, { } s) when !string.IsNullOrWhiteSpace(c) => $"{c}, {s}",
                    ({ } c, _) when !string.IsNullOrWhiteSpace(c) => c,
                    _ => null,
                };

                results.Add(
                    new Model_FuzzySearchResult
                    {
                        Key = vendorId,
                        Label = vendorName,
                        Detail = detail,
                    }
                );
            }

            _logger?.LogInfo($"Fuzzy vendor search found {results.Count} results for '{term}'");
            return Model_Dao_Result_Factory.Success(results);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error fuzzy-searching vendors: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                $"Error searching vendors: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves service dispatch history for a given vendor ID.
    /// Uses: 08_GetOutsideServiceHistoryByVendor.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="vendorId">Vendor ID to filter dispatch records by.</param>
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetOutsideServiceHistoryByVendorAsync(string vendorId)
    {
        try
        {
            _logger?.LogInfo($"Querying outside service history for vendor: {vendorId}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "08_GetOutsideServiceHistoryByVendor.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VendorID", vendorId);

            var records = new List<Model_OutsideServiceHistory>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(
                    new Model_OutsideServiceHistory
                    {
                        VendorID = reader["VendorID"].ToString() ?? string.Empty,
                        VendorName = reader["VendorName"].ToString() ?? string.Empty,
                        VendorCity = reader["VendorCity"] as string,
                        VendorState = reader["VendorState"] as string,
                        DispatchID = reader["DispatchID"]?.ToString(),
                        DispatchDate = reader["DispatchDate"] as DateTime?,
                        PartNumber = reader["PartNumber"] as string,
                        QuantitySent =
                            reader["QuantitySent"] != DBNull.Value
                                ? Convert.ToDecimal(reader["QuantitySent"])
                                : null,
                        DispatchStatus = reader["DispatchStatus"]?.ToString()?.Trim(),
                    }
                );
            }

            _logger?.LogInfo(
                $"Retrieved {records.Count} outside service records for vendor {vendorId}"
            );
            return Model_Dao_Result_Factory.Success(records);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error querying outside service history for vendor {vendorId}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                $"Error querying outside service history by vendor: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns all distinct part numbers serviced by a specific vendor,
    /// along with dispatch count and last dispatch date.
    /// Used to populate the part selection picker after vendor confirmation.
    /// Uses: 09_GetDistinctPartsByVendor.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="vendorId">The vendor ID to query parts for.</param>
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(
        string vendorId
    )
    {
        try
        {
            _logger?.LogInfo($"Querying distinct parts for vendor: {vendorId}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "09_GetDistinctPartsByVendor.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VendorID", vendorId);

            var results = new List<Model_FuzzySearchResult>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var partNumber = reader["PartNumber"].ToString() ?? string.Empty;
                var count = Convert.ToInt32(reader["DispatchCount"]);
                var lastDate = reader["LastDispatchDate"] as DateTime?;
                var detail = lastDate.HasValue
                    ? $"{count} dispatch(es) — last {lastDate.Value:MM/dd/yyyy}"
                    : $"{count} dispatch(es)";

                results.Add(
                    new Model_FuzzySearchResult
                    {
                        Key = partNumber,
                        Label = partNumber,
                        Detail = detail,
                    }
                );
            }

            _logger?.LogInfo($"Retrieved {results.Count} distinct parts for vendor {vendorId}");
            return Model_Dao_Result_Factory.Success(results);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error querying parts for vendor {vendorId}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                $"Error querying parts for vendor: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves service dispatch history filtered by both vendor ID and part number.
    /// Uses: 10_GetOutsideServiceByVendorAndPart.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="vendorId">The vendor ID to filter by.</param>
    /// <param name="partNumber">The part number to filter by.</param>
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetOutsideServiceHistoryByVendorAndPartAsync(string vendorId, string partNumber)
    {
        try
        {
            _logger?.LogInfo(
                $"Querying outside service history for vendor {vendorId}, part {partNumber}"
            );
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "10_GetOutsideServiceByVendorAndPart.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VendorID", vendorId);
            command.Parameters.AddWithValue("@PartNumber", partNumber);

            var records = new List<Model_OutsideServiceHistory>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(
                    new Model_OutsideServiceHistory
                    {
                        VendorID = reader["VendorID"].ToString() ?? string.Empty,
                        VendorName = reader["VendorName"].ToString() ?? string.Empty,
                        VendorCity = reader["VendorCity"] as string,
                        VendorState = reader["VendorState"] as string,
                        DispatchID = reader["DispatchID"]?.ToString(),
                        DispatchDate = reader["DispatchDate"] as DateTime?,
                        PartNumber = reader["PartNumber"] as string,
                        QuantitySent =
                            reader["QuantitySent"] != DBNull.Value
                                ? Convert.ToDecimal(reader["QuantitySent"])
                                : null,
                        DispatchStatus = reader["DispatchStatus"]?.ToString()?.Trim(),
                    }
                );
            }

            _logger?.LogInfo(
                $"Retrieved {records.Count} records for vendor {vendorId}, part {partNumber}"
            );
            return Model_Dao_Result_Factory.Success(records);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error querying outside service history for vendor {vendorId}, part {partNumber}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                $"Error querying outside service history by vendor and part: {ex.Message}",
                ex
            );
        }
    }

    #endregion

    #region Location Queries

    /// <summary>
    /// Fuzzy search for warehouse locations whose ID contains <paramref name="term"/> (LIKE '%term%'),
    /// scoped to <paramref name="warehouseCode"/>.
    /// Uses: 11_FuzzySearchLocationsByWarehouse.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="term">Partial location ID entered by the user.</param>
    /// <param name="warehouseCode">Warehouse to restrict results to (e.g. "002").</param>
    /// <param name="maxResults">Maximum rows to return.</param>
    public async Task<
        Model_Dao_Result<List<Model_FuzzySearchResult>>
    > FuzzySearchLocationsByWarehouseAsync(string term, string warehouseCode, int maxResults = 50)
    {
        try
        {
            var rawTerm = term.Trim();
            var normalizedTerm = rawTerm
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .ToUpperInvariant();

            _logger?.LogInfo(
                $"Fuzzy-searching locations in warehouse '{warehouseCode}' for term '{term}' (max {maxResults})"
            );
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "11_FuzzySearchLocationsByWarehouse.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Term", $"%{rawTerm}%");
            command.Parameters.AddWithValue("@NormalizedTerm", $"%{normalizedTerm}%");
            command.Parameters.AddWithValue("@WarehouseCode", warehouseCode);
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var results = new List<Model_FuzzySearchResult>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var locationId = reader["LocationId"].ToString() ?? string.Empty;
                var description = reader["Description"] as string;
                results.Add(
                    new Model_FuzzySearchResult
                    {
                        Key = locationId,
                        Label = locationId,
                        Detail = description,
                    }
                );
            }

            _logger?.LogInfo(
                $"Fuzzy location search found {results.Count} results for '{term}' in warehouse '{warehouseCode}'"
            );
            return Model_Dao_Result_Factory.Success(results);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error fuzzy-searching locations: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                $"Error searching locations: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves current positive-quantity warehouse locations for either one exact part or all parts
    /// currently found in the requested warehouse location.
    /// Uses: 18_GetMaterialAvailabilityCurrentStock.sql
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="partId"></param>
    /// <param name="warehouseCode"></param>
    public async Task<
        Model_Dao_Result<List<Model_InforVisualMaterialLocationRow>>
    > GetMaterialAvailabilityCurrentStockAsync(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        try
        {
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "18_GetMaterialAvailabilityCurrentStock.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@WarehouseCode", warehouseCode);
            command.Parameters.AddWithValue(
                "@LocationId",
                string.IsNullOrWhiteSpace(locationId) ? DBNull.Value : locationId
            );
            command.Parameters.AddWithValue(
                "@PartId",
                string.IsNullOrWhiteSpace(partId) ? DBNull.Value : partId
            );

            var rows = new List<Model_InforVisualMaterialLocationRow>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rows.Add(
                    new Model_InforVisualMaterialLocationRow
                    {
                        PartId = reader["PartId"].ToString() ?? string.Empty,
                        PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                        WarehouseCode = reader["WarehouseCode"].ToString() ?? string.Empty,
                        LocationId = reader["LocationId"].ToString() ?? string.Empty,
                        Quantity =
                            reader["Quantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["Quantity"]),
                        CommittedQuantity =
                            reader["CommittedQuantity"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["CommittedQuantity"]),
                    }
                );
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error retrieving material availability current stock: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualMaterialLocationRow>>(
                $"Error retrieving material availability current stock: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves active inbound PO-line rows for either one exact part or all parts currently found
    /// in the requested warehouse location.
    /// Uses: 19_GetMaterialAvailabilityIncomingSupply.sql
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="partId"></param>
    /// <param name="warehouseCode"></param>
    public async Task<
        Model_Dao_Result<List<Model_InforVisualIncomingSupplyRow>>
    > GetMaterialAvailabilityIncomingSupplyAsync(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        try
        {
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "19_GetMaterialAvailabilityIncomingSupply.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@WarehouseCode", warehouseCode);
            command.Parameters.AddWithValue(
                "@LocationId",
                string.IsNullOrWhiteSpace(locationId) ? DBNull.Value : locationId
            );
            command.Parameters.AddWithValue(
                "@PartId",
                string.IsNullOrWhiteSpace(partId) ? DBNull.Value : partId
            );

            var rows = new List<Model_InforVisualIncomingSupplyRow>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rows.Add(
                    new Model_InforVisualIncomingSupplyRow
                    {
                        PartId = reader["PartId"].ToString() ?? string.Empty,
                        PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                        WarehouseCode = reader["WarehouseCode"].ToString() ?? string.Empty,
                        PONumber = reader["PONumber"].ToString() ?? string.Empty,
                        POLineNumber = reader["POLineNumber"].ToString() ?? string.Empty,
                        VendorName = reader["VendorName"].ToString() ?? string.Empty,
                        PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
                        OrderedQty =
                            reader["OrderedQty"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["OrderedQty"]),
                        ReceivedQty =
                            reader["ReceivedQty"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["ReceivedQty"]),
                        RemainingQty =
                            reader["RemainingQty"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["RemainingQty"]),
                        LineDesiredReceiveDate = reader["LineDesiredReceiveDate"] as DateTime?,
                        LinePromiseDate = reader["LinePromiseDate"] as DateTime?,
                        LinePromiseShipDate = reader["LinePromiseShipDate"] as DateTime?,
                        LineLastReceivedDate = reader["LineLastReceivedDate"] as DateTime?,
                        HeaderPromiseDate = reader["HeaderPromiseDate"] as DateTime?,
                        HeaderPromiseShipDate = reader["HeaderPromiseShipDate"] as DateTime?,
                        HeaderDesiredReceiveDate = reader["HeaderDesiredReceiveDate"] as DateTime?,
                        OrderDate = reader["OrderDate"] as DateTime?,
                        FreeOnBoard = reader["FreeOnBoard"].ToString() ?? string.Empty,
                        IsBlanketOrder =
                            reader["IsBlanketOrder"] != DBNull.Value
                            && Convert.ToBoolean(reader["IsBlanketOrder"]),
                    }
                );
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error retrieving material availability incoming supply: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualIncomingSupplyRow>>(
                $"Error retrieving material availability incoming supply: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves the associated parent/work-order parts that consume the requested component part,
    /// together with the best available next run date.
    /// Uses: 20_GetMaterialAvailabilityAssociatedPartRuns.sql
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="partId"></param>
    /// <param name="warehouseCode"></param>
    public async Task<
        Model_Dao_Result<List<Model_InforVisualAssociatedPartRunRow>>
    > GetMaterialAvailabilityAssociatedPartRunsAsync(
        string? locationId,
        string? partId,
        string warehouseCode
    )
    {
        try
        {
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                "20_GetMaterialAvailabilityAssociatedPartRuns.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@WarehouseCode", warehouseCode);
            command.Parameters.AddWithValue(
                "@LocationId",
                string.IsNullOrWhiteSpace(locationId) ? DBNull.Value : locationId
            );
            command.Parameters.AddWithValue(
                "@PartId",
                string.IsNullOrWhiteSpace(partId) ? DBNull.Value : partId
            );
            command.Parameters.AddWithValue("@MaxResults", 200);

            var rows = new List<Model_InforVisualAssociatedPartRunRow>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rows.Add(
                    new Model_InforVisualAssociatedPartRunRow
                    {
                        InputPartNumber = reader["InputPartNumber"].ToString() ?? string.Empty,
                        InputPartDescription =
                            reader["InputPartDescription"].ToString() ?? string.Empty,
                        ComponentPartNumber =
                            reader["ComponentPartNumber"].ToString() ?? string.Empty,
                        AssociatedPartNumber =
                            reader["AssociatedPartNumber"].ToString() ?? string.Empty,
                        AssociatedPartDescription =
                            reader["AssociatedPartDescription"].ToString() ?? string.Empty,
                        NextDueToRunDate = reader["NextDueToRunDate"] as DateTime?,
                        IsFutureOrTodayRun =
                            reader["IsFutureOrTodayRun"] != DBNull.Value
                            && Convert.ToBoolean(reader["IsFutureOrTodayRun"]),
                        IsCurrentlyRunning =
                            reader["IsCurrentlyRunning"] != DBNull.Value
                            && Convert.ToBoolean(reader["IsCurrentlyRunning"]),
                        IsPastJob =
                            reader["IsPastJob"] != DBNull.Value
                            && Convert.ToBoolean(reader["IsPastJob"]),
                        NextDueDateSource = reader["NextDueDateSource"].ToString() ?? string.Empty,
                        WorkOrderType = reader["WorkOrderType"].ToString() ?? string.Empty,
                        WorkOrderBaseId = reader["WorkOrderBaseId"].ToString() ?? string.Empty,
                        WorkOrderLotId = reader["WorkOrderLotId"].ToString() ?? string.Empty,
                        WorkOrderSplitId = reader["WorkOrderSplitId"].ToString() ?? string.Empty,
                        WorkOrderSubId = reader["WorkOrderSubId"].ToString() ?? string.Empty,
                        WorkOrderStatusEffectiveDate =
                            reader["WorkOrderStatusEffectiveDate"] as DateTime?,
                        SiteId = reader["SiteId"].ToString() ?? string.Empty,
                        OperationSeqNo =
                            reader["OperationSeqNo"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["OperationSeqNo"]),
                        RequirementPieceNo =
                            reader["RequirementPieceNo"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["RequirementPieceNo"]),
                        WorkOrderStatus = reader["WorkOrderStatus"].ToString() ?? string.Empty,
                        RequirementStatus = reader["RequirementStatus"].ToString() ?? string.Empty,
                        ScheduledStartDate = reader["ScheduledStartDate"] as DateTime?,
                        ScheduledFinishDate = reader["ScheduledFinishDate"] as DateTime?,
                        RequiredDate = reader["RequiredDate"] as DateTime?,
                        QtyPer =
                            reader["QtyPer"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["QtyPer"]),
                        FixedQty =
                            reader["FixedQty"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["FixedQty"]),
                        CalcQty =
                            reader["CalcQty"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["CalcQty"]),
                        IssuedQty =
                            reader["IssuedQty"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["IssuedQty"]),
                        AllocatedQty =
                            reader["AllocatedQty"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["AllocatedQty"]),
                        FulfilledQty =
                            reader["FulfilledQty"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["FulfilledQty"]),
                        UsageUm = reader["UsageUm"].ToString() ?? string.Empty,
                        ScrapPercent =
                            reader["ScrapPercent"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["ScrapPercent"]),
                        OperationType = reader["OperationType"].ToString() ?? string.Empty,
                        ResourceId = reader["ResourceId"].ToString() ?? string.Empty,
                        ServiceId = reader["ServiceId"].ToString() ?? string.Empty,
                        OperationWarehouseId =
                            reader["OperationWarehouseId"].ToString() ?? string.Empty,
                        RunQtyPerCycle =
                            reader["RunQtyPerCycle"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["RunQtyPerCycle"]),
                        SetupHours =
                            reader["SetupHours"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["SetupHours"]),
                        RunHours =
                            reader["RunHours"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["RunHours"]),
                        Dimensions = reader["Dimensions"].ToString() ?? string.Empty,
                        DimensionExpression =
                            reader["DimensionExpression"].ToString() ?? string.Empty,
                        Length =
                            reader["Length"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["Length"]),
                        Width =
                            reader["Width"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["Width"]),
                        Height =
                            reader["Height"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["Height"]),
                        DrawingId = reader["DrawingId"].ToString() ?? string.Empty,
                        DrawingRevision = reader["DrawingRevision"].ToString() ?? string.Empty,
                    }
                );
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error retrieving material availability associated part runs: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualAssociatedPartRunRow>>(
                $"Error retrieving material availability associated part runs: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> when a <c>PART</c> row with <c>ID = <paramref name="partId"/></c>
    /// exists in Infor Visual.
    /// Uses: 12_ValidatePartExists.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="partId">Exact part ID to check.</param>
    public async Task<Model_Dao_Result<bool>> PartExistsAsync(string partId)
    {
        try
        {
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("12_ValidatePartExists.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartId", partId);

            var count = (int)(await command.ExecuteScalarAsync() ?? 0);
            return Model_Dao_Result_Factory.Success(count > 0);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error checking part existence for '{partId}': {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Error checking part existence: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> when a <c>LOCATION</c> row with
    /// <c>ID = <paramref name="locationId"/></c> and
    /// <c>WAREHOUSE_ID = <paramref name="warehouseCode"/></c> exists in Infor Visual.
    /// Uses: 13_ValidateLocationExists.sql
    /// ⚠️ READ-ONLY — no writes to Infor Visual.
    /// </summary>
    /// <param name="locationId">Exact location ID to check.</param>
    /// <param name="warehouseCode">Warehouse code that must match (e.g. "002").</param>
    public async Task<Model_Dao_Result<bool>> LocationExistsAsync(
        string locationId,
        string warehouseCode
    )
    {
        try
        {
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
                resourcePath: "13_ValidateLocationExists.sql"
            );

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@LocationId", locationId);
            command.Parameters.AddWithValue("@WarehouseCode", warehouseCode);

            var count = (int)(await command.ExecuteScalarAsync() ?? 0);
            return Model_Dao_Result_Factory.Success(count > 0);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error checking location existence for '{locationId}' in warehouse '{warehouseCode}': {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Error checking location existence: {ex.Message}",
                ex
            );
        }
    }

    #endregion
}

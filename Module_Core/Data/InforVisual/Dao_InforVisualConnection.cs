using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Core.Data.InforVisual;

/// <summary>
/// Data Access Object for Infor Visual database connections and queries
/// âš ï¸ READ-ONLY ACCESS ONLY - NO WRITES PERMITTED âš ï¸
/// </summary>
public class Dao_InforVisualConnection
{
    private readonly string _connectionString;
    private readonly string _siteId;
    private readonly IService_LoggingUtility? _logger;

    public Dao_InforVisualConnection(string connectionString, IService_LoggingUtility? logger = null, string siteId = "002")
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _siteId = siteId;
        _logger = logger;
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
                $"Connection test failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region Purchase Order Queries

    /// <summary>
    /// Retrieves PO with all line items and parts
    /// Uses: 01_GetPOWithParts.sql
    /// </summary>
    /// <param name="poNumber"></param>
    public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetPOWithPartsAsync(string poNumber)
    {
        try
        {
            _logger?.LogInfo($"Retrieving PO with parts: {poNumber}");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("01_GetPOWithParts.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var poLines = new List<Model_InforVisualPO>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                poLines.Add(new Model_InforVisualPO
                {
                    PoNumber = reader["PoNumber"].ToString() ?? string.Empty,
                    PoLine = Convert.ToInt32(reader["PoLine"]),
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                    OrderedQty = Convert.ToDecimal(reader["OrderedQty"]),
                    ReceivedQty = Convert.ToDecimal(reader["ReceivedQty"]),
                    RemainingQty = Convert.ToDecimal(reader["RemainingQty"]),
                    UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
                    DueDate = reader["DueDate"] as DateTime?,
                    VendorCode = reader["VendorCode"].ToString() ?? string.Empty,
                    VendorName = reader["VendorName"].ToString() ?? string.Empty,
                    PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
                    SiteId = reader["SiteId"].ToString() ?? "002"
                });
            }

            _logger?.LogInfo($"Retrieved {poLines.Count} lines for PO {poNumber}");
            return Model_Dao_Result_Factory.Success(poLines);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPO>>(
                $"Error retrieving PO {poNumber}: {ex.Message}", ex);
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
                $"Error validating PO {poNumber}: {ex.Message}", ex);
        }
    }

    #endregion

    #region Part Queries

    /// <summary>
    /// Retrieves part details by part number
    /// Uses: 03_GetPartByNumber.sql
    /// </summary>
    /// <param name="partNumber"></param>
    public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByNumberAsync(string partNumber)
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
                var part = new Model_InforVisualPart
                {
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    Description = reader["Description"].ToString() ?? string.Empty,
                    UnitCost = reader["UnitCost"] != DBNull.Value ? Convert.ToDecimal(reader["UnitCost"]) : 0,
                    PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
                    OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
                    AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
                    AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
                    DefaultSite = reader["DefaultSite"].ToString() ?? string.Empty,
                    PartStatus = reader["PartStatus"].ToString() ?? string.Empty,
                    ProductLine = reader["ProductLine"].ToString() ?? string.Empty
                };
                _logger?.LogInfo($"Found part: {partNumber}");
                return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(part);
            }

            _logger?.LogWarning($"Part not found: {partNumber}");
            return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(null);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(
                $"Error retrieving part {partNumber}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Searches for parts by description pattern
    /// Uses: 04_SearchPartsByDescription.sql
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="maxResults"></param>
    public async Task<Model_Dao_Result<List<Model_InforVisualPart>>> SearchPartsByDescriptionAsync(
        string searchTerm,
        int maxResults = 50)
    {
        try
        {
            _logger?.LogInfo($"Searching parts by description: '{searchTerm}' (Max: {maxResults})");
            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("04_SearchPartsByDescription.sql");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", searchTerm);
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var parts = new List<Model_InforVisualPart>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                parts.Add(new Model_InforVisualPart
                {
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    Description = reader["Description"].ToString() ?? string.Empty,
                    UnitCost = reader["UnitCost"] != DBNull.Value ? Convert.ToDecimal(reader["UnitCost"]) : 0,
                    PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
                    OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
                    AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
                    AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
                    DefaultSite = reader["DefaultSite"].ToString() ?? string.Empty,
                    PartStatus = reader["PartStatus"].ToString() ?? string.Empty,
                    ProductLine = reader["ProductLine"].ToString() ?? string.Empty
                });
            }

            _logger?.LogInfo($"Found {parts.Count} parts matching '{searchTerm}'");
            return Model_Dao_Result_Factory.Success(parts);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error searching parts: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPart>>(
                $"Error searching parts: {ex.Message}", ex);
        }
    }

    #endregion
}


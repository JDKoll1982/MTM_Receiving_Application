using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Data.InforVisual;

public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility? _logger;
    private readonly IConfiguration? _configuration;
    private readonly bool _useMockData;

    public Dao_InforVisualPO(
        string inforVisualConnectionString, 
        IConfiguration? configuration = null,
        IService_LoggingUtility? logger = null)
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        _connectionString = inforVisualConnectionString;
        _configuration = configuration;
        _logger = logger;
        
        // Check if mock data mode is enabled
        _useMockData = _configuration?.GetValue<bool>("InforVisual:UseMockData") ?? false;
        
        if (_useMockData)
        {
            _logger?.LogInfo("[MOCK DATA MODE] Dao_InforVisualPO initialized with mock data enabled");
        }
    }

    private static void ValidateReadOnlyConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. " +
                    $"Current ApplicationIntent: {builder.ApplicationIntent}. " +
                    $"Writing to Infor Visual ERP database is STRICTLY PROHIBITED. " +
                    "See Constitution Principle X: Infor Visual DAO Architecture.");
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Invalid Infor Visual connection string format: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoNumberAsync(string poNumber)
    {
        // Return mock data if enabled
        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA] Returning mock parts for PO: {poNumber}");
            return CreateMockPoData(poNumber);
        }

        try
        {
            _logger?.LogInfo($"Retrieving PO by number: {poNumber}");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("01_GetPOWithParts.sql");

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var list = new List<Model_InforVisualPO>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Model_InforVisualPO
                {
                    PoNumber = reader["PoNumber"].ToString() ?? string.Empty,
                    PoLine = Convert.ToInt32(reader["PoLine"]),
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                    OrderedQty = Convert.ToDecimal(reader["OrderedQty"]),
                    ReceivedQty = Convert.ToDecimal(reader["ReceivedQty"]),
                    RemainingQty = Convert.ToDecimal(reader["RemainingQty"]),
                    UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
                    DueDate = reader["DueDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["DueDate"]),
                    VendorCode = reader["VendorCode"].ToString() ?? string.Empty,
                    VendorName = reader["VendorName"].ToString() ?? string.Empty,
                    PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
                    SiteId = reader["SiteId"].ToString() ?? "002"
                });
            }

            _logger?.LogInfo($"Retrieved {list.Count} lines for PO {poNumber}");
            return Model_Dao_Result_Factory.Success(list);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPO>>(
                $"Error retrieving PO {poNumber}: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
    {
        // Mock data validation - always returns true for default PO
        if (_useMockData)
        {
            bool isValid = poNumber.Equals("PO-066868", StringComparison.OrdinalIgnoreCase) ||
                           poNumber.Equals("066868", StringComparison.OrdinalIgnoreCase);
            _logger?.LogInfo($"[MOCK DATA] PO validation for {poNumber}: {isValid}");
            return await Task.FromResult(Model_Dao_Result_Factory.Success(isValid));
        }

        try
        {
            _logger?.LogInfo($"Validating PO number: {poNumber}");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("02_ValidatePONumber.sql");
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            bool isValid = count > 0;
            _logger?.LogInfo($"PO validation result for {poNumber}: {isValid}");
            return Model_Dao_Result_Factory.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error validating PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Error validating PO {poNumber}: {ex.Message}",
                ex);
        }
    }

    #region Mock Data Generation

    /// <summary>
    /// Creates mock PO data for testing without Infor Visual connection.
    /// Matches old module mock data structure.
    /// </summary>
    private Model_Dao_Result<List<Model_InforVisualPO>> CreateMockPoData(string poNumber)
    {
        var mockParts = new List<Model_InforVisualPO>
        {
            new Model_InforVisualPO
            {
                PoNumber = poNumber,
                PoLine = 1,
                PartNumber = "MMC0001000",
                PartDescription = "Steel Coil - Cold Rolled - 0.080\" x 48\" - Grade A",
                OrderedQty = 2000,
                ReceivedQty = 0,
                RemainingQty = 2000,
                UnitOfMeasure = "LB",
                DueDate = DateTime.Today.AddDays(7),
                VendorCode = "VENDOR01",
                VendorName = "ABC Steel Corporation",
                PoStatus = "Open",
                SiteId = "002"
            },
            new Model_InforVisualPO
            {
                PoNumber = poNumber,
                PoLine = 2,
                PartNumber = "MMF0002500",
                PartDescription = "Steel Sheet - Hot Rolled - 0.125\" x 60\" x 120\"",
                OrderedQty = 1500,
                ReceivedQty = 500,
                RemainingQty = 1000,
                UnitOfMeasure = "LB",
                DueDate = DateTime.Today.AddDays(14),
                VendorCode = "VENDOR01",
                VendorName = "ABC Steel Corporation",
                PoStatus = "Open",
                SiteId = "002"
            },
            new Model_InforVisualPO
            {
                PoNumber = poNumber,
                PoLine = 3,
                PartNumber = "MMS0003750",
                PartDescription = "Structural Steel Beam - I-Beam 10\" x 20'",
                OrderedQty = 500,
                ReceivedQty = 0,
                RemainingQty = 500,
                UnitOfMeasure = "EA",
                DueDate = DateTime.Today.AddDays(21),
                VendorCode = "VENDOR02",
                VendorName = "XYZ Metals Supply",
                PoStatus = "Open",
                SiteId = "002"
            },
            new Model_InforVisualPO
            {
                PoNumber = poNumber,
                PoLine = 4,
                PartNumber = "MMCSR12345",
                PartDescription = "Special Coil - Quality Hold Required - SR Grade",
                OrderedQty = 1000,
                ReceivedQty = 0,
                RemainingQty = 1000,
                UnitOfMeasure = "LB",
                DueDate = DateTime.Today.AddDays(10),
                VendorCode = "VENDOR03",
                VendorName = "Quality Steel Inc",
                PoStatus = "Open",
                SiteId = "002"
            }
        };

        return Model_Dao_Result_Factory.Success(mockParts);
    }

    #endregion
}


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Data.InforVisual;

public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility? _logger;

    public Dao_InforVisualPO(string inforVisualConnectionString, IService_LoggingUtility? logger = null)
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        _connectionString = inforVisualConnectionString;
        _logger = logger;
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
}


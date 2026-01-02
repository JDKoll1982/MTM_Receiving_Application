using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.InforVisual;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Data.InforVisual;

public class Dao_InforVisualPart
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility? _logger;

    public Dao_InforVisualPart(string inforVisualConnectionString, IService_LoggingUtility? logger = null)
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

    public async Task<Model_Dao_Result<Model_InforVisualPart>> GetByPartNumberAsync(string partNumber)
    {
        try
        {
            _logger?.LogInfo($"Retrieving part by number: {partNumber}");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("03_GetPartByNumber.sql");

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartNumber", partNumber);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var part = new Model_InforVisualPart
                {
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    Description = reader["Description"].ToString() ?? string.Empty,
                    PartType = reader["PartType"].ToString() ?? string.Empty,
                    UnitCost = Convert.ToDecimal(reader["UnitCost"]),
                    PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
                    OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
                    AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
                    AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
                    DefaultSite = reader["DefaultSite"].ToString() ?? "002",
                    PartStatus = reader["PartStatus"].ToString() ?? "ACTIVE",
                    ProductLine = reader["ProductLine"].ToString() ?? string.Empty
                };
                _logger?.LogInfo($"Found part: {partNumber}");
                return Model_Dao_Result_Factory.Success(part);
            }

            _logger?.LogWarning($"Part not found: {partNumber}");
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart>("Part not found");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart>(
                $"Error retrieving part {partNumber}: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_InforVisualPart>>> SearchPartsByDescriptionAsync(
        string searchTerm,
        int maxResults = 50)
    {
        try
        {
            _logger?.LogInfo($"Searching parts by description: '{searchTerm}' (Max: {maxResults})");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("04_SearchPartsByDescription.sql");

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", searchTerm);
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var list = new List<Model_InforVisualPart>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Model_InforVisualPart
                {
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    Description = reader["Description"].ToString() ?? string.Empty,
                    PartType = reader["PartType"].ToString() ?? string.Empty,
                    UnitCost = Convert.ToDecimal(reader["UnitCost"]),
                    PrimaryUom = reader["PrimaryUom"].ToString() ?? "EA",
                    OnHandQty = Convert.ToDecimal(reader["OnHandQty"]),
                    AllocatedQty = Convert.ToDecimal(reader["AllocatedQty"]),
                    AvailableQty = Convert.ToDecimal(reader["AvailableQty"]),
                    DefaultSite = reader["DefaultSite"].ToString() ?? "002",
                    PartStatus = reader["PartStatus"].ToString() ?? "ACTIVE",
                    ProductLine = reader["ProductLine"].ToString() ?? string.Empty
                });
            }

            _logger?.LogInfo($"Found {list.Count} parts matching '{searchTerm}'");
            return Model_Dao_Result_Factory.Success(list);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error searching parts: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPart>>(
                $"Error searching parts: {ex.Message}",
                ex);
        }
    }
}

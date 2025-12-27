using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.InforVisual;

namespace MTM_Receiving_Application.Data.InforVisual;

public class Dao_InforVisualPart
{
    private readonly string _connectionString;

    public Dao_InforVisualPart(string inforVisualConnectionString)
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        _connectionString = inforVisualConnectionString;
    }

    private static void ValidateReadOnlyConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    p.part_id AS PartNumber,
                    p.description AS Description,
                    p.part_type AS PartType,
                    p.unit_cost AS UnitCost,
                    p.u_m AS PrimaryUom,
                    COALESCE(inv.on_hand, 0) AS OnHandQty,
                    COALESCE(inv.allocated, 0) AS AllocatedQty,
                    (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
                    p.site_id AS DefaultSite,
                    p.stat AS PartStatus,
                    p.prod_line AS ProductLine
                FROM part p
                LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
                WHERE p.part_id = @PartNumber";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartNumber", partNumber);

            using var reader = await command.ExecuteReaderAsync();
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
                return DaoResultFactory.Success(part);
            }

            return DaoResultFactory.Failure<Model_InforVisualPart>("Part not found");
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<Model_InforVisualPart>(
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT TOP (@MaxResults)
                    p.part_id AS PartNumber,
                    p.description AS Description,
                    p.part_type AS PartType,
                    p.unit_cost AS UnitCost,
                    p.u_m AS PrimaryUom,
                    COALESCE(inv.on_hand, 0) AS OnHandQty,
                    COALESCE(inv.allocated, 0) AS AllocatedQty,
                    (COALESCE(inv.on_hand, 0) - COALESCE(inv.allocated, 0)) AS AvailableQty,
                    p.site_id AS DefaultSite,
                    p.stat AS PartStatus,
                    p.prod_line AS ProductLine
                FROM part p
                LEFT JOIN inventory inv ON p.part_id = inv.part_id AND inv.site_id = '002'
                WHERE p.description LIKE @SearchTerm + '%'
                ORDER BY p.part_id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", searchTerm);
            command.Parameters.AddWithValue("@MaxResults", maxResults);

            var list = new List<Model_InforVisualPart>();
            using var reader = await command.ExecuteReaderAsync();
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

            return DaoResultFactory.Success(list);
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<List<Model_InforVisualPart>>(
                $"Error searching parts: {ex.Message}",
                ex);
        }
    }
}

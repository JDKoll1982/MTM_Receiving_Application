using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data access object for receiving load operations
/// Handles individual load CRUD operations
/// </summary>
public class Dao_Receiving_Repository_ReceivingLoad
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_ReceivingLoad(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Retrieve all loads for a transaction
    /// </summary>
    /// <param name="transactionId"></param>
    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLoad>>> GetLoadsByTransactionIdAsync(
        int transactionId)
    {
        try
        {
            var loads = new List<Model_Receiving_Entity_ReceivingLoad>();

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("sp_Receiving_Load_SelectByTransactionId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@TransactionId", transactionId);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                loads.Add(MapReaderToLoad(reader));
            }

            return Model_Dao_Result_Factory.Success(loads, loads.Count);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_Entity_ReceivingLoad>>(
                $"Error retrieving loads: {ex.Message}");
        }
    }

    private static Model_Receiving_Entity_ReceivingLoad MapReaderToLoad(SqlDataReader reader)
    {
        return new Model_Receiving_Entity_ReceivingLoad
        {
            LoadId = reader.GetInt32(reader.GetOrdinal("load_id")),
            TransactionId = reader.GetInt32(reader.GetOrdinal("transaction_id")),
            LoadNumber = reader.GetInt32(reader.GetOrdinal("load_number")),
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            PartType = reader.IsDBNull(reader.GetOrdinal("part_type")) ? null : reader.GetString(reader.GetOrdinal("part_type")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            UnitOfMeasure = reader.GetString(reader.GetOrdinal("unit_of_measure")),
            HeatLotNumber = reader.IsDBNull(reader.GetOrdinal("heat_lot_number")) ? null : reader.GetString(reader.GetOrdinal("heat_lot_number")),
            PackageType = reader.IsDBNull(reader.GetOrdinal("package_type")) ? null : reader.GetString(reader.GetOrdinal("package_type")),
            PackagesPerLoad = reader.GetInt32(reader.GetOrdinal("packages_per_load")),
            WeightPerPackage = reader.IsDBNull(reader.GetOrdinal("weight_per_package")) ? null : reader.GetDecimal(reader.GetOrdinal("weight_per_package")),
            ReceivingLocation = reader.IsDBNull(reader.GetOrdinal("receiving_location")) ? null : reader.GetString(reader.GetOrdinal("receiving_location")),
            QualityHoldAcknowledged = reader.GetBoolean(reader.GetOrdinal("quality_hold_acknowledged")),
            QualityHoldAcknowledgedAt = reader.IsDBNull(reader.GetOrdinal("quality_hold_acknowledged_at")) ? null : reader.GetDateTime(reader.GetOrdinal("quality_hold_acknowledged_at")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
        };
    }
}

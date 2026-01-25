using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data access object for receiving transaction operations
/// Handles complete transaction and load persistence
/// </summary>
public class Dao_Receiving_Repository_ReceivingTransaction
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_ReceivingTransaction(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Insert a complete receiving transaction with all loads
    /// </summary>
    /// <param name="transaction"></param>
    public async Task<Model_Dao_Result<int>> InsertTransactionAsync(
        Model_Receiving_Entity_ReceivingTransaction transaction)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("sp_Receiving_Transaction_Insert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@PONumber", (object?)transaction.PONumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@UserId", transaction.UserId);
            command.Parameters.AddWithValue("@UserName", transaction.UserName);
            command.Parameters.AddWithValue("@WorkflowMode", transaction.WorkflowMode.ToString());

            // Serialize loads to JSON
            var loadsJson = JsonSerializer.Serialize(transaction.Loads.Select(load => new
            {
                load.LoadNumber,
                load.PartId,
                load.PartType,
                load.Quantity,
                load.UnitOfMeasure,
                load.HeatLotNumber,
                load.PackageType,
                load.PackagesPerLoad,
                load.WeightPerPackage,
                load.ReceivingLocation,
                load.QualityHoldAcknowledged
            }));

            command.Parameters.AddWithValue("@LoadsJson", loadsJson);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var transactionId = reader.GetInt32(reader.GetOrdinal("TransactionId"));
                return Model_Dao_Result_Factory.Success(transactionId, 1);
            }

            return Model_Dao_Result_Factory.Failure<int>("Failed to save transaction - no result returned");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>($"Error saving transaction: {ex.Message}");
        }
    }
}

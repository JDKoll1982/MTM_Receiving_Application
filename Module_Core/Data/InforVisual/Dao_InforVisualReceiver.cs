using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Data.InforVisual;

/// <summary>
/// Read-only access to the recent receivers the By Receiver list shows.
/// ⚠️ STRICTLY READ ONLY - NO WRITES ALLOWED TO INFOR VISUAL ⚠️
/// Server: VISUAL, Database: MTMFG
/// </summary>
/// <remarks>
/// The single query behind this DAO is a SELECT. Nothing here can write, because the connection
/// string is rejected in the constructor unless it asks for <see cref="ApplicationIntent.ReadOnly"/>.
/// </remarks>
public class Dao_InforVisualReceiver
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility? _logger;

    /// <summary>
    /// Creates the DAO for one connection string, rejecting any connection that is not read only.
    /// </summary>
    /// <param name="inforVisualConnectionString">The Infor Visual connection string, which must carry ApplicationIntent=ReadOnly.</param>
    /// <param name="logger">Optional logger for the read and its failures.</param>
    /// <exception cref="InvalidOperationException">The connection string is not read only.</exception>
    public Dao_InforVisualReceiver(
        string inforVisualConnectionString,
        IService_LoggingUtility? logger = null
    )
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
                    $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. "
                        + $"Current ApplicationIntent: {builder.ApplicationIntent}. "
                        + $"Writing to Infor Visual ERP database is STRICTLY PROHIBITED. "
                        + "See Constitution Principle: Infor Visual DAO Architecture."
                );
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Invalid Infor Visual connection string format: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns the newest inventory receipts, one row per receiver and part, newest received date
    /// first. Read-only Infor Visual (MTMFG); no write is ever issued.
    /// </summary>
    /// <param name="maxRows">Safety cap on the returned rows.</param>
    /// <returns>The rows, or a failure result carrying a readable message.</returns>
    public async Task<Model_Dao_Result<List<Model_InforVisualReceiverRow>>> GetRecentReceiversAsync(
        int maxRows
    )
    {
        try
        {
            _logger?.LogInfo($"Retrieving up to {maxRows} recent Infor Visual receivers.");
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = Helper_SqlQueryLoader.LoadAndPrepareQuery("35_GetRecentReceivers.sql");

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaxRows", maxRows);

            var list = new List<Model_InforVisualReceiverRow>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(
                    new Model_InforVisualReceiverRow
                    {
                        ReceiverNumber = reader["ReceiverNumber"].ToString() ?? string.Empty,
                        ReceivedDate = Convert.ToDateTime(reader["ReceivedDate"]),
                        PONumber = reader["PONumber"].ToString() ?? string.Empty,
                        POLineNumber = reader["POLineNumber"].ToString() ?? string.Empty,
                        PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                        PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                        UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
                        VendorName = reader["VendorName"].ToString() ?? string.Empty,
                        Warehouse = reader["Warehouse"].ToString() ?? string.Empty,
                        LocationReceivedTo =
                            reader["LocationReceivedTo"].ToString() ?? string.Empty,
                        QtyReceived = Convert.ToDecimal(reader["QtyReceived"]),
                        QtyOrdered = Convert.ToDecimal(reader["QtyOrdered"]),
                        QtyRemaining = Convert.ToDecimal(reader["QtyRemaining"]),
                    }
                );
            }

            _logger?.LogInfo($"Retrieved {list.Count} recent Infor Visual receiver rows.");
            return Model_Dao_Result_Factory.Success(list);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving recent Infor Visual receivers: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualReceiverRow>>(
                $"Error retrieving recent receivers: {ex.Message}",
                ex
            );
        }
    }
}

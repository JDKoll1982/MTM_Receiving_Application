using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Data;

/// <summary>
/// The module's own read-only Infor Visual data access. It loads its query files
/// through the core query loader and repeats the read-only connection guard, as
/// no data access class is shared between modules (FR-057, R-DATA-04, R-MOD-07,
/// decision D-10).
/// </summary>
public class Dao_ReconciliationInforVisual
{
    private const string CandidatePairsQuery = "Reconciliation/18a_GetTransferCandidatePairs.sql";
    private const string PairDetailsQuery = "Reconciliation/18b_GetTransferPairDetails.sql";
    private const string LocationBalancesQuery =
        "Reconciliation/18c_GetLocationMovementAfterWindow.sql";

    private readonly string _connectionString;
    private readonly IService_LoggingUtility? _logger;

    /// <summary>Creates the DAO over a Visual connection string.</summary>
    /// <param name="connectionString">A Visual connection string carrying read-only intent.</param>
    /// <param name="logger">An optional logger.</param>
    /// <exception cref="InvalidOperationException">
    /// The connection string does not carry <c>ApplicationIntent=ReadOnly</c>.
    /// </exception>
    public Dao_ReconciliationInforVisual(
        string connectionString,
        IService_LoggingUtility? logger = null
    )
    {
        ValidateReadOnlyConnection(connectionString);

        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Reads the transfer movements for one part around a receipt date. Discovery
    /// runs first, so an empty candidate set costs one round trip rather than two.
    /// </summary>
    /// <param name="partId">The exact part id.</param>
    /// <param name="receivedDate">The receipt-date anchor.</param>
    /// <param name="windowDays">Days either side of the anchor to search.</param>
    public virtual async Task<Model_Dao_Result<List<Model_ReconciliationMovement>>> GetTransferMovementsAsync(
        string partId,
        DateOnly receivedDate,
        int windowDays
    )
    {
        try
        {
            var parameters = new List<SqlParameter>
            {
                new("@PartId", partId),
                new("@ReceivedDate", ToDatabaseDate(receivedDate)),
                new("@WindowDays", windowDays),
            };

            int candidateCount = await CountCandidatePairsAsync(parameters);
            if (candidateCount is 0)
            {
                return Model_Dao_Result_Factory.Success(new List<Model_ReconciliationMovement>());
            }

            string detailsQuery = Helper_SqlQueryLoader.LoadAndPrepareQuery(PairDetailsQuery);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(detailsQuery, connection);
            command.Parameters.AddRange(parameters.ToArray());

            var movements = new List<Model_ReconciliationMovement>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                movements.Add(MapMovement(reader));
            }

            return Model_Dao_Result_Factory.Success(movements);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error reading reconciliation transfer movements for part {partId}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_ReconciliationMovement>>(
                $"Error reading Infor Visual transfer movements: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Reads the balance input for one part: live on-hand plus the signed net
    /// movement at each location after the end of the strict window (FR-023).
    /// </summary>
    /// <param name="partId">The exact part id.</param>
    /// <param name="afterDate">The first day after the strict window.</param>
    public virtual async Task<Model_Dao_Result<List<Model_ReconciliationLocationBalance>>>
        GetLocationBalancesAsync(string partId, DateOnly afterDate)
    {
        try
        {
            string query = Helper_SqlQueryLoader.LoadAndPrepareQuery(LocationBalancesQuery);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PartId", partId);
            command.Parameters.AddWithValue("@AfterDate", ToDatabaseDate(afterDate));

            var balances = new List<Model_ReconciliationLocationBalance>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                balances.Add(
                    new Model_ReconciliationLocationBalance
                    {
                        PartId = ReadString(reader, "PartId"),
                        WarehouseId = ReadString(reader, "WarehouseId"),
                        LocationId = ReadString(reader, "LocationId"),
                        OnHand = ReadDecimal(reader, "OnHand"),
                        MovementAfterWindow = ReadDecimal(reader, "MovementAfterWindow"),
                    }
                );
            }

            return Model_Dao_Result_Factory.Success(balances);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Error reading reconciliation location balances for part {partId}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_ReconciliationLocationBalance>>(
                $"Error reading Infor Visual location balances: {ex.Message}",
                ex
            );
        }
    }

    private async Task<int> CountCandidatePairsAsync(IReadOnlyList<SqlParameter> parameters)
    {
        string query = Helper_SqlQueryLoader.LoadAndPrepareQuery(CandidatePairsQuery);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(CopyParameters(parameters));

        int count = 0;
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            count++;
        }

        return count;
    }

    private static SqlParameter[] CopyParameters(IReadOnlyList<SqlParameter> parameters)
    {
        var copy = new SqlParameter[parameters.Count];

        for (int index = 0; index < parameters.Count; index++)
        {
            SqlParameter source = parameters[index];
            copy[index] = new SqlParameter(source.ParameterName, source.Value);
        }

        return copy;
    }

    private static Model_ReconciliationMovement MapMovement(System.Data.IDataRecord record) =>
        new()
        {
            PartId = ReadString(record, "PartId"),
            PurchaseOrderNumber = ReadString(record, "PONumber"),
            PurchaseOrderLineNumber = ReadString(record, "POLineNumber"),
            SourceWarehouseId = ReadString(record, "SourceWarehouseId"),
            SourceLocationId = ReadString(record, "SourceLocationId"),
            DestinationWarehouseId = ReadString(record, "DestinationWarehouseId"),
            DestinationLocationId = ReadString(record, "DestinationLocationId"),
            Quantity = ReadDecimal(record, "TransferQuantity"),
            TransactionDate = ReadDateTime(record, "TransactionDate"),
            TransactionUserId = ReadString(record, "TransactionUserId"),
            SourceTransactionId = ReadInt32(record, "SourceTransactionId"),
            DestinationTransactionId = ReadInt32(record, "DestinationTransactionId"),
        };

    private static DateTime ToDatabaseDate(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

    private static string ReadString(System.Data.IDataRecord record, string column)
    {
        int ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal)
            ? string.Empty
            : Convert.ToString(record.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static decimal ReadDecimal(System.Data.IDataRecord record, string column)
    {
        int ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal)
            ? 0m
            : Convert.ToDecimal(record.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static int ReadInt32(System.Data.IDataRecord record, string column)
    {
        int ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal)
            ? 0
            : Convert.ToInt32(record.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static DateTime ReadDateTime(System.Data.IDataRecord record, string column)
    {
        int ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal)
            ? DateTime.MinValue
            : Convert.ToDateTime(record.GetValue(ordinal), CultureInfo.InvariantCulture);
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
                    "CONSTITUTIONAL VIOLATION: the reconciliation Visual DAO requires "
                        + $"ApplicationIntent=ReadOnly. Current value is '{builder.ApplicationIntent}'."
                );
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                "CONSTITUTIONAL VIOLATION: invalid Infor Visual connection string provided.",
                ex
            );
        }
    }
}

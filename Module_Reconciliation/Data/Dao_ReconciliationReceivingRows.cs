using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Reconciliation.Data;

/// <summary>
/// The module's own data access over the receiving tables. It reads through two
/// reconciliation-owned procedures and writes through exactly one, which is the
/// whole data surface between this module and Receiving (FR-005, FR-056,
/// R-DATA-01, R-DATA-02).
/// </summary>
public class Dao_ReconciliationReceivingRows
{
    private const string GetDayProcedure = "sp_Reconciliation_ReceivingRows_GetDay";
    private const string ActivityRangeProcedure = "sp_Reconciliation_DayActivity_GetRange";
    private const string UpdateLocationProcedure = "sp_Reconciliation_ReceivingLocation_Update";

    private readonly string _connectionString;
    private readonly IService_LoggingUtility? _logger;

    /// <summary>Creates the DAO over an application database connection.</summary>
    /// <param name="connectionString">The application database connection string.</param>
    /// <param name="logger">An optional logger.</param>
    public Dao_ReconciliationReceivingRows(
        string connectionString,
        IService_LoggingUtility? logger = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>Loads every receiving row for one calendar day, from both sources.</summary>
    /// <param name="date">The day to load.</param>
    public virtual async Task<Model_Dao_Result<List<Model_ReconciliationRow>>> GetDayRowsAsync(DateOnly date)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_transaction_date", ToDatabaseDate(date) },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            GetDayProcedure,
            MapRow,
            parameters
        );
    }

    /// <summary>The days inside the range that hold at least one receiving row.</summary>
    /// <param name="from">Range start, inclusive.</param>
    /// <param name="to">Range end, inclusive.</param>
    public virtual async Task<Model_Dao_Result<List<DateOnly>>> GetActivityDatesAsync(
        DateOnly from,
        DateOnly to
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_from", ToDatabaseDate(from) },
            { "@p_to", ToDatabaseDate(to) },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            ActivityRangeProcedure,
            reader =>
            {
                int ordinal = reader.GetOrdinal("activity_date");
                return reader.IsDBNull(ordinal)
                    ? default
                    : DateOnly.FromDateTime(reader.GetDateTime(ordinal));
            },
            parameters
        );
    }

    /// <summary>
    /// Writes one row's saved location through the single reconciliation-owned
    /// write procedure (FR-041, FR-055, R-DATA-03).
    /// </summary>
    /// <param name="source">Which table the row lives in.</param>
    /// <param name="recordId">The row's id in that table.</param>
    /// <param name="destinationLocation">The location to write.</param>
    /// <param name="operatorUserId">The operator recorded on the write.</param>
    public virtual async Task<Model_Dao_Result<int>> UpdateLocationAsync(
        Enum_ReconciliationRowSource source,
        int recordId,
        string destinationLocation,
        string operatorUserId
    )
    {
        var outputParameter = new MySqlParameter("@p_affected_rows", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new[]
        {
            new MySqlParameter("@p_row_source", MySqlDbType.VarChar, 16)
            {
                Value = ToRowSourceText(source),
            },
            new MySqlParameter("@p_record_id", MySqlDbType.Int32) { Value = recordId },
            new MySqlParameter("@p_destination_location", MySqlDbType.VarChar, 50)
            {
                Value = destinationLocation ?? string.Empty,
            },
            new MySqlParameter("@p_operator_user_id", MySqlDbType.VarChar, 100)
            {
                Value = operatorUserId ?? string.Empty,
            },
            outputParameter,
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            UpdateLocationProcedure,
            parameters,
            _connectionString
        );

        if (result.IsSuccess is false)
        {
            _logger?.LogInfo(
                $"Reconciliation location write failed for {source} row {recordId}: {result.ErrorMessage}"
            );
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        int affectedRows = ToInt32(outputParameter.Value);

        return Model_Dao_Result_Factory.Success(affectedRows, affectedRows);
    }

    private static string ToRowSourceText(Enum_ReconciliationRowSource source) =>
        source is Enum_ReconciliationRowSource.History ? "History" : "CurrentLabel";

    private static DateTime ToDatabaseDate(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

    private static int ToInt32(object? value) =>
        value is null or DBNull ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);

    private static Model_ReconciliationRow MapRow(IDataReader reader) =>
        new()
        {
            RowSource =
                string.Equals(ReadString(reader, "row_source"), "History", StringComparison.OrdinalIgnoreCase)
                    ? Enum_ReconciliationRowSource.History
                    : Enum_ReconciliationRowSource.CurrentLabel,
            RecordId = ReadInt32(reader, "record_id"),
            LoadId = ReadGuid(reader, "load_id"),
            LoadNumber = ReadNullableInt32(reader, "load_number"),
            PartId = ReadString(reader, "part_id"),
            PartDescription = ReadString(reader, "part_description"),
            PurchaseOrderNumber = ReadString(reader, "po_number"),
            PurchaseOrderLineNumber = ReadString(reader, "po_line_number"),
            TransactionDate = ReadDateOnly(reader, "transaction_date"),
            ReceivedAt = ReadNullableDateTime(reader, "received_date"),
            Quantity = ReadInt32(reader, "quantity"),
            WeightQuantity = ReadNullableDecimal(reader, "weight_quantity"),
            UnitOfMeasure = ReadString(reader, "unit_of_measure"),
            SavedLocation = ReadString(reader, "initial_location"),
            VendorName = ReadString(reader, "vendor_name"),
            IsNonPurchaseOrder = ReadBoolean(reader, "is_non_po_item"),
            IsReprint = ReadBoolean(reader, "is_reprint"),
        };

    private static string ReadString(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static int ReadInt32(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static int? ReadNullableInt32(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static decimal? ReadNullableDecimal(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static DateTime? ReadNullableDateTime(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static DateOnly ReadDateOnly(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal)
            ? default
            : DateOnly.FromDateTime(Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture));
    }

    private static Guid? ReadGuid(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        string text = Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
        return Guid.TryParse(text, out Guid parsed) ? parsed : null;
    }

    private static bool ReadBoolean(IDataReader reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) is false && ToInt32(reader.GetValue(ordinal)) != 0;
    }
}

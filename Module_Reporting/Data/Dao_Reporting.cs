using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;

namespace MTM_Receiving_Application.Module_Reporting.Data;

public class Dao_Reporting
{
    private readonly string _connectionString;

    public Dao_Reporting(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = CreateDateRangeParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Reporting_ReceivingHistory_GetByDateRange",
            MapReportRowFromReader,
            parameters
        );
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = CreateDateRangeParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Reporting_DunnageHistory_GetByDateRange",
            MapReportRowFromReader,
            parameters
        );
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = CreateDateRangeParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Reporting_VolvoHistory_GetByDateRange",
            MapReportRowFromReader,
            parameters
        );
    }

    public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = CreateDateRangeParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Reporting_Availability_GetByDateRange",
            MapAvailabilityFromReader,
            parameters
        );
    }

    private static Dictionary<string, object> CreateDateRangeParameters(
        DateTime startDate,
        DateTime endDate
    )
    {
        return new Dictionary<string, object>
        {
            ["start_date"] = startDate.Date,
            ["end_date"] = endDate.Date.AddDays(1),
        };
    }

    private static Model_ReportRow MapReportRowFromReader(IDataReader reader)
    {
        return new Model_ReportRow
        {
            Id = ReadString(reader, "id") ?? string.Empty,
            PONumber = ReadNullableString(reader, "po_number"),
            POLineNumber = ReadNullableString(reader, "po_line_number"),
            PartNumber = ReadNullableString(reader, "part_id", "part_number"),
            PartDescription = ReadNullableString(reader, "part_description"),
            Quantity = ReadNullableDecimal(reader, "quantity"),
            WeightLbs = ReadNullableDecimal(reader, "weight_lbs"),
            HeatLotNumber = ReadNullableString(reader, "heat", "heat_lot_number"),
            CreatedDate = ReadDateTime(reader, "created_at", "created_date", "transaction_date"),
            TransactionDate = ReadNullableDateTime(reader, "transaction_date"),
            CreatedAt = ReadNullableDateTime(reader, "created_at", "created_date"),
            EmployeeNumber = ReadNullableString(reader, "employee_number"),
            CreatedByUsername = ReadNullableString(reader, "created_by_username", "user_id"),
            UserId = ReadNullableString(reader, "user_id"),
            SourceModule = ReadString(reader, "source_module") ?? string.Empty,
            DunnageType = ReadNullableString(reader, "dunnage_type"),
            SpecsCombined = ReadUdcCombined(reader),
            ShipmentNumber = ReadNullableInt(reader, "shipment_number"),
            ReceiverNumber = ReadNullableString(reader, "receiver_number"),
            Status = ReadNullableString(reader, "status"),
            PartCount = ReadNullableInt(reader, "part_count"),
            Location = ReadNullableString(reader, "initial_location", "location"),
            VendorName = ReadNullableString(reader, "vendor_name"),
            Notes = ReadNullableString(reader, "notes"),
            LoadNumber = ReadNullableInt(reader, "load_number"),
            LabelNumber = ReadNullableInt(reader, "label_number"),
            PackagesPerLoad = ReadNullableInt(reader, "packages_per_load"),
            PackageTypeName = ReadNullableString(reader, "package_type_name"),
            WeightPerPackage = ReadNullableDecimal(reader, "weight_per_package"),
            PoStatus = ReadNullableString(reader, "po_status"),
            PoDueDate = ReadNullableDateTime(reader, "po_due_date"),
            QtyOrdered = ReadNullableDecimal(reader, "qty_ordered"),
            UnitOfMeasure = ReadNullableString(reader, "unit_of_measure"),
            RemainingQuantity = ReadNullableInt(reader, "remaining_quantity"),
            CoilsOnSkid = ReadNullableInt(reader, "coils_on_skid"),
            IsNonPOItem = ReadNullableBool(reader, "is_non_po_item") ?? false,
            IsQualityHoldRequired = ReadNullableBool(reader, "is_quality_hold_required") ?? false,
            IsQualityHoldAcknowledged =
                ReadNullableBool(reader, "is_quality_hold_acknowledged") ?? false,
            QualityHoldRestrictionType = ReadNullableString(
                reader,
                "quality_hold_restriction_type"
            ),
            PartSkidTotal = ReadNullableInt(reader, "part_skid_total"),
            QuantityPerSkid = ReadNullableInt(reader, "quantity_per_skid"),
            ReceivedSkidCount = ReadNullableInt(reader, "received_skid_count"),
        };
    }

    private static Dictionary<string, int> MapAvailabilityFromReader(IDataReader reader)
    {
        return new Dictionary<string, int>
        {
            ["Receiving"] = reader.GetInt32(reader.GetOrdinal("receiving_count")),
            ["Dunnage"] = reader.GetInt32(reader.GetOrdinal("dunnage_count")),
            ["Volvo"] = reader.GetInt32(reader.GetOrdinal("volvo_count")),
        };
    }

    private static string? ReadString(IDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!TryGetOrdinal(reader, columnName, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            return Convert.ToString(reader.GetValue(ordinal));
        }

        return null;
    }

    private static string? ReadNullableString(IDataReader reader, params string[] columnNames)
    {
        return ReadString(reader, columnNames);
    }

    private static string? ReadUdcCombined(IDataReader reader)
    {
        var values = new List<string>();
        for (var slot = 1; slot <= 10; slot++)
        {
            var value = ReadNullableString(reader, $"udc{slot}");
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            values.Add(value.Trim());
        }

        return values.Count == 0 ? null : string.Join(" | ", values);
    }

    private static decimal? ReadNullableDecimal(IDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!TryGetOrdinal(reader, columnName, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            return Convert.ToDecimal(reader.GetValue(ordinal));
        }

        return null;
    }

    private static int? ReadNullableInt(IDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!TryGetOrdinal(reader, columnName, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            return Convert.ToInt32(reader.GetValue(ordinal));
        }

        return null;
    }

    private static bool? ReadNullableBool(IDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!TryGetOrdinal(reader, columnName, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            return Convert.ToBoolean(reader.GetValue(ordinal));
        }

        return null;
    }

    private static DateTime ReadDateTime(IDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!TryGetOrdinal(reader, columnName, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            return Convert.ToDateTime(reader.GetValue(ordinal));
        }

        return DateTime.MinValue;
    }

    private static DateTime? ReadNullableDateTime(IDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (!TryGetOrdinal(reader, columnName, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            return Convert.ToDateTime(reader.GetValue(ordinal));
        }

        return null;
    }

    private static bool TryGetOrdinal(IDataReader reader, string columnName, out int ordinal)
    {
        for (var index = 0; index < reader.FieldCount; index++)
        {
            if (
                string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase)
            )
            {
                ordinal = index;
                return true;
            }
        }

        ordinal = -1;
        return false;
    }
}

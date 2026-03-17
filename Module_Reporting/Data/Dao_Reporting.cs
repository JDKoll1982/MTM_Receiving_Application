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
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate.Date },
            { "end_date", endDate.Date }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Reporting_ReceivingHistory_GetByDateRange",
            MapReportRowFromReader,
            parameters);
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate.Date },
            { "end_date", endDate.Date }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Reporting_DunnageHistory_GetByDateRange",
            MapReportRowFromReader,
            parameters);
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate.Date },
            { "end_date", endDate.Date }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Reporting_VolvoHistory_GetByDateRange",
            MapReportRowFromReader,
            parameters);
    }

    public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate.Date },
            { "end_date", endDate.Date }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Reporting_Availability_GetByDateRange",
            MapAvailabilityFromReader,
            parameters);
    }

    private static Model_ReportRow MapReportRowFromReader(IDataReader reader)
    {
        return new Model_ReportRow
        {
            Id = ReadString(reader, "id") ?? string.Empty,
            PONumber = ReadNullableString(reader, "po_number"),
            PartNumber = ReadNullableString(reader, "part_number"),
            PartDescription = ReadNullableString(reader, "part_description"),
            Quantity = ReadNullableDecimal(reader, "quantity"),
            WeightLbs = ReadNullableDecimal(reader, "weight_lbs"),
            HeatLotNumber = ReadNullableString(reader, "heat_lot_number"),
            CreatedDate = ReadDateTime(reader, "created_date"),
            EmployeeNumber = ReadNullableString(reader, "employee_number"),
            CreatedByUsername = ReadNullableString(reader, "created_by_username"),
            SourceModule = ReadString(reader, "source_module") ?? string.Empty,
            DunnageType = ReadNullableString(reader, "dunnage_type"),
            SpecsCombined = ReadNullableString(reader, "specs_combined"),
            ShipmentNumber = ReadNullableInt(reader, "shipment_number"),
            ReceiverNumber = ReadNullableString(reader, "receiver_number"),
            Status = ReadNullableString(reader, "status"),
            PartCount = ReadNullableInt(reader, "part_count")
        };
    }

    private static Dictionary<string, int> MapAvailabilityFromReader(IDataReader reader)
    {
        return new Dictionary<string, int>
        {
            ["Receiving"] = reader.GetInt32(reader.GetOrdinal("receiving_count")),
            ["Dunnage"] = reader.GetInt32(reader.GetOrdinal("dunnage_count")),
            ["Volvo"] = reader.GetInt32(reader.GetOrdinal("volvo_count"))
        };
    }

    private static string? ReadString(IDataReader reader, string columnName)
    {
        if (!TryGetOrdinal(reader, columnName, out var ordinal))
        {
            return null;
        }

        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return Convert.ToString(reader.GetValue(ordinal));
    }

    private static string? ReadNullableString(IDataReader reader, string columnName)
    {
        if (!TryGetOrdinal(reader, columnName, out var ordinal))
        {
            return null;
        }

        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return Convert.ToString(reader.GetValue(ordinal));
    }

    private static decimal? ReadNullableDecimal(IDataReader reader, string columnName)
    {
        if (!TryGetOrdinal(reader, columnName, out var ordinal))
        {
            return null;
        }

        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static int? ReadNullableInt(IDataReader reader, string columnName)
    {
        if (!TryGetOrdinal(reader, columnName, out var ordinal))
        {
            return null;
        }

        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static DateTime ReadDateTime(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.GetDateTime(ordinal);
    }

    private static bool TryGetOrdinal(IDataReader reader, string columnName, out int ordinal)
    {
        for (var index = 0; index < reader.FieldCount; index++)
        {
            if (string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
            {
                ordinal = index;
                return true;
            }
        }

        ordinal = -1;
        return false;
    }
}

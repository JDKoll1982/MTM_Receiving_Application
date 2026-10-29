using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data;

/// <summary>
/// MySQL data access for the Receiving Analytics App Data tab. Every call goes through a
/// stored procedure in the sp_Reporting_ReceivingAnalyticsApp_* family; there is no raw SQL here.
/// </summary>
public class Dao_Tool_ReceivingAnalyticsApp
{
    private readonly string _connectionString;

    public Dao_Tool_ReceivingAnalyticsApp(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <summary>Reads the per-day activity rows for the range (start inclusive, end exclusive).</summary>
    public virtual async Task<
        Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppActivityRow>>
    > GetActivityByDayAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = BuildDateParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync<
            Model_Tool_ReceivingAnalyticsAppActivityRow
        >(
            _connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay",
            MapActivityRow,
            parameters
        );
    }

    /// <summary>Reads the per-day on-time rows for the range (start inclusive, end exclusive).</summary>
    public virtual async Task<
        Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppOnTimeRow>>
    > GetOnTimeByDayAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = BuildDateParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync<
            Model_Tool_ReceivingAnalyticsAppOnTimeRow
        >(
            _connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay",
            MapOnTimeRow,
            parameters
        );
    }

    /// <summary>Reads the per-supplier totals for the range (start inclusive, end exclusive).</summary>
    public virtual async Task<
        Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppSupplierRow>>
    > GetSupplierTotalsAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = BuildDateParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync<
            Model_Tool_ReceivingAnalyticsAppSupplierRow
        >(
            _connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals",
            MapSupplierRow,
            parameters
        );
    }

    /// <summary>
    /// Reads the count of receipts missing a load number for the range
    /// (start inclusive, end exclusive).
    /// </summary>
    public virtual async Task<Model_Dao_Result<int>> GetMissingLoadNumberCountAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = BuildDateParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            _connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount",
            reader => Convert.ToInt32(reader.GetValue(reader.GetOrdinal("missing_load_number_count"))),
            parameters
        );
    }

    /// <summary>
    /// Reads the distinct part numbers the app itself received in the range
    /// (start inclusive, end exclusive). This is the cohort the coverage comparison is limited to,
    /// so Infor Visual is only counted for material the receiving app also handles.
    /// </summary>
    public virtual async Task<Model_Dao_Result<List<string>>> GetReceivedPartNumbersAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = BuildDateParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync<string>(
            _connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers",
            reader => GetNullableString(reader, "part_id"),
            parameters
        );
    }

    private static Dictionary<string, object> BuildDateParameters(
        DateTime startDate,
        DateTime endDate
    )
    {
        return new Dictionary<string, object>
        {
            { "start_date", startDate },
            { "end_date", endDate },
        };
    }

    /// <summary>
    /// Reads the app's completed purchase-order lines attributed to the date each was last received,
    /// which is how Infor Visual dates a PO line. Used by the coverage comparison so both sides
    /// count the same thing.
    /// </summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppCoverageLine>>> GetCoverageLinesAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = BuildDateParameters(startDate, endDate);

        return await Helper_Database_StoredProcedure.ExecuteListAsync<
            Model_Tool_ReceivingAnalyticsAppCoverageLine
        >(
            _connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines",
            reader =>
                new Model_Tool_ReceivingAnalyticsAppCoverageLine
                {
                    LastReceivedDate = reader.GetDateTime(
                        reader.GetOrdinal("last_received_date")
                    ),
                    PartId = GetNullableString(reader, "part_id"),
                    LineCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("line_count"))),
                },
            parameters
        );
    }

    private static Model_Tool_ReceivingAnalyticsAppActivityRow MapActivityRow(IDataReader reader)
    {
        return new Model_Tool_ReceivingAnalyticsAppActivityRow
        {
            BucketDate = reader.GetDateTime(reader.GetOrdinal("bucket_date")),
            PartType = GetNullableString(reader, "part_type"),
            UnitOfMeasure = GetNullableString(reader, "unit_of_measure"),
            LoadCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("load_count"))),
            LabelCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("label_count"))),
            QuantityTotal = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("quantity_total"))),
            ReceiptCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("receipt_count"))),
        };
    }

    private static Model_Tool_ReceivingAnalyticsAppOnTimeRow MapOnTimeRow(IDataReader reader)
    {
        return new Model_Tool_ReceivingAnalyticsAppOnTimeRow
        {
            BucketDate = reader.GetDateTime(reader.GetOrdinal("bucket_date")),
            Status = GetNullableString(reader, "status"),
            ReceiptCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("receipt_count"))),
        };
    }

    private static Model_Tool_ReceivingAnalyticsAppSupplierRow MapSupplierRow(IDataReader reader)
    {
        return new Model_Tool_ReceivingAnalyticsAppSupplierRow
        {
            VendorName = GetNullableString(reader, "vendor_name"),
            UnitOfMeasure = GetNullableString(reader, "unit_of_measure"),
            LabelCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("label_count"))),
            LoadCount = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("load_count"))),
            QuantityTotal = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("quantity_total"))),
        };
    }

    private static string GetNullableString(IDataReader reader, string column)
    {
        var value = reader.GetValue(reader.GetOrdinal(column));
        return value == DBNull.Value ? string.Empty : Convert.ToString(value) ?? string.Empty;
    }
}

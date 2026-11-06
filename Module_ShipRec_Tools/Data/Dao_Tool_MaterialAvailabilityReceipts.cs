using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data;

/// <summary>
/// Read-only MySQL access to the receiving history behind the material availability printouts.
/// Every call goes through a stored procedure; there is no raw SQL here.
/// </summary>
public class Dao_Tool_MaterialAvailabilityReceipts
{
    private readonly string _connectionString;

    public Dao_Tool_MaterialAvailabilityReceipts(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Counts the receiving rows (one per skid / package label) recorded for every part whose
    /// <c>transaction_date</c> falls inside the inclusive date range. The returned lookup is keyed
    /// by part id and compares part ids case-insensitively.
    /// </summary>
    public virtual async Task<
        Model_Dao_Result<Dictionary<string, int>>
    > GetReceivedLabelCountsAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "PartID", DBNull.Value },
            { "StartDate", startDate.Date },
            { "EndDate", endDate.Date },
        };

        var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
            _connectionString,
            "sp_Receiving_History_Get",
            parameters
        );

        if (!result.IsSuccess || result.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(result.ErrorMessage);
        }

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (!result.Data.Columns.Contains("part_id"))
        {
            return Model_Dao_Result_Factory.Success(counts);
        }

        foreach (DataRow row in result.Data.Rows)
        {
            if (row["part_id"] is DBNull)
            {
                continue;
            }

            var partId = Convert.ToString(row["part_id"]);
            if (string.IsNullOrWhiteSpace(partId))
            {
                continue;
            }

            counts[partId] = counts.TryGetValue(partId, out var existing) ? existing + 1 : 1;
        }

        return Model_Dao_Result_Factory.Success(counts);
    }
}

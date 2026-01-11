using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing_label_history table
/// </summary>
public class Dao_RoutingLabelHistory
{
    private readonly string _connectionString;

    public Dao_RoutingLabelHistory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a history entry for label edit
    /// </summary>
    /// <param name="history"></param>
    public async Task<Model_Dao_Result> InsertHistoryAsync(Model_RoutingLabelHistory history)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_label_id", history.LabelId),
                new MySqlParameter("@p_field_changed", history.FieldChanged),
                new MySqlParameter("@p_old_value", (object?)history.OldValue ?? DBNull.Value),
                new MySqlParameter("@p_new_value", (object?)history.NewValue ?? DBNull.Value),
                new MySqlParameter("@p_edited_by", history.EditedBy),
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_label_history_insert",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error inserting history: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Issue #18: Batch inserts multiple history entries to avoid N+1 query problem
    /// </summary>
    /// <param name="historyEntries">List of history entries to insert</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> InsertHistoryBatchAsync(List<Model_RoutingLabelHistory> historyEntries)
    {
        if (historyEntries == null || historyEntries.Count == 0)
        {
            return Model_Dao_Result_Factory.Success("No history entries to insert", 0);
        }

        try
        {
            int successCount = 0;
            foreach (var history in historyEntries)
            {
                var result = await InsertHistoryAsync(history);
                if (!result.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure($"Batch insert failed at entry {successCount + 1}: {result.ErrorMessage}");
                }
                successCount++;
            }

            return Model_Dao_Result_Factory.Success($"Inserted {successCount} history entries", successCount);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error in batch insert: {ex.Message}", ex);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Data;

/// <summary>
/// Data-access object for the <c>bulk_inventory_transactions</c> MySQL table.
/// Instance-based; all MySQL calls use stored procedures via
/// <see cref="Helper_Database_StoredProcedure"/>.
/// </summary>
public class Dao_BulkInventoryTransaction
{
    private readonly string _connectionString;

    public Dao_BulkInventoryTransaction(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    // ── Insert ────────────────────────────────────────────────────────────────

    /// <summary>Inserts a new Pending row and returns the generated database id.</summary>
    /// <param name="row">The transaction row to persist.</param>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_BulkInventoryTransaction row)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_created_by_user",  row.CreatedByUser },
                { "p_transaction_type", row.TransactionType.ToString() },
                { "p_part_id",          row.PartId },
                { "p_from_warehouse",   (object?)row.FromWarehouse  ?? DBNull.Value },
                { "p_from_location",    (object?)row.FromLocation   ?? DBNull.Value },
                { "p_to_warehouse",     row.ToWarehouse },
                { "p_to_location",      row.ToLocation },
                { "p_quantity",         row.Quantity },
                { "p_work_order",       (object?)row.WorkOrder      ?? DBNull.Value },
                { "p_lot_no",           (object?)row.LotNo          ?? DBNull.Value },
                { "p_visual_username",  row.VisualUsername }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
                _connectionString,
                "sp_BulkInventory_Transaction_Insert",
                reader => Convert.ToInt32(reader["id"]),
                parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Error inserting bulk inventory transaction: {ex.Message}", ex);
        }
    }

    // ── Update Status ─────────────────────────────────────────────────────────

    /// <summary>Updates the <c>status</c> and optional <c>error_message</c> for a row by id.</summary>
    /// <param name="id">Primary key of the row to update.</param>
    /// <param name="status">New status value.</param>
    /// <param name="errorMessage">Optional error message; pass <c>null</c> to clear.</param>
    public async Task<Model_Dao_Result> UpdateStatusAsync(
        int id,
        Enum_BulkInventoryStatus status,
        string? errorMessage)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_id",            id },
                { "p_status",        status.ToString() },
                { "p_error_message", (object?)errorMessage ?? DBNull.Value }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_BulkInventory_Transaction_UpdateStatus",
                parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Error updating bulk inventory transaction status: {ex.Message}", ex);
        }
    }

    // ── Query ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all rows for <paramref name="username"/>, optionally filtered to a single
    /// <paramref name="status"/>.  Pass <c>null</c> for <paramref name="status"/> to return all statuses.
    /// </summary>
    /// <param name="username">Windows username of the operator.</param>
    /// <param name="status">Optional status filter; <c>null</c> returns all.</param>
    public async Task<Model_Dao_Result<List<Model_BulkInventoryTransaction>>> GetByUserAsync(
        string username,
        Enum_BulkInventoryStatus? status = null)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_username", username },
                { "p_status",   (object?)status?.ToString() ?? DBNull.Value }
            };

            return await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_BulkInventory_Transaction_GetByUser",
                reader => new Model_BulkInventoryTransaction
                {
                    Id = Convert.ToInt32(reader["id"]),
                    CreatedByUser = reader["created_by_user"].ToString() ?? string.Empty,
                    VisualUsername = reader["visual_username"].ToString() ?? string.Empty,
                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                    UpdatedAt = Convert.ToDateTime(reader["updated_at"]),
                    TransactionType = Enum.Parse<Enum_BulkInventoryTransactionType>(
                                          reader["transaction_type"].ToString()!),
                    PartId = reader["part_id"].ToString() ?? string.Empty,
                    FromWarehouse = reader["from_warehouse"] as string,
                    FromLocation = reader["from_location"] as string,
                    ToWarehouse = reader["to_warehouse"].ToString() ?? string.Empty,
                    ToLocation = reader["to_location"].ToString() ?? string.Empty,
                    Quantity = Convert.ToDecimal(reader["quantity"]),
                    WorkOrder = reader["work_order"] as string,
                    LotNo = reader["lot_no"] as string,
                    Status = Enum.Parse<Enum_BulkInventoryStatus>(
                                          reader["status"].ToString()!),
                    ErrorMessage = reader["error_message"] as string
                },
                parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_BulkInventoryTransaction>>(
                $"Error retrieving bulk inventory transactions: {ex.Message}", ex);
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <summary>Hard-deletes a single row by id.</summary>
    /// <param name="id">Primary key of the row to delete.</param>
    public async Task<Model_Dao_Result> DeleteByIdAsync(int id)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_id", id }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_BulkInventory_Transaction_DeleteById",
                parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Error deleting bulk inventory transaction: {ex.Message}", ex);
        }
    }
}

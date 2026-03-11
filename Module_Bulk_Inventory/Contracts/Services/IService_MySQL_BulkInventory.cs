using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;

/// <summary>
/// Business-layer interface for persisting and querying bulk inventory transaction rows.
/// Wraps <c>Dao_BulkInventoryTransaction</c>; callers use this interface rather than
/// the DAO directly.
/// </summary>
public interface IService_MySQL_BulkInventory
{
    // ── Batch entry ───────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new Pending transaction row and returns the generated id.
    /// Called by <c>ViewModel_BulkInventory_DataEntry.AddRowCommand</c>.
    /// </summary>
    /// <param name="row">The transaction to persist.</param>
    Task<Model_Dao_Result<int>> StartRowAsync(Model_BulkInventoryTransaction row);

    /// <summary>
    /// Updates the editable fields (PartId, locations, quantity, type, work order, lot) of
    /// an existing Pending row. Called on cell-leave auto-save when the row has already been
    /// inserted (<c>row.Id &gt; 0</c>).
    /// </summary>
    /// <param name="row">Row containing the updated values; must have a valid <c>Id</c>.</param>
    Task<Model_Dao_Result> UpdateRowAsync(Model_BulkInventoryTransaction row);

    // ── Automation pipeline ───────────────────────────────────────────────────

    /// <summary>
    /// Marks a row as InProgress before the automation loop begins sending keystrokes.
    /// </summary>
    /// <param name="id">Primary key of the row to audit.</param>
    Task<Model_Dao_Result> WriteAuditAsync(int id);

    /// <summary>
    /// Sets the final outcome (Success, Failed, Skipped, WaitingForConfirmation) for a row.
    /// <paramref name="errorMessage"/> is stored only when <paramref name="status"/> is Failed.
    /// </summary>
    /// <param name="id">Primary key of the row to finalize.</param>
    /// <param name="status">Outcome status to set.</param>
    /// <param name="errorMessage">Optional failure detail; stored only on Failed status.</param>
    Task<Model_Dao_Result> CompleteRowAsync(int id, Enum_BulkInventoryStatus status, string? errorMessage = null);

    // ── General CRUD ──────────────────────────────────────────────────────────

    /// <summary>Updates <c>status</c> and <c>error_message</c> directly (used by consolidation logic).</summary>
    /// <param name="id">Row primary key.</param>
    /// <param name="status">New status.</param>
    /// <param name="errorMessage">Optional error detail.</param>
    Task<Model_Dao_Result> UpdateStatusAsync(int id, Enum_BulkInventoryStatus status, string? errorMessage = null);

    /// <summary>
    /// Returns all rows for <paramref name="username"/>, optionally filtered to one status.
    /// Pass <c>null</c> for <paramref name="status"/> to retrieve all statuses.
    /// </summary>
    /// <param name="username">Windows username of the operator.</param>
    /// <param name="status">Optional status filter; <c>null</c> returns all.</param>
    Task<Model_Dao_Result<List<Model_BulkInventoryTransaction>>> GetByUserAsync(
        string username,
        Enum_BulkInventoryStatus? status = null);

    /// <summary>Hard-deletes a single row by id.</summary>
    /// <param name="id">Primary key of the row to delete.</param>
    Task<Model_Dao_Result> DeleteByIdAsync(int id);
}

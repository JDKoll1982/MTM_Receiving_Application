using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;

/// <summary>
/// Drives Infor Visual via UI Automation to post bulk inventory transactions.
/// All failures are captured as row outcomes — callers never receive raw exceptions.
/// </summary>
public interface IService_VisualInventoryAutomation
{
    /// <summary>
    /// Posts an Inventory Transfer row into Visual's Inventory Transfers window.
    /// Updates the row's status to <c>Success</c> or <c>Failed</c> on completion.
    /// </summary>
    /// <param name="row">The transaction row describing the transfer.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExecuteTransferAsync(Model_BulkInventoryTransaction row, CancellationToken ct);

    /// <summary>
    /// Posts a new inventory transaction (receipt / issue) via Visual's
    /// Inventory Transaction Entry window.
    /// Updates the row's status to <c>Success</c> or <c>Failed</c> on completion.
    /// </summary>
    /// <param name="row">The transaction row to process.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExecuteNewTransactionAsync(Model_BulkInventoryTransaction row, CancellationToken ct);
}
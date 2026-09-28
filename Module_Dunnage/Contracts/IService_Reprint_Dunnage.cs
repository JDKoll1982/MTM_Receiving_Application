using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Dedicated reprint contract for Dunnage, consumed by the Module_Reprint UI. Wraps the dunnage
/// service's reprint history query and re-queue behavior.
/// </summary>
public interface IService_Reprint_Dunnage
{
    Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
        Model_ReprintHistoryFilter filter
    );

    /// <summary>
    /// Re-queues the given dunnage history load UUIDs back into the active label queue, returning
    /// a per-row outcome (queued / already queued / failed).
    /// </summary>
    Task<Model_Dao_Result<Model_ReprintBatchResult>> ReprintAsync(IReadOnlyList<string> loadUuids);
}

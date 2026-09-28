using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;

namespace MTM_Receiving_Application.Module_Volvo.Contracts;

/// <summary>
/// Dedicated reprint contract for Volvo, consumed by the Module_Reprint UI. Reprints Volvo
/// generated-label history rows (volvo_generated_label_history) back into the active generated
/// label queue (volvo_generated_label_data).
/// </summary>
public interface IService_Reprint_Volvo
{
    Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
        Model_ReprintHistoryFilter filter
    );

    /// <summary>
    /// Re-queues the given generated-label history ids back into the active queue, returning a
    /// per-row outcome (queued / already queued / failed).
    /// </summary>
    Task<Model_Dao_Result<Model_ReprintBatchResult>> ReprintAsync(IReadOnlyList<string> historyIds);
}

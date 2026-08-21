using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Core.Models.Reprint;

/// <summary>
/// Per-row outcome of a batch reprint operation. Duplicate (already-queued) rows are skipped
/// rather than aborting the whole batch; the page surfaces a "X queued, Y already queued" summary.
/// </summary>
public class Model_ReprintBatchResult
{
    public List<string> Queued { get; } = new();

    public List<string> AlreadyQueued { get; } = new();

    public List<string> Failed { get; } = new();

    public int QueuedCount => Queued.Count;

    public int AlreadyQueuedCount => AlreadyQueued.Count;

    public int FailedCount => Failed.Count;
}

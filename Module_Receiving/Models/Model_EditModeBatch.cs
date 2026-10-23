using System;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One entry in the Edit Mode batch filter: a set of skids that were entered together for a
/// single part, or the "all batches" entry that clears the filter.
/// </summary>
public sealed class Model_EditModeBatch
{
    /// <summary>
    /// Value matched against <see cref="Model_ReceivingLoad.BatchKey"/>. Null for the
    /// "all batches" entry.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>Part shared by every row in the batch. Empty for the "all batches" entry.</summary>
    public string PartID { get; init; } = string.Empty;

    /// <summary>Timestamp the batch was saved, or null when it is still unsaved session data.</summary>
    public DateTime? CreatedAt { get; init; }

    /// <summary>Number of rows in the batch.</summary>
    public int Count { get; init; }

    /// <summary>Text shown in the batch filter ComboBox.</summary>
    public string Display { get; init; } = string.Empty;

    /// <summary>True when this entry clears the batch filter instead of applying one.</summary>
    public bool IsAllBatches => Key is null;
}

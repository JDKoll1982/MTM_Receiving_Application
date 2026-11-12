using System;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One pass over one day. Every log line written during the run carries
/// <see cref="RunId"/> (FR-069).
/// </summary>
public sealed class Model_ReconciliationRun
{
    /// <summary>The run identity, copyable from the day summary (FR-049).</summary>
    public Guid RunId { get; init; }

    /// <summary>Which day this pass covered.</summary>
    public DateOnly Day { get; init; }

    /// <summary>When the pass started, and the duration source.</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>When the pass finished, or null while it is still running.</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>The Infor Visual user attribution the summary shows.</summary>
    public string VisualUserId { get; init; } = string.Empty;

    /// <summary>True when this run belongs to a sweep.</summary>
    public bool IsSweepChild { get; init; }

    /// <summary>The elapsed duration, or null while the run is unfinished.</summary>
    public TimeSpan? Duration => FinishedAt.HasValue ? FinishedAt.Value - StartedAt : null;
}

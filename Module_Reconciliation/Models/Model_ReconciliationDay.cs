using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One calendar day of receiving rows, its activity state, and whether it completed.
/// </summary>
public sealed class Model_ReconciliationDay
{
    /// <summary>The unit of work (FR-007).</summary>
    public DateOnly Date { get; init; }

    /// <summary>True when at least one receiving row exists for the day (FR-010).</summary>
    public bool HasActivity { get; set; }

    /// <summary>The loaded candidate rows.</summary>
    public IReadOnlyList<Model_ReconciliationRow> Rows { get; set; } = new List<Model_ReconciliationRow>();

    /// <summary>The resolved groups.</summary>
    public IReadOnlyList<Model_ReconciliationGroup> Groups { get; set; } =
        new List<Model_ReconciliationGroup>();

    /// <summary>Exactly one finding per row.</summary>
    public IReadOnlyList<Model_ReconciliationFinding> Findings { get; set; } =
        new List<Model_ReconciliationFinding>();

    /// <summary>The counts and identity the page shows.</summary>
    public Model_ReconciliationDaySummary Summary { get; set; } = new();

    /// <summary>The day's lifecycle state.</summary>
    public Enum_ReconciliationDayState State { get; set; } = Enum_ReconciliationDayState.NotLoaded;

    /// <summary>Empty unless the day failed.</summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>The run that produced this day, or null before a preview.</summary>
    public Model_ReconciliationRun? Run { get; set; }
}

using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Summarizes a receiving location reconciliation run.
/// </summary>
public sealed class Model_ReceivingLocationReconciliationSummary
{
    public bool IncludeAllHistory { get; set; }

    public int CurrentLabelRowsScanned { get; set; }

    public int HistoryRowsScanned { get; set; }

    public int CurrentLabelRowsUpdated { get; set; }

    public int HistoryRowsUpdated { get; set; }

    public int UnchangedCount { get; set; }

    public int AmbiguousCount { get; set; }

    public int NotFoundCount { get; set; }

    public int SkippedCount { get; set; }

    public int ErrorCount { get; set; }

    public int TotalRowsScanned => CurrentLabelRowsScanned + HistoryRowsScanned;

    public int TotalRowsUpdated => CurrentLabelRowsUpdated + HistoryRowsUpdated;

    public List<Model_ReceivingLocationReconciliationItem> UpdatedItems { get; } = [];

    public List<Model_ReceivingLocationReconciliationItem> UnresolvedItems { get; } = [];
}

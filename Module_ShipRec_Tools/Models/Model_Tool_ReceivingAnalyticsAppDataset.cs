using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Assembled app-data result for the Receiving Analytics tab: the raw per-day
/// rows the service fetched plus the missing-load-number count, so a period or
/// supplier-measure change re-buckets in memory instead of re-querying.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppDataset
{
    /// <summary>Activity rows keyed by bucket date, unit, and category.</summary>
    public IReadOnlyList<Model_Tool_ReceivingAnalyticsAppActivityRow> ActivityRows { get; set; } = [];

    /// <summary>On-time classification rows keyed by bucket date.</summary>
    public IReadOnlyList<Model_Tool_ReceivingAnalyticsAppOnTimeRow> OnTimeRows { get; set; } = [];

    /// <summary>Supplier totals keyed by supplier and unit.</summary>
    public IReadOnlyList<Model_Tool_ReceivingAnalyticsAppSupplierRow> SupplierRows { get; set; } = [];

    /// <summary>Number of completed receipts with no load number.</summary>
    public int MissingLoadNumberCount { get; set; }

    /// <summary>
    /// True when the activity rows contain no completed receipts.
    /// </summary>
    public bool IsEmpty => ActivityRows.Count == 0;
}

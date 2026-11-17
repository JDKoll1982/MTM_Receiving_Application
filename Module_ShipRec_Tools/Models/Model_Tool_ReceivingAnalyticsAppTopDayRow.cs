using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One received day for one item type in the Receiving Analytics Record Days panel. Each row is
/// read from MySQL grouped by day and item type; the ranking position is filled in by the view
/// model once the rows come back for the requested range.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppTopDayRow
{
    /// <summary>Calendar date of the receipts, taken from the same day definition the other panels use.</summary>
    public DateTime DayDate { get; set; }

    /// <summary>Item type: Coil (MMC), Sheet (MMF) or Standard.</summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>
    /// Distinct loads that carried this item type that day. A load is counted once per item type it
    /// carried, so the same load can appear in more than one ranking.
    /// </summary>
    public int LoadCount { get; set; }

    /// <summary>Receipt rows for that day and item type; one row of the receiving history is one app label.</summary>
    public int LabelCount { get; set; }

    /// <summary>1-based position inside its item type ranking. Set by the view model, not by the database.</summary>
    public int Rank { get; set; }

    /// <summary>Weekday and date, so a ranked day reads without a second lookup.</summary>
    public string DayText => DayDate.ToString("ddd MM/dd/yyyy");

    /// <summary>Formatted load count for display.</summary>
    public string LoadsText => LoadCount.ToString("N0");

    /// <summary>Formatted label count for display.</summary>
    public string LabelsText => LabelCount.ToString("N0");
}

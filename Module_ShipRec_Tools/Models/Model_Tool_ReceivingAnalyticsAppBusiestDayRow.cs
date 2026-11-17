using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One received day ranked across every item type in the Receiving Analytics Record Days panel.
/// The load and label counts are the day's own totals, not a sum of the per-item-type rankings, so
/// a load that carried both coil and flat stock material is still counted once.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppBusiestDayRow
{
    /// <summary>Calendar date of the receipts, using the same day definition as the other panels.</summary>
    public DateTime DayDate { get; set; }

    /// <summary>Distinct loads worked that day, across every item type.</summary>
    public int LoadCount { get; set; }

    /// <summary>Receipt rows for that day; one row of the receiving history is one app label.</summary>
    public int LabelCount { get; set; }

    /// <summary>1-based position in the ranking. Set by the view model, not by the database.</summary>
    public int Rank { get; set; }

    /// <summary>Weekday and date, so a ranked day reads without a second lookup.</summary>
    public string DayText => DayDate.ToString("ddd MM/dd/yyyy");

    /// <summary>Formatted load count for display.</summary>
    public string LoadsText => LoadCount.ToString("N0");

    /// <summary>Formatted label count for display.</summary>
    public string LabelsText => LabelCount.ToString("N0");
}

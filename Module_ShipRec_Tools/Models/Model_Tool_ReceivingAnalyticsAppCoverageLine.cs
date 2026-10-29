using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One row of the app's coverage counts: purchase-order lines attributed to the date each line was
/// last received, split by part so the category mix can use the same rows.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppCoverageLine
{
    /// <summary>The day this line was last received. Matches how Infor Visual dates a PO line.</summary>
    public DateTime LastReceivedDate { get; set; }

    /// <summary>
    /// The part number. The category is derived from this, not from the stored part type, which is
    /// blank on nearly every receipt row.
    /// </summary>
    public string PartId { get; set; } = string.Empty;

    /// <summary>How many purchase-order lines fall on that date for that part.</summary>
    public int LineCount { get; set; }
}

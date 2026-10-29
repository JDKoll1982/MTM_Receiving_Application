using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One on-time bucket for the Receiving Analytics App Data tab: the date bucket,
/// the on-time status name, and the number of receipts in that bucket.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppOnTimeRow
{
    public DateTime BucketDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ReceiptCount { get; set; }
}

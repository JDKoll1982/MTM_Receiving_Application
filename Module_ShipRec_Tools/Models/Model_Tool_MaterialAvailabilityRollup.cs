using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Aggregated PO progress summary shown on a material availability card.
/// </summary>
public class Model_Tool_MaterialAvailabilityRollup
{
    public decimal OrderedQty { get; set; }

    public decimal ReceivedQty { get; set; }

    public decimal RemainingQty { get; set; }

    public int PurchaseOrderCount { get; set; }

    public int POLineCount { get; set; }

    public int PercentComplete =>
        OrderedQty <= 0
            ? 0
            : (int)Math.Round((ReceivedQty / OrderedQty) * 100M, 0, MidpointRounding.AwayFromZero);

    public string OrderedQtyDisplay => FormatWhole(OrderedQty);

    public string ReceivedQtyDisplay => FormatWhole(ReceivedQty);

    public string RemainingQtyDisplay => FormatWhole(RemainingQty);

    public string PurchaseOrderCountSummary => $"{PurchaseOrderCount} PO(s)";

    public string POLineCountSummary => $"{POLineCount} line(s)";

    public string PercentCompleteSummary => $"{PercentComplete}% complete";

    private static string FormatWhole(decimal value)
    {
        return decimal.Round(value, 0, MidpointRounding.AwayFromZero).ToString("0");
    }
}

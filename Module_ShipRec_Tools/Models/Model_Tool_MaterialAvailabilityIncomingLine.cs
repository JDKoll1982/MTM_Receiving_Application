using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI model for one incoming-material detail line shown in the incoming-material modal.
/// </summary>
public sealed class Model_Tool_MaterialAvailabilityIncomingLine
{
    public string PONumber { get; set; } = string.Empty;

    public string POLineNumber { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string DateLabel { get; set; } = string.Empty;

    public DateTime? Date { get; set; }

    public decimal OrderedQty { get; set; }

    public decimal ReceivedQty { get; set; }

    public decimal RemainingQty { get; set; }

    public string DateDisplay =>
        Date.HasValue ? Date.Value.ToString("MM/dd/yyyy") : "No qualifying date";

    public string OrderedQtyDisplay => FormatWhole(OrderedQty);

    public string ReceivedQtyDisplay => FormatWhole(ReceivedQty);

    public string RemainingQtyDisplay => FormatWhole(RemainingQty);

    public string PurchaseOrderLineDisplay =>
        string.IsNullOrWhiteSpace(POLineNumber) ? PONumber : $"{PONumber} / Line {POLineNumber}";

    private static string FormatWhole(decimal value)
    {
        return decimal.Round(value, 0, MidpointRounding.AwayFromZero).ToString("0");
    }
}

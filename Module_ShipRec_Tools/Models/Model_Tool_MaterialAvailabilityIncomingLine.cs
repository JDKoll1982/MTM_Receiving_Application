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

    /// <summary>
    /// "Confirmed" when the date comes from a PO or PO-line promise date, "Requested" when it comes
    /// from a desired-receive date, and "Received" for a blanket line's last receipt.
    /// </summary>
    public string DeliveryConfidence { get; set; } = string.Empty;

    /// <summary>
    /// True when the date falls beyond the selected highlight window. The row is still shown; the
    /// window only changes the wording and ordering.
    /// </summary>
    public bool IsOutsideHighlightWindow { get; set; }

    /// <summary>True when the date has passed while quantity is still outstanding.</summary>
    public bool IsOverdue { get; set; }

    public decimal OrderedQty { get; set; }

    public decimal ReceivedQty { get; set; }

    public decimal RemainingQty { get; set; }

    public string DateDisplay =>
        Date.HasValue ? Date.Value.ToString("MM/dd/yyyy") : "No delivery date on the PO";

    public int OverdueDays => IsOverdue && Date.HasValue
        ? (DateTime.Today - Date.Value.Date).Days
        : 0;

    /// <summary>
    /// Secondary note shown beside the date in the incoming-material modal, for example
    /// "Overdue by 5 days" or "Beyond 30-day highlight".
    /// </summary>
    public string StatusNoteDisplay
    {
        get
        {
            var overdueNote = IsOverdue
                ? $"Overdue by {OverdueDays} {(OverdueDays == 1 ? "day" : "days")}"
                : string.Empty;
            var windowNote = IsOutsideHighlightWindow ? "Beyond highlight window" : string.Empty;

            return (overdueNote, windowNote) switch
            {
                ("", "") => string.Empty,
                ("", _) => windowNote,
                (_, "") => overdueNote,
                _ => $"{overdueNote} - {windowNote}",
            };
        }
    }

    public bool HasStatusNote => string.IsNullOrWhiteSpace(StatusNoteDisplay) is false;

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

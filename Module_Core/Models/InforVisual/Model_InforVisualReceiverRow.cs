using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Raw read-only receiver row returned from Infor Visual: one row per receiver and part,
/// with received, ordered and remaining quantities. A view of source-system data that is
/// never persisted locally.
/// </summary>
public sealed class Model_InforVisualReceiverRow
{
    /// <summary>
    /// Receiver number from Infor Visual.
    /// </summary>
    public string ReceiverNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date the receiver was received.
    /// </summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>
    /// Purchase order number the receiver line was received against.
    /// </summary>
    public string PONumber { get; set; } = string.Empty;

    /// <summary>
    /// PO line number for the receiver line.
    /// </summary>
    public string POLineNumber { get; set; } = string.Empty;

    /// <summary>
    /// Part number received on the receiver line.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Description of the part from the part master, blank when the master has none.
    /// </summary>
    public string PartDescription { get; set; } = string.Empty;

    /// <summary>
    /// Purchase unit of measure on the representative PO line, so a load is not recorded as EA
    /// when the line is received in pounds or feet.
    /// </summary>
    public string UnitOfMeasure { get; set; } = "EA";

    /// <summary>
    /// Vendor name for the purchase order, falling back to the vendor identifier when no name exists.
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Warehouse the receiving line names.
    /// </summary>
    public string Warehouse { get; set; } = string.Empty;

    /// <summary>
    /// Location the receiver line was received to.
    /// </summary>
    public string LocationReceivedTo { get; set; } = string.Empty;

    /// <summary>
    /// Location as the list shows it: the warehouse alone when the line names no location.
    /// </summary>
    public string LocationReceivedToDisplay =>
        string.IsNullOrWhiteSpace(LocationReceivedTo) ? Warehouse : LocationReceivedTo;

    /// <summary>
    /// Total quantity received for this receiver and part.
    /// </summary>
    public decimal QtyReceived { get; set; }

    /// <summary>
    /// Quantity ordered on the PO line.
    /// </summary>
    public decimal QtyOrdered { get; set; }

    /// <summary>
    /// Quantity remaining on the PO line, clamped to zero when over-received.
    /// </summary>
    public decimal QtyRemaining { get; set; }

    /// <summary>
    /// Identifies the row for the list highlight as <c>receiver|part|line</c>.
    /// </summary>
    public string SelectionKey => $"{ReceiverNumber}|{PartNumber}|{POLineNumber}";

    /// <summary>
    /// Quantity received as the list shows it: up to two decimal places, trailing zeros trimmed.
    /// </summary>
    public string QtyReceivedDisplay => QtyReceived.ToString("0.##");

    /// <summary>
    /// Quantity ordered as the list shows it: up to two decimal places, trailing zeros trimmed.
    /// </summary>
    public string QtyOrderedDisplay => QtyOrdered.ToString("0.##");

    /// <summary>
    /// Quantity remaining as the list shows it: up to two decimal places, trailing zeros trimmed.
    /// </summary>
    public string QtyRemainingDisplay => QtyRemaining.ToString("0.##");
}

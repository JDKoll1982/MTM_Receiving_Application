using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Presentation row for the Delivery Schedule grid. Wraps the raw Infor Visual
/// line plus derived display/formatting members used by x:Bind.
/// </summary>
public partial class Model_Tool_DeliveryScheduleLine : ObservableObject
{
    public string PoNumber { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public DateTime? PoDesiredDate { get; set; }

    public DateTime? PoPromiseDate { get; set; }

    /// <summary>Purchase order creation/order date.</summary>
    public DateTime? OrderDate { get; set; }

    /// <summary>Ship-via / carrier code (PO header).</summary>
    public string Carrier { get; set; } = string.Empty;

    public string PartNumber { get; set; } = string.Empty;

    public decimal OrderQty { get; set; }

    public decimal ReceivedQty { get; set; }

    public decimal RemainingQty { get; set; }

    public DateTime? LineDesiredDate { get; set; }

    public DateTime? LinePromiseDate { get; set; }

    public string PoStatus { get; set; } = string.Empty;

    public string LineStatus { get; set; } = string.Empty;

    public string ReceivedBy { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public string Category { get; set; } = string.Empty;

    public string DeliveryState { get; set; } = string.Empty;

    public string PoState { get; set; } = string.Empty;

    /// <summary>Formatted PO desired date, or "-".</summary>
    public string PoDesiredDateText => PoDesiredDate?.ToShortDateString() ?? "-";

    /// <summary>Formatted PO promise date, or "-".</summary>
    public string PoPromiseDateText => PoPromiseDate?.ToShortDateString() ?? "-";

    /// <summary>Formatted PO order date, or "-".</summary>
    public string OrderDateText => OrderDate?.ToShortDateString() ?? "-";

    /// <summary>Carrier code, or "-".</summary>
    public string CarrierText => string.IsNullOrWhiteSpace(Carrier) ? "-" : Carrier;

    /// <summary>Formatted due date, or "-".</summary>
    public string DueDateText => DueDate?.ToShortDateString() ?? "-";

    /// <summary>Status dot key for the PO Number cell: Closed | Late | Partial | OnTime.</summary>
    public string StatusDotKey =>
        DeliveryState == "Closed"
            ? "Closed"
            : PoState == "Late"
                ? "Late"
                : DeliveryState == "Partial"
                    ? "Partial"
                    : "OnTime";

    /// <summary>Formatted line desired date, or "-".</summary>
    public string LineDesiredDateText => LineDesiredDate?.ToShortDateString() ?? "-";

    /// <summary>Formatted line promise date, or "-".</summary>
    public string LinePromiseDateText => LinePromiseDate?.ToShortDateString() ?? "-";

    /// <summary>Order qty with thousands separator.</summary>
    public string OrderQtyText => OrderQty.ToString("N0");

    /// <summary>Received qty with thousands separator.</summary>
    public string ReceivedQtyText => ReceivedQty.ToString("N0");

    /// <summary>Remaining qty with thousands separator.</summary>
    public string RemainingQtyText => RemainingQty.ToString("N0");

    /// <summary>Human-readable delivery state label (Open/Partial/Closed).</summary>
    public string DeliveryStateLabel => DeliveryState;

    /// <summary>Human-readable PO state label (On Time / Late).</summary>
    public string PoStateLabel => PoState == "Late" ? "Late" : "On Time";
}

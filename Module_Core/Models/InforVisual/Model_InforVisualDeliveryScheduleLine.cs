using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// One receiving-schedule grid row (per PO line) returned from the read-only
/// Infor Visual (MTMFG) database for the Delivery Schedule tool.
/// </summary>
public class Model_InforVisualDeliveryScheduleLine
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

    /// <summary>Parts | MMC Coils | MMF Flat | Outside Service | Uninventoried.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Open | Partial | Closed.</summary>
    public string DeliveryState { get; set; } = string.Empty;

    /// <summary>OnTime | Late.</summary>
    public string PoState { get; set; } = string.Empty;
}

using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Raw read-only incoming PO-line row returned from Infor Visual for the material availability board.
/// </summary>
public class Model_InforVisualIncomingSupplyRow
{
    public string PartId { get; set; } = string.Empty;

    public string PartDescription { get; set; } = string.Empty;

    public string WarehouseCode { get; set; } = string.Empty;

    public string PONumber { get; set; } = string.Empty;

    public string POLineNumber { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string PoStatus { get; set; } = string.Empty;

    public decimal OrderedQty { get; set; }

    public decimal ReceivedQty { get; set; }

    public decimal RemainingQty { get; set; }

    public DateTime? LineDesiredReceiveDate { get; set; }

    public DateTime? LinePromiseDate { get; set; }

    public DateTime? LinePromiseShipDate { get; set; }

    public DateTime? LineLastReceivedDate { get; set; }

    public DateTime? HeaderPromiseDate { get; set; }

    public DateTime? HeaderPromiseShipDate { get; set; }

    public DateTime? HeaderDesiredReceiveDate { get; set; }

    public DateTime? OrderDate { get; set; }

    public string FreeOnBoard { get; set; } = string.Empty;

    public bool IsBlanketOrder { get; set; }
}

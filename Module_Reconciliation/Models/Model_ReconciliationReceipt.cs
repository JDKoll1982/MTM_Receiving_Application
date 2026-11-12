using System;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One Infor Visual receipt that landed straight in a warehouse location, with no
/// transfer pair behind it. The location is where the material actually went, so a
/// group with no movements can still be adjudicated from it (FR-015, LG-15).
/// </summary>
public sealed class Model_ReconciliationReceipt
{
    /// <summary>The received part.</summary>
    public string PartId { get; init; } = string.Empty;

    /// <summary>The warehouse the material was received into.</summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>The location the material was received into.</summary>
    public string LocationId { get; init; } = string.Empty;

    /// <summary>The quantity received into the location.</summary>
    public decimal Quantity { get; init; }

    /// <summary>The purchase order the receipt is booked against, when any.</summary>
    public string PurchaseOrderNumber { get; init; } = string.Empty;

    /// <summary>The purchase order line the receipt is booked against, when any.</summary>
    public string PurchaseOrderLineNumber { get; init; } = string.Empty;

    /// <summary>The real posting timestamp of the receipt.</summary>
    public DateTime ReceiptDateTime { get; init; }

    /// <summary>The Infor Visual user who posted the receipt.</summary>
    public string ReceiptUserId { get; init; } = string.Empty;

    /// <summary>The receipt's transaction id.</summary>
    public int TransactionId { get; init; }
}

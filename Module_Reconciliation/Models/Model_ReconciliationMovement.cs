using System;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One resolved Infor Visual location transfer. Quantity is always positive and
/// the direction is carried by which leg is the source, never by the value's
/// sign and never by transaction id ordering (FR-030, decision D-04).
/// </summary>
public sealed class Model_ReconciliationMovement
{
    /// <summary>The transferred part.</summary>
    public string PartId { get; init; } = string.Empty;

    /// <summary>The purchase order the movement is booked against, when any.</summary>
    public string PurchaseOrderNumber { get; init; } = string.Empty;

    /// <summary>The purchase order line the movement is booked against, when any.</summary>
    public string PurchaseOrderLineNumber { get; init; } = string.Empty;

    /// <summary>The warehouse the material left.</summary>
    public string SourceWarehouseId { get; init; } = string.Empty;

    /// <summary>The location the material left.</summary>
    public string SourceLocationId { get; init; } = string.Empty;

    /// <summary>The warehouse the material arrived in.</summary>
    public string DestinationWarehouseId { get; init; } = string.Empty;

    /// <summary>The location the material arrived in.</summary>
    public string DestinationLocationId { get; init; } = string.Empty;

    /// <summary>The transferred quantity. Always positive.</summary>
    public decimal Quantity { get; init; }

    /// <summary>The destination leg's posting time, which is when the material arrived.</summary>
    public DateTime TransactionDate { get; init; }

    /// <summary>The Visual user attributed to the move.</summary>
    public string TransactionUserId { get; init; } = string.Empty;

    /// <summary>The outbound leg's transaction id.</summary>
    public int SourceTransactionId { get; init; }

    /// <summary>The inbound leg's transaction id.</summary>
    public int DestinationTransactionId { get; init; }

    /// <summary>True when both legs name the same warehouse and location.</summary>
    public bool IsSameLocation =>
        string.Equals(SourceWarehouseId, DestinationWarehouseId, StringComparison.OrdinalIgnoreCase)
        && string.Equals(SourceLocationId, DestinationLocationId, StringComparison.OrdinalIgnoreCase);
}

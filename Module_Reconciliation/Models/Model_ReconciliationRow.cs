using System;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One saved receiving line, which is the unit the operator corrects.
/// <see cref="Quantity"/> is the integer column and is the only field the
/// quantity proof reads (FR-027). <see cref="WeightQuantity"/> is carried for
/// display and is never compared (decision D-05).
/// </summary>
public sealed class Model_ReconciliationRow
{
    /// <summary>Which table the row was read from.</summary>
    public Enum_ReconciliationRowSource RowSource { get; init; }

    /// <summary>The row's own id in its source table.</summary>
    public int RecordId { get; init; }

    /// <summary>The session load identity, when the row has one.</summary>
    public Guid? LoadId { get; init; }

    /// <summary>The load number inside the session. An allocation tie-break.</summary>
    public int? LoadNumber { get; init; }

    /// <summary>The part identifier. Required.</summary>
    public string PartId { get; init; } = string.Empty;

    /// <summary>The part description, shown in the detail pane.</summary>
    public string PartDescription { get; init; } = string.Empty;

    /// <summary>The purchase order number. Empty for a non-purchase-order row.</summary>
    public string PurchaseOrderNumber { get; init; } = string.Empty;

    /// <summary>The purchase order line number, used for evidence matching.</summary>
    public string PurchaseOrderLineNumber { get; init; } = string.Empty;

    /// <summary>The day the row is scoped to (FR-013, R-1).</summary>
    public DateOnly TransactionDate { get; init; }

    /// <summary>The receipt timestamp, for display and ordering only.</summary>
    public DateTime? ReceivedAt { get; init; }

    /// <summary>The integer quantity. The proof compares only this value.</summary>
    public int Quantity { get; init; }

    /// <summary>The decimal weight quantity. Display only, never compared.</summary>
    public decimal? WeightQuantity { get; init; }

    /// <summary>The unit of measure. Display and warning only (FR-028).</summary>
    public string UnitOfMeasure { get; init; } = string.Empty;

    /// <summary>The saved location, which is the value being corrected.</summary>
    public string SavedLocation { get; init; } = string.Empty;

    /// <summary>The vendor name, used to group non-purchase-order rows.</summary>
    public string VendorName { get; init; } = string.Empty;

    /// <summary>True when the row has no purchase order and selects the non-PO resolver.</summary>
    public bool IsNonPurchaseOrder { get; init; }

    /// <summary>True when the row is a reprint and is excluded from candidates (FR-016).</summary>
    public bool IsReprint { get; init; }
}

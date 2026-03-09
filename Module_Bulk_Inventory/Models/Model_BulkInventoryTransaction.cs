using System;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Models;

/// <summary>
/// Represents one row in the bulk_inventory_transactions MySQL table.
/// Maps 1-to-1 with the schema created by T004.
/// </summary>
public class Model_BulkInventoryTransaction
{
    public int Id { get; set; }

    /// <summary>MTM application username of the person who created this batch row.</summary>
    public string CreatedByUser { get; set; } = string.Empty;

    /// <summary>Infor Visual username used to drive the automation session.</summary>
    public string VisualUsername { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Enum_BulkInventoryTransactionType TransactionType { get; set; }

    public string PartId { get; set; } = string.Empty;

    /// <summary>Warehouse code for the source location (Transfer mode only).</summary>
    public string? FromWarehouse { get; set; }

    /// <summary>Location code for the source (Transfer mode only).</summary>
    public string? FromLocation { get; set; }

    /// <summary>Warehouse code for the destination.</summary>
    public string ToWarehouse { get; set; } = string.Empty;

    /// <summary>Location code for the destination.</summary>
    public string ToLocation { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    /// <summary>Work order number in Visual format — e.g. WO-123456 (NewTransaction mode only).</summary>
    public string? WorkOrder { get; set; }

    /// <summary>Lot number (NewTransaction mode only; defaults to "1").</summary>
    public string? LotNo { get; set; }

    public Enum_BulkInventoryStatus Status { get; set; } = Enum_BulkInventoryStatus.Pending;

    /// <summary>Set when <see cref="Status"/> is <see cref="Enum_BulkInventoryStatus.Failed"/>.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Optional inline warning surfaced by ValidateAllCommand (does not block UI entry).
    /// Not persisted to the database.
    /// </summary>
    public string? ValidationMessage { get; set; }

    /// <summary>
    /// UI-only flag: the user has checked this failed row for re-push in the Summary view.
    /// Not persisted to the database.
    /// </summary>
    public bool IsSelectedForRepush { get; set; }
}

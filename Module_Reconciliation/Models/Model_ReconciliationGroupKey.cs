using System;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The identity of one pool of movement evidence. Two keys compare equal when
/// they name the same purchase order, part, day and vendor, so callers never
/// reason about raw string joins (FR-015, decision D-06).
/// </summary>
public sealed record Model_ReconciliationGroupKey(
    string PurchaseOrderNumber,
    string PartId,
    DateOnly TransactionDate,
    string VendorKey
)
{
    /// <summary>
    /// A stable text form for logs. It names every component so a log line can
    /// be traced back to the rows that produced it (FR-069).
    /// </summary>
    public string DisplayText =>
        $"{PartId}|{PurchaseOrderNumber}|{VendorKey}|{TransactionDate:yyyy-MM-dd}";
}

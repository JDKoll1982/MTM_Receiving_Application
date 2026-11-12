using System;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// A location that received net quantity from the group's receipt, after chained
/// movements resolved. <see cref="NetQuantity"/> is what the movement delivered
/// and never changes; <see cref="RemainingQuantity"/> falls as rows are bound to
/// it during allocation (FR-017, FR-022).
/// </summary>
public sealed class Model_ReconciliationDestinationBucket
{
    /// <summary>The warehouse the destination sits in.</summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>The location the module proposes.</summary>
    public string LocationId { get; init; } = string.Empty;

    /// <summary>The inbound minus outbound quantity across the selected movements.</summary>
    public decimal NetQuantity { get; init; }

    /// <summary>The quantity still available to bind rows to.</summary>
    public decimal RemainingQuantity { get; set; }

    /// <summary>The sum of the row quantities bound to this bucket.</summary>
    public decimal AllocatedQuantity { get; set; }

    /// <summary>The latest inbound movement time. The allocation tie-break and pane evidence.</summary>
    public DateTime LatestMovementAt { get; init; }

    /// <summary>The Visual user attributed to the latest inbound movement.</summary>
    public string TransactionUserId { get; init; } = string.Empty;

    /// <summary>The number of inbound legs that fed this bucket.</summary>
    public int MovementCount { get; init; }

    /// <summary>The human-readable chain of locations that reached this bucket.</summary>
    public string EvidenceLocations { get; init; } = string.Empty;

    /// <summary>True when any leg at this location took its running balance below zero.</summary>
    public bool RanNegative { get; init; }

    /// <summary>Binds a row quantity to this bucket and reduces what remains.</summary>
    /// <param name="quantity">The row quantity to bind.</param>
    public void Bind(decimal quantity)
    {
        AllocatedQuantity += quantity;
        RemainingQuantity -= quantity;
    }
}

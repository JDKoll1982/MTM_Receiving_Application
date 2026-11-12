namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One row bound to one destination bucket. Allocation produces these, and the
/// quantity proof consumes them (FR-018, FR-022).
/// </summary>
public sealed class Model_ReconciliationAllocation
{
    /// <summary>The row that was placed.</summary>
    public Model_ReconciliationRow Row { get; init; } = new();

    /// <summary>The bucket the row was placed in.</summary>
    public Model_ReconciliationDestinationBucket Bucket { get; init; } = new();

    /// <summary>The row's quantity, as bound.</summary>
    public decimal Quantity { get; init; }

    /// <summary>True when the bucket's remaining quantity equalled the row quantity.</summary>
    public bool MatchedExactly { get; init; }

    /// <summary>True when the saved-location pass bound it rather than the general pass.</summary>
    public bool BoundBySavedLocation { get; init; }
}

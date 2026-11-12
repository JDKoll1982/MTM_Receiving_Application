using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The verdict the quantity proof hands back for one group: a verdict per bucket
/// plus the source residual (FR-022, FR-023).
/// </summary>
public sealed class Model_ReconciliationProofResult
{
    /// <summary>One verdict per destination bucket.</summary>
    public IReadOnlyList<Model_ReconciliationBucketVerdict> BucketVerdicts { get; init; } =
        new List<Model_ReconciliationBucketVerdict>();

    /// <summary>Part two of the proof.</summary>
    public Model_ReconciliationSourceResidual Residual { get; init; } = new();

    /// <summary>
    /// True only when every bucket verdict passed and the residual agrees with
    /// the as-of balance.
    /// </summary>
    public bool Passed { get; init; }
}

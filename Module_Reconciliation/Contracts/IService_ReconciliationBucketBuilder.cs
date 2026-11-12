using System.Collections.Generic;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Resolves chained transfers into net destination buckets (FR-017, FR-025).
/// </summary>
public interface IService_ReconciliationBucketBuilder
{
    /// <summary>
    /// Computes the net quantity per location from the selected movements. A
    /// location whose net is zero drops out; one that retains quantity stays a
    /// bucket for that remainder.
    /// </summary>
    /// <param name="movements">The selected movement evidence.</param>
    IReadOnlyList<Model_ReconciliationDestinationBucket> Build(
        IReadOnlyList<Model_ReconciliationMovement> movements
    );
}

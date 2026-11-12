using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Binds rows to destination buckets deterministically, with exact decimal
/// arithmetic and no unit gate (FR-018, FR-026, FR-028).
/// </summary>
public interface IService_ReconciliationAllocator
{
    /// <summary>
    /// Runs the saved-location pass first and then the general pass, returning one
    /// allocation per bound row. A row that finds no bucket gets no allocation.
    /// </summary>
    /// <param name="rows">The rows competing for this group's buckets.</param>
    /// <param name="buckets">The destination buckets, mutated as rows are bound.</param>
    Model_Dao_Result<IReadOnlyList<Model_ReconciliationAllocation>> Allocate(
        IReadOnlyList<Model_ReconciliationRow> rows,
        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets
    );
}

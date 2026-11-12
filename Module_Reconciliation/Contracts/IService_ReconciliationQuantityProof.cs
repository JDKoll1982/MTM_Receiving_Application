using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// The only place a line becomes writable. Part one drives every destination
/// bucket to exactly zero; part two checks the source residual against the as-of
/// balance at the receiving location (FR-022, FR-023).
/// </summary>
public interface IService_ReconciliationQuantityProof
{
    /// <summary>Runs both parts and returns a verdict per bucket plus the residual.</summary>
    /// <param name="group">The group being proved.</param>
    /// <param name="allocations">The rows bound to destination buckets.</param>
    Task<Model_Dao_Result<Model_ReconciliationProofResult>> VerifyAsync(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationAllocation> allocations
    );
}

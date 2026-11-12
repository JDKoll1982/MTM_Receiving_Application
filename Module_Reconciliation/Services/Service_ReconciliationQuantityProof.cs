using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// The only gate a line passes before it becomes writable. Part one drives every
/// destination bucket to exactly zero; part two checks that whatever remains in
/// the receiving location equals the as-of balance there (FR-022, FR-023, FR-024).
/// It depends on no database and no view-model state, so it is exercised directly
/// by tests.
/// </summary>
public sealed class Service_ReconciliationQuantityProof : IService_ReconciliationQuantityProof
{
    /// <inheritdoc />
    public Task<Model_Dao_Result<Model_ReconciliationProofResult>> VerifyAsync(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationAllocation> allocations
    )
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(allocations);

        var verdicts = new List<Model_ReconciliationBucketVerdict>();

        foreach (Model_ReconciliationDestinationBucket bucket in group.Buckets)
        {
            decimal allocated = allocations
                .Where(allocation => ReferenceEquals(allocation.Bucket, bucket))
                .Sum(allocation => allocation.Quantity);

            verdicts.Add(
                new Model_ReconciliationBucketVerdict
                {
                    Bucket = bucket,
                    NetQuantity = bucket.NetQuantity,
                    AllocatedQuantity = allocated,
                    RanNegative = bucket.RanNegative,
                }
            );
        }

        Model_ReconciliationSourceResidual residual = BuildResidual(group, allocations);
        bool passed = verdicts.TrueForAll(verdict => verdict.Passed) && residual.Agrees;

        return Task.FromResult(
            Model_Dao_Result_Factory.Success(
                new Model_ReconciliationProofResult
                {
                    BucketVerdicts = verdicts,
                    Residual = residual,
                    Passed = passed,
                }
            )
        );
    }

    private static Model_ReconciliationSourceResidual BuildResidual(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationAllocation> allocations
    )
    {
        decimal totalRowQuantity = group.Rows.Sum(row => (decimal)row.Quantity);
        decimal allocatedToDestinations = allocations.Sum(allocation => allocation.Quantity);

        Model_ReconciliationLocationBalance? balance = group.Balances.FirstOrDefault(candidate =>
            Helper_ReconciliationCanonical.LocationsEqual(
                candidate.LocationId,
                group.ReceivingLocation
            )
        );

        return new Model_ReconciliationSourceResidual
        {
            ReceivingLocation = group.ReceivingLocation,
            ExpectedResidual = totalRowQuantity - allocatedToDestinations,
            BalanceAsOf = balance?.BalanceAsOf,
            BalanceSource =
                balance is null
                    ? string.Empty
                    : "CR_PART_LOCATION on-hand less post-window movement",
        };
    }
}

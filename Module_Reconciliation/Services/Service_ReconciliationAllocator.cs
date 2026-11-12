using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Binds rows to destination buckets deterministically. The saved-location pass
/// runs before the general pass so a row already sitting in a legitimate
/// destination is never moved off it (FR-029, decision D-03). Arithmetic is exact
/// <see cref="decimal"/>, there is no tolerance, and unit of measure is never
/// consulted (FR-026, FR-028).
/// </summary>
public sealed class Service_ReconciliationAllocator : IService_ReconciliationAllocator
{
    /// <inheritdoc />
    public Model_Dao_Result<IReadOnlyList<Model_ReconciliationAllocation>> Allocate(
        IReadOnlyList<Model_ReconciliationRow> rows,
        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets
    )
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(buckets);

        List<Model_ReconciliationRow> orderedRows = rows
            .Where(row => row.Quantity > 0)
            .OrderByDescending(row => row.Quantity)
            .ThenBy(row => row.LoadNumber ?? int.MaxValue)
            .ThenBy(row => row.LoadId ?? Guid.Empty)
            .ToList();

        List<Model_ReconciliationDestinationBucket> allocatable = buckets
            .Where(bucket => bucket.RanNegative is false && bucket.RemainingQuantity > 0m)
            .ToList();

        var allocations = new List<Model_ReconciliationAllocation>();
        var boundRows = new HashSet<Model_ReconciliationRow>();

        BindSavedLocations(orderedRows, allocatable, allocations, boundRows);

        foreach (Model_ReconciliationRow row in orderedRows)
        {
            if (boundRows.Contains(row))
            {
                continue;
            }

            Model_ReconciliationDestinationBucket? bucket = ChooseBucket(row, allocatable);

            if (bucket is null)
            {
                continue;
            }

            allocations.Add(Bind(row, bucket, boundBySavedLocation: false));
        }

        return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ReconciliationAllocation>>(
            allocations
        );
    }

    private static void BindSavedLocations(
        IReadOnlyList<Model_ReconciliationRow> orderedRows,
        IReadOnlyList<Model_ReconciliationDestinationBucket> allocatable,
        List<Model_ReconciliationAllocation> allocations,
        HashSet<Model_ReconciliationRow> boundRows
    )
    {
        foreach (Model_ReconciliationRow row in orderedRows)
        {
            Model_ReconciliationDestinationBucket? bucket = allocatable
                .Where(candidate =>
                    Helper_ReconciliationCanonical.LocationsEqual(
                        candidate.LocationId,
                        row.SavedLocation
                    )
                    && candidate.RemainingQuantity >= row.Quantity
                )
                .OrderBy(candidate => candidate.RemainingQuantity - row.Quantity)
                .ThenByDescending(candidate => candidate.LatestMovementAt)
                .ThenBy(candidate => candidate.LocationId, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (bucket is null)
            {
                continue;
            }

            allocations.Add(Bind(row, bucket, boundBySavedLocation: true));
            boundRows.Add(row);
        }
    }

    private static Model_ReconciliationDestinationBucket? ChooseBucket(
        Model_ReconciliationRow row,
        IReadOnlyList<Model_ReconciliationDestinationBucket> allocatable
    )
    {
        Model_ReconciliationDestinationBucket? exactFit = allocatable
            .Where(candidate => candidate.RemainingQuantity == row.Quantity)
            .OrderByDescending(candidate => candidate.LatestMovementAt)
            .ThenBy(candidate => candidate.LocationId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        if (exactFit is not null)
        {
            return exactFit;
        }

        return allocatable
            .Where(candidate => candidate.RemainingQuantity > row.Quantity)
            .OrderBy(candidate => candidate.RemainingQuantity)
            .ThenByDescending(candidate => candidate.LatestMovementAt)
            .ThenBy(candidate => candidate.LocationId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static Model_ReconciliationAllocation Bind(
        Model_ReconciliationRow row,
        Model_ReconciliationDestinationBucket bucket,
        bool boundBySavedLocation
    )
    {
        bool matchedExactly = bucket.RemainingQuantity == row.Quantity;

        bucket.Bind(row.Quantity);

        return new Model_ReconciliationAllocation
        {
            Row = row,
            Bucket = bucket,
            Quantity = row.Quantity,
            MatchedExactly = matchedExactly,
            BoundBySavedLocation = boundBySavedLocation,
        };
    }
}

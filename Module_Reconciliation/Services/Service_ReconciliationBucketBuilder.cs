using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Resolves chained transfers into net destination buckets. A location is a
/// candidate only when at least one movement arrived in it, so the location the
/// material was received into is never itself a destination bucket. An
/// intermediate location that forwards everything nets to zero and drops out; one
/// that retains quantity stays a bucket for that remainder (FR-017, FR-022, FR-025).
/// </summary>
public sealed class Service_ReconciliationBucketBuilder : IService_ReconciliationBucketBuilder
{
    /// <inheritdoc />
    public IReadOnlyList<Model_ReconciliationDestinationBucket> Build(
        IReadOnlyList<Model_ReconciliationMovement> movements
    )
    {
        ArgumentNullException.ThrowIfNull(movements);

        var inboundTotals = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var outboundTotals = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var inboundLegs = new Dictionary<string, List<Model_ReconciliationMovement>>(
            StringComparer.Ordinal
        );
        var locationLegs = new Dictionary<string, List<LocationLeg>>(StringComparer.Ordinal);

        foreach (Model_ReconciliationMovement movement in movements)
        {
            string sourceKey = Helper_ReconciliationCanonical.Normalize(movement.SourceLocationId);
            string destinationKey = Helper_ReconciliationCanonical.Normalize(
                movement.DestinationLocationId
            );

            Accumulate(inboundTotals, destinationKey, movement.Quantity);
            Accumulate(outboundTotals, sourceKey, movement.Quantity);

            AppendLeg(
                locationLegs,
                destinationKey,
                movement.TransactionDate,
                movement.DestinationTransactionId,
                movement.Quantity
            );
            AppendLeg(
                locationLegs,
                sourceKey,
                movement.TransactionDate,
                movement.SourceTransactionId,
                -movement.Quantity
            );

            if (inboundLegs.TryGetValue(destinationKey, out var legs) is false)
            {
                legs = [];
                inboundLegs[destinationKey] = legs;
            }

            legs.Add(movement);
        }

        var buckets = new List<Model_ReconciliationDestinationBucket>();

        foreach (string locationKey in inboundLegs.Keys)
        {
            decimal netQuantity = inboundTotals[locationKey] - ReadTotal(outboundTotals, locationKey);

            if (netQuantity <= 0m)
            {
                continue;
            }

            List<Model_ReconciliationMovement> evidence = inboundLegs[locationKey];
            Model_ReconciliationMovement latestInbound = evidence
                .OrderByDescending(movement => movement.TransactionDate)
                .ThenByDescending(movement => movement.DestinationTransactionId)
                .First();

            buckets.Add(
                new Model_ReconciliationDestinationBucket
                {
                    WarehouseId = latestInbound.DestinationWarehouseId.Trim(),
                    LocationId = latestInbound.DestinationLocationId.Trim(),
                    NetQuantity = netQuantity,
                    RemainingQuantity = netQuantity,
                    LatestMovementAt = latestInbound.TransactionDate,
                    TransactionUserId = latestInbound.TransactionUserId,
                    MovementCount = evidence.Count,
                    EvidenceLocations = BuildEvidence(evidence, latestInbound.DestinationLocationId),
                    RanNegative = RanNegative(locationLegs[locationKey]),
                }
            );
        }

        return buckets
            .OrderByDescending(bucket => bucket.NetQuantity)
            .ThenByDescending(bucket => bucket.LatestMovementAt)
            .ThenBy(bucket => bucket.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void Accumulate(Dictionary<string, decimal> totals, string key, decimal quantity)
    {
        if (totals.TryGetValue(key, out decimal existing) is false)
        {
            totals[key] = quantity;
            return;
        }

        totals[key] = existing + quantity;
    }

    private static decimal ReadTotal(Dictionary<string, decimal> totals, string key) =>
        totals.TryGetValue(key, out decimal total) ? total : 0m;

    private static void AppendLeg(
        Dictionary<string, List<LocationLeg>> locationLegs,
        string key,
        DateTime at,
        int transactionId,
        decimal delta
    )
    {
        if (locationLegs.TryGetValue(key, out var legs) is false)
        {
            legs = [];
            locationLegs[key] = legs;
        }

        legs.Add(new LocationLeg(at, transactionId, delta));
    }

    private static bool RanNegative(List<LocationLeg> legs)
    {
        decimal running = 0m;

        foreach (LocationLeg leg in legs.OrderBy(leg => leg.At).ThenBy(leg => leg.TransactionId))
        {
            running += leg.Delta;

            if (running < 0m)
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildEvidence(
        IReadOnlyCollection<Model_ReconciliationMovement> inbound,
        string destinationLocation
    )
    {
        IEnumerable<string> sources = inbound
            .Select(movement => movement.SourceLocationId.Trim())
            .Where(source => source.Length != 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(source => source, StringComparer.OrdinalIgnoreCase);

        return string.Join(" → ", sources.Append(destinationLocation.Trim()));
    }

    private readonly record struct LocationLeg(DateTime At, int TransactionId, decimal Delta);
}

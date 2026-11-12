using System;
using System.Collections.Generic;
using FluentAssertions;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// The bucket builder resolves chained transfers into net destinations (FR-017, FR-025).
/// </summary>
public sealed class Service_ReconciliationBucketBuilderTests
{
    private readonly Service_ReconciliationBucketBuilder _builder = new();

    [Fact]
    public void BuildShouldDropLocationThatFullyForwards()
    {
        var movements = new List<Model_ReconciliationMovement>
        {
            Movement("RECV", "SHIP", 5_000m, 1, 2),
            Movement("SHIP", "V-C0-01", 5_000m, 3, 4),
        };

        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _builder.Build(movements);

        buckets.Should().ContainSingle().Which.LocationId.Should().Be("V-C0-01");
        buckets[0].NetQuantity.Should().Be(5_000m);
    }

    [Fact]
    public void BuildShouldKeepLocationThatRetainsQuantity()
    {
        var movements = new List<Model_ReconciliationMovement>
        {
            Movement("RECV", "SHIP", 5_000m, 1, 2),
            Movement("SHIP", "V-C0-01", 3_000m, 3, 4),
        };

        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _builder.Build(movements);

        buckets.Should().HaveCount(2);
        buckets.Should().Contain(bucket => bucket.LocationId == "SHIP" && bucket.NetQuantity == 2_000m);
        buckets.Should().Contain(bucket => bucket.LocationId == "V-C0-01" && bucket.NetQuantity == 3_000m);
    }

    [Fact]
    public void BuildShouldRefuseToMakeBucketOfDestinationThatRanNegative()
    {
        // SHIP ends the window owing material: 500 arrived and 800 left, so its net
        // is negative and it is not a bucket at all (FR-025).
        var movements = new List<Model_ReconciliationMovement>
        {
            Movement("RECV", "SHIP", 500m, 1, 2, at: new DateTime(2026, 11, 12, 8, 0, 0)),
            Movement("SHIP", "V-C0-01", 800m, 3, 4, at: new DateTime(2026, 11, 12, 9, 0, 0)),
        };

        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _builder.Build(movements);

        buckets.Should().ContainSingle().Which.LocationId.Should().Be("V-C0-01");
        buckets.Should().NotContain(bucket => bucket.LocationId == "SHIP");
    }

    [Fact]
    public void BuildShouldMarkBucketThatDippedBelowZeroDuringTheWindow()
    {
        // SHIP ends the window holding 1,000, but the 800 left before the 1,800
        // arrived, so its running balance went below zero inside the window.
        var movements = new List<Model_ReconciliationMovement>
        {
            Movement("SHIP", "V-C0-01", 800m, 1, 2, at: new DateTime(2026, 11, 12, 8, 0, 0)),
            Movement("RECV", "SHIP", 1_800m, 3, 4, at: new DateTime(2026, 11, 12, 9, 0, 0)),
        };

        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _builder.Build(movements);

        Model_ReconciliationDestinationBucket ship = buckets.Should()
            .ContainSingle(bucket => bucket.LocationId == "SHIP")
            .Which;

        ship.NetQuantity.Should().Be(1_000m);
        ship.RanNegative.Should().BeTrue();
    }

    [Fact]
    public void BuildShouldOrderBucketsDeterministically()
    {
        var movements = new List<Model_ReconciliationMovement>
        {
            Movement("RECV", "R-04", 500m, 1, 2),
            Movement("RECV", "SHIP", 5_000m, 3, 4),
            Movement("RECV", "V-C0-01", 2_500m, 5, 6),
        };

        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _builder.Build(movements);

        buckets[0].LocationId.Should().Be("SHIP");
        buckets[1].LocationId.Should().Be("V-C0-01");
        buckets[2].LocationId.Should().Be("R-04");
    }

    private static Model_ReconciliationMovement Movement(
        string source,
        string destination,
        decimal quantity,
        int sourceTransactionId,
        int destinationTransactionId,
        DateTime? at = null
    ) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            SourceWarehouseId = "W1",
            SourceLocationId = source,
            DestinationWarehouseId = "W1",
            DestinationLocationId = destination,
            Quantity = quantity,
            TransactionDate =
                at ?? new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
            TransactionUserId = "vuser",
            SourceTransactionId = sourceTransactionId,
            DestinationTransactionId = destinationTransactionId,
        };
}

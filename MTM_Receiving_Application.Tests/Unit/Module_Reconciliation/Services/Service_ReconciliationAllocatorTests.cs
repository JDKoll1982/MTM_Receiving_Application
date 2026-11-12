using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// Allocation is deterministic, exact and unit-blind (FR-018, FR-026, FR-028, FR-029).
/// </summary>
public sealed class Service_ReconciliationAllocatorTests
{
    private readonly Service_ReconciliationAllocator _allocator = new();

    [Fact]
    public void AllocateShouldBindSavedLocationBeforeTheGeneralPass()
    {
        var savedBucket = Bucket("V-C0-01", 2_500m);
        var otherBucket = Bucket("SHIP", 2_500m);
        Model_ReconciliationRow row = Row(2_500, savedLocation: "V-C0-01");

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [row],
            [otherBucket, savedBucket]
        );

        allocations.Should().ContainSingle();
        allocations[0].Bucket.Should().BeSameAs(savedBucket);
        allocations[0].BoundBySavedLocation.Should().BeTrue();
    }

    [Fact]
    public void AllocateShouldPreferAnExactFitOverASmallerConsumingBucket()
    {
        var largeBucket = Bucket("SHIP", 20m);
        var exactBucket = Bucket("V-C0-01", 5m);
        Model_ReconciliationRow row = Row(5, savedLocation: "RECV");

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [row],
            [largeBucket, exactBucket]
        );

        allocations.Should().ContainSingle();
        allocations[0].Bucket.Should().BeSameAs(exactBucket);
        allocations[0].MatchedExactly.Should().BeTrue();
        largeBucket.RemainingQuantity.Should().Be(20m);
    }

    [Fact]
    public void AllocateShouldOtherwiseTakeTheSmallestSufficientBucket()
    {
        var largerBucket = Bucket("SHIP", 20m);
        var smallerSufficient = Bucket("V-C0-01", 8m);
        Model_ReconciliationRow row = Row(5, savedLocation: "RECV");

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [row],
            [largerBucket, smallerSufficient]
        );

        allocations.Should().ContainSingle();
        allocations[0].Bucket.Should().BeSameAs(smallerSufficient);
        allocations[0].MatchedExactly.Should().BeFalse();
    }

    [Fact]
    public void AllocateShouldOrderRowsByQuantityThenLoadNumberThenLoadId()
    {
        var bucket = Bucket("SHIP", 50m);
        Model_ReconciliationRow higherLoadNumber = Row(50, loadNumber: 2, loadId: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        Model_ReconciliationRow lowerLoadNumber = Row(50, loadNumber: 1, loadId: Guid.Parse("00000000-0000-0000-0000-000000000002"));

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [higherLoadNumber, lowerLoadNumber],
            [bucket]
        );

        allocations.Should().ContainSingle();
        allocations[0].Row.Should().BeSameAs(lowerLoadNumber);
    }

    [Fact]
    public void AllocateShouldNeverGateOnUnitOfMeasure()
    {
        var bucket = Bucket("SHIP", 40m);
        Model_ReconciliationRow pounds = Row(40, unitOfMeasure: "LB");
        Model_ReconciliationRow each = Row(0, unitOfMeasure: "EA");

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [pounds, each],
            [bucket]
        );

        allocations.Should().ContainSingle();
        allocations[0].Row.Should().BeSameAs(pounds);
    }

    [Fact]
    public void AllocateShouldLeaveRowUnallocatedWhenNothingCoversIt()
    {
        var bucket = Bucket("SHIP", 10m);
        Model_ReconciliationRow tooLarge = Row(50);

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [tooLarge],
            [bucket]
        );

        allocations.Should().BeEmpty();
        bucket.RemainingQuantity.Should().Be(10m);
    }

    [Fact]
    public void AllocateShouldNeverDriveABucketBelowZero()
    {
        var bucket = Bucket("SHIP", 30m);

        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(
            [Row(20), Row(20), Row(10)],
            [bucket]
        );

        bucket.RemainingQuantity.Should().Be(0m);
        allocations.Sum(allocation => allocation.Quantity).Should().Be(30m);
    }

    private IReadOnlyList<Model_ReconciliationAllocation> Allocate(
        IReadOnlyList<Model_ReconciliationRow> rows,
        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets
    )
    {
        var result = _allocator.Allocate(rows, buckets);
        result.IsSuccess.Should().BeTrue();

        return result.Data ?? [];
    }

    private static Model_ReconciliationDestinationBucket Bucket(string location, decimal quantity) =>
        new()
        {
            WarehouseId = "W1",
            LocationId = location,
            NetQuantity = quantity,
            RemainingQuantity = quantity,
            LatestMovementAt = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
        };

    private static Model_ReconciliationRow Row(
        int quantity,
        string savedLocation = "RECV",
        int? loadNumber = null,
        Guid? loadId = null,
        string unitOfMeasure = "EA"
    ) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = new DateOnly(2026, 11, 12),
            Quantity = quantity,
            SavedLocation = savedLocation,
            LoadNumber = loadNumber,
            LoadId = loadId,
            UnitOfMeasure = unitOfMeasure,
        };
}

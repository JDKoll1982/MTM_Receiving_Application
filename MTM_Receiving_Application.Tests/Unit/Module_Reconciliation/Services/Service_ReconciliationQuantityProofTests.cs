using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// The quantity proof is the only place a line becomes writable. The MMC0001000
/// reference case is reproduced here as a passing test (FR-022 to FR-026, FR-031).
/// </summary>
public sealed class Service_ReconciliationQuantityProofTests
{
    private readonly Service_ReconciliationBucketBuilder _bucketBuilder = new();
    private readonly Service_ReconciliationAllocator _allocator = new();
    private readonly Service_ReconciliationQuantityProof _proof = new();

    [Fact]
    public async Task VerifyShouldReproduceTheReferenceCase()
    {
        var movements = new List<Model_ReconciliationMovement>
        {
            Movement("RECV", "SHIP", 5_000m, 1, 2),
            Movement("RECV", "V-C0-01", 2_500m, 3, 4),
            Movement("RECV", "R-04", 500m, 5, 6),
            Movement("RECV", "R-05", 1_000m, 7, 8),
        };

        var rows = new List<Model_ReconciliationRow>
        {
            Row(5_000),
            Row(2_500),
            Row(500),
            Row(1_000),
            Row(41_000),
        };

        Model_ReconciliationGroup group = Group(rows, movements);
        IReadOnlyList<Model_ReconciliationAllocation> allocations = Allocate(group);

        Model_ReconciliationProofResult result = await Verify(group, allocations);

        result.BucketVerdicts.Should().HaveCount(4);
        result.BucketVerdicts.Should().OnlyContain(verdict => verdict.ConsumedToZero);
        result.Residual.ExpectedResidual.Should().Be(41_000m);
        result.Residual.BalanceAsOf.Should().Be(41_000m);
        result.Residual.Agrees.Should().BeTrue();
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyShouldFailTheResidualWhenTheBalanceDiffers()
    {
        Model_ReconciliationGroup group = Group(
            [Row(41_000)],
            [Movement("RECV", "SHIP", 9_000m, 1, 2)],
            onHand: 40_999m
        );

        Model_ReconciliationProofResult result = await Verify(group, Allocate(group));

        result.Residual.Difference.Should().Be(1m);
        result.Residual.Agrees.Should().BeFalse();
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyShouldFailABucketThatRanNegative()
    {
        Model_ReconciliationDestinationBucket negativeBucket = Bucket("SHIP", 100m, ranNegative: true);
        Model_ReconciliationGroup group = Group([Row(100)], [], onHand: 0m);
        group.Buckets = [negativeBucket];

        var allocations = new List<Model_ReconciliationAllocation>
        {
            new() { Row = group.Rows[0], Bucket = negativeBucket, Quantity = 100m },
        };

        Model_ReconciliationProofResult result = await Verify(group, allocations);

        result.BucketVerdicts.Should().ContainSingle();
        result.BucketVerdicts[0].Passed.Should().BeFalse();
        result.BucketVerdicts[0].OutcomeWhenFailed.Should().Be(Enum_ReconciliationOutcome.NotEnough);
    }

    [Fact]
    public async Task VerifyShouldUseExactArithmeticWithNoTolerance()
    {
        Model_ReconciliationDestinationBucket bucket = Bucket("SHIP", 100m);
        Model_ReconciliationGroup group = Group([Row(100)], [], onHand: 0m);
        group.Buckets = [bucket];

        var allocations = new List<Model_ReconciliationAllocation>
        {
            new() { Row = group.Rows[0], Bucket = bucket, Quantity = 99.999m },
        };

        Model_ReconciliationProofResult result = await Verify(group, allocations);

        result.BucketVerdicts[0].Remainder.Should().Be(0.001m);
        result.BucketVerdicts[0].Passed.Should().BeFalse();
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyShouldFailWhenNoBalanceWasReturnedForTheReceivingLocation()
    {
        Model_ReconciliationGroup group = Group([Row(41_000)], [], onHand: 41_000m);
        group.Balances = [];

        Model_ReconciliationProofResult result = await Verify(group, []);

        result.Residual.BalanceAsOf.Should().BeNull();
        result.Residual.Agrees.Should().BeFalse();
        result.Passed.Should().BeFalse();
    }

    private async Task<Model_ReconciliationProofResult> Verify(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationAllocation> allocations
    )
    {
        Model_Dao_Result<Model_ReconciliationProofResult> result = await _proof.VerifyAsync(
            group,
            allocations
        );

        result.IsSuccess.Should().BeTrue();

        return result.Data!;
    }

    private IReadOnlyList<Model_ReconciliationAllocation> Allocate(Model_ReconciliationGroup group)
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationAllocation>> result =
            _allocator.Allocate(group.Rows, group.Buckets);

        result.IsSuccess.Should().BeTrue();

        return result.Data!;
    }

    private Model_ReconciliationGroup Group(
        IReadOnlyList<Model_ReconciliationRow> rows,
        IReadOnlyList<Model_ReconciliationMovement> movements,
        decimal onHand = 41_000m
    )
    {
        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _bucketBuilder.Build(
            movements
        );

        return new Model_ReconciliationGroup
        {
            Key = new Model_ReconciliationGroupKey(
                "PO-066868",
                "MMC0001000",
                new DateOnly(2026, 11, 12),
                string.Empty
            ),
            Rows = rows,
            SourceLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "RECV" },
            ReceivingLocation = "RECV",
            Movements = movements,
            Buckets = buckets,
            Balances =
            [
                new Model_ReconciliationLocationBalance
                {
                    PartId = "MMC0001000",
                    WarehouseId = "W1",
                    LocationId = "RECV",
                    OnHand = onHand,
                    MovementAfterWindow = 0m,
                },
            ],
        };
    }

    private static Model_ReconciliationDestinationBucket Bucket(
        string location,
        decimal quantity,
        bool ranNegative = false
    ) =>
        new()
        {
            WarehouseId = "W1",
            LocationId = location,
            NetQuantity = quantity,
            RemainingQuantity = quantity,
            LatestMovementAt = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
            RanNegative = ranNegative,
        };

    private static Model_ReconciliationRow Row(int quantity) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = new DateOnly(2026, 11, 12),
            Quantity = quantity,
            SavedLocation = "RECV",
            UnitOfMeasure = "EA",
        };

    private static Model_ReconciliationMovement Movement(
        string source,
        string destination,
        decimal quantity,
        int sourceTransactionId,
        int destinationTransactionId
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
            TransactionDate = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
            TransactionUserId = "vuser",
            SourceTransactionId = sourceTransactionId,
            DestinationTransactionId = destinationTransactionId,
        };
}

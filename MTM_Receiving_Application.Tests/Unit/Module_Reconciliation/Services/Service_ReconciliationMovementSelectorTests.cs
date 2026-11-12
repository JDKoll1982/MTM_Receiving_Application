using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// The movement selector admits only the evidence reachable from one group's own
/// seeds, and follows a chain to its end (FR-017, FR-030).
/// </summary>
public sealed class Service_ReconciliationMovementSelectorTests
{
    private static readonly DateOnly WorkingDay = new(2026, 11, 12);

    private readonly Service_ReconciliationMovementSelector _selector = new();

    [Fact]
    public void SelectShouldAdmitMovementReachableBySourceLocation()
    {
        Model_ReconciliationGroup group = Group(["RECV"], Row("RECV", 100));
        Model_ReconciliationMovement reachable = Movement("RECV", "SHIP", 40m, 1, 2);
        Model_ReconciliationMovement unrelated = Movement(
            "OTHER",
            "SOMEWHERE",
            60m,
            3,
            4,
            purchaseOrderNumber: "PO-099999"
        );

        IReadOnlyList<Model_ReconciliationMovement> selected = _selector.Select(
            group,
            [reachable, unrelated]
        );

        selected.Should().ContainSingle().Which.Should().BeSameAs(reachable);
    }

    [Fact]
    public void SelectShouldAdmitMovementReachableByPurchaseOrderKey()
    {
        Model_ReconciliationGroup group = Group(["RECV"], Row("RECV", 100));
        Model_ReconciliationMovement byPurchaseOrder = Movement(
            "REMOTE",
            "SHIP",
            40m,
            1,
            2,
            purchaseOrderNumber: "066868",
            purchaseOrderLineNumber: "1"
        );

        IReadOnlyList<Model_ReconciliationMovement> selected = _selector.Select(
            group,
            [byPurchaseOrder]
        );

        selected.Should().ContainSingle().Which.Should().BeSameAs(byPurchaseOrder);
    }

    [Fact]
    public void SelectShouldRepeatUntilNothingMoreIsAdmitted()
    {
        Model_ReconciliationGroup group = Group(["RECV"], Row("RECV", 100));
        Model_ReconciliationMovement firstLeg = Movement("RECV", "SHIP", 40m, 1, 2);
        Model_ReconciliationMovement secondLeg = Movement("SHIP", "V-C0-01", 40m, 3, 4);
        Model_ReconciliationMovement thirdLeg = Movement("V-C0-01", "R-04", 40m, 5, 6);

        IReadOnlyList<Model_ReconciliationMovement> selected = _selector.Select(
            group,
            [thirdLeg, secondLeg, firstLeg]
        );

        selected.Should().HaveCount(3);
        selected.Should().ContainInOrder(firstLeg, secondLeg, thirdLeg);
    }

    [Fact]
    public void SelectShouldDiscardMovementWhoseLegsAreTheSameLocation()
    {
        Model_ReconciliationGroup group = Group(["RECV"], Row("RECV", 100));
        Model_ReconciliationMovement sameLocation = Movement("RECV", "recv", 40m, 1, 2);

        IReadOnlyList<Model_ReconciliationMovement> selected = _selector.Select(
            group,
            [sameLocation]
        );

        selected.Should().BeEmpty();
    }

    [Fact]
    public void SelectShouldReturnAStableOrder()
    {
        Model_ReconciliationGroup group = Group(["RECV"], Row("RECV", 100));
        Model_ReconciliationMovement later = Movement(
            "SHIP",
            "V-C0-01",
            10m,
            3,
            9,
            when: new DateTime(2026, 11, 13, 8, 0, 0, DateTimeKind.Unspecified)
        );
        Model_ReconciliationMovement earlier = Movement(
            "RECV",
            "SHIP",
            10m,
            1,
            2,
            when: new DateTime(2026, 11, 12, 8, 0, 0, DateTimeKind.Unspecified)
        );

        IReadOnlyList<Model_ReconciliationMovement> selected = _selector.Select(
            group,
            [later, earlier]
        );

        selected.Should().ContainInOrder(earlier, later);
    }

    private static Model_ReconciliationGroup Group(string[] locations, params Model_ReconciliationRow[] rows) =>
        new()
        {
            Key = new Model_ReconciliationGroupKey("PO-066868", "MMC0001000", WorkingDay, string.Empty),
            Rows = rows,
            SourceLocations = locations.ToHashSet(StringComparer.OrdinalIgnoreCase),
            ReceivingLocation = locations[0],
        };

    private static Model_ReconciliationRow Row(string savedLocation, int quantity) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = WorkingDay,
            Quantity = quantity,
            SavedLocation = savedLocation,
        };

    private static Model_ReconciliationMovement Movement(
        string source,
        string destination,
        decimal quantity,
        int sourceTransactionId,
        int destinationTransactionId,
        string purchaseOrderNumber = "PO-066868",
        string purchaseOrderLineNumber = "1",
        DateTime? when = null
    ) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = purchaseOrderNumber,
            PurchaseOrderLineNumber = purchaseOrderLineNumber,
            SourceWarehouseId = "W1",
            SourceLocationId = source,
            DestinationWarehouseId = "W1",
            DestinationLocationId = destination,
            Quantity = quantity,
            TransactionDate =
                when ?? new DateTime(2026, 11, 12, 8, 0, 0, DateTimeKind.Unspecified),
            TransactionUserId = "vuser",
            SourceTransactionId = sourceTransactionId,
            DestinationTransactionId = destinationTransactionId,
        };
}

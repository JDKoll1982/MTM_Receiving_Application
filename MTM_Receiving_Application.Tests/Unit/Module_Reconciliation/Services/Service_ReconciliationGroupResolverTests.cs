using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// The group key carries the purchase order as well as the part and the day, and
/// both row sources reconcile together (FR-015, FR-019, FR-020, decision D-06).
/// </summary>
public sealed class Service_ReconciliationGroupResolverTests
{
    private static readonly DateOnly WorkingDay = new(2026, 11, 12);

    private readonly Mock<IService_ReconciliationNonPurchaseOrderResolver> _nonPurchaseOrderResolver =
        new();

    private readonly Service_ReconciliationGroupResolver _resolver;

    public Service_ReconciliationGroupResolverTests()
    {
        _nonPurchaseOrderResolver
            .Setup(resolver => resolver.Resolve(It.IsAny<IReadOnlyList<Model_ReconciliationRow>>()))
            .Returns(
                (IReadOnlyList<Model_ReconciliationRow> rows) =>
                    Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ReconciliationGroup>>(
                        rows.GroupBy(row => row.VendorName)
                            .Select(group => new Model_ReconciliationGroup
                            {
                                Key = new Model_ReconciliationGroupKey(
                                    string.Empty,
                                    Helper_ReconciliationCanonical.Normalize(group.First().PartId),
                                    group.First().TransactionDate,
                                    Helper_ReconciliationCanonical.Normalize(group.Key)
                                ),
                                Rows = group.ToList(),
                                SourceLocations = new HashSet<string>(StringComparer.Ordinal)
                                {
                                    Helper_ReconciliationCanonical.Normalize(
                                        group.First().SavedLocation
                                    ),
                                },
                                ReceivingLocation = group.First().SavedLocation,
                            })
                            .ToList()
                    )
            );

        _resolver = new Service_ReconciliationGroupResolver(_nonPurchaseOrderResolver.Object);
    }

    [Fact]
    public void ResolveShouldMakeOneGroupOfOnePurchaseOrderAndPartOnOneDay()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [Row("PO-066868", 1_000), Row("PO-066868", 2_000)]
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].Rows.Should().HaveCount(2);
        result.Data[0].Key.PurchaseOrderNumber.Should().Be("PO-066868");
    }

    [Fact]
    public void ResolveShouldMakeTwoGroupsWhenThePurchaseOrderDiffersOnTheSameDay()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [Row("PO-066868", 1_000), Row("PO-066914", 1_000)]
        );

        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public void ResolveShouldMakeTwoGroupsWhenTheDayDiffers()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [
                Row("PO-066868", 1_000),
                Row("PO-066868", 1_000, day: WorkingDay.AddDays(-1)),
            ]
        );

        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public void ResolveShouldReconcileCurrentLabelAndHistoryRowsTogether()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [
                Row("PO-066868", 1_000, rowSource: Enum_ReconciliationRowSource.CurrentLabel),
                Row("PO-066868", 1_000, rowSource: Enum_ReconciliationRowSource.History),
            ]
        );

        result.Data.Should().ContainSingle();
        result.Data![0].Rows.Should().HaveCount(2);
    }

    [Fact]
    public void ResolveShouldRouteNonPurchaseOrderRowsToTheInjectedResolver()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [Row("PO-066868", 1_000), NonPurchaseOrderRow("ACME STEEL")]
        );

        result.Data.Should().HaveCount(2);
        _nonPurchaseOrderResolver.Verify(
            resolver => resolver.Resolve(It.IsAny<IReadOnlyList<Model_ReconciliationRow>>()),
            Times.Once
        );
        result.Data!.Should().Contain(group => group.Key.VendorKey == "ACME STEEL");
    }

    [Fact]
    public void ResolveShouldNotGroupARowThatFailsCandidacy()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [Row("PO-066868", 1_000), Row("NOT-A-PO", 1_000)]
        );

        result.Data.Should().ContainSingle();
    }

    private static Model_ReconciliationRow Row(
        string purchaseOrderNumber,
        int quantity,
        DateOnly? day = null,
        Enum_ReconciliationRowSource rowSource = Enum_ReconciliationRowSource.CurrentLabel
    ) =>
        new()
        {
            RowSource = rowSource,
            PartId = "MMC0001000",
            PurchaseOrderNumber = purchaseOrderNumber,
            PurchaseOrderLineNumber = "1",
            TransactionDate = day ?? WorkingDay,
            Quantity = quantity,
            SavedLocation = "RECV",
            UnitOfMeasure = "EA",
        };

    private static Model_ReconciliationRow NonPurchaseOrderRow(string vendor) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = string.Empty,
            TransactionDate = WorkingDay,
            Quantity = 500,
            SavedLocation = "RECV",
            VendorName = vendor,
            IsNonPurchaseOrder = true,
            UnitOfMeasure = "EA",
        };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// A receipt without a purchase order is assessed, never skipped (FR-020, LG-13).
/// </summary>
public sealed class Service_ReconciliationNonPurchaseOrderResolverTests
{
    private const string VisualConnectionString =
        "Data Source=localhost;Initial Catalog=Test;Integrated Security=True;ApplicationIntent=ReadOnly;";

    private static readonly DateOnly WorkingDay = new(2026, 11, 12);

    private readonly Service_ReconciliationNonPurchaseOrderResolver _resolver = new();

    [Fact]
    public void ResolveShouldGroupByPartDayAndVendor()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [
                NonPurchaseOrderRow("ACME STEEL", 500),
                NonPurchaseOrderRow("ACME STEEL", 500),
                NonPurchaseOrderRow("OTHER MILL", 500),
            ]
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.Should().Contain(group => group.Key.VendorKey == "OTHER MILL");
        result.Data.Should().Contain(group => group.Key.VendorKey == "ACME STEEL" && group.Rows.Count == 2);
    }

    [Fact]
    public void ResolveShouldGroupARowWithoutAVendorUnderAnEmptyVendorKey()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [NonPurchaseOrderRow(string.Empty, 500)]
        );

        result.Data.Should().ContainSingle();
        result.Data![0].Key.VendorKey.Should().BeEmpty();
        result.Data[0].Rows.Should().ContainSingle();
    }

    [Fact]
    public void ResolveShouldNeverSkipARowForLackingAPurchaseOrder()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [NonPurchaseOrderRow("ACME STEEL", 500)]
        );

        result.Data!.SelectMany(group => group.Rows).Should().ContainSingle();
        Service_ReconciliationNonPurchaseOrderResolver
            .GetAssessmentFailureReason(NonPurchaseOrderRow("ACME STEEL", 500))
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void ResolveShouldProduceGroupsOfTheSameShapeAsThePurchaseOrderResolver()
    {
        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> result = _resolver.Resolve(
            [NonPurchaseOrderRow("ACME STEEL", 500)]
        );

        Model_ReconciliationGroup group = result.Data![0];
        group.SourceLocations.Should().Contain("RECV");
        group.ReceivingLocation.Should().Be("RECV");
        group.Key.PartId.Should().Be("MMC0001000");
    }

    [Fact]
    public async Task ARowWithoutAVendorShouldReachNotEnoughWithAReasonNamingTheVendor()
    {
        Service_ReconciliationPreviewOrchestrator orchestrator = Orchestrator(
            NonPurchaseOrderRow(string.Empty, 500)
        );

        Model_Dao_Result<Model_ReconciliationDay> result = await orchestrator.PreviewDayAsync(
            WorkingDay
        );

        Model_ReconciliationFinding finding = result.Data!.Findings.Should().ContainSingle().Which;
        finding.Outcome.Should().Be(Enum_ReconciliationOutcome.NotEnough);
        finding.Reason.Should().Contain("vendor");
        result.Data.Summary.Scanned.Should().Be(1);
        result.Data.Summary.TotalAccounted.Should().Be(1);
    }

    private static Service_ReconciliationPreviewOrchestrator Orchestrator(
        params Model_ReconciliationRow[] rows
    )
    {
        var dayReader = new Mock<IService_ReconciliationDayReader>();
        dayReader.Setup(reader => reader.GetDayAsync(WorkingDay))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_ReconciliationDay
                    {
                        Date = WorkingDay,
                        HasActivity = true,
                        Rows = rows,
                        State = Enum_ReconciliationDayState.Loaded,
                        Summary = new Model_ReconciliationDaySummary { Scanned = rows.Length },
                    }
                )
            );

        var inforVisual = new Mock<Dao_ReconciliationInforVisual>(
            VisualConnectionString,
            Mock.Of<IService_LoggingUtility>()
        );
        inforVisual
            .Setup(dao => dao.GetTransferMovementsAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_ReconciliationMovement>
                    {
                        new()
                        {
                            PartId = "MMC0001000",
                            SourceWarehouseId = "W1",
                            SourceLocationId = "RECV",
                            DestinationWarehouseId = "W1",
                            DestinationLocationId = "SHIP",
                            Quantity = 100m,
                            TransactionDate = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
                            SourceTransactionId = 1,
                            DestinationTransactionId = 2,
                        },
                    }
                )
            );
        inforVisual
            .Setup(dao => dao.GetLocationBalancesAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReconciliationLocationBalance>())
            );

        var settings = new Mock<IService_ReconciliationSettings>();
        settings.Setup(service => service.GetDiscoveryWindowDaysAsync()).ReturnsAsync(2);
        settings.Setup(service => service.GetStrictWindowDaysAsync()).ReturnsAsync(1);

        return new Service_ReconciliationPreviewOrchestrator(
            dayReader.Object,
            new Service_ReconciliationGroupResolver(new Service_ReconciliationNonPurchaseOrderResolver()),
            new Service_ReconciliationMovementSelector(),
            new Service_ReconciliationBucketBuilder(),
            new Service_ReconciliationAllocator(),
            new Service_ReconciliationQuantityProof(),
            settings.Object,
            inforVisual.Object
        );
    }

    private static Model_ReconciliationRow NonPurchaseOrderRow(string vendor, int quantity) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = string.Empty,
            TransactionDate = WorkingDay,
            Quantity = quantity,
            SavedLocation = "RECV",
            VendorName = vendor,
            IsNonPurchaseOrder = true,
            UnitOfMeasure = "EA",
        };
}

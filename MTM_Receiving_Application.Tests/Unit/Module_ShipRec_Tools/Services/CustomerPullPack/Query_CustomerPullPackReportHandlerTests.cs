using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Query_CustomerPullPackReportHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDemandLines_WhenDaoSucceeds()
    {
        var filter = CreateFilter();
        var expectedLines = new List<Model_CustomerPullPack_DemandLine>
        {
            new()
            {
                SourceLineKey = "CO-1001|10|1",
                CustomerId = "CUST-100",
                CustomerOrderId = "CO-1001",
                ParentPartId = "PART-100",
            },
        };

        var demandSourceMock = new Mock<IService_CustomerPullPackDemandSource>();
        demandSourceMock
            .Setup(service => service.GetDemandAsync(filter))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expectedLines, expectedLines.Count));
        var waitlistSourceMock = new Mock<IService_CustomerPullPackWaitlistSource>();

        var loggerMock = new Mock<IService_LoggingUtility>();
        var handler = new Query_CustomerPullPackReportHandler(
            demandSourceMock.Object,
            waitlistSourceMock.Object,
            loggerMock.Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedLines);
        demandSourceMock.Verify(service => service.GetDemandAsync(filter), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectRequest_WhenCustomerIdIsMissing()
    {
        var filter = CreateFilter();
        filter.CustomerId = string.Empty;

        var demandSourceMock = new Mock<IService_CustomerPullPackDemandSource>();
        var waitlistSourceMock = new Mock<IService_CustomerPullPackWaitlistSource>();

        var handler = new Query_CustomerPullPackReportHandler(
            demandSourceMock.Object,
            waitlistSourceMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Customer ID is required.");
        demandSourceMock.Verify(
            service => service.GetDemandAsync(It.IsAny<Model_CustomerPullPack_DemandFilter>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnDemandLineMetadata_FromResolvedSource()
    {
        var filter = CreateFilter();
        var expectedLines = new List<Model_CustomerPullPack_DemandLine>
        {
            new()
            {
                SourceLineKey = "CO-1001|10|1",
                CustomerId = "CUST-100",
                CustomerName = "Acme Automotive",
                CustomerOrderId = "CO-1001",
                ParentPartId = "PART-100",
                HasLinkedWaitlist = true,
                LinkedWaitlistId = "WL-1001",
                WaitlistStateDisplay = "Requested (WL-1001)",
                RequesterNote = "Pull before lunch",
                LocationOptions =
                [
                    new Model_CustomerPullPack_LocationOption
                    {
                        LocationKey = "CO-1001|10|1|SUB-01",
                        LocationId = "SUB-01",
                        DisplayLabel = "SUB-01 (22)",
                    },
                ],
            },
        };

        var demandSourceMock = new Mock<IService_CustomerPullPackDemandSource>();
        demandSourceMock
            .Setup(service => service.GetDemandAsync(filter))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expectedLines, expectedLines.Count));

        var waitlistSourceMock = new Mock<IService_CustomerPullPackWaitlistSource>();
        waitlistSourceMock
            .Setup(service => service.GetByIdAsync("WL-1001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_WaitlistEntry
                    {
                        WaitlistId = "WL-1001",
                        SourceLineKey = "CO-1001|10|1",
                        CustomerId = "CUST-100",
                        CustomerOrderId = "CO-1001",
                        ParentPartId = "PART-100",
                        RequestedQuantity = 12,
                        SelectedLocations = ["SUB-01"],
                        CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested,
                    }
                )
            );

        var handler = new Query_CustomerPullPackReportHandler(
            demandSourceMock.Object,
            waitlistSourceMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].WaitlistStateDisplay.Should().Be("Requested (WL-1001)");
        result.Data[0].LocationOptions.Should().ContainSingle();
        result.Data[0].LocationOptions[0].LocationId.Should().Be("SUB-01");
        result.Data[0].RequesterNote.Should().Be("Pull before lunch");
    }

    [Fact]
    public async Task Handle_ShouldFlagRecheckIndicator_WhenCompletedLineContextChanges()
    {
        var filter = CreateFilter();
        var expectedLines = new List<Model_CustomerPullPack_DemandLine>
        {
            new()
            {
                SourceLineKey = "CO-1001|10|1",
                CustomerId = "CUST-100",
                CustomerOrderId = "CO-1001",
                ParentPartId = "PART-100",
                QuantityToPack = 12,
                HasLinkedWaitlist = true,
                LinkedWaitlistId = "WL-1001",
                LocationOptions =
                [
                    new Model_CustomerPullPack_LocationOption
                    {
                        LocationKey = "CO-1001|10|1|SUB-01",
                        LocationId = "SUB-01",
                        DisplayLabel = "SUB-01",
                    },
                ],
            },
        };

        var demandSourceMock = new Mock<IService_CustomerPullPackDemandSource>();
        demandSourceMock
            .Setup(service => service.GetDemandAsync(filter))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expectedLines, expectedLines.Count));

        var waitlistSourceMock = new Mock<IService_CustomerPullPackWaitlistSource>();
        waitlistSourceMock
            .Setup(service => service.GetByIdAsync("WL-1001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_WaitlistEntry
                    {
                        WaitlistId = "WL-1001",
                        SourceLineKey = "CO-1001|10|1",
                        CustomerId = "CUST-100",
                        CustomerOrderId = "CO-1001",
                        ParentPartId = "PART-100",
                        RequestedQuantity = 10,
                        SelectedLocations = ["SUB-01"],
                        CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Completed,
                    }
                )
            );

        var handler = new Query_CustomerPullPackReportHandler(
            demandSourceMock.Object,
            waitlistSourceMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].RecheckIndicator.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldAllocateQtySatisfiedOldestFirstPerParentPartGroup()
    {
        var filter = CreateFilter();
        var demandLines = new List<Model_CustomerPullPack_DemandLine>
        {
            new()
            {
                SourceLineKey = "LINE-1",
                CustomerId = "CUST-100",
                CustomerOrderId = "CO-1001",
                ParentPartId = "PART-100",
                ShipQuantity = 10,
                QuantityToPack = 10,
                FgOnHandQuantity = 15,
                OldestAdded = new DateTime(2026, 5, 20),
            },
            new()
            {
                SourceLineKey = "LINE-2",
                CustomerId = "CUST-100",
                CustomerOrderId = "CO-1002",
                ParentPartId = "PART-100",
                ShipQuantity = 10,
                QuantityToPack = 10,
                FgOnHandQuantity = 15,
                OldestAdded = new DateTime(2026, 5, 21),
            },
            new()
            {
                SourceLineKey = "LINE-3",
                CustomerId = "CUST-100",
                CustomerOrderId = "CO-1003",
                ParentPartId = "PART-100",
                ShipQuantity = 10,
                QuantityToPack = 10,
                FgOnHandQuantity = 15,
                OldestAdded = new DateTime(2026, 5, 22),
            },
            new()
            {
                SourceLineKey = "LINE-4",
                CustomerId = "CUST-100",
                CustomerOrderId = "CO-2001",
                ParentPartId = "PART-200",
                ShipQuantity = 8,
                QuantityToPack = 8,
                FgOnHandQuantity = 8,
                OldestAdded = new DateTime(2026, 5, 20),
            },
        };

        var demandSourceMock = new Mock<IService_CustomerPullPackDemandSource>();
        demandSourceMock
            .Setup(service => service.GetDemandAsync(filter))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(demandLines, demandLines.Count));

        var handler = new Query_CustomerPullPackReportHandler(
            demandSourceMock.Object,
            new Mock<IService_CustomerPullPackWaitlistSource>().Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Single(line => line.SourceLineKey == "LINE-1").QtySatisfied.Should().Be(10);
        result
            .Data.Single(line => line.SourceLineKey == "LINE-1")
            .FulfillmentStatusDisplay.Should()
            .Be("Complete");
        result.Data.Single(line => line.SourceLineKey == "LINE-2").QtySatisfied.Should().Be(5);
        result
            .Data.Single(line => line.SourceLineKey == "LINE-2")
            .FulfillmentStatusDisplay.Should()
            .Be("Partially Filled");
        result.Data.Single(line => line.SourceLineKey == "LINE-3").QtySatisfied.Should().Be(0);
        result
            .Data.Single(line => line.SourceLineKey == "LINE-3")
            .FulfillmentStatusDisplay.Should()
            .BeEmpty();
        result.Data.Single(line => line.SourceLineKey == "LINE-4").QtySatisfied.Should().Be(8);
        result
            .Data.Single(line => line.SourceLineKey == "LINE-4")
            .FulfillmentStatusDisplay.Should()
            .Be("Complete");
    }

    private static Model_CustomerPullPack_DemandFilter CreateFilter()
    {
        return new Model_CustomerPullPack_DemandFilter
        {
            CustomerId = "CUST-100",
            CustomerName = "Acme Automotive",
            DateFrom = new DateTime(2026, 5, 24),
            DateTo = new DateTime(2026, 5, 31),
        };
    }
}

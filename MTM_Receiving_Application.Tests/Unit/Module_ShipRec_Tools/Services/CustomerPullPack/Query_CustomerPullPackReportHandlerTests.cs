using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
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

        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(false);

        var demandDaoMock = new Mock<Dao_CustomerPullPackDemand>(
            CreateReadOnlyConnectionString(),
            appSettingsMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object
        );
        demandDaoMock
            .Setup(dao => dao.GetDemandAsync(filter))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expectedLines, expectedLines.Count));

        var loggerMock = new Mock<IService_LoggingUtility>();
        var handler = new Query_CustomerPullPackReportHandler(
            demandDaoMock.Object,
            loggerMock.Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedLines);
        demandDaoMock.Verify(dao => dao.GetDemandAsync(filter), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectRequest_WhenCustomerIdIsMissing()
    {
        var filter = CreateFilter();
        filter.CustomerId = string.Empty;

        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(false);

        var demandDaoMock = new Mock<Dao_CustomerPullPackDemand>(
            CreateReadOnlyConnectionString(),
            appSettingsMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object
        );

        var handler = new Query_CustomerPullPackReportHandler(
            demandDaoMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await handler.Handle(
            new Query_CustomerPullPackReport(filter),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Customer ID is required.");
        demandDaoMock.Verify(
            dao => dao.GetDemandAsync(It.IsAny<Model_CustomerPullPack_DemandFilter>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnMockCatalogDemand_WhenMockModeIsEnabled()
    {
        var filter = CreateFilter();
        filter.SortMode = Enum_CustomerPullPackSortMode.Part;

        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);

        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetCustomerPullPackDemandRows())
            .Returns(
                new List<Model_InforVisualCustomerPullPackDemandRow>
                {
                    new()
                    {
                        SourceLineKey = "CO-1001|10|1",
                        CustomerId = "CUST-100",
                        CustomerName = "Acme Automotive",
                        CustomerOrderId = "CO-1001",
                        ParentPartId = "PART-100",
                        SourceLocationId = "FG-A1",
                        ShipQuantity = 12,
                        PullDate = new DateTime(2026, 5, 27),
                        QuantityToPack = 12,
                        FgOnHandQuantity = 40,
                        FgLocationId = "FG-A1",
                        HasLinkedWaitlist = true,
                        LinkedWaitlistId = "WL-1001",
                        LinkedWaitlistStatus = "Requested",
                        RequesterNote = "Pull before lunch",
                    },
                }
            );
        mockCatalog
            .Setup(service => service.GetCustomerPullPackLocationRows())
            .Returns(
                new List<Model_InforVisualCustomerPullPackLocationRow>
                {
                    new()
                    {
                        LocationKey = "CO-1001|10|1|SUB-01",
                        SourceLineKey = "CO-1001|10|1",
                        ParentPartId = "PART-100",
                        LocationId = "SUB-01",
                        DisplayLabel = "SUB-01 (22)",
                        OnHandQuantity = 22,
                        SourceType = "SubPartOnHand",
                        InitiallySelected = false,
                    },
                }
            );

        var handler = new Query_CustomerPullPackReportHandler(
            new Dao_CustomerPullPackDemand(
                CreateReadOnlyConnectionString(),
                appSettingsMock.Object,
                new Mock<IService_LoggingUtility>().Object,
                mockCatalog.Object
            ),
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

    private static string CreateReadOnlyConnectionString()
    {
        return "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;Trusted_Connection=True;";
    }
}

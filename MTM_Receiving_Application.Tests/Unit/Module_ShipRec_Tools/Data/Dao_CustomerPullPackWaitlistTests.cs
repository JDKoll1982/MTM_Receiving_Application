using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Data;

public sealed class Dao_CustomerPullPackWaitlistTests
{
    [Fact]
    public async Task GetQueueAsync_ShouldReturnMockEntries_WhenMockModeIsEnabled()
    {
        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);

        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetCustomerPullPackDemandRows())
            .Returns([
                new Model_InforVisualCustomerPullPackDemandRow
                {
                    SourceLineKey = "LINE-1",
                    CustomerId = "VOLVO",
                    CustomerName = "Volvo Group",
                    CustomerOrderId = "CO-1",
                    ParentPartId = "PART-1",
                    QuantityToPack = 12,
                    PullDate = new DateTime(2026, 5, 25),
                    HasLinkedWaitlist = true,
                    LinkedWaitlistId = "CPP-WL-0001",
                    LinkedWaitlistStatus = "Requested",
                    RequesterNote = "Use mock queue",
                },
            ]);
        mockCatalog
            .Setup(service => service.GetCustomerPullPackLocationRows())
            .Returns([
                new Model_InforVisualCustomerPullPackLocationRow
                {
                    LocationKey = "LINE-1|A-01",
                    SourceLineKey = "LINE-1",
                    ParentPartId = "PART-1",
                    LocationId = "A-01",
                    DisplayLabel = "A-01",
                    OnHandQuantity = 5,
                    InitiallySelected = true,
                },
            ]);

        var dao = new Dao_CustomerPullPackWaitlist(
            "Server=localhost;",
            appSettingsMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            mockCatalog.Object
        );

        var result = await dao.GetQueueAsync(customerId: "VOLVO");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].WaitlistId.Should().Be("CPP-WL-0001");
        result.Data[0].SelectedLocations.Should().BeEquivalentTo(["A-01"]);
        result.Data[0].RequesterContextNote.Should().Be("Use mock queue");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMockEntry_WhenMockModeIsEnabled()
    {
        var appSettingsMock = new Mock<IService_AppSettings>();
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);

        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetCustomerPullPackDemandRows())
            .Returns([
                new Model_InforVisualCustomerPullPackDemandRow
                {
                    SourceLineKey = "LINE-2",
                    CustomerId = "VOLVO",
                    CustomerName = "Volvo Group",
                    CustomerOrderId = "CO-2",
                    ParentPartId = "PART-2",
                    QuantityToPack = 20,
                    PullDate = new DateTime(2026, 5, 26),
                    HasLinkedWaitlist = true,
                    LinkedWaitlistId = "CPP-WL-0002",
                    LinkedWaitlistStatus = "Completed",
                    RecheckIndicator = true,
                },
            ]);
        mockCatalog
            .Setup(service => service.GetCustomerPullPackLocationRows())
            .Returns(Array.Empty<Model_InforVisualCustomerPullPackLocationRow>());

        var dao = new Dao_CustomerPullPackWaitlist(
            "Server=localhost;",
            appSettingsMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            mockCatalog.Object
        );

        var result = await dao.GetByIdAsync("CPP-WL-0002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CurrentStatus.Should().Be(Enum_CustomerPullPackWaitlistStatus.Completed);
        result.Data.LocationReviewFlag.Should().BeTrue();
        result.Data.RecheckIndicator.Should().BeTrue();
    }
}

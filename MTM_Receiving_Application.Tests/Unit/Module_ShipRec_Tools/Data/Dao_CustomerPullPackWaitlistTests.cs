using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Data;

public sealed class Service_CustomerPullPackMockWaitlistSourceTests
{
    [Fact]
    public async Task GetQueueAsync_ShouldReturnSeededMockEntries()
    {
        var mockCatalog = new Mock<IService_CustomerPullPackMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetDemandRows())
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
            .Setup(service => service.GetLocationRows())
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

        var service = new Service_CustomerPullPackMockWaitlistSource(mockCatalog.Object);

        var result = await service.GetQueueAsync(customerId: "VOLVO");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].WaitlistId.Should().Be("CPP-WL-0001");
        result.Data[0].SelectedLocations.Should().BeEquivalentTo(["A-01"]);
        result.Data[0].RequesterContextNote.Should().Be("Use mock queue");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSeededMockEntry()
    {
        var mockCatalog = new Mock<IService_CustomerPullPackMockDataCatalog>();
        mockCatalog
            .Setup(service => service.GetDemandRows())
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
            .Setup(service => service.GetLocationRows())
            .Returns(Array.Empty<Model_InforVisualCustomerPullPackLocationRow>());

        var service = new Service_CustomerPullPackMockWaitlistSource(mockCatalog.Object);

        var result = await service.GetByIdAsync("CPP-WL-0002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CurrentStatus.Should().Be(Enum_CustomerPullPackWaitlistStatus.Completed);
        result.Data.LocationReviewFlag.Should().BeTrue();
        result.Data.RecheckIndicator.Should().BeTrue();
    }
}

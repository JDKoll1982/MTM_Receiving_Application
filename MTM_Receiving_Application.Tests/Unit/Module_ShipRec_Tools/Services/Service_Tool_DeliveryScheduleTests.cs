using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

public sealed class Service_Tool_DeliveryScheduleTests
{
    private static Mock<IService_InforVisual> CreateInforVisualMock(
        IReadOnlyList<Model_InforVisualDeliveryScheduleLine>? rows = null
    )
    {
        var mock = new Mock<IService_InforVisual>();
        mock.Setup(service => service.GetDeliveryScheduleLinesAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_InforVisualDeliveryScheduleLine>>(
                    rows is null ? new List<Model_InforVisualDeliveryScheduleLine>() : [.. rows]
                )
            );
        return mock;
    }

    private static Service_Tool_DeliverySchedule CreateService(Mock<IService_InforVisual> inforVisualMock) =>
        new(inforVisualMock.Object, new Mock<IService_LoggingUtility>().Object);

    [Fact]
    public async Task SearchAsync_ShouldReturnMappedRows()
    {
        var source = new List<Model_InforVisualDeliveryScheduleLine>
        {
            new()
            {
                PoNumber = "PO-064008",
                VendorName = "Basic Metals",
                PartNumber = "MMC0000367",
                OrderDate = new System.DateTime(2026, 8, 24),
                Carrier = "OVERNITE",
                OrderQty = 125884,
                ReceivedQty = 125884,
                RemainingQty = 0,
                PoStatus = "R",
                LineStatus = "A",
                ReceivedBy = "JK0LL",
                DueDate = new System.DateTime(2026, 8, 27),
                Category = "MMC Coils",
                DeliveryState = "Closed",
                PoState = "OnTime",
            },
        };
        var service = CreateService(CreateInforVisualMock(source));

        var result = await service.SearchAsync(new Model_InforVisualDeliveryScheduleFilter());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        var row = result.Data![0];
        row.PoNumber.Should().Be("PO-064008");
        row.PartNumber.Should().Be("MMC0000367");
        row.OrderQtyText.Should().Be("125,884");
        row.RemainingQtyText.Should().Be("0");
        row.OrderDateText.Should().Be("8/24/2026");
        row.CarrierText.Should().Be("OVERNITE");
        row.DueDateText.Should().Be("8/27/2026");
        row.DeliveryStateLabel.Should().Be("Closed");
        row.PoStateLabel.Should().Be("On Time");
        row.StatusDotKey.Should().Be("Closed");
    }

    [Fact]
    public async Task SearchAsync_ShouldComputeStatusDotKey_ForOpenOnTimeRow()
    {
        var source = new List<Model_InforVisualDeliveryScheduleLine>
        {
            new()
            {
                PoNumber = "PO-070026",
                DeliveryState = "Open",
                PoState = "OnTime",
            },
        };
        var service = CreateService(CreateInforVisualMock(source));

        var result = await service.SearchAsync(new Model_InforVisualDeliveryScheduleFilter());

        result.Data![0].StatusDotKey.Should().Be("OnTime");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenNoRows()
    {
        var service = CreateService(CreateInforVisualMock());

        var result = await service.SearchAsync(new Model_InforVisualDeliveryScheduleFilter());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldPropagateFailure()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        inforVisualMock
            .Setup(service => service.GetDeliveryScheduleLinesAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_InforVisualDeliveryScheduleLine>>(
                    "boom"
                )
            );
        var service = CreateService(inforVisualMock);

        var result = await service.SearchAsync(new Model_InforVisualDeliveryScheduleFilter());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("boom");
    }

    [Fact]
    public async Task SearchAsync_ShouldPassThroughFilter()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        Model_InforVisualDeliveryScheduleFilter? captured = null;
        inforVisualMock
            .Setup(service => service.GetDeliveryScheduleLinesAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .Callback<Model_InforVisualDeliveryScheduleFilter>(f => captured = f)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_InforVisualDeliveryScheduleLine>>([])
            );
        var service = CreateService(inforVisualMock);

        var filter = new Model_InforVisualDeliveryScheduleFilter
        {
            PartSearch = "MMC",
            ScopeCoils = false,
            ShowLate = true,
        };
        await service.SearchAsync(filter);

        captured.Should().NotBeNull();
        captured!.PartSearch.Should().Be("MMC");
        captured.ScopeCoils.Should().BeFalse();
        captured.ShowLate.Should().BeTrue();
    }
}
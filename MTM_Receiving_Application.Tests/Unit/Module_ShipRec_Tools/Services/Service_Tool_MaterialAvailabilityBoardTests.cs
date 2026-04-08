using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

public sealed class Service_Tool_MaterialAvailabilityBoardTests
{
    [Fact]
    public async Task GetBoardByLocationAsync_ShouldGroupCardsAndSortByEarliestIncomingDate()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var today = DateTime.Today;

        inforVisualMock
            .Setup(service => service.GetMaterialAvailabilityCurrentStockAsync("RECV", null, "002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualMaterialLocationRow>
                    {
                        new()
                        {
                            PartId = "PART-A",
                            PartDescription = "Part A",
                            WarehouseCode = "002",
                            LocationId = "RECV",
                            Quantity = 10,
                        },
                        new()
                        {
                            PartId = "PART-A",
                            PartDescription = "Part A",
                            WarehouseCode = "002",
                            LocationId = "A-02",
                            Quantity = 5,
                        },
                        new()
                        {
                            PartId = "PART-B",
                            PartDescription = "Part B",
                            WarehouseCode = "002",
                            LocationId = "RECV",
                            Quantity = 4,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync("RECV", null, "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualIncomingSupplyRow>
                    {
                        new()
                        {
                            PartId = "PART-A",
                            PartDescription = "Part A",
                            WarehouseCode = "002",
                            PONumber = "PO-1001",
                            POLineNumber = "1",
                            OrderedQty = 100,
                            ReceivedQty = 40,
                            RemainingQty = 60,
                            LineDesiredReceiveDate = today.AddDays(12),
                        },
                        new()
                        {
                            PartId = "PART-B",
                            PartDescription = "Part B",
                            WarehouseCode = "002",
                            PONumber = "PO-1002",
                            POLineNumber = "2",
                            OrderedQty = 20,
                            ReceivedQty = 10,
                            RemainingQty = 10,
                            LinePromiseDate = today.AddDays(4),
                        },
                    }
                )
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByLocationAsync("RECV", "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].PartId.Should().Be("PART-B");
        result.Data[1].PartId.Should().Be("PART-A");
        result.Data[1].QuantityInSearchLocation.Should().Be(10);
        result.Data[1].CurrentLocations.Should().HaveCount(2);
        result.Data[1].IncomingRollup.OrderedQty.Should().Be(100);
        result.Data[1].IncomingRollup.ReceivedQty.Should().Be(40);
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldCreateCardWhenPositiveLocationsDoNotExist()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-ZERO"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-ZERO",
                        Description = "Zero Quantity Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-ZERO", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-ZERO", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualIncomingSupplyRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-ZERO", "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].PartId.Should().Be("PART-ZERO");
        result.Data[0].PartDescription.Should().Be("Zero Quantity Part");
        result.Data[0].CurrentLocations.Should().BeEmpty();
        result.Data[0].TotalPositiveQuantity.Should().Be(0);
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldUseBlanketLastReceivedDateInUpcomingDates()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var blanketDate = DateTime.Today.AddDays(-2);

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-BLANKET"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-BLANKET",
                        Description = "Blanket Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-BLANKET", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-BLANKET", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualIncomingSupplyRow>
                    {
                        new()
                        {
                            PartId = "PART-BLANKET",
                            PartDescription = "Blanket Part",
                            WarehouseCode = "002",
                            PONumber = "PO-BLK-1",
                            POLineNumber = "1",
                            OrderedQty = 50,
                            ReceivedQty = 30,
                            RemainingQty = 20,
                            FreeOnBoard = "Blanket",
                            IsBlanketOrder = true,
                            LineLastReceivedDate = blanketDate,
                        },
                    }
                )
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-BLANKET", "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].UpcomingDates.Should().ContainSingle();
        result.Data[0].UpcomingDates[0].Label.Should().Be("Last received on");
        result.Data[0].UpcomingDates[0].Date.Should().Be(blanketDate.Date);
        result.Data[0].NextDateSummary.Should().Contain("Last received on");
    }
}

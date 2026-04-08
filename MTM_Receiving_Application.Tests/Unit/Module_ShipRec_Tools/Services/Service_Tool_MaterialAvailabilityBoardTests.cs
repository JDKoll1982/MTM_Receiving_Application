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
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync("RECV", null, "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualAssociatedPartRunRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByLocationAsync("RECV", "002", 30);

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
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-ZERO", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualAssociatedPartRunRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-ZERO", "002", 30);

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
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-BLANKET", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualAssociatedPartRunRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-BLANKET", "002", 30);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].UpcomingDates.Should().ContainSingle();
        result.Data[0].UpcomingDates[0].Label.Should().Be("Last received on");
        result.Data[0].UpcomingDates[0].Date.Should().Be(blanketDate.Date);
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldKeepIncomingRollupWhenDueDatesAreMissing()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-NODATE"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-NODATE",
                        Description = "Undated Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-NODATE", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-NODATE", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualIncomingSupplyRow>
                    {
                        new()
                        {
                            PartId = "PART-NODATE",
                            PartDescription = "Undated Part",
                            WarehouseCode = "002",
                            PONumber = "PO-2001",
                            POLineNumber = "1",
                            OrderedQty = 30,
                            ReceivedQty = 5,
                            RemainingQty = 25,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-NODATE", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualAssociatedPartRunRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-NODATE", "002", 30);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].HasIncomingSupply.Should().BeTrue();
        result.Data[0].IncomingRollup.POLineCount.Should().Be(1);
        result.Data[0].NextDateSummary.Should().Contain("no qualifying due dates");
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldFallbackToLastReceivedShipment_WhenNoFutureIncomingDateExists()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var lastShipmentDate = DateTime.Today.AddDays(-14);

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-LASTSHIP"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-LASTSHIP",
                        Description = "Recent Shipment Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service => service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-LASTSHIP", "002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service => service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-LASTSHIP", "002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualIncomingSupplyRow>
                    {
                        new()
                        {
                            PartId = "PART-LASTSHIP",
                            PartDescription = "Recent Shipment Part",
                            WarehouseCode = "002",
                            PONumber = "PO-3001",
                            POLineNumber = "1",
                            OrderedQty = 40,
                            ReceivedQty = 15,
                            RemainingQty = 25,
                            LinePromiseDate = DateTime.Today.AddDays(45),
                            LineLastReceivedDate = lastShipmentDate,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service => service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-LASTSHIP", "002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualAssociatedPartRunRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-LASTSHIP", "002", 30);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].HasIncomingSupply.Should().BeTrue();
        result.Data[0].IncomingRollup.POLineCount.Should().Be(1);
        result.Data[0].UpcomingDates.Should().ContainSingle();
        result.Data[0].UpcomingDates[0].Label.Should().Be("Last received in shipment");
        result.Data[0].UpcomingDates[0].Date.Should().Be(lastShipmentDate.Date);
        result.Data[0].NextDateSummary.Should().Contain("Last received in shipment");
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldFilterAssociatedPartRunsBySelectedWindow()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var today = DateTime.Today;

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-COMP"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-COMP",
                        Description = "Component Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-COMP", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-COMP", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualIncomingSupplyRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-COMP", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualAssociatedPartRunRow>
                    {
                        new()
                        {
                            InputPartNumber = "PART-COMP",
                            AssociatedPartNumber = "ASSY-10",
                            AssociatedPartDescription = "Assembly 10",
                            NextDueToRunDate = today.AddDays(10),
                            IsFutureOrTodayRun = true,
                            NextDueDateSource = "REQUIREMENT.REQUIRED_DATE",
                            WorkOrderType = "M",
                            WorkOrderBaseId = "1001",
                            WorkOrderLotId = "0",
                            WorkOrderSplitId = "0",
                            WorkOrderSubId = "0",
                        },
                        new()
                        {
                            InputPartNumber = "PART-COMP",
                            AssociatedPartNumber = "ASSY-45",
                            AssociatedPartDescription = "Assembly 45",
                            NextDueToRunDate = today.AddDays(45),
                            IsFutureOrTodayRun = true,
                            NextDueDateSource = "WORK_ORDER.SCHED_START_DATE",
                            WorkOrderType = "M",
                            WorkOrderBaseId = "1002",
                            WorkOrderLotId = "0",
                            WorkOrderSplitId = "0",
                            WorkOrderSubId = "0",
                        },
                    }
                )
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-COMP", "002", 30);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].AssociatedPartRuns.Should().ContainSingle();
        result.Data[0].AssociatedPartRuns[0].AssociatedPartNumber.Should().Be("ASSY-10");
        result.Data[0].NextDateSummary.Should().Contain("ASSY-10");
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldPreferLatestHistoricalRun_WhenNoFutureRunExists()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var olderRunDate = new DateTime(2013, 11, 12);
        var latestRunDate = new DateTime(2025, 12, 08);

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("MMC0001146"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "MMC0001146",
                        Description = "Pre-Build, Coil, 20Ga X 35.000",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "MMC0001146", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "MMC0001146", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualIncomingSupplyRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "MMC0001146", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualAssociatedPartRunRow>
                    {
                        new()
                        {
                            InputPartNumber = "MMC0001146",
                            AssociatedPartNumber = "926544",
                            AssociatedPartDescription = "Panel, Back 36-39 NICS 3-3PT",
                            NextDueToRunDate = olderRunDate,
                            IsFutureOrTodayRun = false,
                            NextDueDateSource = "REQUIREMENT.REQUIRED_DATE",
                            WorkOrderType = "M",
                            WorkOrderBaseId = "926544",
                            WorkOrderLotId = "0",
                            WorkOrderSplitId = "0",
                            WorkOrderSubId = "0",
                            OperationSeqNo = 10,
                            RequirementPieceNo = 1,
                        },
                        new()
                        {
                            InputPartNumber = "MMC0001146",
                            AssociatedPartNumber = "926544",
                            AssociatedPartDescription = "Panel, Back 36-39 NICS 3-3PT",
                            NextDueToRunDate = latestRunDate,
                            IsFutureOrTodayRun = false,
                            NextDueDateSource = "REQUIREMENT.REQUIRED_DATE",
                            WorkOrderType = "M",
                            WorkOrderBaseId = "926544",
                            WorkOrderLotId = "0",
                            WorkOrderSplitId = "0",
                            WorkOrderSubId = "0",
                            OperationSeqNo = 20,
                            RequirementPieceNo = 1,
                        },
                    }
                )
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("MMC0001146", "002", null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].AssociatedPartRuns.Should().ContainSingle();
        result.Data[0].AssociatedPartRuns[0].AssociatedPartNumber.Should().Be("926544");
        result.Data[0].AssociatedPartRuns[0].NextDueToRunDate.Should().Be(latestRunDate.Date);
        result.Data[0].AssociatedPartRuns[0].IsFutureOrTodayRun.Should().BeFalse();
        result.Data[0].NextDateSummary.Should().Contain("Latest known run:");
        result.Data[0].NextDateSummary.Should().Contain("12/08/2025");
    }
}

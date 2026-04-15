using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;
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
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-LASTSHIP", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-LASTSHIP", "002")
            )
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
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-LASTSHIP", "002")
            )
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
    public async Task GetBoardByPartAsync_ShouldFormatAssociatedWorkOrderUsingInforVisualEntryFormat()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var today = DateTime.Today;

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-WOFORMAT"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-WOFORMAT",
                        Description = "Work Order Format Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-WOFORMAT", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-WOFORMAT", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualIncomingSupplyRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-WOFORMAT", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualAssociatedPartRunRow>
                    {
                        new()
                        {
                            InputPartNumber = "PART-WOFORMAT",
                            AssociatedPartNumber = "ASSY-WO",
                            AssociatedPartDescription = "Assembly Work Order",
                            NextDueToRunDate = today.AddDays(1),
                            IsFutureOrTodayRun = true,
                            NextDueDateSource = "REQUIREMENT.REQUIRED_DATE",
                            WorkOrderType = "W",
                            WorkOrderBaseId = "70016",
                            WorkOrderLotId = "1",
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

        var result = await service.GetBoardByPartAsync("PART-WOFORMAT", "002", null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].AssociatedPartRuns.Should().ContainSingle();
        result.Data[0].AssociatedPartRuns[0].WorkOrderDisplay.Should().Be("WO-070016");
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
        result.Data[0].AssociatedPartRuns[0].WorkOrderDisplay.Should().Be("WO-926544");
        result.Data[0].NextDateSummary.Should().Contain("Latest known run:");
        result.Data[0].NextDateSummary.Should().Contain("12/08/2025");
    }

    [Fact]
    public async Task FormatBoardForPrintAsync_ShouldCreateReportingStyleHtmlDocument()
    {
        var service = new Service_Tool_MaterialAvailabilityBoard(
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_LoggingUtility>().Object
        );
        var cards = new List<Model_Tool_MaterialAvailabilityCard>
        {
            new()
            {
                PartId = "23-11669-100",
                PartDescription = "Stud, Weld 5/16-18 x 1.000",
                SearchLocationId = "RECV",
                QuantityInSearchLocation = 2000,
                TotalPositiveQuantity = 31396,
                NextDateSummary = "Next run: WO-070016 04/14/2026",
                CurrentLocations =
                [
                    new Model_Tool_MaterialAvailabilityLocation
                    {
                        LocationId = "RECV",
                        Quantity = 2000,
                        IsSearchLocation = true,
                    },
                ],
                IncomingRollup = new Model_Tool_MaterialAvailabilityRollup
                {
                    OrderedQty = 100,
                    ReceivedQty = 40,
                    RemainingQty = 60,
                    PurchaseOrderCount = 1,
                    POLineCount = 1,
                },
                UpcomingDates =
                [
                    new Model_Tool_MaterialAvailabilityIncomingDate
                    {
                        Label = "Line promise",
                        Date = new DateTime(2026, 04, 14),
                    },
                ],
                AssociatedPartRuns =
                [
                    new Model_Tool_MaterialAvailabilityAssociatedPartRun
                    {
                        AssociatedPartNumber = "A66-17608-000",
                        AssociatedPartDescription = "Assembly",
                        WorkOrderDisplay = "WO-070016",
                        NextDueToRunDate = new DateTime(2026, 04, 14),
                        IsFutureOrTodayRun = true,
                    },
                ],
            },
        };

        var result = await service.FormatBoardForPrintAsync(
            cards,
            "Warehouse Location",
            "RECV",
            "002",
            "30"
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HtmlFragment.Should().Contain("Material Availability Board");
        result
            .Data.HtmlFragment.Should()
            .Contain("Warehouse Location: RECV | Warehouse scope: 002 | Look Ahead: 30");
        result.Data.HtmlFragment.Should().Contain("23-11669-100 - Stud, Weld 5/16-18 x 1.000");
        result.Data.HtmlFragment.Should().Contain("WO-070016");
        result.Data.HtmlFragment.Should().Contain("Associated Parts");
        result.Data.PlainText.Should().Contain("Material Availability Board");
        result.Data.PlainText.Should().Contain("A66-17608-000");
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldCalculateRequiredPartsAndEstimatedCoilUseFromRequirementFields()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var runDate = new DateTime(2026, 04, 20);

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("MMC0001146"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart { PartID = "MMC0001146", Description = "Coil Stock" }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "MMC0001146", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualMaterialLocationRow>
                    {
                        new()
                        {
                            PartId = "MMC0001146",
                            PartDescription = "Coil Stock",
                            WarehouseCode = "002",
                            LocationId = "RECV",
                            Quantity = 90,
                        },
                    }
                )
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
                            InputPartDescription = "Coil Stock",
                            ComponentPartNumber = "MMC0001146",
                            AssociatedPartNumber = "A66-17608-000",
                            AssociatedPartDescription = "Parent Assembly",
                            WorkOrderType = "WO",
                            WorkOrderBaseId = "70016",
                            WorkOrderLotId = "0",
                            WorkOrderSplitId = "0",
                            WorkOrderSubId = "0",
                            WorkOrderStatus = "R",
                            WorkOrderStatusEffectiveDate = runDate.AddDays(-1),
                            OperationSeqNo = 20,
                            NextDueToRunDate = runDate,
                            IsFutureOrTodayRun = true,
                            NextDueDateSource = "Requirement required date",
                            RequiredDate = runDate,
                            CalcQty = 100,
                            ScrapPercent = 10,
                            UsageUm = "LBS",
                            OperationType = "STAMP",
                            ResourceId = "PRESS-01",
                        },
                    }
                )
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("MMC0001146", "002", 30);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        var card = result.Data![0];
        var primaryRun = card.PrimaryAssociatedPartRun;

        primaryRun.Should().NotBeNull();
        primaryRun!.RequiredPartsQuantity.Should().Be(100);
        primaryRun.EstimatedCoilUse.Should().Be(110);
        primaryRun.NormalizedUsageUnitOfMeasure.Should().Be("Pounds");
        card.HasEstimatedCoilUseRisk.Should().BeTrue();
        card.EstimatedCoilUseRiskText.Should().Contain("exceeds on hand");
        card.NextRunSummaryDisplay.Should().Contain("WO-070016");
        card.NextRunSummaryDisplay.Should().Contain("Required Parts: 100 Pounds");
        card.NextRunSummaryDisplay.Should().Contain("Estimated Coil Use: 110 Pounds");
    }

    [Fact]
    public async Task GetBoardByPartAsync_ShouldBuildIncomingDetailLinesForModal()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var promiseDate = new DateTime(2026, 04, 18);

        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("PART-DETAIL"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "PART-DETAIL",
                        Description = "Incoming Detail Part",
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "PART-DETAIL", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualMaterialLocationRow>())
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityIncomingSupplyAsync(null, "PART-DETAIL", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualIncomingSupplyRow>
                    {
                        new()
                        {
                            PartId = "PART-DETAIL",
                            PartDescription = "Incoming Detail Part",
                            WarehouseCode = "002",
                            PONumber = "PO-777",
                            POLineNumber = "2",
                            VendorName = "Metro Steel",
                            OrderedQty = 150,
                            ReceivedQty = 60,
                            RemainingQty = 90,
                            LinePromiseDate = promiseDate,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetMaterialAvailabilityAssociatedPartRunsAsync(null, "PART-DETAIL", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualAssociatedPartRunRow>())
            );

        var service = new Service_Tool_MaterialAvailabilityBoard(
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.GetBoardByPartAsync("PART-DETAIL", "002", 30);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].HasIncomingDetails.Should().BeTrue();
        result.Data[0].IncomingLines.Should().ContainSingle();
        result.Data[0].IncomingLines[0].VendorName.Should().Be("Metro Steel");
        result.Data[0].IncomingLines[0].PurchaseOrderLineDisplay.Should().Be("PO-777 / Line 2");
        result.Data[0].IncomingLines[0].DateDisplay.Should().Be("04/18/2026");
    }

    [Fact]
    public async Task FormatWorkOrderDetailsForPrintAsync_ShouldRenderProvidedSections()
    {
        var service = new Service_Tool_MaterialAvailabilityBoard(
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_LoggingUtility>().Object
        );
        var associatedRun = new Model_Tool_MaterialAvailabilityAssociatedPartRun
        {
            AssociatedPartNumber = "A66-17608-000",
            AssociatedPartDescription = "Parent Assembly",
            WorkOrderDisplay = "WO-070016",
            RequiredPartsQuantity = 100,
            EstimatedCoilUse = 110,
            NormalizedUsageUnitOfMeasure = "Pounds",
        };
        var fieldSettings = new Model_Tool_MaterialAvailabilityFieldSettings
        {
            UiVisibleFieldIds =
                MaterialAvailabilityWorkOrderFieldCatalog.DefaultUiVisibleIds.ToHashSet(
                    StringComparer.OrdinalIgnoreCase
                ),
            PrintVisibleFieldIds =
                MaterialAvailabilityWorkOrderFieldCatalog.DefaultPrintVisibleIds.ToHashSet(
                    StringComparer.OrdinalIgnoreCase
                ),
            IsShowAllChipEnabled = true,
        };
        associatedRun.WorkOrderStatus = "Released";
        associatedRun.OperationType = "STAMP";

        var sections = MaterialAvailabilityWorkOrderDetailBuilder.BuildPrintSections(
            associatedRun,
            fieldSettings
        );

        var result = await service.FormatWorkOrderDetailsForPrintAsync(associatedRun, sections);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HtmlFragment.Should().Contain("Work Order Details - WO-070016");
        result.Data.HtmlFragment.Should().Contain("Job Summary");
        result.Data.HtmlFragment.Should().Contain("Operation Context");
        result.Data.PlainText.Should().Contain("Required Parts: 100 Pounds");
    }
}

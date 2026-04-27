using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Services.Database;

public sealed class Service_InforVisualConnectTests
{
    [Fact]
    public async Task FuzzySearchLocationsAsync_ShouldReturnAllMockLocations_WhenTermIsEmpty()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchLocationsAsync(string.Empty, "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result
            .Data!.Select(item => item.Label)
            .Should()
            .Equal("A-RECV-01", "B-RECV-02", "C-RECV-03", "DOCK-4", "QA-RECV", "RECV");
    }

    [Fact]
    public async Task FuzzySearchLocationsAsync_ShouldFilterStableMockLocations_WhenTermIsProvided()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchLocationsAsync("C-RECV", "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].Label.Should().Be("C-RECV-03");
        result.Data[0].Detail.Should().Be("Warehouse 002 — Mock location");
    }

    [Fact]
    public async Task FuzzySearchLocationsAsync_ShouldFail_WhenWarehouseCodeIsBlank()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchLocationsAsync(string.Empty, string.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Warehouse code cannot be empty");
    }

    [Fact]
    public async Task FuzzySearchPartsAsync_ShouldReturnRequestedMockParts_WhenTermMatchesPrefix()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchPartsAsync("MM");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Select(item => item.Key).Should().Contain("MMCCS00740");
        result.Data.Select(item => item.Key).Should().Contain("MMFCS01145");
    }

    [Fact]
    public async Task GetReceivingLocationEvidenceAsync_ShouldReturnMockTransactions_WhenCatalogContainsSavedReceivingRows()
    {
        var service = CreateService(useMockData: true);

        var result = await service.GetReceivingLocationEvidenceAsync(
            "PO-066868",
            "MMC-100",
            "1",
            new System.DateTime(2026, 4, 5)
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].CurrentLocationId.Should().Be("A-01");
        result.Data[0].MatchedTransactionQuantity.Should().Be(10);
        result.Data[0].MatchedTransactionCount.Should().Be(1);
        result.Data[0].MatchedTransactionUserId.Should().Be("seed-user");
        result.Data[0].LatestTransactionLocationId.Should().Be("A-01");
        result.Data[0].LatestTransactionQuantity.Should().Be(10);
        result.Data[0].LatestTransactionUserId.Should().Be("seed-user");
        result.Data[0].ReceiptCount.Should().Be(1);
    }

    [Fact]
    public async Task GetReceivingLocationEvidenceAsync_ShouldFallBackToPoPartMatch_WhenMockDatesAreOutsideRequestedWindow()
    {
        var service = CreateService(useMockData: true);

        var result = await service.GetReceivingLocationEvidenceAsync(
            "PO-066868",
            "MMC-100",
            "1",
            new System.DateTime(2026, 4, 20)
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].CurrentLocationId.Should().Be("A-01");
        result.Data[0].MatchedTransactionQuantity.Should().Be(10);
        result.Data[0].ReceiptCount.Should().Be(1);
    }

    [Fact]
    public async Task GetMaterialAvailabilityAssociatedPartRunsAsync_ShouldReturnMockRows_WhenSearchingByPart()
    {
        var service = CreateService(useMockData: true);

        var result = await service.GetMaterialAvailabilityAssociatedPartRunsAsync(
            null,
            "MMC-100",
            "002"
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].InputPartNumber.Should().Be("MMC-100");
        result.Data[0].AssociatedPartNumber.Should().Be("ASSY-1000");
    }

    [Fact]
    public async Task GetMaterialAvailabilityAssociatedPartRunsAsync_ShouldReturnMockRows_WhenSearchingByLocation()
    {
        var service = CreateService(useMockData: true);

        var result = await service.GetMaterialAvailabilityAssociatedPartRunsAsync(
            "A-01",
            null,
            "002"
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].InputPartNumber.Should().Be("MMC-100");
    }

    private static Service_InforVisualConnect CreateService(bool useMockData)
    {
        var dao = new Dao_InforVisualConnection(
            "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;Trusted_Connection=True;",
            new Mock<IService_LoggingUtility>().Object
        );
        var appSettings = new Mock<IService_AppSettings>();
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        var locations = new[]
        {
            "A-RECV-01",
            "B-RECV-02",
            "C-RECV-03",
            "DOCK-4",
            "QA-RECV",
            "RECV",
        };

        mockCatalog.Setup(service => service.GetLocations()).Returns(locations);
        appSettings.Setup(service => service.GetUseInforVisualMockData()).Returns(useMockData);
        mockCatalog
            .Setup(service => service.GetCatalog())
            .Returns(
                new Model_InforVisualMockDataCatalog
                {
                    Locations = new List<string>(locations),
                    Parts = new List<Model_InforVisualPart>
                    {
                        new()
                        {
                            PartID = "MMCCS00740",
                            POLineNumber = "N/A",
                            PartType = "Coil",
                            QtyOrdered = 4800,
                            UnitOfMeasure = "EA",
                            Description =
                                "Mock coil part for Guided Wizard non-PO fuzzy search validation",
                            DefaultLocationId = "RECV",
                            RemainingQuantity = 960,
                        },
                        new()
                        {
                            PartID = "MMFCS01145",
                            POLineNumber = "N/A",
                            PartType = "Sheet",
                            QtyOrdered = 2400,
                            UnitOfMeasure = "EA",
                            Description =
                                "Mock sheet part for Guided Wizard non-PO fuzzy search validation",
                            DefaultLocationId = "S-00",
                            RemainingQuantity = 480,
                        },
                    },
                    AssociatedPartRuns = new List<Model_InforVisualAssociatedPartRunRow>
                    {
                        new()
                        {
                            InputPartNumber = "MMC-100",
                            InputPartDescription = "Mock component part",
                            AssociatedPartNumber = "ASSY-1000",
                            AssociatedPartDescription = "Mock parent assembly",
                            NextDueToRunDate = new System.DateTime(2026, 4, 15),
                            IsFutureOrTodayRun = true,
                            NextDueDateSource = "WORK_ORDER.SCHED_START_DATE",
                            WorkOrderType = "M",
                            WorkOrderBaseId = "5001",
                            WorkOrderLotId = "0",
                            WorkOrderSplitId = "0",
                            WorkOrderSubId = "0",
                            OperationSeqNo = 10,
                            RequirementPieceNo = 1,
                            WorkOrderStatus = "R",
                            RequirementStatus = "O",
                        },
                    },
                    ReceivingTransactions = new List<Model_InforVisualMockReceivingTransaction>
                    {
                        new()
                        {
                            SourceLoadId = "seed-load-1",
                            PONumber = "PO-066868",
                            PartID = "MMC-100",
                            POLineNumber = "1",
                            Quantity = 10,
                            UnitOfMeasure = "EA",
                            ReceivedDate = new System.DateTime(2026, 4, 5, 8, 0, 0),
                            TransactionDate = new System.DateTime(2026, 4, 5, 8, 0, 0),
                            ReceiptWarehouseId = "002",
                            ReceiptLocationId = "RECV",
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "A-01",
                            UserId = "seed-user",
                        },
                    },
                }
            );
        mockCatalog
            .Setup(service => service.GetReceivingTransactions())
            .Returns(
                new List<Model_InforVisualMockReceivingTransaction>
                {
                    new()
                    {
                        SourceLoadId = "seed-load-1",
                        PONumber = "PO-066868",
                        PartID = "MMC-100",
                        POLineNumber = "1",
                        Quantity = 10,
                        UnitOfMeasure = "EA",
                        ReceivedDate = new System.DateTime(2026, 4, 5, 8, 0, 0),
                        TransactionDate = new System.DateTime(2026, 4, 5, 8, 0, 0),
                        ReceiptWarehouseId = "002",
                        ReceiptLocationId = "RECV",
                        CurrentWarehouseId = "002",
                        CurrentLocationId = "A-01",
                        UserId = "seed-user",
                    },
                }
            );

        return new Service_InforVisualConnect(
            dao,
            appSettings.Object,
            new Mock<IService_LoggingUtility>().Object,
            mockCatalog.Object
        );
    }
}

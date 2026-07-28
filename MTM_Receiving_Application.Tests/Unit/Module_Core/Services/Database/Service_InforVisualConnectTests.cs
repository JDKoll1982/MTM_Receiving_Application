using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Services.Database;

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
    public async Task FuzzySearchCustomersAsync_ShouldFail_WhenTermIsBlank()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchCustomersAsync(string.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Search term cannot be empty");
    }

    [Fact]
    public async Task FuzzySearchCustomersAsync_ShouldReturnDistinctMockCustomers_WhenTermMatchesIdOrName()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchCustomersAsync("Volvo");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].Key.Should().Be("VOLVO");
        result.Data[0].Label.Should().Be("VOLVO - Volvo Trucks");
    }

    [Fact]
    public async Task GetRemainingQuantityAsync_ShouldSumRemainingAcrossDuplicatePartLines_WhenMockPOContainsMultipleLines()
    {
        var service = CreateService(
            useMockData: true,
            catalog: new Model_InforVisualMockDataCatalog
            {
                Locations = new List<string> { "RECV" },
                PurchaseOrders = new List<Model_InforVisualPO>
                {
                    new()
                    {
                        PONumber = "PO-068202",
                        Vendor = "Stern Steel LLC",
                        Status = "R",
                        Parts = new List<Model_InforVisualPart>
                        {
                            new()
                            {
                                PartID = "MMC0000850",
                                POLineNumber = "1",
                                QtyOrdered = 172000,
                                RemainingQuantity = 36220,
                                Description = "Coil, .312 X 14.330",
                            },
                            new()
                            {
                                PartID = "MMC0000850",
                                POLineNumber = "2",
                                QtyOrdered = 156000,
                                RemainingQuantity = 156000,
                                Description = "Coil, .312 X 14.330",
                            },
                        },
                    },
                },
            }
        );

        var result = await service.GetRemainingQuantityAsync("PO-068202", "MMC0000850");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(192220);
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

    [Fact]
    public async Task MockCatalog_ShouldExposeBoardDataForMmc0000650_WhenMockModeIsEnabled()
    {
        var catalog = new Service_InforVisualMockDataCatalog().GetCatalog();
        var service = CreateService(useMockData: true, catalog: catalog);

        var currentStockResult = await service.GetMaterialAvailabilityCurrentStockAsync(
            null,
            "MMC0000650",
            "002"
        );
        var incomingSupplyResult = await service.GetMaterialAvailabilityIncomingSupplyAsync(
            null,
            "MMC0000650",
            "002"
        );
        var associatedRunsResult = await service.GetMaterialAvailabilityAssociatedPartRunsAsync(
            null,
            "MMC0000650",
            "002"
        );

        currentStockResult.IsSuccess.Should().BeTrue();
        currentStockResult.Data.Should().NotBeNullOrEmpty();
        currentStockResult
            .Data!.Select(row => row.LocationId)
            .Should()
            .Contain(new[] { "RECV", "V-D0-08" });

        incomingSupplyResult.IsSuccess.Should().BeTrue();
        incomingSupplyResult.Data.Should().NotBeNullOrEmpty();
        incomingSupplyResult.Data!.Should().Contain(row => row.PONumber == "PO-062200");

        associatedRunsResult.IsSuccess.Should().BeTrue();
        associatedRunsResult.Data.Should().NotBeNullOrEmpty();
        associatedRunsResult
            .Data!.Select(row => row.AssociatedPartNumber)
            .Should()
            .Contain(new[] { "G020978", "G020958" });
    }

    private static Service_InforVisualConnect CreateService(
        bool useMockData,
        Model_InforVisualMockDataCatalog? catalog = null
    )
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
        var activeCatalog = catalog ?? CreateDefaultCatalog(locations);

        mockCatalog.Setup(service => service.GetLocations()).Returns(locations);
        appSettings.Setup(service => service.GetUseInforVisualMockData()).Returns(useMockData);
        mockCatalog.Setup(service => service.GetCatalog()).Returns(activeCatalog);
        mockCatalog
            .Setup(service => service.GetReceivingTransactions())
            .Returns(activeCatalog.ReceivingTransactions);

        return new Service_InforVisualConnect(
            dao,
            appSettings.Object,
            new Mock<IService_LoggingUtility>().Object,
            mockCatalog.Object
        );
    }

    private static Model_InforVisualMockDataCatalog CreateDefaultCatalog(string[] locations)
    {
        return new Model_InforVisualMockDataCatalog
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
                    Description = "Mock coil part for Guided Wizard non-PO fuzzy search validation",
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
        };
    }
}

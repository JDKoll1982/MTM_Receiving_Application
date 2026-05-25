using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;
using DaoResultFactory = MTM_Receiving_Application.Module_Core.Models.Core.Model_Dao_Result_Factory;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public sealed class Service_ReceivingLocationReconciliationTests
{
    [Fact]
    public async Task PreviewLocationsAsync_WhenTransfersSplitAcrossDestinations_ShouldAllocateDeterministically()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000650", 1, "RECV", 4500m, receivedDate),
            CreateLoad("PO-066868", "MMC0000650", 2, "RECV", 5000m, receivedDate),
            CreateLoad("PO-066868", "MMC0000650", 3, "RECV", 7500m, receivedDate),
            CreateLoad("PO-066868", "MMC0000650", 4, "RECV", 4350m, receivedDate),
            CreateLoad("PO-066868", "MMC0000650", 5, "RECV", 2000m, receivedDate),
        };

        var service = CreateService(
            loads,
            new List<Model_InforVisualLocationTransferMovement>
            {
                CreateTransfer(
                    "MMC0000650",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-A0-01",
                    12000m,
                    receivedDate.AddHours(1),
                    "visual-a"
                ),
                CreateTransfer(
                    "MMC0000650",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-B0-01",
                    5000m,
                    receivedDate.AddHours(2),
                    "visual-b"
                ),
                CreateTransfer(
                    "MMC0000650",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-C0-01",
                    6350m,
                    receivedDate.AddHours(3),
                    "visual-c"
                ),
            }
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedItems.Should().HaveCount(5);

        var byLoadNumber = result.Data.UpdatedItems.ToDictionary(item =>
            item.SourceLoad!.LoadNumber
        );
        byLoadNumber[1].ProposedLocation.Should().Be("V-A0-01");
        byLoadNumber[2].ProposedLocation.Should().Be("V-B0-01");
        byLoadNumber[3].ProposedLocation.Should().Be("V-A0-01");
        byLoadNumber[4].ProposedLocation.Should().Be("V-C0-01");
        byLoadNumber[5].ProposedLocation.Should().Be("V-C0-01");
        byLoadNumber[1].MovedByUserId.Should().Be("visual-a");
        byLoadNumber[2].MovedByUserId.Should().Be("visual-b");
        byLoadNumber[4].MovedByUserId.Should().Be("visual-c");
    }

    [Fact]
    public async Task PreviewLocationsAsync_WhenAllRowsMoveToOneDestination_ShouldUpdateEveryRow()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000100", 1, "RECV", 10m, receivedDate),
            CreateLoad("PO-066868", "MMC0000100", 2, "RECV", 20m, receivedDate),
            CreateLoad("PO-066868", "MMC0000100", 3, "RECV", 30m, receivedDate),
        };

        var service = CreateService(
            loads,
            new List<Model_InforVisualLocationTransferMovement>
            {
                CreateTransfer(
                    "MMC0000100",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-D0-01",
                    60m,
                    receivedDate.AddHours(1),
                    "visual-d"
                ),
            }
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().HaveCount(3);
        result.Data.UpdatedItems.Should().OnlyContain(item => item.ProposedLocation == "V-D0-01");
        result.Data.UnresolvedItems.Should().BeEmpty();
    }

    [Fact]
    public async Task PreviewLocationsAsync_WhenMultipleSourcesFeedOneDestination_ShouldConvergeThem()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000200", 1, "RECV", 10m, receivedDate),
            CreateLoad("PO-066869", "MMC0000200", 2, "V-STAGE", 5m, receivedDate),
        };

        var service = CreateService(
            loads,
            new List<Model_InforVisualLocationTransferMovement>
            {
                CreateTransfer(
                    "MMC0000200",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-E0-01",
                    10m,
                    receivedDate.AddHours(1),
                    "visual-e"
                ),
                CreateTransfer(
                    "MMC0000200",
                    "PO-066869",
                    "1",
                    "V-STAGE",
                    "V-E0-01",
                    5m,
                    receivedDate.AddHours(2),
                    "visual-e"
                ),
            }
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().HaveCount(2);
        result.Data.UpdatedItems.Should().OnlyContain(item => item.ProposedLocation == "V-E0-01");
    }

    [Fact]
    public async Task PreviewLocationsAsync_WhenTransfersCoverOnlyPartOfGroup_ShouldLeaveLeftoverRowUnresolved()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000300", 1, "RECV", 10m, receivedDate),
            CreateLoad("PO-066868", "MMC0000300", 2, "RECV", 5m, receivedDate),
        };

        var service = CreateService(
            loads,
            new List<Model_InforVisualLocationTransferMovement>
            {
                CreateTransfer(
                    "MMC0000300",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-F0-01",
                    10m,
                    receivedDate.AddHours(1),
                    "visual-f"
                ),
            }
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().ContainSingle();
        result.Data.UpdatedItems[0].SourceLoad!.LoadNumber.Should().Be(1);
        result.Data.UnresolvedItems.Should().ContainSingle();
        result.Data.UnresolvedItems[0].Resolution.Should().Be("Ambiguous");
    }

    [Fact]
    public async Task PreviewLocationsAsync_WhenSameDayReceiptsSpanMultipleEntries_ShouldQueryTransfersOncePerPartAndDay()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000400", 1, "RECV", 10m, receivedDate),
            CreateLoad("PO-066869", "MMC0000400", 2, "RECV", 5m, receivedDate),
        };

        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var settingsMock = CreateSettingsMock();

        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(DaoResultFactory.Success(loads));
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(DaoResultFactory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransferMovementsAsync("MMC0000400", receivedDate.Date)
            )
            .ReturnsAsync(
                DaoResultFactory.Success(
                    new List<Model_InforVisualLocationTransferMovement>
                    {
                        CreateTransfer(
                            "MMC0000400",
                            "PO-066868",
                            "1",
                            "RECV",
                            "V-G0-01",
                            10m,
                            receivedDate.AddHours(1),
                            "visual-g"
                        ),
                        CreateTransfer(
                            "MMC0000400",
                            "PO-066869",
                            "1",
                            "RECV",
                            "V-H0-01",
                            5m,
                            receivedDate.AddHours(2),
                            "visual-h"
                        ),
                    }
                )
            );

        var service = new Service_ReceivingLocationReconciliation(
            mySqlReceivingMock.Object,
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            settingsMock.Object
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        inforVisualMock.Verify(
            visual =>
                visual.GetReceivingLocationTransferMovementsAsync("MMC0000400", receivedDate.Date),
            Times.Once
        );
    }

    [Fact]
    public async Task PreviewLocationsAsync_WhenTransfersChainThroughStaging_ShouldUseLatestConfirmedDestination()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000500", 1, "RECV", 12m, receivedDate),
        };

        var service = CreateService(
            loads,
            new List<Model_InforVisualLocationTransferMovement>
            {
                CreateTransfer(
                    "MMC0000500",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-STAGE",
                    12m,
                    receivedDate.AddHours(1),
                    "stage-user"
                ),
                CreateTransfer(
                    "MMC0000500",
                    "PO-066868",
                    "1",
                    "V-STAGE",
                    "V-Z9-01",
                    12m,
                    receivedDate.AddHours(2),
                    "final-user"
                ),
            }
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().ContainSingle();
        result.Data.UpdatedItems[0].ProposedLocation.Should().Be("V-Z9-01");
        result.Data.UpdatedItems[0].MovedByUserId.Should().Be("final-user");
    }

    [Fact]
    public async Task PreviewLocationsAsync_WhenTransfersCycleBackIntoSource_ShouldRemainUnresolved()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateLoad("PO-066868", "MMC0000600", 1, "RECV", 10m, receivedDate),
        };

        var service = CreateService(
            loads,
            new List<Model_InforVisualLocationTransferMovement>
            {
                CreateTransfer(
                    "MMC0000600",
                    "PO-066868",
                    "1",
                    "RECV",
                    "V-STAGE",
                    10m,
                    receivedDate.AddHours(1),
                    "cycle-a"
                ),
                CreateTransfer(
                    "MMC0000600",
                    "PO-066868",
                    "1",
                    "V-STAGE",
                    "RECV",
                    10m,
                    receivedDate.AddHours(2),
                    "cycle-b"
                ),
            }
        );

        var result = await service.PreviewLocationsAsync(false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().BeEmpty();
        result.Data.UnresolvedItems.Should().ContainSingle();
        result.Data.UnresolvedItems[0].Resolution.Should().Be("Ambiguous");
    }

    [Fact]
    public async Task GetRecommendedLocationsAsync_ShouldIncludePreviouslyHardcodedLocations_WhenQuantityIsPositive()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var settingsMock = CreateSettingsMock();

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC0000700",
                    "1",
                    receivedDate.Date
                )
            )
            .ReturnsAsync(
                DaoResultFactory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "RECV",
                            CurrentQuantity = 4m,
                            LatestTransactionDate = receivedDate.AddHours(1),
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "FG",
                            CurrentQuantity = 3m,
                            LatestTransactionDate = receivedDate.AddHours(2),
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "WC",
                            CurrentQuantity = 0m,
                            LatestTransactionDate = receivedDate.AddHours(3),
                        },
                    }
                )
            );

        var service = new Service_ReceivingLocationReconciliation(
            mySqlReceivingMock.Object,
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            settingsMock.Object
        );

        var result = await service.GetRecommendedLocationsAsync(
            "PO-066868",
            "MMC0000700",
            "1",
            receivedDate.Date
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Select(location => location.LocationId).Should().Equal("RECV", "FG");
    }

    private static Service_ReceivingLocationReconciliation CreateService(
        List<Model_ReceivingLoad> currentLabelRows,
        List<Model_InforVisualLocationTransferMovement> transferMovements
    )
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var settingsMock = CreateSettingsMock();

        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(DaoResultFactory.Success(currentLabelRows));
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(DaoResultFactory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransferMovementsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()
                )
            )
            .ReturnsAsync(DaoResultFactory.Success(transferMovements));

        return new Service_ReceivingLocationReconciliation(
            mySqlReceivingMock.Object,
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            settingsMock.Object
        );
    }

    private static Mock<IService_ReceivingSettings> CreateSettingsMock()
    {
        var settingsMock = new Mock<IService_ReceivingSettings>();
        settingsMock
            .Setup(settings => settings.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync("[]");
        return settingsMock;
    }

    private static Model_ReceivingLoad CreateLoad(
        string poNumber,
        string partId,
        int loadNumber,
        string location,
        decimal quantity,
        DateTime receivedDate
    )
    {
        return new Model_ReceivingLoad
        {
            LoadID = Guid.NewGuid(),
            PoNumber = poNumber,
            PoLineNumber = "1",
            PartID = partId,
            LoadNumber = loadNumber,
            InitialLocation = location,
            WeightQuantity = quantity,
            UnitOfMeasure = "LBS",
            ReceivedDate = receivedDate,
        };
    }

    private static Model_InforVisualLocationTransferMovement CreateTransfer(
        string partId,
        string poNumber,
        string poLineNumber,
        string sourceLocation,
        string destinationLocation,
        decimal quantity,
        DateTime transactionDate,
        string userId
    )
    {
        return new Model_InforVisualLocationTransferMovement
        {
            PartId = partId,
            PONumber = poNumber,
            POLineNumber = poLineNumber,
            SourceWarehouseId = "002",
            SourceLocationId = sourceLocation,
            DestinationWarehouseId = "002",
            DestinationLocationId = destinationLocation,
            TransferQuantity = quantity,
            TransactionDate = transactionDate,
            TransactionUserId = userId,
        };
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.Settings;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public sealed class Service_ReceivingLocationReconciliationTests
{
    [Fact]
    public async Task PreviewLocationsAsync_ShouldReturnCandidateDetails_WithoutPersistingChanges()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-155", "3", "OLD-LOC");
        currentLabelRow.WeightQuantity = 42;
        currentLabelRow.UnitOfMeasure = "EA";

        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-155",
                    "3",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "B-15",
                            CurrentQuantity = 42,
                            ReceiptCount = 1,
                            LatestTransactionWarehouseId = "002",
                            LatestTransactionLocationId = "B-15",
                            LatestTransactionQuantity = 42,
                            LatestTransactionUserId = "johnk",
                            LatestTransactionDate = new DateTime(2026, 4, 5, 14, 20, 0),
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-155",
                    "3",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationTransaction>
                    {
                        new()
                        {
                            LocationId = "B-15",
                            Quantity = 42,
                            TransactionDate = new DateTime(2026, 4, 5, 14, 20, 0),
                            UserId = "johnk",
                        },
                    }
                )
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedItems.Should().ContainSingle();
        result.Data.UpdatedItems[0].ProposedLocation.Should().Be("B-15");
        result.Data.UpdatedItems[0].QuantityMoved.Should().Be(42);
        result.Data.UpdatedItems[0].MovedByUserId.Should().Be("johnk");
        result.Data.UpdatedItems[0].MovedAt.Should().Be(new DateTime(2026, 4, 5, 14, 20, 0));
        mySqlReceivingMock.Verify(
            service => service.UpdateCurrentLabelDataAsync(It.IsAny<List<Model_ReceivingLoad>>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ReconcileLocationsAsync_ShouldUpdateHistoryRow_WhenCurrentInventoryHasSingleLocation()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var historyRow = CreateLoad("66868", "MMC-100", "1", "OLD-LOC");
        historyRow.WeightQuantity = 12;
        historyRow.UnitOfMeasure = "EA";
        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { historyRow })
            );
        mySqlReceivingMock
            .Setup(service =>
                service.UpdateReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(1);

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-100",
                    "1",
                    historyRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "A-01",
                            CurrentQuantity = 12,
                            ReceiptCount = 1,
                            LatestReceiptWarehouseId = "002",
                            LatestReceiptLocationId = "RECV",
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-100",
                    "1",
                    historyRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualLocationTransaction>())
            );

        var result = await service.ReconcileLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.HistoryRowsUpdated.Should().Be(1);
        result.Data.TotalRowsUpdated.Should().Be(1);
        result.Data.UpdatedItems.Should().ContainSingle();
        historyRow.InitialLocation.Should().Be("A-01");
    }

    [Fact]
    public async Task ReconcileLocationsAsync_ShouldUseTransactionMatch_WhenInventoryHasMultipleLocations()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-200", "2", string.Empty);
        currentLabelRow.WeightQuantity = 8;
        currentLabelRow.UnitOfMeasure = "EA";
        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));
        mySqlReceivingMock
            .Setup(service =>
                service.UpdateCurrentLabelDataAsync(It.IsAny<List<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(1);

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-200",
                    "2",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "A-01",
                            CurrentQuantity = 4,
                            ReceiptCount = 1,
                            LatestTransactionWarehouseId = "002",
                            LatestTransactionLocationId = "B-02",
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "B-02",
                            CurrentQuantity = 8,
                            ReceiptCount = 1,
                            LatestTransactionWarehouseId = "002",
                            LatestTransactionLocationId = "B-02",
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-200",
                    "2",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationTransaction>
                    {
                        new()
                        {
                            LocationId = "B-02",
                            Quantity = 8,
                            TransactionDate = new DateTime(2026, 4, 5, 15, 0, 0),
                            UserId = "johnk",
                        },
                    }
                )
            );

        var result = await service.ReconcileLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CurrentLabelRowsUpdated.Should().Be(1);
        currentLabelRow.InitialLocation.Should().Be("B-02");
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldAllocateMultipleRowsAcrossMultipleLocations_ByQuantity()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var loadOne = CreateLoad("66868", "MMC-650", "1", "RECV");
        loadOne.LoadID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        loadOne.WeightQuantity = 25000;
        loadOne.UnitOfMeasure = "LBS";
        loadOne.LoadNumber = 1;

        var loadTwo = CreateLoad("66868", "MMC-650", "1", "RECV");
        loadTwo.LoadID = Guid.Parse("22222222-2222-2222-2222-222222222222");
        loadTwo.WeightQuantity = 10000;
        loadTwo.UnitOfMeasure = "LBS";
        loadTwo.LoadNumber = 2;

        var loadThree = CreateLoad("66868", "MMC-650", "1", "RECV");
        loadThree.LoadID = Guid.Parse("33333333-3333-3333-3333-333333333333");
        loadThree.WeightQuantity = 15000;
        loadThree.UnitOfMeasure = "LBS";
        loadThree.LoadNumber = 3;

        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_ReceivingLoad> { loadOne, loadTwo, loadThree }
                )
            );
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-650",
                    "1",
                    loadOne.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "V-C0-01",
                            CurrentQuantity = 25000,
                            MatchedTransactionQuantity = 25000,
                            MatchedTransactionCount = 1,
                            MatchedTransactionDate = new DateTime(2026, 4, 5, 9, 30, 0),
                            MatchedTransactionUserId = "split-user",
                            ReceiptCount = 3,
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "V-C0-02",
                            CurrentQuantity = 10000,
                            MatchedTransactionQuantity = 10000,
                            MatchedTransactionCount = 1,
                            MatchedTransactionDate = new DateTime(2026, 4, 5, 9, 35, 0),
                            MatchedTransactionUserId = "split-user",
                            ReceiptCount = 3,
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "V-C0-03",
                            CurrentQuantity = 15000,
                            MatchedTransactionQuantity = 15000,
                            MatchedTransactionCount = 1,
                            MatchedTransactionDate = new DateTime(2026, 4, 5, 9, 40, 0),
                            MatchedTransactionUserId = "split-user",
                            ReceiptCount = 3,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-650",
                    "1",
                    loadOne.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationTransaction>
                    {
                        new()
                        {
                            LocationId = "V-C0-01",
                            Quantity = 25000,
                            TransactionDate = new DateTime(2026, 4, 5, 9, 30, 0),
                            UserId = "split-user",
                        },
                        new()
                        {
                            LocationId = "V-C0-02",
                            Quantity = 10000,
                            TransactionDate = new DateTime(2026, 4, 5, 9, 35, 0),
                            UserId = "split-user",
                        },
                        new()
                        {
                            LocationId = "V-C0-03",
                            Quantity = 15000,
                            TransactionDate = new DateTime(2026, 4, 5, 9, 40, 0),
                            UserId = "split-user",
                        },
                    }
                )
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedItems.Should().HaveCount(3);
        result
            .Data.UpdatedItems.Should()
            .ContainSingle(item =>
                item.SavedRowQuantity == 25000 && item.ProposedLocation == "V-C0-01"
            );
        result
            .Data.UpdatedItems.Should()
            .ContainSingle(item =>
                item.SavedRowQuantity == 10000 && item.ProposedLocation == "V-C0-02"
            );
        result
            .Data.UpdatedItems.Should()
            .ContainSingle(item =>
                item.SavedRowQuantity == 15000 && item.ProposedLocation == "V-C0-03"
            );
    }

    [Fact]
    public async Task ApplyLocationUpdateAsync_ShouldPersistReviewedHistoryRow()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var historyRow = CreateLoad("66868", "MMC-400", "5", "OLD-LOC");
        var item = new Model_ReceivingLocationReconciliationItem
        {
            DataSource = "History",
            PartID = historyRow.PartID,
            PONumber = historyRow.PoNumber ?? string.Empty,
            POLineNumber = historyRow.PoLineNumber,
            ExistingLocation = historyRow.InitialLocation,
            ProposedLocation = "C-77",
            Resolution = "Updated",
            SourceLoad = historyRow,
        };

        mySqlReceivingMock
            .Setup(service =>
                service.UpdateReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(1);

        var result = await service.ApplyLocationUpdateAsync(item);

        result.IsSuccess.Should().BeTrue();
        historyRow.InitialLocation.Should().Be("C-77");
        mySqlReceivingMock.Verify(
            service =>
                service.UpdateReceivingLoadsAsync(
                    It.Is<List<Model_ReceivingLoad>>(loads =>
                        loads.Count == 1 && loads[0] == historyRow
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ReconcileLocationsAsync_ShouldMarkRowAmbiguous_WhenInventoryHasMultipleUnmatchedLocations()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var historyRow = CreateLoad("66868", "MMC-300", string.Empty, "OLD-LOC");
        historyRow.WeightQuantity = 2;
        historyRow.UnitOfMeasure = "EA";
        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { historyRow })
            );

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-300",
                    null,
                    historyRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "A-01",
                            CurrentQuantity = 2,
                            ReceiptCount = 1,
                        },
                        new()
                        {
                            CurrentWarehouseId = "003",
                            CurrentLocationId = "B-05",
                            CurrentQuantity = 2,
                            ReceiptCount = 1,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-300",
                    null,
                    historyRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualLocationTransaction>())
            );

        var result = await service.ReconcileLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AmbiguousCount.Should().Be(1);
        result.Data.TotalRowsUpdated.Should().Be(0);
        mySqlReceivingMock.Verify(
            service => service.UpdateReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>()),
            Times.Never
        );
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldMarkSameLocationAsUnchanged_WhenExactQuantityHistoryMatches()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-550", "1", "V-C0-01");
        currentLabelRow.WeightQuantity = 1500;
        currentLabelRow.UnitOfMeasure = "EA";

        mySqlReceivingMock
            .Setup(s => s.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(s => s.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-550",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "V-C0-01",
                            CurrentQuantity = 3000,
                            MatchedTransactionQuantity = 3000,
                            ReceiptCount = 2,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-550",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationTransaction>
                    {
                        new()
                        {
                            LocationId = "V-C0-01",
                            Quantity = 1500,
                            TransactionDate = new DateTime(2026, 4, 5, 9, 0, 0),
                            UserId = "tester",
                        },
                    }
                )
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().BeEmpty();
        result.Data.UnchangedCount.Should().Be(1);
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldMarkSameLocationAsUnchanged_WhenExactCurrentQuantityMatchesCurrentLocation()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-552", "1", "RECV");
        currentLabelRow.WeightQuantity = 1500;
        currentLabelRow.UnitOfMeasure = "EA";

        mySqlReceivingMock
            .Setup(s => s.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(s => s.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-552",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "RECV",
                            CurrentQuantity = 1500,
                            ReceiptCount = 1,
                            LatestReceiptWarehouseId = "002",
                            LatestReceiptLocationId = "RECV",
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-552",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualLocationTransaction>())
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().BeEmpty();
        result.Data.UnchangedCount.Should().Be(1);
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldFlagPendingVisualReceipt_WhenMysqlQuantityIsAheadOfVisual()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-551", "1", "RECV");
        currentLabelRow.WeightQuantity = 4000;
        currentLabelRow.UnitOfMeasure = "EA";

        mySqlReceivingMock
            .Setup(s => s.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(s => s.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-551",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "V-C0-01",
                            CurrentQuantity = 1500,
                            ReceiptCount = 1,
                            LatestReceiptWarehouseId = "002",
                            LatestReceiptLocationId = "RECV",
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-551",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualLocationTransaction>())
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UnresolvedItems.Should().ContainSingle();
        result.Data.UnresolvedItems[0].Resolution.Should().Be("PendingVisualReceipt");
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldNotProposeClosestFit_WhenVisualQuantityDoesNotExactlyMatch()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-556", "1", "RECV");
        currentLabelRow.WeightQuantity = 8;
        currentLabelRow.UnitOfMeasure = "EA";

        mySqlReceivingMock
            .Setup(s => s.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(s => s.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-556",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "B-22",
                            CurrentQuantity = 10,
                            ReceiptCount = 1,
                            LatestReceiptWarehouseId = "002",
                            LatestReceiptLocationId = "RECV",
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-556",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualLocationTransaction>())
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().BeEmpty();
        result.Data.UnresolvedItems.Should().ContainSingle();
        result.Data.UnresolvedItems[0].Resolution.Should().Be("NotFound");
        result.Data.UnresolvedItems[0].Details.Should().ContainEquivalentOf("exact quantity match");
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldAllocateAllRows_WhenSingleTransferMatchesFullReceiptDayTotal()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var loadOne = CreateLoad("66868", "MMC0000650", "1", "RECV");
        loadOne.WeightQuantity = 5000;
        loadOne.UnitOfMeasure = "EA";
        loadOne.LoadID = Guid.NewGuid();

        var loadTwo = CreateLoad("66868", "MMC0000650", "1", "RECV");
        loadTwo.WeightQuantity = 5000;
        loadTwo.UnitOfMeasure = "EA";
        loadTwo.LoadID = Guid.NewGuid();

        var loadThree = CreateLoad("66868", "MMC0000650", "1", "RECV");
        loadThree.WeightQuantity = 5000;
        loadThree.UnitOfMeasure = "EA";
        loadThree.LoadID = Guid.NewGuid();

        var loadFour = CreateLoad("66868", "MMC0000650", "1", "RECV");
        loadFour.WeightQuantity = 5000;
        loadFour.UnitOfMeasure = "EA";
        loadFour.LoadID = Guid.NewGuid();

        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_ReceivingLoad> { loadOne, loadTwo, loadThree, loadFour }
                )
            );
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC0000650",
                    "1",
                    loadOne.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "V-A0-01",
                            CurrentQuantity = 20000,
                            MatchedTransactionQuantity = 20000,
                            MatchedTransactionCount = 1,
                            TotalMatchedTransactionQuantity = 20000,
                            TotalMatchedTransactionCount = 1,
                            MatchedTransactionDate = new DateTime(2026, 4, 5, 9, 30, 0),
                            MatchedTransactionUserId = "aggregate-user",
                            ReceiptCount = 4,
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC0000650",
                    "1",
                    loadOne.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationTransaction>
                    {
                        new()
                        {
                            LocationId = "V-A0-01",
                            Quantity = 20000,
                            TransactionDate = new DateTime(2026, 4, 5, 9, 30, 0),
                            UserId = "aggregate-user",
                        },
                    }
                )
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedItems.Should().HaveCount(4);
        result.Data.UpdatedItems.Should().OnlyContain(item => item.ProposedLocation == "V-A0-01");
        result.Data.UpdatedItems.Should().OnlyContain(item => item.QuantityMoved == 5000);
        result
            .Data.UpdatedItems.Should()
            .Contain(item => item.AllocationMethod == "Aggregate same-day transfer quantity match");
        result.Data.UnresolvedItems.Should().BeEmpty();
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldProposeWorkCenter_WhenVisualShowsWorkCenterAdjustmentHistoryWithoutOnHandInventory()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        var currentLabelRow = CreateLoad("66868", "MMC-553", "1", "RECV");
        currentLabelRow.WeightQuantity = 1500;
        currentLabelRow.UnitOfMeasure = "EA";

        mySqlReceivingMock
            .Setup(s => s.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(s => s.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-553",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            ReceiptCount = 1,
                            LatestReceiptWarehouseId = "002",
                            LatestReceiptLocationId = "RECV",
                            LatestReceiptEvidenceDate = new DateTime(2026, 4, 5, 8, 0, 0),
                        },
                    }
                )
            );
        inforVisualMock
            .Setup(s =>
                s.GetReceivingLocationTransactionHistoryAsync(
                    "PO-066868",
                    "MMC-553",
                    "1",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationTransaction>
                    {
                        new()
                        {
                            LocationId = "RECV",
                            Quantity = 1500,
                            TransactionDate = new DateTime(2026, 4, 5, 8, 0, 0),
                            UserId = "receiver",
                            ReceiptLocationId = "RECV",
                        },
                        new()
                        {
                            LocationId = "WC",
                            Quantity = 1500,
                            TransactionDate = new DateTime(2026, 4, 5, 10, 0, 0),
                            UserId = "mover",
                            ReceiptLocationId = "RECV",
                        },
                        new()
                        {
                            LocationId = "WC",
                            Quantity = -1500,
                            TransactionDate = new DateTime(2026, 4, 5, 11, 0, 0),
                            UserId = "adjuster",
                            ReceiptLocationId = "RECV",
                        },
                    }
                )
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data!.UpdatedItems.Should().ContainSingle();
        result.Data.UpdatedItems[0].ProposedLocation.Should().Be("WC");
        result.Data.UpdatedItems[0].MovedByUserId.Should().Be("mover");
        result.Data.UpdatedItems[0].MovedAt.Should().Be(new DateTime(2026, 4, 5, 10, 0, 0));
    }

    [Fact]
    public async Task ReconcileLocationsAsync_ShouldUseExpandedHistoryWindow_WhenIncludeAllHistoryIsTrue()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = CreateService(mySqlReceivingMock, inforVisualMock);

        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));

        var result = await service.ReconcileLocationsAsync(includeAllHistory: true);

        result.IsSuccess.Should().BeTrue();
        mySqlReceivingMock.Verify(
            service =>
                service.GetAllReceivingLoadsAsync(
                    It.Is<DateTime>(date => date == new DateTime(2000, 1, 1)),
                    It.Is<DateTime>(date => date.Date == DateTime.Today.AddDays(1).Date)
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task PreviewLocationsAsync_ShouldSkipUpdatedRows_WhenProposedLocationIsIgnoredByUserPreference()
    {
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var inforVisualMock = new Mock<IService_InforVisual>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var sessionManagerMock = new Mock<IService_UserSessionManager>();
        var service = CreateService(
            mySqlReceivingMock,
            inforVisualMock,
            receivingSettingsMock,
            sessionManagerMock
        );

        var currentLabelRow = CreateLoad("66868", "MMC-155", "3", "OLD-LOC");
        mySqlReceivingMock
            .Setup(service => service.GetCurrentLabelDataAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad> { currentLabelRow })
            );
        mySqlReceivingMock
            .Setup(service =>
                service.GetAllReceivingLoadsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_ReceivingLoad>()));
        receivingSettingsMock
            .Setup(service =>
                service.GetStringAsync(
                    ReceivingSettingsKeys.UserPreferences.IgnoredReconciliationLocationsJson,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync("[\"B-15\"]");

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC-155",
                    "3",
                    currentLabelRow.ReceivedDate
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "B-15",
                            CurrentQuantity = 42,
                            ReceiptCount = 1,
                            LatestTransactionWarehouseId = "002",
                            LatestTransactionLocationId = "B-15",
                        },
                    }
                )
            );

        var result = await service.PreviewLocationsAsync(includeAllHistory: false);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedItems.Should().BeEmpty();
        result.Data.SkippedCount.Should().Be(1);
        result.Data.UnresolvedItems.Should().BeEmpty();
    }

    private static Service_ReceivingLocationReconciliation CreateService(
        Mock<IService_MySQL_Receiving> mySqlReceivingMock,
        Mock<IService_InforVisual> inforVisualMock,
        Mock<IService_ReceivingSettings>? receivingSettingsMock = null,
        Mock<IService_UserSessionManager>? sessionManagerMock = null
    )
    {
        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationTransactionHistoryAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_InforVisualLocationTransaction>())
            );

        return new Service_ReceivingLocationReconciliation(
            mySqlReceivingMock.Object,
            inforVisualMock.Object,
            new Mock<IService_LoggingUtility>().Object,
            (receivingSettingsMock ?? new Mock<IService_ReceivingSettings>()).Object,
            (sessionManagerMock ?? new Mock<IService_UserSessionManager>()).Object
        );
    }

    private static Model_ReceivingLoad CreateLoad(
        string poNumber,
        string partId,
        string? poLineNumber,
        string initialLocation
    )
    {
        return new Model_ReceivingLoad
        {
            PoNumber = poNumber,
            PartID = partId,
            PoLineNumber = poLineNumber ?? string.Empty,
            InitialLocation = initialLocation,
            ReceivedDate = new DateTime(2026, 4, 5, 8, 0, 0),
            HistoryRecordID = 10,
        };
    }
}

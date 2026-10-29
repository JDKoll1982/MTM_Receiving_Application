using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

/// <summary>
/// Covers the App Data service: the four reads become one dataset, one failed read fails the
/// dataset and carries its reason, and the comparison sends the identical bounds to Infor Visual.
/// </summary>
public sealed class Service_Tool_ReceivingAnalyticsAppTests
{
    private const string TestConnectionString = "TEST-connection";

    private static readonly DateTime RangeStart = new(2026, 9, 1);
    private static readonly DateTime RangeEnd = new(2026, 10, 1);

    private static Model_Tool_ReceivingAnalyticsAppFilter CreateFilter() =>
        new() { StartDate = RangeStart, EndDate = RangeEnd };

    private static Mock<Dao_Tool_ReceivingAnalyticsApp> CreateDaoMock()
    {
        var dao = new Mock<Dao_Tool_ReceivingAnalyticsApp>(TestConnectionString);
        dao.Setup(x => x.GetActivityByDayAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_ReceivingAnalyticsAppActivityRow>
                    {
                        new()
                        {
                            BucketDate = RangeStart,
                            PartType = "STANDARD",
                            UnitOfMeasure = "LB",
                            QuantityTotal = 10m,
                            LoadCount = 1,
                            LabelCount = 2,
                        },
                    }
                )
            );
        dao.Setup(x => x.GetOnTimeByDayAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_ReceivingAnalyticsAppOnTimeRow>
                    {
                        new()
                        {
                            BucketDate = RangeStart,
                            Status = "OnTime",
                            ReceiptCount = 2,
                        },
                    }
                )
            );
        dao.Setup(x => x.GetSupplierTotalsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_ReceivingAnalyticsAppSupplierRow>
                    {
                        new()
                        {
                            VendorName = "TEST SUPPLIER",
                            UnitOfMeasure = "LB",
                            LabelCount = 2,
                            LoadCount = 1,
                            QuantityTotal = 10m,
                        },
                    }
                )
            );
        dao.Setup(x => x.GetMissingLoadNumberCountAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(4));
        dao.Setup(x => x.GetReceivedPartNumbersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<string> { "TEST-PART-1", "TEST-PART-2" })
            );
        return dao;
    }

    private static Service_Tool_ReceivingAnalyticsApp CreateService(
        Mock<Dao_Tool_ReceivingAnalyticsApp>? dao = null,
        Mock<IService_InforVisual>? inforVisual = null
    ) =>
        new(
            (dao ?? CreateDaoMock()).Object,
            (inforVisual ?? new Mock<IService_InforVisual>()).Object,
            new Mock<IService_LoggingUtility>().Object
        );

    [Fact]
    public async Task GetAppDataAsync_ShouldAssembleTheFourReadsIntoOneDataset()
    {
        var service = CreateService();

        var result = await service.GetAppDataAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ActivityRows.Should().ContainSingle();
        result.Data.OnTimeRows.Should().ContainSingle();
        result.Data.SupplierRows.Should().ContainSingle();
        result.Data.MissingLoadNumberCount.Should().Be(4);
        result.Data.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task GetAppDataAsync_ShouldFailTheWholeDatasetAndCarryTheReason_WhenOneReadFails()
    {
        var dao = CreateDaoMock();
        dao.Setup(x => x.GetSupplierTotalsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<
                    List<Model_Tool_ReceivingAnalyticsAppSupplierRow>
                >("supplier totals unavailable")
            );

        var service = CreateService(dao);

        var result = await service.GetAppDataAsync(CreateFilter());

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("supplier totals unavailable");
    }

    [Fact]
    public async Task GetAppDataAsync_ShouldSendTheSameBoundsToEveryRead()
    {
        var dao = CreateDaoMock();
        var service = CreateService(dao);

        await service.GetAppDataAsync(CreateFilter());

        dao.Verify(x => x.GetActivityByDayAsync(RangeStart, RangeEnd), Times.Once);
        dao.Verify(x => x.GetOnTimeByDayAsync(RangeStart, RangeEnd), Times.Once);
        dao.Verify(x => x.GetSupplierTotalsAsync(RangeStart, RangeEnd), Times.Once);
        dao.Verify(x => x.GetMissingLoadNumberCountAsync(RangeStart, RangeEnd), Times.Once);
    }

    [Fact]
    public async Task GetComparisonAsync_ShouldPassTheSameBoundsToTheErpRead()
    {
        Model_InforVisualReceivingAnalyticsFilter? requested = null;
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(x =>
                x.GetReceivingAnalyticsCoverageByPartAsync(
                    It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()
                )
            )
            .Callback<Model_InforVisualReceivingAnalyticsFilter>(filter => requested = filter)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualReceivingCoveragePoint>
                    {
                        new()
                        {
                            ActivityDate = RangeStart,
                            Category = "Parts",
                            PartId = "TEST-PART-1",
                            LineCount = 5,
                        },
                    }
                )
            );

        var service = CreateService(inforVisual: inforVisual);

        var result = await service.GetComparisonAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        requested.Should().NotBeNull();
        requested!.FromDate.Should().Be(RangeStart);
        requested.ToDate.Should().Be(RangeEnd);
        result.Data!.ErpAvailable.Should().BeTrue();
        result.Data.ErpCoverageSeries.Should().ContainSingle();
        result.Data.ErpCategoryTotals["Parts"].Should().Be(5);
    }

    [Fact]
    public async Task GetComparisonAsync_ShouldCountOnlyErpRowsWhosePartTheAppAlsoReceived()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(x =>
                x.GetReceivingAnalyticsCoverageByPartAsync(
                    It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualReceivingCoveragePoint>
                    {
                        new()
                        {
                            ActivityDate = RangeStart,
                            Category = "Parts",
                            PartId = "TEST-PART-1",
                            LineCount = 3,
                        },
                        new()
                        {
                            ActivityDate = RangeStart,
                            Category = "Parts",
                            PartId = "TEST-PART-2",
                            LineCount = 4,
                        },
                        new()
                        {
                            ActivityDate = RangeStart,
                            Category = "MMC Coils",
                            PartId = "TEST-NEVER-IN-APP",
                            LineCount = 90,
                        },
                    }
                )
            );

        var service = CreateService(inforVisual: inforVisual);

        var result = await service.GetComparisonAsync(CreateFilter());

        result.Data!.ErpCoverageSeries.Single().Count.Should().Be(7);
        result.Data.ErpCategoryTotals["Parts"].Should().Be(7);
        result.Data.ErpCategoryTotals.Should().NotContainKey("MMC Coils");
    }

    [Fact]
    public async Task GetComparisonAsync_ShouldMatchPartNumbersRegardlessOfCase()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(x =>
                x.GetReceivingAnalyticsCoverageByPartAsync(
                    It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualReceivingCoveragePoint>
                    {
                        new()
                        {
                            ActivityDate = RangeStart,
                            Category = "Parts",
                            PartId = " test-part-1 ",
                            LineCount = 6,
                        },
                    }
                )
            );

        var service = CreateService(inforVisual: inforVisual);

        var result = await service.GetComparisonAsync(CreateFilter());

        result.Data!.ErpCoverageSeries.Single().Count.Should().Be(6);
    }

    [Fact]
    public async Task GetComparisonAsync_ShouldLeaveNoErpLine_WhenTheAppPartReadFails()
    {
        var dao = CreateDaoMock();
        dao.Setup(x => x.GetReceivedPartNumbersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<string>>("part numbers unavailable")
            );

        var inforVisual = new Mock<IService_InforVisual>();
        var service = CreateService(dao, inforVisual);

        var result = await service.GetComparisonAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Data!.ErpAvailable.Should().BeFalse();
        inforVisual.Verify(
            x =>
                x.GetReceivingAnalyticsCoverageByPartAsync(
                    It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task GetComparisonAsync_ShouldReportErpUnavailable_WhenTheErpReadFails()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(x =>
                x.GetReceivingAnalyticsCoverageByPartAsync(
                    It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<
                    List<Model_InforVisualReceivingCoveragePoint>
                >("MTMFG unreachable")
            );

        var service = CreateService(inforVisual: inforVisual);

        var result = await service.GetComparisonAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Data!.ErpAvailable.Should().BeFalse();
        result.Data.ErpCoverageSeries.Should().BeEmpty();
        result.Data.ErpCategoryTotals.Should().BeEmpty();
    }
}

using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

public sealed class Service_Tool_ReceivingAnalyticsTests
{
    private static Mock<IService_InforVisual> CreateInforVisualMock(
        IReadOnlyList<Model_InforVisualReceivingAnalyticsPoint>? history = null,
        IReadOnlyList<Model_InforVisualReceivingAnalyticsPoint>? forecast = null
    )
    {
        var mock = new Mock<IService_InforVisual>();
        mock.Setup(service => service.GetReceivingAnalyticsHistoryAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_InforVisualReceivingAnalyticsPoint>>(
                    history is null ? new List<Model_InforVisualReceivingAnalyticsPoint>() : [.. history]
                )
            );
        mock.Setup(service => service.GetReceivingAnalyticsForecastAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_InforVisualReceivingAnalyticsPoint>>(
                    forecast is null ? new List<Model_InforVisualReceivingAnalyticsPoint>() : [.. forecast]
                )
            );
        return mock;
    }

    private static Service_Tool_ReceivingAnalytics CreateService(Mock<IService_InforVisual> inforVisualMock) =>
        new(inforVisualMock.Object, new Mock<IService_LoggingUtility>().Object);

    [Fact]
    public async Task GetAnalyticsAsync_ShouldReturnBothDatasets()
    {
        var history = new List<Model_InforVisualReceivingAnalyticsPoint>
        {
            new() { ActivityDate = new System.DateTime(2026, 8, 31), Category = "MMC Coils", LineCount = 5 },
        };
        var forecast = new List<Model_InforVisualReceivingAnalyticsPoint>
        {
            new() { ActivityDate = new System.DateTime(2026, 9, 4), Category = "Parts", LineCount = 12 },
        };
        var service = CreateService(CreateInforVisualMock(history, forecast));

        var result = await service.GetAnalyticsAsync(new Model_InforVisualReceivingAnalyticsFilter());

        result.IsSuccess.Should().BeTrue();
        result.Data!.History.Should().ContainSingle().Which.Category.Should().Be("MMC Coils");
        result.Data.Forecast.Should().ContainSingle().Which.LineCount.Should().Be(12);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ShouldPropagateHistoryFailure()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        inforVisualMock
            .Setup(service => service.GetReceivingAnalyticsHistoryAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_InforVisualReceivingAnalyticsPoint>>("boom")
            );
        inforVisualMock
            .Setup(service => service.GetReceivingAnalyticsForecastAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_InforVisualReceivingAnalyticsPoint>>([])
            );
        var service = CreateService(inforVisualMock);

        var result = await service.GetAnalyticsAsync(new Model_InforVisualReceivingAnalyticsFilter());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("boom");
    }

    [Fact]
    public async Task ComputeStatsAsync_ShouldComputeTotalsAverageAndPeak()
    {
        var service = CreateService(CreateInforVisualMock());
        var points = new List<Model_Tool_ReceivingAnalyticsPoint>
        {
            new() { Date = new System.DateTime(2026, 8, 31), Category = "Parts", LineCount = 10 },
            new() { Date = new System.DateTime(2026, 8, 31), Category = "MMC Coils", LineCount = 20 },
            new() { Date = new System.DateTime(2026, 9, 1), Category = "Parts", LineCount = 30 },
        };

        var stats = await service.ComputeStatsAsync(points);

        stats.TotalLines.Should().Be(60);
        stats.AvgPerDay.Should().Be(30.0);
        stats.PeakDay.Should().Be(30);
    }

    [Fact]
    public async Task ComputeStatsAsync_ShouldReturnZero_WhenNoPoints()
    {
        var service = CreateService(CreateInforVisualMock());

        var stats = await service.ComputeStatsAsync([]);

        stats.TotalLines.Should().Be(0);
        stats.PeakDay.Should().Be(0);
        stats.AvgPerDay.Should().Be(0.0);
    }

    [Fact]
    public async Task ComputeStatsAsync_ShouldComputeTrendVsPreviousPeriod()
    {
        var service = CreateService(CreateInforVisualMock());
        var current = new List<Model_Tool_ReceivingAnalyticsPoint>
        {
            new() { Date = new System.DateTime(2026, 8, 31), Category = "Parts", LineCount = 150 },
        };
        var previous = new List<Model_Tool_ReceivingAnalyticsPoint>
        {
            new() { Date = new System.DateTime(2026, 8, 24), Category = "Parts", LineCount = 100 },
        };

        var stats = await service.ComputeStatsAsync(current, previous);

        stats.TrendVsLastWeek.Should().Be(50.0);
    }

    [Fact]
    public async Task ComputeStatsAsync_ShouldReturnZeroTrend_WhenNoPreviousPeriod()
    {
        var service = CreateService(CreateInforVisualMock());
        var current = new List<Model_Tool_ReceivingAnalyticsPoint>
        {
            new() { Date = new System.DateTime(2026, 8, 31), Category = "Parts", LineCount = 150 },
        };

        var stats = await service.ComputeStatsAsync(current);

        stats.TrendVsLastWeek.Should().Be(0.0);
        stats.TotalLines.Should().Be(150);
    }
}
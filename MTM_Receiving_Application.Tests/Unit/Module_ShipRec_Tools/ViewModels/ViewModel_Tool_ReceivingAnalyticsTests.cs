using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_ReceivingAnalyticsTests
{
    private static ViewModel_Tool_ReceivingAnalytics CreateViewModel(
        Mock<IService_Tool_ReceivingAnalytics> serviceMock
    ) =>
        new(
            serviceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Mock<IService_Tool_ReceivingAnalytics> CreateServiceMock()
    {
        var mock = new Mock<IService_Tool_ReceivingAnalytics>();
        mock.Setup(service => service.GetAnalyticsAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_Tool_ReceivingAnalytics
                    {
                        History =
                        [
                            new() { Date = new System.DateTime(2026, 8, 31), Category = "Parts", LineCount = 5 },
                            new() { Date = new System.DateTime(2026, 8, 31), Category = "MMC Coils", LineCount = 10 },
                            new() { Date = new System.DateTime(2026, 9, 1), Category = "Parts", LineCount = 7 },
                        ],
                        Forecast =
                        [
                            new() { Date = new System.DateTime(2026, 9, 4), Category = "Parts", LineCount = 9 },
                            new() { Date = new System.DateTime(2026, 9, 5), Category = "MMC Coils", LineCount = 14 },
                        ],
                    }
                )
            );
        mock.Setup(service => service.ComputeStatsAsync(It.IsAny<IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint>>(), It.IsAny<IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint>?>()))
            .ReturnsAsync(
                new Model_Tool_ReceivingAnalyticsStats
                {
                    TotalLines = 22,
                    AvgPerDay = 11.0,
                    PeakDay = 15,
                    TrendVsLastWeek = 10.0,
                }
            );
        return mock;
    }

    [Fact]
    public async Task LoadAsync_ShouldBuildSeriesAndStats_FromHistory()
    {
        var viewModel = CreateViewModel(CreateServiceMock());
        viewModel.ViewMode = "History";

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.TotalLines.Should().Be(22);
        viewModel.AvgPerDay.Should().Be(11.0);
        viewModel.PeakDay.Should().Be(15);
        viewModel.TrendVsLastWeek.Should().Be(10.0);
        viewModel.Series.Should().NotBeEmpty();
        viewModel.XAxes.Should().NotBeEmpty();
        viewModel.YAxes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoadAsync_ShouldRequestPreviousPeriod_WhenCompareEnabled()
    {
        var serviceMock = CreateServiceMock();
        var viewModel = CreateViewModel(serviceMock);
        viewModel.ComparePreviousPeriod = true;

        await viewModel.LoadCommand.ExecuteAsync(null);

        serviceMock.Verify(
            service => service.GetAnalyticsAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task LoadAsync_ShouldRequestSinglePeriod_WhenCompareDisabled()
    {
        var serviceMock = CreateServiceMock();
        var viewModel = CreateViewModel(serviceMock);
        viewModel.ComparePreviousPeriod = false;

        await viewModel.LoadCommand.ExecuteAsync(null);

        serviceMock.Verify(
            service => service.GetAnalyticsAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()),
            Times.Once
        );
    }

    [Fact]
    public async Task LoadAsync_ShouldSwitchDataset_WhenViewModeChanges()
    {
        var viewModel = CreateViewModel(CreateServiceMock());
        viewModel.ViewMode = "Incoming";

        await viewModel.LoadCommand.ExecuteAsync(null);

        // Incoming uses the forecast dataset; with the mock the series still builds.
        viewModel.ViewModeLabel.Should().Be("Incoming");
        viewModel.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ToggleViewMode_ShouldRerenderFromCachedData_WithoutRequery()
    {
        var serviceMock = CreateServiceMock();
        var viewModel = CreateViewModel(serviceMock);
        viewModel.ViewMode = "History";

        await viewModel.LoadCommand.ExecuteAsync(null);
        serviceMock.Verify(
            service => service.GetAnalyticsAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()),
            Times.Once
        );

        // Toggle History -> Incoming: must re-render client-side and NOT re-query.
        viewModel.SelectedViewModeIndex = 1;

        viewModel.ViewModeLabel.Should().Be("Incoming");
        viewModel.Series.Should().NotBeEmpty();
        serviceMock.Verify(
            service => service.GetAnalyticsAsync(It.IsAny<Model_InforVisualReceivingAnalyticsFilter>()),
            Times.Once
        );
    }

    [Fact]
    public void DefaultDateWindow_ShouldCoverHistoryAndIncoming()
    {
        var viewModel = CreateViewModel(CreateServiceMock());

        // Mirrors the WIP analytics queue window: -1 year (history) to +6 months (incoming).
        viewModel.FromDate.Date.Should().Be(DateTime.Today.AddYears(-1));
        viewModel.ToDate.Date.Should().Be(DateTime.Today.AddMonths(6));
    }

    [Fact]
    public async Task ExportAsync_ShouldBuildDocumentWithChart_WhenSeriesExist()
    {
        var viewModel = CreateViewModel(CreateServiceMock());
        await viewModel.LoadCommand.ExecuteAsync(null);

        Model_FormattedReportDocument? captured = null;
        viewModel.RequestExportAsync = doc =>
        {
            captured = doc;
            return Task.FromResult(Model_Dao_Result_Factory.Success(true));
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.DocumentTitle.Should().Be("Receiving Analytics");
        captured.HtmlFragment.Should().Contain("Total Lines");
        // Chart image data URI is embedded as base64 PNG.
        captured.HtmlFragment.Should().Contain(
            "data:image/png;base64,",
            viewModel.LastChartRenderError
        );
    }

    [Fact]
    public async Task ExportAsync_ShouldNotOpen_WhenNoSeries()
    {
        var viewModel = CreateViewModel(CreateServiceMock());
        var opened = false;
        viewModel.RequestExportAsync = _ =>
        {
            opened = true;
            return Task.FromResult(Model_Dao_Result_Factory.Success(true));
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        opened.Should().BeFalse();
    }
}
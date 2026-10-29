using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Covers the App Data tab: the range it asks for, what a load fills, what a period or supplier
/// measure switch re-renders without re-querying, the unit label on every quantity total, and the
/// on-time, supplier and coverage panels built from the cached rows.
/// </summary>
public sealed class ViewModel_Tool_ReceivingAnalyticsAppTests
{
    private const string MillTestStart = "2026-09-30";

    private static ViewModel_Tool_ReceivingAnalyticsApp CreateViewModel(
        Mock<IService_Tool_ReceivingAnalyticsApp> serviceMock,
        decimal? onTimeTarget = 95m
    )
    {
        var settings = new Mock<IService_ShipRecToolsSettings>();
        settings
            .Setup(service => service.GetReceivingAnalyticsOnTimeTargetAsync())
            .ReturnsAsync(onTimeTarget);

        return new(
            serviceMock.Object,
            settings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Mock<IService_Tool_ReceivingAnalyticsApp> CreateServiceMock(
        Model_Tool_ReceivingAnalyticsAppDataset dataset,
        Model_Tool_ReceivingAnalyticsAppComparison? comparison = null
    )
    {
        var mock = new Mock<IService_Tool_ReceivingAnalyticsApp>();
        mock.Setup(service =>
                service.GetAppDataAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(dataset));
        mock.Setup(service =>
                service.GetComparisonAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    comparison ?? new Model_Tool_ReceivingAnalyticsAppComparison()
                )
            );
        return mock;
    }

    private static Model_Tool_ReceivingAnalyticsAppDataset SampleDataset()
    {
        var start = DateTime.Parse(MillTestStart);

        return new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows =
            [
                new()
                {
                    BucketDate = start,
                    PartType = "STANDARD",
                    UnitOfMeasure = "LB",
                    QuantityTotal = 120.5m,
                    LoadCount = 2,
                    LabelCount = 4,
                },
                new()
                {
                    BucketDate = start.AddDays(1),
                    PartType = "MMC-1",
                    UnitOfMeasure = "LB",
                    QuantityTotal = 80m,
                    LoadCount = 1,
                    LabelCount = 2,
                },
                new()
                {
                    BucketDate = start.AddDays(1),
                    PartType = "STANDARD",
                    UnitOfMeasure = "EA",
                    QuantityTotal = 3m,
                    LoadCount = 1,
                    LabelCount = 2,
                },
            ],
            OnTimeRows =
            [
                new()
                {
                    BucketDate = start,
                    Status = "OnTime",
                    ReceiptCount = 4,
                },
            ],
            SupplierRows =
            [
                new()
                {
                    VendorName = "TEST SUPPLIER",
                    UnitOfMeasure = "LB",
                    LabelCount = 4,
                    LoadCount = 2,
                    QuantityTotal = 120.5m,
                },
            ],
            MissingLoadNumberCount = 3,
        };
    }

    private static IReadOnlyList<string> AxisLabels(IEnumerable<ICartesianAxis> axes) =>
        axes.OfType<Axis>().First().Labels?.ToList() ?? [];

    [Fact]
    public void Constructor_ShouldDefaultTheRangeToTheLastThirtyDays()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(new Model_Tool_ReceivingAnalyticsAppDataset())
        );

        viewModel.FromDate.Date.Should().Be(DateTime.Today.AddDays(-29));
        viewModel.ToDate.Date.Should().Be(DateTime.Today);
        viewModel.Period.Should().Be("Day");
        viewModel.SelectedPeriodIndex.Should().Be(0);
    }

    [Fact]
    public async Task LoadAsync_ShouldAskForTheInclusiveRangeAsAnExclusiveEnd()
    {
        Model_Tool_ReceivingAnalyticsAppFilter? requested = null;
        var serviceMock = new Mock<IService_Tool_ReceivingAnalyticsApp>();
        serviceMock
            .Setup(service =>
                service.GetAppDataAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .Callback<Model_Tool_ReceivingAnalyticsAppFilter>(filter => requested = filter)
            .ReturnsAsync(Model_Dao_Result_Factory.Success(SampleDataset()));

        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);

        requested.Should().NotBeNull();
        requested!.StartDate.Should().Be(DateTime.Today.AddDays(-29));
        requested.EndDate.Should().Be(DateTime.Today.AddDays(1));

        viewModel.IsEmpty.Should().BeFalse();
        viewModel.IsBusy.Should().BeFalse();
        viewModel.LoadsLabelsSeries.Should().HaveCount(2);
        viewModel.LoadsLabelsXAxes.Should().NotBeEmpty();
        viewModel.QuantitySeries.Should().NotBeEmpty();
        viewModel.MissingLoadNumberText.Should().Contain("3");
    }

    [Fact]
    public async Task LoadAsync_ShouldOpenOnTheDominantUnit()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.UnitOptions.Should().BeEquivalentTo("LB", "EA");
        viewModel.SelectedUnit.Should().Be("LB");
        viewModel.QuantityLabel.Should().Be("Quantity (LB)");
    }

    [Fact]
    public async Task LoadAsync_ShouldDrawOneQuantityLinePerUnit_WhenTwoUnitsTie()
    {
        var start = DateTime.Parse(MillTestStart);
        var dataset = new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows =
            [
                Activity(start, "STANDARD", "LB", 60m),
                Activity(start, "STANDARD", "EA", 40m),
            ],
        };

        var viewModel = CreateViewModel(CreateServiceMock(dataset));

        await viewModel.LoadCommand.ExecuteAsync(null);

        // One receipt each, so no unit wins and nothing is added across them: two lines, each
        // naming its own unit.
        viewModel.SelectedUnit.Should().BeEmpty();
        SeriesValues(viewModel.QuantitySeries, "Quantity (EA)").Sum().Should().Be(40d);
        SeriesValues(viewModel.QuantitySeries, "Quantity (LB)").Sum().Should().Be(60d);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetIsEmpty_WhenTheRangeHoldsNoCompletedReceipts()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(new Model_Tool_ReceivingAnalyticsAppDataset())
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.IsEmpty.Should().BeTrue();
        viewModel.LoadsLabelsSeries.Should().BeEmpty();
        viewModel.QuantitySeries.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectedPeriodIndex_ShouldRebucketWithoutQueryingAgain()
    {
        var serviceMock = CreateServiceMock(SampleDataset());
        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);
        var dailyLabelCount = AxisLabels(viewModel.LoadsLabelsXAxes).Count;
        var weekdaysInRange = Enumerable
            .Range(0, 30)
            .Select(offset => DateTime.Today.AddDays(-29 + offset))
            .Count(day => day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday);

        viewModel.SelectedPeriodIndex = 1;

        dailyLabelCount.Should().Be(weekdaysInRange);
        viewModel.Period.Should().Be("Week");
        AxisLabels(viewModel.LoadsLabelsXAxes).Count.Should().BeInRange(5, 6);
        serviceMock.Verify(
            service => service.GetAppDataAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>()),
            Times.Once
        );
    }

    [Fact]
    public async Task LoadAsync_ShouldLeaveWeekendsOffTheDayAxis()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));

        await viewModel.LoadCommand.ExecuteAsync(null);

        var weekendLabels = Enumerable
            .Range(0, 30)
            .Select(offset => DateTime.Today.AddDays(-29 + offset))
            .Where(day => day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            .Select(day => day.ToString("MM/dd"))
            .ToList();

        AxisLabels(viewModel.LoadsLabelsXAxes).Should().NotIntersectWith(weekendLabels);
        AxisLabels(viewModel.OnTimeXAxes).Should().NotIntersectWith(weekendLabels);
        AxisLabels(viewModel.CoverageXAxes).Should().NotIntersectWith(weekendLabels);
    }

    [Fact]
    public async Task Clear_ShouldRestoreTheDefaultsAndEmptyThePanels()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.FromDate = DateTimeOffset.Now.AddDays(-100);
        viewModel.SelectedPeriodIndex = 2;
        viewModel.SelectedUnit = "EA";

        viewModel.ClearCommand.Execute(null);

        viewModel.FromDate.Date.Should().Be(DateTime.Today.AddDays(-29));
        viewModel.ToDate.Date.Should().Be(DateTime.Today);
        viewModel.Period.Should().Be("Day");
        viewModel.SelectedPeriodIndex.Should().Be(0);
        viewModel.SelectedUnit.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
        viewModel.LoadsLabelsSeries.Should().BeEmpty();
        viewModel.QuantitySeries.Should().BeEmpty();
        viewModel.MissingLoadNumberText.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_ShouldRaiseTheAlertButton_WhenTheAppReadFails()
    {
        var serviceMock = new Mock<IService_Tool_ReceivingAnalyticsApp>();
        serviceMock
            .Setup(service =>
                service.GetAppDataAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalyticsAppDataset>(
                    "MySQL is unreachable."
                )
            );
        serviceMock
            .Setup(service =>
                service.GetComparisonAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_Tool_ReceivingAnalyticsAppComparison())
            );

        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.HasWarning.Should().BeTrue();
        viewModel.WarningTitle.Should().Be("The app's own records could not be read");
        viewModel.WarningDetail.Should().Contain("MySQL is unreachable.");
        viewModel.WarningDetail.Should().Contain("Press Load");
    }

    [Fact]
    public async Task LoadAsync_ShouldRaiseTheAlertButton_WhenTheRangeEndsBeforeItStarts()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));
        viewModel.ToDate = viewModel.FromDate.AddDays(-1);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.HasWarning.Should().BeTrue();
        viewModel.WarningDetail.Should().Contain("To date");
    }

    [Fact]
    public async Task LoadAsync_ShouldClearTheAlertButton_WhenALoadSucceeds()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));
        viewModel.ToDate = viewModel.FromDate.AddDays(-1);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.HasWarning.Should().BeTrue();

        viewModel.ToDate = new DateTimeOffset(DateTime.Today);
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.HasWarning.Should().BeFalse();
        viewModel.WarningTitle.Should().BeEmpty();
        viewModel.WarningDetail.Should().BeEmpty();
    }

    [Fact]
    public async Task Clear_ShouldClearTheAlertButton()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));
        viewModel.ToDate = viewModel.FromDate.AddDays(-1);
        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.HasWarning.Should().BeTrue();

        viewModel.ClearCommand.Execute(null);

        viewModel.HasWarning.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldOpenOnTheFirstPanel()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(new Model_Tool_ReceivingAnalyticsAppDataset())
        );

        viewModel.SelectedPanelIndex.Should().Be(0);
    }

    [Fact]
    public async Task Clear_ShouldLeaveTheNavbarOnThePanelTheUserChose()
    {
        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset()));
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectedPanelIndex = 2;

        viewModel.ClearCommand.Execute(null);

        viewModel.SelectedPanelIndex.Should().Be(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void QuantityLabel_ShouldReadEA_WhenTheUnitIsBlank(string unit)
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(new Model_Tool_ReceivingAnalyticsAppDataset())
        );

        viewModel.SelectedUnit = unit;

        viewModel.QuantityLabel.Should().Be("Quantity (EA)");
    }

    [Theory]
    [InlineData("lb", "Quantity (LB)")]
    [InlineData("Coils", "Quantity (COILS)")]
    public void QuantityLabel_ShouldUppercaseTheUnit(string unit, string expected)
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(new Model_Tool_ReceivingAnalyticsAppDataset())
        );

        viewModel.SelectedUnit = unit;

        viewModel.QuantityLabel.Should().Be(expected);
    }

    // ---- Were we on time -------------------------------------------------------------------

    /// <summary>One activity row keeps the range non-empty, so the on-time panel renders.</summary>
    private static Model_Tool_ReceivingAnalyticsAppDataset OnTimeDataset(
        int early,
        int onTime,
        int late,
        int noDueDate
    )
    {
        var start = DateTime.Parse(MillTestStart);
        var onTimeRows = new List<Model_Tool_ReceivingAnalyticsAppOnTimeRow>();

        void Add(string status, int count)
        {
            if (count > 0)
            {
                onTimeRows.Add(
                    new()
                    {
                        BucketDate = start,
                        Status = status,
                        ReceiptCount = count,
                    }
                );
            }
        }

        Add("Early", early);
        Add("OnTime", onTime);
        Add("Late", late);
        Add("NoDueDate", noDueDate);

        return new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows =
            [
                new()
                {
                    BucketDate = start,
                    PartType = "STANDARD",
                    UnitOfMeasure = "LB",
                    QuantityTotal = 10m,
                    LoadCount = 1,
                    LabelCount = 1,
                },
            ],
            OnTimeRows = onTimeRows,
        };
    }

    private static double OnTimeBucketTotal(
        ViewModel_Tool_ReceivingAnalyticsApp viewModel,
        string status
    ) => SeriesValues(viewModel.OnTimeSeries, status).Sum();

    [Fact]
    public async Task LoadAsync_ShouldSplitDatedReceiptsIntoEarlyOnTimeAndLate()
    {
        var viewModel = CreateViewModel(CreateServiceMock(OnTimeDataset(3, 5, 2, 0)));

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel
            .OnTimeSeries.Select(series => series.Name)
            .Should()
            .Equal("Early", "OnTime", "Late");
        OnTimeBucketTotal(viewModel, "Early").Should().Be(3);
        OnTimeBucketTotal(viewModel, "OnTime").Should().Be(5);
        OnTimeBucketTotal(viewModel, "Late").Should().Be(2);
    }

    [Fact]
    public async Task LoadAsync_ShouldKeepTheNoDueDateRemainderOutOfThePercentage()
    {
        // 8 of the 10 dated receipts arrived early or on time, and the 7 undated ones are
        // neither: counting them would drag the share down to 47.06 per cent.
        var viewModel = CreateViewModel(CreateServiceMock(OnTimeDataset(3, 5, 2, 7)));

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.OnTimePercentage.Should().Be(80m);
        viewModel.OnTimePercentText.Should().Be("80.0%");
        viewModel.NoDueDateText.Should().Contain("7");
        OnTimeBucketTotal(viewModel, "Early").Should().Be(3);
        OnTimeBucketTotal(viewModel, "OnTime").Should().Be(5);
        OnTimeBucketTotal(viewModel, "Late").Should().Be(2);
    }

    [Fact]
    public async Task LoadAsync_ShouldCountAReceiptCompletedOnItsDueDateAsOnTime()
    {
        var viewModel = CreateViewModel(CreateServiceMock(OnTimeDataset(0, 4, 0, 0)));

        await viewModel.LoadCommand.ExecuteAsync(null);

        OnTimeBucketTotal(viewModel, "OnTime").Should().Be(4);
        OnTimeBucketTotal(viewModel, "Late").Should().Be(0);
        viewModel.OnTimePercentage.Should().Be(100m);
    }

    [Fact]
    public async Task LoadAsync_ShouldLeaveThePercentageAbsent_WhenNoReceiptCarriesADueDate()
    {
        var viewModel = CreateViewModel(CreateServiceMock(OnTimeDataset(0, 0, 0, 6)));

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.OnTimePercentage.Should().BeNull();
        viewModel.OnTimePercentText.Should().Be("n/a");
        viewModel.IsOnTimeBelowTarget.Should().BeFalse();
        viewModel.ShowTargetedPercentage.Should().BeFalse();
        viewModel.ShowPlainPercentage.Should().BeTrue();
        viewModel.NoDueDateText.Should().Contain("6");
    }

    [Fact]
    public async Task LoadAsync_ShouldShowThePercentagePlainly_WhenNoTargetIsConfigured()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(OnTimeDataset(3, 5, 2, 0)),
            onTimeTarget: null
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.HasTarget.Should().BeFalse();
        viewModel.OnTimeTarget.Should().BeNull();
        viewModel.IsOnTimeBelowTarget.Should().BeFalse();
        viewModel.OnTimePercentText.Should().Be("80.0%");
    }

    [Theory]
    [InlineData(96, 4, 95, false)]
    [InlineData(94, 6, 95, true)]
    [InlineData(92, 8, 90, false)]
    public async Task LoadAsync_ShouldCompareThePercentageToTheConfiguredTarget(
        int onTime,
        int late,
        decimal target,
        bool expectedBelow
    )
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(OnTimeDataset(0, onTime, late, 0)),
            onTimeTarget: target
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.OnTimeTarget.Should().Be(target);
        viewModel.HasTarget.Should().BeTrue();
        viewModel.IsOnTimeBelowTarget.Should().Be(expectedBelow);
    }

    private static IReadOnlyList<double> SeriesValues(IReadOnlyList<ISeries> series, string name)
    {
        for (var index = 0; index < series.Count; index++)
        {
            if (series[index].Name == name)
            {
                return SeriesValuesAt(series, index);
            }
        }

        return [];
    }

    private static IReadOnlyList<double> SeriesValuesAt(IReadOnlyList<ISeries> series, int index) =>
        series[index] switch
        {
            LineSeries<double> line => line.Values?.ToList() ?? [],
            ColumnSeries<double> column => column.Values?.ToList() ?? [],
            StackedColumnSeries<double> stacked => stacked.Values?.ToList() ?? [],
            _ => [],
        };

    // ---- Who is filling our dock -----------------------------------------------------------

    private static Model_Tool_ReceivingAnalyticsAppSupplierRow Supplier(
        string vendor,
        string unit,
        int labels,
        int loads,
        decimal quantity
    ) =>
        new()
        {
            VendorName = vendor,
            UnitOfMeasure = unit,
            LabelCount = labels,
            LoadCount = loads,
            QuantityTotal = quantity,
        };

    private static Model_Tool_ReceivingAnalyticsAppDataset SupplierDataset(
        params Model_Tool_ReceivingAnalyticsAppSupplierRow[] suppliers
    )
    {
        var start = DateTime.Parse(MillTestStart);

        return new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows =
            [
                new()
                {
                    BucketDate = start,
                    PartType = "STANDARD",
                    UnitOfMeasure = "LB",
                    QuantityTotal = 10m,
                    LoadCount = 1,
                    LabelCount = 1,
                },
            ],
            SupplierRows = suppliers,
        };
    }

    [Fact]
    public async Task LoadAsync_ShouldRankSuppliersByLabelsByDefault()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(
                SupplierDataset(
                    Supplier("BRAVO", "LB", 2, 1, 5m),
                    Supplier("ALPHA", "LB", 9, 3, 90m),
                    Supplier("CHARLIE", "LB", 5, 2, 40m)
                )
            )
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SupplierMeasure.Should().Be("Labels");
        viewModel
            .SupplierRankings.Select(entry => entry.VendorName)
            .Should()
            .Equal("ALPHA", "CHARLIE", "BRAVO");
        viewModel.SupplierSeries.Should().ContainSingle();
        SeriesValuesAt(viewModel.SupplierSeries, 0).Should().Equal(9d, 5d, 2d);
        viewModel.SupplierMeasureLabel.Should().Be("labels");
    }

    [Fact]
    public async Task LoadAsync_ShouldSumOnlyTheSelectedUnit_WhenRankingByQuantity()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(
                SupplierDataset(
                    Supplier("ALPHA", "LB", 4, 2, 100m),
                    Supplier("ALPHA", "EA", 4, 2, 900m),
                    Supplier("BRAVO", "LB", 3, 1, 30m)
                )
            )
        );

        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SupplierMeasure = "Quantity";

        viewModel.SelectedUnit.Should().Be("LB");
        viewModel
            .SupplierRankings.Select(entry => entry.Measure)
            .Should()
            .Equal(100m, 30m);
        viewModel.SupplierMeasureLabel.Should().Be("quantity in LB");
    }

    [Fact]
    public async Task LoadAsync_ShouldCollapseTheTailIntoOneRemainder_SoTheSharesReachOneHundred()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(
                SupplierDataset(
                    Supplier("S1", "LB", 10, 1, 1m),
                    Supplier("S2", "LB", 9, 1, 1m),
                    Supplier("S3", "LB", 8, 1, 1m),
                    Supplier("S4", "LB", 7, 1, 1m),
                    Supplier("S5", "LB", 6, 1, 1m),
                    Supplier("S6", "LB", 5, 1, 1m),
                    Supplier("S7", "LB", 4, 1, 1m)
                )
            )
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SupplierRankings.Should().HaveCount(6);
        viewModel.SupplierRankings[^1].IsRemainder.Should().BeTrue();
        viewModel.SupplierRankings[^1].Measure.Should().Be(9m);
        viewModel.SupplierRankings[^1].VendorName.Should().Contain("2");
        viewModel.SupplierRankings[^1].CumulativeShare.Should().Be(100m);
        viewModel.SupplierRankings[^2].CumulativeShare.Should().BeLessThan(100m);
    }

    [Fact]
    public async Task LoadAsync_ShouldRankABlankSupplierAsUnknown()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(
                SupplierDataset(Supplier(string.Empty, "LB", 2, 1, 1m), Supplier("ALPHA", "LB", 3, 1, 1m))
            )
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel
            .SupplierRankings.Select(entry => entry.VendorName)
            .Should()
            .Equal("ALPHA", "Unknown");
    }

    [Fact]
    public async Task LoadAsync_ShouldReachOneHundredPercent_ForASingleSupplier()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(SupplierDataset(Supplier("ALPHA", "LB", 1, 1, 1m)))
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SupplierRankings.Should().ContainSingle();
        viewModel.SupplierRankings[0].IsRemainder.Should().BeFalse();
        viewModel.SupplierRankings[0].CumulativeShare.Should().Be(100m);
        viewModel.SupplierLeadShareText.Should().Contain("100.0%");
    }

    [Fact]
    public async Task SupplierMeasure_ShouldReRankWithoutQueryingAgain()
    {
        var serviceMock = CreateServiceMock(
            SupplierDataset(Supplier("ALPHA", "LB", 9, 1, 10m), Supplier("BRAVO", "LB", 2, 8, 90m))
        );
        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);
        viewModel.SupplierRankings[0].VendorName.Should().Be("ALPHA");

        viewModel.SupplierMeasure = "Loads";

        viewModel.SupplierRankings[0].VendorName.Should().Be("BRAVO");
        viewModel.SupplierMeasureLabel.Should().Be("loads");
        serviceMock.Verify(
            service => service.GetAppDataAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>()),
            Times.Once
        );
    }

    // ---- How much does our app actually see ------------------------------------------------

    private static Model_Tool_ReceivingAnalyticsAppActivityRow Activity(
        DateTime date,
        string partType,
        string unit,
        decimal quantity,
        int receipts = 1
    ) =>
        new()
        {
            BucketDate = date,
            PartType = partType,
            UnitOfMeasure = unit,
            QuantityTotal = quantity,
            LoadCount = 1,
            LabelCount = receipts,
            ReceiptCount = receipts,
        };

    private static Model_Tool_ReceivingAnalyticsAppComparison ComparisonWithErpTotals(
        DateTime date,
        int appCount,
        int erpCount,
        Dictionary<string, int> erpTotals
    ) =>
        new()
        {
            AppCoverageSeries = [new(date, appCount)],
            ErpCoverageSeries = [new(date, erpCount)],
            ErpCategoryTotals = erpTotals,
        };

    [Fact]
    public async Task LoadAsync_ShouldRequestTheComparisonWithTheSameBoundsAsTheAppData()
    {
        Model_Tool_ReceivingAnalyticsAppFilter? appFilter = null;
        Model_Tool_ReceivingAnalyticsAppFilter? comparisonFilter = null;

        var serviceMock = new Mock<IService_Tool_ReceivingAnalyticsApp>();
        serviceMock
            .Setup(service =>
                service.GetAppDataAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .Callback<Model_Tool_ReceivingAnalyticsAppFilter>(filter => appFilter = filter)
            .ReturnsAsync(Model_Dao_Result_Factory.Success(SampleDataset()));
        serviceMock
            .Setup(service =>
                service.GetComparisonAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .Callback<Model_Tool_ReceivingAnalyticsAppFilter>(filter => comparisonFilter = filter)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_Tool_ReceivingAnalyticsAppComparison())
            );

        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);

        comparisonFilter.Should().NotBeNull();
        comparisonFilter!.StartDate.Should().Be(appFilter!.StartDate);
        comparisonFilter.EndDate.Should().Be(appFilter.EndDate);
    }

    [Fact]
    public async Task LoadAsync_ShouldAlignBothCoverageLinesOnTheSameDates()
    {
        var first = DateTime.Parse(MillTestStart);
        var comparison = new Model_Tool_ReceivingAnalyticsAppComparison
        {
            AppCoverageSeries = [new(first, 5), new(first.AddDays(2), 3)],
            ErpCoverageSeries = [new(first.AddDays(1), 7)],
        };

        var viewModel = CreateViewModel(CreateServiceMock(SampleDataset(), comparison));

        await viewModel.LoadCommand.ExecuteAsync(null);

        AxisLabels(viewModel.CoverageXAxes).Should().HaveCount(3);
        SeriesValues(viewModel.CoverageSeries, "App").Should().Equal(5d, 0d, 3d);
        SeriesValues(viewModel.CoverageSeries, "ERP").Should().Equal(0d, 7d, 0d);
        viewModel.IsErpUnavailable.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ShouldNameTheSameFiveCategoriesOnBothSplits_EachSummingTo100()
    {
        var start = DateTime.Parse(MillTestStart);
        // Quantity and receipt count are deliberately inverse here, so a share of quantity and a
        // share of receipts cannot both satisfy the assertions below.
        var dataset = new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows =
            [
                Activity(start, "STANDARD", "LB", 60m, receipts: 2),
                Activity(start, "MMC-1", "LB", 30m, receipts: 5),
                Activity(start, "MMF-1", "LB", 10m, receipts: 3),
            ],
        };
        var comparison = ComparisonWithErpTotals(
            start,
            4,
            9,
            new Dictionary<string, int>
            {
                ["Parts"] = 50,
                ["MMC Coils"] = 20,
                ["MMF Flat"] = 10,
                ["Outside Service"] = 15,
                ["Uninventoried"] = 5,
            }
        );

        var viewModel = CreateViewModel(CreateServiceMock(dataset, comparison));

        await viewModel.LoadCommand.ExecuteAsync(null);

        AxisLabels(viewModel.MixXAxes)
            .Should()
            .Equal("Parts", "MMC Coils", "MMF Flat", "Outside Service", "Uninventoried");

        var app = SeriesValuesAt(viewModel.MixAppSeries, 0);
        var erp = SeriesValuesAt(viewModel.MixErpSeries, 0);
        app.Should().HaveCount(5);
        erp.Should().HaveCount(5);
        app.Sum().Should().BeApproximately(100d, 0.1d);
        erp.Sum().Should().BeApproximately(100d, 0.1d);
        // 2, 5 and 3 receipts out of 10, not the 60, 30 and 10 in quantity.
        app[0].Should().BeApproximately(20d, 0.05d);
        app[1].Should().BeApproximately(50d, 0.05d);
        app[2].Should().BeApproximately(30d, 0.05d);
        erp[0].Should().BeApproximately(50d, 0.05d);
    }

    [Fact]
    public async Task LoadAsync_ShouldGiveTheOneCategoryAMonthHolds_AHundredPerCentAppShare()
    {
        var start = DateTime.Parse(MillTestStart);
        // One category, two units: the unit a label was recorded in does not split the split, so
        // every receipt lands in Parts and the app side reads 100 per cent.
        var dataset = new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows =
            [
                Activity(start, "STANDARD", "LB", 40m, receipts: 3),
                Activity(start.AddDays(1), "STANDARD", "EA", 2m, receipts: 7),
            ],
        };
        var comparison = ComparisonWithErpTotals(
            start,
            10,
            10,
            new Dictionary<string, int>
            {
                ["Parts"] = 10,
                ["MMC Coils"] = 0,
                ["MMF Flat"] = 0,
                ["Outside Service"] = 0,
                ["Uninventoried"] = 0,
            }
        );

        var viewModel = CreateViewModel(CreateServiceMock(dataset, comparison));

        await viewModel.LoadCommand.ExecuteAsync(null);

        var app = SeriesValuesAt(viewModel.MixAppSeries, 0);
        app[0].Should().BeApproximately(100d, 0.05d);
        app.Skip(1).Should().Equal(0d, 0d, 0d, 0d);
    }

    [Fact]
    public async Task LoadAsync_ShouldKeepAnErpOnlyCategoryAsItsOwnSegment()
    {
        var start = DateTime.Parse(MillTestStart);
        var dataset = new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows = [Activity(start, "STANDARD", "LB", 10m)],
        };
        var comparison = ComparisonWithErpTotals(
            start,
            3,
            12,
            new Dictionary<string, int>
            {
                ["Parts"] = 6,
                ["MMC Coils"] = 0,
                ["MMF Flat"] = 0,
                ["Outside Service"] = 8,
                ["Uninventoried"] = 2,
            }
        );

        var viewModel = CreateViewModel(CreateServiceMock(dataset, comparison));

        await viewModel.LoadCommand.ExecuteAsync(null);

        var app = SeriesValuesAt(viewModel.MixAppSeries, 0);
        var erp = SeriesValuesAt(viewModel.MixErpSeries, 0);
        app[3].Should().Be(0d);
        app[4].Should().Be(0d);
        erp[3].Should().BeApproximately(50d, 0.05d);
        erp[4].Should().BeApproximately(12.5d, 0.05d);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetIsErpUnavailable_WhenTheComparisonFails()
    {
        var serviceMock = CreateServiceMock(SampleDataset());
        serviceMock
            .Setup(service =>
                service.GetComparisonAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalyticsAppComparison>(
                    "MTMFG unreachable"
                )
            );

        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.IsErpUnavailable.Should().BeTrue();
        viewModel.ShowErpUnavailable.Should().BeTrue();
        viewModel.ShowComparison.Should().BeFalse();
        viewModel.IsEmpty.Should().BeFalse();
        viewModel.LoadsLabelsSeries.Should().NotBeEmpty();
        viewModel.OnTimeSeries.Should().NotBeEmpty();
        viewModel.SupplierSeries.Should().NotBeEmpty();
        viewModel.CoverageSeries.Should().BeEmpty();
        viewModel.MixAppSeries.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_ShouldSetIsErpUnavailable_WhenTheErpSideWasNotReadable()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(
                SampleDataset(),
                new Model_Tool_ReceivingAnalyticsAppComparison { ErpAvailable = false }
            )
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.IsErpUnavailable.Should().BeTrue();
        viewModel.CoverageSeries.Should().BeEmpty();
        viewModel.OnTimeSeries.Should().NotBeEmpty();
    }
}

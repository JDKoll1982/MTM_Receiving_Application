using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using SkiaSharp;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Receiving Analytics tool: an aggregate chart of receiving
/// history (past received) vs incoming items by date and category, plus summary
/// stat cards. Data is read from Infor Visual (read-only) and rendered with
/// LiveCharts2. The View Mode toggle switches between History and Incoming.
/// </summary>
public partial class ViewModel_Tool_ReceivingAnalytics : ViewModel_Tool_Base
{
    private const string PartsCategory = "Parts";
    private const string CoilsCategory = "MMC Coils";
    private const string FlatCategory = "MMF Flat";
    private const string OutsideCategory = "Outside Service";
    private const string UninventoriedCategory = "Uninventoried";

    private readonly IService_Tool_ReceivingAnalytics _service;

    /// <summary>
    /// Set by the view code-behind to open a generated HTML report in the browser.
    /// </summary>
    public Func<Model_FormattedReportDocument, Task<Model_Dao_Result<bool>>>? RequestExportAsync
    {
        get;
        set;
    }

    /// <summary>
    /// Most recent analytics datasets (History + Forecast) loaded from Infor Visual.
    /// The History/Incoming toggle switches over these cached datasets client-side
    /// (mirroring the WIP app: both queries are queued once, then toggled without a
    /// re-query). Null until the first successful load.
    /// </summary>
    private Model_Tool_ReceivingAnalytics? _lastData;

    /// <summary>
    /// Previous-period dataset (same window length, immediately prior) when
    /// <see cref="ComparePreviousPeriod"/> is enabled. Null when not requested.
    /// </summary>
    private IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint>? _lastPreviousPoints;

    // Default window mirrors the WIP app's analytics queue: -1 year (history) to
    // +6 months (incoming forecast). Keeps both View Mode datasets populated by default.
    [ObservableProperty]
    private DateTimeOffset _fromDate = DateTimeOffset.Now.Date.AddYears(-1);

    [ObservableProperty]
    private DateTimeOffset _toDate = DateTimeOffset.Now.Date.AddMonths(6);

    /// <summary>0=History, 1=Incoming/Forecast. Kept in sync with <see cref="ViewMode"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewMode))]
    [NotifyPropertyChangedFor(nameof(ViewModeLabel))]
    private int _selectedViewModeIndex;

    /// <summary>"History" or "Incoming".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewModeLabel))]
    private string _viewMode = "History";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodLabel))]
    private string _viewPeriod = "Day";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodLabel))]
    private string _chartType = "Line";

    /// <summary>0=Day, 1=Week, 2=Month (kept in sync with <see cref="ViewPeriod"/>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewPeriod))]
    private int _selectedPeriodIndex;

    /// <summary>0=Line, 1=Bar, 2=Stacked (kept in sync with <see cref="ChartType"/>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChartType))]
    private int _selectedChartTypeIndex;

    [ObservableProperty]
    private bool _scopeParts = true;

    [ObservableProperty]
    private bool _scopeCoils = true;

    [ObservableProperty]
    private bool _scopeFlat = true;

    [ObservableProperty]
    private bool _scopeOutside = true;

    [ObservableProperty]
    private bool _scopeUninventoried = true;

    [ObservableProperty]
    private bool _comparePreviousPeriod;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalLinesText))]
    private int _totalLines;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvgPerDayText))]
    private double _avgPerDay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeakDayText))]
    private int _peakDay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrendText))]
    private double _trendVsLastWeek;

    [ObservableProperty]
    private IReadOnlyList<ISeries> _series = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _xAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _yAxes = [];

    /// <summary>Display label for the current view period (Day/Week/Month).</summary>
    public string PeriodLabel => $"View Period: {ViewPeriod}";

    /// <summary>Label shown next to the History/Incoming toggle.</summary>
    public string ViewModeLabel => ViewMode;

    /// <summary>
    /// Last chart-image render error, if any. Used for diagnostics and tests.
    /// </summary>
    public string LastChartRenderError { get; private set; } = string.Empty;

    public string TotalLinesText => TotalLines.ToString("N0");

    public string AvgPerDayText => AvgPerDay.ToString("N1");

    public string PeakDayText => PeakDay.ToString("N0");

    public string TrendText =>
        TrendVsLastWeek > 0
            ? $"▲ {TrendVsLastWeek:0.#}%"
            : TrendVsLastWeek < 0
                ? $"▼ {Math.Abs(TrendVsLastWeek):0.#}%"
                : "0%";

    public ViewModel_Tool_ReceivingAnalytics(
        IService_Tool_ReceivingAnalytics service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        ToolTitle = "Receiving Analytics";
        ToolDescription = "Chart receiving history (past received) vs incoming items by date and category.";
        ViewMode = "History";
        ViewPeriod = "Day";
        ChartType = "Line";
    }

    partial void OnSelectedViewModeIndexChanged(int value)
    {
        ViewMode = value switch
        {
            1 => "Incoming",
            _ => "History",
        };

        // Switch the chart over the already-loaded datasets client-side (no re-query),
        // matching the WIP app's History/Incoming toggle behavior.
        if (_lastData is not null)
        {
            _ = RenderFromLastDataAsync();
        }
    }

    partial void OnViewModeChanged(string value)
    {
        SelectedViewModeIndex = value switch
        {
            "Incoming" => 1,
            _ => 0,
        };
    }

    partial void OnSelectedPeriodIndexChanged(int value)
    {
        ViewPeriod = value switch
        {
            1 => "Week",
            2 => "Month",
            _ => "Day",
        };
    }

    partial void OnSelectedChartTypeIndexChanged(int value)
    {
        ChartType = value switch
        {
            1 => "Bar",
            2 => "Stacked",
            _ => "Line",
        };
    }

    partial void OnViewPeriodChanged(string value)
    {
        SelectedPeriodIndex = value switch
        {
            "Week" => 1,
            "Month" => 2,
            _ => 0,
        };
    }

    partial void OnChartTypeChanged(string value)
    {
        SelectedChartTypeIndex = value switch
        {
            "Bar" => 1,
            "Stacked" => 2,
            _ => 0,
        };
    }

    public void ActivateView()
    {
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ShowStatus("Loading receiving analytics...", InfoBarSeverity.Informational);

            var filter = BuildFilter();
            var result = await _service.GetAnalyticsAsync(filter);
            if (!result.IsSuccess || result.Data is null)
            {
                _lastData = null;
                _lastPreviousPoints = null;
                ResetStatsAndSeries();
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            _lastData = result.Data;

            _lastPreviousPoints = null;
            if (ComparePreviousPeriod)
            {
                var previousResult = await _service.GetAnalyticsAsync(BuildPreviousPeriodFilter(filter));
                if (previousResult.IsSuccess && previousResult.Data is not null)
                {
                    _lastPreviousPoints =
                        ViewMode == "Incoming"
                            ? previousResult.Data.Forecast
                            : previousResult.Data.History;
                }
            }

            await RenderFromLastDataAsync();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Tool_ReceivingAnalytics)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Renders the chart and stats from the cached datasets, honoring the current
    /// <see cref="ViewMode"/> (History = past received, Incoming = forecast). This is
    /// shared by the initial load and the client-side History/Incoming toggle.
    /// </summary>
    private async Task RenderFromLastDataAsync()
    {
        if (_lastData is null)
        {
            return;
        }

        var activePoints = ViewMode == "Incoming" ? _lastData.Forecast : _lastData.History;
        var stats = await _service.ComputeStatsAsync(activePoints, _lastPreviousPoints);

        TotalLines = stats.TotalLines;
        AvgPerDay = stats.AvgPerDay;
        PeakDay = stats.PeakDay;
        TrendVsLastWeek = stats.TrendVsLastWeek;

        BuildSeries(activePoints);
        ShowStatus(
            activePoints.Count > 0
                ? $"Loaded {activePoints.Count} data point(s); {TotalLines} total line(s)."
                : "No receiving data matches the selected filters.",
            activePoints.Count > 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
        );
    }

    private void ResetStatsAndSeries()
    {
        TotalLines = 0;
        AvgPerDay = 0;
        PeakDay = 0;
        TrendVsLastWeek = 0;
        Series = [];
    }

    private static Model_InforVisualReceivingAnalyticsFilter BuildPreviousPeriodFilter(
        Model_InforVisualReceivingAnalyticsFilter current
    )
    {
        if (current.FromDate is null || current.ToDate is null)
        {
            return current;
        }

        var span = (current.ToDate.Value.Date - current.FromDate.Value.Date).Days + 1;
        var previousFrom = current.FromDate.Value.Date.AddDays(-span);
        var previousTo = current.ToDate.Value.Date.AddDays(-span);

        return new Model_InforVisualReceivingAnalyticsFilter
        {
            FromDate = previousFrom,
            ToDate = previousTo,
            ScopeParts = current.ScopeParts,
            ScopeCoils = current.ScopeCoils,
            ScopeFlat = current.ScopeFlat,
            ScopeOutside = current.ScopeOutside,
            ScopeUninventoried = current.ScopeUninventoried,
            MaxResults = current.MaxResults,
        };
    }

    [RelayCommand]
    private void Clear()
    {
        FromDate = DateTimeOffset.Now.Date.AddYears(-1);
        ToDate = DateTimeOffset.Now.Date.AddMonths(6);
        ViewMode = "History";
        ViewPeriod = "Day";
        ChartType = "Line";
        ScopeParts = true;
        ScopeCoils = true;
        ScopeFlat = true;
        ScopeOutside = true;
        ScopeUninventoried = true;
        ComparePreviousPeriod = false;
        _lastData = null;
        _lastPreviousPoints = null;
        ResetStatsAndSeries();
        ShowStatus("Filters cleared.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            if (RequestExportAsync is null)
            {
                ShowStatus("Export is unavailable in this context.", InfoBarSeverity.Warning);
                return;
            }

            if (Series.Count == 0)
            {
                ShowStatus("Nothing to export yet. Load the chart first.", InfoBarSeverity.Warning);
                return;
            }

            var document = await BuildExportDocumentAsync();
            var result = await RequestExportAsync(document);
            if (!result.IsSuccess || !result.Data)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ShowStatus("Opened the Receiving Analytics HTML export in your browser.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ExportAsync),
                nameof(ViewModel_Tool_ReceivingAnalytics)
            );
        }
    }

    private async Task<Model_FormattedReportDocument> BuildExportDocumentAsync()
    {
        var rangeLabel = $"{FromDate.Date:MM/dd/yyyy} - {ToDate.Date:MM/dd/yyyy}";
        var chartImageDataUri = await RenderChartToDataUriAsync();

        var html = new StringBuilder();
        html.AppendLine("<h1>Receiving Analytics</h1>");
        html.AppendLine($"<p><strong>Date Range:</strong> {rangeLabel}</p>");
        html.AppendLine($"<p><strong>View Mode:</strong> {ViewMode} &nbsp; <strong>View Period:</strong> {ViewPeriod}</p>");
        html.AppendLine($"<p><strong>Total Lines:</strong> {TotalLinesText}</p>");
        html.AppendLine($"<p><strong>Avg/Day:</strong> {AvgPerDayText} &nbsp; <strong>Peak Day:</strong> {PeakDayText} &nbsp; <strong>Trend vs Last Week:</strong> {TrendText}</p>");

        if (chartImageDataUri is not null)
        {
            html.AppendLine(
                $"<img src=\"{chartImageDataUri}\" alt=\"Receiving Analytics chart\" style=\"max-width:100%; height:auto;\" />"
            );
        }

        return new Model_FormattedReportDocument
        {
            DocumentTitle = "Receiving Analytics",
            HtmlFragment = html.ToString(),
            PageCss = "@page { margin: 0.4in; } body { font-family: Segoe UI, sans-serif; }",
        };
    }

    private async Task<string?> RenderChartToDataUriAsync()
    {
        try
        {
            // Render the same series/axes headlessly to a PNG and embed as base64.
            var chart = new SKCartesianChart
            {
                Width = 900,
                Height = 500,
                Series = Series.ToArray(),
                XAxes = XAxes.ToArray(),
                YAxes = YAxes.ToArray(),
            };

            using var image = chart.GetImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());
            return $"data:image/png;base64,{base64}";
        }
        catch (Exception ex)
        {
            LastChartRenderError = ex.ToString();
            _logger.LogError($"ReceivingAnalytics: failed to render chart image. {ex.Message}", ex);
            return null;
        }
    }

    private Model_InforVisualReceivingAnalyticsFilter BuildFilter()
    {
        // View period collapses the date range to the nearest Day/Week/Month boundary.
        var from = FromDate.Date;
        var to = ToDate.Date;

        switch (ViewPeriod)
        {
            case "Week":
                from = from.AddDays(-((int)from.DayOfWeek));
                break;
            case "Month":
                from = new DateTime(from.Year, from.Month, 1);
                break;
        }

        return new Model_InforVisualReceivingAnalyticsFilter
        {
            FromDate = from,
            ToDate = to,
            ScopeParts = ScopeParts,
            ScopeCoils = ScopeCoils,
            ScopeFlat = ScopeFlat,
            ScopeOutside = ScopeOutside,
            ScopeUninventoried = ScopeUninventoried,
            MaxResults = 50000,
        };
    }

    private void BuildSeries(IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint> points)
    {
        var categories = new[]
        {
            (Key: PartsCategory, Label: "Parts", Color: SKColors.DodgerBlue),
            (Key: CoilsCategory, Label: "MMC (Coils)", Color: SKColors.MediumSeaGreen),
            (Key: FlatCategory, Label: "MMF (Flat)", Color: SKColors.MediumPurple),
            (Key: OutsideCategory, Label: "Outside Service", Color: SKColors.Orange),
            (Key: UninventoriedCategory, Label: "Uninventoried", Color: SKColors.Gray),
        };

        var enabled = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            [PartsCategory] = ScopeParts,
            [CoilsCategory] = ScopeCoils,
            [FlatCategory] = ScopeFlat,
            [OutsideCategory] = ScopeOutside,
            [UninventoriedCategory] = ScopeUninventoried,
        };

        // Build a sorted, distinct date index so each series uses a stable integer
        // X coordinate and the axis can render short-date labels from a Labels list
        // (avoids DateTime tick/seconds conversion issues in the axis labeler).
        var distinctDates = new SortedSet<DateTime>();
        foreach (var point in points)
        {
            distinctDates.Add(point.Date.Date);
        }

        var dates = distinctDates.ToList();
        var dateIndex = new Dictionary<DateTime, int>(dates.Count);
        for (var index = 0; index < dates.Count; index++)
        {
            dateIndex[dates[index]] = index;
        }

        var seriesList = new List<ISeries>();
        foreach (var category in categories)
        {
            if (!enabled.TryGetValue(category.Key, out var isEnabled) || !isEnabled)
            {
                continue;
            }

            var categoryPoints = points
                .Where(p => string.Equals(p.Category, category.Key, StringComparison.Ordinal))
                .OrderBy(p => p.Date)
                .Select(
                    p =>
                        new ScheduleChartPoint(
                            dateIndex.TryGetValue(p.Date.Date, out var i) ? i : 0,
                            p.LineCount
                        )
                )
                .ToList();

            if (categoryPoints.Count == 0)
            {
                continue;
            }

            var name = category.Label;
            var stroke = new SolidColorPaint(category.Color) { StrokeThickness = 2 };

            switch (ChartType)
            {
                case "Bar":
                    seriesList.Add(
                        new ColumnSeries<ScheduleChartPoint>
                        {
                            Name = name,
                            Values = categoryPoints,
                            Mapping = (point, _) => new(point.X, point.Count),
                            Fill = new SolidColorPaint(category.Color),
                        }
                    );
                    break;
                case "Stacked":
                    seriesList.Add(
                        new StackedColumnSeries<ScheduleChartPoint>
                        {
                            Name = name,
                            Values = categoryPoints,
                            Mapping = (point, _) => new(point.X, point.Count),
                            Fill = new SolidColorPaint(category.Color),
                        }
                    );
                    break;
                default:
                    seriesList.Add(
                        new LineSeries<ScheduleChartPoint>
                        {
                            Name = name,
                            Values = categoryPoints,
                            Mapping = (point, _) => new(point.X, point.Count),
                            GeometrySize = 6,
                            Stroke = stroke,
                            Fill = null,
                        }
                    );
                    break;
            }
        }

        Series = seriesList;

        var labels = new List<string>(dates.Count);
        foreach (var date in dates)
        {
            labels.Add(date.ToShortDateString());
        }

        XAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = 0,
                TextSize = 12,
                MinStep = 1,
                UnitWidth = 1,
            },
        ];

        YAxes =
        [
            new Axis
            {
                Labeler = value => ((int)value).ToString(),
                TextSize = 12,
            },
        ];
    }

    /// <summary>
    /// Chart point whose X coordinate is an integer index into the axis date labels.
    /// </summary>
    private sealed record ScheduleChartPoint(int X, int Count);
}

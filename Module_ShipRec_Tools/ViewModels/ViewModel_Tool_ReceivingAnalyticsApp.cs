using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using SkiaSharp;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Drives the App Data tab of Receiving Analytics: the range and period filters, and the
/// how-busy-were-we panel built from the app's own completed receipts. The rows are cached on
/// load, so a period or unit change re-buckets in memory and never re-queries.
/// </summary>
public partial class ViewModel_Tool_ReceivingAnalyticsApp : ViewModel_Tool_Base
{
    /// <summary>How many suppliers are ranked individually before the tail is grouped.</summary>
    private const int LeadingSupplierCount = 5;

    private readonly IService_Tool_ReceivingAnalyticsApp _service;
    private readonly IService_ShipRecToolsSettings _settings;

    private Model_Tool_ReceivingAnalyticsAppDataset? _lastData;
    private Model_Tool_ReceivingAnalyticsAppFilter? _lastFilter;
    private Model_Tool_ReceivingAnalyticsAppComparison? _lastComparison;

    /// <summary>
    /// Which panel the navbar shows: 0 = Activity, 1 = On Time, 2 = Suppliers, 3 = Coverage.
    /// </summary>
    [ObservableProperty]
    private int _selectedPanelIndex;

    [ObservableProperty]
    private DateTimeOffset _fromDate = new(DateTime.Today.AddDays(-29));

    [ObservableProperty]
    private DateTimeOffset _toDate = new(DateTime.Today);

    /// <summary>"Day", "Week" or "Month".</summary>
    [ObservableProperty]
    private string _period = "Day";

    /// <summary>0=Day, 1=Week, 2=Month (kept in sync with <see cref="Period"/>).</summary>
    [ObservableProperty]
    private int _selectedPeriodIndex;

    /// <summary>
    /// Unit the quantity totals are summed in. Blank when two units tie, and the quantity panel
    /// then shows one line per unit instead of adding across units.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(QuantityLabel))]
    private string _selectedUnit = string.Empty;

    /// <summary>Units the loaded range actually contains.</summary>
    [ObservableProperty]
    private IReadOnlyList<string> _unitOptions = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowComparison))]
    [NotifyPropertyChangedFor(nameof(ShowErpUnavailable))]
    private bool _isEmpty = true;

    [ObservableProperty]
    private IReadOnlyList<ISeries> _loadsLabelsSeries = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _loadsLabelsXAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _loadsLabelsYAxes = [];

    [ObservableProperty]
    private IReadOnlyList<ISeries> _quantitySeries = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _quantityXAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _quantityYAxes = [];

    /// <summary>How many completed receipts in range carry no load number.</summary>
    [ObservableProperty]
    private string _missingLoadNumberText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<ISeries> _onTimeSeries = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _onTimeXAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _onTimeYAxes = [];

    /// <summary>
    /// Early plus on-time over the receipts that carry a due date, as a percentage. Null when no
    /// receipt in range carries a due date, so an absent figure never reads as zero or as a pass.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTargetedPercentage))]
    [NotifyPropertyChangedFor(nameof(ShowPlainPercentage))]
    private decimal? _onTimePercentage;

    /// <summary>The target in force, read from settings on every load. Null means none is configured.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTarget))]
    [NotifyPropertyChangedFor(nameof(OnTimeTargetText))]
    [NotifyPropertyChangedFor(nameof(ShowTargetedPercentage))]
    [NotifyPropertyChangedFor(nameof(ShowPlainPercentage))]
    private decimal? _onTimeTarget;

    [ObservableProperty]
    private bool _isOnTimeBelowTarget;

    [ObservableProperty]
    private string _onTimePercentText = string.Empty;

    /// <summary>The receipts that carry no PO due date, which are never counted as on time.</summary>
    [ObservableProperty]
    private string _noDueDateText = string.Empty;

    /// <summary>True when a target is configured, so the percentage can be marked pass or fail.</summary>
    public bool HasTarget => OnTimeTarget.HasValue;

    /// <summary>The target in force, for example <c>Target 95.0%</c>. Empty when none is configured.</summary>
    public string OnTimeTargetText =>
        OnTimeTarget is { } target ? $"Target {target:N1}%" : string.Empty;

    /// <summary>A figure is marked pass or fail only where a target and a percentage both exist.</summary>
    public bool ShowTargetedPercentage => HasTarget && OnTimePercentage.HasValue;

    /// <summary>An absent figure, or one with no target, is shown plainly with no colour.</summary>
    public bool ShowPlainPercentage => !ShowTargetedPercentage;

    /// <summary>The measure the supplier ranking is counted in, naming the unit for a quantity.</summary>
    public string SupplierMeasureLabel =>
        SupplierMeasureValue switch
        {
            Enum_ReceivingAnalyticsSupplierMeasure.Quantity =>
                $"quantity in {NormalizeUnit(SelectedUnit)}",
            Enum_ReceivingAnalyticsSupplierMeasure.Loads => "loads",
            _ => "labels",
        };

    /// <summary>The comparison charts are drawn only when the range holds work and the ERP was read.</summary>
    public bool ShowComparison => IsEmpty is false && IsErpUnavailable is false;

    /// <summary>The ERP message replaces the comparison charts, never both at once.</summary>
    public bool ShowErpUnavailable => IsEmpty is false && IsErpUnavailable;

    /// <summary>
    /// The app side of the mix panel. Both halves are shares of receipts, so the label names the
    /// measure rather than a unit.
    /// </summary>
    public string AppMixLabel => "App receipts";

    /// <summary>"Labels", "Quantity" or "Loads".</summary>
    [ObservableProperty]
    private string _supplierMeasure = "Labels";

    /// <summary>0=Labels, 1=Quantity, 2=Loads (kept in sync with <see cref="SupplierMeasure"/>).</summary>
    [ObservableProperty]
    private int _selectedSupplierMeasureIndex;

    /// <summary>The ranked suppliers, with the tail grouped into one remainder entry.</summary>
    [ObservableProperty]
    private IReadOnlyList<SupplierRankingEntry> _supplierRankings = [];

    [ObservableProperty]
    private IReadOnlyList<ISeries> _supplierSeries = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _supplierXAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _supplierYAxes = [];

    /// <summary>What the leading suppliers add up to, for example <c>of labels</c>.</summary>
    [ObservableProperty]
    private string _supplierLeadShareText = string.Empty;

    /// <summary>Two coverage lines over one set of dates: the app's receipts and the ERP's.</summary>
    [ObservableProperty]
    private IReadOnlyList<ISeries> _coverageSeries = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _coverageXAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _coverageYAxes = [];

    /// <summary>The app's share of receipts per ERP category.</summary>
    [ObservableProperty]
    private IReadOnlyList<ISeries> _mixAppSeries = [];

    /// <summary>The ERP's share of receipt lines per ERP category.</summary>
    [ObservableProperty]
    private IReadOnlyList<ISeries> _mixErpSeries = [];

    /// <summary>The five ERP category names, shared by both sides of the mix panel.</summary>
    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _mixXAxes = [];

    [ObservableProperty]
    private IEnumerable<ICartesianAxis> _mixYAxes = [];

    /// <summary>True when the ERP half could not be read, which empties the comparison panel only.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowComparison))]
    [NotifyPropertyChangedFor(nameof(ShowErpUnavailable))]
    private bool _isErpUnavailable = true;

    /// <summary>Every quantity total is named by its unit, for example <c>Quantity (LB)</c>.</summary>
    public string QuantityLabel => $"Quantity ({NormalizeUnit(SelectedUnit)})";

    public ViewModel_Tool_ReceivingAnalyticsApp(
        IService_Tool_ReceivingAnalyticsApp service,
        IService_ShipRecToolsSettings settings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    private Enum_ReceivingAnalyticsPeriod PeriodValue =>
        Period switch
        {
            "Week" => Enum_ReceivingAnalyticsPeriod.Week,
            "Month" => Enum_ReceivingAnalyticsPeriod.Month,
            _ => Enum_ReceivingAnalyticsPeriod.Day,
        };

    private Enum_ReceivingAnalyticsSupplierMeasure SupplierMeasureValue =>
        SupplierMeasure switch
        {
            "Quantity" => Enum_ReceivingAnalyticsSupplierMeasure.Quantity,
            "Loads" => Enum_ReceivingAnalyticsSupplierMeasure.Loads,
            _ => Enum_ReceivingAnalyticsSupplierMeasure.Labels,
        };

    partial void OnSelectedPeriodIndexChanged(int value)
    {
        Period = value switch
        {
            1 => "Week",
            2 => "Month",
            _ => "Day",
        };
    }

    partial void OnPeriodChanged(string value)
    {
        SelectedPeriodIndex = value switch
        {
            "Week" => 1,
            "Month" => 2,
            _ => 0,
        };

        RenderFromLastData();
    }

    partial void OnSelectedUnitChanged(string value)
    {
        RenderFromLastData();
    }

    partial void OnSelectedSupplierMeasureIndexChanged(int value)
    {
        SupplierMeasure = value switch
        {
            1 => "Quantity",
            2 => "Loads",
            _ => "Labels",
        };
    }

    partial void OnSupplierMeasureChanged(string value)
    {
        SelectedSupplierMeasureIndex = value switch
        {
            "Quantity" => 1,
            "Loads" => 2,
            _ => 0,
        };

        RenderFromLastData();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (ToDate.Date < FromDate.Date)
        {
            ShowStatus(
                "The end date is before the start date, so nothing was loaded.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus("Loading the app's receiving activity...", InfoBarSeverity.Informational);

            var filter = BuildFilter();
            var targetTask = ReadOnTimeTargetAsync();
            // The ERP read is the slow half, so it runs alongside the app reads.
            var comparisonTask = ReadComparisonAsync(filter);
            var result = await _service.GetAppDataAsync(filter);
            OnTimeTarget = await targetTask;
            _lastComparison = await comparisonTask;

            if (!result.IsSuccess || result.Data is null)
            {
                _lastData = null;
                _lastFilter = null;
                _lastComparison = null;
                ClearPanels();
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            _lastData = result.Data;
            _lastFilter = filter;
            SelectDominantUnit(result.Data);
            RenderFromLastData();

            if (result.Data.IsEmpty)
            {
                ShowStatus(
                    "No completed receipts in the selected range.",
                    InfoBarSeverity.Warning
                );
                return;
            }

            ShowStatus(
                IsErpUnavailable
                    ? $"Loaded {result.Data.ActivityRows.Count} activity row(s). Infor Visual could not be read, so the comparison panel is empty."
                    : $"Loaded {result.Data.ActivityRows.Count} activity row(s) from the app's records.",
                IsErpUnavailable ? InfoBarSeverity.Warning : InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Tool_ReceivingAnalyticsApp)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        _lastData = null;
        _lastFilter = null;
        _lastComparison = null;

        FromDate = new DateTimeOffset(DateTime.Today.AddDays(-29));
        ToDate = new DateTimeOffset(DateTime.Today);
        SelectedPeriodIndex = 0;
        Period = "Day";
        SelectedSupplierMeasureIndex = 0;
        SupplierMeasure = "Labels";
        SelectedUnit = string.Empty;
        ClearPanels();

        ShowStatus("Filters cleared.", InfoBarSeverity.Informational);
    }

    private Model_Tool_ReceivingAnalyticsAppFilter BuildFilter() =>
        new()
        {
            // The picker range ends on the last day the user names; the stored procedures take
            // an exclusive end, so the start of the following day is passed.
            StartDate = FromDate.Date,
            EndDate = ToDate.Date.AddDays(1),
        };

    /// <summary>
    /// Reads the configured on-time target. A settings read that fails leaves no target, so the
    /// percentage is shown plainly instead of failing the whole load.
    /// </summary>
    private async Task<decimal?> ReadOnTimeTargetAsync()
    {
        try
        {
            return await _settings.GetReceivingAnalyticsOnTimeTargetAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"ReceivingAnalytics: the on-time target could not be read, so no target is shown. {ex.Message}"
            );
            return null;
        }
    }

    /// <summary>
    /// Reads the ERP comparison. A failure here empties the comparison panel only; the three app
    /// panels keep the figures they already have.
    /// </summary>
    private async Task<Model_Tool_ReceivingAnalyticsAppComparison?> ReadComparisonAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    )
    {
        try
        {
            var result = await _service.GetComparisonAsync(filter);
            return result.IsSuccess ? result.Data : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("ReceivingAnalytics: the ERP comparison could not be read.", ex);
            return null;
        }
    }

    /// <summary>
    /// Opens the quantity panel on the unit the range's completed work happened in most often.
    /// Two units tied means no single unit is chosen, and the panel shows one line per unit.
    /// </summary>
    private void SelectDominantUnit(Model_Tool_ReceivingAnalyticsAppDataset dataset)
    {
        var rowsPerUnit = dataset
            .ActivityRows.GroupBy(row => NormalizeUnit(row.UnitOfMeasure), StringComparer.Ordinal)
            .Select(group => (Unit: group.Key, Rows: group.Count()))
            .OrderByDescending(entry => entry.Rows)
            .ThenBy(entry => entry.Unit, StringComparer.Ordinal)
            .ToList();

        var unitOptions = new List<string>(rowsPerUnit.Count);
        foreach (var entry in rowsPerUnit)
        {
            unitOptions.Add(entry.Unit);
        }

        UnitOptions = unitOptions;

        var tied = rowsPerUnit.Count > 1 && rowsPerUnit[0].Rows == rowsPerUnit[1].Rows;
        SelectedUnit = rowsPerUnit.Count == 0 || tied ? string.Empty : rowsPerUnit[0].Unit;
    }

    /// <summary>
    /// Re-renders both panels from the cached rows, so a period or unit change never re-queries.
    /// </summary>
    private void RenderFromLastData()
    {
        if (_lastData is null || _lastFilter is null)
        {
            ClearPanels();
            return;
        }

        IsEmpty = _lastData.IsEmpty;
        MissingLoadNumberText = BuildMissingLoadNumberText(_lastData.MissingLoadNumberCount);

        if (_lastData.IsEmpty)
        {
            ClearSeries();
            return;
        }

        var buckets = BuildBuckets(_lastFilter.StartDate, _lastFilter.EndDate, PeriodValue);
        var bucketLabels = new List<string>(buckets.Count);
        foreach (var bucket in buckets)
        {
            bucketLabels.Add(bucket.Label);
        }

        BuildLoadsAndLabels(buckets, bucketLabels);
        BuildQuantityStack(buckets, bucketLabels);
        BuildOnTimeStack(buckets, bucketLabels);
        BuildSupplierRanking();
        BuildComparisonPanels();
    }

    /// <summary>
    /// Stacks the early, on-time and late receipts per period. The percentage is early plus
    /// on-time over the receipts that carry a due date; the undated remainder is reported
    /// separately and is never counted as on time.
    /// </summary>
    private void BuildOnTimeStack(List<Bucket> buckets, List<string> bucketLabels)
    {
        foreach (var row in _lastData!.OnTimeRows)
        {
            var index = BucketIndexOf(buckets, row.BucketDate);
            if (index < 0)
            {
                continue;
            }

            switch (row.Status)
            {
                case "Early":
                    buckets[index].Early += row.ReceiptCount;
                    break;
                case "OnTime":
                    buckets[index].OnTime += row.ReceiptCount;
                    break;
                case "Late":
                    buckets[index].Late += row.ReceiptCount;
                    break;
                case "NoDueDate":
                    buckets[index].NoDueDate += row.ReceiptCount;
                    break;
            }
        }

        OnTimeSeries =
        [
            BuildOnTimeSeries("Early", buckets, bucket => bucket.Early, SKColors.MediumSeaGreen),
            BuildOnTimeSeries("OnTime", buckets, bucket => bucket.OnTime, SKColors.DodgerBlue),
            BuildOnTimeSeries("Late", buckets, bucket => bucket.Late, SKColors.IndianRed),
        ];
        OnTimeXAxes = [BuildCategoryAxis(bucketLabels)];
        OnTimeYAxes = [BuildCountAxis()];

        var early = buckets.Sum(bucket => bucket.Early);
        var onTime = buckets.Sum(bucket => bucket.OnTime);
        var late = buckets.Sum(bucket => bucket.Late);
        var dated = early + onTime + late;

        OnTimePercentage =
            dated == 0 ? null : Math.Round(100m * (early + onTime) / dated, 1, MidpointRounding.AwayFromZero);
        OnTimePercentText = OnTimePercentage is { } percentage ? $"{percentage:N1}%" : "n/a";
        IsOnTimeBelowTarget =
            OnTimePercentage is { } value && OnTimeTarget is { } target && value < target;
        NoDueDateText = BuildNoDueDateText(buckets.Sum(bucket => bucket.NoDueDate));
    }

    private static StackedColumnSeries<double> BuildOnTimeSeries(
        string name,
        List<Bucket> buckets,
        Func<Bucket, int> count,
        SKColor color
    )
    {
        var values = new double[buckets.Count];
        for (var index = 0; index < buckets.Count; index++)
        {
            values[index] = count(buckets[index]);
        }

        return new StackedColumnSeries<double>
        {
            Name = name,
            Values = values,
            Fill = new SolidColorPaint(color),
        };
    }

    private static string BuildNoDueDateText(int count) =>
        count switch
        {
            0 => "Every completed receipt in this range carries a PO due date.",
            1 => "1 completed receipt carries no PO due date, so it is never counted as on time.",
            _ =>
                $"{count:N0} completed receipts carry no PO due date, so they are never counted as on time.",
        };

    /// <summary>
    /// Ranks the suppliers by the selected measure and groups everything past the leading few
    /// into one remainder, so the cumulative share across the entries reaches one hundred per cent.
    /// The ranking is rebuilt from the cached rows, never from a new query.
    /// </summary>
    private void BuildSupplierRanking()
    {
        var unit = NormalizeUnit(SelectedUnit);
        var measure = SupplierMeasureValue;

        // A supplier's label and load counts repeat on every unit row, so they are read once per
        // supplier. Quantity is the only figure that belongs to a single unit row.
        var suppliers = new Dictionary<string, SupplierTotal>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in _lastData!.SupplierRows)
        {
            var name = NormalizeVendor(row.VendorName);
            suppliers.TryGetValue(name, out var running);
            running ??= new SupplierTotal(name, 0, 0, 0m);

            var inSelectedUnit = string.Equals(
                NormalizeUnit(row.UnitOfMeasure),
                unit,
                StringComparison.Ordinal
            )
                ? row.QuantityTotal
                : 0m;

            suppliers[name] = new SupplierTotal(
                name,
                Math.Max(running.Labels, row.LabelCount),
                Math.Max(running.Loads, row.LoadCount),
                running.Quantity + inSelectedUnit
            );
        }

        var ranked = suppliers
            .Values.Select(supplier => (
                supplier.Name,
                Measure: measure switch
                {
                    Enum_ReceivingAnalyticsSupplierMeasure.Quantity => supplier.Quantity,
                    Enum_ReceivingAnalyticsSupplierMeasure.Loads => supplier.Loads,
                    _ => supplier.Labels,
                }
            ))
            .Where(entry => entry.Measure > 0)
            .OrderByDescending(entry => entry.Measure)
            .ThenBy(entry => entry.Name, StringComparer.Ordinal)
            .ToList();

        var total = ranked.Sum(entry => entry.Measure);
        if (total <= 0)
        {
            SupplierRankings = [];
            SupplierSeries = [];
            SupplierXAxes = [];
            SupplierYAxes = [];
            SupplierLeadShareText = "No supplier activity in this range.";
            return;
        }

        var entries = new List<SupplierRankingEntry>(LeadingSupplierCount + 1);
        var runningTotal = 0m;
        foreach (var entry in ranked.Take(LeadingSupplierCount))
        {
            runningTotal += entry.Measure;
            entries.Add(
                new SupplierRankingEntry(
                    entry.Name,
                    entry.Measure,
                    ShareOf(runningTotal, total),
                    false
                )
            );
        }

        var tail = ranked.Skip(LeadingSupplierCount).ToList();
        if (tail.Count > 0)
        {
            var tailMeasure = tail.Sum(entry => entry.Measure);
            entries.Add(
                new SupplierRankingEntry(
                    $"Other suppliers ({tail.Count})",
                    tailMeasure,
                    100m,
                    true
                )
            );
        }

        SupplierRankings = entries;
        SupplierSeries =
        [
            new ColumnSeries<double>
            {
                Name = measure switch
                {
                    Enum_ReceivingAnalyticsSupplierMeasure.Quantity => QuantityLabel,
                    Enum_ReceivingAnalyticsSupplierMeasure.Loads => "Loads",
                    _ => "Labels",
                },
                Values = entries.ConvertAll(entry => (double)entry.Measure),
                Fill = new SolidColorPaint(SKColors.SteelBlue),
            },
        ];
        SupplierXAxes =
        [
            new Axis
            {
                Labels = entries.ConvertAll(entry => entry.VendorName),
                LabelsRotation = -30,
                TextSize = 11,
            },
        ];
        SupplierYAxes = [BuildCountAxis()];

        var leadingCount = entries.Count - (tail.Count > 0 ? 1 : 0);
        var leadingShare = entries[leadingCount - 1].CumulativeShare;
        SupplierLeadShareText =
            (leadingCount == 1
                ? $"The only supplier accounts for {leadingShare:N1}% of {SupplierMeasureLabel}."
                : $"The top {leadingCount} suppliers account for {leadingShare:N1}% of {SupplierMeasureLabel}.")
            + (tail.Count > 0
                ? $" The remaining {tail.Count} make up the rest, so the shares total 100%."
                : string.Empty);
    }

    private static decimal ShareOf(decimal part, decimal total) =>
        Math.Round(100m * part / total, 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Draws the two coverage lines and the two-sided category mix. When the ERP half could not be
    /// read, only this panel empties: the three app panels are left exactly as they were.
    /// </summary>
    private void BuildComparisonPanels()
    {
        var comparison = _lastComparison;
        if (comparison?.ErpAvailable is not true)
        {
            IsErpUnavailable = true;
            ClearComparisonSeries();
            return;
        }

        IsErpUnavailable = false;
        BuildCoverage(comparison);
        BuildCategoryMix(comparison);
    }

    /// <summary>
    /// Both lines are drawn over one set of dates, so a reader compares the same days on both
    /// sides. A date only one side recorded still gets a point, at zero, on the other line.
    /// Weekend dates are dropped from both sides together, so excluding weekend work from the app
    /// never reads as lost coverage against the ERP.
    /// </summary>
    private void BuildCoverage(Model_Tool_ReceivingAnalyticsAppComparison comparison)
    {
        var dates = comparison
            .AppCoverageSeries.Select(point => point.Date.Date)
            .Concat(comparison.ErpCoverageSeries.Select(point => point.Date.Date))
            .Where(date => !IsWeekend(date))
            .Distinct()
            .Order()
            .ToList();

        var appCounts = CountsByDate(comparison.AppCoverageSeries);
        var erpCounts = CountsByDate(comparison.ErpCoverageSeries);

        var appValues = new double[dates.Count];
        var erpValues = new double[dates.Count];
        var labels = new List<string>(dates.Count);
        for (var index = 0; index < dates.Count; index++)
        {
            appValues[index] = appCounts.GetValueOrDefault(dates[index]);
            erpValues[index] = erpCounts.GetValueOrDefault(dates[index]);
            labels.Add(dates[index].ToString("MM/dd"));
        }

        CoverageSeries =
        [
            BuildCoverageLine("App", appValues, SKColors.DodgerBlue),
            BuildCoverageLine("ERP", erpValues, SKColors.DarkOrange),
        ];
        CoverageXAxes = [BuildCategoryAxis(labels)];
        CoverageYAxes = [BuildCountAxis()];
    }

    private static LineSeries<double> BuildCoverageLine(
        string name,
        double[] values,
        SKColor color
    ) =>
        new()
        {
            Name = name,
            Values = values,
            GeometrySize = 6,
            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill = null,
        };

    private static Dictionary<DateTime, int> CountsByDate(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint> points
    ) =>
        points
            .GroupBy(point => point.Date.Date)
            .ToDictionary(group => group.Key, group => group.Sum(point => point.Count));

    /// <summary>
    /// Both sides name the same five ERP categories and both count purchase-order lines, so a reader
    /// is comparing classification and never two different units. The app's part type reaches the ERP
    /// name through the one shared translation; an ERP category with no app counterpart simply
    /// carries a zero app share instead of being merged or dropped.
    /// </summary>
    private void BuildCategoryMix(Model_Tool_ReceivingAnalyticsAppComparison comparison)
    {
        var appTotals = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var category in Translation_ReceivingAnalyticsCategory.ErpCategories)
        {
            appTotals[category] = comparison.AppCategoryTotals.TryGetValue(category, out var lines)
                ? lines
                : 0m;
        }

        var erpTotals = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var category in Translation_ReceivingAnalyticsCategory.ErpCategories)
        {
            erpTotals[category] = comparison.ErpCategoryTotals.TryGetValue(category, out var lines)
                ? lines
                : 0m;
        }

        MixAppSeries =
        [
            new ColumnSeries<double>
            {
                Name = AppMixLabel,
                Values = SharesOf(appTotals),
                Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
            },
        ];
        MixErpSeries =
        [
            new ColumnSeries<double>
            {
                Name = "ERP receipts",
                Values = SharesOf(erpTotals),
                Fill = new SolidColorPaint(SKColors.SteelBlue),
            },
        ];
        MixXAxes =
        [
            new Axis
            {
                Labels = Translation_ReceivingAnalyticsCategory.ErpCategories.ToList(),
                LabelsRotation = -30,
                TextSize = 10,
            },
        ];
        MixYAxes =
        [
            new Axis
            {
                Labeler = value => $"{value:N0}%",
                TextSize = 11,
            },
        ];
    }

    /// <summary>
    /// Each side's share of its own total, over the five ERP categories in display order. A side
    /// with nothing in range shares nothing, so the axis still names five categories at zero.
    /// </summary>
    private static List<double> SharesOf(IReadOnlyDictionary<string, decimal> totals)
    {
        var total = 0m;
        foreach (var category in Translation_ReceivingAnalyticsCategory.ErpCategories)
        {
            total += totals.GetValueOrDefault(category);
        }

        var values = new List<double>(Translation_ReceivingAnalyticsCategory.ErpCategories.Count);
        foreach (var category in Translation_ReceivingAnalyticsCategory.ErpCategories)
        {
            var value = totals.GetValueOrDefault(category);
            values.Add(total <= 0m ? 0d : (double)(100m * value / total));
        }

        return values;
    }

    private void BuildLoadsAndLabels(List<Bucket> buckets, List<string> bucketLabels)
    {
        // The load and label counts are day totals repeated on every category and unit row of
        // that day, so each day is read once and the days are then added into the bucket.
        var perDay = new Dictionary<DateTime, (int Loads, int Labels)>();
        foreach (var day in _lastData!.ActivityRows.GroupBy(row => row.BucketDate.Date))
        {
            perDay[day.Key] = (day.Max(row => row.LoadCount), day.Max(row => row.LabelCount));
        }

        var loads = new double[buckets.Count];
        var labels = new double[buckets.Count];
        for (var index = 0; index < buckets.Count; index++)
        {
            for (var day = buckets[index].Start; day < buckets[index].EndExclusive; day = day.AddDays(1))
            {
                if (!perDay.TryGetValue(day, out var totals))
                {
                    continue;
                }

                loads[index] += totals.Loads;
                labels[index] += totals.Labels;
            }
        }

        LoadsLabelsSeries =
        [
            new LineSeries<double>
            {
                Name = "Loads",
                Values = loads,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                Fill = null,
            },
            new LineSeries<double>
            {
                Name = "Labels",
                Values = labels,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(SKColors.MediumSeaGreen) { StrokeThickness = 2 },
                Fill = null,
            },
        ];

        LoadsLabelsXAxes = [BuildCategoryAxis(bucketLabels)];
        LoadsLabelsYAxes = [BuildCountAxis()];
    }

    private void BuildQuantityStack(List<Bucket> buckets, List<string> bucketLabels)
    {
        var series = new List<ISeries>();

        if (string.IsNullOrWhiteSpace(SelectedUnit))
        {
            // Two units tie, so no single unit is totalled. One line per unit keeps them apart.
            foreach (
                var unitRows in _lastData!
                    .ActivityRows.GroupBy(row => NormalizeUnit(row.UnitOfMeasure))
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
            )
            {
                var totals = new double[buckets.Count];
                foreach (var row in unitRows)
                {
                    var index = BucketIndexOf(buckets, row.BucketDate);
                    if (index >= 0)
                    {
                        totals[index] += (double)row.QuantityTotal;
                    }
                }

                series.Add(
                    new LineSeries<double>
                    {
                        Name = $"Quantity ({unitRows.Key})",
                        Values = totals,
                        GeometrySize = 6,
                        Stroke = new SolidColorPaint(SKColors.DarkOrange) { StrokeThickness = 2 },
                        Fill = null,
                    }
                );
            }
        }
        else
        {
            var unit = NormalizeUnit(SelectedUnit);
            foreach (var category in Translation_ReceivingAnalyticsCategory.AppCategories)
            {
                var totals = new double[buckets.Count];
                var hasValues = false;

                foreach (var row in _lastData!.ActivityRows)
                {
                    if (
                        !string.Equals(NormalizeUnit(row.UnitOfMeasure), unit, StringComparison.Ordinal)
                        || !string.Equals(
                            Translation_ReceivingAnalyticsCategory.FromPartType(row.PartType),
                            category,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        continue;
                    }

                    var index = BucketIndexOf(buckets, row.BucketDate);
                    if (index < 0)
                    {
                        continue;
                    }

                    totals[index] += (double)row.QuantityTotal;
                    hasValues = true;
                }

                if (!hasValues)
                {
                    continue;
                }

                series.Add(
                    new StackedColumnSeries<double>
                    {
                        Name = category,
                        Values = totals,
                        Fill = new SolidColorPaint(CategoryColor(category)),
                    }
                );
            }
        }

        QuantitySeries = series;
        QuantityXAxes = [BuildCategoryAxis(bucketLabels)];
        QuantityYAxes = [BuildCountAxis()];
    }

    private void ClearPanels()
    {
        IsEmpty = true;
        IsErpUnavailable = true;
        MissingLoadNumberText = string.Empty;
        OnTimePercentage = null;
        OnTimePercentText = string.Empty;
        NoDueDateText = string.Empty;
        IsOnTimeBelowTarget = false;
        SupplierRankings = [];
        SupplierLeadShareText = string.Empty;
        UnitOptions = [];
        ClearSeries();
    }

    private void ClearSeries()
    {
        LoadsLabelsSeries = [];
        LoadsLabelsXAxes = [];
        LoadsLabelsYAxes = [];
        QuantitySeries = [];
        QuantityXAxes = [];
        QuantityYAxes = [];
        OnTimeSeries = [];
        OnTimeXAxes = [];
        OnTimeYAxes = [];
        SupplierSeries = [];
        SupplierXAxes = [];
        SupplierYAxes = [];
        ClearComparisonSeries();
    }

    private void ClearComparisonSeries()
    {
        CoverageSeries = [];
        CoverageXAxes = [];
        CoverageYAxes = [];
        MixAppSeries = [];
        MixErpSeries = [];
        MixXAxes = [];
        MixYAxes = [];
    }

    private static string BuildMissingLoadNumberText(int count) =>
        count switch
        {
            0 => "Every completed receipt in this range has a load number.",
            1 =>
                "1 completed receipt in this range has no load number, so its label number stands in as the load.",
            _ =>
                $"{count:N0} completed receipts in this range have no load number, so their label numbers stand in as the load.",
        };

    /// <summary>A blank stored unit reads as EA, so the database and the screen agree.</summary>
    private static string NormalizeUnit(string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? "EA" : unit.Trim().ToUpperInvariant();

    /// <summary>A blank supplier reads as Unknown, so its work stays in the ranking.</summary>
    private static string NormalizeVendor(string? vendor) =>
        string.IsNullOrWhiteSpace(vendor) ? "Unknown" : vendor.Trim();

    private static SKColor CategoryColor(string appCategory) =>
        appCategory switch
        {
            Translation_ReceivingAnalyticsCategory.AppCoil => SKColors.MediumSeaGreen,
            Translation_ReceivingAnalyticsCategory.AppSheet => SKColors.MediumPurple,
            _ => SKColors.DodgerBlue,
        };

    private static Axis BuildCategoryAxis(List<string> labels) =>
        new()
        {
            Labels = labels,
            LabelsRotation = 0,
            TextSize = 12,
            MinStep = 1,
            UnitWidth = 1,
        };

    private static Axis BuildCountAxis() =>
        new()
        {
            Labeler = value => value.ToString("N0"),
            TextSize = 12,
        };

    /// <summary>
    /// The continuous bucket axis for the range: every day, Monday-to-Sunday week or calendar
    /// month is present, including the ones with no completed receipts.
    /// </summary>
    /// <summary>A Saturday or Sunday. The panels do not read weekend work.</summary>
    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static List<Bucket> BuildBuckets(
        DateTime start,
        DateTime endExclusive,
        Enum_ReceivingAnalyticsPeriod period
    )
    {
        var buckets = new List<Bucket>();
        var rangeStart = start.Date;
        if (endExclusive <= rangeStart)
        {
            return buckets;
        }

        switch (period)
        {
            case Enum_ReceivingAnalyticsPeriod.Week:
                var weekStart = rangeStart;
                while (weekStart.DayOfWeek != DayOfWeek.Monday)
                {
                    weekStart = weekStart.AddDays(-1);
                }

                while (weekStart < endExclusive)
                {
                    buckets.Add(
                        new Bucket(weekStart, weekStart.AddDays(7), $"Week of {weekStart:MM/dd}")
                    );
                    weekStart = weekStart.AddDays(7);
                }

                break;

            case Enum_ReceivingAnalyticsPeriod.Month:
                var monthStart = new DateTime(rangeStart.Year, rangeStart.Month, 1);
                while (monthStart < endExclusive)
                {
                    buckets.Add(
                        new Bucket(monthStart, monthStart.AddMonths(1), monthStart.ToString("MMM yyyy"))
                    );
                    monthStart = monthStart.AddMonths(1);
                }

                break;

            default:
                for (var day = rangeStart; day < endExclusive; day = day.AddDays(1))
                {
                    if (IsWeekend(day))
                    {
                        continue;
                    }

                    buckets.Add(new Bucket(day, day.AddDays(1), day.ToString("MM/dd")));
                }

                break;
        }

        return buckets;
    }

    private static int BucketIndexOf(List<Bucket> buckets, DateTime date)
    {
        var day = date.Date;
        for (var index = 0; index < buckets.Count; index++)
        {
            if (day >= buckets[index].Start && day < buckets[index].EndExclusive)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>One axis bucket: its start, its exclusive end, and the label the axis shows.</summary>
    private sealed record Bucket(DateTime Start, DateTime EndExclusive, string Label)
    {
        public int Early { get; set; }

        public int OnTime { get; set; }

        public int Late { get; set; }

        public int NoDueDate { get; set; }
    }

    /// <summary>One row of the supplier ranking: a supplier, or the grouped remainder.</summary>
    public sealed record SupplierRankingEntry(
        string VendorName,
        decimal Measure,
        decimal CumulativeShare,
        bool IsRemainder
    );

    /// <summary>One supplier's totals across every unit row of the loaded range.</summary>
    private sealed record SupplierTotal(string Name, int Labels, int Loads, decimal Quantity);
}

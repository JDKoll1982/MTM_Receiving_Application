using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Receiving Analytics Record Days tab: the busiest days the app itself recorded,
/// read from MySQL and ranked by loads for each item type (Coil, Sheet/Flat Stock, Other/Standard).
/// </summary>
public partial class ViewModel_Tool_ReceivingAnalyticsTopDays : ViewModel_Tool_Base
{
    /// <summary>Days kept per item type when the requested count is not usable.</summary>
    public const int DefaultTopCount = 10;

    private readonly IService_Tool_ReceivingAnalyticsApp _service;

    private bool _hasLoaded;

    [ObservableProperty]
    private DateTimeOffset _fromDate = new(DateTime.Today.AddDays(-89));

    [ObservableProperty]
    private DateTimeOffset _toDate = new(DateTime.Today);

    /// <summary>0=5, 1=10, 2=20.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TopCount))]
    private int _selectedTopCountIndex = 1;

    /// <summary>How many days each item-type list keeps.</summary>
    public IReadOnlyList<int> TopCountOptions { get; } = [5, 10, 20];

    public int TopCount =>
        SelectedTopCountIndex >= 0 && SelectedTopCountIndex < TopCountOptions.Count
            ? TopCountOptions[SelectedTopCountIndex]
            : DefaultTopCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCoilDays))]
    private ObservableCollection<Model_Tool_ReceivingAnalyticsAppTopDayRow> _coilDays = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFlatStockDays))]
    private ObservableCollection<Model_Tool_ReceivingAnalyticsAppTopDayRow> _flatStockDays = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOtherDays))]
    private ObservableCollection<Model_Tool_ReceivingAnalyticsAppTopDayRow> _otherDays = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBusiestDays))]
    private ObservableCollection<Model_Tool_ReceivingAnalyticsAppBusiestDayRow> _busiestDays = [];

    public bool HasCoilDays => CoilDays.Count > 0;

    public bool HasFlatStockDays => FlatStockDays.Count > 0;

    public bool HasOtherDays => OtherDays.Count > 0;

    public bool HasBusiestDays => BusiestDays.Count > 0;

    public string CoilEmptyStateText => "No coil days in this range.";

    public string FlatStockEmptyStateText => "No flat stock days in this range.";

    public string OtherEmptyStateText => "No other item days in this range.";

    public string BusiestDaysEmptyStateText => "No receiving days in this range.";

    public ViewModel_Tool_ReceivingAnalyticsTopDays(
        IService_Tool_ReceivingAnalyticsApp service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        ToolTitle = "Receiving Analytics";
        ToolDescription = "Busiest receiving days by item type, counted from the receiving database.";
    }

    /// <summary>Loads the panel the first time the tool becomes active.</summary>
    public void ActivateView()
    {
        if (_hasLoaded)
        {
            return;
        }

        _ = LoadAsync();
    }

    partial void OnSelectedTopCountIndexChanged(int value)
    {
        if (_hasLoaded)
        {
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ShowStatus("Loading record days from the receiving database...", InfoBarSeverity.Informational);

            var filter = BuildFilter();
            var topDaysTask = _service.GetTopDaysByItemTypeAsync(filter, TopCount);
            var busiestDaysTask = _service.GetBusiestDaysAsync(filter);

            await Task.WhenAll(topDaysTask, busiestDaysTask);

            var topDaysResult = await topDaysTask;
            if (!topDaysResult.IsSuccess)
            {
                _hasLoaded = false;
                ResetRows();
                ShowStatus(topDaysResult.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            var rows = topDaysResult.Data ?? [];
            CoilDays = BuildRanking(rows, Translation_ReceivingAnalyticsCategory.AppCoil);
            FlatStockDays = BuildRanking(rows, Translation_ReceivingAnalyticsCategory.AppSheet);
            OtherDays = BuildRanking(rows, Translation_ReceivingAnalyticsCategory.AppStandard);

            var busiestResult = await busiestDaysTask;
            BusiestDays = busiestResult.IsSuccess
                ? BuildBusiestRanking(busiestResult.Data ?? [])
                : [];

            _hasLoaded = true;

            if (!busiestResult.IsSuccess)
            {
                ShowStatus(busiestResult.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            var hasAny = CoilDays.Count + FlatStockDays.Count + OtherDays.Count > 0;
            ShowStatus(
                hasAny
                    ? $"Top {TopCount} day(s) per item type, {FromDate.Date:MM/dd/yyyy} - {ToDate.Date:MM/dd/yyyy}."
                    : "No receiving days match the selected range.",
                hasAny ? InfoBarSeverity.Success : InfoBarSeverity.Warning
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Tool_ReceivingAnalyticsTopDays)
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
        // Drop the loaded flag first so resetting the top-count index cannot kick off a reload.
        _hasLoaded = false;
        FromDate = new DateTimeOffset(DateTime.Today.AddDays(-89));
        ToDate = new DateTimeOffset(DateTime.Today);
        SelectedTopCountIndex = 1;
        ResetRows();
        ShowStatus("Filters cleared.", InfoBarSeverity.Informational);
    }

    /// <summary>
    /// Keeps the range as typed: the view model owns the inclusive From/To semantics and the
    /// service takes the end as exclusive, so the last day the user picked is included.
    /// </summary>
    private Model_Tool_ReceivingAnalyticsAppFilter BuildFilter()
    {
        var start = FromDate.Date;
        var end = ToDate.Date;
        if (end < start)
        {
            (start, end) = (end, start);
        }

        return new Model_Tool_ReceivingAnalyticsAppFilter
        {
            StartDate = start,
            EndDate = end.AddDays(1),
        };
    }

    /// <summary>
    /// Ranks the rows for one item type by loads, newest first on a tie, and keeps the top count.
    /// </summary>
    private ObservableCollection<Model_Tool_ReceivingAnalyticsAppTopDayRow> BuildRanking(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsAppTopDayRow> rows,
        string itemType
    )
    {
        var ranked = rows
            .Where(row => string.Equals(row.ItemType, itemType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.LoadCount)
            .ThenByDescending(row => row.DayDate)
            .Take(TopCount)
            .ToList();

        for (var index = 0; index < ranked.Count; index++)
        {
            ranked[index].Rank = index + 1;
        }

        return new ObservableCollection<Model_Tool_ReceivingAnalyticsAppTopDayRow>(ranked);
    }

    /// <summary>
    /// Ranks the day totals across every item type by loads, newest first on a tie, and keeps the
    /// top count. The counts are the day's own totals, so a load carrying two item types is not
    /// counted twice.
    /// </summary>
    private ObservableCollection<Model_Tool_ReceivingAnalyticsAppBusiestDayRow> BuildBusiestRanking(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsAppBusiestDayRow> rows
    )
    {
        var ranked = rows
            .OrderByDescending(row => row.LoadCount)
            .ThenByDescending(row => row.DayDate)
            .Take(TopCount)
            .ToList();

        for (var index = 0; index < ranked.Count; index++)
        {
            ranked[index].Rank = index + 1;
        }

        return new ObservableCollection<Model_Tool_ReceivingAnalyticsAppBusiestDayRow>(ranked);
    }

    private void ResetRows()
    {
        CoilDays = [];
        FlatStockDays = [];
        OtherDays = [];
        BusiestDays = [];
    }
}

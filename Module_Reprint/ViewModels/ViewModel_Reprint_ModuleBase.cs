using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Reprint.Models;
using MTM_Receiving_Application.Module_Reprint.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Reprint.ViewModels;

/// <summary>
/// Shared behavior for the three Reprint Labels sub-pages (Receiving / Dunnage / Volvo):
/// date-range filters with presets, Search By + text, a checkbox history grid with red
/// already-queued rows, and a bottom footer (Select All / Reprint Selected / Back).
/// The per-module contract (history query + re-queue) is supplied by derived view models.
/// </summary>
public abstract partial class ViewModel_Reprint_ModuleBase : ViewModel_Shared_Base
{
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private bool _isUpdatingSelectAll;
    private bool _suppressFilterReload;

    protected abstract string SearchBySettingsKey { get; }

    protected abstract string ModuleDisplayName { get; }

    protected abstract string ColumnOptionsSettingsKey { get; }

    protected abstract IReadOnlyList<Model_ReprintSearchByOption> BuildSearchByOptions();

    protected abstract Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> LoadHistoryAsync(
        Model_ReprintHistoryFilter filter
    );

    protected abstract Task<Model_Dao_Result<Model_ReprintBatchResult>> ExecuteReprintAsync(
        IReadOnlyList<string> historyIds
    );

    public event EventHandler? BackRequested;

    public event EventHandler<Model_ReprintBatchResult>? ReprintCompleted;

    [ObservableProperty]
    private DateTimeOffset? _startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset? _endDate = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _datePresetsExpanded;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Model_ReprintSearchByOption? _selectedSearchBy;

    [ObservableProperty]
    private ObservableCollection<Model_ReprintSelectableRow> _historyRows = [];

    [ObservableProperty]
    private bool _hasRows;

    [ObservableProperty]
    private bool? _isSelectAllChecked;

    [ObservableProperty]
    private int _selectedCount;

    public ObservableCollection<Model_ReprintSearchByOption> SearchByOptions { get; } = [];

    public ObservableCollection<Model_ReprintColumnOption> ColumnOptions { get; } = [];

    public string DatePresetsToggleGlyph => DatePresetsExpanded ? "\uE76B" : "\uE76C";

    public bool CanReprint => SelectedCount > 0 && !IsBusy;

    public bool IsSelectAllEnabled => HistoryRows.Any(row => row.CanSelect);

    public bool HasAlreadyQueuedRows => HistoryRows.Any(row => row.AlreadyQueued);

    public string SelectAllLabel => IsSelectAllChecked == true ? "Select None" : "Select All";

    public string ReprintButtonLabel =>
        SelectedCount > 0 ? $"Reprint Selected ({SelectedCount})" : "Reprint Selected";

    protected ViewModel_Reprint_ModuleBase(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// Called by the hosting page when it loads: builds the Search By options, restores the
    /// persisted Search By choice, and loads the initial (Today) history.
    /// </summary>
    public virtual async Task ActivateAsync()
    {
        foreach (var option in BuildSearchByOptions())
        {
            SearchByOptions.Add(option);
        }

        var persistedSearchBy = await GetPersistedSearchByAsync();
        SelectedSearchBy =
            SearchByOptions.FirstOrDefault(option => option.Key == persistedSearchBy)
            ?? SearchByOptions.FirstOrDefault();

        BuildColumnOptions();
        await LoadColumnVisibilityAsync();

        _suppressFilterReload = true;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        _suppressFilterReload = false;
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            DetachRowHandlers();
            var filter = new Model_ReprintHistoryFilter
            {
                StartDate = StartDate?.Date,
                EndDate = EndDate?.Date,
                SearchBy = SelectedSearchBy?.Key ?? "part",
                SearchText = SearchText,
            };

            var result = await LoadHistoryAsync(filter);
            if (!result.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    result.ErrorMessage ?? $"Failed to load {ModuleDisplayName} history.",
                    Enum_ErrorSeverity.Warning
                );
                HistoryRows = [];
                HasRows = false;
                UpdateSelectionState();
                return;
            }

            var sourceRows = result.Data ?? [];
            var rows = sourceRows.ConvertAll(row => new Model_ReprintSelectableRow(row));
            foreach (var row in rows)
            {
                row.PropertyChanged += Row_PropertyChanged;
            }

            HistoryRows = new ObservableCollection<Model_ReprintSelectableRow>(rows);
            HasRows = HistoryRows.Count > 0;
            ApplyColumnVisibilityToRows();
            UpdateSelectionState();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Clears the date range so the full history is searched.</summary>
    [RelayCommand]
    private async Task LoadAllAsync()
    {
        _suppressFilterReload = true;
        StartDate = null;
        EndDate = null;
        _suppressFilterReload = false;
        await ReloadAsync();
    }

    [RelayCommand]
    private Task LoadTodayAsync() => ApplyDateRangeAsync(DateTime.Today, DateTime.Today);

    [RelayCommand]
    private void ToggleDatePresets() => DatePresetsExpanded = !DatePresetsExpanded;

    [RelayCommand]
    private Task LoadYesterdayAsync() =>
        ApplyDateRangeAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));

    [RelayCommand]
    private Task LoadThisWeekAsync()
    {
        var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        return ApplyDateRangeAsync(start, DateTime.Today);
    }

    [RelayCommand]
    private Task LoadThisMonthAsync()
    {
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        return ApplyDateRangeAsync(start, DateTime.Today);
    }

    [RelayCommand]
    private Task LoadThisQuarterAsync()
    {
        var quarterStartMonth = ((DateTime.Today.Month - 1) / 3) * 3 + 1;
        var start = new DateTime(DateTime.Today.Year, quarterStartMonth, 1);
        return ApplyDateRangeAsync(start, DateTime.Today);
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        _suppressFilterReload = true;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        SearchText = string.Empty;
        _suppressFilterReload = false;
        await ReloadAsync();
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(CanReprint))]
    private async Task ReprintAsync()
    {
        var selectedIds = HistoryRows
            .Where(row => row.IsSelected && row.CanSelect)
            .Select(row => row.HistoryId)
            .ToList();

        if (selectedIds.Count == 0)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await ExecuteReprintAsync(selectedIds);
            if (!result.IsSuccess || result.Data is null)
            {
                await _errorHandler.HandleErrorAsync(
                    result.ErrorMessage ?? "Reprint failed.",
                    Enum_ErrorSeverity.Warning
                );
                return;
            }

            ReprintCompleted?.Invoke(this, result.Data);
        }
        finally
        {
            IsBusy = false;
        }

        await ReloadAsync();
    }

    private async Task ApplyDateRangeAsync(DateTime start, DateTime end)
    {
        _suppressFilterReload = true;
        StartDate = start;
        EndDate = end;
        _suppressFilterReload = false;
        await ReloadAsync();
    }

    private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model_ReprintSelectableRow.IsSelected))
        {
            UpdateSelectionState();
        }
    }

    private void DetachRowHandlers()
    {
        foreach (var row in HistoryRows)
        {
            row.PropertyChanged -= Row_PropertyChanged;
        }
    }

    private void UpdateSelectionState()
    {
        _isUpdatingSelectAll = true;
        try
        {
            var selectable = HistoryRows.Where(row => row.CanSelect).ToList();
            var selected = selectable.Count(row => row.IsSelected);

            if (selectable.Count == 0 || selected == 0)
            {
                IsSelectAllChecked = false;
            }
            else if (selected == selectable.Count)
            {
                IsSelectAllChecked = true;
            }
            else
            {
                IsSelectAllChecked = null;
            }

            SelectedCount = selected;
        }
        finally
        {
        OnPropertyChanged(nameof(SelectAllLabel));
        OnPropertyChanged(nameof(ReprintButtonLabel));
            _isUpdatingSelectAll = false;
        }

        OnPropertyChanged(nameof(CanReprint));
        OnPropertyChanged(nameof(IsSelectAllEnabled));
        ReprintCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSelectAllCheckedChanged(bool? value)
    {
        if (_isUpdatingSelectAll)
        {
            return;
        }

        foreach (var row in HistoryRows.Where(row => row.CanSelect))
        {
            row.IsSelected = value == true;
        }

        UpdateSelectionState();
    }

    partial void OnDatePresetsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(DatePresetsToggleGlyph));
    }

    partial void OnStartDateChanged(DateTimeOffset? value)
    {
        if (!_suppressFilterReload)
        {
            _ = ReloadAsync();
        }
    }

    partial void OnEndDateChanged(DateTimeOffset? value)
    {
        if (!_suppressFilterReload)
        {
            _ = ReloadAsync();
        }
    }

    partial void OnSelectedSearchByChanged(Model_ReprintSearchByOption? value)
    {
        if (value is null || SearchByOptions.Count == 0)
        {
            return;
        }

        _ = ReloadAsync();
        _ = PersistSearchByAsync();
    }

    public event EventHandler? ShowColumnChooserRequested;

    [RelayCommand]
    private void ShowColumnChooser() => ShowColumnChooserRequested?.Invoke(this, EventArgs.Empty);

    private void BuildColumnOptions()
    {
        ColumnOptions.Clear();
        ColumnOptions.Add(
            new Model_ReprintColumnOption { Key = "Date", Header = "Date", IsVisible = true }
        );
        ColumnOptions.Add(
            new Model_ReprintColumnOption { Key = "Part", Header = "Part", IsVisible = true }
        );
        ColumnOptions.Add(
            new Model_ReprintColumnOption { Key = "Qty", Header = "Qty", IsVisible = true }
        );
        ColumnOptions.Add(
            new Model_ReprintColumnOption
            {
                Key = "Reference",
                Header = "Reference",
                IsVisible = true,
            }
        );
    }

    private async Task LoadColumnVisibilityAsync()
    {
        var userId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        var result = await _settingsCore.GetSettingAsync(
            ReprintSettingsKeys.Category,
            ColumnOptionsSettingsKey,
            userId
        );

        if (!result.IsSuccess)
        {
            return;
        }

        var stored = result.Data?.Value ?? string.Empty;
        var visibleKeys = stored
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (visibleKeys.Count == 0)
        {
            return;
        }

        foreach (var option in ColumnOptions)
        {
            option.IsVisible = visibleKeys.Contains(option.Key);
        }
    }

    private async Task SaveColumnVisibilityAsync()
    {
        var userId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        var value = string.Join(
            ",",
            ColumnOptions.Where(option => option.IsVisible).Select(option => option.Key)
        );
        await _settingsCore.SetSettingAsync(
            ReprintSettingsKeys.Category,
            ColumnOptionsSettingsKey,
            value,
            userId
        );
    }

    /// <summary>Applies the current <see cref="ColumnOptions"/> to the grid rows.</summary>
    public void ApplyColumnVisibilityToRows()
    {
        bool IsVisible(string key) =>
            ColumnOptions.FirstOrDefault(option => option.Key == key)?.IsVisible ?? true;

        foreach (var row in HistoryRows)
        {
            row.ShowDateColumn = IsVisible("Date");
            row.ShowPartColumn = IsVisible("Part");
            row.ShowQtyColumn = IsVisible("Qty");
            row.ShowReferenceColumn = IsVisible("Reference");
        }
    }

    /// <summary>
    /// Called after the column chooser is accepted: mirrors visibility to the grid rows and
    /// persists the choice per module.
    /// </summary>
    public async Task ApplyAndPersistColumnVisibilityAsync()
    {
        ApplyColumnVisibilityToRows();
        await SaveColumnVisibilityAsync();
    }

    private async Task<string> GetPersistedSearchByAsync()
    {
        var userId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        var result = await _settingsCore.GetSettingAsync(
            ReprintSettingsKeys.Category,
            SearchBySettingsKey,
            userId
        );
        return result.IsSuccess ? result.Data?.Value ?? string.Empty : string.Empty;
    }

    private async Task PersistSearchByAsync()
    {
        if (SelectedSearchBy is null)
        {
            return;
        }

        var userId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        await _settingsCore.SetSettingAsync(
            ReprintSettingsKeys.Category,
            SearchBySettingsKey,
            SelectedSearchBy.Key,
            userId
        );
    }
}

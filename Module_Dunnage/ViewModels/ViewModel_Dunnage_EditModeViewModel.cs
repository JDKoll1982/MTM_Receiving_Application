using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Edit Mode (historical data editing)
/// </summary>
public partial class ViewModel_Dunnage_EditMode : ViewModel_Shared_Base
{
    private const string PartSelectionPlaceholderText = "Select Part ID";

    private enum EditModeLoadSource
    {
        None,
        CurrentMemory,
        CurrentLabels,
        History,
    }

    private sealed record Model_DunnageLoadSnapshot(
        string PartId,
        int? TypeId,
        string TypeName,
        string TypeIcon,
        decimal Quantity,
        string PoNumber,
        string? Location,
        string? LabelNumber
    );

    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Pagination _paginationService;
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Window _windowService;
    private readonly IService_Help _helpService;
    private readonly IService_InforVisual _inforVisualService;

    private const int PAGE_SIZE = 50;

    public ViewModel_Dunnage_EditMode(
        IService_MySQL_Dunnage dunnageService,
        IService_Pagination paginationService,
        IService_DunnageWorkflow workflowService,
        IService_Window windowService,
        IService_InforVisual inforVisualService,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageService = dunnageService;
        _paginationService = paginationService;
        _workflowService = workflowService;
        _windowService = windowService;
        _inforVisualService = inforVisualService;
        _helpService = helpService;

        _workflowService.LabelDataCleared += OnLabelDataCleared;

        // T166: Set page size to 50
        _paginationService.PageSize = PAGE_SIZE;

        // T167: Subscribe to PageChanged event
        _paginationService.PageChanged += OnPageChanged;

        // Set default date range (last 7 days)
        ToDate = DateTimeOffset.Now;
        FromDate = DateTimeOffset.Now.AddDays(-7);
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _filteredLoads = new();

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _selectedLoads = new();

    [ObservableProperty]
    private Model_DunnageLoad? _focusedLoad;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _fromDate;

    [ObservableProperty]
    private DateTimeOffset? _toDate;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalRecords = 0;

    [ObservableProperty]
    private bool _canSave = false;

    [ObservableProperty]
    private bool _canNavigate = false;

    private List<Model_DunnageLoad> _allLoads = new();
    private readonly Dictionary<Guid, Model_DunnageLoadSnapshot> _originalLoadSnapshots = new();
    private readonly List<Model_DunnageLoad> _removedLoads = new();
    private EditModeLoadSource _currentLoadSource = EditModeLoadSource.None;

    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchText));
        ApplySearchFilter(1);
    }

    // T164: Dynamic button text for date filters
    public string LastWeekButtonText => "Last Week";

    public string TodayButtonText => "Today";

    public string ThisWeekButtonText
    {
        get
        {
            var today = DateTime.Now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (startOfWeek > today)
            {
                startOfWeek = startOfWeek.AddDays(-7);
            }

            return "This Week";
        }
    }

    public string ThisMonthButtonText => "This Month";

    public string ThisQuarterButtonText
    {
        get
        {
            var today = DateTime.Now.Date;
            var quarter = (today.Month - 1) / 3 + 1;
            var startMonth = (quarter - 1) * 3 + 1;
            var quarterStart = new DateTime(today.Year, startMonth, 1);
            return "This Quarter";
        }
    }

    #endregion

    #region Load Data Commands

    /// <summary>
    /// T151: Load data from current workflow session (unsaved loads in memory)
    /// </summary>
    [RelayCommand]
    private async Task LoadFromCurrentMemoryAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading session data...";

            if (
                _workflowService.CurrentSession == null
                || _workflowService.CurrentSession.Loads.Count == 0
            )
            {
                // T156: Info message for empty session
                await _errorHandler.HandleErrorAsync(
                    "No unsaved loads in session",
                    Enum_ErrorSeverity.Info,
                    null,
                    true
                );
                _allLoads = new List<Model_DunnageLoad>();
                StatusMessage = "No data in session";
                _logger.LogInfo("No unsaved loads found in current session", "EditMode");
                return;
            }

            _allLoads = _workflowService.CurrentSession.Loads.ToList();
            _currentLoadSource = EditModeLoadSource.CurrentMemory;
            EnsureDisplayLoadNumbers();
            CaptureOriginalSnapshots();
            ApplySearchFilter(1);
            StatusMessage = $"Loaded {TotalRecords} loads from session";

            _logger.LogInfo($"Loaded {TotalRecords} loads from current session", "EditMode");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading session data",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Load current label queue from the active <c>dunnage_label_data</c> database table.
    /// </summary>
    [RelayCommand]
    private async Task LoadFromCurrentLabelsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading active label data...";

            var result = await _dunnageService.GetActiveLabelDataAsync();

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "LoadFromCurrentLabelsAsync", true);
                return;
            }

            _allLoads = result.Data ?? new List<Model_DunnageLoad>();
            _currentLoadSource = EditModeLoadSource.CurrentLabels;
            EnsureDisplayLoadNumbers();
            CaptureOriginalSnapshots();
            ApplySearchFilter(1);
            StatusMessage = $"Loaded {TotalRecords} active label(s)";

            _logger.LogInfo(
                $"Loaded {TotalRecords} active labels from dunnage_label_data queue",
                "EditMode"
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading active label data",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadFromHistoryAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading historical data...";

            var startDate = FromDate?.DateTime ?? DateTime.Now.AddDays(-7);
            var endDate = ToDate?.DateTime ?? DateTime.Now;

            var result = await _dunnageService.GetLoadsByDateRangeAsync(startDate, endDate);

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "LoadFromHistoryAsync", true);
                return;
            }

            _allLoads = result.Data ?? new List<Model_DunnageLoad>();
            _currentLoadSource = EditModeLoadSource.History;
            EnsureDisplayLoadNumbers();
            CaptureOriginalSnapshots();
            ApplySearchFilter(1);
            StatusMessage = $"Loaded {TotalRecords} records";

            _logger.LogInfo(
                $"Loaded {TotalRecords} historical loads from {startDate:d} to {endDate:d}",
                "EditMode"
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading historical data",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Shows contextual help for edit mode.
    /// </summary>
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.EditMode");
    }

    /// <summary>
    /// T158: Set filter to last 7 days (today - 7 days to today)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterLastWeekAsync()
    {
        if (IsBusy)
            return;
        ToDate = DateTime.Now.Date;
        FromDate = DateTime.Now.Date.AddDays(-7);
        await LoadFromHistoryAsync();
    }

    /// <summary>
    /// T159: Set filter to today only (today 00:00 to today 23:59)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterTodayAsync()
    {
        if (IsBusy)
            return;
        FromDate = DateTime.Now.Date;
        ToDate = DateTime.Now.Date;
        await LoadFromHistoryAsync();
    }

    /// <summary>
    /// T160: Set filter to this week (Monday to Sunday of current week)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterThisWeekAsync()
    {
        if (IsBusy)
            return;
        var today = DateTime.Now.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (startOfWeek > today) // If today is Sunday
        {
            startOfWeek = startOfWeek.AddDays(-7);
        }
        FromDate = startOfWeek;
        ToDate = today;
        await LoadFromHistoryAsync();
    }

    /// <summary>
    /// T161: Set filter to this month (first to last day of current month)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterThisMonthAsync()
    {
        if (IsBusy)
            return;
        var today = DateTime.Now.Date;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = today;
        await LoadFromHistoryAsync();
    }

    /// <summary>
    /// T162: Set filter to this quarter (first to last day of current quarter)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterThisQuarterAsync()
    {
        if (IsBusy)
            return;
        var today = DateTime.Now.Date;
        var quarter = (today.Month - 1) / 3 + 1;
        var startMonth = (quarter - 1) * 3 + 1;
        FromDate = new DateTime(today.Year, startMonth, 1);
        ToDate = today;
        await LoadFromHistoryAsync();
    }

    /// <summary>
    /// T163: Clear date filters (show last year of data)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterShowAllAsync()
    {
        if (IsBusy)
            return;
        FromDate = DateTime.Now.Date.AddYears(-1);
        ToDate = DateTime.Now.Date;
        await LoadFromHistoryAsync();
    }

    #endregion

    #region Pagination Commands

    [RelayCommand]
    private void FirstPage()
    {
        if (CurrentPage == 1)
        {
            return;
        }

        LoadPage(1);
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        LoadPage(CurrentPage - 1);
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage >= TotalPages)
        {
            return;
        }

        LoadPage(CurrentPage + 1);
    }

    [RelayCommand]
    private void LastPage()
    {
        if (CurrentPage == TotalPages)
        {
            return;
        }

        LoadPage(TotalPages);
    }

    private void LoadPage(int pageNumber)
    {
        _paginationService.GoToPage(pageNumber);
        CurrentPage = _paginationService.CurrentPage;

        var pageLoads = _paginationService.GetCurrentPageItems<Model_DunnageLoad>();

        ReplaceFilteredLoads(pageLoads);

        StatusMessage = $"Page {CurrentPage} of {TotalPages}";
        _logger.LogInfo($"Loaded page {CurrentPage} of {TotalPages}", "EditMode");
    }

    #endregion

    #region Edit Commands

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var load in FilteredLoads)
        {
            load.IsSelected = true;
        }

        SyncSelectedLoadsFromFlags();
        UpdateCanSave();
        _logger.LogInfo($"Selected all {FilteredLoads.Count} loads on page", "EditMode");
    }

    [RelayCommand]
    private void RemoveSelectedRows()
    {
        SyncSelectedLoadsFromFlags();

        if (SelectedLoads.Count == 0 && FocusedLoad is not null)
        {
            SelectedLoads.Add(FocusedLoad);
        }

        if (SelectedLoads.Count == 0)
        {
            return;
        }

        var loadsToRemove = SelectedLoads.ToList();

        foreach (var load in loadsToRemove)
        {
            if (_currentLoadSource != EditModeLoadSource.CurrentMemory)
            {
                _removedLoads.Add(load);
            }

            FilteredLoads.Remove(load);
            _allLoads.Remove(load);
        }

        SelectedLoads.Clear();
        FocusedLoad = null;
        EnsureDisplayLoadNumbers();
        ApplySearchFilter(CurrentPage);

        UpdateCanSave();

        _logger.LogInfo($"Removed {loadsToRemove.Count} loads", "EditMode");
    }

    [RelayCommand]
    private async Task SelectTypeAsync(Model_DunnageLoad? load)
    {
        if (load is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading dunnage types...";

            var typesResult = await _dunnageService.GetAllTypesAsync();
            if (!typesResult.IsSuccess || typesResult.Data == null)
            {
                await _errorHandler.HandleDaoErrorAsync(typesResult, nameof(SelectTypeAsync), true);
                return;
            }

            var partsResult = await _dunnageService.GetAllPartsAsync();
            if (!partsResult.IsSuccess || partsResult.Data == null)
            {
                await _errorHandler.HandleDaoErrorAsync(partsResult, nameof(SelectTypeAsync), true);
                return;
            }

            var partCountsByType = partsResult
                .Data.GroupBy(part => part.TypeId)
                .ToDictionary(group => group.Key, group => group.Count());

            var availableTypes = typesResult
                .Data.Where(type => partCountsByType.ContainsKey(type.Id))
                .OrderBy(type => type.TypeName)
                .ToList();

            if (availableTypes.Count == 0)
            {
                await _errorHandler.HandleErrorAsync(
                    "No dunnage types with available parts were found.",
                    Enum_ErrorSeverity.Info,
                    null,
                    true
                );
                return;
            }

            var options = availableTypes.ConvertAll(type => new Model_FuzzySearchResult
            {
                Key = type.Id.ToString(),
                Label = type.TypeName,
                Detail = $"{partCountsByType[type.Id]} part ID(s)",
            });

            var selection = await ShowFuzzyPickerAsync(
                options,
                "Select Dunnage Type",
                "Choose a dunnage type for this row."
            );

            if (selection is null)
            {
                return;
            }

            var selectedType = availableTypes.First(type => type.Id.ToString() == selection.Key);
            var typeChanged = load.TypeId != selectedType.Id;

            load.TypeId = selectedType.Id;
            load.TypeName = selectedType.TypeName;
            load.DunnageType = selectedType.TypeName;
            load.TypeIcon = selectedType.Icon;

            if (typeChanged)
            {
                load.PartId = string.Empty;
            }

            UpdateCanSave();
            StatusMessage = $"Selected type {selectedType.TypeName}";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error selecting dunnage type",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectPartAsync(Model_DunnageLoad? load)
    {
        if (load is null)
        {
            return;
        }

        if (!load.TypeId.HasValue || load.TypeId.Value <= 0)
        {
            await _errorHandler.HandleErrorAsync(
                "Select a dunnage type before choosing a part ID.",
                Enum_ErrorSeverity.Info,
                null,
                true
            );
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading parts...";

            var partsResult = await _dunnageService.GetPartsByTypeAsync(load.TypeId.Value);
            if (!partsResult.IsSuccess || partsResult.Data == null)
            {
                await _errorHandler.HandleDaoErrorAsync(partsResult, nameof(SelectPartAsync), true);
                return;
            }

            var availableParts = partsResult.Data.OrderBy(part => part.PartId).ToList();
            if (availableParts.Count == 0)
            {
                await _errorHandler.HandleErrorAsync(
                    "No part IDs are available for the selected dunnage type.",
                    Enum_ErrorSeverity.Info,
                    null,
                    true
                );
                return;
            }

            var options = availableParts.ConvertAll(part => new Model_FuzzySearchResult
            {
                Key = part.PartId,
                Label = part.PartId,
                Detail = string.IsNullOrWhiteSpace(part.HomeLocation)
                    ? part.DunnageTypeName
                    : $"Home: {part.HomeLocation}",
            });

            var selection = await ShowFuzzyPickerAsync(
                options,
                "Select Dunnage Part ID",
                $"Choose a part ID for type {load.TypeName}."
            );

            if (selection is null)
            {
                return;
            }

            var selectedPart = availableParts.First(part => part.PartId == selection.Key);
            load.PartId = selectedPart.PartId;

            UpdateCanSave();
            StatusMessage = $"Selected part ID {selectedPart.PartId}";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error selecting dunnage part ID",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectLocationAsync(Model_DunnageLoad? load)
    {
        if (load is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading locations...";

            var locationsResult = await _inforVisualService.FuzzySearchLocationsAsync(
                string.Empty,
                "002"
            );

            if (!locationsResult.IsSuccess || locationsResult.Data == null)
            {
                await _errorHandler.HandleDaoErrorAsync(
                    locationsResult,
                    nameof(SelectLocationAsync),
                    true
                );
                return;
            }

            var availableLocations = locationsResult
                .Data.OrderBy(location => location.Label)
                .ToList();
            if (availableLocations.Count == 0)
            {
                await _errorHandler.HandleErrorAsync(
                    "No matching Dunnage locations were found in Infor Visual warehouse 002.",
                    Enum_ErrorSeverity.Info,
                    null,
                    true
                );
                return;
            }

            var selection = await ShowFuzzyPickerAsync(
                availableLocations,
                "Select Dunnage Location",
                "Choose a location from Infor Visual warehouse 002."
            );

            if (selection is null)
            {
                return;
            }

            load.Location = string.IsNullOrWhiteSpace(selection.Key)
                ? selection.Label
                : selection.Key;
            UpdateCanSave();
            StatusMessage = $"Selected location {load.Location}";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error selecting location",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Save Commands

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (!HasRowsOrPendingRemovals())
        {
            StatusMessage = "No changes to save";
            return;
        }

        try
        {
            IsBusy = true;
            CanSave = false;
            StatusMessage = "Saving changes...";

            var editedLoads = GetEditedLoads();
            var removedLoadCount = _removedLoads.Count;
            if (editedLoads.Count == 0 && _removedLoads.Count == 0)
            {
                StatusMessage = "No changes to save";
                _logger.LogInfo("SaveAllAsync invoked with no edited Dunnage rows", "EditMode");
                return;
            }

            var userConfirmedSave = await ConfirmSaveAsync(editedLoads.Count, removedLoadCount);
            if (!userConfirmedSave)
            {
                StatusMessage = "Save cancelled";
                _logger.LogInfo("User cancelled Dunnage Edit Mode save", "EditMode");
                return;
            }

            foreach (var load in editedLoads)
            {
                if (!HasValidRequiredSelections(load))
                {
                    StatusMessage = "All loads must have Type and Part ID";
                    return;
                }
            }

            _logger.LogInfo(
                $"Saving {editedLoads.Count} edited and {removedLoadCount} removed Dunnage load(s) from source {_currentLoadSource}",
                "EditMode"
            );

            var deleteResult = await DeleteRemovedLoadsAsync();
            if (!deleteResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(deleteResult, "SaveAllAsync", true);
                return;
            }

            var remainingLoads = _allLoads.ToList();

            Model_Dao_Result saveResult = _currentLoadSource switch
            {
                EditModeLoadSource.CurrentMemory => await _dunnageService.SaveLoadsAsync(
                    remainingLoads
                ),
                EditModeLoadSource.CurrentLabels =>
                    await _dunnageService.UpdateActiveLabelLoadsAsync(remainingLoads),
                EditModeLoadSource.History => await _dunnageService.UpdateHistoryLoadsAsync(
                    remainingLoads
                ),
                _ => Model_Dao_Result_Factory.Failure(
                    "No Dunnage Edit Mode data source is currently loaded."
                ),
            };

            if (!saveResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            await ReloadCurrentSourceAsync();
            StatusMessage =
                $"Successfully saved {editedLoads.Count} updated and {removedLoadCount} removed load(s)";
            _logger.LogInfo(
                $"Saved {editedLoads.Count} updated and {removedLoadCount} removed Dunnage load(s)",
                "EditMode"
            );
            await ShowSaveCompletedAsync(editedLoads.Count, removedLoadCount);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving loads",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
            CanSave = true;
        }
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private async Task ReturnToModeSelectionAsync()
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show dialog: XamlRoot is null", null, "EditMode");
            await _errorHandler.HandleErrorAsync(
                "Unable to display dialog",
                Enum_ErrorSeverity.Error,
                null,
                true
            );
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Change Mode?",
            Content =
                "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
            PrimaryButtonText = "Yes, Change Mode",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot,
        };

        var result = await dialog.ShowAsync().AsTask();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            try
            {
                _logger.LogInfo(
                    "User confirmed return to mode selection, clearing data",
                    "EditMode"
                );
                FilteredLoads.Clear();
                _allLoads.Clear();
                _workflowService.ClearSession();
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Failed to return to mode selection: {ex.Message}",
                    ex,
                    "EditMode"
                );
                await _errorHandler.HandleErrorAsync(
                    "Failed to return to mode selection",
                    Enum_ErrorSeverity.Error,
                    ex,
                    true
                );
            }
        }
        else
        {
            _logger.LogInfo("User cancelled return to mode selection", "EditMode");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// T167: Event handler for pagination service PageChanged event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPageChanged(object? sender, EventArgs e)
    {
        // Update FilteredLoads with current page items
        var pageLoads = _paginationService.GetCurrentPageItems<Model_DunnageLoad>();

        ReplaceFilteredLoads(pageLoads);

        CurrentPage = _paginationService.CurrentPage;
        TotalPages = _paginationService.TotalPages;
        StatusMessage = $"Page {CurrentPage} of {TotalPages}";

        _logger.LogInfo($"Page changed to {CurrentPage} of {TotalPages}", "EditMode");
    }

    private void OnLabelDataCleared(object? sender, EventArgs e)
    {
        if (_currentLoadSource != EditModeLoadSource.CurrentLabels)
        {
            return;
        }

        _allLoads.Clear();
        ReplaceFilteredLoads(Array.Empty<Model_DunnageLoad>());
        SelectedLoads.Clear();
        _removedLoads.Clear();
        _originalLoadSnapshots.Clear();
        _currentLoadSource = EditModeLoadSource.None;
        TotalRecords = 0;
        CurrentPage = 1;
        TotalPages = 1;
        CanNavigate = false;
        CanSave = false;
        StatusMessage = "Current Labels were cleared. Reload data to continue editing.";
    }

    private void UpdateCanSave()
    {
        var hasValidRemainingRows =
            _allLoads.Count == 0 || _allLoads.All(HasValidRequiredSelections);

        CanSave = hasValidRemainingRows && (GetEditedLoads().Count > 0 || _removedLoads.Count > 0);
    }

    private bool HasRowsOrPendingRemovals()
    {
        return _allLoads.Count > 0 || _removedLoads.Count > 0;
    }

    private static bool HasValidRequiredSelections(Model_DunnageLoad load)
    {
        return !string.IsNullOrWhiteSpace(load.TypeName)
            && !string.IsNullOrWhiteSpace(load.PartId)
            && !string.Equals(
                load.PartId.Trim(),
                PartSelectionPlaceholderText,
                StringComparison.OrdinalIgnoreCase
            );
    }

    private void SyncSelectedLoadsFromFlags()
    {
        SelectedLoads.Clear();

        foreach (var load in _allLoads.Where(load => load.IsSelected))
        {
            SelectedLoads.Add(load);
        }
    }

    private void EnsureDisplayLoadNumbers()
    {
        if (_allLoads.Count == 0)
        {
            return;
        }

        var needsFallbackLoadNumbers = _allLoads.All(load => load.LoadNumber <= 0);
        if (!needsFallbackLoadNumbers)
        {
            return;
        }

        for (var index = 0; index < _allLoads.Count; index++)
        {
            _allLoads[index].LoadNumber = index + 1;
        }
    }

    private void CaptureOriginalSnapshots()
    {
        _originalLoadSnapshots.Clear();
        _removedLoads.Clear();

        foreach (var load in _allLoads)
        {
            _originalLoadSnapshots[load.LoadUuid] = CreateSnapshot(load);
        }

        UpdateCanSave();
    }

    private List<Model_DunnageLoad> GetEditedLoads()
    {
        return _allLoads.Where(HasChanges).ToList();
    }

    private bool HasChanges(Model_DunnageLoad load)
    {
        if (!_originalLoadSnapshots.TryGetValue(load.LoadUuid, out var original))
        {
            return true;
        }

        return CreateSnapshot(load) != original;
    }

    private static Model_DunnageLoadSnapshot CreateSnapshot(Model_DunnageLoad load)
    {
        return new Model_DunnageLoadSnapshot(
            load.PartId,
            load.TypeId,
            load.TypeName,
            load.TypeIcon,
            load.Quantity,
            load.PoNumber,
            load.Location,
            load.LabelNumber
        );
    }

    private async Task ReloadCurrentSourceAsync()
    {
        switch (_currentLoadSource)
        {
            case EditModeLoadSource.CurrentMemory:
                _allLoads =
                    _workflowService.CurrentSession?.Loads.ToList()
                    ?? new List<Model_DunnageLoad>();
                _currentLoadSource = EditModeLoadSource.CurrentMemory;
                EnsureDisplayLoadNumbers();
                CaptureOriginalSnapshots();
                ApplySearchFilter(CurrentPage);
                break;

            case EditModeLoadSource.CurrentLabels:
                await LoadFromCurrentLabelsAsync();
                break;

            case EditModeLoadSource.History:
                await LoadFromHistoryAsync();
                break;
        }
    }

    private async Task<bool> ConfirmSaveAsync(int editedLoadCount, int removedLoadCount)
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            await _errorHandler.HandleErrorAsync(
                "Unable to display the save confirmation dialog.",
                Enum_ErrorSeverity.Error,
                null,
                true
            );
            return false;
        }

        var content = BuildSaveConfirmationMessage(editedLoadCount, removedLoadCount);
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Save Changes?",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        var result = await dialog.ShowAsync().AsTask();
        return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
    }

    private async Task ShowSaveCompletedAsync(int editedLoadCount, int removedLoadCount)
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Save Complete",
            Content = BuildSaveCompletedMessage(editedLoadCount, removedLoadCount),
            CloseButtonText = "OK",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot,
        };

        await dialog.ShowAsync().AsTask();
    }

    private static string BuildSaveConfirmationMessage(int editedLoadCount, int removedLoadCount)
    {
        if (editedLoadCount > 0 && removedLoadCount > 0)
        {
            return $"This will save {editedLoadCount} edited row(s) and remove {removedLoadCount} row(s).";
        }

        if (editedLoadCount > 0)
        {
            return $"This will save {editedLoadCount} edited row(s).";
        }

        return $"This will remove {removedLoadCount} row(s).";
    }

    private static string BuildSaveCompletedMessage(int editedLoadCount, int removedLoadCount)
    {
        if (editedLoadCount > 0 && removedLoadCount > 0)
        {
            return $"Saved {editedLoadCount} row(s) and removed {removedLoadCount} row(s).";
        }

        if (editedLoadCount > 0)
        {
            return $"Saved {editedLoadCount} row(s).";
        }

        return $"Removed {removedLoadCount} row(s).";
    }

    private async Task<Model_Dao_Result> DeleteRemovedLoadsAsync()
    {
        if (_removedLoads.Count == 0)
        {
            return Model_Dao_Result_Factory.Success();
        }

        foreach (var removedLoad in _removedLoads)
        {
            Model_Dao_Result deleteResult = _currentLoadSource switch
            {
                EditModeLoadSource.CurrentLabels =>
                    await _dunnageService.DeleteActiveLabelLoadAsync(
                        removedLoad.LoadUuid.ToString()
                    ),
                EditModeLoadSource.History => await _dunnageService.DeleteLoadAsync(
                    removedLoad.LoadUuid.ToString()
                ),
                _ => Model_Dao_Result_Factory.Success(),
            };

            if (!deleteResult.Success)
            {
                return deleteResult;
            }
        }

        return Model_Dao_Result_Factory.Success();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private void ApplySearchFilter(int pageNumber)
    {
        var filteredResults = _allLoads.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchTerm = SearchText.Trim();
            filteredResults = filteredResults.Where(load => MatchesSearch(load, searchTerm));
        }

        var filteredList = filteredResults.ToList();
        TotalRecords = filteredList.Count;
        _paginationService.SetSource(filteredList);
        TotalPages = _paginationService.TotalPages;
        CanNavigate = TotalPages > 1;

        if (filteredList.Count == 0)
        {
            CurrentPage = 1;
            ReplaceFilteredLoads(Array.Empty<Model_DunnageLoad>());
            StatusMessage = string.IsNullOrWhiteSpace(SearchText)
                ? "No loads found"
                : $"No loads match \"{SearchText}\"";
            return;
        }

        var targetPage = Math.Min(Math.Max(pageNumber, 1), TotalPages);
        LoadPage(targetPage);
    }

    private static bool MatchesSearch(Model_DunnageLoad load, string searchTerm)
    {
        return load.LoadNumber.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || load.PartId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || load.TypeName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || load.PoNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || (load.Location?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            || load.CreatedBy.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private void ReplaceFilteredLoads(IEnumerable<Model_DunnageLoad> loads)
    {
        FilteredLoads = new ObservableCollection<Model_DunnageLoad>(loads);
    }

    private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerAsync(
        IReadOnlyList<Model_FuzzySearchResult> options,
        string title,
        string subtitle
    )
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            await _errorHandler.HandleErrorAsync(
                "Unable to open the selection dialog.",
                Enum_ErrorSeverity.Error,
                null,
                true
            );
            return null;
        }

        var dialog = new Dialog_FuzzySearchPicker(options, title, subtitle)
        {
            XamlRoot = xamlRoot,
            PrimaryButtonText = "Select",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
        };

        var result = await dialog.ShowAsync();
        return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary
            ? dialog.SelectedResult
            : null;
    }

    #endregion

    #region Help Content Helpers

    /// <summary>
    /// Gets a tooltip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    /// <summary>
    /// Gets a placeholder by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

    /// <summary>
    /// Gets a tip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTip(string key) => _helpService.GetTip(key);

    #endregion
}

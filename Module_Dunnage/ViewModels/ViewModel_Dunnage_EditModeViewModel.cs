using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Edit Mode (historical data editing)
/// </summary>
public partial class ViewModel_Dunnage_EditMode : ViewModel_Shared_Base
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Pagination _paginationService;
    private readonly IService_DunnageCSVWriter _csvWriter;
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Window _windowService;
    private readonly IService_Help _helpService;

    private const int PAGE_SIZE = 50;

    public ViewModel_Dunnage_EditMode(
        IService_MySQL_Dunnage dunnageService,
        IService_Pagination paginationService,
        IService_DunnageCSVWriter csvWriter,
        IService_DunnageWorkflow workflowService,
        IService_Window windowService,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _dunnageService = dunnageService;
        _paginationService = paginationService;
        _csvWriter = csvWriter;
        _workflowService = workflowService;
        _windowService = windowService;
        _helpService = helpService;

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

    // T164: Dynamic button text for date filters
    public string LastWeekButtonText => $"Last Week ({DateTime.Now.Date.AddDays(-7):MMM d} - {DateTime.Now.Date:MMM d})";

    public string TodayButtonText => $"Today ({DateTime.Now.Date:MMM d})";

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

            return $"This Week ({startOfWeek:MMM d} - {today:MMM d})";
        }
    }

    public string ThisMonthButtonText => $"This Month ({DateTime.Now:MMMM yyyy})";

    public string ThisQuarterButtonText
    {
        get
        {
            var today = DateTime.Now.Date;
            var quarter = (today.Month - 1) / 3 + 1;
            var startMonth = (quarter - 1) * 3 + 1;
            var quarterStart = new DateTime(today.Year, startMonth, 1);
            return $"This Quarter (Q{quarter} {today.Year})";
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

            if (_workflowService.CurrentSession == null || _workflowService.CurrentSession.Loads.Count == 0)
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
            TotalRecords = _allLoads.Count;

            // Set pagination source
            _paginationService.SetSource(_allLoads);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            // Load first page
            LoadPage(1);

            CanNavigate = TotalPages > 1;
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
    /// T152: Load data from most recent CSV export (Current Labels)
    /// T153: CSV parsing error handling with line number reporting
    /// T155: Error handling for missing CSV file
    /// </summary>
    [RelayCommand]
    private async Task LoadFromCurrentLabelsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading label data...";

            var username = Environment.UserName;
            var localPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MTM_Receiving_Application",
                "DunnageData.csv"
            );

            // T155: Check if file exists
            if (!System.IO.File.Exists(localPath))
            {
                await _errorHandler.HandleErrorAsync(
                    "No label file found for current user",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true
                );
                _allLoads = new List<Model_DunnageLoad>();
                StatusMessage = "No label file found";
                _logger.LogWarning($"CSV file not found at {localPath}", "EditMode");
                return;
            }

            // T152: Parse CSV using CsvHelper
            using var reader = new System.IO.StreamReader(localPath);
            using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            var loadsList = new List<Model_DunnageLoad>();
            int lineNumber = 1; // Start at 1 (header is line 0)

            try
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    lineNumber++;
                    try
                    {
                        var load = new Model_DunnageLoad
                        {
                            TypeName = csv.GetField<string>("Type") ?? string.Empty,
                            PartId = csv.GetField<string>("PartID") ?? string.Empty,
                            Quantity = csv.GetField<decimal>("Quantity"),
                            PoNumber = csv.GetField<string>("PO") ?? string.Empty,
                            Location = csv.GetField<string>("Location") ?? string.Empty,
                            ReceivedDate = DateTime.TryParse(csv.GetField<string>("Date"), out var date) ? date : DateTime.Now
                        };

                        loadsList.Add(load);
                    }
                    catch (Exception rowEx)
                    {
                        // T153: Line-specific error reporting
                        _logger.LogWarning($"Failed to parse line {lineNumber}: {rowEx.Message}", "EditMode");
                    }
                }
            }
            catch (Exception parseEx)
            {
                // T153: CSV parsing error with line number
                await _errorHandler.HandleErrorAsync(
                    $"CSV parsing error at line {lineNumber}: {parseEx.Message}",
                    Enum_ErrorSeverity.Warning,
                    parseEx,
                    true
                );
                return;
            }

            _allLoads = loadsList;
            TotalRecords = _allLoads.Count;

            // Set pagination source
            _paginationService.SetSource(_allLoads);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            // Load first page
            LoadPage(1);

            CanNavigate = TotalPages > 1;
            StatusMessage = $"Loaded {TotalRecords} loads from labels";

            _logger.LogInfo($"Loaded {TotalRecords} loads from CSV file", "EditMode");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading label data",
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
            TotalRecords = _allLoads.Count;

            // Set pagination source
            _paginationService.SetSource(_allLoads);
            TotalPages = _paginationService.TotalPages;
            CurrentPage = _paginationService.CurrentPage;

            // Load first page
            LoadPage(1);

            CanNavigate = TotalPages > 1;
            StatusMessage = $"Loaded {TotalRecords} records";

            _logger.LogInfo($"Loaded {TotalRecords} historical loads from {startDate:d} to {endDate:d}", "EditMode");
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

    /// <summary>\n    /// Shows contextual help for edit mode\n    /// </summary>\n    [RelayCommand]\n    private async Task ShowHelpAsync()\n    {\n        await _helpService.ShowHelpAsync(\"Dunnage.EditMode\");\n    }

    /// <summary>
    /// T158: Set filter to last 7 days (today - 7 days to today)
    /// </summary>
    [RelayCommand]
    private async Task SetFilterLastWeekAsync()
    {
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
        FromDate = DateTime.Now.Date.AddYears(-1);
        ToDate = DateTime.Now.Date;
        await LoadFromHistoryAsync();
    }

    [RelayCommand]
    private void SetDateRangeToday()
    {
        FromDate = DateTime.Now.Date;
        ToDate = DateTime.Now.Date;
    }

    [RelayCommand]
    private void SetDateRangeLastWeek()
    {
        ToDate = DateTime.Now.Date;
        FromDate = DateTime.Now.Date.AddDays(-7);
    }

    [RelayCommand]
    private void SetDateRangeLastMonth()
    {
        ToDate = DateTime.Now.Date;
        FromDate = DateTime.Now.Date.AddMonths(-1);
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

        FilteredLoads.Clear();
        foreach (var load in pageLoads)
        {
            FilteredLoads.Add(load);
        }

        StatusMessage = $"Page {CurrentPage} of {TotalPages}";
        _logger.LogInfo($"Loaded page {CurrentPage} of {TotalPages}", "EditMode");
    }

    #endregion

    #region Edit Commands

    [RelayCommand]
    private void SelectAll()
    {
        SelectedLoads.Clear();
        foreach (var load in FilteredLoads)
        {
            SelectedLoads.Add(load);
        }

        UpdateCanSave();
        _logger.LogInfo($"Selected all {FilteredLoads.Count} loads on page", "EditMode");
    }

    [RelayCommand]
    private void RemoveSelectedRows()
    {
        if (SelectedLoads.Count == 0)
        {
            return;
        }

        var loadsToRemove = SelectedLoads.ToList();

        foreach (var load in loadsToRemove)
        {
            FilteredLoads.Remove(load);
            _allLoads.Remove(load);
        }

        SelectedLoads.Clear();
        TotalRecords = _allLoads.Count;
        UpdateCanSave();

        _logger.LogInfo($"Removed {loadsToRemove.Count} loads", "EditMode");
    }

    #endregion

    #region Save Commands

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (_allLoads.Count == 0)
        {
            return;
        }

        try
        {
            IsBusy = true;
            CanSave = false;
            StatusMessage = "Saving changes...";

            // Validate all loads
            foreach (var load in _allLoads)
            {
                if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
                {
                    StatusMessage = "All loads must have Type and Part ID";
                    return;
                }
            }

            // Save to database
            var saveResult = await _dunnageService.SaveLoadsAsync(_allLoads);

            if (!saveResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            // Export to CSV
            var csvResult = await _csvWriter.WriteToCSVAsync(_allLoads);

            if (!csvResult.LocalSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    csvResult.ErrorMessage ?? "CSV export failed",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true);
            }

            StatusMessage = $"Successfully saved {_allLoads.Count} loads";
            _logger.LogInfo($"Saved {_allLoads.Count} loads", "EditMode");
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
            await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Change Mode?",
            Content = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
            PrimaryButtonText = "Yes, Change Mode",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync().AsTask();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            try
            {
                _logger.LogInfo("User confirmed return to mode selection, clearing data", "EditMode");
                FilteredLoads.Clear();
                _allLoads.Clear();
                _workflowService.ClearSession();
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to return to mode selection: {ex.Message}", ex, "EditMode");
                await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex, true);
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

        FilteredLoads.Clear();
        foreach (var load in pageLoads)
        {
            FilteredLoads.Add(load);
        }

        CurrentPage = _paginationService.CurrentPage;
        TotalPages = _paginationService.TotalPages;
        StatusMessage = $"Page {CurrentPage} of {TotalPages}";

        _logger.LogInfo($"Page changed to {CurrentPage} of {TotalPages}", "EditMode");
    }

    private void UpdateCanSave()
    {
        CanSave = _allLoads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
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



using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using System.Collections.Generic;
using System.IO;

namespace MTM_Receiving_Application.ReceivingModule.ViewModels
{
    /// <summary>
    /// ViewModel for Edit Mode - allows editing existing loads but not adding new ones.
    /// </summary>
    public partial class ViewModel_Receiving_EditMode : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_Receiving _mysqlService;
        private readonly IService_CSVWriter _csvWriter;
        private readonly IService_Pagination _paginationService;
        private readonly IService_Help _helpService;

        private readonly List<Model_ReceivingLoad> _allLoads = new();
        private List<Model_ReceivingLoad> _filteredLoads = new();

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        [ObservableProperty]
        private Enum_DataSourceType _currentDataSource = Enum_DataSourceType.Memory;

        [ObservableProperty]
        private string _selectAllButtonText = "Select All";

        [ObservableProperty]
        private DateTimeOffset _filterStartDate = DateTimeOffset.Now.AddDays(-7);

        [ObservableProperty]
        private DateTimeOffset _filterEndDate = DateTimeOffset.Now;

        [ObservableProperty]
        private string _thisMonthButtonText = DateTime.Now.ToString("MMMM");

        [ObservableProperty]
        private string _thisQuarterButtonText = GetQuarterText(DateTime.Now);

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _gotoPageNumber = 1;

        private string? _currentCsvPath;
        private readonly List<Model_ReceivingLoad> _deletedLoads = new();
        private readonly IService_Window _windowService;

        public ObservableCollection<Enum_PackageType> PackageTypes { get; } = new(Enum.GetValues<Enum_PackageType>());

        /// <summary>
        /// Initializes a new instance of the EditModeViewModel class.
        /// </summary>
        /// <param name="workflowService"></param>
        /// <param name="mysqlService"></param>
        /// <param name="csvWriter"></param>
        /// <param name="paginationService"></param>
        /// <param name="errorHandler"></param>
        /// <param name="logger"></param>
        /// <param name="windowService"></param>
        /// <param name="helpService"></param>
        public ViewModel_Receiving_EditMode(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_Receiving mysqlService,
            IService_CSVWriter csvWriter,
            IService_Pagination paginationService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Window windowService,
            IService_Help helpService)
            : base(errorHandler, logger)
        {
            _windowService = windowService;
            _workflowService = workflowService;
            _mysqlService = mysqlService;
            _csvWriter = csvWriter;
            _paginationService = paginationService;
            _helpService = helpService;

            _loads = new ObservableCollection<Model_ReceivingLoad>();
            _loads.CollectionChanged += Loads_CollectionChanged;

            _paginationService.PageChanged += OnPageChanged;
            _paginationService.PageSize = 20;

            _logger.LogInfo("Edit Mode initialized");
        }

        /// <summary>
        /// Gets the text representation of the quarter for a given date.
        /// </summary>
        /// <param name="date"></param>
        private static string GetQuarterText(DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            return quarter switch
            {
                1 => "Jan-Mar",
                2 => "Apr-Jun",
                3 => "Jul-Sep",
                4 => "Oct-Dec",
                _ => "Quarter"
            };
        }

        /// <summary>
        /// Handles the pagination page changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageChanged(object? sender, EventArgs e)
        {
            UpdatePagedDisplay();
        }

        /// <summary>
        /// Updates the displayed loads based on the current page.
        /// </summary>
        private void UpdatePagedDisplay()
        {
            var pageItems = _paginationService.GetCurrentPageItems<Model_ReceivingLoad>();

            Loads.Clear();
            foreach (var item in pageItems)
            {
                Loads.Add(item);
            }

            CurrentPage = _paginationService.CurrentPage;
            TotalPages = _paginationService.TotalPages;
            GotoPageNumber = CurrentPage;

            NotifyPaginationCommands();
            NotifyCommands();
        }

        /// <summary>
        /// Notifies that pagination command execution status has changed.
        /// </summary>
        private void NotifyPaginationCommands()
        {
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
            FirstPageCommand.NotifyCanExecuteChanged();
            LastPageCommand.NotifyCanExecuteChanged();
            GoToPageCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Handles changes to the Loads collection to attach/detach property change listeners.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Model_ReceivingLoad item in e.NewItems)
                {
                    item.PropertyChanged += Load_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Model_ReceivingLoad item in e.OldItems)
                {
                    item.PropertyChanged -= Load_PropertyChanged;
                }
            }

            NotifyCommands();
        }

        /// <summary>
        /// Handles property changes on individual load items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model_ReceivingLoad.IsSelected))
            {
                RemoveRowCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Notifies that command execution status has changed.
        /// </summary>
        private void NotifyCommands()
        {
            SaveCommand.NotifyCanExecuteChanged();
            RemoveRowCommand.NotifyCanExecuteChanged();
            SelectAllCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Handles changes to the filter start date.
        /// </summary>
        partial void OnFilterStartDateChanged(DateTimeOffset value) => ApplyDateFilter();

        /// <summary>
        /// Handles changes to the filter end date.
        /// </summary>
        partial void OnFilterEndDateChanged(DateTimeOffset value) => ApplyDateFilter();

        /// <summary>
        /// Applies the date filter to the loaded data.
        /// </summary>
        private void ApplyDateFilter()
        {
            if (_allLoads.Count == 0)
            {
                return;
            }

            FilterAndPaginate();
        }

        /// <summary>
        /// Filters the master list and updates pagination.
        /// </summary>
        private void FilterAndPaginate()
        {
            var start = FilterStartDate.Date;
            var end = FilterEndDate.Date.AddDays(1).AddTicks(-1);

            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                _filteredLoads = _allLoads.Where(l => l.ReceivedDate >= start && l.ReceivedDate <= end).ToList();
            }
            else
            {
                _filteredLoads = _allLoads.Where(l => l.ReceivedDate >= start && l.ReceivedDate <= end).ToList();
            }

            _paginationService.SetSource(_filteredLoads);
        }

        /// <summary>
        /// Sets the date filter to the last 7 days.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterLastWeekAsync()
        {
            FilterStartDate = DateTime.Today.AddDays(-7);
            FilterEndDate = DateTime.Today;
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        /// <summary>
        /// Sets the date filter to today.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterTodayAsync()
        {
            FilterStartDate = DateTime.Today;
            FilterEndDate = DateTime.Today;
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        /// <summary>
        /// Sets the date filter to the current week.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterThisWeekAsync()
        {
            var today = DateTime.Today;
            var start = today.AddDays(-(int)today.DayOfWeek);
            var end = start.AddDays(6);
            FilterStartDate = start;
            FilterEndDate = end;
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        /// <summary>
        /// Sets the date filter to the current month.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterThisMonthAsync()
        {
            var today = DateTime.Today;
            FilterStartDate = new DateTime(today.Year, today.Month, 1);
            FilterEndDate = FilterStartDate.AddMonths(1).AddDays(-1);
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        /// <summary>
        /// Sets the date filter to the current quarter.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterThisQuarterAsync()
        {
            var today = DateTime.Today;
            int quarter = (today.Month - 1) / 3 + 1;
            FilterStartDate = new DateTime(today.Year, 3 * quarter - 2, 1);
            FilterEndDate = FilterStartDate.AddMonths(3).AddDays(-1);
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        /// <summary>
        /// Sets the date filter to show all records (last year).
        /// </summary>
        [RelayCommand]
        private async Task SetFilterShowAllAsync()
        {
            FilterStartDate = DateTime.Today.AddYears(-1);
            FilterEndDate = DateTime.Today;
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        /// <summary>
        /// Navigates to the previous page.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private void PreviousPage() => _paginationService.PreviousPage();

        /// <summary>
        /// Navigates to the next page.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void NextPage() => _paginationService.NextPage();

        /// <summary>
        /// Navigates to the first page.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private void FirstPage() => _paginationService.FirstPage();

        /// <summary>
        /// Navigates to the last page.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void LastPage() => _paginationService.LastPage();

        /// <summary>
        /// Navigates to a specific page number.
        /// </summary>
        [RelayCommand]
        private void GoToPage() => _paginationService.GoToPage(GotoPageNumber);

        /// <summary>
        /// Determines if navigation to the next page is possible.
        /// </summary>
        private bool CanGoNext() => _paginationService.HasNextPage;

        /// <summary>
        /// Determines if navigation to the previous page is possible.
        /// </summary>
        private bool CanGoPrevious() => _paginationService.HasPreviousPage;

        /// <summary>
        /// Loads data from the current in-memory session.
        /// </summary>
        [RelayCommand]
        private async Task LoadFromCurrentMemoryAsync()
        {
            try
            {
                _logger.LogInfo("Loading data from current memory");
                IsBusy = true;
                StatusMessage = "Loading from current session...";
                _deletedLoads.Clear();

                var currentLoads = _workflowService.CurrentSession.Loads;
                if (currentLoads.Count == 0)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No data in current session. Please use Manual Entry mode to create new loads.",
                        Enum_ErrorSeverity.Warning);
                    return;
                }

                _allLoads.Clear();
                foreach (var load in currentLoads)
                {
                    _allLoads.Add(load);
                }

                StatusMessage = $"Loaded {_allLoads.Count} loads from current session";
                _logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from current memory");
                CurrentDataSource = Enum_DataSourceType.Memory;
                SelectAllButtonText = "Select All";

                FilterStartDate = DateTimeOffset.Now.AddYears(-1);
                FilterEndDate = DateTimeOffset.Now;

                FilterAndPaginate();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from current memory: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to load data from current session", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Loads data from current label CSV files.
        /// </summary>
        [RelayCommand]
        private async Task LoadFromCurrentLabelsAsync()
        {
            try
            {
                _logger.LogInfo("User initiated Current Labels (CSV) load");
                IsBusy = true;
                StatusMessage = "Checking for existing label files...";

                if (await TryLoadFromDefaultCsvAsync())
                {
                    return;
                }

                await _errorHandler.ShowErrorDialogAsync(
                    "No Labels Found",
                    "Could not find any current label files in the default locations.",
                    Enum_ErrorSeverity.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from labels: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to load data from label file", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Attempts to load data from default CSV locations (local or network).
        /// </summary>
        private async Task<bool> TryLoadFromDefaultCsvAsync()
        {
            string localPath = _csvWriter.GetLocalCSVPath();
            if (File.Exists(localPath))
            {
                try
                {
                    _logger.LogInfo($"Attempting to load from local CSV: {localPath}");
                    var loadedData = await _csvWriter.ReadFromCSVAsync(localPath);

                    if (loadedData.Count > 0)
                    {
                        _deletedLoads.Clear();
                        _allLoads.Clear();
                        foreach (var load in loadedData)
                        {
                            _allLoads.Add(load);
                            _workflowService.CurrentSession.Loads.Add(load);
                        }
                        StatusMessage = $"Loaded {_allLoads.Count} loads from local labels";
                        _logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from local labels");
                        CurrentDataSource = Enum_DataSourceType.CurrentLabels;
                        _currentCsvPath = localPath;
                        SelectAllButtonText = "Select All";

                        FilterStartDate = DateTimeOffset.Now.AddYears(-1);
                        FilterEndDate = DateTimeOffset.Now;

                        FilterAndPaginate();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to load from local labels: {ex.Message}");
                }
            }

            try
            {
                string networkPath = _csvWriter.GetNetworkCSVPath();
                if (File.Exists(networkPath))
                {
                    _logger.LogInfo($"Attempting to load from network labels: {networkPath}");
                    var loadedData = await _csvWriter.ReadFromCSVAsync(networkPath);

                    if (loadedData.Count > 0)
                    {
                        _deletedLoads.Clear();
                        _allLoads.Clear();
                        foreach (var load in loadedData)
                        {
                            _allLoads.Add(load);
                            _workflowService.CurrentSession.Loads.Add(load);
                        }
                        StatusMessage = $"Loaded {_allLoads.Count} loads from network labels";
                        _logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from network labels");
                        CurrentDataSource = Enum_DataSourceType.CurrentLabels;
                        _currentCsvPath = networkPath;
                        SelectAllButtonText = "Select All";

                        FilterStartDate = DateTimeOffset.Now.AddYears(-1);
                        FilterEndDate = DateTimeOffset.Now;

                        FilterAndPaginate();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to load from network labels: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Loads historical data from the database.
        /// </summary>
        [RelayCommand]
        private async Task LoadFromHistoryAsync()
        {
            try
            {
                _logger.LogInfo("User initiated history load");
                IsBusy = true;
                StatusMessage = "Loading from history...";

                var startDate = FilterStartDate.Date;
                var endDate = FilterEndDate.Date;

                _logger.LogInfo($"Loading receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                var result = await _mysqlService.GetAllReceivingLoadsAsync(startDate, endDate);

                if (!result.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        $"Failed to load from history: {result.ErrorMessage}",
                        Enum_ErrorSeverity.Error);
                    return;
                }

                if (result.Data == null || result.Data.Count == 0)
                {
                    await _errorHandler.HandleErrorAsync(
                        "No receiving records found in the specified date range.",
                        Enum_ErrorSeverity.Warning);
                    return;
                }

                _deletedLoads.Clear();
                _allLoads.Clear();
                foreach (var load in result.Data)
                {
                    _allLoads.Add(load);
                    _workflowService.CurrentSession.Loads.Add(load);
                }

                StatusMessage = $"Loaded {_allLoads.Count} loads from history";
                _logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from history");
                CurrentDataSource = Enum_DataSourceType.History;
                SelectAllButtonText = "Select All";

                FilterAndPaginate();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from history: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to load data from history", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Selects or deselects all currently displayed loads.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSelectAll))]
        private void SelectAll()
        {
            bool anyUnselected = Loads.Any(l => !l.IsSelected);

            if (anyUnselected)
            {
                foreach (var load in Loads)
                {
                    load.IsSelected = true;
                }

                SelectAllButtonText = "Deselect All";
            }
            else
            {
                foreach (var load in Loads)
                {
                    load.IsSelected = false;
                }

                SelectAllButtonText = "Select All";
            }
        }

        /// <summary>
        /// Determines if the Select All command can be executed.
        /// </summary>
        private bool CanSelectAll() => Loads.Count > 0;

        /// <summary>
        /// Removes the selected row(s) from the collection.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRemoveRow))]
        private void RemoveRow()
        {
            var selectedLoads = Loads.Where(l => l.IsSelected).ToList();

            if (selectedLoads.Count > 0)
            {
                _logger.LogInfo($"Removing {selectedLoads.Count} selected loads");
                foreach (var load in selectedLoads)
                {
                    _deletedLoads.Add(load);
                    _workflowService.CurrentSession.Loads.Remove(load);
                    _allLoads.Remove(load);
                    _filteredLoads.Remove(load);
                    Loads.Remove(load);
                }

                FilterAndPaginate();
            }
            else if (SelectedLoad != null)
            {
                _logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
                _deletedLoads.Add(SelectedLoad);
                _workflowService.CurrentSession.Loads.Remove(SelectedLoad);
                _allLoads.Remove(SelectedLoad);
                _filteredLoads.Remove(SelectedLoad);
                Loads.Remove(SelectedLoad);

                FilterAndPaginate();
            }
            else
            {
                _logger.LogWarning("RemoveRow called with no selected load(s)");
            }
            SaveCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Determines if the Remove Row command can be executed.
        /// </summary>
        private bool CanRemoveRow() => Loads.Any(l => l.IsSelected);

        /// <summary>
        /// Saves the changes made to the loads.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            try
            {
                _logger.LogInfo($"Validating and saving {_filteredLoads.Count} loads from edit mode");
                IsBusy = true;
                StatusMessage = "Validating loads...";

                // Set default Heat/Lot if empty for all loads being processed
                // We update _allLoads to ensure consistency across all data sources
                foreach (var load in _allLoads)
                {
                    if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                    {
                        load.HeatLotNumber = "Nothing Entered";
                    }
                }

                var validationErrors = ValidateLoads(_filteredLoads);
                if (validationErrors.Count > 0)
                {
                    var errorMessage = string.Join("\n", validationErrors);
                    _logger.LogWarning($"Edit mode validation failed: {validationErrors.Count} errors");
                    await _errorHandler.HandleErrorAsync(
                        $"Validation failed:\n{errorMessage}",
                        Enum_ErrorSeverity.Warning);
                    return;
                }

                StatusMessage = "Saving data...";

                switch (CurrentDataSource)
                {
                    case Enum_DataSourceType.Memory:
                        await _workflowService.AdvanceToNextStepAsync();
                        break;

                    case Enum_DataSourceType.CurrentLabels:
                        if (string.IsNullOrEmpty(_currentCsvPath))
                        {
                            await _errorHandler.HandleErrorAsync("No label file path available for saving.", Enum_ErrorSeverity.Error);
                            return;
                        }

                        _logger.LogInfo($"Overwriting label file: {_currentCsvPath}");
                        await _csvWriter.WriteToFileAsync(_currentCsvPath, _allLoads, append: false);
                        StatusMessage = "Label file updated successfully";
                        await _errorHandler.ShowErrorDialogAsync("Success", "Label file updated successfully.", Enum_ErrorSeverity.Info);
                        break;

                    case Enum_DataSourceType.History:
                        _logger.LogInfo("Updating history records");
                        int deleted = 0;
                        if (_deletedLoads.Count > 0)
                        {
                            _logger.LogInfo($"Deleting {_deletedLoads.Count} removed records");
                            deleted = await _mysqlService.DeleteReceivingLoadsAsync(_deletedLoads);
                        }

                        int updated = 0;
                        if (_filteredLoads.Count > 0)
                        {
                            updated = await _mysqlService.UpdateReceivingLoadsAsync(_filteredLoads);
                        }

                        StatusMessage = $"History updated ({updated} updated, {deleted} deleted)";
                        await _errorHandler.ShowErrorDialogAsync("Success", $"History updated successfully.\n{updated} records updated.\n{deleted} records deleted.", Enum_ErrorSeverity.Info);
                        break;
                }

                _logger.LogInfo($"Edit mode save completed successfully for source: {CurrentDataSource}");
                _deletedLoads.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save edit mode data: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Determines if the Save command can be executed.
        /// </summary>
        private bool CanSave()
        {
            return _filteredLoads.Count > 0 || _deletedLoads.Count > 0;
        }

        /// <summary>
        /// Returns to the mode selection screen after confirmation.
        /// </summary>
        [RelayCommand]
        private async Task ReturnToModeSelectionAsync()
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
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
                    _logger.LogInfo("User confirmed return to mode selection, resetting workflow");
                    await _workflowService.ResetWorkflowAsync();
                    _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to reset workflow: {ex.Message}");
                    await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
                }
            }
            else
            {
                _logger.LogInfo("User cancelled return to mode selection");
            }
        }

        /// <summary>
        /// Validates the list of loads before saving.
        /// </summary>
        /// <param name="loadsToValidate"></param>
        private System.Collections.Generic.List<string> ValidateLoads(IEnumerable<Model_ReceivingLoad> loadsToValidate)
        {
            var errors = new System.Collections.Generic.List<string>();

            if (!loadsToValidate.Any() && _deletedLoads.Count == 0)
            {
                errors.Add("No loads to save");
                return errors;
            }

            foreach (var load in loadsToValidate)
            {
                if (string.IsNullOrWhiteSpace(load.PartID))
                {
                    errors.Add($"Load #{load.LoadNumber}: Part ID is required");
                }

                if (load.WeightQuantity <= 0)
                {
                    errors.Add($"Load #{load.LoadNumber}: Weight/Quantity must be greater than zero");
                }

                if (load.PackagesPerLoad <= 0)
                {
                    errors.Add($"Load #{load.LoadNumber}: Packages per load must be greater than zero");
                }
            }

            return errors;
        }

        /// <summary>
        /// Shows contextual help for edit mode
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.EditMode");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

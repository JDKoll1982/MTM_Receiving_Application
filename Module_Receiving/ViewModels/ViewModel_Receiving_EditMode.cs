using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    /// <summary>
    /// ViewModel for Edit Mode — view, search, filter, and edit existing receiving loads.
    /// </summary>
    public partial class ViewModel_Receiving_EditMode : ViewModel_Shared_Base, IResettableViewModel
    {
        private const string EditModeOwnershipRestrictionMessage =
            "You can only view, change, or remove rows that you created. Admin and Developer users can access all Edit Mode rows.";

        // ------------------------------------------------------------------ services
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_Receiving _mysqlService;
        private readonly IService_ReceivingLabelData _labelDataService;
        private readonly IService_Pagination _paginationService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingSettings _receivingSettings;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Window _windowService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_UserPrivileges _userPrivileges;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private bool _isLoadingEditModePreferences;

        private enum DateFilterPreset
        {
            None,
            LastWeek,
            Today,
            Yesterday,
            ThisWeek,
            ThisMonth,
            ThisQuarter,
            ShowAll,
        }

        // ------------------------------------------------------------------ data
        private readonly List<Model_ReceivingLoad> _allLoads = new();
        private List<Model_ReceivingLoad> _filteredLoads = new();
        private readonly List<Model_ReceivingLoad> _deletedLoads = new();
        private string? _currentLabelDataPath;

        // ------------------------------------------------------------------ bindable collections
        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        /// <summary>All column definitions, in order. Driven by column-chooser dialog.</summary>
        public ObservableCollection<Model_EditModeColumn> ColumnSettings { get; } = new();

        /// <summary>Options shown in the "Search by" ComboBox.</summary>
        public ObservableCollection<string> SearchByOptions { get; } =
            new()
            {
                "All Fields",
                "Part ID",
                "PO Number",
                "Heat/Lot",
                "Employee #",
                "Part Description",
                "Vendor",
            };

        public ObservableCollection<Enum_PackageType> PackageTypes { get; } =
            new(Enum.GetValues<Enum_PackageType>());

        // ------------------------------------------------------------------ selection
        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        [ObservableProperty]
        private Enum_DataSourceType _currentDataSource = Enum_DataSourceType.Memory;

        [ObservableProperty]
        private string _selectAllButtonText = "Select All";

        // ------------------------------------------------------------------ search
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _searchByColumnKey = "All Fields";

        partial void OnSearchTextChanged(string value)
        {
            OnPropertyChanged(nameof(HasSearchText));
            FilterAndPaginate();
        }

        partial void OnSearchByColumnKeyChanged(string value)
        {
            FilterAndPaginate();
            PersistEditModePreferencesIfReady();
        }

        /// <summary>True when search text is non-empty; drives the clear-button visibility.</summary>
        public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

        // ------------------------------------------------------------------ result summary
        [ObservableProperty]
        private string _resultSummary = string.Empty;

        // ------------------------------------------------------------------ empty state
        private bool _hasLoadedData;

        [ObservableProperty]
        private string _emptyStateMessage = string.Empty;

        /// <summary>
        /// True when a load has completed but there are no rows to show (empty source, or the
        /// search/date filter matched nothing). Drives the "nothing found" image overlay.
        /// </summary>
        public bool ShowEmptyState => _hasLoadedData && _filteredLoads.Count == 0;

        private void ResetEmptyState()
        {
            _hasLoadedData = false;
            EmptyStateMessage = string.Empty;
            OnPropertyChanged(nameof(ShowEmptyState));
        }

        private void ShowEmptyStateFor(string message)
        {
            _hasLoadedData = true;
            _filteredLoads = new List<Model_ReceivingLoad>();
            EmptyStateMessage = message;
            OnPropertyChanged(nameof(ShowEmptyState));
        }

        // ------------------------------------------------------------------ sort
        [ObservableProperty]
        private string _sortColumn = string.Empty;

        [ObservableProperty]
        private bool _sortAscending = true;

        // ------------------------------------------------------------------ page size
        /// <summary>Choices available in the per-page ComboBox.</summary>
        public ObservableCollection<int> PageSizeOptions { get; } = [20, 50, 100, 200];

        [ObservableProperty]
        private int _selectedPageSize = 20;

        partial void OnSelectedPageSizeChanged(int value)
        {
            _paginationService.PageSize = value;
            if (_filteredLoads.Count > 0)
            {
                _paginationService.SetSource(_filteredLoads);
            }

            PersistEditModePreferencesIfReady();
        }

        // ------------------------------------------------------------------ date filter
        [ObservableProperty]
        private DateTimeOffset _filterStartDate = DateTimeOffset.Now.AddDays(-7);

        [ObservableProperty]
        private DateTimeOffset _filterEndDate = DateTimeOffset.Now;

        private DateFilterPreset _activeDateFilter = DateFilterPreset.None;

        private bool _isApplyingPresetFilter;

        [ObservableProperty]
        private string _thisMonthButtonText = DateTime.Now.ToString("MMMM");

        [ObservableProperty]
        private string _thisQuarterButtonText = GetQuarterText(DateTime.Now);

        [ObservableProperty]
        private bool _datePresetsExpanded;

        public string DatePresetsToggleGlyph => DatePresetsExpanded ? "\uE76B" : "\uE76C";

        partial void OnDatePresetsExpandedChanged(bool value)
        {
            OnPropertyChanged(nameof(DatePresetsToggleGlyph));
        }

        [RelayCommand]
        private void ToggleDatePresets() => DatePresetsExpanded = !DatePresetsExpanded;

        // ------------------------------------------------------------------ pagination
        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _gotoPageNumber = 1;

        // ------------------------------------------------------------------ UI text (loaded from settings)
        [ObservableProperty]
        private string _editModeLoadDataFromText = "Load Data From:";

        [ObservableProperty]
        private string _editModeCurrentMemoryText = "Current Memory";

        [ObservableProperty]
        private string _editModeCurrentLabelsText = "Current Labels";

        [ObservableProperty]
        private string _editModeHistoryText = "History";

        [ObservableProperty]
        private string _editModeFilterDateText = "Filter Date:";

        [ObservableProperty]
        private string _editModeToText = "to";

        [ObservableProperty]
        private string _editModeLastWeekText = "Last Week";

        [ObservableProperty]
        private string _editModeTodayText = "Today";

        [ObservableProperty]
        private string _editModeThisWeekText = "This Week";

        [ObservableProperty]
        private string _editModeShowAllText = "Show All";

        [ObservableProperty]
        private string _editModePageText = "Page";

        [ObservableProperty]
        private string _editModeOfText = "of";

        [ObservableProperty]
        private string _editModeGoText = "Go";

        [ObservableProperty]
        private string _editModeSaveAndFinishText = "Save & Finish";

        [ObservableProperty]
        private string _editModeRemoveRowText = "Remove Row";

        [ObservableProperty]
        private string _editModeColumnLoadNumberText = "Load #";

        [ObservableProperty]
        private string _editModeColumnPartIdText = "Part ID";

        [ObservableProperty]
        private string _editModeColumnWeightQtyText = "Weight/Qty";

        [ObservableProperty]
        private string _editModeColumnHeatLotText = "Heat/Lot";

        [ObservableProperty]
        private string _editModeColumnPkgTypeText = "Pkg Type";

        [ObservableProperty]
        private string _editModeColumnPkgsPerLoadText = "Pkgs/Load";

        [ObservableProperty]
        private string _editModeColumnWtPerPkgText = "UOM/Pkg";

        private static readonly Brush ActiveFilterButtonBrush = new SolidColorBrush(
            Colors.DodgerBlue
        );
        private static readonly Brush InactiveFilterButtonBrush = new SolidColorBrush(
            Colors.Transparent
        );

        private static Brush ResolveFilterButtonBackground(bool isActive)
        {
            return isActive ? ActiveFilterButtonBrush : InactiveFilterButtonBrush;
        }

        public Brush LastWeekFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.LastWeek
        );

        public Brush TodayFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.Today
        );

        public Brush YesterdayFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.Yesterday
        );

        public Brush ThisWeekFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.ThisWeek
        );

        public Brush ThisMonthFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.ThisMonth
        );

        public Brush ThisQuarterFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.ThisQuarter
        );

        public Brush ShowAllFilterButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter == DateFilterPreset.ShowAll
        );

        /// <summary>Background for the date-range toolbar button, highlighted while a quick preset filter is active.</summary>
        public Brush DateRangeButtonBackground => ResolveFilterButtonBackground(
            _activeDateFilter != DateFilterPreset.None
        );

        // ------------------------------------------------------------------ event — tells the View to open the column-chooser dialog
        /// <summary>Raised when the user clicks the "Columns" toolbar button.</summary>
        public event EventHandler? ShowColumnChooserRequested;

        // ------------------------------------------------------------------ constructor
        public ViewModel_Receiving_EditMode(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_Receiving mysqlService,
            IService_ReceivingLabelData labelDataService,
            IService_Pagination paginationService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Window windowService,
            IService_Help helpService,
            IService_ReceivingSettings receivingSettings,
            IService_Notification notificationService,
            IService_ReceivingValidation validationService,
            IService_UserSessionManager sessionManager,
            IService_UserPrivileges userPrivileges,
            IService_ViewModelRegistry viewModelRegistry
        )
            : base(errorHandler, logger, notificationService)
        {
            _windowService = windowService;
            _workflowService = workflowService;
            _mysqlService = mysqlService;
            _labelDataService = labelDataService;
            _paginationService = paginationService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;
            _validationService = validationService;
            _sessionManager = sessionManager;
            _userPrivileges = userPrivileges;
            _viewModelRegistry = viewModelRegistry;

            _loads = new ObservableCollection<Model_ReceivingLoad>();
            _loads.CollectionChanged += Loads_CollectionChanged;

            _paginationService.PageChanged += OnPageChanged;
            _paginationService.PageSize = 20;
            _workflowService.StepChanged += OnWorkflowStepChanged;
            _viewModelRegistry.Register(this);

            _logger.LogInfo("Edit Mode initialized");

            _ = LoadUITextAsync()
                .ContinueWith(_ => LoadColumnVisibilityAsync(), TaskScheduler.Default);
        }

        public void ResetToDefaults()
        {
            _allLoads.Clear();
            _filteredLoads = new List<Model_ReceivingLoad>();
            _deletedLoads.Clear();
            ReplaceLoads(Array.Empty<Model_ReceivingLoad>());
            ResetEmptyState();
            SelectedLoad = null;
            CurrentDataSource = Enum_DataSourceType.Memory;
            SearchText = string.Empty;
            SearchByColumnKey = "All Fields";
            ResultSummary = string.Empty;
            SortColumn = string.Empty;
            SortAscending = true;
            SelectedPageSize = 20;
            CurrentPage = 1;
            TotalPages = 1;
            GotoPageNumber = 1;
            FilterStartDate = DateTimeOffset.Now.AddDays(-7);
            FilterEndDate = DateTimeOffset.Now;
            SetActiveDateFilter(DateFilterPreset.None);
            SelectAllButtonText = "Select All";
            StatusMessage = string.Empty;
            _currentLabelDataPath = null;
        }

        private bool CurrentUserHasFullEditModeAccess()
        {
            var currentUser = _sessionManager.CurrentSession?.User;
            return currentUser != null
                && _userPrivileges.IsInitialized
                && _userPrivileges.CurrentUserId == currentUser.EmployeeNumber
                && _userPrivileges.HasAnyRole("Admin", "Developer");
        }

        private bool IsLoadOwnedByCurrentUser(Model_ReceivingLoad load)
        {
            if (CurrentUserHasFullEditModeAccess())
            {
                return true;
            }

            var currentUser = _sessionManager.CurrentSession?.User;
            if (currentUser == null)
            {
                return false;
            }

            if (
                currentUser.EmployeeNumber > 0
                && load.EmployeeNumber > 0
                && currentUser.EmployeeNumber == load.EmployeeNumber
            )
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(currentUser.WindowsUsername)
                && !string.IsNullOrWhiteSpace(load.UserId)
                && string.Equals(
                    currentUser.WindowsUsername,
                    load.UserId,
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private List<Model_ReceivingLoad> ApplyOwnershipVisibilityRules(
            IEnumerable<Model_ReceivingLoad> loads,
            string sourceName
        )
        {
            var sourceLoads = loads?.ToList() ?? new List<Model_ReceivingLoad>();
            if (CurrentUserHasFullEditModeAccess())
            {
                return sourceLoads;
            }

            var visibleLoads = sourceLoads.Where(IsLoadOwnedByCurrentUser).ToList();
            if (visibleLoads.Count != sourceLoads.Count)
            {
                _logger.LogInfo(
                    $"Hid {sourceLoads.Count - visibleLoads.Count} {sourceName} rows created by other users."
                );
            }

            return visibleLoads;
        }

        private async Task<bool> EnsureCurrentUserCanManageLoadsAsync(
            IEnumerable<Model_ReceivingLoad> loads,
            string actionDescription
        )
        {
            var candidateLoads = loads?.ToList() ?? new List<Model_ReceivingLoad>();
            if (candidateLoads.Count == 0 || CurrentUserHasFullEditModeAccess())
            {
                return true;
            }

            var unauthorizedCount = candidateLoads.Count(load => !IsLoadOwnedByCurrentUser(load));
            if (unauthorizedCount == 0)
            {
                return true;
            }

            _logger.LogWarning(
                $"Blocked edit-mode {actionDescription} for {unauthorizedCount} row(s) created by other users."
            );
            await _errorHandler.ShowErrorDialogAsync(
                "Access Restricted",
                EditModeOwnershipRestrictionMessage,
                Enum_ErrorSeverity.Warning
            );
            return false;
        }

        private void OnWorkflowStepChanged(object? sender, EventArgs e)
        {
            if (_workflowService.CurrentStep != Enum_ReceivingWorkflowStep.EditMode)
            {
                return;
            }

            _ =
                _workflowService.RequestedEditDataSource == Enum_DataSourceType.CurrentLabels
                    ? LoadFromCurrentLabelsAsync()
                    : LoadFromCurrentMemoryAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                EditModeLoadDataFromText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeLoadDataFrom
                );
                EditModeCurrentMemoryText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeCurrentMemory
                );
                EditModeCurrentLabelsText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeCurrentLabels
                );
                EditModeHistoryText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeHistory
                );
                EditModeFilterDateText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeFilterDate
                );
                EditModeToText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeTo
                );
                EditModeLastWeekText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeLastWeek
                );
                EditModeTodayText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeToday
                );
                EditModeThisWeekText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeThisWeek
                );
                EditModeShowAllText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeShowAll
                );
                EditModePageText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModePage
                );
                EditModeOfText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeOf
                );
                EditModeGoText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeGo
                );
                EditModeSaveAndFinishText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSaveAndFinish
                );
                EditModeRemoveRowText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeRemoveRow
                );

                EditModeColumnLoadNumberText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnLoadNumber
                );
                EditModeColumnPartIdText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnPartId
                );
                EditModeColumnWeightQtyText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnWeightQty
                );
                EditModeColumnHeatLotText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnHeatLot
                );
                EditModeColumnPkgTypeText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnPkgType
                );
                EditModeColumnPkgsPerLoadText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnPkgsPerLoad
                );
                EditModeColumnWtPerPkgText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnWtPerPkg
                );

                _logger.LogInfo("Edit Mode UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error loading Edit Mode UI text from settings: {ex.Message}",
                    ex
                );
            }
        }

        // ------------------------------------------------------------------ column visibility
        /// <summary>
        /// Loads the user's saved column visibility preference from settings and populates
        /// <see cref="ColumnSettings"/>. Falls back to the default visible set if none stored.
        /// </summary>
        /// <param name="userId">Optional user ID for user-scoped settings.</param>
        internal async Task LoadColumnVisibilityAsync(int? userId = null)
        {
            _isLoadingEditModePreferences = true;
            try
            {
                var stored = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnVisibility,
                    userId
                );

                var savedSearchBy = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSearchByColumn,
                    userId
                );

                if (
                    !string.IsNullOrWhiteSpace(savedSearchBy)
                    && SearchByOptions.Contains(savedSearchBy)
                )
                {
                    SearchByColumnKey = savedSearchBy;
                }

                // Sort preference
                var savedSortColumn = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSortColumn,
                    userId
                );
                var savedSortAscending = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSortAscending,
                    userId
                );

                SortColumn = savedSortColumn ?? string.Empty;
                SortAscending = savedSortAscending != "false";

                // Page-size preference
                var savedPageSize = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.EditModePageSize,
                    userId
                );

                if (int.TryParse(savedPageSize, out int ps) && PageSizeOptions.Contains(ps))
                {
                    _selectedPageSize = ps; // bypass partial callback — no data yet
                    _paginationService.PageSize = ps;
                    OnPropertyChanged(nameof(SelectedPageSize));
                }

                var visibleKeys = new HashSet<string>(
                    stored.Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                );

                ColumnSettings.Clear();
                foreach (var col in BuildAllColumns())
                {
                    col.IsVisible =
                        col.IsAlwaysVisible
                        || (
                            visibleKeys.Count == 0
                                ? IsDefaultVisible(col.Key)
                                : visibleKeys.Contains(col.Key)
                        );
                    ColumnSettings.Add(col);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading column visibility: {ex.Message}", ex);
                ColumnSettings.Clear();
                foreach (var col in BuildAllColumns())
                {
                    col.IsVisible = IsDefaultVisible(col.Key);
                    ColumnSettings.Add(col);
                }
            }
            finally
            {
                _isLoadingEditModePreferences = false;
            }
        }

        /// <summary>
        /// Persists the current column visibility selection to settings.
        /// Called after the column-chooser dialog is confirmed.
        /// </summary>
        /// <param name="userId">Optional user ID for user-scoped settings.</param>
        internal async Task SaveColumnVisibilityAsync(int? userId = null)
        {
            try
            {
                var visible = string.Join(
                    ",",
                    ColumnSettings.Where(c => c.IsVisible).Select(c => c.Key)
                );

                await _receivingSettings.SaveStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeColumnVisibility,
                    visible,
                    userId
                );

                await _receivingSettings.SaveStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSearchByColumn,
                    SearchByColumnKey,
                    userId
                );

                await _receivingSettings.SaveStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSortColumn,
                    SortColumn,
                    userId
                );

                await _receivingSettings.SaveStringAsync(
                    ReceivingSettingsKeys.UiText.EditModeSortAscending,
                    SortAscending ? "true" : "false",
                    userId
                );

                await _receivingSettings.SaveStringAsync(
                    ReceivingSettingsKeys.UiText.EditModePageSize,
                    SelectedPageSize.ToString(),
                    userId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving column visibility: {ex.Message}", ex);
            }
        }

        /// <summary>Raises the event that tells the View to open the column-chooser dialog.</summary>
        [RelayCommand]
        private void ShowColumnChooser() =>
            ShowColumnChooserRequested?.Invoke(this, EventArgs.Empty);

        // ------------------------------------------------------------------ column definitions
        private static IEnumerable<Model_EditModeColumn> BuildAllColumns() =>
            [
                new()
                {
                    Key = "LoadNumber",
                    Header = "Load #",
                    IsAlwaysVisible = true,
                },
                new() { Key = "ReceivedDate", Header = "Received Date" },
                new()
                {
                    Key = "PartID",
                    Header = "Part ID",
                    IsAlwaysVisible = true,
                },
                new() { Key = "PartType", Header = "Part Type" },
                new() { Key = "PONumber", Header = "PO Number" },
                new() { Key = "POLineNumber", Header = "PO Line #" },
                new() { Key = "WeightQuantity", Header = "Weight/Qty" },
                new() { Key = "HeatLotNumber", Header = "Heat/Lot" },
                new() { Key = "InitialLocation", Header = "Location" },
                new() { Key = "RemainingQuantity", Header = "Remaining Qty" },
                new() { Key = "PackagesPerLoad", Header = "Pkgs/Load" },
                new() { Key = "PackageType", Header = "Pkg Type" },
                new() { Key = "WeightPerPackage", Header = "UOM/Pkg" },
                new() { Key = "IsNonPOItem", Header = "Non-PO?" },
                new() { Key = "UserId", Header = "User" },
                new() { Key = "EmployeeNumber", Header = "Employee #" },
                new() { Key = "IsQualityHoldRequired", Header = "QH Required?" },
                new() { Key = "IsQualityHoldAcknowledged", Header = "QH Acknowledged?" },
                new() { Key = "QualityHoldRestrictionType", Header = "QH Restriction" },
                new() { Key = "PartDescription", Header = "Part Description" },
                new() { Key = "UnitOfMeasure", Header = "UOM" },
                new() { Key = "QtyOrdered", Header = "Qty Ordered" },
                new() { Key = "POVendor", Header = "Vendor" },
                new() { Key = "POStatus", Header = "PO Status" },
                new() { Key = "PODueDate", Header = "PO Due Date" },
            ];

        private static bool IsDefaultVisible(string key) =>
            key
                is "LoadNumber"
                    or "ReceivedDate"
                    or "IsNonPOItem"
                    or "PartID"
                    or "PONumber"
                    or "WeightQuantity"
                    or "HeatLotNumber"
                    or "InitialLocation"
                    or "PackagesPerLoad"
                    or "PackageType"
                    or "WeightPerPackage";

        // ------------------------------------------------------------------ search
        [RelayCommand]
        private void ClearSearch() => SearchText = string.Empty;

        /// <summary>Called from the View's Sorting event handler to apply a column sort.</summary>
        /// <param name="columnKey">The column key to sort by.</param>
        /// <param name="ascending">True to sort ascending; false for descending.</param>
        internal void SortBy(string columnKey, bool ascending)
        {
            SortColumn = columnKey;
            SortAscending = ascending;
            FilterAndPaginate();
            PersistEditModePreferencesIfReady();
        }

        private void PersistEditModePreferencesIfReady()
        {
            if (_isLoadingEditModePreferences)
            {
                return;
            }

            _ = SaveColumnVisibilityAsync();
        }

        private static List<Model_ReceivingLoad> ApplySort(
            List<Model_ReceivingLoad> source,
            string column,
            bool ascending
        )
        {
            Func<List<Model_ReceivingLoad>, List<Model_ReceivingLoad>> sorter = column switch
            {
                "LoadNumber" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.LoadNumber).ToList()
                        : lst.OrderByDescending(l => l.LoadNumber).ToList(),
                "ReceivedDate" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.ReceivedDate).ToList()
                        : lst.OrderByDescending(l => l.ReceivedDate).ToList(),
                "PartID" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PartID, StringComparer.OrdinalIgnoreCase).ToList()
                        : lst.OrderByDescending(l => l.PartID, StringComparer.OrdinalIgnoreCase)
                            .ToList(),
                "PartType" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PartType).ToList()
                        : lst.OrderByDescending(l => l.PartType).ToList(),
                "PONumber" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PoNumber).ToList()
                        : lst.OrderByDescending(l => l.PoNumber).ToList(),
                "POLineNumber" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PoLineNumber).ToList()
                        : lst.OrderByDescending(l => l.PoLineNumber).ToList(),
                "WeightQuantity" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.WeightQuantity).ToList()
                        : lst.OrderByDescending(l => l.WeightQuantity).ToList(),
                "HeatLotNumber" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.HeatLotNumber, StringComparer.OrdinalIgnoreCase)
                            .ToList()
                        : lst.OrderByDescending(
                                l => l.HeatLotNumber,
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToList(),
                "InitialLocation" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.InitialLocation, StringComparer.OrdinalIgnoreCase)
                            .ToList()
                        : lst.OrderByDescending(
                                l => l.InitialLocation,
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToList(),
                "RemainingQuantity" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.RemainingQuantity).ToList()
                        : lst.OrderByDescending(l => l.RemainingQuantity).ToList(),
                "PackagesPerLoad" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PackagesPerLoad).ToList()
                        : lst.OrderByDescending(l => l.PackagesPerLoad).ToList(),
                "PackageType" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PackageTypeName).ToList()
                        : lst.OrderByDescending(l => l.PackageTypeName).ToList(),
                "WeightPerPackage" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.WeightPerPackage).ToList()
                        : lst.OrderByDescending(l => l.WeightPerPackage).ToList(),
                "IsNonPOItem" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.IsNonPOItem).ToList()
                        : lst.OrderByDescending(l => l.IsNonPOItem).ToList(),
                "UserId" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.UserId).ToList()
                        : lst.OrderByDescending(l => l.UserId).ToList(),
                "EmployeeNumber" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.EmployeeNumber).ToList()
                        : lst.OrderByDescending(l => l.EmployeeNumber).ToList(),
                "IsQualityHoldRequired" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.IsQualityHoldRequired).ToList()
                        : lst.OrderByDescending(l => l.IsQualityHoldRequired).ToList(),
                "IsQualityHoldAcknowledged" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.IsQualityHoldAcknowledged).ToList()
                        : lst.OrderByDescending(l => l.IsQualityHoldAcknowledged).ToList(),
                "QualityHoldRestrictionType" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.QualityHoldRestrictionType).ToList()
                        : lst.OrderByDescending(l => l.QualityHoldRestrictionType).ToList(),
                "PartDescription" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PartDescription, StringComparer.OrdinalIgnoreCase)
                            .ToList()
                        : lst.OrderByDescending(
                                l => l.PartDescription,
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToList(),
                "UnitOfMeasure" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.UnitOfMeasure).ToList()
                        : lst.OrderByDescending(l => l.UnitOfMeasure).ToList(),
                "QtyOrdered" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.QtyOrdered).ToList()
                        : lst.OrderByDescending(l => l.QtyOrdered).ToList(),
                "POVendor" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PoVendor, StringComparer.OrdinalIgnoreCase).ToList()
                        : lst.OrderByDescending(l => l.PoVendor, StringComparer.OrdinalIgnoreCase)
                            .ToList(),
                "POStatus" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PoStatus).ToList()
                        : lst.OrderByDescending(l => l.PoStatus).ToList(),
                "PODueDate" => lst =>
                    ascending
                        ? lst.OrderBy(l => l.PoDueDate).ToList()
                        : lst.OrderByDescending(l => l.PoDueDate).ToList(),
                _ => lst => lst,
            };

            return sorter(source);
        }

        /// <summary>Gets the text representation of the quarter for a given date.</summary>
        /// <param name="date">The date whose quarter label is needed.</param>
        private static string GetQuarterText(DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            return quarter switch
            {
                1 => "Jan-Mar",
                2 => "Apr-Jun",
                3 => "Jul-Sep",
                4 => "Oct-Dec",
                _ => "Quarter",
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

            ReplaceLoads(pageItems);

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
        private void Loads_CollectionChanged(
            object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
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
        private void Load_PropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (e.PropertyName == nameof(Model_ReceivingLoad.IsSelected))
            {
                RemoveRowCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnSelectedLoadChanged(Model_ReceivingLoad? value)
        {
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

        public void HandleCurrentLabelQueueCleared()
        {
            if (CurrentDataSource != Enum_DataSourceType.CurrentLabels)
            {
                return;
            }

            _logger.LogInfo(
                "Current label queue cleared while Edit Mode is displaying current labels. Clearing grid."
            );

            _deletedLoads.Clear();
            _allLoads.Clear();
            _filteredLoads = new List<Model_ReceivingLoad>();
            ReplaceLoads(Array.Empty<Model_ReceivingLoad>());
            ResetEmptyState();
            SelectedLoad = null;
            ResultSummary = "0 records";
            StatusMessage = "Current label queue cleared.";
            SelectAllButtonText = "Select All";

            _paginationService.SetSource(_filteredLoads);
            CurrentPage = _paginationService.CurrentPage;
            TotalPages = _paginationService.TotalPages;
            GotoPageNumber = CurrentPage;

            NotifyPaginationCommands();
            NotifyCommands();
        }

        partial void OnLoadsChanging(
            ObservableCollection<Model_ReceivingLoad>? oldValue,
            ObservableCollection<Model_ReceivingLoad> newValue
        )
        {
            if (oldValue is null)
            {
                return;
            }

            oldValue.CollectionChanged -= Loads_CollectionChanged;

            foreach (var item in oldValue)
            {
                item.PropertyChanged -= Load_PropertyChanged;
            }
        }

        partial void OnLoadsChanged(ObservableCollection<Model_ReceivingLoad> value)
        {
            value.CollectionChanged += Loads_CollectionChanged;

            foreach (var item in value)
            {
                item.PropertyChanged += Load_PropertyChanged;
            }

            NotifyCommands();
        }

        private void ReplaceLoads(IEnumerable<Model_ReceivingLoad> loads)
        {
            Loads = new ObservableCollection<Model_ReceivingLoad>(loads);
        }

        /// <summary>
        /// Handles changes to the filter start date.
        /// </summary>
        partial void OnFilterStartDateChanged(DateTimeOffset value)
        {
            if (!_isApplyingPresetFilter)
            {
                ClearActiveDateFilter();
            }

            ApplyDateFilter();
        }

        /// <summary>
        /// Handles changes to the filter end date.
        /// </summary>
        partial void OnFilterEndDateChanged(DateTimeOffset value)
        {
            if (!_isApplyingPresetFilter)
            {
                ClearActiveDateFilter();
            }

            ApplyDateFilter();
        }

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
        /// Filters the master list by date and search term, then updates pagination.
        /// </summary>
        private void FilterAndPaginate()
        {
            var start = FilterStartDate.Date;
            var end = FilterEndDate.Date.AddDays(1).AddTicks(-1);

            List<Model_ReceivingLoad> result = _allLoads
                .Where(l => l.ReceivedDate >= start && l.ReceivedDate <= end)
                .ToList();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                result = SearchByColumnKey switch
                {
                    "Part ID" => result
                        .Where(l => l.PartID.Contains(term, StringComparison.OrdinalIgnoreCase))
                        .ToList(),
                    "PO Number" => result
                        .Where(l =>
                            (l.PoNumber ?? string.Empty).Contains(
                                term,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        .ToList(),
                    "Heat/Lot" => result
                        .Where(l =>
                            l.HeatLotNumber.Contains(term, StringComparison.OrdinalIgnoreCase)
                        )
                        .ToList(),
                    "Employee #" => result
                        .Where(l => l.EmployeeNumber.ToString().Contains(term))
                        .ToList(),
                    "Part Description" => result
                        .Where(l =>
                            l.PartDescription.Contains(term, StringComparison.OrdinalIgnoreCase)
                        )
                        .ToList(),
                    "Vendor" => result
                        .Where(l => l.PoVendor.Contains(term, StringComparison.OrdinalIgnoreCase))
                        .ToList(),
                    _ => result
                        .Where(l =>
                            l.PartID.Contains(term, StringComparison.OrdinalIgnoreCase)
                            || (l.PoNumber ?? string.Empty).Contains(
                                term,
                                StringComparison.OrdinalIgnoreCase
                            )
                            || l.HeatLotNumber.Contains(term, StringComparison.OrdinalIgnoreCase)
                            || l.EmployeeNumber.ToString().Contains(term)
                            || l.PartDescription.Contains(term, StringComparison.OrdinalIgnoreCase)
                            || l.PoVendor.Contains(term, StringComparison.OrdinalIgnoreCase)
                        )
                        .ToList(),
                };
            }

            if (!string.IsNullOrEmpty(SortColumn))
            {
                result = ApplySort(result, SortColumn, SortAscending);
            }

            _filteredLoads = result;
            _paginationService.SetSource(_filteredLoads);

            ResultSummary = string.IsNullOrWhiteSpace(SearchText)
                ? $"{_filteredLoads.Count:N0} record{(_filteredLoads.Count == 1 ? "" : "s")}"
                : $"{_filteredLoads.Count:N0} of {_allLoads.Count:N0} record{(_allLoads.Count == 1 ? "" : "s")} matching \"{SearchText}\"";

            if (_allLoads.Count > 0)
            {
                _hasLoadedData = true;
            }

            EmptyStateMessage = string.IsNullOrWhiteSpace(SearchText)
                ? "No records found."
                : $"No records match \"{SearchText}\".";
            OnPropertyChanged(nameof(ShowEmptyState));
        }

        /// <summary>
        /// Sets the date filter to the last 7 days.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterLastWeekAsync()
        {
            ApplyPresetDateFilter(
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                DateFilterPreset.LastWeek
            );
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
            ApplyPresetDateFilter(DateTime.Today, DateTime.Today, DateFilterPreset.Today);
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
        /// Sets the date filter to yesterday.
        /// </summary>
        [RelayCommand]
        private async Task SetFilterYesterdayAsync()
        {
            ApplyPresetDateFilter(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(-1),
                DateFilterPreset.Yesterday
            );
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
            ApplyPresetDateFilter(start, end, DateFilterPreset.ThisWeek);
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
            var start = new DateTime(today.Year, today.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            ApplyPresetDateFilter(start, end, DateFilterPreset.ThisMonth);
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
            var start = new DateTime(today.Year, 3 * quarter - 2, 1);
            var end = start.AddMonths(3).AddDays(-1);
            ApplyPresetDateFilter(start, end, DateFilterPreset.ThisQuarter);
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
            ApplyPresetDateFilter(
                DateTime.Today.AddYears(-1),
                DateTime.Today,
                DateFilterPreset.ShowAll
            );
            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        [RelayCommand]
        private async Task SearchByDateAsync()
        {
            ClearActiveDateFilter();

            if (CurrentDataSource == Enum_DataSourceType.History)
            {
                await LoadFromHistoryAsync();
            }
            else
            {
                FilterAndPaginate();
            }
        }

        private void ApplyPresetDateFilter(DateTime startDate, DateTime endDate, DateFilterPreset preset)
        {
            _isApplyingPresetFilter = true;
            try
            {
                FilterStartDate = startDate;
                FilterEndDate = endDate;
                SetActiveDateFilter(preset);
            }
            finally
            {
                _isApplyingPresetFilter = false;
            }
        }

        private void ClearActiveDateFilter()
        {
            if (_activeDateFilter != DateFilterPreset.None)
            {
                SetActiveDateFilter(DateFilterPreset.None);
            }
        }

        private void SetActiveDateFilter(DateFilterPreset preset)
        {
            if (_activeDateFilter == preset)
            {
                return;
            }

            _activeDateFilter = preset;
            OnPropertyChanged(nameof(LastWeekFilterButtonBackground));
            OnPropertyChanged(nameof(TodayFilterButtonBackground));
            OnPropertyChanged(nameof(YesterdayFilterButtonBackground));
            OnPropertyChanged(nameof(ThisWeekFilterButtonBackground));
            OnPropertyChanged(nameof(ThisMonthFilterButtonBackground));
            OnPropertyChanged(nameof(ThisQuarterFilterButtonBackground));
            OnPropertyChanged(nameof(ShowAllFilterButtonBackground));
            OnPropertyChanged(nameof(DateRangeButtonBackground));
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
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }

                var visibleLoads = ApplyOwnershipVisibilityRules(currentLoads, "current session");
                if (visibleLoads.Count == 0)
                {
                    await _errorHandler.ShowErrorDialogAsync(
                        "No Editable Loads",
                        EditModeOwnershipRestrictionMessage,
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }

                _allLoads.Clear();
                foreach (var load in visibleLoads)
                {
                    _allLoads.Add(load);
                }

                StatusMessage =
                    visibleLoads.Count == currentLoads.Count
                        ? $"Loaded {_allLoads.Count} loads from current session"
                        : $"Loaded {visibleLoads.Count} of {currentLoads.Count} loads from current session. Rows created by other users are hidden.";
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
                await _errorHandler.HandleErrorAsync(
                    "Failed to load data from current session",
                    Enum_ErrorSeverity.Error,
                    ex
                );
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Loads data from the active label print queue (receiving_label_data table).
        /// </summary>
        [RelayCommand]
        private async Task LoadFromCurrentLabelsAsync()
        {
            try
            {
                _logger.LogInfo("User initiated Current Labels (DB) load");
                IsBusy = true;
                StatusMessage = "Loading current label queue from database...";
                ResetEmptyState();

                var result = await _mysqlService.GetCurrentLabelDataAsync();

                if (!result.IsSuccess)
                {
                    await _errorHandler.ShowErrorDialogAsync(
                        "Load Failed",
                        result.ErrorMessage ?? "Failed to load current label data from database.",
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }

                var loadedData =
                    result.Data ?? new System.Collections.Generic.List<Model_ReceivingLoad>();

                if (loadedData.Count == 0)
                {
                    ShowEmptyStateFor(
                        "The current label queue is empty. No labels have been printed today."
                    );
                    return;
                }

                var visibleLoads = ApplyOwnershipVisibilityRules(loadedData, "current label queue");
                if (visibleLoads.Count == 0)
                {
                    await _errorHandler.ShowErrorDialogAsync(
                        "No Editable Labels",
                        EditModeOwnershipRestrictionMessage,
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }

                _deletedLoads.Clear();
                _allLoads.Clear();
                foreach (var load in visibleLoads)
                {
                    _allLoads.Add(load);
                }
                StatusMessage =
                    visibleLoads.Count == loadedData.Count
                        ? $"Loaded {_allLoads.Count} loads from current label queue"
                        : $"Loaded {visibleLoads.Count} of {loadedData.Count} loads from current label queue. Rows created by other users are hidden.";
                _logger.LogInfo(
                    $"Successfully loaded {_allLoads.Count} loads from current label queue"
                );
                CurrentDataSource = Enum_DataSourceType.CurrentLabels;

                FilterStartDate = DateTimeOffset.Now.AddYears(-1);
                FilterEndDate = DateTimeOffset.Now;

                FilterAndPaginate();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from label queue: {ex.Message}");
                await _errorHandler.HandleErrorAsync(
                    "Failed to load data from label queue",
                    Enum_ErrorSeverity.Error,
                    ex
                );
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Attempts to load data from the default label-data location.
        /// </summary>
        private async Task<bool> TryLoadFromDefaultLabelDataAsync()
        {
            try
            {
                string networkPath = await _labelDataService.GetLabelDataPathAsync();
                if (File.Exists(networkPath))
                {
                    _logger.LogInfo($"Attempting to load from network labels: {networkPath}");
                    var loadedData = await _labelDataService.LoadLabelDataAsync(networkPath);

                    if (loadedData.Count > 0)
                    {
                        var visibleLoads = ApplyOwnershipVisibilityRules(
                            loadedData,
                            "network labels"
                        );
                        if (visibleLoads.Count == 0)
                        {
                            return false;
                        }

                        _deletedLoads.Clear();
                        _allLoads.Clear();
                        foreach (var load in visibleLoads)
                        {
                            _allLoads.Add(load);
                            _workflowService.CurrentSession.Loads.Add(load);
                        }
                        StatusMessage =
                            visibleLoads.Count == loadedData.Count
                                ? $"Loaded {_allLoads.Count} loads from network labels"
                                : $"Loaded {visibleLoads.Count} of {loadedData.Count} loads from network labels. Rows created by other users are hidden.";
                        _logger.LogInfo(
                            $"Successfully loaded {_allLoads.Count} loads from network labels"
                        );
                        CurrentDataSource = Enum_DataSourceType.CurrentLabels;
                        _currentLabelDataPath = networkPath;
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
                ResetEmptyState();

                var startDate = FilterStartDate.Date;
                var endDate = FilterEndDate.Date;

                _logger.LogInfo(
                    $"Loading receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
                );
                var result = await _mysqlService.GetAllReceivingLoadsAsync(startDate, endDate);

                if (!result.IsSuccess)
                {
                    await _errorHandler.HandleErrorAsync(
                        $"Failed to load from history: {result.ErrorMessage}",
                        Enum_ErrorSeverity.Error
                    );
                    return;
                }

                if (result.Data == null || result.Data.Count == 0)
                {
                    ShowEmptyStateFor(
                        "No receiving records found in the specified date range."
                    );
                    return;
                }

                var visibleLoads = ApplyOwnershipVisibilityRules(result.Data, "history");
                if (visibleLoads.Count == 0)
                {
                    await _errorHandler.ShowErrorDialogAsync(
                        "No Editable History",
                        EditModeOwnershipRestrictionMessage,
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }

                _deletedLoads.Clear();
                _allLoads.Clear();
                foreach (var load in visibleLoads)
                {
                    _allLoads.Add(load);
                    _workflowService.CurrentSession.Loads.Add(load);
                }

                StatusMessage =
                    visibleLoads.Count == result.Data.Count
                        ? $"Loaded {_allLoads.Count} loads from history"
                        : $"Loaded {visibleLoads.Count} of {result.Data.Count} loads from history. Rows created by other users are hidden.";
                _logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from history");
                CurrentDataSource = Enum_DataSourceType.History;
                SelectAllButtonText = "Select All";

                FilterAndPaginate();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load from history: {ex.Message}");
                await _errorHandler.HandleErrorAsync(
                    "Failed to load data from history",
                    Enum_ErrorSeverity.Error,
                    ex
                );
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
        private async Task RemoveRowAsync()
        {
            var selectedLoads = Loads.Where(l => l.IsSelected).ToList();

            if (selectedLoads.Count > 0)
            {
                if (!await EnsureCurrentUserCanManageLoadsAsync(selectedLoads, "remove"))
                {
                    return;
                }

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
                if (!await EnsureCurrentUserCanManageLoadsAsync(new[] { SelectedLoad }, "remove"))
                {
                    return;
                }

                _logger.LogInfo(
                    $"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})"
                );
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
                _logger.LogInfo(
                    $"Validating and saving {_filteredLoads.Count} loads from edit mode"
                );
                IsBusy = true;
                StatusMessage = "Validating loads...";

                if (
                    !await EnsureCurrentUserCanManageLoadsAsync(
                        _filteredLoads.Concat(_deletedLoads),
                        "save"
                    )
                )
                {
                    return;
                }

                // Set default Heat/Lot if empty for all loads being processed
                // We update _allLoads to ensure consistency across all data sources
                foreach (var load in _allLoads)
                {
                    if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                    {
                        load.HeatLotNumber = "Nothing Entered";
                    }

                    // Check for quality hold requirement on each load
                    if (!string.IsNullOrWhiteSpace(load.PartID))
                    {
                        var (isRestricted, restrictionType) =
                            await _validationService.IsRestrictedPartAsync(load.PartID);
                        if (isRestricted)
                        {
                            load.IsQualityHoldRequired = true;
                            load.QualityHoldRestrictionType = restrictionType;
                        }
                    }
                }

                // Check if any loads have quality holds that haven't been acknowledged yet
                var loadsWithUnacknowledgedHolds = _allLoads
                    .Where(l => l.IsQualityHoldRequired && !l.IsQualityHoldAcknowledged)
                    .ToList();

                if (loadsWithUnacknowledgedHolds.Count > 0)
                {
                    // Show confirmation dialog for quality hold acknowledgment
                    var acknowledged = await ShowQualityHoldConfirmationAsync(
                        loadsWithUnacknowledgedHolds
                    );
                    if (!acknowledged)
                    {
                        _logger.LogInfo("User cancelled save due to unacknowledged quality holds");
                        return; // Block save if user doesn't acknowledge
                    }

                    // Mark all as acknowledged
                    foreach (var load in loadsWithUnacknowledgedHolds)
                    {
                        load.IsQualityHoldAcknowledged = true;
                    }
                }

                var validationErrors = ValidateLoads(_filteredLoads, CurrentDataSource);
                if (validationErrors.Count > 0)
                {
                    var errorMessage = string.Join("\n", validationErrors);
                    _logger.LogWarning(
                        $"Edit mode validation failed: {validationErrors.Count} errors"
                    );
                    await _errorHandler.HandleErrorAsync(
                        $"Validation failed:\n{errorMessage}",
                        Enum_ErrorSeverity.Warning
                    );
                    return;
                }

                StatusMessage = "Saving data...";

                switch (CurrentDataSource)
                {
                    case Enum_DataSourceType.Memory:
                        await _workflowService.AdvanceToNextStepAsync();
                        break;

                    case Enum_DataSourceType.CurrentLabels:
                    {
                        // Current Labels are loaded from the DB queue (receiving_label_data).
                        // Removed rows are deleted and remaining rows are updated.
                        int requestedLabelUpdates = _filteredLoads.Count;
                        int requestedLabelDeletes = _deletedLoads.Count;

                        _logger.LogInfo("Updating current label queue records");
                        int labelDeleted = 0;
                        if (_deletedLoads.Count > 0)
                        {
                            _logger.LogInfo(
                                $"Deleting {_deletedLoads.Count} removed label queue records"
                            );
                            labelDeleted = await _mysqlService.DeleteCurrentLabelDataAsync(
                                _deletedLoads
                            );
                        }

                        int labelUpdated = 0;
                        if (_filteredLoads.Count > 0)
                        {
                            labelUpdated = await _mysqlService.UpdateCurrentLabelDataAsync(
                                _filteredLoads
                            );
                        }

                        var displayedLabelUpdated = Math.Max(labelUpdated, requestedLabelUpdates);
                        var displayedLabelDeleted = Math.Max(labelDeleted, requestedLabelDeletes);

                        StatusMessage =
                            $"Label queue updated ({displayedLabelUpdated} updated, {displayedLabelDeleted} deleted)";
                        await _errorHandler.ShowErrorDialogAsync(
                            "Success",
                            $"Label queue updated successfully.\n{displayedLabelUpdated} label record(s) updated.\n{displayedLabelDeleted} label record(s) deleted.",
                            Enum_ErrorSeverity.Info
                        );
                        break;
                    }

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
                        await _errorHandler.ShowErrorDialogAsync(
                            "Success",
                            $"History updated successfully.\n{updated} records updated.\n{deleted} records deleted.",
                            Enum_ErrorSeverity.Info
                        );
                        break;
                }

                _logger.LogInfo(
                    $"Edit mode save completed successfully for source: {CurrentDataSource}"
                );
                _deletedLoads.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save edit mode data: {ex.Message}");
                await _errorHandler.HandleErrorAsync(
                    "Failed to save receiving data",
                    Enum_ErrorSeverity.Critical,
                    ex
                );
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

        private bool CanReprintFromHistory() =>
            CurrentDataSource == Enum_DataSourceType.History
            && SelectedLoad?.HistoryRecordID.HasValue == true
            && !IsBusy;

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
                await _errorHandler.HandleErrorAsync(
                    "Unable to display dialog",
                    Enum_ErrorSeverity.Error
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
                    _logger.LogInfo("User confirmed return to mode selection, resetting workflow");
                    await _workflowService.ResetWorkflowAsync();
                    _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to reset workflow: {ex.Message}");
                    await _errorHandler.HandleErrorAsync(
                        "Failed to return to mode selection",
                        Enum_ErrorSeverity.Error,
                        ex
                    );
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
        /// <param name="loadsToValidate">The loads to validate.</param>
        /// <param name="dataSource">The data source type, used to adjust validation rules.</param>
        private System.Collections.Generic.List<string> ValidateLoads(
            IEnumerable<Model_ReceivingLoad> loadsToValidate,
            Enum_DataSourceType dataSource = Enum_DataSourceType.Memory
        )
        {
            var errors = new System.Collections.Generic.List<string>();

            if (!loadsToValidate.Any() && _deletedLoads.Count == 0)
            {
                errors.Add("No loads to save");
                return errors;
            }

            // History and CurrentLabels records may legitimately have blank PartID (non-PO items)
            // or zero PackagesPerLoad (legacy data entered before this field was added).
            bool isEditingStoredRecords =
                dataSource == Enum_DataSourceType.History
                || dataSource == Enum_DataSourceType.CurrentLabels;

            foreach (var load in loadsToValidate)
            {
                if (!isEditingStoredRecords && string.IsNullOrWhiteSpace(load.PartID))
                {
                    errors.Add($"Load #{load.LoadNumber}: Part ID is required");
                }

                if (load.WeightQuantity <= 0)
                {
                    errors.Add(
                        $"Load #{load.LoadNumber}: Weight/Quantity must be greater than zero"
                    );
                }

                if (!isEditingStoredRecords && load.PackagesPerLoad <= 0)
                {
                    errors.Add(
                        $"Load #{load.LoadNumber}: Packages per load must be greater than zero"
                    );
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

        #region Quality Hold Confirmation

        /// <summary>
        /// Shows a confirmation dialog for quality hold acknowledgment before allowing save.
        /// This is the SECOND and FINAL acknowledgment required.
        /// </summary>
        /// <param name="loadsWithHolds">List of loads requiring quality acknowledgment</param>
        /// <returns>True if user acknowledges; false if cancelled</returns>
        private async Task<bool> ShowQualityHoldConfirmationAsync(
            List<Model_ReceivingLoad> loadsWithHolds
        )
        {
            ArgumentNullException.ThrowIfNull(loadsWithHolds);
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show quality hold dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync(
                    "Unable to display dialog",
                    Enum_ErrorSeverity.Error
                );
                return false;
            }

            // Build list of restricted parts
            var restrictedPartsList = string.Join(
                "\n",
                loadsWithHolds.Select(l => $"  • {l.PartID} ({l.QualityHoldRestrictionType})")
            );

            var content =
                $"⚠️ FINAL QUALITY HOLD CONFIRMATION ⚠️\n\n"
                + $"This is your SECOND and FINAL acknowledgment.\n\n"
                + $"The following parts require quality hold:\n\n{restrictedPartsList}\n\n"
                + $"BEFORE YOU PROCEED:\n"
                + $"✓ Have you contacted Quality?\n"
                + $"✓ Has Quality physically inspected these loads?\n"
                + $"✓ Has Quality accepted these loads?\n"
                + $"✓ Are you ready to save WITHOUT signing paperwork?\n\n"
                + $"This is a critical quality control checkpoint.\n"
                + $"DO NOT proceed unless Quality has accepted.";

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "⚠️ FINAL QUALITY HOLD CONFIRMATION - Action Required",
                Content = new Microsoft.UI.Xaml.Controls.TextBlock
                {
                    Text = content,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                    FontSize = 14,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.DarkRed
                    ),
                },
                PrimaryButtonText = "✓ YES - Quality Has Accepted - Save Now",
                CloseButtonText = "✗ NO - Cancel Save",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                XamlRoot = xamlRoot,
            };

            var result = await dialog.ShowAsync().AsTask();
            return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
        }

        #endregion
    }
}

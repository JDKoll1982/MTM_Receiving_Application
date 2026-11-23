using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels;

/// <summary>
/// The By Receiver list: the newest inventory receipts from Infor Visual, searched, paged, and
/// chosen to start the guided walk at Load Entry.
/// </summary>
/// <remarks>
/// The query reads one bounded window on entry and on refresh. Search and paging run over that
/// window in memory, so the pager always reflects the filtered count and typing never waits on the
/// source. The chosen row is written to <see cref="IService_ReceivingWorkflow"/> before the step
/// changes, because Load Entry reads the PO number and the part from it.
/// </remarks>
public partial class ViewModel_Receiving_ReceiverSelection
    : ViewModel_Receiving_StepBase,
        IResettableViewModel
{
    /// <summary>
    /// How many rows one read returns. The list is a working surface over recent receipts, so a
    /// bounded window is the requirement rather than a shortcut.
    /// </summary>
    /// <remarks>
    /// simplified: one 1000-row window is read and the search and page slice run over it in memory;
    /// move the search term and a page parameter into the query when the window stops covering the
    /// operator's history.
    /// </remarks>
    private const int RecentReceiverWindowSize = 1000;

    private readonly IService_ReceivingWorkflow _workflowService;
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_Pagination _paginationService;
    private readonly IService_ReceivingSettings _receivingSettings;

    private List<Model_InforVisualReceiverRow> _window = new();
    private bool _isLoading;
    private bool _hasLoaded;

    [ObservableProperty]
    private ObservableCollection<Model_InforVisualReceiverRow> _rows = new();

    [ObservableProperty]
    private Model_InforVisualReceiverRow? _selectedRow;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _pageSize = 50;

    [ObservableProperty]
    private bool _isFailed;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isChosen;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // UI text, loaded from settings so every string on the screen has one owner.
    [ObservableProperty]
    private string _searchPlaceholderText = "Search by receiver number";

    [ObservableProperty]
    private string _rowsPerPageText = "Rows per page";

    [ObservableProperty]
    private string _refreshText = "Refresh";

    [ObservableProperty]
    private string _previousPageText = "Previous page";

    [ObservableProperty]
    private string _nextPageText = "Next page";

    [ObservableProperty]
    private string _pageIndicatorText = "Page {0} of {1}";

    [ObservableProperty]
    private string _emptyMessageText = "No receiver matched the search.";

    [ObservableProperty]
    private string _failureMessageText = "The recent receivers could not be loaded.";

    [ObservableProperty]
    private string _retryText = "Retry";

    [ObservableProperty]
    private string _columnReceiverNumberText = "Receiver Number";

    [ObservableProperty]
    private string _columnPoNumberText = "PO #";

    [ObservableProperty]
    private string _columnLineNumberText = "Line #";

    [ObservableProperty]
    private string _columnPartNumberText = "Part Number";

    [ObservableProperty]
    private string _columnVendorText = "Vendor";

    [ObservableProperty]
    private string _columnLocationReceivedToText = "Location Received to";

    [ObservableProperty]
    private string _columnQtyReceivedText = "Qty Received";

    [ObservableProperty]
    private string _columnQtyOrderedText = "Qty Ordered";

    [ObservableProperty]
    private string _columnQtyRemainingText = "Qty Remaining on Line";

    [ObservableProperty]
    private string _searchAccessibilityName = "Receiver number search";

    [ObservableProperty]
    private string _rowsPerPageAccessibilityName = "Rows per page";

    [ObservableProperty]
    private string _listAccessibilityName = "Recent receivers";

    /// <summary>
    /// Gets the row counts the page-size control offers, in the order it shows them.
    /// </summary>
    public ObservableCollection<int> PageSizes { get; } = [25, 50, 100, 200];

    /// <summary>Gets a value indicating whether a read is in flight.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether the mode is open with rows on screen.</summary>
    public bool IsLoaded => _hasLoaded && !IsFailed && !IsEmpty && !IsChosen;

    /// <summary>Gets a value indicating whether the mode has not been opened yet.</summary>
    public bool IsIdle => !_hasLoaded && !_isLoading;

    /// <summary>Gets a value indicating whether the page holds at least one row.</summary>
    public bool HasRows => Rows.Count > 0;

    /// <summary>
    /// Gets the pager's page number. The controls stay present when everything fits on one page;
    /// they are simply inactive.
    /// </summary>
    public int CurrentPage => _paginationService.CurrentPage;

    /// <summary>Gets how many pages the filtered list spans.</summary>
    public int TotalPages => Math.Max(1, _paginationService.TotalPages);

    /// <summary>Gets how many rows the search matched.</summary>
    public int TotalItems => _paginationService.TotalItems;

    /// <summary>Gets a value indicating whether a previous page exists.</summary>
    public bool CanGoPreviousPage => _paginationService.HasPreviousPage;

    /// <summary>Gets a value indicating whether a next page exists.</summary>
    public bool CanGoNextPage => _paginationService.HasNextPage;

    /// <summary>Gets the page indicator, such as "Page 2 of 7".</summary>
    public string PageIndicator =>
        string.Format(
            string.IsNullOrWhiteSpace(PageIndicatorText) ? "Page {0} of {1}" : PageIndicatorText,
            CurrentPage,
            TotalPages
        );

    public ViewModel_Receiving_ReceiverSelection(
        IService_ReceivingWorkflow workflowService,
        IService_InforVisual inforVisualService,
        IService_Pagination paginationService,
        IService_ReceivingSettings receivingSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService =
            workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _paginationService =
            paginationService ?? throw new ArgumentNullException(nameof(paginationService));
        _receivingSettings =
            receivingSettings ?? throw new ArgumentNullException(nameof(receivingSettings));

        _paginationService.PageSize = PageSize;
        _workflowService.StepChanged += OnStepChanged;

        _ = LoadTextAsync();

        // The stored default can open the mode before this view model exists, in which case the
        // step-change event has already fired and only this first read reaches the source.
        if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.ReceiverSelection)
        {
            _ = LoadAsync();
        }
    }

    private async Task LoadTextAsync()
    {
        try
        {
            SearchPlaceholderText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.SearchPlaceholder
            );
            RowsPerPageText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.RowsPerPage
            );
            RefreshText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.Refresh
            );
            PreviousPageText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.PreviousPage
            );
            NextPageText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.NextPage
            );
            PageIndicatorText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.PageIndicator
            );
            EmptyMessageText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.EmptyMessage
            );
            FailureMessageText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.FailureMessage
            );
            RetryText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.Retry
            );
            SearchAccessibilityName = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.Accessibility.ReceiverSelectionSearch
            );
            RowsPerPageAccessibilityName = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.Accessibility.ReceiverSelectionRowsPerPage
            );
            ListAccessibilityName = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.Accessibility.ReceiverSelectionList
            );

            ColumnReceiverNumberText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnReceiverNumber
            );
            ColumnPoNumberText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnPoNumber
            );
            ColumnLineNumberText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnLineNumber
            );
            ColumnPartNumberText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnPartNumber
            );
            ColumnVendorText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnVendor
            );
            ColumnLocationReceivedToText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnLocationReceivedTo
            );
            ColumnQtyReceivedText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnQtyReceived
            );
            ColumnQtyOrderedText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnQtyOrdered
            );
            ColumnQtyRemainingText = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.ReceiverSelection.ColumnQtyRemaining
            );

            var configuredPageSize = await _receivingSettings.GetIntAsync(
                ReceivingSettingsKeys.ReceiverSelection.DefaultPageSize
            );
            if (PageSizes.Contains(configuredPageSize))
            {
                PageSize = configuredPageSize;
            }

            NotifyPaginationChanged();
        }
        catch (Exception ex)
        {
            // The screen keeps the defaults already set on the properties above.
            _logger.LogError($"Error loading receiver list text from settings: {ex.Message}", ex);
        }
    }

    private void OnStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep != Enum_ReceivingWorkflowStep.ReceiverSelection)
        {
            return;
        }

        _ = LoadAsync();
    }

    /// <summary>
    /// Reads the source again. This is both the first read when the mode opens and the Refresh
    /// action, and it never leaves the mode.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();

    /// <summary>Issues a second read after a failure.</summary>
    [RelayCommand]
    private async Task RetryAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(IsIdle));
        IsFailed = false;
        IsChosen = false;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _inforVisualService.GetRecentReceiversAsync(
                RecentReceiverWindowSize
            );

            if (!result.IsSuccess || result.Data is null)
            {
                _window = new List<Model_InforVisualReceiverRow>();
                ReportFailure(result.ErrorMessage);
                return;
            }

            _window = result.Data;
            _hasLoaded = true;
            ApplySource();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading recent receivers: {ex.Message}", ex);
            _window = new List<Model_InforVisualReceiverRow>();
            ReportFailure(ex.Message);
        }
        finally
        {
            _isLoading = false;
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsIdle));
            OnPropertyChanged(nameof(IsLoaded));
        }
    }

    private void ReportFailure(string? message)
    {
        // A blank failure message would leave the surface looking broken, so the readable
        // fallback stands in for it.
        ErrorMessage = string.IsNullOrWhiteSpace(message) ? FailureMessageText : message;
        IsFailed = true;
        IsEmpty = false;
        _hasLoaded = true;
        ApplySource();
        ShowStatus(ErrorMessage, InfoBarSeverity.Warning);
        OnPropertyChanged(nameof(IsLoaded));
    }

    /// <summary>
    /// Re-applies the search to the read window and redisplays from the first page, then moves to
    /// the page that holds the row the walk was started from.
    /// </summary>
    private void ApplySource()
    {
        var filtered = ApplySearch(_window).ToList();
        _paginationService.SetSource(filtered);
        MoveToSelectedRowPage(filtered);
        RefreshPageRows();
    }

    /// <summary>
    /// Puts the page holding the row the walk was started from in front, so Back out of Load Entry
    /// lands on that row rather than on page one with nothing highlighted.
    /// </summary>
    private void MoveToSelectedRowPage(IReadOnlyList<Model_InforVisualReceiverRow> rows)
    {
        var key = _workflowService.SelectedReceiverKey;

        if (string.IsNullOrWhiteSpace(key) || _paginationService.PageSize <= 0)
        {
            return;
        }

        for (var index = 0; index < rows.Count; index++)
        {
            if (string.Equals(rows[index].SelectionKey, key, StringComparison.Ordinal))
            {
                _paginationService.GoToPage((index / _paginationService.PageSize) + 1);
                return;
            }
        }
    }

    private IEnumerable<Model_InforVisualReceiverRow> ApplySearch(
        IEnumerable<Model_InforVisualReceiverRow> rows
    )
    {
        var term = SearchText?.Trim() ?? string.Empty;

        if (term.Length == 0)
        {
            return rows;
        }

        return rows.Where(row =>
            (row.ReceiverNumber ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase)
        );
    }

    private void RefreshPageRows()
    {
        Rows = new ObservableCollection<Model_InforVisualReceiverRow>(
            _paginationService.GetCurrentPageItems<Model_InforVisualReceiverRow>()
        );

        IsEmpty = _hasLoaded && !IsFailed && _paginationService.TotalItems == 0;
        RestoreHighlight();
        NotifyPaginationChanged();
        OnPropertyChanged(nameof(HasRows));
        OnPropertyChanged(nameof(IsLoaded));
    }

    /// <summary>
    /// Puts the row the walk was started from back on the highlight when the list is shown again.
    /// </summary>
    private void RestoreHighlight()
    {
        var key = _workflowService.SelectedReceiverKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            SelectedRow = null;
            return;
        }

        SelectedRow =
            Rows.FirstOrDefault(row => string.Equals(row.SelectionKey, key, StringComparison.Ordinal))
            ?? _window.FirstOrDefault(row =>
                string.Equals(row.SelectionKey, key, StringComparison.Ordinal)
            );
    }

    private void NotifyPaginationChanged()
    {
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
        OnPropertyChanged(nameof(PageIndicator));
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!_hasLoaded || IsFailed)
        {
            return;
        }

        // The search runs on the already-filtered window, so it can never surface a receiver the
        // inventory or removal rules exclude.
        ApplySource();
    }

    partial void OnPageSizeChanged(int value)
    {
        if (value < 1)
        {
            return;
        }

        // The pagination service resets to the first page whenever the size changes.
        _paginationService.PageSize = value;
        RefreshPageRows();
    }

    /// <summary>
    /// Moves back one page of the filtered list.
    /// </summary>
    [RelayCommand]
    private void GoToPreviousPage()
    {
        if (_paginationService.PreviousPage())
        {
            RefreshPageRows();
        }
    }

    /// <summary>
    /// Moves forward one page of the filtered list.
    /// </summary>
    [RelayCommand]
    private void GoToNextPage()
    {
        if (_paginationService.NextPage())
        {
            RefreshPageRows();
        }
    }

    /// <summary>
    /// Carries the chosen row into the workflow state and moves on to Load Entry.
    /// </summary>
    /// <param name="row">The row the operator picked.</param>
    [RelayCommand]
    private void SelectRow(Model_InforVisualReceiverRow? row)
    {
        if (row is null)
        {
            return;
        }

        SelectedRow = row;

        // Load Entry reads the PO number and the part when it generates the loads, so both are
        // in place before the step changes.
        _workflowService.SelectedReceiver = row;
        _workflowService.CurrentPONumber = row.PONumber;
        _workflowService.CurrentPart = new Model_InforVisualPart
        {
            PartID = row.PartNumber,
            POLineNumber = row.POLineNumber,
            QtyOrdered = row.QtyOrdered,
            RemainingQuantity = (int)Math.Max(0m, Math.Round(row.QtyRemaining)),
            DefaultLocationId = row.LocationReceivedTo,
            Description = row.PartDescription,
            UnitOfMeasure = string.IsNullOrWhiteSpace(row.UnitOfMeasure)
                ? "EA"
                : row.UnitOfMeasure,
        };
        _workflowService.CurrentLocation = row.LocationReceivedTo;

        // The Heat/Lot page looks the vendor's custom variable column up by PO vendor name, so the
        // row's vendor has to be in place or that column disappears for a by-receiver walk.
        _workflowService.CurrentPOVendor = row.VendorName;

        IsChosen = true;
        OnPropertyChanged(nameof(IsLoaded));

        _workflowService.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry);
    }

    /// <summary>
    /// Clears the list state for a new receipt. The next visit to the mode reads the source again.
    /// </summary>
    public void ResetToDefaults()
    {
        _hasLoaded = false;
        _window = new List<Model_InforVisualReceiverRow>();
        _paginationService.SetSource(new List<Model_InforVisualReceiverRow>());
        SearchText = string.Empty;
        SelectedRow = null;
        IsFailed = false;
        IsEmpty = false;
        IsChosen = false;
        ErrorMessage = string.Empty;
        Rows = new ObservableCollection<Model_InforVisualReceiverRow>();
        NotifyPaginationChanged();
        OnPropertyChanged(nameof(HasRows));
        OnPropertyChanged(nameof(IsIdle));
        OnPropertyChanged(nameof(IsLoaded));
    }
}

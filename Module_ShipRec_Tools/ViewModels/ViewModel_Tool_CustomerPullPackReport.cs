using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Customer Pull n' Pack report page.
/// </summary>
public partial class ViewModel_Tool_CustomerPullPackReport : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_Notification _notificationService;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _dateFrom = new(DateTime.Today);

    [ObservableProperty]
    private DateTimeOffset _dateTo = new(DateTime.Today.AddDays(7));

    [ObservableProperty]
    private bool _showShortagesOnly;

    [ObservableProperty]
    private bool _showUnpulledOnly;

    [ObservableProperty]
    private bool _showLateOrdersOnly;

    [ObservableProperty]
    private string _partOrOrderSearchText = string.Empty;

    [ObservableProperty]
    private Enum_CustomerPullPackSortMode _selectedSortMode =
        Enum_CustomerPullPackSortMode.PullDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDemandLines))]
    [NotifyPropertyChangedFor(nameof(IsEmptyStateVisible))]
    [NotifyPropertyChangedFor(nameof(IsSelectionVisible))]
    private ObservableCollection<Model_CustomerPullPack_DemandLine> _demandLines = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectionVisible))]
    private Model_CustomerPullPack_DemandLine? _selectedDemandLine;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyStateVisible))]
    private bool _hasAttemptedLoad;

    [ObservableProperty]
    private string _activeCustomerId = string.Empty;

    [ObservableProperty]
    private string _activeCustomerName = string.Empty;

    [ObservableProperty]
    private int _shortageLineCount;

    [ObservableProperty]
    private int _lateOrderLineCount;

    [ObservableProperty]
    private int _linkedWaitlistLineCount;

    private string _duplicateWaitlistId = string.Empty;

    public Func<
        ViewModel_Dialog_CustomerPullPackWaitlistEditor,
        Task<bool>
    >? ShowWaitlistEditorAsync { get; set; }

    public IReadOnlyList<Enum_CustomerPullPackSortMode> SortModes { get; } =
        Enum.GetValues<Enum_CustomerPullPackSortMode>();

    public bool HasDemandLines => DemandLines.Count > 0;

    public bool IsEmptyStateVisible =>
        HasAttemptedLoad && IsBusy is false && HasDemandLines is false;

    public bool IsSelectionVisible => SelectedDemandLine is not null;

    public string CurrentCustomerDisplay =>
        string.IsNullOrWhiteSpace(ActiveCustomerId) ? "Select a customer and refresh the report."
        : string.IsNullOrWhiteSpace(ActiveCustomerName) ? ActiveCustomerId
        : $"{ActiveCustomerId} - {ActiveCustomerName}";

    public string EmptyStateTitle =>
        string.IsNullOrWhiteSpace(ActiveCustomerId)
            ? "No customer selected yet."
            : $"No open demand found for {ActiveCustomerId} in the selected range.";

    public string EmptyStateMessage =>
        string.IsNullOrWhiteSpace(ActiveCustomerId)
            ? "Enter a customer ID or customer display string, set the date range, and refresh the report."
            : "The customer selection is valid, but there are no rows to display for the current date range and filters. Choose another customer, widen the date range, or refresh if data was expected.";

    public string LinkedWaitlistSummary =>
        LinkedWaitlistLineCount == 0
            ? "No rows linked to waitlist"
            : $"{LinkedWaitlistLineCount} rows linked to waitlist";

    public string SelectionSummary =>
        $"{DemandLines.Count(static line => line.IsSelected)} selected lines";

    public string SelectedParentPartId => SelectedDemandLine?.ParentPartId ?? string.Empty;

    public string SelectedCustomerOrderId => SelectedDemandLine?.CustomerOrderId ?? string.Empty;

    public string SelectedCustomerName => SelectedDemandLine?.CustomerName ?? string.Empty;

    public string SelectedPullDateDisplay =>
        SelectedDemandLine is null
            ? string.Empty
            : SelectedDemandLine.PullDate.ToString("MM/dd/yyyy");

    public string SelectedFgLocationId => SelectedDemandLine?.FgLocationId ?? string.Empty;

    public string SelectedSubPartAvailabilitySummary =>
        SelectedDemandLine?.SubPartAvailabilitySummary ?? string.Empty;

    public string SelectedRequesterNote => SelectedDemandLine?.RequesterNote ?? string.Empty;

    public IReadOnlyList<Model_CustomerPullPack_LocationOption> SelectedDemandLineLocationOptions =>
        SelectedDemandLine?.LocationOptions ?? [];

    public bool SelectedDemandLineHasSelectableLocations =>
        SelectedDemandLineLocationOptions.Count > 0;

    public string SelectedDemandLineLocationSummary =>
        SelectedDemandLine is null
            ? string.Empty
            : string.Join(
                ", ",
                SelectedDemandLineLocationOptions
                    .Where(static option => option.Selected)
                    .Select(static option => option.LocationId)
            );

    public ViewModel_Tool_CustomerPullPackReport(
        IMediator mediator,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(sessionManager);
        _mediator = mediator;
        _notificationService = notificationService;
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// Shows the shared guidance message for the Customer Pull n' Pack tool when it becomes active.
    /// </summary>
    public void ActivateView()
    {
        ShowStatus(
            "Choose a customer, adjust filters if needed, and refresh the report.",
            InfoBarSeverity.Informational
        );
    }

    [RelayCommand]
    private async Task RefreshReportAsync()
    {
        var customerId = ParseCustomerId(CustomerSearchText);
        var customerName = ParseCustomerName(CustomerSearchText);

        if (string.IsNullOrWhiteSpace(customerId))
        {
            ShowStatus(
                "Customer ID is required before loading the report.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var startDate = DateFrom.Date;
        var endDate = DateTo.Date;

        if (startDate > endDate)
        {
            ShowStatus(
                "The start date cannot be later than the end date.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus(
                $"Loading Customer Pull n' Pack demand for {customerId}...",
                InfoBarSeverity.Informational
            );

            var filter = new Model_CustomerPullPack_DemandFilter
            {
                CustomerId = customerId,
                CustomerName = customerName,
                DateFrom = startDate,
                DateTo = endDate,
                ShortagesOnly = ShowShortagesOnly,
                UnpulledOnly = ShowUnpulledOnly,
                LateOrdersOnly = ShowLateOrdersOnly,
                SortMode = SelectedSortMode,
                SearchText = PartOrOrderSearchText.Trim(),
            };

            var result = await _mediator.Send(new Query_CustomerPullPackReport(filter));
            HasAttemptedLoad = true;

            if (!result.IsSuccess || result.Data is null)
            {
                ReplaceDemandLines(Array.Empty<Model_CustomerPullPack_DemandLine>());
                ClearDuplicateNotice();
                ActiveCustomerId = customerId;
                ActiveCustomerName = customerName;
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ReplaceDemandLines(result.Data);
            ClearDuplicateNotice();
            ActiveCustomerId = customerId;
            ActiveCustomerName = result.Data.FirstOrDefault()?.CustomerName ?? customerName;

            if (DemandLines.Count == 0)
            {
                ShowStatus(
                    $"No open demand found for {ActiveCustomerId} in the selected range.",
                    InfoBarSeverity.Informational
                );
                return;
            }

            ShowStatus(
                $"Loaded {DemandLines.Count} demand lines for {ActiveCustomerId}.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RefreshReportAsync),
                nameof(ViewModel_Tool_CustomerPullPackReport)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearReportFilters()
    {
        PartOrOrderSearchText = string.Empty;
        ShowShortagesOnly = false;
        ShowUnpulledOnly = false;
        ShowLateOrdersOnly = false;
        SelectedSortMode = Enum_CustomerPullPackSortMode.PullDate;

        if (HasAttemptedLoad && string.IsNullOrWhiteSpace(CustomerSearchText) is false)
        {
            ShowStatus(
                "Report filters reset. Refresh to load the default view again.",
                InfoBarSeverity.Informational
            );
            return;
        }

        ShowStatus("Report filters reset.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private void ResetCustomerSelection()
    {
        CustomerSearchText = string.Empty;
        ActiveCustomerId = string.Empty;
        ActiveCustomerName = string.Empty;
        HasAttemptedLoad = false;
        ReplaceDemandLines(Array.Empty<Model_CustomerPullPack_DemandLine>());
        ClearDuplicateNotice();
        ShowStatus("Customer selection cleared.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task CreateOrUpdateWaitlistAsync()
    {
        var selectedLines = DemandLines.Where(static line => line.IsSelected).ToList();
        if (selectedLines.Count == 0)
        {
            ShowStatus(
                "Select at least one compatible report line before opening the waitlist editor.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var parentPartIds = selectedLines
            .Select(static line => line.ParentPartId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (parentPartIds.Count > 1)
        {
            ShowStatus(
                "Select lines for one parent part at a time before creating or updating waitlist work.",
                InfoBarSeverity.Warning
            );
            return;
        }

        await OpenWaitlistEditorForLinesAsync(selectedLines);
    }

    [RelayCommand]
    private async Task ReopenDuplicateWaitlistAsync()
    {
        if (string.IsNullOrWhiteSpace(_duplicateWaitlistId))
        {
            return;
        }

        var linkedWaitlistResult = await _mediator.Send(
            new Query_CustomerPullPackLinkedWaitlist(_duplicateWaitlistId)
        );
        if (!linkedWaitlistResult.IsSuccess || linkedWaitlistResult.Data is null)
        {
            ShowStatus(linkedWaitlistResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        var matchingLine = DemandLines.FirstOrDefault(line =>
            string.Equals(
                line.SourceLineKey,
                linkedWaitlistResult.Data.SourceLineKey,
                StringComparison.OrdinalIgnoreCase
            )
        );
        if (matchingLine is null)
        {
            ShowStatus(
                "The existing waitlist item was found, but its report line is not visible in the current report filters.",
                InfoBarSeverity.Warning
            );
            return;
        }

        foreach (var line in DemandLines)
        {
            line.IsSelected = string.Equals(
                line.SourceLineKey,
                matchingLine.SourceLineKey,
                StringComparison.OrdinalIgnoreCase
            );
        }

        RefreshDemandLinesSnapshot();
        ClearDuplicateNotice();
        await OpenWaitlistEditorForLinesAsync([matchingLine]);
    }

    [RelayCommand]
    private void SelectDemandLine(Model_CustomerPullPack_DemandLine? demandLine)
    {
        if (demandLine is null)
        {
            return;
        }

        SelectedDemandLine = demandLine;
    }

    private void ReplaceDemandLines(IEnumerable<Model_CustomerPullPack_DemandLine> lines)
    {
        var orderedLines = lines.ToList();
        DemandLines = new ObservableCollection<Model_CustomerPullPack_DemandLine>(orderedLines);
        SelectedDemandLine = DemandLines.FirstOrDefault();
        ShortageLineCount = orderedLines.Count(static line => line.ShortageFlag);
        LateOrderLineCount = orderedLines.Count(static line => line.LateOrderFlag);
        LinkedWaitlistLineCount = orderedLines.Count(static line => line.HasLinkedWaitlist);
        OnPropertyChanged(nameof(CurrentCustomerDisplay));
        OnPropertyChanged(nameof(LinkedWaitlistSummary));
        OnPropertyChanged(nameof(SelectedParentPartId));
        OnPropertyChanged(nameof(SelectedCustomerOrderId));
        OnPropertyChanged(nameof(SelectedCustomerName));
        OnPropertyChanged(nameof(SelectedPullDateDisplay));
        OnPropertyChanged(nameof(SelectedFgLocationId));
        OnPropertyChanged(nameof(SelectedSubPartAvailabilitySummary));
        OnPropertyChanged(nameof(SelectedRequesterNote));
        OnPropertyChanged(nameof(SelectedDemandLineLocationOptions));
        OnPropertyChanged(nameof(SelectedDemandLineHasSelectableLocations));
        OnPropertyChanged(nameof(SelectedDemandLineLocationSummary));
        OnPropertyChanged(nameof(SelectionSummary));
    }

    partial void OnSelectedDemandLineChanged(Model_CustomerPullPack_DemandLine? value)
    {
        OnPropertyChanged(nameof(SelectedParentPartId));
        OnPropertyChanged(nameof(SelectedCustomerOrderId));
        OnPropertyChanged(nameof(SelectedCustomerName));
        OnPropertyChanged(nameof(SelectedPullDateDisplay));
        OnPropertyChanged(nameof(SelectedFgLocationId));
        OnPropertyChanged(nameof(SelectedSubPartAvailabilitySummary));
        OnPropertyChanged(nameof(SelectedRequesterNote));
        OnPropertyChanged(nameof(SelectedDemandLineLocationOptions));
        OnPropertyChanged(nameof(SelectedDemandLineHasSelectableLocations));
        OnPropertyChanged(nameof(SelectedDemandLineLocationSummary));
    }

    private async Task OpenWaitlistEditorForLinesAsync(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines
    )
    {
        if (ShowWaitlistEditorAsync is null)
        {
            ShowStatus(
                "The waitlist editor is not available in the current report host.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var existingEntries = await LoadExistingEntriesAsync(selectedLines);
        ApplySelectionContext(selectedLines, existingEntries);

        var editorViewModel = new ViewModel_Dialog_CustomerPullPackWaitlistEditor(
            selectedLines,
            existingEntries,
            _errorHandler,
            _logger,
            _notificationService
        );

        var shouldSave = await ShowWaitlistEditorAsync(editorViewModel);
        if (!shouldSave)
        {
            return;
        }

        if (!editorViewModel.ValidateInputs(out var validationMessage))
        {
            ShowStatus(validationMessage, InfoBarSeverity.Warning);
            return;
        }

        var currentUserId =
            _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
        var currentUserDisplayName =
            _sessionManager.CurrentSession?.User?.DisplayName ?? currentUserId;
        var batchEntries = editorViewModel.BuildBatchEntries(currentUserId, currentUserDisplayName);

        var saveResult = await _mediator.Send(
            new Command_CustomerPullPackBatchUpsert(batchEntries)
        );
        if (!saveResult.IsSuccess || saveResult.Data is null)
        {
            var duplicateWaitlistId = saveResult.ReturnValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(duplicateWaitlistId) is false)
            {
                _duplicateWaitlistId = duplicateWaitlistId;
                var duplicateNoticeMessage = string.IsNullOrWhiteSpace(saveResult.ErrorMessage)
                    ? "An open waitlist item already exists for one of the selected report lines. Open the existing item instead of creating a duplicate."
                    : saveResult.ErrorMessage;

                ShowStatusWithAction(
                    duplicateNoticeMessage,
                    InfoBarSeverity.Warning,
                    "Open Existing Item",
                    async () => await ReopenDuplicateWaitlistCommand.ExecuteAsync(null)
                );
                return;
            }

            ShowStatus(saveResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        ApplySavedEntries(saveResult.Data);
        ClearDuplicateNotice();
        ShowStatus(
            $"Saved {saveResult.Data.Count} Customer Pull n' Pack waitlist item(s).",
            InfoBarSeverity.Success
        );
    }

    private async Task<
        Dictionary<string, Model_CustomerPullPack_WaitlistEntry>
    > LoadExistingEntriesAsync(IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines)
    {
        var existingEntries = new Dictionary<string, Model_CustomerPullPack_WaitlistEntry>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (
            var line in selectedLines.Where(line =>
                string.IsNullOrWhiteSpace(line.LinkedWaitlistId) is false
            )
        )
        {
            var linkedWaitlistResult = await _mediator.Send(
                new Query_CustomerPullPackLinkedWaitlist(line.LinkedWaitlistId)
            );
            if (linkedWaitlistResult.IsSuccess && linkedWaitlistResult.Data is not null)
            {
                existingEntries[line.SourceLineKey] = linkedWaitlistResult.Data;
            }
        }

        return existingEntries;
    }

    private void ApplySelectionContext(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines,
        IReadOnlyDictionary<string, Model_CustomerPullPack_WaitlistEntry> existingEntries
    )
    {
        foreach (var line in selectedLines)
        {
            if (existingEntries.TryGetValue(line.SourceLineKey, out var existingEntry))
            {
                line.RequesterNote = existingEntry.RequesterContextNote;
                line.HasLinkedWaitlist = true;
                line.LinkedWaitlistId = existingEntry.WaitlistId;
                line.WaitlistStateDisplay = string.IsNullOrWhiteSpace(existingEntry.WaitlistId)
                    ? existingEntry.CurrentStatus.ToString()
                    : $"{existingEntry.CurrentStatus} ({existingEntry.WaitlistId})";

                foreach (var locationOption in line.LocationOptions)
                {
                    locationOption.Selected = existingEntry.SelectedLocations.Contains(
                        locationOption.LocationId,
                        StringComparer.OrdinalIgnoreCase
                    );
                }

                continue;
            }

            foreach (var locationOption in line.LocationOptions)
            {
                locationOption.Selected = false;
            }
        }

        RefreshDemandLinesSnapshot();
    }

    private void ApplySavedEntries(IReadOnlyList<Model_CustomerPullPack_WaitlistEntry> savedEntries)
    {
        foreach (var savedEntry in savedEntries)
        {
            var matchingLine = DemandLines.FirstOrDefault(line =>
                string.Equals(
                    line.SourceLineKey,
                    savedEntry.SourceLineKey,
                    StringComparison.OrdinalIgnoreCase
                )
            );
            if (matchingLine is null)
            {
                continue;
            }

            matchingLine.IsSelected = false;
            matchingLine.HasLinkedWaitlist = true;
            matchingLine.LinkedWaitlistId = savedEntry.WaitlistId;
            matchingLine.RequesterNote = savedEntry.RequesterContextNote;
            matchingLine.WaitlistStateDisplay = string.IsNullOrWhiteSpace(savedEntry.WaitlistId)
                ? savedEntry.CurrentStatus.ToString()
                : $"{savedEntry.CurrentStatus} ({savedEntry.WaitlistId})";

            foreach (var locationOption in matchingLine.LocationOptions)
            {
                locationOption.Selected = savedEntry.SelectedLocations.Contains(
                    locationOption.LocationId,
                    StringComparer.OrdinalIgnoreCase
                );
            }
        }

        RefreshDemandLinesSnapshot();
    }

    private void RefreshDemandLinesSnapshot()
    {
        var currentSelectedLineKey = SelectedDemandLine?.SourceLineKey ?? string.Empty;
        var snapshot = DemandLines.ToList();
        DemandLines = new ObservableCollection<Model_CustomerPullPack_DemandLine>(snapshot);
        SelectedDemandLine =
            DemandLines.FirstOrDefault(line =>
                string.Equals(
                    line.SourceLineKey,
                    currentSelectedLineKey,
                    StringComparison.OrdinalIgnoreCase
                )
            ) ?? DemandLines.FirstOrDefault();
        LinkedWaitlistLineCount = snapshot.Count(static line => line.HasLinkedWaitlist);
        OnPropertyChanged(nameof(SelectionSummary));
        OnPropertyChanged(nameof(LinkedWaitlistSummary));
    }

    private void ClearDuplicateNotice()
    {
        _duplicateWaitlistId = string.Empty;
        _notificationService.ClearStatusAction();
    }

    private static string ParseCustomerId(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return string.Empty;
        }

        var trimmedInput = rawInput.Trim();
        var separatorIndex = trimmedInput.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex > 0)
        {
            return trimmedInput[..separatorIndex].Trim().ToUpperInvariant();
        }

        separatorIndex = trimmedInput.IndexOf('·');
        if (separatorIndex > 0)
        {
            return trimmedInput[..separatorIndex].Trim().ToUpperInvariant();
        }

        return trimmedInput.ToUpperInvariant();
    }

    private static string ParseCustomerName(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return string.Empty;
        }

        var trimmedInput = rawInput.Trim();
        var separatorIndex = trimmedInput.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex > 0 && separatorIndex + 3 < trimmedInput.Length)
        {
            return trimmedInput[(separatorIndex + 3)..].Trim();
        }

        separatorIndex = trimmedInput.IndexOf('·');
        if (separatorIndex > 0 && separatorIndex + 1 < trimmedInput.Length)
        {
            return trimmedInput[(separatorIndex + 1)..].Trim();
        }

        return string.Empty;
    }
}

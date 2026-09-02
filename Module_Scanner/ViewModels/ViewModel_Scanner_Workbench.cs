using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContentDialogResult = Microsoft.UI.Xaml.Controls.ContentDialogResult;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Views;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.ViewModels;

/// <summary>
/// Workbench ViewModel: the current list entry form, part/location validation workflow
/// (ScannerUpdate.md Step 6b), non-blocking header errors, and input lockout during
/// automation. The current list is auto-created when the page loads.
/// </summary>
public partial class ViewModel_Scanner_Workbench : ViewModel_Shared_Base
{
    private readonly IService_ScannerNavigation _navigationService;
    private readonly IService_ScannerWorkflow _workflowService;
    private readonly IService_ScannerValidation _validationService;
    private readonly IService_ScannerExecution _executionService;
    private readonly IService_ScannerHotkey _hotkeyService;
    private readonly IService_Window _windowService;

    // ── Entry fields ─────────────────────────────────────────────────────────────
    [ObservableProperty]
    private string _newPartId = string.Empty;

    [ObservableProperty]
    private string _newFromWarehouse = "002";

    [ObservableProperty]
    private string _newToWarehouse = "002";

    // ── Input states ─────────────────────────────────────────────────────────────
    /// <summary>Part Number is always editable unless automation is running.</summary>
    [ObservableProperty]
    private bool _isPartEditable = true;

    /// <summary>Search mode toggle: false = by part number, true = by location.</summary>
    [ObservableProperty]
    private bool _isLocationModeEnabled;

    public Enum_ScannerSearchMode SearchMode =>
        IsLocationModeEnabled ? Enum_ScannerSearchMode.Location : Enum_ScannerSearchMode.PartNumber;

    public Enum_SharedLookupType LookupType =>
        IsLocationModeEnabled ? Enum_SharedLookupType.Location : Enum_SharedLookupType.PartNumber;

    public string LookupHeaderText => IsLocationModeEnabled ? "Location" : "Part";

    public string LookupPlaceholderText =>
        IsLocationModeEnabled ? "Enter location" : "Enter part number";

    partial void OnIsLocationModeEnabledChanged(bool value)
    {
        // Clear the lookup input so a stale value is not re-validated in the other mode.
        NewPartId = string.Empty;
        PartIdClearRequested?.Invoke();
        OnPropertyChanged(nameof(SearchMode));
        OnPropertyChanged(nameof(LookupType));
        OnPropertyChanged(nameof(LookupHeaderText));
        OnPropertyChanged(nameof(LookupPlaceholderText));
    }

    // ── Header error area (non-blocking, 5-second auto-clear) ────────────────────
    [ObservableProperty]
    private string _headerErrorText = string.Empty;

    [ObservableProperty]
    private bool _isHeaderErrorVisible;

    private CancellationTokenSource? _headerErrorTokenSource;

    // ── Current list ─────────────────────────────────────────────────────────────
    [ObservableProperty]
    private Model_ScannerBatchSession? _currentSession;

    [ObservableProperty]
    private ObservableCollection<Model_ScannerBatchItem> _sessionItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSessionItem))]
    [NotifyPropertyChangedFor(nameof(IsSendEnabled))]
    private Model_ScannerBatchItem? _selectedSessionItem;

    // ── Send confirmation ────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSendEnabled))]
    private bool _isSendPromptVisible;

    private Model_ScannerBatchItem? _pendingSendItem;

    // Reentrancy guard: Enter + LostFocus (or a double scan) can trigger the part/location
    // add flow twice. Ignore a second run while the first is still in flight so the part is
    // not validated/added twice.
    private bool _isValidationFlowRunning;

    public bool HasActiveSession => CurrentSession is not null;

    public bool HasSelectedSessionItem => SelectedSessionItem is not null;

    /// <summary>
    /// Send is enabled only when the selected line has validated (Valid state) and no
    /// automation, confirmation prompt, or busy state is active.
    /// </summary>
    public bool IsSendEnabled =>
        SelectedSessionItem is { ValidationState: Enum_ScannerValidationState.Valid }
        && !_executionService.IsAutomationRunning
        && !IsSendPromptVisible
        && !IsBusy;

    /// <summary>
    /// Primary button label. Always reads "Send"; whether the selected line is ready is shown
    /// by the row status and the fix-issues dialog, not by the button text.
    /// </summary>
    public string SendButtonText => "Send";

    public string OwnerUserId { get; } = Environment.UserName;

    public string OwnerDisplayName { get; } = Environment.UserName;

    public TimeSpan TransferConfirmTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan TransferConfirmPollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Raised so the view can show the stock-location picker (Step 6b-a1-a).</summary>
    public event Func<string, string, string, Task<IReadOnlyList<Model_ScannerStockPick>>>? FromLocationInventoryPickerRequested;

    /// <summary>Raised so the view can show the parts-at-location picker (location search mode).</summary>
    public event Func<string, string, Task<IReadOnlyList<Model_ScannerStockPick>>>? LocationPartsPickerRequested;

    /// <summary>Raised so the view can return focus to the Part lookup (Step 6b-a2).</summary>
    public event Action? PartIdFocusRequested;

    /// <summary>Raised so the view can clear the Part lookup input (prevents modal reopen).</summary>
    public event Action? PartIdClearRequested;

    /// <summary>Raised so the view can focus the first row's destination cell after the modal populates the list.</summary>
    public event Action? FirstRowToCellFocusRequested;

    /// <summary>
    /// Raised when the operator is about to add one or more lines that already match rows
    /// in the current list (same part and source location). The view shows a confirmation
    /// dialog and returns true to add anyway, false to cancel.
    /// </summary>
    public event Func<string, Task<bool>>? DuplicateAddConfirmationRequested;

    public ViewModel_Scanner_Workbench(
        IService_ScannerNavigation navigationService,
        IService_ScannerWorkflow workflowService,
        IService_ScannerValidation validationService,
        IService_ScannerExecution executionService,
        IService_ScannerHotkey hotkeyService,
        IService_Window windowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(workflowService);
        ArgumentNullException.ThrowIfNull(validationService);
        ArgumentNullException.ThrowIfNull(executionService);
        ArgumentNullException.ThrowIfNull(hotkeyService);
        ArgumentNullException.ThrowIfNull(windowService);
        _navigationService = navigationService;
        _workflowService = workflowService;
        _validationService = validationService;
        _executionService = executionService;
        _hotkeyService = hotkeyService;
        _windowService = windowService;

        // Input lockout while automation runs (ScannerUpdate.md Task 1).
        _executionService.PropertyChanged += OnExecutionPropertyChanged;

        // Keep Send enablement in sync with the busy state (base-class observable).
        PropertyChanged += OnWorkbenchPropertyChanged;
    }

    private void OnExecutionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IService_ScannerExecution.IsAutomationRunning))
        {
            IsPartEditable = !_executionService.IsAutomationRunning;
            RecomputeSendEnabled();
        }
    }

    private void OnWorkbenchPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsBusy))
        {
            RecomputeSendEnabled();
        }
    }

    partial void OnSelectedSessionItemChanged(Model_ScannerBatchItem? value)
    {
        RecomputeSendEnabled();
    }

    private void RecomputeSendEnabled()
    {
        OnPropertyChanged(nameof(IsSendEnabled));
        OnPropertyChanged(nameof(SendButtonText));
        SendSelectedCommand.NotifyCanExecuteChanged();
    }

    // ── Page lifecycle ───────────────────────────────────────────────────────────

    /// <summary>Auto-creates (or resumes) the current list when the Workbench loads.</summary>
    public async Task<Model_Dao_Result<Model_ScannerBatchSession>> EnsureCurrentSessionAsync()
    {
        var result = await _workflowService.EnsureCurrentSessionAsync(
            OwnerUserId,
            OwnerDisplayName,
            await ResolveActiveProfileIdAsync()
        );

        if (result.Success && result.Data is not null)
        {
            CurrentSession = result.Data;
            SessionItems = [.. result.Data.Items.OrderBy(item => item.SequenceNumber)];
        }

        return result;
    }

    private async Task<Guid> ResolveActiveProfileIdAsync()
    {
        var profiles = await _workflowService.GetProfilesAsync(OwnerUserId);
        if (profiles?.Success == true && profiles.Data is { Count: > 0 })
        {
            return (profiles.Data.FirstOrDefault(profile => profile.IsDefaultForUser)
                ?? profiles.Data[0]).ProfileId;
        }

        return Guid.Empty;
    }

    public async Task<Model_ScannerProfile> ResolveActiveProfileAsync()
    {
        var profiles = await _workflowService.GetProfilesAsync(OwnerUserId);
        if (profiles?.Success == true && profiles.Data is { Count: > 0 })
        {
            return profiles.Data.FirstOrDefault(profile => profile.IsDefaultForUser)
                ?? profiles.Data[0];
        }

        return new Model_ScannerProfile
        {
            OwnerUserId = OwnerUserId,
            ProfileName = "Default",
            IsDefaultForUser = true,
        };
    }

    // ── Step 6b: Part focus-lost validation ──────────────────────────────────────

    /// <summary>
    /// Runs the Step 6b part validation: part exists? -> stock locations -> modal ->
    /// apply selection (or header error + refocus when no stock).
    /// </summary>
    /// <param name="partId"></param>
    public async Task PartValidationCompletedAsync(string partId)
    {
        if (_isValidationFlowRunning)
        {
            return;
        }

        _isValidationFlowRunning = true;
        try
        {
            await RunPartValidationCoreAsync(partId);
        }
        finally
        {
            _isValidationFlowRunning = false;
        }
    }

    private async Task RunPartValidationCoreAsync(string partId)
    {
        ClearHeaderError();

        NewPartId = partId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(NewPartId))
        {
            return;
        }

        var exists = await _validationService.PartExistsAsync(NewPartId);
        if (!exists.Success)
        {
            ShowHeaderError(
                string.IsNullOrWhiteSpace(exists.ErrorMessage)
                    ? "Part lookup is currently unavailable."
                    : exists.ErrorMessage
            );
            PartIdFocusRequested?.Invoke();
            return;
        }

        if (!exists.Data)
        {
            // Edge-case fix: say the part was not found, not that it has no stock.
            ShowHeaderError($"Part number '{NewPartId}' was not found. Please check the part number.");
            PartIdFocusRequested?.Invoke();
            return;
        }

        var stock = await _validationService.GetLocationsWithStockAsync(NewPartId, NewFromWarehouse);
        if (!stock.Success || stock.Data is null || stock.Data.Count == 0)
        {
            await ShowNoStockErrorAsync();
            return;
        }

        // Step 6b-a1-a: the view shows the stock-location modal (multi-select).
        if (FromLocationInventoryPickerRequested is null)
        {
            await ShowNoStockErrorAsync();
            return;
        }

        var picks = await FromLocationInventoryPickerRequested(
            NewPartId,
            NewFromWarehouse,
            string.Empty
        );

        if (picks is null || picks.Count == 0)
        {
            // Step 6b-a1-b: user cancelled the modal -> clear the part input so the shared
            // lookup cannot re-validate and reopen the modal.
            ClearPartInput();
            ShowStatus("Stock selection cancelled.", InfoBarSeverity.Informational);
            return;
        }

        await ApplyStockLocationSelectionAsync(picks);
    }

    /// <summary>
    /// Runs the location-search validation: location exists? -> parts at that location ->
    /// parts modal -> populate rows with that source location (mirror of the part workflow).
    /// </summary>
    /// <param name="location"></param>
    public async Task LocationValidationCompletedAsync(string location)
    {
        if (_isValidationFlowRunning)
        {
            return;
        }

        _isValidationFlowRunning = true;
        try
        {
            await RunLocationValidationCoreAsync(location);
        }
        finally
        {
            _isValidationFlowRunning = false;
        }
    }

    private async Task RunLocationValidationCoreAsync(string location)
    {
        ClearHeaderError();

        NewPartId = location?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(NewPartId))
        {
            return;
        }

        var locationResult = await _validationService.ValidateLocationAsync(NewPartId, NewFromWarehouse);
        if (!locationResult.Success || locationResult.Data?.IsValid != true)
        {
            ShowHeaderError(
                string.IsNullOrWhiteSpace(locationResult.ErrorMessage)
                    ? "Location is not valid. Please check the format."
                    : locationResult.ErrorMessage
            );
            PartIdFocusRequested?.Invoke();
            return;
        }

        var canonicalLocation = locationResult.Data.CanonicalLocation;
        NewPartId = canonicalLocation;

        var parts = await _validationService.GetPartsAtLocationAsync(canonicalLocation, NewFromWarehouse);
        if (!parts.Success || parts.Data is null || parts.Data.Count == 0)
        {
            await ShowNoStockErrorAsync("No parts have quantity at this location.");
            return;
        }

        if (LocationPartsPickerRequested is null)
        {
            await ShowNoStockErrorAsync("No parts have quantity at this location.");
            return;
        }

        var picks = await LocationPartsPickerRequested(canonicalLocation, NewFromWarehouse);
        if (picks is null || picks.Count == 0)
        {
            ClearPartInput();
            ShowStatus("Part selection cancelled.", InfoBarSeverity.Informational);
            return;
        }

        await ApplyLocationPartsSelectionAsync(picks, canonicalLocation);
    }

    private async Task ApplyStockLocationSelectionAsync(IReadOnlyList<Model_ScannerStockPick> picks)
    {
        await PopulateLinesAsync(picks, fallbackPartId: NewPartId, fallbackFromLocation: string.Empty);
    }

    private async Task ApplyLocationPartsSelectionAsync(IReadOnlyList<Model_ScannerStockPick> picks, string location)
    {
        await PopulateLinesAsync(picks, fallbackPartId: string.Empty, fallbackFromLocation: location);
    }

    private async Task PopulateLinesAsync(
        IReadOnlyList<Model_ScannerStockPick> picks,
        string fallbackPartId,
        string fallbackFromLocation
    )
    {
        if (CurrentSession is null)
        {
            ShowStatus("The current list is not ready.", InfoBarSeverity.Warning);
            return;
        }

        var lines = ExpandStockPicks(picks, fallbackPartId, fallbackFromLocation);

        // Edge-case fix: warn before adding rows that duplicate an existing part + source
        // location already in the list, so an accidental double scan is caught.
        var existingRows = CurrentSession.Items.ToList();
        var duplicateLineCount = lines.Count(line =>
            existingRows.Any(row =>
                string.Equals(row.PayloadPartId, line.PartId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    row.PayloadFromLocation,
                    line.FromLocation,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        );

        if (duplicateLineCount > 0 && DuplicateAddConfirmationRequested is not null)
        {
            var proceed = await DuplicateAddConfirmationRequested(
                $"{duplicateLineCount} of the selected line(s) match rows already in the current "
                + "list (same part and source location). Add them anyway?"
            );
            if (!proceed)
            {
                ClearPartInput();
                ShowStatus(
                    "Add cancelled - the selected line(s) are already in the list.",
                    InfoBarSeverity.Informational
                );
                return;
            }
        }

        IsBusy = true;
        try
        {
            var addedCount = 0;
            foreach (var line in lines)
            {
                var item = new Model_ScannerBatchItem
                {
                    SessionId = CurrentSession.SessionId,
                    SequenceNumber = CurrentSession.Items.Count + 1,
                    PayloadPartId = line.PartId,
                    PayloadFromWarehouse = NewFromWarehouse,
                    PayloadFromLocation = line.FromLocation,
                    PayloadToWarehouse = NewToWarehouse,
                    PayloadQuantity = line.Quantity,
                    MaxQuantity = line.MaxQuantity,
                    ValidationState = Enum_ScannerValidationState.NotValidated,
                };

                var save = await _workflowService.UpsertBatchItemAsync(CurrentSession, item);
                if (!save.Success || save.Data is null)
                {
                    ShowStatus(
                        string.IsNullOrWhiteSpace(save.ErrorMessage)
                            ? "Unable to add the selected line."
                            : save.ErrorMessage,
                        InfoBarSeverity.Error
                    );
                    return;
                }

                CurrentSession = save.Data;
                addedCount++;
            }

            SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
            SelectedSessionItem = SessionItems.LastOrDefault();

            // Clear the lookup input so it cannot re-validate and reopen the modal.
            ClearPartInput();

            RecomputeSendEnabled();
            FirstRowToCellFocusRequested?.Invoke();
            ShowStatus(
                addedCount == 1
                    ? "Line added to the current list. Enter the destination in the table."
                    : $"{addedCount} line(s) added to the current list. Enter each destination in the table.",
                InfoBarSeverity.Success
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string NormalizeQuantity(string? raw)
    {
        if (
            decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity)
            && quantity > 0
        )
        {
            return quantity.ToString("0.########", CultureInfo.InvariantCulture);
        }

        return "1";
    }

    /// <summary>One concrete current-list line produced from a modal pick.</summary>
    /// <param name="PartId"></param>
    /// <param name="FromLocation"></param>
    /// <param name="Quantity"></param>
    /// <param name="MaxQuantity"></param>
    private sealed record StockLine(string PartId, string FromLocation, string Quantity, decimal? MaxQuantity);

    /// <summary>
    /// Expands the stock picks into concrete current-list lines: one line per pick when a
    /// single transaction, or <see cref="Model_ScannerStockPick.TransactionCount"/> lines
    /// (quantity 1 each) when multiple transactions are requested. Each line carries the
    /// source location's on-hand as its maximum quantity guard.
    /// </summary>
    /// <param name="picks"></param>
    /// <param name="fallbackPartId"></param>
    /// <param name="fallbackFromLocation"></param>
    private static IReadOnlyList<StockLine> ExpandStockPicks(
        IReadOnlyList<Model_ScannerStockPick> picks,
        string fallbackPartId,
        string fallbackFromLocation
    )
    {
        var lines = new List<StockLine>();

        foreach (var pick in picks)
        {
            var count = Math.Max(1, pick.TransactionCount);
            var max = pick.OnHand > 0 ? pick.OnHand : (decimal?)null;
            var partId = string.IsNullOrWhiteSpace(pick.PartId) ? fallbackPartId : pick.PartId;
            var fromLocation = string.IsNullOrWhiteSpace(pick.Location) ? fallbackFromLocation : pick.Location;

            if (count == 1)
            {
                lines.Add(new StockLine(partId, fromLocation, NormalizeQuantity(pick.Quantity), max));
            }
            else
            {
                for (var index = 0; index < count; index++)
                {
                    lines.Add(new StockLine(partId, fromLocation, "1", max));
                }
            }
        }

        return lines;
    }

    private void ClearPartInput()
    {
        NewPartId = string.Empty;
        PartIdClearRequested?.Invoke();
    }

    private async Task ShowNoStockErrorAsync(
        string message = "The part number does not have any quantity in-house."
    )
    {
        ShowHeaderError(message);
        PartIdFocusRequested?.Invoke();
        await Task.CompletedTask;
    }

    // ── Non-blocking header error (5-second auto-clear) ──────────────────────────

    public void ShowHeaderError(string message)
    {
        _headerErrorTokenSource?.Cancel();
        _headerErrorTokenSource = new CancellationTokenSource();
        var token = _headerErrorTokenSource.Token;

        HeaderErrorText = message;
        IsHeaderErrorVisible = true;

        _ = AutoClearHeaderErrorAsync(token);
    }

    public void ClearHeaderError()
    {
        _headerErrorTokenSource?.Cancel();
        HeaderErrorText = string.Empty;
        IsHeaderErrorVisible = false;
    }

    private async Task AutoClearHeaderErrorAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            HeaderErrorText = string.Empty;
            IsHeaderErrorVisible = false;
        }
        catch (TaskCanceledException)
        {
            // A newer input/error evaluation loop superseded this one.
        }
    }

    // ── Location helpers (used by the view code-behind) ──────────────────────────

    public string FormatLocation(string location)
    {
        return _validationService.FormatLocation(location);
    }

    public Task<Model_Dao_Result<IReadOnlyList<Model_InforVisualMaterialLocationRow>>> GetFromInventoryLocationsAsync()
    {
        return _validationService.GetLocationsWithStockAsync(NewPartId, NewFromWarehouse);
    }

    public Task<Model_Dao_Result<IReadOnlyList<Model_InforVisualMaterialLocationRow>>> GetPartsAtLocationForPickerAsync(
        string location,
        string warehouseCode
    )
    {
        return _validationService.GetPartsAtLocationAsync(location, warehouseCode);
    }

    // ── Per-row validation (editable To / Qty columns) ──────────────────────────

    /// <summary>
    /// Sanitizes and validates one current-list row after the operator edits its
    /// destination or quantity, updates the row's Status, persists it, and refreshes
    /// whether Send is available. A row is not send-eligible until it validates.
    /// </summary>
    /// <param name="item"></param>
    public async Task<Model_ScannerItemValidationResult?> ValidateSessionItemAsync(
        Model_ScannerBatchItem item
    )
    {
        if (item is null || CurrentSession is null)
        {
            return null;
        }

        // Step 6b-a1-c-b: sanitize the destination format before validation.
        var sanitizedTo = Helper_ScannerLocationFormat.SanitizeLocationFormat(item.PayloadToLocation);
        if (!string.IsNullOrEmpty(sanitizedTo))
        {
            item.PayloadToLocation = sanitizedTo;
        }

        Model_ScannerItemValidationResult? validationResult = null;

        // Quantity-too-high guard using the on-hand captured from the stock-location modal.
        if (
            item.MaxQuantity is decimal maxQuantity
            && decimal.TryParse(item.PayloadQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity)
            && quantity > maxQuantity
        )
        {
            item.ValidationState = Enum_ScannerValidationState.Invalid;
            item.ValidationMessage = "Qty too High";
            item.ValidationNotes = $"Quantity cannot exceed the on-hand of {maxQuantity}.";
        }
        else
        {
            var validation = await _validationService.ValidateNewItemAsync(
                new Model_ScannerItemValidationRequest
                {
                    SessionId = CurrentSession.SessionId,
                    ItemId = item.ItemId,
                    PartId = item.PayloadPartId,
                    FromWarehouse = item.PayloadFromWarehouse,
                    FromLocation = item.PayloadFromLocation,
                    ToWarehouse = item.PayloadToWarehouse,
                    ToLocation = item.PayloadToLocation,
                    Quantity = item.PayloadQuantity,
                }
            );

            if (validation.Success && validation.Data is not null)
            {
                item.ApplyValidationResult(validation.Data);
                validationResult = validation.Data;
            }
            else
            {
                item.ValidationState = Enum_ScannerValidationState.Invalid;
                item.ValidationMessage = "Validation unavailable.";
                item.ValidationNotes =
                    string.IsNullOrWhiteSpace(validation.ErrorMessage)
                        ? "Validation failed due to service error."
                        : validation.ErrorMessage;
            }
        }

        // Persist the edited row so the change survives navigation and app restarts. The
        // model is observable, so the Status column updates in place; do NOT rebuild the
        // SessionItems collection here or the focus the operator was moving to is lost.
        if (CurrentSession.Items.Any(candidate => candidate.ItemId == item.ItemId))
        {
            var persist = await _workflowService.UpsertBatchItemAsync(CurrentSession, item);
            if (persist.Success && persist.Data is not null)
            {
                CurrentSession = persist.Data;
            }
        }

        RecomputeSendEnabled();
        return validationResult;
    }

    /// <summary>
    /// Revalidates rows resumed from the database that were previously marked valid so the
    /// in-memory on-hand quantity guard is restored against current stock. The on-hand
    /// amount is intentionally not persisted; this runs after an app restart when the guard
    /// is missing. Rows already re-guarded this session (or not yet valid) are skipped.
    /// </summary>
    public async Task RevalidateResumedValidItemsAsync()
    {
        if (CurrentSession is null)
        {
            return;
        }

        var rowsToRevalidate = CurrentSession.Items
            .Where(item =>
                item.ExecutionState == Enum_ScannerExecutionState.Waiting
                && item.MaxQuantity is null
                && item.ValidationState == Enum_ScannerValidationState.Valid
            )
            .ToList();

        foreach (var item in rowsToRevalidate)
        {
            await ValidateSessionItemAsync(item);
        }
    }

    // ── Commands ─────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void NavigateToWorkbench()
    {
        _navigationService.ShowWorkbench();
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        _navigationService.ShowHistory();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.ShowSettings();
    }

    /// <summary>
    /// Launches (or activates) VMINVENT and sends the open-window shortcut so the operator
    /// can reach the Inventory Transfers window from the Scanner Workbench.
    /// </summary>
    [RelayCommand]
    private async Task OpenInventoryAsync()
    {
        if (_executionService.IsAutomationRunning)
        {
            ShowStatus("Wait for the current send to finish before opening the inventory window.", InfoBarSeverity.Warning);
            return;
        }

        var profile = await ResolveActiveProfileAsync();
        var result = await _executionService.OpenInventoryWindowAsync(profile);

        ShowStatus(
            string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? "Inventory window opened."
                : result.ErrorMessage,
            result.Success ? InfoBarSeverity.Success : InfoBarSeverity.Warning
        );
    }

    [RelayCommand]
    private async Task RemoveSelectedSessionItemAsync()
    {
        if (CurrentSession is null || SelectedSessionItem is null)
        {
            return;
        }

        var itemToRemove = CurrentSession.Items.FirstOrDefault(candidate =>
            candidate.ItemId == SelectedSessionItem.ItemId);
        if (itemToRemove is null)
        {
            return;
        }

        CurrentSession.Items.Remove(itemToRemove);
        ReorderSessionItems();

        var persist = await _workflowService.ReplaceSessionItemsAsync(CurrentSession);
        if (!persist.Success || persist.Data is null)
        {
            ShowStatus(
                string.IsNullOrWhiteSpace(persist.ErrorMessage)
                    ? "Unable to remove the selected item."
                    : persist.ErrorMessage,
                InfoBarSeverity.Error
            );
            return;
        }

        CurrentSession = persist.Data;
        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
        SelectedSessionItem = SessionItems.FirstOrDefault();
        ShowStatus("Selected item removed.", InfoBarSeverity.Success);
    }

    [RelayCommand(CanExecute = nameof(CanSendSelected))]
    private async Task SendSelectedAsync()
    {
        if (CurrentSession is null || SelectedSessionItem is null)
        {
            return;
        }

        if (IsSendPromptVisible)
        {
            return;
        }

        // When the selected line is not valid the button is labeled "Validate": surface the
        // issues instead of sending.
        if (SelectedSessionItem.ValidationState != Enum_ScannerValidationState.Valid)
        {
            await ShowFixIssuesDialogAsync(SelectedSessionItem);
            return;
        }

        IsBusy = true;
        try
        {
            var profile = await ResolveActiveProfileAsync();
            var result = await _executionService.SendSpecificItemAsync(
                CurrentSession,
                SelectedSessionItem,
                profile
            );

            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to send the selected scanner item."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            if (result.Data.Stopped && result.Data.FailedCount > 0)
            {
                ShowStatus(result.Data.FailureMessage, InfoBarSeverity.Warning);
                return;
            }

            if (result.Data.SentCount > 0)
            {
                // Edge-case fix: never auto-confirm a send. The operator states whether the
                // transaction actually saved in Infor Visual (Yes/No) after returning to the
                // app, so a line is only cleared from the list on an explicit Yes.
                _pendingSendItem = SelectedSessionItem;
                IsSendPromptVisible = true;
                return;
            }

            if (result.Data.SkippedCount > 0)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.Data.FailureMessage)
                        ? "The selected line is not ready to send."
                        : result.Data.FailureMessage,
                    InfoBarSeverity.Warning
                );
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSendSelected()
    {
        return SelectedSessionItem is not null
            && !IsSendPromptVisible
            && !_executionService.IsAutomationRunning;
    }

    private async Task ShowFixIssuesDialogAsync(Model_ScannerBatchItem item)
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            ShowStatus($"Fix the issues before sending: {item.StatusText}", InfoBarSeverity.Warning);
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Fix the issues before sending",
            Content = item.StatusText,
            CloseButtonText = "OK",
            XamlRoot = xamlRoot,
        };
        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);
        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task ConfirmSavedAsync()
    {
        var savedItem = _pendingSendItem;
        _pendingSendItem = null;
        IsSendPromptVisible = false;

        if (CurrentSession is not null && savedItem is not null)
        {
            var itemToRemove = CurrentSession.Items.FirstOrDefault(candidate =>
                candidate.ItemId == savedItem.ItemId);
            if (itemToRemove is not null)
            {
                // Edge-case fix: clear exactly the one line the operator confirmed was saved.
                // No history record is written; all other lines stay untouched (one per click).
                CurrentSession.Items.Remove(itemToRemove);
                ReorderSessionItems();

                var persist = await _workflowService.ReplaceSessionItemsAsync(CurrentSession);
                if (persist.Success && persist.Data is not null)
                {
                    CurrentSession = persist.Data;
                }

                SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];

                // Edge-case fix: move the selection to the next remaining line so the main
                // button is ready to send it (and never gets stuck on "Validate").
                SelectedSessionItem = SessionItems.FirstOrDefault();
                RecomputeSendEnabled();
                ShowStatus(
                    $"Line {savedItem.SequenceNumber} cleared from the current list.",
                    InfoBarSeverity.Success
                );
            }
        }
    }

    [RelayCommand]
    private async Task ConfirmNotSavedAsync()
    {
        var keptItem = _pendingSendItem;
        _pendingSendItem = null;
        IsSendPromptVisible = false;

        // Edge-case fix: treat the line as never sent - keep it in the list, restore its
        // Waiting state, and persist so it can be sent again on a retry.
        if (CurrentSession is not null && keptItem is not null)
        {
            keptItem.ExecutionState = Enum_ScannerExecutionState.Waiting;
            keptItem.SentUtc = null;
            keptItem.LastUpdatedUtc = DateTime.UtcNow;

            await _workflowService.UpsertBatchItemAsync(CurrentSession, keptItem);

            SelectedSessionItem = CurrentSession.Items.FirstOrDefault(item =>
                item.ItemId == keptItem.ItemId);
        }

        RecomputeSendEnabled();
        ShowStatus("Line kept as not sent. You can send it again.", InfoBarSeverity.Warning);
    }

    [RelayCommand]
    private async Task CheckAllAsync()
    {
        if (CurrentSession is null || CurrentSession.Items.Count == 0)
        {
            return;
        }

        // Edge-case fix: only validate rows that are not already valid. Rows the operator
        // already validated are left untouched (e.g., 3 validated rows are not re-checked
        // when a new row is added).
        var rowsToValidate = CurrentSession.Items
            .Where(item => item.ValidationState != Enum_ScannerValidationState.Valid)
            .ToList();

        if (rowsToValidate.Count == 0)
        {
            ShowStatus(
                "All items in the current list are already validated.",
                InfoBarSeverity.Informational
            );
            return;
        }

        IsBusy = true;
        try
        {
            foreach (var item in rowsToValidate)
            {
                await ValidateSessionItemAsync(item);
            }

            var invalidCount = rowsToValidate.Count(item =>
                item.ValidationState == Enum_ScannerValidationState.Invalid
            );

            RecomputeSendEnabled();
            ShowStatus(
                invalidCount == 0
                    ? $"Validated {rowsToValidate.Count} scanner item(s). All are ready for send review."
                    : $"Validated {rowsToValidate.Count} scanner item(s). {invalidCount} still require review.",
                invalidCount == 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ManageItemsDialogAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("There is no current list to manage.", InfoBarSeverity.Warning);
            return;
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            ShowStatus("Unable to open Manage Items because the window root is unavailable.", InfoBarSeverity.Warning);
            return;
        }

        var dialog = new View_Scanner_ManageItemsDialog(CurrentSession, _validationService)
        {
            XamlRoot = xamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            ShowStatus("Manage Items closed without changes.", InfoBarSeverity.Informational);
            return;
        }

        CurrentSession.Items.Clear();
        foreach (var item in dialog.GetItemsSnapshot().OrderBy(candidate => candidate.SequenceNumber))
        {
            item.SessionId = CurrentSession.SessionId;
            CurrentSession.Items.Add(item);
        }

        CurrentSession.RecalculateItemCounters();
        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
        SelectedSessionItem = SessionItems.FirstOrDefault(candidate =>
            SelectedSessionItem is not null && candidate.ItemId == SelectedSessionItem.ItemId);

        var persist = await _workflowService.ReplaceSessionItemsAsync(CurrentSession);
        if (!persist.Success || persist.Data is null)
        {
            ShowStatus(
                string.IsNullOrWhiteSpace(persist.ErrorMessage)
                    ? "Manage Items changes were applied locally but could not be saved."
                    : persist.ErrorMessage,
                InfoBarSeverity.Error
            );
            return;
        }

        CurrentSession = persist.Data;
        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
        SelectedSessionItem = SessionItems.FirstOrDefault(candidate =>
            SelectedSessionItem is not null && candidate.ItemId == SelectedSessionItem.ItemId);
        ShowStatus("Manage Items changes saved.", InfoBarSeverity.Success);
    }

    [RelayCommand]
    private async Task ClearListAsync()
    {
        if (CurrentSession is null || CurrentSession.Items.Count == 0)
        {
            ShowStatus("The current list is already empty.", InfoBarSeverity.Informational);
            return;
        }

        CurrentSession.Items.Clear();
        CurrentSession.RecalculateItemCounters();
        SessionItems = [];
        SelectedSessionItem = null;
        RecomputeSendEnabled();

        // Persist the empty list so the clear survives navigation and app restarts.
        var persist = await _workflowService.ReplaceSessionItemsAsync(CurrentSession);
        if (!persist.Success || persist.Data is null)
        {
            ShowStatus(
                string.IsNullOrWhiteSpace(persist.ErrorMessage)
                    ? "The list was cleared locally but could not be saved."
                    : persist.ErrorMessage,
                InfoBarSeverity.Error
            );
            return;
        }

        CurrentSession = persist.Data;
        SessionItems = [];
        ShowStatus("Current list cleared.", InfoBarSeverity.Success);
    }

    [RelayCommand]
    private Task ExportAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("There is no current list to export.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        var exportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var exportPath = Path.Combine(
            exportDirectory,
            $"scanner-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt"
        );

        var lines = new System.Collections.Generic.List<string>
        {
            "Scanner workbench export",
            $"Session: {CurrentSession.SessionName}",
            $"Status: {CurrentSession.Status}",
            $"Items: {CurrentSession.Items.Count}",
            string.Empty,
        };

        foreach (var item in CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber))
        {
            lines.Add(
                $"{item.SequenceNumber}\t{item.PayloadPartId}\t{item.PayloadFromWarehouse}/{item.PayloadFromLocation}\t{item.PayloadToWarehouse}/{item.PayloadToLocation}\t{item.PayloadQuantity}\t{item.ExecutionState}\t{item.ValidationState}"
            );
        }

        File.WriteAllLines(exportPath, lines);
        ShowStatus($"Exported {CurrentSession.Items.Count} item(s) to {exportPath}.", InfoBarSeverity.Success);
        return Task.CompletedTask;
    }

    private void ReorderSessionItems()
    {
        if (CurrentSession is null)
        {
            return;
        }

        var orderedItems = CurrentSession.Items
            .OrderBy(candidate => candidate.SequenceNumber)
            .ToList();
        CurrentSession.Items.Clear();

        for (var index = 0; index < orderedItems.Count; index++)
        {
            orderedItems[index].SequenceNumber = index + 1;
            orderedItems[index].LastUpdatedUtc = DateTime.UtcNow;
            CurrentSession.Items.Add(orderedItems[index]);
        }

        CurrentSession.RecalculateItemCounters();
        CurrentSession.LastUpdatedUtc = DateTime.UtcNow;
    }

    // ── Hotkey integration ───────────────────────────────────────────────────────

    public void Activate()
    {
        _hotkeyService.SendShortcutPressed -= OnSendShortcutPressed;
        _hotkeyService.SendShortcutPressed += OnSendShortcutPressed;
    }

    public void Deactivate()
    {
        _hotkeyService.SendShortcutPressed -= OnSendShortcutPressed;
    }

    private void OnSendShortcutPressed(object? sender, EventArgs e)
    {
        _ = SendSelectedCommand.ExecuteAsync(null);
    }
}

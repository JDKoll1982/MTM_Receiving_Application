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

    // ── Entry fields (baseline state per ScannerUpdate.md Task 6) ─────────────────
    [ObservableProperty]
    private string _newPartId = string.Empty;

    [ObservableProperty]
    private string _newFromWarehouse = "002";

    [ObservableProperty]
    private string _newFromLocation = string.Empty;

    [ObservableProperty]
    private string _newToWarehouse = "002";

    [ObservableProperty]
    private string _newToLocation = string.Empty;

    [ObservableProperty]
    private string _newQuantity = "1";

    [ObservableProperty]
    private string _lastValidationStatus = string.Empty;

    [ObservableProperty]
    private string _lastValidationNotes = string.Empty;

    // ── Baseline input states (Task 6) ───────────────────────────────────────────
    /// <summary>Part Number is always editable unless automation is running.</summary>
    [ObservableProperty]
    private bool _isPartEditable = true;

    /// <summary>From Location is read-only at all times.</summary>
    public bool IsFromLocationReadOnly => true;

    /// <summary>To Location is read-only at all times.</summary>
    public bool IsToLocationReadOnly => true;

    /// <summary>To Location is disabled until a source resolves (Step 6b-a1-c).</summary>
    [ObservableProperty]
    private bool _isToLocationEnabled;

    /// <summary>Quantity is read-only at all times.</summary>
    public bool IsQuantityReadOnly => true;

    /// <summary>Quantity is enabled once the From location resolves.</summary>
    [ObservableProperty]
    private bool _isQuantityEnabled;

    /// <summary>Add button is disabled at baseline; enabled when the row is complete.</summary>
    [ObservableProperty]
    private bool _isAddEnabled;

    /// <summary>Maximum quantity for the resolved From location.</summary>
    [ObservableProperty]
    private decimal? _maxQuantity;

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
    private Model_ScannerBatchItem? _selectedSessionItem;

    // ── Send confirmation ────────────────────────────────────────────────────────
    [ObservableProperty]
    private bool _isSendPromptVisible;

    private Model_ScannerBatchItem? _pendingSendItem;

    public bool HasActiveSession => CurrentSession is not null;

    public bool HasSelectedSessionItem => SelectedSessionItem is not null;

    public string OwnerUserId { get; } = Environment.UserName;

    public string OwnerDisplayName { get; } = Environment.UserName;

    public TimeSpan TransferConfirmTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan TransferConfirmPollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Raised so the view can show the stock-location picker (Step 6b-a1-a).</summary>
    public event Func<string, string, string, Task<Model_ScannerStockPick?>>? FromLocationInventoryPickerRequested;

    /// <summary>Raised so the view can return focus to the Part lookup (Step 6b-a2).</summary>
    public event Action? PartIdFocusRequested;

    /// <summary>Raised so the view can enable, focus, and select all on the To Location (Step 6b-a1-c).</summary>
    public event Action? ToLocationFocusRequested;

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
    }

    private void OnExecutionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IService_ScannerExecution.IsAutomationRunning))
        {
            IsPartEditable = !_executionService.IsAutomationRunning;
            IsToLocationEnabled = !_executionService.IsAutomationRunning && IsToLocationEnabled;
            IsQuantityEnabled = !_executionService.IsAutomationRunning && IsQuantityEnabled;
            RecomputeAddEnabled();
        }
    }

    partial void OnNewToLocationChanged(string value)
    {
        RecomputeAddEnabled();
    }

    private void RecomputeAddEnabled()
    {
        IsAddEnabled = !_executionService.IsAutomationRunning
            && CurrentSession is not null
            && !string.IsNullOrWhiteSpace(NewToLocation);
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
        if (profiles is not null && profiles.Success && profiles.Data is { Count: > 0 })
        {
            return (profiles.Data.FirstOrDefault(profile => profile.IsDefaultForUser)
                ?? profiles.Data[0]).ProfileId;
        }

        return Guid.Empty;
    }

    public async Task<Model_ScannerProfile> ResolveActiveProfileAsync()
    {
        var profiles = await _workflowService.GetProfilesAsync(OwnerUserId);
        if (profiles is not null && profiles.Success && profiles.Data is { Count: > 0 })
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
    public async Task PartValidationCompletedAsync(string partId)
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
            await ShowNoStockErrorAsync();
            return;
        }

        var stock = await _validationService.GetLocationsWithStockAsync(NewPartId, NewFromWarehouse);
        if (!stock.Success || stock.Data is null || stock.Data.Count == 0)
        {
            await ShowNoStockErrorAsync();
            return;
        }

        // Step 6b-a1-a: the view shows the stock-location modal.
        if (FromLocationInventoryPickerRequested is null)
        {
            await ShowNoStockErrorAsync();
            return;
        }

        var picked = await FromLocationInventoryPickerRequested(
            NewPartId,
            NewFromWarehouse,
            NewFromLocation
        );

        if (picked is null || string.IsNullOrWhiteSpace(picked.Location))
        {
            // Step 6b-a1-b: user cancelled the modal -> treat as no stock.
            await ShowNoStockErrorAsync();
            return;
        }

        await ApplyStockLocationSelectionAsync(picked);
    }

    private async Task ApplyStockLocationSelectionAsync(Model_ScannerStockPick pick)
    {
        // Step 6b-a1-c: fill From Loc + Qty, then enable/focus To Location.
        NewFromLocation = pick.Location;
        if (decimal.TryParse(pick.Quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
        {
            SetFromQuantityLimit(quantity);
        }
        else
        {
            ClearFromQuantityLimit();
            quantity = 0m;
        }

        NewQuantity = quantity.ToString("0.####", CultureInfo.InvariantCulture);

        IsToLocationEnabled = true;
        NewToLocation = string.Empty;
        RecomputeAddEnabled();
        ToLocationFocusRequested?.Invoke();

        ShowStatus("Source location resolved. Enter the destination location.", InfoBarSeverity.Success);
        await Task.CompletedTask;
    }

    private async Task ShowNoStockErrorAsync()
    {
        ShowHeaderError("The part number does not have any quantity in-house.");
        PartIdFocusRequested?.Invoke();
        await Task.CompletedTask;
    }

    // ── Step 6b-a1-c-a: To Location format sanitization ──────────────────────────

    /// <summary>
    /// Sanitizes the To Location on focus-lost (Step 6b-a1-c-b). Returns true when the
    /// format is valid; on failure shows a non-blocking header error and re-focuses.
    /// </summary>
    public async Task<bool> ValidateToLocationFormatAsync()
    {
        var rawValue = NewToLocation?.Trim() ?? string.Empty;
        var sanitized = Helper_ScannerLocationFormat.SanitizeLocationFormat(rawValue);

        if (!string.IsNullOrEmpty(sanitized))
        {
            NewToLocation = sanitized;
            RecomputeAddEnabled();
            return true;
        }

        ShowHeaderError("Invalid location layout entered. Please check your format.");
        await Task.CompletedTask;
        return false;
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

    public async Task<Model_ScannerLocationValidationResult> ValidateFromLocationAsync()
    {
        var validation = await _validationService.ValidateLocationAsync(
            NewFromLocation,
            NewFromWarehouse
        );

        if (!validation.Success || validation.Data is null)
        {
            return new Model_ScannerLocationValidationResult
            {
                IsValid = false,
                Message = string.IsNullOrWhiteSpace(validation.ErrorMessage)
                    ? "Location validation is currently unavailable."
                    : validation.ErrorMessage,
            };
        }

        if (validation.Data.IsValid)
        {
            NewFromLocation = validation.Data.CanonicalLocation;
        }

        return validation.Data;
    }

    public async Task<Model_ScannerLocationValidationResult> ValidateToLocationAsync()
    {
        var validation = await _validationService.ValidateLocationAsync(NewToLocation, NewToWarehouse);

        if (!validation.Success || validation.Data is null)
        {
            return new Model_ScannerLocationValidationResult
            {
                IsValid = false,
                Message = string.IsNullOrWhiteSpace(validation.ErrorMessage)
                    ? "Location validation is currently unavailable."
                    : validation.ErrorMessage,
            };
        }

        if (validation.Data.IsValid)
        {
            NewToLocation = validation.Data.CanonicalLocation;
        }

        return validation.Data;
    }

    public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetFromLocationSuggestionsAsync()
    {
        return _validationService.GetLocationSuggestionsAsync(NewFromLocation, NewFromWarehouse);
    }

    public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetToLocationSuggestionsAsync()
    {
        return _validationService.GetLocationSuggestionsAsync(NewToLocation, NewToWarehouse);
    }

    public Task<Model_Dao_Result<IReadOnlyList<Model_InforVisualMaterialLocationRow>>> GetFromInventoryLocationsAsync()
    {
        return _validationService.GetLocationsWithStockAsync(NewPartId, NewFromWarehouse);
    }

    public void SetFromQuantityLimit(decimal available)
    {
        MaxQuantity = available;
        IsQuantityEnabled = true;
    }

    public void ClearFromQuantityLimit()
    {
        IsQuantityEnabled = false;
        MaxQuantity = null;
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

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (!IsAddEnabled)
        {
            return;
        }

        if (CurrentSession is null)
        {
            ShowStatus("The current list is not ready.", InfoBarSeverity.Warning);
            return;
        }

        var item = new Model_ScannerBatchItem
        {
            SessionId = CurrentSession.SessionId,
            SequenceNumber = CurrentSession.Items.Count + 1,
            PayloadPartId = NewPartId,
            PayloadFromWarehouse = NewFromWarehouse,
            PayloadFromLocation = NewFromLocation,
            PayloadToWarehouse = NewToWarehouse,
            PayloadToLocation = NewToLocation,
            PayloadQuantity = NewQuantity,
        };

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
            LastValidationStatus = validation.Data.State.ToString();
            LastValidationNotes = validation.Data.Notes;
        }
        else
        {
            item.ValidationState = Enum_ScannerValidationState.Invalid;
            item.ValidationMessage = "Validation unavailable.";
            item.ValidationNotes =
                string.IsNullOrWhiteSpace(validation.ErrorMessage)
                    ? "Validation failed due to service error."
                    : validation.ErrorMessage;
            LastValidationStatus = item.ValidationState.ToString();
            LastValidationNotes = item.ValidationNotes;
        }

        var save = await _workflowService.UpsertBatchItemAsync(CurrentSession, item);
        if (!save.Success || save.Data is null)
        {
            ShowStatus(
                string.IsNullOrWhiteSpace(save.ErrorMessage)
                    ? "Unable to stage scanner item."
                    : save.ErrorMessage,
                InfoBarSeverity.Error
            );
            return;
        }

        CurrentSession = save.Data;
        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
        SelectedSessionItem = SessionItems.LastOrDefault();

        ResetEntryFields();
        ShowStatus(
            item.ValidationState == Enum_ScannerValidationState.Valid
                ? "Item added to the current list."
                : "Item added with validation issues. Review Status/Notes before sending.",
            item.ValidationState == Enum_ScannerValidationState.Valid
                ? InfoBarSeverity.Success
                : InfoBarSeverity.Warning
        );
    }

    private void ResetEntryFields()
    {
        NewPartId = string.Empty;
        NewFromLocation = string.Empty;
        NewToLocation = string.Empty;
        NewQuantity = "1";
        ClearFromQuantityLimit();
        IsToLocationEnabled = false;
        RecomputeAddEnabled();
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

    [RelayCommand(CanExecute = nameof(CanSendNext))]
    private async Task SendNextAsync()
    {
        if (CurrentSession is null || CurrentSession.Items.Count == 0)
        {
            return;
        }

        if (IsSendPromptVisible)
        {
            return;
        }

        if (CurrentSession.StopRequested)
        {
            CurrentSession.StopRequested = false;
            CurrentSession.StopReason = Enum_ScannerStopReason.UserStop;
            CurrentSession.Status = Enum_ScannerSessionStatus.Stopped;
            ShowStatus("Stop requested. The current list remains intact.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var profile = await ResolveActiveProfileAsync();
            var result = await _executionService.SendNextItemAsync(CurrentSession, profile);

            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to send the next scanner item."
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

            var sentItem = FindNextSentItem();
            if (result.Data.SentCount > 0 && sentItem is not null)
            {
                _pendingSendItem = sentItem;
                await WaitForTransferConfirmationAsync(sentItem);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Model_ScannerBatchItem? FindNextSentItem()
    {
        return CurrentSession?.Items
            .Where(item => item.ExecutionState == Enum_ScannerExecutionState.Sent)
            .OrderByDescending(item => item.SentUtc)
            .FirstOrDefault();
    }

    private async Task WaitForTransferConfirmationAsync(Model_ScannerBatchItem item)
    {
        var deadlineUtc = DateTime.UtcNow + TransferConfirmTimeout;
        var afterUtc = item.SentUtc ?? DateTime.UtcNow.AddSeconds(-1);

        if (!decimal.TryParse(item.PayloadQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
        {
            quantity = 0m;
        }

        ShowStatus("Verifying transfer in Infor Visual...", InfoBarSeverity.Informational);

        while (DateTime.UtcNow < deadlineUtc)
        {
            var check = await _validationService.TransferSavedSinceAsync(
                item.PayloadPartId,
                item.PayloadFromWarehouse,
                item.PayloadFromLocation,
                item.PayloadToWarehouse,
                item.PayloadToLocation,
                quantity,
                afterUtc
            );

            if (check.Success && check.Data)
            {
                await ConfirmSavedAsync();
                return;
            }

            await Task.Delay(TransferConfirmPollInterval);
        }

        IsSendPromptVisible = true;
    }

    private bool CanSendNext()
    {
        return !IsSendPromptVisible
            && !IsBusy
            && CurrentSession?.Items.Count > 0;
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
                CurrentSession.Items.Remove(itemToRemove);
                ReorderSessionItems();

                var persist = await _workflowService.ReplaceSessionItemsAsync(CurrentSession);
                if (persist.Success && persist.Data is not null)
                {
                    CurrentSession = persist.Data;
                }

                SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
                ShowStatus($"Line {savedItem.SequenceNumber} saved to history.", InfoBarSeverity.Success);
            }
        }
    }

    [RelayCommand]
    private async Task ConfirmNotSavedAsync()
    {
        if (_pendingSendItem is not null)
        {
            _pendingSendItem.ExecutionState = Enum_ScannerExecutionState.Waiting;
            _pendingSendItem.LastUpdatedUtc = DateTime.UtcNow;
        }

        _pendingSendItem = null;
        IsSendPromptVisible = false;

        await _executionService.ClearTargetFormAsync();
        PartIdFocusRequested?.Invoke();
        ShowStatus("Form cleared. Re-enter the part and try again.", InfoBarSeverity.Warning);
    }

    [RelayCommand]
    private async Task CheckAllAsync()
    {
        if (CurrentSession is null || CurrentSession.Items.Count == 0)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var validation = await _validationService.ValidateSessionItemsAsync(CurrentSession);
            if (!validation.Success || validation.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(validation.ErrorMessage)
                        ? "Unable to validate scanner items."
                        : validation.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
            SelectedSessionItem = SessionItems.FirstOrDefault(candidate =>
                SelectedSessionItem is not null && candidate.ItemId == SelectedSessionItem.ItemId);

            var invalidCount = validation.Data.Count(result => result.State == Enum_ScannerValidationState.Invalid);
            ShowStatus(
                invalidCount == 0
                    ? $"Validated {validation.Data.Count} scanner items. All items are ready for send review."
                    : $"Validated {validation.Data.Count} scanner items. {invalidCount} item(s) still require review.",
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
    private Task ClearListAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("The current list is already empty.", InfoBarSeverity.Informational);
            return Task.CompletedTask;
        }

        CurrentSession.Items.Clear();
        CurrentSession.RecalculateItemCounters();
        SessionItems = [];
        SelectedSessionItem = null;
        ResetEntryFields();
        ShowStatus("Current list cleared. Add items to build a new list.", InfoBarSeverity.Informational);
        return Task.CompletedTask;
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
        _ = SendNextCommand.ExecuteAsync(null);
    }
}

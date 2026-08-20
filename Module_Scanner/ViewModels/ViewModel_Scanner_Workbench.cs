using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Views;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.ViewModels;

/// <summary>
/// Placeholder ViewModel for the scanner workbench page.
/// </summary>
public partial class ViewModel_Scanner_Workbench : ViewModel_Shared_Base
{
    private readonly IService_ScannerNavigation _navigationService;
    private readonly IService_ScannerWorkflow _workflowService;
    private readonly IService_ScannerValidation _validationService;
    private readonly IService_ScannerExecution _executionService;
    private readonly IService_ScannerHotkey _hotkeyService;
    private readonly IService_Window _windowService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveSession))]
    private Model_ScannerBatchSession? _currentSession;

    [ObservableProperty]
    private ObservableCollection<Model_ScannerBatchItem> _sessionItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSessionItem))]
    private Model_ScannerBatchItem? _selectedSessionItem;

    private Model_ScannerProfile? _activeProfile;

    [ObservableProperty]
    private string _ownerUserId = Environment.UserName;

    [ObservableProperty]
    private string _ownerDisplayName = Environment.UserName;

    [ObservableProperty]
    private string _appWindowTitleSnapshot = "Inventory Transfers";

    [ObservableProperty]
    private string _appWindowClassSnapshot = string.Empty;

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

    public bool HasActiveSession => CurrentSession is not null;

    public bool HasSelectedSessionItem => SelectedSessionItem is not null;

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
    }

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
    private async Task StartDraftSessionAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _workflowService.StartSessionAsync(new Model_ScannerSessionStartRequest
            {
                OwnerUserId = OwnerUserId,
                OwnerDisplayName = OwnerDisplayName,
                AppWindowTitleSnapshot = AppWindowTitleSnapshot,
                AppWindowClassSnapshot = AppWindowClassSnapshot,
            });

            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to start scanner session."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            CurrentSession = new Model_ScannerBatchSession
            {
                SessionId = result.Data.SessionId,
                OwnerUserId = OwnerUserId,
                OwnerDisplayName = OwnerDisplayName,
                AppWindowTitleSnapshot = AppWindowTitleSnapshot,
                AppWindowClassSnapshot = AppWindowClassSnapshot,
                Status = result.Data.Status,
                CreatedUtc = result.Data.CreatedUtc,
                LastUpdatedUtc = result.Data.CreatedUtc,
                SessionName = $"{OwnerDisplayName} Draft {result.Data.CreatedUtc:yyyy-MM-dd HH:mm:ss}",
            };
            SessionItems = [];
            ShowStatus(result.Data.Message, InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddDraftItemAsync()
    {
        if (!await EnsureSessionAsync())
        {
            return;
        }

        if (CurrentSession is null)
        {
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

        var validation = await _validationService.ValidateNewItemAsync(new Model_ScannerItemValidationRequest
        {
            SessionId = CurrentSession.SessionId,
            ItemId = item.ItemId,
            PartId = item.PayloadPartId,
            FromWarehouse = item.PayloadFromWarehouse,
            FromLocation = item.PayloadFromLocation,
            ToWarehouse = item.PayloadToWarehouse,
            ToLocation = item.PayloadToLocation,
            Quantity = item.PayloadQuantity,
        });

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

        var severity = item.ValidationState == Enum_ScannerValidationState.Valid
            ? InfoBarSeverity.Success
            : InfoBarSeverity.Warning;
        var message = item.ValidationState == Enum_ScannerValidationState.Valid
            ? "Item added to draft session."
            : "Item added with validation issues. Review Status/Notes before sending.";
        ShowStatus(message, severity);
    }

    [RelayCommand]
    private async Task RemoveSelectedSessionItemAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before removing items.", InfoBarSeverity.Warning);
            return;
        }

        if (SelectedSessionItem is null)
        {
            ShowStatus("Select an item before removing.", InfoBarSeverity.Warning);
            return;
        }

        var itemToRemove = CurrentSession.Items.FirstOrDefault(candidate =>
            candidate.ItemId == SelectedSessionItem.ItemId
        );
        if (itemToRemove is null)
        {
            ShowStatus("The selected item could not be found in the session.", InfoBarSeverity.Warning);
            return;
        }

        CurrentSession.Items.Remove(itemToRemove);

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

    [RelayCommand]
    private async Task BuildRunSnapshotAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before sending items.", InfoBarSeverity.Warning);
            return;
        }

        if (CurrentSession.Items.Count == 0)
        {
            ShowStatus("Add at least one item before sending.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            CurrentSession.Status = Enum_ScannerSessionStatus.Running;
            CurrentSession.LastSendStartedUtc = DateTime.UtcNow;

            var run = await _workflowService.BuildRunSnapshotAsync(CurrentSession);
            if (!run.Success || run.Data is null)
            {
                CurrentSession.Status = Enum_ScannerSessionStatus.Failed;
                CurrentSession.LastFailureMessage = run.ErrorMessage ?? "Failed to create run snapshot.";
                ShowStatus(CurrentSession.LastFailureMessage, InfoBarSeverity.Error);
                return;
            }

            CurrentSession.Status = run.Data.FailedItems > 0
                ? Enum_ScannerSessionStatus.Failed
                : Enum_ScannerSessionStatus.Completed;
            CurrentSession.LastSendEndedUtc = run.Data.EndedUtc ?? DateTime.UtcNow;
            CurrentSession.LastFailureMessage = run.Data.FailureSummary;
            ShowStatus(
                $"Run snapshot created. Sent: {run.Data.SentItems}, Failed: {run.Data.FailedItems}, Waiting: {run.Data.WaitingItems}.",
                run.Data.FailedItems > 0 ? InfoBarSeverity.Warning : InfoBarSeverity.Success
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task ManageItemsDialogAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before managing the batch.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        return ManageItemsDialogCoreAsync();
    }

    private async Task ManageItemsDialogCoreAsync()
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            ShowStatus("Unable to open Manage Items because the window root is unavailable.", InfoBarSeverity.Warning);
            return;
        }

        if (CurrentSession is null)
        {
            ShowStatus("Start a session before managing the batch.", InfoBarSeverity.Warning);
            return;
        }

        var dialog = new View_Scanner_ManageItemsDialog(CurrentSession, _validationService)
        {
            XamlRoot = xamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);

        var result = await dialog.ShowAsync();
        if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
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
        CurrentSession.LastUpdatedUtc = DateTime.UtcNow;
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
    private async Task CheckAllAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start or load a session before validating items.", InfoBarSeverity.Warning);
            return;
        }

        if (CurrentSession.Items.Count == 0)
        {
            ShowStatus("Add at least one item before validating the batch.", InfoBarSeverity.Warning);
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

            if (SelectedSessionItem is not null)
            {
                SelectedSessionItem = SessionItems.FirstOrDefault(candidate => candidate.ItemId == SelectedSessionItem.ItemId);
            }

            var invalidCount = validation.Data.Count(result => result.State == Enum_ScannerValidationState.Invalid);
            var firstIssue = validation.Data.FirstOrDefault(result => result.State == Enum_ScannerValidationState.Invalid)
                ?? validation.Data.FirstOrDefault();

            if (firstIssue is not null)
            {
                LastValidationStatus = firstIssue.State.ToString();
                LastValidationNotes = firstIssue.Notes;
            }

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
    private async Task SendNextAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before sending items.", InfoBarSeverity.Warning);
            return;
        }

        if (CurrentSession.Items.Count == 0)
        {
            ShowStatus("Add at least one item before sending.", InfoBarSeverity.Warning);
            return;
        }

        if (IsBusy)
        {
            return;
        }

        // A pending stop is acknowledged on the next explicit send so the operator controls
        // exactly when the batch resumes after stopping between send cycles.
        if (CurrentSession.StopRequested)
        {
            CurrentSession.StopRequested = false;
            CurrentSession.StopReason = Enum_ScannerStopReason.UserStop;
            CurrentSession.Status = Enum_ScannerSessionStatus.Stopped;
            ShowStatus("Stop requested. The current batch remains intact.", InfoBarSeverity.Warning);
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

            RefreshSessionAfterExecution(result.Data);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SendAllAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before sending items.", InfoBarSeverity.Warning);
            return;
        }

        if (CurrentSession.Items.Count == 0)
        {
            ShowStatus("Add at least one item before sending.", InfoBarSeverity.Warning);
            return;
        }

        if (IsBusy)
        {
            return;
        }

        if (CurrentSession.StopRequested)
        {
            CurrentSession.StopRequested = false;
            CurrentSession.StopReason = Enum_ScannerStopReason.UserStop;
            CurrentSession.Status = Enum_ScannerSessionStatus.Stopped;
            ShowStatus("Stop requested. The current batch remains intact.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var profile = await ResolveActiveProfileAsync();
            var result = await _executionService.SendAllAsync(CurrentSession, profile);

            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to send the scanner batch."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            RefreshSessionAfterExecution(result.Data);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopAfterThisAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before requesting a stop.", InfoBarSeverity.Warning);
            return;
        }

        var result = await _executionService.RequestStopAsync(CurrentSession);
        if (!result.Success)
        {
            ShowStatus(
                string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Unable to request a stop."
                    : result.ErrorMessage,
                InfoBarSeverity.Error
            );
            return;
        }

        CurrentSession.Status = CurrentSession.Status == Enum_ScannerSessionStatus.Running
            ? Enum_ScannerSessionStatus.Stopped
            : CurrentSession.Status;
        ShowStatus("Stop requested. The current batch will stop after the next completed item.", InfoBarSeverity.Warning);
    }

    [RelayCommand]
    private Task ClearHistoryAsync()
    {
        CurrentSession = null;
        SessionItems = [];
        SelectedSessionItem = null;
        ShowStatus("Workbench state cleared. Start a new scanner session to continue.", InfoBarSeverity.Informational);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task ExportAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before exporting the current batch.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        var exportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var exportPath = Path.Combine(
            exportDirectory,
            $"scanner-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt"
        );

        var lines = new List<string>
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

    private async Task<bool> EnsureSessionAsync()
    {
        if (CurrentSession is not null)
        {
            return true;
        }

        await StartDraftSessionAsync();
        return CurrentSession is not null;
    }

    // ── Hotkey integration ──────────────────────────────────────────────────────

    /// <summary>
    /// Subscribes the workbench to the global scanner hotkeys. Called from the page Loaded
    /// event so subscription lifetime matches the page lifetime and never leaks.
    /// </summary>
    public void Activate() => SubscribeHotkeys();

    /// <summary>Unsubscribes the workbench from the global scanner hotkeys. Called from page Unloaded.</summary>
    public void Deactivate() => UnsubscribeHotkeys();

    private void SubscribeHotkeys()
    {
        _hotkeyService.SendShortcutPressed -= OnSendShortcutPressed;
        _hotkeyService.SendShortcutPressed += OnSendShortcutPressed;
        _hotkeyService.StopShortcutPressed -= OnStopShortcutPressed;
        _hotkeyService.StopShortcutPressed += OnStopShortcutPressed;
    }

    private void UnsubscribeHotkeys()
    {
        _hotkeyService.SendShortcutPressed -= OnSendShortcutPressed;
        _hotkeyService.StopShortcutPressed -= OnStopShortcutPressed;
    }

    private void OnSendShortcutPressed(object? sender, EventArgs e)
    {
        // Fire and forget: the command reports its own status. Runs on the UI thread because
        // WM_HOTKEY is dispatched through the window message loop.
        _ = SendNextCommand.ExecuteAsync(null);
    }

    private void OnStopShortcutPressed(object? sender, EventArgs e)
    {
        _ = StopAfterThisCommand.ExecuteAsync(null);
    }

    private async Task<Model_ScannerProfile> ResolveActiveProfileAsync()
    {
        if (_activeProfile is not null)
        {
            return _activeProfile;
        }

        var profiles = await _workflowService.GetProfilesAsync(OwnerUserId);
        var resolved =
            profiles is { Success: true, Data: { Count: > 0 } }
                ? profiles.Data.FirstOrDefault(profile => profile.IsDefaultForUser)
                    ?? profiles.Data[0]
                : new Model_ScannerProfile
                {
                    OwnerUserId = OwnerUserId,
                    ProfileName = "Default",
                    TargetExecutableName = "VMINVENT.exe",
                    AppWindowTitle = AppWindowTitleSnapshot,
                    TargetChildWindowTitle = AppWindowTitleSnapshot,
                    FromWarehouseDefault = NewFromWarehouse,
                    ToWarehouseDefault = NewToWarehouse,
                };

        _activeProfile = resolved;
        return resolved;
    }

    private void RefreshSessionAfterExecution(Model_ScannerExecutionOutcome outcome)
    {
        if (CurrentSession is null)
        {
            return;
        }

        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];

        if (outcome.FailedCount > 0)
        {
            CurrentSession.Status = Enum_ScannerSessionStatus.Failed;
            ShowStatus(
                string.IsNullOrWhiteSpace(outcome.FailureMessage)
                    ? "A scanner item failed to send."
                    : outcome.FailureMessage,
                InfoBarSeverity.Error
            );
            return;
        }

        if (outcome.Stopped)
        {
            CurrentSession.Status = Enum_ScannerSessionStatus.Stopped;
            ShowStatus(
                outcome.SentCount > 0
                    ? $"Sent {outcome.SentCount} scanner item(s), then stopped at the operator request."
                    : "Stop requested. The current batch remains intact.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (outcome.SentCount > 0)
        {
            CurrentSession.Status = CurrentSession.WaitingItems > 0
                ? Enum_ScannerSessionStatus.Running
                : Enum_ScannerSessionStatus.Completed;
            ShowStatus(
                CurrentSession.WaitingItems > 0
                    ? $"Sent {outcome.SentCount} scanner item(s). More items remain waiting."
                    : $"Sent {outcome.SentCount} scanner item(s). The batch is complete.",
                CurrentSession.WaitingItems > 0
                    ? InfoBarSeverity.Informational
                    : InfoBarSeverity.Success
            );
            return;
        }

        CurrentSession.Status = Enum_ScannerSessionStatus.Ready;
        ShowStatus(
            string.IsNullOrWhiteSpace(outcome.FailureMessage)
                ? "No waiting items are ready to send yet."
                : outcome.FailureMessage,
            InfoBarSeverity.Warning
        );
    }

    partial void OnSelectedSessionItemChanged(Model_ScannerBatchItem? value)
    {
        RemoveSelectedSessionItemCommand.NotifyCanExecuteChanged();
    }
}
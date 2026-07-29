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
using MTM_Receiving_Application.Module_Core.Models.Enums;
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
    private readonly IService_Window _windowService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveSession))]
    private Model_ScannerBatchSession? _currentSession;

    [ObservableProperty]
    private ObservableCollection<Model_ScannerBatchItem> _sessionItems = [];

    [ObservableProperty]
    private Model_ScannerBatchItem? _selectedSessionItem;

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

    public ViewModel_Scanner_Workbench(
        IService_ScannerNavigation navigationService,
        IService_ScannerWorkflow workflowService,
        IService_ScannerValidation validationService,
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
        ArgumentNullException.ThrowIfNull(windowService);
        _navigationService = navigationService;
        _workflowService = workflowService;
        _validationService = validationService;
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

        var dialog = new View_Scanner_ManageItemsDialog(CurrentSession)
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
    private Task SendNextAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before sending items.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        if (CurrentSession.Items.Count == 0)
        {
            ShowStatus("Add at least one item before sending.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        var nextItem = CurrentSession.Items
            .OrderBy(item => item.SequenceNumber)
            .FirstOrDefault(item => item.ExecutionState == Enum_ScannerExecutionState.Waiting && item.ValidationState == Enum_ScannerValidationState.Valid);

        if (nextItem is null)
        {
            CurrentSession.Status = Enum_ScannerSessionStatus.Ready;
            ShowStatus("No waiting items are ready to send yet.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        if (CurrentSession.StopRequested)
        {
            CurrentSession.StopRequested = false;
            CurrentSession.Status = Enum_ScannerSessionStatus.Stopped;
            CurrentSession.StopReason = Enum_ScannerStopReason.UserStop;
            ShowStatus("Stop requested. The current batch remains intact.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        nextItem.ExecutionState = Enum_ScannerExecutionState.Sent;
        nextItem.SentUtc = DateTime.UtcNow;
        nextItem.LastUpdatedUtc = DateTime.UtcNow;
        CurrentSession.RecalculateItemCounters();
        CurrentSession.LastSendEndedUtc = DateTime.UtcNow;
        CurrentSession.LastUpdatedUtc = DateTime.UtcNow;
        CurrentSession.Status = CurrentSession.WaitingItems > 0
            ? Enum_ScannerSessionStatus.Running
            : Enum_ScannerSessionStatus.Completed;

        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
        ShowStatus(
            CurrentSession.Status == Enum_ScannerSessionStatus.Completed
                ? "Sent the next scanner item. The batch is complete."
                : "Sent the next scanner item. More items remain waiting.",
            CurrentSession.Status == Enum_ScannerSessionStatus.Completed
                ? InfoBarSeverity.Success
                : InfoBarSeverity.Informational
        );

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task SendAllAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before sending items.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        if (CurrentSession.Items.Count == 0)
        {
            ShowStatus("Add at least one item before sending.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        var eligibleItems = CurrentSession.Items
            .Where(item => item.ExecutionState == Enum_ScannerExecutionState.Waiting && item.ValidationState == Enum_ScannerValidationState.Valid)
            .OrderBy(item => item.SequenceNumber)
            .ToList();

        if (eligibleItems.Count == 0)
        {
            CurrentSession.Status = Enum_ScannerSessionStatus.Ready;
            ShowStatus("No waiting items are ready to send yet.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        foreach (var item in eligibleItems)
        {
            if (CurrentSession.StopRequested)
            {
                break;
            }

            item.ExecutionState = Enum_ScannerExecutionState.Sent;
            item.SentUtc = DateTime.UtcNow;
            item.LastUpdatedUtc = DateTime.UtcNow;
        }

        CurrentSession.RecalculateItemCounters();
        CurrentSession.LastSendEndedUtc = DateTime.UtcNow;
        CurrentSession.LastUpdatedUtc = DateTime.UtcNow;
        CurrentSession.Status = CurrentSession.StopRequested
            ? Enum_ScannerSessionStatus.Stopped
            : CurrentSession.WaitingItems > 0
                ? Enum_ScannerSessionStatus.Running
                : Enum_ScannerSessionStatus.Completed;

        SessionItems = [.. CurrentSession.Items.OrderBy(candidate => candidate.SequenceNumber)];
        ShowStatus(
            CurrentSession.Status == Enum_ScannerSessionStatus.Completed
                ? $"Sent {eligibleItems.Count} scanner item(s). The batch is complete."
                : $"Sent {eligibleItems.Count} scanner item(s). More items remain waiting.",
            CurrentSession.Status == Enum_ScannerSessionStatus.Completed
                ? InfoBarSeverity.Success
                : InfoBarSeverity.Informational
        );

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task StopAfterThisAsync()
    {
        if (CurrentSession is null)
        {
            ShowStatus("Start a session before requesting a stop.", InfoBarSeverity.Warning);
            return Task.CompletedTask;
        }

        CurrentSession.StopRequested = true;
        CurrentSession.StopReason = Enum_ScannerStopReason.UserStop;
        CurrentSession.Status = CurrentSession.Status == Enum_ScannerSessionStatus.Running
            ? Enum_ScannerSessionStatus.Stopped
            : CurrentSession.Status;
        ShowStatus("Stop requested. The current batch will stop after the next completed item.", InfoBarSeverity.Warning);

        return Task.CompletedTask;
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
}
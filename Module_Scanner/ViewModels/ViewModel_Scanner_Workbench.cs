using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
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
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(workflowService);
        ArgumentNullException.ThrowIfNull(validationService);
        _navigationService = navigationService;
        _workflowService = workflowService;
        _validationService = validationService;
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
    private void ManageItems()
    {
        ShowStatus("Manage-items dialog implementation is the next scanner slice.", InfoBarSeverity.Informational);
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
    private void SendNext()
    {
        ShowStatus("Send-next execution wiring continues in the scanner automation slice.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private void SendAll()
    {
        ShowStatus("Send-all execution wiring continues in the scanner automation slice.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private void StopAfterThis()
    {
        ShowStatus("Stop request is applied between items only.", InfoBarSeverity.Warning);
    }

    [RelayCommand]
    private void ClearHistory()
    {
        ShowStatus("History clearing is not enabled from workbench in this slice.", InfoBarSeverity.Warning);
    }

    [RelayCommand]
    private void Export()
    {
        ShowStatus("Export wiring is planned for scanner history/export slice.", InfoBarSeverity.Informational);
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
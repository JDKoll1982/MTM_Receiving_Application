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
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the dedicated Customer Pull n' Pack waitlist queue.
/// </summary>
public partial class ViewModel_Tool_CustomerPullPackQueue : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_CustomerPullPackDemandSource _demandSource;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private ObservableCollection<Model_CustomerPullPack_WaitlistEntry> _queueItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUnassignOwner))]
    [NotifyPropertyChangedFor(nameof(HasSelectedQueueItem))]
    private Model_CustomerPullPack_WaitlistEntry? _selectedQueueItem;

    [ObservableProperty]
    private ObservableCollection<Model_CustomerPullPack_QueueLocationDetail> _selectedQueueLocationDetails =
    [];

    [ObservableProperty]
    private Model_CustomerPullPack_QueueLocationDetail? _selectedQueueLocationDetail;

    [ObservableProperty]
    private string _customerFilter = string.Empty;

    [ObservableProperty]
    private string _locationFilter = string.Empty;

    [ObservableProperty]
    private string _requesterFilter = string.Empty;

    [ObservableProperty]
    private string _ownerFilter = string.Empty;

    [ObservableProperty]
    private bool _includeRequested = true;

    [ObservableProperty]
    private bool _includeAccepted = true;

    [ObservableProperty]
    private bool _includeProblem = true;

    [ObservableProperty]
    private bool _includeCompleted;

    [ObservableProperty]
    private bool _includeCancelled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProblemSelected))]
    private Enum_CustomerPullPackWaitlistStatus _selectedStatus =
        Enum_CustomerPullPackWaitlistStatus.Accepted;

    [ObservableProperty]
    private Enum_CustomerPullPackProblemReason _selectedProblemReason =
        Enum_CustomerPullPackProblemReason.None;

    [ObservableProperty]
    private string _handlerNote = string.Empty;

    [ObservableProperty]
    private Enum_CustomerPullPackPrintMode _selectedPrintMode =
        Enum_CustomerPullPackPrintMode.WaitlistOnly;

    public IReadOnlyList<Enum_CustomerPullPackWaitlistStatus> EditableStatuses { get; } =
        Enum.GetValues<Enum_CustomerPullPackWaitlistStatus>();

    public IReadOnlyList<Enum_CustomerPullPackProblemReason> ProblemReasons { get; } =
        Enum.GetValues<Enum_CustomerPullPackProblemReason>();

    public IReadOnlyList<Enum_CustomerPullPackPrintMode> EditablePrintModes { get; } =
    [
        Enum_CustomerPullPackPrintMode.WaitlistOnly,
        Enum_CustomerPullPackPrintMode.PullList,
        Enum_CustomerPullPackPrintMode.SelectedContext,
    ];

    public Func<Model_CustomerPullPack_PrintContext, Task>? ShowPrintPreviewAsync { get; set; }

    public bool HasQueueItems => QueueItems.Count > 0;

    public bool HasSelectedQueueItem => SelectedQueueItem is not null;

    public bool HasSelectedQueueLocationDetails => SelectedQueueLocationDetails.Count > 0;

    public bool IsProblemSelected => SelectedStatus == Enum_CustomerPullPackWaitlistStatus.Problem;

    public bool CanUnassignOwner =>
        SelectedQueueItem is not null
        && string.IsNullOrWhiteSpace(SelectedQueueItem.CurrentOwnerUserId) is false
        && string.Equals(
            SelectedQueueItem.CurrentOwnerUserId,
            CurrentUserId,
            StringComparison.OrdinalIgnoreCase
        );

    public string CurrentUserId =>
        _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;

    public string CurrentUserDisplayName =>
        _sessionManager.CurrentSession?.User?.DisplayName ?? CurrentUserId;

    public ViewModel_Tool_CustomerPullPackQueue(
        IMediator mediator,
        IService_CustomerPullPackDemandSource demandSource,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _demandSource = demandSource;
        _sessionManager = sessionManager;
    }

    public async Task RefreshSelectedQueueLocationDetailsAsync()
    {
        if (SelectedQueueItem is null)
        {
            SelectedQueueLocationDetails = [];
            SelectedQueueLocationDetail = null;
            return;
        }

        var filter = new Model_CustomerPullPack_DemandFilter
        {
            CustomerId = SelectedQueueItem.CustomerId,
            DateFrom = SelectedQueueItem.RequestTimestamp.Date.AddYears(-1),
            DateTo = SelectedQueueItem.RequestTimestamp.Date.AddYears(1),
            SortMode = Enum_CustomerPullPackSortMode.PullDate,
        };

        var demandResult = await _demandSource.GetDemandAsync(filter);
        IEnumerable<Model_CustomerPullPack_QueueLocationDetail> details = [];

        if (demandResult.IsSuccess && demandResult.Data is not null)
        {
            details = demandResult
                .Data.Where(line =>
                    string.Equals(
                        line.CustomerOrderId,
                        SelectedQueueItem.CustomerOrderId,
                        StringComparison.OrdinalIgnoreCase
                    )
                    && string.Equals(
                        line.ParentPartId,
                        SelectedQueueItem.ParentPartId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .SelectMany(static line => line.LocationOptions)
                .GroupBy(static option => option.LocationId, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var firstOption = group.First();
                    return new Model_CustomerPullPack_QueueLocationDetail
                    {
                        ParentPartId = firstOption.ParentPartId,
                        LocationId = firstOption.LocationId,
                        OnHandQuantity = group.Max(static option => option.OnHandQuantity),
                    };
                });
        }

        if (!details.Any())
        {
            details = SelectedQueueItem.SelectedLocations.Select(
                locationId => new Model_CustomerPullPack_QueueLocationDetail
                {
                    ParentPartId = SelectedQueueItem.ParentPartId,
                    LocationId = locationId,
                    OnHandQuantity = 0,
                }
            );
        }

        SelectedQueueLocationDetails =
            new ObservableCollection<Model_CustomerPullPack_QueueLocationDetail>(
                details.OrderBy(
                    static detail => detail.LocationId,
                    StringComparer.OrdinalIgnoreCase
                )
            );
        SelectedQueueLocationDetail = SelectedQueueLocationDetails.FirstOrDefault();
        OnPropertyChanged(nameof(HasSelectedQueueLocationDetails));
    }

    public async Task ActivateViewAsync()
    {
        ShowStatus(
            "Review Requested, Accepted, and Problem waitlist work from the dedicated queue.",
            InfoBarSeverity.Informational
        );
        await LoadUserDefaultsAsync();
        await RefreshQueueAsync();
    }

    public async Task LoadUserDefaultsAsync()
    {
        var result = await _mediator.Send(new Query_CustomerPullPackDefaults(CurrentUserId));
        if (result is null || !result.IsSuccess || result.Data is null)
        {
            return;
        }

        IncludeRequested = result.Data.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Requested
        );
        IncludeAccepted = result.Data.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Accepted
        );
        IncludeProblem = result.Data.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Problem
        );
        IncludeCompleted = result.Data.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Completed
        );
        IncludeCancelled = result.Data.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Cancelled
        );
        SelectedPrintMode = result.Data.DefaultPrintPreset;
    }

    [RelayCommand]
    private async Task RefreshQueueAsync()
    {
        try
        {
            IsBusy = true;
            var statusSet = BuildStatusSet();
            var useDefaultOpenWork =
                IncludeRequested
                && IncludeAccepted
                && IncludeProblem
                && !IncludeCompleted
                && !IncludeCancelled;

            var result = await _mediator.Send(
                new Query_CustomerPullPackWaitlistQueue(
                    CustomerId: CustomerFilter.Trim(),
                    RequesterUserId: RequesterFilter.Trim(),
                    CurrentOwnerUserId: OwnerFilter.Trim(),
                    LocationId: LocationFilter.Trim(),
                    StatusSet: statusSet,
                    UseDefaultOpenWork: useDefaultOpenWork,
                    MaxResults: 250
                )
            );

            if (!result.IsSuccess || result.Data is null)
            {
                QueueItems = [];
                SelectedQueueItem = null;
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            QueueItems = new ObservableCollection<Model_CustomerPullPack_WaitlistEntry>(
                result.Data
            );
            SelectedQueueItem = QueueItems.FirstOrDefault();
            ShowStatus($"Loaded {QueueItems.Count} waitlist item(s).", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RefreshQueueAsync),
                nameof(ViewModel_Tool_CustomerPullPackQueue)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveStatusUpdateAsync()
    {
        if (SelectedQueueItem is null)
        {
            ShowStatus(
                "Select a queue item before saving status changes.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _mediator.Send(
                new Command_CustomerPullPackUpdateStatus(
                    SelectedQueueItem.WaitlistId,
                    SelectedStatus,
                    CurrentUserId,
                    CurrentUserDisplayName,
                    SelectedProblemReason,
                    HandlerNote
                )
            );

            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ShowStatus(
                $"Saved {result.Data.CustomerOrderId} as {result.Data.CurrentStatus}.",
                InfoBarSeverity.Success
            );
            var waitlistId = result.Data.WaitlistId;
            await RefreshQueueAsync();
            RestoreSelection(waitlistId);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveStatusUpdateAsync),
                nameof(ViewModel_Tool_CustomerPullPackQueue)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UnassignOwnerAsync()
    {
        if (SelectedQueueItem is null)
        {
            ShowStatus(
                "Select a queue item before un-assigning ownership.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            var waitlistId = SelectedQueueItem.WaitlistId;
            var result = await _mediator.Send(
                new Command_CustomerPullPackUnassignOwner(
                    waitlistId,
                    CurrentUserId,
                    CurrentUserDisplayName
                )
            );

            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ShowStatus(
                "Ownership cleared for the selected waitlist item.",
                InfoBarSeverity.Success
            );
            await RefreshQueueAsync();
            RestoreSelection(waitlistId);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(UnassignOwnerAsync),
                nameof(ViewModel_Tool_CustomerPullPackQueue)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PrintSelectedModeAsync()
    {
        if (ShowPrintPreviewAsync is null)
        {
            ShowStatus(
                "Print preview is not available in the current waitlist host.",
                InfoBarSeverity.Warning
            );
            return;
        }

        var selectedEntries =
            SelectedPrintMode == Enum_CustomerPullPackPrintMode.SelectedContext
                ? SelectedQueueItem is null
                    ? []
                    : [SelectedQueueItem]
                : QueueItems.ToList();
        if (selectedEntries.Count == 0)
        {
            ShowStatus("Load queue work before opening a print preview.", InfoBarSeverity.Warning);
            return;
        }

        var printResult = await _mediator.Send(
            new Query_CustomerPullPackPrintContext(
                SelectedPrintMode,
                CustomerFilter,
                string.Empty,
                $"Owner: {OwnerFilter} | Requester: {RequesterFilter} | Status filters applied",
                [],
                selectedEntries,
                string.Empty
            )
        );

        if (!printResult.IsSuccess || printResult.Data is null)
        {
            ShowStatus(printResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        await ShowPrintPreviewAsync(printResult.Data);
    }

    partial void OnSelectedQueueItemChanged(Model_CustomerPullPack_WaitlistEntry? value)
    {
        if (value is null)
        {
            HandlerNote = string.Empty;
            SelectedProblemReason = Enum_CustomerPullPackProblemReason.None;
            SelectedStatus = Enum_CustomerPullPackWaitlistStatus.Accepted;
            SelectedQueueLocationDetails = [];
            SelectedQueueLocationDetail = null;
            OnPropertyChanged(nameof(HasSelectedQueueLocationDetails));
            return;
        }

        HandlerNote = value.HandlerNote;
        SelectedProblemReason = value.ProblemReason;
        SelectedStatus = value.CurrentStatus;
    }

    private List<Enum_CustomerPullPackWaitlistStatus> BuildStatusSet()
    {
        var statuses = new List<Enum_CustomerPullPackWaitlistStatus>();

        if (IncludeRequested)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Requested);
        }

        if (IncludeAccepted)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Accepted);
        }

        if (IncludeProblem)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Problem);
        }

        if (IncludeCompleted)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Completed);
        }

        if (IncludeCancelled)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Cancelled);
        }

        return statuses;
    }

    private void RestoreSelection(string waitlistId)
    {
        if (string.IsNullOrWhiteSpace(waitlistId))
        {
            return;
        }

        SelectedQueueItem =
            QueueItems.FirstOrDefault(item =>
                string.Equals(item.WaitlistId, waitlistId, StringComparison.OrdinalIgnoreCase)
            ) ?? QueueItems.FirstOrDefault();
    }
}

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
/// ViewModel for the scanner History page (prior sessions).
/// </summary>
public partial class ViewModel_Scanner_History : ViewModel_Shared_Base
{
    private readonly IService_ScannerNavigation _navigationService;
    private readonly IService_ScannerWorkflow _workflowService;

    [ObservableProperty]
    private ObservableCollection<Model_ScannerHistoryEntry> _entries = [];

    [ObservableProperty]
    private ObservableCollection<Model_ScannerHistoryItem> _selectedEntryItems = [];

    [ObservableProperty]
    private Model_ScannerHistoryEntry? _selectedEntry;

    [ObservableProperty]
    private string _ownerUserId = Environment.UserName;

    [ObservableProperty]
    private DateTimeOffset _dateFromUtc = DateTimeOffset.UtcNow.AddDays(-30);

    [ObservableProperty]
    private DateTimeOffset _dateToUtc = DateTimeOffset.UtcNow;

    [ObservableProperty]
    private Enum_ScannerSessionStatus? _statusFilter;

    [ObservableProperty]
    private double _maxResults = 100;

    public ObservableCollection<Enum_ScannerSessionStatus?> StatusOptions { get; } =
    [
        null,
        Enum_ScannerSessionStatus.Ready,
        Enum_ScannerSessionStatus.Running,
        Enum_ScannerSessionStatus.Completed,
        Enum_ScannerSessionStatus.Failed,
        Enum_ScannerSessionStatus.Stopped,
    ];

    public ViewModel_Scanner_History(
        IService_ScannerNavigation navigationService,
        IService_ScannerWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(workflowService);
        _navigationService = navigationService;
        _workflowService = workflowService;
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
    public async Task RefreshHistoryAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _workflowService.GetHistoryAsync(new Model_ScannerHistoryQueryRequest
            {
                OwnerUserId = OwnerUserId,
                DateFromUtc = DateFromUtc.UtcDateTime,
                DateToUtc = DateToUtc.UtcDateTime,
                StatusFilter = StatusFilter,
                MaxResults = MaxResults <= 0 ? 100 : Convert.ToInt32(MaxResults),
            });

            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to load scanner history."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            Entries =
            [
                .. result
                    .Data.Entries.OrderByDescending(entry => entry.StartedUtc)
            ];
            SelectedEntry = Entries.FirstOrDefault();
            ShowStatus($"Loaded {Entries.Count} scanner history records.", InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        StatusFilter = null;
        DateFromUtc = DateTimeOffset.UtcNow.AddDays(-30);
        DateToUtc = DateTimeOffset.UtcNow;
        MaxResults = 100;
        ShowStatus("History filters reset.", InfoBarSeverity.Informational);
    }

    partial void OnSelectedEntryChanged(Model_ScannerHistoryEntry? value)
    {
        SelectedEntryItems = value is null
            ? []
            : [.. value.Items.OrderBy(item => item.SequenceNumber)];
    }
}

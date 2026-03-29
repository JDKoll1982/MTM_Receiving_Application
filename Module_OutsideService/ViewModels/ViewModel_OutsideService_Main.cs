using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.ViewModels;

/// <summary>
/// Shell ViewModel for the Outside Service module.
/// </summary>
public partial class ViewModel_OutsideService_Main
    : ViewModel_Shared_Base,
        IViewModel_HeaderTitleProvider
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRequestEntryVisible))]
    [NotifyPropertyChangedFor(nameof(IsWaitlistVisible))]
    [NotifyPropertyChangedFor(nameof(IsSetupVisible))]
    [NotifyPropertyChangedFor(nameof(IsHistoryVisible))]
    [NotifyPropertyChangedFor(nameof(CurrentSectionTitle))]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]
    private string _currentSection = "Waitlist";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSetupEnabled))]
    private Model_OutsideServiceRequestLine? _selectedWaitlistLine;

    public ViewModel_OutsideService_Main(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        Title = "Outside Service";
    }

    /// <summary>
    /// Gets the current shell header title.
    /// </summary>
    public string CurrentSectionTitle =>
        CurrentSection switch
        {
            "Waitlist" => "Outside Service - Active Waitlist",
            "RequestEntry" => "Outside Service - Request Entry",
            "Setup" => "Outside Service - Setup",
            "History" => "Outside Service - Complete History",
            _ => "Outside Service",
        };

    public string CurrentHeaderTitle => CurrentSectionTitle;

    /// <summary>
    /// Gets whether the request-entry section is visible.
    /// </summary>
    public bool IsRequestEntryVisible => CurrentSection == "RequestEntry";

    /// <summary>
    /// Gets whether the active waitlist section is visible.
    /// </summary>
    public bool IsWaitlistVisible => CurrentSection == "Waitlist";

    /// <summary>
    /// Gets whether the setup section is visible.
    /// </summary>
    public bool IsSetupVisible => CurrentSection == "Setup";

    /// <summary>
    /// Gets whether the history section is visible.
    /// </summary>
    public bool IsHistoryVisible => CurrentSection == "History";

    /// <summary>
    /// Gets whether the Setup tab can be opened.
    /// </summary>
    public bool IsSetupEnabled => SelectedWaitlistLine is not null;

    partial void OnSelectedWaitlistLineChanged(Model_OutsideServiceRequestLine? value)
    {
        if (value is null && IsSetupVisible)
        {
            CurrentSection = "Waitlist";
        }
    }

    public void UpdateSelectedWaitlistLine(Model_OutsideServiceRequestLine? line)
    {
        SelectedWaitlistLine = line;
    }

    [RelayCommand]
    private void ShowRequestEntry()
    {
        CurrentSection = "RequestEntry";
    }

    [RelayCommand]
    private void ShowWaitlist()
    {
        CurrentSection = "Waitlist";
    }

    [RelayCommand]
    private void ShowSetup()
    {
        if (!IsSetupEnabled)
        {
            return;
        }

        CurrentSection = "Setup";
    }

    [RelayCommand]
    private void ShowHistory()
    {
        CurrentSection = "History";
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.ViewModels;

/// <summary>
/// Shell ViewModel for the Outside Service module.
/// </summary>
public partial class ViewModel_OutsideService_Main : ViewModel_Shared_Base
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRequestEntryVisible))]
    [NotifyPropertyChangedFor(nameof(IsWaitlistVisible))]
    [NotifyPropertyChangedFor(nameof(IsSetupVisible))]
    [NotifyPropertyChangedFor(nameof(IsHistoryVisible))]
    private string _currentSection = "RequestEntry";

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
        CurrentSection = "Setup";
    }

    [RelayCommand]
    private void ShowHistory()
    {
        CurrentSection = "History";
    }
}

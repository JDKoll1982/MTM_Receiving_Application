using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for core theme settings.
/// </summary>
public partial class ViewModel_Settings_Theme : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _statusMessage = "Theme controls will be available here.";

    public ViewModel_Settings_Theme(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
    }
}

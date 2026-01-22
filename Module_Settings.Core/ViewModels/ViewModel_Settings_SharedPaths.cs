using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for shared path settings.
/// </summary>
public partial class ViewModel_Settings_SharedPaths : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _statusMessage = "Shared path defaults will be managed here.";

    public ViewModel_Settings_SharedPaths(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for user/privilege settings.
/// </summary>
public partial class ViewModel_Settings_Users : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _statusMessage = "User/Privilege management is managed by Core Settings roles.";

    public ViewModel_Settings_Users(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
    }
}

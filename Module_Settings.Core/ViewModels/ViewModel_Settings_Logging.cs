using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for logging settings.
/// </summary>
public partial class ViewModel_Settings_Logging : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _statusMessage = "Logging defaults will be configurable here.";

    public ViewModel_Settings_Logging(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for the Core Settings window shell.
/// </summary>
public partial class ViewModel_SettingsWindow : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _title = "Core Settings";

    public ViewModel_SettingsWindow(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
    }
}

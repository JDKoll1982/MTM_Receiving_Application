using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for database settings.
/// </summary>
public partial class ViewModel_Settings_Database : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _statusMessage = "Database configuration defaults are managed through Core Settings.";

    public ViewModel_Settings_Database(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
    }
}

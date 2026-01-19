using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_SettingsOverview : ViewModel_Shared_Base
{
    public ViewModel_Settings_Dunnage_SettingsOverview(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        Title = "Dunnage Settings";
    }
}

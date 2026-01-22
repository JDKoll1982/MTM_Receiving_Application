using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

public sealed partial class ViewModel_Settings_Routing_UserPreferences : ViewModel_Shared_Base
{
    public ViewModel_Settings_Routing_UserPreferences(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        Title = "Routing User Preferences";
    }
}

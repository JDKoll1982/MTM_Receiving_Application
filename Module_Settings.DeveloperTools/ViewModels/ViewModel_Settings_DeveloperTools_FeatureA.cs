using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

public sealed partial class ViewModel_Settings_DeveloperTools_FeatureA : ViewModel_Shared_Base
{
    public ViewModel_Settings_DeveloperTools_FeatureA(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
    }
}

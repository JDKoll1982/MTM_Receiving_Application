using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

public sealed partial class ViewModel_Settings_Reporting_Permissions : ViewModel_Shared_Base
{
    public ViewModel_Settings_Reporting_Permissions(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        Title = "Reporting Permissions";
    }
}

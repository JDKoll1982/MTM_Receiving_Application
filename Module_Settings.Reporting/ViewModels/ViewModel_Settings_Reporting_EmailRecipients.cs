using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

public sealed partial class ViewModel_Settings_Reporting_EmailRecipients
    : ViewModel_Settings_EmailRecipientsEditorBase
{
    public ViewModel_Settings_Reporting_EmailRecipients(
        IService_ReportingRecipientSettings recipientSettingsService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(recipientSettingsService, errorHandler, logger, notificationService)
    {
        Title = "Reporting Email Recipients";
    }
}

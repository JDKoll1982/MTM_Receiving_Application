using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Contracts;

namespace MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

public sealed partial class ViewModel_Settings_Volvo_EmailRecipients
    : ViewModel_Settings_EmailRecipientsEditorBase
{
    public ViewModel_Settings_Volvo_EmailRecipients(
        IService_VolvoRecipientSettings recipientSettingsService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(recipientSettingsService, errorHandler, logger, notificationService)
    {
        Title = "Volvo Email Recipients";
    }
}

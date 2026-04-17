using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

public sealed partial class ViewModel_Settings_Volvo_NavigationHub
    : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Volvo_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Volvo Settings";
        CurrentStepTitle = NavigationTitle;
    }

    public void Save() { }

    public void Reset() { }

    public void Cancel() { }

    public void Back() { }

    public void Next() { }
}

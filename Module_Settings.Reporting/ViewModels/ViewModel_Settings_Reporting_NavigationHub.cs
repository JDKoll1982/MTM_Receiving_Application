using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

public sealed partial class ViewModel_Settings_Reporting_NavigationHub : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Reporting_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Reporting Navigation";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_Reporting_SettingsOverview)),
            new Model_SettingsNavigationStep("File I/O", typeof(Views.View_Settings_Reporting_FileIO)),
            new Model_SettingsNavigationStep("Email / UX", typeof(Views.View_Settings_Reporting_EmailUx)),
            new Model_SettingsNavigationStep("Business Rules", typeof(Views.View_Settings_Reporting_BusinessRules)),
            new Model_SettingsNavigationStep("Permissions", typeof(Views.View_Settings_Reporting_Permissions)));
    }

    public void Save() { }
    public void Reset() { }
    public void Cancel() { }
    public void Back() { }
    public void Next() { }
}

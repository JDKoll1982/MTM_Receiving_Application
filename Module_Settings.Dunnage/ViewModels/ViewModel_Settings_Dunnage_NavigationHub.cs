using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_NavigationHub : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Dunnage_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Dunnage Navigation";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_Dunnage_SettingsOverview)),
            new Model_SettingsNavigationStep("User Preferences", typeof(Views.View_Settings_Dunnage_UserPreferences)),
            new Model_SettingsNavigationStep("UI/UX", typeof(Views.View_Settings_Dunnage_UiUx)),
            new Model_SettingsNavigationStep("Workflow", typeof(Views.View_Settings_Dunnage_Workflow)),
            new Model_SettingsNavigationStep("Permissions", typeof(Views.View_Settings_Dunnage_Permissions)),
            new Model_SettingsNavigationStep("Audit", typeof(Views.View_Settings_Dunnage_Audit)));
    }

    public void Save() { }
    public void Reset() { }
    public void Cancel() { }
    public void Back() { }
    public void Next() { }
}

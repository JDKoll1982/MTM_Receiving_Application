using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

public sealed partial class ViewModel_Settings_DeveloperTools_NavigationHub : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_DeveloperTools_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Developer Tools";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_DeveloperTools_SettingsOverview)),
            new Model_SettingsNavigationStep("Feature A", typeof(Views.View_Settings_DeveloperTools_FeatureA)),
            new Model_SettingsNavigationStep("Feature B", typeof(Views.View_Settings_DeveloperTools_FeatureB)),
            new Model_SettingsNavigationStep("Feature C", typeof(Views.View_Settings_DeveloperTools_FeatureC)),
            new Model_SettingsNavigationStep("Feature D", typeof(Views.View_Settings_DeveloperTools_FeatureD)),
            new Model_SettingsNavigationStep("Database Test", typeof(Views.View_SettingsDeveloperTools_DatabaseTest)));
    }

    public void Save() { }
    public void Reset() { }
    public void Cancel() { }
    public void Back() { }
    public void Next() { }
}

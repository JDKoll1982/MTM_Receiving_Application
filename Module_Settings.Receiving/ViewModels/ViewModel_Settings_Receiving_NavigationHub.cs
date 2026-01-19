using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_NavigationHub : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Receiving_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(pagination, errorHandler, logger)
    {
        NavigationTitle = "Receiving Navigation";
        CurrentStepTitle = NavigationTitle;

        // Placeholder steps until the Receiving settings pages are implemented.
        SetSteps(
            new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_Receiving_SettingsOverview)),
            new Model_SettingsNavigationStep("Defaults", typeof(Views.View_Settings_Receiving_Defaults)),
            new Model_SettingsNavigationStep("Validation", typeof(Views.View_Settings_Receiving_Validation)),
            new Model_SettingsNavigationStep("Preferences", typeof(Views.View_Settings_Receiving_UserPreferences)),
            new Model_SettingsNavigationStep("Business Rules", typeof(Views.View_Settings_Receiving_BusinessRules)),
            new Model_SettingsNavigationStep("ERP Integration", typeof(Views.View_Settings_Receiving_Integrations)));
    }

    public new void Save()
    {
    }

    public new void Reset()
    {
    }

    public new void Cancel()
    {
    }

    public new void Back()
    {
    }

    public new void Next()
    {
    }
}

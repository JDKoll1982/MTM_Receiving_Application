using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

public sealed partial class ViewModel_Settings_Routing_NavigationHub : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Routing_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Routing Navigation";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_Routing_SettingsOverview)),
            new Model_SettingsNavigationStep("File I/O", typeof(Views.View_Settings_Routing_FileIO)),
            new Model_SettingsNavigationStep("UI/UX", typeof(Views.View_Settings_Routing_UiUx)),
            new Model_SettingsNavigationStep("Business Rules", typeof(Views.View_Settings_Routing_BusinessRules)),
            new Model_SettingsNavigationStep("Resilience", typeof(Views.View_Settings_Routing_Resilience)),
            new Model_SettingsNavigationStep("User Preferences", typeof(Views.View_Settings_Routing_UserPreferences)));
    }

    public void Save() { }
    public void Reset() { }
    public void Cancel() { }
    public void Back() { }
    public void Next() { }
}

using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

public sealed partial class ViewModel_Settings_Volvo_NavigationHub : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Volvo_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(pagination, errorHandler, logger)
    {
        NavigationTitle = "Volvo Navigation";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep("Overview", typeof(Views.View_Settings_Volvo_SettingsOverview)),
            new Model_SettingsNavigationStep("Database Settings", typeof(Views.View_Settings_Volvo_DatabaseSettings)),
            new Model_SettingsNavigationStep("Connection Strings", typeof(Views.View_Settings_Volvo_ConnectionStrings)),
            new Model_SettingsNavigationStep("File Paths", typeof(Views.View_Settings_Volvo_FilePaths)),
            new Model_SettingsNavigationStep("UI Configuration", typeof(Views.View_Settings_Volvo_UiConfiguration)),
            new Model_SettingsNavigationStep("Hardcoded to Externalize", typeof(Views.View_Settings_Volvo_ExternalizationBacklog)));
    }

    public new void Save() { }
    public new void Reset() { }
    public new void Cancel() { }
    public new void Back() { }
    public new void Next() { }
}

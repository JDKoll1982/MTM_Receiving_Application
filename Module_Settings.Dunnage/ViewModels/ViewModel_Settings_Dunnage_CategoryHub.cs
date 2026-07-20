using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_CategoryHub
    : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Dunnage_CategoryHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Dunnage Settings";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep(
                "Personal Defaults",
                typeof(Views.View_Settings_Dunnage_PersonalDefaults)
            ),
            new Model_SettingsNavigationStep(
                "Image Assets",
                typeof(Views.View_Settings_Dunnage_ImageAssets)
            ),
            new Model_SettingsNavigationStep(
                "Image Presentation",
                typeof(Views.View_Settings_Dunnage_ImagePresentation)
            ),
            new Model_SettingsNavigationStep(
                "Workflow Visuals",
                typeof(Views.View_Settings_Dunnage_WorkflowVisuals)
            ),
            new Model_SettingsNavigationStep(
                "Keyboard Shortcuts",
                typeof(Views.View_Settings_Dunnage_KeyboardShortcuts)
            )
        );
    }

    public void Save() { }

    public void Reset() { }

    public void Cancel() { }

    public void Back() { }

    public void Next() { }
}

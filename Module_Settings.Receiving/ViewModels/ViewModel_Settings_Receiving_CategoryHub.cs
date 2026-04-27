using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_CategoryHub
    : ViewModel_SettingsNavigationHubBase
{
    public ViewModel_Settings_Receiving_CategoryHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Receiving Settings";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep(
                "Entry Defaults",
                typeof(Views.View_Settings_Receiving_EntryDefaults)
            ),
            new Model_SettingsNavigationStep(
                "Validation Rules",
                typeof(Views.View_Settings_Receiving_ValidationRules)
            ),
            new Model_SettingsNavigationStep(
                "Part Formatting",
                typeof(Views.View_Settings_Receiving_PartFormatting)
            ),
            new Model_SettingsNavigationStep(
                "Reconciliation",
                typeof(Views.View_Settings_Receiving_Reconciliation)
            ),
            new Model_SettingsNavigationStep(
                "Workflow Defaults",
                typeof(Views.View_Settings_Receiving_WorkflowDefaults)
            ),
            new Model_SettingsNavigationStep(
                "Keyboard Shortcuts",
                typeof(Views.View_Settings_Receiving_KeyboardShortcuts)
            )
        );
    }

    public void Save() { }

    public void Reset() { }

    public void Cancel() { }

    public void Back() { }

    public void Next() { }
}

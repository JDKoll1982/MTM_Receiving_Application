using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_NavigationHub
    : ViewModel_SettingsNavigationHubBase
{
    public ISettingsNavigationActions? CurrentActions { get; set; }

    public ViewModel_Settings_Receiving_NavigationHub(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(pagination, errorHandler, logger, notificationService)
    {
        NavigationTitle = "Receiving Navigation";
        CurrentStepTitle = NavigationTitle;

        SetSteps(
            new Model_SettingsNavigationStep(
                "Defaults",
                typeof(Views.View_Settings_Receiving_Defaults)
            ),
            new Model_SettingsNavigationStep(
                "Validation",
                typeof(Views.View_Settings_Receiving_Validation)
            ),
            new Model_SettingsNavigationStep(
                "Part Number Auto Padding",
                typeof(Views.View_Settings_Receiving_UserPreferences)
            ),
            new Model_SettingsNavigationStep(
                "Workflow Options",
                typeof(Views.View_Settings_Receiving_BusinessRules)
            )
        );
    }

    public async Task SaveAsync()
    {
        if (CurrentActions is null)
        {
            return;
        }

        await CurrentActions.SaveAsync();
    }

    public async Task ResetAsync()
    {
        if (CurrentActions is null)
        {
            return;
        }

        await CurrentActions.ResetAsync();
    }

    public async Task CancelAsync()
    {
        if (CurrentActions is null)
        {
            return;
        }

        await CurrentActions.CancelAsync();
    }

    public async Task BackAsync()
    {
        if (CurrentActions is null)
        {
            return;
        }

        await CurrentActions.BackAsync();
    }

    public async Task NextAsync()
    {
        if (CurrentActions is null)
        {
            return;
        }

        await CurrentActions.NextAsync();
    }
}

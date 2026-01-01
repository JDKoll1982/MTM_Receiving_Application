using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Settings;

/// <summary>
/// ViewModel for Settings Mode Selection - allows user to choose which settings area to access
/// </summary>
public partial class Settings_ModeSelectionViewModel : Shared_BaseViewModel
{
    private readonly IService_SettingsWorkflow _workflowService;

    public Settings_ModeSelectionViewModel(
        IService_SettingsWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }

    #region Navigation Commands

    [RelayCommand]
    private void SelectReceivingSettings()
    {
        _logger.LogInfo("User selected Receiving settings");
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.ReceivingSettings);
    }

    [RelayCommand]
    private void SelectDunnageSettings()
    {
        _logger.LogInfo("User selected Dunnage settings");
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.DunnageSettings);
    }

    [RelayCommand]
    private void SelectShippingSettings()
    {
        _logger.LogInfo("User selected Shipping settings");
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.ShippingSettings);
    }

    [RelayCommand]
    private void SelectVolvoSettings()
    {
        _logger.LogInfo("User selected Volvo settings");
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.VolvoSettings);
    }

    [RelayCommand]
    private void SelectAdministrativeSettings()
    {
        _logger.LogInfo("User selected Administrative settings");
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.AdministrativeSettings);
    }

    #endregion
}

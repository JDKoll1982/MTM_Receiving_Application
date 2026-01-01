using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Settings;

public partial class Settings_DunnageModeViewModel : Shared_BaseViewModel
{
    private readonly IService_SettingsWorkflow _workflowService;

    public Settings_DunnageModeViewModel(
        IService_SettingsWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }

    [RelayCommand]
    private void Back()
    {
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.ModeSelection);
    }

    [RelayCommand]
    private void SelectDunnageTypes()
    {
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.DunnageTypes);
    }

    [RelayCommand]
    private void SelectDunnageInventory()
    {
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.DunnageInventory);
    }
}

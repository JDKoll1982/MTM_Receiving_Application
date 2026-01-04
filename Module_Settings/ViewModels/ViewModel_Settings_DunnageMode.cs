using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Enums;
using MTM_Receiving_Application.Module_Settings.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.ViewModels;

public partial class ViewModel_Settings_DunnageMode : ViewModel_Shared_Base
{
    private readonly IService_SettingsWorkflow _workflowService;

    public ViewModel_Settings_DunnageMode(
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


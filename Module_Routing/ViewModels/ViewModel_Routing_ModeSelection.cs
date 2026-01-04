using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for the routing mode selection screen.
/// Allows users to choose between Wizard, Manual (Grid), and History modes.
/// </summary>
public partial class ViewModel_Routing_ModeSelection : ViewModel_Shared_Base
{
    private readonly IService_RoutingWorkflow _workflowService;

    [ObservableProperty]
    private bool _isWizardModeDefault;

    [ObservableProperty]
    private bool _isManualModeDefault;

    public ViewModel_Routing_ModeSelection(
        IService_RoutingWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }

    [RelayCommand]
    private async Task SelectWizardModeAsync()
    {
        _logger.LogInfo("User selected Wizard Mode.");
        // For now, Wizard mode can just go to Label Entry or a placeholder
        // Since we don't have a full wizard implementation yet, we'll route to LabelEntry but maybe with a different flag if needed
        // Or we can create a placeholder view for Wizard.
        // Let's route to Wizard step and let the Workflow ViewModel handle the view.
        _workflowService.GoToStep(Enum_Routing_WorkflowStep.Wizard);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SelectManualModeAsync()
    {
        _logger.LogInfo("User selected Manual Mode.");
        _workflowService.GoToStep(Enum_Routing_WorkflowStep.LabelEntry);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SelectHistoryModeAsync()
    {
        _logger.LogInfo("User selected History Mode.");
        _workflowService.GoToStep(Enum_Routing_WorkflowStep.History);
        await Task.CompletedTask;
    }

    // Placeholder commands for setting defaults - implementation would require user preferences service integration
    [RelayCommand]
    private void SetWizardAsDefault()
    {
        // TODO: Implement saving preference
        IsWizardModeDefault = true;
        IsManualModeDefault = false;
    }

    [RelayCommand]
    private void SetManualAsDefault()
    {
        // TODO: Implement saving preference
        IsManualModeDefault = true;
        IsWizardModeDefault = false;
    }
}

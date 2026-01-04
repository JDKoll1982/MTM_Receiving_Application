using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Enums;
using MTM_Receiving_Application.Module_Settings.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.ViewModels;

/// <summary>
/// Main workflow coordinator ViewModel for Settings
/// </summary>
public partial class ViewModel_Settings_Workflow : ViewModel_Shared_Base
{
    private readonly IService_SettingsWorkflow _workflowService;

    [ObservableProperty]
    private Enum_SettingsWorkflowStep _currentStep = Enum_SettingsWorkflowStep.ModeSelection;

    [ObservableProperty]
    private string _currentStepTitle = "Settings";

    [ObservableProperty]
    private bool _isAdminPageVisible;

    public ViewModel_Settings_Workflow(
        IService_SettingsWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _workflowService.StepChanged += OnStepChanged;
    }

    private void OnStepChanged(object? sender, Enum_SettingsWorkflowStep step)
    {
        CurrentStep = step;
        IsAdminPageVisible = step == Enum_SettingsWorkflowStep.DunnageTypes ||
                             step == Enum_SettingsWorkflowStep.DunnageInventory;

        CurrentStepTitle = step switch
        {
            Enum_SettingsWorkflowStep.ModeSelection => "Settings",
            Enum_SettingsWorkflowStep.ReceivingSettings => "Receiving Settings",
            Enum_SettingsWorkflowStep.DunnageSettings => "Dunnage Settings",
            Enum_SettingsWorkflowStep.ShippingSettings => "UPS / FedEx Settings",
            Enum_SettingsWorkflowStep.VolvoSettings => "Volvo Settings",
            Enum_SettingsWorkflowStep.AdministrativeSettings => "Administrative Settings",
            Enum_SettingsWorkflowStep.DunnageTypes => "Dunnage Types",
            Enum_SettingsWorkflowStep.DunnageInventory => "Dunnage Inventory",
            _ => "Settings",
        };
    }
}


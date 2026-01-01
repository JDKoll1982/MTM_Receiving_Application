using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using System;

namespace MTM_Receiving_Application.Services;

/// <summary>
/// Service for managing Settings workflow navigation
/// </summary>
public class Service_SettingsWorkflow : IService_SettingsWorkflow
{
    public event EventHandler<Enum_SettingsWorkflowStep>? StepChanged;

    private Enum_SettingsWorkflowStep _currentStep = Enum_SettingsWorkflowStep.ModeSelection;

    public void GoToStep(Enum_SettingsWorkflowStep step)
    {
        _currentStep = step;
        StepChanged?.Invoke(this, step);
    }

    public void GoBack()
    {
        // Return to mode selection
        GoToStep(Enum_SettingsWorkflowStep.ModeSelection);
    }

    public void Reset()
    {
        GoToStep(Enum_SettingsWorkflowStep.ModeSelection);
    }
}

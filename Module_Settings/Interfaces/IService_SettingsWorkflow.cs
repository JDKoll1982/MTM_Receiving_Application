using MTM_Receiving_Application.Module_Settings.Enums;
using System;

namespace MTM_Receiving_Application.Module_Settings.Interfaces;

/// <summary>
/// Service for managing Settings workflow navigation and state
/// </summary>
public interface IService_SettingsWorkflow
{
    /// <summary>
    /// Event raised when workflow step changes
    /// </summary>
    public event EventHandler<Enum_SettingsWorkflowStep>? StepChanged;

    /// <summary>
    /// Navigate to a specific step in the settings workflow
    /// </summary>
    /// <param name="step"></param>
    public void GoToStep(Enum_SettingsWorkflowStep step);

    /// <summary>
    /// Go back to the previous step
    /// </summary>
    public void GoBack();

    /// <summary>
    /// Reset workflow to initial state
    /// </summary>
    public void Reset();
}

using MTM_Receiving_Application.Models.Enums;
using System;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service for managing Settings workflow navigation and state
/// </summary>
public interface IService_SettingsWorkflow
{
    /// <summary>
    /// Event raised when workflow step changes
    /// </summary>
    event EventHandler<Enum_SettingsWorkflowStep>? StepChanged;

    /// <summary>
    /// Navigate to a specific step in the settings workflow
    /// </summary>
    void GoToStep(Enum_SettingsWorkflowStep step);

    /// <summary>
    /// Go back to the previous step
    /// </summary>
    void GoBack();

    /// <summary>
    /// Reset workflow to initial state
    /// </summary>
    void Reset();
}

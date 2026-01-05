using System;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for orchestrating the routing workflow state machine.
/// Manages step transitions and session state.
/// </summary>
public interface IService_RoutingWorkflow
{
    /// <summary>
    /// Event raised when a status message should be shown.
    /// </summary>
    public event EventHandler<string> StatusMessageRaised;

    /// <summary>
    /// Raises a status message.
    /// </summary>
    /// <param name="message"></param>
    public void RaiseStatusMessage(string message);

    /// <summary>
    /// Event raised when the workflow step changes.
    /// </summary>
    public event EventHandler StepChanged;

    /// <summary>
    /// Gets the current workflow step.
    /// </summary>
    public Enum_Routing_WorkflowStep CurrentStep { get; }

    /// <summary>
    /// Transitions to a specific workflow step.
    /// </summary>
    /// <param name="step"></param>
    public void GoToStep(Enum_Routing_WorkflowStep step);

    /// <summary>
    /// Resets the workflow to the initial state.
    /// </summary>
    public void ResetWorkflow();
}

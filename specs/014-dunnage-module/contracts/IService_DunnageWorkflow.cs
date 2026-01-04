using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for orchestrating the dunnage workflow state machine.
/// Manages step transitions, validation gates, and session state.
/// </summary>
public interface IService_DunnageWorkflow
{
    /// <summary>
    /// Gets the current workflow step.
    /// </summary>
    Enum_DunnageWorkflowStep CurrentStep { get; }

    /// <summary>
    /// Gets the current session.
    /// </summary>
    Model_DunnageSession CurrentSession { get; }

    /// <summary>
    /// Event raised when the workflow step changes.
    /// </summary>
    event EventHandler StepChanged;

    /// <summary>
    /// Event raised when a status message should be shown.
    /// </summary>
    event EventHandler<string> StatusMessageRaised;

    /// <summary>
    /// Starts a new dunnage workflow session.
    /// Loads any existing persisted session if available.
    /// </summary>
    /// <returns>True if existing session restored, false if new session</returns>
    Task<bool> StartWorkflowAsync();

    /// <summary>
    /// Advances to the next step if validation passes.
    /// </summary>
    /// <returns>Result indicating success and any validation errors</returns>
    Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();

    /// <summary>
    /// Goes to a specific step.
    /// </summary>
    /// <param name="step">Target step</param>
    void GoToStep(Enum_DunnageWorkflowStep step);

    /// <summary>
    /// Saves all loads in session to CSV and database.
    /// </summary>
    /// <returns>Result with save operation details</returns>
    Task<Model_SaveResult> SaveSessionAsync();

    /// <summary>
    /// Clears the current session.
    /// </summary>
    void ClearSession();

    /// <summary>
    /// Adds current load to session and resets for next entry.
    /// </summary>
    void AddCurrentLoadToSession();
}



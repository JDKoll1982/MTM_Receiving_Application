using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for orchestrating the receiving workflow state machine.
/// Manages step transitions, validation gates, and session state.
/// </summary>
public interface IService_ReceivingWorkflow
{
    /// <summary>
    /// Event raised when a status message should be shown.
    /// </summary>
    event EventHandler<string> StatusMessageRaised;

    /// <summary>
    /// Raises a status message.
    /// </summary>
    /// <param name="message">Status message to display</param>
    void RaiseStatusMessage(string message);

    /// <summary>
    /// Event raised when the workflow step changes.
    /// </summary>
    event EventHandler StepChanged;

    /// <summary>
    /// Gets the current workflow step.
    /// </summary>
    Enum_ReceivingWorkflowStep CurrentStep { get; }

    /// <summary>
    /// Gets the current session.
    /// </summary>
    Model_ReceivingSession CurrentSession { get; }

    /// <summary>
    /// Gets or sets the current PO number being processed.
    /// </summary>
    string? CurrentPONumber { get; set; }

    /// <summary>
    /// Gets or sets the current part being processed.
    /// </summary>
    Model_InforVisualPart? CurrentPart { get; set; }

    /// <summary>
    /// Gets or sets whether the current item is a non-PO item.
    /// </summary>
    bool IsNonPOItem { get; set; }

    /// <summary>
    /// Gets or sets the number of loads to create for the current part.
    /// </summary>
    int NumberOfLoads { get; set; }

    /// <summary>
    /// Starts a new receiving workflow session.
    /// Loads any existing persisted session if available.
    /// </summary>
    /// <returns>True if existing session restored, false if new session</returns>
    Task<bool> StartWorkflowAsync();

    /// <summary>
    /// Advances to the next step if validation passes.
    /// </summary>
    /// <returns>Result indicating success and any validation errors</returns>
    Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync();

    /// <summary>
    /// Goes back to the previous step.
    /// </summary>
    /// <returns>Result indicating success</returns>
    Model_ReceivingWorkflowStepResult GoToPreviousStep();

    /// <summary>
    /// Goes to a specific step (used for "Add Another Part/PO").
    /// Clears form data BEFORE navigation to fix known bug.
    /// </summary>
    /// <param name="step">Target step</param>
    /// <returns>Result indicating success</returns>
    Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step);

    /// <summary>
    /// Adds current loads to session and resets for next part entry.
    /// </summary>
    Task AddCurrentPartToSessionAsync();

    /// <summary>
    /// Saves all loads in session to CSV and database.
    /// </summary>
    /// <param name="messageProgress">Progress reporter for status messages</param>
    /// <param name="percentProgress">Progress reporter for percentage completion</param>
    /// <returns>Result with save operation details</returns>
    Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null);

    /// <summary>
    /// Resets the workflow to initial state.
    /// </summary>
    Task ResetWorkflowAsync();

    /// <summary>
    /// Resets the CSV files (deletes them).
    /// </summary>
    Task<Model_CSVDeleteResult> ResetCSVFilesAsync();

    /// <summary>
    /// Persists current session state to JSON.
    /// </summary>
    Task PersistSessionAsync();
}



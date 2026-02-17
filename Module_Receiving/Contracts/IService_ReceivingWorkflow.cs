using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Contracts
{
    /// <summary>
    /// Service for orchestrating the receiving workflow state machine.
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    public interface IService_ReceivingWorkflow
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
        public Enum_ReceivingWorkflowStep CurrentStep { get; }

        /// <summary>
        /// Gets the current session.
        /// </summary>
        public Model_ReceivingSession CurrentSession { get; }

        /// <summary>
        /// Gets or sets the current PO number being processed.
        /// </summary>
        public string? CurrentPONumber { get; set; }

        /// <summary>
        /// Gets or sets the current part being processed.
        /// </summary>
        public Model_InforVisualPart? CurrentPart { get; set; }

        /// <summary>
        /// Gets or sets whether the current item is a non-PO item.
        /// </summary>
        public bool IsNonPOItem { get; set; }

        /// <summary>
        /// Gets or sets the number of loads to create for the current part.
        /// </summary>
        public int NumberOfLoads { get; set; }

        /// <summary>
        /// Starts a new receiving workflow session.
        /// Loads any existing persisted session if available.
        /// </summary>
        /// <returns>True if existing session restored, false if new session</returns>
        public Task<bool> StartWorkflowAsync();

        /// <summary>
        /// Advances to the next step if validation passes.
        /// </summary>
        /// <returns>Result indicating success and any validation errors</returns>
        public Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync();

        /// <summary>
        /// Goes back to the previous step.
        /// </summary>
        /// <returns>Result indicating success</returns>
        public Model_ReceivingWorkflowStepResult GoToPreviousStep();

        /// <summary>
        /// Goes to a specific step (used for "Add Another Part/PO").
        /// </summary>
        /// <param name="step">Target step</param>
        /// <returns>Result indicating success</returns>
        public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step);

        /// <summary>
        /// Adds current loads to session and resets for next part entry.
        /// </summary>
        public Task AddCurrentPartToSessionAsync();

        /// <summary>
        /// Saves all loads in session to CSV and database.
        /// </summary>
        /// <param name="messageProgress">Progress reporter for status messages</param>
        /// <param name="percentProgress">Progress reporter for percentage completion</param>
        /// <returns>Result with save operation details</returns>
        public Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null);

        /// <summary>
        /// Clears all UI inputs in registered ViewModels.
        /// </summary>
        public void ClearUIInputs();

        /// <summary>
        /// Saves all loads in session to XLS only.
        /// </summary>
        public Task<Model_SaveResult> SaveToXLSOnlyAsync();

        /// <summary>
        /// Saves all loads in session to database only.
        /// </summary>
        public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();

        /// <summary>
        /// Resets the workflow to initial state.
        /// </summary>
        public Task ResetWorkflowAsync();

        ///  <summary>
        /// Resets the XLS files (deletes them).
        /// </summary>
        public Task<Model_XLSDeleteResult> ResetXLSFilesAsync();

        /// <summary>
        /// Persists current session state to JSON.
        /// </summary>
        public Task PersistSessionAsync();
    }
}

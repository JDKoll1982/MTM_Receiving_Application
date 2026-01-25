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
    /// OBSOLETE: Use CQRS commands/queries via MediatR instead (StartWorkflowCommand, NavigateToStepCommand, etc.).
    /// This interface is deprecated in favor of the consolidated 3-step workflow implemented via CQRS handlers.
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    [Obsolete("Use CQRS commands/queries (StartWorkflowCommand, NavigateToStepCommand, etc.) via MediatR. This 12-step wizard interface is replaced by the consolidated 3-step workflow.", DiagnosticId = "RECV001", UrlFormat = "https://github.com/JDKoll1982/MTM_Receiving_Application/wiki/CQRS-Migration")]
    public interface IService_Receiving_Infrastructure_Workflow
    {
        /// <summary>
        /// Event raised when a status message should be shown.
        /// OBSOLETE: Use MediatR handlers for state notifications.
        /// </summary>
        [Obsolete("Use MediatR handlers for status updates.", DiagnosticId = "RECV001")]
        public event EventHandler<string> StatusMessageRaised;

        /// <summary>
        /// Raises a status message.
        /// OBSOLETE: Use MediatR handlers for status updates.
        /// </summary>
        [Obsolete("Use MediatR handlers for status updates.", DiagnosticId = "RECV001")]
        public void RaiseStatusMessage(string message);

        /// <summary>
        /// Event raised when the workflow step changes.
        /// OBSOLETE: Use NavigateToStepCommand/GetSessionQuery for step state.
        /// </summary>
        [Obsolete("Use NavigateToStepCommand and GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public event EventHandler StepChanged;

        /// <summary>
        /// Gets the current workflow step.
        /// OBSOLETE: Use GetSessionQuery to retrieve current step.
        /// </summary>
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public Enum_ReceivingWorkflowStep CurrentStep { get; }

        /// <summary>
        /// Gets the current session.
        /// OBSOLETE: Use GetSessionQuery.
        /// </summary>
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public Model_ReceivingSession CurrentSession { get; }

        /// <summary>
        /// Gets or sets the current PO number being processed.
        /// OBSOLETE: Use UpdateStep1Command.
        /// </summary>
        [Obsolete("Use UpdateStep1Command via MediatR.", DiagnosticId = "RECV001")]
        public string? CurrentPONumber { get; set; }

        /// <summary>
        /// Gets or sets the current part being processed.
        /// OBSOLETE: Use GetPartLookupQuery and UpdateStep1Command.
        /// </summary>
        [Obsolete("Use GetPartLookupQuery and UpdateStep1Command via MediatR.", DiagnosticId = "RECV001")]
        public Model_InforVisualPart? CurrentPart { get; set; }

        /// <summary>
        /// Gets or sets whether the current item is a non-PO item.
        /// OBSOLETE: This is handled in UpdateStep1Command mode parameter.
        /// </summary>
        [Obsolete("Mode is handled in UpdateStep1Command.", DiagnosticId = "RECV001")]
        public bool IsNonPOItem { get; set; }

        /// <summary>
        /// Gets or sets the number of loads to create for the current part.
        /// OBSOLETE: Use UpdateStep1Command.
        /// </summary>
        [Obsolete("Use UpdateStep1Command via MediatR.", DiagnosticId = "RECV001")]
        public int NumberOfLoads { get; set; }

        /// <summary>
        /// Starts a new receiving workflow session.
        /// OBSOLETE: Use StartWorkflowCommand.
        /// </summary>
        [Obsolete("Use StartWorkflowCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task<bool> StartWorkflowAsync();

        /// <summary>
        /// Advances to the next step if validation passes.
        /// OBSOLETE: Use NavigateToStepCommand.
        /// </summary>
        [Obsolete("Use NavigateToStepCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync();

        /// <summary>
        /// Goes back to the previous step.
        /// OBSOLETE: Use NavigateToStepCommand with IsEditMode=true.
        /// </summary>
        [Obsolete("Use NavigateToStepCommand with IsEditMode=true via MediatR.", DiagnosticId = "RECV001")]
        public Model_ReceivingWorkflowStepResult GoToPreviousStep();

        /// <summary>
        /// Goes to a specific step (used for "Add Another Part/PO").
        /// OBSOLETE: Use NavigateToStepCommand.
        /// </summary>
        /// <param name="step">Target step</param>
        /// <returns>Result indicating success</returns>
        [Obsolete("Use NavigateToStepCommand via MediatR.", DiagnosticId = "RECV001")]
        public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step);

        /// <summary>
        /// Adds current loads to session and resets for next part entry.
        /// OBSOLETE: Use UpdateStep1Command and subsequent NavigateToStepCommand.
        /// </summary>
        public Task AddCurrentPartToSessionAsync();

        /// <summary>
        /// Saves all loads in session to CSV and database.
        /// OBSOLETE: Use SaveSessionCommand.
        /// </summary>
        /// <param name="messageProgress">Progress reporter for status messages</param>
        /// <param name="percentProgress">Progress reporter for percentage completion</param>
        /// <returns>Result with save operation details</returns>
        [Obsolete("Use SaveSessionCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null);

        /// <summary>
        /// Clears all UI inputs in registered ViewModels.
        /// OBSOLETE: Use ClearUIInputsCommand.
        /// </summary>
        [Obsolete("Use ClearUIInputsCommand via MediatR.", DiagnosticId = "RECV001")]
        public void ClearUIInputs();

        /// <summary>
        /// Saves all loads in session to CSV only.
        /// OBSOLETE: Use SaveToCSVOnlyCommand.
        /// </summary>
        [Obsolete("Use SaveToCSVOnlyCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task<Model_SaveResult> SaveToCSVOnlyAsync();

        /// <summary>
        /// Saves all loads in session to database only.
        /// OBSOLETE: Use SaveToDatabaseOnlyCommand.
        /// </summary>
        [Obsolete("Use SaveToDatabaseOnlyCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();

        /// <summary>
        /// Resets the workflow to initial state.
        /// OBSOLETE: Use ResetWorkflowCommand.
        /// </summary>
        [Obsolete("Use ResetWorkflowCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task ResetWorkflowAsync();

        /// <summary>
        /// Resets the CSV files (deletes them).
        /// OBSOLETE: Use ResetCSVFilesCommand.
        /// </summary>
        [Obsolete("Use ResetCSVFilesCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task<Model_CSVDeleteResult> ResetCSVFilesAsync();

        /// <summary>
        /// Persists current session state to JSON.
        /// OBSOLETE: Use PersistSessionCommand.
        /// </summary>
        [Obsolete("Use PersistSessionCommand via MediatR.", DiagnosticId = "RECV001")]
        public Task PersistSessionAsync();
    }
}

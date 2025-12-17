```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for orchestrating the receiving workflow state machine.
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    public interface IService_ReceivingWorkflow
    {
        /// <summary>
        /// Gets the current workflow step.
        /// </summary>
        WorkflowStep CurrentStep { get; }

        /// <summary>
        /// Gets the current session.
        /// </summary>
        Model_ReceivingSession CurrentSession { get; }

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
        Task<WorkflowStepResult> AdvanceToNextStepAsync();

        /// <summary>
        /// Goes back to the previous step.
        /// </summary>
        /// <returns>Result indicating success</returns>
        WorkflowStepResult GoToPreviousStep();

        /// <summary>
        /// Goes to a specific step (used for "Add Another Part/PO").
        /// </summary>
        /// <param name="step">Target step</param>
        /// <returns>Result indicating success</returns>
        WorkflowStepResult GoToStep(WorkflowStep step);

        /// <summary>
        /// Adds current loads to session and resets for next part entry.
        /// </summary>
        Task AddCurrentPartToSessionAsync();

        /// <summary>
        /// Saves all loads in session to CSV and database.
        /// </summary>
        /// <returns>Result with save operation details</returns>
        Task<SaveResult> SaveSessionAsync();

        /// <summary>
        /// Resets the workflow to initial state.
        /// </summary>
        Task ResetWorkflowAsync();

        /// <summary>
        /// Persists current session state to JSON.
        /// </summary>
        Task PersistSessionAsync();
    }

    /// <summary>
    /// Workflow steps enumeration.
    /// </summary>
    public enum WorkflowStep
    {
        CSVReset = 0,
        POEntry = 1,
        PartSelection = 2,
        LoadEntry = 3,
        WeightQuantityEntry = 4,
        HeatLotEntry = 5,
        PackageTypeEntry = 6,
        Review = 7,
        Saving = 8,
        Complete = 9
    }

    /// <summary>
    /// Result of a workflow step transition.
    /// </summary>
    public class WorkflowStepResult
    {
        public bool Success { get; set; }
        public WorkflowStep NewStep { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();

        public static WorkflowStepResult SuccessResult(WorkflowStep newStep, string message = "") => new()
        {
            Success = true,
            NewStep = newStep,
            Message = message
        };

        public static WorkflowStepResult ErrorResult(List<string> errors) => new()
        {
            Success = false,
            ValidationErrors = errors
        };
    }

    /// <summary>
    /// Result of save operation.
    /// </summary>
    public class SaveResult
    {
        public bool Success { get; set; }
        public int LoadsSaved { get; set; }
        public bool LocalCSVSuccess { get; set; }
        public bool NetworkCSVSuccess { get; set; }
        public bool DatabaseSuccess { get; set; }
        public string? LocalCSVPath { get; set; }
        public string? NetworkCSVPath { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public bool IsFullSuccess => LocalCSVSuccess && NetworkCSVSuccess && DatabaseSuccess;
        public bool IsPartialSuccess => (LocalCSVSuccess || DatabaseSuccess) && !IsFullSuccess;
    }
}
```

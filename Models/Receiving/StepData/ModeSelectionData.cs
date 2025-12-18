namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for Mode Selection step.
    /// Represents the workflow mode choice (Guided vs Manual).
    /// </summary>
    public class ModeSelectionData
    {
        /// <summary>
        /// Selected workflow mode.
        /// </summary>
        public WorkflowMode SelectedMode { get; set; } = WorkflowMode.None;
    }

    /// <summary>
    /// Workflow mode enumeration.
    /// </summary>
    public enum WorkflowMode
    {
        None = 0,
        Guided = 1,
        Manual = 2
    }
}

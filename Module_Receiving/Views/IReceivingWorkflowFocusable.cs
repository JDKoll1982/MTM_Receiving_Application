namespace MTM_Receiving_Application.Module_Receiving.Views
{
    /// <summary>
    /// Defines a workflow view that can move focus to its primary input when the step is accessed.
    /// </summary>
    public interface IReceivingWorkflowFocusable
    {
        /// <summary>
        /// Moves focus to the primary input for the current workflow step.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when focus was applied; <see langword="false"/> when the target input
        /// is not ready yet, which signals the host to retry after the next layout pass.
        /// </returns>
        bool FocusForAccess();
    }
}
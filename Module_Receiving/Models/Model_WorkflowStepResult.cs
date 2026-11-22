using MTM_Receiving_Application.Module_Dunnage.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    public class Model_WorkflowStepResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Non-blocking notice for a step that succeeded. Set when the step could not verify
        /// something against Infor Visual, so the operator still learns the value went through
        /// unchecked.
        /// </summary>
        public string WarningMessage { get; set; } = string.Empty;

        public Enum_DunnageWorkflowStep? TargetStep { get; set; }
    }
}

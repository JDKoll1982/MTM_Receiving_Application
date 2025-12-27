using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Models.Receiving
{
    public class Model_WorkflowStepResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Enum_DunnageWorkflowStep? TargetStep { get; set; }
    }
}

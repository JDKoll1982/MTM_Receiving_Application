
using MTM_Receiving_Application.Module_Dunnage.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    public class Model_Receiving_Result_WorkflowStepResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Enum_DunnageWorkflowStep? TargetStep { get; set; }
    }
}

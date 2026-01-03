
using MTM_Receiving_Application.DunnageModule.Enums;

namespace MTM_Receiving_Application.ReceivingModule.Models
{
    public class Model_WorkflowStepResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Enum_DunnageWorkflowStep? TargetStep { get; set; }
    }
}

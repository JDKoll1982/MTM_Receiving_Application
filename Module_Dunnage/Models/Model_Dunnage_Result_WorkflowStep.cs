using System.Collections.Generic;
using MTM_Receiving_Application.Module_Dunnage.Enums;

namespace MTM_Receiving_Application.Module_Dunnage.Models
{
    /// <summary>
    /// Result object for Dunnage workflow step operations.
    /// Module-specific implementation for Dunnage workflow.
    /// </summary>
    public class Model_Dunnage_Result_WorkflowStep
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Enum_DunnageWorkflowStep? TargetStep { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public static Model_Dunnage_Result_WorkflowStep SuccessResult(
            Enum_DunnageWorkflowStep targetStep,
            string message = "")
        {
            return new Model_Dunnage_Result_WorkflowStep
            {
                IsSuccess = true,
                TargetStep = targetStep,
                ErrorMessage = message
            };
        }

        public static Model_Dunnage_Result_WorkflowStep ErrorResult(List<string> errors)
        {
            return new Model_Dunnage_Result_WorkflowStep
            {
                IsSuccess = false,
                ErrorMessage = string.Join("; ", errors),
                ValidationErrors = errors,
                TargetStep = null
            };
        }
    }
}

using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    public class Model_Receiving_Result_WorkflowStepResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Enum_ReceivingWorkflowStep? TargetStep { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Factory method to create a successful workflow step result
        /// </summary>
        /// <param name="targetStep"></param>
        /// <param name="message"></param>
        public static Model_Receiving_Result_WorkflowStepResult SuccessResult(
            Enum_ReceivingWorkflowStep targetStep,
            string message = "")
        {
            return new Model_Receiving_Result_WorkflowStepResult
            {
                IsSuccess = true,
                TargetStep = targetStep,
                ErrorMessage = message
            };
        }

        /// <summary>
        /// Factory method to create a failed workflow step result
        /// </summary>
        /// <param name="errors"></param>
        public static Model_Receiving_Result_WorkflowStepResult ErrorResult(List<string> errors)
        {
            return new Model_Receiving_Result_WorkflowStepResult
            {
                IsSuccess = false,
                ErrorMessage = string.Join("; ", errors),
                ValidationErrors = errors,
                TargetStep = null
            };
        }
    }
}

using System.Collections.Generic;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// Result of a workflow step transition for Receiving.
    /// </summary>
    public class Model_ReceivingWorkflowStepResult
    {
        public bool Success { get; set; }
        public Enum_ReceivingWorkflowStep NewStep { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();

        public static Model_ReceivingWorkflowStepResult SuccessResult(Enum_ReceivingWorkflowStep newStep, string message = "") => new()
        {
            Success = true,
            NewStep = newStep,
            Message = message
        };

        public static Model_ReceivingWorkflowStepResult ErrorResult(List<string> errors) => new()
        {
            Success = false,
            ValidationErrors = errors
        };
    }
}

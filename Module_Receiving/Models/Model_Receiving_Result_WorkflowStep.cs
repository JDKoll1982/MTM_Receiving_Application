using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of a workflow step transition for Receiving.
    /// </summary>
    public class Model_Receiving_Result_WorkflowStep
    {
        public bool Success { get; set; }
        public Enum_ReceivingWorkflowStep NewStep { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();

        public static Model_Receiving_Result_WorkflowStep SuccessResult(Enum_ReceivingWorkflowStep newStep, string message = "") => new()
        {
            Success = true,
            NewStep = newStep,
            Message = message
        };

        public static Model_Receiving_Result_WorkflowStep ErrorResult(List<string> errors) => new()
        {
            Success = false,
            ValidationErrors = errors
        };
    }
}


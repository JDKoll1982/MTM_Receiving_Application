using System.Collections.Generic;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// Result of a receiving validation operation.
    /// </summary>
    public class Model_ReceivingValidationResult
    {
        public bool IsValid { get; set; }
        public Enum_ValidationSeverity Severity { get; set; } = Enum_ValidationSeverity.Error;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static Model_ReceivingValidationResult Success() => new() { IsValid = true };
        public static Model_ReceivingValidationResult Error(string message) => new()
        {
            IsValid = false,
            Severity = Enum_ValidationSeverity.Error,
            Message = message,
            Errors = new List<string> { message }
        };
        public static Model_ReceivingValidationResult Warning(string message) => new()
        {
            IsValid = true, // Warnings don't block
            Severity = Enum_ValidationSeverity.Warning,
            Message = message
        };
    }
}

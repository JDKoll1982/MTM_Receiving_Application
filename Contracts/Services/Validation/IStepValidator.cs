using System.Threading.Tasks;

namespace MTM_Receiving_Application.Contracts.Services.Validation
{
    /// <summary>
    /// Interface for step-specific validation logic.
    /// Enables composable, testable validation rules for workflow steps.
    /// </summary>
    /// <typeparam name="TInput">The input data type to validate</typeparam>
    public interface IStepValidator<TInput> where TInput : class
    {
        /// <summary>
        /// Validates the input data asynchronously.
        /// </summary>
        /// <param name="input">The input data to validate</param>
        /// <returns>Validation result with success flag and error messages</returns>
        Task<ValidationResult> ValidateAsync(TInput input);
    }

    /// <summary>
    /// Result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates if validation passed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Additional validation errors (for multiple validation failures).
        /// </summary>
        public System.Collections.Generic.List<string> Errors { get; set; } = new();

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static ValidationResult Success() => new ValidationResult { IsValid = true };

        /// <summary>
        /// Creates a failed validation result with error message.
        /// </summary>
        public static ValidationResult Failure(string errorMessage) => new ValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            Errors = new System.Collections.Generic.List<string> { errorMessage }
        };

        /// <summary>
        /// Creates a failed validation result with multiple errors.
        /// </summary>
        public static ValidationResult Failure(System.Collections.Generic.List<string> errors) => new ValidationResult
        {
            IsValid = false,
            ErrorMessage = string.Join("; ", errors),
            Errors = errors
        };
    }
}

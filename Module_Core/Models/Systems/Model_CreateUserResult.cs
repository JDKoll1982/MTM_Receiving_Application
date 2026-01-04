namespace MTM_Receiving_Application.Module_Core.Models.Systems
{
    /// <summary>
    /// Result of a user creation attempt.
    /// </summary>
    public class Model_CreateUserResult
    {
        public bool Success { get; set; }
        public int EmployeeNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static Model_CreateUserResult SuccessResult(int employeeNumber) => new()
        {
            Success = true,
            EmployeeNumber = employeeNumber
        };

        public static Model_CreateUserResult ErrorResult(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }
}


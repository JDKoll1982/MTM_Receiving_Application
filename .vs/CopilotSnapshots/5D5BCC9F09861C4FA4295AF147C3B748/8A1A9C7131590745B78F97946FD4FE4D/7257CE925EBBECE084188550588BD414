using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Core.Models.Systems
{
    /// <summary>
    /// Result of an authentication attempt.
    /// </summary>
    public class Model_AuthenticationResult
    {
        public bool Success { get; set; }
        public Model_User? User { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static Model_AuthenticationResult SuccessResult(Model_User user) => new()
        {
            Success = true,
            User = user
        };

        public static Model_AuthenticationResult ErrorResult(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }
}


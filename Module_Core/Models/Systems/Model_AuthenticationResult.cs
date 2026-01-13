using System;

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

        public static Model_AuthenticationResult SuccessResult(Model_User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return new Model_AuthenticationResult
            {
                Success = true,
                User = user,
                ErrorMessage = string.Empty
            };
        }

        public static Model_AuthenticationResult ErrorResult(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));
            }

            return new Model_AuthenticationResult
            {
                Success = false,
                User = null,
                ErrorMessage = message
            };
        }
    }
}


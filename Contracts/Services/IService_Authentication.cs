using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service contract for user authentication operations.
    /// Handles Windows username and PIN-based authentication flows.
    /// </summary>
    public interface IService_Authentication
    {
        /// <summary>
        /// Authenticates a user by Windows username (automatic login for personal workstations).
        /// </summary>
        /// <param name="windowsUsername">Windows username from Environment.UserName</param>
        /// <param name="progress">Optional progress reporter for splash screen updates</param>
        /// <returns>Authentication result with user data or error message</returns>
        Task<AuthenticationResult> AuthenticateByWindowsUsernameAsync(
            string windowsUsername, 
            IProgress<string>? progress = null);

        /// <summary>
        /// Authenticates a user by username and PIN (shared terminal login).
        /// </summary>
        /// <param name="username">Username (Windows username or full name)</param>
        /// <param name="pin">4-digit numeric PIN</param>
        /// <param name="progress">Optional progress reporter for splash screen updates</param>
        /// <returns>Authentication result with user data or error message</returns>
        Task<AuthenticationResult> AuthenticateByPinAsync(
            string username, 
            string pin, 
            IProgress<string>? progress = null);

        /// <summary>
        /// Creates a new user account with validation.
        /// </summary>
        /// <param name="user">User model with account data</param>
        /// <param name="createdBy">Windows username of account creator</param>
        /// <param name="progress">Optional progress reporter for UI feedback</param>
        /// <returns>Result with new employee number or error message</returns>
        Task<CreateUserResult> CreateNewUserAsync(
            Model_User user, 
            string createdBy, 
            IProgress<string>? progress = null);

        /// <summary>
        /// Validates PIN format and uniqueness.
        /// </summary>
        /// <param name="pin">4-digit PIN to validate</param>
        /// <param name="excludeEmployeeNumber">Optional employee number to exclude from uniqueness check</param>
        /// <returns>Validation result with error message if invalid</returns>
        Task<ValidationResult> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null);

        /// <summary>
        /// Detects workstation type (personal workstation or shared terminal).
        /// Queries workstation_config table to determine authentication flow.
        /// </summary>
        /// <param name="computerName">Computer name (defaults to Environment.MachineName)</param>
        /// <returns>Workstation configuration with type and timeout settings</returns>
        Task<Model_WorkstationConfig> DetectWorkstationTypeAsync(string? computerName = null);

        /// <summary>
        /// Retrieves list of active departments for dropdown population.
        /// </summary>
        /// <returns>List of department names</returns>
        Task<List<string>> GetActiveDepartmentsAsync();

        /// <summary>
        /// Logs a user activity event for audit trail.
        /// </summary>
        /// <param name="eventType">Event type (login_success, login_failed, session_timeout, user_created)</param>
        /// <param name="username">Username involved in event</param>
        /// <param name="workstationName">Computer name where event occurred</param>
        /// <param name="details">Additional event details</param>
        Task LogUserActivityAsync(string eventType, string username, string workstationName, string details);
    }

    /// <summary>
    /// Result of an authentication attempt.
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public Model_User? User { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static AuthenticationResult SuccessResult(Model_User user) => new()
        {
            Success = true,
            User = user
        };

        public static AuthenticationResult ErrorResult(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Result of a user creation attempt.
    /// </summary>
    public class CreateUserResult
    {
        public bool Success { get; set; }
        public int EmployeeNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static CreateUserResult SuccessResult(int employeeNumber) => new()
        {
            Success = true,
            EmployeeNumber = employeeNumber
        };

        public static CreateUserResult ErrorResult(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static ValidationResult Valid() => new() { IsValid = true };
        public static ValidationResult Invalid(string message) => new()
        {
            IsValid = false,
            ErrorMessage = message
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Models.Systems;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Services.Authentication
{
    /// <summary>
    /// Service implementation for user authentication operations.
    /// Orchestrates authentication flows and coordinates with data access layer.
    /// </summary>
    public class Service_Authentication : IService_Authentication
    {
        private readonly Dao_User _daoUser;
        private readonly IService_ErrorHandler _errorHandler;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="daoUser">User data access object</param>
        /// <param name="errorHandler">Error handler for logging</param>
        public Service_Authentication(Dao_User daoUser, IService_ErrorHandler errorHandler)
        {
            _daoUser = daoUser ?? throw new ArgumentNullException(nameof(daoUser));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        // ====================================================================
        // Authentication Methods
        // ====================================================================

        /// <inheritdoc/>
        public async Task<AuthenticationResult> AuthenticateByWindowsUsernameAsync(
            string windowsUsername, 
            IProgress<string>? progress = null)
        {
            try
            {
                progress?.Report("Authenticating Windows user...");

                // Validate input
                if (string.IsNullOrWhiteSpace(windowsUsername))
                {
                    return AuthenticationResult.ErrorResult("Windows username is required");
                }

                // Query database for user
                var result = await _daoUser.GetUserByWindowsUsernameAsync(windowsUsername);

                if (!result.Success)
                {
                    progress?.Report("User not found in database");
                    return AuthenticationResult.ErrorResult(result.ErrorMessage);
                }

                if (result.Data == null)
                {
                    return AuthenticationResult.ErrorResult("User data is invalid");
                }

                // Log successful authentication
                await LogUserActivityAsync(
                    "login_success",
                    windowsUsername,
                    Environment.MachineName,
                    "Windows authentication successful");

                progress?.Report($"Welcome, {result.Data.FullName}");
                return AuthenticationResult.SuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("Authentication by Windows username failed", Enum_ErrorSeverity.Error, ex, false);
                return AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthenticationResult> AuthenticateByPinAsync(
            string username, 
            string pin, 
            IProgress<string>? progress = null)
        {
            try
            {
                progress?.Report("Validating credentials...");

                // Validate input
                if (string.IsNullOrWhiteSpace(username))
                {
                    return AuthenticationResult.ErrorResult("Username is required");
                }

                if (string.IsNullOrWhiteSpace(pin))
                {
                    return AuthenticationResult.ErrorResult("PIN is required");
                }

                // Validate PIN format only (not uniqueness - that's for creation, not authentication)
                if (pin.Length != 4 || !pin.All(char.IsDigit))
                {
                    return AuthenticationResult.ErrorResult("PIN must be exactly 4 numeric digits");
                }

                // Query database for user with PIN
                var result = await _daoUser.ValidateUserPinAsync(username, pin);

                if (!result.Success || result.Data == null)
                {
                    // Log failed attempt
                    await LogUserActivityAsync(
                        "login_failed",
                        username,
                        Environment.MachineName,
                        $"Invalid credentials for user: {username}");

                    progress?.Report("Invalid username or PIN");
                    return AuthenticationResult.ErrorResult("Invalid username or PIN");
                }

                // Log successful authentication
                await LogUserActivityAsync(
                    "login_success",
                    result.Data.WindowsUsername,
                    Environment.MachineName,
                    $"PIN authentication successful for {result.Data.FullName}");

                progress?.Report($"Welcome, {result.Data.FullName}");
                return AuthenticationResult.SuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("Authentication by PIN failed", Enum_ErrorSeverity.Error, ex, false);
                return AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
            }
        }

        // ====================================================================
        // User Management
        // ====================================================================

        /// <inheritdoc/>
        public async Task<CreateUserResult> CreateNewUserAsync(
            Model_User user, 
            string createdBy, 
            IProgress<string>? progress = null)
        {
            try
            {
                progress?.Report("Validating user data...");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(user.WindowsUsername))
                {
                    return CreateUserResult.ErrorResult("Windows username is required");
                }

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    return CreateUserResult.ErrorResult("Full name is required");
                }

                if (string.IsNullOrWhiteSpace(user.Department))
                {
                    return CreateUserResult.ErrorResult("Department is required");
                }

                if (string.IsNullOrWhiteSpace(user.Shift))
                {
                    return CreateUserResult.ErrorResult("Shift is required");
                }

                // Validate PIN
                var pinValidation = await ValidatePinAsync(user.Pin);
                if (!pinValidation.IsValid)
                {
                    return CreateUserResult.ErrorResult(pinValidation.ErrorMessage);
                }

                progress?.Report("Creating user account...");

                // Create user in database
                var result = await _daoUser.CreateNewUserAsync(user, createdBy);

                if (!result.Success || result.Data == 0)
                {
                    return CreateUserResult.ErrorResult(result.ErrorMessage);
                }

                // Log user creation
                await LogUserActivityAsync(
                    "user_created",
                    user.WindowsUsername,
                    Environment.MachineName,
                    $"New user created: {user.FullName} (Emp #{result.Data}) by {createdBy}");

                progress?.Report($"Account created successfully. Employee #{result.Data}");
                return CreateUserResult.SuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("Create new user failed", Enum_ErrorSeverity.Error, ex, false);
                return CreateUserResult.ErrorResult($"User creation error: {ex.Message}");
            }
        }

        // ====================================================================
        // Validation Methods
        // ====================================================================

        /// <inheritdoc/>
        public async Task<ValidationResult> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null)
        {
            try
            {
                // Check format (4 digits)
                if (string.IsNullOrWhiteSpace(pin))
                {
                    return ValidationResult.Invalid("PIN is required");
                }

                if (pin.Length != 4)
                {
                    return ValidationResult.Invalid("PIN must be exactly 4 digits");
                }

                if (!Regex.IsMatch(pin, @"^\d{4}$"))
                {
                    return ValidationResult.Invalid("PIN must contain only numeric digits");
                }

                // Check uniqueness
                var uniqueResult = await _daoUser.IsPinUniqueAsync(pin, excludeEmployeeNumber);
                if (uniqueResult.Success && uniqueResult.Data == false)
                {
                    return ValidationResult.Invalid("This PIN is already in use");
                }

                return ValidationResult.Valid();
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("PIN validation failed", Enum_ErrorSeverity.Error, ex, false);
                return ValidationResult.Invalid($"Validation error: {ex.Message}");
            }
        }

        // ====================================================================
        // Configuration Methods
        // ====================================================================

        /// <inheritdoc/>
        public async Task<Model_WorkstationConfig> DetectWorkstationTypeAsync(string? computerName = null)
        {
            try
            {
                computerName ??= Environment.MachineName;

                // Get list of shared terminal names from database
                var sharedTerminalsResult = await _daoUser.GetSharedTerminalNamesAsync();

                var config = new Model_WorkstationConfig(computerName);

                if (sharedTerminalsResult.Success && sharedTerminalsResult.Data != null)
                {
                    // Check if current computer is in shared terminals list
                    var isShared = sharedTerminalsResult.Data.Contains(computerName);
                    config.WorkstationType = isShared ? "shared_terminal" : "personal_workstation";
                    config.Description = isShared 
                        ? "Shared terminal - PIN authentication required" 
                        : "Personal workstation - Windows authentication";
                }
                else
                {
                    // Default to personal workstation if query fails
                    config.WorkstationType = "personal_workstation";
                    config.Description = "Personal workstation (default)";
                }

                return config;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("Workstation type detection failed", Enum_ErrorSeverity.Warning, ex, false);
                
                // Return safe default
                return new Model_WorkstationConfig(computerName ?? Environment.MachineName)
                {
                    WorkstationType = "personal_workstation",
                    Description = "Personal workstation (default after error)"
                };
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetActiveDepartmentsAsync()
        {
            try
            {
                var result = await _daoUser.GetActiveDepartmentsAsync();

                if (result.Success && result.Data != null)
                {
                    return result.Data;
                }

                // Return empty list if query fails
                return new List<string>();
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("Get active departments failed", Enum_ErrorSeverity.Warning, ex, false);
                return new List<string>();
            }
        }

        // ====================================================================
        // Activity Logging
        // ====================================================================

        /// <inheritdoc/>
        public async Task LogUserActivityAsync(string eventType, string username, string workstationName, string details)
        {
            try
            {
                await _daoUser.LogUserActivityAsync(eventType, username, workstationName, details);
            }
            catch (Exception ex)
            {
                // Log errors but don't block application flow
                if (_errorHandler != null)
                    await _errorHandler.HandleErrorAsync("Activity logging failed", Enum_ErrorSeverity.Info, ex, false);
                System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }
    }
}

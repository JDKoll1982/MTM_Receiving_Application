using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Services.Authentication
{
    /// <summary>
    /// Service implementation for user authentication operations.
    /// Orchestrates authentication flows and coordinates with data access layer.
    /// </summary>
    public class Service_Authentication : IService_Authentication
    {
        private readonly Dao_User _daoUser;
        private readonly IService_ErrorHandler _errorHandler;
        private static readonly Regex _regex = new Regex(@"^\d{4}$");

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
        public async Task<Model_AuthenticationResult> AuthenticateByWindowsUsernameAsync(
            string windowsUsername,
            IProgress<string>? progress = null)
        {
            try
            {
                progress?.Report("Authenticating Windows user...");

                // Validate input
                if (string.IsNullOrWhiteSpace(windowsUsername))
                {
                    return Model_AuthenticationResult.ErrorResult("Windows username is required");
                }

                // Query database for user
                var result = await _daoUser.GetUserByWindowsUsernameAsync(windowsUsername);

                if (!result.Success)
                {
                    progress?.Report("User not found in database");
                    return Model_AuthenticationResult.ErrorResult(result.ErrorMessage);
                }

                if (result.Data == null)
                {
                    return Model_AuthenticationResult.ErrorResult("User data is invalid");
                }

                // Log successful authentication
                await LogUserActivityAsync(
                    "login_success",
                    windowsUsername,
                    Environment.MachineName,
                    "Windows authentication successful");

                progress?.Report($"Welcome, {result.Data.FullName}");
                return Model_AuthenticationResult.SuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    await _errorHandler.HandleErrorAsync("Authentication by Windows username failed", Enum_ErrorSeverity.Error, ex, false);
                }

                return Model_AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<Model_AuthenticationResult> AuthenticateByPinAsync(
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
                    return Model_AuthenticationResult.ErrorResult("Username is required");
                }

                if (string.IsNullOrWhiteSpace(pin))
                {
                    return Model_AuthenticationResult.ErrorResult("PIN is required");
                }

                // Validate PIN format only (not uniqueness - that's for creation, not authentication)
                if (pin.Length != 4 || !pin.All(char.IsDigit))
                {
                    return Model_AuthenticationResult.ErrorResult("PIN must be exactly 4 numeric digits");
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
                    return Model_AuthenticationResult.ErrorResult("Invalid username or PIN");
                }

                // Log successful authentication
                await LogUserActivityAsync(
                    "login_success",
                    result.Data.WindowsUsername,
                    Environment.MachineName,
                    $"PIN authentication successful for {result.Data.FullName}");

                progress?.Report($"Welcome, {result.Data.FullName}");
                return Model_AuthenticationResult.SuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    await _errorHandler.HandleErrorAsync("Authentication by PIN failed", Enum_ErrorSeverity.Error, ex, false);
                }

                return Model_AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
            }
        }

        // ====================================================================
        // User Management
        // ====================================================================

        /// <inheritdoc/>
        public async Task<Model_CreateUserResult> CreateNewUserAsync(
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
                    return Model_CreateUserResult.ErrorResult("Windows username is required");
                }

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    return Model_CreateUserResult.ErrorResult("Full name is required");
                }

                if (string.IsNullOrWhiteSpace(user.Department))
                {
                    return Model_CreateUserResult.ErrorResult("Department is required");
                }

                if (string.IsNullOrWhiteSpace(user.Shift))
                {
                    return Model_CreateUserResult.ErrorResult("Shift is required");
                }

                // Validate PIN
                var pinValidation = await ValidatePinAsync(user.Pin);
                if (!pinValidation.IsValid)
                {
                    return Model_CreateUserResult.ErrorResult(pinValidation.ErrorMessage);
                }

                progress?.Report("Creating user account...");

                // Create user in database
                var result = await _daoUser.CreateNewUserAsync(user, createdBy);

                if (!result.Success || result.Data == 0)
                {
                    return Model_CreateUserResult.ErrorResult(result.ErrorMessage);
                }

                // Log user creation
                await LogUserActivityAsync(
                    "user_created",
                    user.WindowsUsername,
                    Environment.MachineName,
                    $"New user created: {user.FullName} (Emp #{result.Data}) by {createdBy}");

                progress?.Report($"Account created successfully. Employee #{result.Data}");
                return Model_CreateUserResult.SuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    await _errorHandler.HandleErrorAsync("Create new user failed", Enum_ErrorSeverity.Error, ex, false);
                }

                return Model_CreateUserResult.ErrorResult($"User creation error: {ex.Message}");
            }
        }

        // ====================================================================
        // Validation Methods
        // ====================================================================

        /// <inheritdoc/>
        public async Task<Model_ValidationResult> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null)
        {
            try
            {
                // Check format (4 digits)
                if (string.IsNullOrWhiteSpace(pin))
                {
                    return Model_ValidationResult.Invalid("PIN is required");
                }

                if (pin.Length != 4)
                {
                    return Model_ValidationResult.Invalid("PIN must be exactly 4 digits");
                }

                if (!_regex.IsMatch(pin))
                {
                    return Model_ValidationResult.Invalid("PIN must contain only numeric digits");
                }

                return Model_ValidationResult.Valid();
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    await _errorHandler.HandleErrorAsync("PIN validation failed", Enum_ErrorSeverity.Error, ex, false);
                }

                return Model_ValidationResult.Invalid($"Validation error: {ex.Message}");
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
                computerName = computerName.Trim();

                // Get list of shared terminal names from database
                var sharedTerminalsResult = await _daoUser.GetSharedTerminalNamesAsync();

                var config = new Model_WorkstationConfig(computerName);

                if (sharedTerminalsResult.Success && sharedTerminalsResult.Data != null)
                {
                    // Check if current computer is in shared terminals list
                    var isShared = sharedTerminalsResult.Data.Any(name =>
                        string.Equals(name?.Trim(), computerName, StringComparison.OrdinalIgnoreCase));
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

                // Best-effort persistence: ensure this machine exists in workstation_config.
                // If the SP isn't deployed yet, ignore and proceed.
                try
                {
                    await _daoUser.UpsertWorkstationConfigAsync(
                        config.ComputerName,
                        config.WorkstationType,
                        isActive: true,
                        description: config.Description);
                }
                catch
                {
                    // Swallow: persistence must not block startup.
                }

                return config;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    await _errorHandler.HandleErrorAsync("Workstation type detection failed", Enum_ErrorSeverity.Warning, ex, false);
                }

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
                {
                    await _errorHandler.HandleErrorAsync("Get active departments failed", Enum_ErrorSeverity.Warning, ex, false);
                }

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
                {
                    await _errorHandler.HandleErrorAsync("Activity logging failed", Enum_ErrorSeverity.Info, ex, false);
                }

                System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }
    }
}


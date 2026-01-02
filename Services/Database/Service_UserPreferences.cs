using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Services.Database;

public class Service_UserPreferences : IService_UserPreferences
{
    private readonly Dao_User _userDao;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_UserPreferences(
        Dao_User userDao,
        IService_LoggingUtility logger,
        IService_ErrorHandler errorHandler)
    {
        _userDao = userDao;
        _logger = logger;
        _errorHandler = errorHandler;
    }

    public async Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username)
    {
        try
        {
            // BUSINESS RULE: Username normalization
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Failure<Model_UserPreference>(
                    "Username cannot be empty");
            }

            // DELEGATE TO DAO
            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);

            if (!userResult.IsSuccess)
            {
                _logger.LogError(
                   $"Failed to retrieve user {normalizedUsername}: {userResult.ErrorMessage}", null, "UserPreferences");
                return Model_Dao_Result_Factory.Failure<Model_UserPreference>(userResult.ErrorMessage);
            }

            if (userResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_UserPreference>("User not found");
            }

            var preference = new Model_UserPreference
            {
                Username = userResult.Data.WindowsUsername,
                PreferredPackageType = userResult.Data.DefaultReceivingMode ?? "Package",
                LastUpdated = DateTime.Now // Approximate
            };

            // LOGGING
            _logger.LogInfo(
                $"Retrieved preference for user {normalizedUsername}", "UserPreferences");

            return Model_Dao_Result_Factory.Success(preference);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error in {nameof(GetLatestUserPreferenceAsync)}: {ex.Message}",
                Enum_ErrorSeverity.Critical,
                ex);

            return Model_Dao_Result_Factory.Failure<Model_UserPreference>(
                "An error occurred while retrieving user preferences.", ex);
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode)
    {
        try
        {
            // BUSINESS VALIDATION
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Failure("Username cannot be empty");
            }

            if (defaultMode != "Package" && defaultMode != "Pallet")
            {
                return Model_Dao_Result_Factory.Failure(
                    $"Invalid default mode '{defaultMode}'. Must be 'Package' or 'Pallet'.");
            }

            // Get User ID first
            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure($"User '{normalizedUsername}' not found.");
            }

            // DELEGATE TO DAO
            var result = await _userDao.UpdateDefaultModeAsync(userResult.Data.EmployeeNumber, defaultMode);

            // LOGGING
            if (result.IsSuccess)
            {
                _logger.LogInfo(
                    $"Updated preference for {normalizedUsername}: {defaultMode}", "UserPreferences");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error in {nameof(UpdateDefaultModeAsync)}: {ex.Message}",
                Enum_ErrorSeverity.Critical,
                ex);

            return Model_Dao_Result_Factory.Failure("An error occurred while updating preference.", ex);
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode)
    {
        try
        {
            // BUSINESS VALIDATION
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Failure("Username cannot be empty");
            }

            // Validate mode values (allow empty string to clear preference)
            if (!string.IsNullOrEmpty(defaultMode) &&
                defaultMode != "guided" &&
                defaultMode != "manual" &&
                defaultMode != "edit")
            {
                return Model_Dao_Result_Factory.Failure(
                    $"Invalid default receiving mode '{defaultMode}'. Must be 'guided', 'manual', 'edit', or empty.");
            }

            // Get User ID first
            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure($"User '{normalizedUsername}' not found.");
            }

            // DELEGATE TO DAO - use UpdateDefaultReceivingModeAsync when DAO is updated
            var result = await _userDao.UpdateDefaultReceivingModeAsync(
                userResult.Data.EmployeeNumber,
                string.IsNullOrEmpty(defaultMode) ? null : defaultMode);

            // LOGGING
            if (result.IsSuccess)
            {
                _logger.LogInfo(
                    $"Updated default receiving mode for {normalizedUsername} to '{defaultMode}'",
                    "UserPreferences");
            }
            else
            {
                _logger.LogError(
                    $"Failed to update receiving mode for {normalizedUsername}: {result.ErrorMessage}",
                    null,
                    "UserPreferences");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error in {nameof(UpdateDefaultReceivingModeAsync)}: {ex.Message}",
                Enum_ErrorSeverity.Critical,
                ex);

            return Model_Dao_Result_Factory.Failure(
                "An error occurred while updating default receiving mode.", ex);
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode)
    {
        try
        {
            // BUSINESS VALIDATION
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Failure("Username cannot be empty");
            }

            // Validate mode values (allow empty string to clear preference)
            if (!string.IsNullOrEmpty(defaultMode) &&
                defaultMode != "guided" &&
                defaultMode != "manual" &&
                defaultMode != "edit")
            {
                return Model_Dao_Result_Factory.Failure(
                    $"Invalid default dunnage mode '{defaultMode}'. Must be 'guided', 'manual', 'edit', or empty.");
            }

            // Get User ID first
            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure($"User '{normalizedUsername}' not found.");
            }

            // DELEGATE TO DAO
            var result = await _userDao.UpdateDefaultDunnageModeAsync(
                userResult.Data.EmployeeNumber,
                string.IsNullOrEmpty(defaultMode) ? null : defaultMode);

            // LOGGING
            if (result.IsSuccess)
            {
                _logger.LogInfo(
                    $"Updated default dunnage mode for {normalizedUsername} to '{defaultMode}'",
                    "UserPreferences");
            }
            else
            {
                _logger.LogError(
                    $"Failed to update dunnage mode for {normalizedUsername}: {result.ErrorMessage}",
                    null,
                    "UserPreferences");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error in {nameof(UpdateDefaultDunnageModeAsync)}: {ex.Message}",
                Enum_ErrorSeverity.Critical,
                ex);

            return Model_Dao_Result_Factory.Failure(
                "An error occurred while updating default dunnage mode.", ex);
        }
    }
}

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
    private readonly ILoggingService _logger;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_UserPreferences(
        Dao_User userDao,
        ILoggingService logger,
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
                return DaoResultFactory.Failure<Model_UserPreference>(
                    "Username cannot be empty");
            }

            // DELEGATE TO DAO
            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);

            if (!userResult.IsSuccess)
            {
                _logger.LogError(
                   $"Failed to retrieve user {normalizedUsername}: {userResult.ErrorMessage}", null, "UserPreferences");
                return DaoResultFactory.Failure<Model_UserPreference>(userResult.ErrorMessage);
            }

            if (userResult.Data == null)
            {
                return DaoResultFactory.Failure<Model_UserPreference>("User not found");
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

            return DaoResultFactory.Success(preference);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error in {nameof(GetLatestUserPreferenceAsync)}: {ex.Message}",
                Enum_ErrorSeverity.Critical,
                ex);

            return DaoResultFactory.Failure<Model_UserPreference>(
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
                return DaoResultFactory.Failure("Username cannot be empty");
            }

            if (defaultMode != "Package" && defaultMode != "Pallet")
            {
                return DaoResultFactory.Failure(
                    $"Invalid default mode '{defaultMode}'. Must be 'Package' or 'Pallet'.");
            }

            // Get User ID first
            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return DaoResultFactory.Failure($"User '{normalizedUsername}' not found.");
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

            return DaoResultFactory.Failure("An error occurred while updating preference.", ex);
        }
    }
}

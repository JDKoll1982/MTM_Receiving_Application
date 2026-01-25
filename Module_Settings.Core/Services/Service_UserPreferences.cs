using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Service for user preference operations.
/// </summary>
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

    public async Task<Model_Dao_Result<Model_Receiving_Entity_UserPreference>> GetLatestUserPreferenceAsync(string username)
    {
        try
        {
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_UserPreference>(
                    "Username cannot be empty");
            }

            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);

            if (!userResult.IsSuccess)
            {
                _logger.LogError(
                   $"Failed to retrieve user {normalizedUsername}: {userResult.ErrorMessage}", null, "UserPreferences");
                return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_UserPreference>(userResult.ErrorMessage);
            }

            if (userResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_UserPreference>("User not found");
            }

            var preference = new Model_Receiving_Entity_UserPreference
            {
                Username = userResult.Data.WindowsUsername,
                DefaultMode = null,
                DefaultReceivingMode = string.IsNullOrWhiteSpace(userResult.Data.DefaultReceivingMode)
                    ? "guided"
                    : userResult.Data.DefaultReceivingMode,
                DefaultDunnageMode = string.IsNullOrWhiteSpace(userResult.Data.DefaultDunnageMode)
                    ? "guided"
                    : userResult.Data.DefaultDunnageMode
            };

            return Model_Dao_Result_Factory.Success(preference);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting user preference for {username}", ex, "UserPreferences");
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_UserPreference>(ex.Message);
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode)
    {
        try
        {
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Success();
            }

            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                _logger.LogWarning($"Default mode update skipped: user not found ({normalizedUsername})");
                return Model_Dao_Result_Factory.Success();
            }

            var normalizedMode = string.IsNullOrWhiteSpace(defaultMode)
                ? null
                : defaultMode.Trim().ToLowerInvariant();

            var result = await _userDao.UpdateDefaultModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
            if (!result.Success)
            {
                _logger.LogWarning($"Default mode update failed (non-blocking): {result.ErrorMessage}");
            }

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating default mode for {username} (non-blocking)", ex, "UserPreferences");
            return Model_Dao_Result_Factory.Success();
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode)
    {
        try
        {
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Success();
            }

            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                _logger.LogWarning($"Receiving default update skipped: user not found ({normalizedUsername})");
                return Model_Dao_Result_Factory.Success();
            }

            var normalizedMode = string.IsNullOrWhiteSpace(defaultMode)
                ? null
                : defaultMode.Trim().ToLowerInvariant();

            var result = await _userDao.UpdateDefaultReceivingModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
            if (!result.Success)
            {
                _logger.LogWarning($"Receiving default update failed (non-blocking): {result.ErrorMessage}");
            }

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating receiving default for {username} (non-blocking)", ex, "UserPreferences");
            return Model_Dao_Result_Factory.Success();
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode)
    {
        try
        {
            var normalizedUsername = username?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return Model_Dao_Result_Factory.Success();
            }

            var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                _logger.LogWarning($"Dunnage default update skipped: user not found ({normalizedUsername})");
                return Model_Dao_Result_Factory.Success();
            }

            var normalizedMode = string.IsNullOrWhiteSpace(defaultMode)
                ? null
                : defaultMode.Trim().ToLowerInvariant();

            var result = await _userDao.UpdateDefaultDunnageModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
            if (!result.Success)
            {
                _logger.LogWarning($"Dunnage default update failed (non-blocking): {result.ErrorMessage}");
            }

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating dunnage default for {username} (non-blocking)", ex, "UserPreferences");
            return Model_Dao_Result_Factory.Success();
        }
    }
}

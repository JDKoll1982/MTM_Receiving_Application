using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Interfaces;

namespace MTM_Receiving_Application.Module_Settings.Services;

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

            // Map to Model_UserPreference (assuming user data contains preferences or we need another DAO call)
            // For now, returning a dummy or partial preference based on user data if available
            // In a real scenario, we might have a separate table for preferences or columns in User table

            // Assuming Model_User has preference fields or we construct default
            var preference = new Model_UserPreference
            {
                Username = userResult.Data.WindowsUsername,
                // Default values or mapped from user
                DefaultMode = "Receiving", // Example default
                DefaultReceivingMode = "Guided",
                DefaultDunnageMode = "Types"
            };

            return Model_Dao_Result_Factory.Success(preference);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting user preference for {username}", ex, "UserPreferences");
            return Model_Dao_Result_Factory.Failure<Model_UserPreference>(ex.Message);
        }
    }

    public async Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode)
    {
        // Implementation placeholder - would call DAO to update user preference
        await Task.Delay(10); // Simulate async work
        return Model_Dao_Result_Factory.Success();
    }

    public async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode)
    {
        // Implementation placeholder
        await Task.Delay(10);
        return Model_Dao_Result_Factory.Success();
    }

    public async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode)
    {
        // Implementation placeholder
        await Task.Delay(10);
        return Model_Dao_Result_Factory.Success();
    }
}


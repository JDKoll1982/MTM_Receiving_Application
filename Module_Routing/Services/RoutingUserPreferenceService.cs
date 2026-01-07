using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for user preferences (default mode, validation toggle)
/// </summary>
public class RoutingUserPreferenceService : IRoutingUserPreferenceService
{
    private readonly Dao_RoutingUserPreference _daoUserPreference;
    private readonly IService_LoggingUtility _logger;

    public RoutingUserPreferenceService(
        Dao_RoutingUserPreference daoUserPreference,
        IService_LoggingUtility logger)
    {
        _daoUserPreference = daoUserPreference ?? throw new ArgumentNullException(nameof(daoUserPreference));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves user preferences for the specified employee
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <returns>Result with user preference or null if not found</returns>
    public async Task<Model_Dao_Result<Model_RoutingUserPreference>> GetUserPreferenceAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting user preference for employee {employeeNumber}");

            var result = await _daoUserPreference.GetUserPreferenceAsync(employeeNumber);

            if (result.IsSuccess && result.Data != null)
            {
                return result;
            }

            // Return defaults if no preference exists
            var defaultPreference = new Model_RoutingUserPreference
            {
                EmployeeNumber = employeeNumber,
                DefaultMode = nameof(Enum_RoutingMode.WIZARD),
                EnableValidation = true
            };

            return Model_Dao_Result_Factory.Success(defaultPreference);
        }
        catch (Exception ex)
        {
            // Issue #11: Standardized error handling pattern
            await _logger.LogErrorAsync($"Error getting user preference for employee {employeeNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_RoutingUserPreference>($"Failed to retrieve user preferences: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves or updates user preferences
    /// </summary>
    /// <param name="preference">User preference to save</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> SaveUserPreferenceAsync(Model_RoutingUserPreference preference)
    {
        try
        {
            await _logger.LogInfoAsync($"Saving user preference for employee {preference.EmployeeNumber}");

            // Validate mode
            if (!Enum.TryParse<Enum_RoutingMode>(preference.DefaultMode, out _))
            {
                return Model_Dao_Result_Factory.Failure($"Invalid mode: {preference.DefaultMode}");
            }

            return await _daoUserPreference.SaveUserPreferenceAsync(preference);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error saving user preference: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error saving preferences: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Resets user preferences to default values
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> ResetToDefaultsAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Resetting preferences to defaults for employee {employeeNumber}");

            var defaultPreference = new Model_RoutingUserPreference
            {
                EmployeeNumber = employeeNumber,
                DefaultMode = nameof(Enum_RoutingMode.WIZARD),
                EnableValidation = true
            };

            return await _daoUserPreference.SaveUserPreferenceAsync(defaultPreference);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error resetting preferences: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error resetting preferences: {ex.Message}", ex);
        }
    }
}


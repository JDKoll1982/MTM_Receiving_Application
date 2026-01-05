using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service interface for user preferences: default mode, validation toggle
/// </summary>
public interface IRoutingUserPreferenceService
{
    /// <summary>
    /// Retrieves user preferences for employee (returns defaults if not set)
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <returns>Result with user preference data (default_mode, enable_validation)</returns>
    public Task<Model_Dao_Result<Model_RoutingUserPreference>> GetUserPreferenceAsync(int employeeNumber);

    /// <summary>
    /// Creates or updates user preferences
    /// </summary>
    /// <param name="preference">User preference data to save</param>
    /// <returns>Result indicating success or failure</returns>
    public Task<Model_Dao_Result> SaveUserPreferenceAsync(Model_RoutingUserPreference preference);

    /// <summary>
    /// Resets user preferences to defaults (Wizard mode, validation enabled)
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <returns>Result indicating success or failure</returns>
    public Task<Model_Dao_Result> ResetToDefaultsAsync(int employeeNumber);
}

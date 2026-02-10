using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Service contract for user preference operations.
/// </summary>
public interface IService_UserPreferences
{
    Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username);
    Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string? defaultMode);
    Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string? defaultMode);
    Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string? defaultMode);
}

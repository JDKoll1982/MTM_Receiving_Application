using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services;

public interface IService_UserPreferences
{
    /// <summary>Get user's latest receiving mode preference</summary>
    Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username);

    /// <summary>Update user's default receiving mode</summary>
    Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode);
}

using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;

namespace MTM_Receiving_Application.Contracts.Services;

public interface IService_UserPreferences
{
    /// <summary>Get user's latest receiving mode preference</summary>
    /// <param name="username"></param>
    public Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username);

    /// <summary>Update user's default receiving mode</summary>
    /// <param name="username"></param>
    /// <param name="defaultMode"></param>
    public Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode);

    /// <summary>Update user's default receiving workflow mode (guided, manual, edit)</summary>
    /// <param name="username"></param>
    /// <param name="defaultMode"></param>
    public Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode);

    /// <summary>Update user's default dunnage mode</summary>
    /// <param name="username"></param>
    /// <param name="defaultMode"></param>
    public Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode);
}

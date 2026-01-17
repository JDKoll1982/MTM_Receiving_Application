using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Facade for external modules to interact with Core Settings without exposing CQRS internals.
/// </summary>
public interface IService_SettingsCoreFacade
{
    Task<Model_Dao_Result<Model_SettingsValue>> GetSettingAsync(string category, string key, int? userId = null);
    Task<Model_Dao_Result> SetSettingAsync(string category, string key, string value, int? userId = null);
    Task<Model_Dao_Result> ResetSettingAsync(string category, string key, int? userId = null);
    Task InitializeDefaultsAsync(int? userId = null);
}

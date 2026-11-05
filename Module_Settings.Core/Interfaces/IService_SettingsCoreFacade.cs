using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Facade for external modules to interact with Core Settings without exposing CQRS internals.
/// </summary>
public interface IService_SettingsCoreFacade
{
    Task<Model_Dao_Result<Model_SettingsValue>> GetSettingAsync(
        string category,
        string key,
        int? userId = null
    );
    Task<Model_Dao_Result> SetSettingAsync(
        string category,
        string key,
        string value,
        int? userId = null
    );
    Task<Model_Dao_Result> ResetSettingAsync(string category, string key, int? userId = null);

    /// <summary>
    /// Makes sure a row exists for every setting <paramref name="userId"/> needs, so a later read never
    /// has to invent a default.
    /// </summary>
    /// <param name="userId">The user whose settings to seed, or null to seed the system-wide ones.</param>
    /// <param name="progress">Told about each setting as it is checked, so a slow start can be shown.</param>
    Task InitializeDefaultsAsync(
        int? userId = null,
        IProgress<Model_SettingsSeedProgress>? progress = null
    );
}

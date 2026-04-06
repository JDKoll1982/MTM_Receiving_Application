using System.Globalization;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Dunnage.Services;

/// <summary>
/// Retrieves Module_Dunnage settings through Settings.Core and falls back to module defaults.
/// </summary>
public class Service_DunnageSettings : IService_DunnageSettings
{
    private const string Category = "Dunnage";

    private readonly IService_SettingsCoreFacade _settings;

    public Service_DunnageSettings(IService_SettingsCoreFacade settings)
    {
        _settings = settings;
    }

    public async Task<string> GetStringAsync(string key, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var result = await _settings.GetSettingAsync(Category, key, userId);
        if (
            result.IsSuccess
            && result.Data != null
            && string.IsNullOrWhiteSpace(result.Data.Value) is false
        )
        {
            return result.Data.Value;
        }

        if (DunnageSettingsDefaults.StringDefaults.TryGetValue(key, out var defaultValue))
        {
            return defaultValue;
        }

        return string.Empty;
    }

    public async Task<bool> GetBoolAsync(string key, int? userId = null)
    {
        var value = await GetStringAsync(key, userId);
        return bool.TryParse(value, out var parsed) && parsed;
    }

    public async Task<int> GetIntAsync(string key, int? userId = null)
    {
        var value = await GetStringAsync(key, userId);
        return int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var parsed
        )
            ? parsed
            : 0;
    }

    public async Task SaveStringAsync(string key, string value, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        await _settings.SetSettingAsync(Category, key, value, userId);
    }
}

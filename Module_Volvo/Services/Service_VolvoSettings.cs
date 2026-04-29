using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Settings;

namespace MTM_Receiving_Application.Module_Volvo.Services;

public class Service_VolvoSettings : IService_VolvoSettings
{
    private const string Category = "Volvo";

    private readonly IService_SettingsCoreFacade _settings;

    public Service_VolvoSettings(IService_SettingsCoreFacade settings)
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

        if (VolvoSettingsDefaults.StringDefaults.TryGetValue(key, out var defaultValue))
        {
            return defaultValue;
        }

        return string.Empty;
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

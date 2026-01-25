using System;
using System.Globalization;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Retrieves Module_Receiving settings through Settings.Core and falls back to module defaults.
/// </summary>
public class Service_Receiving_Infrastructure_Settings : IService_Receiving_Infrastructure_Settings
{
    private const string Category = "Receiving";

    private readonly IService_SettingsCoreFacade _settings;

    public Service_Receiving_Infrastructure_Settings(IService_SettingsCoreFacade settings)
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
        if (result.IsSuccess && result.Data != null && !string.IsNullOrWhiteSpace(result.Data.Value))
        {
            return result.Data.Value;
        }

        if (ReceivingSettingsDefaults.StringDefaults.TryGetValue(key, out var defaultValue))
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
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
    }

    public async Task<string> FormatAsync(string key, object? arg0, int? userId = null)
    {
        var template = await GetStringAsync(key, userId);
        return string.Format(CultureInfo.CurrentCulture, template, arg0);
    }

    public async Task<string> FormatAsync(string key, object? arg0, object? arg1, int? userId = null)
    {
        var template = await GetStringAsync(key, userId);
        return string.Format(CultureInfo.CurrentCulture, template, arg0, arg1);
    }
}

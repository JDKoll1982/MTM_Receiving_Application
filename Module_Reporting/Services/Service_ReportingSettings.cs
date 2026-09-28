using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Reporting.Services;

public sealed class Service_ReportingSettings : IService_ReportingSettings
{
    private readonly IService_SettingsCoreFacade _settings;

    public Service_ReportingSettings(IService_SettingsCoreFacade settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<Model_ReportingPreviewSettings> GetPreviewSettingsAsync(int? userId = null)
    {
        var model = new Model_ReportingPreviewSettings();

        var modeResult = await _settings.GetSettingAsync(
            ReportingSettingsKeys.Category,
            ReportingSettingsKeys.Preview.RowDisplayMode,
            userId
        );

        if (
            modeResult.IsSuccess
            && modeResult.Data is not null
            && Enum.TryParse<Enum_ReportingPreviewRowDisplayMode>(
                modeResult.Data.Value,
                ignoreCase: true,
                out var mode
            )
        )
        {
            model.RowDisplayMode = mode;
        }

        var modulePrefsResult = await _settings.GetSettingAsync(
            ReportingSettingsKeys.Category,
            ReportingSettingsKeys.Preview.ModulePreferencesJson,
            userId
        );

        if (
            modulePrefsResult.IsSuccess
            && modulePrefsResult.Data is not null
            && string.IsNullOrWhiteSpace(modulePrefsResult.Data.Value) is false
        )
        {
            try
            {
                model.ModuleSettings =
                    JsonSerializer.Deserialize<
                        Dictionary<string, Model_ReportingPreviewModuleSettings>
                    >(modulePrefsResult.Data.Value)
                    ?? new Dictionary<
                        string,
                        Model_ReportingPreviewModuleSettings
                    >(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                model.ModuleSettings = new Dictionary<
                    string,
                    Model_ReportingPreviewModuleSettings
                >(StringComparer.OrdinalIgnoreCase);
            }
        }

        return model;
    }

    public async Task<Model_Dao_Result> SavePreviewSettingsAsync(
        Model_ReportingPreviewSettings settings,
        int? userId = null
    )
    {
        ArgumentNullException.ThrowIfNull(settings);

        var rowModeResult = await _settings.SetSettingAsync(
            ReportingSettingsKeys.Category,
            ReportingSettingsKeys.Preview.RowDisplayMode,
            settings.RowDisplayMode.ToString(),
            userId
        );

        if (!rowModeResult.IsSuccess)
        {
            return rowModeResult;
        }

        var serializedModuleSettings = JsonSerializer.Serialize(settings.ModuleSettings);
        var modulesResult = await _settings.SetSettingAsync(
            ReportingSettingsKeys.Category,
            ReportingSettingsKeys.Preview.ModulePreferencesJson,
            serializedModuleSettings,
            userId
        );

        if (!modulesResult.IsSuccess)
        {
            return modulesResult;
        }

        return Model_Dao_Result_Factory.Success();
    }
}

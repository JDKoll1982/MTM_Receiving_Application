using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Reads Material Availability Board settings through the shared Settings.Core facade.
/// </summary>
public sealed class Service_ShipRecToolsSettings : IService_ShipRecToolsSettings
{
    private readonly IService_SettingsCoreFacade _settings;

    public Service_ShipRecToolsSettings(IService_SettingsCoreFacade settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<Model_Tool_MaterialAvailabilityFieldSettings> GetMaterialAvailabilityFieldSettingsAsync()
    {
        var uiVisibleIds = await GetFieldIdSetAsync(
            ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderUiVisibleFieldIds,
            MaterialAvailabilityWorkOrderFieldCatalog.DefaultUiVisibleIds
        );
        var printVisibleIds = await GetFieldIdSetAsync(
            ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderPrintVisibleFieldIds,
            MaterialAvailabilityWorkOrderFieldCatalog.DefaultPrintVisibleIds
        );
        var isShowAllChipEnabled = await GetBoolAsync(
            ShipRecToolsSettingsKeys.MaterialAvailability.WorkOrderAllowShowAllChip,
            true
        );

        return new Model_Tool_MaterialAvailabilityFieldSettings
        {
            UiVisibleFieldIds = uiVisibleIds,
            PrintVisibleFieldIds = printVisibleIds,
            IsShowAllChipEnabled = isShowAllChipEnabled,
        };
    }

    private async Task<HashSet<string>> GetFieldIdSetAsync(
        string key,
        IReadOnlyList<string> fallback
    )
    {
        var result = await _settings.GetSettingAsync(ShipRecToolsSettingsKeys.Category, key);
        if (
            !result.IsSuccess
            || result.Data is null
            || string.IsNullOrWhiteSpace(result.Data.Value)
        )
        {
            return new HashSet<string>(fallback, StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(result.Data.Value) ?? [];
            var knownIds = MaterialAvailabilityWorkOrderFieldCatalog
                .All.Select(field => field.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new HashSet<string>(
                parsed.Where(knownIds.Contains),
                StringComparer.OrdinalIgnoreCase
            );
        }
        catch
        {
            return new HashSet<string>(fallback, StringComparer.OrdinalIgnoreCase);
        }
    }

    private async Task<bool> GetBoolAsync(string key, bool fallback)
    {
        var result = await _settings.GetSettingAsync(ShipRecToolsSettingsKeys.Category, key);
        return
            result.IsSuccess
            && result.Data is not null
            && bool.TryParse(result.Data.Value, out var parsed)
            ? parsed
            : fallback;
    }
}

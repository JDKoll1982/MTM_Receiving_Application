using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
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

    public async Task<Model_Tool_POLineSpecSearchOptions> GetPOLineSpecSearchOptionsAsync()
    {
        var searchMode = await GetStringAsync(
            ShipRecToolsSettingsKeys.POLineSpecSearch.SearchMode,
            Model_Tool_POLineSpecSearchOptions.DefaultSearchMode
        );

        var poStatusFilter = await GetStringAsync(
            ShipRecToolsSettingsKeys.POLineSpecSearch.PoStatusFilter,
            string.Empty
        );

        var visibleLines = await GetIntAsync(
            ShipRecToolsSettingsKeys.POLineSpecSearch.VisibleLines,
            Model_Tool_POLineSpecSearchOptions.DefaultVisibleLines
        );

        var visibleColumnKeys = await GetStringSetAsync(
            ShipRecToolsSettingsKeys.POLineSpecSearch.VisibleColumnKeys,
            Model_Tool_POLineSpecSearchOptions.DefaultVisibleColumnKeys,
            Model_Tool_POLineSpecSearchOptions.AvailableColumnKeys
        );

        if (visibleColumnKeys.Count == 0)
        {
            visibleColumnKeys = new HashSet<string>(
                Model_Tool_POLineSpecSearchOptions.DefaultVisibleColumnKeys,
                StringComparer.OrdinalIgnoreCase
            );
        }

        return new Model_Tool_POLineSpecSearchOptions
        {
            SearchMode = Model_Tool_POLineSpecSearchOptions.SearchModeOptions.Contains(
                searchMode,
                StringComparer.OrdinalIgnoreCase
            )
                ? Model_Tool_POLineSpecSearchOptions.SearchModeOptions.First(option =>
                    string.Equals(option, searchMode, StringComparison.OrdinalIgnoreCase)
                )
                : Model_Tool_POLineSpecSearchOptions.DefaultSearchMode,
            PoStatusFilter = poStatusFilter,
            VisibleLines = visibleLines,
            VisibleColumnKeys = visibleColumnKeys,
        };
    }

    public async Task<Model_Dao_Result> SavePOLineSpecSearchOptionsAsync(
        Model_Tool_POLineSpecSearchOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        var normalizedSearchMode = Model_Tool_POLineSpecSearchOptions.SearchModeOptions.Contains(
            options.SearchMode,
            StringComparer.OrdinalIgnoreCase
        )
            ? Model_Tool_POLineSpecSearchOptions.SearchModeOptions.First(mode =>
                string.Equals(mode, options.SearchMode, StringComparison.OrdinalIgnoreCase)
            )
            : Model_Tool_POLineSpecSearchOptions.DefaultSearchMode;

        var lines = options.VisibleLines;
        if (Model_Tool_POLineSpecSearchOptions.VisibleLinesOptions.Contains(lines) is false)
        {
            lines = Model_Tool_POLineSpecSearchOptions.DefaultVisibleLines;
        }

        var visibleColumns = options.VisibleColumnKeys
            .Where(static key => string.IsNullOrWhiteSpace(key) is false)
            .Where(
                key =>
                    Model_Tool_POLineSpecSearchOptions.AvailableColumnKeys.Contains(
                        key,
                        StringComparer.OrdinalIgnoreCase
                    )
            )
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (visibleColumns.Count == 0)
        {
            visibleColumns =
            [
                .. Model_Tool_POLineSpecSearchOptions.DefaultVisibleColumnKeys,
            ];
        }

        if (visibleColumns.Contains("SpecExcerpt", StringComparer.OrdinalIgnoreCase) is false)
        {
            visibleColumns.Add("SpecExcerpt");
        }

        var modeResult = await _settings.SetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            ShipRecToolsSettingsKeys.POLineSpecSearch.SearchMode,
            normalizedSearchMode
        );
        if (!modeResult.IsSuccess)
        {
            return modeResult;
        }

        var statusResult = await _settings.SetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            ShipRecToolsSettingsKeys.POLineSpecSearch.PoStatusFilter,
            options.PoStatusFilter?.Trim() ?? string.Empty
        );
        if (!statusResult.IsSuccess)
        {
            return statusResult;
        }

        var linesResult = await _settings.SetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            ShipRecToolsSettingsKeys.POLineSpecSearch.VisibleLines,
            lines.ToString()
        );
        if (!linesResult.IsSuccess)
        {
            return linesResult;
        }

        var columnsJson = JsonSerializer.Serialize(visibleColumns);
        var columnsResult = await _settings.SetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            ShipRecToolsSettingsKeys.POLineSpecSearch.VisibleColumnKeys,
            columnsJson
        );
        if (!columnsResult.IsSuccess)
        {
            return columnsResult;
        }

        return Model_Dao_Result_Factory.Success();
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

    private async Task<HashSet<string>> GetStringSetAsync(
        string key,
        IReadOnlyList<string> fallback,
        IReadOnlyList<string> allowedValues
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
            var allowed = allowedValues.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new HashSet<string>(
                parsed.Where(allowed.Contains),
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

    private async Task<string> GetStringAsync(string key, string fallback)
    {
        var result = await _settings.GetSettingAsync(ShipRecToolsSettingsKeys.Category, key);
        return
            result.IsSuccess
            && result.Data is not null
            && string.IsNullOrWhiteSpace(result.Data.Value) is false
            ? result.Data.Value
            : fallback;
    }

    private async Task<int> GetIntAsync(string key, int fallback)
    {
        var result = await _settings.GetSettingAsync(ShipRecToolsSettingsKeys.Category, key);
        return
            result.IsSuccess
            && result.Data is not null
            && int.TryParse(result.Data.Value, out var parsed)
            ? parsed
            : fallback;
    }
}

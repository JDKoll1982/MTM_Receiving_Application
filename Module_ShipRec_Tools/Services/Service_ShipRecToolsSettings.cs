using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Reads Material Availability Board settings through the shared Settings.Core facade.
/// </summary>
public sealed class Service_ShipRecToolsSettings : IService_ShipRecToolsSettings
{
    private readonly IService_SettingsCoreFacade _settings;
    private readonly IService_UserSessionManager _sessionManager;

    public Service_ShipRecToolsSettings(
        IService_SettingsCoreFacade settings,
        IService_UserSessionManager sessionManager
    )
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
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

    public async Task<bool> GetLotMarkingEnabledAsync() =>
        await GetBoolAsync(ShipRecToolsSettingsKeys.LotMarking.Enabled, true);

    public async Task<Enum_LotMarkingCollisionPolicy> GetLotMarkingCollisionPolicyAsync()
    {
        var raw = await GetStringAsync(
            ShipRecToolsSettingsKeys.LotMarking.CollisionPolicy,
            nameof(Enum_LotMarkingCollisionPolicy.Probe)
        );

        return Enum.TryParse<Enum_LotMarkingCollisionPolicy>(raw, true, out var policy)
            ? policy
            : Enum_LotMarkingCollisionPolicy.Probe;
    }

    public async Task<IReadOnlyCollection<string>> GetLotMarkingEnabledShapesAsync()
    {
        var allGlyphs = Model_LotMarkingPalette.Shapes.Select(static shape => shape.Glyph).ToList();
        return await GetStringSetAsync(
            ShipRecToolsSettingsKeys.LotMarking.PaletteEnabledShapes,
            allGlyphs,
            allGlyphs
        );
    }

    public async Task<IReadOnlyCollection<string>> GetLotMarkingEnabledColorsAsync()
    {
        var allCodes = Model_LotMarkingPalette.Colors.Select(static color => color.Code).ToList();
        return await GetStringSetAsync(
            ShipRecToolsSettingsKeys.LotMarking.PaletteEnabledColors,
            allCodes,
            allCodes
        );
    }

    public async Task<bool> GetLotMarkingShapeFirstAsync() =>
        await GetBoolAsync(ShipRecToolsSettingsKeys.LotMarking.DisplayShapeFirst, true);

    public async Task<Model_Tool_LotMarkingFilter> GetLotMarkingListFilterAsync()
    {
        var userId = CurrentUserId;

        var from = await GetUserStringAsync(
            ShipRecToolsSettingsKeys.LotMarking.ListFromDate,
            string.Empty,
            userId
        );
        var to = await GetUserStringAsync(
            ShipRecToolsSettingsKeys.LotMarking.ListToDate,
            string.Empty,
            userId
        );
        var sortFieldRaw = await GetUserStringAsync(
            ShipRecToolsSettingsKeys.LotMarking.ListSortField,
            nameof(Enum_LotMarkingSortField.LastReceiptDate),
            userId
        );
        var descendingRaw = await GetUserStringAsync(
            ShipRecToolsSettingsKeys.LotMarking.ListSortDescending,
            "true",
            userId
        );

        var sortField = Enum.TryParse<Enum_LotMarkingSortField>(sortFieldRaw, true, out var field)
            ? field
            : Enum_LotMarkingSortField.LastReceiptDate;

        var isDescending =
            bool.TryParse(descendingRaw, out var parsedDescending) && parsedDescending;

        return new Model_Tool_LotMarkingFilter(
            ParseStoredDate(from),
            ParseStoredDate(to),
            sortField,
            isDescending
        );
    }

    public async Task<Model_Dao_Result> SaveLotMarkingListFilterAsync(
        Model_Tool_LotMarkingFilter filter
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        var userId = CurrentUserId;

        var writes = new[]
        {
            (
                ShipRecToolsSettingsKeys.LotMarking.ListFromDate,
                FormatStoredDate(filter.FromDate)
            ),
            (ShipRecToolsSettingsKeys.LotMarking.ListToDate, FormatStoredDate(filter.ToDate)),
            (
                ShipRecToolsSettingsKeys.LotMarking.ListSortField,
                filter.SortField.ToString()
            ),
            (
                ShipRecToolsSettingsKeys.LotMarking.ListSortDescending,
                filter.IsDescending.ToString()
            ),
        };

        // Saved one key at a time because that is the shape the settings store offers. The first
        // failure is returned and the rest are skipped, so a user is told once rather than four times.
        foreach (var (key, value) in writes)
        {
            var result = await _settings.SetSettingAsync(
                ShipRecToolsSettingsKeys.Category,
                key,
                value,
                userId
            );

            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Model_Dao_Result_Factory.Success();
    }

    public async Task<HashSet<string>> GetLotMarkingListVisibleColumnKeysAsync()
    {
        var raw = await GetUserStringAsync(
            ShipRecToolsSettingsKeys.LotMarking.ListVisibleColumnKeys,
            string.Empty,
            CurrentUserId
        );

        var keys = ParseStringList(raw);
        var normalized = Model_Tool_LotMarkingColumnCatalog.Normalize(keys);

        // Turning every column off leaves nothing to read, so the default comes back rather than an
        // empty grid a user cannot recover from without opening the Filter window.
        return normalized.Count > 0
            ? normalized
            : Model_Tool_LotMarkingColumnCatalog
                .Normalize(Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys);
    }

    public Task<Model_Dao_Result> SaveLotMarkingListVisibleColumnKeysAsync(
        IReadOnlyCollection<string> columnKeys
    )
    {
        var serialized = JsonSerializer.Serialize(
            Model_Tool_LotMarkingColumnCatalog.Normalize(columnKeys)
        );

        return _settings.SetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            ShipRecToolsSettingsKeys.LotMarking.ListVisibleColumnKeys,
            serialized,
            CurrentUserId
        );
    }

    /// <summary>
    /// The signed-in employee number, which is what the settings store keys a user-scoped value by.
    /// Null before anyone signs in, which those settings reject rather than store against the wrong
    /// person.
    /// </summary>
    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    private async Task<string> GetUserStringAsync(string key, string fallback, int? userId)
    {
        var result = await _settings.GetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            key,
            userId
        );

        return result.IsSuccess
            && result.Data is not null
            && string.IsNullOrWhiteSpace(result.Data.Value) is false
            ? result.Data.Value
            : fallback;
    }

    private static DateTime? ParseStoredDate(string value) =>
        DateTime.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed
        )
            ? parsed
            : null;

    private static string FormatStoredDate(DateTime? value) =>
        value is null ? string.Empty : value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static List<string> ParseStringList(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(raw) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<decimal?> GetReceivingAnalyticsOnTimeTargetAsync()
    {
        var result = await _settings.GetSettingAsync(
            ShipRecToolsSettingsKeys.Category,
            ShipRecToolsSettingsKeys.ReceivingAnalytics.OnTimeTarget
        );
        if (
            !result.IsSuccess
            || result.Data is null
            || string.IsNullOrWhiteSpace(result.Data.Value)
        )
        {
            return null;
        }

        return decimal.TryParse(
            result.Data.Value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var parsed
        )
            ? parsed
            : null;
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

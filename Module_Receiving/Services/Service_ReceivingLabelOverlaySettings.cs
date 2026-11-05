using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Stores each Label Overlay output's printer setup and list filter as JSON in the per-user settings
/// table, through the Settings.Core facade.
/// </summary>
/// <remarks>
/// Every read is total: a missing, unreadable or hand-edited value falls back to that output's defaults
/// rather than failing, because a broken setting must never stop the screen from opening. Writes are only
/// ever to the user's own row — a value chosen here cannot affect another user or the module's defaults.
/// </remarks>
public sealed class Service_ReceivingLabelOverlaySettings : IService_ReceivingLabelOverlaySettings
{
    private const string Category = "Receiving";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IService_SettingsCoreFacade _settings;

    public Service_ReceivingLabelOverlaySettings(IService_SettingsCoreFacade settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<Model_Dao_Result<Model_LabelOverlayPrintSettings>> GetPrintProfileAsync(
        Enum_LabelOverlayPrintFormat format,
        int? userId = null
    )
    {
        var stored = await ReadAsync(PrintProfileKey(format), userId);

        if (stored is null)
        {
            return Model_Dao_Result_Factory.Success(CreateDefaultProfile(format));
        }

        try
        {
            var settings = JsonSerializer.Deserialize<Model_LabelOverlayPrintSettings>(
                stored,
                SerializerOptions
            );

            return Model_Dao_Result_Factory.Success(
                settings is null ? CreateDefaultProfile(format) : RepairProfile(settings, format)
            );
        }
        catch (JsonException)
        {
            return Model_Dao_Result_Factory.Success(CreateDefaultProfile(format));
        }
    }

    public async Task<Model_Dao_Result> SavePrintProfileAsync(
        Enum_LabelOverlayPrintFormat format,
        Model_LabelOverlayPrintSettings settings,
        int? userId = null
    )
    {
        ArgumentNullException.ThrowIfNull(settings);

        return await WriteAsync(
            PrintProfileKey(format),
            JsonSerializer.Serialize(RepairProfile(settings, format), SerializerOptions),
            userId
        );
    }

    public async Task<Model_Dao_Result<Model_LabelOverlayListFilter>> GetListFilterAsync(
        Enum_LabelOverlayPrintFormat format,
        int? userId = null
    )
    {
        var stored = await ReadAsync(ListFilterKey(format), userId);

        if (stored is null)
        {
            return Model_Dao_Result_Factory.Success(Model_LabelOverlayListFilter.Default);
        }

        try
        {
            var storedFilter = JsonSerializer.Deserialize<Model_StoredListFilter>(
                stored,
                SerializerOptions
            );

            return Model_Dao_Result_Factory.Success(
                storedFilter is null ? Model_LabelOverlayListFilter.Default : storedFilter.ToFilter()
            );
        }
        catch (JsonException)
        {
            return Model_Dao_Result_Factory.Success(Model_LabelOverlayListFilter.Default);
        }
    }

    public async Task<Model_Dao_Result> SaveListFilterAsync(
        Enum_LabelOverlayPrintFormat format,
        Model_LabelOverlayListFilter filter,
        int? userId = null
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        var stored = Model_StoredListFilter.FromFilter(filter);

        return await WriteAsync(
            ListFilterKey(format),
            JsonSerializer.Serialize(stored, SerializerOptions),
            userId
        );
    }

    public async Task<string> GetCardSheetPartIdAsync(int? userId = null) =>
        await ReadAsync(ReceivingSettingsKeys.LabelOverlay.CardSheetPartId, userId) ?? string.Empty;

    public Task<Model_Dao_Result> SaveCardSheetPartIdAsync(string partId, int? userId = null) =>
        WriteAsync(
            ReceivingSettingsKeys.LabelOverlay.CardSheetPartId,
            partId?.Trim() ?? string.Empty,
            userId
        );

    private static string PrintProfileKey(Enum_LabelOverlayPrintFormat format) =>
        format == Enum_LabelOverlayPrintFormat.CardSheet
            ? ReceivingSettingsKeys.LabelOverlay.CardSheetPrintProfile
            : ReceivingSettingsKeys.LabelOverlay.OverlayPrintProfile;

    private static string ListFilterKey(Enum_LabelOverlayPrintFormat format) =>
        format == Enum_LabelOverlayPrintFormat.CardSheet
            ? ReceivingSettingsKeys.LabelOverlay.CardSheetListFilter
            : ReceivingSettingsKeys.LabelOverlay.OverlayListFilter;

    /// <summary>
    /// A profile with this output's own name on it, so a value copied from the other output cannot make
    /// the card sheet print as an overlay.
    /// </summary>
    private static Model_LabelOverlayPrintSettings CreateDefaultProfile(
        Enum_LabelOverlayPrintFormat format
    ) => new() { Format = format };

    /// <summary>Puts implausible values back to something printable, and fixes the output name.</summary>
    private static Model_LabelOverlayPrintSettings RepairProfile(
        Model_LabelOverlayPrintSettings settings,
        Enum_LabelOverlayPrintFormat format
    )
    {
        settings.Format = format;

        if (settings.PaperWidthInches <= 0f || settings.PaperHeightInches <= 0f)
        {
            settings.PaperWidthInches = Model_LabelOverlayPrintSettings.DefaultPaperWidthInches;
            settings.PaperHeightInches = Model_LabelOverlayPrintSettings.DefaultPaperHeightInches;
        }

        if (
            float.IsNaN(settings.CardSheetMarginInches)
            || settings.CardSheetMarginInches < 0f
            || settings.CardSheetMarginInches > 1f
        )
        {
            settings.CardSheetMarginInches =
                Model_LabelOverlayPrintSettings.DefaultCardSheetMarginInches;
        }

        if (
            settings.Copies < 1
            || settings.Copies > Model_LabelOverlayPrintSettings.MaximumCopies
        )
        {
            settings.Copies = Model_LabelOverlayPrintSettings.DefaultCopies;
        }

        if (string.IsNullOrWhiteSpace(settings.PaperName))
        {
            settings.PaperName = "Letter";
        }

        settings.PrinterName = settings.PrinterName?.Trim() ?? string.Empty;

        // A driver block that is not valid base64 would throw on every read, so it is dropped here.
        if (!string.IsNullOrWhiteSpace(settings.DevModeBase64))
        {
            try
            {
                _ = Convert.FromBase64String(settings.DevModeBase64);
            }
            catch (FormatException)
            {
                settings.DevModeBase64 = string.Empty;
            }
        }

        return settings;
    }

    private async Task<string?> ReadAsync(string key, int? userId)
    {
        try
        {
            var result = await _settings.GetSettingAsync(Category, key, userId);

            return result.IsSuccess && result.Data is not null
                && !string.IsNullOrWhiteSpace(result.Data.Value)
                ? result.Data.Value
                : null;
        }
        catch (Exception)
        {
            // Settings that cannot be read are treated as absent; the caller uses its defaults.
            return null;
        }
    }

    private async Task<Model_Dao_Result> WriteAsync(string key, string value, int? userId)
    {
        try
        {
            return await _settings.SetSettingAsync(Category, key, value, userId);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"The Label Overlay setting '{key}' could not be saved: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// The filter as it is written to the table. The sort field is stored by name rather than by number,
    /// so reordering the enum later cannot silently change which column a saved list is sorted by, and
    /// dates are stored as ISO days so they cannot drift with a culture change.
    /// </summary>
    private sealed class Model_StoredListFilter
    {
        public string FromDate { get; set; } = string.Empty;

        public string ToDate { get; set; } = string.Empty;

        public string SortField { get; set; } = string.Empty;

        public bool IsDescending { get; set; }

        public List<string> VisibleColumnKeys { get; set; } = [];

        public static Model_StoredListFilter FromFilter(Model_LabelOverlayListFilter filter) =>
            new()
            {
                FromDate = ToIso(filter.Filter.FromDate),
                ToDate = ToIso(filter.Filter.ToDate),
                SortField = filter.Filter.SortField.ToString(),
                IsDescending = filter.Filter.IsDescending,
                VisibleColumnKeys = filter.VisibleColumnKeys.ToList(),
            };

        public Model_LabelOverlayListFilter ToFilter()
        {
            var sortField = Enum.TryParse<Enum_LotMarkingSortField>(
                SortField,
                ignoreCase: true,
                out var parsed
            )
                ? parsed
                : Model_Tool_LotMarkingFilter.Default.SortField;

            var columns = Model_Tool_LotMarkingColumnCatalog.Normalize(VisibleColumnKeys);

            return new Model_LabelOverlayListFilter(
                new Model_Tool_LotMarkingFilter(
                    ParseIso(FromDate),
                    ParseIso(ToDate),
                    sortField,
                    IsDescending
                ),
                columns.Count == 0
                    ? Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys
                    : columns.ToList()
            );
        }

        private static string ToIso(DateTime? value) =>
            value is null ? string.Empty : value.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        private static DateTime? ParseIso(string? value) =>
            DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed
            )
                ? parsed.Date
                : null;
    }
}

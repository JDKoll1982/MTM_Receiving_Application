using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Settings;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

public class Service_SettingsUserLabelButtons : IService_SettingsUserLabelButtons
{
    private const int MaxButtons = 10;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly Dao_SettingsCoreUser _userSettingsDao;
    private readonly IService_UserSessionManager _sessionManager;

    public Service_SettingsUserLabelButtons(
        IService_SettingsCoreFacade settingsCore,
        Dao_SettingsCoreUser userSettingsDao,
        IService_UserSessionManager sessionManager
    )
    {
        _settingsCore = settingsCore;
        _userSettingsDao = userSettingsDao;
        _sessionManager = sessionManager;
    }

    public async Task<List<Model_MainWindowLabelButton>> GetButtonsAsync()
    {
        var result = await _settingsCore.GetSettingAsync(
            CoreSettingsKeys.UserCategory,
            CoreSettingsKeys.LabelView.MainWindowButtons
        );

        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data?.Value))
        {
            return BuildDefaultButtons();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<Model_MainWindowLabelButton>>(
                result.Data.Value,
                JsonOptions
            );

            var sanitized = Sanitize(parsed);
            return await MigrateLegacyPathsIfNeededAsync(sanitized);
        }
        catch
        {
            var defaults = BuildDefaultButtons();
            return await MigrateLegacyPathsIfNeededAsync(defaults);
        }
    }

    public async Task<Model_Dao_Result> SaveButtonsAsync(List<Model_MainWindowLabelButton> buttons)
    {
        var sanitized = Sanitize(buttons?.ToList());
        var json = JsonSerializer.Serialize(sanitized);

        return await _settingsCore.SetSettingAsync(
            CoreSettingsKeys.UserCategory,
            CoreSettingsKeys.LabelView.MainWindowButtons,
            json
        );
    }

    public async Task<Model_Dao_Result> ResetToDefaultsAsync()
    {
        return await SaveButtonsAsync(BuildDefaultButtons());
    }

    private static List<Model_MainWindowLabelButton> Sanitize(
        IReadOnlyList<Model_MainWindowLabelButton>? buttons
    )
    {
        var source = buttons ?? Array.Empty<Model_MainWindowLabelButton>();
        var valid = source
            .Where(button => button != null)
            .Select(
                button =>
                    new Model_MainWindowLabelButton
                    {
                        Id = string.IsNullOrWhiteSpace(button.Id)
                            ? Guid.NewGuid().ToString("N")
                            : button.Id,
                        Label = string.IsNullOrWhiteSpace(button.Label)
                            ? "Label"
                            : button.Label.Trim(),
                        LabelPath = button.LabelPath?.Trim() ?? string.Empty,
                        IconKey = string.IsNullOrWhiteSpace(button.IconKey)
                            ? "PackageVariantClosed"
                            : button.IconKey,
                        Accent = Enum.IsDefined(button.Accent)
                            ? button.Accent
                            : Enum_MainWindowLabelButtonAccent.Neutral,
                        IsEnabled = button.IsEnabled,
                        SortOrder = button.SortOrder,
                    }
            )
            .OrderBy(button => button.SortOrder)
            .Take(MaxButtons)
            .ToList();

        if (valid.Count == 0)
        {
            valid = BuildDefaultButtons();
        }

        for (var index = 0; index < valid.Count; index++)
        {
            valid[index].SortOrder = index;
        }

        return valid;
    }

    private static List<Model_MainWindowLabelButton> BuildDefaultButtons()
    {
        return new List<Model_MainWindowLabelButton>
        {
            new()
            {
                Id = "receiving",
                Label = "Receiving",
                IconKey = "LabelVariant",
                Accent = Enum_MainWindowLabelButtonAccent.Green,
                IsEnabled = true,
                SortOrder = 0,
            },
            new()
            {
                Id = "mini-receiving",
                Label = "Mini Receiving",
                IconKey = "Label",
                Accent = Enum_MainWindowLabelButtonAccent.Red,
                IsEnabled = true,
                SortOrder = 1,
            },
            new()
            {
                Id = "dunnage",
                Label = "Dunnage",
                IconKey = "PackageVariantClosed",
                Accent = Enum_MainWindowLabelButtonAccent.Blue,
                IsEnabled = true,
                SortOrder = 2,
            },
            new()
            {
                Id = "volvo",
                Label = "Volvo",
                IconKey = "Car",
                Accent = Enum_MainWindowLabelButtonAccent.Black,
                IsEnabled = true,
                SortOrder = 3,
            },
        };
    }

    private async Task<List<Model_MainWindowLabelButton>> MigrateLegacyPathsIfNeededAsync(
        List<Model_MainWindowLabelButton> buttons
    )
    {
        var userId = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
        if (userId <= 0)
        {
            return buttons;
        }

        var migrationMap = new Dictionary<string, (string Category, string Key)>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["receiving"] = ("Receiving.UserLabels", "ReceivingLabelPath"),
            ["mini-receiving"] = ("Receiving.UserLabels", "MiniReceivingLabelPath"),
            ["dunnage"] = ("Dunnage.UserLabels", "DunnageLabelPath"),
            ["volvo"] = ("Volvo.UserLabels", "VolvoLabelPath"),
        };

        var migratedAny = false;
        foreach (var button in buttons)
        {
            if (string.IsNullOrWhiteSpace(button.LabelPath) is false)
            {
                continue;
            }

            if (!migrationMap.TryGetValue(button.Id, out var source))
            {
                continue;
            }

            var sourceResult = await _userSettingsDao.GetByKeyAsync(
                userId,
                source.Category,
                source.Key
            );

            var legacyPath = sourceResult.Data?.SettingValue;
            if (string.IsNullOrWhiteSpace(legacyPath))
            {
                continue;
            }

            button.LabelPath = legacyPath.Trim();
            migratedAny = true;
        }

        if (migratedAny)
        {
            await SaveButtonsAsync(buttons);
        }

        return buttons;
    }
}

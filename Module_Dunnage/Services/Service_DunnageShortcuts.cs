using System;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Services;

/// <summary>
/// Persists Dunnage workflow keyboard shortcuts through the shared settings facade.
/// </summary>
public class Service_DunnageShortcuts : IService_DunnageShortcuts
{
    private const string SettingsCategory = "Dunnage";

    private readonly IService_LoggingUtility _logger;
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _userSessionManager;

    public Service_DunnageShortcuts(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager userSessionManager,
        IService_LoggingUtility logger
    )
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _userSessionManager =
            userSessionManager ?? throw new ArgumentNullException(nameof(userSessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Settings_DunnageShortcuts> GetShortcutsAsync()
    {
        var shortcuts = Model_Settings_DunnageShortcuts.CreateDefault();

        try
        {
            shortcuts.ModeSelectionShortcut = await LoadBindingAsync(
                DunnageSettingsKeys.Shortcuts.ModeSelection,
                shortcuts.ModeSelectionShortcut
            );
            shortcuts.ClearLabelDataShortcut = await LoadBindingAsync(
                DunnageSettingsKeys.Shortcuts.ClearLabelData,
                shortcuts.ClearLabelDataShortcut
            );
            shortcuts.NextStepShortcut = await LoadBindingAsync(
                DunnageSettingsKeys.Shortcuts.NextStep,
                shortcuts.NextStepShortcut
            );
            shortcuts.BackStepShortcut = await LoadBindingAsync(
                DunnageSettingsKeys.Shortcuts.BackStep,
                shortcuts.BackStepShortcut
            );
            shortcuts.HelpShortcut = await LoadBindingAsync(
                DunnageSettingsKeys.Shortcuts.Help,
                shortcuts.HelpShortcut
            );
            shortcuts.IsToggleSimpleNavigationEnabled = await LoadBoolAsync(
                DunnageSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
                shortcuts.IsToggleSimpleNavigationEnabled
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to load Dunnage workflow keyboard shortcuts: {ex.Message}",
                ex
            );
        }

        return shortcuts;
    }

    public async Task SaveShortcutsAsync(Model_Settings_DunnageShortcuts shortcuts)
    {
        ArgumentNullException.ThrowIfNull(shortcuts);

        await SaveBindingAsync(
            DunnageSettingsKeys.Shortcuts.ModeSelection,
            shortcuts.ModeSelectionShortcut
        );
        await SaveBindingAsync(
            DunnageSettingsKeys.Shortcuts.ClearLabelData,
            shortcuts.ClearLabelDataShortcut
        );
        await SaveBindingAsync(DunnageSettingsKeys.Shortcuts.NextStep, shortcuts.NextStepShortcut);
        await SaveBindingAsync(DunnageSettingsKeys.Shortcuts.BackStep, shortcuts.BackStepShortcut);
        await SaveBindingAsync(DunnageSettingsKeys.Shortcuts.Help, shortcuts.HelpShortcut);

        await SaveSettingAsync(
            DunnageSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
            shortcuts.IsToggleSimpleNavigationEnabled.ToString()
        );
    }

    public async Task SaveToggleSimpleNavigationEnabledAsync(bool isEnabled)
    {
        await SaveSettingAsync(
            DunnageSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
            isEnabled.ToString()
        );
    }

    private int? CurrentUserId => _userSessionManager.CurrentSession?.User?.EmployeeNumber;

    private async Task<Model_KeyboardShortcutBinding> LoadBindingAsync(
        string key,
        Model_KeyboardShortcutBinding fallback
    )
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (
            result.IsSuccess
            && result.Data != null
            && !string.IsNullOrWhiteSpace(result.Data.Value)
        )
        {
            try
            {
                var binding = JsonSerializer.Deserialize<Model_KeyboardShortcutBinding>(
                    result.Data.Value
                );
                if (binding != null)
                {
                    binding.Key = Helper_KeyboardShortcuts.NormalizeKey(binding.Key);
                    return binding;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    $"Shortcut binding '{key}' could not be parsed. Using default. {ex.Message}"
                );
            }
        }

        return fallback.Clone();
    }

    private async Task<bool> LoadBoolAsync(string key, bool fallback)
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (
            result.IsSuccess
            && result.Data != null
            && bool.TryParse(result.Data.Value, out var parsed)
        )
        {
            return parsed;
        }

        return DunnageSettingsDefaults.BoolDefaults.TryGetValue(key, out var defaultValue)
            ? defaultValue
            : fallback;
    }

    private async Task SaveBindingAsync(string key, Model_KeyboardShortcutBinding binding)
    {
        binding.Key = Helper_KeyboardShortcuts.NormalizeKey(binding.Key);
        var serializedBinding = JsonSerializer.Serialize(binding);
        await SaveSettingAsync(key, serializedBinding);
    }

    private async Task SaveSettingAsync(string key, string value)
    {
        var result = await _settingsCore.SetSettingAsync(
            SettingsCategory,
            key,
            value ?? string.Empty,
            CurrentUserId
        );

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to save Dunnage shortcut setting '{key}'. {result.ErrorMessage}"
            );
        }
    }
}

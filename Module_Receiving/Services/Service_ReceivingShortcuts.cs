using System;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Persists receiving workflow keyboard shortcuts through the shared settings facade.
/// </summary>
public class Service_ReceivingShortcuts : IService_ReceivingShortcuts
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_LoggingUtility _logger;
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _userSessionManager;

    public Service_ReceivingShortcuts(
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

    /// <inheritdoc />
    public async Task<Model_Settings_ReceivingShortcuts> GetShortcutsAsync()
    {
        var shortcuts = Model_Settings_ReceivingShortcuts.CreateDefault();

        try
        {
            shortcuts.ModeSelectionShortcut = await LoadBindingAsync(
                ReceivingSettingsKeys.Shortcuts.ModeSelection,
                shortcuts.ModeSelectionShortcut
            );
            shortcuts.ClearLabelDataShortcut = await LoadBindingAsync(
                ReceivingSettingsKeys.Shortcuts.ClearLabelData,
                shortcuts.ClearLabelDataShortcut
            );
            shortcuts.NextStepShortcut = await LoadBindingAsync(
                ReceivingSettingsKeys.Shortcuts.NextStep,
                shortcuts.NextStepShortcut
            );
            shortcuts.BackStepShortcut = await LoadBindingAsync(
                ReceivingSettingsKeys.Shortcuts.BackStep,
                shortcuts.BackStepShortcut
            );
            shortcuts.HelpShortcut = await LoadBindingAsync(
                ReceivingSettingsKeys.Shortcuts.Help,
                shortcuts.HelpShortcut
            );
            shortcuts.IsToggleSimpleNavigationEnabled = await LoadBoolAsync(
                ReceivingSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
                shortcuts.IsToggleSimpleNavigationEnabled
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to load receiving workflow keyboard shortcuts: {ex.Message}",
                ex
            );
        }

        return shortcuts;
    }

    /// <inheritdoc />
    public async Task SaveShortcutsAsync(Model_Settings_ReceivingShortcuts shortcuts)
    {
        ArgumentNullException.ThrowIfNull(shortcuts);

        await SaveBindingAsync(
            ReceivingSettingsKeys.Shortcuts.ModeSelection,
            shortcuts.ModeSelectionShortcut
        );
        await SaveBindingAsync(
            ReceivingSettingsKeys.Shortcuts.ClearLabelData,
            shortcuts.ClearLabelDataShortcut
        );
        await SaveBindingAsync(
            ReceivingSettingsKeys.Shortcuts.NextStep,
            shortcuts.NextStepShortcut
        );
        await SaveBindingAsync(
            ReceivingSettingsKeys.Shortcuts.BackStep,
            shortcuts.BackStepShortcut
        );
        await SaveBindingAsync(ReceivingSettingsKeys.Shortcuts.Help, shortcuts.HelpShortcut);

        await SaveSettingAsync(
            ReceivingSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
            shortcuts.IsToggleSimpleNavigationEnabled.ToString()
        );
    }

    /// <inheritdoc />
    public async Task SaveToggleSimpleNavigationEnabledAsync(bool isEnabled)
    {
        await SaveSettingAsync(
            ReceivingSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled,
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

        return ReceivingSettingsDefaults.BoolDefaults.TryGetValue(key, out var defaultValue)
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
                $"Failed to save receiving shortcut setting '{key}'. {result.ErrorMessage}"
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_PartFormatting : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Receiving";
    private const string WarehouseCode = "002";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_ReceivingValidation _receivingValidation;
    private readonly IService_InforVisual _inforVisualService;

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    [ObservableProperty]
    private bool _isPaddingEnabled;

    [ObservableProperty]
    private ObservableCollection<Model_PartNumberPrefixRule> _prefixRules;

    [ObservableProperty]
    private Model_PartNumberPrefixRule? _selectedRule;

    [ObservableProperty]
    private string _testInput = string.Empty;

    [ObservableProperty]
    private string _testOutput = string.Empty;

    public ViewModel_Settings_Receiving_PartFormatting(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ReceivingValidation receivingValidation,
        IService_InforVisual inforVisualService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        Title = "Receiving Settings";
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _receivingValidation =
            receivingValidation ?? throw new ArgumentNullException(nameof(receivingValidation));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _prefixRules = new ObservableCollection<Model_PartNumberPrefixRule>();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await LoadSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error initializing receiving part formatting settings: {ex.Message}",
                ex
            );
            await _errorHandler.HandleErrorAsync(
                "Failed to initialize receiving part formatting settings",
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                ex
            );
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsPaddingEnabled = await GetBoolSettingAsync(
                ReceivingSettingsKeys.PartNumberPadding.Enabled,
                true
            );
            var rulesJson = await GetStringSettingAsync(
                ReceivingSettingsKeys.PartNumberPadding.RulesJson
            );

            var rulesToApply = new List<Model_PartNumberPrefixRule>();
            if (!string.IsNullOrWhiteSpace(rulesJson))
            {
                var rules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(rulesJson);
                if (rules != null)
                {
                    foreach (var rule in rules)
                    {
                        ApplyDefaultRuleName(rule);
                        rulesToApply.Add(rule);
                    }
                }
            }

            if (rulesToApply.Count == 0)
            {
                rulesToApply.Add(CreateDefaultRule("Coil", "MMC"));
                rulesToApply.Add(CreateDefaultRule("Flatstock", "MMF"));
            }

            ReplacePrefixRules(rulesToApply);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading receiving part formatting settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to load receiving part formatting settings",
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                ex
            );
        }
    }

    [RelayCommand]
    private void AddRule()
    {
        var newRule = new Model_PartNumberPrefixRule
        {
            Name = "New Rule",
            Prefix = "NEW",
            MaxLength = 10,
            PadChar = '0',
            IsEnabled = true,
        };
        PrefixRules.Add(newRule);
        SelectedRule = newRule;
        _logger.LogInfo("Added new part number padding rule");
    }

    [RelayCommand]
    private void RemoveRule()
    {
        if (SelectedRule == null)
        {
            return;
        }

        var removedRule = SelectedRule;
        var removedIndex = PrefixRules.IndexOf(removedRule);
        PrefixRules.Remove(removedRule);
        SelectedRule =
            PrefixRules.Count == 0
                ? null
                : PrefixRules[Math.Min(removedIndex, PrefixRules.Count - 1)];
        _logger.LogInfo($"Removed part number padding rule with prefix: {removedRule.Prefix}");
        TestPadding();
    }

    [RelayCommand]
    private void TestPadding()
    {
        if (string.IsNullOrWhiteSpace(TestInput))
        {
            TestOutput = string.Empty;
            return;
        }

        var input = TestInput.Trim();
        var result = IsPaddingEnabled
            ? Model_PartNumberPrefixRule.ApplyBestMatchingRule(PrefixRules.ToArray(), input)
            : input;

        TestOutput = result;
    }

    partial void OnTestInputChanged(string value)
    {
        TestPadding();
    }

    partial void OnIsPaddingEnabledChanged(bool value)
    {
        TestPadding();
    }


    public void RefreshTestOutput()
    {
        TestPadding();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving...";

            await SaveSettingAsync(
                ReceivingSettingsKeys.PartNumberPadding.Enabled,
                IsPaddingEnabled.ToString()
            );
            await SaveSettingAsync(
                ReceivingSettingsKeys.PartNumberPadding.RulesJson,
                JsonSerializer.Serialize(PrefixRules.ToArray())
            );

            ShowStatus("Receiving part formatting settings saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving receiving part formatting settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to save receiving part formatting settings",
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                ex
            );
            StatusMessage = "Save failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        try
        {
            IsBusy = true;
            await ResetSettingAsync(ReceivingSettingsKeys.PartNumberPadding.Enabled);
            await ResetSettingAsync(ReceivingSettingsKeys.PartNumberPadding.RulesJson);
            TestInput = string.Empty;
            TestOutput = string.Empty;
            ShowStatus("Receiving part formatting settings reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset receiving part formatting settings",
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> GetBoolSettingAsync(string key, bool fallback)
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

        return fallback;
    }

    private async Task<string> GetStringSettingAsync(string key)
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (result.IsSuccess && result.Data != null)
        {
            return result.Data.Value;
        }

        if (ReceivingSettingsDefaults.StringDefaults.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return string.Empty;
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
            await _errorHandler.HandleDaoErrorAsync(result, $"Save {key}");
        }
    }

    private async Task ResetSettingAsync(string key)
    {
        var result = await _settingsCore.ResetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Reset {key}");
        }
    }

    private static Model_PartNumberPrefixRule CreateDefaultRule(string name, string prefix)
    {
        return new Model_PartNumberPrefixRule
        {
            Name = name,
            Prefix = prefix,
            MaxLength = 10,
            PadChar = '0',
            IsEnabled = true,
        };
    }

    private static void ApplyDefaultRuleName(Model_PartNumberPrefixRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.Name))
        {
            return;
        }

        rule.Name = rule.Prefix?.Trim().ToUpperInvariant() switch
        {
            "MMC" => "Coil",
            "MMF" => "Flatstock",
            _ => string.Empty,
        };
    }

    private void ReplacePrefixRules(IEnumerable<Model_PartNumberPrefixRule> rules)
    {
        var selectedPrefix = SelectedRule?.Prefix?.Trim();
        var selectedName = SelectedRule?.Name?.Trim();

        PrefixRules = new ObservableCollection<Model_PartNumberPrefixRule>(rules);

        if (PrefixRules.Count == 0)
        {
            SelectedRule = null;
            return;
        }

        SelectedRule = PrefixRules.FirstOrDefault(rule =>
            string.Equals(rule.Prefix?.Trim(), selectedPrefix, StringComparison.OrdinalIgnoreCase)
            && string.Equals(rule.Name?.Trim(), selectedName, StringComparison.OrdinalIgnoreCase)
        );

        SelectedRule ??= PrefixRules[0];
    }


    private static string NormalizeLocationForMatch(string? location)
    {
        return new string(
            (location ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray()
        );
    }
}

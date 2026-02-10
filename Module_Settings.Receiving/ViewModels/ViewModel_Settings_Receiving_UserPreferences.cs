using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_UserPreferences : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Receiving";
    
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    [ObservableProperty]
    private bool _isPaddingEnabled;

    [ObservableProperty]
    private ObservableCollection<Model_PartNumberPrefixRule> _prefixRules;

    [ObservableProperty]
    private Model_PartNumberPrefixRule? _selectedRule;

    // Test input/output
    [ObservableProperty]
    private string _testInput = string.Empty;

    [ObservableProperty]
    private string _testOutput = string.Empty;

    public ViewModel_Settings_Receiving_UserPreferences(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) 
        : base(errorHandler, logger, notificationService)
    {
        Title = "Part Number Padding";
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
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
            _logger.LogError($"Error initializing part number padding settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync("Failed to initialize part number padding settings", Module_Core.Models.Enums.Enum_ErrorSeverity.Medium, ex);
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsPaddingEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.PartNumberPadding.Enabled, true);
            var rulesJson = await GetStringSettingAsync(ReceivingSettingsKeys.PartNumberPadding.RulesJson);

            if (!string.IsNullOrWhiteSpace(rulesJson))
            {
                var rules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(rulesJson);
                if (rules != null)
                {
                    PrefixRules.Clear();
                    foreach (var rule in rules)
                    {
                        PrefixRules.Add(rule);
                    }
                }
            }

            // Add default rule if none exist
            if (PrefixRules.Count == 0)
            {
                PrefixRules.Add(new Model_PartNumberPrefixRule
                {
                    Prefix = "MMC",
                    MaxLength = 10,
                    PadChar = '0',
                    IsEnabled = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading part number padding settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync("Failed to load part number padding settings", Module_Core.Models.Enums.Enum_ErrorSeverity.Medium, ex);
        }
    }

    [RelayCommand]
    private void AddRule()
    {
        var newRule = new Model_PartNumberPrefixRule
        {
            Prefix = "NEW",
            MaxLength = 10,
            PadChar = '0',
            IsEnabled = true
        };
        PrefixRules.Add(newRule);
        SelectedRule = newRule;
        _logger.LogInfo("Added new part number padding rule");
    }

    [RelayCommand]
    private void RemoveRule()
    {
        if (SelectedRule != null)
        {
            PrefixRules.Remove(SelectedRule);
            _logger.LogInfo($"Removed part number padding rule with prefix: {SelectedRule.Prefix}");
            SelectedRule = null;
        }
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
        var result = input;

        if (IsPaddingEnabled)
        {
            foreach (var rule in PrefixRules.Where(r => r.IsEnabled))
            {
                result = rule.FormatPartNumber(result);
            }
        }

        TestOutput = result;
        _logger.LogInfo($"Test padding: '{input}' → '{result}'");
    }

    partial void OnTestInputChanged(string value)
    {
        TestPadding();
    }

    partial void OnIsPaddingEnabledChanged(bool value)
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

            // Save enabled flag
            await SaveSettingAsync(ReceivingSettingsKeys.PartNumberPadding.Enabled, IsPaddingEnabled.ToString());

            // Save rules as JSON
            var rulesJson = JsonSerializer.Serialize(PrefixRules.ToArray());
            await SaveSettingAsync(ReceivingSettingsKeys.PartNumberPadding.RulesJson, rulesJson);

            StatusMessage = "Saved successfully";
            _logger.LogInfo("Part number padding settings saved successfully");
            ShowStatus("Part number padding settings saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving part number padding settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync("Failed to save part number padding settings", Module_Core.Models.Enums.Enum_ErrorSeverity.Medium, ex);
            StatusMessage = "Save failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> GetBoolSettingAsync(string key, bool fallback)
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (result.IsSuccess && result.Data != null && bool.TryParse(result.Data.Value, out var parsed))
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
        var result = await _settingsCore.SetSettingAsync(SettingsCategory, key, value ?? string.Empty, CurrentUserId);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Save {key}");
        }
    }
}




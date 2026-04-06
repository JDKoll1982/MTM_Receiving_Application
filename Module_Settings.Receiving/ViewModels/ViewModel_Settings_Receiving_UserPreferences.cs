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

public sealed partial class ViewModel_Settings_Receiving_UserPreferences : ViewModel_Shared_Base
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
    private bool _validateAllHistoryForLocationReconciliation;

    [ObservableProperty]
    private ObservableCollection<string> _ignoredReconciliationLocations;

    [ObservableProperty]
    private string _pendingIgnoredLocation = string.Empty;

    [ObservableProperty]
    private string? _selectedIgnoredLocation;

    // Test input/output
    [ObservableProperty]
    private string _testInput = string.Empty;

    [ObservableProperty]
    private string _testOutput = string.Empty;

    public ViewModel_Settings_Receiving_UserPreferences(
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
        Title = "Receiving User Preferences";
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _receivingValidation =
            receivingValidation ?? throw new ArgumentNullException(nameof(receivingValidation));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _prefixRules = new ObservableCollection<Model_PartNumberPrefixRule>();
        _ignoredReconciliationLocations = new ObservableCollection<string>();

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
            await _errorHandler.HandleErrorAsync(
                "Failed to initialize part number padding settings",
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
            ValidateAllHistoryForLocationReconciliation = await GetBoolSettingAsync(
                ReceivingSettingsKeys.BusinessRules.ValidateAllHistoryForLocationReconciliation,
                ReceivingSettingsDefaults.BoolDefaults[
                    ReceivingSettingsKeys.BusinessRules.ValidateAllHistoryForLocationReconciliation
                ]
            );
            var rulesJson = await GetStringSettingAsync(
                ReceivingSettingsKeys.PartNumberPadding.RulesJson
            );
            var ignoredLocationsJson = await GetStringSettingAsync(
                ReceivingSettingsKeys.UserPreferences.IgnoredReconciliationLocationsJson
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

            // Add default rule if none exist
            if (rulesToApply.Count == 0)
            {
                rulesToApply.Add(CreateDefaultRule("Coil", "MMC"));
                rulesToApply.Add(CreateDefaultRule("Flatstock", "MMF"));
            }

            ReplacePrefixRules(rulesToApply);
            ReplaceIgnoredLocations(DeserializeIgnoredLocations(ignoredLocationsJson));
            PendingIgnoredLocation = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading part number padding settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to load part number padding settings",
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

    public async Task<Model_ReceivingValidationResult> ValidateIgnoredLocationAsync()
    {
        if (string.IsNullOrWhiteSpace(PendingIgnoredLocation))
        {
            return Model_ReceivingValidationResult.Success();
        }

        var locationToValidate = PendingIgnoredLocation.Trim();
        var validation = await _receivingValidation.ValidateLocationAsync(
            locationToValidate,
            WarehouseCode
        );

        if (validation.IsValid)
        {
            PendingIgnoredLocation = locationToValidate;
        }

        return validation;
    }

    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetIgnoredLocationSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(PendingIgnoredLocation))
        {
            return Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>());
        }

        if (_receivingValidation.UseMockLocationList)
        {
            var normalizedSearch = NormalizeLocationForMatch(PendingIgnoredLocation);
            var locationSuggestions = _receivingValidation
                .PresetLocations.Where(presetLocation =>
                {
                    var normalizedPreset = NormalizeLocationForMatch(presetLocation);
                    return normalizedPreset.Contains(
                        normalizedSearch,
                        StringComparison.OrdinalIgnoreCase
                    );
                })
                .OrderByDescending(presetLocation =>
                    presetLocation.StartsWith(
                        PendingIgnoredLocation.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ThenBy(presetLocation => presetLocation, StringComparer.OrdinalIgnoreCase)
                .Select(presetLocation => new Model_FuzzySearchResult
                {
                    Key = presetLocation,
                    Label = presetLocation,
                    Detail = "Preset mock location",
                })
                .ToList();

            return Model_Dao_Result_Factory.Success(locationSuggestions);
        }

        var fuzzyResult = await _inforVisualService.FuzzySearchLocationsAsync(
            PendingIgnoredLocation.Trim(),
            WarehouseCode
        );
        if (!fuzzyResult.IsSuccess || fuzzyResult.Data is null)
        {
            return fuzzyResult;
        }

        var suggestions = fuzzyResult
            .Data.Where(static result => string.IsNullOrWhiteSpace(result.Label) is false)
            .GroupBy(static result => result.Label.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();

        return Model_Dao_Result_Factory.Success(suggestions);
    }

    public bool TryAddIgnoredLocation(string? location, out string message)
    {
        var trimmedLocation = location?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedLocation))
        {
            message = "Enter a location to ignore before adding it to reconciliation preferences.";
            return false;
        }

        if (
            IgnoredReconciliationLocations.Any(existing =>
                string.Equals(existing, trimmedLocation, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            PendingIgnoredLocation = string.Empty;
            message = $"Location '{trimmedLocation}' is already in the ignore list.";
            return false;
        }

        IgnoredReconciliationLocations.Add(trimmedLocation);
        PendingIgnoredLocation = string.Empty;
        message = $"Location '{trimmedLocation}' will be ignored during reconciliation.";
        return true;
    }

    [RelayCommand]
    private void RemoveIgnoredLocation()
    {
        if (string.IsNullOrWhiteSpace(SelectedIgnoredLocation))
        {
            return;
        }

        var locationToRemove = SelectedIgnoredLocation;
        IgnoredReconciliationLocations.Remove(locationToRemove);
        SelectedIgnoredLocation = null;
        ShowStatus($"Removed '{locationToRemove}' from ignored reconciliation locations.");
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

            // Save enabled flag
            await SaveSettingAsync(
                ReceivingSettingsKeys.PartNumberPadding.Enabled,
                IsPaddingEnabled.ToString()
            );

            if (!string.IsNullOrWhiteSpace(PendingIgnoredLocation))
            {
                var validation = await ValidateIgnoredLocationAsync();
                if (!validation.IsValid)
                {
                    ShowStatus(
                        $"Resolve the pending ignored location before saving. {validation.Message}",
                        InfoBarSeverity.Warning
                    );
                    StatusMessage = "Save blocked";
                    return;
                }

                if (TryAddIgnoredLocation(PendingIgnoredLocation, out var addMessage))
                {
                    ShowStatus(addMessage, InfoBarSeverity.Success);
                }
            }

            await SaveSettingAsync(
                ReceivingSettingsKeys.BusinessRules.ValidateAllHistoryForLocationReconciliation,
                ValidateAllHistoryForLocationReconciliation.ToString()
            );

            // Save rules as JSON
            var rulesJson = JsonSerializer.Serialize(PrefixRules.ToArray());
            await SaveSettingAsync(ReceivingSettingsKeys.PartNumberPadding.RulesJson, rulesJson);
            await SaveSettingAsync(
                ReceivingSettingsKeys.UserPreferences.IgnoredReconciliationLocationsJson,
                JsonSerializer.Serialize(IgnoredReconciliationLocations.ToArray())
            );

            StatusMessage = "Saved successfully";
            _logger.LogInfo("Receiving user preferences saved successfully");
            ShowStatus("Receiving user preferences saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving part number padding settings: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to save part number padding settings",
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

    private void ReplaceIgnoredLocations(IEnumerable<string> locations)
    {
        IgnoredReconciliationLocations = new ObservableCollection<string>(
            locations
                .Where(location => string.IsNullOrWhiteSpace(location) is false)
                .Select(location => location.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(location => location, StringComparer.OrdinalIgnoreCase)
        );

        SelectedIgnoredLocation = null;
    }

    private static IEnumerable<string> DeserializeIgnoredLocations(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch
        {
            return json
                .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(location => location.Trim());
        }
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

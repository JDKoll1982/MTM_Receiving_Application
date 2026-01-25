using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_BusinessRules : ViewModel_Shared_Base, ISettingsNavigationActions, ISettingsNavigationNavState
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private bool _autoSaveEnabled;

    [ObservableProperty]
    private string _autoSaveIntervalSeconds = string.Empty;

    [ObservableProperty]
    private bool _saveToCsvEnabled;

    [ObservableProperty]
    private bool _saveToNetworkCsvEnabled;

    [ObservableProperty]
    private bool _saveToDatabaseEnabled;

    [ObservableProperty]
    private string _defaultModeOnStartup = string.Empty;

    [ObservableProperty]
    private bool _rememberLastMode;

    [ObservableProperty]
    private bool _confirmModeChange;

    [ObservableProperty]
    private bool _autoFillHeatLotEnabled;

    [ObservableProperty]
    private bool _savePackageTypeAsDefault;

    [ObservableProperty]
    private bool _showReviewTableByDefault;

    [ObservableProperty]
    private bool _allowEditAfterSave;

    public ViewModel_Settings_Receiving_BusinessRules(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        Title = "Receiving Business Rules";

        _ = LoadSettingsAsync();
    }

    public bool IsBackVisible => false;
    public bool IsNextVisible => false;
    public bool IsCancelVisible => true;
    public bool IsSaveVisible => true;
    public bool IsResetVisible => true;

    public async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveEnabled, AutoSaveEnabled.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveIntervalSeconds, AutoSaveIntervalSeconds);
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToCsvEnabled, SaveToCsvEnabled.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToNetworkCsvEnabled, SaveToNetworkCsvEnabled.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToDatabaseEnabled, SaveToDatabaseEnabled.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.DefaultModeOnStartup, DefaultModeOnStartup);
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.RememberLastMode, RememberLastMode.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ConfirmModeChange, ConfirmModeChange.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoFillHeatLotEnabled, AutoFillHeatLotEnabled.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SavePackageTypeAsDefault, SavePackageTypeAsDefault.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ShowReviewTableByDefault, ShowReviewTableByDefault.ToString());
            await SaveSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AllowEditAfterSave, AllowEditAfterSave.ToString());

            ShowStatus("Receiving business rules saved.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to save receiving business rules.", Enum_ErrorSeverity.Error, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ResetAsync()
    {
        try
        {
            IsBusy = true;

            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveEnabled);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveIntervalSeconds);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToCsvEnabled);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToNetworkCsvEnabled);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToDatabaseEnabled);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.DefaultModeOnStartup);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.RememberLastMode);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ConfirmModeChange);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoFillHeatLotEnabled);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SavePackageTypeAsDefault);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ShowReviewTableByDefault);
            await ResetSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AllowEditAfterSave);

            await LoadSettingsAsync();
            ShowStatus("Receiving business rules reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to reset receiving business rules.", Enum_ErrorSeverity.Error, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CancelAsync()
    {
        await LoadSettingsAsync();
    }

    public Task BackAsync() => Task.CompletedTask;

    public Task NextAsync() => Task.CompletedTask;

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    private async Task LoadSettingsAsync()
    {
        try
        {
            AutoSaveEnabled = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveEnabled, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveEnabled]);
            AutoSaveIntervalSeconds = await GetStringSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveIntervalSeconds);
            SaveToCsvEnabled = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToCsvEnabled, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToCsvEnabled]);
            SaveToNetworkCsvEnabled = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToNetworkCsvEnabled, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToNetworkCsvEnabled]);
            SaveToDatabaseEnabled = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToDatabaseEnabled, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToDatabaseEnabled]);
            DefaultModeOnStartup = await GetStringSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.DefaultModeOnStartup);
            RememberLastMode = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.RememberLastMode, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.RememberLastMode]);
            ConfirmModeChange = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ConfirmModeChange, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ConfirmModeChange]);
            AutoFillHeatLotEnabled = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoFillHeatLotEnabled, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoFillHeatLotEnabled]);
            SavePackageTypeAsDefault = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SavePackageTypeAsDefault, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SavePackageTypeAsDefault]);
            ShowReviewTableByDefault = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ShowReviewTableByDefault, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ShowReviewTableByDefault]);
            AllowEditAfterSave = await GetBoolSettingAsync(Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AllowEditAfterSave, Helper_Receiving_Infrastructure_SettingsDefaults.BoolDefaults[Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AllowEditAfterSave]);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to load receiving business rules.", Enum_ErrorSeverity.Warning, ex, false);
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

        if (Helper_Receiving_Infrastructure_SettingsDefaults.StringDefaults.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        if (Helper_Receiving_Infrastructure_SettingsDefaults.IntDefaults.TryGetValue(key, out var intFallback))
        {
            return intFallback.ToString();
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

    private async Task ResetSettingAsync(string key)
    {
        var result = await _settingsCore.ResetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Reset {key}");
        }
    }
}

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

            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled, AutoSaveEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveIntervalSeconds, AutoSaveIntervalSeconds);
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToCsvEnabled, SaveToCsvEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToNetworkCsvEnabled, SaveToNetworkCsvEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToDatabaseEnabled, SaveToDatabaseEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup, DefaultModeOnStartup);
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.RememberLastMode, RememberLastMode.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.ConfirmModeChange, ConfirmModeChange.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoFillHeatLotEnabled, AutoFillHeatLotEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.SavePackageTypeAsDefault, SavePackageTypeAsDefault.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault, ShowReviewTableByDefault.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.BusinessRules.AllowEditAfterSave, AllowEditAfterSave.ToString());

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

            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveIntervalSeconds);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToCsvEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToNetworkCsvEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToDatabaseEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.RememberLastMode);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.ConfirmModeChange);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoFillHeatLotEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.SavePackageTypeAsDefault);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.AllowEditAfterSave);

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
            AutoSaveEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled]);
            AutoSaveIntervalSeconds = await GetStringSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveIntervalSeconds);
            SaveToCsvEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToCsvEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.SaveToCsvEnabled]);
            SaveToNetworkCsvEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToNetworkCsvEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.SaveToNetworkCsvEnabled]);
            SaveToDatabaseEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.SaveToDatabaseEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.SaveToDatabaseEnabled]);
            DefaultModeOnStartup = await GetStringSettingAsync(ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup);
            RememberLastMode = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.RememberLastMode, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.RememberLastMode]);
            ConfirmModeChange = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.ConfirmModeChange, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.ConfirmModeChange]);
            AutoFillHeatLotEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoFillHeatLotEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.AutoFillHeatLotEnabled]);
            SavePackageTypeAsDefault = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.SavePackageTypeAsDefault, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.SavePackageTypeAsDefault]);
            ShowReviewTableByDefault = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault]);
            AllowEditAfterSave = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.AllowEditAfterSave, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.AllowEditAfterSave]);
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

        if (ReceivingSettingsDefaults.StringDefaults.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        if (ReceivingSettingsDefaults.IntDefaults.TryGetValue(key, out var intFallback))
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

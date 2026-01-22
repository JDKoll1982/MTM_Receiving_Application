using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_Integrations : ViewModel_Shared_Base, ISettingsNavigationActions, ISettingsNavigationNavState
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private bool _erpSyncEnabled;

    [ObservableProperty]
    private bool _autoPullPoDataEnabled;

    [ObservableProperty]
    private bool _autoPullPartDataEnabled;

    [ObservableProperty]
    private bool _syncToInforVisual;

    [ObservableProperty]
    private string _erpConnectionTimeout = string.Empty;

    [ObservableProperty]
    private bool _retryFailedSyncs;

    [ObservableProperty]
    private string _maxSyncRetries = string.Empty;

    public ViewModel_Settings_Receiving_Integrations(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        Title = "Receiving Integrations";

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

            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.ErpSyncEnabled, ErpSyncEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.AutoPullPoDataEnabled, AutoPullPoDataEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.AutoPullPartDataEnabled, AutoPullPartDataEnabled.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.SyncToInforVisual, SyncToInforVisual.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.ErpConnectionTimeout, ErpConnectionTimeout);
            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.RetryFailedSyncs, RetryFailedSyncs.ToString());
            await SaveSettingAsync(ReceivingSettingsKeys.Integrations.MaxSyncRetries, MaxSyncRetries);

            ShowStatus("Receiving integrations saved.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to save receiving integrations.", Enum_ErrorSeverity.Error, ex);
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

            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.ErpSyncEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.AutoPullPoDataEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.AutoPullPartDataEnabled);
            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.SyncToInforVisual);
            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.ErpConnectionTimeout);
            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.RetryFailedSyncs);
            await ResetSettingAsync(ReceivingSettingsKeys.Integrations.MaxSyncRetries);

            await LoadSettingsAsync();
            ShowStatus("Receiving integrations reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to reset receiving integrations.", Enum_ErrorSeverity.Error, ex);
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
            ErpSyncEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.Integrations.ErpSyncEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Integrations.ErpSyncEnabled]);
            AutoPullPoDataEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.Integrations.AutoPullPoDataEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Integrations.AutoPullPoDataEnabled]);
            AutoPullPartDataEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.Integrations.AutoPullPartDataEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Integrations.AutoPullPartDataEnabled]);
            SyncToInforVisual = await GetBoolSettingAsync(ReceivingSettingsKeys.Integrations.SyncToInforVisual, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Integrations.SyncToInforVisual]);
            ErpConnectionTimeout = await GetStringSettingAsync(ReceivingSettingsKeys.Integrations.ErpConnectionTimeout);
            RetryFailedSyncs = await GetBoolSettingAsync(ReceivingSettingsKeys.Integrations.RetryFailedSyncs, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Integrations.RetryFailedSyncs]);
            MaxSyncRetries = await GetStringSettingAsync(ReceivingSettingsKeys.Integrations.MaxSyncRetries);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to load receiving integrations.", Enum_ErrorSeverity.Warning, ex, false);
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

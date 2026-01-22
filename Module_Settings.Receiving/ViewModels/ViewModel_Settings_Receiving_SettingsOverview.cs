using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_SettingsOverview : ViewModel_Shared_Base, ISettingsNavigationActions, ISettingsNavigationNavState
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private string _defaultReceivingMode = string.Empty;

    [ObservableProperty]
    private bool _autoSaveEnabled;

    [ObservableProperty]
    private string _autoSaveEnabledSummary = string.Empty;

    [ObservableProperty]
    private bool _erpSyncEnabled;

    [ObservableProperty]
    private string _erpSyncEnabledSummary = string.Empty;

    public ViewModel_Settings_Receiving_SettingsOverview(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        Title = "Receiving Settings";

        _ = LoadSettingsAsync();
    }

    public bool IsBackVisible => false;
    public bool IsNextVisible => false;
    public bool IsCancelVisible => false;
    public bool IsSaveVisible => false;
    public bool IsResetVisible => false;

    public Task SaveAsync() => Task.CompletedTask;

    public Task ResetAsync() => Task.CompletedTask;

    public Task CancelAsync() => Task.CompletedTask;

    public Task BackAsync() => Task.CompletedTask;

    public Task NextAsync() => Task.CompletedTask;

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    private async Task LoadSettingsAsync()
    {
        try
        {
            DefaultReceivingMode = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode);
            AutoSaveEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled]);
            ErpSyncEnabled = await GetBoolSettingAsync(ReceivingSettingsKeys.Integrations.ErpSyncEnabled, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Integrations.ErpSyncEnabled]);

            AutoSaveEnabledSummary = AutoSaveEnabled ? "Enabled" : "Disabled";
            ErpSyncEnabledSummary = ErpSyncEnabled ? "Enabled" : "Disabled";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to load receiving settings overview.", Enum_ErrorSeverity.Warning, ex, false);
        }
    }

    private async Task<string> GetStringSettingAsync(string key)
    {
        var result = await _settingsCore.GetSettingAsync(SettingsCategory, key, CurrentUserId);
        if (result.IsSuccess && result.Data != null)
        {
            return result.Data.Value;
        }

        return ReceivingSettingsDefaults.StringDefaults.TryGetValue(key, out var fallback)
            ? fallback
            : string.Empty;
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
}

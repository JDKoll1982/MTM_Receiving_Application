using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_Defaults : ViewModel_Shared_Base, ISettingsNavigationActions, ISettingsNavigationNavState
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private string _defaultPackageType = string.Empty;

    [ObservableProperty]
    private string _defaultPackagesPerLoad = string.Empty;

    [ObservableProperty]
    private string _defaultWeightPerPackage = string.Empty;

    [ObservableProperty]
    private string _defaultUnitOfMeasure = string.Empty;

    [ObservableProperty]
    private string _defaultLocation = string.Empty;

    [ObservableProperty]
    private string _defaultLoadNumberPrefix = string.Empty;

    [ObservableProperty]
    private string _defaultReceivingMode = string.Empty;

    public ViewModel_Settings_Receiving_Defaults(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        Title = "Receiving Defaults";

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

            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultPackageType, DefaultPackageType);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultPackagesPerLoad, DefaultPackagesPerLoad);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultWeightPerPackage, DefaultWeightPerPackage);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultUnitOfMeasure, DefaultUnitOfMeasure);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLocation, DefaultLocation);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLoadNumberPrefix, DefaultLoadNumberPrefix);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode, DefaultReceivingMode);

            ShowStatus("Receiving defaults saved.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to save receiving defaults.", Enum_ErrorSeverity.Error, ex);
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

            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultPackageType);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultPackagesPerLoad);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultWeightPerPackage);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultUnitOfMeasure);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLocation);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLoadNumberPrefix);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode);

            await LoadSettingsAsync();
            ShowStatus("Receiving defaults reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to reset receiving defaults.", Enum_ErrorSeverity.Error, ex);
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
            DefaultPackageType = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultPackageType);
            DefaultPackagesPerLoad = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultPackagesPerLoad);
            DefaultWeightPerPackage = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultWeightPerPackage);
            DefaultUnitOfMeasure = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultUnitOfMeasure);
            DefaultLocation = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLocation);
            DefaultLoadNumberPrefix = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLoadNumberPrefix);
            DefaultReceivingMode = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to load receiving defaults.", Enum_ErrorSeverity.Warning, ex, false);
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

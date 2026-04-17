using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_EntryDefaults : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_SettingsErrorHandler _settingsErrorHandler;

    [ObservableProperty]
    private string _defaultLocation = string.Empty;

    public ViewModel_Settings_Receiving_EntryDefaults(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_SettingsErrorHandler settingsErrorHandler,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        _settingsErrorHandler = settingsErrorHandler;
        Title = "Receiving Entry Defaults";
        DefaultLocation = "RECV";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLocation, DefaultLocation);
            await _settingsErrorHandler.ShowSuccessAsync(
                "Receiving entry defaults saved successfully.",
                "Save Successful"
            );
            ShowStatus("Receiving entry defaults saved.");
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                "Failed to save receiving entry defaults.",
                "Save Error",
                ex
            );
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
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultLocation);
            await LoadSettingsAsync();
            ShowStatus("Receiving entry defaults reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset receiving entry defaults.",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private int? CurrentUserId => _sessionManager.CurrentSession?.User?.EmployeeNumber;

    private async Task LoadSettingsAsync()
    {
        try
        {
            DefaultLocation = await GetStringSettingAsync(
                ReceivingSettingsKeys.Defaults.DefaultLocation
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load receiving entry defaults.",
                Enum_ErrorSeverity.Warning,
                ex,
                false
            );
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
        var result = await _settingsCore.SetSettingAsync(
            SettingsCategory,
            key,
            value ?? string.Empty,
            CurrentUserId
        );
        if (!result.IsSuccess)
        {
            await _settingsErrorHandler.HandleErrorAsync(
                result.ErrorMessage ?? "Unknown error occurred",
                $"Save {key}"
            );
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

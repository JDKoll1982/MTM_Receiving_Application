using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using WinRT;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_Defaults : ViewModel_Shared_Base, ISettingsNavigationActions, ISettingsNavigationNavState
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_SettingsErrorHandler _settingsErrorHandler;
    private Window? _settingsWindow;

    [ObservableProperty]
    private string _defaultReceivingMode = string.Empty;

    [ObservableProperty]
    private string _labelTableSaveLocation = string.Empty;

    public ViewModel_Settings_Receiving_Defaults(
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_SettingsErrorHandler settingsErrorHandler,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        _settingsErrorHandler = settingsErrorHandler;
        Title = "Receiving Defaults";

        // Initialize with defaults before async load
        DefaultReceivingMode = "Guided";
        LabelTableSaveLocation = string.Empty;

        // Load current settings asynchronously
        _ = LoadSettingsAsync();
    }

    public bool IsBackVisible => false;
    public bool IsNextVisible => false;
    public bool IsCancelVisible => true;
    public bool IsSaveVisible => true;
    public bool IsResetVisible => true;

    /// <summary>
    /// Sets the settings window to be used for the folder picker dialog.
    /// This should be called from the view's code-behind.
    /// </summary>
    /// <param name="settingsWindow"></param>
    public void SetSettingsWindow(Window settingsWindow)
    {
        _settingsWindow = settingsWindow;
    }

    public async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            // Both settings are user-scoped to ensure they're properly personalized
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode, DefaultReceivingMode);
            await SaveSettingAsync(ReceivingSettingsKeys.Defaults.LabelTableSaveLocation, LabelTableSaveLocation);

            await _settingsErrorHandler.ShowSuccessAsync("Receiving defaults saved successfully.", "Save Successful");
            ShowStatus("Receiving defaults saved.");
        }
        catch (Exception ex)
        {
            await _settingsErrorHandler.HandleErrorAsync("Failed to save receiving defaults.", "Save Error", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PerformSaveAsync()
    {
        await SaveAsync();
    }

    public async Task ResetAsync()
    {
        try
        {
            IsBusy = true;

            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode);
            await ResetSettingAsync(ReceivingSettingsKeys.Defaults.LabelTableSaveLocation);

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
            var mode = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.DefaultReceivingMode);
            var location = await GetStringSettingAsync(ReceivingSettingsKeys.Defaults.LabelTableSaveLocation);
            
            System.Diagnostics.Debug.WriteLine($"[LoadSettings] Mode loaded: '{mode}'");
            System.Diagnostics.Debug.WriteLine($"[LoadSettings] Location loaded: '{location}'");
            
            DefaultReceivingMode = mode;
            LabelTableSaveLocation = location;
            
            System.Diagnostics.Debug.WriteLine($"[LoadSettings] Mode set to: '{DefaultReceivingMode}'");
            System.Diagnostics.Debug.WriteLine($"[LoadSettings] Location set to: '{LabelTableSaveLocation}'");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadSettings] Error: {ex.Message}");
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
            await _settingsErrorHandler.HandleErrorAsync(result.ErrorMessage ?? "Unknown error occurred", $"Save {key}");
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

    [RelayCommand]
    private async Task BrowseLabelTableLocationAsync()
    {
        try
        {
            var folderPicker = new FolderPicker();
            
            // Initialize the folder picker with the settings window
            if (_settingsWindow != null)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WinRT.Interop.WindowNative.GetWindowHandle(_settingsWindow));
            }

            // Set folder picker options
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Show the folder picker dialog
            var folder = await folderPicker.PickSingleFolderAsync();
            
            if (folder != null)
            {
                LabelTableSaveLocation = folder.Path;
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Failed to browse for label database table save location.", Enum_ErrorSeverity.Warning, ex, false);
        }
    }
}

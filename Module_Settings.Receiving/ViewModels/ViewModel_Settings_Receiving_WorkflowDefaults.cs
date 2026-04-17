using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

public sealed partial class ViewModel_Settings_Receiving_WorkflowDefaults : ViewModel_Shared_Base
{
    private const string SettingsCategory = "Receiving";

    private readonly IService_AppSettings _appSettings;
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPreferences _userPreferences;
    private readonly IService_UserPrivileges _userPrivileges;

    [ObservableProperty]
    private string _defaultModeOnStartup = string.Empty;

    [ObservableProperty]
    private bool _confirmModeChange;

    [ObservableProperty]
    private bool _showReviewTableByDefault;

    [ObservableProperty]
    private bool _useInforVisualMockData;

    [ObservableProperty]
    private Visibility _mockDataToggleVisibility = Visibility.Collapsed;

    public IReadOnlyList<ReceivingWorkflowDefaultOption> DefaultModeOptions { get; } =
    [
        new("Mode Selection", nameof(Enum_ReceivingWorkflowStep.ModeSelection)),
        new("Guided Wizard", "Guided"),
        new("Manual Entry", nameof(Enum_ReceivingWorkflowStep.ManualEntry)),
        new("Edit Mode", nameof(Enum_ReceivingWorkflowStep.EditMode)),
    ];

    public ViewModel_Settings_Receiving_WorkflowDefaults(
        IService_AppSettings appSettings,
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_UserPreferences userPreferences,
        IService_UserPrivileges userPrivileges,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _appSettings = appSettings;
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        _userPreferences = userPreferences;
        _userPrivileges = userPrivileges;
        Title = "Receiving Workflow Defaults";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            await SaveSettingAsync(
                ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup,
                DefaultModeOnStartup
            );
            await SaveSettingAsync(
                ReceivingSettingsKeys.BusinessRules.ConfirmModeChange,
                ConfirmModeChange.ToString()
            );
            await SaveSettingAsync(
                ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault,
                ShowReviewTableByDefault.ToString()
            );

            if (MockDataToggleVisibility == Visibility.Visible)
            {
                var appSettingsResult = await _appSettings.SetUseInforVisualMockDataAsync(
                    UseInforVisualMockData
                );
                if (!appSettingsResult.IsSuccess)
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        appSettingsResult,
                        "Save mock data mode"
                    );
                }
            }

            await SyncStartupModeAsync();
            ShowStatus("Receiving workflow defaults saved.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save receiving workflow defaults.",
                Enum_ErrorSeverity.Error,
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
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.ConfirmModeChange);
            await ResetSettingAsync(ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault);

            if (MockDataToggleVisibility == Visibility.Visible)
            {
                var appSettingsResult = await _appSettings.SetUseInforVisualMockDataAsync(false);
                if (!appSettingsResult.IsSuccess)
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        appSettingsResult,
                        "Reset mock data mode"
                    );
                }
            }

            await LoadSettingsAsync();
            await SyncStartupModeAsync();
            ShowStatus("Receiving workflow defaults reset.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset receiving workflow defaults.",
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
            MockDataToggleVisibility = await DetermineMockDataToggleVisibilityAsync();
            DefaultModeOnStartup = await GetStringSettingAsync(
                ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup
            );
            ConfirmModeChange = await GetBoolSettingAsync(
                ReceivingSettingsKeys.BusinessRules.ConfirmModeChange,
                ReceivingSettingsDefaults.BoolDefaults[
                    ReceivingSettingsKeys.BusinessRules.ConfirmModeChange
                ]
            );
            ShowReviewTableByDefault = await GetBoolSettingAsync(
                ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault,
                ReceivingSettingsDefaults.BoolDefaults[
                    ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault
                ]
            );
            UseInforVisualMockData = _appSettings.GetUseInforVisualMockData();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load receiving workflow defaults.",
                Enum_ErrorSeverity.Warning,
                ex,
                false
            );
        }
    }

    private async Task<Visibility> DetermineMockDataToggleVisibilityAsync()
    {
        var currentUserId = CurrentUserId;
        if (!currentUserId.HasValue)
        {
            return Visibility.Collapsed;
        }

        if (!_userPrivileges.IsInitialized || _userPrivileges.CurrentUserId != currentUserId.Value)
        {
            var initializeResult = await _userPrivileges.InitializeAsync(currentUserId.Value);
            if (!initializeResult.Success)
            {
                return Visibility.Collapsed;
            }
        }

        return _userPrivileges.HasPermissionLevel(Enum_SettingsPermissionLevel.Developer)
            ? Visibility.Visible
            : Visibility.Collapsed;
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

        if (ReceivingSettingsDefaults.IntDefaults.TryGetValue(key, out var intFallback))
        {
            return intFallback.ToString();
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

    private async Task SyncStartupModeAsync()
    {
        var currentUser = _sessionManager.CurrentSession?.User;
        if (currentUser == null)
        {
            return;
        }

        var normalizedMode = DefaultModeOnStartup.Trim();
        string? syncedMode = normalizedMode switch
        {
            nameof(Enum_ReceivingWorkflowStep.ManualEntry) or "Manual" => "manual",
            nameof(Enum_ReceivingWorkflowStep.EditMode) or "Edit" => "edit",
            "Guided" => "guided",
            _ => null,
        };

        var result = await _userPreferences.UpdateDefaultReceivingModeAsync(
            currentUser.WindowsUsername,
            syncedMode
        );
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, "Sync startup mode");
            return;
        }

        currentUser.DefaultReceivingMode = syncedMode;
    }
}

public sealed record ReceivingWorkflowDefaultOption(string DisplayName, string Value);

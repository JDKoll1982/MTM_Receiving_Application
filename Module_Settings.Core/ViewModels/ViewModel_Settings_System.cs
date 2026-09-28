using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Defaults;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for system-level core settings.
/// </summary>
public partial class ViewModel_Settings_System : ViewModel_Shared_Base
{
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly ISettingsMetadataRegistry _registry;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPrivileges _userPrivileges;

    private static readonly string[] ThemeOptions = { "System", "Light", "Dark" };
    private static readonly string[] LoggingLevelOptions =
    {
        "Verbose",
        "Debug",
        "Information",
        "Warning",
        "Error",
        "Fatal",
    };

    [ObservableProperty]
    private ObservableCollection<Model_SettingsDefinition> _definitions;

    [ObservableProperty]
    private ObservableCollection<Model_SystemSettingInfoRow> _nonSettableSettings = new();

    [ObservableProperty]
    private bool _canManageSystemSettings;

    [ObservableProperty]
    private string _statusMessage = "Loading system settings...";

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private string _selectedLoggingLevel = "Information";

    [ObservableProperty]
    private string _inactivityTimeoutMinutes = "30";

    [ObservableProperty]
    private string _timerCheckIntervalDisplay = "60 seconds (application constant)";

    [ObservableProperty]
    private string _workstationDefaultTimeoutsDisplay =
        $"Shared terminal: {WorkstationDefaults.SharedTerminalTimeoutMinutes} min, personal workstation: {WorkstationDefaults.PersonalWorkstationTimeoutMinutes} min";

    public IReadOnlyList<string> ThemeChoices => ThemeOptions;
    public IReadOnlyList<string> LoggingLevelChoices => LoggingLevelOptions;

    public ViewModel_Settings_System(
        IService_SettingsCoreFacade settingsCore,
        ISettingsMetadataRegistry registry,
        IService_UserSessionManager sessionManager,
        IService_UserPrivileges userPrivileges,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore;
        _registry = registry;
        _sessionManager = sessionManager;
        _userPrivileges = userPrivileges;

        Definitions = new ObservableCollection<Model_SettingsDefinition>(
            _registry.GetAll().Where(d => d.Scope == Enum_SettingsScope.System)
        );

        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!CanManageSystemSettings)
        {
            StatusMessage = "Admin privileges are required to modify system settings.";
            return;
        }

        if (!int.TryParse(InactivityTimeoutMinutes, out var timeoutMinutes))
        {
            StatusMessage = "Inactivity timeout must be a whole number.";
            return;
        }

        if (timeoutMinutes < 5 || timeoutMinutes > 240)
        {
            StatusMessage = "Inactivity timeout must be between 5 and 240 minutes.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving system settings...";

            await SaveSettingAsync(
                CoreSettingsKeys.SystemCategory,
                "Core.Theme",
                SelectedTheme
            );
            await SaveSettingAsync(
                CoreSettingsKeys.SystemCategory,
                "Core.Logging.Level",
                SelectedLoggingLevel
            );
            await SaveSettingAsync(
                CoreSettingsKeys.SystemCategory,
                CoreSettingsKeys.Session.InactivityTimeoutMinutes,
                timeoutMinutes.ToString()
            );

            if (_sessionManager.CurrentSession is not null)
            {
                _sessionManager.CurrentSession.TimeoutDuration = TimeSpan.FromMinutes(
                    timeoutMinutes
                );
            }

            StatusMessage = "System settings saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_System)
            );
            StatusMessage = "Failed to save system settings.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading system settings...";

            await EnsurePrivilegeStateAsync();
            CanManageSystemSettings = _userPrivileges.HasPermissionLevel(
                Enum_SettingsPermissionLevel.Admin
            );

            SelectedTheme = await GetStringSettingAsync(CoreSettingsKeys.SystemCategory, "Core.Theme");
            if (!ThemeOptions.Contains(SelectedTheme, StringComparer.OrdinalIgnoreCase))
            {
                SelectedTheme = "System";
            }

            SelectedLoggingLevel = await GetStringSettingAsync(
                CoreSettingsKeys.SystemCategory,
                "Core.Logging.Level"
            );
            if (
                !LoggingLevelOptions.Contains(
                    SelectedLoggingLevel,
                    StringComparer.OrdinalIgnoreCase
                )
            )
            {
                SelectedLoggingLevel = "Information";
            }

            var timeoutValue = await GetStringSettingAsync(
                CoreSettingsKeys.SystemCategory,
                CoreSettingsKeys.Session.InactivityTimeoutMinutes
            );
            InactivityTimeoutMinutes = int.TryParse(timeoutValue, out var timeoutMinutes)
                ? Math.Clamp(timeoutMinutes, 5, 240).ToString()
                : "30";

            await LoadNonSettableSettingsAsync();

            StatusMessage = CanManageSystemSettings
                ? "System settings loaded."
                : "Loaded in read-only mode. Admin privileges are required for edits.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Settings_System)
            );
            StatusMessage = "Failed to load system settings.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnsurePrivilegeStateAsync()
    {
        var currentUserId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        if (!currentUserId.HasValue)
        {
            return;
        }

        if (_userPrivileges.IsInitialized && _userPrivileges.CurrentUserId == currentUserId.Value)
        {
            return;
        }

        await _userPrivileges.InitializeAsync(currentUserId.Value);
    }

    private async Task LoadNonSettableSettingsAsync()
    {
        var rows = new List<Model_SystemSettingInfoRow>();
        var nonSettableDefinitions = Definitions
            .Where(definition =>
                definition.IsSensitive
                || definition.PermissionLevel == Enum_SettingsPermissionLevel.Developer
            )
            .OrderBy(definition => definition.DisplayName)
            .ToList();

        foreach (var definition in nonSettableDefinitions)
        {
            var displayValue = definition.IsSensitive
                ? "Stored securely (masked)."
                : await GetStringSettingAsync(definition.Category, definition.Key);

            var reason = definition.IsSensitive
                ? "Sensitive secret is intentionally not editable in this surface."
                : "Developer-level setting is shown for visibility only.";

            rows.Add(
                new Model_SystemSettingInfoRow(
                    definition.DisplayName,
                    definition.Key,
                    displayValue,
                    reason
                )
            );
        }

        rows.Add(
            new Model_SystemSettingInfoRow(
                "Session Timer Check Interval",
                "Runtime.Session.TimerIntervalSeconds",
                TimerCheckIntervalDisplay,
                "Compiled runtime constant (not persisted to settings tables)."
            )
        );
        rows.Add(
            new Model_SystemSettingInfoRow(
                "Workstation Default Timeouts",
                "Runtime.Session.WorkstationDefaults",
                WorkstationDefaultTimeoutsDisplay,
                "Calculated from workstation type defaults in application code."
            )
        );

        NonSettableSettings = new ObservableCollection<Model_SystemSettingInfoRow>(rows);
    }

    private async Task<string> GetStringSettingAsync(string category, string key)
    {
        var result = await _settingsCore.GetSettingAsync(category, key);
        if (result.IsSuccess && result.Data != null)
        {
            return result.Data.Value;
        }

        return string.Empty;
    }

    private async Task SaveSettingAsync(string category, string key, string value)
    {
        var result = await _settingsCore.SetSettingAsync(category, key, value ?? string.Empty);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, $"Save {key}");
            throw new InvalidOperationException(result.ErrorMessage ?? $"Save failed for {key}");
        }
    }
}

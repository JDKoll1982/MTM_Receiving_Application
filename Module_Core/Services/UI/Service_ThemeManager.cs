using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Views;

namespace MTM_Receiving_Application.Module_Core.Services.UI;

/// <summary>
/// Applies persisted user theme settings to the current application window.
/// </summary>
public class Service_ThemeManager : IService_ThemeManager
{
    private const string ThemeCategory = "User";
    private const string ThemeKey = "ui_theme";

    private readonly Dao_User _userDao;
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_LoggingUtility _logger;

    public Service_ThemeManager(
        Dao_User userDao,
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_LoggingUtility logger
    )
    {
        _userDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ElementTheme> GetSavedThemeAsync()
    {
        var userId = await ResolveCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            return ElementTheme.Default;
        }

        var result = await _settingsCore.GetSettingAsync(ThemeCategory, ThemeKey, userId.Value);
        if (
            !result.IsSuccess
            || result.Data is null
            || string.IsNullOrWhiteSpace(result.Data.Value)
        )
        {
            return ElementTheme.Default;
        }

        return ParseTheme(result.Data.Value);
    }

    public async Task ApplySavedThemeAsync()
    {
        var theme = await GetSavedThemeAsync();

        ApplyTheme(theme);
    }

    public async Task ApplyThemeAsync(ElementTheme theme)
    {
        var userId = await ResolveCurrentUserIdAsync();
        if (userId.HasValue)
        {
            var persistResult = await _settingsCore.SetSettingAsync(
                ThemeCategory,
                ThemeKey,
                SerializeTheme(theme),
                userId.Value
            );

            if (!persistResult.IsSuccess)
            {
                _logger.LogWarning(
                    $"Failed to persist ui_theme setting: {persistResult.ErrorMessage}"
                );
            }
        }

        ApplyTheme(theme);
    }

    private void ApplyTheme(ElementTheme theme)
    {
        ApplyThemeToWindow(App.MainWindow, theme);
        ApplyThemeToWindow(View_Settings_CoreWindow.GetActiveHost()?.GetHostWindow(), theme);
    }

    private static void ApplyThemeToWindow(Window? window, ElementTheme theme)
    {
        if (window?.Content is FrameworkElement content)
        {
            content.RequestedTheme = theme;
        }
    }

    private static ElementTheme ParseTheme(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "light" => ElementTheme.Light,
            "dark" => ElementTheme.Dark,
            "default" => ElementTheme.Default,
            _ => ElementTheme.Default,
        };
    }

    private static string SerializeTheme(ElementTheme theme)
    {
        return theme switch
        {
            ElementTheme.Light => "Light",
            ElementTheme.Dark => "Dark",
            _ => "Default",
        };
    }

    private async Task<int?> ResolveCurrentUserIdAsync()
    {
        if (
            _sessionManager.CurrentSession?.User?.EmployeeNumber is int currentUserId
            && currentUserId > 0
        )
        {
            return currentUserId;
        }

        var userResult = await _userDao.GetUserByWindowsUsernameAsync(Environment.UserName);
        if (!userResult.IsSuccess || userResult.Data is null)
        {
            return null;
        }

        return userResult.Data.EmployeeNumber;
    }
}

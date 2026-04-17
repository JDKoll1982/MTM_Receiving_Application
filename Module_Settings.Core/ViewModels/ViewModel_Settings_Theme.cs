using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for core theme settings.
/// </summary>
public partial class ViewModel_Settings_Theme : ViewModel_Shared_Base
{
    private readonly IService_ThemeManager _themeManager;

    private bool _isInitializing;

    [ObservableProperty]
    private string _selectedTheme = ThemeSystemDefaultOption;

    [ObservableProperty]
    private string _statusMessage = "Choose a light or dark theme for your session.";

    public const string ThemeLightOption = "Light";
    public const string ThemeDarkOption = "Dark";
    public const string ThemeSystemDefaultOption = "System Default";

    public IReadOnlyList<string> ThemeOptions { get; } =
    [ThemeLightOption, ThemeDarkOption, ThemeSystemDefaultOption];

    public ViewModel_Settings_Theme(
        IService_ThemeManager themeManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _themeManager =
            themeManager ?? throw new System.ArgumentNullException(nameof(themeManager));
        _ = InitializeAsync();
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (_isInitializing)
        {
            return;
        }

        _ = ApplyThemeAsync(value);
    }

    private async System.Threading.Tasks.Task InitializeAsync()
    {
        try
        {
            _isInitializing = true;
            var savedTheme = await _themeManager.GetSavedThemeAsync();
            SelectedTheme = ToThemeOption(savedTheme);
            StatusMessage = GetStatusMessage(savedTheme, isApplied: false);
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private async System.Threading.Tasks.Task ApplyThemeAsync(string selectedTheme)
    {
        var theme = selectedTheme switch
        {
            ThemeLightOption => ElementTheme.Light,
            ThemeDarkOption => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };

        await _themeManager.ApplyThemeAsync(theme);

        StatusMessage = GetStatusMessage(theme, isApplied: true);
    }

    private static string ToThemeOption(ElementTheme theme)
    {
        return theme switch
        {
            ElementTheme.Light => ThemeLightOption,
            ElementTheme.Dark => ThemeDarkOption,
            _ => ThemeSystemDefaultOption,
        };
    }

    private static string GetStatusMessage(ElementTheme theme, bool isApplied)
    {
        return theme switch
        {
            ElementTheme.Light => isApplied
                ? "Light mode applied and saved."
                : "Light mode is currently active.",
            ElementTheme.Dark => isApplied
                ? "Dark mode applied and saved."
                : "Dark mode is currently active.",
            _ => isApplied
                ? "System default theme applied and saved."
                : "System default theme is currently active.",
        };
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

/// <summary>
/// The brush a converter returns when it is only applying a highlight: the theme's own
/// text colour, so a value the converter touches stays legible in the dark theme instead
/// of being pinned to black.
/// </summary>
public static class Helper_UI_ThemeTextBrush
{
    private static readonly SolidColorBrush LightThemeTextBrush = new(Microsoft.UI.Colors.Black);
    private static readonly SolidColorBrush DarkThemeTextBrush = new(Microsoft.UI.Colors.White);

    /// <summary>The primary text brush for the theme the application window is showing.</summary>
    public static SolidColorBrush PrimaryText =>
        IsDarkTheme() ? DarkThemeTextBrush : LightThemeTextBrush;

    private static bool IsDarkTheme()
    {
        // The theme manager sets RequestedTheme on the window's content, so the content's
        // ActualTheme is the theme the operator sees. The application's own requested theme
        // only follows the operating system and can disagree with a stored user choice.
        if (App.MainWindow?.Content is FrameworkElement content)
        {
            return content.ActualTheme == ElementTheme.Dark;
        }

        return Application.Current?.RequestedTheme == ApplicationTheme.Dark;
    }
}

# Title Bar Button Visibility Fix - Light Mode

## Issue
Window control buttons (minimize, maximize, close) were invisible in light mode across the application. The buttons were present but had white foreground colors on a light background, making them impossible to see.

## Root Cause
Title bar button foreground colors were hardcoded to white (`Color.FromArgb(255, 255, 255, 255)`) in the `ConfigureTitleBar()` methods. This worked in dark mode but made the buttons invisible in light mode.

## Solution
Implemented theme-aware dynamic coloring for title bar buttons:
- **Dark Mode**: White buttons (255, 255, 255) - visible against dark background
- **Light Mode**: Black buttons (0, 0, 0) - visible against light background
- Added `ActualThemeChanged` event subscription to update colors when theme changes

## Files Modified

### 1. MainWindow.xaml.cs
**Changes:**
- Split `ConfigureTitleBar()` into two methods:
  - `ConfigureTitleBar()` - Initial setup
  - `UpdateTitleBarColors()` - Theme-aware color updates
- Added theme detection logic based on `Content.ActualTheme`
- Subscribed to `ActualThemeChanged` event in constructor
- Dynamic button colors:
  - Dark mode: White (#FFFFFF) foreground
  - Light mode: Black (#000000) foreground
  - Inactive colors adjusted appropriately for each theme

### 2. Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs
**Changes:**
- Applied identical fix as MainWindow
- Same `UpdateTitleBarColors()` method implementation
- Same theme change subscription pattern
- Ensures Settings window buttons are visible in both themes

## Testing
### Before Fix
- ✗ Light mode: Buttons invisible (white on white)
- ✓ Dark mode: Buttons visible (white on dark)

### After Fix
- ✓ Light mode: Buttons visible (black on light)
- ✓ Dark mode: Buttons visible (white on dark)
- ✓ Theme switching: Buttons update colors dynamically

## Other Windows Checked
- **View_Shared_IconSelectorWindow**: Uses `ExtendsContentIntoTitleBar` but doesn't customize colors - uses system defaults (correct behavior)
- **View_Shared_SplashScreenWindow**: Doesn't use custom title bar - no changes needed

## Code Pattern

```csharp
private void UpdateTitleBarColors()
{
    if (!AppWindowTitleBar.IsCustomizationSupported())
    {
        return;
    }

    var titleBar = AppWindow.TitleBar;
    var isDarkMode = (Content as FrameworkElement)?.ActualTheme == ElementTheme.Dark;

    if (isDarkMode)
    {
        // White buttons for dark mode
        var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
        titleBar.ButtonForegroundColor = foregroundColor;
        titleBar.ButtonHoverForegroundColor = foregroundColor;
        titleBar.ButtonPressedForegroundColor = foregroundColor;
        titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 160, 160, 160);
    }
    else
    {
        // Black buttons for light mode
        var foregroundColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
        titleBar.ButtonForegroundColor = foregroundColor;
        titleBar.ButtonHoverForegroundColor = foregroundColor;
        titleBar.ButtonPressedForegroundColor = foregroundColor;
        titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 96, 96, 96);
    }
}
```

## Architecture Notes
- Solution is reusable across all windows with custom title bars
- No breaking changes to existing functionality
- Respects user theme preferences (light/dark/system)
- Automatically updates when user switches themes
- Follows WinUI 3 best practices for title bar customization

## Related Documentation
- [WinUI 3 Title bar customization](https://learn.microsoft.com/en-us/windows/apps/develop/title-bar)
- [ElementTheme Enumeration](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.elementtheme)
- [AppWindowTitleBar Class](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.appwindowtitlebar)

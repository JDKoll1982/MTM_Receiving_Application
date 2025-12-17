# Window Sizing Strategy for MTM Receiving Application

## Overview
This document defines the standardized window sizing approach for consistent dialog and window display across the application.

## Window Dimensions

### Splash Screen Window
- **Size**: 500px × 450px
- **Resizable**: No
- **Centered**: Yes
- **Title Bar**: Custom transparent
- **Usage**: Application startup and initialization progress

### Main Application Window
- **Size**: 1200px × 800px (recommended)
- **Resizable**: Yes
- **Maximized on Startup**: Optional
- **Title Bar**: Standard with branding

### ContentDialog Sizing

#### NewUserSetupDialog
- **Width**: 600px (set in XAML)
- **Max Height**: 500px (ScrollViewer)
- **Content**: Two-column form layout
- **Primary Use**: First-time user account creation

#### SharedTerminalLoginDialog
- **Width**: 400px (recommended)
- **Max Height**: 300px (recommended)
- **Content**: Username + PIN entry
- **Primary Use**: Shared terminal authentication

## Implementation

### Using WindowExtensions Helper

All windows should use the `WindowExtensions` helper class for consistent configuration:

```csharp
using MTM_Receiving_Application.Helpers.UI;

public MyWindow()
{
    InitializeComponent();
    
    // Configure window
    this.SetWindowSize(500, 450);
    this.SetFixedSize();
    this.CenterOnScreen();
    this.HideTitleBarIcon();
    this.MakeTitleBarTransparent();
}
```

### Available Extension Methods

- `SetWindowSize(width, height)` - Resize window
- `CenterOnScreen()` - Center on primary display
- `SetFixedSize()` - Make non-resizable
- `HideTitleBarIcon()` - Remove icon and system menu
- `UseCustomTitleBar()` - Extend content into title bar
- `MakeTitleBarTransparent()` - Transparent title bar buttons

## Best Practices

### Window Sizing Guidelines
1. **Splash screens**: 500×450 or smaller
2. **Dialogs**: 400-600px wide, auto-height with max
3. **Main windows**: 1200×800 or larger
4. **Always** center windows on first display
5. **Use fixed sizes** for dialogs and splash screens

### Dialog Sizing Strategy
```csharp
// For ContentDialogs, set width in XAML:
<ContentDialog Width="600" ...>
    <ScrollViewer MaxHeight="500">
        <!-- Content -->
    </ScrollViewer>
</ContentDialog>
```

### Responsive Considerations
- Use `ScrollViewer` with `MaxHeight` for dialogs
- Use `Grid` with star-sized rows for flexible layouts
- Set `MinWidth`/`MinHeight` for main windows
- Test on 1920×1080 (primary) and 1366×768 (minimum)

## Testing Checklist

- [ ] Window appears centered on primary display
- [ ] Window is not resizable (if fixed size)
- [ ] Title bar behaves as expected
- [ ] Content fits without scrolling (or scrolls correctly)
- [ ] Dialogs display properly on top of main window
- [ ] Multiple displays handled correctly

## Display Size Targets

### Primary Target: 1920×1080 (Full HD)
- Main window: 1200×800
- Dialogs: 600px max width
- Splash: 500×450

### Minimum Target: 1366×768 (HD)
- Main window: 1024×768
- Dialogs: 500px max width
- Splash: 450×400

### Secondary Target: 2560×1440 (QHD) and 3840×2160 (4K)
- Scale automatically via Windows DPI settings
- Test with 150% and 200% scaling

## Related Files

- `Helpers\UI\WindowExtensions.cs` - Extension methods
- `Views\Shared\SplashScreenWindow.xaml(.cs)` - Splash screen implementation
- `Views\Shared\NewUserSetupDialog.xaml` - User setup dialog
- `Views\Shared\SharedTerminalLoginDialog.xaml(.cs)` - Login dialog
- `MainWindow.xaml(.cs)` - Main application window

## Future Enhancements

- [ ] Save/restore window positions (user preference)
- [ ] Per-monitor DPI awareness
- [ ] Multi-display positioning preferences
- [ ] Window state persistence (maximized/normal)
- [ ] Custom dialog positioning relative to parent

---

**Last Updated**: January 2025  
**Version**: 1.0.0

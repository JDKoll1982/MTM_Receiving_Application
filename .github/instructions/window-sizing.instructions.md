---
description: Window sizing and positioning standards for consistent UI across the MTM Receiving Application
applyTo: '**/*Window.xaml.cs, **/*Dialog.xaml.cs, **/Views/**/*.xaml'
---

# Window Sizing Strategy

## Window Dimensions Standards

### Splash Screen Window
- **Size**: 500px × 450px
- **Resizable**: No (use OverlappedPresenter.IsResizable = false)
- **Centered**: Yes (use CenterWindow() method)
- **Title Bar**: Custom transparent with hidden buttons

### Main Application Window
- **Size**: 1200px × 800px (recommended initial size)
- **Resizable**: Yes
- **Title Bar**: Standard with branding

### ContentDialog Sizing

#### NewUserSetupDialog
- **Width**: 600px (set in XAML)
- **Max Height**: 500px (use ScrollViewer)
- **Layout**: Two-column form with Grid

#### SharedTerminalLoginDialog
- **Width**: 400px
- **Max Height**: 300px
- **Layout**: Single column, compact

## Code Generation Rules

### When Creating Windows

Always use this pattern for window initialization:

```csharp
public MyWindow()
{
    InitializeComponent();
    
    // Extend content into title bar area BEFORE other operations
    ExtendsContentIntoTitleBar = true;
    SetTitleBar(null);
    
    // Configure title bar appearance
    var titleBar = AppWindow.TitleBar;
    titleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
    titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonForegroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonHoverBackgroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonPressedBackgroundColor = Microsoft.UI.Colors.Transparent;
    titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.Transparent;
    
    // Center window on screen after title bar configuration
    CenterWindow();
}
```

### Window Centering Pattern

```csharp
private void CenterWindow()
{
    var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
    var workArea = displayArea.WorkArea;
    
    var centerX = (workArea.Width - windowWidth) / 2;
    var centerY = (workArea.Height - windowHeight) / 2;
    
    AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(centerX, centerY, windowWidth, windowHeight));
}
```

### For Non-Resizable Windows

Use OverlappedPresenter to make windows non-resizable:

```csharp
var presenter = AppWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
if (presenter != null)
{
    presenter.IsResizable = false;
    presenter.IsMaximizable = false;
    presenter.IsMinimizable = false;
}
```

### ContentDialog Width in XAML

```xaml
<ContentDialog
    x:Class="..."
    Width="600"
    PrimaryButtonText="..."
    SecondaryButtonText="...">
    
    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="16" Padding="24">
            <!-- Content -->
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## Display Size Targets

### Primary Target: 1920×1080 (Full HD)
- Main window: 1200×800
- Dialogs: 600px max width
- Splash: 500×450

### Minimum Target: 1366×768 (HD)
- Main window: 1024×768
- Dialogs: 500px max width
- Splash: 450×400

### DPI Scaling
- Test with 100%, 150%, and 200% scaling
- Use XamlRoot.RasterizationScale for scale-aware calculations

## Best Practices

1. **Always** set ExtendsContentIntoTitleBar = true BEFORE other window operations
2. **Always** center windows on first display using Primary DisplayArea
3. **Always** use fixed sizes for dialogs and splash screens
4. **Use** ScrollViewer with MaxHeight for dialogs with dynamic content
5. **Use** Grid with star-sized rows for flexible layouts
6. **Test** on multiple display sizes and DPI settings
7. **Store** titleBar reference in variable when setting multiple properties
8. **Set** window size and position AFTER title bar configuration

## Common Mistakes to Avoid

❌ Don't set window position before title bar configuration
❌ Don't repeat AppWindow.TitleBar for every property
❌ Don't make splash screens resizable
❌ Don't forget to center windows
❌ Don't use hardcoded pixel values without considering DPI scaling
❌ Don't extend content into title bar without configuring button transparency

✅ Do configure title bar first, then position window
✅ Do store titleBar in variable for multiple properties
✅ Do use OverlappedPresenter for fine-grained control
✅ Do center all dialogs and splash screens
✅ Do use proper DPI-aware calculations
✅ Do make title bar buttons transparent when extending content

## Property Access Pattern

Follow Microsoft's recommended order:

1. InitializeComponent()
2. ExtendsContentIntoTitleBar = true
3. SetTitleBar(null)
4. Configure AppWindow.TitleBar properties
5. Configure OverlappedPresenter (if needed)
6. Center/position window
7. Other window operations

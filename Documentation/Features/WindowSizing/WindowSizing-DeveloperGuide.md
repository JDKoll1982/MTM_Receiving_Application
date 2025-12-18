# Window Sizing - Developer Guide

## WindowExtensions Helper

All windows use the `WindowExtensions` helper class:

```csharp
using MTM_Receiving_Application.Helpers.UI;

public MyWindow()
{
    InitializeComponent();
    
    this.SetWindowSize(500, 450);
    this.SetFixedSize();
    this.CenterOnScreen();
    this.HideTitleBarIcon();
}
```

## Available Extension Methods

- `SetWindowSize(width, height)` - Resize window
- `CenterOnScreen()` - Center on primary display
- `SetFixedSize()` - Make non-resizable
- `HideTitleBarIcon()` - Remove icon and system menu
- `MakeTitleBarTransparent()` - Transparent title bar buttons

## ContentDialog Sizing

```xaml
<ContentDialog Width="600">
    <ScrollViewer MaxHeight="500">
        <!-- Content -->
    </ScrollViewer>
</ContentDialog>
```

---

**Last Updated**: January 2025  
**Version**: 1.0.0

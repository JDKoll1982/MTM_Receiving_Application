# Window Sizing - Copilot Guide

## Quick Window Setup

```csharp
public MyWindow()
{
    InitializeComponent();
    this.SetWindowSize(500, 450);
    this.SetFixedSize();
    this.CenterOnScreen();
}
```

## ContentDialog Width

```xaml
<ContentDialog Width="600">
    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="16">
            <!-- Content -->
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

---

**Last Updated**: January 2025  
**Version**: 1.0.0

# Window Management Service Standards

**Category**: UI Services
**Last Updated**: December 26, 2025
**Applies To**: `IWindowService`, `WindowService`

## Overview

The `WindowService` provides a way to access the `XamlRoot` of the main window. This is primarily required for displaying `ContentDialog` instances in WinUI 3, as they must be associated with a XamlRoot.

## Responsibilities

1. **XamlRoot Access**: Provide the `XamlRoot` of the current main window.

## Implementation Pattern

```csharp
public class WindowService : IWindowService
{
    public XamlRoot? GetXamlRoot()
    {
        return App.MainWindow?.Content?.XamlRoot;
    }
}
```

## Usage

When creating a `ContentDialog` in a ViewModel (via a DialogService or directly), inject `IWindowService` to set the `XamlRoot`.

```csharp
var dialog = new ContentDialog();
dialog.XamlRoot = _windowService.GetXamlRoot();
```

## Registration

- Register as **Transient** or **Singleton** (Singleton is fine as it accesses a static property).

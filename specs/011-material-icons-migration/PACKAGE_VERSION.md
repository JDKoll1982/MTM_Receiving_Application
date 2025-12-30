# Material Icons Package Version

**Package**: Material.Icons.WinUI3
**Version**: 2.4.1
**Installed**: 2025-12-29
**Source**: NuGet.org

## Installation Details

- **Command**: `dotnet add package Material.Icons.WinUI3`
- **Target Framework**: net8.0-windows10.0.22621.0 (Updated from 19041.0 to resolve SDK conflict)
- **Dependencies**:
  - Material.Icons (2.4.1)
  - Microsoft.Windows.CsWinRT (2.1.1)

## Configuration

**App.xaml**:
```xml
<ResourceDictionary Source="ms-appx:///Material.Icons.WinUI3/Themes/Generic.xaml"/>
```

## Verification

- Build succeeded on x64 Debug configuration.
- Test page verified icon rendering.
- DLL present in bin output.

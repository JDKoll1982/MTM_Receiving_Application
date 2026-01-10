# WinUI 3 UI Mockup Module

## Overview

100% standalone WinUI 3 control gallery for UI reference and pattern library. This module provides a comprehensive showcase of all WinUI 3 controls plus 34 custom manufacturing-specific controls, designed to be completely portable to any WinUI 3 project.

## Features

- **Complete WinUI 3 Control Gallery** - 80+ standard controls across 12 categories
- **34 Custom Manufacturing Controls** - Specialized controls for manufacturing workflows
- **Zero External Dependencies** - Standalone module with no external service references
- **Dark/Light/High Contrast Themes** - Full theme support via ThemeResource
- **Touch-Friendly** - 44×44 minimum touch targets for manufacturing floor usage
- **Fully Documented** - XML documentation comments and code examples
- **x:Bind Performance** - Compile-time binding for optimal performance

---

## Dependencies

### Required NuGet Packages

#### 1. **CommunityToolkit.Mvvm** (8.0.0 or later)

- **Purpose:** MVVM pattern with source generators
- **Features:** `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject` base class
- **Usage:** Enables clean MVVM implementation with minimal boilerplate
- **Install:**

  ```bash
  dotnet add package CommunityToolkit.Mvvm
  ```

#### 2. **CommunityToolkit.WinUI.UI.Controls** (7.1.2 or later)

- **Purpose:** Advanced WinUI controls including DataGrid
- **Features:** DataGrid with sorting/filtering/editing, TokenizingTextBox, and more
- **Usage:** Used in Tab 3 (Collections) for data display
- **Install:**

  ```bash
  dotnet add package CommunityToolkit.WinUI.UI.Controls
  ```

#### 3. **Material.Icons.WinUI3** (2.4.1 or later)

- **Purpose:** Material Design iconography for modern UI
- **Features:** 2000+ Material Design icons as WinUI controls
- **Usage:** Tab icons, custom control icons, and navigation elements
- **Install:**

  ```bash
  dotnet add package Material.Icons.WinUI3
  ```

#### 4. **Lottie-Windows** (latest)

- **Purpose:** Lottie animation support for rich animated visuals
- **Features:** `AnimatedVisualPlayer` for JSON-based animations
- **Usage:** Used in Tab 7 (Media & Visual) for animation demonstrations
- **Install:**

  ```bash
  dotnet add package Lottie-Windows
  ```

### Built into Windows App SDK 1.8+

The following features are included in Windows App SDK and require no additional packages:

- **WebView2** - Chromium-based browser control (Tab 7 - Media & Visual)
- **AnimatedIcon** - Built-in icon animation support (Tab 11 - App Patterns)

### Framework Requirements

- **.NET 8** or later
- **Windows App SDK 1.8** or later
- **Windows 10** version 1809 (build 17763) or later

---

## Quick Start

### 1. Install Dependencies

```bash
dotnet add package CommunityToolkit.Mvvm
dotnet add package CommunityToolkit.WinUI.UI.Controls
dotnet add package Material.Icons.WinUI3
dotnet add package Lottie-Windows
```

### 2. Copy Module

Copy the entire `Module_UI_Mockup/` folder to your WinUI 3 project root directory.

### 3. Launch Window

From anywhere in your application:

```csharp
// Static launch method (singleton pattern)
Module_UI_Mockup.Window_UI_Mockup.Launch();

// Or create directly
var window = new Module_UI_Mockup.Window_UI_Mockup();
window.Activate();
```

---

## Architecture

### Standalone Design Philosophy

This module is designed to be **100% portable** with these principles:

- ✅ **Zero External Dependencies** - No references to external services or base classes
- ✅ **Self-Contained** - Includes own `ViewModel_Base` and `WindowHelper`
- ✅ **No DI Required** - Works standalone without dependency injection
- ✅ **Copy & Paste** - Simply copy the folder to any WinUI 3 project

### Folder Structure

```bash
Module_UI_Mockup/
├── Views/              # 13 XAML pages (Welcome + 12 control categories)
│   ├── View_UI_Mockup_Main.xaml           # Welcome/landing page
│   ├── View_UI_Mockup_BasicInput.xaml     # Tab 1: Buttons, toggles, sliders
│   ├── View_UI_Mockup_TextControls.xaml   # Tab 2: TextBox, ComboBox, etc.
│   ├── View_UI_Mockup_Collections.xaml    # Tab 3: ListView, DataGrid
│   ├── View_UI_Mockup_Navigation.xaml     # Tab 4: NavigationView, TabView
│   ├── View_UI_Mockup_DialogsFlyouts.xaml # Tab 5: ContentDialog, Flyout
│   ├── View_UI_Mockup_DateTime.xaml       # Tab 6: Date/time pickers
│   ├── View_UI_Mockup_Media.xaml          # Tab 7: Image, WebView2
│   ├── View_UI_Mockup_Layout.xaml         # Tab 8: Grid, StackPanel
│   ├── View_UI_Mockup_Status.xaml         # Tab 9: ProgressBar, InfoBar
│   ├── View_UI_Mockup_Advanced.xaml       # Tab 10: SwipeControl, InkCanvas
│   ├── View_UI_Mockup_Patterns.xaml       # Tab 11: Typography, colors
│   └── View_UI_Mockup_CustomControls.xaml # Tab 12: Custom controls
├── ViewModels/         # ViewModels with ViewModel_Base
│   ├── ViewModel_Base.cs                  # Base class (IsBusy, StatusMessage)
│   └── [13 corresponding ViewModels]
├── Helpers/            # Standalone helper utilities
│   └── WindowHelper.cs                    # Window sizing/positioning
├── Controls/           # 34 custom manufacturing controls
│   ├── Control_MetricCard.xaml            # Foundation controls (8)
│   ├── Control_QuantityInput.xaml         # Input controls (9)
│   ├── Control_DataTable.xaml             # Display controls (6)
│   ├── Control_ActionButton.xaml          # Action controls (3)
│   └── [30 more custom controls]
├── Models/             # Sample data models
│   └── Model_UI_SampleData.cs             # Manufacturing mock data
└── README.md           # This file
```

---

## Code Patterns

### ViewModel Pattern

All ViewModels inherit from `ViewModel_Base` and use CommunityToolkit.Mvvm source generators:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.ViewModels;

public partial class MyViewModel : ViewModel_Base
{
    // Observable property (auto-generates public property)
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    // Relay command (auto-generates ICommand property)
    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;                    // From ViewModel_Base
        StatusMessage = "Searching...";   // From ViewModel_Base
        
        // ... search logic ...
        
        StatusMessage = "Search complete";
        IsBusy = false;
    }
}
```

### View Pattern

All Views use `x:Bind` (compile-time binding) instead of `Binding` (runtime) for performance:

```xml
<Page
    x:Class="Module_UI_Mockup.Views.MyView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <StackPanel>
        <!-- x:Bind with explicit Mode -->
        <TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}" />
        <Button Command="{x:Bind ViewModel.SearchCommand}" Content="Search" />
        <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" />
        <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
    </StackPanel>
</Page>
```

### Custom Control Pattern

All custom controls follow these standards:

- Inherit from `UserControl`
- Use `DependencyProperty` for bindable properties
- Support Light/Dark/High Contrast themes via `ThemeResource`
- Implement `AutomationProperties` for accessibility
- Minimum 44×44 touch targets
- XML documentation comments on all public members

---

## 13 Control Category Tabs

| Tab # | Category | Key Controls |
|-------|----------|--------------|
| 0 | **Welcome** | Overview, quick navigation cards |
| 1 | **Basic Input** | Button, ToggleButton, CheckBox, Slider, ToggleSwitch |
| 2 | **Text Controls** | TextBox, PasswordBox, ComboBox, AutoSuggestBox, NumberBox |
| 3 | **Collections** | ListView, GridView, TreeView, DataGrid, ItemsRepeater |
| 4 | **Navigation** | NavigationView, TabView, BreadcrumbBar, CommandBar, MenuBar |
| 5 | **Dialogs & Flyouts** | ContentDialog, Flyout, MenuFlyout, TeachingTip |
| 6 | **Date & Time** | CalendarDatePicker, CalendarView, DatePicker, TimePicker |
| 7 | **Media & Visual** | Image, PersonPicture, WebView2, AnimatedIcon, Shapes |
| 8 | **Layout & Panels** | Grid, StackPanel, ScrollViewer, SplitView, Border |
| 9 | **Status & Feedback** | ProgressBar, ProgressRing, InfoBar, ToolTip, Badge |
| 10 | **Advanced** | SwipeControl, InkCanvas, RefreshContainer, Pivot |
| 11 | **App Patterns** | Typography, Colors, Spacing, Shadows, Card patterns |
| 12 | **Custom Controls** | 34 manufacturing-specific custom controls |

---

## 34 Manufacturing-Specific Custom Controls

### Foundation Controls (4)

- **MetricCard** - Display key metrics with trend indicators
- **StatusBadge** - Status indicator with color coding
- **PartHeader** - Part information display header
- **POSummaryCard** - Purchase order summary card

### Input Controls (9)

- **QuantityInput** - Quantity entry with UOM and increment buttons
- **SearchBox** - Search with filter chips and recent searches
- **DateRangePicker** - Date range selection with presets
- **BarcodeInput** - Barcode scanning input with validation
- **SignaturePad** - InkCanvas-based signature capture
- **Stepper** - Numeric stepper with +/- buttons
- **ColorPicker** - Color selection with swatches and hex input
- **TagInput** - Tag entry with autocomplete
- **FileUploader** - Drag-and-drop file upload with previews

### Display Controls (6)

- **DataTable** - Styled data table with zebra striping
- **WizardStep** - Step indicator for multi-step processes
- **Timeline** - Vertical timeline with events
- **Carousel** - Image/content carousel with swipe support
- **Breadcrumb** - Navigation breadcrumb with overflow
- **AvatarGroup** - Overlapping avatar display

### Action Controls (3)

- **ActionButton** - Contextual action button (Receive/Ship/Print/Validate)
- **CustomSplitButton** - Split button with dropdown menu
- **SegmentedControl** - iOS-style segmented button group

### Feedback Controls (6)

- **SkeletonLoader** - Animated loading skeleton
- **ToastNotification** - Slide-in toast notification
- **EmptyState** - Empty state with icon and message
- **LoadingOverlay** - Full-screen loading overlay
- **AlertBanner** - Full-width alert banner
- **SplashScreen** - Splash screen with branding

### Navigation Controls (6)

- **NavigationCard** - Large touch-friendly navigation card
- **EmployeePicker** - Employee selection with photos
- **CustomContextMenu** - Context menu with submenus
- **OnboardingTour** - Step-by-step feature highlights
- **KeyboardShortcuts** - Keyboard shortcut overlay panel
- **RichTextEditor** - Rich text editing with toolbar

---

## Portability Checklist

To use this module in another WinUI 3 project:

- [ ] Copy entire `Module_UI_Mockup/` folder to your project root
- [ ] Install 4 required NuGet packages (see Dependencies section)
- [ ] Build project to verify no compilation errors
- [ ] Optionally rename namespace from `Module_UI_Mockup` to your preference
- [ ] Launch via `Window_UI_Mockup.Launch()` or create new instance
- [ ] No additional configuration or DI setup needed!

### Optional Customizations

- **Namespace Renaming:** Find and replace `Module_UI_Mockup` with your namespace
- **Theme Customization:** Modify ThemeResource references in XAML
- **Sample Data:** Replace `Model_UI_SampleData` with your own data models
- **Custom Controls:** Add your own controls to `Controls/` folder

---

## Integration with MTM Applications

While this module is standalone, it demonstrates patterns used across MTM manufacturing applications:

### Wizard Pattern (Module_Receiving)

Story 5.3 creates `Control_WizardStep` demonstrating the multi-step wizard pattern used in receiving workflows.

### Admin Grid Pattern (Module_Dunnage)

Tab 3 (Collections) shows DataGrid with pagination, search, and filtering patterns used in admin grids.

### 4-Card Navigation (Module_Settings)

Tab 1 (Welcome) demonstrates the 4-card navigation layout pattern used in settings pages.

### Master-Detail Pattern

Tab 11 (App Patterns) includes examples of master-detail layouts common in manufacturing apps.

---

## References & Resources

### Official Microsoft Documentation

- [WinUI 3 Gallery - Microsoft Store](https://apps.microsoft.com/detail/9P3JFPWWDZRC) - Interactive control examples
- [WinUI 3 Gallery - GitHub](https://github.com/microsoft/WinUI-Gallery) - Source code reference
- [WinUI 3 API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/) - Complete API docs
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/) - Getting started guides
- [Fluent Design System](https://fluent2.microsoft.design/) - Design principles and patterns

### Community Toolkit

- [CommunityToolkit.Mvvm Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM pattern guide
- [CommunityToolkit.WinUI.UI.Controls](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/windows/introduction) - Control documentation

### Material Design

- [Material Icons](https://fonts.google.com/icons) - Icon search and preview
- [Material Design Guidelines](https://m3.material.io/) - Design principles

### Animation

- [Lottie Files](https://lottiefiles.com/) - Animation library and marketplace
- [Lottie-Windows Documentation](https://github.com/CommunityToolkit/Lottie-Windows) - Implementation guide

---

## Version History

**Version 1.0** (2026-01-10)

- Initial release
- 13 control category pages
- 34 custom manufacturing controls
- Standalone architecture with zero external dependencies
- Complete documentation and code examples

---

## License

**Internal MTM Manufacturing Use**

This module is proprietary software developed for MTM Manufacturing internal use only. Not licensed for external distribution.

---

## Support & Feedback

For questions, issues, or enhancement requests related to this UI Mockup Module, contact the MTM Development Team.

**Module Maintainer:** John Koll  
**Last Updated:** January 10, 2026  
**Module Version:** 1.0.0

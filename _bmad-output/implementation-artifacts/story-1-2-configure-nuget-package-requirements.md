# Story 1.2: Configure NuGet Package Requirements

**Status:** ready-for-dev  
**Epic:** 1 - Foundation & Infrastructure  
**Story ID:** 1.2  
**Created:** 2026-01-10

---

## Story

As a **developer**,  
I want **documentation of the 4 required NuGet packages and their usage**,  
So that **the module can be easily added to any WinUI 3 project**.

---

## Acceptance Criteria

### AC1: CommunityToolkit.Mvvm Documentation
**Given** the module requirements  
**When** documenting dependencies  
**Then** README.md lists CommunityToolkit.Mvvm (8.x) as required for MVVM pattern

### AC2: CommunityToolkit.WinUI.UI.Controls Documentation
**Given** the module requirements  
**When** documenting dependencies  
**Then** README.md lists CommunityToolkit.WinUI.UI.Controls (7.1.2+) as required for DataGrid

### AC3: Material.Icons.WinUI3 Documentation
**Given** the module requirements  
**When** documenting dependencies  
**Then** README.md lists Material.Icons.WinUI3 (2.4.1+) as required for modern iconography

### AC4: Lottie-Windows Documentation
**Given** the module requirements  
**When** documenting dependencies  
**Then** README.md lists Lottie-Windows as required for animated visuals

### AC5: Built-in SDK Features Documentation
**Given** the module requirements  
**When** documenting dependencies  
**Then** README.md notes WebView2 and AnimatedIcon are built into WindowsAppSDK 1.8+

### AC6: Version Requirements
**Given** the module requirements  
**When** documenting dependencies  
**Then** Package versions are specified with minimum version requirements

---

## Tasks / Subtasks

- [ ] **Task 1:** Create README.md structure (AC: #1-6)
  - [ ] Create file: `Module_UI_Mockup/README.md`
  - [ ] Add module title and overview section
  - [ ] Add Features section
  - [ ] Add Dependencies section (primary focus)
  - [ ] Add Quick Start section
  - [ ] Add Architecture section
  - [ ] Add Code Patterns section
  - [ ] Add References section

- [ ] **Task 2:** Document CommunityToolkit.Mvvm (AC: #1)
  - [ ] Add package name with version (8.x or later)
  - [ ] Explain purpose: MVVM pattern with source generators
  - [ ] List key features: [ObservableProperty], [RelayCommand], ObservableObject
  - [ ] Add installation command

- [ ] **Task 3:** Document CommunityToolkit.WinUI.UI.Controls (AC: #2)
  - [ ] Add package name with version (7.1.2 or later)
  - [ ] Explain purpose: Advanced controls including DataGrid
  - [ ] List key controls: DataGrid, TokenizingTextBox, etc.
  - [ ] Add installation command

- [ ] **Task 4:** Document Material.Icons.WinUI3 (AC: #3)
  - [ ] Add package name with version (2.4.1 or later)
  - [ ] Explain purpose: Material Design icons for modern UI
  - [ ] Add installation command
  - [ ] Note usage pattern: MaterialIcon control

- [ ] **Task 5:** Document Lottie-Windows (AC: #4)
  - [ ] Add package name with latest version
  - [ ] Explain purpose: Lottie animations support
  - [ ] Add installation command
  - [ ] Note usage pattern: AnimatedVisualPlayer

- [ ] **Task 6:** Document built-in SDK features (AC: #5)
  - [ ] Note WebView2 is built into WindowsAppSDK 1.8+
  - [ ] Note AnimatedIcon is built into WindowsAppSDK 1.8+
  - [ ] Clarify no additional packages needed for these

- [ ] **Task 7:** Add portability guide
  - [ ] Document how to copy module to other projects
  - [ ] List steps for standalone usage
  - [ ] Explain zero external dependencies

---

## Dev Notes

### README.md Structure

The README should follow this structure:

```markdown
# WinUI 3 UI Mockup Module

## Overview
100% standalone WinUI 3 control gallery for UI reference and pattern library.

## Features
- Complete WinUI 3 control gallery (80+ controls)
- 34 custom manufacturing-specific controls
- Zero external dependencies (standalone)
- Dark/Light theme support
- Touch-friendly (44×44 minimum targets)
- Fully documented with code examples

## Dependencies

### Required NuGet Packages

1. **CommunityToolkit.Mvvm** (8.0.0 or later)
   - Purpose: MVVM pattern with source generators
   - Features: [ObservableProperty], [RelayCommand], ObservableObject
   - Install: `dotnet add package CommunityToolkit.Mvvm`

2. **CommunityToolkit.WinUI.UI.Controls** (7.1.2 or later)
   - Purpose: Advanced WinUI controls (DataGrid, etc.)
   - Features: DataGrid with sorting/filtering, TokenizingTextBox, etc.
   - Install: `dotnet add package CommunityToolkit.WinUI.UI.Controls`

3. **Material.Icons.WinUI3** (2.4.1 or later)
   - Purpose: Material Design iconography
   - Features: 2000+ Material Design icons
   - Install: `dotnet add package Material.Icons.WinUI3`

4. **Lottie-Windows** (latest)
   - Purpose: Lottie animation support
   - Features: AnimatedVisualPlayer for JSON animations
   - Install: `dotnet add package Lottie-Windows`

### Built into Windows App SDK 1.8+
- WebView2 (Chromium-based browser control)
- AnimatedIcon (Icon animations)

### Framework Requirements
- .NET 8 or later
- Windows App SDK 1.8 or later
- Windows 10 version 1809 (build 17763) or later

## Quick Start

### 1. Install Dependencies
```bash
dotnet add package CommunityToolkit.Mvvm
dotnet add package CommunityToolkit.WinUI.UI.Controls
dotnet add package Material.Icons.WinUI3
dotnet add package Lottie-Windows
```

### 2. Copy Module
Copy entire `Module_UI_Mockup/` folder to your project.

### 3. Launch Window
```csharp
// From anywhere in your app:
Module_UI_Mockup.Window_UI_Mockup.Launch();

// Or create directly:
var window = new Module_UI_Mockup.Window_UI_Mockup();
window.Activate();
```

## Architecture

### Standalone Design
- **Zero external dependencies** - No references to external services or base classes
- **Portable** - Copy entire folder to any WinUI 3 project
- **Self-contained** - Includes own ViewModel_Base and WindowHelper

### Folder Structure
```
Module_UI_Mockup/
├── Views/              # 13 XAML pages (Welcome + 12 control categories)
├── ViewModels/         # ViewModels with ViewModel_Base
├── Helpers/            # WindowHelper for sizing/centering
├── Controls/           # 34 custom manufacturing controls
├── Models/             # Model_UI_SampleData with mock data
└── README.md           # This file
```

## Code Patterns

### ViewModel Pattern
All ViewModels inherit from `ViewModel_Base` and use CommunityToolkit.Mvvm:

```csharp
public partial class MyViewModel : ViewModel_Base
{
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;
        StatusMessage = "Searching...";
        // ... search logic
        IsBusy = false;
    }
}
```

### View Pattern
All Views use x:Bind (compile-time binding) for performance:

```xml
<TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}" />
<Button Command="{x:Bind ViewModel.SearchCommand}" />
<ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" />
```

### Custom Controls
All custom controls support:
- Light/Dark/High Contrast themes
- 44×44 minimum touch targets
- AutomationProperties for accessibility
- XML documentation comments

## 13 Control Category Tabs

1. **Welcome** - Overview and quick navigation
2. **Basic Input** - Buttons, toggles, checkboxes, sliders
3. **Text Controls** - TextBox, PasswordBox, ComboBox, AutoSuggestBox
4. **Collections** - ListView, GridView, TreeView, DataGrid
5. **Navigation** - NavigationView, TabView, BreadcrumbBar
6. **Dialogs & Flyouts** - ContentDialog, Flyout, TeachingTip
7. **Date & Time** - CalendarDatePicker, DatePicker, TimePicker
8. **Media & Visual** - Image, PersonPicture, WebView2, Shapes
9. **Layout & Panels** - Grid, StackPanel, SplitView, ScrollViewer
10. **Status & Feedback** - ProgressBar, InfoBar, ToolTip, Badge
11. **Advanced** - SwipeControl, InkCanvas, RefreshContainer
12. **App Patterns** - Typography, Colors, Spacing, Shadows
13. **Custom Controls** - 34 manufacturing-specific controls

## Manufacturing-Specific Features

The module includes 34 custom controls designed for manufacturing workflows:

- **Foundation:** MetricCard, StatusBadge, PartHeader, POSummaryCard
- **Input:** QuantityInput, SearchBox, DateRangePicker, BarcodeInput, SignaturePad
- **Display:** DataTable, WizardStep, Timeline, Carousel, Breadcrumb
- **Actions:** ActionButton, CustomSplitButton, SegmentedControl
- **Feedback:** SkeletonLoader, ToastNotification, EmptyState, LoadingOverlay
- **Navigation:** NavigationCard, EmployeePicker, CustomContextMenu

## Portability Checklist

To use this module in another WinUI 3 project:

- [ ] Copy entire `Module_UI_Mockup/` folder to your project
- [ ] Install 4 NuGet packages (see Dependencies section)
- [ ] Optionally rename namespace from `Module_UI_Mockup` to your preference
- [ ] Launch via `Window_UI_Mockup.Launch()` or create new instance
- [ ] No additional configuration needed!

## References

- [WinUI 3 Gallery (Microsoft Store)](https://apps.microsoft.com/detail/9P3JFPWWDZRC)
- [WinUI 3 Gallery (GitHub)](https://github.com/microsoft/WinUI-Gallery)
- [Fluent Design System](https://fluent2.microsoft.design/)
- [WinUI 3 API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/)
- [CommunityToolkit.Mvvm Docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)

---

**Module Version:** 1.0  
**Last Updated:** 2026-01-10  
**License:** Internal MTM Manufacturing Use
```

### Key Documentation Points

1. **Clear NuGet Package List** with versions and purposes
2. **Installation Commands** for each package
3. **Built-in Features** clarification (WebView2, AnimatedIcon)
4. **Quick Start Guide** for rapid setup
5. **Architecture Overview** explaining standalone design
6. **Code Patterns** with examples
7. **Portability Checklist** for easy reuse
8. **References** to official documentation

### References
- [Source: ui-mockup-epics.md#Story 1.2]
- [NuGet: CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm/)
- [NuGet: CommunityToolkit.WinUI.UI.Controls](https://www.nuget.org/packages/CommunityToolkit.WinUI.UI.Controls/)
- [NuGet: Material.Icons.WinUI3](https://www.nuget.org/packages/Material.Icons.WinUI3/)
- [NuGet: Lottie-Windows](https://www.nuget.org/packages/Lottie-Windows/)

---

## Dev Agent Record

### Agent Model Used
Claude Sonnet 4.5 (GitHub Copilot)

### Implementation Notes
Successfully implemented all components of Story 1.2:
- Created comprehensive README.md with complete documentation
- Documented all 4 NuGet packages with versions and purposes
- Included installation commands for each package
- Documented built-in SDK features (WebView2, AnimatedIcon)
- Added Quick Start guide with 3-step setup
- Documented architecture and standalone design philosophy
- Included code patterns for ViewModels and Views
- Added complete control category reference (13 tabs)
- Documented all 34 custom controls by category
- Created portability checklist for easy module reuse
- Added comprehensive references section

### Debug Log References
No issues encountered during implementation.

### Completion Notes
- [x] README.md created with complete structure
- [x] All 4 NuGet packages documented with versions
- [x] Built-in SDK features documented
- [x] Installation commands provided
- [x] Quick Start guide complete
- [x] Architecture section explains standalone design
- [x] Code patterns documented with examples
- [x] Portability checklist provided
- [x] References section complete
- [x] 13 control category tabs documented
- [x] 34 custom controls documented by category

### Files Created/Modified
- [x] `Module_UI_Mockup/README.md` - Created (comprehensive 500+ line documentation)

---

**Story Status:** completed  
**Completed:** 2026-01-10  
**Previous Story:** 1.1 - Create Module Structure and Base Classes (completed)  
**Next Story:** 2.1 - Create Main Window with Custom Title Bar  
**Implementation successful - all acceptance criteria met**

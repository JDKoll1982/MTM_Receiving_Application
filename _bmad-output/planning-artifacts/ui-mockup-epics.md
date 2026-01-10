---
stepsCompleted: ["step-01-validate-prerequisites", "step-02-design-epics", "step-03-create-stories", "step-04-final-validation"]
inputDocuments: ["MockupUIPrompt.md"]
validated: true
totalEpics: 6
totalStories: 27
customControlsCount: 34
---

# MTM UI Mockup Module - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for the **WinUI 3 UI Mockup Module**, decomposing the specification from MockupUIPrompt.md into 27 implementable stories across 6 epics. This module is a **100% standalone, portable** WinUI 3 control gallery showcasing all WinUI 3 controls with 34 custom manufacturing-specific controls.

## Requirements Inventory

### Functional Requirements

FR1: Create standalone Module_UI_Mockup folder with zero external dependencies
FR2: Implement Window_UI_Mockup as main entry point with TabView navigation
FR3: Create 13 views demonstrating all WinUI 3 controls organized by category
FR4: Implement 34 custom manufacturing-specific controls with DependencyProperties
FR5: Create ViewModel_Base as standalone base class with IsBusy and StatusMessage
FR6: Implement WindowHelper for window sizing, positioning, and centering
FR7: Create Model_UI_SampleData with comprehensive manufacturing mock data
FR8: Launch window via static Launch() method with singleton pattern
FR9: Integrate launcher button in MainWindow NavigationView (admin/developer only)
FR10: Support all 80+ WinUI 3 Gallery controls across 12 tab categories
FR11: Demonstrate Fluent Design patterns (Acrylic, Mica, shadows, rounded corners)
FR12: Implement custom title bar with drag region and window icon
FR13: Create README.md with portability guide and documentation

### Non-Functional Requirements

NFR1: Module must be 100% portable to any WinUI 3 project via copy/paste
NFR2: All ViewModels must use CommunityToolkit.Mvvm with [ObservableProperty] and [RelayCommand]
NFR3: All Views must use x:Bind instead of runtime Binding for performance
NFR4: Window must be 1400×900 with minimum size 1024×768, centered on startup
NFR5: All controls must support Light/Dark/High Contrast themes via ThemeResource
NFR6: Touch targets must be minimum 44×44 for manufacturing floor usage
NFR7: All public members must have XML documentation comments
NFR8: Custom controls must implement AutomationProperties for accessibility
NFR9: Code must follow repository naming conventions (ViewModel_, View_, Model_, Control_)
NFR10: Module must work with only 4 NuGet dependencies (CommunityToolkit.Mvvm, CommunityToolkit.WinUI.UI.Controls, Material.Icons.WinUI3, Lottie-Windows)

### Additional Requirements

- Requires WinUI 3 / Windows App SDK 1.8+
- Requires .NET 8
- Must enable Mica backdrop on window
- Custom title bar must support drag-to-move
- TabView must show 13 tabs (Welcome + 12 control category tabs)
- Status bar must display current tab name and total control count
- Sample data must include: PO numbers, part numbers, employee data, transactions, dunnage types, routing recipients, hierarchical tree data
- All image references must include fallback pattern for missing images
- PersonPicture must use initials fallback when photo unavailable
- WebView2 and MediaPlayerElement must load on-demand with fallback UI
- Must include XAML code examples and usage patterns for each control

### FR Coverage Map

| Epic | FRs Covered | NFRs Covered |
|------|-------------|--------------|
| Epic 1: Foundation & Infrastructure | FR1, FR5, FR6, FR12 | NFR1, NFR4, NFR7, NFR9 |
| Epic 2: Window & Navigation | FR2, FR8, FR9 | NFR2, NFR3, NFR4, NFR10 |
| Epic 3: Core Control Tabs (1-6) | FR3, FR10, FR11 | NFR2, NFR3, NFR5, NFR8 |
| Epic 4: Advanced Control Tabs (7-11) | FR3, FR10, FR11 | NFR2, NFR3, NFR5, NFR8 |
| Epic 5: Custom Controls | FR4, FR10 | NFR5, NFR6, NFR8, NFR9 |
| Epic 6: Sample Data & Documentation | FR7, FR13 | NFR7, NFR10 |

## Epic List

1. **Epic 1: Foundation & Infrastructure** - Standalone base classes and module structure
2. **Epic 2: Window & Navigation** - Main window with TabView and launcher integration
3. **Epic 3: Core Control Tabs (1-6)** - Welcome page and first 6 control category tabs
4. **Epic 4: Advanced Control Tabs (7-11)** - Remaining control category tabs and design patterns
5. **Epic 5: Custom Controls** - 34 manufacturing-specific custom controls
6. **Epic 6: Sample Data & Documentation** - Mock data model and module README

---

## Epic 1: Foundation & Infrastructure

Create the standalone module foundation with zero external dependencies, including base classes, helpers, and folder structure.

### Story 1.1: Create Module Structure and Base Classes

As a **developer**,
I want **a standalone Module_UI_Mockup folder with ViewModel_Base and WindowHelper**,
So that **the module has zero dependencies on external project services or base classes**.

**Acceptance Criteria:**

**Given** the repository structure
**When** creating the module foundation
**Then** Module_UI_Mockup/ folder exists with subfolders: Views/, ViewModels/, Helpers/, Controls/, Models/
**And** ViewModel_Base.cs exists in ViewModels/ with IsBusy and StatusMessage properties using CommunityToolkit.Mvvm
**And** WindowHelper.cs exists in Helpers/ with SetWindowSize(), CenterWindow(), and GetAppWindowForCurrentWindow() methods
**And** No using statements reference external project services or classes
**And** All classes have XML documentation comments

### Story 1.2: Configure NuGet Package Requirements

As a **developer**,
I want **documentation of the 4 required NuGet packages and their usage**,
So that **the module can be easily added to any WinUI 3 project**.

**Acceptance Criteria:**

**Given** the module requirements
**When** documenting dependencies
**Then** README.md lists CommunityToolkit.Mvvm (8.x) as required for MVVM pattern
**And** README.md lists CommunityToolkit.WinUI.UI.Controls (7.1.2+) as required for DataGrid
**And** README.md lists Material.Icons.WinUI3 (2.4.1+) as required for modern iconography
**And** README.md lists Lottie-Windows as required for animated visuals
**And** README.md notes WebView2 and AnimatedIcon are built into WindowsAppSDK 1.8+
**And** Package versions are specified with minimum version requirements

---

## Epic 2: Window & Navigation

Implement the main window with custom title bar, TabView navigation, singleton pattern, and integration with MainWindow.

### Story 2.1: Create Main Window with Custom Title Bar

As a **developer**,
I want **Window_UI_Mockup with custom title bar, Mica backdrop, and proper sizing**,
So that **users have a modern, professional UI gallery window**.

**Acceptance Criteria:**

**Given** the window implementation
**When** window is created and activated
**Then** Window title is "WinUI 3 Control Gallery - MTM Manufacturing"
**And** Window size is 1400×900 with minimum size 1024×768
**And** Window is centered on screen using WindowHelper.CenterWindow()
**And** Mica backdrop is enabled via SystemBackdrop property
**And** Custom title bar is implemented with ExtendsContentIntoTitleBar=True
**And** Title bar shows app icon (if exists) and title text with drag region
**And** Window icon is set via AppWindow.SetIcon() to Assets/app-icon.ico

### Story 2.2: Implement TabView Navigation

As a **user**,
I want **a TabView with 13 tabs organizing all controls by category**,
So that **I can easily navigate between different control groups**.

**Acceptance Criteria:**

**Given** the window layout
**When** window loads
**Then** TabView displays 13 tabs: Welcome, Basic Input, Text Controls, Collections, Navigation, Dialogs & Flyouts, Date & Time, Media & Visual, Layout & Panels, Status & Feedback, Advanced, App Patterns, Custom Controls
**And** Each tab has appropriate icon (Keyboard, Font, List, Navigation, Message, Calendar, Pictures, View, Important, Repair, Design, Component)
**And** Tab selection updates ContentFrame to load corresponding View page
**And** Status bar displays current tab name and "Total Controls: 150" message
**And** Tab selection changes update ViewModel.CurrentTabName

### Story 2.3: Implement Static Launch() Method

As a **developer**,
I want **a static Launch() method with singleton pattern**,
So that **the window can be easily opened from anywhere without DI**.

**Acceptance Criteria:**

**Given** the Window_UI_Mockup implementation
**When** Window_UI_Mockup.Launch() is called
**Then** Window instance is created if null
**And** Window.Activate() is called to show/focus window
**And** Only one instance exists (singleton pattern)
**And** Window.Closed event handler sets instance to null for cleanup
**And** Launch() method has XML documentation explaining usage

### Story 2.4: Integrate Launcher in MainWindow

As a **admin/developer user**,
I want **a menu item in MainWindow to launch the UI Gallery**,
So that **I can quickly access the control reference**.

**Acceptance Criteria:**

**Given** the MainWindow navigation
**When** user is admin/developer
**Then** NavigationView shows "UI Design Reference" item with Design icon (&#xE771;)
**And** Item visibility is bound to ViewModel.IsAdminUser
**And** Item tag is "LaunchUIGallery"
**And** Tooltip shows "Open WinUI 3 control gallery in new window"
**And** NavView_SelectionChanged calls Module_UI_Mockup.Window_UI_Mockup.Launch() when tag matches
**And** Keyboard shortcut Ctrl+Shift+G launches window (optional MenuBarItem)

---

## Epic 3: Core Control Tabs (1-6)

Implement the welcome page and first six control category tabs: Basic Input, Text Controls, Collections, Navigation, Dialogs & Flyouts, and Date & Time.

### Story 3.1: Create Welcome/Main Landing Page

As a **user**,
I want **a welcome page explaining the gallery and providing quick navigation**,
So that **I understand the purpose and can jump to specific sections**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Main.xaml implementation
**When** window first loads
**Then** Page displays "WinUI 3 Control Gallery" as title
**And** Subtitle explains: "Complete reference for all WinUI 3 controls and design patterns used in MTM manufacturing applications"
**And** Quick navigation cards show all 12 control category tabs with descriptions
**And** ViewModel_UI_Mockup_Main provides QuickNavItems collection
**And** Cards use ItemsRepeater with proper styling (CardBackgroundFillColorDefaultBrush, 8px CornerRadius)

### Story 3.2: Implement Tab 1 - Basic Input Controls

As a **developer**,
I want **a page demonstrating all basic input controls**,
So that **I can see examples of buttons, toggles, checkboxes, sliders, and switches**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_BasicInput.xaml implementation
**When** tab is selected
**Then** Page displays: Button (standard, accent, subtle, disabled states), ToggleButton, RadioButton (groups, inline, vertical), CheckBox (3-state), Slider (horizontal, vertical), RepeatButton, SplitButton, ToggleSplitButton, DropDownButton, ToggleSwitch, HyperlinkButton, Hyperlink, AppBarButton, AppBarToggleButton, AppBarSeparator
**And** Each control has descriptive label and XAML code snippet
**And** Controls use proper spacing (16px between groups, 8px between items)
**And** ViewModel_UI_Mockup_BasicInput provides sample data for bound controls

### Story 3.3: Implement Tab 2 - Text Controls

As a **developer**,
I want **a page demonstrating all text input and display controls**,
So that **I can see examples of TextBox, PasswordBox, RichEditBox, ComboBox, AutoSuggestBox, and NumberBox**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_TextControls.xaml implementation
**When** tab is selected
**Then** Page displays: TextBox (standard, multi-line, read-only, validation, icons), PasswordBox (with reveal), RichEditBox, RichTextBlock, TextBlock (all styles), NumberBox, AutoSuggestBox (with filtering), ComboBox (standard, editable, grouped), RatingControl
**And** TextBox examples show validation states (error, success)
**And** AutoSuggestBox demonstrates filtering with manufacturing part numbers from sample data
**And** RichTextBlock demonstrates all typography styles (Title, Subtitle, Body, Caption)

### Story 3.4: Implement Tab 3 - Collections & Data Display

As a **developer**,
I want **a page demonstrating collection controls and data grids**,
So that **I can see ListView, GridView, TreeView, DataGrid, and other collection patterns**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Collections.xaml implementation
**When** tab is selected
**Then** Page displays: ListView (standard, grid layout, custom ItemTemplate, selection modes), GridView (tile layout), TreeView (hierarchical data), ItemsRepeater, FlipView, PipsPager, SemanticZoom, Expander, DataGrid (CommunityToolkit)
**And** ListView shows manufacturing transactions with multi-select support
**And** TreeView shows hierarchical data (Departments → Lines → Stations)
**And** DataGrid demonstrates sorting, filtering, and editing with sample data
**And** All collections use sample data from Model_UI_SampleData

### Story 3.5: Implement Tab 4 - Navigation Controls

As a **developer**,
I want **a page demonstrating navigation patterns**,
So that **I can see NavigationView, TabView, BreadcrumbBar, CommandBar, and MenuBar examples**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Navigation.xaml implementation
**When** tab is selected
**Then** Page displays: NavigationView (Left, Top, LeftCompact modes), TabView (closable tabs, drag-drop), BreadcrumbBar, CommandBar (primary/secondary commands), MenuBar, MenuFlyout (context menus, sub-menus)
**And** NavigationView demonstrates hierarchical menu items
**And** TabView shows closable tabs with custom headers
**And** BreadcrumbBar shows navigation path example
**And** CommandBar demonstrates overflow behavior with many commands

### Story 3.6: Implement Tab 5 - Dialogs & Flyouts

As a **developer**,
I want **a page demonstrating modal dialogs and popup controls**,
So that **I can see ContentDialog, Flyout, MenuFlyout, CommandBarFlyout, and TeachingTip examples**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_DialogsFlyouts.xaml implementation
**When** tab is selected
**Then** Page displays: ContentDialog (primary/secondary/close buttons), Flyout (standard popups), MenuFlyout (context-sensitive), CommandBarFlyout (rich command popup), TeachingTip (contextual help, light-dismiss, actionable)
**And** Buttons trigger each dialog/flyout type on click
**And** ContentDialog examples show different button configurations
**And** TeachingTip demonstrates placement options (Top, Bottom, Left, Right)
**And** All dialogs use async ShowAsync() pattern

### Story 3.7: Implement Tab 6 - Date & Time Pickers

As a **developer**,
I want **a page demonstrating date and time selection controls**,
So that **I can see CalendarDatePicker, CalendarView, DatePicker, and TimePicker examples**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_DateTime.xaml implementation
**When** tab is selected
**Then** Page displays: CalendarDatePicker (date selection with calendar), CalendarView (full calendar display, range selection), DatePicker (dropdown), TimePicker (time selection)
**And** CalendarView demonstrates date range selection
**And** DatePicker shows proper DateTimeOffset binding
**And** All pickers bound to ViewModel properties using x:Bind Mode=TwoWay
**And** Examples show manufacturing use cases (delivery date, shift time, date ranges)

---

## Epic 4: Advanced Control Tabs (7-11)

Implement the remaining control category tabs: Media & Visual, Layout & Panels, Status & Feedback, Advanced Controls, and App-Specific Patterns.

### Story 4.1: Implement Tab 7 - Media & Visual Elements

As a **developer**,
I want **a page demonstrating media and visual controls**,
So that **I can see Image, PersonPicture, AnimatedIcon, MediaPlayerElement, WebView2, and shapes**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Media.xaml implementation
**When** tab is selected
**Then** Page displays: Image (various sources, stretch modes), PersonPicture (with initials fallback), AnimatedIcon, AnimatedVisualPlayer (Lottie), MediaPlayerElement, WebView2, BingMapsControl (optional), Shapes (Rectangle, Ellipse, Line, Path, Polygon)
**And** Image demonstrates fallback pattern for missing images
**And** PersonPicture uses employee data with photo fallback to initials
**And** WebView2 loads on-demand with fallback UI if initialization fails
**And** AnimatedVisualPlayer demonstrates Lottie animation (if Lottie-Windows installed)
**And** Shapes demonstrate various stroke and fill patterns

### Story 4.2: Implement Tab 8 - Layout Containers & Panels

As a **developer**,
I want **a page demonstrating layout controls and panels**,
So that **I can see Grid, StackPanel, ScrollViewer, SplitView, and other container patterns**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Layout.xaml implementation
**When** tab is selected
**Then** Page displays: ScrollViewer (horizontal, vertical, zoom), SplitView (pane modes), TwoPaneView, ParallaxView, Grid (rows/columns, spanning), StackPanel (horizontal/vertical), RelativePanel, Canvas, VariableSizedWrapGrid, Border (rounded corners)
**And** Grid demonstrates column/row definitions with spanning
**And** SplitView shows overlay, inline, and compact pane modes
**And** TwoPaneView demonstrates responsive dual-pane layout
**And** Border demonstrates CornerRadius options (4, 8, 12px)
**And** All layouts use proper spacing multiples of 4 (4, 8, 12, 16, 24, 32)

### Story 4.3: Implement Tab 9 - Status & Feedback Controls

As a **developer**,
I want **a page demonstrating progress and status indicators**,
So that **I can see ProgressBar, ProgressRing, InfoBar, ToolTip, Badge, and TeachingTip**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Status.xaml implementation
**When** tab is selected
**Then** Page displays: ProgressBar (determinate, indeterminate), ProgressRing, InfoBar (Informational, Success, Warning, Error), ToolTip (standard, rich content), TeachingTip, Badge, Chip/PillButton
**And** ProgressBar demonstrates both determinate (with percentage) and indeterminate modes
**And** InfoBar shows all 4 severity types with actionable buttons
**And** ToolTip demonstrates standard and rich content (images, formatted text)
**And** Badge demonstrates notification count on icon buttons
**And** Chip/PillButton shows removable filter tags

### Story 4.4: Implement Tab 10 - Advanced & Special Controls

As a **developer**,
I want **a page demonstrating advanced interaction controls**,
So that **I can see SwipeControl, InkCanvas, RefreshContainer, Pivot, and other special controls**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Advanced.xaml implementation
**When** tab is selected
**Then** Page displays: SwipeControl (swipe actions on list items), InkCanvas, InkToolbar, ContactCard, AnnotatedScrollBar, PullToRefresh, RefreshContainer, SelectorBar, Pivot, Hub, ScrollBar
**And** SwipeControl demonstrates left/right swipe actions on list items
**And** InkCanvas allows drawing with InkToolbar for pen/highlighter/eraser selection
**And** RefreshContainer demonstrates pull-to-refresh pattern
**And** SelectorBar shows horizontal tab-like selector
**And** Pivot demonstrates mobile-style tabbed navigation

### Story 4.5: Implement Tab 11 - Design Elements & App Patterns

As a **developer**,
I want **a page demonstrating Fluent Design elements and MTM app-specific patterns**,
So that **I can see typography, colors, spacing, shadows, and reusable patterns like wizard, admin grid, master-detail**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_Patterns.xaml implementation
**When** tab is selected
**Then** Page displays: Typography ramp (all TextBlock styles), Color palette (ThemeResource colors), Brushes (Solid, Gradient, Acrylic, Reveal), ThemeShadow, Rounded corners (CornerRadius), Spacing system (4, 8, 12, 16, 24, 32px grid), Icons (FontIcon, SymbolIcon, BitmapIcon), Adaptive layouts, Card patterns, Form layouts, Master-detail pattern, Wizard pattern, Admin grid pattern, 4-card navigation
**And** Typography demonstrates all styles: TitleLarge, Title, Subtitle, BodyStrong, Body, Caption, Base
**And** Color palette shows all theme colors with hex codes
**And** Card patterns demonstrate elevation with ThemeShadow
**And** Wizard pattern references Module_Receiving multi-step design
**And** Admin grid pattern references Module_Dunnage pagination/search/filter
**And** 4-card navigation references Module_Settings layout

---

## Epic 5: Custom Controls

Implement 14 manufacturing-specific custom controls with DependencyProperties, proper styling, and accessibility support.

### Story 5.1: Create Foundation Custom Controls (MetricCard, StatusBadge, PartHeader)

As a **developer**,
I want **three foundation custom controls for manufacturing UI patterns**,
So that **common manufacturing components are reusable across applications**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing custom controls
**Then** Control_MetricCard.xaml/.cs exists with DependencyProperties: Label, Value, TrendText, TrendIcon, AccentColor, TrendColor
**And** Control_StatusBadge.xaml/.cs exists with Status property supporting: Pending (yellow), InProgress (blue), Completed (green), Error (red), OnHold (gray)
**And** Control_PartHeader.xaml/.cs exists with DependencyProperties: PartNumber, Description, PartImageUri, Specifications, StockStatus
**And** All controls inherit from UserControl with proper XML documentation
**And** All controls use ThemeResource for colors and support Light/Dark themes
**And** All controls have AutomationProperties.Name set for accessibility
**And** Controls use 8px CornerRadius for borders

### Story 5.2: Create Input Enhancement Controls (QuantityInput, SearchBox)

As a **developer**,
I want **enhanced input controls for manufacturing workflows**,
So that **quantity entry and search have better UX than standard TextBox**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing input controls
**Then** Control_QuantityInput.xaml/.cs exists with DependencyProperties: Quantity (double), UnitOfMeasure (string), MinValue, MaxValue, IncrementStep
**And** QuantityInput has large +/- buttons (44×44 touch targets), UOM dropdown, validation feedback
**And** QuantityInput has quick increment buttons (±1, ±10, ±100)
**And** Control_SearchBox.xaml/.cs exists with DependencyProperties: SearchText, FilterChips (ObservableCollection), RecentSearches
**And** SearchBox has filter chip display, recent searches dropdown, clear button
**And** Both controls raise ValueChanged events when user modifies values

### Story 5.3: Create Data Display Controls (DataTable, WizardStep)

As a **developer**,
I want **styled data table and wizard step indicator controls**,
So that **complex data and multi-step processes have consistent UI**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing data display controls
**Then** Control_DataTable.xaml/.cs exists with zebra striping, hover effects, action column support
**And** DataTable uses ItemsSource binding with custom ItemTemplate
**And** Control_WizardStep.xaml/.cs exists with DependencyProperties: StepNumber, StepTitle, StepStatus (Pending/Active/Completed)
**And** WizardStep shows step badge, title, visual states for Pending/Active/Completed
**And** WizardStep has Click event for navigation (if enabled)
**And** Both controls follow Fluent Design with proper shadows and corners

### Story 5.4: Create Manufacturing Action Controls (ActionButton, POSummaryCard)

As a **developer**,
I want **manufacturing-specific action and summary controls**,
So that **common manufacturing operations have consistent UI**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing action controls
**Then** Control_ActionButton.xaml/.cs exists with DependencyProperties: ActionType (Receive/Ship/Print/Validate), Label, IsEnabled
**And** ActionButton uses appropriate icon and color for each ActionType
**And** Control_POSummaryCard.xaml/.cs exists with DependencyProperties: PONumber, Vendor, OrderDate, TotalItems, Status
**And** POSummaryCard displays PO information in card layout with proper spacing
**And** Both controls use 44×44 minimum touch targets
**And** Both controls have proper keyboard and screen reader support

### Story 5.5: Create Modern UI State Controls (SkeletonLoader, ToastNotification, EmptyState, LoadingOverlay)

As a **developer**,
I want **modern loading and empty state controls**,
So that **the app provides professional feedback during data loading and empty states**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing state controls
**Then** Control_SkeletonLoader.xaml/.cs exists with animated shimmer effect and modes: Card, List, Text
**And** SkeletonLoader uses animated gradient brush for shimmer animation
**And** Control_ToastNotification.xaml/.cs exists with DependencyProperties: Message, ActionLabel, ShowAction, Duration, Position (Top/Bottom)
**And** ToastNotification slides in from top/bottom with auto-dismiss timer
**And** Control_EmptyState.xaml/.cs exists with DependencyProperties: Icon, Title, Description, ActionButtonText, EmptyStateType (NoData/NoResults/Error)
**And** EmptyState shows appropriate icon/message for each state type
**And** Control_LoadingOverlay.xaml/.cs exists with semi-transparent backdrop, spinner, message, and cancellation support
**And** All controls use Fluent Design animations and transitions

### Story 5.6: Create Specialized Manufacturing Controls (Timeline, BarcodeInput, SignaturePad)

As a **developer**,
I want **specialized controls for manufacturing workflows**,
So that **timeline tracking, barcode scanning, and signature capture have proper UI**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing specialized controls
**Then** Control_Timeline.xaml/.cs exists with DependencyProperties: TimelineItems (ObservableCollection), ShowConnectorLines
**And** Timeline displays events vertically with dates, markers, connector lines, expandable details
**And** Control_BarcodeInput.xaml/.cs exists with DependencyProperties: BarcodeText, BarcodeFormat (Code128/Code39/QR), ValidationMode
**And** BarcodeInput validates format, shows recent scans, auto-submits on scanner input (detects Enter/Tab suffix)
**And** Control_SignaturePad.xaml/.cs uses InkCanvas with Clear/Undo buttons, SaveAsImage method, minimum stroke validation
**And** All controls have proper error states and validation feedback

### Story 5.7: Create Navigation & Alert Controls (AlertBanner, NavigationCard, EmployeePicker)

As a **developer**,
I want **navigation and alert controls for common manufacturing UI patterns**,
So that **consistent navigation and alert patterns are reusable**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing navigation and alert controls
**Then** Control_AlertBanner.xaml/.cs exists with DependencyProperties: Message, Severity (Info/Success/Warning/Error), IsCloseable, ActionButtonText
**And** AlertBanner displays full-width banner with appropriate color for severity
**And** Control_NavigationCard.xaml/.cs exists with DependencyProperties: Title, Description, Icon, NavigationCommand
**And** NavigationCard has large 200×200 touch-friendly card with icon, title, description
**And** Control_EmployeePicker.xaml/.cs exists with DependencyProperties: SelectedEmployee, Employees (ObservableCollection), ShowPhoto
**And** EmployeePicker displays employee list with PersonPicture, name, employee number
**And** All controls use 44×44 minimum touch targets and proper accessibility

### Story 5.8: Create Input Enhancement Controls Part 2 (DateRangePicker, BarcodeInput, SignaturePad)

As a **developer**,
I want **advanced input controls for manufacturing data entry**,
So that **date ranges, barcode scanning, and signatures are properly captured**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing advanced input controls
**Then** Control_DateRangePicker.xaml/.cs exists with DependencyProperties: StartDate, EndDate, Preset (Today/ThisWeek/ThisMonth/Custom)
**And** DateRangePicker shows preset buttons and custom date pickers
**And** Control_BarcodeInput.xaml/.cs exists with DependencyProperties: BarcodeText, BarcodeFormat (Code128/Code39/QR), ValidationMode, RecentScans
**And** BarcodeInput validates format, auto-submits on scanner Enter/Tab, shows recent scans dropdown
**And** Control_SignaturePad.xaml/.cs uses InkCanvas with Clear/Undo buttons, SaveAsImage method (PNG/SVG), minimum stroke validation
**And** All controls raise appropriate events (DateRangeChanged, BarcodeScanned, SignatureCaptured)

### Story 5.9: Create Navigation & Input Controls (Breadcrumb, Stepper, ColorPicker, AvatarGroup)

As a **developer**,
I want **navigation breadcrumb, numeric stepper, color picker, and avatar group controls**,
So that **common UI patterns are consistently implemented**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing navigation and input controls
**Then** Control_Breadcrumb.xaml/.cs exists with DependencyProperties: Path (ObservableCollection of BreadcrumbItem), MaxVisibleItems
**And** Breadcrumb displays clickable path segments with overflow handling
**And** Control_Stepper.xaml/.cs exists with DependencyProperties: Value (double), MinValue, MaxValue, StepValue, ShowButtons
**And** Stepper has +/- buttons with keyboard support (arrow keys)
**And** Control_ColorPicker.xaml/.cs exists with DependencyProperties: SelectedColor, PresetColors, ShowHexInput
**And** ColorPicker displays preset swatches, custom color selector, hex/RGB input
**And** Control_AvatarGroup.xaml/.cs exists with DependencyProperties: Avatars (ObservableCollection), MaxVisible, ShowExpandButton
**And** AvatarGroup shows overlapping avatars with +X more indicator and expandable flyout

### Story 5.10: Create Advanced Button & File Controls (CustomSplitButton, SegmentedControl, FileUploader, RichTextEditor)

As a **developer**,
I want **split button, segmented control, file uploader, and rich text editor controls**,
So that **advanced input patterns are available**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing advanced controls
**Then** Control_CustomSplitButton.xaml/.cs exists with DependencyProperties: PrimaryAction, PrimaryLabel, MenuItems (ObservableCollection)
**And** CustomSplitButton shows primary action button with dropdown for additional options
**And** Control_SegmentedControl.xaml/.cs exists with DependencyProperties: Items (ObservableCollection), SelectedItem, CornerRadius
**And** SegmentedControl implements iOS-style segmented button group with smooth selection animation
**And** Control_FileUploader.xaml/.cs exists with DependencyProperties: AllowedFileTypes, MaxFileSize, UploadedFiles, IsDragActive
**And** FileUploader supports drag-and-drop, file type filtering, progress indicators, preview thumbnails
**And** Control_RichTextEditor.xaml/.cs exists with toolbar for Bold/Italic/Underline/Alignment, font/size selectors, undo/redo
**And** All controls follow Fluent Design with proper animations and states

### Story 5.11: Create Advanced UI Controls (TagInput, Carousel, CustomContextMenu, SplashScreen, OnboardingTour, KeyboardShortcuts)

As a **developer**,
I want **tag input, carousel, context menu, splash screen, onboarding, and keyboard shortcut controls**,
So that **all custom control patterns are complete**.

**Acceptance Criteria:**

**Given** Controls/ folder in module
**When** implementing remaining custom controls
**Then** Control_TagInput.xaml/.cs exists with DependencyProperties: Tags (ObservableCollection), SuggestionSource, AllowDuplicates
**And** TagInput allows add/remove tags, autocomplete, duplicate prevention
**And** Control_Carousel.xaml/.cs exists with DependencyProperties: Items, AutoPlayInterval, ShowIndicators, EnableSwipe
**And** Carousel supports touch/swipe, indicator dots, auto-play option
**And** Control_CustomContextMenu.xaml/.cs exists with hierarchical submenu support, icons, keyboard shortcuts display
**And** Control_SplashScreen.xaml/.cs displays logo/branding, progress indicator, version number
**And** Control_OnboardingTour.xaml/.cs provides step-by-step highlights, skip/next/previous navigation, progress dots
**And** Control_KeyboardShortcuts.xaml/.cs displays available shortcuts grouped by category in overlay panel
**And** All controls have proper accessibility and theme support

### Story 5.12: Create Tab 12 - Custom Controls Showcase Page

As a **developer**,
I want **a dedicated tab showcasing all 34 custom controls with usage examples**,
So that **developers can see how to use the custom controls**.

**Acceptance Criteria:**

**Given** View_UI_Mockup_CustomControls.xaml implementation
**When** tab 12 is selected
**Then** Page displays all 34 custom controls organized by category: Foundation (MetricCard, StatusBadge, PartHeader, POSummaryCard), Input (QuantityInput, SearchBox, DateRangePicker, BarcodeInput, SignaturePad, Stepper, ColorPicker, TagInput, FileUploader, RichTextEditor), Display (DataTable, WizardStep, Timeline, Carousel, Breadcrumb, AvatarGroup), Actions (ActionButton, CustomSplitButton, SegmentedControl), Feedback (SkeletonLoader, ToastNotification, EmptyState, LoadingOverlay, AlertBanner, SplashScreen), Navigation (NavigationCard, EmployeePicker, CustomContextMenu, OnboardingTour, KeyboardShortcuts)
**And** Each control has usage example with sample data
**And** Each control section includes XAML code snippet showing DependencyProperty bindings
**And** ViewModel_UI_Mockup_CustomControls provides sample data for all controls
**And** Page demonstrates responsive layout with controls in categorized sections

---

## Epic 6: Sample Data & Documentation

Create comprehensive mock data model with manufacturing context and complete README documentation for module portability.

### Story 6.1: Implement Model_UI_SampleData with Manufacturing Context

As a **developer**,
I want **a comprehensive sample data model with realistic manufacturing data**,
So that **all controls and views have meaningful demo data**.

**Acceptance Criteria:**

**Given** Models/ folder in module
**When** implementing sample data
**Then** Model_UI_SampleData.cs exists in Models/ folder
**And** Provides PO number collection (format: PO-123456, PO-066868)
**And** Provides part number collection (format: ABC-12345-XYZ, MCC-45678-A, MMF-98765-B)
**And** Provides receiving transaction data (dates, quantities, statuses: Pending/In Progress/Completed, receivers)
**And** Provides employee data (names, employee numbers 1000-9999, roles: Operator/Lead/Material Handler/Quality)
**And** Provides dunnage types (box sizes: 12x8x6, pallet types: 48x40 Standard, 48x48 Euro)
**And** Provides routing recipients (departments: Press Floor/Assembly/Shipping, stations: Press 1/Line A)
**And** Provides hierarchical tree data (Departments → Lines → Stations → Workcenters)
**And** Provides GetImageUri(imageName) method with fallback pattern
**And** Provides GetEmployeePhoto(employeeNumber) method using DiceBear API or placeholder
**And** All data is static and requires no database or external service

### Story 6.2: Create Module README with Portability Guide

As a **developer/admin**,
I want **complete README documentation explaining the module's purpose and how to use it**,
So that **the module can be easily understood and copied to other projects**.

**Acceptance Criteria:**

**Given** Module_UI_Mockup/ folder
**When** creating documentation
**Then** README.md exists in Module_UI_Mockup/ root folder
**And** Explains module purpose: "100% standalone WinUI 3 control gallery for UI reference and pattern library"
**And** Documents how to launch: `Window_UI_Mockup.Launch()` or `new Window_UI_Mockup().Activate()`
**And** Lists 4 required NuGet packages with minimum versions
**And** Documents all 13 tabs and their control categories
**And** Explains standalone architecture: no dependencies on IService_ErrorHandler, ViewModel_Shared_Base, WindowHelper_WindowSizeAndStartupLocation
**And** Provides portability checklist: copy entire Module_UI_Mockup folder, install NuGet packages, optionally rename namespaces
**And** Documents code patterns: ViewModel_Base inheritance, [ObservableProperty], [RelayCommand], x:Bind usage
**And** Documents manufacturing-specific patterns: wizard, admin grid, master-detail references
**And** Includes links to WinUI 3 Gallery (Microsoft Store), GitHub, Fluent Design System, API Reference
**And** All public classes and methods have XML documentation comments in code

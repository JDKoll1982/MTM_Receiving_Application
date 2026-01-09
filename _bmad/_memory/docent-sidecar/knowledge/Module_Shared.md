---
module_name: Module_Shared
module_path: Module_Shared/
last_analyzed: 2026-01-09
last_validated: 2026-01-09
analyst: Docent v1.0.0
documentation_scope: full-module-analysis
component_counts:
  views_xaml_pages: 5
  viewmodels: 6
  module_services: 0
  module_daos: 0
  module_models: 0
  module_enums: 0
key_workflows:
  - Splash screen startup progress window
  - Shared terminal PIN login dialog with lockout
  - New user setup dialog (account creation)
  - Central help dialog with related topics, dismissible tips, copy-to-clipboard
  - Icon selector window (Material Icons) with search + paging
integration_points:
  dependencies:
    - Module_Core services (authentication, help, notification, focus, logging, error handling, navigation/session)
    - CommunityToolkit.Mvvm
    - WinUI 3 (Window, ContentDialog, InfoBar)
    - Material.Icons.WinUI3 (icon selector and help icon rendering)
notes:
  companion_docs:
    - _bmad/_memory/docent-sidecar/knowledge/Module_Shared-CodeInventory.md
    - _bmad/_memory/docent-sidecar/knowledge/Module_Shared-Database.md
---

# Module_Shared - Module Documentation

## Module Overview

Module_Shared provides shared UI surfaces used across the app (startup splash, authentication dialogs, new-user setup, centralized help dialog, and a reusable icon selector).

This module does **not** define its own DAOs/services; instead it depends on Module_Core-provided services (authentication, help, notification, error handling, logging, focus, session).

## Mermaid Workflow Diagrams

### Startup Splash → Login → Main Window

```mermaid
flowchart TD
  App[App Startup] --> Splash[View_Shared_SplashScreenWindow]
  Splash --> Init[Initialization Tasks]
  Init --> Login[View_Shared_SharedTerminalLoginDialog]
  Login -->|Success| Main[Main Window]
  Login -->|Cancelled| Exit[Exit App]
  Login -->|Locked Out| Exit
```

### Central Help Dialog

```mermaid
flowchart TD
  HelpTrigger[User requests help] --> HelpDlg[View_Shared_HelpDialog]
  HelpDlg --> Content[IService_Help.GetHelpContent]
  HelpDlg --> Related[Related topics]
  Related --> HelpDlg
  HelpDlg -->|Dismiss tip| Persist[IService_Help.SetDismissedAsync]
  HelpDlg -->|Copy| Clipboard[Clipboard.SetContent]
```

### Icon Selector Window

```mermaid
flowchart TD
  Caller[Caller needs icon] --> IconWin[View_Shared_IconSelectorWindow]
  IconWin --> Filter[Search + ShowAll toggle]
  Filter --> Page[Paging 16/page]
  Page --> Select[User clicks icon]
  Select --> Return[WaitForSelectionAsync completes]
  IconWin -->|Cancel/Close| Return
```

## Key Behaviors

### Splash screen window

- Window: `View_Shared_SplashScreenWindow`
- Uses `ViewModel_Shared_SplashScreen` to drive status text + progress value.
- Important safety behavior: if user manually closes the splash window (not a programmatic close), the app exits.

### Shared terminal login

- Dialog: `View_Shared_SharedTerminalLoginDialog`
- ViewModel: `ViewModel_Shared_SharedTerminalLogin`
- Validates username and a 4-digit numeric PIN.
- Enforces a 3-attempt limit; on lockout:
  - disables inputs/buttons
  - waits ~5 seconds
  - sets `ViewModel.IsLockedOut = true` and closes
- Uses `IService_Focus` to focus username on open.

### New user setup

- Dialog: `View_Shared_NewUserSetupDialog`
- ViewModel: `ViewModel_Shared_NewUserSetup`
- Loads Departments via `IService_Authentication.GetActiveDepartmentsAsync()`
- Supports “Other” department with a custom text field.
- Optional ERP credential capture (Visual/Infor username/password) for integration; shows a warning in UI.
- Creates account via `IService_Authentication.CreateNewUserAsync()` after validating employee number and PIN rules.

### Central help dialog

- Dialog: `View_Shared_HelpDialog` + `ViewModel_Shared_HelpDialog`
- Loads a `Model_HelpContent` and optional related topics.
- Supports dismissible “Tip” help entries via `IService_Help.IsDismissedAsync()` and `SetDismissedAsync()`.
- Copy-to-clipboard command copies the help content text.

### Base ViewModel behavior

- `ViewModel_Shared_Base` provides `IsBusy`, `StatusMessage`, and a `ShowStatus()` helper.
- Note: `ShowStatus()` uses `App.GetService<IService_Notification>()` (service locator) to publish global notifications.

## Code Inventory

See: [_bmad/_memory/docent-sidecar/knowledge/Module_Shared-CodeInventory.md](../docent-sidecar/knowledge/Module_Shared-CodeInventory.md)

## Database & Integration Notes

See: [_bmad/_memory/docent-sidecar/knowledge/Module_Shared-Database.md](../docent-sidecar/knowledge/Module_Shared-Database.md)

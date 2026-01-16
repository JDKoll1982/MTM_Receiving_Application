---
module_name: Module_Shared
component: code-inventory
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Shared - Code Inventory

Companion to:

- [_bmad/_memory/docent-sidecar/knowledge/Module_Shared.md](../docent-sidecar/knowledge/Module_Shared.md)

## Views (XAML)

| View | Type | Backing ViewModel | Purpose |
|------|------|-------------------|---------|
| View_Shared_SplashScreenWindow | Window | ViewModel_Shared_SplashScreen | Startup splash with progress + status |
| View_Shared_SharedTerminalLoginDialog | ContentDialog | ViewModel_Shared_SharedTerminalLogin | Username + PIN login with 3-attempt lockout |
| View_Shared_NewUserSetupDialog | ContentDialog | ViewModel_Shared_NewUserSetup | Create user account when not found |
| View_Shared_HelpDialog | ContentDialog | ViewModel_Shared_HelpDialog | Central help viewer with related topics + dismissible tips |
| View_Shared_IconSelectorWindow | Window | (code-behind managed) | Select Material icon with search + paging |

## ViewModels

| ViewModel | Responsibilities | Key Dependencies |
|----------|------------------|------------------|
| ViewModel_Shared_Base | Common VM props (`IsBusy`, `StatusMessage`) and `ShowStatus()` | IService_ErrorHandler, IService_LoggingUtility, IService_Notification (via App.GetService) |
| ViewModel_Shared_MainWindow | Maps notification severity to WinUI `InfoBarSeverity`; user display | IService_UserSessionManager, IService_Notification |
| ViewModel_Shared_SplashScreen | Progress + indeterminate splash state | IService_ErrorHandler, IService_LoggingUtility |
| ViewModel_Shared_SharedTerminalLogin | PIN auth workflow + result flags | IService_Authentication |
| ViewModel_Shared_NewUserSetup | Department list + create user workflow | IService_Authentication |
| ViewModel_Shared_HelpDialog | Loads help content + related topics; dismiss + copy | IService_Help |

## Module-local Services / DAOs

- None. Module_Shared relies on Module_Core service registrations.

## External Libraries

- `CommunityToolkit.Mvvm`
- `Material.Icons.WinUI3`
- WinUI 3 primitives: `Window`, `ContentDialog`, `InfoBar`

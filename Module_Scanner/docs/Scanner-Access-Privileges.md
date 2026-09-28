# Scanner Access Privileges

Last Updated: 2026-09-28

Scanner access is controlled by a single plant-wide allow-list instead of the earlier
hardcoded developer check. An admin or developer maintains the list from
Core Settings, and the change applies to every workstation at once.

## Where the Setting Lives

- Category: `Scanner`
- Key: `Scanner.Access.AllowedEmployeeNumbers`
- Scope: `System` (plant-wide, not per user or per workstation)
- Permission level: `Admin` (admins and developers)
- Data type: `Json` — an array of employee numbers, for example `[7,42]`

The value is stored through the existing core settings pipeline, so it reuses the standard
audit trail, locking, and caching behavior.

## Who Can Change It

Open Core Settings and choose the **Scanner Access** card. The card is visible to admin and
developer accounts only, and that gate is deliberately independent of the allow-list itself so
an administrator can never lock themselves out of the page.

## How Access Is Decided

1. If the allow-list has never been saved (empty array), the legacy developer rule still
   applies: `jkoll`, `johnk`, or any user whose department is `Developer`.
2. Once the allow-list contains at least one employee number, the list is absolute. Only the
   listed employees may use Scanner, and an unresolved session user is denied.
3. If the setting cannot be read at all, the app falls back to the legacy developer rule and
   logs a warning. Access is never widened by a read failure.

## What the Privilege Blocks

- The Scanner entry in the main navigation bar (hidden and disabled).
- The global `Ctrl+Alt+M` scanner send hotkey (registered only for allowed users).
- Navigation into the Scanner module: the allow-list is re-read when a user actually tries to
  open Scanner, so a just-saved change takes effect immediately.

## Files That Implement This

- `Module_Scanner/Settings/ScannerSettingsKeys.cs` — setting category and key.
- `Module_Scanner/Contracts/IService_ScannerAccessPolicy.cs` — policy contract.
- `Module_Scanner/Services/Service_ScannerAccessPolicy.cs` — allow-list read, save, and cache.
- `Module_Scanner/Helpers/Helper_ScannerAccess.cs` — legacy fallback rule.
- `Module_Settings.Core/Views/View_Settings_ScannerAccess.xaml` — admin page.
- `Module_Settings.Core/ViewModels/ViewModel_Settings_ScannerAccess.cs` — page logic.
- `MainWindow.xaml.cs` — navigation gate, hotkey gate, and entry guard.

## See Also

- `Module_Scanner/docs/redesign-plan.md`
- `Module_Settings.Core/Defaults/settings.manifest.json`

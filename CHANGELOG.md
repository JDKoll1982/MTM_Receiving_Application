# CHANGELOG

All notable changes to the MTM Receiving Application are documented in this file.
Categories follow Keep a Changelog conventions: Added, Changed, Fixed, Removed, Security.

## Unreleased

### Added

#### Application windows open maximized (2026-09-10)

- Every top-level application window now opens maximized, so users no longer have to maximize
  windows manually. Applies to the main window, the Settings window, the Volvo shipment history
  detail window, and the shared icon selector.
- Added a `Maximize()` extension to `Module_Core/Helpers/UI/Helper_WindowExtensions.cs`, matching
  the existing `SetWindowSize`, `CenterOnScreen`, and `SetFixedSize` helpers rather than repeating
  presenter logic in each window constructor.
- Each window still sets an explicit pre-maximize size, so un-maximizing restores a sensible
  window size instead of the framework default.
- The new-user setup dialog is a `ContentDialog`, so it automatically fills the main window and
  needed no separate change.
- The splash screen is intentionally excluded. It declares
  `SetFixedSize(disableMaximize: true, disableMinimize: true)` for its branded startup layout, and
  forcing it to maximize would override that design decision.
- Files changed:
  - `Module_Core/Helpers/UI/Helper_WindowExtensions.cs`
  - `MainWindow.xaml.cs`
  - `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`
  - `Module_Shared/Views/View_Shared_IconSelectorWindow.xaml.cs`
  - `Module_Volvo/Views/View_Volvo_ShipmentHistoryDetailWindow.cs`

### Fixed

#### Maximized startup no longer flashes the theme or skips the startup page (2026-09-10)

- **Symptom:** After windows were changed to open maximized, the main window became visible
  during startup, visibly switched from the default theme to the saved theme, and opened on the
  empty `Dashboard` placeholder instead of the Receiving workflow mode-selection page.
- **Cause:** `MainWindow` maximized itself in its constructor, before `Application` activated the
  window. Maximizing a WinUI window that has not been activated yet makes the OS show it
  immediately, so it painted with the default theme while startup was still running. The early
  show also consumed the first activation, so the first `Activated` event that drives the startup
  navigation never reached the handler and `_hasNavigatedOnStartup` stayed `false`.
- **Fix:** Removed the constructor-level maximize and applied it in
  `Service_OnStartup_AppLifecycle` immediately after `App.MainWindow?.Activate()`. The window
  stays hidden until it is activated (restoring the previous theme timing and startup navigation)
  and is maximized in the same dispatcher turn, so it is never painted at the restore size.
- Files changed:
  - `MainWindow.xaml.cs`
  - `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

#### Ship/Rec Tools no longer resizes the main window (2026-09-10)

- **Symptom:** Opening Ship/Rec Tools from another module, or selecting any tool card, changed
  the size of the main window.
- **Cause:** `View_ShipRecTools_Main.UpdateActiveViewStatus` called `RestoreMainWindowSize()` on
  every visible-tool change, which resized the maximized main window back to 1450 x 900.
- **Fix:** Removed `RestoreMainWindowSize()` and its call sites. The resize was redundant — it
  re-applied the size the main window already sets during startup.
- Files changed:
  - `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`

#### Receiving history delete no longer raises a false missing-row error (2026-09-10)

- **Symptom:** Removing loads from Receiving Edit Mode failed with
  `InvalidOperationException: No receiving history row matched the delete request for load '1'`.
  The failure surfaced to the user as `Failed to save receiving data`.
- **Impact:** The selected history rows were not deleted and the transaction rolled back, so no
  data was lost. The real cost was that receiving history rows could not be removed at all.
- **Cause:** `sp_Receiving_Load_Delete` ends with `SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;`.
  MySQL reports the row count of a stored procedure's **final** statement when the procedure is
  invoked, and `SET` always reports `0`. `Dao_ReceivingLoad.DeleteLoadsAsync` read
  `AffectedRows <= 0` as "no matching row" and threw, even though the preceding `DELETE` had
  succeeded.
- **Fix:** When `AffectedRows <= 0`, `DeleteLoadsAsync` now re-checks the row with
  `ReceivingHistoryRowExistsAsync` and throws only when the row is genuinely still present. This
  matches the guard already used by `UpdateLoadsAsync` in the same DAO and by
  `Dao_ReceivingLabelData`.
- **Files changed:** `Module_Receiving/Data/Dao_ReceivingLoad.cs`

## Verification Evidence

### Reported affected rows versus actual deletion

A throwaway probe ran against the test schema (`mtm_receiving_application_test`) to isolate the
affected-rows behavior of a `CALL`. Both variants deleted their target row.

| Call body                                                       | Rows reported by client | Rows actually remaining |
| --------------------------------------------------------------- | ----------------------- | ----------------------- |
| `SET FOREIGN_KEY_CHECKS=0; DELETE ...; SET FOREIGN_KEY_CHECKS=back;` | 0                       | 0                       |
| `DELETE ...;`                                                   | 1                       | 0                       |

### Deployed procedure matches the repository

The live `sp_Receiving_Load_Delete` definition was read from the database and compared against
`Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql`.
The trailing `SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;` is present in both, so the bug
is in the stored procedure's shape, not in a deployment drift.

### Introducing change

The trailing `SET` was appended by commit `1e4119e1` on 2026-07-20, which added foreign-key-check
handling across the Receiving and Volvo procedures. Any caller that infers row existence from the
affected-rows count of those procedures inherited the same defect.

## Related Risk

61 stored procedures under
`Database/Database_Deployment/Sql_Files/StoredProcedures/` end with a
`SET FOREIGN_KEY_CHECKS` restore rather than a data-modifying statement. Every DAO that calls one
of these procedures and branches on `AffectedRows` is exposed to the same false-negative result.

Call sites already guarded against this and known to be safe:

- `Dao_ReceivingLoad.UpdateLoadsAsync` — re-checks row existence when `AffectedRows <= 0`.
- `Dao_ReceivingLabelData` — re-checks row existence and falls back to an idempotent path.

`Dao_ReceivingLoad.DeleteLoadsAsync` was the only receiving delete path missing the guard and is
resolved by this change.

## Follow-Up

- [ ] Audit remaining DAO call sites that branch on `AffectedRows` for the 61 procedures with a
      trailing `SET FOREIGN_KEY_CHECKS` restore, prioritizing delete and update paths.
- [ ] Decide whether to fix the shape of the stored procedures themselves. Moving the
      `FOREIGN_KEY_CHECKS` restore into the `EXIT HANDLER` and terminating the body with the
      data-modifying statement would restore meaningful affected-rows reporting for every caller.
      This is a database-schema change and requires explicit approval before implementation.

## See Also

- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql`
- <https://keepachangelog.com/en/1.1.0/>

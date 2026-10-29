# CHANGELOG

All notable changes to the MTM Receiving Application are documented in this file.
Categories follow Keep a Changelog conventions: Added, Changed, Fixed, Removed, Security.

## Unreleased

### Added

#### Receiving Analytics split into ERP Data and App Data tabs (2026-10-29)

- **What changed:** The Receiving Analytics page is now two tabs. **ERP Data** is today's page,
  unchanged, and stays the tab that opens. **App Data** is new: it reads the app's own
  `receiving_history` instead of Infor Visual and answers what the dock actually did. It carries its
  own date range and its own filters, so switching tabs does not disturb the other tab's state.
- **What App Data shows**, in the pinned order, one panel per page behind a navbar with the filter
  bar above it:
  **Activity** (loads, labels and quantity by category, switchable between day, week and
  month), **On Time** (early, on time and late against the PO due date, with the on-time
  percentage coloured against the configured target), **Suppliers** (supplier ranking
  by labels, quantity or loads, with the leading suppliers' share and a remainder), and **Coverage**
  (two receipt-count coverage lines over the same dates, and the app's
  category split beside Infor Visual's over the same five categories). Each panel owns the full area
  below the navbar, so none of them scrolls, and reloading or clearing leaves you on the page you
  were reading. Each page carries one plain sentence saying what its figures are read from.
  Saturdays and Sundays are not read: no weekend day appears on a day axis and weekend
  receipts count towards no total, with the Infor Visual side of the coverage chart dropping the same
  weekend dates. The Coverage page compares only the part numbers our own receipts carry for the
  selected dates, because Infor Visual records material that never flows through the app; on live data
  that limits a thirty-day comparison from 1,791 Infor Visual lines to 291. Both sides of the
  comparison count purchase-order lines on the date each was last received, the way Infor Visual dates
  a line, rather than counting labels: the same window holds 970 labels but only 271 PO lines, so
  labels would have reported 333 per cent coverage where the line unit reports 93. The category mix on
  the same page uses that same cohort and unit, and the ERP Data tab keeps reading all of Infor Visual.
- **Why:** Infor Visual knows what arrived at the plant; only the app knows what an operator
  completed and whether it was on time. The comparison panels are what prove the app is capturing
  the real work.
- **Notes:** Both halves of the category mix count receipts, one app label against one Infor Visual
  receipt line, which is the same alignment rule the coverage panel uses. App Data uses four new
  stored procedures in the `sp_Reporting_ReceivingAnalyticsApp_*` family; the app database stays
  read-only and Infor Visual is still opened with read-only intent. Nothing on the page names an
  employee. New setting `ReceivingAnalytics.OnTimeTarget`, administrator scope, default **95**
  per cent; with no value configured the percentage is shown plainly with no colour. Known limits
  are inherited from the unchanged ERP query.
- Files changed:
  - `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalytics.xaml`
  - `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs`
  - `Module_ShipRec_Tools/Services/Service_Tool_ReceivingAnalyticsApp.cs`
  - `Module_ShipRec_Tools/Data/Dao_Tool_ReceivingAnalyticsApp.cs`
  - `Module_ShipRec_Tools/Helpers/Translation_ReceivingAnalyticsCategory.cs`
  - `Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs`
  - `Module_Settings.Core/Defaults/settings.manifest.json`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay.sql`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay.sql`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount.sql`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals.sql`
  - `Module_ShipRec_Tools/docs/ReceivingAnalyticsAppDataTab_FeatureUpdate.md`

#### Packing list scan probe tool (2026-10-22)

- **What changed:** Added `tools/PackingListScanProbe`, a standalone .NET 10 console tool that
  OCRs scanned packing lists, bills of lading, ERP screenshots, and supplier emails using the
  in-box `Windows.Media.Ocr` and `Windows.Data.Pdf` APIs, and prints the part numbers, purchase
  orders, quantities, heats, dates, and warehouses it finds. Usage is documented in
  `PACKING_LIST_SCAN_PROBE.md`.
- **Why:** To evaluate scan-to-receive capture for the receiving workflow against real MTM
  documents before wiring any of it into the app. `docling-parse` was evaluated first and cannot
  do this: it parses born-digital PDFs and returns zero text on copier scans.
- **Notes:** The tool is not referenced by the solution and the app project excludes `tools\**`;
  `docling-parse` and full `docling` measurements are recorded in the new document. No app,
  schema, or stored procedure change.

#### WIP added to the dunnage Inventory Type options (2026-09-24)

- **What changed:** The **Add New Dunnage Part** and **Edit Dunnage Part** dialogs now offer
  **WIP** alongside Not Inventoried, Adjust In, Receive In, and Both.
- **Why:** Users need to flag dunnage that is consumed by work in progress instead of being
  received into stock.
- **Notes:** No schema or stored procedure change was needed.
  `dunnage_requires_inventory.inventory_method` is a free-text `VARCHAR(100)` column, and both part
  stored procedures only special-case blank and `NOT INVENTORIED`, so WIP is stored and shown
  as-is in the part details card, the review step, and saved history.
- Files changed:
  - `Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml`
  - `Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml`

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

### Changed

#### Weight/Quantity boxes accept digits only and update Current Total while typing (2026-10-08)

- **Symptom:** On the guided **Receiving - Enter Weight & Quantity** step, the **Current Total**
  readout did not move until a box was committed (Enter, spin-button click, or clicking away), so
  users could not watch the running total while keying loads.
- **Cause:** Each load row used a `NumberBox` with `AcceptsExpression="True"`. `NumberBox` only
  commits `Value` on Enter, spin-button click, or focus change, so the bound
  `Model_ReceivingLoad.WeightQuantity` — and therefore `CurrentTotal` — stayed behind the typed
  text. `AcceptsExpression` also allowed expressions such as `2+3`, and unparseable text resolved
  to `NaN`, which the decimal converter silently turned into `0`.
- **Fix:** Each load row is now a `TextBox` (`InputScope="Number"`) bound per keystroke
  (`UpdateSourceTrigger=PropertyChanged`) through the new `Converter_DecimalToBlankText`, so
  `WeightQuantity` — and the `CurrentTotal` the view model already recalculates from
  `Model_ReceivingLoad.PropertyChanged` — update on every digit. A `BeforeTextChanging` handler
  cancels any keystroke containing a non-digit character, so an unparseable value can never reach
  the model and Current Total can never be computed from bad input.
- **Notes:** Weight/Quantity stays whole-number entry, matching the `Enter whole number`
  placeholder and the label-side rounding in `Dao_ReceivingLabelData`. The blank sentinel is `0`,
  so `Converter_DecimalToBlankText` renders `0` as an empty box, mirroring
  `Converter_IntToBlankText`. Focus-on-step-entry now targets the first `TextBox`.
- Files changed:
  - `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml`
  - `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs`
  - `Module_Core/Converters/Converter_DecimalToBlankText.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Core/Converters/Converter_DecimalToBlankTextTests.cs`

#### Package Type counts start empty and update EA per Package while typing (2026-09-24)

- **Symptom:** The guided **Receiving - Enter Package Type** step opened with `1` already shown in
  every package-count box, so Auto-Fill had nothing blank to fill and users had to clear each box
  before typing real counts. The **EA per Package** readout also only changed after a box was
  committed (Enter, Tab, or clicking away).
- **Cause:** `Model_ReceivingLoad.OnPartIDChanged` forced `PackagesPerLoad` to `1` whenever a part
  ID was assigned, and the guided workflow assigns the part ID while it generates the loads. The
  readout then lagged because `NumberBox.ValueChanged` only fires on Enter, spin-button click, or
  focus change, and the `NumberBox` template never feeds typed text back to `Value`/`Text`.
- **Fix:** Assigning a part ID no longer seeds the package count, so counts stay at the blank
  sentinel (`0`) and the boxes render empty for Auto-Fill. Each package-count box is now a
  `TextBox` bound per keystroke (`UpdateSourceTrigger=PropertyChanged`), which makes
  `EA per Package` recalculate as digits are typed. Blank rows still fall back to a single package
  when the step is left (guided Package Type, or Manual Entry save), keeping the previous end
  result. Blank text handling comes from the new `Converter_IntToBlankText`, which maps `0` to an
  empty string and treats empty, unparseable, or non-positive text as `0`.
- **Trade-off:** a `TextBox` has no arrow-key stepping and no built-in numeric validation, so
  non-digit text leaves the count blank instead of being overwritten. This matches how the Manual
  Entry grid already handles the same column.
- Files changed:
  - `Module_Receiving/Models/Model_ReceivingLoad.cs`
  - `Module_Receiving/Views/View_Receiving_PackageType.xaml`
  - `Module_Receiving/Views/View_Receiving_PackageType.xaml.cs`
  - `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
  - `Module_Core/Converters/Converter_IntToBlankText.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Core/Converters/Converter_IntToBlankTextTests.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Models/Model_ReceivingLoadTests.cs`

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

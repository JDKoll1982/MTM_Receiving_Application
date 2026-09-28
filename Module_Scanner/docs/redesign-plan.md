# Module_Scanner Redesign Plan

Last Updated: 2026-08-26

## Overview

Ground-up redesign of the Scanner module. The legacy implementation is preserved for
reference only under `docs/reference/module-scanner-legacy/` as `.txt` files. This document
captures the new architecture and how it satisfies `ScannerUpdate.md`.

## Goals

- Remove the entire Draft feature: no "Start Draft" / "Save For Later" buttons, no
  `StartSessionAsync`, no `BuildRunSnapshotAsync`, and no run tables/DAOs.
- Only two concepts remain: the **current list** (persisted, auto-created when the Workbench
  loads) and **History** (list of prior sessions).
- Implement every task in `ScannerUpdate.md`:
  - Input lockout during automation + integration tests.
  - Workbench baseline input states.
  - Part-number validation workflow (stock-location modal, non-blocking header errors,
    5-second auto-clear).
  - To-location format sanitization (16 layout variants).
  - "Manage Item" modal size fix.
- Leaner module: drop Advanced Bulk Move (not required by the task list).

## Architecture

Layer flow (unchanged from repo rules): View -> ViewModel -> Service -> DAO -> MySQL.
Infor Visual (SQL Server) remains read-only.

### New file layout

- `Contracts/` - service interfaces
- `Data/` - `Dao_ScannerBatchSession`, `Dao_ScannerBatchItem`, `Dao_ScannerProfile`
- `Helpers/` - `Helper_ScannerAccess`, `Helper_ScannerSequence`, `Helper_ScannerLocationFormat`
- `Models/` - session/item/profile models, history query models, enums (no Draft)
- `Services/` - workflow, validation, execution, navigation, input engine, hotkey
- `Settings/` - `ScannerSettingsKeys`
- `ViewModels/` - Main, Workbench, History, Settings
- `Views/` - Main, Workbench, History, Settings, ManageItemsDialog

### Removed vs added

| Removed | Added |
| --- | --- |
| `StartSessionAsync` / `BuildRunSnapshotAsync` | `EnsureCurrentSessionAsync` (auto-create on Workbench load) |
| `Model_ScannerSessionStartRequest/Response` | - |
| `Dao_ScannerRunHistory`, run tables/SPs | - |
| `Model_ScannerRun` / `Model_ScannerRunItem` | `Model_ScannerHistoryEntry` / `Model_ScannerHistoryItem` |
| `Enum_ScannerSessionStatus.Draft` | status `Ready` used for a new current list |
| Advanced Bulk Move page/VM/service | - |
| - | `Helper_ScannerLocationFormat` (16-variant sanitizer) |
| - | `IsAutomationRunning` + input lockout in Workbench VM |

## ScannerUpdate.md mapping

- Task 1/2/2b: `Service_ScannerExecution` raises automation state; Workbench binds
  `IsAutomationRunning` and disables inputs; integration tests verify `Enabled = false`.
- Task 3/4: buttons and commands removed.
- Task 5: `View_Scanner_ManageItemsDialog` sized via `ContentDialogMinWidth` (1000) /
  `ContentDialogMaxWidth` (1600) / `ContentDialogMaxHeight` (760) resources + a
  `ScrollViewer`-capped list.
- Task 6: baseline input states in `View_Scanner_Workbench.xaml`.
- Step 6b: part focus-lost validation -> stock-location `ContentDialog` -> header error with
  5s auto-clear; per-row To/Qty `LostFocus` -> `Helper_ScannerLocationFormat.Sanitize` +
  `ValidateSessionItemAsync`.
- Step 6b-a1 modal: multi-select rows (checkbox), column headers (Location / Total Qty / Qty /
  # Trans), Total Qty formatted without trailing zeros, editable Qty clamped to on-hand, and a
  # of Transactions `NumberBox` (min 1). The picker dialog uses the same ContentDialog sizing
  resources as the larger repo dialogs (`ContentDialogMinWidth` 900 / `ContentDialogMaxWidth`
  1600 / `ContentDialogMaxHeight` 760) so the primary Part/Location column (star-weighted
  `1.5*`) stays readable instead of collapsing to the default 548px width. Cancel or Use
  Selected clears the Part Number input to prevent modal reopen. Use Selected populates the
  current list immediately (picks x # trans), leaving destinations empty.
- Top bar reduced to the Part lookup only; From / To / Qty are edited per-row in the table
  (`Model_ScannerBatchItem` is observable). Each row edit re-validates on focus loss and updates
  its Status column. Row validation persists silently WITHOUT rebuilding `SessionItems`, so the
  focus the operator was moving to is preserved.
- Table columns align with the header via `ListView.ItemContainerStyle`
  (`HorizontalContentAlignment=Stretch` + zero horizontal `Padding`) — `ListViewItem` defaults to
  left-aligned content with horizontal padding that shifted rows off the header.
- Status column shows friendly text + color: `✅ Ready!` (green), `! <reason>` (red: Qty too High
  / Qty less than 1 / To location required), `⏳ Enter destination` (gray).
  `Converter_ValidationStatusToBrush` maps `bool?` to green/red/gray. The reason is derived from
  the row's own data first (so it survives an app restart), then falls back to the stored
  validation message.
- Cell navigation: Tab = To -> Qty -> next row's To -> Qty; Enter = To -> next row's Qty, Qty ->
  next row's To; clicking goes where clicked. Past the last row focus returns to the Part lookup.
- Quantity "too High" is checked against the on-hand captured from the stock modal
  (`Model_ScannerBatchItem.MaxQuantity`, carried via `Model_ScannerStockPick.OnHand`).
- Primary button toggles **Send / Validate**: the label is "Validate" when the selected row is
  not valid, and clicking it shows a "Fix the issues before sending" popup instead of sending.
  When the selected row validates, the button becomes "Send" and sends the selected line only
  (`SendSpecificItemAsync`).
- **Search mode toggle** (Part / Location, top bar). Part mode = existing workflow. Location
  mode mirrors it: enter a location -> validate it -> modal lists parts with stock at that
  location (with a Select All / Select None toggle) -> rows are populated with the picked parts
  and that source location (`GetPartsAtLocationAsync` via `GetMaterialAvailabilityCurrentStock`).
- "Remove Selected" moved out of the table header to the top bar (beside the Part lookup and the
  search-mode toggle), which also restores header/row column alignment.
- Footer "Open Inventory" button launches/activates VMINVENT and sends the open-window shortcut
  (`Alt+I`) so the operator can reach the Inventory Transfers window
  (`IService_ScannerExecution.OpenInventoryWindowAsync`).

## Database

- Keep: `receiving_scanner_session`, `receiving_scanner_item`, `receiving_scanner_profile`.
- Drop: `receiving_scanner_run`, `receiving_scanner_run_item`, run SPs, scanner functions/views.
- Migration `14_Migration_receiving_scanner_redesign.sql` drops legacy objects and removes
  existing Draft sessions.

## Validation

- Build: `dotnet build MTM_Receiving_Application.csproj -p:Platform=x64`
- Tests: `dotnet test MTM_Receiving_Application.Tests/... --filter FullyQualifiedName~Module_Scanner`

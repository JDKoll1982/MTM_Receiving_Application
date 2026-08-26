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
- Task 5: `View_Scanner_ManageItemsDialog` sized via `MinWidth`/`MinHeight` + `ScrollViewer`.
- Task 6: baseline input states in `View_Scanner_Workbench.xaml`.
- Step 6b: part focus-lost validation -> stock-location `ContentDialog` -> header error with
  5s auto-clear; To-location `LostFocus` -> `Helper_ScannerLocationFormat.Sanitize`.

## Database

- Keep: `receiving_scanner_session`, `receiving_scanner_item`, `receiving_scanner_profile`.
- Drop: `receiving_scanner_run`, `receiving_scanner_run_item`, run SPs, scanner functions/views.
- Migration `14_Migration_receiving_scanner_redesign.sql` drops legacy objects and removes
  existing Draft sessions.

## Validation

- Build: `dotnet build MTM_Receiving_Application.csproj -p:Platform=x64`
- Tests: `dotnet test MTM_Receiving_Application.Tests/... --filter FullyQualifiedName~Module_Scanner`

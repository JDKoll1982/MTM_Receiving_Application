<!--
  If no file was refences with this prompt you are to reference docs/Module_Bulk_Inventory/07_Tasks.md to understand the following:
  ╔══════════════════════════════════════════════════════════════════════════════╗
  ║  AI AGENT PROMPT — Module_Bulk_Inventory Implementation                    ║
  ╚══════════════════════════════════════════════════════════════════════════════╝

  ── PERSONAS ──────────────────────────────────────────────────────────────────

  You operate as a team of three specialists.  Each persona activates for the
  work that falls within their domain.  All three share one rule: never guess —
  read the file first.

  ┌─────────────────────────────────────────────────────────────────────────────┐
  │ PERSONA 1 — "THE ARCHITECT"                                                 │
  │ Activates for: Phase 0–4 (schema, Core services, navigation restructure)   │
  │                                                                             │
  │ You are a senior .NET software architect with 15 years on WinUI/WPF MVVM   │
  │ desktop apps and deep MySQL schema design experience.  You think in layers  │
  │ and dependency graphs before you touch a file.  You are obsessive about     │
  │ keeping Module_Core free of upward dependencies — it must never import from │
  │ Module_Receiving or any feature module.  When a task asks you to move a     │
  │ model or split a service, you check every referencing file and update all   │
  │ of them.  You write stored procedures that log to auth_activity_log because │
  │ you know auditing is not optional in a manufacturing environment.           │
  │                                                                             │
  │ Your instinct on every task: "What breaks if I do this wrong, and how do   │
  │ I prove it didn't break?"  You always run dotnet build before marking [x]. │
  └─────────────────────────────────────────────────────────────────────────────┘

  ┌─────────────────────────────────────────────────────────────────────────────┐
  │ PERSONA 2 — "THE AUTOMATION ENGINEER"                                       │
  │ Activates for: Phase 5–7 (DAOs, services, Visual UI automation logic)      │
  │                                                                             │
  │ You are a specialist in UI automation and ERP integration.  You have        │
  │ automated Infor Visual workflows using FlaUI before and you know exactly    │
  │ where it fails: timing, focus, spurious dialogs, and partial window state.  │
  │ You study 04_ResearchReferences.md and 05_LegacyPitfalls.md thoroughly     │
  │ before writing a single automation step.  You treat 06_WorkflowDiagram.md  │
  │ as law — every branch in the diagram becomes an explicit code path, never   │
  │ a fallthrough.                                                              │
  │                                                                             │
  │ You prefix every IService_LoggingUtility call in automation code with       │
  │ // [VISUAL_LOG] without being reminded.  You know why it exists.           │
  │                                                                             │
  │ DAOs are your foundation.  You return Model_Dao_Result from every method.  │
  │ You never throw.  You never write raw SQL — stored procedures only.        │
  │ SQL Server (Infor Visual) is read-only.  You never write to it.            │
  └─────────────────────────────────────────────────────────────────────────────┘

  ┌─────────────────────────────────────────────────────────────────────────────┐
  │ PERSONA 3 — "THE UI CRAFTSPERSON"                                           │
  │ Activates for: Phase 8–10 (Views, keyboard shortcuts, validation polish)   │
  │                                                                             │
  │ You are a WinUI 3 UI specialist who cares deeply about the experience of a  │
  │ warehouse worker using this app under time pressure.  You know that x:Bind  │
  │ is not optional — runtime Binding is forbidden in this codebase.  Every     │
  │ binding has an explicit Mode.  PasswordBox.Password is wired from           │
  │ code-behind because you know it is not a dependency property.              │
  │                                                                             │
  │ You build the full-screen push overlay so it is impossible to accidentally  │
  │ interact with the MTM app while Visual is being automated.  You wire F5,   │
  │ F6, and Escape exactly as 02_Suggestions.md specifies.  You add the legend │
  │ strip to every data-entry screen so users never have to hunt for shortcuts. │
  │                                                                             │
  │ You put zero business logic in code-behind.  If you find yourself writing  │
  │ more than an event forwarder in .xaml.cs, you stop and move the logic to   │
  │ the ViewModel.                                                              │
  └─────────────────────────────────────────────────────────────────────────────┘

  ── SHARED RULES (all three personas) ────────────────────────────────────────

  BEFORE WRITING ANY CODE:
  1. Read the Quick Reference table (immediately below) — know every file in play.
  2. Read the spec doc(s) listed for the task you are about to start.
  3. Read the existing files you will touch, as well as any dependencies of those
     files — never modify code you have not read.
  4. Confirm the task's dependencies (marked "depends on Txx") are already complete.

  WHILE WORKING:
  - Complete one task at a time.  Mark it [x] before moving to the next.
  - Follow all rules in `.github/copilot-instructions.md` (MVVM layer separation,
    x:Bind only, instance DAOs, stored procedures for MySQL, SQL Server read-only).
  - All async methods end with the Async suffix.
  - DAOs return Model_Dao_Result — never throw.
  - Visual automation log calls are preceded by // [VISUAL_LOG].

  AFTER EACH TASK:
  - Run `dotnet build` — zero errors before marking [x].
  - If a task introduces a new stored procedure, deploy it to
    mtm_receiving_application before implementing the DAO method that calls it.

  WHEN YOU ARE FINISHED:
  - All tasks marked [x].
  - Run the end-to-end checkpoint at the bottom of this file.
  - Update the Last Updated date on this file.
-->

# Tasks: Module_Bulk_Inventory

**Last Updated:** 2026-03-08 (rev 12 — T000–T028 complete; solution builds clean; T029–T032 not started)

---

## 📊 Current Status Report — 2026-03-08 (rev 12)

> **Solution builds clean.** All files for Phases 0–8 are on disk and compiling.
> Phase 9–10 tasks are ready to start.

### ✅ Complete and building (T000–T028)

All infrastructure, database, settings, model, DAO, service, ViewModel, View, and DI wiring tasks are done.
The solution builds with zero errors.

**Additional fixes applied in rev 12 (all pre-existing compile errors):**
- `ViewModel_BulkInventory_DataEntry.cs` — added `using Microsoft.UI.Xaml.Controls` (resolved `ContentDialogResult` and `InfoBarSeverity`), added `<param>` doc tags on `DeleteRowAsync`, `OpenPartSearchAsync`, `OpenLocationSearchAsync`, and added `HasInterruptedRows` stub property (T022)
- `ViewModel_BulkInventory_Push.cs` — added `<param name="rows">` doc tag on `StartPushAsync` (T023)
- `ViewModel_BulkInventory_Summary.cs` — added `<param name="completedRows">` doc tag on `LoadResults` (T024)
- `View_BulkInventory_Push.xaml` — Skip Row button now uses `Click="SkipRow_Click"` instead of wrong `StartPushCommand` binding (T026)
- `View_BulkInventory_Push.xaml.cs` — added `SkipRow_Click` handler (T026)
- `View_BulkInventory_Summary.xaml` — added `Page.Resources` with `CountToVisibilityConverter`, `InverseBoolConverter`, `InfoBarSeverityConverter`; fixed `IsStatusBarOpen` → `IsStatusOpen` (T027)
- `View_BulkInventory_DataEntry.xaml` — added `UserControl.Resources` with `InverseBoolConverter` and `DecimalToDoubleConverter`; removed invalid `{x:Bind ViewModel.CanPushBatch}` binding on Push Batch button (Command binding handles enable state) (T025/T028)
- `View_BulkInventory_Host.xaml.cs` — `ShowDataEntry()` now passes ViewModel via constructor `new View_BulkInventory_DataEntry(vm)` instead of object initializer (T028)
- `Module_Core/Converters/Converter_DecimalToDouble.cs` — new converter for `NumberBox.Value` ↔ `decimal` binding
- `Module_ShipRec_Tools/Views/View_Tool_OutsideServiceHistory.xaml` — removed unsupported `SortMemberPath` attributes; added `Tag` to all columns
- `Module_ShipRec_Tools/Views/View_Tool_OutsideServiceHistory.xaml.cs` — changed `e.Column.SortMemberPath` to `e.Column.Tag as string`; removed `e.Handled = true` (not available on `DataGridColumnEventArgs`)

### 🔲 Not started (T029–T032)

| Task | Summary |
|------|---------|
| T029 | Credential guard — bind `BulkInventoryNavItem.Visibility` to `VisualUsername` at startup/login |
| T030 | Crash recovery — reset stale `InProgress` rows to `Pending` on app startup |
| T031 | `ValidateAllCommand` implementation — Infor Visual PART + location LIKE queries |
| T032 | F5 / F6 / Escape keyboard shortcuts + legend strip |

---
**Input docs:** `docs/Module_Bulk_Inventory/` — all files 01 through 06
**Status key:** `[ ]` not started · `[~]` in progress · `[x]` complete

---

## Quick Reference — All Files in This Build

> **For AI agents:** Read this section before touching any task.  It maps every file involved
> in the Module_Bulk_Inventory build to its purpose and the tasks that own it.  Use it to
> orient yourself before reading a task, to avoid duplicate work, and to know which spec doc
> to consult when a task description refers to a requirement.

### Spec & Design Docs  *(read-only — never edit during implementation)*

| File | What it contains | When to open it |
|------|-----------------|-----------------|
| `docs/Module_Bulk_Inventory/01_Assumptions.md` | Locked decisions: DB schema shape, consolidation rule, credential guard rule, `V_` table note, warehouse/lot defaults | Any time a task says **"per assumptions"** or you need to verify a design constraint |
| `docs/Module_Bulk_Inventory/02_Suggestions.md` | UX details: validation inline display, overlay behaviour, WaitingForConfirmation state, cancel sequence, re-push skip-consolidation rule | T019, T020, T022, T023, T025, T026 |
| `docs/Module_Bulk_Inventory/03_PreRequisites.md` | Environment requirements: Visual exe path, warehouse/lot settings, credential guard `SHOP2`/`MTMDC` check | T000 series, T002, T003, T009, T029 |
| `docs/Module_Bulk_Inventory/04_ResearchReferences.md` | Legacy app research: Visual window names, field tab-order, transaction type determination from WO field | T010, T011, T019, T020 |
| `docs/Module_Bulk_Inventory/05_LegacyPitfalls.md` | Known failure modes from the old WinForms app and how to avoid them | T019, T020, T031 |
| `docs/Module_Bulk_Inventory/06_WorkflowDiagram.md` | Mermaid flowcharts for all Visual automation paths (Transfer, New Transaction, cancel, conflict, re-check) | T019, T020, T023, T026 — primary reference for automation branching logic |
| `docs/Module_Bulk_Inventory/08_UserGuide.md` | End-user guide — also defines credential-setup UX for §2 and §11 | T000 series, T029 |

---

### Database Files

#### Auth stored procedures — Phase 0 prerequisites (all new)

| File | Purpose | Task |
|------|---------|------|
| `Database/StoredProcedures/Auth/sp_Auth_User_GetAll.sql` | Return all `auth_users` rows ordered by `full_name` | T000 |
| `Database/StoredProcedures/Auth/sp_Auth_User_Update.sql` | Full user record update + audit log row | T000 |
| `Database/StoredProcedures/Auth/sp_Auth_User_UpdateVisualCredentials.sql` | Update Visual credential fields only + audit log row | T000 |
| `Database/StoredProcedures/Auth/sp_Auth_User_Deactivate.sql` | Soft-delete — sets `is_active = 0` | T000 |

#### Bulk Inventory schema & stored procedures (all new)

| File | Purpose | Task |
|------|---------|------|
| `Database/Schemas/bulk_inventory_transactions.sql` | `bulk_inventory_transactions` table DDL | T004 |
| `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_Insert.sql` | Insert a new transaction row | T005 |
| `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_UpdateStatus.sql` | Update `status`, `error_message`, `completed_at` on a row | T006 |
| `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_GetByUser.sql` | Return all rows for a given `user_id` | T007 |
| `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_DeleteById.sql` | Hard-delete a row by `id` (used by Clear All) | T008 |

---

### Module_Core — Modified existing files

| File | What changes | Task(s) |
|------|-------------|---------|
| `Module_Core/Data/Authentication/Dao_User.cs` | Add `GetAllAsync`, `UpdateAsync`, `UpdateVisualCredentialsAsync`, `DeactivateAsync` | T000a |
| `Module_Core/Models/InforVisual/Model_InforVisualPO.cs` | **Moved here** from `Module_Receiving/Models/` | T001 |
| `Module_Core/Models/InforVisual/Model_InforVisualPart.cs` | **Moved here** from `Module_Receiving/Models/` | T001 |
| `Module_Core/Services/Database/Service_InforVisualConnect.cs` | Remove `Module_Receiving` usings; add Part + Location validation methods | T001, T031 |

### Module_Core — New files

| File | Purpose | Task(s) |
|------|---------|---------|
| `Module_Core/Contracts/Services/IService_VisualCredentialValidator.cs` | Interface: `ValidateAsync(username, password)` → is it a blocked shared account? | T009 |
| `Module_Core/Services/IService_VisualCredentialValidator` impl | Checks `VisualUsername` against `SHOP2`/`MTMDC` blocklist | T009 |
| `Module_Core/Contracts/Services/IService_UIAutomation.cs` | Interface for FlaUI/UI Automation operations (FindWindow, SendKeys, etc.) | T010 |
| `Module_Core/Services/Service_UIAutomation.cs` | Concrete FlaUI implementation registered as Singleton | T011 |

---

### Module_Settings.Core — Modified existing files

| File | What changes | Task(s) |
|------|-------------|---------|
| `Module_Settings.Core/Defaults/settings.manifest.json` | Add `VisualExePath`, `DefaultWarehouseCode`, `DefaultLotNo` entries | T002 |
| `Module_Settings.Core/Views/View_Settings_SharedPaths.xaml` | Add TextBox controls for Visual path, warehouse code, lot no | T003 |
| `Module_Settings.Core/ViewModels/ViewModel_Settings_SharedPaths.cs` | Observable properties + save command for the three new fields | T003 |
| `Module_Settings.Core/ViewModels/ViewModel_Settings_Users.cs` | **Replace stub** — full user list + add/edit/deactivate ViewModel | T000b |
| `Module_Settings.Core/Views/View_Settings_Users.xaml` | **Replace stub** — full two-column user management UI | T000c |
| `Module_Settings.Core/Views/View_Settings_Users.xaml.cs` | Add `PasswordBox.PasswordChanged` code-behind handler | T000c |

---

### Module_Receiving — Modified existing files

| File | What changes | Task(s) |
|------|-------------|---------|
| `Module_Receiving/Data/` *(all Infor Visual DAOs)* | Update `using` statements after model move | T001 |
| `Module_Receiving/Models/Model_InforVisualPO.cs` | **Moved to** `Module_Core/Models/InforVisual/` | T001 |
| `Module_Receiving/Models/Model_InforVisualPart.cs` | **Moved to** `Module_Core/Models/InforVisual/` | T001 |

---

### Module_Bulk_Inventory — All new files

#### Enums & Models

| File | Purpose | Task |
|------|---------|------|
| `Module_Bulk_Inventory/Enums/Enum_BulkInventoryStatus.cs` | `Pending`, `InProgress`, `WaitingForConfirmation`, `Success`, `Failed`, `Skipped`, `Consolidated` | T015 |
| `Module_Bulk_Inventory/Models/Model_BulkInventoryTransaction.cs` | Row model: PartId, FromLocation, ToLocation, Qty, WorkOrder, Status, ErrorMessage, etc. | T015 |

#### Data

| File | Purpose | Task |
|------|---------|------|
| `Module_Bulk_Inventory/Data/Dao_BulkInventoryTransaction.cs` | CRUD for `bulk_inventory_transactions` via stored procedures | T016 |

#### Services

| File | Purpose | Task |
|------|---------|------|
| `Module_Bulk_Inventory/Contracts/Services/IService_MySQL_BulkInventory.cs` | MySQL service interface (insert, update status, get by user, delete) | T017 |
| `Module_Bulk_Inventory/Services/Service_MySQL_BulkInventory.cs` | MySQL service implementation wrapping `Dao_BulkInventoryTransaction` | T018 |
| `Module_Bulk_Inventory/Contracts/Services/IService_BulkInventory_FuzzySearch.cs` | Fuzzy search interface: `SearchPartsAsync` + `SearchLocationsAsync` against Infor Visual | T022 |
| `Module_Bulk_Inventory/Services/Service_BulkInventory_FuzzySearch.cs` | Implements `IService_BulkInventory_FuzzySearch` using read-only SQL Server LIKE queries | T022 |
| `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` | Visual UI automation: Transfer fill sequence (T019) + New Transaction fill sequence (T020) | T019, T020 |

#### ViewModels

| File | Purpose | Task |
|------|---------|------|
| `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_DataEntry.cs` | Grid rows, Add/Delete/ClearAll/ValidateAll/PushBatch commands, interrupted-batch banner | T022 |
| `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_Push.cs` | Overlay state, row-by-row progress, Cancel command, WaitingForConfirmation transitions | T023 |
| `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_Summary.cs` | Success/Failed/Skipped counts, re-push selection, Done command | T024 |

#### Views

| File | Purpose | Task |
|------|---------|------|
| `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml` | Grid with Add/Delete/ClearAll/ValidateAll/PushBatch toolbar; legend strip | T025 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml.cs` | Code-behind (minimal — wires keyboard shortcuts) | T025 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_Push.xaml` | Full-screen dark overlay, progress message, Cancel button | T026 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_Push.xaml.cs` | F6 / Escape shortcut handlers | T026 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_Summary.xaml` | Badge counts, failed-row checklist, Re-push Selected + Done buttons | T027 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_Summary.xaml.cs` | Code-behind (minimal) | T027 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_Host.xaml` | Page container — hosts DataEntry, Push overlay, and Summary as child frames | T028 |
| `Module_Bulk_Inventory/Views/View_BulkInventory_Host.xaml.cs` | Navigation between child views | T028 |

---

### Infrastructure & App Shell — Modified existing files

| File | What changes | Task(s) |
|------|-------------|---------|
| `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` | Add `AddBulkInventoryModule()` extension method; register all Bulk Inventory DAOs, services, ViewModels | T014, T021 |
| `MainWindow.xaml` | Restructure `NavigationView` to add `BulkInventoryNavItem` with visibility binding | T012 |
| `MainWindow.xaml.cs` | Add `BulkInventoryPage` case to `NavView_SelectionChanged`; wire credential guard to nav item visibility at startup/login | T013, T029 |

---

> ## ⚠️ Visual Automation Logging Convention
>
> Every `IService_LoggingUtility` call inside a Visual automation method **must** be preceded by
> the single-line comment `// [VISUAL_LOG]` on the line directly above it.
>
> ```csharp
> // [VISUAL_LOG]
> _logger.LogInformation("Parts popup dismissed for part {PartId}", partId);
> // [END_VISUAL_LOG]
> ```
>
> This marker exists so a future script can locate and strip all Visual automation logging in one
> pass without touching other log statements.  No other code may use the `[VISUAL_LOG]` tag.

---

## Phase 0 — Prerequisite: User Management UI (`View_Settings_Users.xaml`)

`View_Settings_Users.xaml` currently exists as a placeholder stub.  The Bulk Inventory
credential guard reads `VisualUsername` from the logged-in user's record — but there is
presently **no UI to set or edit those credentials**.  These tasks must be complete before
any Bulk Inventory credential-guard work (T009, T029) can be tested end-to-end.

T000 and T000a are independent; T000b depends on T000a; T000c depends on T000b.

---

- [x] **T000 [P] — Create four missing Auth stored procedures**

  > **References:** `Module_Core/Data/Authentication/Dao_User.cs` — existing SP naming pattern

  The following stored procedures are called by new DAO methods added in T000a.  Follow the
  header-comment and `DELIMITER` convention used in all other files under
  `Database/StoredProcedures/`.

  | File | Purpose | Key parameters |
  |------|---------|----------------|
  | `sp_Auth_User_GetAll.sql` | Return all rows from `auth_users` ordered by `full_name` | none |
  | `sp_Auth_User_Update.sql` | Full user record update (all editable fields) | `p_employee_number`, `p_full_name`, `p_pin`, `p_department`, `p_shift`, `p_is_active`, `p_visual_username`, `p_visual_password`, `p_updated_by` |
  | `sp_Auth_User_UpdateVisualCredentials.sql` | Update only the Visual credential fields | `p_employee_number`, `p_visual_username`, `p_visual_password`, `p_updated_by` |
  | `sp_Auth_User_Deactivate.sql` | Soft-delete — sets `is_active = 0` | `p_employee_number`, `p_updated_by` |

  `sp_Auth_User_Update` and `sp_Auth_User_UpdateVisualCredentials` must write a row to
  `auth_activity_log` on success (same pattern as `sp_Auth_User_Create`).

  Deploy to `mtm_receiving_application` before T000a is implemented.

  **Files touched:**
  - `Database/StoredProcedures/Auth/sp_Auth_User_GetAll.sql` ← new
  - `Database/StoredProcedures/Auth/sp_Auth_User_Update.sql` ← new
  - `Database/StoredProcedures/Auth/sp_Auth_User_UpdateVisualCredentials.sql` ← new
  - `Database/StoredProcedures/Auth/sp_Auth_User_Deactivate.sql` ← new

---

- [x] **T000a [P] — Extend `Dao_User` with four new methods**

  > **References:** `Module_Core/Data/Authentication/Dao_User.cs` existing pattern ·
  > `Module_Core/Models/Systems/Model_User.cs` (`VisualUsername`, `VisualPassword` fields)

  *Depends on T000 (SPs deployed).*

  Add the following four methods to `Module_Core/Data/Authentication/Dao_User.cs`.
  Follow the exact pattern of the existing methods in that file — use
  `Helper_Database_StoredProcedure` for all calls; return `Model_Dao_Result` or
  `Model_Dao_Result<T>`; never throw.

  ```csharp
  // Returns all users — used by the Settings ▸ Users list
  public virtual Task<Model_Dao_Result<List<Model_User>>> GetAllAsync()

  // Full record update — used by the edit form Save button
  public virtual Task<Model_Dao_Result> UpdateAsync(Model_User user, string updatedBy)

  // Credential-only update — used by the inline Visual credentials section
  public virtual Task<Model_Dao_Result> UpdateVisualCredentialsAsync(
      int employeeNumber, string? visualUsername, string? visualPassword, string updatedBy)

  // Soft-delete (sets is_active = false) — used by the Deactivate button
  public virtual Task<Model_Dao_Result> DeactivateAsync(int employeeNumber, string updatedBy)
  ```

  `UpdateAsync` maps `Model_User` fields to the `@p_*` parameters defined in
  `sp_Auth_User_Update`.

  **Files touched:**
  - `Module_Core/Data/Authentication/Dao_User.cs`

---

- [x] **T000b — Build `ViewModel_Settings_Users` — user list + add/edit/deactivate**

  > **References:** `Module_Settings.Core/ViewModels/ViewModel_Settings_Users.cs` (stub to replace) ·
  > `Module_Settings.Core/ViewModels/ViewModel_Settings_Database.cs` (pattern to follow) ·
  > `Module_Core/Services/Authentication/Service_Authentication.cs` (`CreateNewUserAsync` usage)

  *Depends on T000a.*

  Replace the stub body of `ViewModel_Settings_Users` with a full implementation.
  The class remains `partial`, inherits `ViewModel_Shared_Base`, and is registered as
  `Transient` in DI (it already is — do not change the DI registration).

  Constructor injects: `Dao_User`, `IService_ErrorHandler`, `IService_LoggingUtility`,
  `IService_Notification`, `IService_UserSessionManager`.

  **Observable properties:**
  - `ObservableCollection<Model_User> Users`
  - `Model_User? SelectedUser` — the row currently selected in the list
  - `Model_User? EditingUser` — a clone of `SelectedUser` bound to the edit form; `null` when no
    form is open
  - `bool IsEditing` — `true` when edit form is visible
  - `bool IsAddingNew` — `true` when form is in add-new mode (vs edit mode)
  - `string VisualPasswordMask` — always `"••••••••"` when `EditingUser.VisualPassword` is not
    null/empty; empty string otherwise.  Never expose the real password in a bindable string.

  **Commands (all `[RelayCommand]`):**

  | Command | Action |
  |---------|--------|
  | `LoadUsersCommand` | Calls `GetAllAsync`, populates `Users`, clears editing state |
  | `AddNewUserCommand` | Sets `EditingUser = new Model_User()`, `IsAddingNew = true`, `IsEditing = true` |
  | `EditUserCommand(Model_User user)` | Clones the row into `EditingUser`, `IsAddingNew = false`, `IsEditing = true` |
  | `SaveUserCommand` | If `IsAddingNew`: calls `Dao_User.CreateNewUserAsync`; else: calls `UpdateAsync`. Validates required fields inline before calling DAO. On success: reloads list, closes form. |
  | `SaveVisualCredentialsCommand` | Calls `UpdateVisualCredentialsAsync` using `EditingUser.EmployeeNumber` and the values from the credential fields. Shows success notification on `IService_Notification`. |
  | `DeactivateUserCommand(Model_User user)` | Shows confirmation dialog (`IService_Notification.ConfirmAsync`). On confirm: calls `DeactivateAsync`, reloads list. Cannot deactivate the currently logged-in user (`IService_UserSessionManager.CurrentSession.User.EmployeeNumber`). |
  | `CancelEditCommand` | Sets `EditingUser = null`, `IsEditing = false` |

  **Visual password field rule:** The actual password value is held in `EditingUser.VisualPassword`
  (set when the user types in the password `PasswordBox`).  The `PasswordBox` value is wired from
  code-behind via the `PasswordChanged` event — not via `x:Bind` (WinUI 3 `PasswordBox.Password`
  is not a dependency property).  See T000c for the code-behind pattern.

  **Files touched:**
  - `Module_Settings.Core/ViewModels/ViewModel_Settings_Users.cs`

---

- [x] **T000c — Build `View_Settings_Users.xaml` — full user management UI**

  > **References:** `Module_Settings.Core/Views/View_Settings_SharedPaths.xaml` (layout pattern) ·
  > `Module_Settings.Core/Views/View_Settings_Users.xaml` (stub to replace)

  *Depends on T000b.*

  Replace the stub `View_Settings_Users.xaml` with a full implementation.
  All bindings use `x:Bind` with explicit `Mode` except the `PasswordBox` (see below).

  **Layout — two-column `Grid`:**

  *Left column — user list:*
  - `ListView` bound `OneWay` to `ViewModel.Users`; `SelectedItem` bound `TwoWay` to
    `ViewModel.SelectedUser`.
  - Each row shows `FullName`, `Department`, `Shift`, a green/grey active badge, and a
    small `FontIcon` lock if `HasErpAccess == false`.
  - Toolbar above the list: **Add User** button (`ViewModel.AddNewUserCommand`),
    **Edit** button (enabled when `SelectedUser != null`, fires `ViewModel.EditUserCommand(SelectedUser)`),
    **Deactivate** button (enabled when `SelectedUser != null && SelectedUser.IsActive`,
    fires `ViewModel.DeactivateUserCommand(SelectedUser)`).

  *Right column — edit form (`Visibility` bound to `ViewModel.IsEditing`):*

  Core fields section (sub-heading "User Details"):
  - `TextBox` for `FullName` (TwoWay)
  - `TextBox` for `EmployeeNumber` — read-only when `IsAddingNew == false`
  - `TextBox` for `WindowsUsername`
  - `TextBox` for `Pin` (4 digits, `MaxLength=4`)
  - `ComboBox` for `Department` (items loaded from `Dao_User.GetActiveDepartmentsAsync`)
  - `ComboBox` for `Shift` (items: `1st Shift`, `2nd Shift`, `3rd Shift`)
  - `ToggleSwitch` for `IsActive` — hidden when `IsAddingNew == true`

  Visual credentials section (sub-heading "Infor Visual Credentials"):
  - `InfoBar` with `Severity=Warning` and message
    *"Users with credentials SHOP2 or MTMDC cannot access Bulk Inventory."*
    `IsOpen` bound `OneWay` to a helper property `IsBlockedVisualAccount` on the ViewModel
    (returns `true` when `EditingUser.VisualUsername` uppercased equals `SHOP2` or `MTMDC`).
  - `TextBox` for `VisualUsername` (TwoWay).
  - `PasswordBox` for Visual password — **wired via code-behind** `PasswordChanged` event:
  
    ```csharp
    // View_Settings_Users.xaml.cs
    private void VisualPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel.EditingUser is not null)
            ViewModel.EditingUser.VisualPassword = VisualPasswordBox.Password;
    }
    ```

  - `TextBlock` showing `ViewModel.VisualPasswordMask` (read-only masked indicator).
  - **Save Visual Credentials** button — `Command="{x:Bind ViewModel.SaveVisualCredentialsCommand}"`;
    disabled when `VisualUsername` is empty.

  Form footer:
  - **Save** button — `Command="{x:Bind ViewModel.SaveUserCommand}"`.
  - **Cancel** button — `Command="{x:Bind ViewModel.CancelEditCommand}"`.

  **Files touched:**
  - `Module_Settings.Core/Views/View_Settings_Users.xaml`
  - `Module_Settings.Core/Views/View_Settings_Users.xaml.cs`

---

## Phase 1 — Setup: Cross-Cutting Fixes (no module code yet)

These tasks fix existing issues in unrelated areas that `Module_Bulk_Inventory` will depend on.
They have no dependencies on each other and can be done in any order.

---

- [x] **T001 [P] — Fix cross-module model references in `Service_InforVisualConnect.cs`**

  > **References:** `03_PreRequisites.md §P1.4`

  `Module_Core/Services/Database/Service_InforVisualConnect.cs` currently imports models from
  `Module_Receiving`, which inverts the layer dependency (`Module_Core` → `Module_Receiving`).

  1. Move `Model_InforVisualPO` and `Model_InforVisualPart` from `Module_Receiving/Models/` to
     `Module_Core/Models/InforVisual/`.
  2. Remove any `using` aliases or `extern alias` hacks in `Service_InforVisualConnect.cs` that
     were added to paper over the circular reference.
  3. Update all `using` statements in `Module_Receiving/Data/` Infor Visual DAOs to point at the
     new `Module_Core/Models/InforVisual/` location.
  4. Build the solution — no errors or warnings related to these files should remain.

  **Files touched:**
  - `Module_Core/Models/InforVisual/Model_InforVisualPO.cs` ← moved here
  - `Module_Core/Models/InforVisual/Model_InforVisualPart.cs` ← moved here
  - `Module_Core/Services/Database/Service_InforVisualConnect.cs`
  - `Module_Receiving/Data/` (any file importing `Model_InforVisualPO` / `Model_InforVisualPart`)

---

- [x] **T002 [P] — Add Visual exe path, default warehouse code, and default lot no to settings manifest**

  > **References:** `03_PreRequisites.md §P1.5` · `01_Assumptions.md §7.1 Correction`

  Add the following three entries to
  `Module_Settings.Core/Defaults/settings.manifest.json` (follow the exact JSON shape already
  used in that file for all other entries):

  | key | category | defaultValue |
  |-----|----------|--------------|
  | `Core.SharedPaths.InforVisualExePath` | `System` | `\\\\visual\\visual908$\\VMFG` |
  | `BulkInventory.Defaults.WarehouseCode` | `BulkInventory` | `002` |
  | `BulkInventory.Defaults.LotNo` | `BulkInventory` | `1` |

  Full JSON shapes are quoted verbatim in `03_PreRequisites.md §P1.5` — copy them exactly.
  `dataType`, `scope`, `permissionLevel`, `isSensitive`, and `validationRules` values are
  specified there as well.

  **Files touched:**
  - `Module_Settings.Core/Defaults/settings.manifest.json`

---

- [x] **T003 — Extend `View_Settings_SharedPaths` with Visual path, warehouse code, and lot no controls**

  > **References:** `03_PreRequisites.md §P1.5` · `01_Assumptions.md §2.5 Correction` ·
  > `01_Assumptions.md §7.1 Correction`

  *Depends on T002 (settings manifest entries must exist first).*

  Study the existing pattern in `Module_Settings.Core/Views/View_Settings_SharedPaths.xaml` and
  its paired ViewModel before changing anything.

  1. Add a text-input row for **Infor Visual Executable Root Path** key
     `Core.SharedPaths.InforVisualExePath`.
  2. Add a short text-input row for **Default Warehouse Code**
     key `BulkInventory.Defaults.WarehouseCode`.
  3. Add a short text-input row for **Default Lot Number**
     key `BulkInventory.Defaults.LotNo`.
  4. In `ViewModel_Settings_SharedPaths.cs` add `[ObservableProperty]` fields for each new
     setting.  Load them from `Dao_SettingsCoreSystem.GetByKeyAsync(category, key)` in the
     existing `LoadAsync` method and save them back in the existing `SaveAsync` method.
  5. Use `x:Bind` with `Mode=TwoWay` for all new XAML bindings.

  **Files touched:**
  - `Module_Settings.Core/Views/View_Settings_SharedPaths.xaml`
  - `Module_Settings.Core/ViewModels/ViewModel_Settings_SharedPaths.cs`

---

## Phase 2 — Foundational: Database

Complete the schema before writing any DAO code.  T004 must finish before T005–T008 are deployed;
T005–T008 themselves can be written in parallel.

---

- [x] **T004 — Create `bulk_inventory_transactions` MySQL table schema**

  > **References:** `03_PreRequisites.md §P1.3` · `01_Assumptions.md §5.1–5.3`

  Create `Database/Schemas/bulk_inventory_transactions.sql` containing the exact `CREATE TABLE`
  statement quoted in `03_PreRequisites.md §P1.3`.  Verify the `ENUM` values match the full
  status set from the workflow diagrams:
  `Pending`, `InProgress`, `WaitingForConfirmation`, `Success`, `Failed`, `Skipped`, `Consolidated`.

  Deploy to `mtm_receiving_application` before proceeding to T005–T008.

  **Files touched:**
  - `Database/Schemas/bulk_inventory_transactions.sql` ← new

---

- [x] **T005 [P] — Create `sp_BulkInventory_Transaction_Insert.sql`**

  > **References:** `03_PreRequisites.md §P2.1` · `01_Assumptions.md §5.2`

  *Depends on T004.*

  Create `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_Insert.sql`.
  Parameters: `p_created_by_user`, `p_transaction_type`, `p_part_id`, `p_from_warehouse`,
  `p_from_location`, `p_to_warehouse`, `p_to_location`, `p_quantity`, `p_work_order`,
  `p_lot_no`, `p_visual_username`.
  Returns the new `id` via `SELECT LAST_INSERT_ID()`.
  Follow the header comment and `DELIMITER` pattern used in existing SPs under
  `Database/StoredProcedures/`.

  **Files touched:**
  - `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_Insert.sql` ← new

---

- [x] **T006 [P] — Create `sp_BulkInventory_Transaction_UpdateStatus.sql`**

  > **References:** `03_PreRequisites.md §P2.1` · `06_WorkflowDiagram.md — Diagram B UPDS/UPDF nodes`

  *Depends on T004.*

  Create `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_UpdateStatus.sql`.
  Parameters: `p_id INT UNSIGNED`, `p_status ENUM(...)`, `p_error_message TEXT`.
  Updates `status`, `error_message`, and `updated_at` for the given row.

  **Files touched:**
  - `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_UpdateStatus.sql` ← new

---

- [x] **T007 [P] — Create `sp_BulkInventory_Transaction_GetByUser.sql`**

  > **References:** `03_PreRequisites.md §P2.1` · `06_WorkflowDiagram.md — Diagram A §0 crash recovery`

  *Depends on T004.*

  Create `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_GetByUser.sql`.
  Parameters: `p_username VARCHAR(100)`, optional `p_status ENUM(...)` (NULL = all statuses).
  Returns all columns ordered by `created_at DESC`.
  This SP is used both for loading a user's current pending batch and for the startup crash-recovery
  check (query rows where `status = 'InProgress'` and `updated_at < NOW() - INTERVAL 5 MINUTE`).

  **Files touched:**
  - `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_GetByUser.sql` ← new

---

- [x] **T008 [P] — Create `sp_BulkInventory_Transaction_DeleteById.sql`**

  > **References:** `03_PreRequisites.md §P2.1` · `02_Suggestions.md §2.1`

  *Depends on T004.*

  Create `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_DeleteById.sql`.
  Parameters: `p_id INT UNSIGNED`.
  Hard deletes the row.  Used when the user removes a row from the grid during data entry
  (delete is not available during an active push — the overlay blocks it).

  **Files touched:**
  - `Database/StoredProcedures/BulkInventory/sp_BulkInventory_Transaction_DeleteById.sql` ← new

---

## Phase 3 — Foundational: Module_Core Services

These service interfaces and implementations live in `Module_Core` so any future module requiring
Visual automation can reuse them.  T009 and T010 are independent and can be written in parallel;
T011 depends on T010.

---

- [x] **T009 [P] — Create `IService_VisualCredentialValidator` and its implementation**

  > **References:** `03_PreRequisites.md §P1.1` · `01_Assumptions.md §2.2–2.3` ·
  > `02_Suggestions.md §1.3` · `06_WorkflowDiagram.md — Diagram A §CG node`

  **Interface** — create `Module_Core/Contracts/Services/IService_VisualCredentialValidator.cs`:

  ```csharp
  public interface IService_VisualCredentialValidator
  {
      bool IsAllowed(string visualUserName);
      string? GetBlockedReason(string visualUserName);
  }
  ```

  **Implementation** — create `Module_Core/Services/Database/Service_VisualCredentialValidator.cs`:
  - Blocked usernames (case-insensitive): `SHOP2`, `MTMDC`.
  - `IsAllowed` returns `false` if the trimmed, uppercased username matches either blocked value.
  - `GetBlockedReason` returns a user-facing message when blocked, `null` when allowed.
  - No external dependencies — pure logic, no async needed.

  **Registration** — add to `Module_Core`'s `CoreServiceExtensions.cs` (or equivalent) as
  `Singleton`:

  ```csharp
  services.AddSingleton<IService_VisualCredentialValidator, Service_VisualCredentialValidator>();
  ```

  **Files touched:**
  - `Module_Core/Contracts/Services/IService_VisualCredentialValidator.cs` ← new
  - `Module_Core/Services/Database/Service_VisualCredentialValidator.cs` ← new
  - `Module_Core/` DI registration file

---

- [x] **T010 [P] — Create `IService_UIAutomation` interface in Module_Core**

  > **References:** `03_PreRequisites.md §P1.2` · `02_Suggestions.md §1.2` ·
  > `05_LegacyPitfalls.md PF-01, PF-03, PF-04, PF-07` ·
  > `04_ResearchReferences.md §1.4–1.5`

  Create `Module_Core/Contracts/Services/IService_UIAutomation.cs`.

  The interface is **entirely window-agnostic** — it must not reference Infor Visual, VMINVENT,
  or any specific application.  The full minimum surface is quoted verbatim in
  `03_PreRequisites.md §P1.2`.  Copy it exactly; do not omit any method.

  Key design constraints (each named after the pitfall it prevents):
  - All window-wait methods use polling with `Task.Delay` + `CancellationToken` — never `Thread.Sleep` (PF-03).
  - `WaitForPopupAsync` returns `IntPtr` (`IntPtr.Zero` = not found) — caller decides what to do (PF-04).
  - `SetForegroundVerified` returns `bool` — caller checks before sending keys (PF-07).

  **Files touched:**
  - `Module_Core/Contracts/Services/IService_UIAutomation.cs` ← new

---

- [x] **T011 — Create `Service_UIAutomation` implementation and register both services**

  > **References:** `03_PreRequisites.md §P1.2` · `05_LegacyPitfalls.md PF-03, PF-04, PF-07, PF-09` ·
  > `02_Suggestions.md §1.1`

  *Depends on T010.*

  Create `Module_Core/Services/VisualAutomation/Service_UIAutomation.cs` implementing
  `IService_UIAutomation`.

  Implementation rules (each enforced by a pitfall):
  - **PF-03** — every `await` delay uses `Task.Delay(ms, ct)`, never `Thread.Sleep`.
  - **PF-04** — polling loops always include `ct.ThrowIfCancellationRequested()` and
    `await Task.Delay(pollMs, ct)`.
  - **PF-07** — `SetForegroundVerified` calls `SetForegroundWindow`, then immediately checks
    `GetForegroundWindow() == hwnd`; returns `false` if focus was not acquired.
  - **PF-09** — `FillFieldAsync` checks `element.Current.IsEnabled &&
    element.Current.IsKeyboardFocusable` before sending keystrokes; throws
    `InvalidOperationException` with the `automationId` if the field is not ready.
  - **PF-08** — `SendKeys` is a thin wrapper around `System.Windows.Forms.SendKeys.Send`;
    credentials must never be passed through it — only non-sensitive field values.

  Register in `Module_Core`'s DI extension as Singleton:

  ```csharp
  services.AddSingleton<IService_UIAutomation, Service_UIAutomation>();
  ```

  **Files touched:**
  - `Module_Core/Services/VisualAutomation/Service_UIAutomation.cs` ← new
  - `Module_Core/` DI registration file

---

## Phase 4 — Foundational: Navigation Restructure

Must be complete before the Bulk Inventory nav item can be wired to the credential guard.

---

- [x] **T012 [P] — Restructure `MainWindow.xaml` NavigationView**

  > **References:** `03_PreRequisites.md §P2.3` · `04_ResearchReferences.md §2.2`

  Study `MainWindow.xaml` fully before editing.

  1. Remove `End of Day Reports` and `Ship/Rec Tools` from `NavigationView.MenuItems`.
  2. Add a `NavigationView.FooterMenuItems` block (XAML quoted in `03_PreRequisites.md §P2.3`)
     containing in order:
     - `BulkInventoryNavItem` — `Tag="BulkInventoryPage"`, `Visibility="Collapsed"`
     - `End of Day Reports` — `Tag="ReportingMainPage"`
     - `Ship/Rec Tools` — `Tag="ShipRecToolsPage"`
  3. The `MenuItems` final order must be: Receiving Labels, Dunnage Labels, Volvo Dunnage Requisition.
  4. Do not change any existing `Tag` strings — routing `switch` cases depend on them.

  **Files touched:**
  - `MainWindow.xaml`

---

- [x] **T013 — Add `BulkInventoryPage` case to `NavView_SelectionChanged` in `MainWindow.xaml.cs`**

  > **References:** `03_PreRequisites.md §P2.4` · `04_ResearchReferences.md §2.2`

  *Depends on T012.*

  1. Add `case "BulkInventoryPage":` to the navigation switch statement; navigate to
     `View_BulkInventory_Host` (created in T028).
  2. If the switch is already large (8+ cases), extract it to a
     `Dictionary<string, Type>` keyed on `Tag` string and valued on `Page` type, per the
     suggestion in `03_PreRequisites.md §P2.4`.  This refactor must not change any existing
     routing behaviour.

  **Files touched:**
  - `MainWindow.xaml.cs`

---

- [x] **T014 — Add `Module_Bulk_Inventory` DI registration stub**

  > **References:** `03_PreRequisites.md §P2.2`

  *Depends on nothing; can be done at any time but must exist before T022.*

  In `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`:
  1. Add `services.AddBulkInventoryModule(configuration);` inside `AddModuleServices()`.
  2. Add the private extension method stub (body returns immediately with a `// TODO` comment)
     exactly as shown in `03_PreRequisites.md §P2.2`.

  **Files touched:**
  - `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

---

## Phase 5 — Data Layer: Module_Bulk_Inventory

---

- [x] **T015 [P] — Create `Model_BulkInventoryTransaction`**

  > **References:** `01_Assumptions.md §3.2, §5.1` · `02_Suggestions.md §2.2` ·
  > `06_WorkflowDiagram.md — Diagram B row status notes`

  Create `Module_Bulk_Inventory/Models/Model_BulkInventoryTransaction.cs`.

  Properties must map 1-to-1 with the `bulk_inventory_transactions` table (T004).
  Include:
  - `Id` (`int`)
  - `CreatedByUser`, `VisualUsername` (`string`)
  - `CreatedAt`, `UpdatedAt` (`DateTime`)
  - `TransactionType` (`Enum_BulkInventoryTransactionType` — values `Transfer`, `NewTransaction`)
  - `PartId`, `FromWarehouse`, `FromLocation`, `ToWarehouse`, `ToLocation` (`string`, nullable where column is nullable)
  - `Quantity` (`decimal`)
  - `WorkOrder`, `LotNo` (`string?`)
  - `Status` (`Enum_BulkInventoryStatus` — values `Pending`, `InProgress`, `WaitingForConfirmation`, `Success`, `Failed`, `Skipped`, `Consolidated`)
  - `ErrorMessage` (`string?`)

  Create the two enums in `Module_Bulk_Inventory/Enums/`:
  - `Enum_BulkInventoryTransactionType.cs`
  - `Enum_BulkInventoryStatus.cs`

  **Files touched:**
  - `Module_Bulk_Inventory/Models/Model_BulkInventoryTransaction.cs` ← new
  - `Module_Bulk_Inventory/Enums/Enum_BulkInventoryTransactionType.cs` ← new
  - `Module_Bulk_Inventory/Enums/Enum_BulkInventoryStatus.cs` ← new

---

- [x] **T016 — Create `Dao_BulkInventoryTransaction`**

  > **References:** `01_Assumptions.md §5.4` · `03_PreRequisites.md §P2.1` ·
  > `04_ResearchReferences.md §2.2 (Module_Receiving/Data/ pattern)`

  *Depends on T005–T008 (SPs deployed) and T015 (model exists).*

  Create `Module_Bulk_Inventory/Data/Dao_BulkInventoryTransaction.cs`.

  - Instance-based, not static.  Constructor accepts `string connectionString` with
    `ArgumentNullException.ThrowIfNull`.
  - Uses `Helper_Database_StoredProcedure` for all MySQL calls (same pattern as
    `Module_Receiving/Data/`).
  - Returns `Model_Dao_Result` or `Model_Dao_Result<T>` from every method — never throws.
  - Methods:
    - `InsertAsync(Model_BulkInventoryTransaction row)` → `Model_Dao_Result<int>` (returns new id)
    - `UpdateStatusAsync(int id, Enum_BulkInventoryStatus status, string? errorMessage)` → `Model_Dao_Result`
    - `GetByUserAsync(string username, Enum_BulkInventoryStatus? status = null)` → `Model_Dao_Result<List<Model_BulkInventoryTransaction>>`
    - `DeleteByIdAsync(int id)` → `Model_Dao_Result`

  **Files touched:**
  - `Module_Bulk_Inventory/Data/Dao_BulkInventoryTransaction.cs` ← new

---

## Phase 6 — Service Layer: Module_Bulk_Inventory

---

- [x] **T017 [P] — Create `IService_MySQL_BulkInventory` interface**

  > **References:** `01_Assumptions.md §5.4` · `06_WorkflowDiagram.md — Diagram B AUD/UPDS/UPDF nodes`

  Create `Module_Bulk_Inventory/Contracts/Services/IService_MySQL_BulkInventory.cs`.

  Methods mirror those in `Dao_BulkInventoryTransaction` (T016) but return business-layer
  result types.  Add `StartRowAsync` (inserts + returns id) and `CompleteRowAsync`
  (updates status to Success/Failed/Skipped) as the primary workflow entry points used by
  the automation loop.

  **Files touched:**
  - `Module_Bulk_Inventory/Contracts/Services/IService_MySQL_BulkInventory.cs` ← new

---

- [x] **T018 — Create `Service_MySQL_BulkInventory` implementation**

  > **References:** `01_Assumptions.md §5.4` · `06_WorkflowDiagram.md — Diagram B AUD/UPDS/UPDF`

  *Depends on T016 and T017.*

  Create `Module_Bulk_Inventory/Services/Service_MySQL_BulkInventory.cs` implementing
  `IService_MySQL_BulkInventory`.
  - Constructor injects `Dao_BulkInventoryTransaction` and `IService_LoggingUtility`.
  - Delegates directly to the DAO; adds logging for key state transitions (row started,
    row completed, error stored).

  **Files touched:**
  - `Module_Bulk_Inventory/Services/Service_MySQL_BulkInventory.cs` ← new

---

- [x] **T019 — Create `Service_VisualInventoryAutomation` — Transfer fill sequence**

  > **References:** `04_ResearchReferences.md §1.2, §1.4, §1.6` ·
  > `06_WorkflowDiagram.md — Diagram C (all nodes)` ·
  > `02_Suggestions.md §3.5` · `05_LegacyPitfalls.md PF-01, PF-05, PF-06, PF-07`

  *Depends on T011 (`IService_UIAutomation` implementation), T017, T018.*

  Create `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs`.
  - Constructor injects `IService_UIAutomation`, `IService_LoggingUtility`,
    and `IService_MySQL_BulkInventory`.

  Implement `ExecuteTransferAsync(Model_BulkInventoryTransaction row, CancellationToken ct)`:

  | Step | Action | Notes |
  |------|--------|-------|
  | 1 | `FillFieldAsync("4102", row.PartId)` | Part ID |
  | 2 | `await Task.Delay(1000, ct)` | Wait for Visual to process Part ID |
  | 3 | `WaitForPopupAsync("Gupta:AccFrame","Parts", 2s, 100ms, ct)` | Poll — do NOT use one-shot check (PF-07) |
  | 3a | If hwnd ≠ Zero: `SetForegroundVerified` → `{UP}{ENTER}` → `WaitForWindowToCloseAsync(3s)` | If focus fails → row Failed |
  | 4 | `FillFieldAsync("4111", row.Quantity)` | Quantity |
  | 5 | `FillFieldAsync("4123", warehouseCode)` | From Warehouse (read from settings) |
  | 6 | `FillFieldAsync("4124", row.FromLocation)` | From Location |
  | 7 | `FillFieldAsync("4142", warehouseCode)` | To Warehouse (read from settings) |
  | 8 | `FillFieldAsync("4143", row.ToLocation, sendTab: true)` | To Location + TAB triggers duplicate check |
  | 9 | `WaitForPopupAsync("Gupta:AccFrame","Inventory Transaction Entry", 2s, 100ms, ct)` | Duplicate warning poll |
  | 9a | If hwnd ≠ Zero: set row status `WaitingForConfirmation` → `WaitForWindowToCloseAsync(5min, 200ms, ct)` | User must dismiss; on timeout/cancel → Failed |

  All error paths (focus failed, popup did not close, timeout) must set row status to `Failed`
  and store a descriptive `ErrorMessage` — never throw an exception (PF-05).

  **Logging** — every meaningful automation event must be logged with `// [VISUAL_LOG]` above each call:

  ```csharp
  // [VISUAL_LOG]
  _logger.LogInformation("Transfer: Part ID {PartId} filled for row {RowId}", row.PartId, row.Id);
  // [END_VISUAL_LOG]

  // [VISUAL_LOG]
  _logger.LogInformation("Transfer: Parts popup dismissed for row {RowId}", row.Id);
  // [END_VISUAL_LOG]

  // [VISUAL_LOG]
  _logger.LogWarning("Transfer: WaitingForConfirmation — row {RowId} awaiting user input", row.Id);
  // [END_VISUAL_LOG]

  // [VISUAL_LOG]
  _logger.LogError("Transfer: row {RowId} Failed — {Reason}", row.Id, reason);
  // [END_VISUAL_LOG]
  ```

  **Files touched:**
  - `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` ← new

---

- [x] **T020 — Extend `Service_VisualInventoryAutomation` — New Transaction fill sequence**

  > **References:** `04_ResearchReferences.md §1.2, §1.5` ·
  > `06_WorkflowDiagram.md — Diagram D (all nodes)` ·
  > `01_Assumptions.md §3.5` · `05_LegacyPitfalls.md PF-03, PF-09`

  *Depends on T019.*

  Add `ExecuteNewTransactionAsync(Model_BulkInventoryTransaction row, CancellationToken ct)` to
  `Service_VisualInventoryAutomation`:

  | Step | Action | Notes |
  |------|--------|-------|
  | 1 | `FindWindow("Inventory Transaction Entry - Infor VISUAL - MTMFG")` | Locate window |
  | 2 | If `_appOpenedTransferWindow == true`: `SendMessage(transferHwnd, WM_CLOSE)` · set flag `false` | Close stale Transfer window |
  | 3 | `FillFieldAsync("4115", row.WorkOrder, sendTab: true)` | Format `WO-######` |
  | 4 | `WaitForPopupAsync("Gupta:AccFrame","Parts", 2s, 100ms, ct)` | Precautionary check |
  | 4a | If popup: `SetForegroundVerified` → `{UP}{ENTER}` → `WaitForWindowToCloseAsync(3s)` | |
  | 5 | `FillFieldAsync("4116", lotNo, sendTab: true)` | `lotNo` from settings default `"1"` |
  | 6 | `FillFieldAsync("4143", row.Quantity)` | ⚠️ Same AutomationId as Transfer To-Location — different window |
  | 7 | `FillFieldAsync("4148", warehouseCode)` | To Warehouse from settings |
  | 8 | `FillFieldAsync("4152", row.ToLocation, sendTab: true)` | To Location |
  | 9 | Row → `Success` | |

  Apply `// [VISUAL_LOG]` above every `_logger` call in this method — same convention as T019.

  Also add the `_appOpenedTransferWindow` bool field to the service class (set `true` after
  `LaunchVMINVENT()` succeeds, `false` when existing Visual session was reused).

  **Files touched:**
  - `Module_Bulk_Inventory/Services/Service_VisualInventoryAutomation.cs` ← extend

---

- [x] **T021 — Wire all Bulk Inventory services into DI (`AddBulkInventoryModule`)**

  > **References:** `03_PreRequisites.md §P2.2` · `01_Assumptions.md §5.3, §5.4`

  *Depends on T014 (stub exists), T016, T018, T019, T020.*

  Fill in the `AddBulkInventoryModule` stub in
  `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`:

  ```csharp
  var connectionString = Helper_Database_Variables.GetConnectionString();
  services.AddSingleton(sp => new Dao_BulkInventoryTransaction(connectionString));
  services.AddSingleton<IService_MySQL_BulkInventory, Service_MySQL_BulkInventory>();
  services.AddSingleton<Service_VisualInventoryAutomation>();
  services.AddTransient<ViewModel_BulkInventory_DataEntry>();
  services.AddTransient<ViewModel_BulkInventory_Push>();
  services.AddTransient<ViewModel_BulkInventory_Summary>();
  services.AddTransient<View_BulkInventory_Host>();
  ```

  ViewModels and Views are registered as Transient (new instance per navigation).
  DAOs and services are Singleton (stateless, reusable).

  **Files touched:**
  - `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

---

## Phase 7 — ViewModel Layer

---

- [x] **T022 [P] — Create `ViewModel_BulkInventory_DataEntry`**

  > **References:** `02_Suggestions.md §2.1, §2.1a, §2.2, §2.3, §2.4, §2.6` ·
  > `01_Assumptions.md §3.1–3.6` · `06_WorkflowDiagram.md — Diagram A §2` ·
  > `Module_Core/Dialogs/Dialog_FuzzySearchPicker.xaml.cs` (picker API)

  *Depends on T018 (service), T015 (model).*

  Create `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_DataEntry.cs` as a `partial`
  class inheriting `ViewModel_Shared_Base`.  Constructor injects `IService_MySQL_BulkInventory`,
  `IService_BulkInventory_FuzzySearch`, `IService_ErrorHandler`, `IService_LoggingUtility`.

  The view sets `ViewModel.XamlRoot` immediately after instantiation (needed to host
  `Dialog_FuzzySearchPicker`).  The ViewModel exposes `XamlRoot` as a settable property of
  type `XamlRoot?`.

  Observable properties:
  - `ObservableCollection<Model_BulkInventoryTransaction> Rows`
  - `bool HasValidationWarnings` (computed from `Rows`)
  - `bool CanPushBatch` → `!IsBusy && !HasValidationWarnings && Rows.Any()`
  - `XamlRoot? XamlRoot` — set by the View on load; required to host ContentDialogs

  Commands (all `[RelayCommand]`):
  - `AddRowCommand` — appends a blank `Pending` row, persists via `InsertAsync`
  - `DeleteRowCommand(Model_BulkInventoryTransaction row)` — calls `DeleteByIdAsync`, removes from collection
  - `ClearAllCommand` — deletes all rows (confirm before executing)
  - `ValidateAllCommand` — for each row queries Infor Visual SQL Server (`PART` table + location
    existence); surfaces warnings inline on the row's `ValidationMessage` property (no dialog).
    Updates `HasValidationWarnings` after completion.
  - `PushBatchCommand` — disabled while `HasValidationWarnings` is true; navigate to Push view
  - `OpenPartSearchCommand(Model_BulkInventoryTransaction row)` — calls
    `IService_BulkInventory_FuzzySearch.SearchPartsAsync(row.PartId)`, opens
    `Dialog_FuzzySearchPicker` (title: `"Select Part"`, subtitle: `"Matching parts for '{term}':"`);
    on `ContentDialogResult.Primary` writes `SelectedResult.Key` → `row.PartId`.
  - `OpenLocationSearchCommand((Model_BulkInventoryTransaction Row, string Field) args)` — calls
    `IService_BulkInventory_FuzzySearch.SearchLocationsAsync(term, warehouseCode)`, opens
    `Dialog_FuzzySearchPicker` (title: `"Select Location"`);
    on `ContentDialogResult.Primary` writes `SelectedResult.Key` → `row.FromLocation` or
    `row.ToLocation` depending on `args.Field` (`"FromLocation"` / `"ToLocation"`).

  **New service contract (also created in this task):**
  Create `Module_Bulk_Inventory/Contracts/Services/IService_BulkInventory_FuzzySearch.cs`:
  ```csharp
  public interface IService_BulkInventory_FuzzySearch
  {
      Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchPartsAsync(string term);
      Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchLocationsAsync(string term, string warehouseCode);
  }
  ```
  Implement in `Module_Bulk_Inventory/Services/Service_BulkInventory_FuzzySearch.cs`,
  injecting `IService_InforVisualConnect` (or an equivalent read-only SQL Server helper) for
  the underlying `PART` and `LOCATION` LIKE queries.

  **Files touched:**
  - `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_DataEntry.cs` ← new
  - `Module_Bulk_Inventory/Contracts/Services/IService_BulkInventory_FuzzySearch.cs` ← new
  - `Module_Bulk_Inventory/Services/Service_BulkInventory_FuzzySearch.cs` ← new

---

- [x] **T023 — Create `ViewModel_BulkInventory_Push`**

  > **References:** `02_Suggestions.md §3.4, §3.6` · `06_WorkflowDiagram.md — Diagram A §3,
  > Diagram B (all nodes)` · `01_Assumptions.md §1.3 Correction`

  *Depends on T019, T020 (automation service), T018 (MySQL service).*

  Create `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_Push.cs` as a `partial`
  class inheriting `ViewModel_Shared_Base`.  Constructor injects `Service_VisualInventoryAutomation`,
  `IService_MySQL_BulkInventory`, `IService_ErrorHandler`, `IService_LoggingUtility`.

  Key observable properties:
  - `bool IsAutomationRunning` — drives overlay visibility
  - `string OverlayStatusMessage` — status text shown on overlay (transitions per `06_WorkflowDiagram.md §3.6` table)
  - `int ProcessedCount`, `int TotalCount`

  Commands:
  - `StartPushCommand` — consolidates rows (group by `PartId + FromLocation + ToLocation`, sum qty,
    mark originals `Consolidated`), then starts the row loop
  - `CancelPushCommand` — calls `_cts.Cancel()`

  Row loop (runs in `Task.Run` to avoid blocking UI thread):
  1. For each consolidated row, `WriteAuditAsync(InProgress)` before any keystrokes.
  2. Check `CancellationToken`; if fired → mark current row `Skipped` → exit.
  3. Check F6 skip hotkey → mark `Skipped` → continue.
  4. Call `ExecuteTransferAsync` or `ExecuteNewTransactionAsync` based on `row.TransactionType`.
  5. On return, `UpdateStatusAsync(Success/Failed)`.
  6. After all rows complete → `IsAutomationRunning = false` → navigate to Summary view.

  **Files touched:**
  - `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_Push.cs` ← new

---

- [x] **T024 [P] — Create `ViewModel_BulkInventory_Summary`**

  > **References:** `02_Suggestions.md §2.5` · `06_WorkflowDiagram.md — Diagram B §5`

  *Depends on T018.*

  Create `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_Summary.cs`.

  Properties:
  - `int SuccessCount`, `int FailedCount`, `int SkippedCount`
  - `ObservableCollection<Model_BulkInventoryTransaction> FailedRows`

  Commands:
  - `RePushSelectedCommand` — re-triggers the automation loop for selected Failed rows only;
    **consolidation is skipped** — rows are pushed as-is in their current state.
  - `DoneCommand` — navigates back to Data Entry view and clears the grid.

  **Files touched:**
  - `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_Summary.cs` ← new

---

## Phase 8 — View Layer

---

- [x] **T025 [P] — Create `View_BulkInventory_DataEntry.xaml`**

  > **References:** `02_Suggestions.md §2.1, §2.4, §2.6` · `01_Assumptions.md §4.2` ·
  > `06_WorkflowDiagram.md — Diagram A §2 FSP sub-flow`

  *Depends on T022.*

  Create `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml`.
  - `DataGrid` bound to `ViewModel.Rows` with inline-editable columns: Part ID, From Location,
    To Location, Quantity, Work Order (optional), Status (read-only badge), Validation Message
    (read-only inline warning).
  - Toolbar: **Add Row**, **Delete Row**, **Clear All**, **Validate All**, **Push Batch** buttons.
  - `Push Batch` button `IsEnabled="{x:Bind ViewModel.CanPushBatch, Mode=OneWay}"`.

  **Fuzzy Search Picker cell templates** (see `02_Suggestions.md §2.6`):

  The **Part ID**, **From Location**, and **To Location** columns must each render a custom
  `DataGridTemplateColumn` containing:
  - A `TextBox` for direct text entry (bound `TwoWay` to the corresponding row property).
  - A small `Button` with a 🔍 `FontIcon` docked to the right edge of the cell.
  - The button `Command` is bound to `ViewModel.OpenPartSearchCommand` (for Part ID) or
    `ViewModel.OpenLocationSearchCommand` (for From/To Location) using `x:Bind` on the Page
    level, with the row item passed as `CommandParameter`.
  - For location columns, the `CommandParameter` is a tuple `(row, "FromLocation")` or
    `(row, "ToLocation")` as required by `OpenLocationSearchCommand`.
  - The 🔍 button is **always visible** (not hover-only) to aid discoverability.

  **Code-behind:**
  In `View_BulkInventory_DataEntry.xaml.cs`, after the ViewModel is set, assign `XamlRoot`:
  ```csharp
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
      base.OnNavigatedTo(e);
      ViewModel.XamlRoot = this.XamlRoot;
  }
  ```
  Also wire `F2` in the `KeyDown` handler to open the fuzzy picker for the currently active
  cell (call the appropriate ViewModel command for the focused column).

  - Keyboard legend strip at the bottom of the view listing: F2 = Search, F5 = Push, F6 = Skip,
    Escape = Cancel (per `02_Suggestions.md §2.4` and `§2.6`).
  - All bindings use `x:Bind` with explicit `Mode`.

  **Files touched:**
  - `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml` ← new
  - `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml.cs` ← new

---

- [x] **T026 — Create `View_BulkInventory_Push.xaml` with full-screen overlay**

  > **References:** `02_Suggestions.md §3.6` · `06_WorkflowDiagram.md — Diagram B OVS node`

  *Depends on T023.*

  Create `Module_Bulk_Inventory/Views/View_BulkInventory_Push.xaml`.

  The overlay `Grid` must be the **last child** of the page root `Grid` (renders on top of
  everything else):
  - Background `#AA000000` (67 % black).
  - `Visibility` bound `OneWay` to `ViewModel.IsAutomationRunning` via
    `BooleanToVisibilityConverter`.
  - `IsHitTestVisible="True"` with `KeyDown` and `PointerPressed` handlers that set
    `e.Handled = true` — swallows all input while automation runs.
  - Contains: `ProgressRing`, `TextBlock` bound to `ViewModel.OverlayStatusMessage`, row counter
    `TextBlock` e.g. *"Row 3 of 12"*, and a single **Cancel** button wired to
    `ViewModel.CancelPushCommand`.
  - When `WaitingForConfirmation` the overlay message pulses — the overlay is NOT hidden.

  **Files touched:**
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Push.xaml` ← new
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Push.xaml.cs` ← new

---

- [x] **T027 [P] — Create `View_BulkInventory_Summary.xaml`**

  > **References:** `02_Suggestions.md §2.5` · `06_WorkflowDiagram.md — Diagram B §5 SUM node`

  *Depends on T024.*

  Create `Module_Bulk_Inventory/Views/View_BulkInventory_Summary.xaml`.
  - Three count badges: Success (green), Failed (red), Skipped (grey).
  - List of Failed rows with checkboxes enabling multi-select for re-push.
  - **Re-push Selected** button (disabled when no failed rows are checked).
  - **Done** button returns to Data Entry and clears the session.

  **Files touched:**
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Summary.xaml` ← new
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Summary.xaml.cs` ← new

---

- [x] **T028 — Create `View_BulkInventory_Host.xaml` page container**

  > **References:** `01_Assumptions.md §4.1` · `04_ResearchReferences.md §2.2 (Module_ShipRec_Tools pattern)`

  *Depends on T025, T026, T027.*

  Create `Module_Bulk_Inventory/Views/View_BulkInventory_Host.xaml` — a single `Page` with a
  `ContentControl` whose `Content` is swapped between the three sub-views:
  1. Data Entry → Push → Summary

  Study `Module_ShipRec_Tools` for the exact navigation registry / routing pattern to follow.
  Window size must remain **1450×900 px** (per `01_Assumptions.md §4.2`).

  **Files touched:**
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Host.xaml` ← new
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Host.xaml.cs` ← new

---

## Phase 9 — Startup & Credential Integration

---

- [x] **T029 — Wire credential guard to `BulkInventoryNavItem` visibility at startup / login**

  > **References:** `03_PreRequisites.md §P1.1` · `01_Assumptions.md §2.2–2.3` ·
  > `06_WorkflowDiagram.md — Diagram A §CG node`

  *Depends on T009 (credential validator), T012 (nav item exists).*

  In `MainWindow.xaml.cs` (or the startup orchestration class that evaluates session state):
  1. After the user session is established, call
     `IService_VisualCredentialValidator.IsAllowed(session.User.VisualUsername)`.
  2. If `true` → set `BulkInventoryNavItem.Visibility = Visibility.Visible`.
  3. If `false` → keep `Visibility.Collapsed` (the item remains hidden; no error dialog shown).
  4. Re-evaluate on every session change (login / credential update in Settings).

  **Files touched:**
  - `MainWindow.xaml.cs` (or startup orchestration class)

---

- [x] **T030 — Add crash recovery check for stale `InProgress` records on app startup**

  > **References:** `06_WorkflowDiagram.md — Diagram A §0 (SIR/BNR nodes)` ·
  > `01_Assumptions.md §3.1`

  *Depends on T016 (DAO), T018 (service), T029 (session available at startup).*

  On app startup, after the user session is established:
  1. Call `GetByUserAsync(username, status: InProgress)`.
  2. Filter results to rows where `updated_at < DateTime.UtcNow.AddMinutes(-5)`.
  3. If any stale rows found:
     a. For each: call `UpdateStatusAsync(id, Failed, "Interrupted by app restart")`.
     b. Show the *"Interrupted Batch"* banner on `View_BulkInventory_DataEntry` (or the app
        shell) with two actions: **View in Summary** and **Re-push**.
  4. If no stale rows: proceed normally.

  **Files touched:**
  - App startup or `MainWindow.xaml.cs` startup path
  - `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_DataEntry.cs` ← add banner property

---

## Phase 10 — Validation & Polish

---

- [x] **T031 — Implement `ValidateAllCommand` — Infor Visual SQL Server PART + location check**

  > **References:** `02_Suggestions.md §2.3` · `01_Assumptions.md §3.6` ·
  > `04_ResearchReferences.md §2.2 (Module_Core/Data/InforVisual/ pattern)` ·
  > `06_WorkflowDiagram.md — Diagram A §2 VAL/VQ/VW nodes`

  *Depends on T022 (ViewModel command stub exists).*

  Implement the body of `ValidateAllCommand` in `ViewModel_BulkInventory_DataEntry`:
  1. For each `Pending` row, call a new method on `IService_InforVisual` (from Module_Core)
     that checks:
     - `PART` table: `Part_ID = row.PartId` exists.
     - Location existence: verify `row.FromLocation` and `row.ToLocation` exist for
       warehouse `002` using the existing Infor Visual SQL Server read-only pattern
       (see `Module_Core/Data/InforVisual/`).
  2. If a lookup fails → set `row.ValidationMessage` inline; do not show a dialog.
  3. After all rows checked → recompute `HasValidationWarnings`.
  4. If `HasValidationWarnings == true` → `CanPushBatch` becomes `false` automatically.
  5. The **Push Batch** button must remain disabled until all warnings are cleared (no bypass).

  **Files touched:**
  - `Module_Bulk_Inventory/ViewModels/ViewModel_BulkInventory_DataEntry.cs`
  - `Module_Core/Contracts/Services/IService_InforVisual.cs` ← add validation method signatures if not present
  - `Module_Core/Services/Database/Service_InforVisualConnect.cs` ← add validation implementations if not present

---

- [x] **T032 — Wire F5 / F6 / Escape keyboard shortcuts and add legend strip**

  > **References:** `02_Suggestions.md §2.4`

  *Depends on T025 (Data Entry view), T026 (Push view).*

  On `View_BulkInventory_DataEntry`:
  - **F5** → invoke `ViewModel.PushBatchCommand` (if enabled).
  - **F6** → invoke `ViewModel.SkipCurrentRowCommand` (add this no-op stub to ViewModel if not present).
  - **Escape** → invoke `ViewModel.ClearAllCommand` (confirm before clearing).
  - Add a `TextBlock` legend strip docked to the bottom of the view:
    `F5 = Push Batch   |   F6 = Skip Row   |   Escape = Clear`

  On `View_BulkInventory_Push` (while overlay is hidden / between rows):
  - **F6** → sets a flag observed by the Push ViewModel loop to skip the next row.
  - **Escape** → invokes `ViewModel.CancelPushCommand`.

  All keyboard handlers belong in `*.xaml.cs` code-behind as input events only (no business
  logic); they delegate to ViewModel commands.

  **Files touched:**
  - `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml`
  - `Module_Bulk_Inventory/Views/View_BulkInventory_DataEntry.xaml.cs`
  - `Module_Bulk_Inventory/Views/View_BulkInventory_Push.xaml.cs`

---

**Checkpoint — all tasks complete:** Build solution, navigate to Bulk Inventory (as a non-SHOP2/MTMDC user), enter a mixed batch of Transfer and New Transaction rows, run Validate All, click Push Batch, observe overlay, confirm both row types complete successfully, verify Summary screen counts, and confirm stale-record banner appears after a simulated crash (manually set a row to `InProgress` + `updated_at` 6+ minutes ago then restart the app).

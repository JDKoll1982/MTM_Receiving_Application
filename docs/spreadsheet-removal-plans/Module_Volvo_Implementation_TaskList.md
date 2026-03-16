# Module Volvo — Implementation Task List

Last Updated: 2026-03-15

## Context

The Volvo module currently uses `volvo_label_data` (header) and `volvo_line_data` (lines) as
persistent active-record tables. Completed shipments are soft-deleted via `is_archived=1` and
`status='completed'`. There are no physical history tables — reporting views (`view_volvo_history`,
`view_volvo_label_data_history`) read directly from the active tables.

This task list implements the two-table lifecycle defined in `Module_Volvo_Database_Plan.md`:
completed shipments are moved to real archive tables (`volvo_label_history`, `volvo_line_history`)
by a dedicated Clear Label Data command, preserving the full header + line payload with archive
metadata. Reporting views are updated to UNION ALL both active and history tables so no data
disappears post-migration.

---

## Phase 1 — Schema (History Tables)
### Task 1.1: Create `volvo_label_history` archive table ✅ DONE
- [x] Mirror all columns from `volvo_label_data` (id, shipment_date, shipment_number, po_number,
  receiver_number, employee_number, notes, status, created_date, modified_date)
- [x] Add `original_id INT NOT NULL` to preserve the active-table primary key reference
- [x] Add archive metadata: `archived_at DATETIME NOT NULL`, `archived_by VARCHAR(100) NOT NULL`,
  `archive_batch_id CHAR(36) NOT NULL`
- [x] Add indexes: `idx_archive_batch_id`, `idx_shipment_date`, `idx_employee_number`

> File: `Database/Schemas/43_Table_volvo_label_history.sql`

### Task 1.2: Create `volvo_line_history` archive table ✅ DONE
- [x] Mirror all columns from `volvo_line_data` (id, shipment_id, part_number, quantity_per_skid,
  received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
- [x] Add `original_id INT NOT NULL` and `original_shipment_id INT NOT NULL` to preserve FKs
- [x] Add `shipment_history_id INT NOT NULL` FK to `volvo_label_history.id`
- [x] Add archive metadata: `archived_at DATETIME NOT NULL`, `archived_by VARCHAR(100) NOT NULL`,
  `archive_batch_id CHAR(36) NOT NULL`

> File: `Database/Schemas/44_Table_volvo_line_history.sql`

---

## Phase 2 — Stored Procedure
### Task 2.1: Create `sp_Volvo_LabelData_ClearToHistory` ✅ DONE
- [x] Accept `p_archived_by VARCHAR(100)` input
- [x] Generate a UUID for `p_archive_batch_id` output
- [x] Move only `status='completed'` header rows from `volvo_label_data` to `volvo_label_history`
- [x] Move associated line rows from `volvo_line_data` to `volvo_line_history` (keyed by moved
  shipment IDs)
- [x] Delete moved lines from `volvo_line_data`
- [x] Delete moved headers from `volvo_label_data`
- [x] Wrap entire operation in a transaction with `ROLLBACK` on `SQLEXCEPTION`
- [x] Output `p_headers_moved INT`, `p_lines_moved INT`, `p_archive_batch_id CHAR(36)`,
  `p_status INT`, `p_error_message VARCHAR(1000)`

> File: `Database/StoredProcedures/Volvo/sp_Volvo_LabelData_ClearToHistory.sql`

---

## Phase 3 — DAO
### Task 3.1: Create `Dao_VolvoLabelHistory` ✅ DONE
- [x] Instance-based DAO, injected connection string
- [x] `ClearToHistoryAsync(string archivedBy)` — calls `sp_Volvo_LabelData_ClearToHistory`,
  returns `Model_Dao_Result<(int HeadersMoved, int LinesMoved)>`
- [x] Never throw exceptions — return failure result on error

> File: `Module_Volvo/Data/Dao_VolvoLabelHistory.cs`

### Task 3.2: Register DAO in DI ✅ DONE
- [x] Register `Dao_VolvoLabelHistory` as Singleton in `ModuleServicesExtensions.cs`
  `AddVolvoModule` method

> `Dao_VolvoLabelHistory` is absent from the `AddVolvoModule` registrations in
> `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.

---

## Phase 4 — CQRS (Command + Handler)
### Task 4.1: Create `ClearLabelDataCommand` ✅ DONE
- [x] `ClearLabelDataCommand : IRequest<Model_Dao_Result<int>>` in
  `Module_Volvo/Requests/Commands/`
- [x] Single property: `ArchivedBy string`

> File: `Module_Volvo/Requests/Commands/ClearLabelDataCommand.cs`

### Task 4.2: Create `ClearLabelDataCommandHandler` ✅ DONE
- [x] Inject `Dao_VolvoLabelHistory` and `IService_VolvoAuthorization`
- [x] Check `CanCompleteShipmentsAsync()` before proceeding
- [x] Call `ClearToHistoryAsync`, return total rows moved (headers + lines) as `Data`
- [x] Return failure result on error — do not throw

> File: `Module_Volvo/Handlers/Commands/ClearLabelDataCommandHandler.cs`

---

## Phase 5 — ViewModel
### Task 5.1: Add `ClearLabelDataAsync` RelayCommand to `ViewModel_Volvo_ShipmentEntry` ✅ DONE
- [x] Show confirmation `ContentDialog` with "Clear Label Data" terminology
- [x] Block if no completed shipments exist (guard with status message)
- [x] Call `_mediator.Send(new ClearLabelDataCommand { ArchivedBy = employeeNumber })`
- [x] Show success message on completion (rows moved count)
- [x] Use `IsBusy` guard pattern consistent with existing commands

---

## Phase 6 — Reporting View Updates
### Task 6.1: Update `view_volvo_history` to include history table ✅ DONE
- [x] UNION ALL records from `volvo_label_history` so cleared shipments appear in reporting

> File: `Database/Schemas/36_View_volvo_history.sql`

### Task 6.2: Update `view_volvo_label_data_history` to include history tables ✅ DONE
- [x] UNION ALL records from `volvo_label_history` + `volvo_line_history` so cleared shipment
  lines appear in flattened reporting view

> File: `Database/Schemas/37_View_volvo_label_data_history.sql`

---

## Phase 7 — Unreachable Code & Outstanding TODOs

These items were found during a code audit and represent stubbed or deferred work that needs to
be tracked. All three stub methods use the same pattern: an early `return` of a failure result
followed by the original implementation inside a comment block tagged
`// --- original implementation below (unreachable) ---`.

### Task 7.1: Implement `Helper_VolvoShipmentCalculations.GenerateLabelAsync`
- [ ] Remove the early-return stub and the unreachable comment block
- [ ] Implement (or wire through MediatR) the database-backed label generation logic
- [ ] Remove `[STUB]` tag from XML doc comment in `Helper_VolvoShipmentCalculations.cs` and
  `IService_Volvo.cs`
- [ ] Remove `[STUB]` tag from `ViewModel_Volvo_ShipmentEntry.GenerateLabelsAsync` XML doc

> **Unreachable code** in `Module_Volvo/Helpers/Helper_VolvoShipmentCalculations.cs` — line ~135.
> Matching TODO in `Module_Volvo/Contracts/IService_Volvo.cs` and
> `Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs` (~line 495).

### Task 7.2: Implement `Service_VolvoMasterData.ImportDataAsync`
- [ ] Remove the early-return stub and the unreachable comment block
- [ ] Implement the actual CSV-to-database import using `Dao_VolvoPart` and `Dao_VolvoPartComponent`
- [ ] Remove `[STUB]` / TODO references from `IService_VolvoMasterData.cs` (~line 59),
  `Service_VolvoMasterData.cs` (~line 260), `ImportPartsCsvCommandHandler.cs` (~line 18),
  and `ViewModel_Volvo_Settings.cs` (~line 374)

> **Unreachable code** in `Module_Volvo/Services/Service_VolvoMasterData.cs` — line ~262.
> Matching unreachable block in `Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandler.cs`
> — line ~36.

### Task 7.3: Enforce reference check in `Dao_VolvoPart.DeactivateAsync`
- [ ] Implement OUT-parameter support in `Helper_Database_StoredProcedure` (or use a raw
  `MySqlCommand` in the DAO directly, consistent with `Dao_VolvoLabelHistory`)
- [ ] Read `active_reference_count` from `sp_volvo_part_check_references` and block deactivation
  when references exist

> **Bypass TODO** in `Module_Volvo/Data/Dao_VolvoPart.cs` — line ~123. The reference-check call
> is executed but the OUT parameter value is ignored because the helper does not yet support it.
> Cascade protection is currently unenforced.

### Task 7.4: Replace `Environment.UserName` with authenticated user in
`SavePendingShipmentCommandHandler`
- [ ] Replace `Environment.UserName` with the value from `IService_UserSessionManager` (or
  equivalent auth service) once Issue #6 is resolved

> `Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandler.cs` — line ~44.
> Consistent with the broader Issue #6 auth gap below.

### Task 7.5: Implement role-based authorization in `Service_VolvoAuthorization` (Issue #6)
- [ ] Inject `IService_UserSessionManager` (or equivalent) once available
- [ ] Replace the four placeholder `return Success` stubs with actual role checks in:
  - `CanManageShipmentsAsync` — allow all Volvo operators
  - `CanManageMasterDataAsync` — restrict to supervisors/admins
  - `CanCompleteShipmentsAsync` — may require supervisor approval
  - `CanGenerateLabelsAsync` — confirm required role

> `Module_Volvo/Services/Service_VolvoAuthorization.cs` — lines ~31, 64, 92, 120.
> All four methods currently return `Success = true` unconditionally.

### Task 7.6: Migrate or remove `SaveShipmentInternalAsync` in `ViewModel_Volvo_ShipmentEntry`
- [ ] Audit callers of `SaveShipmentInternalAsync` inside the ViewModel
- [ ] Migrate each caller to send `SavePendingShipmentCommand` via MediatR directly
- [ ] Remove `SaveShipmentInternalAsync` once callers are migrated

> `Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs` — line ~1121 (legacy wrapper).

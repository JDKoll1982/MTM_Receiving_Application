# Module Dunnage — Implementation Task List

Last Updated: 2026-07-14

## Phase 1 — Schema Alignment (Queue + History)
### Task 1.1: Create `dunnage_label_data` active queue table
- [x] Create table with all fixed label fields (`load_uuid`, `part_id`, `dunnage_type_id`, `dunnage_type_name`, `dunnage_type_icon`, `quantity`, `po_number`, `received_date`, `user_id`, `location`, `label_number`)
- [x] Add `specs_json JSON NULL` column for dynamic per-line spec snapshot
- [x] Set appropriate indexes for queue consumer lookups

#### Subtasks (Task 1.1)
- [x] Add `load_uuid`, `part_id`, `quantity`, `received_date`, `user_id`
- [x] Add `po_number`, `location`, `label_number`
- [x] Add `dunnage_type_id`, `dunnage_type_name`, `dunnage_type_icon`
- [x] Add `specs_json JSON NULL` for dynamic spec snapshot
- [x] Add `created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP`
- [x] Add indexes: `idx_load_uuid`, `idx_part_id`, `idx_received_date`

### Task 1.2: Align `dunnage_history` to preserve full label context
- [x] Add all missing workflow fields from plan gap analysis to history table
- [x] Add archive metadata columns for clear operation traceability

#### Subtasks (Task 1.2)
- [x] Add `po_number VARCHAR(50) NULL`
- [x] Add `type_id INT NULL`, `type_name VARCHAR(100) NULL`, `type_icon VARCHAR(100) NULL`
- [x] Add `location VARCHAR(100) NULL`, `label_number VARCHAR(50) NULL`
- [x] Add `specs_json JSON NULL`
- [x] Add `archived_at DATETIME NULL`, `archived_by VARCHAR(100) NULL`, `archive_batch_id CHAR(36) NULL`

### Task 1.3: Add migration artifacts
- [x] Create versioned SQL schema file `40_Table_dunnage_label_data.sql`
- [x] Create versioned SQL migration file `41_Migration_dunnage_history_parity.sql`
- [x] Include comments describing queue/archive lifecycle intent

---

## Phase 2 — Stored Procedure Lifecycle
### Task 2.1: Queue insert procedures
- [x] Create `sp_Dunnage_LabelData_Insert` (single-row insert into `dunnage_label_data`)
- [x] Create `sp_Dunnage_LabelData_InsertBatch` (JSON batch insert into `dunnage_label_data`)

#### Subtasks (Task 2.1)
- [x] Accept full fixed field list + `specs_json` as input
- [x] Handle edge cases for NULL `po_number`, `location`, `label_number`
- [x] Batch SP iterates JSON array and calls underlying insert per row

### Task 2.2: Clear-to-history procedure
- [x] Implement `sp_Dunnage_LabelData_ClearToHistory`

#### Subtasks (Task 2.2)
- [x] Insert-select all rows from `dunnage_label_data` into `dunnage_history`
- [x] Stamp archive metadata (`archived_at`, `archived_by`, `archive_batch_id`)
- [x] Delete moved rows from `dunnage_label_data`
- [x] Wrap in transaction with `ROLLBACK` on `SQLEXCEPTION`
- [x] Output `p_rows_moved`, `p_archive_batch_id`, `p_status`, `p_error_message`

---

## Phase 3 — DAO and Service Wiring
### Task 3.1: Create `Dao_DunnageLabelData`
- [x] Add `InsertBatchAsync(List<Model_DunnageLoad> loads, string user)` serializing full field list + `specs_json`
- [x] Add `ClearToHistoryAsync(string archivedBy)` calling `sp_Dunnage_LabelData_ClearToHistory`
- [x] Register DAO as singleton in DI (`ModuleServicesExtensions.cs`)

### Task 3.2: Update save path in `Service_DunnageWorkflow` and `Service_MySQL_Dunnage`
- [x] Route `SaveLoadsAsync` to write to `dunnage_label_data` (via `Dao_DunnageLabelData`) instead of `dunnage_history`
- [x] Extend JSON serialization in batch to include all fixed fields + `specs_json`
- [x] Populate `SpecValues`/`Specs` from `Model_DunnageLoad` before serialization

### Task 3.3: Implement `ClearLabelDataAsync` service method
- [x] Add `ClearLabelDataAsync(string archivedBy)` to `IService_MySQL_Dunnage` interface
- [x] Implement in `Service_MySQL_Dunnage` delegating to `Dao_DunnageLabelData.ClearToHistoryAsync`
- [x] Return moved-count and error details to caller

### Task 3.4: Add clear workflow method to `IService_DunnageWorkflow`
- [x] Add `ClearLabelDataAsync()` to `IService_DunnageWorkflow` interface
- [x] Implement in `Service_DunnageWorkflow` calling `_dunnageService.ClearLabelDataAsync`
- [x] Return `Model_Dao_Result<int>` with rows-moved count

### Task 3.5: Read-model parity
- [ ] Ensure `Dao_DunnageLoad.MapFromReader` hydrates new `dunnage_history` columns for edit/history views

---

## Phase 4 — UI / Workflow Integration
### Task 4.1: Wire clear action in ViewModel
- [x] Add `ClearLabelDataAsync` RelayCommand to `ViewModel_Dunnage_WorkFlowViewModel`
- [x] Confirmation dialog with "Clear Label Data" terminology
- [x] `IsBusy` guard and status feedback on result

### Task 4.2: Status/feedback updates
- [x] Show queue save status (rows saved) after workflow completion
- [x] Show archive transfer result (rows moved) on clear action

---

## Phase 5 — Validation and Safeguards
### Task 5.1: Integration test SQL scripts
- [x] Create `Database/00-Test/06-Test-Dunnage-LabelData-InsertAndVerify.sql` — full field queue write
- [x] Create `Database/00-Test/07-Test-Dunnage-LabelData-ClearToHistory.sql` — atomic queue-to-history transfer

### Task 5.2: Data quality checks
- [x] Validate no field loss between `dunnage_label_data` queue row and archived `dunnage_history` row
- [x] Validate counts before/after clear operation match exactly

### Task 5.3: Operational scripts
- [ ] Add reconciliation script for queue/history consistency checks

---

## Current Execution State
- [x] Phase 1 started
- [x] Phase 1 completed
- [x] Phase 2 started
- [x] Phase 2 completed
- [x] Phase 3 started
- [x] Phase 3 completed (Task 3.5 read-model parity deferred)
- [x] Phase 4 started
- [x] Phase 4 completed
- [x] Phase 5 started
- [ ] Phase 5 completed (Task 3.5 and Task 5.3 remain)

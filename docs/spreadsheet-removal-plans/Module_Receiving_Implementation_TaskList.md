# Module Receiving — Implementation Task List

Last Updated: 2026-02-18

## Phase 1 — Schema Alignment (Queue + History)
### Task 1.1: Align `receiving_label_data` to full workflow line payload
- [x] Add all missing columns required by workflow/XLS parity (25-field coverage)
- [x] Preserve backward compatibility with existing consumers (`quantity`, `transaction_date`, etc.)
- [x] Validate nullability/default strategy for non-PO and optional fields

#### Subtasks (Task 1.1)
- [x] Add `load_id`, `load_number`, `part_type`, `po_line_number`
- [x] Add `po_vendor`, `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`
- [x] Add `packages_per_load`, `package_type_name`, `weight_per_package`, `is_non_po_item`, `received_date`
- [x] Add `user_id`, `is_quality_hold_required`, `is_quality_hold_acknowledged`, `quality_hold_restriction_type`
- [x] Add `weight_quantity` to preserve label queue parity with workflow data
- [x] Convert `po_number` to string-compatible type for PO formats

### Task 1.2: Align `receiving_history` to preserve full label context
- [x] Add all missing workflow/XLS fields to history
- [x] Add archive metadata columns for clear operation traceability

#### Subtasks (Task 1.2)
- [x] Add `PartDescription`, `POVendor`, `POStatus`, `PODueDate`, `QtyOrdered`
- [x] Add `UnitOfMeasure`, `RemainingQuantity`, `UserID`, `EmployeeNumber`
- [x] Add `IsQualityHoldRequired`, `IsQualityHoldAcknowledged`, `QualityHoldRestrictionType`
- [x] Add `ArchivedAt`, `ArchivedBy`, `ArchiveBatchID`

### Task 1.3: Add migration artifact
- [x] Create versioned SQL migration script under `Database/Schemas`
- [x] Include comments describing queue/archive lifecycle intent

---

## Phase 2 — Stored Procedure Lifecycle
### Task 2.1: Queue write procedures
- [x] Create/update `sp_Receiving_LabelData_Insert`
- [x] Create/update `sp_Receiving_LabelData_Update` (if edit queue write path needed)

#### Subtasks (Task 2.1)
- [x] Accept full workflow field list as input
- [x] Normalize PO/non-PO edge cases consistently

### Task 2.2: Clear-to-history procedure
- [x] Implement `sp_Receiving_LabelData_ClearToHistory`

#### Subtasks
- [x] Insert-select from queue to history
- [x] Stamp archive metadata (`ArchivedAt`, `ArchivedBy`, `ArchiveBatchID`)
- [x] Delete moved rows from queue
- [x] Wrap in transaction with rollback guarantees

---

## Phase 3 — DAO and Service Wiring
### Task 3.1: Queue DAO write path
- [x] Add queue DAO methods for insert/update
- [x] Map full `Model_ReceivingLoad` payload to queue SP params

### Task 3.2: Clear action service path
- [x] Implement service method invoking clear-to-history SP
- [x] Return moved-count and error details to ViewModel/UI

### Task 3.3: Read-model parity
- [x] Ensure read mappers include added fields for edit/history scenarios

---

## Phase 4 — UI / Workflow Integration
### Task 4.1: Rename and rebind clear action
- [x] Rename clear action text to `Clear Label Data`
- [x] Wire command to clear-to-history service call (not file deletion)

### Task 4.2: Status/feedback updates
- [x] Show queue save status after workflow completion
- [x] Show archive transfer result on clear action

---

## Phase 5 — Validation and Safeguards
### Task 5.1: Integration tests
- [x] Test queue write with full field coverage
- [x] Test clear-to-history atomic transfer
- [x] Test failure rollback (no partial delete)

### Task 5.2: Data quality checks
- [x] Validate no data loss between queue row and archived history row
- [x] Validate counts before/after clear operation

### Task 5.3: Operational scripts
- [x] Add optional reconciliation query/script for queue/history consistency

---

## Current Execution State
- [x] Phase 1 started
- [x] Phase 1 completed
- [x] Phase 2 started
- [x] Phase 3 started
- [x] Phase 4 started
- [x] Phase 4 completed
- [x] Phase 5 started
- [x] Phase 5 completed

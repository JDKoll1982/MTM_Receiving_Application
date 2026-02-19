# Module Dunnage — Database Replacement Plan

Last Updated: 2026-02-18

## Goal
Implement a two-table lifecycle for Dunnage where workflow completion writes to `dunnage_label_data` (active queue for LabelView2022), and `Clear Label Data` moves those rows to `dunnage_history`.

## Master Branch Spreadsheet Output (Source of Truth)
From `master` `Module_Dunnage/Services/Service_DunnageXLSWriter.cs`, line output included:

### Fixed columns (dynamic export variant)
- ID (`load_uuid`)
- PartID
- DunnageType
- Quantity
- PONumber
- ReceivedDate
- UserId
- Location
- LabelNumber

### Dynamic columns
- One column per spec key from dunnage type definitions (e.g., width/length/material/etc.), values from `Model_DunnageLoad.Specs`/`SpecValues`.

## Required Lifecycle (Authoritative)
1. User completes workflow
2. Save line rows to `dunnage_label_data` (active label queue)
3. User clicks `Clear Label Data`
4. Move rows from `dunnage_label_data` → `dunnage_history`
5. Delete moved rows from `dunnage_label_data`

## Gap Analysis
### Required fixed columns in `dunnage_label_data`
- `load_uuid`
- `part_id`
- `dunnage_type`
- `quantity`
- `po_number`
- `received_date`
- `user_id`
- `location`
- `label_number`

### Required dynamic columns in `dunnage_label_data`
- Full per-line spec snapshot (all spec keys/values used in workflow).

### Missing in current architecture
Current save path writes directly to `dunnage_history` and there is no dedicated `dunnage_label_data` active queue table.

### History parity requirements
`dunnage_history` must retain all queue business fields (plus archive metadata), so no information is lost on clear.

### Additional fields currently not persisted end-to-end
- `po_number`
- `dunnage_type` snapshot (`type_id`, `type_name`, `type_icon`)
- `location`
- `label_number`
- Per-line dynamic spec values

## Implementation Plan
1. **Schema creation/alignment (queue + archive)**
   - Create `dunnage_label_data` table with all fixed fields and `specs_json`.
   - Add/align nullable columns on `dunnage_history`:
     - `po_number VARCHAR(...)`
     - `type_id INT NULL`
     - `type_name VARCHAR(...)`
     - `type_icon VARCHAR(...)`
     - `location VARCHAR(...)`
     - `label_number VARCHAR(...)`
2. **Dynamic spec persistence design**
   - Add `specs_json JSON NULL` to both `dunnage_label_data` and `dunnage_history`.
   - Optional (recommended for analytics/search): add child table `dunnage_history_specs(load_uuid, spec_key, spec_value)`.
3. **Stored procedure updates**
   - Add queue SPs:
     - `sp_Dunnage_LabelData_Insert`
     - `sp_Dunnage_LabelData_InsertBatch`
     - `sp_Dunnage_LabelData_ClearToHistory` (transactional move)
   - Keep existing history SPs for direct history queries.
4. **DAO updates**
   - Route workflow save writes to `dunnage_label_data` DAO/SPs.
   - Implement clear action DAO method calling `ClearToHistory` SP.
   - Extend batch serialization to include all fixed fields + `specs_json`.
   - Extend mapper to hydrate new columns for edit/history views.
5. **Workflow wiring**
   - Ensure `Model_DunnageLoad` values (`PoNumber`, `Location`, `TypeName`, `TypeIcon`, `LabelNumber`, `Specs`) are populated before save.
6. **Verification**
   - Integration test: workflow complete writes to `dunnage_label_data` with full fixed + dynamic fields.
   - Integration test: `Clear Label Data` moves all rows to `dunnage_history` and empties active queue.
   - Validate edit mode/history round-trips without data loss.

## Deliverables
- Dunnage queue/history schema scripts (`dunnage_label_data` + `dunnage_history` parity).
- Queue insert/batch/clear-to-history stored procedures.
- DAO/service updates to active queue + clear lifecycle.
- Tests proving full-field queue write and atomic queue→history transfer.

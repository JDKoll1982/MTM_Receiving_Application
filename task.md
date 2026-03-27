# Receiving History Reporting And Persistence Findings

Last Updated: 2026-03-27

## Scope

This document captures the current codebase state for `receiving_label_data`, `receiving_history`, and the `Module_Reporting` receiving-history pipeline. It replaces the original scratch notes with file-backed findings.

## Purchasing Department Answers

### When material is ordered

- When a PO is created, the PO header `Status` field indicates whether the order has been placed.
- If the header status is `Released`, the PO has been sent to the supplier.
- The desired delivery date is visible on the PO header.
- If the PO has multiple lines with different delivery dates, those dates appear on each PO line in the `RecvDate` column.
- Once the vendor confirms the order, Purchasing usually attaches the confirmation to the paper clip on the PO header.
- Purchasing then enters the confirmed date in the `Promise Delivery Date` field on the PO header.
- If available, a sales order number is entered into the `Sales Order ID` field on the header.

### When the vendor confirms a delivery date

- The confirmed delivery date is entered in the PO header `Promise Delivery Date` field.
- Purchasing typically does not update the line-level dates after confirmation.
- If line-level dates differ, the latest line date is used as the header `Desired Recv Date`.

### Blanket and bulk orders

- Blanket orders are usually identified by `BLANKET` in the PO header `FOB` field.
- The header `Desired Recv Date` holds the final delivery or planned consumption date.
- Line-level delivery schedules are not fully maintained because releases happen later from planning requests.
- As a result, expected arrival on a specific date may need to come from release activity, not just the original PO line setup.

## Requested Reporting Columns Vs Current Codebase

The current reporting path is:

- `Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingHistory_GetByDateRange.sql`
- `Module_Reporting/Data/Dao_Reporting.cs`

### Requested columns for Module_Reporting

| Requested Field                 | In `receiving_label_data` queue | In canonical `receiving_history` schema | Returned by reporting proc now | Current finding                                                        |
| ------------------------------- | ------------------------------- | --------------------------------------- | ------------------------------ | ---------------------------------------------------------------------- |
| `quantity`                      | Yes                             | Yes                                     | Yes                            | Available now                                                          |
| `part_id`                       | Yes                             | Yes                                     | Yes, as `part_number`          | Available now, but aliased                                             |
| `po_number`                     | Yes                             | Yes                                     | Yes                            | Available now                                                          |
| `employee_number`               | Yes                             | Yes                                     | Yes                            | Available now                                                          |
| `heat`                          | Yes                             | Yes                                     | No                             | Stored, but not selected by reporting proc                             |
| `transaction_date`              | Yes                             | Yes                                     | No                             | Reporting uses `created_date`, not raw `transaction_date`              |
| `initial_location`              | Yes                             | Yes                                     | Yes, as `location`             | Available now, but aliased                                             |
| `vendor_name`                   | Yes                             | Yes                                     | No                             | Stored in view, but omitted from reporting proc                        |
| `part_description`              | Yes                             | Yes                                     | No                             | Stored in view, but omitted from reporting proc                        |
| `part_skid_total`               | Yes                             | Yes                                     | No                             | Archived, but omitted from view and reporting proc                     |
| `created_at` with time-of-day   | Yes                             | Yes                                     | No                             | View converts to date-only `created_date`; time-of-day is lost         |
| `unit_of_measure`               | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |
| `is_quality_hold_required`      | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |
| `is_quality_hold_acknowledged`  | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |
| `quality_hold_restriction_type` | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |

### Current reporting output

`sp_Reporting_ReceivingHistory_GetByDateRange` currently returns only:

- `id`
- `po_number`
- `po_line_number`
- `part_number`
- `quantity`
- `created_date`
- `employee_number`
- `created_by_username` as `NULL`
- `source_module`
- `location`
- `notes`
- `load_number`
- `label_number`
- `packages_per_load`
- `package_type_name`
- `coils_on_skid`
- `is_non_po_item`
- `quantity_per_skid` as `NULL`
- `received_skid_count` as `NULL`

This means the reporting proc is the current bottleneck for several requested receiving fields, even when the underlying queue or view already has some of them.

## Duplicate Column Analysis

### What is actually duplicated today

The canonical schema file `Database/Database_Deployment/Sql_Files/Schemas/10_Table_receiving_history.sql` defines only snake_case history columns.

However, `Database/Database_Deployment/Sql_Files/Migrations/01_Migration_receiving_label_queue_history_alignment.sql` adds additional PascalCase columns into `receiving_history`, including:

- `PartDescription`
- `POVendor`
- `POStatus`
- `PODueDate`
- `QtyOrdered`
- `UnitOfMeasure`
- `RemainingQuantity`
- `UserID`
- `EmployeeNumber`
- `IsQualityHoldRequired`
- `IsQualityHoldAcknowledged`
- `QualityHoldRestrictionType`
- `ArchivedAt`
- `ArchivedBy`
- `ArchiveBatchID`

The new dump-style seed file `Database/Database_Deployment/Sql_Files/SeedData/04_seed_receiving_history.sql` also includes those same PascalCase columns.

### Important distinction

Some PascalCase names still appear in the current codebase, but they should be treated as cleanup targets, not compatibility features to preserve.

Examples of procedures and read paths that currently still rely on PascalCase names and should be updated to snake_case-only result sets:

- `sp_Receiving_LabelData_GetAll.sql`
- `sp_Receiving_History_Get.sql`
- `sp_Receiving_Load_GetAll.sql`

The target state for this work is:

- no physical duplicate PascalCase columns in `receiving_history`
- no stored-procedure result aliases that rename snake_case columns into PascalCase names
- DAO and reporting mappers updated to read the actual snake_case column names directly

### Requested duplicate cleanup review

| Duplicate Column  | Correct Column / Action                                   | Current codebase finding                                                                         |
| ----------------- | --------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `PartDescription` | `part_description`                                        | Physical duplicate added by migration; read procedures and mappers must be updated to snake_case |
| `POVendor`        | `vendor_name` for history snapshots, `po_vendor` in queue | Physical duplicate added by migration; read procedures and mappers must be updated to snake_case |
| `EmployeeNumber`  | `employee_number`                                         | Physical duplicate added by migration; read procedures and mappers must be updated to snake_case |
| `ArchivedAt`      | Remove if not needed                                      | Found in migration and seed dump; no active C# usage found                                       |
| `ArchivedBy`      | Remove if not needed                                      | Found in migration and seed dump; no active C# usage found                                       |
| `ArchiveBatchID`  | Remove if not needed                                      | Found in migration and seed dump; no active C# usage found                                       |

### Recommendation

- Do not keep PascalCase stored-procedure aliases.
- Remove the physical duplicate PascalCase columns from `receiving_history` and update the affected stored procedures and DAO/reporting mappers in the same coordinated change.
- Return and read actual snake_case column names end-to-end so SQL contracts and C# mappings match the real schema.
- Remove `ArchivedAt`, `ArchivedBy`, and `ArchiveBatchID` if archive-batch tracking is not a business requirement, because they are not actively consumed by the app today.

## Quality Hold Logic Status

### What exists today

Quality hold data is persisted into the active queue table:

- `receiving_label_data.is_quality_hold_required`
- `receiving_label_data.is_quality_hold_acknowledged`
- `receiving_label_data.quality_hold_restriction_type`

This is wired through:

- `sp_Receiving_LabelData_Insert.sql`
- `sp_Receiving_LabelData_Update.sql`
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs`
- `Module_Receiving/Data/Dao_ReceivingLoad.cs`

### UI and workflow behavior today

- Initial restriction detection is implemented in `Module_Receiving/Services/Service_QualityHoldWarning.cs`.
- Manual entry sets `IsQualityHoldRequired` and `QualityHoldRestrictionType`, then sets `IsQualityHoldAcknowledged = true` only after the final confirmation dialog.
- Edit mode does the same final acknowledgment step.
- Guided workflow blocks progression if any manual-entry loads still require acknowledgment.

### What is missing

- `sp_Receiving_LabelData_ClearToHistory.sql` does not archive the quality-hold fields into `receiving_history`.
- `sp_Receiving_History_Get.sql` hardcodes history quality-hold values to `0` and `NULL`.
- `sp_Reporting_ReceivingHistory_GetByDateRange.sql` does not return any quality-hold fields.
- I did not find a guided-mode path that explicitly sets `IsQualityHoldAcknowledged = true`; the explicit final-acknowledgment setters are in Manual Entry and Edit Mode.

### End-user and UI recommendation

- If quality hold must be auditable after the queue is cleared, archive all three fields into `receiving_history` and expose them in Module_Reporting.
- If guided mode should enforce the same dual-confirmation rule as manual/edit mode, add a final acknowledgment checkpoint before Review or Save.
- If reporting users need to know whether a load was held, add the three fields to the reporting view and stored procedure rather than relying on queue-only data.

## Why These Columns Are Coming Through As Null

| Column               | Current codebase state                                                                             | Why it is null today                                                                                                                                                                                                               | Change needed                                                                                              |
| -------------------- | -------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `po_status`          | Saved to queue from guided and PO-assisted manual flows                                            | `sp_Receiving_LabelData_ClearToHistory.sql` does not archive it into history; reporting does not return it                                                                                                                         | Archive it into history and expose it in reporting                                                         |
| `po_due_date`        | Queue supports it, but upstream population is incomplete                                           | Guided PO entry explicitly sets `_workflowService.CurrentPODueDate = null`; `Model_InforVisualPO` has no due-date property; service conversion drops line `DueDate`; manual selected-PO path also does not assign `load.PoDueDate` | Decide due-date source rule, extend service model, carry due date onto the load, archive it, and report it |
| `qty_ordered`        | Saved to queue from current part or selected PO part                                               | Not archived to canonical history and not returned by reporting                                                                                                                                                                    | Archive it into history and expose it in reporting                                                         |
| `unit_of_measure`    | Saved to queue, defaults to `EA` when missing                                                      | Not archived to canonical history and not returned by reporting                                                                                                                                                                    | Archive it into history and expose it in reporting                                                         |
| `remaining_quantity` | Saved to queue from current part or selected PO part                                               | Not archived to canonical history and not returned by reporting                                                                                                                                                                    | Archive it into history and expose it in reporting                                                         |
| `user_id`            | Guided workflow uses `WindowsUsername`; manual entry also backfills from session `WindowsUsername` | Queue supports it, but history archive ignores it and reporting does not expose it                                                                                                                                                 | Archive `user_id` into history and expose it in reporting                                                  |

### Additional note on `po_due_date`

The current SQL query already retrieves per-line due date data from Infor Visual:

- `Database/InforVisualScripts/Queries/01_GetPOWithParts.sql` uses `pol.PROMISE_DATE AS DueDate`
- `Module_Core/Data/InforVisual/Dao_InforVisualPO.cs` maps `DueDate` into `Model_InforVisualPOLine`

But that value is dropped when the service converts DAO results into the higher-level PO model:

- `Module_Core/Models/InforVisual/Model_InforVisualPO.cs` has no due-date property
- `Module_Core/Services/Database/Service_InforVisualConnect.cs` does not copy due date into the service model
- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs` explicitly sets `CurrentPODueDate = null`

Based on Purchasing's guidance, the final implementation likely needs two supported date paths:

- Line-level due date for standard PO lines
- Header-level promised delivery date for confirmed or blanket-order scenarios

### Infor Visual schema source of truth

For all Infor Visual-related table discovery, relationship checks, and future query design, use the schema exports under `docs/InforVisual/DatabaseCSVFiles/` as the primary source of truth instead of inferring structure from ad hoc SQL alone.

Required reference files:

- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_CheckConstraints.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_ColumnDetails.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_DefaultConstraints.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_FKs.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Indexes.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_PKs.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_TableRowCounts.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Tables.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Triggers.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_UniqueConstraints.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Views.csv`

Use these files to validate:

- which table or view actually owns a field
- whether a relationship is backed by a real FK or unique constraint
- whether a lookup should be header-level, line-level, or composite-key based
- whether a large table needs extra care because of row count or indexing
- whether a query target is a real SQL Server view or a base table with a `V_` prefix

## Recommended Implementation Order

1. Standardize `receiving_history` on the intended snake_case schema and remove the physical duplicate PascalCase columns and stored-procedure aliases as part of one coordinated contract cleanup.
2. Expand `sp_Receiving_LabelData_ClearToHistory.sql` so it archives the newer queue fields into snake_case history columns only: `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, and the three quality-hold fields.
3. Update `02_View_receiving_history.sql`, `sp_Reporting_ReceivingHistory_GetByDateRange.sql`, and affected DAO mappings to return and read only the approved snake_case receiving-report fields.
4. Fix PO due date population by carrying Infor Visual due-date data through the service model and workflow state.
5. Decide whether guided mode also needs an explicit final quality-hold acknowledgment step, then implement it consistently across all receiving modes.

## Context Map

### Files to Modify

| File                                                                                                                 | Purpose                         | Expected Change                                                                             |
| -------------------------------------------------------------------------------------------------------------------- | ------------------------------- | ------------------------------------------------------------------------------------------- |
| `Database/Database_Deployment/Sql_Files/Schemas/10_Table_receiving_history.sql`                                      | Canonical history schema        | Decide final snake_case history columns and remove drift from the canonical contract        |
| `Database/Database_Deployment/Sql_Files/Migrations/01_Migration_receiving_label_queue_history_alignment.sql`         | Live DB alignment               | Replace duplicate-column creation with the final migration strategy                         |
| `Database/Database_Deployment/Sql_Files/SeedData/04_seed_receiving_history.sql`                                      | Seed dump for history           | Align with final schema or remove if this dump should not be the source of truth            |
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql`        | Queue-to-history archival       | Archive the newer queue fields into history and stop losing PO, user, and quality-hold data |
| `Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql`                                         | Reporting-facing receiving view | Expose the approved receiving fields, including timestamp handling decisions                |
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingHistory_GetByDateRange.sql` | Module_Reporting data source    | Return the final approved field set for receiving reports                                   |
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_Get.sql`                     | Edit/history loader contract    | Stop hardcoding missing values and return snake_case columns only                           |
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_GetAll.sql`                | Queue loader contract           | Return actual snake_case column names and update DAO mapping accordingly                    |
| `Module_Core/Models/InforVisual/Model_InforVisualPO.cs`                                                              | Service-layer PO model          | Add header-level due-date support if that becomes the chosen contract                       |
| `Module_Core/Services/Database/Service_InforVisualConnect.cs`                                                        | DAO-to-service mapping          | Carry due-date data from DAO results into the service model                                 |
| `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`                                                         | Guided PO load behavior         | Stop nulling PO due date and assign the correct due-date source                             |
| `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`                                                     | Manual PO-assisted selection    | Assign `PoDueDate` when a selected PO part is applied                                       |
| `Module_Receiving/Services/Service_ReceivingWorkflow.cs`                                                             | Guided load generation          | Continue copying PO fields onto loads after the due-date contract is fixed                  |
| `Module_Reporting/Data/Dao_Reporting.cs`                                                                             | Report row mapping              | Map any newly returned snake_case columns required by the receiving report UI               |
| `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-workflow.json`                                    | Metadata accuracy               | Update validation hints and workflow notes if behavior changes                              |

### Reader And Mapper Cutover List

These are the concrete C# read paths that should be updated as part of the snake_case-only contract change:

| File                                              | Reader or Mapper Method                      | Current Responsibility                                    | Required Snake Case Change                                                                                                    |
| ------------------------------------------------- | -------------------------------------------- | --------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `Module_Receiving/Data/Dao_ReceivingLabelData.cs` | `GetCurrentLabelDataAsync()`                 | Reads `sp_Receiving_LabelData_GetAll` into `DataRow` rows | Keep the procedure output in snake_case and keep the method aligned with the updated mapper                                   |
| `Module_Receiving/Data/Dao_ReceivingLabelData.cs` | `MapRowToLoad(DataRow row)`                  | Maps current queue rows into `Model_ReceivingLoad`        | Replace PascalCase reads like `LoadID`, `PartDescription`, `PODueDate`, `UserID` with snake_case column names                 |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs`      | `GetHistoryAsync(...)`                       | Reads `sp_Receiving_History_Get` results                  | Expect snake_case history columns only                                                                                        |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs`      | `GetAllAsync(...)`                           | Reads `sp_Receiving_Load_GetAll` results                  | Expect snake_case load/history columns only                                                                                   |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs`      | `MapRowToLoad(DataRow row)`                  | Maps history/load rows into `Model_ReceivingLoad`         | Replace PascalCase reads like `PONumber`, `POVendor`, `QtyOrdered`, `QualityHoldRestrictionType` with snake_case column names |
| `Module_Reporting/Data/Dao_Reporting.cs`          | `GetReceivingHistoryAsync(...)`              | Reads `sp_Reporting_ReceivingHistory_GetByDateRange`      | Keep the reporting procedure output in snake_case and keep the method aligned with the mapper                                 |
| `Module_Reporting/Data/Dao_Reporting.cs`          | `MapReportRowFromReader(IDataReader reader)` | Maps receiving report rows into `Model_ReportRow`         | Read the final receiving-report field set directly by snake_case column name only                                             |

The stored procedures that feed those readers are:

- `sp_Receiving_LabelData_GetAll`
- `sp_Receiving_History_Get`
- `sp_Receiving_Load_GetAll`
- `sp_Reporting_ReceivingHistory_GetByDateRange`

### Dependencies

| File                                                               | Relationship                                                                           |
| ------------------------------------------------------------------ | -------------------------------------------------------------------------------------- |
| `Database/InforVisualScripts/Queries/01_GetPOWithParts.sql`        | Already provides line-level `DueDate` from `PROMISE_DATE`                              |
| `Module_Core/Data/InforVisual/Dao_InforVisualPO.cs`                | Already maps `DueDate` into `Model_InforVisualPOLine`                                  |
| `Module_Core/Models/InforVisual/Model_InforVisualPOLine.cs`        | Already carries `DueDate`                                                              |
| `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Tables.csv`        | Use to confirm table and column ownership before changing any Infor Visual query logic |
| `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_ColumnDetails.csv` | Use to confirm nullability and type details for mapped Infor Visual fields             |
| `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_FKs.csv`           | Use to validate PO header/line and related join paths before changing query logic      |
| `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_PKs.csv`           | Use to validate single-row and composite-key lookup assumptions                        |
| `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Views.csv`         | Use to distinguish actual SQL views from `V_`-prefixed base tables                     |
| `Module_Receiving/Data/Dao_ReceivingLabelData.cs`                  | Already persists the newer queue fields                                                |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs`                       | Already persists the newer queue/load fields                                           |
| `Module_Receiving/Services/Service_QualityHoldWarning.cs`          | Sets quality-hold-required state and restriction type                                  |
| `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`      | Explicitly sets final quality-hold acknowledgment in edit mode                         |

### Test Files

No focused receiving-history or reporting tests were found under `MTM_Receiving_Application.Tests` for this workflow. Treat this as a test gap that should be addressed or explicitly accepted.

### Reference Patterns

| File                                                             | Pattern To Follow                                                                                                                                     |
| ---------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Module_Receiving/Data/Dao_ReceivingLabelData.cs`                | Existing queue persistence for `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, and quality-hold fields |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs`                     | Same persistence pattern through the load DAO path                                                                                                    |
| `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs` | Manual/edit quality-hold final confirmation behavior                                                                                                  |

## Implementation Checklist

### Phase 1: Finalize The History Contract

- [x] Decide the canonical `receiving_history` contract in `Database/Database_Deployment/Sql_Files/Schemas/10_Table_receiving_history.sql`.
- [x] Treat `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, `is_quality_hold_required`, `is_quality_hold_acknowledged`, and `quality_hold_restriction_type` as approved snake_case history fields and carry them through schema, archive, read, and reporting paths.
- [x] Remove `ArchivedAt`, `ArchivedBy`, and `ArchiveBatchID` if no codebase or SQL usage is found that depends on them as business data.
- [x] Update `Database/Database_Deployment/Sql_Files/Migrations/01_Migration_receiving_label_queue_history_alignment.sql` so it removes duplicate PascalCase columns and aligns live databases to the final snake_case-only schema.
- [x] Align or retire `Database/Database_Deployment/Sql_Files/SeedData/04_seed_receiving_history.sql` so it no longer reintroduces schema drift.
- [x] Verify: schema, migration, and seed definitions all describe the same final history table.

### Phase 2: Fix Queue-To-History Archival

- [x] Expand `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql` to archive the newer queue fields into history.
- [x] Preserve the intended `coils_on_skid` versus `packages_per_load` behavior and document the final rule clearly inside the procedure comments.
- [x] Update the archive procedure to target only the final snake_case fields.
- [ ] Verify: clearing label data no longer drops `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, or quality-hold fields.

### Phase 3: Fix Receiving History Read Contracts

- [x] Update `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_Get.sql` so history retrieval reads real archived values instead of hardcoded `NULL`, `0`, or alias-only fallbacks.
- [x] Update `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_GetAll.sql` and `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_GetAll.sql` so queue and load/history reads also return actual snake_case column names only.
- [x] Remove stored-procedure aliases and return actual snake_case column names only.
- [x] Update `Module_Receiving/Data/Dao_ReceivingLabelData.cs` so `GetCurrentLabelDataAsync()` and `MapRowToLoad(DataRow row)` use snake_case column names directly.
- [x] Update `Module_Receiving/Data/Dao_ReceivingLoad.cs` so `GetHistoryAsync(...)`, `GetAllAsync(...)`, and `MapRowToLoad(DataRow row)` use snake_case column names directly.
- [ ] Verify: edit/history views load actual archived PO, user, and quality-hold values.

### Phase 4: Expose The Approved Reporting Fields

- [x] Update `Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql` to expose the approved receiving-report fields.
- [x] Decide whether reporting should use full `created_at` timestamp, `transaction_date`, or both.
- [x] Update `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingHistory_GetByDateRange.sql` to return the final approved field set for Module_Reporting.
- [x] Update `Module_Reporting/Data/Dao_Reporting.cs` so `GetReceivingHistoryAsync(...)` and `MapReportRowFromReader(IDataReader reader)` read the final report field set directly from snake_case column names.
- [ ] Verify: the reporting proc returns `heat`, `vendor_name`, `part_description`, `part_skid_total`, and any newly approved PO, UOM, user, or quality-hold columns.

### Phase 5: Fix PO Due Date Population

- [x] Implement the approved business rule for `po_due_date` as documented in `Database/MappedLogic/Receiving-PO-DueDate-Logic.md`.
- [x] Validate any Infor Visual table, column, PK, FK, or view assumptions for `po_due_date` behavior against the CSV files in `docs/InforVisual/DatabaseCSVFiles/` before changing query logic.
- [x] If the app should use header-level promise date for confirmed POs, extend `Module_Core/Models/InforVisual/Model_InforVisualPO.cs` accordingly and map it in `Module_Core/Services/Database/Service_InforVisualConnect.cs`.
- [x] If the app should use line-level dates, carry the correct line `DueDate` from `Model_InforVisualPOLine` into the part or workflow selection path.
- [x] Update `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs` so it stops setting `_workflowService.CurrentPODueDate = null`.
- [x] Update `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs` so selected PO parts assign `load.PoDueDate` consistently.
- [ ] Verify: guided and manual PO-assisted receiving both populate `PoDueDate` before save.

### Phase 6: Align Quality Hold Behavior Across Modes

- [-] Confirm whether guided mode requires the same final acknowledgment step already used in Manual Entry and Edit Mode.
- [-] If yes, add the missing guided-mode acknowledgment path before Review or Save.
- [x] Ensure the final acknowledged state persists through queue save, history archive, and reporting.
- [ ] Verify: `IsQualityHoldRequired`, `IsQualityHoldAcknowledged`, and `QualityHoldRestrictionType` survive queue save, clear-to-history, and reporting.

### Phase 7: Cleanup And Metadata

- [x] Update `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-workflow.json` if workflow behavior, validation hints, or reporting expectations change.
- [x] Update documentation and DAO/read-model notes so SQL result sets are expected to use snake_case column names only.
- [x] Verify: metadata reflects the actual workflow and reporting behavior after the changes.

## Quick Non-Technical Summary

In plain English, this implementation will make the receiving history more complete and more trustworthy.

- The app will stop losing important receiving details when labels are cleared from the active work queue into history.
- Receiving reports will show more of the information users expect to see, instead of leaving several fields blank.
- Purchase order details such as status, due date, ordered quantity, remaining quantity, and unit of measure will follow the item farther through the process.
- The saved history will better show who processed the receiving and whether a quality hold was required and acknowledged.
- Date information will be handled more clearly so reports can show the right receiving date and, if needed, the actual saved time.
- The database structure will be cleaned up so the app is using one clear version of each field instead of carrying duplicate versions of the same information.

From an end-user point of view, the main result should be simpler: after a receiving transaction is saved and later reported on, the report should look more complete, match what the user entered or selected more closely, and provide a better audit trail for follow-up questions.

## Verification Checklist

- [ ] Deploy the updated schema and procedures to a non-production database.
- [ ] Save receiving queue records from guided mode and manual mode.
- [ ] Clear label data and confirm history rows contain the expected PO, UOM, user, and quality-hold values.
- [ ] Run the receiving reporting procedure for the saved date range and confirm the returned columns match the approved list.
- [ ] Confirm `created_at` and `transaction_date` behavior matches the reporting requirement.
- [x] Build the solution with `dotnet build MTM_Receiving_Application.slnx`.
- [x] Run `dotnet test MTM_Receiving_Application.slnx`.

## Rollback Plan

1. Revert the updated SQL schema, migration, view, and stored procedure files together if the new history contract causes mismatches.
2. Restore the previous archive procedure if clear-to-history starts failing or producing incompatible history rows.
3. Revert the PO due-date model and workflow changes together if guided/manual PO loading becomes inconsistent.
4. If a migration has already been applied to a live database, use a dedicated rollback SQL script rather than relying on source revert alone.

## Risks

- Changing the physical `receiving_history` schema can break any environment that already consumed the duplicate PascalCase columns directly.
- Removing duplicate columns and aliases in one pass can break existing DAO and reporting mappings unless every affected reader is updated together.
- `PODueDate` has both line-level and header-level interpretations, so implementing the wrong source rule can produce incorrect reporting.
- Quality-hold behavior currently differs by mode, so partial fixes can create inconsistent audit trails.
- There is no focused automated coverage for this workflow today, which increases regression risk.

## Agents to Use and When

The recommended agent flow for this work is based on the currently installed plugin agents.

- The `awesome-copilot` plugin contains the main custom agent catalog.
- The `dotnet` plugin directory appears to be mainly skills and supporting assets, not the primary source of custom agents for this task.
- This implementation spans SQL schema, MySQL stored procedures, report shaping, Infor Visual data flow, and some possible WinUI workflow behavior, so sequencing matters.

### Recommended Order

| Order | Agent                                             | Status                                                       | When To Use                                               | Why                                                                                                            |
| ----- | ------------------------------------------------- | ------------------------------------------------------------ | --------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1     | `Context Architect`                               | Ready                                                        | Before any implementation work                            | Best for mapping affected files, dependencies, ordering, and risk across the SQL and C# layers                 |
| 2     | `Expert .NET software engineer mode instructions` | Pending on `Context Architect`                               | Main implementation phase                                 | Best general coding agent for the .NET, MVVM, DAO, and reporting changes in this repo                          |
| 3     | `WinUI 3 Expert`                                  | Pending on `Expert .NET software engineer mode instructions` | Only if the implementation changes receiving UI behavior  | Best for XAML, dialogs, workflow screens, and WinUI-specific rules                                             |
| 4     | `MS-SQL Database Administrator`                   | Pending on `Context Architect`                               | Only if live Infor Visual SQL Server validation is needed | Useful for SQL Server-side verification, but not the primary implementation agent for the MySQL/reporting work |
| 5     | `Doublecheck`                                     | Pending on `Expert .NET software engineer mode instructions` | Final verification phase                                  | Best for checking implementation claims, assumptions, and external references before closing the work          |

### Agent Prompts To Use

#### 1. `Context Architect`

**Intended purpose:** Build the implementation map before code changes begin.

**Prompt:**

```text
Create a context map for implementing the receiving history and reporting changes described in task.md. Identify the exact SQL schema files, MySQL stored procedures, reporting files, DAO mappings, workflow files, and metadata files that must change together. Call out dependency order, test gaps, and any risky ripple effects. Do not implement code yet.
```

#### 2. `Expert .NET software engineer mode instructions`

**Intended purpose:** Perform the main implementation work once the dependency map is stable.

**Status:** Pending on `Context Architect`

**Prompt:**

```text
Implement the receiving history and reporting changes from task.md using the approved context map. Preserve the repo's MVVM, service, and DAO rules. Prioritize the work in this order: history schema alignment, queue-to-history archival updates, history/reporting stored procedure updates, PO due date data flow, and any required metadata updates. Keep changes minimal, consistent, and saIffe.
```

#### 3. `WinUI 3 Expert`

**Intended purpose:** Handle only the UI-facing parts if workflow prompts, dialogs, review screens, or receiving views need changes.

**Status:** Pending on `Expert .NET software engineer mode instructions`

**Prompt:**

```text
Review the receiving implementation changes and determine whether any WinUI 3 view or workflow updates are required for guided, manual, or edit mode because of the new PO due date or quality hold behavior. If UI changes are needed, apply them using WinUI 3 best practices and preserve the existing x:Bind and MVVM patterns.
```

#### 4. `MS-SQL Database Administrator`

**Intended purpose:** Validate the SQL Server and Infor Visual side only when source data behavior needs confirmation.

**Status:** Pending on `Context Architect`

**Prompt:**

```text
Validate the Infor Visual SQL Server source assumptions used by task.md, especially the due date behavior for purchase orders and lines. Confirm which fields and tables provide the usable due date information for standard orders, confirmed orders, and blanket orders. Focus on verification, not app implementation.
```

#### 5. `Doublecheck`

**Intended purpose:** Verify the implementation assumptions and the final reported behavior before sign-off.

**Status:** Pending on `Expert .NET software engineer mode instructions`

**Prompt:**

```text
Verify the key implementation claims from the completed receiving history work. Focus on whether the final solution correctly preserves receiving data from queue to history, exposes the approved report fields, handles PO due date logic consistently, and maintains quality hold auditability. Flag anything that still looks weak, ambiguous, or unverified.
```

### Agents Not Recommended As The Primary Driver

- `Noob Mode Assistant`
  Best for plain-English stakeholder explanations, not for leading a cross-layer implementation.
- `CAST Imaging Structural Quality Advisor Agent`
  Useful for structural risk review, but not the strongest primary implementation agent for this specific SQL-plus-.NET task.

## Suggestions To Resolve Risky Ripple Effects First

If the goal is to reduce the highest-risk ripple effects before implementation starts, this is the safest order.

### 1. Freeze the data contract before touching behavior

Do not start with the reporting proc or the ViewModels.

Start by locking down one approved `receiving_history` contract and writing down exactly which fields are:

- canonical physical columns
- explicitly deferred

Recommended first decisions:

1. Keep snake_case as the only physical target shape for `receiving_history`.
2. Remove PascalCase names from both physical schema and stored-procedure result sets so SQL contracts use snake_case only.
3. Decide now whether `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, `is_quality_hold_required`, `is_quality_hold_acknowledged`, and `quality_hold_restriction_type` are approved history fields or explicitly deferred.
   - these are approved history fields.
4. Decide now whether `ArchivedAt`, `ArchivedBy`, and `ArchiveBatchID` are true business requirements or cleanup candidates.
   - If nothing in the codebase / sql files references them for data remove them.

Why this should happen first:

- it prevents the archive procedure, seed file, view, and DAO mappings from drifting in different directions
- it reduces the chance of making reporting changes against a table contract that will immediately change again

### 2. Use a coordinated snake_case-only rollout

Do not preserve aliases as an intermediate steady state.

Safer rollout suggestion:

1. Standardize all write paths to target the final snake_case fields.
2. Update read procedures to return actual snake_case column names only.
3. Update every affected DAO and reporting mapper in the same change so readers match the new SQL contract immediately.
4. Remove duplicate physical PascalCase columns only as part of that coordinated contract update, not as a later partial cleanup.

Why this should happen first:

- it avoids a long-lived mixed contract where SQL and C# disagree about field names
- it makes the final schema, stored procedures, and mappers converge on one naming standard

### 3. Pick one archive owner and treat the other callers as compatibility wrappers

There are multiple receiving clear-to-history call paths in the codebase.

Recommended rule:

- treat the queue archive behavior as owned by the `receiving_label_data` path
- review every caller of `sp_Receiving_LabelData_ClearToHistory`
- keep other DAO/service wrappers aligned, but do not let them evolve independently

Minimum review target before coding:

- `Module_Receiving/Data/Dao_ReceivingLabelData.cs`
- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Module_Receiving/Services/Service_MySQL_Receiving.cs`
- `Module_Receiving/Services/Service_ReceivingWorkflow.cs`

Why this should happen first:

- the biggest silent regression risk here is updating one wrapper path and forgetting the other
- clear ownership makes later testing and rollback simpler

### 4. Split “data preservation” from “UI behavior changes”

Do not mix these into one implementation wave:

- history/reporting field preservation
- guided-mode quality-hold workflow changes
- due-date UI or step-flow changes

Recommended rule:

1. First preserve the missing fields from queue to history.
2. Then expose them through reporting.
3. Only after that, decide whether guided mode needs a new acknowledgment step or UI checkpoint.

Why this should happen first:

- preserving data is lower-risk and easier to verify than changing wizard behavior
- otherwise a workflow bug can get mistaken for a reporting or archival bug

### 5. Resolve the PO due date rule as a written business decision before coding

`po_due_date` is the highest-risk logic ambiguity in this task.

Approved pre-implementation decision:

1. Line-level due date remains a valid source and should be mapped for both current use and future features.
   - Purchasing can set a PO line due date earlier or later than the overall PO due date.
   - That line-level value should be preserved in the mapped logic documentation for future feature work.
2. Header-level promise date or desired receive date should also be mapped for future features.
   - The current receiving flow does not use that header-level date as the primary saved value, but the source logic should still be documented and preserved.
3. Guided mode should store the selected PO line's due date when it is present.
   - If the selected line-level due date is not blank or `NULL`, store that value.
   - Otherwise, store the header-level promise date.
   - This matches the current UX constraint where the user selects only one PO line row.
4. Manual PO-assisted entry should use the same rule as guided mode.
   - If the selected line-level due date is not blank or `NULL`, store that value.
   - Otherwise, store the header-level promise date.
   - This keeps both PO-assisted paths aligned around a single selected line row.
5. Blanket orders should not get a separate storage rule right now.
   - Map the available line-level and header-level sources in `Database/MappedLogic` for future features.
   - For the current feature set, keep the same source-selection rule described above unless later business rules require a blanket-order-specific override.

Mapped logic reference:

- `Database/MappedLogic/Receiving-PO-DueDate-Logic.md`

Recommended implementation discipline:

- create an assumption or decision file before coding if this is still ambiguous
- do not let `Model_InforVisualPO`, `Service_InforVisualConnect`, `ViewModel_Receiving_POEntry`, and `ViewModel_Receiving_ManualEntry` each invent their own due-date rule

Why this should happen first:

- a wrong due-date rule will produce believable but incorrect reporting
- that is harder to detect than an outright failure

### 6. Make reporting additive first, not redesign-first

Do not redesign the reporting UI first.

Safer reporting sequence:

1. Expand the receiving view and receiving reporting stored procedure to return the approved field set.
2. Update `Dao_Reporting` to map those fields safely.
3. Only then decide which new columns should be visible in preview/export UI by default.
4. If needed, update `Model_ReportRow` and reporting preview column options as a separate step after snake_case result mapping is stable.

Why this should happen first:

- it keeps the first reporting change focused on data correctness
- it reduces the chance of mixing display decisions with data-contract debugging

### 7. Protect import and seed paths from accidental breakage

The history table is not only used by the live queue-to-history archive flow.

Before implementation starts, explicitly review:

- `Database/Database_Deployment/Sql_Files/SeedData/04_seed_receiving_history.sql`
- `Database/Scripts/ImportReceivingHistory/Import-ReceivingHistory.ps1`

Recommended rule:

- any history-schema change is incomplete until seed/import paths are either updated or explicitly declared out of scope

Why this should happen first:

- these are the most likely places for “works in app, fails in maintenance/import workflow” regressions

### 8. Add a small mandatory verification gate before broad implementation

Before broad code changes start, define a minimum go/no-go gate.

Recommended gate:

- one approved history schema contract
- one approved PO due-date rule
- one approved decision on guided-mode quality-hold acknowledgment scope
- one confirmed list of receiving report fields
- one confirmed plan for duplicate-column cleanup timing
- one identified owner for archive path validation
- one explicit statement that focused test coverage is currently missing

If any of those are still unresolved, treat implementation as not ready.

### 9. Keep the first implementation wave intentionally narrow

Recommended first wave:

1. finalize schema contract
2. fix queue-to-history archival
3. fix history read contract
4. fix reporting contract
5. validate with real queue-to-history examples

Recommended second wave:

1. PO due-date propagation changes
2. guided-mode quality-hold parity changes
3. metadata refresh
4. metadata refresh

Why this is safer:

- it reduces the number of moving parts in the first deployment
- it gives a clean checkpoint where data preservation is fixed even if workflow refinement is still pending

### 10. Add explicit reviewer checkpoints for this work

When implementation starts, require review against this short checklist:

1. No ViewModel talks directly to a DAO.
2. No MySQL raw SQL is added in C#.
3. No SQL Server write path is introduced.
4. The archive procedure and both receiving DAO wrappers still agree on the stored procedure contract.
5. The reporting proc output matches `Dao_Reporting` and the report row model using snake_case field names only.
6. The due-date rule is documented, not implied.
7. Quality-hold persistence is verified separately from quality-hold workflow gating.
8. Metadata is updated if workflow behavior changes.

## Recommended “Resolve Risks First” Mini-Plan

If we want a short pre-implementation phase, I would recommend this exact order:

1. Approve the final `receiving_history` column contract.
2. Approve the PO due-date business rule.
3. Approve whether guided mode is in or out for final quality-hold acknowledgment changes in this wave.
4. Plan one coordinated change that removes duplicate physical PascalCase columns and stored-procedure aliases together.
5. Review archive callers and name one clear owner path.
6. Review import and seed compatibility.
7. Start implementation only after those decisions are locked.

## Bottom Line

The safest way to reduce ripple effects is to avoid starting with UI changes or report-column cosmetics.

The first work should narrow ambiguity, freeze the database contract, remove duplicate columns and aliases in one coordinated snake_case-only contract update, and keep the initial implementation wave focused on data correctness before behavior expansion.

## Review Notes

- `[-] Confirm whether guided mode requires the same final acknowledgment step already used in Manual Entry and Edit Mode.`
  I found the explicit final acknowledgment setter in Manual Entry and Edit Mode, but not an equivalent guided-mode confirmation path. This is a product/workflow decision that still needs to be confirmed before code should be added.

- `[-] If yes, add the missing guided-mode acknowledgment path before Review or Save.`
  This remains blocked by the unresolved guided-mode business decision above. Implementing it now would risk changing workflow behavior without approval.

- Open verification items remain intentionally open because they require non-code validation in a deployed database environment, not just source review.

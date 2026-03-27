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

| Requested Field               | In `receiving_label_data` queue | In canonical `receiving_history` schema | Returned by reporting proc now | Current finding                                                        |
| ----------------------------- | ------------------------------- | --------------------------------------- | ------------------------------ | ---------------------------------------------------------------------- |
| `quantity`                    | Yes                             | Yes                                     | Yes                            | Available now                                                          |
| `part_id`                     | Yes                             | Yes                                     | Yes, as `part_number`          | Available now, but aliased                                             |
| `po_number`                   | Yes                             | Yes                                     | Yes                            | Available now                                                          |
| `employee_number`             | Yes                             | Yes                                     | Yes                            | Available now                                                          |
| `heat`                        | Yes                             | Yes                                     | No                             | Stored, but not selected by reporting proc                             |
| `transaction_date`            | Yes                             | Yes                                     | No                             | Reporting uses `created_date`, not raw `transaction_date`              |
| `initial_location`            | Yes                             | Yes                                     | Yes, as `location`             | Available now, but aliased                                             |
| `vendor_name`                 | Yes                             | Yes                                     | No                             | Stored in view, but omitted from reporting proc                        |
| `part_description`            | Yes                             | Yes                                     | No                             | Stored in view, but omitted from reporting proc                        |
| `part_skid_total`             | Yes                             | Yes                                     | No                             | Archived, but omitted from view and reporting proc                     |
| `created_at` with time-of-day | Yes                             | Yes                                     | No                             | View converts to date-only `created_date`; time-of-day is lost         |
| `UnitOfMeasure`               | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |
| `IsQualityHoldRequired`       | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |
| `IsQualityHoldAcknowledged`   | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |
| `QualityHoldRestrictionType`  | Yes                             | No in canonical schema                  | No                             | Queue supports it; history/report do not currently carry it end-to-end |

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

Some PascalCase names in the codebase are not physical duplicate columns. They are result-set aliases used by stored procedures so existing DAO mappers can keep reading names like `PartDescription`, `POVendor`, and `EmployeeNumber`.

Examples:

- `sp_Receiving_LabelData_GetAll.sql`
- `sp_Receiving_History_Get.sql`
- `sp_Receiving_Load_GetAll.sql`

Those aliases are safe and intentional. The actual cleanup target is the physical duplicate history columns created by the migration and reflected in the seed dump.

### Requested duplicate cleanup review

| Duplicate Column  | Correct Column / Action                                   | Current codebase finding                                                              |
| ----------------- | --------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| `PartDescription` | `part_description`                                        | Physical duplicate added by migration; PascalCase alias still used in read procedures |
| `POVendor`        | `vendor_name` for history snapshots, `po_vendor` in queue | Physical duplicate added by migration; PascalCase alias still used in read procedures |
| `EmployeeNumber`  | `employee_number`                                         | Physical duplicate added by migration; PascalCase alias still used in read procedures |
| `ArchivedAt`      | Remove if not needed                                      | Found in migration and seed dump; no active C# usage found                            |
| `ArchivedBy`      | Remove if not needed                                      | Found in migration and seed dump; no active C# usage found                            |
| `ArchiveBatchID`  | Remove if not needed                                      | Found in migration and seed dump; no active C# usage found                            |

### Recommendation

- Keep PascalCase stored-procedure aliases until DAO mapping is standardized.
- Remove the physical duplicate PascalCase columns from `receiving_history` only after the archive procedure and any live database instances are standardized on snake_case fields.
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

| Column              | Current codebase state                                                                             | Why it is null today                                                                                                                                                                                                               | Change needed                                                                                              |
| ------------------- | -------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `POStatus`          | Saved to queue from guided and PO-assisted manual flows                                            | `sp_Receiving_LabelData_ClearToHistory.sql` does not archive it into history; reporting does not return it                                                                                                                         | Archive it into history and expose it in reporting                                                         |
| `PODueDate`         | Queue supports it, but upstream population is incomplete                                           | Guided PO entry explicitly sets `_workflowService.CurrentPODueDate = null`; `Model_InforVisualPO` has no due-date property; service conversion drops line `DueDate`; manual selected-PO path also does not assign `load.PoDueDate` | Decide due-date source rule, extend service model, carry due date onto the load, archive it, and report it |
| `QtyOrdered`        | Saved to queue from current part or selected PO part                                               | Not archived to canonical history and not returned by reporting                                                                                                                                                                    | Archive it into history and expose it in reporting                                                         |
| `UnitOfMeasure`     | Saved to queue, defaults to `EA` when missing                                                      | Not archived to canonical history and not returned by reporting                                                                                                                                                                    | Archive it into history and expose it in reporting                                                         |
| `RemainingQuantity` | Saved to queue from current part or selected PO part                                               | Not archived to canonical history and not returned by reporting                                                                                                                                                                    | Archive it into history and expose it in reporting                                                         |
| `UserID`            | Guided workflow uses `WindowsUsername`; manual entry also backfills from session `WindowsUsername` | Queue supports it, but history archive ignores it and reporting does not expose it                                                                                                                                                 | Archive `user_id` into history and expose it in reporting                                                  |

### Additional note on `PODueDate`

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

## Recommended Implementation Order

1. Standardize `receiving_history` on the intended snake_case schema and remove the physical duplicate PascalCase columns after live DB cleanup.
2. Expand `sp_Receiving_LabelData_ClearToHistory.sql` so it archives the newer queue fields: `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, and the three quality-hold fields.
3. Update `02_View_receiving_history.sql` and `sp_Reporting_ReceivingHistory_GetByDateRange.sql` to return only the approved receiving-report fields.
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
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_Get.sql`                     | Edit/history loader contract    | Stop hardcoding missing values if history starts storing the new fields                     |
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_GetAll.sql`                | Queue loader contract           | Keep aliases consistent if DAO mapping remains PascalCase-based                             |
| `Module_Core/Models/InforVisual/Model_InforVisualPO.cs`                                                              | Service-layer PO model          | Add header-level due-date support if that becomes the chosen contract                       |
| `Module_Core/Services/Database/Service_InforVisualConnect.cs`                                                        | DAO-to-service mapping          | Carry due-date data from DAO results into the service model                                 |
| `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`                                                         | Guided PO load behavior         | Stop nulling PO due date and assign the correct due-date source                             |
| `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`                                                     | Manual PO-assisted selection    | Assign `PoDueDate` when a selected PO part is applied                                       |
| `Module_Receiving/Services/Service_ReceivingWorkflow.cs`                                                             | Guided load generation          | Continue copying PO fields onto loads after the due-date contract is fixed                  |
| `Module_Reporting/Data/Dao_Reporting.cs`                                                                             | Report row mapping              | Map any newly returned columns required by the receiving report UI                          |
| `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-workflow.json`                                    | Metadata accuracy               | Update validation hints and workflow notes if behavior changes                              |

### Dependencies

| File                                                          | Relationship                                                   |
| ------------------------------------------------------------- | -------------------------------------------------------------- |
| `Database/InforVisualScripts/Queries/01_GetPOWithParts.sql`   | Already provides line-level `DueDate` from `PROMISE_DATE`      |
| `Module_Core/Data/InforVisual/Dao_InforVisualPO.cs`           | Already maps `DueDate` into `Model_InforVisualPOLine`          |
| `Module_Core/Models/InforVisual/Model_InforVisualPOLine.cs`   | Already carries `DueDate`                                      |
| `Module_Receiving/Data/Dao_ReceivingLabelData.cs`             | Already persists the newer queue fields                        |
| `Module_Receiving/Data/Dao_ReceivingLoad.cs`                  | Already persists the newer queue/load fields                   |
| `Module_Receiving/Services/Service_QualityHoldWarning.cs`     | Sets quality-hold-required state and restriction type          |
| `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs` | Explicitly sets final quality-hold acknowledgment in edit mode |

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

- [ ] Decide the canonical `receiving_history` contract in `Database/Database_Deployment/Sql_Files/Schemas/10_Table_receiving_history.sql`.
- [ ] Confirm whether `receiving_history` should permanently store `po_status`, `po_due_date`, `qty_ordered`, `unit_of_measure`, `remaining_quantity`, `user_id`, `is_quality_hold_required`, `is_quality_hold_acknowledged`, and `quality_hold_restriction_type` as snake_case columns.
- [ ] Decide whether `ArchivedAt`, `ArchivedBy`, and `ArchiveBatchID` are true business requirements or should be removed.
- [ ] Update `Database/Database_Deployment/Sql_Files/Migrations/01_Migration_receiving_label_queue_history_alignment.sql` so it migrates live databases toward the final schema instead of adding duplicate PascalCase columns.
- [ ] Align or retire `Database/Database_Deployment/Sql_Files/SeedData/04_seed_receiving_history.sql` so it no longer reintroduces schema drift.
- [ ] Verify: schema, migration, and seed definitions all describe the same final history table.

### Phase 2: Fix Queue-To-History Archival

- [ ] Expand `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql` to archive the newer queue fields into history.
- [ ] Preserve the intended `coils_on_skid` versus `packages_per_load` behavior and document the final rule clearly inside the procedure comments.
- [ ] If duplicate PascalCase history columns are being removed, update the archive procedure to target only the final snake_case fields.
- [ ] Verify: clearing label data no longer drops `POStatus`, `PODueDate`, `QtyOrdered`, `UnitOfMeasure`, `RemainingQuantity`, `UserID`, or quality-hold fields.

### Phase 3: Fix Receiving History Read Contracts

- [ ] Update `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_Get.sql` so history retrieval reads real archived values instead of hardcoded `NULL`, `0`, or alias-only fallbacks.
- [ ] Keep PascalCase aliases only where needed for DAO compatibility.
- [ ] Review `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_GetAll.sql` to ensure queue reads still match the DAO contract after any schema cleanup.
- [ ] Verify: edit/history views load actual archived PO, user, and quality-hold values.

### Phase 4: Expose The Approved Reporting Fields

- [ ] Update `Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql` to expose the approved receiving-report fields.
- [ ] Decide whether reporting should use full `created_at` timestamp, `transaction_date`, or both.
- [ ] Update `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingHistory_GetByDateRange.sql` to return the final approved field set for Module_Reporting.
- [ ] Update `Module_Reporting/Data/Dao_Reporting.cs` if new columns are added to the report row mapping.
- [ ] Verify: the reporting proc returns `heat`, `vendor_name`, `part_description`, `part_skid_total`, and any newly approved PO, UOM, user, or quality-hold columns.

### Phase 5: Fix PO Due Date Population

- [ ] Decide the business rule for `PODueDate`.
- [ ] If the app should use header-level promise date for confirmed POs, extend `Module_Core/Models/InforVisual/Model_InforVisualPO.cs` accordingly and map it in `Module_Core/Services/Database/Service_InforVisualConnect.cs`.
- [ ] If the app should use line-level dates, carry the correct line `DueDate` from `Model_InforVisualPOLine` into the part or workflow selection path.
- [ ] Update `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs` so it stops setting `_workflowService.CurrentPODueDate = null`.
- [ ] Update `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs` so selected PO parts assign `load.PoDueDate` consistently.
- [ ] Verify: guided and manual PO-assisted receiving both populate `PoDueDate` before save.

### Phase 6: Align Quality Hold Behavior Across Modes

- [ ] Confirm whether guided mode requires the same final acknowledgment step already used in Manual Entry and Edit Mode.
- [ ] If yes, add the missing guided-mode acknowledgment path before Review or Save.
- [ ] Ensure the final acknowledged state persists through queue save, history archive, and reporting.
- [ ] Verify: `IsQualityHoldRequired`, `IsQualityHoldAcknowledged`, and `QualityHoldRestrictionType` survive queue save, clear-to-history, and reporting.

### Phase 7: Cleanup And Metadata

- [ ] Update `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-workflow.json` if workflow behavior, validation hints, or reporting expectations change.
- [ ] If PascalCase aliases remain temporarily, document that distinction clearly so future schema cleanup does not break DAO mapping.
- [ ] Verify: metadata reflects the actual workflow and reporting behavior after the changes.

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
- [ ] Build the solution with `dotnet build MTM_Receiving_Application.slnx`.
- [ ] Run `dotnet test MTM_Receiving_Application.slnx`.

## Rollback Plan

1. Revert the updated SQL schema, migration, view, and stored procedure files together if the new history contract causes mismatches.
2. Restore the previous archive procedure if clear-to-history starts failing or producing incompatible history rows.
3. Revert the PO due-date model and workflow changes together if guided/manual PO loading becomes inconsistent.
4. If a migration has already been applied to a live database, use a dedicated rollback SQL script rather than relying on source revert alone.

## Risks

- Changing the physical `receiving_history` schema can break any environment that already consumed the duplicate PascalCase columns directly.
- Removing duplicate columns too early can break stored procedures or reports that still expect those fields to exist physically.
- `PODueDate` has both line-level and header-level interpretations, so implementing the wrong source rule can produce incorrect reporting.
- Quality-hold behavior currently differs by mode, so partial fixes can create inconsistent audit trails.
- There is no focused automated coverage for this workflow today, which increases regression risk.

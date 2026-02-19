# Module Receiving — Database Replacement Plan

Last Updated: 2026-02-18

## Goal
Implement a two-table lifecycle for Receiving where workflow completion writes to `label_data` (active label queue for LabelView2022), and `Clear Label Data` moves rows to `history` and clears the active queue.

## Master Branch Spreadsheet Output (Source of Truth)
From `master` `Module_Receiving/Services/Service_XLSWriter.cs`, each line wrote 25 columns:
1. LoadID
2. Load Number
3. Part ID
4. Part Description
5. Part Type
6. PO Number
7. PO Line Number
8. PO Vendor
9. PO Status
10. PO Due Date
11. Qty Ordered
12. Weight/Quantity (lbs)
13. Unit of Measure
14. Heat/Lot Number
15. Remaining Quantity
16. Packages Per Load
17. Package Type
18. Weight Per Package
19. Is Non-PO Item
20. Received Date
21. User ID
22. Employee Number
23. Quality Hold Required
24. Quality Hold Acknowledged
25. Quality Hold Restriction Type

## Required Lifecycle (Authoritative)
1. User completes workflow
2. Save line rows to `receiving_label_data` (active queue consumed by LabelView2022)
3. User clicks `Clear Label Data`
4. Move every row from `receiving_label_data` → `receiving_history`
5. Delete moved rows from `receiving_label_data`

## Gap Analysis (Required columns for Receiving label queue)
### Existing `receiving_label_data` coverage is incomplete
Current table has only a subset (e.g., quantity, part_id, po_number, employee_number, heat, transaction_date, etc.), but the workflow/XLS row had 25 fields.

### Missing from `receiving_label_data` (must be added)
- LoadID
- LoadNumber
- PartDescription
- PartType
- POLineNumber
- PoVendor
- PoStatus
- PoDueDate
- QtyOrdered
- UnitOfMeasure
- RemainingQuantity
- PackagesPerLoad
- PackageTypeName
- WeightPerPackage
- IsNonPOItem
- ReceivedDate (full datetime)
- UserId
- EmployeeNumber (already present but validate type/format compatibility)
- IsQualityHoldRequired
- IsQualityHoldAcknowledged
- QualityHoldRestrictionType

### `receiving_history` target
`receiving_history` should contain the same line-level business columns as `receiving_label_data` (plus audit/archive metadata), so archive does not lose label context.

## Implementation Plan
1. **Schema alignment (queue + archive)**
   - Expand `receiving_label_data` to include all 25 line fields used by workflow/XLS output.
   - Ensure `receiving_history` has matching business fields (plus audit/archive metadata).
   - Keep backward-compatible nullability/defaults during rollout.
2. **Stored procedure updates**
   - Add/update procedures for active queue operations:
     - `sp_Receiving_LabelData_Insert`
     - `sp_Receiving_LabelData_Update` (if edit path writes to active queue)
     - `sp_Receiving_LabelData_ClearToHistory` (transactional move)
   - `ClearToHistory` must perform insert-select into history + delete from active queue in one transaction.
3. **DAO updates**
   - Change workflow save path to write to `receiving_label_data` DAO/SP.
   - Implement clear path DAO method that calls `ClearToHistory` transactional SP.
   - Ensure map/read methods include all line fields in both queue/history paths.
4. **Service and model validation**
   - Confirm `Model_ReceivingLoad` is fully mapped from workflow steps before save.
   - Add null/value guards for optional PO/non-PO scenarios.
5. **Clear Label Data behavior**
   - Wire existing `ResetXLS/Clear` action to `Clear Label Data` semantics.
   - Operation contract: if move succeeds, active queue is empty and all rows are in history.
   - If move fails, no partial data loss (transaction rollback).
6. **Verification**
   - Integration test: workflow complete writes full 25-field row to `receiving_label_data`.
   - Integration test: `Clear Label Data` moves all rows to `receiving_history` and deletes from active queue.
   - Validate history/edit retrieval includes all mapped columns.

## Deliverables
- SQL migrations for `receiving_label_data` + `receiving_history` parity.
- Receiving stored procedures for insert/update/clear-to-history.
- Updated DAO/service mappings in `Module_Receiving` using active queue semantics.
- Tests proving: (1) full-field queue write, (2) atomic queue→history move on clear.

---
applyTo: "Module_Receiving/**/*.{cs,xaml},Database/StoredProcedures/Receiving/**/*.sql,Database/Schemas/*receiving*.sql,Database/00-Test/*Receiving*.sql,Database/Scripts/receiving_label_history_reconciliation.sql,docs/spreadsheet-removal-plans/Module_Receiving*"
---

# Receiving Label Data Lifecycle

## Purpose
Preserve the post-spreadsheet Receiving architecture where label rows are queued in MySQL and archived on explicit clear action.

## Required Lifecycle (Do Not Break)
1. User completes Receiving workflow.
2. Application writes rows to `receiving_label_data` (active queue for LabelView2022).
3. User clicks **Clear Label Data**.
4. System moves queued rows to `receiving_history`.
5. System deletes moved rows from `receiving_label_data`.

## Non-Negotiable Rules
- Workflow save path MUST write to `receiving_label_data`, not directly to `receiving_history`.
- Clear operation MUST be transactional (insert-select to history + delete queue in one transaction).
- Clear operation MUST stamp archive metadata: `ArchivedAt`, `ArchivedBy`, `ArchiveBatchID`.
- If clear fails, queue data MUST remain intact (rollback semantics).

## Field Parity Rules
- `receiving_label_data` and `receiving_history` must preserve the full receiving line payload used by label generation.
- Any added queue business field must be evaluated for matching history persistence.
- DAO mapping must hydrate newly added history columns for edit/history views.

## UI/UX Language Rules
- User-facing wording should use **Clear Label Data** terminology.
- Avoid introducing new **Reset XLS** text in Receiving UI, dialogs, or status messages.

## Validation Artifacts
When lifecycle behavior changes, update and use:
- `Database/00-Test/04-Test-Receiving-LabelData-ClearToHistory.sql`
- `Database/00-Test/05-Test-Receiving-LabelData-Insert-FieldCoverage.sql`
- `Database/Scripts/receiving_label_history_reconciliation.sql`

## Implementation Anchors
- Queue insert SP: `sp_Receiving_LabelData_Insert`
- Queue update SP: `sp_Receiving_LabelData_Update`
- Clear/archive SP: `sp_Receiving_LabelData_ClearToHistory`
- Queue DAO: `Dao_ReceivingLabelData`
- History/read DAO: `Dao_ReceivingLoad`
- Workflow clear action entry point: `Service_ReceivingWorkflow.ResetXLSFilesAsync`

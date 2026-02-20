# TASK003 - Implement Receiving Spreadsheet-Removal Lifecycle

**Status:** Completed  
**Added:** 2026-02-19  
**Updated:** 2026-02-19  
**Completed:** 2026-02-19

## Original Request
Implement Receiving database replacement lifecycle:
- Save workflow rows to `receiving_label_data` (active queue)
- On **Clear Label Data**, move rows to `receiving_history` and clear queue
- Maintain full field parity and provide validation artifacts

## Thought Process
The primary risk was partial migration that changed schema but left behavior inconsistent (e.g., direct history writes, legacy XLS wording, missing archive metadata, incomplete read-model mapping). Work was sequenced by phases: schema, SP lifecycle, DAO/service wiring, UI wording, then validation scripts.

## Implementation Plan
1. Align queue/history schemas for full payload parity
2. Implement queue insert/update and clear-to-history stored procedures
3. Wire DAO/service flow to queue semantics
4. Update UI/status terminology to Clear Label Data
5. Add focused SQL validation scripts and reconciliation script
6. Update read-model parity for history/edit retrieval

## Progress Tracking

**Overall Status:** Completed - 100%

### Subtasks

| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 3.1 | Add receiving schema alignment migration | Complete | 2026-02-19 | Added queue/history parity + archive metadata |
| 3.2 | Add queue lifecycle SPs | Complete | 2026-02-19 | Insert, update, clear-to-history |
| 3.3 | Wire DAO/service to queue model | Complete | 2026-02-19 | Added `Dao_ReceivingLabelData` |
| 3.4 | Replace reset semantics with clear/archive | Complete | 2026-02-19 | `ResetXLSFilesAsync` now archives queue |
| 3.5 | Update Receiving UI/status text | Complete | 2026-02-19 | Clear Label Data terminology |
| 3.6 | Add SQL validation scripts | Complete | 2026-02-19 | Clear transfer + field coverage + reconciliation |
| 3.7 | Complete read-model parity mapping | Complete | 2026-02-19 | Mapper and `sp_Receiving_Load_GetAll` expanded |


## Progress Log
### 2026-02-19
- Added and updated Receiving schema/SP assets for queue/archive lifecycle.
- Added/updated DAO and service flow to write queue rows and archive on clear.
- Updated receiving UI defaults to remove XLS terminology.
- Added focused SQL tests:
  - `04-Test-Receiving-LabelData-ClearToHistory.sql`
  - `05-Test-Receiving-LabelData-Insert-FieldCoverage.sql`
- Added reconciliation script: `receiving_label_history_reconciliation.sql`.
- Expanded history read mapper and `sp_Receiving_Load_GetAll` for new columns.
- Verified with non-interactive builds after major changes.

## Key Files
- `Database/Schemas/38_Migration_receiving_label_queue_history_alignment.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_LabelData_Insert.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_LabelData_Update.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql`
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs`
- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Module_Receiving/Services/Service_MySQL_Receiving.cs`
- `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
- `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`
- `Database/00-Test/04-Test-Receiving-LabelData-ClearToHistory.sql`
- `Database/00-Test/05-Test-Receiving-LabelData-Insert-FieldCoverage.sql`
- `Database/Scripts/receiving_label_history_reconciliation.sql`

## Outcome
Receiving module now follows the required queue/archive lifecycle and has executable SQL validation coverage for both clear transfer and queue field parity.

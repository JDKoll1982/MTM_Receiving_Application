# Active Context

**Last Updated:** 2026-02-19

## Current Work Focus

### Module_Receiving Spreadsheet-Removal Lifecycle (TASK003)
Receiving now follows queue/archive behavior in MySQL:
- Workflow save writes to `receiving_label_data`.
- **Clear Label Data** moves rows to `receiving_history` and clears queue.
- Edit/history retrieval maps newly added archive fields.

## Recent Changes

### Added/Updated: Receiving Queue and Archive Lifecycle

**Database and stored procedure artifacts:**
- `Database/Schemas/38_Migration_receiving_label_queue_history_alignment.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_LabelData_Insert.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_LabelData_Update.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql`
- `Database/StoredProcedures/Receiving/sp_Receiving_Load_GetAll.sql` (expanded field output)

**Application wiring artifacts:**
- `Module_Receiving/Data/Dao_ReceivingLabelData.cs`
- `Module_Receiving/Data/Dao_ReceivingLoad.cs` (read-model parity mapping)
- `Module_Receiving/Services/Service_MySQL_Receiving.cs`
- `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
- `Module_Receiving/Settings/ReceivingSettingsDefaults.cs`

**Validation artifacts:**
- `Database/00-Test/04-Test-Receiving-LabelData-ClearToHistory.sql`
- `Database/00-Test/05-Test-Receiving-LabelData-Insert-FieldCoverage.sql`
- `Database/Scripts/receiving_label_history_reconciliation.sql`

**Governance artifacts:**
- `.github/instructions/receiving-labeldata-lifecycle.instructions.md`

**Build Status:**
- Non-interactive build validation succeeded after implementation changes.

## Next Steps
- Optional: automate execution of focused SQL validation scripts in CI or release checklist.
- Optional: mirror lifecycle guidance for modules adopting similar queue/archive patterns.

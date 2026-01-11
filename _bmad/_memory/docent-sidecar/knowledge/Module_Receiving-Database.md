---
module_name: Module_Receiving
component: database-mapping
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Receiving - Database Schema Details

This file is a companion to the main module doc:

- [_bmad/_memory/docent-sidecar/knowledge/Module_Receiving.md](../docent-sidecar/knowledge/Module_Receiving.md)

## MySQL Tables and Stored Procedures (Observed)

### receiving_history

Used for Guided, Manual, and Edit workflows (save + history queries).

| DAO / Service Path | Stored Procedure | Script Path | Notes |
|--------------------|------------------|-------------|------|
| Dao_ReceivingLoad.SaveLoadsAsync | sp_Receiving_Load_Insert | Database/StoredProcedures/Receiving/sp_Receiving_Load_Insert.sql | Inserts one row per load within a MySQL transaction. |
| Dao_ReceivingLoad.UpdateLoadsAsync | sp_Receiving_Load_Update | Database/StoredProcedures/Receiving/sp_Receiving_Load_Update.sql | Updates one row per load within a MySQL transaction. |
| Dao_ReceivingLoad.DeleteLoadsAsync | sp_Receiving_Load_Delete | Database/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql | Deletes rows by LoadID within a MySQL transaction. |
| Dao_ReceivingLoad.GetHistoryAsync | sp_Receiving_History_Get | Database/StoredProcedures/Receiving/sp_Receiving_History_Get.sql | Returns rows by PartID and date range. |
| Dao_ReceivingLoad.GetAllAsync | sp_Receiving_Load_GetAll | Database/StoredProcedures/Receiving/sp_Receiving_Load_GetAll.sql | Returns rows by date range (Edit Mode history). |

### receiving_package_types

Used for storing a default package type per part.

| DAO | Stored Procedure | Script Path | Notes |
|-----|------------------|-------------|------|
| Dao_PackageTypePreference.GetPreferenceAsync | sp_Receiving_PackageTypePreference_Get | Database/StoredProcedures/Receiving/sp_Receiving_PackageTypePreference_Get.sql | Select by PartID. |
| Dao_PackageTypePreference.SavePreferenceAsync | sp_Receiving_PackageTypePreference_Save | Database/StoredProcedures/Receiving/sp_Receiving_PackageTypePreference_Save.sql | Upsert by PartID (ON DUPLICATE KEY). |
| Dao_PackageTypePreference.DeletePreferenceAsync | sp_Receiving_PackageTypePreference_Delete | Database/StoredProcedures/Receiving/sp_Receiving_PackageTypePreference_Delete.sql | Delete by PartID. |

### receiving_label_data

Used for receiving label line inserts.

| DAO | Stored Procedure | Script Path | Notes |
|-----|------------------|-------------|------|
| Dao_ReceivingLine.InsertReceivingLineAsync | sp_Receiving_Line_Insert | Database/StoredProcedures/Receiving/sp_Receiving_Line_Insert.sql | Uses output params p_Status and p_ErrorMsg. |

## Stored Procedures Referenced but Script Not Found

The following stored procedures are referenced in code but no matching .sql script was found in Database/ (as of this analysis date):

- sp_package_preferences_get_by_user
- sp_package_preferences_upsert

These are used by Dao_PackageTypePreference for user-level preferences (Model_UserPreference).

## Infor Visual (SQL Server) Read-Only Access

Module_Receiving consumes Infor Visual data via `IService_InforVisual` (implemented in Module_Core). This includes:

- PO lookup with parts (used in PO Entry)
- Part lookup by ID (used for Non-PO)
- Remaining quantity and same-day receiving quantity checks

Module_Receiving itself does not execute SQL Server queries directly.

## Risks / Observations

- Dao_ReceivingLoad uses a transaction and throws internally to abort on failures; failures are returned as Model_Dao_Result, but the throw pattern is still worth monitoring.
- Service_MySQL_Receiving (Module_Core) throws on DAO failures; callers need to catch and surface errors.
- User-preference stored procedures are missing from the repo scripts (may be intentional if DB-managed elsewhere).

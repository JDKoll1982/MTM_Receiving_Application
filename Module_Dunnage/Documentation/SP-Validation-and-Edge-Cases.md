# Module_Dunnage — Stored Procedure Validation & Edge Cases

Last Updated: 2025-07-14 (defects 1–6 resolved)

---

## How to Read This Document

Each entry lists the DAO method, the stored procedure it calls, whether the parameter contract matches, and any edge cases or risks discovered. Severity levels used below: **CRITICAL** (will fail at runtime), **HIGH** (data is silently wrong or lost), **MEDIUM** (fragile, may fail under specific conditions), **LOW** (cosmetic or minor risk).

---

## Confirmed Defects

### ✅ 1 — Dao_DunnageType.InsertAsync → sp_dunnage_types_insert — CRITICAL — FIXED

The C# method passes a MySqlParameter named @p_user. The stored procedure declares its third input as p_created_by. These names do not match. MySQL will receive an unrecognised parameter and the call will throw at runtime.

In addition, the C# code expects an OUT parameter @p_new_id to carry back the inserted row ID. The stored procedure does not declare any OUT parameter. Instead it returns the ID via a SELECT LAST_INSERT_ID() result set. The OUT parameter will never be populated, so Convert.ToInt32(pNewId.Value) will either throw a null-reference exception or silently return 0. Every new dunnage type insert will either crash or return a false ID of 0.

Affected operations: Creating a new dunnage type from the Admin screen.

### ✅ 2 — Dao_DunnageUserPreference.UpsertAsync → sp_Dunnage_UserPreferences_Upsert — CRITICAL — FIXED

The C# dictionary sends keys "pref_key" and "pref_value". The AddParameters helper auto-prepends p_ to keys that do not already start with it, producing @p_pref_key and @p_pref_value. The stored procedure declares its parameters as p_preference_key and p_preference_value. Neither name matches.

The stored procedure also declares two OUT parameters (p_status and p_error_msg) that the C# never passes. On platforms where the MySQL connector requires all declared parameters to be bound, the call will fail before any SQL executes.

Net result: user mode preferences and recently used icon data cannot be saved. Errors will surface silently because the DAO returns a failure result rather than throwing.

Affected operations: saving default dunnage mode, saving recently used icons.

### ✅ 3 — Dao_DunnageLine.InsertDunnageLineAsync → sp_Dunnage_Line_Insert — HIGH — FIXED

The stored procedure body consists of two lines: SET p_Status = 1 and SET p_ErrorMsg = 'sp_Dunnage_Line_Insert is deprecated'. It does not insert any data. The C# DAO checks result.Success which will be true, so callers will believe the insert succeeded. Nothing is actually written to the database.

Additionally, Dao_DunnageLine is declared as a static class. The architecture rules explicitly forbid static DAOs. It also obtains its connection string by calling Helper_Database_Variables.GetConnectionString directly, bypassing the injected connection string pattern used by every other DAO.

Affected operations: any legacy path that routes through the old dunnage line insert flow.

### ✅ 4 — Dao_DunnageLoad.InsertAsync / InsertBatchAsync → sp_dunnage_loads_insert / sp_Dunnage_Loads_InsertBatch — HIGH — FIXED

Both stored procedures write only four fields to dunnage_history: load_uuid, part_id, quantity, and the current timestamp. The C# model carries dunnage_type_id, type_name, type_icon, po_number, location, label_number, and specs_json, but none of those reach the database through this path.

The canonical save path introduced later (Dao_DunnageLabelData → sp_Dunnage_LabelData_Insert → sp_Dunnage_LabelData_ClearToHistory) does preserve the full field set. If Service_MySQL_Dunnage.SaveLoadsAsync still routes through the load DAO rather than the label-data DAO, data is permanently lost at every save.

Affected operations: workflow save, manual entry save, edit mode save.

### ✅ 5 — Dao_DunnageLoad MapFromReader — Missing Field Mappings — MEDIUM — FIXED

The stored procedures sp_Dunnage_Loads_GetAll, sp_Dunnage_Loads_GetByDateRange, and sp_Dunnage_Loads_GetById all return type_id and type_name columns via JOIN to dunnage_parts and dunnage_types. The C# MapFromReader function reads load_uuid, part_id, quantity, received_date, created_by, created_date, modified_by, and modified_date only. The type_id and type_name columns are returned from the database but silently discarded. Model_DunnageLoad properties TypeId, TypeName, DunnageType, and TypeIcon will always be null or default when records are loaded from history.

Affected operations: Edit Mode grid (Current Labels from DB), any history display.

---

## Structural and Architecture Issues

### ✅ 6 — Console.WriteLine Connection String Exposure — MEDIUM — FIXED

Dao_DunnageType.GetAllAsync contains two Console.WriteLine calls that print the full connection string and a summary of the result set. Connection strings contain database credentials. If application output is captured by any logging sink, monitoring agent, or crash reporter, credentials will be exposed in plain text.

### ✅ 7 — TOCTOU Race Condition on Type Name Uniqueness — LOW — FIXED

The admin flow calls CheckDuplicateNameAsync before InsertAsync. The stored procedure sp_dunnage_types_insert also enforces uniqueness via an EXISTS check and raises SIGNAL SQLSTATE '45000' if a duplicate is found. Between the two-step check-then-insert pattern in the ViewModel, a concurrent user on a different workstation could insert the same type name. The SP guard would then throw, but the error message surfaces as a generic DAO failure rather than a user-friendly "already exists" message.

### ✅ 8 — sp_Dunnage_Specs_GetAllKeys 50-Key Ceiling — LOW — FIXED

The procedure enumerates JSON object keys using a generated sequence from 1 to 50. Any JSON spec object with more than 50 top-level keys will have keys beyond position 50 silently dropped from the result set. The spec column grid in Manual Entry and Edit Mode would be missing columns.

The sequence ceiling has been raised from 50 to 100. The database is MySQL 5.7 so the JSON_TABLE approach (which would eliminate the ceiling entirely) is not available — that requires MySQL 8.0+. In practice, dunnage spec objects are not expected to approach 100 top-level keys.

### ✅ 9 — sp_dunnage_parts_search Case-Sensitive JSON Value Search — LOW — FIXED

The LIKE pattern inside JSON_SEARCH was applied without LOWER/UPPER normalisation. A search for "foam" would not match a stored value of "Foam".

Fixed by wrapping both the JSON document and the search term in LOWER() before comparison. The part_id LIKE clause was also normalised. Both changes are MySQL 5.7 compatible.

---

## Stored Procedure Parameters — Full Reference

The table below shows each DAO method, the SP it calls, and whether the parameter contract is verified as correct.

| DAO Method | Stored Procedure | Parameter Match |
|---|---|---|
| Dao_DunnageLabelData.InsertBatchAsync (loop) | sp_Dunnage_LabelData_Insert | Confirmed correct — all 12 params match |
| Dao_DunnageLabelData.ClearToHistoryAsync | sp_Dunnage_LabelData_ClearToHistory | Confirmed correct — IN and 4 OUT params match |
| Dao_DunnageLoad.GetAllAsync | sp_Dunnage_Loads_GetAll | Confirmed correct — TypeId and TypeName now mapped |
| Dao_DunnageLoad.GetByDateRangeAsync | sp_Dunnage_Loads_GetByDateRange | Confirmed correct — TypeId and TypeName now mapped |
| Dao_DunnageLoad.GetByIdAsync | sp_Dunnage_Loads_GetById | Confirmed correct — TypeId and TypeName now mapped |
| ~~Dao_DunnageLoad.InsertAsync~~ | ~~sp_dunnage_loads_insert~~ | Removed — dead code, lossy SP deleted (issue 4) |
| ~~Dao_DunnageLoad.InsertBatchAsync~~ | ~~sp_Dunnage_Loads_InsertBatch~~ | Removed — dead code, lossy SP deleted (issue 4) |
| Dao_DunnageLoad.UpdateAsync | sp_dunnage_loads_update | Confirmed correct |
| Dao_DunnageLoad.DeleteAsync | sp_dunnage_loads_delete | Confirmed correct |
| Dao_DunnageType.GetAllAsync | sp_Dunnage_Types_GetAll | Confirmed correct |
| Dao_DunnageType.GetByIdAsync | sp_Dunnage_Types_GetById | Confirmed correct |
| Dao_DunnageType.InsertAsync | sp_dunnage_types_insert | ✅ Fixed — p_user and OUT p_new_id now correct (issue 1) |
| Dao_DunnageType.UpdateAsync | sp_dunnage_types_update | Confirmed correct |
| Dao_DunnageType.DeleteAsync | sp_dunnage_types_delete | Confirmed correct |
| Dao_DunnageType.CountPartsAsync | sp_Dunnage_Types_CountParts | Confirmed correct |
| Dao_DunnageType.CountTransactionsAsync | sp_Dunnage_Types_CountTransactions | Confirmed correct |
| Dao_DunnageType.CheckDuplicateNameAsync | sp_Dunnage_Types_CheckDuplicate | Confirmed correct |
| Dao_DunnagePart.GetAllAsync | sp_Dunnage_Parts_GetAll | Confirmed correct |
| Dao_DunnagePart.GetByTypeAsync | sp_Dunnage_Parts_GetByType | Confirmed correct |
| Dao_DunnagePart.GetByIdAsync | sp_Dunnage_Parts_GetById | Confirmed correct |
| Dao_DunnagePart.InsertAsync | sp_dunnage_parts_insert | Confirmed correct |
| Dao_DunnagePart.UpdateAsync | sp_dunnage_parts_update | Confirmed correct |
| Dao_DunnagePart.DeleteAsync | sp_dunnage_parts_delete | Confirmed correct |
| Dao_DunnagePart.CountTransactionsAsync | sp_Dunnage_Parts_CountTransactions | Confirmed correct |
| Dao_DunnagePart.SearchAsync | sp_dunnage_parts_search | Confirmed correct; see issue 9 for search limitations |
| Dao_DunnageSpec.GetByTypeAsync | sp_Dunnage_Specs_GetByType | Confirmed correct |
| Dao_DunnageSpec.GetAllAsync | sp_Dunnage_Specs_GetAll | Confirmed correct |
| Dao_DunnageSpec.GetByIdAsync | sp_Dunnage_Specs_GetById | Confirmed correct |
| Dao_DunnageSpec.InsertAsync | sp_dunnage_specs_insert | Confirmed correct |
| Dao_DunnageSpec.UpdateAsync | sp_dunnage_specs_update | Confirmed correct |
| Dao_DunnageSpec.DeleteByIdAsync | sp_Dunnage_Specs_DeleteById | Confirmed correct |
| Dao_DunnageSpec.DeleteByTypeAsync | sp_Dunnage_Specs_DeleteByType | Confirmed correct |
| Dao_DunnageSpec.CountPartsUsingSpecAsync | sp_Dunnage_Specs_CountPartsUsingSpec | Confirmed correct |
| Dao_DunnageSpec.GetAllSpecKeysAsync | sp_Dunnage_Specs_GetAllKeys | Confirmed correct; see 50-key ceiling issue 8 |
| Dao_InventoriedDunnage.GetAllAsync | sp_Dunnage_Inventory_GetAll | Confirmed correct |
| Dao_InventoriedDunnage.CheckAsync | sp_Dunnage_Inventory_Check | Confirmed correct |
| Dao_InventoriedDunnage.GetByPartAsync | sp_Dunnage_Inventory_GetByPart | Confirmed correct |
| Dao_InventoriedDunnage.InsertAsync | sp_Dunnage_Inventory_Insert | Confirmed correct |
| Dao_InventoriedDunnage.UpdateAsync | sp_Dunnage_Inventory_Update | Confirmed correct |
| Dao_InventoriedDunnage.DeleteAsync | sp_Dunnage_Inventory_Delete | Confirmed correct |
| Dao_DunnageUserPreference.UpsertAsync | sp_Dunnage_UserPreferences_Upsert | ✅ Fixed — p_pref_key / p_pref_value now correct (issue 2) |
| Dao_DunnageUserPreference.GetRecentlyUsedIconsAsync | sp_Dunnage_UserPreferences_GetRecentIcons | Confirmed correct |
| ~~Dao_DunnageLine.InsertDunnageLineAsync~~ | ~~sp_Dunnage_Line_Insert~~ | Removed — static DAO and deprecated SP deleted (issue 3) |

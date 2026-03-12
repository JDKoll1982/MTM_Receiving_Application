# Module_Receiving — Stored Procedure Validation & Edge Cases

Last Updated: 2025-07-14

---

## How to Read This Document

Each entry lists the DAO method, the stored procedure it calls, whether the parameter contract matches, and any edge cases or risks discovered. Severity levels: **CRITICAL** (will fail at runtime), **HIGH** (data is silently wrong or lost), **MEDIUM** (fragile, may fail under specific conditions), **LOW** (cosmetic or minor risk).

---

## Confirmed Defects

### 1 — Dao_ReceivingLine.InsertReceivingLineAsync → sp_Receiving_Line_Insert — HIGH

The stored procedure is explicitly marked deprecated in its SQL file. Its body does nothing except set p_Status to 1 and set an error message stating that the SP is deprecated and callers should use the receiving_history table. Despite this, the C# DAO calls it, checks result.Success which is true, and reports success. No data is written to any table. Any code path that still routes through Dao_ReceivingLine will silently discard data.

Parameters match the SP signature, so there is no execution error. The defect is pure silent data loss.

Affected operations: any legacy receiving line insertion that hasn't been migrated to the label-data workflow.

### 2 — Dao_PackageTypePreference.GetByUserAsync → sp_package_preferences_get_by_user — CRITICAL

No SQL definition file for sp_package_preferences_get_by_user exists anywhere in the Database/StoredProcedures directory. If this SP has not been deployed to the database, every call to GetByUserAsync will throw a MySQL "Stored procedure does not exist" exception. The C# catch block will return a failure result, meaning user package-type preferences will never load.

### 3 — Dao_PackageTypePreference.UpsertAsync → sp_package_preferences_upsert — CRITICAL

Same issue as above — no SQL definition file for sp_package_preferences_upsert exists in the stored procedures directory. If not deployed, every save of a user package-type preference will fail silently.

### 4 — sp_Receiving_QualityHolds_GetByLoadID Has Undeclared OUT Parameters — MEDIUM

The SP declares two OUT parameters: p_Status and p_ErrorMsg. The C# DAO (Dao_QualityHold.GetQualityHoldsByLoadIDAsync) passes only the IN parameter @p_LoadID. With MySQL Connector/NET and CommandType.StoredProcedure the connector auto-discovers parameters when none are provided, but when parameters are explicitly bound the undeclared OUTs may cause a "parameter not bound" failure on some connector versions. The result is that quality hold history may not load under certain MySQL connector configurations.

### 5 — sp_Receiving_QualityHolds_Insert: LoadID Type Mismatch — MEDIUM

The stored procedure declares p_LoadID as INT. The new receiving workflow identifies loads by GUID (CHAR 36). Model_QualityHold.LoadID is typed as int in C#. If the quality hold feature is wired to the GUID-based load model, the integer ID passed will either be meaningless (default 0) or cause a foreign key violation against a non-existent receiving_history row.

This reflects an incomplete migration from integer load IDs to GUID-based load tracking.

Affected operations: quality hold insertion and retrieval for loads created through the current GUID workflow.

### 6 — sp_Receiving_LabelData_ClearToHistory Archives Fewer Fields Than stored in Label Data — HIGH

The ClearToHistory SP inserts into receiving_history using a fixed column list. The following columns present in receiving_label_data are not carried over to receiving_history:

- weight_quantity (the decimal weight-based quantity used for weighing workflows)
- remaining_quantity (quantity remaining on the PO at time of receipt)
- user_id (the logged-in user who performed the receipt)
- po_vendor, po_status, po_due_date, qty_ordered, unit_of_measure (all PO detail fields)
- package_type_name, packages_per_load, weight_per_package (packaging detail)
- is_quality_hold_required, is_quality_hold_acknowledged, quality_hold_restriction_type

Once the archive/clear operation is performed, these values are permanently gone. The receiving_history table stores a narrow subset of the full receiving event. Any report or audit that reads from history will be missing these fields.

### 7 — Dao_PackageTypePreference.GetPreferenceAsync → sp_Receiving_PackageTypePreference_Get — MEDIUM

The SP does SELECT * FROM receiving_package_types WHERE PartID = p_PartID. The C# mapper reads columns by name: PreferenceID, PartID, PackageTypeName, CustomTypeName, LastModified. If the physical table column names are in different case or named differently from what the mapper expects, the reader.GetOrdinal call will throw an IndexOutOfRangeException at runtime. This can only be confirmed by checking the actual table DDL against the mapper column names.

---

## Structural and Design Issues

### 8 — Dao_ReceivingLoad and Dao_ReceivingLabelData Both Call sp_Receiving_LabelData_Insert — LOW

Both DAOs implement their own SaveLoadsAsync method using identical parameter dictionaries calling the same SP. This is a maintenance hazard. If the SP signature changes or if additional fields need to be added, both methods must be updated in parallel. Any future divergence between the two will produce inconsistent results depending on which code path is used.

### 9 — sp_Receiving_History_Get Returns Hardcoded NULL for Most Fields — LOW

The procedure returns NULL for PartType, POLineNumber, POStatus, PODueDate, QtyOrdered, UnitOfMeasure, RemainingQuantity, PackagesPerLoad, PackageTypeName, WeightPerPackage, UserID, and QualityHold fields. These are nullable in C# and the UI will show blanks for all these columns in history views. This is a structural limitation of what the receiving_history table stores, but it means the "View History" feature provides very little usable detail beyond part ID, quantity, PO number, and date.

---

## Stored Procedure Parameters — Full Reference

| DAO Method | Stored Procedure | Parameter Match |
|---|---|---|
| Dao_ReceivingLabelData.SaveLoadsAsync | sp_Receiving_LabelData_Insert | Confirmed correct — all 33 params match |
| Dao_ReceivingLabelData.ClearLabelDataToHistoryAsync | sp_Receiving_LabelData_ClearToHistory | Confirmed correct — IN and 4 OUT params match |
| Dao_ReceivingLabelData.GetCurrentLabelDataAsync | sp_Receiving_LabelData_GetAll | Confirmed correct — no params |
| Dao_ReceivingLabelData.UpdateCurrentLabelDataAsync | sp_Receiving_LabelData_Update | Confirmed correct — all 31 params match |
| Dao_ReceivingLoad.SaveLoadsAsync | sp_Receiving_LabelData_Insert | Confirmed correct — duplicate of LabelData path |
| Dao_ReceivingLoad.UpdateLoadsAsync | sp_Receiving_Load_Update | Confirmed correct — all 13 params match |
| Dao_ReceivingLoad.DeleteLoadsAsync | sp_Receiving_Load_Delete | Confirmed correct |
| Dao_ReceivingLoad.GetHistoryAsync | sp_Receiving_History_Get | Confirmed correct; see issue 9 for NULL fields |
| Dao_ReceivingLine.InsertReceivingLineAsync | sp_Receiving_Line_Insert | DEFECT — deprecated SP (see issue 1) |
| Dao_QualityHold.InsertQualityHoldAsync | sp_Receiving_QualityHolds_Insert | Params match; LoadID type mismatch (see issue 5) |
| Dao_QualityHold.GetQualityHoldsByLoadIDAsync | sp_Receiving_QualityHolds_GetByLoadID | Undeclared OUT params (see issue 4) |
| Dao_QualityHold.UpdateQualityHoldAcknowledgmentAsync | sp_Receiving_QualityHolds_Update | Confirmed correct |
| Dao_PackageTypePreference.GetPreferenceAsync | sp_Receiving_PackageTypePreference_Get | SELECT * may misalign with mapper (see issue 7) |
| Dao_PackageTypePreference.SavePreferenceAsync | sp_Receiving_PackageTypePreference_Save | Confirmed correct |
| Dao_PackageTypePreference.GetByUserAsync | sp_package_preferences_get_by_user | DEFECT — SP not found in codebase (see issue 2) |
| Dao_PackageTypePreference.UpsertAsync | sp_package_preferences_upsert | DEFECT — SP not found in codebase (see issue 3) |

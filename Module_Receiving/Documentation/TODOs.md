# Module_Receiving — Open TODOs

Last Updated: 2025-07-14

---

## How to Read This Document

Every entry was found by reading source files, searching for TODO or FIXME markers, and identifying stub implementations or deprecated paths that have not yet been replaced. Each item includes the file and method, what the intent appears to be, and whether it is blocking for production use.

---

## ViewModels

### VM-01 — ViewModel_Receiving_ManualEntry — Save & Finish Wiring

File: Module_Receiving\ViewModels\ViewModel_Receiving_ManualEntry.cs

The ViewModel declares all the observable properties and UI text properties required for the Manual Entry grid (add row, remove row, auto-fill, save and finish). The binding infrastructure exists and settings load correctly. However, no calls to the workflow save service were observed in the reviewed code. The completeness of the save path from this ViewModel to the database needs to be confirmed.

Blocking: Potentially yes — if Save & Finish does not persist data the Manual Entry mode is non-functional.

### VM-02 — ViewModel_Receiving_ModeSelection — Default Mode Persistence Path

File: Module_Receiving\ViewModels\ViewModel_Receiving_ModeSelection.cs

The "Set as default mode" checkbox is bound and the mode selection UI is implemented. Whether the selected default mode is actually saved to user preferences (via the settings DAO or package preference DAO) and restored on next launch needs verification against the corresponding service method.

Blocking: Low — does not affect core receiving, only convenience.

---

## Services

### SVC-01 — Service_ReceivingWorkflow — Post-Reset Default Mode Check

File: Module_Receiving\Services\Service_ReceivingWorkflow.cs

After a session reset the service reads the current user's DefaultReceivingMode. The code that consumes this value to pre-select a workflow step exists but was only partially visible in the reviewed area. The full path from reading the preference to navigating to the correct mode step should be confirmed end-to-end.

Blocking: Low — defaults to the standard landing page if not set.

### SVC-02 — IService_ReceivingWorkflow — AddAnotherPart Method

File: Module_Receiving\Contracts\IService_ReceivingWorkflow.cs

The interface documents an AddAnotherPartAsync method for the "Add Another Part/PO" workflow path. Whether this is fully implemented in Service_ReceivingWorkflow and all callers handle it correctly should be verified before the multi-part receiving workflow is considered production-ready.

Blocking: Medium — affects users who need to receive multiple parts in a single session without restarting.

---

## Data Access Layer

### DAO-01 — Dao_ReceivingLine — Deprecated, Routes to No-Op SP

File: Module_Receiving\Data\Dao_ReceivingLine.cs

This DAO calls sp_Receiving_Line_Insert, which is explicitly deprecated and performs no insert. Any code still routing through this DAO silently discards data. See SP-Validation-and-Edge-Cases.md issue 1 for full details.

The DAO should be deleted once all callers have been confirmed to use the label-data or history path instead.

Blocking: Yes if any active path still uses this DAO.

### DAO-02 — Dao_PackageTypePreference — Two SPs with No SQL Definitions

File: Module_Receiving\Data\Dao_PackageTypePreference.cs

GetByUserAsync calls sp_package_preferences_get_by_user and UpsertAsync calls sp_package_preferences_upsert. Neither of these stored procedures has a corresponding SQL definition file in the Database/StoredProcedures directory. The SPs may have been planned but never written, or may exist only in a local database and never been committed.

Until these SPs exist in the database, user-level package type preferences cannot be loaded or saved.

Blocking: Yes for user package preference functionality.

### DAO-03 — Dao_ReceivingLoad.UpdateLoadsAsync — Does Not Preserve All Fields

File: Module_Receiving\Data\Dao_ReceivingLoad.cs

The update method sends only 13 fields to sp_Receiving_Load_Update. The SP only updates part_id, po_number, quantity, heat, transaction_date, label_number, and is_non_po_item in receiving_history. Remaining fields like weight_quantity, packages_per_load, vendor_name, and quality hold status are not updated. If an in-session edit modifies these values, the changes are not persisted unless the label-data update path is also called.

Blocking: Medium — could result in stale data in history after an edit.

---

## Stored Procedures

### SP-01 — sp_Receiving_Line_Insert — Deprecated, Body Is a No-Op

File: Database\StoredProcedures\Receiving\sp_Receiving_Line_Insert.sql

The SP body comments state it is deprecated. It takes all input parameters and returns p_Status = 1 without writing anything. This should be removed once Dao_ReceivingLine is removed.

### SP-02 — sp_Receiving_History_Get — Many Fields Return NULL

File: Database\StoredProcedures\Receiving\sp_Receiving_History_Get.sql

The history query returns NULL for approximately half the columns in the result set because receiving_history does not store those fields. History views are limited to part ID, quantity, PO number, vendor name, employee number, heat, transaction date, and non-PO flag. This is a schema limitation but should be noted as a known gap when displaying receiving history to users.

### SP-03 — sp_Receiving_LabelData_ClearToHistory — Missing Field Archiving

File: Database\StoredProcedures\Receiving\sp_Receiving_LabelData_ClearToHistory.sql

Several receiving_label_data columns are not carried into receiving_history during the archive operation. Weight quantity, remaining quantity, user ID, and all PO detail columns are not preserved. See SP-Validation-and-Edge-Cases.md issue 6 for the full list.

# Module_Dunnage — Open TODOs

Last Updated: 2025-07-14

---

## How to Read This Document

Every entry was found by searching source files for TODO, FIXME, and stub implementations. Each item includes the file and method where the marker lives, what the intent appears to be, and whether it is blocking for production use.

---

## ViewModels

### ✅ VM-01 — ViewModel_Dunnage_WorkFlowViewModel.AddLineAsync — RESOLVED

File: Module_Dunnage\ViewModels\ViewModel_Dunnage_WorkFlowViewModel.cs

The method had no XAML binding, no defined behavior, and no design notes. The dead stub has been removed. If this feature is designed in future, it should be added back as a properly scoped command with a service backing it.

### ✅ VM-02 — ViewModel_Dunnage_EditModeViewModel.LoadFromCurrentLabelsAsync — RESOLVED

File: Module_Dunnage\ViewModels\ViewModel_Dunnage_EditModeViewModel.cs

Implemented. Created sp_Dunnage_LabelData_GetAll.sql, added Dao_DunnageLabelData.GetActiveLabelDataAsync (with full field mapper and JSON spec deserializer), added to IService_MySQL_Dunnage interface, implemented in Service_MySQL_Dunnage, and replaced the no-op stub with a real implementation that mirrors the LoadFromHistoryAsync pattern.

---

## Data Access Layer

### ✅ DAO-01 — Dao_DunnageLine — Deprecated DAO on Deprecated SP — RESOLVED

File: Module_Dunnage\Data\Dao_DunnageLine.cs

Dao_DunnageLine.cs and sp_dunnage_line_Insert.sql have been deleted. The DAO had no registered callers or DI registration.

---

## Services

### ✅ SVC-01 — Service_MySQL_Dunnage.SaveLoadsAsync — Save Path Ambiguity — RESOLVED

File: Module_Dunnage\Services\Service_MySQL_Dunnage.cs

Confirmed. SaveLoadsAsync routes through Dao_DunnageLabelData.InsertBatchAsync (full 12-field set via sp_Dunnage_LabelData_Insert). The legacy Dao_DunnageLoad.InsertAsync and InsertBatchAsync methods and their partial-schema SPs have been removed.

---

## Stored Procedures

### ✅ SP-01 — sp_Dunnage_Line_Insert — Deprecated, Body Is a No-Op — RESOLVED

Deleted alongside Dao_DunnageLine.cs.

### ✅ SP-02 — sp_Dunnage_Loads_Insert / sp_Dunnage_Loads_InsertBatch — Partial Schema Insert — RESOLVED

Both SQL files deleted. Active save path confirmed to be Dao_DunnageLabelData (full field set via sp_Dunnage_LabelData_Insert).

---

## Debug Code Left in Production Classes

### ✅ DBG-01 — Dao_DunnageType — Console.WriteLine with Connection String — RESOLVED

All three Console.WriteLine calls removed from Dao_DunnageType.GetAllAsync.

# Module_Dunnage — Open TODOs

Last Updated: 2025-07-14

---

## How to Read This Document

Every entry was found by searching source files for TODO, FIXME, and stub implementations. Each item includes the file and method where the marker lives, what the intent appears to be, and whether it is blocking for production use.

---

## ViewModels

### VM-01 — ViewModel_Dunnage_WorkFlowViewModel.AddLineAsync — Not Implemented

File: Module_Dunnage\ViewModels\ViewModel_Dunnage_WorkFlowViewModel.cs

The RelayCommand method AddLineAsync contains a single comment "TODO: Implement when ready" and returns immediately. There is no command bound to the UI for this at present, but the command exists in the ViewModel and the RelayCommand attribute generates a public AddLineCommand property that is bindable. If any XAML ever binds to AddLineCommand, pressing the button will do nothing silently.

Blocking: No — not currently bound in any XAML view.

### VM-02 — ViewModel_Dunnage_EditModeViewModel.LoadFromCurrentLabelsAsync — Not Implemented

File: Module_Dunnage\ViewModels\ViewModel_Dunnage_EditModeViewModel.cs

The "Current Labels" button in the Edit Mode view is bound to this command. The method body immediately logs a warning and sets a status message stating the feature is not yet implemented, then returns. The grid stays empty. Users clicking the button see a placeholder message rather than any data.

The intent is to read from the dunnage_label_data active queue. The DAO and stored procedure (Dao_DunnageLabelData, sp_Dunnage_LabelData_Insert) both exist, but there is no corresponding GetAllAsync method on the DAO to populate the Edit Mode grid.

Blocking: Yes for Edit Mode workflow — the primary data-load path for Edit Mode is a no-op.

---

## Data Access Layer

### DAO-01 — Dao_DunnageLine — Deprecated DAO on Deprecated SP

File: Module_Dunnage\Data\Dao_DunnageLine.cs

This DAO is static, which violates the architecture rule requiring all DAOs to be instance-based. It also calls the stored procedure sp_Dunnage_Line_Insert, which is marked deprecated in its SQL definition and performs no database operation (see SP-Validation-and-Edge-Cases.md issue 3).

Any code path that still reaches this DAO will appear to succeed but write nothing. The DAO should either be removed or replaced with an instance-based implementation pointing to the correct table and SP once a new design is agreed.

Blocking: Yes if any workflow still routes through this DAO.

---

## Services

### SVC-01 — Service_MySQL_Dunnage.SaveLoadsAsync — Save Path Ambiguity

File: Module_Dunnage\Services\Service_MySQL_Dunnage.cs

The service method SaveLoadsAsync is called by ReviewViewModel, EditModeViewModel, and ManualEntryViewModel after every successful workflow completion. It is not confirmed whether this method routes through Dao_DunnageLabelData (the label-data queue, full field set) or Dao_DunnageLoad (the legacy history insert, 4-field truncated SP).

If it uses the legacy path, all rich dunnage metadata (type, location, specs, PO number) is silently lost. This needs to be verified and documented.

Blocking: Potentially high — depends on which DAO is called internally.

---

## Stored Procedures

### SP-01 — sp_Dunnage_Line_Insert — Deprecated, Body Is a No-Op

File: Database\StoredProcedures\Dunnage\sp_dunnage_line_Insert.sql

The SP body only sets the status flag and sets an error message naming itself as deprecated. It takes parameters and provides the illusion of an insert but writes nothing. Kept as a stub to avoid breaking callers. Should be removed once all callers are removed or redirected.

### SP-02 — sp_Dunnage_Loads_Insert / sp_Dunnage_Loads_InsertBatch — Partial Schema Insert

File: Database\StoredProcedures\Dunnage\sp_dunnage_loads_insert.sql
File: Database\StoredProcedures\Dunnage\sp_Dunnage_Loads_InsertBatch.sql

These SPs only write 4 columns into dunnage_history. They were written before the full dunnage_label_data schema was finalised. The fuller insert path via sp_Dunnage_LabelData_Insert followed by sp_Dunnage_LabelData_ClearToHistory was added later. Depending on which path is currently active in Service_MySQL_Dunnage, these SPs may be vestigial or still in the active code path causing data loss.

---

## Debug Code Left in Production Classes

### DBG-01 — Dao_DunnageType — Console.WriteLine with Connection String

File: Module_Dunnage\Data\Dao_DunnageType.cs, method GetAllAsync

Two Console.WriteLine calls print the raw connection string (including credentials) and result summary to stdout. This is a security exposure. Should be replaced with structured logger calls at Debug level.

# Module_Dunnage — Stored Procedure & Edge Case Audit v2.0

Last Updated: 2026-03-13

---

## Progress Checklist

| ID | Title | Severity | Status |
|---|---|---|---|
| DB-SP-01 | sp_Dunnage_Specs_GetAllKeys Ceiling Now 100 (MySQL 5.7) | LOW | ⬜ Open |
| DB-SP-02 | sp_Dunnage_LabelData_GetAll Has No GetByUser Variant | LOW | ⬜ Open |
| DB-SP-03 | dunnage_label_data Has No Index on user_id or received_date | MEDIUM | ⬜ Open |
| DB-SP-04 | sp_Dunnage_Parts_Search LIMIT 100 Silently Truncates Results | LOW | ⬜ Open |
| DB-SP-05 | No Stored Procedure Wiring for UpdateLoadAsync / DeleteLoadAsync | HIGH | ✅ Fixed |
| DB-DAO-01 | Dao_DunnageCustomField Has No Service Interface or UI Surface | MEDIUM | ⬜ Open |
| DB-DAO-02 | DeserializeSpecValues Silently Swallows JSON Errors | LOW | ✅ Fixed |
| DB-DAO-03 | Dao_DunnageLoad.GetAllAsync Not Used in Service or ViewModel | LOW | ⬜ Open |
| WF-01 | Workflow Session Not Cleared on Cancel From Review Step | MEDIUM | ✅ Fixed |
| WF-02 | AdvanceToNextStepAsync Does Not Validate DetailsEntry Fields | MEDIUM | ✅ Fixed |
| WF-03 | No Confirmation Dialog Before Clearing Label Queue | HIGH | ✅ Already Resolved |
| WF-04 | StartWorkflowAsync Returns Task but Is Never Awaited | LOW | ⬜ Open |
| UX-01 | Edit Mode Date Filters Trigger Independent DB Calls Without Debounce | LOW | ⬜ Open |
| UX-02 | Duplicate Date Range Setter Commands | LOW | ⬜ Open |
| UX-03 | StatusMessage Not Reset After Successful Load | LOW | ⬜ Open |
| UX-04 | Review Step Single-View Shows No Total Page Indicator | LOW | ⬜ Open |
| UX-05 | IsBusy Not Set During LoadSessionLoads in Review ViewModel | LOW | ⬜ Open |
| ARCH-01 | Model_DunnageSession Uses Manual INotifyPropertyChanged | LOW | ⬜ Open |
| ARCH-02 | Service_DunnageWorkflow Directly References Dao_DunnagePart Namespace | MEDIUM | ⬜ Open |
| ARCH-03 | ViewModel_Dunnage_Review Leaks StepChanged Event Subscription | MEDIUM | ⬜ Open |
| ARCH-04 | Service_MySQL_Dunnage UpdateLoadAsync / DeleteLoadAsync Are Stubs | HIGH | ✅ Fixed |
| ARCH-05 | Dao_DunnageCustomField Registered in DI But No Interface Defined | LOW | ⬜ Open |
| SEC-01 | Model_DunnageSession Stores Full Load List for Session Lifetime | LOW | ⬜ Open |
| PERF-01 | ManualEntry InitializeAsync Loads All Parts on Startup | MEDIUM | ⬜ Open |
| PERF-02 | InventoriedDunnage.CheckAsync Returns Single Row Per Call | LOW | ⬜ Open |

---

## How to Read This Document

This is the v2.0 rewrite of the SP validation audit. It covers the full Module_Dunnage surface area — not just stored procedures — and groups findings by category. Severity levels: **CRITICAL** (will fail at runtime), **HIGH** (data wrong or lost), **MEDIUM** (fragile, fails under specific conditions), **LOW** (minor risk or improvement opportunity).

All 9 issues from v1.0 have been resolved. The items below are new findings from the full codebase scan.

---

## Category: Database — Stored Procedures

### DB-SP-01 — sp_Dunnage_Specs_GetAllKeys Ceiling Now 100 (MySQL 5.7 Constraint) — LOW

The UNION-ALL numbers generator ceiling was raised from 50 to 100 in this round of fixes. On MySQL 5.7 there is no JSON_TABLE function, so a hard ceiling is unavoidable. If spec objects ever have more than 100 top-level keys, the surplus keys will be silently dropped. Upgrading the database to MySQL 8.0 would allow the ceiling to be removed entirely via JSON_TABLE.

Affected operations: Spec column headers in Manual Entry and Edit Mode.

### DB-SP-02 — sp_Dunnage_LabelData_GetAll Has No sp_Dunnage_LabelData_GetByUser Variant — LOW

The new GetActiveLabelDataAsync path returns all rows from the active queue regardless of which user created them. In a multi-workstation environment, User A's unsaved labels are visible in User B's Edit Mode "Current Labels" view. A filtered variant scoped to user_id may be needed for shared-terminal deployments.

Affected operations: Edit Mode — Current Labels tab.

### DB-SP-03 — dunnage_label_data Has No Index on user_id or received_date — MEDIUM

The GetAll query performs a full table scan with ORDER BY received_date. As the queue grows between archive cycles, response time will degrade linearly. An index on (received_date) or (user_id, received_date) would keep reads fast.

Affected operations: Edit Mode — Current Labels load time.

### DB-SP-04 — sp_Dunnage_Parts_Search LIMIT 100 Silently Truncates Results — LOW

The search SP returns at most 100 rows with no indication to the caller that the result set was truncated. If the actual match count exceeds 100, users searching for a broad term will not see all results and will not know results were cut off. The DAO and service do not expose or propagate a truncation warning.

Affected operations: Part search in Admin, Part Selection step of guided workflow.

### DB-SP-05 — No Stored Procedure for UpdateLoadAsync or DeleteLoadAsync — HIGH

`Service_MySQL_Dunnage.UpdateLoadAsync` and `DeleteLoadAsync` both return hard-coded failure strings without calling the DAO. `Dao_DunnageLoad.UpdateAsync` and `DeleteAsync` exist and point to `sp_dunnage_loads_update` and `sp_dunnage_loads_delete`, which are confirmed correct in the v1.0 audit. The service layer simply never calls them. Edit Mode save/delete operations in the UI will always fail.

Affected operations: Edit Mode — save edited row, delete selected rows.

---

## Category: Database — Data Access Objects

### DB-DAO-01 — Dao_DunnageCustomField Has No Service Interface or UI Surface — MEDIUM

`Dao_DunnageCustomField` is registered in DI and has full Insert/GetByType/Update/Delete methods. No entry exists in `IService_MySQL_Dunnage`, no service method wraps it, and no ViewModel calls it. Custom fields are invisible to users. The model `Model_CustomFieldDefinition` and enum plumbing exist, suggesting this was scaffolded but never completed.

Affected operations: Any UI that would allow per-type custom field definitions.

### DB-DAO-02 — DeserializeSpecValues in Dao_DunnageLabelData Silently Swallows JSON Errors — LOW

The private `DeserializeSpecValues` method catches all exceptions from `JsonSerializer.Deserialize` and returns null. If a row in `dunnage_label_data` has malformed `specs_json`, the load will appear in the grid with no specs and no visible indication that specs failed to load. Adding a structured log call in the catch block would make these failures observable.

Affected operations: Edit Mode — Current Labels grid spec column display.

### DB-DAO-03 — Dao_DunnageLoad.GetAllAsync Not Used in Service or ViewModel — LOW

`IService_MySQL_Dunnage.GetAllLoadsAsync` calls `Dao_DunnageLoad.GetAllAsync` which calls `sp_Dunnage_Loads_GetAll`. This reads from `dunnage_history` (archived records). However no ViewModel or command calls `GetAllLoadsAsync`. The method exists on the interface and is implemented but has no consumer.

---

## Category: Workflow

### WF-01 — Workflow Session Not Cleared on Cancel From Review Step — MEDIUM

`Service_DunnageWorkflow.ClearSession` is called when `StartWorkflowAsync` begins. If a user reaches the Review step and then navigates away without saving (e.g., using the system back button or the ReturnToModeSelection command), the session loads from the previous run remain in `CurrentSession.Loads`. Restarting the workflow will call `ClearSession` again via `StartWorkflowAsync`, but navigating directly back to an earlier step (TypeSelection) without going through ModeSelection bypasses the clear.

Affected operations: Any re-entry into the guided workflow without going through ModeSelection.

### WF-02 — AdvanceToNextStepAsync Does Not Validate DetailsEntry Fields — MEDIUM

The workflow step transition from `DetailsEntry` to `Review` calls `AddCurrentLoadToSession()` without validating that required details fields (PO Number, Location, Specs) have been filled in. Users can advance to Review with empty detail fields and the load will save with blank metadata.

Affected operations: Guided workflow — DetailsEntry step.

### WF-03 — No Confirmation Dialog Before Clearing Label Queue — HIGH

`ClearLabelDataAsync` permanently moves all rows from `dunnage_label_data` to `dunnage_history`. There is no confirmation dialog before this action is triggered from the UI. An accidental click archives the entire active print queue. The archived rows can be found in history but the active queue is gone.

Affected operations: Review step — the "Complete / Archive" action.

### WF-04 — StartWorkflowAsync Returns Task but Is Never Awaited — LOW

`StartWorkflowAsync` is declared `Task<bool>` but its callers in ViewModels call it with `await` in some places and fire-and-forget in others. The method body is synchronous (no actual async work) so this is benign today but creates risk if async work is added inside it in future without callers being updated.

---

## Category: User Experience

### UX-01 — Edit Mode Date Filters Trigger Independent Database Calls Without Debounce — LOW

The six date filter commands (`SetFilterLastWeekAsync`, `SetFilterTodayAsync`, `SetFilterThisWeekAsync`, etc.) each call `LoadFromHistoryAsync` directly and independently. There is no debounce or cancellation of a prior in-flight query. If a user rapidly clicks multiple filter buttons, multiple concurrent database calls will be made and the grid will be populated by whichever one finishes last.

Affected operations: Edit Mode — any date filter button sequence.

### UX-02 — Duplicate Date Range Setter Commands — LOW

The ViewModel has two sets of date-range commands that do overlapping things: `SetFilterLastWeekAsync / SetFilterTodayAsync / SetFilterThisWeekAsync` (which load immediately) and `SetDateRangeToday / SetDateRangeLastWeek / SetDateRangeLastMonth` (which only set the dates without loading). This creates inconsistency — some buttons update the grid, others silently update the date fields only. Users pressing "Today" from one surface vs another get different behaviour.

Affected operations: Edit Mode — all date filter controls.

### UX-03 — StatusMessage Not Reset After Successful LoadFromCurrentLabelsAsync — LOW

After loading current labels, StatusMessage shows the row count. If the user then switches to a history filter and the history load fails, the old "Loaded N active label(s)" message briefly persists before being replaced by the error message. The status field should be cleared at the start of every load operation.

### UX-04 — Review Step Single-View Navigation Shows No Total Page Indicator — LOW

The single-entry carousel in the Review step shows `CurrentEntryIndex` but there is no total count label in the view alongside it (e.g., "Entry 2 of 7"). Users cannot see at a glance how many entries remain without scrolling.

Affected operations: Review step — guided workflow single-view mode.

### UX-05 — IsBusy Not Set During LoadSessionLoads in Review ViewModel — LOW

`ViewModel_Dunnage_Review.LoadSessionLoads` is synchronous (no await) and does not set `IsBusy = true` before iterating the session loads. For large sessions this can cause the UI to feel unresponsive briefly without the loading indicator being shown.

---

## Category: Architecture & Code Quality

### ARCH-01 — Model_DunnageSession Uses Manual INotifyPropertyChanged Instead of ObservableObject — LOW

`Model_DunnageSession` implements `INotifyPropertyChanged` manually with a private `SetField<T>` helper method and a manually maintained event. All other observable classes in the module use `ObservableObject` from CommunityToolkit.Mvvm. The manual implementation is functionally correct but inconsistent with the project standard and increases maintenance overhead.

### ARCH-02 — Service_DunnageWorkflow Directly References Dao_DunnagePart Namespace — MEDIUM

`Service_DunnageWorkflow.cs` has a `using MTM_Receiving_Application.Module_Dunnage.Data;` import. The architecture rule states that services must not directly reference DAO types. If this using statement is being consumed to instantiate or type-check any DAO class (rather than just being an unused import), it is an architecture violation. The file should be verified and the using removed if it is unused.

### ARCH-03 — ViewModel_Dunnage_Review Subscribes to StepChanged But Never Unsubscribes — MEDIUM

In the constructor, `_workflowService.StepChanged += OnWorkflowStepChanged` adds an event subscription. There is no `IDisposable.Dispose` or finalizer that calls `-=` to remove it. ViewModels are registered as Transient, so a new instance is created each time the user navigates to Review. Each discarded instance leaks an event subscription that will fire `LoadSessionLoads()` on a stale ViewModel when the step changes, potentially causing exceptions or redundant database reads on collected objects.

Affected operations: Any repeated navigation to the Review step.

### ARCH-04 — Service_MySQL_Dunnage.UpdateLoadAsync and DeleteLoadAsync Are Stubs — HIGH

(See also DB-SP-05.) These methods return `Model_Dao_Result_Factory.Failure(...)` without calling the DAO. The stubs are not marked TODO and give no indication in the interface contract that they are unimplemented. Callers receive a failure result that looks identical to a real database error.

### ARCH-05 — Dao_DunnageCustomField Registered in DI But No Interface Defined — LOW

`Dao_DunnageCustomField` is registered as a Singleton in `ModuleServicesExtensions.cs` and injected into `Service_MySQL_Dunnage`, but `Service_MySQL_Dunnage` does not actually use it — the field is declared but all custom field service methods are absent from the interface. The injection is dead weight.

---

## Category: Security

### SEC-01 — Model_DunnageSession Stores Full Load List in Memory for Session Lifetime — LOW

`Service_DunnageWorkflow` is registered as a Singleton. The `CurrentSession.Loads` collection accumulates across multiple workflow completions if `ClearSession` is not called. In a shared-terminal scenario, a second user could read previous user load data from the session without any authentication check. `ClearSession` should be called reliably on user switch or logout.

---

## Category: Performance

### PERF-01 — ManualEntry InitializeAsync Loads All Parts for All Types on Startup — MEDIUM

`ViewModel_Dunnage_ManualEntry.InitializeAsync` loads all types and all parts at startup. If the dunnage_parts table is large and the user only needs one type's worth of parts, the upfront load is wasteful. Parts should be loaded lazily when a type is selected, matching the pattern in the guided workflow's PartSelection step.

### PERF-02 — InventoriedDunnage.CheckAsync Returns Single Row Per Call — LOW

Every time a part is displayed in the workflow, `IsPartInventoriedAsync` fires a single `sp_Dunnage_Inventory_Check` call per part. If a batch of parts needs inventory checking (e.g., batch import), this results in N round trips. A batch-check SP variant would eliminate this for bulk operations.

---

## Stored Procedure Parameters — Full Reference (Updated)

| DAO Method | Stored Procedure | Status |
|---|---|---|
| Dao_DunnageLabelData.InsertBatchAsync (loop) | sp_Dunnage_LabelData_Insert | ✅ Correct |
| Dao_DunnageLabelData.ClearToHistoryAsync | sp_Dunnage_LabelData_ClearToHistory | ✅ Correct |
| Dao_DunnageLabelData.GetActiveLabelDataAsync | sp_Dunnage_LabelData_GetAll | ✅ New — added this round |
| Dao_DunnageLoad.GetAllAsync | sp_Dunnage_Loads_GetAll | ✅ Correct — TypeId/TypeName mapped |
| Dao_DunnageLoad.GetByDateRangeAsync | sp_Dunnage_Loads_GetByDateRange | ✅ Correct — TypeId/TypeName mapped |
| Dao_DunnageLoad.GetByIdAsync | sp_Dunnage_Loads_GetById | ✅ Correct — TypeId/TypeName mapped |
| Dao_DunnageLoad.UpdateAsync | sp_dunnage_loads_update | ✅ Correct — but service stub blocks it (DB-SP-05) |
| Dao_DunnageLoad.DeleteAsync | sp_dunnage_loads_delete | ✅ Correct — but service stub blocks it (DB-SP-05) |
| Dao_DunnageType.GetAllAsync | sp_Dunnage_Types_GetAll | ✅ Correct |
| Dao_DunnageType.GetByIdAsync | sp_Dunnage_Types_GetById | ✅ Correct |
| Dao_DunnageType.InsertAsync | sp_dunnage_types_insert | ✅ Fixed v1.0 |
| Dao_DunnageType.UpdateAsync | sp_dunnage_types_update | ✅ Correct |
| Dao_DunnageType.DeleteAsync | sp_dunnage_types_delete | ✅ Correct |
| Dao_DunnageType.CountPartsAsync | sp_Dunnage_Types_CountParts | ✅ Correct |
| Dao_DunnageType.CountTransactionsAsync | sp_Dunnage_Types_CountTransactions | ✅ Correct |
| Dao_DunnageType.CheckDuplicateNameAsync | sp_Dunnage_Types_CheckDuplicate | ✅ Correct |
| Dao_DunnagePart.GetAllAsync | sp_Dunnage_Parts_GetAll | ✅ Correct |
| Dao_DunnagePart.GetByTypeAsync | sp_Dunnage_Parts_GetByType | ✅ Correct |
| Dao_DunnagePart.GetByIdAsync | sp_Dunnage_Parts_GetById | ✅ Correct |
| Dao_DunnagePart.InsertAsync | sp_dunnage_parts_insert | ✅ Correct |
| Dao_DunnagePart.UpdateAsync | sp_dunnage_parts_update | ✅ Correct |
| Dao_DunnagePart.DeleteAsync | sp_dunnage_parts_delete | ✅ Correct |
| Dao_DunnagePart.CountTransactionsAsync | sp_Dunnage_Parts_CountTransactions | ✅ Correct |
| Dao_DunnagePart.SearchAsync | sp_dunnage_parts_search | ✅ Correct — case-insensitive fixed v1.0 |
| Dao_DunnageSpec.GetByTypeAsync | sp_Dunnage_Specs_GetByType | ✅ Correct |
| Dao_DunnageSpec.GetAllAsync | sp_Dunnage_Specs_GetAll | ✅ Correct |
| Dao_DunnageSpec.GetByIdAsync | sp_Dunnage_Specs_GetById | ✅ Correct |
| Dao_DunnageSpec.InsertAsync | sp_dunnage_specs_insert | ✅ Correct |
| Dao_DunnageSpec.UpdateAsync | sp_dunnage_specs_update | ✅ Correct |
| Dao_DunnageSpec.DeleteByIdAsync | sp_Dunnage_Specs_DeleteById | ✅ Correct |
| Dao_DunnageSpec.DeleteByTypeAsync | sp_Dunnage_Specs_DeleteByType | ✅ Correct |
| Dao_DunnageSpec.CountPartsUsingSpecAsync | sp_Dunnage_Specs_CountPartsUsingSpec | ✅ Correct |
| Dao_DunnageSpec.GetAllSpecKeysAsync | sp_Dunnage_Specs_GetAllKeys | ✅ Ceiling raised 50→100 (v1.0) |
| Dao_InventoriedDunnage.GetAllAsync | sp_Dunnage_Inventory_GetAll | ✅ Correct |
| Dao_InventoriedDunnage.CheckAsync | sp_Dunnage_Inventory_Check | ✅ Correct |
| Dao_InventoriedDunnage.GetByPartAsync | sp_Dunnage_Inventory_GetByPart | ✅ Correct |
| Dao_InventoriedDunnage.InsertAsync | sp_Dunnage_Inventory_Insert | ✅ Correct |
| Dao_InventoriedDunnage.UpdateAsync | sp_Dunnage_Inventory_Update | ✅ Correct |
| Dao_InventoriedDunnage.DeleteAsync | sp_Dunnage_Inventory_Delete | ✅ Correct |
| Dao_DunnageUserPreference.UpsertAsync | sp_Dunnage_UserPreferences_Upsert | ✅ Fixed v1.0 |
| Dao_DunnageUserPreference.GetRecentlyUsedIconsAsync | sp_Dunnage_UserPreferences_GetRecentIcons | ✅ Correct |
| Dao_DunnageCustomField.InsertAsync | sp_Dunnage_CustomFields_Insert | ⚠️ Params appear correct; never called (DB-DAO-01) |
| Dao_DunnageCustomField.GetByTypeAsync | sp_Dunnage_CustomFields_GetByType | ⚠️ Never called (DB-DAO-01) |

# Assumptions — Reprint from History Feature (Edit Mode)

**Date:** 05/14/2026  
**Feature:** Label Reprint from History → `receiving_label_data`

---

## Assumptions

### 1. New column name is `is_reprint` (TINYINT(1), DEFAULT 0)

**Why needed:** The column name is not specified — only the behavior.  
**Impact if wrong:** Column and SP parameter names must be renamed everywhere.  
**Alternatives considered:** `is_label_reprint`, `is_from_history`, `reprint_flag`.  
**→ Proceeding with `is_reprint`.**

---

### 2. "Only allow a label to be moved from history to label_data once" is enforced by checking for an existing `is_reprint = 1` row in `receiving_label_data` with the same `load_guid` (history `load_guid` = label_data `load_id`)

**Why needed:** The linking key between the two tables is `receiving_history.load_guid` ↔ `receiving_label_data.load_id`. Using this GUID to detect "already queued" is the natural deduplication point.  
**Impact if wrong:** Double-queuing of the same history row would be allowed.  
**Alternatives considered:** A separate flag column on `receiving_history` (e.g., `is_queued_for_reprint`) to block re-selection.  
**→ Proceeding with SP-side GUID check only. No new column on `receiving_history`.**

---

### 3. The new `sp_Receiving_LabelData_InsertFromHistory` SP re-uses the history row's existing `load_guid` as the new `load_id` in `receiving_label_data`, sets `is_reprint = 1`, and relies on the existing `INSERT IGNORE` / duplicate-key guard to prevent double-inserts

**Why needed:** This is the cleanest deduplication strategy given the existing table design.  
**Impact if wrong:** Duplicate rows could appear or inserts could silently fail.

---

### 4. "When the user clears label data" refers to the main Clear Label Data workflow action (archival operation), NOT just Edit Mode row deletion

The `sp_Receiving_LabelData_ClearToHistory` SP is modified so that:
- Rows where `is_reprint = 1` are **NOT copied** to `receiving_history` (already there).
- Rows where `is_reprint = 1` ARE still **deleted** from `receiving_label_data` on clear.

When a user removes a reprint row in Edit Mode via the Remove Row button and clicks Save, that row is still **deleted from `receiving_label_data`** via `sp_Receiving_LabelData_Delete` — the same as any other row. The history record is untouched.

**Impact if wrong:** A normal Delete from Edit Mode might be intended to have different behavior for reprint rows.

---

### 5. The "Reprint from History" button appears in the toolbar only when `CurrentDataSource == History`

**Why needed:** You can only move a row from history to the label queue when you are viewing the History source.  
**Impact if wrong:** Button placement or visibility logic may need to change.

---

### 6. Red text for reprint rows uses the DataGrid `LoadingRow` event in the code-behind

**Why needed:** CommunityToolkit WinUI DataGrid's `DataGridTextColumn` does not support per-row foreground binding without converting to template columns. The `LoadingRow` event is the standard WinUI approach.  
**Impact if wrong:** None — this is purely a presentation decision. Can be changed to converter-based approach if preferred.  
**Color:** `e.Row.ActualTheme == Dark` → `#FF6464` (light red), `Light` → `#C00000` (deep red). Both are readable against system backgrounds in their respective themes. No dependency on quality-hold color; these are new values.

---

### 7. The `IsReprint` property is NOT shown as a dedicated DataGrid column by default (it is indicated by row color instead)

**Why needed:** The red row text already communicates reprint status visually. Adding a column would be redundant.  
**Impact if wrong:** Users might want to filter/sort by reprint status — can add a column later if needed.

---

### 8. Moving a history row to the label queue sets `load_id` = history's `load_guid`, `received_date` = history's `created_at`, and copies all other fields verbatim. The `load_number` is set to the history record's `load_number`

**Why needed:** The target row needs a valid `load_id` for the existing Edit Mode Update/Delete SP flow.  
**Impact if wrong:** Load number display or existing DAO checks could be affected.

---

## Request to User

Please review and confirm (or correct) these 8 assumptions before implementation begins. Reply with any corrections and I will update the plan before writing a single line of code.

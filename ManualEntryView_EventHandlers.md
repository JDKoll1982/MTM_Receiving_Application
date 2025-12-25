# ManualEntryView Event Handlers Analysis

## Current Event Handlers

### 1. `ManualEntryDataGrid_SelectionChanged`
**Purpose:** Intended to trigger edit mode when a row is selected.
**Logic:** Checks if a valid item is selected, then uses `DispatcherQueue` to call `BeginEdit()` if the column is not read-only.
**Issues:** 
- Fires whenever `SelectedItem` changes.
- Can conflict with `CurrentCellChanged`.
- May fire multiple times during navigation.

### 2. `ManualEntryDataGrid_CurrentCellChanged`
**Purpose:** Intended to trigger edit mode when the user moves to a different cell within the same row or a different row.
**Logic:** Uses `DispatcherQueue` to call `BeginEdit()` if the column is not read-only.
**Issues:**
- Redundant with `SelectionChanged` in many cases.
- Aggressive `BeginEdit` can interfere with ComboBox columns (opening/closing immediately).

### 3. `ManualEntryDataGrid_KeyDown`
**Purpose:** Custom keyboard navigation.
**Logic:**
- **Enter:** 
    - Shift+Enter: Handled manually (Move Up).
    - Enter: Default behavior (Move Down).
- **Tab:** Default behavior.
- **Left/Right:** Handled manually (Commit + Move).
**Issues:**
- Mixing default and manual handling can lead to inconsistent behavior.
- "Double moves" likely caused by default behavior + `CurrentCellChanged` or `SelectionChanged` logic firing unexpectedly or multiple times.

### 4. `ManualEntryDataGrid_Tapped`
**Purpose:** Handle clicks on empty space or headers.
**Logic:**
- If grid empty: Add new row, select A1, BeginEdit.
- If header clicked (SelectedItem null): Select A1, BeginEdit.
**Issues:**
- Logic seems sound but might not be reliable if `SelectionChanged` fires first and interferes.

## Proposed Strategy for Refactoring

1.  **Centralize Navigation Logic:** Handle **ALL** navigation keys (Enter, Tab, Arrows) manually in `KeyDown` to prevent default behavior conflicts. Set `e.Handled = true` for all of them.
2.  **Single Entry Point for Edit Mode:** Remove `SelectionChanged` and `CurrentCellChanged` handlers. Instead, trigger `BeginEdit` explicitly at the end of the manual navigation logic in `KeyDown`.
3.  **Pointer/Click Handling:** For mouse clicks, use `PointerPressed` or `Tapped` to detect cell clicks. If a cell is clicked, manually trigger `BeginEdit`. This avoids the "double event" from selection changes.
4.  **ComboBox Handling:** Ensure `BeginEdit` is called, but allow the control to handle the dropdown.
5.  **New Row Handling:** When a new row is added (via button or empty click), explicitly set focus and edit mode on the new row's first cell.

## Column Mapping Plan
- **A:** Load # (ReadOnly)
- **B:** Part ID (Width="*")
- **C:** Weight/Qty
- **D:** Heat/Lot
- **E:** Pkg Type (ComboBox)
- **F:** Pkgs/Load
- **G:** Wt/Pkg (ReadOnly, Calculated)

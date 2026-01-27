# Feature Specification: Manual Entry Mode for Bulk Receiving Data

**Feature Branch**: `003-manual-mode-specification`  
**Created**: 2026-01-25  
**Status**: Draft  
**Input**: Manual Entry Mode enables high-volume receiving scenarios where users enter multiple loads directly into a data grid without navigating through step-by-step workflows, optimized for power users and bulk receiving operations.

## Overview

Manual Entry Mode is a specialized workflow designed for high-volume receiving operations (50+ loads) where users need to enter complete receiving transactions using a spreadsheet-like grid interface. This mode is optimized for experienced users who can efficiently enter data in bulk format without the guided step-by-step experience of Guided Mode. Manual Entry Mode includes all validation, bulk copy operations, and CSV export functionality.

## User Scenarios & Testing

### User Story 1 - High-Volume Bulk Data Entry (Priority: P1)

Manufacturing facility receiving 100+ loads of identical parts needs to enter data quickly using a grid-based interface rather than step-by-step navigation.

**Why this priority**: Core use case for Manual Entry Mode. Many receiving scenarios involve high-volume identical parts that require efficient bulk data entry.

**Independent Test**: Can be tested by entering PO/Part information, switching to Manual Entry Mode, and entering data for 10 loads directly into the grid with validation working correctly. Delivers immediate value by enabling fast bulk entry.

**Acceptance Scenarios**:

1. **Given** I am in Mode Selection, **When** I select "Manual Entry Mode", **Then** I see a grid showing pre-initialized rows based on load count with columns: Load #, Weight/Quantity, Heat Lot, Package Type, Packages Per Load, Validation Status
2. **Given** I am in Manual Entry Mode, **When** I click on the first empty cell in Weight column, **Then** I can begin typing immediately with focus moving down after each entry
3. **Given** I am entering data in the grid, **When** I press Tab, **Then** the focus moves to the next cell to the right, and if I'm at the last column, Tab moves to first column of next row
4. **Given** I am entering data across multiple rows, **When** I press Enter, **Then** the focus moves down to the same column in the next row for efficient vertical data entry
5. **Given** I am entering bulk data, **When** I enter data for Load 1 and then use "Copy All Fields to All Rows" button, **Then** only empty cells in all other rows are filled and occupied cells are preserved
6. **Given** I have entered data in 50 rows, **When** I scroll down in the grid, **Then** the performance remains smooth with virtual scrolling enabled (no freezing)
7. **Given** I am entering load data, **When** I notice a validation error (negative weight), **Then** I see an immediate inline error message next to the invalid cell
8. **Given** I have entered all data, **When** I click "Validate All Rows", **Then** the system performs comprehensive validation and shows summary: "X rows valid, Y rows have errors"
9. **Given** I have completed data entry, **When** I click "Save and Finish", **Then** all data is validated and saved with single save operation to database and CSV
10. **Given** I am using keyboard navigation in the grid, **When** I select multiple rows using Shift+Click, **Then** I can perform bulk operations on selected rows only

## User Story 2 - Efficient Bulk Copy Operations (Priority: P1)

Power user with 50 identical loads needs to enter data once and efficiently apply it to all loads without repetitive manual entry.

**Why this priority**: Critical efficiency feature for Manual Entry Mode use case. Users expect spreadsheet-like copy operations.

**Independent Test**: Can be tested by entering data for Load 1, clicking "Copy All Fields to All Rows", and verifying all rows get the data in empty cells only. Delivers value by enabling fast bulk entry.

**Acceptance Scenarios**:

1. **Given** I am in Manual Entry Mode with empty grid showing 50 rows, **When** I enter data for Row 1 (Weight, Heat Lot, Package Type, Packages) and click "Copy All Fields to All Rows" button, **Then** all 50 rows are filled with Row 1 data in empty cells only, completed in under 1 second
2. **Given** I have partially filled rows (some with heat lots, others with package types), **When** I click "Copy All Fields to All Rows" from Row 1, **Then** only completely empty cells are filled, and any occupied cells are preserved
3. **Given** I am in Manual Entry Mode, **When** I select Row 15 and click "Set as Copy Source", **Then** Row 15 becomes the designated source (shown with highlight or indicator) for subsequent copy operations
4. **Given** Row 15 is set as copy source, **When** I click "Copy All Fields to All Rows", **Then** data copies from Row 15 to empty cells in all other rows
5. **Given** I have used bulk copy to fill multiple rows, **When** I click "Clear AutoFilled Data", **Then** I see options: "Clear All AutoFilled Fields", "Clear AutoFilled Heat Lot Only", "Clear AutoFilled Weight Only", etc.
6. **Given** I select "Clear AutoFilled Weight Only", **When** I confirm, **Then** only auto-filled weight cells are cleared while other auto-filled fields (heat lot, package) are preserved
7. **Given** I am in Manual Entry Mode, **When** I select multiple rows using Shift+Click (e.g., rows 5-10), **Then** I can use "Copy to Selected Rows Only" to bulk copy only to those selected rows
8. **Given** I have used bulk copy and want to verify what will be copied, **When** I click "Preview Copy Operation", **Then** I see a preview showing empty cells (green) and occupied cells (yellow) that will be preserved
9. **Given** I have filled 30 rows with bulk copy and then add 10 new rows, **When** I append rows to the grid, **Then** the new rows automatically receive the copy source data in empty cells with notification "New rows initialized"
10. **Given** I am using bulk copy on a grid with 100+ rows, **When** the copy operation executes, **Then** a progress bar shows "Copying to empty cells in 100 rows..." and completes with summary

## User Story 3 - Toolbar Quick Actions (Priority: P1)

User needs quick access to common operations (Add Row, Remove Row, Validate, Copy) through toolbar buttons to maintain efficient workflow without menu navigation.

**Why this priority**: Essential UX for efficient bulk entry. Toolbar buttons are expected interface pattern for grid-based applications.

**Independent Test**: Can be tested by clicking toolbar buttons and verifying each action works correctly (Add Row increases row count, Remove Row removes selected row, etc.). Delivers value by enabling fast workflow.

**Acceptance Scenarios**:

1. **Given** I am in Manual Entry Mode, **When** I look at the toolbar above the grid, **Then** I see buttons: Add Row, Add Multiple Rows, Remove Row, Set as Copy Source, AutoFill (dropdown), Validate All, and Clear AutoFilled Data (dropdown)
2. **Given** I am viewing the grid with 30 rows, **When** I click the "Add Row" button, **Then** a new empty row is added at the bottom of the grid with Load # auto-incremented
3. **Given** I click "Add Multiple Rows" button, **When** a dialog appears asking "How many rows to add?", **Then** I enter a number and the specified number of rows are added
4. **Given** I have selected Row 5, **When** I click "Remove Row" button, **Then** Row 5 is deleted and subsequent rows are renumbered automatically
5. **Given** I have selected rows 3-8 using Shift+Click, **When** I click "Remove Row", **Then** all selected rows are removed and grid is updated accordingly
6. **Given** I am in the grid, **When** I click the "AutoFill" toolbar button (dropdown), **Then** I see options: Copy All Fields to All Rows, Copy All to Selected, Copy Weight Only, Copy Heat Lot Only, Copy Package Type Only, Copy Packages Only
7. **Given** I have AutoFilled data, **When** I click the "Clear AutoFilled Data" dropdown button, **Then** I see options: Clear All AutoFilled, Clear AutoFilled Weight, Clear AutoFilled Heat Lot, Clear AutoFilled Package Type
8. **Given** I am viewing the grid, **When** I click "Validate All" button in toolbar, **Then** the grid performs comprehensive validation and shows summary box showing: "X rows valid, Y rows with errors" with button to "Jump to Next Error"
9. **Given** I click "Jump to Next Error" in validation summary, **When** multiple errors exist, **Then** grid scrolls to first error and highlights that row, and subsequent clicks cycle through each error
10. **Given** I am using toolbar buttons frequently, **When** I hover over any button, **Then** a tooltip appears explaining the button function and showing keyboard shortcut (if available)

## User Story 4 - Flexible Row Management (Priority: P2)

User needs to dynamically add/remove rows, change row count, and organize data as requirements change during entry process.

**Why this priority**: Supports realistic workflows where load count changes during data entry or rows need to be reorganized.

**Independent Test**: Can be tested by adding rows beyond initial count, removing some rows, and verifying data integrity. Delivers value by enabling flexible data entry workflows.

**Acceptance Scenarios**:

1. **Given** I started with 30 rows, **When** I realize I need 35 loads, **Then** I can click "Add Multiple Rows" and add 5 new rows which are appended with auto-numbered Load #
2. **Given** I have entered data in rows 1-30, **When** I add 5 new rows, **Then** existing data remains intact and new rows are empty and ready for data entry
3. **Given** I have 40 rows but realize load count should be 35, **When** I select rows 36-40 and click "Remove Row", **Then** those rows are deleted
4. **Given** I have removed rows in the middle of the grid (e.g., deleted row 15), **When** the deletion completes, **Then** all Load # values are automatically renumbered to remain sequential
5. **Given** I have partially filled grid with mixed data, **When** I select a range of rows and cut them, **Then** I can paste them to a different location in the grid
6. **Given** I want to move data from Row 5 to Row 10, **When** I select Row 5 and use "Move Row" command, **Then** I can specify target row and data is moved with subsequent rows shifted
7. **Given** I am managing rows in the grid, **When** I right-click on a row header, **Then** I see context menu with options: Insert Row Above, Insert Row Below, Delete Row, Duplicate Row
8. **Given** I click "Duplicate Row" on Row 1, **When** the action completes, **Then** a new row is created with identical data to Row 1 (copy of all fields)
9. **Given** I have made changes to row structure (added/removed rows), **When** I need to revert, **Then** I can click "Undo" to restore previous row configuration
10. **Given** I have a grid with many rows, **When** I need to add rows at a specific location, **Then** I can right-click on a row and insert rows above/below at that position

## User Story 5 - Data Validation and Error Feedback (Priority: P2)

System must provide immediate validation feedback as user enters data to catch errors early and maintain data quality without requiring explicit validation step.

**Why this priority**: Reduces errors and improves data quality. Users expect immediate feedback during data entry.

**Independent Test**: Can be tested by entering invalid data (negative weight, empty heat lot) and verifying error message appears immediately. Delivers value by preventing invalid data entry.

**Acceptance Scenarios**:

1. **Given** I am entering data in a cell, **When** I leave the cell (blur event), **Then** the system validates that specific cell and shows error if invalid (e.g., "Weight must be positive")
2. **Given** I have entered an invalid value, **When** the error appears, **Then** the cell background turns light red and error message appears below the grid
3. **Given** I am viewing validation errors, **When** I click on an error message, **Then** the grid scrolls to that cell and highlights it for easy correction
4. **Given** I have corrected an invalid cell, **When** I leave the cell again, **Then** the error message disappears and cell background returns to normal
5. **Given** I am in Manual Entry Mode, **When** I enter 0 or negative value in Weight column, **Then** immediate error: "Weight must be positive"
6. **Given** I leave Heat Lot cell empty, **When** I exit the row, **Then** error: "Heat Lot is required"
7. **Given** I enter invalid Package Type format, **When** I leave the field, **Then** system suggests valid package types based on part selection
8. **Given** I have multiple validation errors on the page, **When** I click "Validate All", **Then** all errors are shown in summary box with error count by type (e.g., "3 Weight errors, 5 Heat Lot errors")
9. **Given** validation errors exist, **When** I attempt to save, **Then** the save is blocked and system shows message: "Cannot save: X validation errors exist. Fix errors before saving."
10. **Given** I have row with missing required field, **When** I hover over the cell, **Then** tooltip shows "Required field" in red

## User Story 6 - Keyboard Efficiency and Shortcuts (Priority: P3)

Power user needs efficient keyboard shortcuts to navigate grid without mouse, enabling faster data entry for experienced users.

**Why this priority**: Enhances efficiency for power users. Keyboard navigation is expected in spreadsheet-like interfaces.

**Independent Test**: Can be tested by using keyboard shortcuts (Tab, Shift+Tab, Ctrl+C, Ctrl+V) and verifying navigation/copy operations work correctly. Delivers value by enabling expert-mode data entry.

**Acceptance Scenarios**:

1. **Given** I am in Manual Entry Mode grid, **When** I press Tab, **Then** focus moves to next cell to the right
2. **Given** I am at last column, **When** I press Tab, **Then** focus moves to first column of next row
3. **Given** I am in a cell, **When** I press Shift+Tab, **Then** focus moves to previous cell to the left
4. **Given** I am at first column, **When** I press Shift+Tab, **Then** focus moves to last column of previous row
5. **Given** I am in a cell, **When** I press Ctrl+Shift+C (Copy Source), **Then** the current row becomes the copy source
6. **Given** I am in a row, **When** I press Ctrl+K (AutoFill), **Then** a menu appears with copy options
7. **Given** I am in grid with errors, **When** I press Ctrl+E (Jump to Error), **Then** focus moves to first cell with validation error
8. **Given** I want to select a range of rows, **When** I click row 5 and Shift+Click row 10, **Then** rows 5-10 are selected
9. **Given** I have selected rows, **When** I press Delete, **Then** those rows are deleted with confirmation
10. **Given** I am navigating grid, **When** I press Ctrl+A, **Then** all data cells are selected for bulk operations

## Edge Cases

- What happens when user enters 1000+ rows in Manual Entry Mode? (Assumption: Use virtual scrolling for performance) - Correct
- What happens when user selects bulk copy but source row has validation errors? (Assumption: Block copy with tooltip "Source row has validation errors") - Needs Implementation
- How does system handle switching between Manual Entry Mode and Edit Mode? (Assumption: Session data is preserved, user can switch back) - Needs Review
- What happens when user closes Manual Entry Mode without saving? (Assumption: Show warning "You have unsaved data. Save before closing?") - Needs Implementation
- How does system handle copying data from Manual Entry Mode to Guided Mode workflow? (Assumption: Convert grid data structure to step-by-step session) - Needs Review
- What happens when user bulk copy with 100+ rows and network connection drops? (Assumption: Copy operation continues locally, sync when connection restored) - Correct
- How does keyboard shortcut conflict resolution work if shortcuts overlap with OS shortcuts? (Assumption: Use Ctrl+Shift combinations to avoid conflicts) - Needs Implementation

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide grid-based data entry interface for Manual Entry Mode
- **FR-002**: System MUST support dynamic row management (add, remove, insert, delete rows)
- **FR-003**: System MUST provide toolbar with quick-action buttons for common operations
- **FR-004**: System MUST implement efficient bulk copy operations (copy to all/selected rows)
- **FR-005**: System MUST validate data in real-time as user enters values
- **FR-006**: System MUST support keyboard navigation throughout the grid
- **FR-007**: System MUST provide keyboard shortcuts for power users (Tab, Shift+Tab, Ctrl+C, Ctrl+V, etc.)
- **FR-008**: System MUST display immediate error feedback for invalid entries
- **FR-009**: System MUST prevent saving when validation errors exist
- **FR-010**: System MUST show validation summary when "Validate All" is clicked
- **FR-011**: System MUST support row selection (single, range, multiple using Ctrl+Click)
- **FR-012**: System MUST support bulk operations on selected rows only
- **FR-013**: System MUST implement virtual scrolling for grids with 100+ rows
- **FR-014**: System MUST track which data was auto-filled vs. manually entered
- **FR-015**: System MUST support undo/redo operations for row management
- **FR-016**: System MUST auto-renumber Load # when rows are inserted/deleted
- **FR-017**: System MUST display row count and data entry progress
- **FR-018**: System MUST support copy/paste operations between cells
- **FR-019**: System MUST maintain data integrity during bulk operations
- **FR-020**: System MUST export Manual Entry Mode data to CSV with identical format as Guided Mode

### Key Entities

- **ManualEntrySession**: Stores session state for Manual Entry Mode including row data, copy source designation, validation state
- **ManualEntryRow**: Represents single row in grid with fields: Load#, Weight, HeatLot, PackageType, PackagesPerLoad, ValidationErrors, IsAutoFilled metadata
- **AutoFillMetadata**: Tracks which cells were auto-filled and from which source row
- **ValidationState**: Stores validation errors per row/cell with error messages
- **RowSelectionState**: Tracks selected rows for bulk operations
- **KeyboardShortcutMap**: Defines keyboard shortcuts for grid navigation and operations
- **GridState**: Persists grid configuration (column widths, sort order, visible rows)

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can enter 50 loads in Manual Entry Mode in less than 3 minutes using bulk copy
- **SC-002**: Keyboard navigation enables 30% faster data entry compared to mouse-based entry
- **SC-003**: All validation errors are caught before save (0% invalid data saved)
- **SC-004**: Grid remains responsive (no freezing) with 100+ rows
- **SC-005**: Bulk copy operations complete in under 1 second for 100 rows
- **SC-006**: Data exported from Manual Entry Mode matches Guided Mode export format exactly
- **SC-007**: Power users adopt keyboard shortcuts for 80%+ of operations within first week
- **SC-008**: Undo/Redo functionality prevents accidental data loss in 95%+ of scenarios

## Out of Scope

The following items are explicitly OUT OF SCOPE for Manual Entry Mode:

- **Import from External Files**: Importing CSV/Excel data; data entry only manual
- **Advanced Filtering**: Filtering rows by criteria; not needed for single-transaction entry
- **Sorting**: Sorting rows by column; load order must be preserved
- **Conditional Formatting**: Color-coding rows by validation status (error feedback sufficient)
- **Print Preview**: Printing grid data; not required use case
- **Custom Column Layout**: Rearranging columns; fixed column order maintained
- **Data Type Enforcement**: Preventing non-numeric entry in numeric fields; validation provides feedback

## Risks & Mitigation

- **Risk 1**: Performance degradation with very large grid (1000+ rows)
  - *Mitigation*: Implement virtual scrolling, test with large datasets, monitor performance

- **Risk 2**: User mistakes due to fast keyboard entry without immediate validation
  - *Mitigation*: Provide real-time validation, show error feedback immediately, prevent save with errors

- **Risk 3**: Data loss if user closes without saving
  - *Mitigation*: Show unsaved changes warning, implement auto-save, support undo/redo

- **Risk 4**: Keyboard shortcuts conflicting with browser/OS shortcuts
  - *Mitigation*: Use Ctrl+Shift combinations, document shortcuts, provide UI access to all functions

## Notes

- Manual Entry Mode is optimized for high-volume receiving scenarios (50+ loads)
- Grid interface should feel like familiar spreadsheet application
- All validation rules are identical to Guided Mode
- Data from Manual Entry Mode must export identically to Guided Mode
- This specification provides foundation for rebuilding Manual Entry Mode from scratch

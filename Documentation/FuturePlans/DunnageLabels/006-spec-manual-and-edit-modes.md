# Feature Specification:  Dunnage Manual Entry and Edit Modes

**Feature Branch**:  `006-manual-and-edit-modes`  
**Created**: 2025-12-26  
**Status**: Ready for Implementation  
**Parent Feature**:  Dunnage Receiving System V2  
**Depends On**: 004-services-layer

## Overview

Create grid-based data entry modes for power users:  Manual Entry for batch receiving and Edit Mode for reviewing/modifying transaction history.  These modes provide alternative workflows to the wizard for users who prefer spreadsheet-style data entry.

**Architecture Principle**: Follow existing ManualEntryView/EditModeView patterns from Receiving module.  Use CommunityToolkit.WinUI DataGrid with editable columns. 

## User Scenarios & Testing

### User Story 1 - Manual Entry Grid with Batch Operations (Priority: P1)

As a **power user**, I need a DataGrid where I can quickly enter multiple dunnage loads in spreadsheet style with toolbar operations (Add Row, Add Multiple, Remove Row, Auto-Fill) so that I can efficiently process large batches. 

**Why this priority**:  Manual entry is the fastest method for experienced users receiving many items.  Essential for high-volume scenarios.

**Independent Test**: Can be tested by opening manual entry mode, adding rows, filling data in grid cells, using auto-fill, and saving batch to verify all loads are persisted correctly.

**Acceptance Scenarios**: 

1. **Given** user selects Manual Entry mode, **When** view loads, **Then** empty DataGrid appears with columns:  Load#, Type, PartID, Qty, PO, Location, and dynamic spec columns
2. **Given** manual entry grid, **When** user clicks "Add Row", **Then** new empty row is added with auto-incremented LoadNumber
3. **Given** manual entry grid, **When** user clicks "Add Multiple", **Then** dialog prompts for count and optional pre-populate values (Type, PartID, Location)
4. **Given** 10 new rows created, **When** user enters Type and PartID in first row, **Then** spec columns auto-populate with part's spec values
5. **Given** selected row, **When** user clicks "Remove Row", **Then** row is deleted from collection

---

### User Story 2 - Auto-Fill from Part Master Data (Priority: P1)

As a **manual entry user**, I need Auto-Fill to populate spec values from the part's master definition when I select a PartID so that I don't have to manually enter Width, Height, Depth, etc.  every time. 

**Why this priority**: Auto-fill dramatically reduces data entry time and errors. Matches existing receiving workflow pattern.

**Independent Test**: Can be tested by entering a PartID in a row, triggering auto-fill (via button or selection change), and verifying Type and all spec columns are populated from `dunnage_part_numbers. DunnageSpecValues`.

**Acceptance Scenarios**:

1. **Given** empty row, **When** user enters PartID "PALLET-48X40" and tabs out, **Then** Type auto-fills to "Pallet", Width to 48, Height to 40, Depth to 6
2. **Given** row with PartID selected, **When** user clicks "Auto-Fill" button, **Then** spec values refresh from part master data
3. **Given** row with partially filled specs, **When** auto-fill triggers, **Then** only empty spec fields are filled (existing values preserved)
4. **Given** invalid PartID entered, **When** auto-fill attempts, **Then** error message displays "Part ID not found" and fields remain empty
5. **Given** multiple rows with same PartID, **When** auto-fill is applied, **Then** all rows get same spec values from part definition

---

### User Story 3 - Edit Mode with History Loading (Priority: P1)

As a **receiving user**, I need to load and edit historical dunnage transaction records with filtering by date range so that I can correct errors or review recent receiving activity.

**Why this priority**:  Edit mode provides correction workflow and historical visibility. Essential for data quality and auditing.

**Independent Test**: Can be tested by clicking "Load from History", selecting date range, verifying records load in grid, editing values, and saving changes to verify database updates.

**Acceptance Scenarios**:

1. **Given** edit mode view loads, **When** user clicks "Load from History", **Then** date filter dialog appears
2. **Given** date range selected (last 7 days), **When** loading completes, **Then** DataGrid shows all loads within range with checkbox column for selection
3. **Given** loaded history records, **When** user edits Quantity in a row, **Then** cell updates and row is marked as modified
4. **Given** multiple records selected via checkboxes, **When** user clicks "Remove Row", **Then** confirmation dialog asks "Delete X selected records permanently?"
5. **Given** modified records, **When** user clicks "Save Changes", **Then** updates are persisted to database and success message displays

---

### User Story 4 - Edit Mode Data Sources (Priority: P2)

As a **receiving user**, I need to load data from three sources (Current Memory, Current Labels, History) so that I can review unsaved work, re-process labels, or correct historical data.

**Why this priority**:  Multiple data sources provide flexibility for different correction scenarios. Priority P2 because History is most critical. 

**Independent Test**: Can be tested by saving loads to session (Current Memory), exporting CSV (Current Labels), and querying database (History), then verifying each load source returns expected data.

**Acceptance Scenarios**:

1. **Given** wizard workflow has unsaved loads, **When** edit mode "Load from Current Memory" is clicked, **Then** session loads populate grid
2. **Given** CSV file exists at local path, **When** "Load from Current Labels" is clicked, **Then** CSV is parsed and rows populate grid
3. **Given** database has historical records, **When** "Load from History" with date filter is clicked, **Then** database records populate grid
4. **Given** Current Labels CSV missing, **When** load is attempted, **Then** error message displays "No label file found for current user"
5. **Given** Current Memory is empty, **When** load is attempted, **Then** info message displays "No unsaved loads in session"

---

### User Story 5 - Date Filtering and Pagination (Priority: P2)

As a **edit mode user**, I need to filter loads by date range (Last Week, Today, This Week, This Month, This Quarter, Show All) and paginate results (50 per page) so that I can efficiently navigate large datasets.

**Why this priority**: Filtering and pagination are critical for usability with large historical datasets. Priority P2 because basic history loading is P1.

**Independent Test**: Can be tested by loading large dataset, applying date filters, verifying correct records display, using pagination controls, and confirming page navigation works correctly.

**Acceptance Scenarios**:

1. **Given** edit mode with history loaded, **When** user clicks "This Week" filter, **Then** only loads from current week display
2. **Given** custom date range selected (12/01 to 12/26), **When** filter applies, **Then** loads within range are shown
3. **Given** 150 loads in filtered dataset, **When** ItemsPerPage is 50, **Then** TotalPages calculates to 3
4. **Given** page 1 displayed, **When** user clicks "Next Page", **Then** page 2 loads (records 51-100)
5. **Given** page navigation controls, **When** user enters "3" and clicks "Go", **Then** page 3 loads directly

---

### Edge Cases

- What happens when adding multiple rows with count > 100?  (Warn user about performance, confirm action)
- What happens when auto-fill is triggered but part has no spec values in database? (Fill Type only, leave specs blank)
- What happens when editing a load that was deleted by another user? (Database error on save, refresh grid)
- What happens when CSV parse fails due to malformed data? (Error message with line number, don't load partial data)
- What happens when user tries to save with invalid data (qty=0, blank PartID)? (Validation highlights errors, blocks save)

## Requirements

### Functional Requirements - Manual Entry

#### DunnageManualEntryView. xaml + DunnageManualEntryViewModel
- **FR-001**: View MUST display DataGrid with columns: Load#, Type, PartID, Qty, PO, Location, plus dynamic spec columns
- **FR-002**: View MUST provide toolbar with buttons: Add Row, Add Multiple, Remove Row, Auto-Fill
- **FR-003**: View MUST provide "Mode Selection" button to return to mode selection
- **FR-004**: View MUST provide "Save & Finish" button to persist and export
- **FR-005**: ViewModel MUST maintain ObservableCollection<Model_DunnageLoad> Loads
- **FR-006**:  ViewModel MUST provide AddRowCommand that creates new load with auto-incremented LoadNumber
- **FR-007**: ViewModel MUST provide AddMultipleRowsCommand that opens dialog for batch creation
- **FR-008**:  ViewModel MUST provide RemoveRowCommand that deletes selected row from collection
- **FR-009**:  ViewModel MUST provide AutoFillCommand that populates spec values from part master data
- **FR-010**: ViewModel MUST provide SaveCommand that validates, inserts to database, and exports CSV
- **FR-011**: Dynamic spec columns MUST be generated based on union of all spec keys from GetAllSpecKeysAsync
- **FR-012**: Type column MUST be ComboBox populated from GetAllTypesAsync
- **FR-013**: PartID column MUST support manual entry with validation against GetPartByIdAsync

### Functional Requirements - Add Multiple Dialog

#### AddMultipleRowsDialog (ContentDialog)
- **FR-014**: Dialog MUST provide NumberBox for row count (1-100)
- **FR-015**: Dialog MUST provide optional ComboBox for Type pre-population
- **FR-016**: Dialog MUST provide optional ComboBox for PartID pre-population (filtered by Type if selected)
- **FR-017**: Dialog MUST provide optional TextBox for Location pre-population
- **FR-018**: When Type and PartID are selected, **Then** created rows auto-fill with part's spec values
- **FR-019**: When only count is specified, **Then** empty rows are created
- **FR-020**: Dialog MUST validate count > 0 and <= 100

### Functional Requirements - Auto-Fill Logic

- **FR-021**: Auto-Fill MUST trigger when user selects PartID from ComboBox or enters valid PartID and tabs out
- **FR-022**:  Auto-Fill MUST query GetPartByIdAsync to retrieve part definition
- **FR-023**: Auto-Fill MUST populate Type from part's DunnageTypeID
- **FR-024**: Auto-Fill MUST deserialize DunnageSpecValues JSON and populate spec columns
- **FR-025**: Auto-Fill MUST NOT overwrite user-entered values in Quantity, PO, Location columns
- **FR-026**: Auto-Fill MUST handle missing spec keys gracefully (leave cell empty if spec doesn't exist for part)

### Functional Requirements - Edit Mode

#### DunnageEditModeView.xaml + DunnageEditModeViewModel
- **FR-027**: View MUST match EditModeView pattern from Receiving module (toolbar, date filter, grid, footer)
- **FR-028**: View MUST provide "Load Data From" toolbar:  Current Memory, Current Labels, History buttons
- **FR-029**: View MUST provide date filter toolbar: Start/End CalendarDatePicker, preset filter buttons
- **FR-030**: View MUST provide DataGrid with checkbox column for multi-select
- **FR-031**: View MUST provide pagination controls: First, Previous, Page X of Y, Go, Next, Last buttons
- **FR-032**: ViewModel MUST provide LoadFromCurrentMemoryCommand that loads workflow service's CurrentSession. Loads
- **FR-033**:  ViewModel MUST provide LoadFromCurrentLabelsCommand that parses CSV from local path
- **FR-034**: ViewModel MUST provide LoadFromHistoryCommand that calls GetLoadsByDateRangeAsync
- **FR-035**: ViewModel MUST maintain AllLoads (full dataset) and Loads (paginated subset) collections
- **FR-036**:  ViewModel MUST provide SelectAllCommand that toggles IsSelected on all rows
- **FR-037**: ViewModel MUST provide RemoveRowCommand that hard deletes selected rows via DeleteLoadAsync
- **FR-038**:  ViewModel MUST provide SaveChangesCommand that updates modified rows via UpdateLoadAsync

### Functional Requirements - Date Filtering

- **FR-039**: ViewModel MUST provide FilterStartDate and FilterEndDate properties bound to CalendarDatePickers
- **FR-040**: ViewModel MUST provide SetFilterLastWeekCommand (today - 7 days to today)
- **FR-041**: ViewModel MUST provide SetFilterTodayCommand (today 00:00 to today 23:59)
- **FR-042**: ViewModel MUST provide SetFilterThisWeekCommand (Monday to Sunday of current week)
- **FR-043**: ViewModel MUST provide SetFilterThisMonthCommand (first day to last day of current month)
- **FR-044**: ViewModel MUST provide SetFilterThisQuarterCommand (first day to last day of current quarter)
- **FR-045**: ViewModel MUST provide SetFilterShowAllCommand (clear date filters, show all loaded records)
- **FR-046**: Date filter buttons MUST display dynamic text (e.g., "Dec 2024" for This Month, "Q4 2024" for This Quarter)

### Functional Requirements - Pagination

- **FR-047**: ViewModel MUST provide CurrentPage, TotalPages, GotoPageNumber, ItemsPerPage properties
- **FR-048**: ItemsPerPage MUST default to 50
- **FR-049**: TotalPages MUST calculate as Ceiling(FilteredLoads. Count / ItemsPerPage)
- **FR-050**:  Loads collection MUST display items from index ((CurrentPage-1) * ItemsPerPage) to (CurrentPage * ItemsPerPage)
- **FR-051**: ViewModel MUST provide FirstPageCommand, PreviousPageCommand, NextPageCommand, LastPageCommand, GoToPageCommand
- **FR-052**: Navigation commands MUST update CurrentPage and refresh Loads collection
- **FR-053**: Commands MUST disable when already at boundary (e.g., Previous disabled on page 1)

### Validation Requirements

- **FR-054**:  Manual Entry Save MUST validate all rows have PartID, Quantity > 0
- **FR-055**: Manual Entry Save MUST validate all PartIDs exist in dunnage_part_numbers table
- **FR-056**: Edit Mode Remove MUST show confirmation dialog with count of selected rows
- **FR-057**: Edit Mode Save MUST only update rows marked as modified (track changes)
- **FR-058**: CSV parsing MUST handle missing columns gracefully (optional fields default to empty)

## Success Criteria

### Measurable Outcomes - Manual Entry

- **SC-001**: User can add 50 rows in under 10 seconds using "Add Multiple"
- **SC-002**: Auto-fill populates spec values in under 500ms per row
- **SC-003**:  DataGrid renders 100 rows with dynamic spec columns without lag
- **SC-004**: Save operation successfully inserts 100 loads in under 3 seconds
- **SC-005**:  Validation correctly identifies and highlights errors (missing PartID, qty=0)

### Measurable Outcomes - Edit Mode

- **SC-006**:  History loading retrieves 500 records in under 2 seconds
- **SC-007**: Date filters correctly apply and update grid in under 500ms
- **SC-008**:  Pagination navigates between pages with no visible delay
- **SC-009**: CSV parsing successfully loads valid DunnageData.csv files
- **SC-010**: Multi-select and batch delete correctly removes selected rows
- **SC-011**: Current Memory loading retrieves unsaved wizard session data

## Non-Functional Requirements

- **NFR-001**: DataGrid MUST use CommunityToolkit.WinUI. UI. Controls. DataGrid
- **NFR-002**: All grid operations MUST use x: Bind for compile-time binding
- **NFR-003**:  Grid performance MUST support 1000+ rows with pagination
- **NFR-004**:  CSV parsing MUST handle files up to 10MB
- **NFR-005**: Error messages MUST be user-friendly (no technical jargon)
- **NFR-006**: All commands MUST disable when operation is invalid (e.g., Remove with no selection)

## Out of Scope

- ❌ Inline cell validation (validate on Save only)
- ❌ Excel import/export (CSV only)
- ❌ Column reordering/hiding (fixed column layout)
- ❌ Grid row sorting by clicking headers (pagination order only)
- ❌ Undo/Redo for grid edits (explicit save/cancel only)
- ❌ Soft delete in Edit Mode (hard delete with confirmation)

## Dependencies

- 004-services-layer (IService_MySQL_Dunnage, IService_DunnageCSVWriter, IService_DunnageWorkflow)
- NuGet:  CommunityToolkit.WinUI.UI.Controls (for DataGrid)
- NuGet: CsvHelper (for CSV parsing in Edit Mode)
- Project:  BaseViewModel
- Existing: ManualEntryView/EditModeView patterns from Receiving module

## Files to be Created

### Manual Entry
- `Views/Receiving/DunnageManualEntryView.xaml`
- `Views/Receiving/DunnageManualEntryView.xaml.cs`
- `ViewModels/Receiving/DunnageManualEntryViewModel.cs`
- `Views/Receiving/Dialogs/AddMultipleRowsDialog.xaml`
- `Views/Receiving/Dialogs/AddMultipleRowsDialog.xaml.cs`

### Edit Mode
- `Views/Receiving/DunnageEditModeView.xaml`
- `Views/Receiving/DunnageEditModeView.xaml.cs`
- `ViewModels/Receiving/DunnageEditModeViewModel.cs`

## Review & Acceptance Checklist

### Requirement Completeness
- [x] Manual Entry grid structure and toolbar operations are fully specified
- [x] Auto-fill logic from part master data is clearly defined
- [x] Edit Mode data sources (Memory, Labels, History) are enumerated
- [x] Date filtering presets and logic are explicit
- [x] Pagination calculation and navigation are detailed

### Clarity & Unambiguity
- [x] DataGrid column definitions are specified
- [x] Add Multiple dialog inputs are defined
- [x] Auto-fill trigger conditions are clear (PartID selection/entry)
- [x] Date filter preset date ranges are explicit
- [x] Pagination item count (50 per page) is specified

### Testability
- [x] Manual Entry can be tested by adding/filling/saving rows
- [x] Edit Mode can be tested by loading from each source and verifying data
- [x] Success criteria are measurable (timing, record counts, validation accuracy)
- [x] Edge cases define error scenarios

### Pattern Alignment
- [x] Follows existing ManualEntryView. xaml pattern (toolbar, grid, save button)
- [x] Follows existing EditModeView.xaml pattern (load toolbar, date filter, pagination)
- [x] Uses same DataGrid component and binding patterns
- [x] Uses same command naming conventions

## Clarifications

### Resolved Questions

**Q1**: Should Auto-Fill copy from last transaction or part master data?  
**A1**: Part master data (`dunnage_part_numbers. DunnageSpecValues`). This is consistent, unlike Receiving which uses last transaction history.

**Q2**: Should manual entry validate PartID on cell exit or on Save?  
**A2**: On Save.  Allow fast data entry without interruption.  Batch validate before persistence.

**Q3**: Should Edit Mode support inline editing of spec values?  
**A3**: Yes.  Spec columns are editable.  Update `DunnageSpecValues` JSON on save.

**Q4**: Should CSV parsing in Edit Mode validate against database?   
**A4**: Basic parsing only. Validate PartID and Type exist when user clicks Save, not on load.

**Q5**: Should pagination preserve selections when changing pages?  
**A5**:  No. Selections clear on page change. Simplifies state management.

**Q6**: Should date filter apply immediately or require "Apply" button?  
**A6**:  Apply immediately when preset button clicked. Manual date range applies on CalendarDatePicker value change. 
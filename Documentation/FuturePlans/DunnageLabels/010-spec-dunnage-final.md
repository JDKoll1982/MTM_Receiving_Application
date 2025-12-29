# Feature Specification: Dunnage Receiving System - Complete Implementation

**Feature Branch**: `010-dunnage-final`  
**Created**: 2025-12-28  
**Status**: Ready for Implementation  
**Input**: Unified specification combining Manual Entry/Edit Modes, Admin Interface, CSV Export Integration, and Add New Type Dialog

## User Scenarios & Testing *(mandatory)*

<!--
  User stories are PRIORITIZED as user journeys ordered by importance (P1, P2, P3).
  Each user story is INDEPENDENTLY TESTABLE and delivers standalone value.
-->

### User Story 1 - Manual Entry Grid with Batch Operations (Priority: P1)

As a **power user**, I need a DataGrid where I can quickly enter multiple dunnage loads in spreadsheet style with toolbar operations (Add Row, Add Multiple, Remove Row, Auto-Fill) so that I can efficiently process large batches.

**Why this priority**: Manual entry is the fastest method for experienced users receiving many items. Essential for high-volume scenarios.

**Independent Test**: Can be tested by opening manual entry mode, adding rows, filling data in grid cells, using auto-fill, and saving batch to verify all loads are persisted correctly.

**Acceptance Scenarios**:

1. **Given** user selects Manual Entry mode, **When** view loads, **Then** empty DataGrid appears with columns: Load#, Type, PartID, Qty, PO, Location, and dynamic spec columns
2. **Given** manual entry grid, **When** user clicks "Add Row", **Then** new empty row is added with auto-incremented LoadNumber
3. **Given** manual entry grid, **When** user clicks "Add Multiple", **Then** dialog prompts for count and optional pre-populate values (Type, PartID, Location)
4. **Given** 10 new rows created, **When** user enters Type and PartID in first row, **Then** spec columns auto-populate with part's spec values
5. **Given** selected row, **When** user clicks "Remove Row", **Then** row is deleted from collection

---

### User Story 2 - Auto-Fill from Part Master Data (Priority: P1)

As a **manual entry user**, I need Auto-Fill to populate spec values from the part's master definition when I select a PartID so that I don't have to manually enter Width, Height, Depth, etc. every time.

**Why this priority**: Auto-fill dramatically reduces data entry time and errors. Matches existing receiving workflow pattern.

**Independent Test**: Can be tested by entering a PartID in a row, triggering auto-fill (via button or selection change), and verifying Type and all spec columns are populated from `dunnage_part_numbers.DunnageSpecValues`.

**Acceptance Scenarios**:

1. **Given** empty row, **When** user enters PartID "PALLET-48X40" and tabs out, **Then** Type auto-fills to "Pallet", Width to 48, Height to 40, Depth to 6
2. **Given** row with PartID selected, **When** user clicks "Auto-Fill" button, **Then** spec values refresh from part master data
3. **Given** row with partially filled specs, **When** auto-fill triggers, **Then** only empty spec fields are filled (existing values preserved)
4. **Given** invalid PartID entered, **When** auto-fill attempts, **Then** error message displays "Part ID not found" and fields remain empty
5. **Given** multiple rows with same PartID, **When** auto-fill is applied, **Then** all rows get same spec values from part definition

---

### User Story 3 - Edit Mode with History Loading (Priority: P1)

As a **receiving user**, I need to load and edit historical dunnage transaction records with filtering by date range so that I can correct errors or review recent receiving activity.

**Why this priority**: Edit mode provides correction workflow and historical visibility. Essential for data quality and auditing.

**Independent Test**: Can be tested by clicking "Load from History", selecting date range, verifying records load in grid, editing values, and saving changes to verify database updates.

**Acceptance Scenarios**:

1. **Given** edit mode view loads, **When** user clicks "Load from History", **Then** date filter dialog appears
2. **Given** date range selected (last 7 days), **When** loading completes, **Then** DataGrid shows all loads within range with checkbox column for selection
3. **Given** loaded history records, **When** user edits Quantity in a row, **Then** cell updates and row is marked as modified
4. **Given** multiple records selected via checkboxes, **When** user clicks "Remove Row", **Then** confirmation dialog asks "Delete X selected records permanently?"
5. **Given** modified records, **When** user clicks "Save Changes", **Then** updates are persisted to database and success message displays

---

### User Story 4 - Admin Main Navigation Hub (Priority: P1)

As a **system administrator**, I need a main navigation page with 4 management areas (Types, Specs, Parts, Inventoried List) so that I can access all dunnage configuration features from one central location.

**Why this priority**: Navigation hub is the entry point for all admin functionality. Without it, no configuration is accessible.

**Independent Test**: Can be tested by navigating to Settings > Dunnage Management, verifying 4 navigation cards display, clicking each card, and confirming correct management view opens.

**Acceptance Scenarios**:

1. **Given** Settings page loads, **When** user clicks "Dunnage Management" expander, **Then** "Launch Dunnage Admin" button appears
2. **Given** Dunnage Admin button clicked, **When** admin main view loads, **Then** 4 cards display: Manage Dunnage Types, Manage Specs per Type, Manage Dunnage Parts, Inventoried Parts List
3. **Given** admin main view, **When** user clicks "Manage Dunnage Types", **Then** type management view loads in same content area
4. **Given** any management view, **When** user clicks "Back to Management", **Then** main navigation hub reappears
5. **Given** main navigation cards, **When** displayed, **Then** each card shows icon, title, and description of functionality

---

### User Story 5 - Dunnage Type Management (Priority: P1)

As an **administrator**, I need to view, add, edit, and delete dunnage types with impact analysis so that I can maintain the type taxonomy safely.

**Why this priority**: Types are the foundation of the dunnage hierarchy. Type management must exist before specs and parts can be managed.

**Independent Test**: Can be tested by adding a new type "Container", editing its name to "Container Box", attempting to delete a type with parts (should block with impact count), and deleting an unused type (should succeed).

**Acceptance Scenarios**:

1. **Given** type management view loads, **When** displaying types list, **Then** DataGrid shows all types with columns: TypeName, DateAdded, AddedBy, LastModified, Actions (Edit/Delete buttons)
2. **Given** type management view, **When** user clicks "+ Add New Type", **Then** 3-step add type form appears (Step 1: Name, Step 2: Select default specs, Step 3: Optional custom specs)
3. **Given** add type form Step 1, **When** user enters "Container" and clicks Next, **Then** Step 2 displays checkboxes for default specs (Width, Height, Depth, IsInventoriedToVisual, Material, Color, etc.)
4. **Given** add type form Step 2 with specs selected, **When** user clicks "Save Type", **Then** type is created, spec schema is inserted, success message displays
5. **Given** type with 15 dependent parts, **When** user clicks Delete, **Then** confirmation dialog displays "⚠️ Warning: 15 parts use this type. Deleting will remove all parts and transaction history. Type DELETE to confirm."

---

### User Story 6 - Dunnage Part Management (Priority: P1)

As an **administrator**, I need to view, add, edit, and delete dunnage parts with filtering and search so that I can maintain the master parts catalog.

**Why this priority**: Parts are the primary receiving entities. Part management is critical for system operation.

**Independent Test**: Can be tested by adding part "PALLET-60X48" with spec values, filtering by type "Pallet", searching for "60X48", editing spec values, and deleting unused part.

**Acceptance Scenarios**:

1. **Given** part management view loads, **When** displaying parts, **Then** DataGrid shows PartID, Type, spec columns (Width, Height, Depth, etc.), DateAdded, Actions with pagination (20 per page)
2. **Given** part management toolbar, **When** user selects "Filter Type: Pallet", **Then** grid refreshes to show only Pallet parts
3. **Given** search box, **When** user enters "60X48", **Then** grid filters to parts with matching PartID or spec values
4. **Given** user clicks "+ Add New Part", **When** form loads, **Then** 3-step form appears (Step 1: Select Type, Step 2: Enter PartID, Step 3: Enter spec values with dynamic inputs based on type's schema)
5. **Given** part "PALLET-48X40" with 127 transaction records, **When** user clicks Delete, **Then** confirmation shows "⚠️ 127 transaction records reference this part. Deleting will orphan those records. Type DELETE to confirm."

---

### User Story 7 - Dynamic CSV Column Generation (Priority: P1)

As a **LabelView template designer**, I need CSV exports to include all possible specification columns (union of all spec keys across all types) so that label templates can reference any spec field without modification.

**Why this priority**: Dynamic columns eliminate the need to update label templates when new spec fields are added. Critical for system maintainability.

**Independent Test**: Can be tested by creating types with different specs (Type A: Width/Height, Type B: Width/Material), exporting loads of both types, and verifying CSV has all unique spec columns (Width, Height, Material).

**Acceptance Scenarios**:

1. **Given** database has types with varying specs, **When** `GetAllSpecKeysAsync()` is called, **Then** union of all unique spec keys is returned
2. **Given** union of spec keys is ["Width", "Height", "Depth", "Material", "IsInventoriedToVisual"], **When** CSV header is generated, **Then** columns are: DunnageType, PartID, Quantity, PONumber, ReceivedDate, Employee, Depth, Height, IsInventoriedToVisual, Material, Width (alphabetical after fixed)
3. **Given** load with Type "Pallet" (has Width/Height/Depth), **When** CSV row is written, **Then** Width/Height/Depth cells are populated, Material cell is blank
4. **Given** load with Type "Foam" (has Material), **When** CSV row is written, **Then** Material cell is populated, Width/Height/Depth cells are blank
5. **Given** new spec "Color" added to Type "Crate", **When** next export occurs, **Then** "Color" column appears in CSV for all loads

---

### User Story 8 - Dual-Path File Writing (Priority: P1)

As a **receiving user**, I need CSV files written to both local AppData (guaranteed) and network share (best-effort) so that labels are accessible even when network is unavailable.

**Why this priority**: Network reliability is unpredictable. Local fallback ensures workflow continuity. Network path enables centralized label management.

**Independent Test**: Can be tested by disconnecting network, exporting loads, verifying local file created, reconnecting network, exporting again, and verifying both local and network files exist.

**Acceptance Scenarios**:

1. **Given** CSV export is triggered, **When** local path write succeeds, **Then** file is created at `%APPDATA%\MTM_Receiving_Application\DunnageData.csv`
2. **Given** network is available, **When** network path write succeeds, **Then** file is created at `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv`
3. **Given** network share is unavailable, **When** export attempts, **Then** local file succeeds, network fails with logged error, user sees "Labels saved locally (network unavailable)"
4. **Given** user folder doesn't exist on network share, **When** export attempts, **Then** directory is created automatically before file write
5. **Given** both writes succeed, **When** export completes, **Then** user sees "Labels saved successfully to local and network locations"

---

### User Story 9 - RFC 4180 CSV Formatting (Priority: P1)

As a **LabelView integration**, I need CSV files to properly escape special characters (commas, quotes, newlines) per RFC 4180 standard so that data parses correctly in label templates.

**Why this priority**: Improper escaping causes label printing errors or data corruption. Standards compliance is non-negotiable.

**Independent Test**: Can be tested by creating loads with special characters (PartID with comma, PO with quotes, Location with newline), exporting, and verifying cell escaping is correct.

**Acceptance Scenarios**:

1. **Given** PartID contains comma "PALLET-48,40", **When** CSV is written, **Then** cell is quoted: `"PALLET-48,40"`
2. **Given** PONumber contains quotes `PO "12345"`, **When** CSV is written, **Then** quotes are doubled and cell is quoted: `"PO ""12345"""`
3. **Given** Location contains newline "Dock A\nBay 2", **When** CSV is written, **Then** cell is quoted with preserved newline: `"Dock A\nBay 2"`
4. **Given** spec value is numeric 48, **When** CSV is written, **Then** cell is unquoted: `48`
5. **Given** cell value is null/empty, **When** CSV is written, **Then** cell is blank (no quotes): ``

---

### User Story 10 - Add New Type Dialog Without Scrolling (Priority: P1)

As a **receiving supervisor**, I need to create new dunnage types quickly (under 60 seconds) when vendors change packaging, with all form sections visible without scrolling for standard workflows (≤5 custom fields).

**Why this priority**: Scrolling breaks flow and causes context loss. 90% of dunnage types have ≤5 custom fields. Eliminating scrolling for this majority use case reduces configuration time by ~40%.

**Independent Test**: Can be fully tested by creating a new dunnage type with 5 or fewer custom fields on a 1920x1080 monitor and verifying no vertical scrollbar appears and all elements are visible without scrolling.

**Acceptance Scenarios**:

1. **Given** a 1920x1080 monitor with standard taskbar (880px available height), **When** user opens the Add New Dunnage Type dialog, **Then** all form sections (Basic Information, Icon Selection, Custom Specifications) are visible without scrolling
2. **Given** user has entered Type Name and selected an icon, **When** they add up to 5 custom fields, **Then** no vertical scrollbar appears and the dialog height remains ≤750px
3. **Given** user is tabbing through form fields, **When** they press Tab, **Then** focus flows logically top-to-bottom without jumping between sections
4. **Given** user has added 6 or more custom fields, **When** the custom fields section exceeds available space, **Then** only the custom fields section displays a scrollbar (not the entire dialog)

---

### User Story 11 - Real-Time Validation with Inline Feedback (Priority: P1)

As a **new receiving clerk**, I need validation errors to display immediately (not just on submit) so I can correct mistakes as I type instead of scanning the entire form after clicking submit.

**Why this priority**: Real-time validation is a modern UX standard that reduces errors by 60% and accelerates training. Blocking submission until valid ensures data quality.

**Independent Test**: Can be fully tested by entering invalid data (empty Type Name, special characters in Field Name) and verifying red borders and error messages appear immediately with 300ms debounce.

**Acceptance Scenarios**:

1. **Given** user starts typing in the Type Name field, **When** they clear all text or enter only whitespace, **Then** a red border appears and error message displays: "Type name is required"
2. **Given** user enters "Pallet" as Type Name and a type named "Pallet" already exists, **When** the field loses focus, **Then** a yellow warning icon appears with message: "A dunnage type named 'Pallet' already exists. Consider using a different name or editing the existing type." (non-blocking)
3. **Given** user is entering a Field Name, **When** they type 42 characters, **Then** a character counter displays below the field: "42/100 characters"
4. **Given** user enters special characters in Field Name (e.g., "Weight<lbs>"), **When** they type the invalid character, **Then** a red border appears and error displays: "Field name cannot contain special characters: <>{}[]|\n\t"
5. **Given** user has validation errors (empty Type Name), **When** they attempt to click "Add Type", **Then** the button is disabled (grayed out) until all required fields are valid

---

### User Story 12 - Custom Field Preview with Edit/Delete/Reorder (Priority: P1)

As a **quality control manager**, I need to verify that custom field structures match specifications before saving, and need the ability to reorder fields after creation without deleting and re-adding.

**Why this priority**: Creating complex dunnage types (8+ fields) without preview leads to errors requiring deletion and recreation. Reordering capability saves 5+ minutes per complex type configuration.

**Independent Test**: Can be fully tested by adding 3 custom fields, verifying they appear in a styled preview list, then dragging to reorder, editing a field, and deleting a field.

**Acceptance Scenarios**:

1. **Given** user adds a new custom field named "Material" (Text, Required), **When** they click "Add Field", **Then** the field immediately appears in a styled preview card showing icon, name, type, and required status
2. **Given** user has added 3 custom fields, **When** they hover over a field preview card, **Then** edit button, delete button, and drag handle become visible
3. **Given** user wants to reorder fields, **When** they drag the "Weight" field above "Material", **Then** the preview updates immediately and tab order adjusts accordingly
4. **Given** user realizes a field name is incorrect, **When** they click the edit button on a field preview, **Then** the field details populate in the "New Field" section for modification
5. **Given** user has added 25 custom fields (maximum), **When** they attempt to add another, **Then** the "Add Field" button is disabled and an InfoBar appears: "Maximum 25 custom fields per type. Please remove a field to add another."

---

### User Story 13 - Edit Mode Data Sources (Priority: P2)

As a **receiving user**, I need to load data from three sources (Current Memory, Current Labels, History) so that I can review unsaved work, re-process labels, or correct historical data.

**Why this priority**: Multiple data sources provide flexibility for different correction scenarios. Priority P2 because History is most critical (P1).

**Independent Test**: Can be tested by saving loads to session (Current Memory), exporting CSV (Current Labels), and querying database (History), then verifying each load source returns expected data.

**Acceptance Scenarios**:

1. **Given** wizard workflow has unsaved loads, **When** edit mode "Load from Current Memory" is clicked, **Then** session loads populate grid
2. **Given** CSV file exists at local path, **When** "Load from Current Labels" is clicked, **Then** CSV is parsed and rows populate grid
3. **Given** database has historical records, **When** "Load from History" with date filter is clicked, **Then** database records populate grid
4. **Given** Current Labels CSV missing, **When** load is attempted, **Then** error message displays "No label file found for current user"
5. **Given** Current Memory is empty, **When** load is attempted, **Then** info message displays "No unsaved loads in session"

---

### User Story 14 - Date Filtering and Pagination (Priority: P2)

As an **edit mode user**, I need to filter loads by date range (Last Week, Today, This Week, This Month, This Quarter, Show All) and paginate results (50 per page) so that I can efficiently navigate large datasets.

**Why this priority**: Filtering and pagination are critical for usability with large historical datasets. Priority P2 because basic history loading is P1.

**Independent Test**: Can be tested by loading large dataset, applying date filters, verifying correct records display, using pagination controls, and confirming page navigation works correctly.

**Acceptance Scenarios**:

1. **Given** edit mode with history loaded, **When** user clicks "This Week" filter, **Then** only loads from current week display
2. **Given** custom date range selected (12/01 to 12/26), **When** filter applies, **Then** loads within range are shown
3. **Given** 150 loads in filtered dataset, **When** ItemsPerPage is 50, **Then** TotalPages calculates to 3
4. **Given** page 1 displayed, **When** user clicks "Next Page", **Then** page 2 loads (records 51-100)
5. **Given** page navigation controls, **When** user enters "3" and clicks "Go", **Then** page 3 loads directly

---

### User Story 15 - Inventoried Parts List Management (Priority: P2)

As an **administrator**, I need to manage the list of parts requiring Visual ERP inventory tracking so that users receive appropriate notifications during data entry.

**Why this priority**: Inventoried list drives data quality notifications. Priority P2 because core receiving works without it, but data quality suffers.

**Independent Test**: Can be tested by adding "PALLET-48X40" to inventoried list with method "Both", editing method to "Adjust In", and removing from list.

**Acceptance Scenarios**:

1. **Given** inventoried list view loads, **When** displaying list, **Then** DataGrid shows PartID, Type, InventoryMethod, Notes, DateAdded, AddedBy, Actions
2. **Given** inventoried list, **When** user clicks "+ Add Part to List", **Then** dialog opens with PartID ComboBox (searchable), InventoryMethod dropdown (Adjust In, Receive In, Both), Notes TextBox
3. **Given** add dialog, **When** user selects method "Both" and saves, **Then** part is added to inventoried_dunnage_list table
4. **Given** inventoried part listed, **When** user clicks Edit, **Then** dialog allows changing InventoryMethod and Notes only (PartID readonly)
5. **Given** inventoried part selected, **When** user clicks Remove, **Then** confirmation dialog displays "ℹ️ Part will no longer trigger inventory notifications. Continue?"

---

### User Story 16 - Visual Icon Picker with Search (Priority: P2)

As a **receiving supervisor**, I want to maintain visual consistency across related dunnage types using an icon picker with search and categories, so I can quickly find and apply appropriate icons.

**Why this priority**: Visual icon selection is 3x faster than text-based selection and improves visual consistency. Recent icons feature accelerates repetitive configuration.

**Independent Test**: Can be fully tested by opening the icon picker, searching for "box", filtering by category "Containers", selecting an icon from recently used section, and verifying the preview updates immediately.

**Acceptance Scenarios**:

1. **Given** user opens the dialog, **When** they view the Icon Selection section, **Then** they see a visual preview of the currently selected icon (default: Box &#xE7B8;) with icon name
2. **Given** user wants to choose a different icon, **When** they view the icon picker, **Then** they see a 6-column × 3-row grid (18 icons visible) organized in tabs: "Containers", "Materials", "Warnings", "Tools", "All"
3. **Given** user has used certain icons before, **When** they open the icon picker, **Then** the top 6 most-used icons appear in a "Recently Used" section (persistent via user preferences)
4. **Given** user is looking for a specific icon, **When** they type "box" in the search box, **Then** only icons matching "box" keyword are displayed in the grid
5. **Given** user selects a new icon from the grid, **When** they click it, **Then** the selected icon preview updates immediately and the icon has an accent border

---

### User Story 17 - LabelView Template Validation (Priority: P2)

As a **system implementer**, I need documentation and test data to validate that LabelView templates correctly map to CSV columns so that labels print with correct data.

**Why this priority**: Integration validation prevents production issues. Priority P2 because it's testing/documentation, not core functionality.

**Independent Test**: Can be tested by generating sample CSV with all column types, importing to LabelView, creating test template, and verifying all fields map correctly.

**Acceptance Scenarios**:

1. **Given** sample CSV with 20 loads (various types/specs), **When** imported to LabelView, **Then** all rows parse without errors
2. **Given** LabelView template referencing "Width" field, **When** label prints for load with Width=48, **Then** label displays "48"
3. **Given** LabelView template referencing "IsInventoriedToVisual" field, **When** load has IsInventoriedToVisual=true, **Then** label displays "True" or custom formatted value
4. **Given** load missing optional spec (Material blank), **When** label template references Material, **Then** label displays blank or default value (no error)
5. **Given** CSV with 100 loads, **When** batch printed in LabelView, **Then** all 100 labels print correctly without manual intervention

---

### User Story 18 - Export Error Handling and Logging (Priority: P2)

As a **system administrator**, I need detailed error logging for CSV export failures so that I can diagnose issues with file permissions, network paths, or data formatting.

**Why this priority**: Export failures block workflow. Detailed logging enables fast resolution. Priority P2 because basic export works (P1), this is enhanced diagnostics.

**Independent Test**: Can be tested by inducing failures (remove directory write permissions, disconnect network mid-write, corrupt data), and verifying error logs contain actionable information.

**Acceptance Scenarios**:

1. **Given** local directory is write-protected, **When** export attempts, **Then** error is logged with message "Failed to write local CSV: Access denied to {path}"
2. **Given** network share path is malformed, **When** export attempts, **Then** error is logged with message "Invalid network path: {path}"
3. **Given** load contains invalid character encoding, **When** CSV write attempts, **Then** error is logged with load ID and problematic field
4. **Given** disk is full, **When** export attempts, **Then** error is logged "Insufficient disk space for CSV export"
5. **Given** any export error, **When** logged, **Then** log includes: timestamp, username, load count, file paths attempted, exception details

---

### User Story 19 - Duplicate Existing Dunnage Type (Priority: P2)

As a **quality control manager**, I need to create similar dunnage types by duplicating existing ones, reducing configuration time from 5 minutes to 90 seconds per type.

**Why this priority**: Bulk configuration scenarios are common during product launches. Duplication reduces configuration time by 70% and ensures consistency.

**Independent Test**: Can be fully tested by right-clicking an existing dunnage type in the main grid, selecting "Duplicate Type", verifying the dialog opens with pre-populated data including " (Copy)" suffix, modifying the name, and saving.

**Acceptance Scenarios**:

1. **Given** user right-clicks a dunnage type in the management grid, **When** the context menu appears, **Then** they see "Duplicate Type" option
2. **Given** user clicks "Duplicate Type" on "Pallet 48x40", **When** the Add New Type dialog opens, **Then** Type Name is pre-populated with "Pallet 48x40 (Copy)", icon matches source, and all custom fields are copied in same order
3. **Given** user has duplicated a type with validation rules (e.g., Weight min:1 max:9999), **When** they view the custom fields, **Then** validation rules are copied to the new type
4. **Given** user modifies the duplicated type name to "Pallet 48x48", **When** they click "Add Type", **Then** a new type is created with the copied structure and modified name

---

### User Story 20 - Field Validation Rules Builder (Priority: P3)

As a **quality control manager**, I want to enforce data quality (e.g., Weight must be 1-9999 with 2 decimals) by setting validation rules when defining custom fields.

**Why this priority**: Advanced validation reduces bad data entry by 80%. However, basic validation (required/optional) covers most use cases, making this enhancement-level priority.

**Independent Test**: Can be fully tested by creating a Number field, setting min:1 max:9999 decimals:2, saving the type, then entering data in the main receiving workflow and verifying values outside the range are rejected.

**Acceptance Scenarios**:

1. **Given** user is adding a Number field named "Weight", **When** they expand "Validation Rules", **Then** they see input fields for Min Value, Max Value, and Decimal Places (0-4)
2. **Given** user is adding a Text field named "Part Number", **When** they expand "Validation Rules", **Then** they see Max Length input and pattern options (Starts with, Ends with, Contains, Custom Regex)
3. **Given** user is adding a Date field named "Manufacture Date", **When** they expand "Validation Rules", **Then** they see Min Date and Max Date pickers with presets (Today, 30/60/90 days ago, Custom)
4. **Given** user has set validation rules for "Weight" (min:1, max:9999, decimals:2), **When** they view the field preview, **Then** a human-readable summary appears: "Number (1-9999, 2 decimals, Required)"
5. **Given** user has created a type with validation rules, **When** data is entered in the receiving workflow, **Then** values violating the rules are rejected in real-time with specific error messages

---

### Edge Cases

- **What happens when adding multiple rows with count > 100?** Warn user about performance, confirm action
- **What happens when auto-fill is triggered but part has no spec values in database?** Fill Type only, leave specs blank
- **What happens when editing a load that was deleted by another user?** Database error on save, refresh grid
- **What happens when CSV parse fails due to malformed data?** Error message with line number, don't load partial data
- **What happens when user tries to save with invalid data (qty=0, blank PartID)?** Validation highlights errors, blocks save
- **What happens when trying to add duplicate type name?** Yellow warning with "View Existing Type" link, but allow save (user might want variant)
- **What happens when deleting type with specs but no parts?** Specs cascade delete automatically, allow deletion
- **What happens when adding spec field with duplicate key for same type?** Validation error: "Spec key already exists for this type"
- **What happens when editing part to change PartID that already exists?** Validation error: "PartID must be unique"
- **What happens when adding part to inventoried list that's already listed?** Validation error: "Part already in inventoried list"
- **What happens when username contains invalid path characters?** Sanitize username for file path: replace with underscore
- **What happens when CSV file is locked by another process?** Retry with incremented filename: DunnageData_1.csv, log warning
- **What happens when spec value is very long (>32,000 characters)?** Truncate with warning in log, CSV cell limit
- **What happens when exporting 10,000 loads in one batch?** Stream write to avoid memory issues, show progress indicator
- **What happens when spec key contains spaces or special characters?** Sanitize column header: replace spaces with underscores, remove special chars
- **What happens when user clicks "Add Type" without entering a Type Name?** Primary button disabled, red border on field, error "Type name is required", focus returns
- **What happens when user adds no custom fields?** Save allowed with confirmation: "You haven't added any custom fields. This type will only track basic information. Continue?" with "Don't show again" checkbox
- **What happens when user tries to add a 26th custom field?** "Add Field" button disabled after #25, InfoBar: "Maximum 25 custom fields per type"
- **What happens when user enters special characters in Field Name?** Block <>{}[]|\n\t with error, allow ()- and spaces
- **What happens when icon library fails to load?** Fallback to Unicode symbols, error logged, InfoBar: "Icon library unavailable. Default icons will be used."
- **What happens when user clicks Cancel with unsaved changes?** Confirmation: "You have unsaved changes. Are you sure you want to cancel?" with "Discard Changes" (red) and "Continue Editing" (primary)
- **What happens when database connection is lost during save?** Loading spinner on button, 10-second timeout, error dialog with retry option, data retained


## Requirements *(mandatory)*

### Functional Requirements - Manual Entry Mode

#### DunnageManualEntryView.xaml + DunnageManualEntryViewModel

- **FR-001**: View MUST display DataGrid with columns: Load#, Type, PartID, Qty, PO, Location, plus dynamic spec columns
- **FR-002**: View MUST provide toolbar with buttons: Add Row, Add Multiple, Remove Row, Auto-Fill
- **FR-003**: View MUST provide "Mode Selection" button to return to mode selection
- **FR-004**: View MUST provide "Save & Finish" button to persist and export
- **FR-005**: ViewModel MUST maintain ObservableCollection<Model_DunnageLoad> Loads
- **FR-006**: ViewModel MUST provide AddRowCommand that creates new load with auto-incremented LoadNumber
- **FR-007**: ViewModel MUST provide AddMultipleRowsCommand that opens dialog for batch creation
- **FR-008**: ViewModel MUST provide RemoveRowCommand that deletes selected row from collection
- **FR-009**: ViewModel MUST provide AutoFillCommand that populates spec values from part master data
- **FR-010**: ViewModel MUST provide SaveCommand that validates, inserts to database, and exports CSV
- **FR-011**: Dynamic spec columns MUST be generated based on union of all spec keys from GetAllSpecKeysAsync
- **FR-012**: Type column MUST be ComboBox populated from GetAllTypesAsync
- **FR-013**: PartID column MUST support manual entry with validation against GetPartByIdAsync

#### AddMultipleRowsDialog (ContentDialog)

- **FR-014**: Dialog MUST provide NumberBox for row count (1-100)
- **FR-015**: Dialog MUST provide optional ComboBox for Type pre-population
- **FR-016**: Dialog MUST provide optional ComboBox for PartID pre-population (filtered by Type if selected)
- **FR-017**: Dialog MUST provide optional TextBox for Location pre-population
- **FR-018**: When Type and PartID are selected, created rows MUST auto-fill with part's spec values
- **FR-019**: When only count is specified, empty rows MUST be created
- **FR-020**: Dialog MUST validate count > 0 and <= 100

#### Auto-Fill Logic

- **FR-021**: Auto-Fill MUST trigger when user selects PartID from ComboBox or enters valid PartID and tabs out
- **FR-022**: Auto-Fill MUST query GetPartByIdAsync to retrieve part definition
- **FR-023**: Auto-Fill MUST populate Type from part's DunnageTypeID
- **FR-024**: Auto-Fill MUST deserialize DunnageSpecValues JSON and populate spec columns
- **FR-025**: Auto-Fill MUST NOT overwrite user-entered values in Quantity, PO, Location columns
- **FR-026**: Auto-Fill MUST handle missing spec keys gracefully (leave cell empty if spec doesn't exist for part)

### Functional Requirements - Edit Mode

#### DunnageEditModeView.xaml + DunnageEditModeViewModel

- **FR-027**: View MUST match EditModeView pattern from Receiving module (toolbar, date filter, grid, footer)
- **FR-028**: View MUST provide "Load Data From" toolbar: Current Memory, Current Labels, History buttons
- **FR-029**: View MUST provide date filter toolbar: Start/End CalendarDatePicker, preset filter buttons
- **FR-030**: View MUST provide DataGrid with checkbox column for multi-select
- **FR-031**: View MUST provide pagination controls: First, Previous, Page X of Y, Go, Next, Last buttons
- **FR-032**: ViewModel MUST provide LoadFromCurrentMemoryCommand that loads workflow service's CurrentSession.Loads
- **FR-033**: ViewModel MUST provide LoadFromCurrentLabelsCommand that parses CSV from local path
- **FR-034**: ViewModel MUST provide LoadFromHistoryCommand that calls GetLoadsByDateRangeAsync
- **FR-035**: ViewModel MUST maintain AllLoads (full dataset) and Loads (paginated subset) collections
- **FR-036**: ViewModel MUST provide SelectAllCommand that toggles IsSelected on all rows
- **FR-037**: ViewModel MUST provide RemoveRowCommand that hard deletes selected rows via DeleteLoadAsync
- **FR-038**: ViewModel MUST provide SaveChangesCommand that updates modified rows via UpdateLoadAsync

#### Date Filtering

- **FR-039**: ViewModel MUST provide FilterStartDate and FilterEndDate properties bound to CalendarDatePickers
- **FR-040**: ViewModel MUST provide SetFilterLastWeekCommand (today - 7 days to today)
- **FR-041**: ViewModel MUST provide SetFilterTodayCommand (today 00:00 to today 23:59)
- **FR-042**: ViewModel MUST provide SetFilterThisWeekCommand (Monday to Sunday of current week)
- **FR-043**: ViewModel MUST provide SetFilterThisMonthCommand (first day to last day of current month)
- **FR-044**: ViewModel MUST provide SetFilterThisQuarterCommand (first day to last day of current quarter)
- **FR-045**: ViewModel MUST provide SetFilterShowAllCommand (clear date filters, show all loaded records)
- **FR-046**: Date filter buttons MUST display dynamic text (e.g., "Dec 2024" for This Month, "Q4 2024" for This Quarter)

#### Pagination

- **FR-047**: ViewModel MUST provide CurrentPage, TotalPages, GotoPageNumber, ItemsPerPage properties
- **FR-048**: ItemsPerPage MUST default to 50
- **FR-049**: TotalPages MUST calculate as Ceiling(FilteredLoads.Count / ItemsPerPage)
- **FR-050**: Loads collection MUST display items from index ((CurrentPage-1) * ItemsPerPage) to (CurrentPage * ItemsPerPage)
- **FR-051**: ViewModel MUST provide FirstPageCommand, PreviousPageCommand, NextPageCommand, LastPageCommand, GoToPageCommand
- **FR-052**: Navigation commands MUST update CurrentPage and refresh Loads collection
- **FR-053**: Commands MUST disable when already at boundary (e.g., Previous disabled on page 1)

### Functional Requirements - Admin Navigation

#### DunnageAdminMainView.xaml + DunnageAdminMainViewModel

- **FR-054**: View MUST display 4 navigation cards in 2x2 grid layout
- **FR-055**: Cards MUST show: Manage Dunnage Types, Manage Specs per Type, Manage Dunnage Parts, Inventoried Parts List
- **FR-056**: Each card MUST display icon, title, description, and clickable button
- **FR-057**: ViewModel MUST manage visibility flags: IsMainNavigationVisible, IsManageTypesVisible, IsManageSpecsVisible, IsManagePartsVisible, IsInventoriedListVisible
- **FR-058**: ViewModel MUST provide navigation commands: NavigateToManageTypes, NavigateToManageSpecs, NavigateToManageParts, NavigateToInventoriedList, ReturnToMainNavigation
- **FR-059**: Navigation MUST hide main hub and show selected management view (mutual exclusion)

### Functional Requirements - Type Management

#### DunnageAdminTypesView.xaml + DunnageAdminTypesViewModel

- **FR-060**: View MUST display DataGrid with types list (columns: TypeName, DateAdded, AddedBy, LastModified, Edit/Delete buttons)
- **FR-061**: View MUST provide toolbar: "+ Add New Type" button, Back to Management button
- **FR-062**: ViewModel MUST load types from GetAllTypesAsync on initialization
- **FR-063**: ViewModel MUST provide ShowAddTypeCommand that displays add type dialog
- **FR-064**: ViewModel MUST provide ShowEditTypeCommand that opens edit dialog with pre-populated values
- **FR-065**: ViewModel MUST provide ShowDeleteConfirmationCommand that calls impact analysis (GetPartCountByTypeAsync, GetTransactionCountByTypeAsync)
- **FR-066**: Delete confirmation MUST display impact counts and require typing "DELETE" to confirm
- **FR-067**: ViewModel MUST provide SaveNewTypeCommand that calls InsertTypeAsync and refreshes grid
- **FR-068**: ViewModel MUST provide DeleteTypeCommand that calls DeleteTypeAsync after confirmation

#### Add New Type Dialog

- **FR-069**: Dialog MUST display all form sections without vertical scrolling at 1920x1080 resolution with ≤5 custom fields (MaxHeight="750")
- **FR-070**: System MUST enforce consistent button styling with CornerRadius="4" on all buttons
- **FR-071**: System MUST provide real-time validation for Type Name (required, max 100 chars, debounce 300ms)
- **FR-072**: System MUST validate Field Name (required, unique, max 100 chars, no special chars <>{}[]|\, debounce 300ms)
- **FR-073**: System MUST display custom field preview cards immediately after adding fields, showing icon, name, type, required status
- **FR-074**: System MUST provide Edit/Delete/Drag buttons on field preview cards (visible on hover)
- **FR-075**: System MUST support drag-and-drop reordering of custom fields with automatic tab order adjustment
- **FR-076**: System MUST provide visual icon picker with 6-column grid, category tabs (All, Containers, Materials, Warnings, Tools)
- **FR-077**: Icon picker MUST include search filter and recently used section (top 6 icons persistent via user preferences)
- **FR-078**: System MUST disable "Add Type" primary button until all required fields are valid
- **FR-079**: System MUST validate for duplicate Type Names with non-blocking yellow warning and "View Existing Type" link
- **FR-080**: System MUST enforce maximum 25 custom fields per type with warning at 10 fields and hard block at 25
- **FR-081**: System MUST prompt for confirmation when closing dialog with unsaved changes
- **FR-082**: System MUST support keyboard shortcuts: Tab, Enter (submit when valid), Esc (cancel), Ctrl+Enter (submit), Ctrl+F (icon search), Alt+A (add field)
- **FR-083**: System MUST sanitize Field Names for database storage (display "Weight (lbs)" → column "weight_lbs") while preserving display name
- **FR-084**: System MUST handle database connection failures with 10-second timeout, error dialog, data retention, and retry option

### Functional Requirements - Part Management

#### DunnageAdminPartsView.xaml + DunnageAdminPartsViewModel

- **FR-085**: View MUST display DataGrid with parts (columns: PartID, Type, dynamic spec columns, DateAdded, Edit/Delete buttons)
- **FR-086**: View MUST provide toolbar: "+ Add New Part", Type filter ComboBox, Search TextBox
- **FR-087**: View MUST provide pagination controls (20 items per page)
- **FR-088**: ViewModel MUST load parts from GetAllPartsAsync with optional filtering
- **FR-089**: ViewModel MUST provide FilterByTypeCommand that calls GetPartsByTypeAsync
- **FR-090**: ViewModel MUST provide SearchPartsCommand that calls SearchPartsAsync
- **FR-091**: ViewModel MUST provide ShowAddPartCommand that displays 3-step form
- **FR-092**: ViewModel MUST provide ShowDeleteConfirmationCommand that calls GetTransactionCountByPartAsync

#### Add Part Form (Multi-Step)

- **FR-093**: Step 1 MUST provide type selection ComboBox
- **FR-094**: Step 2 MUST provide PartID TextBox with uniqueness validation
- **FR-095**: Step 3 MUST dynamically generate spec input controls based on selected type's schema
- **FR-096**: Step 3 MUST provide NumberBox for number specs, TextBox for text, CheckBox for boolean
- **FR-097**: Step 3 MUST provide checkbox "Add to Inventoried Parts List" with method and notes inputs
- **FR-098**: Form MUST create part record AND optionally inventoried list record in transaction on Save

### Functional Requirements - Inventoried Parts List

#### DunnageAdminInventoriedView.xaml + DunnageAdminInventoriedViewModel

- **FR-099**: View MUST display DataGrid with inventoried parts (columns: PartID, Type, InventoryMethod, Notes, DateAdded, AddedBy, Edit/Remove buttons)
- **FR-100**: View MUST provide toolbar: "+ Add Part to List" button
- **FR-101**: ViewModel MUST load inventoried parts from GetAllInventoriedPartsAsync
- **FR-102**: ViewModel MUST provide ShowAddToListCommand that opens add dialog
- **FR-103**: ViewModel MUST provide ShowEditEntryCommand that opens edit dialog
- **FR-104**: ViewModel MUST provide ShowRemoveConfirmationCommand with soft warning (informational, not blocking)
- **FR-105**: Add dialog MUST provide searchable PartID ComboBox, InventoryMethod dropdown (Adjust In, Receive In, Both), Notes TextBox
- **FR-106**: Edit dialog MUST make PartID readonly (cannot change which part, only method/notes)

### Functional Requirements - CSV Export

#### Service_DunnageCSVWriter

- **FR-107**: Service MUST call GetAllSpecKeysAsync() to retrieve union of all spec keys before writing CSV
- **FR-108**: CSV headers MUST be ordered: Fixed columns first (DunnageType, PartID, Quantity, PONumber, ReceivedDate, Employee), then spec keys alphabetically
- **FR-109**: Service MUST write to local path: `%APPDATA%\MTM_Receiving_Application\DunnageData.csv`
- **FR-110**: Service MUST write to network path: `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv`
- **FR-111**: Service MUST create user subdirectory on network share if it doesn't exist
- **FR-112**: Service MUST sanitize username for valid file path (replace `\/:*?"<>|` with underscore)
- **FR-113**: Service MUST escape CSV cells per RFC 4180 (quote cells with comma/quote/newline, double internal quotes)
- **FR-114**: Service MUST handle null/empty spec values as blank cells (no quotes)
- **FR-115**: Service MUST log errors separately for local and network write failures
- **FR-116**: Service MUST return CSVWriteResult with LocalSuccess, NetworkSuccess, ErrorMessage, LocalFilePath, NetworkFilePath

#### Dynamic Column Handling

- **FR-117**: GetAllSpecKeysAsync() MUST query all records from dunnage_specs table
- **FR-118**: GetAllSpecKeysAsync() MUST deserialize each record's DunnageSpecs JSON
- **FR-119**: GetAllSpecKeysAsync() MUST extract all unique keys from all spec schemas
- **FR-120**: GetAllSpecKeysAsync() MUST return keys sorted alphabetically
- **FR-121**: For each load in export, spec values MUST be populated from load's part's DunnageSpecValues JSON
- **FR-122**: If spec key exists in union but not in load's part, cell MUST be blank
- **FR-123**: Spec value data types MUST be preserved (number unquoted, string quoted if contains special chars, boolean as "True"/"False")

#### File Path Management

- **FR-124**: Service MUST expand `%APPDATA%` environment variable for local path
- **FR-125**: Service MUST replace `{Username}` placeholder with Environment.UserName for network path
- **FR-126**: Service MUST validate network path is reachable before attempting write (timeout: 3 seconds)
- **FR-127**: If network validation fails, MUST skip network write and log warning (don't block local write)
- **FR-128**: Service MUST overwrite existing CSV files (no append, always replace)
- **FR-129**: If file is locked, MUST attempt retry with incremented filename (DunnageData_1.csv, _2.csv, etc., max 5 attempts)

#### Error Handling

- **FR-130**: Local write failure MUST return CSVWriteResult with LocalSuccess=false, ErrorMessage populated
- **FR-131**: Network write failure MUST NOT fail entire operation if local succeeds (return LocalSuccess=true, NetworkSuccess=false)
- **FR-132**: Both write failures MUST return LocalSuccess=false, NetworkSuccess=false, ErrorMessage with both error details
- **FR-133**: All exceptions MUST be caught, logged via ILoggingService, and wrapped in result
- **FR-134**: Encoding errors MUST log problematic load ID and field name
- **FR-135**: Disk space errors MUST log required space vs available space

#### LabelView Integration

- **FR-136**: CSV encoding MUST be UTF-8 with BOM (for Excel/LabelView compatibility)
- **FR-137**: Line endings MUST be CRLF (`\r\n`) per RFC 4180
- **FR-138**: Boolean spec values MUST serialize as "True"/"False" (LabelView string fields)
- **FR-139**: Numeric spec values MUST use invariant culture formatting (period decimal separator, no thousands separator)
- **FR-140**: Date fields MUST format as `yyyy-MM-dd HH:mm:ss` (sortable, LabelView compatible)
- **FR-141**: Empty columns MUST NOT be omitted (preserve column count across all rows for LabelView column mapping)

### Functional Requirements - Validation

- **FR-142**: Manual Entry Save MUST validate all rows have PartID, Quantity > 0
- **FR-143**: Manual Entry Save MUST validate all PartIDs exist in dunnage_part_numbers table
- **FR-144**: Edit Mode Remove MUST show confirmation dialog with count of selected rows
- **FR-145**: Edit Mode Save MUST only update rows marked as modified (track changes)
- **FR-146**: CSV parsing MUST handle missing columns gracefully (optional fields default to empty)
- **FR-147**: Type name MUST be unique (case-insensitive check)
- **FR-148**: Spec key MUST be unique per type
- **FR-149**: PartID MUST be unique globally
- **FR-150**: PartID MUST match pattern validation if specified (alphanumeric, hyphens, underscores)
- **FR-151**: Required spec fields MUST be filled before saving part
- **FR-152**: Default spec fields (Width, Height, Depth, IsInventoriedToVisual) CANNOT be deleted

### Functional Requirements - Impact Analysis

- **FR-153**: Type delete MUST show counts: dependent parts, dependent transactions, dependent specs
- **FR-154**: Spec delete MUST show count: parts with this spec value
- **FR-155**: Part delete MUST show count: transaction records referencing this part
- **FR-156**: All delete confirmations with impact MUST require typing "DELETE" to confirm
- **FR-157**: Inventoried list remove MUST show informational warning only (no blocking)

### Functional Requirements - Duplicate Type

- **FR-158**: System MUST provide "Duplicate Type" context menu option in type management grid
- **FR-159**: Duplicate dialog MUST pre-populate Type Name with original + " (Copy)" suffix
- **FR-160**: Duplicate MUST copy icon selection from source type
- **FR-161**: Duplicate MUST copy all custom fields with same order, type, required status, validation rules
- **FR-162**: User MUST be able to modify duplicated data before saving
- **FR-163**: Duplicate MUST create entirely new type record (not link to original)

### Performance Requirements

- **FR-164**: CSV write MUST handle 1,000 loads in under 5 seconds
- **FR-165**: CSV write MUST stream rows (not load all into memory) to support 10,000+ loads
- **FR-166**: GetAllSpecKeysAsync() result SHOULD be cached per export operation (called once, not per load)
- **FR-167**: Network path reachability check MUST timeout after 3 seconds (don't block indefinitely)
- **FR-168**: DataGrid MUST use CommunityToolkit.WinUI.UI.Controls.DataGrid
- **FR-169**: All grid operations MUST use x:Bind for compile-time binding
- **FR-170**: Grid performance MUST support 1000+ rows with pagination
- **FR-171**: CSV parsing MUST handle files up to 10MB

### Key Entities

- **Dunnage Type**: Represents a category of dunnage (e.g., Pallet, Crate, Blocking) with attributes: TypeID, TypeName, IconGlyph, CreatedDate, IsActive, IsDeleted
- **Custom Field Definition**: Represents user-defined specifications for a dunnage type with attributes: FieldID, TypeID, FieldName, FieldType (Text/Number/Date/Boolean), DisplayOrder, IsRequired, ValidationRules (JSON), DatabaseColumnName
- **Dunnage Part**: Master catalog entry with attributes: PartID, TypeID, DunnageSpecValues (JSON), DateAdded, IsActive
- **Dunnage Load**: Transaction record with attributes: LoadID, PartID, TypeID, Quantity, PONumber, ReceivedDate, EmployeeNumber, Location, DunnageSpecValues (JSON)
- **Inventoried Part Entry**: Parts requiring Visual ERP tracking with attributes: EntryID, PartID, InventoryMethod (AdjustIn/ReceiveIn/Both), Notes, DateAdded, AddedBy
- **Icon Definition**: Available icons in visual picker with attributes: IconGlyph (Unicode), IconName, Category (Containers/Materials/Warnings/Tools)
- **User Icon Preference**: Recently used icons per user with attributes: UserID, IconGlyph, UsageCount, LastUsedDate
- **CSV Write Result**: Export operation result with attributes: LocalSuccess, NetworkSuccess, LocalFilePath, NetworkFilePath, ErrorMessage


## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 90% of standard dunnage type configurations (≤5 custom fields) complete in under 60 seconds without scrolling (baseline: 90 seconds with scrolling)
- **SC-002**: User can add 50 rows in under 10 seconds using "Add Multiple"
- **SC-003**: Auto-fill populates spec values in under 500ms per row
- **SC-004**: DataGrid renders 100 rows with dynamic spec columns without lag
- **SC-005**: Save operation successfully inserts 100 loads in under 3 seconds
- **SC-006**: Validation correctly identifies and highlights errors (missing PartID, qty=0)
- **SC-007**: History loading retrieves 500 records in under 2 seconds
- **SC-008**: Date filters correctly apply and update grid in under 500ms
- **SC-009**: Pagination navigates between pages with no visible delay
- **SC-010**: CSV parsing successfully loads valid DunnageData.csv files
- **SC-011**: Multi-select and batch delete correctly removes selected rows
- **SC-012**: Current Memory loading retrieves unsaved wizard session data
- **SC-013**: Administrator can add new type with specs in under 1 minute
- **SC-014**: Impact analysis displays accurate counts before all delete operations
- **SC-015**: Type/Spec/Part filtering and search return results in under 500ms
- **SC-016**: Part management pagination handles 1000+ parts efficiently
- **SC-017**: All CRUD operations successfully persist to database and refresh UI
- **SC-018**: Validation errors display user-friendly messages and prevent invalid data
- **SC-019**: Multi-step forms maintain state when navigating between steps
- **SC-020**: Inventoried list changes immediately affect receiving workflow notifications
- **SC-021**: CSV files written to both local and network paths when network is available
- **SC-022**: Local CSV always succeeds even when network fails (99.9% success rate)
- **SC-023**: CSV headers include all spec keys from union (verified with 10 types, 30 unique specs)
- **SC-024**: CSV escaping correctly handles 100% of test cases (commas, quotes, newlines, edge chars)
- **SC-025**: LabelView imports CSV without errors for 100 test loads
- **SC-026**: Export of 1,000 loads completes in under 5 seconds
- **SC-027**: Export errors are logged with sufficient detail for diagnosis (100% of failures include actionable info)
- **SC-028**: File locking handled gracefully with retry (95% success rate on locked file scenarios)
- **SC-029**: Zero UI consistency complaints in user feedback after redesign (baseline: 4 complaints about mixed button styles)
- **SC-030**: 75% reduction in validation-related support tickets (baseline: 8 tickets/month)
- **SC-031**: Custom field reordering feature used in 30% of multi-field type configurations within first month
- **SC-032**: Visual icon picker search feature used in 50% of dialog sessions
- **SC-033**: Type duplication feature reduces bulk configuration time by 70% (from 5 min/type to 90 sec/type)
- **SC-034**: 95% of users successfully complete dunnage type creation on first attempt without errors (baseline: 78% success rate)

## Assumptions *(optional)*

- Users have 1920x1080 or higher resolution monitors (minimum supported: 1366x768)
- Average dunnage type has 3-5 custom fields (90% of use cases based on existing data)
- Icon library contains 50-100 distinct icons organized in 4 categories
- MySQL 5.7.24 compatibility means validation rules stored as JSON string (no JSON column type)
- User preferences are stored per-user in `user_preferences` table with JSON field
- Icon picker loads all icons at once (not paginated) due to small dataset size
- Recently used icons persist indefinitely (no automatic cleanup)
- Custom field DisplayOrder starts at 1 and increments by 1 (no gaps allowed)
- Sanitized Field Names for database columns must be unique within a type (enforced at DAO layer)
- Dialog is modal (blocks interaction with parent window until closed)
- Network share `\\mtmanu-fs01\Expo Drive` is available during business hours
- LabelView templates exist and are maintained by IT department
- CSV file size typically under 5MB for daily operations
- Database connection is generally reliable (< 1% failure rate)
- Users understand basic spreadsheet concepts for manual entry mode

## Dependencies *(optional)*

### Phase Dependencies

- **Phase 1 Infrastructure**: Complete (database tables, stored procedures from specs 004-005)
- **Database Schema**: 
  - `dunnage_types` table with columns: type_id, type_name, icon_glyph, created_date, is_active, is_deleted
  - `dunnage_specs` table with columns: spec_id, type_id, spec_key, spec_type, is_required, validation_rules, display_order
  - `dunnage_part_numbers` table with columns: part_id, type_id, dunnage_spec_values (JSON), date_added, is_active
  - `dunnage_loads` table with columns: load_id, part_id, type_id, quantity, po_number, received_date, employee_number, location, dunnage_spec_values (JSON)
  - `inventoried_dunnage_list` table with columns: entry_id, part_id, inventory_method, notes, date_added, added_by
  - `custom_field_definitions` table with columns: field_id, type_id, field_name, field_type, display_order, is_required, validation_rules, database_column_name
  - `user_preferences` table with columns: user_id, preference_key, preference_value (JSON)

### Stored Procedures

- `sp_dunnage_types_get_all`, `sp_dunnage_types_insert`, `sp_dunnage_types_update`, `sp_dunnage_types_delete`
- `sp_dunnage_types_check_duplicate`, `sp_dunnage_types_get_part_count`, `sp_dunnage_types_get_transaction_count`
- `sp_dunnage_specs_get_all`, `sp_dunnage_specs_get_by_type`, `sp_dunnage_specs_insert`, `sp_dunnage_specs_delete`
- `sp_dunnage_parts_get_all`, `sp_dunnage_parts_get_by_id`, `sp_dunnage_parts_get_by_type`, `sp_dunnage_parts_search`
- `sp_dunnage_parts_insert`, `sp_dunnage_parts_update`, `sp_dunnage_parts_delete`, `sp_dunnage_parts_get_transaction_count`
- `sp_dunnage_loads_get_by_date_range`, `sp_dunnage_loads_insert_batch`, `sp_dunnage_loads_update`, `sp_dunnage_loads_delete`
- `sp_inventoried_dunnage_get_all`, `sp_inventoried_dunnage_insert`, `sp_inventoried_dunnage_update`, `sp_inventoried_dunnage_delete`
- `sp_custom_fields_insert`, `sp_custom_fields_get_by_type`, `sp_user_preferences_upsert`, `sp_user_icon_usage_track`

### Existing Components

- `BaseViewModel` (provides IsBusy, StatusMessage, error handling)
- `IService_ErrorHandler` (user error dialogs, exception handling)
- `ILoggingService` (audit logging)
- `INavigationService` (page navigation)
- Settings page infrastructure (for admin navigation entry point)
- ManualEntryView/EditModeView patterns from Receiving module

### Services (from spec 004)

- `IService_MySQL_Dunnage` for all CRUD operations
- `IService_DunnageCSVWriter` for CSV export
- `IService_DunnageWorkflow` for wizard state management
- `IService_DunnageValidation` for business rule validation

### NuGet Packages

- `CommunityToolkit.WinUI.UI.Controls` (for DataGrid)
- `CommunityToolkit.Mvvm` (commands, properties)
- `CsvHelper` (for RFC 4180 CSV writing/parsing)
- Segoe Fluent Icons (bundled with WinUI 3) or custom icon font file

### External Systems

- **Infor Visual ERP**: READ-ONLY integration for PO validation (not part of this spec)
- **LabelView**: Label printing software (integration via CSV export)
- **Network Share**: `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\`

### Configuration

- Local and network file paths in app settings (`appsettings.json`)
- DunnageCSV section with LocalPath, NetworkPathTemplate, RetryCount, RetryDelayMs

## Out of Scope *(optional)*

- ❌ Inline cell validation in manual entry grid (validate on Save only)
- ❌ Excel import/export (CSV only)
- ❌ Column reordering/hiding in DataGrid (fixed column layout)
- ❌ Grid row sorting by clicking headers (pagination order only)
- ❌ Undo/Redo for grid edits (explicit save/cancel only)
- ❌ Soft delete in Edit Mode (hard delete with confirmation)
- ❌ Bulk import of types/specs/parts from CSV (manual CRUD only)
- ❌ Audit trail of who changed what when (basic metadata only: EntryUser, AlterUser)
- ❌ Role-based access control (all users with Settings access can admin)
- ❌ Spec schema versioning (immediate update, no version history)
- ❌ Part duplication/cloning feature in admin UI (manual copy only)
- ❌ Advanced search with multiple filters (simple text search only)
- ❌ CSV import functionality (export only)
- ❌ Custom CSV column ordering (fixed + alphabetical only)
- ❌ CSV compression/archival (raw CSV only)
- ❌ Email delivery of CSV files (manual retrieval only)
- ❌ LabelView template creation (assumes templates exist)
- ❌ Barcode generation in CSV (LabelView generates barcodes)
- ❌ Advanced validation rule builder UI (basic validation only - P3 deferred)
- ❌ Bulk import from CSV/Excel (separate feature specification)
- ❌ Live preview panel showing actual receiving form layout (P3 enhancement)
- ❌ Field templates for common dunnage categories (may be included if time permits)
- ❌ Editing existing dunnage types in dialog (separate Edit Type Dialog)
- ❌ Deleting dunnage types in dialog (handled in management grid context menu)
- ❌ Internationalization/localization (English-only for initial release)
- ❌ Mobile/tablet responsive layout (desktop-only application)
- ❌ Accessibility features beyond standard WinUI 3 defaults
- ❌ Undo/redo functionality within dialog
- ❌ Auto-save drafts (user must explicitly save or cancel)

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: UI requirements clearly separate presentation (XAML-only Views) from logic (ViewModels with [ObservableProperty], [RelayCommand], validation methods)
- [x] **Database Layer**: Data persistence requirements specify entities (dunnage_types, dunnage_parts, dunnage_loads, custom_field_definitions, user_preferences, inventoried_dunnage_list) without implementation details. DAOs will use stored procedures exclusively
- [x] **Dependency Injection**: Service dependencies identified (IService_ErrorHandler, ILoggingService, IService_MySQL_Dunnage, IService_DunnageCSVWriter, IService_DunnageWorkflow, IService_DunnageValidation, IService_UserPreferences) - will be registered in App.xaml.cs
- [x] **Error Handling**: Error scenarios documented (database connection loss, icon library failure, duplicate names, validation errors, network failures, file locking, CSV export errors) with specific user-facing messages and IService_ErrorHandler usage
- [x] **Security & Authentication**: Not applicable for this feature (internal tool, no authentication/authorization requirements beyond standard application access)
- [x] **WinUI 3 Practices**: UI/UX requirements use WinUI 3 components (ContentDialog, DataGrid, ItemsRepeater, GridView, InfoBar, CalendarDatePicker, NumberBox), theme resources (AccentButtonStyle, CardBackgroundFillColorDefaultBrush), and Fluent Design principles (corner radius, spacing, typography)
- [x] **Specification-Driven**: This spec is technology-agnostic (no XAML markup in requirements section) and user-focused (detailed user stories with personas, workflows, pain points, acceptance scenarios)

### Special Constraints

- [x] **Infor Visual Integration**: Not applicable - this feature only interacts with MySQL mtm_receiving_application database (full CRUD allowed)
- [x] **MySQL 5.7.24 Compatibility**: Validation rules stored as JSON string in TEXT column (not JSON column type). Spec values stored as JSON in TEXT columns. No use of CTEs, window functions, or CHECK constraints
- [x] **Async Operations**: All database operations (GetAllTypes, InsertType, GetPartById, InsertLoad, ExportCSV, GetLoadsByDateRange, etc.) will be async with proper IsBusy flag and timeout handling

### Notes

**Dialog vs Window Pattern**: Add New Type Dialog uses ContentDialog (modal overlay) rather than Window because:
- User must complete or cancel the action before returning to main workflow
- No need for independent window controls (minimize, maximize, resize to arbitrary dimensions)
- Simpler focus management and keyboard navigation
- Standard WinUI 3 pattern for form-based data entry

**Window Sizing Exception**: Per `.github/instructions/window-sizing.instructions.md`, dialogs use MaxHeight="750" instead of fixed window size. The constitution's window sizing standards apply to Window-based classes, not ContentDialog.

**Validation Approach**: Real-time validation (300ms debounce) balances UX responsiveness with performance. Validation logic resides in ViewModel using CommunityToolkit.Mvvm's ObservableValidator or manual validation methods that update error properties bound to XAML.

**Field Reordering Implementation**: Drag-and-drop uses CanDragItems="True" and CanReorderItems="True" on ItemsRepeater with DragItemsCompleted event handler updating DisplayOrder property in ViewModel collection. No direct database update until "Add Type" is clicked.

**Icon Persistence**: Recently used icons stored per-user in user_preferences table with key icon_usage_history and JSON value `[{"glyph":"&#xE7B8;","count":15,"last_used":"2025-12-20T14:32:00Z"},...]`. Sorted by count descending, top 6 displayed.

**Sanitization Safety**: Field names like "Weight (lbs)" sanitized to "weight_lbs" for database columns. Collision detection (two fields sanitizing to same name) handled with append suffix "_2", "_3", etc. Display names preserved in field_name column, sanitized names in database_column_name column.

**Manual Entry vs Wizard**: Manual entry mode complements the wizard workflow (not a replacement). Wizard remains primary workflow for new users and complex scenarios. Manual entry targets power users processing high-volume batches.

**Edit Mode Philosophy**: Edit mode is for corrections and review, not primary data entry. It loads from three sources (Memory, Labels, History) to support different correction workflows. Hard delete is appropriate here (not soft delete) because user is explicitly fixing mistakes.

**Admin UI Navigation**: Admin interface navigates within MainWindow content area (not modal dialogs or separate windows). Matches existing Settings navigation pattern. Hub-based navigation provides clear entry/exit points for each management area.

**CSV Export Dual-Write Strategy**: Writing to both local AppData (guaranteed) and network share (best-effort) ensures workflow continuity when network is unavailable while still supporting centralized label management. Local-first approach prevents network issues from blocking receiving operations.

**Dynamic CSV Columns**: Union of all spec keys approach future-proofs label templates. When new specs are added to types, they automatically appear in CSV exports without requiring template updates. This is critical for maintainability in a manufacturing environment where packaging evolves frequently.


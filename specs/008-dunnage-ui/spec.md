# Feature Specification: Dunnage Wizard Workflow UI

**Feature Branch**: `008-dunnage-ui`  
**Created**: 2025-12-26  
**Status**:  Ready for Implementation  
**Parent Feature**:  Dunnage Receiving System V2  
**Depends On**: `007-architecture-compliance` & `006-dunnage-services`

## Overview

Create the complete wizard workflow user interface for guided dunnage receiving.  This includes the main orchestrator page and all step-specific views (Mode Selection → Type Selection → Part Selection → Quantity Entry → Details Entry → Review & Save).

**Architecture Principle**: Strict MVVM pattern with compile-time bindings (`x:Bind`). Each step is a separate UserControl with dedicated ViewModel.  Main orchestrator manages step visibility.

**Design Reference**: A complete HTML/CSS/JavaScript mockup is available in `specs/008-dunnage-ui/mockups/` and MUST be used as the visual design reference during implementation. The mockup demonstrates:
- Exact layout and spacing for all workflow modes (Wizard, Manual Entry, Edit Mode)
- Button placement, grid structures, and form layouts
- InfoBar positioning and messaging patterns
- Mode selection card design with default preference checkboxes
- Pagination controls and navigation patterns matching existing Receiving workflow UI

All XAML views should replicate the mockup's structure using WinUI 3 controls with equivalent styling from the application's theme.

**Color Consistency Requirement**: All UI elements MUST use the same WinUI 3 theme resources as the existing Receiving workflow to maintain visual consistency:
- **Accent headers**: `{ThemeResource AccentFillColorDefaultBrush}` for header backgrounds with white foreground text
- **Card containers**: `{ThemeResource CardBackgroundFillColorDefaultBrush}` for card backgrounds
- **Card borders**: `{ThemeResource CardStrokeColorDefaultBrush}` for card border colors
- **Accent icons**: `{ThemeResource AccentTextFillColorPrimaryBrush}` for icon foreground colors
- **Text styles**: `{StaticResource BodyStrongTextBlockStyle}` for emphasized text
- **Corner radius**: 8px standard for all rounded corners (CornerRadius="8")
- **Padding/spacing**: 16px standard padding, 12px vertical padding for headers, 8px spacing between elements

Refer to existing Receiving views (WeightQuantityView.xaml, ReviewGridView.xaml) for exact color resource usage patterns. 

## User Scenarios & Testing

### User Story 1 - Wizard Orchestration & Mode Selection (Priority: P1)

As a **receiving user**, I need to select between Wizard, Manual Entry, or Edit Mode so that I can choose the workflow that matches my receiving scenario, and optionally set a default mode to skip this selection in future sessions.

**Why this priority**: Mode selection is the entry point for all dunnage workflows. Without it, users cannot access any functionality. Default mode preference improves efficiency for users who always use the same workflow.

**Independent Test**: Can be tested by launching the Dunnage page, verifying mode selection view displays with 3 options, clicking each option, confirming correct workflow navigation, and testing default mode persistence.

**Acceptance Scenarios**: 

1. **Given** user navigates to Dunnage Label page, **When** page loads, **Then** mode selection view is visible with Wizard, Manual Entry, and Edit Mode cards, each with "Set as default mode" checkbox
2. **Given** mode selection view is displayed, **When** user clicks "Guided Wizard" card, **Then** type selection view becomes visible and mode selection hides
3. **Given** mode selection view is displayed, **When** user clicks "Manual Entry" card, **Then** manual entry grid view becomes visible
4. **Given** mode selection view is displayed, **When** user clicks "Edit Mode" card, **Then** edit mode view becomes visible
5. **Given** wizard orchestrator ViewModel, **When** step changes, **Then** corresponding view visibility property updates and only one step view is visible
6. **Given** user checks "Set as default mode" on Wizard card, **When** checkbox is checked, **Then** preference is saved and page automatically navigates to Wizard mode on next launch
7. **Given** user has set a default mode, **When** page loads, **Then** mode selection is skipped and default mode view displays immediately
8. **Given** user is in any workflow mode, **When** "Mode Selection" button is clicked, **Then** all workflows hide and mode selection view becomes visible (clears default temporarily)

---

### User Story 2 - Dynamic Type Selection with Pagination (Priority: P1)

As a **receiving user**, I need to select a dunnage type from a paginated button grid (9 types per page) so that I can classify the dunnage being received.

**Why this priority**:  Type selection is the first data-gathering step in the wizard. It determines which parts and specs are available in subsequent steps.

**Independent Test**: Can be tested by loading type selection view, verifying 9 types display in 3x3 grid, using Next/Previous buttons to paginate, selecting a type, and confirming advancement to part selection.

**Acceptance Scenarios**:

1. **Given** 11 seed types exist in database, **When** type selection view loads, **Then** first 9 types display in 3x3 button grid with page "1 of 2"
2. **Given** page 1 of type grid, **When** user clicks "Next Page", **Then** remaining 2 types display on page 2
3. **Given** page 2 of type grid, **When** user clicks "Previous Page", **Then** first 9 types display again
4. **Given** type selection view, **When** user clicks "Pallet" button, **Then** type is selected, stored in session, and part selection view appears
5. **Given** type selection view, **When** user clicks "+ Add New Type" quick add button, **Then** add type dialog opens without leaving wizard

---

### User Story 3 - Part Selection with Inventory Notification (Priority: P1)

As a **receiving user**, I need to select or enter a part ID and see inventory notification if the part requires Visual ERP tracking so that I know the receiving method required.

**Why this priority**: Part selection is the core data identifier for receiving.  Inventory notification prevents data quality issues. 

**Independent Test**: Can be tested by entering a part ID, verifying inventory check occurs, confirming notification displays with correct method (Adjust In/Receive In), and advancing to quantity entry.

**Acceptance Scenarios**:

1. **Given** type "Pallet" is selected, **When** part selection view loads, **Then** dropdown shows all parts of type "Pallet"
2. **Given** part dropdown, **When** user selects "PALLET-48X40", **Then** part details display (Width: 48, Height: 40, Depth: 6, Inventoried: Yes)
3. **Given** part "PALLET-48X40" is in inventoried list, **When** selection occurs, **Then** InfoBar displays "⚠️ This part requires inventory in Visual.  Method: Adjust In"
4. **Given** inventoried part with no PO entered yet, **When** part loads, **Then** inventory method defaults to "Adjust In"
5. **Given** part selection view, **When** user clicks "+ Add New Part", **Then** quick add dialog opens with type pre-selected

---

### User Story 4 - Quantity Entry (Priority: P1)

As a **receiving user**, I need to enter the quantity being received so that the correct amount is recorded in the transaction.

**Why this priority**:  Quantity is a required field for all receiving transactions. Simple step with validation.

**Independent Test**: Can be tested by entering various quantities (valid, zero, negative), verifying validation, and confirming advancement to details entry.

**Acceptance Scenarios**:

1. **Given** part selection is complete, **When** quantity entry view loads, **Then** NumberBox defaults to 1 with selected type and part name displayed
2. **Given** quantity NumberBox, **When** user enters 10, **Then** value updates to 10 and Next button becomes enabled
3. **Given** quantity NumberBox, **When** user enters 0, **Then** validation error displays "Quantity must be greater than 0"
4. **Given** valid quantity entered, **When** user clicks Next, **Then** details entry view appears with quantity stored in session
5. **Given** quantity view, **When** user clicks Back, **Then** part selection view reappears with previous selection intact

---

### User Story 5 - Dynamic Details Entry with Spec Inputs (Priority: P1)

As a **receiving user**, I need to enter PO number, location, and dynamic specification values (generated from type's spec schema) so that I can provide complete receiving information.

**Why this priority**: Details entry captures all optional/variable data.  Dynamic spec generation is core architectural feature.

**Independent Test**: Can be tested by viewing details form, verifying spec input controls match selected type's schema (TextBox for numbers, CheckBox for boolean), entering values, and confirming inventory method updates when PO changes.

**Acceptance Scenarios**:

1. **Given** selected type "Pallet" has specs (Width, Height, Depth, IsInventoriedToVisual), **When** details view loads, **Then** 4 input controls are generated matching data types
2. **Given** spec "Width" (number type), **When** form renders, **Then** NumberBox control displays with label "Width" and unit "inches"
3. **Given** spec "IsInventoriedToVisual" (boolean type), **When** form renders, **Then** CheckBox control displays with label
4. **Given** details form with inventoried part, **When** user enters PO Number "PO123456", **Then** inventory method updates to "Receive In" and InfoBar message updates
5. **Given** details form with inventoried part, **When** user clears PO Number, **Then** inventory method reverts to "Adjust In"

---

### User Story 6 - Review & Save with Add Another (Priority: P1)

As a **receiving user**, I need to review all entered data, save it as a batch, and optionally add another load to the same session so that I can efficiently receive multiple items.

**Why this priority**: Review is the final validation step before database commit and CSV export. Add Another enables batching. 

**Independent Test**: Can be tested by completing wizard to review step, verifying data grid shows all entered values, clicking Add Another to return to type selection with session intact, clicking Save All to persist and export CSV.

**Acceptance Scenarios**:

1. **Given** all wizard steps completed, **When** review view loads, **Then** DataGrid displays load with all values (Type, PartID, Qty, PO, Location, specs)
2. **Given** review view with 1 load, **When** user clicks "Add Another", **Then** type selection view appears and current load remains in session
3. **Given** review view with 2 loads, **When** data displays, **Then** both loads are visible in grid
4. **Given** review view with loads, **When** user clicks "Save All", **Then** loads are inserted into database, CSV is exported, success message displays
5. **Given** successful save, **When** operation completes, **Then** session clears and workflow returns to mode selection

---

### User Story 7 - Bulk Operations & Data Utilities (Priority: P2)

As a **receiving user**, I need to auto-fill repeated values, sort loads for optimal printing, and save sessions to history so that I can work efficiently with batch data entry.

**Why this priority**: Utility functions improve productivity for users entering multiple loads with similar data. Auto-fill reduces repetitive typing, sorting optimizes label printing order, and history saves allow resuming incomplete sessions.

**Independent Test**: Can be tested by entering multiple loads with common values, clicking "Fill Blank Spaces" to auto-populate from last entry, clicking "Sort for Printing" to reorder by Part ID/PO, and clicking "Save to History" to persist session for later completion.

**Acceptance Scenarios**:

1. **Given** user has entered 3 loads with PO "PO123456", **When** user starts 4th load and clicks "Fill Blank Spaces", **Then** PO field auto-fills with "PO123456" from previous load
2. **Given** manual entry grid has 10 loads with varying Part IDs, **When** user clicks "Sort for Printing", **Then** loads are reordered by Part ID (ascending), then PO Number (ascending)
3. **Given** review view has 5 loads, **When** user clicks "Save to History", **Then** loads are saved to database with status "In Progress" and session ID is stored for later retrieval
4. **Given** user has incomplete history session, **When** user navigates to Edit Mode and clicks "Load from History", **Then** previously saved loads are loaded into grid for continued editing
5. **Given** auto-fill is active, **When** user manually changes a field value, **Then** auto-fill respects manual input and does not overwrite
6. **Given** loads are sorted, **When** new load is added, **Then** it appears at bottom of grid (unsorted position) until "Sort for Printing" is clicked again
7. **Given** user clicks "Save to History" with validation errors, **When** invalid loads exist, **Then** error message displays "Cannot save: 2 loads have validation errors. Fix before saving to history."

**Implementation Notes**:
- Auto-fill logic MUST copy values from most recent load: PO Number, Location, and spec values (but NOT Type, Part ID, or Quantity)
- Sorting priority: Part ID (ascending) → PO Number (ascending) → Type Name (ascending)
- History saves MUST include session ID (GUID), timestamp, employee number, and status flag ("In Progress" vs "Completed")
- "Save to History" is different from "Save All" - history preserves incomplete work, "Save All" marks loads as completed and exports CSV

---

### Edge Cases

- What happens when type selection loads but database is unreachable?  (Error message, disable type selection)
- What happens when user navigates Back from quantity entry to part selection? (Previous part selection is restored from session)
- What happens when dynamic spec inputs are generated but part has no spec values? (Inputs render empty, optional fields)
- What happens when CSV export fails but database save succeeds? (Show partial success message, loads saved but labels may be manual)
- What happens when user tries to advance without filling required spec fields? (Validation blocks advancement, highlight missing fields)

## Requirements

### Functional Requirements - Main Orchestrator

#### DunnageLabelPage.xaml + DunnageWorkflowViewModel
- **FR-001**: Page MUST provide navigation from MainWindow NavigationView menu
- **FR-002**: Page MUST host all wizard step UserControls with visibility bindings
- **FR-003**: ViewModel MUST manage step visibility flags: IsModeSelectionVisible, IsManualEntryVisible, IsEditModeVisible, IsTypeSelectionVisible, IsPartSelectionVisible, IsQuantityEntryVisible, IsDetailsEntryVisible, IsReviewVisible
- **FR-004**: ViewModel MUST inject IService_DunnageWorkflow for state management
- **FR-005**: ViewModel MUST subscribe to workflow StepChanged event and update visibility flags
- **FR-006**: Only ONE step view MUST be visible at any time (mutual exclusion)
- **FR-007**: Page MUST display InfoBar at top for status messages with severity (Info, Success, Warning, Error)
- **FR-008**: ViewModel MUST provide IsStatusOpen, StatusMessage, and StatusSeverity properties for InfoBar binding
- **FR-009**: ViewModel MUST provide ReturnToModeSelectionCommand accessible from all workflow modes
- **FR-010**: "Mode Selection" button MUST be visible in all non-mode-selection views

### Functional Requirements - Mode Selection

#### DunnageModeSelectionView.xaml + DunnageModeSelectionViewModel
- **FR-011**: View MUST display 3 cards: Guided Wizard Mode, Manual Entry Mode, Edit Mode
- **FR-011a**: Mode selection cards MUST use `{ThemeResource CardBackgroundFillColorDefaultBrush}` background, `{ThemeResource CardStrokeColorDefaultBrush}` border, CornerRadius="8", and 16px padding to match Receiving UI card styling
- **FR-012**: Each card MUST show mode icon, title, and description
- **FR-013**: Wizard card MUST describe "Step-by-step process for standard receiving workflow"
- **FR-014**: Manual card MUST describe "Customizable grid for bulk data entry and editing"
- **FR-015**: Edit card MUST describe "Edit existing loads without adding new ones"
- **FR-016**: Each card MUST have "Set as default mode" checkbox below it
- **FR-017**: ViewModel MUST provide SelectGuidedModeCommand, SelectManualModeCommand, SelectEditModeCommand
- **FR-018**: ViewModel MUST provide IsGuidedModeDefault, IsManualModeDefault, IsEditModeDefault properties
- **FR-019**: ViewModel MUST provide SetGuidedAsDefaultCommand, SetManualAsDefaultCommand, SetEditAsDefaultCommand
- **FR-020**: Commands MUST save preference using IService_UserPreferences
- **FR-021**: Commands MUST call workflow service to transition to appropriate step/mode
- **FR-022**: ViewModel MUST check default mode preference on initialization and auto-navigate if set
- **FR-023**: Only ONE mode can be set as default at a time (mutually exclusive checkboxes)

### Functional Requirements - Type Selection

#### DunnageTypeSelectionView.xaml + DunnageTypeSelectionViewModel
- **FR-024**: View MUST display types in 3x3 button grid (9 types per page)
- **FR-024a**: Type selection buttons MUST be styled as cards with `{ThemeResource CardBackgroundFillColorDefaultBrush}` background, `{ThemeResource CardStrokeColorDefaultBrush}` border, CornerRadius="8", and hover effects matching Receiving UI button patterns
- **FR-025**: View MUST provide pagination controls: Previous Page, Page X of Y, Next Page
- **FR-026**: View MUST provide "+ Add New Type" quick add button
- **FR-027**: ViewModel MUST load all types from `IService_MySQL_Dunnage` on initialization
- **FR-028**: ViewModel MUST calculate total pages: Ceiling(TotalTypes / 9)
- **FR-029**: ViewModel MUST maintain CurrentPage property and update DisplayedTypes collection when page changes
- **FR-030**: ViewModel MUST provide SelectTypeCommand that stores type in session and advances workflow
- **FR-031**: ViewModel MUST provide NextPageCommand, PreviousPageCommand with enable/disable logic
- **FR-032**: Quick add button MUST open dialog/flyout without leaving wizard (session preservation)

### Functional Requirements - Part Selection

#### DunnagePartSelectionView.xaml + DunnagePartSelectionViewModel
- **FR-033**: View MUST display ComboBox with parts filtered by selected type
- **FR-034**: View MUST display "+ Add New Part" quick add button
- **FR-035**: View MUST display InfoBar for inventory notification (initially hidden)
- **FR-036**: View MUST display part details panel (Type, spec values, inventory status)
- **FR-037**: ViewModel MUST load parts for selected type using `IService_MySQL_Dunnage.GetPartsByTypeAsync`
- **FR-038**: ViewModel MUST call `IService_MySQL_Dunnage.IsPartInventoriedAsync` when part is selected
- **FR-029**: ViewModel MUST display InfoBar if part is inventoried with method "Adjust In" (no PO yet)
- **FR-030**: ViewModel MUST provide SelectPartCommand that stores part in session and advances workflow
- **FR-031**: InfoBar message MUST be "⚠️ This part requires inventory in Visual. Method: Adjust In"

### Functional Requirements - Quantity Entry

#### DunnageQuantityEntryView.xaml + DunnageQuantityEntryViewModel
- **FR-032**: View MUST display NumberBox for quantity with minimum value 1
- **FR-033**: View MUST display selected type and part name for context
- **FR-033a**: Context header MUST use `{ThemeResource AccentFillColorDefaultBrush}` background with white foreground text and `{StaticResource BodyStrongTextBlockStyle}`, matching WeightQuantityView.xaml info header pattern
- **FR-034**: ViewModel MUST initialize Quantity property to 1 by default
- **FR-035**: ViewModel MUST validate Quantity > 0 before allowing advancement
- **FR-036**: ViewModel MUST provide GoNextCommand that stores quantity in session and advances
- **FR-037**: ViewModel MUST provide GoBackCommand that returns to part selection with session intact

### Functional Requirements - Details Entry

#### DunnageDetailsEntryView.xaml + DunnageDetailsEntryViewModel
- **FR-038**: View MUST display TextBox for PO Number (optional)
- **FR-039**: View MUST display TextBox for Location (optional)
- **FR-040**: View MUST display InfoBar for inventoried parts with dynamic method
- **FR-040a**: InfoBar MUST use Severity="Informational" (blue accent color) to match Receiving UI InfoBar styling, NOT Warning/Error severity
- **FR-041**: View MUST dynamically generate spec input controls based on selected part's type specs
- **FR-042**: ViewModel MUST load spec definitions from selected part's type using `IService_MySQL_Dunnage.GetSpecsForTypeAsync`
- **FR-043**: ViewModel MUST generate controls:  NumberBox for "number", TextBox for "text", CheckBox for "boolean"
- **FR-044**: ViewModel MUST bind spec input values to SpecValues dictionary
- **FR-045**: ViewModel MUST implement OnPoNumberChanged partial method to update inventory method
- **FR-046**: When PO is blank, inventory method MUST be "Adjust In"; when PO has value, method MUST be "Receive In"
- **FR-047**: InfoBar message MUST update dynamically:  "⚠️ This part requires inventory in Visual. Method: {InventoryMethod}"
- **FR-048**: ViewModel MUST validate required specs before allowing advancement

### Functional Requirements - Review & Save

#### DunnageReviewView.xaml + DunnageReviewViewModel
- **FR-049**: View MUST display DataGrid with all loads in session (columns: Type, PartID, Qty, PO, Location, Method)
- **FR-049a**: DataGrid container MUST use `{ThemeResource CardBackgroundFillColorDefaultBrush}` background and `{ThemeResource CardStrokeColorDefaultBrush}` border with CornerRadius="8" to match ReviewGridView.xaml card styling
- **FR-050**: View MUST display "Add Another" button to return to type selection
- **FR-051**: View MUST display "Save All" button to persist and export
- **FR-052**: View MUST display "Cancel" button to clear session and return to mode selection
- **FR-053**: ViewModel MUST bind to workflow service's CurrentSession. Loads collection
- **FR-054**: ViewModel MUST provide AddAnotherCommand that calls workflow. GoToStep(TypeSelection) without clearing session
- **FR-055**: ViewModel MUST provide SaveAllCommand that calls workflow.SaveSessionAsync()
- **FR-056**: Save operation MUST insert loads to database via `IService_MySQL_Dunnage.SaveLoadsAsync`
- **FR-057**: Save operation MUST export CSV via `IService_DunnageCSVWriter.WriteToCSVAsync`
- **FR-058**: Save success MUST display TeachingTip or InfoBar with "Successfully saved X loads and exported labels"
- **FR-059**: Save success MUST clear session and return to mode selection

### Functional Requirements - Bulk Operations & Data Utilities

#### Shared Across Review, Manual Entry, and Edit Mode ViewModels
- **FR-059a**: ViewModels MUST provide FillBlankSpacesCommand that auto-fills empty fields from most recent load
- **FR-059b**: Auto-fill logic MUST copy: PO Number, Location, and all spec values from previous load
- **FR-059c**: Auto-fill logic MUST NOT copy: Type, Part ID, Quantity, or Load ID
- **FR-059d**: Auto-fill MUST preserve any user-entered values (do not overwrite manual input)
- **FR-059e**: ViewModels MUST provide SortForPrintingCommand that reorders loads
- **FR-059f**: Sort order MUST be: Part ID (ascending) → PO Number (ascending) → Type Name (ascending)
- **FR-059g**: ViewModels MUST provide SaveToHistoryCommand that persists session as "In Progress"
- **FR-059h**: History save MUST generate unique session GUID and store with timestamp and employee number
- **FR-059i**: History save MUST validate all loads before persisting (quantity > 0, required specs filled)
- **FR-059j**: History save MUST call IService_MySQL_Dunnage.SaveSessionToHistoryAsync with session object
- **FR-059k**: View toolbar MUST provide "Fill Blank Spaces", "Sort for Printing", and "Save to History" buttons alongside "Save All"
- **FR-059l**: "Save to History" button MUST be enabled when 1+ loads exist, regardless of validation state
- **FR-059m**: "Save to History" with invalid loads MUST display error: "Cannot save: X loads have validation errors. Fix before saving to history."
- **FR-059n**: Successful history save MUST display InfoBar message: "Session saved to history (Session ID: {GUID})"
- **FR-059o**: Edit Mode "Load from History" MUST query IService_MySQL_Dunnage.GetHistorySessionsAsync and display sessions in dropdown with timestamp

### Functional Requirements - Manual Entry Mode

#### DunnageManualEntryView.xaml + DunnageManualEntryViewModel
- **FR-060**: View MUST display editable DataGrid for batch entry with columns: Type, Part ID, Quantity, PO Number, Location, Specs
- **FR-061**: View MUST provide "Add Row" button to add single empty row
- **FR-062**: View MUST provide "Add Multiple" button to add N empty rows via dialog
- **FR-063**: View MUST provide "Remove Row" button to delete selected rows
- **FR-064**: View MUST provide "Auto-Fill" button to populate specs based on selected part
- **FR-065**: View MUST provide "Mode Selection" button to return to mode selection
- **FR-066**: View MUST display InfoBar at top showing "Workflow cleared. Please select a mode" initially
- **FR-067**: View MUST display "Save & Finish" button to persist all valid rows
- **FR-068**: ViewModel MUST provide ObservableCollection<Model_DunnageLoad> for grid binding
- **FR-069**: ViewModel MUST validate rows: Type, Part ID, and Quantity > 0 are required
- **FR-070**: ViewModel MUST highlight invalid rows visually (background color or border)
- **FR-071**: ViewModel MUST provide AddRowCommand that appends new Model_DunnageLoad to collection
- **FR-072**: ViewModel MUST provide AddMultipleCommand that shows dialog and adds N rows
- **FR-073**: ViewModel MUST provide RemoveRowCommand that removes selected rows
- **FR-074**: ViewModel MUST provide AutoFillCommand that populates part specs from database
- **FR-075**: ViewModel MUST provide SaveAllCommand that validates and persists valid rows only
- **FR-076**: Save operation MUST call IService_MySQL_Dunnage.SaveLoadsAsync with valid rows
- **FR-077**: Save operation MUST export CSV via IService_DunnageCSVWriter.WriteToCSVAsync
- **FR-078**: Save success MUST display count of saved rows and clear grid

### Functional Requirements - Edit Mode

#### DunnageEditModeView.xaml + DunnageEditModeViewModel
- **FR-079**: View MUST display filter toolbar with "Load Data From" buttons: Current Memory, Current Labels, History
- **FR-080**: View MUST provide date range filter with DatePickers (From/To) and quick buttons (Last Week, Today, This Week, December, Oct-Dec, Show All)
- **FR-081**: View MUST display editable DataGrid showing historical loads
- **FR-082**: View MUST provide "Select All" button to select all filtered rows
- **FR-083**: View MUST provide "Remove Row" button to delete selected rows from database
- **FR-084**: View MUST provide "Mode Selection" button to return to mode selection
- **FR-085**: View MUST display InfoBar showing "Workflow cleared. Please select a mode" initially
- **FR-086**: View MUST display "Save & Finish" button to commit edits
- **FR-087**: View MUST display pagination controls at bottom (First, Previous, Page X of Y, Go, Next, Last)
- **FR-088**: ViewModel MUST provide LoadFromMemoryCommand that loads current session loads
- **FR-089**: ViewModel MUST provide LoadFromLabelsCommand that loads last exported CSV
- **FR-090**: ViewModel MUST provide LoadFromHistoryCommand that queries database for loads within date range
- **FR-091**: ViewModel MUST filter loads by date range using IService_MySQL_Dunnage.GetLoadsByDateRangeAsync
- **FR-092**: ViewModel MUST support pagination with configurable page size (default 50 rows per page)
- **FR-093**: ViewModel MUST provide SelectAllCommand, RemoveRowCommand
- **FR-094**: ViewModel MUST provide SaveAllCommand that updates modified rows in database
- **FR-095**: Remove operation MUST call IService_MySQL_Dunnage.DeleteLoadsAsync with selected load IDs
- **FR-096**: Save operation MUST call IService_MySQL_Dunnage.UpdateLoadsAsync with edited rows
- **FR-097**: Save success MUST display count of updated/deleted rows

## Success Criteria

### Measurable Outcomes

- **SC-001**: User can complete full wizard workflow from mode selection to save in under 2 minutes
- **SC-002**: Type selection pagination displays 9 types per page with correct page count
- **SC-003**: Dynamic spec inputs are generated correctly for all seed type schemas (Width, Height, Depth, IsInventoriedToVisual)
- **SC-004**: Inventory notification displays and updates correctly when part is inventoried and PO changes
- **SC-005**: Review grid displays all loads with correct data in all columns
- **SC-006**: "Add Another" workflow accumulates multiple loads in same session
- **SC-007**: "Save All" successfully persists loads and exports CSV to both local and network paths
- **SC-008**: Validation prevents advancement with invalid data (quantity=0, required specs missing)
- **SC-009**: Quick add dialogs open without losing wizard session state
- **SC-010**: Back navigation preserves previously entered values
- **SC-011**: Manual Entry mode allows batch entry of 10+ rows in under 3 minutes
- **SC-012**: Edit Mode loads and filters 100+ historical records in under 2 seconds
- **SC-013**: Default mode preference persists across application sessions
- **SC-014**: InfoBar messages display with appropriate severity and dismiss correctly

## Non-Functional Requirements

- **NFR-001**: All views MUST use x:Bind (compile-time binding) instead of Binding
- **NFR-002**: All views MUST use WinUI 3 controls (NavigationView, InfoBar, NumberBox, TeachingTip)
- **NFR-003**: Window size MUST be 1400x900 pixels (standard receiving window size)
- **NFR-004**: Type selection buttons MUST be visually distinct with hover effects
- **NFR-004a**: All UI elements MUST use WinUI 3 theme resources (`{ThemeResource ...}`) for colors to match existing Receiving workflow visual style. NO hardcoded color values (#RRGGBB) are permitted except in theme resource definitions
- **NFR-005**: Spec input controls MUST display unit labels where applicable (e.g., "inches")
- **NFR-006**: All user-facing text MUST be clear and concise (no technical jargon)
- **NFR-007**: InfoBar severity MUST be "Informational" (blue accent color via Severity="Informational", not yellow Warning or red Error) to match Receiving UI notification patterns
- **NFR-008**: UI layout MUST match mockup in `specs/008-dunnage-ui/mockups/` for consistency with existing Receiving workflow
- **NFR-009**: All accent colors (headers, icons, highlights) MUST use `{ThemeResource AccentFillColorDefaultBrush}` and `{ThemeResource AccentTextFillColorPrimaryBrush}` to ensure system-wide theme consistency

## Implementation Guidance

### Design Reference Priority

When implementing this feature, follow this reference hierarchy:

1. **Primary**: This specification (`specs/008-dunnage-ui/spec.md`) - Authoritative requirements, functional specifications, and acceptance criteria
2. **Secondary**: HTML Mockup (`specs/008-dunnage-ui/mockups/`) - Visual design reference for layout, spacing, and UI patterns
3. **Tertiary**: Existing Receiving Workflow UI - Fallback for any patterns, behaviors, or implementation details not covered by spec or mockup

### Using the Mockup

The HTML mockup in `specs/008-dunnage-ui/mockups/` serves as a **visual design reference only** during implementation. It demonstrates UI structure and interaction patterns but does NOT supersede functional requirements in this specification.

**What the mockup provides:**

1. **Visual Layout**: Exact spacing, alignment, and control placement
2. **Grid Structures**: DataGrid column widths, row heights, and scroll behavior
3. **Button Placement**: Toolbar buttons, navigation controls, and action buttons positioned
4. **InfoBar Patterns**: Status messages, severity colors, and positioning examples
5. **Mode Selection Cards**: Card layout, checkbox placement, and descriptions
6. **Form Layouts**: Input field widths, label alignment, and grouping

**When mockup is insufficient:**

- Refer to existing **Receiving Workflow** pages (Receiving Labels) for similar patterns
- Match button styles, grid behaviors, navigation patterns, and error handling from Receiving UI
- Maintain consistency with established application conventions (e.g., "Mode Selection" button placement, InfoBar usage, validation patterns)

### Mockup Files

- **`index.html`** - Mode selection landing page
- **`pages/dunnage-wizard.html`** - Guided wizard with all 6 steps
- **`pages/manual-entry.html`** - Grid-based batch entry mode
- **`pages/edit-mode.html`** - History review and editing mode
- **`css/styles.css`** - Custom component styles
- **`css/winui-theme.css`** - WinUI 3 color theme variables
- **`js/*.js`** - Functional logic demonstrating workflows

### Testing the Mockup

Open `specs/008-dunnage-ui/mockups/index.html` in a web browser to interact with all three workflow modes and verify expected behavior before implementing in WinUI 3.

## Out of Scope

- ❌ Wizard skip-ahead navigation (must proceed sequentially)
- ❌ Wizard save-and-resume from middle step (complete or cancel only)
- ❌ Barcode scanning for Part ID (manual entry only)
- ❌ Camera integration for specs (manual entry only)
- ❌ Bulk type/part creation from wizard (use Admin UI)
- ❌ Print preview of labels (CSV export only)

## Dependencies

- 004-services-layer (IService_DunnageWorkflow, IService_MySQL_Dunnage, IService_DunnageCSVWriter)
- Project: BaseViewModel (all ViewModels inherit from this)
- NuGet: CommunityToolkit.Mvvm (for RelayCommand, ObservableProperty)
- NuGet: Microsoft.WindowsAppSDK (for WinUI 3 controls)

## Files to be Created

### Main Page
- `Views/Receiving/DunnageLabelPage. xaml`
- `Views/Receiving/DunnageLabelPage.xaml.cs`
- `ViewModels/Receiving/DunnageWorkflowViewModel.cs`

### Wizard Step Views
- `Views/Receiving/DunnageModeSelectionView.xaml`
- `Views/Receiving/DunnageModeSelectionView.xaml.cs`
- `ViewModels/Receiving/DunnageModeSelectionViewModel.cs`

- `Views/Receiving/DunnageTypeSelectionView.xaml`
- `Views/Receiving/DunnageTypeSelectionView.xaml.cs`
- `ViewModels/Receiving/DunnageTypeSelectionViewModel. cs`

- `Views/Receiving/DunnagePartSelectionView.xaml`
- `Views/Receiving/DunnagePartSelectionView.xaml. cs`
- `ViewModels/Receiving/DunnagePartSelectionViewModel.cs`

- `Views/Receiving/DunnageQuantityEntryView.xaml`
- `Views/Receiving/DunnageQuantityEntryView.xaml.cs`
- `ViewModels/Receiving/DunnageQuantityEntryViewModel.cs`

- `Views/Receiving/DunnageDetailsEntryView.xaml`
- `Views/Receiving/DunnageDetailsEntryView. xaml.cs`
- `ViewModels/Receiving/DunnageDetailsEntryViewModel.cs`

- `Views/Receiving/DunnageReviewView.xaml`
- `Views/Receiving/DunnageReviewView.xaml.cs`
- `ViewModels/Receiving/DunnageReviewViewModel.cs`

## Review & Acceptance Checklist

### Requirement Completeness
- [x] All 6 wizard steps are fully specified (mode, type, part, qty, details, review)
- [x] Dynamic spec generation logic is clearly defined
- [x] Inventory notification behavior is explicit (method changes with PO)
- [x] Pagination logic for type selection is detailed
- [x] Session management across steps is specified

### Clarity & Unambiguity
- [x] Control types for each input are specified (NumberBox, TextBox, CheckBox, ComboBox)
- [x] Visibility binding strategy is clear (one step visible at a time)
- [x] Command names and behaviors are explicit
- [x] InfoBar messages are verbatim specified
- [x] Validation rules are enumerated per step

### Testability
- [x] Each user story can be tested by walking through wizard steps
- [x] Success criteria are measurable (completion time, data accuracy, UI behavior)
- [x] Edge cases define error scenarios and expected handling

### UX Quality
- [x] Wizard flow is intuitive and sequential
- [x] Visual feedback is provided at each step (InfoBar, validation errors)
- [x] Context is maintained (show selected type/part on subsequent steps)
- [x] Quick add buttons enable workflow efficiency
- [x] Add Another enables batching without restarting

## Clarifications

### Resolved Questions

**Q1**: Should wizard allow skipping steps?  
**A1**: No. Sequential navigation only. Ensures data consistency.

**Q2**: Should dynamic spec inputs show default values?  
**A2**: Yes. Load default values from spec schema JSON if defined. Otherwise blank.

**Q3**: Should inventory notification block advancement?  
**A3**: No. Informational only. User can proceed but is warned. 

**Q4**: Should type selection remember last page when user goes Back?  
**A4**: Yes. Store `_lastSelectedPage` in ViewModel session (not persistent, only within workflow session).

**Q5**: Should Save All validate all loads before saving?  
**A5**: Yes. Validate all quantities > 0 and required specs filled. Show error if any load is invalid.
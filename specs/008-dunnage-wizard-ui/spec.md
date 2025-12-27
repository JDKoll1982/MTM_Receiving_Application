# Feature Specification: Dunnage Wizard Workflow UI

**Feature Branch**: `008-dunnage-wizard-ui`  
**Created**: 2025-12-26  
**Status**:  Ready for Implementation  
**Parent Feature**:  Dunnage Receiving System V2  
**Depends On**: `007-architecture-compliance` & `006-dunnage-services`

## Overview

Create the complete wizard workflow user interface for guided dunnage receiving.  This includes the main orchestrator page and all step-specific views (Mode Selection → Type Selection → Part Selection → Quantity Entry → Details Entry → Review & Save).

**Architecture Principle**: Strict MVVM pattern with compile-time bindings (`x: Bind`). Each step is a separate UserControl with dedicated ViewModel.  Main orchestrator manages step visibility. 

## User Scenarios & Testing

### User Story 1 - Wizard Orchestration & Mode Selection (Priority: P1)

As a **receiving user**, I need to select between Wizard, Manual Entry, or Edit Mode so that I can choose the workflow that matches my receiving scenario.

**Why this priority**: Mode selection is the entry point for all dunnage workflows. Without it, users cannot access any functionality.

**Independent Test**: Can be tested by launching the Dunnage page, verifying mode selection view displays with 3 options, clicking each option, and confirming correct workflow navigation.

**Acceptance Scenarios**: 

1. **Given** user navigates to Dunnage Label page, **When** page loads, **Then** mode selection view is visible with Wizard, Manual Entry, and Edit Mode cards
2. **Given** mode selection view is displayed, **When** user clicks "Select Wizard Mode", **Then** type selection view becomes visible and mode selection hides
3. **Given** mode selection view is displayed, **When** user clicks "Select Manual Mode", **Then** manual entry grid view becomes visible
4. **Given** mode selection view is displayed, **When** user clicks "Select Edit Mode", **Then** edit mode view becomes visible
5. **Given** wizard orchestrator ViewModel, **When** step changes, **Then** corresponding view visibility property updates and only one step view is visible

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

### Edge Cases

- What happens when type selection loads but database is unreachable?  (Error message, disable type selection)
- What happens when user navigates Back from quantity entry to part selection? (Previous part selection is restored from session)
- What happens when dynamic spec inputs are generated but part has no spec values? (Inputs render empty, optional fields)
- What happens when CSV export fails but database save succeeds? (Show partial success message, loads saved but labels may be manual)
- What happens when user tries to advance without filling required spec fields? (Validation blocks advancement, highlight missing fields)

## Requirements

### Functional Requirements - Main Orchestrator

#### DunnageLabelPage. xaml + DunnageWorkflowViewModel
- **FR-001**: Page MUST provide navigation from MainWindow NavigationView menu
- **FR-002**: Page MUST host all wizard step UserControls with visibility bindings
- **FR-003**:  ViewModel MUST manage step visibility flags:  IsModeSelectionVisible, IsTypeSelectionVisible, IsPartSelectionVisible, IsQuantityEntryVisible, IsDetailsEntryVisible, IsReviewVisible
- **FR-004**: ViewModel MUST inject IService_DunnageWorkflow for state management
- **FR-005**:  ViewModel MUST subscribe to workflow StepChanged event and update visibility flags
- **FR-006**:  Only ONE step view MUST be visible at any time (mutual exclusion)

### Functional Requirements - Mode Selection

#### DunnageModeSelectionView.xaml + DunnageModeSelectionViewModel
- **FR-007**: View MUST display 3 cards:  Wizard Mode, Manual Entry Mode, Edit Mode
- **FR-008**: Each card MUST show mode icon, title, and description
- **FR-009**: Wizard card MUST describe "Step-by-step guided entry:  Type → Part → Quantity → Details → Review"
- **FR-010**:  Manual card MUST describe "Grid-based batch entry: Fill multiple rows in a data grid"
- **FR-011**: Edit card MUST describe "Review & Edit: View and modify history records with search and filter"
- **FR-012**: ViewModel MUST provide SelectWizardModeCommand, SelectManualModeCommand, SelectEditModeCommand
- **FR-013**: Commands MUST call workflow service to transition to appropriate step/mode

### Functional Requirements - Type Selection

#### DunnageTypeSelectionView.xaml + DunnageTypeSelectionViewModel
- **FR-014**: View MUST display types in 3x3 button grid (9 types per page)
- **FR-015**: View MUST provide pagination controls:  Previous Page, Page X of Y, Next Page
- **FR-016**: View MUST provide "+ Add New Type" quick add button
- **FR-017**: ViewModel MUST load all types from `IService_MySQL_Dunnage` on initialization
- **FR-018**: ViewModel MUST calculate total pages:  Ceiling(TotalTypes / 9)
- **FR-019**: ViewModel MUST maintain CurrentPage property and update DisplayedTypes collection when page changes
- **FR-020**: ViewModel MUST provide SelectTypeCommand that stores type in session and advances workflow
- **FR-021**: ViewModel MUST provide NextPageCommand, PreviousPageCommand with enable/disable logic
- **FR-022**: Quick add button MUST open dialog/flyout without leaving wizard (session preservation)

### Functional Requirements - Part Selection

#### DunnagePartSelectionView.xaml + DunnagePartSelectionViewModel
- **FR-023**: View MUST display ComboBox with parts filtered by selected type
- **FR-024**:  View MUST display "+ Add New Part" quick add button
- **FR-025**: View MUST display InfoBar for inventory notification (initially hidden)
- **FR-026**: View MUST display part details panel (Type, spec values, inventory status)
- **FR-027**: ViewModel MUST load parts for selected type using `IService_MySQL_Dunnage.GetPartsByTypeAsync`
- **FR-028**: ViewModel MUST call `IService_MySQL_Dunnage.IsPartInventoriedAsync` when part is selected
- **FR-029**: ViewModel MUST display InfoBar if part is inventoried with method "Adjust In" (no PO yet)
- **FR-030**: ViewModel MUST provide SelectPartCommand that stores part in session and advances workflow
- **FR-031**: InfoBar message MUST be "⚠️ This part requires inventory in Visual. Method: Adjust In"

### Functional Requirements - Quantity Entry

#### DunnageQuantityEntryView.xaml + DunnageQuantityEntryViewModel
- **FR-032**: View MUST display NumberBox for quantity with minimum value 1
- **FR-033**: View MUST display selected type and part name for context
- **FR-034**: ViewModel MUST initialize Quantity property to 1 by default
- **FR-035**:  ViewModel MUST validate Quantity > 0 before allowing advancement
- **FR-036**: ViewModel MUST provide GoNextCommand that stores quantity in session and advances
- **FR-037**: ViewModel MUST provide GoBackCommand that returns to part selection with session intact

### Functional Requirements - Details Entry

#### DunnageDetailsEntryView.xaml + DunnageDetailsEntryViewModel
- **FR-038**: View MUST display TextBox for PO Number (optional)
- **FR-039**: View MUST display TextBox for Location (optional)
- **FR-040**: View MUST display InfoBar for inventoried parts with dynamic method
- **FR-041**: View MUST dynamically generate spec input controls based on selected part's type specs
- **FR-042**: ViewModel MUST load spec definitions from selected part's type using `IService_MySQL_Dunnage.GetSpecsForTypeAsync`
- **FR-043**:  ViewModel MUST generate controls:  NumberBox for "number", TextBox for "text", CheckBox for "boolean"
- **FR-044**: ViewModel MUST bind spec input values to SpecValues dictionary
- **FR-045**: ViewModel MUST implement OnPoNumberChanged partial method to update inventory method
- **FR-046**: When PO is blank, inventory method MUST be "Adjust In"; when PO has value, method MUST be "Receive In"
- **FR-047**: InfoBar message MUST update dynamically:  "⚠️ This part requires inventory in Visual. Method: {InventoryMethod}"
- **FR-048**: ViewModel MUST validate required specs before allowing advancement

### Functional Requirements - Review & Save

#### DunnageReviewView.xaml + DunnageReviewViewModel
- **FR-049**: View MUST display DataGrid with all loads in session (columns: Type, PartID, Qty, PO, Location, Method)
- **FR-050**: View MUST display "Add Another" button to return to type selection
- **FR-051**:  View MUST display "Save All" button to persist and export
- **FR-052**: View MUST display "Cancel" button to clear session and return to mode selection
- **FR-053**: ViewModel MUST bind to workflow service's CurrentSession. Loads collection
- **FR-054**: ViewModel MUST provide AddAnotherCommand that calls workflow. GoToStep(TypeSelection) without clearing session
- **FR-055**:  ViewModel MUST provide SaveAllCommand that calls workflow.SaveSessionAsync()
- **FR-056**: Save operation MUST insert loads to database via `IService_MySQL_Dunnage.SaveLoadsAsync`
- **FR-057**: Save operation MUST export CSV via `IService_DunnageCSVWriter.WriteToCSVAsync`
- **FR-058**:  Save success MUST display TeachingTip or InfoBar with "Successfully saved X loads and exported labels"
- **FR-059**:  Save success MUST clear session and return to mode selection

## Success Criteria

### Measurable Outcomes

- **SC-001**: User can complete full wizard workflow from mode selection to save in under 2 minutes
- **SC-002**: Type selection pagination displays 9 types per page with correct page count
- **SC-003**: Dynamic spec inputs are generated correctly for all seed type schemas (Width, Height, Depth, IsInventoriedToVisual)
- **SC-004**: Inventory notification displays and updates correctly when part is inventoried and PO changes
- **SC-005**: Review grid displays all loads with correct data in all columns
- **SC-006**: "Add Another" workflow accumulates multiple loads in same session
- **SC-007**:  "Save All" successfully persists loads and exports CSV to both local and network paths
- **SC-008**:  Validation prevents advancement with invalid data (quantity=0, required specs missing)
- **SC-009**: Quick add dialogs open without losing wizard session state
- **SC-010**: Back navigation preserves previously entered values

## Non-Functional Requirements

- **NFR-001**: All views MUST use x:Bind (compile-time binding) instead of Binding
- **NFR-002**:  All views MUST use WinUI 3 controls (NavigationView, InfoBar, NumberBox, TeachingTip)
- **NFR-003**:  Window size MUST be 1400x900 pixels (standard receiving window size)
- **NFR-004**: Type selection buttons MUST be visually distinct with hover effects
- **NFR-005**: Spec input controls MUST display unit labels where applicable (e.g., "inches")
- **NFR-006**:  All user-facing text MUST be clear and concise (no technical jargon)
- **NFR-007**: InfoBar severity MUST be "Informational" (blue, not warning/error)

## Out of Scope

- ❌ Wizard skip-ahead navigation (must proceed sequentially)
- ❌ Wizard save-and-resume from middle step (complete or cancel only)
- ❌ Barcode scanning for Part ID (manual entry only)
- ❌ Camera integration for specs (manual entry only)
- ❌ Bulk type/part creation from wizard (use Admin UI)
- ❌ Print preview of labels (CSV export only)

## Dependencies

- 004-services-layer (IService_DunnageWorkflow, IService_MySQL_Dunnage, IService_DunnageCSVWriter)
- Project:  BaseViewModel (all ViewModels inherit from this)
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
**A1**: No.  Sequential navigation only. Ensures data consistency.

**Q2**: Should dynamic spec inputs show default values?  
**A2**: Yes. Load default values from spec schema JSON if defined.  Otherwise blank.

**Q3**: Should inventory notification block advancement?  
**A3**: No. Informational only. User can proceed but is warned. 

**Q4**: Should type selection remember last page when user goes Back?  
**A4**: Yes. Store `_lastSelectedPage` in ViewModel session (not persistent, only within workflow session).

**Q5**: Should Save All validate all loads before saving?  
**A5**: Yes. Validate all quantities > 0 and required specs filled.  Show error if any load is invalid. 
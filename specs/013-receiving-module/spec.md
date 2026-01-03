# Feature Specification: Receiving Module

**Feature Branch**: `013-receiving-module`  
**Created**: 2026-01-03  
**Status**: Active  
**Input**: User description: "Reimplement Receiving workflow with consistent naming conventions, clear module boundaries, and bug fixes"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Implement Receiving Module with Consistent Naming (Priority: P2) 🎯 MVP

As a receiving clerk, I want to use a reimplemented Receiving workflow with clear, consistent naming so that the application is easier to understand and maintain.

**Why this priority**: Receiving is the primary workflow and generates the most business value. It must be implemented first to validate the new module architecture before replicating the pattern for other modules.

**Independent Test**: Can be fully tested by navigating to "Receiving Labels" in the main menu and completing a full receiving workflow (Mode Selection → PO Entry → Package Type → Load Entry → Weight/Quantity → Heat/Lot → Review → Save). Delivers complete receiving functionality independently of other modules.

**Acceptance Scenarios**:

1. **Given** a new receiving session, **When** clerk selects Guided mode, **Then** workflow advances through all 10 steps with consistent `ViewModel_Receiving_*` naming
2. **Given** PO number entry, **When** clerk enters "63150", **Then** system auto-formats to "PO-063150" and validates against Infor Visual (READ ONLY)
3. **Given** completed receiving data, **When** clerk clicks "Save to Database", **Then** data is saved to MySQL using stored procedures and CSV is exported
4. **Given** review screen, **When** clerk clicks "Add Another Part", **Then** form clears BEFORE navigation and session preserves reviewed loads (fixes known bug)

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide Mode Selection screen with Guided and Manual entry options
- **FR-002**: System MUST validate PO numbers against Infor Visual database (READ ONLY)
- **FR-003**: System MUST auto-format PO numbers ("63150" → "PO-063150")
- **FR-004**: System MUST support Package Type selection (Box, Pallet, Loose Parts)
- **FR-005**: System MUST allow Load Entry with auto-incrementing load numbers
- **FR-006**: System MUST capture Weight/Quantity measurements
- **FR-007**: System MUST require Heat/Lot traceability information
- **FR-008**: System MUST provide Review screen with data grid and summary
- **FR-009**: System MUST save data to MySQL using stored procedures only
- **FR-010**: System MUST export CSV files compatible with LabelView 2022
- **FR-011**: System MUST fix "Add Another Part" bug (clear form BEFORE navigation)

### Key Entities

- **Receiving Load**: Main receiving record with PO, Part, Quantity, Weight, Heat/Lot, Package Type
- **Receiving Line**: Line items within a load (for multi-line loads)
- **Receiving Session**: Workflow state management (current step, entered data, reviewed loads)
- **Package Type Preference**: User preference for default package type

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete full receiving workflow in <5 minutes for typical PO
- **SC-002**: PO validation queries Infor Visual successfully (read-only, no errors)
- **SC-003**: Data saves to MySQL using stored procedures (100% compliance)
- **SC-004**: CSV files compatible with LabelView 2022 (labels print correctly)
- **SC-005**: "Add Another Part" bug fixed (form clears before navigation, session preserved)

## Assumptions *(optional)*

- Infor Visual database is accessible (may be offline occasionally)
- Users have LabelView 2022 installed and configured
- MySQL database is accessible and stored procedures are deployed
- Authentication provides employee number from context

## Dependencies *(optional)*

- **Phase 1 Infrastructure**: Complete (MVVM architecture, DI, error handling, logging)
- **Database Tables**: receiving_loads, receiving_lines, package_type_preferences (see data-model.md)
- **Stored Procedures**: sp_receiving_load_insert, sp_receiving_load_get_by_date_range, etc.
- **External Systems**: 
  - Infor Visual (SQL Server) - READ ONLY for PO/Part validation
  - LabelView 2022 - CSV import for label printing
- **Other Features**: 
  - BaseViewModel, BaseWindow (for MVVM pattern)
  - Shared End-of-Day Reporting Module (for integrated reporting)

## Out of Scope *(optional)*

- Direct integration with purchasing system (manual PO entry)
- Automatic LabelView triggering (user manually imports CSV)
- Mobile app version (Windows desktop only)
- Multi-language support (English only)

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: Views (XAML), ViewModels (business logic), Models (data), Services (operations) clearly separated
- [x] **Database Layer**: All operations via stored procedures, return Model_Dao_Result, no raw SQL
- [x] **Dependency Injection**: Services registered in App.xaml.cs (IService_ReceivingWorkflow, IService_InforVisual, etc.)
- [x] **Error Handling**: All exceptions handled via IService_ErrorHandler, user-facing errors shown in InfoBars/ContentDialogs
- [x] **Security & Authentication**: Employee number from authentication context (no additional auth required)
- [x] **WinUI 3 Practices**: x:Bind (compile-time), ObservableCollection, async/await, CommunityToolkit.Mvvm attributes ([ObservableProperty], [RelayCommand])
- [x] **Specification-Driven**: This spec follows Speckit workflow with user stories, requirements, success criteria, and templates

---

**Reference**: See [../011-module-reimplementation/spec.md](../011-module-reimplementation/spec.md) - User Story 2 for complete context


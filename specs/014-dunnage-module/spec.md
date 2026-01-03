# Feature Specification: Dunnage Module

**Feature Branch**: `014-dunnage-module`  
**Created**: 2026-01-03  
**Status**: Active  
**Input**: User description: "Reimplement Dunnage workflow with admin capabilities, consistent naming conventions, and clear module boundaries"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Implement Dunnage Module with Admin Features (Priority: P3) 🎯 MVP

As a warehouse manager, I want to use a reimplemented Dunnage workflow with admin capabilities so that I can manage dunnage types, parts, and inventory efficiently.

**Why this priority**: Dunnage is secondary to Receiving in business priority but follows the same architectural pattern. Implementing it validates that the module architecture is reusable and consistent.

**Independent Test**: Can be fully tested by navigating to "Dunnage Labels" and completing both user workflows (Type Selection → Part Selection → Details Entry → Quantity Entry → Review → Save) and admin workflows (Admin Types, Admin Parts, Admin Inventory). Delivers complete dunnage functionality independently.

**Acceptance Scenarios**:

1. **Given** dunnage workflow, **When** user selects a dunnage type, **Then** system displays Material.Icons and loads associated parts with consistent `ViewModel_Dunnage_*` naming
2. **Given** admin mode, **When** manager creates a new dunnage type, **Then** type is saved with specs and immediately available in user workflow
3. **Given** inventoried parts, **When** user selects a part, **Then** system filters to show only inventoried dunnage items
4. **Given** completed dunnage data, **When** user clicks "Save", **Then** data is saved to MySQL and CSV is exported

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide Type Selection screen with Material.Icons display
- **FR-002**: System MUST filter parts by inventoried status in user workflow
- **FR-003**: System MUST generate dynamic forms based on type specifications
- **FR-004**: System MUST support Quantity Entry for dunnage loads
- **FR-005**: System MUST provide Review screen with data grid and summary
- **FR-006**: System MUST save data to MySQL using stored procedures only
- **FR-007**: System MUST export CSV files compatible with LabelView 2022
- **FR-008**: System MUST provide Admin Types interface for CRUD operations
- **FR-009**: System MUST provide Admin Parts interface for CRUD operations
- **FR-010**: System MUST provide Admin Specs interface for custom field definitions
- **FR-011**: System MUST provide Admin Inventory interface for inventoried parts management

### Key Entities

- **Dunnage Type**: Type definition with Material.Icons code and description
- **Dunnage Part**: Part associated with type, inventoried status
- **Dunnage Spec**: Custom specification for dynamic form generation
- **Dunnage Load**: Generated dunnage load record
- **Inventoried Dunnage**: Parts marked as inventoried (filters user workflow)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete full dunnage workflow in <5 minutes
- **SC-002**: Admin can manage types, parts, and specs efficiently
- **SC-003**: Dynamic forms generated from type specifications correctly
- **SC-004**: Parts filtered by inventoried status in user workflow
- **SC-005**: CSV files compatible with LabelView 2022 (labels print correctly)

## Assumptions *(optional)*

- Material.Icons.WinUI3 NuGet package is installed
- Users have LabelView 2022 installed and configured
- MySQL database is accessible and stored procedures are deployed
- Authentication provides employee number from context

## Dependencies *(optional)*

- **Phase 1 Infrastructure**: Complete (MVVM architecture, DI, error handling, logging)
- **Database Tables**: dunnage_types, dunnage_parts, dunnage_specs, dunnage_loads, inventoried_dunnage (see data-model.md)
- **Stored Procedures**: sp_dunnage_type_*, sp_dunnage_part_*, sp_dunnage_load_* (see data-model.md)
- **External Systems**: 
  - LabelView 2022 - CSV import for label printing
- **Other Features**: 
  - BaseViewModel, BaseWindow (for MVVM pattern)
  - Shared End-of-Day Reporting Module (for integrated reporting)

## Out of Scope *(optional)*

- Direct integration with inventory management system (manual inventoried list)
- Automatic LabelView triggering (user manually imports CSV)
- Mobile app version (Windows desktop only)
- Multi-language support (English only)

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: Views (XAML), ViewModels (business logic), Models (data), Services (operations) clearly separated
- [x] **Database Layer**: All operations via stored procedures, return Model_Dao_Result, no raw SQL
- [x] **Dependency Injection**: Services registered in App.xaml.cs (IService_DunnageWorkflow, IService_DunnageAdminWorkflow, etc.)
- [x] **Error Handling**: All exceptions handled via IService_ErrorHandler, user-facing errors shown in InfoBars/ContentDialogs
- [x] **Security & Authentication**: Employee number from authentication context (no additional auth required)
- [x] **WinUI 3 Practices**: x:Bind (compile-time), ObservableCollection, async/await, CommunityToolkit.Mvvm attributes ([ObservableProperty], [RelayCommand])
- [x] **Specification-Driven**: This spec follows Speckit workflow with user stories, requirements, success criteria, and templates

---

**Reference**: See [../011-module-reimplementation/spec.md](../011-module-reimplementation/spec.md) - User Story 3 for complete context


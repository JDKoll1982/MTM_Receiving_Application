# Feature Specification: Routing Module

**Feature Branch**: `001-routing-module`  
**Created**: 2026-01-03  
**Status**: Active  
**Input**: User description: "Implement Routing module for internal routing labels with daily history tracking, auto-lookup, and label numbering"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Implement Routing Module for Internal Routing Labels (Priority: P4) 🎯 MVP

As a receiving clerk, I want to create internal routing labels for inter-department delivery so that I can efficiently route received materials to specific departments or individuals.

**Why this priority**: This is a new feature that doesn't exist in the legacy system. It's lower priority because it's not replacing existing functionality but adding new capability. However, it validates that the module architecture supports new features, not just reimplementation.

**Independent Test**: Can be fully tested by navigating to "Internal Routing" and completing the routing workflow (Label Entry → Add to Queue → Print Labels → Save to History). Delivers new internal routing label functionality independently.

**Acceptance Scenarios**:

1. **Given** routing label entry, **When** clerk selects recipient from Deliver To dropdown, **Then** system auto-fills Department if recipient has a default department
2. **Given** PO number entry, **When** clerk enters "63150", **Then** system auto-formats to "PO-063150" for label display
3. **Given** label queue with entries, **When** clerk clicks "Duplicate Row", **Then** system copies all fields to new row and increments label number
4. **Given** completed label entries, **When** clerk clicks "Print Labels", **Then** labels are exported to CSV for LabelView printing (template: "Expo - Mini UPS Label ver. 1.0")
5. **Given** printed labels, **When** clerk clicks "Save to History", **Then** validated labels are transferred to history table and current entries are cleared
6. **Given** history view, **When** clerk views daily history, **Then** labels are grouped by date with alternating row colors

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide Label Entry screen with data grid for multiple labels
- **FR-002**: System MUST auto-fill Department when recipient is selected (if default exists)
- **FR-003**: System MUST auto-format PO numbers ("63150" → "PO-063150")
- **FR-004**: System MUST support Duplicate Row functionality (copy all fields, increment label number)
- **FR-005**: System MUST auto-increment label numbers per day (resets daily)
- **FR-006**: System MUST export CSV files compatible with LabelView 2022 (template: "Expo - Mini UPS Label ver. 1.0")
- **FR-007**: System MUST save labels to history table after printing
- **FR-008**: System MUST provide History view with date-based grouping and alternating row colors
- **FR-009**: System MUST track employee number for each label
- **FR-010**: System MUST support recipient lookup and management

### Key Entities

- **Routing Label**: Label record with Deliver To, Department, PO Number, Work Order, Employee Number, Label Number, Date
- **Routing Recipient**: Recipient lookup with default department
- **Routing History**: Archived labels grouped by date

## Success Criteria *(mandurable)*

### Measurable Outcomes

- **SC-001**: Users can create and print routing labels in <3 minutes
- **SC-002**: CSV files compatible with LabelView 2022 (labels print correctly)
- **SC-003**: Label numbers auto-increment correctly per day
- **SC-004**: History view displays labels grouped by date with alternating colors
- **SC-005**: Recipient lookup auto-fills department correctly

## Assumptions *(optional)*

- Users have LabelView 2022 installed and configured
- MySQL database is accessible and stored procedures are deployed
- Authentication provides employee number from context
- Recipients are managed by admin users

## Dependencies *(optional)*

- **Phase 1 Infrastructure**: Complete (MVVM architecture, DI, error handling, logging)
- **Database Tables**: routing_labels, routing_recipients (see data-model.md)
- **Stored Procedures**: sp_routing_label_*, sp_routing_recipient_* (see data-model.md)
- **External Systems**: 
  - LabelView 2022 - CSV import for label printing
- **Other Features**: 
  - BaseViewModel, BaseWindow (for MVVM pattern)
  - Shared End-of-Day Reporting Module (for integrated reporting)

## Out of Scope *(optional)*

- Direct integration with shipping systems (manual label printing)
- Automatic LabelView triggering (user manually imports CSV)
- Mobile app version (Windows desktop only)
- Multi-language support (English only)

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: Views (XAML), ViewModels (business logic), Models (data), Services (operations) clearly separated
- [x] **Database Layer**: All operations via stored procedures, return Model_Dao_Result, no raw SQL
- [x] **Dependency Injection**: Services registered in App.xaml.cs (IService_Routing, IService_Routing_History, IService_Routing_RecipientLookup)
- [x] **Error Handling**: All exceptions handled via IService_ErrorHandler, user-facing errors shown in InfoBars/ContentDialogs
- [x] **Security & Authentication**: Employee number from authentication context (no additional auth required)
- [x] **WinUI 3 Practices**: x:Bind (compile-time), ObservableCollection, async/await, CommunityToolkit.Mvvm attributes ([ObservableProperty], [RelayCommand])
- [x] **Specification-Driven**: This spec follows Speckit workflow with user stories, requirements, success criteria, and templates

---

**Reference**: See [../011-module-reimplementation/spec.md](../011-module-reimplementation/spec.md) - User Story 4 for complete context


# Feature Specification: End-of-Day Reporting Module

**Feature Branch**: `[016-reporting-module]`  
**Created**: 2026-01-03  
**Status**: Addition to Original Spec  
**Input**: User description: "Analysis of the Google Sheets-based routing label system revealed a sophisticated End-of-Day (EoD) Reporting feature that filters, normalizes, and exports data for email communication. This functionality should be implemented as a fourth module that works across all three primary modules (Receiving, Dunnage, Routing)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate End-of-Day Reports (Priority: P1)

As a supervisor, I want to generate end-of-day email reports for Receiving, Dunnage, and Routing activities so that I can communicate daily work summary to stakeholders.

**Why this priority**: This is the core functionality for the reporting module, enabling supervisors to share daily summaries.

**Independent Test**: Can be fully tested by selecting date ranges and modules, generating reports, and verifying output formats.

**Acceptance Scenarios**:

1. **Given** EoD Reports screen, **When** supervisor selects "Receiving" module and date range (01/02/2026 to 01/03/2026), **Then** system displays filtered receiving history with normalized PO numbers
2. **Given** filtered Receiving data, **When** supervisor clicks "Export to CSV", **Then** system generates CSV file with columns: PO Number, Part, Description, Qty, Weight, Heat/Lot, Date
3. **Given** filtered Routing data, **When** supervisor clicks "Copy for Email", **Then** system formats data as table with alternating row colors grouped by date
4. **Given** Dunnage history, **When** supervisor generates report, **Then** system includes Type, Part, Specs (concatenated), Quantity, Date
5. **Given** multiple date ranges tested, **When** supervisor switches modules, **Then** date range persists across module changes
6. **Given** PO validation, **When** system encounters formats like `"63150"`, `"063150B"`, `"Customer Supplied"`, **Then** output shows `"PO-063150"`, `"PO-063150B"`, `"Customer Supplied"` respectively

---

### User Story 2 - Routing Module Enhancements (Priority: P2)

As a user, I want enhanced routing features including auto-lookup, history tracking, and label numbering so that the system matches the Google Sheets workflow.

**Why this priority**: These enhancements improve usability and match existing workflows.

**Independent Test**: Can be tested by creating labels, archiving to history, and verifying auto-fills and numbering.

**Acceptance Scenarios**:

1. **Given** routing label entry, **When** user selects "Deliver To" person, **Then** department auto-fills from recipient lookup
2. **Given** routing labels, **When** user clicks "Archive to History", **Then** today's labels move to history and table clears
3. **Given** history view, **When** user views labels, **Then** labels are grouped by date with alternating colors
4. **Given** label entry, **When** user duplicates a row, **Then** label number auto-increments

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to select date ranges and modules for filtering history data
- **FR-002**: System MUST normalize PO numbers according to the specified algorithm (e.g., "63150" → "PO-063150")
- **FR-003**: System MUST export filtered data to CSV format for email attachments
- **FR-004**: System MUST format data as tables with date grouping and alternating colors for email body
- **FR-005**: System MUST support cross-module reporting for Receiving, Dunnage, and Routing data
- **FR-006**: System MUST auto-fill department when selecting recipients in routing
- **FR-007**: System MUST archive today's routing labels to history with confirmation
- **FR-008**: System MUST auto-increment label numbers and support row duplication in routing

### Key Entities *(include if feature involves data)*

- **ReportDateRange**: Represents date range selection with start/end dates and selected module
- **ReportRow**: Generic model for export data with fields dictionary, date, and group key
- **Routing_Recipient**: Represents recipients with name, default department, and active status
- **Routing_Label**: Represents labels with auto-incrementing numbers, deliver to, department, etc.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can generate reports for all three modules within specified date ranges
- **SC-002**: PO normalization handles all specified formats correctly (100% accuracy on test cases)
- **SC-003**: CSV export includes correct columns and data for each module
- **SC-004**: Email formatting applies date grouping and alternating colors as specified
- **SC-005**: Routing auto-lookup fills department for 100% of recipients with defaults
- **SC-006**: History archival moves all today's labels without data loss
- **SC-007**: Label numbering increments correctly on duplication

## Assumptions *(optional)*

- All three primary modules (Receiving, Dunnage, Routing) are implemented and storing data correctly
- Database views can be created for cross-module querying
- Users have appropriate permissions to view history data across modules

## Dependencies *(optional)*

- **Phase 1 Infrastructure**: Complete (database, DI, MVVM setup)
- **Database Tables**: routing_recipients, routing_labels, views for history
- **Stored Procedures**: For data retrieval and archival operations
- **Other Features**: Receiving, Dunnage, and Routing modules must be functional

## Out of Scope *(optional)*

- Real-time email sending (only data formatting for copy/paste)
- Advanced filtering beyond date range and module selection
- Integration with external email systems

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: UI requirements separate presentation from logic with ViewModels and Services
- [x] **Database Layer**: Data persistence specifies entities without implementation details
- [x] **Dependency Injection**: Service dependencies identified for DI registration
- [x] **Error Handling**: Error scenarios documented in acceptance tests
- [x] **Security & Authentication**: No additional auth requirements
- [x] **WinUI 3 Practices**: UI requirements appropriate for WinUI 3
- [x] **Specification-Driven**: Spec is technology-agnostic and user-focused

### Special Constraints

- [x] **Infor Visual Integration**: No Infor Visual queries required
- [x] **MySQL 5.7.24 Compatibility**: Uses basic SQL features compatible with MySQL 5.7
- [x] **Async Operations**: All data operations noted for async implementation

### Notes
This feature adds a fourth cross-cutting module while maintaining constitutional alignment. PO normalization ensures data consistency across modules.

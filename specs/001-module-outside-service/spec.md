# Feature Specification: Module_OutsideService

**Feature Branch**: `001-module-outside-service`
**Created**: 2026-03-27
**Last Updated**: 2026-03-29
**Status**: Draft

## Scope Summary

Module_OutsideService is a new top-level module for outside-service requests.
Outside-service requests are created by the Outside Service Coordinator and then worked by Shipping.
All three lifecycle phases - `Initialize`, `Setup`, and `Complete` - are delivered in one release.
Each waitlist line progresses independently through `Initialize -> Setup -> Complete`.

**Per-Package Quantity Model**

A line may contain one or more physical packages.
Each package carries its own quantity value.
Package quantities on the same line can all be different.
For example, Part A123 with four packages may have quantities 10, 15, 8, and 20 - each value belonging to exactly one package.
The system must store, preserve, and display individual package quantities without flattening, averaging, or assuming equality across packages.

**Role Boundaries**

- `Initialize` is owned by the Outside Service Coordinator.
- `Setup` and `Complete` are owned by Shipping.
- Vendor selection is a Shipping responsibility and belongs only in the `Setup` phase.

**Data Ownership**

MySQL is the system of record for all request, line, and package data.
Infor Visual is read-only and used only for part validation, Part Match Helper suggestions, and vendor history lookups.
No data is written back to Infor Visual.

## User Scenarios And Testing

### User Story 1 - Create An Outside Service Request

**Priority**: P1

An Outside Service Coordinator needs to create a request for one or more parts that must be sent to an outside-service vendor.

The coordinator starts a new request on the request-entry screen, then opens the Add Line modal to enter each line.
Inside the Add Line modal the coordinator enters the Part ID, the number of packages, and then one quantity for each individual package.
If the entered Part ID does not exactly match a part in Infor Visual, the system launches the Part Match Helper from inside the modal.
The Part Match Helper returns the chosen Part ID back into the modal so the rest of the line data is preserved.

**Independent Test**: Open the module, start a request, open the Add Line modal, enter a valid Part ID, enter four packages with four different quantities, save the line, repeat with a second line using a mismatched part resolved through the Part Match Helper, save the request, and confirm every saved line appears in the active waitlist with phase `Initialize` and every package quantity is stored exactly as entered.

**Acceptance Scenarios**:

1. **Given** the coordinator opens Module_OutsideService, **When** they create a valid request with at least one line and save it, **Then** the system stores the request in MySQL and each line appears in the waitlist with phase `Initialize`.
2. **Given** a request line has four packages with four different quantities, **When** the coordinator saves the line, **Then** the system preserves all four package quantities exactly as entered without modification.
3. **Given** the entered Part ID does not exactly match any part in Infor Visual, **When** the coordinator leaves the Part ID field in the Add Line modal, **Then** the system opens the Part Match Helper offering similar parts in plain language.
4. **Given** the Part Match Helper offers the correct part, **When** the coordinator selects it, **Then** the corrected Part ID is returned to the Add Line modal and all other field values already entered are preserved.
5. **Given** two packages on the same line have different quantities, **When** the line is saved and reloaded, **Then** each package displays its own original quantity value in the correct package row.

---

### User Story 2 - Set Up A Waitlist Line

**Priority**: P1

Shipping needs to open a line in `Initialize`, select or enter a vendor, record BOL and shipment details, and move the line to `Setup`.

**Independent Test**: Open an `Initialize` line from the active waitlist, choose a vendor suggestion, enter BOL information, save, and confirm the line now shows phase `Setup`.

**Acceptance Scenarios**:

1. **Given** a line is in `Initialize`, **When** Shipping completes required setup fields, **Then** the line moves to `Setup`.
2. **Given** prior outside-service history exists for the part, **When** Shipping opens the setup screen, **Then** the system shows vendor suggestions from that history.
3. **Given** vendor suggestions are not usable, **When** Shipping enters a custom vendor name, **Then** the system saves the custom value with the line.
4. **Given** a line moves to `Setup`, **When** any user views the active waitlist, **Then** the line appears in the `Setup` group with vendor and BOL detail visible.

---

### User Story 3 - Complete A Waitlist Line

**Priority**: P1

Shipping needs to mark a `Setup` line complete once the shipment has left the facility and be able to view that line in the completed history.

**Independent Test**: Open a `Setup` line, confirm completion, save, and verify the line moves to the completed-history view with vendor and BOL details intact.

**Acceptance Scenarios**:

1. **Given** a line is in `Setup`, **When** Shipping confirms the shipment is complete, **Then** the line moves to `Complete` and leaves the active waitlist.
2. **Given** lines are in `Complete`, **When** a user opens the completed-history view, **Then** the lines appear with part, vendor, BOL, package count, and completion timestamp.
3. **Given** a line is completed, **When** the user opens its detail in the history view, **Then** the individual package quantities remain visible.

## Edge Cases

- **Unequal packages**: Part A123 with four packages may have quantities 10, 15, 8, and 20. Each is stored individually. The system must not assume the same quantity repeats.
- **Package count mismatch**: The number of quantity rows must equal the package count before save is allowed. Extra or missing rows block save.
- **Zero or invalid quantity**: A package quantity of zero, negative, blank, or non-numeric blocks save and highlights the invalid row.
- **Mismatched Part ID with no suggestions**: If the Part Match Helper has no matches to offer, the user must correct the Part ID manually before save.
- **No vendor suggestions for a part**: Shipping can still continue by entering a custom vendor value.
- **Same part on multiple lines**: Each line is stored independently. The system does not merge them.
- **Lines in different phases from the same request**: Phase is tracked per line. The system does not force all lines in a request to share a phase.
- **Empty waitlist**: The module displays an empty-state message instead of a blank grid.
- **Inactive part in Infor Visual**: Handling of inactive-but-valid parts is an open follow-up item.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST provide a new top-level navigation destination for Module_OutsideService.
- **FR-002**: Version 1 MUST allow the coordinator to create a new outside-service request from a request-entry screen.
- **FR-003**: The request-entry screen MUST show the lines already added to the current request before save.
- **FR-004**: New request lines MUST be added through an Add Line modal launched from the request-entry screen.
- **FR-005**: The system MUST allow a request to contain one or more part lines.
- **FR-006**: Each request line MUST capture Part ID and Package Count in the Add Line modal.
- **FR-007**: Each request line MUST store one quantity value per physical package so that a four-package line stores four distinct quantity values.
- **FR-008**: The system MUST allow all package quantities on the same line to be different from one another.
- **FR-009**: The system MUST require the number of quantity entries to equal the Package Count before save is allowed.
- **FR-010**: The system MUST require all package quantities to be positive numeric values before save is allowed.
- **FR-011**: The Add Line modal MUST validate the entered Part ID against Infor Visual before allowing the line to be saved.
- **FR-012**: When an entered Part ID does not exactly match Infor Visual, the system MUST open the Part Match Helper from inside the Add Line modal.
- **FR-013**: The Part Match Helper MUST offer a list of similar part options in plain language.
- **FR-014**: When the user selects a suggested part in the Part Match Helper, the selected Part ID MUST be returned to the Add Line modal and all other field values already entered MUST be preserved.
- **FR-015**: The system MUST persist all request, line, and package data in MySQL.
- **FR-016**: The system MUST NOT write any data to Infor Visual.
- **FR-017**: Each newly saved waitlist line MUST begin in phase `Initialize`.
- **FR-018**: The system MUST implement the line-level lifecycle `Initialize -> Setup -> Complete`.
- **FR-019**: The system MUST display an active waitlist showing open lines in `Initialize` and `Setup`.
- **FR-020**: The active waitlist MUST show the current phase for each line.
- **FR-021**: Vendor selection MUST be available only in the `Setup` phase.
- **FR-022**: The `Setup` screen MUST show vendor suggestions from prior outside-service history for the part when that history exists.
- **FR-023**: The `Setup` screen MUST allow Shipping to enter a custom vendor when suggestions are not usable.
- **FR-024**: The `Setup` screen MUST capture BOL number, scheduled ship information, and shipping contact.
- **FR-025**: The `Complete` phase MUST move the line out of the active waitlist and into the completed-history view.
- **FR-026**: The completed-history view MUST display completed lines with part ID, vendor, BOL, package count, and completion timestamp.
- **FR-027**: The completed-history view MUST allow the user to open a completed line and see the individual package quantities.
- **FR-028**: The system MUST record request-level metadata including created-by username and created timestamp.
- **FR-029**: The implementation MUST follow the project MVVM flow: View -> ViewModel -> Service -> DAO -> Database.
- **FR-030**: All MySQL writes MUST use stored procedures through the project-standard DAO pattern.
- **FR-031**: All Infor Visual access MUST use a read-only connection (`ApplicationIntent=ReadOnly`).

### Non-Goals For Version 1

- No cancellation or void workflow.
- No post-save editing of a submitted request.
- No write-back to Infor Visual.
- No bulk import of request lines.
- No print or export for active waitlist lines.

## Key Entities

- **OutsideServiceRequest**: Header record storing request number, created-by user, created timestamp, and request-level notes.
- **OutsideServiceRequestLine**: Child line storing Part ID, package count, current phase, and all Shipping-owned setup and completion fields.
- **OutsideServiceRequestPackage**: Child package record storing package sequence number and that package's individual quantity. A four-package line has exactly four package records with potentially four different quantity values.
- **OutsideServicePartMatchSuggestion**: Read-only suggestion from Infor Visual when the entered Part ID is close but not exact.
- **OutsideServiceVendorSuggestion**: Read-only vendor name from prior outside-service history in Infor Visual for the selected Part ID.
- **Enum_OutsideServiceLinePhase**: `Initialize`, `Setup`, `Complete`.

## Proposed Version 1 Data Model

### MySQL Application Tables

| Table                             | Purpose              | Key Columns                                                                                                                                                                                                                                                                      |
| --------------------------------- | -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `outside_service_request`         | Request header       | `outside_service_request_id`, `request_number`, `created_by_user`, `created_utc`, `request_notes`                                                                                                                                                                                |
| `outside_service_request_line`    | Request line         | `outside_service_request_line_id`, `outside_service_request_id`, `line_number`, `part_id`, `package_count`, `line_phase`, `setup_vendor_name`, `setup_vendor_source`, `bol_number`, `scheduled_ship_utc`, `shipping_contact`, `setup_notes`, `completed_utc`, `completion_notes` |
| `outside_service_request_package` | Per-package quantity | `outside_service_request_package_id`, `outside_service_request_line_id`, `package_sequence`, `package_quantity`                                                                                                                                                                  |

**Per-package quantity rule**: `outside_service_request_package` stores exactly one row per physical package.
A line with `package_count = 4` must have exactly four rows, each with its own `package_quantity`.
The application validates this before save and reloads all rows when displaying the line.

### Proposed Stored Procedures

| Stored Procedure                                | Purpose                                                             |
| ----------------------------------------------- | ------------------------------------------------------------------- |
| `sp_outside_service_request_insert`             | Save a new request header                                           |
| `sp_outside_service_request_line_insert`        | Save a new request line                                             |
| `sp_outside_service_request_package_insert`     | Save one package quantity entry for a line                          |
| `sp_outside_service_request_get_open`           | Return active waitlist lines with header context                    |
| `sp_outside_service_request_get_by_id`          | Return a request with its lines and all per-package quantity rows   |
| `sp_outside_service_request_line_update_setup`  | Save vendor and shipment setup fields and move the line to `Setup`  |
| `sp_outside_service_request_line_mark_complete` | Save completion data and move the line to `Complete`                |
| `sp_outside_service_request_get_completed`      | Return completed-history lines with line and package detail         |

## Proposed Module Architecture

### Folder Structure

```text
Module_OutsideService/
  Contracts/
    IService_OutsideService.cs
  Data/
    Dao_OutsideServiceRequest.cs
  Models/
    Model_OutsideServiceRequest.cs
    Model_OutsideServiceRequestLine.cs
    Model_OutsideServiceRequestPackage.cs
    Model_OutsideServicePartMatchSuggestion.cs
    Model_OutsideServiceVendorSuggestion.cs
    Enum_OutsideServiceLinePhase.cs
  Services/
    Service_OutsideService.cs
  ViewModels/
    ViewModel_OutsideService_Main.cs
    ViewModel_OutsideService_RequestEntry.cs
    ViewModel_OutsideService_AddLineModal.cs
    ViewModel_OutsideService_Waitlist.cs
    ViewModel_OutsideService_Setup.cs
    ViewModel_OutsideService_CompleteHistory.cs
  Views/
    View_OutsideService_Main.xaml
    View_OutsideService_RequestEntry.xaml
    View_OutsideService_AddLineModal.xaml
    View_OutsideService_Waitlist.xaml
    View_OutsideService_Setup.xaml
    View_OutsideService_CompleteHistory.xaml
```

### Integration Points

| Area                 | Planned Change                                                                                                           |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| Navigation           | Add a new top-level menu entry in `MainWindow.xaml` and a route entry in `MainWindow.xaml.cs`                            |
| Dependency Injection | Add an `AddOutsideServiceModule` registration method in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` |
| MySQL                | Add module-specific tables and stored procedures                                                                         |
| Infor Visual         | Read-only: part validation, Part Match Helper suggestions, and vendor suggestion lookups                                 |
| Documentation        | Spec and end-user workflow remain under `docs/Modules/Module_OutsideService/`                                            |

## Line Phase Notes

### Initialize Phase

The coordinator captures on the request-entry screen and inside the Add Line modal:

- Part ID with Infor Visual validation.
- Package Count - the number of physical packages for the line.
- One quantity per package. Each row in the package-quantity table is one physical package; rows on the same line can hold different values.
- Part Match Helper recovery when the Part ID does not match, launched from the modal and returned back into it.
- Request-level notes.

The coordinator does not enter vendor information at this stage.

### Setup Phase

Shipping captures:

- Vendor name from suggestions or custom entry.
- Vendor source indicator.
- BOL number.
- Scheduled ship date or timestamp.
- Shipping contact.
- Setup notes.

The package-quantity breakdown from `Initialize` is read-only in this phase.

### Complete Phase

Shipping captures:

- Completion timestamp.
- Completion notes.

On save the line moves to the completed-history view.
All setup and package details are retained for audit.

## Success Criteria

- **SC-001**: A coordinator can create a request with at least one line in under two minutes using the request-entry screen and Add Line modal without leaving the module.
- **SC-002**: A line with four packages having four different quantities saves, reloads, and displays all four distinct values correctly without loss or modification.
- **SC-003**: When a Part ID does not exactly match Infor Visual the user can recover through the Part Match Helper and return to the Add Line modal without losing already-entered field values.
- **SC-004**: Every newly saved line appears in the active waitlist with phase `Initialize`.
- **SC-005**: Shipping can move a line from `Initialize` to `Setup` and then to `Complete` without leaving the module area.
- **SC-006**: The spec and end-user workflow document agree on per-package quantity behavior, modal-based line entry, Part Match Helper placement, and the lifecycle with no conflicting wording.

## Open Follow-Up Items

- Confirm whether line-level notes are required in addition to request-level notes.
- Confirm whether Shipping should be allowed to edit package quantities after a line reaches `Setup`.
- Confirm whether request numbers should be sequential human-readable values or GUID-backed display tokens.
- Confirm whether vendor suggestions should be ranked by most-recent use, most-frequent use, or a combined rule.
- Confirm behavior when the Part ID is valid but marked inactive in Infor Visual.

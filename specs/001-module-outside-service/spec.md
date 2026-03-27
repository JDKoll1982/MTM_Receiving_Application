# Feature Specification: Module_OutsideService

**Feature Branch**: `001-module-outside-service`  
**Created**: 2026-03-27  
**Last Updated**: 2026-03-27  
**Status**: Draft  
**Input**: User description: "Create a new Module_OutsideService waitlist for the Outside Service Coordinator to add requests for Shipping to schedule parts with a vendor. Initial Entry sets Part ID(s), number of packages, and quantity per package. Vendor selection belongs to Shipping. Each waitlist line must move through Initialize, Setup, and Complete. Part-number mismatch handling must use a user-friendly part-match helper. Package quantities can differ per package. Create a full specification document and an end-user workflow document with mockup UI elements."

## Scope Summary

Module_OutsideService is a new top-level module for outside-service requests that are created by the Outside Service Coordinator and then worked by Shipping.
Version 1 delivers the complete line lifecycle in one release.
Each waitlist line progresses through `Initialize`, `Setup`, and `Complete`.
`Initialize` is coordinator-owned.
`Setup` and `Complete` are Shipping-owned.

The request-entry flow must support one or more part lines per request.
Each part line must support one or more physical packages.
Each package may have its own quantity.
The system must not assume a single repeated quantity per package.

## User Scenarios And Testing

### User Story 1 - Create An Outside Service Request (Priority: P1)

An Outside Service Coordinator needs to create a request for one or more parts that must be sent to an outside-service vendor.
The coordinator enters the part ID, the package count, and the quantity for each physical package on the line.
If the entered part does not exactly match Infor Visual, the system helps the user find the correct part through a plain-language Part Match Helper.

**Why this priority**: Without request entry there is no waitlist for Shipping to act on.

**Independent Test**: Open the module, enter a request with one or more lines, give each line valid package quantities, resolve a mismatched part through the Part Match Helper when needed, save the request, and confirm each saved line appears in the active waitlist with phase `Initialize`.

**Acceptance Scenarios**:

1. **Given** the coordinator opens Module_OutsideService, **When** they enter a valid request with at least one line and save it, **Then** the system stores the request in MySQL and shows each saved line in the waitlist with phase `Initialize`.
2. **Given** a request line has four packages with four different quantities, **When** the coordinator saves the request, **Then** the system preserves all four package quantities exactly as entered.
3. **Given** the entered part number does not exactly match a part in Infor Visual, **When** the coordinator leaves the part field or tries to continue, **Then** the system opens a Part Match Helper that offers similar parts in user-friendly language.
4. **Given** the Part Match Helper offers a correct part, **When** the coordinator selects it, **Then** the corrected part is applied to the request line without re-entering the rest of the line.

---

### User Story 2 - Set Up A Waitlist Line (Priority: P1)

Shipping needs to open a line that is waiting in `Initialize`, choose or enter the vendor, record BOL and shipping details, and move the line into `Setup`.

**Why this priority**: Vendor selection and shipment setup are part of the delivered lifecycle, not a later enhancement.

**Independent Test**: Open a line from the active waitlist, set vendor and BOL information, save, and confirm the line moves from `Initialize` to `Setup`.

**Acceptance Scenarios**:

1. **Given** a line is in `Initialize`, **When** Shipping opens the line and completes the required setup data, **Then** the line moves to `Setup`.
2. **Given** prior outside-service history exists for the selected part, **When** Shipping opens the setup screen, **Then** the system offers vendor suggestions based on that part history.
3. **Given** the vendor suggestions are not usable, **When** Shipping enters a custom vendor, **Then** the system accepts the custom value and saves it with the line.

---

### User Story 3 - Complete A Waitlist Line (Priority: P1)

Shipping needs to finish the line lifecycle by marking the line `Complete` once the shipment has left the facility and then viewing the completed line in history.

**Why this priority**: The user explicitly requested that all three phases be delivered in one go.

**Independent Test**: Open a line that is already in `Setup`, mark it complete, and confirm it is shown in the completed-history view with its vendor and BOL details.

**Acceptance Scenarios**:

1. **Given** a line is in `Setup`, **When** Shipping confirms the shipment is complete, **Then** the line moves to `Complete`.
2. **Given** one or more lines are in `Complete`, **When** a user opens the completed-history view, **Then** the system shows those lines with enough detail for follow-up and audit review.

## Edge Cases

- What happens when a request contains multiple lines for the same part ID? The system preserves the lines as entered unless future consolidation rules are intentionally added.
- What happens when the coordinator enters a package count that does not match the number of package-quantity entries? The line cannot be saved until the counts align.
- What happens when a package quantity is zero, negative, blank, or non-numeric? The affected line cannot be saved and the invalid package entry must be highlighted.
- What happens when the part number does not match anything in Infor Visual and the Part Match Helper has no usable suggestions? The user must correct the part manually before the request can be saved.
- What happens when there are no vendor suggestions for a part in `Setup`? Shipping can still continue by entering a custom vendor.
- What happens when different lines in the same request are in different phases? The system treats phase as a line-level field and does not force the whole request into one status.
- What happens when the waitlist has no active lines? The module shows an empty-state message instead of a blank grid.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST provide a new top-level navigation destination named `Module_OutsideService`.
- **FR-002**: Version 1 MUST allow the Outside Service Coordinator to create a new outside-service request.
- **FR-003**: The system MUST allow a request to contain one or more part lines.
- **FR-004**: Each request line MUST capture `Part ID` and `Package Count`.
- **FR-005**: Each request line MUST store one quantity value for each physical package on that line.
- **FR-006**: The system MUST allow package quantities on the same line to be different from one another.
- **FR-007**: The system MUST require the number of package-quantity entries to equal the package count before save.
- **FR-008**: The system MUST require all package quantities to be positive numeric values.
- **FR-009**: Version 1 MUST validate entered part IDs against the Infor Visual database before a request can be saved.
- **FR-010**: When an entered part ID does not exactly match Infor Visual, Version 1 MUST open a user-facing Part Match Helper that shows similar part options.
- **FR-011**: The Part Match Helper MUST let the coordinator apply a suggested part or return to manual correction.
- **FR-012**: The system MUST persist Version 1 outside-service requests in MySQL and MUST NOT use Infor Visual as the write store for the feature.
- **FR-013**: Each newly created waitlist line MUST begin in phase `Initialize`.
- **FR-014**: The system MUST use the line-level lifecycle `Initialize -> Setup -> Complete` for each waitlist line.
- **FR-015**: The system MUST display an active waitlist view of open lines stored by Module_OutsideService.
- **FR-016**: The `Initialize` phase MUST allow requests to be saved without vendor selection because vendor selection is owned by Shipping.
- **FR-017**: The `Setup` phase MUST allow Shipping to choose a vendor suggestion or enter a custom vendor.
- **FR-018**: The `Setup` phase MUST show vendor suggestions derived from prior outside-service history for the selected part when that history exists.
- **FR-019**: The `Setup` phase MUST support BOL number and shipment setup fields stored outside Infor Visual.
- **FR-020**: The `Complete` phase MUST support moving a line into a completed-history view.
- **FR-021**: The active waitlist MUST show the current phase at the line level.
- **FR-022**: The completed-history view MUST show completed lines with enough detail for follow-up and auditing.
- **FR-023**: The system MUST record request-level creation metadata including who created the request and when it was created.
- **FR-024**: The implementation MUST follow the project MVVM flow `View -> ViewModel -> Service -> DAO -> Database`.
- **FR-025**: Any MySQL write access for this module MUST use the project-standard DAO and stored-procedure patterns.
- **FR-026**: The module MUST keep Infor Visual access read-only and use it only for part validation, part-match suggestions, vendor suggestions, and history-derived reference data.

### Non-Goals For Version 1

- No cancellation or void flow.
- No post-save editing workflow for already-submitted requests.
- No write-back to Infor Visual.

## Key Entities

- **OutsideServiceRequest**: Request header created by the Outside Service Coordinator. Stores request number, created-by metadata, created timestamp, and request-level notes.
- **OutsideServiceRequestLine**: Child line under a request. Stores part ID, package count, current phase, and Shipping-owned setup and completion fields.
- **OutsideServiceRequestPackage**: Child package row under a request line. Stores package sequence and package quantity so each package can carry its own quantity.
- **OutsideServicePartMatchSuggestion**: Read-only suggestion shown when the entered part is close to a real part but not an exact match.
- **OutsideServiceVendorSuggestion**: Read-only vendor suggestion derived from prior outside-service history for the selected part.
- **OutsideServiceLinePhase**: Line-level progress model. Allowed values are `Initialize`, `Setup`, and `Complete`.

## Proposed Version 1 Data Model

### Application Tables

| Table | Purpose | Key Columns |
| ----- | ------- | ----------- |
| `outside_service_request` | Request header table | `outside_service_request_id`, `request_number`, `created_by_user`, `created_utc`, `request_notes` |
| `outside_service_request_line` | Request line table | `outside_service_request_line_id`, `outside_service_request_id`, `line_number`, `part_id`, `package_count`, `line_phase`, `setup_vendor_name`, `setup_vendor_source`, `bol_number`, `scheduled_ship_utc`, `shipping_contact`, `setup_notes`, `completed_utc`, `completion_notes` |
| `outside_service_request_package` | Per-package quantity table | `outside_service_request_package_id`, `outside_service_request_line_id`, `package_sequence`, `package_quantity` |

### Proposed Stored Procedures

| Stored Procedure | Purpose |
| ---------------- | ------- |
| `sp_outside_service_request_insert` | Save a new request header |
| `sp_outside_service_request_line_insert` | Save a request line |
| `sp_outside_service_request_package_insert` | Save one package quantity entry for a line |
| `sp_outside_service_request_get_open` | Return active waitlist lines and header summary |
| `sp_outside_service_request_get_by_id` | Return a request with its lines and package rows |
| `sp_outside_service_request_line_update_setup` | Save vendor and shipment setup data and move a line to `Setup` |
| `sp_outside_service_request_line_mark_complete` | Save completion data and move a line to `Complete` |
| `sp_outside_service_request_get_completed` | Return completed-history lines |

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
    ViewModel_OutsideService_Waitlist.cs
    ViewModel_OutsideService_Setup.cs
    ViewModel_OutsideService_CompleteHistory.cs
  Views/
    View_OutsideService_Main.xaml
    View_OutsideService_RequestEntry.xaml
    View_OutsideService_Waitlist.xaml
    View_OutsideService_Setup.xaml
    View_OutsideService_CompleteHistory.xaml
```

### Integration Points

| Area | Planned Change |
| ---- | -------------- |
| Navigation | Add a new top-level menu entry in `MainWindow.xaml` and a route entry in `MainWindow.xaml.cs` |
| Dependency Injection | Add an `AddOutsideServiceModule` registration method in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` |
| MySQL | Add module-specific tables and stored procedures for request, line, and package persistence |
| Infor Visual | Use read-only part validation, Part Match Helper suggestions, and vendor suggestion lookups |
| Documentation | Keep the spec and end-user workflow under the current module documentation structure |

## Line Phase Notes

### Initialize

The `Initialize` phase captures:

- part ID
- package count
- one quantity for each package
- request-level notes
- line-level notes if the line needs extra context

### Setup

The `Setup` phase captures:

- selected vendor
- vendor source or custom-vendor indicator
- BOL number
- scheduled ship date or scheduled ship timestamp
- shipping contact or handoff owner
- Shipping notes

### Complete

The `Complete` phase captures:

- completed timestamp
- completion notes
- final vendor and BOL context for history review
- movement from active waitlist to completed-history presentation

## Success Criteria

- **SC-001**: A coordinator can create a valid outside-service request with at least one part line in under 2 minutes without leaving the module.
- **SC-002**: A line with multiple packages and different package quantities saves and reloads without losing or flattening any package quantity values.
- **SC-003**: When a part ID does not exactly match Infor Visual, the user can recover through the Part Match Helper without abandoning the request.
- **SC-004**: 100% of newly saved lines appear in the active waitlist with phase `Initialize`.
- **SC-005**: Shipping can move a line from `Initialize` to `Setup` and then to `Complete` without leaving the module area.
- **SC-006**: The specification and end-user workflow document agree on the delivered line-level lifecycle and package-detail behavior with no conflicting wording.

## Open Follow-Up Items

- Confirm whether line-level notes are required in addition to request-level notes.
- Confirm whether Shipping should be allowed to edit package details after a line reaches `Setup`.
- Confirm whether request numbers should be sequential human-readable values or GUID-backed display tokens.
- Confirm whether vendor suggestions should be ranked by most recent use, most frequent use, or a combined rule.

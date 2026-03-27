# Feature Specification: Module_OutsideService

**Feature Branch**: `001-module-outside-service`  
**Created**: 2026-03-27  
**Status**: Draft  
**Input**: User description: "Create a new Module_OutsideService waitlist for the Outside Service Coordinator to add requests for Shipping to schedule parts with a vendor. Initial entry captures Part ID, package count, and quantity per package. Shipment Scheduled and Shipped are part of the long-term lifecycle. Vendor can be suggested from prior outside-service history by part or entered manually. Create a full specification document and an end-user workflow document with mockup UI elements."

## Scope Summary

Module_OutsideService is a new top-level module for coordinator-managed outside-service requests.
The first delivery includes the full waitlist-line lifecycle.
Each waitlist line moves through three phases: **Initialize -> Setup -> Complete**.
`Initialize` is owned by the Outside Service Coordinator, while `Setup` and `Complete` are owned by Shipping.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create An Outside Service Request (Priority: P1)

An Outside Service Coordinator needs to enter a new request for one or more part IDs that must be sent out for external work. The coordinator records the part ID, the number of packages for each part, and the quantity per package so Shipping can later schedule the outbound shipment. If the entered part number does not match Infor Visual, the system helps the user find the right part by showing close matches in a Part Match Helper.

**Why this priority**: This is the core business action that creates value. Without request entry, there is no waitlist for Shipping to act on.

**Independent Test**: Can be fully tested by opening the module, entering a request with one or more part lines, saving it, and confirming each saved line appears in the active waitlist with phase `Initialize`.

**Acceptance Scenarios**:

1. **Given** the coordinator opens Module_OutsideService, **When** they enter a valid request with at least one part line and save it, **Then** the system stores the request in MySQL and shows each line in the active waitlist with phase `Initialize`.
2. **Given** a coordinator enters a request without any vendor selection, **When** the request is saved, **Then** the system stores it as an open waitlist item for Shipping follow-up.
3. **Given** a request contains multiple part lines, **When** the coordinator saves the request, **Then** the system keeps the lines together under one request while allowing each line to progress independently later.
4. **Given** the entered part number does not exactly match a part in Infor Visual, **When** the coordinator leaves the part field or tries to save, **Then** the system opens a Part Match Helper showing similar parts so the coordinator can choose the correct one.

---

### User Story 2 - Review And Set Up Waitlist Lines (Priority: P2)

The Outside Service Coordinator and Shipping team need a single place to see open outside-service waitlist lines, understand each line's current phase, and move eligible lines from `Initialize` into `Setup`.

**Why this priority**: Once requests are created, the team needs immediate visibility into open work and must be able to complete setup tasks in the same release.

**Independent Test**: Can be tested by saving multiple requests, opening the waitlist, and verifying that saved lines appear with the correct counts and phase, then moving a line from `Initialize` to `Setup` with vendor and BOL information.

**Acceptance Scenarios**:

1. **Given** one or more requests exist, **When** a user opens the waitlist view, **Then** the system lists open lines in a consistent newest-first order with their current phase.
2. **Given** a Shipping user opens a line that is in `Initialize`, **When** they assign a vendor and complete the required BOL and shipment setup details, **Then** that line moves to phase `Setup`.

---

### User Story 3 - Complete Waitlist Lines And Move Them To History (Priority: P3)

The business needs Shipping to finish the process in the same delivery by marking each line `Complete` once it leaves the facility, and then showing completed lines in history.

**Why this priority**: The three-phase lifecycle is part of the feature definition, not a later enhancement.

**Independent Test**: Can be tested by moving a line from `Setup` to `Complete` and verifying the completed line appears in the completed-history view.

**Acceptance Scenarios**:

1. **Given** a line is already in `Setup`, **When** Shipping confirms the line has left the facility, **Then** the line moves to `Complete`.
2. **Given** one or more lines are `Complete`, **When** a user opens the completed-history view, **Then** the system shows those lines with vendor and BOL details for follow-up.

---

## Edge Cases

- What happens when the coordinator enters multiple lines for the same part ID in a single request? The system should preserve the lines exactly as entered unless future consolidation rules are explicitly defined.
- What happens when package count or quantity per package is zero or negative? The request must be blocked with a clear validation error.
- What happens when a part ID has no Infor Visual outside-service history? The Version 1 request should still save because vendor selection does not happen yet.
- What happens when the entered part number does not match anything in Infor Visual and the Part Match Helper has no useful result? The system must block save for that line and tell the user to correct the part number manually.
- What happens when the same part appears in prior history with multiple vendors? The `Setup` phase should show suggestions in a predictable order while still allowing manual override.
- What happens when Shipping opens the waitlist before any requests exist? The module should show an empty-state message rather than a blank grid.
- What happens when a request is partially complete but not saved? The module should keep unsaved data in-memory for the current session only unless future draft persistence is intentionally added.
- What happens when different lines under the same request are in different phases? The waitlist must show phase at the line level and must not force all lines in a request to share one phase.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a new top-level navigation destination named `Module_OutsideService`.
- **FR-002**: Version 1 MUST allow the Outside Service Coordinator to create a new outside-service request.
- **FR-003**: The system MUST allow a request to contain one or more part lines.
- **FR-004**: Each part line MUST capture `Part ID`, `Number Of Packages`, and `Quantity Per Package`.
- **FR-005**: The system MUST require at least one valid part line before a request can be saved.
- **FR-006**: The system MUST validate that package counts and quantities are positive numeric values.
- **FR-007**: Version 1 MUST validate entered part IDs against the Infor Visual database before a request can be saved.
- **FR-008**: When an entered part ID does not exactly match Infor Visual, Version 1 MUST open a user-facing Part Match Helper that shows similar part options.
- **FR-009**: The Part Match Helper MUST let the coordinator pick one of the suggested parts or return to manual correction.
- **FR-010**: The system MUST persist Version 1 outside-service requests in MySQL and MUST NOT use Infor Visual as the write store for the feature.
- **FR-011**: The system MUST assign a waitlist line phase of `Initialize` to newly created lines.
- **FR-012**: The system MUST use the line-level phase model `Initialize -> Setup -> Complete` for each waitlist line.
- **FR-013**: The system MUST display an active waitlist view of open requests and lines stored by Module_OutsideService.
- **FR-014**: The `Initialize` phase MUST allow requests to be saved without a vendor selection because vendor assignment is owned by Shipping.
- **FR-015**: The `Setup` phase MUST show vendor suggestions derived from prior outside-service history for the selected part ID when that history exists.
- **FR-016**: The `Setup` phase MUST allow a custom vendor value when the suggestion list is unsuitable or empty.
- **FR-017**: The system MUST separate the new active waitlist from the existing Ship/Rec Outside Service History lookup tool.
- **FR-018**: The system MUST record request-level audit fields for creation metadata, including who created the request and when it was created.
- **FR-019**: The `Setup` phase MUST support selected vendor, BOL number, and explicit shipment data stored outside Infor Visual.
- **FR-020**: The `Complete` phase MUST support moving completed lines into a history-oriented view or archive model.
- **FR-021**: The implementation MUST follow the project MVVM flow `View -> ViewModel -> Service -> DAO -> Database`.
- **FR-022**: Any MySQL write access for this module MUST use the project-standard DAO and stored-procedure patterns.
- **FR-023**: The module MUST keep Infor Visual access read-only and use it only for part validation, part-match suggestions, vendor suggestions, or other historical reference data.
- **FR-024**: The active waitlist MUST show the current phase at the line level.
- **FR-025**: The completed-history view MUST show completed lines with enough detail for follow-up and auditing.

### Non-Goals For Version 1

- No cancellation or void flow.
- No post-save editing workflow.
- No write-back to Infor Visual.

## Key Entities *(include if feature involves data)*

- **OutsideServiceRequest**: The request header created by the Outside Service Coordinator. Stores request number, created-by metadata, created timestamp, and summary information for the waitlist.
- **OutsideServiceRequestLine**: A child line belonging to an outside-service request. Stores part ID, package count, quantity per package, line ordering, current phase, and Shipping-owned setup/completion details.
- **OutsideServicePartMatchSuggestion**: A read-only list of likely Infor Visual parts shown when the entered part number does not exactly match a real part. This powers the user-facing Part Match Helper in Version 1.
- **OutsideServiceVendorSuggestion**: A read-only suggestion derived from prior Infor Visual outside-service history for a part ID. It is used during the `Setup` phase.
- **OutsideServiceLinePhase**: The business progress model for each waitlist line. The phases are `Initialize`, `Setup`, and `Complete`.

## Proposed Version 1 Data Model

### Application Tables

| Table | Purpose | Key Columns |
| ----- | ------- | ----------- |
| `outside_service_request` | Request header table for the active waitlist | `outside_service_request_id`, `request_number`, `created_by_user`, `created_utc`, `notes` |
| `outside_service_request_line` | Request detail lines | `outside_service_request_line_id`, `outside_service_request_id`, `line_number`, `part_id`, `package_count`, `quantity_per_package`, `line_phase`, `vendor_display_name`, `vendor_source`, `bol_number`, `setup_utc`, `completed_utc` |

### Proposed Stored Procedures

| Stored Procedure | Purpose |
| ---------------- | ------- |
| `sp_outside_service_request_insert` | Save a new request header |
| `sp_outside_service_request_line_insert` | Save a request line |
| `sp_outside_service_request_line_update_setup` | Move a line to `Setup` and save Shipping-owned setup data |
| `sp_outside_service_request_line_mark_complete` | Move a line to `Complete` |
| `sp_outside_service_request_get_open` | Return active waitlist requests and lines |
| `sp_outside_service_request_get_completed` | Return completed-history lines |
| `sp_outside_service_request_get_by_id` | Return a saved request and its lines |

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
| Navigation | Add a new top-level menu entry in `MainWindow.xaml` and route map entry in `MainWindow.xaml.cs` |
| Dependency Injection | Add an `AddOutsideServiceModule` registration method in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` |
| MySQL | Add module-specific tables and stored procedures for request persistence |
| Infor Visual | Use read-only part validation plus Part Match Helper behavior, and reuse outside-service history queries for Shipping-owned vendor suggestion behavior during `Setup` |
| Documentation | Add module documentation under `docs/Modules/Module_OutsideService` |

## Line Phase Notes

### Initialize

The `Initialize` phase captures:

- part ID
- number of packages
- quantity per package
- request-level notes

### Setup

The `Setup` phase captures:

- selected vendor
- BOL number
- scheduled ship date
- selected shipping contact or handoff owner
- confirmation that Shipping accepted the request

### Complete

The `Complete` phase captures:

- completed or shipped date
- final vendor confirmation details
- movement from active waitlist to history/archive presentation

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A coordinator can create a valid outside-service request with at least one line in under 2 minutes without leaving the module, including correcting an invalid part through the Part Match Helper when needed.
- **SC-002**: 100% of saved waitlist lines appear in the active waitlist immediately after save with phase `Initialize`.
- **SC-003**: Coordinators can save valid requests without being blocked by Shipping-owned vendor decisions.
- **SC-004**: Shipping can move a line from `Initialize` to `Setup` and then to `Complete` without leaving the module area.
- **SC-005**: The specification and end-user workflow document agree on the delivered line-phase model with no conflicting lifecycle definitions.

## Open Follow-Up Items

- Confirm whether only the Outside Service Coordinator can create requests, or whether Leads should also be allowed.
- Confirm how many part suggestions the Part Match Helper should show before asking the user to manually refine the entry.
- Confirm whether vendor suggestions for Shipping should be ranked by most recent use, most frequent use, or both.
- Confirm whether Version 1 needs per-request notes only, or both request-level and line-level notes.
- Confirm whether request numbers should be human-readable sequential values or GUID-backed display tokens.

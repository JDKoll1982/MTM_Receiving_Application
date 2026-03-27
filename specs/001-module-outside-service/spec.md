# Feature Specification: Module_OutsideService

**Feature Branch**: `001-module-outside-service`  
**Created**: 2026-03-27  
**Status**: Draft  
**Input**: User description: "Create a new Module_OutsideService waitlist for the Outside Service Coordinator to add requests for Shipping to schedule parts with a vendor. Initial entry captures Part ID, package count, and quantity per package. Shipment Scheduled and Shipped are part of the long-term lifecycle. Vendor can be suggested from prior outside-service history by part or entered manually. Create a full specification document and an end-user workflow document with mockup UI elements."

## Scope Summary

Module_OutsideService is a new top-level module for coordinator-managed outside-service requests.
Version 1 is limited to **Initial Entry** and **active waitlist visibility**.
The later states **Shipment Scheduled** and **Shipped** are intentionally documented as future phases so the design can grow without forcing those workflows into the first delivery.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create An Outside Service Request (Priority: P1)

An Outside Service Coordinator needs to enter a new request for one or more part IDs that must be sent out for external work. The coordinator records the part ID, the number of packages for each part, and the quantity per package so Shipping can later schedule the outbound shipment.

**Why this priority**: This is the core business action that creates value. Without request entry, there is no waitlist for Shipping to act on.

**Independent Test**: Can be fully tested by opening the module, entering a request with one or more part lines, saving it, and confirming the request appears in the active waitlist with status `InitialEntry`.

**Acceptance Scenarios**:

1. **Given** the coordinator opens Module_OutsideService, **When** they enter a valid request with at least one part line and save it, **Then** the system stores the request in MySQL and shows it in the active waitlist.
2. **Given** a coordinator enters a request without any vendor selection, **When** the request is saved, **Then** the system stores it as an open waitlist item for Shipping follow-up.
3. **Given** a request contains multiple part lines, **When** the coordinator saves the request, **Then** the system keeps the lines together under one request without requiring Shipping-owned details.

---

### User Story 2 - Review The Active Waitlist (Priority: P2)

The Outside Service Coordinator and Shipping team need a single place to see open outside-service requests that are ready for scheduling work, without mixing those requests into the existing Ship/Rec history lookup tool.

**Why this priority**: Once requests are created, the team needs immediate visibility into open work. This makes Version 1 operationally useful without requiring scheduled or shipped state transitions yet.

**Independent Test**: Can be tested by saving multiple requests, opening the waitlist, and verifying that the saved requests appear with the correct line counts and `InitialEntry` lifecycle state.

**Acceptance Scenarios**:

1. **Given** one or more requests exist, **When** a user opens the waitlist view, **Then** the system lists open requests in a consistent newest-first order.
2. **Given** a request contains multiple part lines, **When** the waitlist is shown, **Then** the request summary shows the number of lines and enough detail for Shipping to identify the work item.

---

### User Story 3 - Prepare For Future Scheduling And Shipping (Priority: P3)

The business expects the same request to later move into `ShipmentScheduled` and then `Shipped`, with vendor selection, BOL number, and shipment tracking captured outside Infor Visual by Shipping. The first release does not implement these transitions, but the design must leave room for them.

**Why this priority**: This protects the module design from becoming a dead-end while keeping Version 1 focused.

**Independent Test**: Can be tested as a design review by verifying the specification, entities, and workflow document all define the future states and fields consistently without requiring them in the first implementation.

**Acceptance Scenarios**:

1. **Given** the Version 1 specification, **When** the lifecycle is reviewed, **Then** `ShipmentScheduled` and `Shipped` are clearly documented as future phases rather than current delivery scope.
2. **Given** the future scheduling workflow is planned, **When** a later release begins, **Then** the design already identifies required fields such as selected vendor, BOL number, and scheduled shipment details.

---

## Edge Cases

- What happens when the coordinator enters multiple lines for the same part ID in a single request? The system should preserve the lines exactly as entered unless future consolidation rules are explicitly defined.
- What happens when package count or quantity per package is zero or negative? The request must be blocked with a clear validation error.
- What happens when a part ID has no Infor Visual outside-service history? The Version 1 request should still save because vendor selection does not happen yet.
- What happens when the same part appears in prior history with multiple vendors? The future Shipping scheduling workflow should show suggestions in a predictable order while still allowing manual override.
- What happens when Shipping opens the waitlist before any requests exist? The module should show an empty-state message rather than a blank grid.
- What happens when a request is partially complete but not saved? The module should keep unsaved data in-memory for the current session only unless future draft persistence is intentionally added.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a new top-level navigation destination named `Module_OutsideService`.
- **FR-002**: Version 1 MUST allow the Outside Service Coordinator to create a new outside-service request.
- **FR-003**: The system MUST allow a request to contain one or more part lines.
- **FR-004**: Each part line MUST capture `Part ID`, `Number Of Packages`, and `Quantity Per Package`.
- **FR-005**: The system MUST require at least one valid part line before a request can be saved.
- **FR-006**: The system MUST validate that package counts and quantities are positive numeric values.
- **FR-007**: The system MUST persist Version 1 outside-service requests in MySQL and MUST NOT use Infor Visual as the write store for the feature.
- **FR-008**: The system MUST assign a Version 1 lifecycle state of `InitialEntry` to newly created requests.
- **FR-009**: The system MUST display an active waitlist view of open requests stored by Module_OutsideService.
- **FR-010**: Version 1 MUST allow requests to be saved without a vendor selection because vendor assignment is owned by Shipping.
- **FR-011**: The future `ShipmentScheduled` phase MUST show vendor suggestions derived from prior outside-service history for the selected part ID when that history exists.
- **FR-012**: The future `ShipmentScheduled` phase MUST allow a custom vendor value when the suggestion list is unsuitable or empty.
- **FR-013**: The system MUST separate the new active waitlist from the existing Ship/Rec Outside Service History lookup tool.
- **FR-014**: The system MUST record request-level audit fields for creation metadata, including who created the request and when it was created.
- **FR-015**: The future lifecycle states `ShipmentScheduled` and `Shipped` MUST be documented in the design, but MUST NOT be required for Version 1 delivery.
- **FR-016**: The future `ShipmentScheduled` phase MUST support selected vendor, BOL number, and explicit shipment data stored outside Infor Visual.
- **FR-017**: The future `Shipped` phase MUST support moving completed requests into a history-oriented view or archive model.
- **FR-018**: The implementation MUST follow the project MVVM flow `View -> ViewModel -> Service -> DAO -> Database`.
- **FR-019**: Any MySQL write access for this module MUST use the project-standard DAO and stored-procedure patterns.
- **FR-020**: The module MUST keep Infor Visual access read-only and use it only for vendor suggestions or historical reference data.
- **FR-021**: Version 1 MUST NOT require scheduling, shipping confirmation, cancellation, edit-after-save workflows, or vendor assignment unless they are later approved as a separate scope expansion.

### Non-Goals For Version 1

- No Shipment Scheduled transition.
- No Shipped transition.
- No BOL entry screen.
- No cancellation or void flow.
- No post-save editing workflow.
- No write-back to Infor Visual.

## Key Entities *(include if feature involves data)*

- **OutsideServiceRequest**: The request header created by the Outside Service Coordinator. Stores request number, lifecycle status, created-by metadata, created timestamp, and summary information for the waitlist.
- **OutsideServiceRequestLine**: A child line belonging to an outside-service request. Stores part ID, package count, quantity per package, and line ordering.
- **OutsideServiceVendorSuggestion**: A read-only suggestion derived from prior Infor Visual outside-service history for a part ID. It is reserved for the future Shipping scheduling workflow.
- **OutsideServiceLifecycleState**: The business status model for the request. Version 1 uses `InitialEntry`. Future states include `ShipmentScheduled` and `Shipped`.

## Proposed Version 1 Data Model

### Application Tables

| Table | Purpose | Key Columns |
| ----- | ------- | ----------- |
| `outside_service_request` | Request header table for the active waitlist | `outside_service_request_id`, `request_number`, `status`, `created_by_user`, `created_utc`, `notes` |
| `outside_service_request_line` | Request detail lines | `outside_service_request_line_id`, `outside_service_request_id`, `line_number`, `part_id`, `package_count`, `quantity_per_package` |

### Proposed Stored Procedures

| Stored Procedure | Purpose |
| ---------------- | ------- |
| `sp_outside_service_request_insert` | Save a new request header |
| `sp_outside_service_request_line_insert` | Save a request line |
| `sp_outside_service_request_get_open` | Return active waitlist requests |
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
    Enum_OutsideServiceStatus.cs
  Services/
    Service_OutsideService.cs
  ViewModels/
    ViewModel_OutsideService_Main.cs
    ViewModel_OutsideService_RequestEntry.cs
    ViewModel_OutsideService_Waitlist.cs
  Views/
    View_OutsideService_Main.xaml
    View_OutsideService_RequestEntry.xaml
    View_OutsideService_Waitlist.xaml
```

### Integration Points

| Area | Planned Change |
| ---- | -------------- |
| Navigation | Add a new top-level menu entry in `MainWindow.xaml` and route map entry in `MainWindow.xaml.cs` |
| Dependency Injection | Add an `AddOutsideServiceModule` registration method in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` |
| MySQL | Add module-specific tables and stored procedures for request persistence |
| Infor Visual | Reuse existing outside-service history queries for future Shipping-owned vendor suggestion behavior only |
| Documentation | Add module documentation under `docs/Modules/Module_OutsideService` |

## Future Phase Design Notes

### Shipment Scheduled

The later `ShipmentScheduled` phase is expected to capture:

- selected vendor
- BOL number
- scheduled ship date
- selected shipping contact or handoff owner
- confirmation that Shipping accepted the request

### Shipped

The later `Shipped` phase is expected to capture:

- shipped date
- final vendor confirmation details
- movement from active waitlist to history/archive presentation

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A coordinator can create a valid outside-service request with at least one line in under 2 minutes without leaving the module.
- **SC-002**: 100% of saved Version 1 requests appear in the active waitlist immediately after save.
- **SC-003**: Coordinators can save valid requests without being blocked by Shipping-owned vendor decisions.
- **SC-004**: The specification and end-user workflow document agree on the Version 1 scope and future-phase boundaries with no conflicting lifecycle definitions.
- **SC-005**: Future implementation can add `ShipmentScheduled` and `Shipped` without replacing the Version 1 request-entry model.

## Open Follow-Up Items

- Confirm whether only the Outside Service Coordinator can create requests, or whether Leads should also be allowed.
- Confirm whether vendor suggestions for Shipping should be ranked by most recent use, most frequent use, or both.
- Confirm whether Version 1 needs per-request notes only, or both request-level and line-level notes.
- Confirm whether request numbers should be human-readable sequential values or GUID-backed display tokens.

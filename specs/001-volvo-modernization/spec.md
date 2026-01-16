# Feature Specification: Module_Volvo CQRS Modernization

**Feature Branch**: `001-volvo-modernization`  
**Created**: 2026-01-16  
**Status**: Draft  
**Input**: User description: "Modernize Module_Volvo to CQRS architecture with MediatR handlers, FluentValidation, strict constitutional compliance, and zero deviations from core principles"

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Volvo Shipment Entry with CQRS (Priority: P1)

Users must be able to create and manage Volvo dunnage requisition shipments using the modernized CQRS architecture without experiencing any functional degradation or workflow changes from the current implementation.

**Why this priority**: This is the primary workflow for the Volvo module, handling shipment creation, part selection, component explosion, and label generation. Without this working, the module provides no business value.

**Independent Test**: Can be fully tested by opening the Volvo shipment entry screen, adding parts with skid quantities, generating label CSV files, and completing the shipment. Success is measured by identical output files and database records compared to the current implementation.

**Acceptance Scenarios**:

1. **Given** user opens Volvo shipment entry screen, **When** the screen loads, **Then** active parts catalog is displayed and pending shipment (if exists) is loaded via CQRS query handlers
2. **Given** user searches for a part by number, **When** typing in search box, **Then** AutoSuggest shows filtered results from cached catalog without database calls
3. **Given** user adds a part with received skid quantity, **When** "Add Part" is clicked, **Then** CQRS command handler validates input, adds line to shipment, and updates UI
4. **Given** user generates label CSV, **When** "Generate Labels" is clicked, **Then** CQRS query handler performs component explosion and creates LabelView-formatted CSV file identical to legacy format
5. **Given** user completes shipment, **When** "Complete" is clicked, **Then** CQRS command handler validates all fields via FluentValidation, saves shipment, archives it, and navigates to history

---

### User Story 2 - Volvo Parts Master Data Management with Validation (Priority: P2)

Administrators must be able to manage Volvo parts master data (CRUD operations for parts and components) with strict validation rules enforced through FluentValidation, ensuring data integrity before database writes.

**Why this priority**: Master data quality directly impacts shipment accuracy and component explosion calculations. Without proper validation, corrupted data can cause shipment failures.

**Independent Test**: Can be fully tested by adding/editing/deactivating parts through the settings screen, importing CSV files with both valid and invalid data, and verifying validation error messages appear before database writes.

**Acceptance Scenarios**:

1. **Given** administrator adds new part, **When** required fields are missing or invalid, **Then** FluentValidation rejects command before DAO execution and displays user-friendly error
2. **Given** administrator imports CSV with parts, **When** file contains duplicate part numbers, **Then** validation pipeline detects duplicates and shows specific error messages
3. **Given** administrator edits component for part, **When** component quantity is negative or zero, **Then** validator rejects the command with clear error message
4. **Given** administrator deactivates part in use, **When** part has active pending shipments, **Then** business rule validation prevents deactivation with explanatory message

---

### User Story 3 - Volvo Shipment History with Optimized Queries (Priority: P3)

Users must be able to view, filter, and export historical Volvo shipments with improved performance through read-optimized CQRS query handlers and proper caching strategies.

**Why this priority**: Historical reporting is important for audit trails and trend analysis, but doesn't block daily operations. Performance improvements are valuable but not critical for MVP.

**Independent Test**: Can be fully tested by opening history screen, applying date filters, viewing shipment details, editing past shipments, and exporting to CSV - all operations must complete faster than legacy implementation.

**Acceptance Scenarios**:

1. **Given** user opens history screen, **When** screen loads with default date range, **Then** CQRS query handler retrieves last 30 days of shipments with optimized projection
2. **Given** user filters by date range, **When** filter is applied, **Then** query handler executes with proper indexes and returns results within 1 second for up to 1000 records
3. **Given** user views shipment details, **When** clicking on row, **Then** query handler loads shipment with lines in single database round-trip
4. **Given** user exports history to CSV, **When** "Export" is clicked, **Then** query handler streams data to CsvHelper without loading entire dataset into memory

---

### Edge Cases

- **What happens when pending shipment exists but has corrupted data?** Validation pipeline must detect corruption during load and show error dialog with option to archive corrupted shipment.
- **How does system handle concurrent CSV import operations?** Command handlers must enforce single-threaded import via pessimistic lock and queue subsequent requests.
- **What if component explosion results in zero quantities?** Business rule validator must flag parts with zero-quantity components and require manual review before label generation.
- **How are obsolete parts handled during shipment completion?** Query handlers must check part active status at completion time and warn user if any parts were deactivated since shipment creation.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST migrate all ViewModels to inject `IMediator` instead of `IService_Volvo` and `IService_VolvoMasterData`
- **FR-002**: System MUST create CQRS query handlers for all read operations (GetParts, GetShipment, GetHistory) returning `Model_Dao_Result<T>`
- **FR-003**: System MUST create CQRS command handlers for all write operations (CreateShipment, AddLine, CompleteShipment, SavePart) with transaction support
- **FR-004**: System MUST implement FluentValidation validators for all commands, enforcing business rules before DAO execution
- **FR-005**: System MUST register MediatR with global pipeline behaviors for validation, logging, and error handling from Module_Core
- **FR-006**: System MUST preserve exact file format for LabelView CSV generation (same column order, headers, and data formatting)
- **FR-007**: System MUST maintain backward compatibility with existing database stored procedures and schema
- **FR-008**: ViewModels MUST remain partial classes inheriting from `ViewModel_Shared_Base` and using `[ObservableProperty]`/`[RelayCommand]`
- **FR-009**: All Views MUST continue using `x:Bind` for data binding with no business logic in code-behind
- **FR-010**: DAOs MUST remain instance-based classes returning `Model_Dao_Result` without throwing exceptions
- **FR-011**: Services (`Service_Volvo`, `Service_VolvoMasterData`) MUST be deprecated and removed after migration to handlers
- **FR-012**: Navigation flow MUST remain identical (ShipmentEntry → History → Settings and back)
- **FR-013**: Authorization checks MUST be migrated to authorization pipeline behavior or command validators
- **FR-014**: Component explosion algorithm MUST produce identical results to legacy implementation (verified via unit tests)
- **FR-015**: CSV import/export MUST use CsvHelper library for consistent parsing and generation
- **FR-016**: System MUST achieve 80%+ test coverage for handlers, validators, and critical business logic
- **FR-017**: All async operations MUST support cancellation tokens passed through mediator pipeline
- **FR-018**: Error messages MUST be user-friendly and non-technical (no stack traces or raw SQL errors)
- **FR-019**: Audit logging MUST capture user actions for all write commands (create, update, complete, archive)
- **FR-020**: Module MUST remain 100% independent with zero dependencies on other feature modules

### Key Entities

- **VolvoShipment**: Represents a dunnage requisition shipment with date, number, notes, status (Pending/Completed), and employee number
- **VolvoShipmentLine**: Individual parts in shipment with part number, description, received skids, and quantity per skid
- **VolvoPart**: Master data for Volvo parts with part number, description, quantity per skid, and active status
- **VolvoPartComponent**: Component explosion data linking parent parts to child parts with component quantities
- **VolvoSetting**: Configuration settings for email recipients, SMTP details, and operational parameters

### Non-Functional Requirements

- **NFR-001**: Label CSV generation MUST complete within 3 seconds for shipments with up to 50 lines
- **NFR-002**: History queries MUST return results within 1 second for date ranges containing up to 1000 shipments
- **NFR-003**: Part search/autocomplete MUST respond within 200ms by using in-memory cached catalog
- **NFR-004**: System MUST maintain memory efficiency by disposing database connections properly in DAOs
- **NFR-005**: All CQRS handlers MUST be stateless and thread-safe for concurrent requests

### Technical Constraints

- **TC-001**: MUST use MediatR 12.0+ with explicit handler registration (no assembly scanning for Module_Volvo)
- **TC-002**: MUST use FluentValidation 11.0+ with async validator support
- **TC-003**: MUST use CsvHelper 30.0+ for CSV operations matching existing Module_Dunnage patterns
- **TC-004**: MUST use Serilog structured logging via `IService_LoggingUtility` with context enrichment
- **TC-005**: MUST NOT modify MySQL stored procedures or database schema during refactoring
- **TC-006**: MUST NOT change public interfaces of existing DAOs (for backward compatibility during transition)
- **TC-007**: MUST register all handlers in `App.xaml.cs` using same lifetime scopes as DAOs (Singleton for stateless)
- **TC-008**: MUST use `Enum_ErrorSeverity` for all error classifications matching Module_Core patterns

### Security Requirements

- **SR-001**: Authorization checks MUST be enforced at handler level before data access
- **SR-002**: User context MUST be captured from `IService_UserSessionManager` and included in audit logs
- **SR-003**: Sensitive data (if any) MUST NOT be logged in structured logs or error messages
- **SR-004**: CSV export MUST sanitize cell values to prevent formula injection attacks

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

1. **CQRS Migration Completeness**: 100% of ViewModels use `IMediator` with zero direct service calls (verified via grep search)
2. **Test Coverage**: Minimum 80% code coverage for handlers, validators, and business logic (measured by dotnet test --collect:"XPlat Code Coverage")
3. **Performance Parity**: Label CSV generation completes in ≤ current implementation time (3 seconds for 50 lines)
4. **Functional Parity**: All 15 user workflows produce identical outputs to legacy implementation (verified via integration tests)
5. **Constitutional Compliance**: Zero violations of Principles I-VII when audited via compliance checklist
6. **Zero Regressions**: All existing unit tests and integration tests pass without modification
7. **Build Success**: `dotnet build` completes with zero errors and zero warnings
8. **Navigation Integrity**: User can complete all workflows (Entry → Labels → Complete → History → Settings → Back) without errors

### Validation Criteria

- **Manual Testing**: QA team validates all 3 user stories with acceptance scenarios on development environment
- **Automated Testing**: CI/CD pipeline runs full test suite and verifies coverage threshold
- **Code Review**: Architecture review confirms adherence to constitution.md principles
- **Performance Profiling**: Label generation and history queries benchmarked against baseline metrics

---

## Dependencies *(mandatory)*

### Upstream Dependencies

- **Module_Core**: Requires global MediatR pipeline behaviors (ValidationBehavior, LoggingBehavior, ExceptionHandlingBehavior) to be registered in `App.xaml.cs`
- **Database**: Requires existing MySQL stored procedures for Volvo tables (`sp_Volvo_*`) to remain unchanged
- **NuGet Packages**: Requires MediatR, FluentValidation, CsvHelper, Serilog packages already installed in project

### Downstream Impact

- **Module_Core Services**: `Service_Volvo` and `Service_VolvoMasterData` will be deprecated - no other modules depend on them
- **Reporting Module**: `Dao_Reporting.GetVolvoHistoryAsync()` uses database view - no changes needed
- **App.xaml.cs**: Will require updates to register handlers and remove legacy service registrations

---

## Assumptions *(mandatory)*

1. **MediatR Infrastructure**: Assumes `Module_Core` already has global pipeline behaviors implemented (ValidationBehavior, LoggingBehavior) that can be reused
2. **Database Stability**: Assumes MySQL stored procedures are stable and well-tested, requiring no schema changes
3. **CsvHelper Patterns**: Assumes CSV import/export patterns from `Module_Dunnage` can be replicated for Volvo CSV operations
4. **Single-User Context**: Assumes Volvo module operates in single-user workstation mode (no concurrent multi-user editing of same shipment)
5. **LabelView Format**: Assumes external LabelView software expects exact CSV format currently generated (no flexibility for header changes)
6. **Authorization Simplicity**: Assumes current `Service_VolvoAuthorization` placeholder logic (allow-all with logging) is acceptable in modernized form
7. **Component Explosion Algorithm**: Assumes existing component explosion logic in `Service_Volvo.CalculateComponentExplosion()` is correct and should be preserved exactly
8. **No Breaking Changes**: Assumes refactoring must maintain 100% backward compatibility with existing Views, Models, and DAOs

---

## Out of Scope *(mandatory)*

- Changing database schema or stored procedures (schema is frozen)
- Modifying LabelView CSV file format (external dependency)
- Adding new Volvo features beyond existing functionality
- Implementing real role-based access control (using placeholder authorization)
- Migrating other modules (Module_Receiving, Module_Dunnage) to CQRS
- Performance optimizations beyond what CQRS naturally provides
- Creating new UI screens or changing existing UI layouts
- Implementing real-time collaboration or multi-user shipment editing
- Adding telemetry or observability beyond structured logging

---

## Risks *(mandatory)*

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Component explosion algorithm produces different results after refactoring | **CRITICAL** | Medium | Implement property-based tests comparing legacy vs new implementation with 1000+ test cases |
| LabelView CSV format changes break downstream label printing | **HIGH** | Low | Create golden file tests verifying byte-for-byte identical CSV output |
| ViewModels become tightly coupled to handler contracts | **MEDIUM** | High | Use DTOs/request objects separate from domain models to decouple ViewModel from handler internals |
| FluentValidation async validators cause UI freezing | **MEDIUM** | Medium | Use `ConfigureAwait(false)` in all validators and test on UI thread |
| Missing pipeline behaviors cause validation to be skipped | **HIGH** | Low | Write integration tests verifying pipeline executes for every command |
| Incomplete migration leaves hybrid Service+Mediator code | **MEDIUM** | Medium | Use feature flags to toggle between legacy and CQRS paths during transition |
| Performance regression on history queries with large datasets | **MEDIUM** | Low | Benchmark before/after with 10,000+ historical records and optimize indexes |

---

## Compliance Alignment *(mandatory for STRICT mode)*

### Constitutional Principles Checklist

- **Principle I (MVVM & View Purity)**: ✅ ViewModels remain partial, inherit `ViewModel_Shared_Base`, Views use `x:Bind` exclusively, no code-behind logic
- **Principle II (Data Access Integrity)**: ✅ DAOs unchanged (instance-based, stored procedures only, return `Model_Dao_Result`)
- **Principle III (CQRS + Mediator First)**: ✅ PRIMARY FOCUS - Migrate all workflows to MediatR handlers with single-responsibility pattern
- **Principle IV (DI & Modular Boundaries)**: ✅ Handlers registered in App.xaml.cs, Module_Volvo services moved from Module_Core (if any), 100% module independence
- **Principle V (Validation & Structured Logging)**: ✅ FluentValidation on all commands, Serilog via `IService_LoggingUtility`, `IService_ErrorHandler` for UI errors
- **Principle VI (Security & Session Discipline)**: ✅ Authorization checks via validators or behaviors, user context from `IService_UserSessionManager`, audit breadcrumbs
- **Principle VII (Library-First Reuse)**: ✅ MediatR (orchestration), FluentValidation (rules), CsvHelper (exports), Serilog (logging), no custom alternatives

**Zero Deviations Policy**: Any code changes violating above principles MUST be rejected in code review. This specification serves as the constitutional contract.

---

## Notes

- This is an **architectural refactoring**, not a feature addition - users should notice zero functional changes
- Task breakdown will be generated separately using module-modernization-tasks.md template
- Constitutional audit will be performed BEFORE any code changes to create explicit violation checklist
- Implementation will follow 9-phase approach from module-rebuilder agent (Phase 0-5 + Compliance + Documentation + Deployment)
- Parallel development NOT recommended - sequential phases ensure stability at each checkpoint

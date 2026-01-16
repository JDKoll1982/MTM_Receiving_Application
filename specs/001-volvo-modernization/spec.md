# Feature Specification: Module_Volvo CQRS Modernization

**Feature Branch**: `001-volvo-modernization`  
**Created**: January 16, 2026  
**Status**: Draft  
**Input**: User description: "module-rebuilder Module_Volvo"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Volvo Shipment Entry with CQRS (Priority: P1)

Users must be able to create and manage Volvo shipments (entering part numbers, skid counts, generating labels, and completing shipments) using modernized CQRS architecture without experiencing functional degradation or workflow changes from current implementation.

**Why this priority**: This is the primary workflow of the Volvo module. Without shipment entry capability, the module provides no value to receiving operations.

**Independent Test**: Can be fully tested by creating a test shipment with multiple parts, generating labels, and completing the shipment. Success measured by identical CSV label output and database records compared to current implementation.

**Acceptance Scenarios**:

1. **Given** user opens Volvo Shipment Entry screen, **When** screen loads, **Then** shipment form is initialized with current date and next shipment number via CQRS query handler
2. **Given** user searches for a part number, **When** typing in search field, **Then** CQRS query handler returns autocomplete suggestions from master data
3. **Given** user adds a part with skid count, **When** "Add Part" clicked, **Then** CQRS command handler validates and adds part to shipment list
4. **Given** user has completed shipment data, **When** "Generate Labels" clicked, **Then** CQRS query handler produces CSV file with identical format to legacy system
5. **Given** user completes shipment, **When** "Complete Shipment" clicked, **Then** CQRS command handler validates, saves to database, generates labels, sends email notification, and clears form
6. **Given** user has pending shipment, **When** returning to screen, **Then** CQRS query handler loads pending shipment data for continuation

---

### User Story 2 - Volvo Shipment History & Editing (Priority: P2)

Users must be able to view historical Volvo shipments with filtering capabilities and edit existing shipments when needed, using CQRS patterns for efficient data retrieval and updates.

**Why this priority**: History viewing and editing are important administrative functions but not critical to day-to-day receiving workflow. Users can continue receiving without this feature.

**Independent Test**: Can be tested by filtering shipments by date range and status, then editing a completed shipment. Success measured by correct filtered results and successful update of shipment data.

**Acceptance Scenarios**:

1. **Given** user opens Volvo History screen, **When** screen loads, **Then** CQRS query handler retrieves recent shipments with pagination
2. **Given** user sets date range and status filter, **When** "Filter" clicked, **Then** CQRS query handler returns matching shipments
3. **Given** user selects a shipment, **When** "Edit" clicked, **Then** CQRS query handler loads full shipment details with line items for editing
4. **Given** user modifies shipment data, **When** "Save" clicked, **Then** CQRS command handler validates and updates shipment via stored procedure
5. **Given** user wants to export history, **When** "Export" clicked, **Then** CQRS query handler generates CSV export of filtered shipments

---

### User Story 3 - Volvo Master Data Management (Priority: P2)

Users must be able to manage Volvo part master data (adding, editing, deactivating parts, managing bill-of-materials components) using CQRS commands and queries with validation.

**Why this priority**: Master data management is typically performed during setup or occasional updates, not during active receiving operations. The system can function with existing master data.

**Independent Test**: Can be tested by adding a new part with components, editing part details, and deactivating an obsolete part. Success verified by database queries confirming changes.

**Acceptance Scenarios**:

1. **Given** user opens Volvo Settings screen, **When** screen loads, **Then** CQRS query handler retrieves all active parts with component counts
2. **Given** user toggles "Show Inactive", **When** checkbox changed, **Then** CQRS query handler retrieves filtered part list (active/all)
3. **Given** user clicks "Add Part", **When** dialog opens, **Then** user enters part number and quantity per skid
4. **Given** user saves new part, **When** "Save" clicked, **Then** CQRS command handler with FluentValidation validates and inserts part
5. **Given** user selects a part and clicks "Edit", **When** dialog opens, **Then** CQRS query handler loads current part details
6. **Given** user modifies part data, **When** "Save" clicked, **Then** CQRS command handler validates and updates part
7. **Given** user selects active part, **When** "Deactivate" clicked, **Then** CQRS command handler marks part as inactive
8. **Given** user selects part with components, **When** "View Components" clicked, **Then** CQRS query handler retrieves BOM components
9. **Given** user exports master data, **When** "Export CSV" clicked, **Then** CQRS query handler generates CSV export
10. **Given** user imports CSV file, **When** "Import CSV" clicked, **Then** CQRS command handler validates and bulk inserts/updates parts

---

### User Story 4 - Email Notification & Label Preview (Priority: P3)

Users must be able to preview email notifications before completing shipments and generate label files independently, using CQRS queries for data retrieval and formatting.

**Why this priority**: Email preview is a quality-of-life feature. The system can send emails without preview capability. This enhances user confidence but is not essential to core workflow.

**Independent Test**: Can be tested by previewing email for a shipment with multiple parts and discrepancies. Success measured by correct HTML/text formatting matching legacy system.

**Acceptance Scenarios**:

1. **Given** user has entered shipment with parts, **When** "Preview Email" clicked, **Then** CQRS query handler formats email data (HTML + plain text) and displays preview dialog
2. **Given** email preview is open, **When** user clicks "Copy to Clipboard", **Then** email text is copied for external use
3. **Given** shipment has discrepancies, **When** email formatted, **Then** CQRS query handler includes discrepancy details in email body

---

### Edge Cases

- What happens when user tries to add duplicate part to same shipment? → CQRS command validation rejects duplicate with clear error message
- How does system handle network interruption during shipment completion? → Command handler returns failure result, shipment remains in pending state for retry
- What if master data CSV import has invalid part numbers or formats? → FluentValidation in command handler rejects invalid rows, returns detailed error report
- How does system handle concurrent shipment edits by different users? → Optimistic concurrency check in update command handler detects conflicts
- What if stored procedure returns unexpected error during save? → DAO returns Model_Dao_Result failure, command handler propagates to UI via IService_ErrorHandler
- How does system handle part with zero or negative quantity per skid? → FluentValidation rejects with "Quantity must be greater than zero" message
- What if shipment has no parts when user tries to complete? → Command validation prevents completion, displays "Shipment must have at least one part"

## Requirements *(mandatory)*

### Functional Requirements

#### CQRS Architecture (Principle III Compliance)

- **FR-001**: System MUST refactor all ViewModels to use `IMediator` for commands and queries instead of direct service calls
- **FR-002**: System MUST create query handlers for all read operations (get shipments, get parts, get history, load pending shipment, autocomplete search)
- **FR-003**: System MUST create command handlers for all write operations (create shipment, update shipment, complete shipment, add part, update part, deactivate part)
- **FR-004**: System MUST create FluentValidation validators for all command models (shipment data, part data, import data)
- **FR-005**: System MUST implement pipeline behaviors for logging, validation, and error handling using existing Module_Core infrastructure

#### Data Access Layer (Principle II Compliance)

- **FR-006**: System MUST verify all DAOs are instance-based (NOT static) and return `Model_Dao_Result<T>`
- **FR-007**: System MUST ensure all MySQL operations use stored procedures (sp_volvo_shipment_*, sp_volvo_part_*, sp_volvo_line_*)
- **FR-008**: System MUST maintain existing stored procedure interfaces (parameter names, return schemas) for backward compatibility
- **FR-009**: Services MUST delegate to DAOs for all database access (no direct Helper_Database_* usage in services)

#### MVVM Purity (Principle I Compliance)

- **FR-010**: ViewModels MUST remain `partial` classes inheriting from `ViewModel_Shared_Base`
- **FR-011**: ViewModels MUST use `[ObservableProperty]` and `[RelayCommand]` attributes (no manual INotifyPropertyChanged)
- **FR-012**: Views MUST use `x:Bind` for all ViewModel bindings (migrate any remaining `{Binding}` to `x:Bind`)
- **FR-013**: Views MUST NOT contain business logic in `.xaml.cs` code-behind (only UI initialization)

#### Functional Preservation

- **FR-014**: System MUST generate label CSV files with identical format to legacy implementation (column order, headers, data format)
- **FR-015**: System MUST calculate component explosions using identical algorithm (skids × qty/skid × components)
- **FR-016**: System MUST format email notifications (HTML and plain text) with identical content to legacy
- **FR-017**: System MUST preserve existing validation rules (duplicate parts, required fields, skid count ranges)
- **FR-018**: System MUST maintain pending shipment workflow (save in-progress, resume later)

#### Testing & Quality (Principle VIII Compliance)

- **FR-019**: System MUST achieve minimum 80% code coverage for all handlers and validators
- **FR-020**: System MUST include property-based tests for calculation algorithms (component explosion, piece counts)
- **FR-021**: System MUST include integration tests for handler → service → DAO → database flow
- **FR-022**: System MUST include golden file tests for CSV label generation and email formatting

### Key Entities

- **Model_VolvoShipment**: Represents a Volvo shipment header (shipment number, date, status, notes, PO number, receiver number)
- **Model_VolvoShipmentLine**: Represents individual parts in a shipment (part number, received skid count, expected counts, discrepancy notes)
- **Model_VolvoPart**: Master data for Volvo parts (part number, quantity per skid, active/inactive status)
- **Model_VolvoPartComponent**: Bill-of-materials components for parts (component part number, quantity per parent)
- **Model_VolvoEmailData**: Email notification data structure (recipients, subject, HTML body, plain text body)
- **VolvoShipmentStatus**: Enum for shipment states (Pending, Completed)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: CQRS Migration Completeness - 100% of ViewModel operations use IMediator (verified via grep search for `IMediator` presence and absence of direct service calls in ViewModels)
- **SC-002**: Handler Implementation - All 21 identified operations have corresponding command/query handlers (12 queries + 9 commands, verified by file count in Handlers/Commands/ and Handlers/Queries/)
- **SC-003**: Validation Coverage - All command handlers have FluentValidation validators (verified by 1:1 mapping of commands to validators)
- **SC-004**: Test Coverage - Minimum 80% code coverage for handlers, validators, and services (verified by test coverage report)
- **SC-005**: Functional Parity - CSV label files are byte-for-byte identical to legacy implementation (verified by diff comparison using golden files)
- **SC-006**: Functional Parity - Email HTML/text output matches legacy implementation exactly (verified by string comparison in tests)
- **SC-007**: Functional Parity - Component explosion calculations produce identical results to legacy (verified by property-based tests with 1000+ test cases)
- **SC-008**: Constitutional Compliance - Zero violations of Principles I-VII (verified by architecture validation tools and code review)
- **SC-009**: Zero Regressions - All existing integration tests pass without modification (verified by `dotnet test --filter "FullyQualifiedName~Module_Volvo"`)
- **SC-010**: Build Success - `dotnet build` completes with zero errors and zero warnings in Release configuration
- **SC-011**: Performance Parity - Shipment completion time ≤ current implementation average (measured via LoggingBehavior execution time logs)
- **SC-012**: Database Integrity - All database operations maintain existing stored procedure contracts (verified by schema validation tests)

## Dependencies *(mandatory)*

### Upstream Dependencies

- **Module_Core CQRS Infrastructure**: MediatR 12.4.1, FluentValidation 11.10.0, LoggingBehavior, ValidationBehavior, AuditBehavior (already installed)
- **Module_Core Base Classes**: `ViewModel_Shared_Base`, `IService_ErrorHandler`, `IService_LoggingUtility` (already exists)
- **Existing Stored Procedures**: `sp_volvo_shipment_insert`, `sp_volvo_shipment_update`, `sp_volvo_shipment_complete`, `sp_volvo_line_insert`, `sp_volvo_part_insert`, `sp_volvo_part_update`, `sp_volvo_part_getall` (must not change)
- **MySQL Database**: `mtm_receiving_application` schema with tables `volvo_label_data`, `volvo_label_parts_data`, `volvo_master_data`, `volvo_master_data_components` (schema unchanged)
- **NuGet Packages**: CommunityToolkit.Mvvm 8.2+, MySql.Data 9.0+, Microsoft.Extensions.DependencyInjection 8.0+ (already installed)

### Downstream Dependencies (Services to Deprecate)

- **Service_Volvo**: Business logic will move to command/query handlers (keep for gradual migration, mark obsolete after completion)
- **Service_VolvoMasterData**: Master data logic will move to handlers (keep for gradual migration, mark obsolete after completion)

### New Dependencies Required

- **Mapster** (optional but recommended): For DTO mapping between models and requests/responses (alternative: AutoMapper)
- **Ardalis.GuardClauses**: For cleaner validation in handlers (recommended by Principle VII)
- **Bogus**: For generating test data in unit tests (recommended by Principle VIII)
- **NSubstitute** or **Moq**: For mocking in unit tests (Moq likely already installed)

## Assumptions *(mandatory)*

- **Database Schema Stability**: Volvo tables and stored procedures will not change during modernization (no breaking schema changes)
- **Single-User Workflow**: No concurrent editing of same shipment by multiple users (optimistic concurrency optional but not required for MVP)
- **Backward Compatibility Required**: Existing CSV label files must be consumable by external Volvo systems without modification
- **Email Format Locked**: Email HTML/text format must match legacy exactly to maintain consistency with recipient expectations
- **Stored Procedure Reuse**: All existing stored procedures will be reused as-is (no new stored procedures needed)
- **Existing DAOs Compliant**: DAOs are already instance-based and return `Model_Dao_Result<T>` (verified in initial analysis)
- **Test Framework Consistency**: xUnit + FluentAssertions + Moq pattern from other modules applies here
- **Development Database Available**: MySQL test database with Volvo schema and seed data is available for integration testing
- **No UI Redesign**: XAML views will maintain current layout and controls (only binding migration from `Binding` to `x:Bind`)

## Out of Scope *(mandatory)*

- **Database Schema Changes**: No modifications to Volvo tables or stored procedures
- **UI Redesign**: No changes to View layouts, control types, or visual styling
- **New Features**: No additional functionality beyond what exists in current implementation
- **Migration of Other Modules**: Only Module_Volvo is being modernized in this feature
- **Performance Optimizations**: No proactive performance improvements beyond CQRS architecture benefits
- **Concurrent Editing Support**: No optimistic/pessimistic locking for multi-user scenarios
- **Real-Time Notifications**: No SignalR or real-time updates for shipment status changes
- **Advanced Reporting**: No new reporting capabilities beyond existing export functions
- **API/Web Service Integration**: No REST API or external system integrations
- **Mobile/Web UI**: Desktop WinUI 3 application only

## Risks *(mandatory)*

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Component explosion calculation produces different results after refactoring | CRITICAL | Medium | Property-based tests with 1000+ random test cases comparing old vs new calculations. Golden file tests for known scenarios. |
| CSV label format changes break downstream Volvo systems | CRITICAL | Low | Byte-for-byte comparison tests against golden files. Integration test with actual label printer validation. |
| Email HTML/text format differs from legacy, confusing recipients | HIGH | Low | String comparison tests against known-good email outputs. Visual inspection during UAT. |
| FluentValidation rules miss edge cases from legacy validation | HIGH | Medium | Comprehensive test coverage mapping all legacy validation paths to FluentValidation rules. |
| Missing MediatR pipeline behaviors cause silent failures | HIGH | Low | Integration tests verify LoggingBehavior, ValidationBehavior, AuditBehavior execute for all handlers. |
| ViewModels still directly call services after refactoring | MEDIUM | Medium | Static code analysis tool to detect service injection in ViewModels (should only inject IMediator). |
| Test coverage below 80% threshold | MEDIUM | Low | TDD approach: write tests before handlers. Coverage report integrated into CI/CD pipeline. |
| Performance degradation from MediatR overhead | MEDIUM | Low | Benchmark tests comparing legacy vs CQRS execution times. Target: <10ms overhead per operation. |
| Binding errors from `Binding` to `x:Bind` migration | MEDIUM | Medium | Compile-time validation catches most errors. Runtime testing of all UI workflows. |
| Stored procedure contract assumptions incorrect | HIGH | Low | Integration tests validate all DAO operations against actual stored procedures in test database. |

## Compliance Alignment *(mandatory)*

### Constitutional Principles Checklist

- **Principle I (MVVM & View Purity)**: ✅ ViewModels remain partial, inherit `ViewModel_Shared_Base`, use `[ObservableProperty]`/`[RelayCommand]`. Views migrate to `x:Bind` exclusively. No business logic in code-behind.
  
- **Principle II (Data Access Integrity)**: ✅ DAOs verified as instance-based returning `Model_Dao_Result<T>`. All MySQL operations use existing stored procedures. Services delegate to DAOs only.

- **Principle III (CQRS + Mediator First)**: ✅ **PRIMARY FOCUS** - ViewModels refactored to inject `IMediator` instead of services. All read operations become query handlers. All write operations become command handlers with FluentValidation.

- **Principle IV (DI & Modular Boundaries)**: ✅ All handlers, validators, and services registered in `App.xaml.cs`. No service locator pattern. Handlers use constructor injection for DAOs and services.

- **Principle V (Validation & Structured Logging)**: ✅ FluentValidation validators created for all commands. Serilog structured logging via LoggingBehavior captures all handler executions with timing.

- **Principle VI (Security & Session Discipline)**: ✅ AuditBehavior logs user context for all operations. Existing authentication/authorization patterns maintained. No changes to session management.

- **Principle VII (Library-First Reuse)**: ✅ Leverages MediatR, FluentValidation, Serilog (already installed). Recommends Mapster for DTO mapping, Ardalis.GuardClauses for validation, Bogus for test data.

**Zero Deviations Policy**: Any code changes violating above principles MUST be rejected in code review. This specification serves as the constitutional contract for Module_Volvo modernization.

### Pre-Modernization Violations Identified

Based on initial analysis, the following constitutional violations exist in current Module_Volvo implementation:

1. **Principle III Violation**: ViewModels directly inject and call `IService_Volvo` and `IService_VolvoMasterData` instead of using `IMediator`
2. **Principle I Violation (Minor)**: Some Views use `{Binding}` instead of `x:Bind` in DataGrid columns (20 occurrences found)
3. **Principle V Violation**: No FluentValidation validators exist (validation logic embedded in services/ViewModels)

**Modernization resolves ALL violations** by:

- Refactoring ViewModels to use `IMediator` exclusively
- Migrating all `{Binding}` to `x:Bind` with appropriate Mode
- Creating FluentValidation validators for all command models

## Workflow Analysis *(informational)*

### Current ViewModel Commands (to be converted to MediatR handlers)

**ViewModel_Volvo_ShipmentEntry (26 methods)**:

- `InitializeAsync` → Query: GetInitialShipmentDataQuery
- `LoadPendingShipmentAsync` → Query: GetPendingShipmentQuery
- `UpdatePartSuggestions` → Query: SearchVolvoPartsQuery
- `AddPart` → Command: AddPartToShipmentCommand
- `RemovePart` → Command: RemovePartFromShipmentCommand
- `GenerateLabelsAsync` → Query: GenerateLabelCsvQuery
- `PreviewEmailAsync` → Query: FormatEmailDataQuery
- `SaveAsPendingAsync` → Command: SavePendingShipmentCommand
- `CompleteShipmentAsync` → Command: CompleteShipmentCommand
- `ClearShipmentForm` → Local UI state (no handler needed)
- `StartNewEntry` → Local UI state (no handler needed)
- `ViewHistory` → Navigation (no handler needed)

**ViewModel_Volvo_History (9 methods)**:

- Constructor initialization → Query: GetRecentShipmentsQuery
- `FilterAsync` → Query: GetShipmentHistoryQuery (with filter parameters)
- `ViewDetailAsync` → Query: GetShipmentDetailQuery
- `EditAsync` → Command: UpdateShipmentCommand
- `ExportAsync` → Query: ExportShipmentsQuery
- `GoBack` → Navigation (no handler needed)

**ViewModel_Volvo_Settings (12 methods)**:

- Constructor initialization → Query: GetAllVolvoPartsQuery
- `RefreshAsync` → Query: GetAllVolvoPartsQuery (with active/inactive filter)
- `AddPartAsync` → Command: AddVolvoPartCommand
- `EditPartAsync` → Command: UpdateVolvoPartCommand
- `DeactivatePartAsync` → Command: DeactivateVolvoPartCommand
- `ViewComponentsAsync` → Query: GetPartComponentsQuery
- `ExportCsvAsync` → Query: ExportPartsCsvQuery
- `ImportCsvAsync` → Command: ImportPartsCsvCommand

### Total Handlers Required

- **Query Handlers**: 11 (reads, searches, exports, formatting)
- **Command Handlers**: 8 (inserts, updates, deletes, imports)
- **Validators**: 8 (one per command)

---

## Notes for Implementation

### Critical Success Factors

1. **Golden File Tests First**: Create golden files for CSV labels and email formats BEFORE refactoring to ensure byte-for-byte comparison
2. **Property-Based Testing for Calculations**: Component explosion logic is complex - use property-based tests (FsCheck or similar) with 1000+ test cases
3. **Gradual Migration**: Keep legacy services during transition, mark as `[Obsolete]` after ViewModel refactoring complete
4. **Integration Tests for Pipeline**: Verify LoggingBehavior, ValidationBehavior, AuditBehavior execute correctly for all handlers
5. **Binding Migration Carefully**: Convert `{Binding}` to `x:Bind` one View at a time with full UI testing to catch compile-time errors

### Architecture Decision Records

- **ADR-001**: Use MediatR for CQRS instead of custom command/query infrastructure (leverage existing Module_Core setup)
- **ADR-002**: Reuse existing stored procedures as-is (no schema changes to minimize risk)
- **ADR-003**: Mapster recommended over AutoMapper for DTO mapping (faster, less configuration overhead)
- **ADR-004**: Keep services temporarily during migration for rollback capability (deprecate after full migration)
- **ADR-005**: Use Ardalis.GuardClauses in handlers for cleaner validation syntax (constitutional Principle VII library-first)

### Testing Strategy

- **Unit Tests**: Handlers in isolation with mocked DAOs/services (Moq)
- **Integration Tests**: Full pipeline (handler → service → DAO → database) with test database
- **Property-Based Tests**: Calculation algorithms (component explosion, piece counts) with random inputs
- **Golden File Tests**: CSV label generation and email formatting against known-good outputs
- **UI Tests**: Manual testing of all workflows after binding migration to `x:Bind`

### Documentation Updates Required

- Update `Module_Volvo/README.md` with CQRS architecture overview
- Create `Module_Volvo/HANDLERS.md` documenting all query/command handlers
- Update `.github/copilot-instructions.md` with Volvo-specific CQRS examples
- Add ADRs to `.specify/memory/architecture-decisions.md`

---

**Specification Status**: ✅ READY FOR TASK GENERATION

**Next Steps**:

1. Run `/speckit.tasks` to generate implementation checklist
2. Run `@module-compliance-auditor Module_Volvo` for detailed violation report
3. Run `/speckit.plan` for detailed technical implementation plan
4. Begin Phase 1: Setup (handlers, validators, tests infrastructure)

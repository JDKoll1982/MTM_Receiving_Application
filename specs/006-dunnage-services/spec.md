# Feature Specification: Dunnage Services Layer

**Feature Branch**: `006-dunnage-services`  
**Created**: 2025-12-26  
**Status**:  Ready for Implementation  
**Parent Feature**: Dunnage Receiving System V2  
**Depends On**: 005-dunnage-stored-procedures

> **CRITICAL IMPLEMENTATION NOTES**:
> 1. **Stored Procedure Alignment**: The `IService_MySQL_Dunnage` interface has been explicitly synchronized with the stored procedures defined in `specs/CompletedSpecs/005-dunnage-stored-procedures/spec.md`. Any changes to method signatures here must be reflected in the underlying SPs and DAOs.
> 2. **User Auditing**: All modification methods (Insert/Update/Delete) require a user identifier. Implementations must inject `IService_UserSessionManager` to retrieve the current user for the `p_user` stored procedure parameters.
> 3. **Validation Strategy**: Services perform "Fail Fast" business logic validation (e.g., checking dependencies before delete). DAOs rely on database constraints. Do not duplicate SQL constraint logic in C# unless for user experience (e.g., pre-checking uniqueness).
> 4. **CSV Dual-Write**: The CSV service must strictly adhere to the "Local First, Network Best-Effort" policy. Network failures must never crash the application or prevent the local save.

## Overview

Create the service layer that provides business logic, workflow orchestration, and CSV export functionality for the dunnage feature. This layer sits between ViewModels and DAOs, handling state management, validation, and cross-cutting concerns.

**Architecture Principle**: Services are registered as singletons (workflow) or transients (stateless operations) in DI container.  All services use dependency injection and interface-based contracts.

## User Scenarios & Testing

### User Story 1 - Dunnage Workflow State Machine (Priority: P1)

As a **ViewModel developer**, I need a workflow service that manages the state machine for the wizard workflow (Mode → Type → Part → Quantity → Details → Review → Save) so that I can orchestrate multi-step data entry.

**Why this priority**: The workflow service is the backbone of the wizard UI. Without it, the wizard cannot navigate between steps or maintain session state. 

**Independent Test**: Can be tested by creating a workflow instance, advancing through steps programmatically, verifying state transitions, and validating that session data persists correctly across steps.

**Acceptance Scenarios**:

1. **Given** a new workflow instance, **When** `StartWorkflowAsync()` is called, **Then** current step is set to ModeSelection and empty session is created
2. **Given** workflow is on TypeSelection step, **When** `AdvanceToNextStepAsync()` is called with valid type, **Then** current step advances to PartSelection and selected type is stored in session
3. **Given** workflow is on ReviewStep, **When** `SaveSessionAsync()` is called, **Then** all loads in session are persisted to database and CSV is exported
4. **Given** workflow has session data, **When** `ClearSession()` is called, **Then** all session properties are reset to defaults
5. **Given** workflow state changes, **When** step transition occurs, **Then** `StepChanged` event fires with correct step information

---

### User Story 2 - MySQL Dunnage Service (Priority: P1)

As a **ViewModel or admin UI developer**, I need a service that wraps all DAO operations with business logic, validation, and error handling so that I can perform dunnage CRUD operations without directly calling DAOs.

**Why this priority**:  This service provides the primary data access interface for all ViewModels.  It adds validation, logging, and business rules on top of raw DAO operations.

**Independent Test**: Can be tested by calling service methods with various inputs (valid, invalid, edge cases) and verifying correct error messages, validation enforcement, and successful DAO delegation.

**Acceptance Scenarios**:

1. **Given** `GetAllTypesAsync()` is called, **When** DAO succeeds, **Then** service returns list of types with logging of operation
2. **Given** `DeleteTypeAsync()` is called for type with parts, **When** validation executes, **Then** service checks part count first and returns error message without calling DAO
3. **Given** `InsertPartAsync()` is called with duplicate PartID, **When** DAO returns error, **Then** service wraps error in user-friendly message
4. **Given** `GetAllSpecKeysAsync()` is called, **When** multiple types have different specs, **Then** service returns union of all unique spec keys for CSV headers
5. **Given** any service method encounters exception, **When** error occurs, **Then** exception is logged and user-friendly error returned

---

### User Story 3 - CSV Export Service (Priority:  P1)

As a **workflow orchestrator**, I need a service that exports dunnage loads to CSV format for LabelView label printing so that receiving data integrates with the existing label system.

**Why this priority**: CSV export is the final step of the receiving workflow. Without it, the system cannot produce labels for received dunnage.

**Independent Test**:  Can be tested by providing a list of loads, calling `WriteToCSVAsync()`, and verifying CSV file is created at correct paths (local AppData and network share) with proper headers and data formatting.

**Acceptance Scenarios**:

1. **Given** a list of 10 loads, **When** `WriteToCSVAsync()` is called, **Then** CSV files are created in both local and network locations
2. **Given** loads have various spec values, **When** CSV is generated, **Then** headers include fixed columns (Type, PartID, Qty, PO, Date, User) plus all spec keys in alphabetical order
3. **Given** a load missing optional spec value, **When** CSV is written, **Then** cell is left blank (empty string)
4. **Given** CSV write to network fails, **When** network is unavailable, **Then** local file still succeeds and error is logged
5. **Given** no loads provided, **When** `WriteToCSVAsync()` is called, **Then** service returns error indicating no data to export

---

### Edge Cases

- What happens when workflow advances without required session data?  (Service validates before advancing, returns error)
- What happens when getting spec keys from database with no specs defined? (Return empty union, no error)
- What happens when CSV contains special characters (commas, quotes)? (Properly escape per CSV RFC 4180)
- What happens when network share path is invalid or inaccessible? (Log error, succeed on local file creation)
- What happens when two users export CSV simultaneously? (Separate user folders prevent collisions)

## Requirements

### Functional Requirements - Workflow Service

#### IService_DunnageWorkflow / Service_DunnageWorkflow
- **FR-001**: Service MUST provide `WorkflowStep CurrentStep { get; }` property for step tracking
- **FR-002**: Service MUST provide `Model_DunnageSession CurrentSession { get; }` property for state management
- **FR-003**:  Service MUST provide `Task<bool> StartWorkflowAsync()` to initialize workflow
- **FR-004**: Service MUST provide `Task<WorkflowStepResult> AdvanceToNextStepAsync()` to navigate forward
- **FR-005**: Service MUST provide `void GoToStep(WorkflowStep step)` for direct navigation
- **FR-006**:  Service MUST provide `Task<SaveResult> SaveSessionAsync()` to persist session data
- **FR-007**: Service MUST provide `void ClearSession()` to reset workflow state
- **FR-008**:  Service MUST fire `event EventHandler StepChanged` when step transitions occur
- **FR-009**: Service MUST fire `event EventHandler<string> StatusMessageRaised` for user notifications
- **FR-010**:  Service MUST validate session data before allowing step advancement (e.g., Type selected before Part)

### Functional Requirements - MySQL Service

#### IService_MySQL_Dunnage / Service_MySQL_Dunnage
##### Type Operations (5 methods)
- **FR-011**: Service MUST provide `Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync()`
- **FR-012**: Service MUST provide `Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId)`
- **FR-013**: Service MUST provide `Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type)` with validation
- **FR-014**: Service MUST provide `Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type)` with validation
- **FR-015**: Service MUST provide `Task<Model_Dao_Result> DeleteTypeAsync(int typeId)` with impact analysis

##### Spec Operations (6 methods)
- **FR-016**: Service MUST provide `Task<Model_Dao_Result<Model_DunnageSpec>> GetSpecsForTypeAsync(int typeId)`
- **FR-017**: Service MUST provide `Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec)` with JSON validation
- **FR-018**:  Service MUST provide `Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec)` with JSON validation
- **FR-019**: Service MUST provide `Task<Model_Dao_Result> DeleteSpecAsync(int specId)` with impact analysis
- **FR-020**:  Service MUST provide `Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId)` with confirmation
- **FR-021**: Service MUST provide `Task<List<string>> GetAllSpecKeysAsync()` returning union of all spec keys

##### Part Operations (7 methods)
- **FR-022**: Service MUST provide `Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync()`
- **FR-023**: Service MUST provide `Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId)`
- **FR-024**: Service MUST provide `Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId)`
- **FR-025**: Service MUST provide `Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part)` with PartID validation
- **FR-026**: Service MUST provide `Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part)` with spec validation
- **FR-027**:  Service MUST provide `Task<Model_Dao_Result> DeletePartAsync(string partId)` with transaction check
- **FR-027a**: Service MUST provide `Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId)`

##### Load Operations (6 methods)
- **FR-028**: Service MUST provide `Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads)` with batch validation
- **FR-029**: Service MUST provide `Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end)`
- **FR-029a**: Service MUST provide `Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync()`
- **FR-029b**: Service MUST provide `Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid)`
- **FR-029c**: Service MUST provide `Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load)`
- **FR-029d**: Service MUST provide `Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid)`

##### Inventory Operations (6 methods)
- **FR-030**: Service MUST provide `Task<bool> IsPartInventoriedAsync(string partId)` for quick checks
- **FR-031**: Service MUST provide `Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId)`
- **FR-032**: Service MUST provide `Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync()`
- **FR-033**: Service MUST provide `Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item)`
- **FR-034**: Service MUST provide `Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId)`
- **FR-034a**: Service MUST provide `Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item)`

##### Impact Analysis (4 methods)
- **FR-035**: Service MUST provide `Task<int> GetPartCountByTypeIdAsync(int typeId)` for delete validation
- **FR-036**: Service MUST provide `Task<int> GetTransactionCountByPartIdAsync(string partId)` for delete validation
- **FR-037**: Service MUST provide `Task<int> GetTransactionCountByTypeIdAsync(int typeId)` for delete validation
- **FR-037a**: Service MUST provide `Task<int> GetPartCountBySpecKeyAsync(int typeId, string specKey)` for spec delete validation

### Functional Requirements - CSV Service

#### IService_DunnageCSVWriter / Service_DunnageCSVWriter
- **FR-038**:  Service MUST provide `Task<CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads)`
- **FR-039**: Service MUST write CSV to local path:  `%APPDATA%\MTM_Receiving_Application\DunnageData.csv`
- **FR-040**: Service MUST write CSV to network path: `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv`
- **FR-041**: CSV headers MUST include fixed columns: DunnageType, PartID, Quantity, PONumber, ReceivedDate, Employee
- **FR-042**: CSV headers MUST include all spec keys from union of spec schemas, alphabetically sorted
- **FR-043**: CSV cells MUST escape special characters (commas, quotes, newlines) per RFC 4180
- **FR-044**: Service MUST handle network path failures gracefully (succeed on local, log network error)
- **FR-045**: Service MUST create user subdirectories if they don't exist
- **FR-046**: Service MUST use `GetAllSpecKeysAsync()` from MySQL service to build dynamic columns

### Validation Requirements

- **FR-047**:  Workflow service MUST validate Type is selected before allowing Part selection
- **FR-048**:  Workflow service MUST validate Part is selected before allowing Quantity entry
- **FR-049**:  Workflow service MUST validate Quantity > 0 before allowing Details entry
- **FR-050**:  MySQL service MUST validate PartID uniqueness before inserting parts
- **FR-051**: MySQL service MUST validate JSON spec schemas before inserting/updating specs
- **FR-052**: CSV service MUST validate loads list is not empty before writing

### Error Handling Requirements

- **FR-053**: All services MUST catch exceptions and return user-friendly error messages
- **FR-054**: All services MUST log errors using `ILoggingService`
- **FR-055**: Network CSV write failures MUST NOT fail the entire operation (local succeeds)
- **FR-056**: DAO errors MUST be wrapped with context (e.g., "Failed to delete type 'Pallet':  15 parts depend on it")

## Success Criteria

### Measurable Outcomes - Workflow Service

- **SC-001**:  Workflow can navigate through all steps in sequence without errors
- **SC-002**:  Session data persists correctly across step transitions
- **SC-003**:  `StepChanged` event fires for every step transition
- **SC-004**: Invalid transitions (e.g., skip to Review without data) are blocked with error messages
- **SC-005**: `SaveSessionAsync()` successfully persists loads and triggers CSV export

### Measurable Outcomes - MySQL Service

- **SC-006**: All 34 service methods delegate correctly to corresponding DAO methods
- **SC-007**: Impact analysis methods return accurate counts before delete operations
- **SC-008**:  `GetAllSpecKeysAsync()` returns correct union of spec keys across all types
- **SC-009**: All validation logic executes before DAO calls (fail fast)
- **SC-010**: All errors are logged with sufficient context for debugging

### Measurable Outcomes - CSV Service

- **SC-011**: CSV files are created in both local and network locations (when network available)
- **SC-012**: CSV headers include all fixed columns plus dynamic spec columns
- **SC-013**: CSV data correctly populates all columns with proper escaping
- **SC-014**:  Network failures do not prevent local CSV creation
- **SC-015**:  Multiple users can write CSV simultaneously without file collisions (separate folders)

## Non-Functional Requirements

- **NFR-001**:  Workflow service MUST be registered as singleton (maintains state)
- **NFR-002**:  MySQL service MUST be registered as transient (stateless)
- **NFR-003**:  CSV service MUST be registered as transient (stateless)
- **NFR-004**: All services MUST use constructor injection for dependencies
- **NFR-005**: All service methods MUST complete within 1 second (excluding network I/O)
- **NFR-006**: CSV write operations MUST handle files up to 10MB without memory issues
- **NFR-007**: All public methods MUST have XML documentation comments

## Out of Scope

- ❌ Caching of database queries (direct DAO calls each time)
- ❌ Background CSV export queue (synchronous export only)
- ❌ CSV import functionality (export only)
- ❌ Email notification service (future feature)
- ❌ Custom validation rule engine (simple inline validation)
- ❌ Undo/redo workflow functionality (session clear only)

## Dependencies

- 005-dunnage-stored-procedures (all models and DAOs must exist)
- Project:  `ILoggingService` (for error logging)
- Project: `IService_ErrorHandler` (for user-friendly error messages)
- Project: `IService_UserSessionManager` (for user auditing in database operations)
- NuGet: `CsvHelper` or manual CSV writer (for RFC 4180 compliance)
- NuGet: `System.Text.Json` (for JSON validation)
- Configuration: Network share path from settings
- Configuration: Database connection string

## Files to be Created

### Contracts (`Contracts/Services/`)
- `IService_DunnageWorkflow.cs`
- `IService_MySQL_Dunnage.cs`
- `IService_DunnageCSVWriter.cs`

### Implementations (`Services/Receiving/`)
- `Service_DunnageWorkflow.cs`
- `Service_DunnageCSVWriter.cs`

### Implementations (`Services/Database/`)
- `Service_MySQL_Dunnage.cs`

### Supporting Types
- `WorkflowStep.cs` (enum:  ModeSelection, TypeSelection, PartSelection, QuantityEntry, DetailsEntry, Review)
- `WorkflowStepResult.cs` (class: Success, ErrorMessage)
- `SaveResult.cs` (class: Success, ErrorMessage, RecordsSaved)
- `CSVWriteResult.cs` (class: LocalSuccess, NetworkSuccess, ErrorMessage, FilePath)

## Review & Acceptance Checklist

### Requirement Completeness
- [x] All 3 service interfaces are fully defined with method signatures
- [x] Workflow state machine transitions are clearly specified
- [x] CSV file paths and format are explicitly defined
- [x] Validation rules are enumerated for each service
- [x] Error handling strategy is consistent across all services

### Clarity & Unambiguity
- [x] Dependency injection registration strategy is specified (singleton vs transient)
- [x] Workflow step enum values are defined
- [x] CSV column ordering is explicit (fixed first, then alphabetical specs)
- [x] Event signatures are defined for workflow events
- [x] Impact analysis usage is clear (call before delete operations)

### Testability
- [x] Workflow service can be tested by simulating step transitions
- [x] MySQL service can be tested by mocking DAOs
- [x] CSV service can be tested by verifying file creation and content
- [x] Success criteria are measurable (method calls, event firing, file creation)

### Architecture Alignment
- [x] Follows project DI patterns (constructor injection)
- [x] Follows project error handling patterns (ILoggingService, IService_ErrorHandler)
- [x] Follows project service layer patterns (interface/implementation separation)
- [x] Workflow service similar to existing Service_ReceivingWorkflow

## Clarifications

### Resolved Questions

**Q1**: Should workflow service maintain session state or be stateless?  
**A1**: Maintain state.  Registered as singleton.  Similar to existing `Service_ReceivingWorkflow` pattern.

**Q2**: Should CSV service write to both locations or try network first?  
**A2**:  Write to both.  Local MUST succeed.  Network is best-effort with error logging.

**Q3**:  Should services validate input or rely on DAO validation?  
**A3**: Services validate business rules and fail fast.  DAOs enforce database constraints.  Dual-layer validation.

**Q4**: Should GetAllSpecKeysAsync cache results?   
**A4**: No caching. Specs change infrequently. Query on-demand for accuracy.

**Q5**: Should workflow service auto-advance on save or wait for user?  
**A5**: Wait for user. Workflow provides navigation methods, ViewModels decide when to call them.

**Q6**: Should CSV service create backup files?   
**A6**: No.  Always overwrite `DunnageData.csv`. LabelView reads the latest file. 

# Feature Specification: Architecture Compliance Refactoring

**Feature Branch**: `007-architecture-compliance`  
**Created**: 2025-12-27  
**Status**: Draft  
**Input**: User description: "Comprehensive architecture compliance refactoring to enforce constitutional principles: convert all static DAOs to instance-based with DI, create missing DAOs (ReceivingLoad, PackageTypePreference, InforVisualPO, InforVisualPart), refactor services to delegate to DAOs instead of direct database access, fix ViewModel violations, update DI registration, and create architecture documentation to prevent future violations"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - ViewModel Architecture Compliance (Priority: P1)

**Description**: As a developer maintaining the MTM Receiving Application, I need ViewModels to access data exclusively through Services (not DAOs directly), so that business logic remains properly layered and testable according to MVVM architectural principles defined in Constitution v1.2.0.

**Why this priority**: Critical violations with 1-sprint SLA per constitution Amendment 8. Currently blocks architectural integrity and creates maintenance debt in ReceivingModeSelectionViewModel and ReceivingLabelViewModel.

**Independent Test**: Can be fully tested by verifying that ReceivingModeSelectionViewModel and ReceivingLabelViewModel no longer contain any `using Data.*` statements or DAO instantiation calls, and successfully compile with new service interfaces injected via constructor.

**Acceptance Scenarios**:

1. **Given** ReceivingModeSelectionViewModel needs user preference data, **When** GetLatestUserPreferenceAsync is called, **Then** it delegates to injected IService_UserPreferences instead of directly instantiating Dao_User
2. **Given** ReceivingLabelViewModel needs to insert receiving line data, **When** InsertReceivingLineAsync is called, **Then** it delegates to injected IService_MySQL_ReceivingLine instead of calling Dao_ReceivingLine directly
3. **Given** developer reviews ViewModel code, **When** analyzing dependencies, **Then** dependency graph shows ViewModel → Service → DAO → Database (no ViewModel → DAO shortcuts)
4. **Given** application starts, **When** DI container resolves ViewModels, **Then** all required service interfaces are successfully injected without runtime errors

---

### User Story 2 - Service-to-DAO Delegation Pattern (Priority: P1)

**Description**: As a developer implementing business logic, I need Services to delegate all database operations to DAOs rather than directly executing SQL or stored procedures, so that data access logic is centralized, reusable, and follows the constitutional Pattern A (Service → DAO → Database).

**Why this priority**: Critical violation affecting 3 services with direct database access. Required for Amendment 4 compliance and prevents duplicate database code across the application.

**Independent Test**: Can be fully tested by verifying Service_MySQL_Receiving, Service_MySQL_PackagePreferences, and Service_InforVisual no longer contain MySqlConnection or SqlConnection instantiation, and all database operations route through DAO method calls.

**Acceptance Scenarios**:

1. **Given** Service_MySQL_Receiving needs receiving load data, **When** GetReceivingLoadsAsync is called, **Then** it calls Dao_ReceivingLoad.GetAllAsync() instead of executing sp_get_receiving_loads directly
2. **Given** Service_MySQL_PackagePreferences needs package type preferences, **When** GetPackageTypePreferenceAsync is called, **Then** it delegates to Dao_PackageTypePreference.GetByUserAsync() instead of executing stored procedure directly
3. **Given** Service_InforVisual needs PO validation, **When** ValidatePoNumberAsync is called, **Then** it calls Dao_InforVisualPO.GetByPoNumberAsync() with READ-ONLY connection enforced
4. **Given** developer traces database call, **When** using dependency graph analysis, **Then** execution path shows Service → DAO → Helper_Database_StoredProcedure (no Service → Database shortcuts)

---

### User Story 3 - Instance-Based DAO Pattern (Priority: P2)

**Description**: As a developer creating or maintaining DAOs, I need all DAO classes to use instance-based pattern with constructor injection instead of static methods, so that dependencies are managed through DI container and DAOs are testable with mocking frameworks.

**Why this priority**: Affects 5 static DAOs (Dunnage domain). Required for Amendment 2 compliance and enables proper unit testing with Moq. Blocks future DAO creation until pattern is established.

**Independent Test**: Can be fully tested by converting Dao_DunnageLoad to instance-based, registering it in App.xaml.cs, and verifying Service_MySQL_Dunnage successfully injects and calls it without static method references.

**Acceptance Scenarios**:

1. **Given** Dao_DunnageLoad is converted to instance-based, **When** Service_MySQL_Dunnage constructor executes, **Then** IDaoFactory injects Dao_DunnageLoad instance successfully
2. **Given** developer creates new DAO, **When** following dao-instance-pattern.instructions.md, **Then** DAO class is instance-based with parameterless constructor and registered as Singleton in DI container
3. **Given** unit test for Service_MySQL_Dunnage, **When** mocking IDaoFactory, **Then** Moq can substitute mock DAO instance instead of static class (enables testability)
4. **Given** all 5 Dunnage DAOs converted, **When** application starts, **Then** DI container resolves all DAO dependencies without circular reference errors
5. **Given** static DAO pattern is deprecated, **When** developer runs vscode-csharp-dependency-graph analysis, **Then** no static DAO classes appear in Data/* namespace

---

### User Story 4 - Infor Visual READ-ONLY DAOs (Priority: P2)

**Description**: As a developer integrating with Infor Visual ERP data, I need dedicated READ-ONLY DAOs (Dao_InforVisualPO, Dao_InforVisualPart) that enforce ApplicationIntent=ReadOnly connection string validation, so that accidental write operations to the ERP database are prevented at the architecture layer.

**Why this priority**: Required for Amendment 6 (Principle X). Prevents catastrophic data corruption in production ERP system. Currently Service_InforVisual has no architectural enforcement of READ-ONLY constraint.

**Independent Test**: Can be fully tested by attempting to call a hypothetical Insert/Update/Delete method on Dao_InforVisualPO and verifying it throws InvalidOperationException at compile-time or constructor validation time (before database connection).

**Acceptance Scenarios**:

1. **Given** Dao_InforVisualPO is instantiated, **When** constructor validates connection string, **Then** it throws InvalidOperationException if ApplicationIntent=ReadOnly is missing
2. **Given** developer tries to add UpdatePoStatusAsync method to Dao_InforVisualPO, **When** code review occurs, **Then** constitutional violation is flagged (only SELECT operations allowed)
3. **Given** Service_InforVisual calls Dao_InforVisualPO.GetByPoNumberAsync, **When** executing query, **Then** connection uses Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly
4. **Given** Dao_InforVisualPart queries part master data, **When** SELECT executes, **Then** query succeeds with read-only intent and returns expected Model_InforVisualPart objects
5. **Given** all Infor Visual queries consolidated, **When** reviewing Service_InforVisual, **Then** zero direct SqlConnection instantiation remains (all delegated to DAOs)

---

### User Story 5 - Architecture Documentation & Prevention (Priority: P3)

**Description**: As a developer new to the MTM Receiving Application, I need comprehensive architecture documentation (service-dao-pattern, dao-instance-pattern, dependency-analysis) that explains forbidden patterns with examples, so that I can implement new features without introducing constitutional violations.

**Why this priority**: Prevents future violations through education. Lower priority than fixing current violations but critical for long-term maintainability and team onboarding.

**Independent Test**: Can be fully tested by a new developer following .github/instructions/service-dao-pattern.instructions.md to create a new Service+DAO pair and verifying vscode-csharp-dependency-graph shows compliant architecture.

**Acceptance Scenarios**:

1. **Given** developer reads service-dao-pattern.instructions.md, **When** implementing new feature, **Then** service delegates all database calls to DAO without direct connection usage
2. **Given** developer follows dao-instance-pattern.instructions.md, **When** creating Dao_NewEntity, **Then** DAO is instance-based with DI registration and returns Model_Dao_Result
3. **Given** developer runs dependency-analysis.instructions.md validation, **When** analyzing codebase, **Then** vscode-csharp-dependency-graph shows no ViewModel→DAO or Service→Database violations
4. **Given** architecture-refactoring-guide.instructions.md is created, **When** converting static DAO to instance-based, **Then** step-by-step guide ensures DaoFactory pattern and DI registration are correct
5. **Given** documentation is complete, **When** constitution Amendment 9 is reviewed, **Then** all 4 required documentation files exist with PlantUML diagrams and forbidden pattern examples

---

### Edge Cases

- **What happens when DI container fails to resolve DAO dependency?** Application startup fails with descriptive error message identifying missing registration in App.xaml.cs ConfigureServices
- **How does system handle circular dependency between DAOs?** Constitutional prohibition (Amendment 3) prevents DAO-to-DAO dependencies; if attempted, vscode-csharp-dependency-graph flags violation during development
- **What if developer uses wrong DAO pattern (static vs instance-based) in new code?** Code review checklist (constitution Amendment 7) catches violation before merge; dependency graph analysis fails CI/CD build
- **How does Infor Visual DAO prevent accidental writes after deployment?** Connection string validation in constructor + SQL Server READ-ONLY intent + code review enforcement creates three layers of protection
- **What if service needs data from multiple DAOs?** Service injects multiple DAO interfaces via constructor (composition pattern); DAOs remain independent without cross-dependencies
- **How are DAO integration tests handled after instance-based refactor?** Test project registers DAOs in test DI container with real MySQL connection; unit tests use Moq to substitute mock DAOs

## Requirements *(mandatory)*

### Functional Requirements

#### ViewModel Architecture Compliance (P1)
- **FR-001**: System MUST eliminate all direct DAO access from ReceivingModeSelectionViewModel by creating IService_UserPreferences interface and Service_UserPreferences implementation
- **FR-002**: System MUST eliminate Dao_ReceivingLine call from ReceivingLabelViewModel by creating IService_MySQL_ReceivingLine interface and Service_MySQL_ReceivingLine implementation
- **FR-003**: ViewModels MUST inject service interfaces via constructor parameters registered in App.xaml.cs DI container
- **FR-004**: System MUST verify no `using Data.*` namespace imports exist in ViewModels/* directory after refactoring

#### Service-to-DAO Delegation (P1)
- **FR-005**: Service_MySQL_Receiving MUST delegate all database operations to Dao_ReceivingLoad instance (injected via IDaoFactory or direct constructor injection)
- **FR-006**: Service_MySQL_PackagePreferences MUST delegate all database operations to Dao_PackageTypePreference instance
- **FR-007**: Service_InforVisual MUST delegate all Infor Visual database queries to Dao_InforVisualPO and Dao_InforVisualPart instances
- **FR-008**: Services MUST NOT instantiate MySqlConnection, SqlConnection, or call Helper_Database_StoredProcedure directly (delegated to DAOs only)

#### Instance-Based DAO Pattern (P2)
- **FR-009**: System MUST convert Dao_DunnageLoad, Dao_DunnageType, Dao_DunnagePart, Dao_DunnageSpec, and Dao_InventoriedDunnage from static classes to instance-based classes
- **FR-010**: All DAOs MUST use parameterless constructors that retrieve connection string from Helper_Database_Variables internally
- **FR-011**: All DAOs MUST be registered as Singletons in App.xaml.cs ConfigureServices method
- **FR-012**: System MUST create IDaoFactory interface (or inject DAOs directly into services) to manage DAO instance provisioning
- **FR-013**: All DAO methods MUST return Model_Dao_Result or Model_Dao_Result<T> using DaoResultFactory

#### Infor Visual READ-ONLY DAOs (P2)
- **FR-014**: System MUST create Dao_InforVisualPO with constructor validation ensuring connection string contains ApplicationIntent=ReadOnly
- **FR-015**: System MUST create Dao_InforVisualPart with same READ-ONLY connection string validation
- **FR-016**: Infor Visual DAOs MUST throw InvalidOperationException during instantiation if ApplicationIntent=ReadOnly is missing from connection string
- **FR-017**: Infor Visual DAOs MUST only expose SELECT query methods (GetByPoNumberAsync, GetPartByIdAsync, etc.) - no Insert/Update/Delete methods allowed
- **FR-018**: System MUST create Model_InforVisualPO and Model_InforVisualPart data models matching ERP schema

#### DI Container Registration (P1)
- **FR-019**: App.xaml.cs ConfigureServices MUST register all DAOs as Singletons (services.AddSingleton<Dao_EntityName>())
- **FR-020**: App.xaml.cs MUST register new service interfaces: IService_UserPreferences, IService_MySQL_ReceivingLine as Transient
- **FR-021**: System MUST update ViewModel constructor signatures to inject new service interfaces
- **FR-022**: DI container MUST resolve all DAO and Service dependencies without runtime errors during application startup

#### Architecture Documentation (P3)
- **FR-023**: System MUST create .github/instructions/architecture-refactoring-guide.instructions.md with step-by-step static-to-instance DAO conversion process
- **FR-024**: System MUST create .github/instructions/service-dao-pattern.instructions.md with forbidden Pattern B examples and required Pattern A template
- **FR-025**: System MUST create .github/instructions/dependency-analysis.instructions.md with vscode-csharp-dependency-graph usage and violation detection process
- **FR-026**: System MUST create .github/instructions/dao-instance-pattern.instructions.md with complete DAO creation template and DI registration checklist
- **FR-027**: All documentation files MUST use PlantUML diagrams for architecture visualizations (no ASCII art per markdown-documentation.instructions.md)
- **FR-028**: System MUST update .github/instructions/mvvm-pattern.instructions.md to include ViewModel→Service→DAO flow diagram
- **FR-029**: System MUST update .github/instructions/dao-pattern.instructions.md to deprecate static pattern and mandate instance-based with examples

### Key Entities

- **Dao_ReceivingLoad**: Instance-based DAO for MySQL receiving_loads table operations (GetAllAsync, GetByDateRangeAsync, GetByIdAsync, InsertAsync, UpdateAsync)
- **Dao_PackageTypePreference**: Instance-based DAO for MySQL package_type_preferences table operations (GetByUserAsync, UpsertAsync)
- **Dao_InforVisualPO**: READ-ONLY instance-based DAO for Infor Visual purchase order queries (Server=VISUAL, Database=MTMFG, ApplicationIntent=ReadOnly)
- **Dao_InforVisualPart**: READ-ONLY instance-based DAO for Infor Visual part master queries (Server=VISUAL, Database=MTMFG, ApplicationIntent=ReadOnly)
- **Model_InforVisualPO**: Data model matching Infor Visual PO schema (po_num, po_line, part, qty, due_date, vendor, status)
- **Model_InforVisualPart**: Data model matching Infor Visual part master schema (part_id, description, unit_cost, warehouse, on_hand_qty)
- **IService_UserPreferences**: Service interface for user preference operations (GetLatestUserPreferenceAsync, SaveUserPreferenceAsync)
- **Service_UserPreferences**: Implementation of IService_UserPreferences delegating to Dao_User instance
- **IService_MySQL_ReceivingLine**: Service interface for receiving line operations (InsertReceivingLineAsync, GetReceivingLinesByLoadAsync)
- **Service_MySQL_ReceivingLine**: Implementation of IService_MySQL_ReceivingLine delegating to Dao_ReceivingLine instance
- **IDaoFactory**: Factory interface for provisioning DAO instances (GetDao<T>() or specific GetReceivingLoadDao() methods)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero ViewModel classes contain `using Data.*` namespace imports (verified via grep search across ViewModels/ directory)
- **SC-002**: Zero Service classes instantiate MySqlConnection or SqlConnection objects (verified via grep search across Services/ directory)
- **SC-003**: All 5 Dunnage DAOs (DunnageLoad, DunnageType, DunnagePart, DunnageSpec, InventoriedDunnage) are instance-based and registered in DI container (verified by App.xaml.cs inspection)
- **SC-004**: vscode-csharp-dependency-graph analysis shows zero ViewModel→DAO edges and zero Service→Database edges (only Service→DAO→Database paths exist)
- **SC-005**: Application builds and runs without DI resolution errors (dotnet build && dotnet run succeeds)
- **SC-006**: All unit tests pass after refactoring (dotnet test returns 100% pass rate)
- **SC-007**: Dao_InforVisualPO and Dao_InforVisualPart constructor validation prevents instantiation without ApplicationIntent=ReadOnly (unit test verifies InvalidOperationException thrown)
- **SC-008**: Four new architecture documentation files exist in .github/instructions/ with PlantUML diagrams (verified via file existence check)
- **SC-009**: Constitution Amendment 8 critical violations (ViewModel→DAO) resolved within 1 sprint (2 weeks from spec approval)
- **SC-010**: Code review checklist includes architecture validation step referencing vscode-csharp-dependency-graph analysis (documented in .github/PULL_REQUEST_TEMPLATE.md or equivalent)

## Assumptions *(optional)*

- **Database Connection Management**: Assumes Helper_Database_Variables.GetConnectionString() and Helper_Database_Variables.GetInforVisualConnectionString() provide correct connection strings with appropriate timeout/pooling settings
- **DI Container Lifecycle**: Assumes Singleton DAO registration is appropriate (single instance per application lifetime); if thread-safety issues arise, pattern will be re-evaluated
- **DAO Factory Pattern**: Assumes either IDaoFactory abstraction or direct DAO injection into services is acceptable; implementation plan will determine optimal approach based on service complexity
- **Infor Visual Schema Stability**: Assumes Infor Visual PO and Part table schemas are stable and documented; if schema changes, Model_InforVisualPO and Model_InforVisualPart will require updates
- **Static DAO Migration Risk**: Assumes existing static DAO method signatures can be preserved during instance-based conversion to minimize service refactoring; breaking changes will be minimized
- **Documentation Tooling**: Assumes developers have PlantUML extension installed in VS Code for editing architecture diagrams
- **Test Coverage**: Assumes existing integration tests for Dao_DunnageLoad, Dao_DunnageType, etc. can be adapted to instance-based pattern without full rewrite
- **Performance Impact**: Assumes DI container overhead for DAO resolution is negligible compared to database I/O time; no performance degradation expected from instance-based pattern

## Dependencies *(mandatory)*

- **Constitution v1.2.0**: Spec implements Amendments 1-9 from SYNC IMPACT REPORT (2025-12-27)
- **DaoResultFactory**: Circular dependency fix completed (December 2025) - required for all DAO return types
- **Helper_Database_Variables**: Provides GetConnectionString() and GetInforVisualConnectionString() - must support ApplicationIntent parameter
- **Helper_Database_StoredProcedure**: Provides ExecuteStoredProcedureAsync<T> and ExecuteNonQueryAsync - used by all DAOs
- **App.xaml.cs DI Container**: Microsoft.Extensions.DependencyInjection configured - required for DAO and Service registration
- **vscode-csharp-dependency-graph Extension**: Provides architecture validation tooling - required for verification and ongoing compliance
- **PlantUML Extension**: Required for creating and editing architecture diagrams in documentation files
- **Existing DAOs**: Dao_User, Dao_ReceivingLine, Dao_DunnageLoad, Dao_DunnageType, Dao_DunnagePart, Dao_DunnageSpec, Dao_InventoriedDunnage (will be refactored)
- **Existing Services**: Service_MySQL_Receiving, Service_MySQL_PackagePreferences, Service_InforVisual (will be refactored)
- **Existing ViewModels**: ReceivingModeSelectionViewModel, ReceivingLabelViewModel (will be refactored)

## Out of Scope *(mandatory)*

- **Asynchronous DAO Methods**: All existing DAOs already use async/await pattern; no synchronous-to-async conversion required
- **Database Schema Changes**: No MySQL table or stored procedure modifications; refactoring is code-only
- **Infor Visual Write Operations**: Explicitly forbidden by constitution; no INSERT/UPDATE/DELETE functionality will be implemented
- **DAO Unit Tests**: Existing integration tests will be adapted, but comprehensive unit test suite for all DAO methods is out of scope (future work)
- **Performance Optimization**: No query optimization or caching layer implementation; focus is architecture compliance only
- **Error Handling Refactoring**: Assumes existing IService_ErrorHandler and Model_Dao_Result error handling is sufficient; no error handling pattern changes
- **UI Changes**: Zero UI modifications; refactoring is backend architecture only
- **Breaking API Changes**: DAO method signatures will be preserved where possible to minimize ripple effects on services
- **Third-Party DAO Frameworks**: No Entity Framework, Dapper, or other ORM introduction; maintaining Helper_Database_StoredProcedure pattern
- **CI/CD Pipeline Integration**: Architecture validation via vscode-csharp-dependency-graph is manual during development; automated CI/CD checks are future work

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: Spec enforces ViewModel → Service → DAO → Database layering (Constitution Principle I, Amendment 1)
- [x] **Database Layer**: All database operations delegated to DAO layer with Model_Dao_Result return types (Constitution Principle II, Amendment 2)
- [x] **Dependency Injection**: All DAOs registered as Singletons, all Services registered as Transient (Constitution Principle III, Amendment 3)
- [x] **Error Handling**: Model_Dao_Result pattern preserves existing error handling via IService_ErrorHandler
- [x] **Security & Authentication**: Infor Visual DAOs enforce READ-ONLY constraint via ApplicationIntent validation (Constitution Principle X, Amendment 6)
- [x] **WinUI 3 Practices**: No UI changes; refactoring is backend architecture only
- [x] **Specification-Driven**: Spec is technology-agnostic describing "what" (compliance requirements) not "how" (implementation details)

### Special Constraints

- [x] **Infor Visual Integration**: Dao_InforVisualPO and Dao_InforVisualPart explicitly marked as READ-ONLY with ApplicationIntent=ReadOnly enforcement (FR-014 through FR-017)
- [x] **MySQL 5.7.24 Compatibility**: No stored procedure or schema changes; refactoring uses existing Helper_Database_StoredProcedure pattern
- [x] **Async Operations**: All DAO methods already async; no synchronous database calls exist in codebase

### Notes

**Constitutional Authority**: This specification directly implements Constitution v1.2.0 SYNC IMPACT REPORT (2025-12-27) Follow-Up Items 1-13. All requirements trace to specific constitutional amendments:

- FR-001 through FR-004 implement Amendment 1 (ViewModel layer separation)
- FR-005 through FR-008 implement Amendment 4 (Service→DAO delegation)
- FR-009 through FR-013 implement Amendment 2 (instance-based DAOs)
- FR-014 through FR-018 implement Amendment 6 (Infor Visual READ-ONLY)
- FR-019 through FR-022 implement Amendment 3 (DI registration)
- FR-023 through FR-029 implement Amendment 9 (architecture documentation)

**Critical Violation SLA**: Amendment 8 requires ViewModel→DAO violations (FR-001, FR-002) to be resolved within 1 sprint (2 weeks). Priority P1 user stories address this constraint.

**Validation Mechanism**: Success Criteria SC-004 requires vscode-csharp-dependency-graph analysis to show zero architectural violations, implementing Amendment 7 (Architecture Validation Tools) requirement for ongoing compliance monitoring.

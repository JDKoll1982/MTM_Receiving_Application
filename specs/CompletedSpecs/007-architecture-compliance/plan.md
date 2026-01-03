# Implementation Plan: Architecture Compliance Refactoring

**Branch**: `007-architecture-compliance` | **Date**: 2025-12-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-architecture-compliance/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Comprehensive architecture refactoring to enforce MTM Receiving Application Constitution v1.2.0 principles. Addresses 4 critical violations discovered via dependency analysis:
1. ViewModels directly accessing DAOs (ViewModel→DAO pattern violation)
2. Services with direct database access instead of delegating to DAOs
3. Static DAO pattern preventing DI and testability
4. Missing READ-ONLY enforcement for Infor Visual ERP integration

**Primary Requirements**:
- Convert all static DAOs to instance-based with DI registration
- Create 4 new DAOs: Dao_ReceivingLoad, Dao_PackageTypePreference, Dao_InforVisualPO, Dao_InforVisualPart
- Refactor ViewModels to access data through Services only (eliminate direct DAO calls)
- Refactor Services to delegate database operations to DAOs (eliminate direct MySqlConnection/SqlConnection usage)
- Create comprehensive architecture documentation to prevent future violations

**Technical Approach**: Pattern migration from static/direct-access to instance-based/layered architecture using DaoFactory pattern, with 1-sprint SLA for critical violations per constitution Amendment 8.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: WinUI 3 (Windows App SDK 1.8), CommunityToolkit.Mvvm 8.x, MySql.Data 8.x, Microsoft.Data.SqlClient, Microsoft.Extensions.DependencyInjection  
**Storage**: MySQL 5.7.24 (mtm_receiving_application) + SQL Server 2019 (Infor Visual - READ ONLY)  
**Testing**: xUnit 2.x, Moq 4.x for unit/integration tests  
**Target Platform**: Windows 10/11 (x64), WinUI 3 desktop application  
**Project Type**: Single WinUI 3 project with MVVM architecture (MTM_Receiving_Application.csproj)  
**Performance Goals**: <200ms UI responsiveness for DAO operations, <500ms for service layer business logic  
**Constraints**: 
  - MySQL 5.7.24: No JSON functions, CTEs, window functions, CHECK constraints
  - Infor Visual: ApplicationIntent=ReadOnly MANDATORY, SELECT queries only
  - Constitutional 1-sprint SLA for critical violations (ViewModel→DAO, Service→Database)
**Scale/Scope**: 
  - 5 static DAOs to refactor (Dunnage domain)
  - 4 new instance-based DAOs to create
  - 2 ViewModels with direct DAO access violations
  - 3 Services with direct database access violations
  - ~15 DI registrations to add/update in App.xaml.cs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](.specify/memory/constitution.md) v1.2.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: Plan separates ViewModels, Views, Models, and Services - **Fixes existing violations**
  - ✅ ReceivingModeSelectionViewModel will delegate to IService_UserPreferences (removes Dao_User access)
  - ✅ ReceivingLabelViewModel will delegate to IService_MySQL_ReceivingLine (removes Dao_ReceivingLine access)
  - ✅ All ViewModels will use Service layer exclusively (no DAO imports)
  
- [x] **II. Database Layer**: Plan uses Model_Dao_Result pattern, stored procedures only, async operations - **Enforces instance-based pattern**
  - ✅ All DAOs return Model_Dao_Result<T> using DaoResultFactory
  - ✅ All DAOs will be instance-based with constructor injection
  - ✅ Services delegate to DAOs (Service→DAO→Database layering enforced)
  - ✅ MySQL operations use stored procedures via Helper_Database_StoredProcedure
  - ✅ Infor Visual DAOs use direct SELECT queries with READ-ONLY connection
  
- [x] **III. Dependency Injection**: All services will be registered in DI container with interfaces
  - ✅ All DAOs registered as Singletons in App.xaml.cs
  - ✅ New service interfaces created: IService_UserPreferences, IService_MySQL_ReceivingLine
  - ✅ Services injected into ViewModels via constructor
  - ✅ DAOs injected into Services via constructor (or IDaoFactory)
  
- [x] **IV. Error Handling & Logging**: Plan includes IService_ErrorHandler and ILoggingService usage
  - ✅ ViewModels use _errorHandler.HandleException() for all errors
  - ✅ Services log exceptions before returning failure results
  - ✅ DAOs return Model_Dao_Result.Failure() instead of throwing exceptions
  
- [x] **V. Security & Authentication**: Authentication/authorization approach specified if applicable
  - ⚠️ N/A - Refactoring does not change authentication mechanisms
  
- [x] **VI. WinUI 3 Modern Practices**: UI uses x:Bind, ObservableCollection, async/await patterns
  - ⚠️ N/A - Refactoring does not modify XAML views
  
- [x] **VII. Specification-Driven**: This plan follows Speckit workflow structure
  - ✅ Plan generated via /speckit.plan command
  - ✅ Tasks will be generated via /speckit.tasks command (Phase 2)
  - ✅ Architecture documentation will be created per Amendment 9 requirements

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)
  - ✅ No platform changes required
  
- [x] **Database**: MySQL for app data, SQL Server for Infor Visual (READ ONLY)
  - ✅ MySQL DAOs use MySql.Data 8.x
  - ✅ Infor Visual DAOs use Microsoft.Data.SqlClient with ApplicationIntent=ReadOnly
  
- [x] **MySQL 5.7.24 Compatible**: No JSON functions, CTEs, window functions, CHECK constraints
  - ✅ All stored procedures already MySQL 5.7.24 compatible
  - ✅ No new stored procedures required (using existing sp_get_receiving_loads, etc.)
  
- [x] **Required Packages**: CommunityToolkit.Mvvm, MySql.Data, Microsoft.Data.SqlClient
  - ✅ All packages already referenced in MTM_Receiving_Application.csproj
  - ⚠️ May need Microsoft.Extensions.DependencyInjection.Abstractions for IDaoFactory (verify in Phase 0)

### Critical Constraints

- [x] **Infor Visual READ ONLY**: If feature queries VISUAL/MTMFG database:
  - [x] Connection uses ApplicationIntent=ReadOnly
    - ✅ Dao_InforVisualPO constructor validates connection string contains ApplicationIntent=ReadOnly
    - ✅ Dao_InforVisualPart constructor validates connection string contains ApplicationIntent=ReadOnly
    - ✅ InvalidOperationException thrown if validation fails
  - [x] Only SELECT queries via stored procedures
    - ⚠️ **EXCEPTION**: Infor Visual DAOs use direct SQL SELECT queries (not stored procedures) per constitution allowance
    - ✅ No INSERT/UPDATE/DELETE methods in Dao_InforVisualPO or Dao_InforVisualPart
  - [x] No INSERT, UPDATE, DELETE, or DDL operations
    - ✅ Constitutional prohibition enforced at DAO interface design level
    - ✅ Code review checklist will flag any write operations
  - [x] Graceful handling of connection failures
    - ✅ DAOs return Model_Dao_Result.Failure() on connection errors
    - ✅ Services log connection failures via ILoggingService
    
- [x] **Forbidden Practices**: No direct SQL, no DAO exceptions, no logic in code-behind, no service locator
  - ✅ MySQL DAOs use stored procedures exclusively (no raw SQL in C#)
  - ✅ All DAOs return Model_Dao_Result (no exceptions thrown)
  - ✅ No XAML code-behind changes (refactoring is backend/ViewModel only)
  - ✅ Constructor injection used (no ServiceLocator pattern)

### Justification for Violations

**Infor Visual Direct SQL Queries**: Constitution Amendment 10 (Principle X) explicitly allows direct SQL for Infor Visual READ-ONLY queries because:
- Infor Visual stored procedures require elevated permissions not granted to SHOP2 user
- READ-ONLY connection intent enforced at connection string level (ApplicationIntent=ReadOnly)
- SQL Server enforces read-only intent regardless of query content
- Alternative (stored procedures) rejected due to ERP vendor permissions constraints

**No New XAML Files**: Refactoring focuses on architecture layer (ViewModels/Services/DAOs); existing Views unchanged because:
- ViewModel contracts preserved (public properties/commands remain same signatures)
- Dependency injection changes are transparent to XAML bindings
- UI behavior unchanged - only internal data access mechanism refactored

**Violation Resolution Timeline**: Constitution Amendment 8 mandates 1-sprint (2-week) SLA for critical violations:
- ViewModel→DAO violations (ReceivingModeSelectionViewModel, ReceivingLabelViewModel)
- Service→Database violations (Service_MySQL_Receiving, Service_MySQL_PackagePreferences, Service_InforVisual)
- Plan targets Phase 2 completion within 1 sprint (8-12 workdays estimated)

## Project Structure

### Documentation (this feature)

```text
specs/007-architecture-compliance/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output: DAO pattern research, DaoFactory design decisions
├── data-model.md        # Phase 1 output: Models and DAO method signatures
├── quickstart.md        # Phase 1 output: Developer guide for implementing refactoring
├── contracts/           # Phase 1 output: Service interfaces and DAO contracts
│   ├── IService_UserPreferences.md
│   ├── IService_MySQL_ReceivingLine.md
│   ├── IDaoFactory.md (if factory pattern chosen)
│   ├── Dao_ReceivingLoad.md (method contracts)
│   ├── Dao_PackageTypePreference.md (method contracts)
│   ├── Dao_InforVisualPO.md (method contracts)
│   └── Dao_InforVisualPart.md (method contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
MTM_Receiving_Application/
├── ViewModels/
│   ├── Shared/
│   │   └── BaseViewModel.cs (unchanged)
│   └── Receiving/
│       ├── ReceivingModeSelectionViewModel.cs (MODIFIED - remove Dao_User, add IService_UserPreferences)
│       └── ReceivingLabelViewModel.cs (MODIFIED - remove Dao_ReceivingLine, add IService_MySQL_ReceivingLine)
│
├── Services/
│   ├── Receiving/
│   │   ├── Service_MySQL_Receiving.cs (MODIFIED - delegate to Dao_ReceivingLoad instead of direct DB)
│   │   └── Service_MySQL_ReceivingLine.cs (NEW - wraps Dao_ReceivingLine for ViewModel usage)
│   ├── Database/
│   │   ├── Service_MySQL_PackagePreferences.cs (MODIFIED - delegate to Dao_PackageTypePreference)
│   │   ├── Service_InforVisual.cs (MODIFIED - delegate to Dao_InforVisualPO/Part)
│   │   └── Service_UserPreferences.cs (NEW - wraps Dao_User for ViewModel usage)
│   └── DaoFactory.cs (NEW - if factory pattern chosen in Phase 0 research)
│
├── Contracts/Services/
│   ├── IService_UserPreferences.cs (NEW)
│   ├── IService_MySQL_ReceivingLine.cs (NEW)
│   └── IDaoFactory.cs (NEW - if factory pattern chosen)
│
├── Data/
│   ├── Receiving/
│   │   ├── Dao_ReceivingLoad.cs (NEW - instance-based)
│   │   ├── Dao_ReceivingLine.cs (MODIFIED if static, otherwise reference only)
│   │   └── Dao_PackageTypePreference.cs (NEW - instance-based)
│   ├── Dunnage/
│   │   ├── Dao_DunnageLoad.cs (MODIFIED - static → instance-based)
│   │   ├── Dao_DunnageType.cs (MODIFIED - static → instance-based)
│   │   ├── Dao_DunnagePart.cs (MODIFIED - static → instance-based)
│   │   ├── Dao_DunnageSpec.cs (MODIFIED - static → instance-based)
│   │   └── Dao_InventoriedDunnage.cs (MODIFIED - static → instance-based)
│   └── InforVisual/
│       ├── Dao_InforVisualPO.cs (NEW - instance-based, READ-ONLY)
│       └── Dao_InforVisualPart.cs (NEW - instance-based, READ-ONLY)
│
├── Models/
│   ├── Receiving/
│   │   ├── Model_ReceivingLoad.cs (unchanged - already exists)
│   │   └── Model_PackageTypePreference.cs (NEW or verify exists)
│   └── InforVisual/
│       ├── Model_InforVisualPO.cs (NEW)
│       └── Model_InforVisualPart.cs (NEW)
│
├── .github/instructions/
│   ├── architecture-refactoring-guide.instructions.md (NEW - static to instance DAO conversion)
│   ├── service-dao-pattern.instructions.md (NEW - forbidden Pattern B vs required Pattern A)
│   ├── dependency-analysis.instructions.md (NEW - vscode-csharp-dependency-graph usage)
│   ├── dao-instance-pattern.instructions.md (NEW - DAO creation template with DI)
│   ├── mvvm-pattern.instructions.md (MODIFIED - add ViewModel→Service→DAO flow diagram)
│   └── dao-pattern.instructions.md (MODIFIED - deprecate static, mandate instance-based)
│
├── App.xaml.cs (MODIFIED - 15+ DI registrations added)
```

**Structure Decision**: Single WinUI 3 project architecture is preserved. Refactoring focuses on backend architecture (ViewModels/Services/DAOs) without changing project structure or introducing new assemblies. All changes are within existing MTM_Receiving_Application.csproj.

## Complexity Tracking

> **No violations requiring justification - Constitution Check passed**

All architecture changes align with constitutional principles. The refactoring specifically **fixes existing violations** rather than introducing new ones:

| Previous Violation | Fix Applied | Constitution Principle |
|--------------------|-------------|------------------------|
| ViewModel→DAO direct access | ViewModel→Service→DAO pattern | Principle I (MVVM), Amendment 8 |
| Service→Database direct access | Service→DAO→Database delegation | Principle II (Database Layer) |
| Static DAO pattern | Instance-based DAOs with DI | Principle II, III (DI Everywhere) |
| Missing Infor Visual READ-ONLY enforcement | Constructor validation + DAO design constraints | Principle X (Infor Visual DAO Architecture) |

**Complexity Justification**: This refactoring intentionally increases short-term implementation complexity (converting static→instance-based, creating service wrappers) to achieve long-term architectural simplicity:
- **Testability**: Instance-based DAOs enable Moq mocking in unit tests (static classes cannot be mocked)
- **Maintainability**: Service→DAO→Database layering centralizes database logic (no duplication across services)
- **Constitutional Compliance**: Explicit enforcement of architectural principles prevents future violations
- **Type Safety**: Dependency injection provides compile-time verification of dependencies

---

## Constitution Check Re-Evaluation (Post-Design)

**Status**: ✅ **PASSED** - All Phase 1 design artifacts conform to constitutional principles

### Design Artifacts Review

**research.md** (Phase 0):
- ✅ Direct DAO injection pattern chosen over factory abstraction (aligns with Principle III - explicit dependencies)
- ✅ SqlConnectionStringBuilder validation chosen for Infor Visual READ-ONLY enforcement (aligns with Principle X)
- ✅ Constructor parameter injection chosen for DAOs (aligns with Principle III - testability)
- ✅ Incremental migration strategy with backward compatibility (aligns with Constitutional SLA requirements)
- ✅ Thin DAOs / Rich Services pattern (aligns with Principle I, II - layer separation)
- ✅ No new package dependencies required (all existing packages sufficient)

**data-model.md** (Phase 1):
- ✅ Model_InforVisualPO and Model_InforVisualPart use ObservableObject pattern (aligns with Principle I - MVVM)
- ✅ All new DAOs return Model_Dao_Result<T> (aligns with Principle II - Database Layer Consistency)
- ✅ All new DAOs are instance-based with constructor injection (aligns with Principle II, III)
- ✅ Infor Visual DAOs documented as READ-ONLY with constitutional enforcement (aligns with Principle X)
- ✅ PlantUML dependency diagram shows proper ViewModel→Service→DAO→Database layering (no violations)
- ✅ Service interfaces created: IService_UserPreferences, IService_MySQL_ReceivingLine (aligns with Principle III)

**contracts/** (Phase 1):
- ✅ All contracts use .md files (not .cs files) to avoid compilation issues in specs folder
- ✅ Service contracts delegate to DAOs (IService_UserPreferences → Dao_User)
- ✅ DAO contracts enforce Model_Dao_Result pattern (Dao_ReceivingLoad.md, Dao_PackageTypePreference.md)
- ✅ Infor Visual DAO contracts include constructor validation examples (Dao_InforVisualPO.md, Dao_InforVisualPart.md)
- ✅ All contracts prohibit forbidden patterns (no Insert/Update/Delete in Infor Visual DAOs)
- ✅ Error handling pattern documented: try-catch with DaoResultFactory.Failure() (no exceptions thrown)

**quickstart.md** (Phase 1):
- ✅ Developer guide follows constitutional principles at each step
- ✅ Incremental migration process preserves working codebase (aligns with SLA requirements)
- ✅ Verification steps include dependency analysis to confirm no ViewModel→DAO edges
- ✅ Checklist includes constitutional compliance verification
- ✅ Troubleshooting section addresses common DI and connection string issues

### Principle-by-Principle Verification

**Principle I (MVVM Architecture)**:
- ✅ Design eliminates ViewModel→DAO violations (ReceivingModeSelectionViewModel, ReceivingLabelViewModel)
- ✅ ViewModels inject IService_UserPreferences, IService_MySQL_ReceivingLine via constructor
- ✅ No business logic in Views (XAML unchanged - refactoring is backend only)
- ✅ PlantUML diagram confirms ViewModel→Service→DAO flow

**Principle II (Database Layer Consistency)**:
- ✅ All DAOs return Model_Dao_Result<T> using DaoResultFactory (no self-referencing static methods)
- ✅ All DAOs are instance-based classes (not static)
- ✅ All DAOs registered in DI container as Singletons
- ✅ Services delegate to DAOs (no direct MySqlConnection/SqlConnection in services)
- ✅ MySQL DAOs use stored procedures via Helper_Database_StoredProcedure
- ✅ Infor Visual DAOs use direct SQL SELECT (constitutionally allowed for READ-ONLY)

**Principle III (Dependency Injection Everywhere)**:
- ✅ All DAOs receive connection string via constructor parameter (not internal static access)
- ✅ All Services inject DAOs via constructor (direct injection, not factory abstraction)
- ✅ All ViewModels inject Services via constructor
- ✅ App.xaml.cs registration pattern documented with ~15 new registrations
- ✅ Service interfaces created for all new services (IService_UserPreferences, IService_MySQL_ReceivingLine)

**Principle IV (Error Handling & Logging)**:
- ✅ Services use IService_ErrorHandler and ILoggingService (injected via constructor)
- ✅ DAOs return Model_Dao_Result.Failure() instead of throwing exceptions
- ✅ Service layer logs all database operations for audit trail

**Principle V (Security & Authentication)**:
- ⚠️ N/A - Refactoring does not modify authentication mechanisms

**Principle VI (WinUI 3 Modern Practices)**:
- ⚠️ N/A - Refactoring does not modify XAML views (backend architecture only)

**Principle VII (Specification-Driven Development)**:
- ✅ Plan follows Speckit workflow structure (plan.md, research.md, data-model.md, contracts/, quickstart.md)
- ✅ All Phase 0 and Phase 1 deliverables completed

**Principle VIII (Testing & Quality Assurance)**:
- ✅ Unit test patterns documented in contract files
- ✅ Integration test approach defined (real DAO instances with test database)
- ✅ Mockability enabled by instance-based DAOs (Moq can substitute DAO implementations)

**Principle IX (Code Quality & Maintainability)**:
- ✅ Naming conventions followed (PascalCase, _camelCase for private fields)
- ✅ PlantUML diagrams used for architecture visualization (not ASCII art)
- ✅ Code examples in quickstart.md follow constitutional patterns

**Principle X (Infor Visual DAO Architecture - READ-ONLY)**:
- ✅ Dao_InforVisualPO and Dao_InforVisualPart enforce ApplicationIntent=ReadOnly via constructor validation
- ✅ SqlConnectionStringBuilder used for robust connection string parsing
- ✅ InvalidOperationException thrown if READ-ONLY intent missing (fails fast at instantiation)
- ✅ No Insert/Update/Delete methods in Infor Visual DAO contracts
- ✅ Constitutional violation message included in exception text

### Critical Constraints Verification

**Infor Visual READ-ONLY Enforcement**:
- ✅ Connection string validation uses SqlConnectionStringBuilder (robust parsing)
- ✅ ApplicationIntent enum comparison (type-safe, not string matching)
- ✅ Constructor validation (fails before any queries execute)
- ✅ Double-check enforcement: constructor validation + SQL Server READ-ONLY mode
- ✅ Unit tests verify InvalidOperationException thrown for non-readonly connections

**Forbidden Practices Elimination**:
- ✅ No direct SQL in MySQL DAOs (all use stored procedures)
- ✅ No exceptions thrown from DAOs (all return Model_Dao_Result.Failure())
- ✅ No business logic in Views (XAML unchanged)
- ✅ No service locator pattern (constructor injection only)
- ✅ No ViewModel→DAO dependencies (all routed through Service layer)

### Risks & Mitigation

**Risk**: Static DAO migration breaks existing integration tests  
**Mitigation**: Incremental migration with backward compatibility wrappers; verify tests after each DAO conversion

**Risk**: DI resolution errors at runtime  
**Mitigation**: App.xaml.cs registration checklist in quickstart.md; verify application starts after each registration batch

**Risk**: Connection string validation too strict for Infor Visual  
**Mitigation**: SqlConnectionStringBuilder handles all valid syntax variations; Helper_Database_Variables already includes ApplicationIntent=ReadOnly

**Risk**: Performance degradation from DI overhead  
**Mitigation**: DAO instances are Singletons (no per-request instantiation); DI resolution overhead negligible vs database I/O time

### Conclusion

**Constitution Check Re-Evaluation: ✅ PASSED**

All Phase 1 design artifacts align with constitutional principles. Design is ready for Phase 2 implementation (task breakdown via `/speckit.tasks` command).

**Key Compliance Points**:
1. ViewModel→Service→DAO→Database layering enforced (no shortcuts)
2. Instance-based DAOs with DI enable testability and consistent patterns
3. Infor Visual READ-ONLY constraint enforced at constructor level
4. Service layer provides business logic; DAOs provide data access only
5. No new constitutional violations introduced; existing violations eliminated

**Next Command**: `/speckit.tasks` to generate task breakdown for implementation

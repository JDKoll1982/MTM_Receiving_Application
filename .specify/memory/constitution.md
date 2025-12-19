<!--
CONSTITUTION SYNC IMPACT REPORT
Generated: 2025-12-18

VERSION CHANGE: 1.1.0 → 1.2.0 (MINOR - Added Modular Architecture Principle)

PRINCIPLES ADDED IN v1.2.0:
✅ X. Modular Architecture (MANDATORY)

PRINCIPLES FROM v1.1.0:
✅ VIII. Testing & Quality Assurance
✅ IX. Code Quality & Maintainability

PRINCIPLES FROM v1.0.0:
✅ I. MVVM Architecture (NON-NEGOTIABLE)
✅ II. Database Layer Consistency
✅ III. Dependency Injection Everywhere
✅ IV. Error Handling & Logging
✅ V. Security & Authentication
✅ VI. WinUI 3 Modern Practices
✅ VII. Specification-Driven Development

SECTIONS ADDED:
✅ Technology Constraints
✅ Development Workflow
✅ Infor Visual Integration Rules
✅ Governance

TEMPLATES REQUIRING UPDATES:
✅ .github/agents/copilot-instructions.md - Already updated with Infor Visual constraints
✅ .github/instructions/*.instructions.md - Existing, aligned with principles
✅ .specify/templates/*.md - To be validated against constitution principles
⚠️ README.md - May need updates to reference constitution

FOLLOW-UP ITEMS:
✅ Review spec-template.md for constitution compliance sections - COMPLETE
✅ Review plan-template.md for principle alignment checks - COMPLETE
✅ Review tasks-template.md for quality gate references - COMPLETE
✅ Update README.md with link to constitution - COMPLETE
✅ Add constitution review to PR checklist - COMPLETE

ALL FOLLOW-UP ITEMS COMPLETED: 2025-12-17

RATIONALE FOR VERSION 1.0.0:
- Initial ratification of project constitution
- Captures existing practices from codebase analysis
- Documents critical Infor Visual READ ONLY constraint
- Establishes MVVM, DI, and error handling as non-negotiable
- Formalizes Speckit workflow integration

RATIONALE FOR VERSION 1.1.0 (2025-12-18):
- Formalized testing standards (xUnit, Moq, coverage requirements)
- Elevated code quality practices to constitutional principles
- Documented performance and UI responsiveness mandates
- Codified naming conventions as non-negotiable standards
- Integrated performance-and-stability.instructions.md guidance

RATIONALE FOR VERSION 1.2.0 (2025-12-18):
- Elevated modular architecture to constitutional principle (MANDATORY)
- Documented workflow modularization patterns from Phase 1 implementation
- Established BaseStepViewModel<T> pattern as standard for workflows
- Mandated validator extraction and independent testing
- Prohibited hardcoded switch statements and duplicated navigation logic
- Codified Open/Closed Principle for feature extensibility

STAKEHOLDER ALIGNMENT:
- Based on existing code patterns in codebase
- Incorporates documentation from Documentation/ folder
- Aligns with .github/instructions/ guidance files
- Reflects REUSABLE_SERVICES.md service patterns
- Enforces Infor Visual constraints per recent updates
-->

# MTM Receiving Application Constitution

## Core Principles

### I. MVVM Architecture (NON-NEGOTIABLE)

**Separation of Concerns**:
- ViewModels contain ALL business logic, data binding, and commands
- Views contain ONLY UI markup (XAML) and minimal UI-specific code-behind
- Models are pure data classes with INotifyPropertyChanged support
- Services encapsulate cross-cutting concerns and external dependencies

**Implementation Requirements**:
- ALL ViewModels MUST inherit from `BaseViewModel` or use `ObservableObject`
- ALL data binding MUST use `x:Bind` (compile-time) over `Binding` (runtime)
- ALL ViewModels MUST be registered in DI container (`App.xaml.cs`)
- CommunityToolkit.Mvvm MUST be used: `[ObservableProperty]`, `[RelayCommand]`, `partial` classes
- NO business logic in `.xaml.cs` code-behind files (UI event handlers only)

**Rationale**: MVVM ensures testability, maintainability, and clear separation between UI and logic, critical for a production WinUI 3 application.

---

### II. Database Layer Consistency

**Model_Dao_Result Pattern (MANDATORY)**:
- ALL DAO methods MUST return `Model_Dao_Result<T>` or `Model_Dao_Result`
- NO exceptions thrown from DAO layer - return `Model_Dao_Result.Failure()` instead
- Check `result.IsSuccess` before accessing `result.Data`
- Store exceptions in `result.Exception` for logging

**Stored Procedures vs Direct SQL**:
- **MySQL (mtm_receiving_application)**: NO direct SQL in C# code - ALL operations via stored procedures.
- **Infor Visual (SQL Server)**: Direct SQL queries ARE ALLOWED (and preferred) for READ ONLY operations.
- `Helper_Database_StoredProcedure` is the ONLY way to execute MySQL procedures.
- Parameter names in C# match stored procedure parameters (WITHOUT `p_` prefix - added automatically)
- ALL DAO methods MUST be async (`Task<Model_Dao_Result<T>>`)

**Multi-Database Support**:
- MySQL for application database (MTM_Receiving_Database)
- **SQL Server for Infor Visual database (STRICTLY READ ONLY - NO WRITES EVER)**
- Separate helpers: `Helper_Database_StoredProcedure` (MySQL)
- Connection details:
  - Infor Visual: Server=VISUAL, Database=MTMFG, Warehouse=002, User=SHOP2, Password=SHOP
  - `ApplicationIntent=ReadOnly` REQUIRED for Infor Visual connections

**Rationale**: Consistent error handling and transaction management across all database operations. Infor Visual read-only constraint prevents accidental data corruption in production ERP system.

---

### III. Dependency Injection Everywhere

**Service Registration**:
- ALL services MUST be registered in `App.xaml.cs` `ConfigureServices()` method
- Core services as Singletons: `ILoggingService`, `IService_ErrorHandler`
- ViewModels as Transient (new instance per navigation)
- Views resolve ViewModels via constructor injection

**Constructor Injection Only**:
- NO service locator pattern
- NO static service access (except backward-compatible wrappers)
- Services receive dependencies via constructor parameters
- Private fields for injected services: `private readonly IService _service;`

**Service Interfaces**:
- ALL services MUST have an interface (e.g., `IService_Authentication`, `ILoggingService`)
- Interface defines contract, implementation provides behavior
- Enables testing with mocks and future implementations

**Rationale**: DI enables loose coupling, testability, and runtime service replacement without code changes.

---

### IV. Error Handling & Logging

**Centralized Error Handler**:
- `IService_ErrorHandler` MUST be used for ALL error displays and exception handling
- Method signature: `HandleException(Exception ex, Enum_ErrorSeverity severity, string callerName, string controlName)`
- Severity levels: Low, Medium, High, Critical, Fatal
- User-friendly error dialogs (WinUI 3 ContentDialog)
- Automatic logging of all errors

**Logging Service**:
- `ILoggingService` MUST be used for ALL logging
- CSV-based logging with separate files:
  - `ApplicationLog.csv` - Normal application events
  - `DatabaseErrorLog.csv` - Database errors only
  - `ApplicationErrorLog.csv` - Application exceptions
- Async initialization with background queue processing
- Log rotation and archive management

**No Silent Failures**:
- ALL errors MUST be logged (even if not shown to user)
- Database failures MUST return `Model_Dao_Result.Failure()` with error message
- UI operations MUST show user feedback (success or error)

**Rationale**: Comprehensive error tracking and user experience. CSV logs enable easy troubleshooting without database dependencies.

---

### V. Security & Authentication

**Multi-Tier Authentication**:
- Personal workstations: Automatic Windows username login (Environment.UserName)
- Shared terminals: Username + 4-digit PIN authentication
- Session timeouts: 30 minutes (personal), 15 minutes (shared)
- Activity tracking: Mouse, keyboard, window activation

**Security Requirements**:
- Failed login attempts logged to `user_activity_log` table
- 3-attempt lockout on shared terminals (5-second delay)
- PIN uniqueness enforced in database
- Infor Visual database: **READ ONLY ACCESS ONLY** - enforced at connection level

**Audit Trail**:
- ALL user actions logged with employee number and timestamp
- Session start/end events tracked
- Automatic session termination on timeout
- Graceful logout with audit entry

**Rationale**: Protects sensitive manufacturing data while enabling convenient access based on workstation type. Audit trail ensures accountability.

---

### VI. WinUI 3 Modern Practices

**UI Framework**:
- Windows App SDK 1.8+ (WinUI 3)
- .NET 8.0 target framework
- CommunityToolkit.WinUI.UI.Controls for enhanced controls (DataGrid, etc.)
- NavigationView for main application navigation

**Data Binding**:
- `x:Bind` over `{Binding}` - compile-time validation and performance
- ObservableCollection for dynamic lists
- INotifyPropertyChanged via CommunityToolkit.Mvvm attributes
- Two-way binding where user input is required

**Async/Await Everywhere**:
- ALL I/O operations MUST be async (database, file, network)
- Use `ConfigureAwait(false)` in library code
- UI operations on Dispatcher thread when needed
- `IsBusy` property pattern for loading states

**Rationale**: Modern WinUI 3 best practices for performance, maintainability, and user experience.

---

### VII. Specification-Driven Development

**Speckit Workflow**:
- Features start with specification (`/speckit.specify`)
- Planning phase defines technical approach (`/speckit.plan`)
- Tasks generated with dependencies (`/speckit.tasks`)
- Implementation follows tasks sequentially
- Checklists validate completeness (`/speckit.checklist`)

**Documentation Requirements**:
- Specification must be technology-agnostic (no implementation details)
- Plan must include research decisions with rationale
- Tasks must be specific, testable, and include file paths
- Contracts define service interfaces before implementation
- Data models documented with relationships

**Feature Structure**:
```
specs/
  001-feature-name/
    spec.md               # User-facing requirements
    plan.md               # Technical approach
    tasks.md              # Implementation tasks
    research.md           # Technical decisions
    data-model.md         # Database schema
    contracts/            # Service interfaces
    checklists/           # Validation
```

**Rationale**: Structured approach prevents scope creep, ensures stakeholder alignment, and provides clear implementation roadmap.

---

### VIII. Testing & Quality Assurance

**Testing Framework Standards**:
- xUnit as the primary testing framework
- Moq for service mocking and isolation
- Separate test project: `MTM_Receiving_Application.Tests`
- Test organization: `Unit/` and `Integration/` folders
- ALL service logic MUST have unit tests
- ALL database operations MUST have integration tests

**Test Coverage Requirements**:
- ViewModels: Test command logic, property changes, validation
- Services: Test business logic, error handling, edge cases
- DAOs: Integration tests with test database (NOT production)
- Minimum coverage: 80% for service and ViewModel layers
- Critical paths: 100% coverage (authentication, data persistence)

**Test-Driven Development (TDD)**:
- Write tests BEFORE implementation for new features
- Ensure tests FAIL before implementing functionality
- Red-Green-Refactor cycle for complex logic
- Contract tests for service interfaces
- Integration tests for cross-service interactions

**Testing Best Practices**:
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Arrange-Act-Assert pattern for test structure
- One assertion focus per test (avoid multiple concerns)
- Mock external dependencies (database, file system, network)
- Test data must NOT affect production Infor Visual database
- Clean up test resources in Dispose/teardown methods

**Manual Testing Requirements**:
- UI testing for all user-facing features
- Acceptance criteria validation from spec.md
- Cross-platform testing (x64, ARM64 when applicable)
- Accessibility testing (keyboard navigation, screen readers)
- Performance testing for database-heavy operations (<500ms target)

**Rationale**: Comprehensive testing prevents regressions, validates requirements, and enables confident refactoring. Test-first approach ensures code is inherently testable and meets specifications.

---

### IX. Code Quality & Maintainability

**Naming Conventions (MANDATORY)**:
- **Classes**: PascalCase with prefixes
  - ViewModels: `{Feature}ViewModel` (e.g., `ReceivingWorkflowViewModel`)
  - Services: `Service_{Feature}` with interface `IService_{Feature}`
  - DAOs: `Dao_{EntityName}` (static or instance based on pattern)
  - Models: `Model_{EntityName}`
  - Enums: `Enum_{Name}`
- **Methods**: PascalCase, async methods end with `Async`
- **Properties**: PascalCase for public, `_camelCase` for private fields
- **Parameters**: camelCase
- **Constants**: PascalCase in static classes (NOT UPPER_SNAKE_CASE)

**File Organization**:
- Match namespace to folder structure
- One public class per file (nested classes allowed)
- File name matches primary class name
- Use file-scoped namespaces (preferred)
- Group related classes in feature folders

**Performance & UI Responsiveness (NON-NEGOTIABLE)**:
- **Never block the UI thread** - all I/O operations MUST be async
- File operations: `await File.WriteAllTextAsync()`, NOT `File.WriteAllText()`
- Network operations: Wrap in `Task.Run()` or use native async APIs
- Database operations: ALL DAO methods MUST be async
- Immediate UI feedback: Update status BEFORE awaiting long operations
- Use `DispatcherQueue.TryEnqueue()` for UI updates from background threads
- Target: Database operations <500ms, UI interactions <100ms response

**Logging Standards**:
- Non-blocking logging with background queue processing
- Logging failures MUST NOT crash the application
- Use try-catch around logging operations
- Structured log entries with timestamp, severity, context
- CSV format for easy troubleshooting without database dependencies

**Code Documentation**:
- XML comments for ALL public APIs (classes, methods, properties)
- Inline comments for complex logic or non-obvious decisions
- README.md updates for user-facing feature changes
- Update REUSABLE_SERVICES.md when adding new service patterns
- Document breaking changes in CHANGELOG.md

**Build Optimization**:
- Disable expensive build steps in Debug configuration
- Use `EnableMsixTooling=false` for Debug builds during development
- Fast iteration: Debug builds should complete in <30 seconds
- Release builds may include full packaging and signing

**Data Integrity**:
- Validate data lengths against database schema BEFORE insert
- Sanitize input in service layer (ViewModel → Service → DAO)
- Match C# model properties to database column types
- Default values in models MUST match database defaults
- Truncate or reject data exceeding VARCHAR limits with clear error messages

**Rationale**: Consistent code quality enables team productivity, reduces bugs, and ensures maintainability. Performance standards prevent UI freezing and poor user experience. Naming conventions reduce cognitive load and enable predictable code navigation.

---

### X. Modular Architecture (MANDATORY)

**Modularity Principle**:
- ALL features, workflows, and components MUST be developed with modularity as a core design constraint
- Code MUST be structured to minimize coupling and maximize reusability
- New features MUST NOT require modifications to existing code (Open/Closed Principle)
- Components MUST be independently testable and replaceable

**Implementation Requirements**:
- **Base Classes**: Use generic base classes (e.g., `BaseStepViewModel<TStepData>`) to eliminate code duplication
- **Data Contracts**: Define explicit DTOs for data transfer between components (e.g., step data classes)
- **Validators**: Extract validation logic into composable, independently testable validators implementing `IStepValidator<T>`
- **Service Interfaces**: ALL services MUST be interface-based for dependency injection and testability
- **Plugin Architecture**: Design workflows and features to support runtime configuration and extension

**Workflow Modularization Standards**:
- Workflow steps MUST be self-contained with clear input/output contracts
- Step ViewModels MUST inherit from `BaseStepViewModel<TStepData>`
- Step data MUST be encapsulated in strongly-typed DTO classes
- Step validation MUST be extracted into independent validator classes
- Navigation logic MUST be centralized, NOT scattered across ViewModels
- Lifecycle hooks (OnNavigatedTo, OnNavigatedFrom) MUST be used for state management

**Prohibited Patterns**:
- ❌ Hardcoded switch statements for workflow transitions (use engine-based navigation)
- ❌ Duplicated navigation/event subscription logic across ViewModels
- ❌ Tightly coupled validation logic in ViewModels or services
- ❌ Direct dependencies between workflow steps
- ❌ Global mutable state (use immutable context objects)

**Modularity Benefits**:
- **Reduced Duplication**: Base classes eliminate ~40% boilerplate code
- **Improved Testability**: Independent validators enable focused unit testing
- **Better Maintainability**: Navigation changes in one place, not scattered across files
- **Easy Extension**: Add new steps/features without modifying existing code
- **Team Productivity**: Consistent patterns reduce learning curve for new developers

**Enforcement**:
- Code reviews MUST verify modularity compliance
- Pull requests MUST demonstrate reduced coupling
- New workflow steps MUST use `BaseStepViewModel<T>` pattern
- Validators MUST be registered in DI container and independently tested
- Architecture decisions MUST document modularity considerations

**Examples of Modular Design**:
```csharp
// ✅ CORRECT: Modular step ViewModel with explicit contract
public partial class LoadEntryViewModel : BaseStepViewModel<LoadEntryData>
{
    protected override WorkflowStep ThisStep => WorkflowStep.LoadEntry;
    protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync() { ... }
}

// ❌ WRONG: Tightly coupled ViewModel with manual navigation
public partial class LoadEntryViewModel : BaseViewModel
{
    private void OnStepChanged(object? sender, EventArgs e) { ... } // Boilerplate
    [RelayCommand]
    private async Task ValidateAndContinue() { ... } // Duplicate logic
}
```

**Rationale**: Modular architecture is essential for long-term maintainability, team scalability, and feature extensibility. It prevents technical debt accumulation and enables rapid feature development without risk of breaking existing functionality. The receiving workflow modularization (Phase 1) demonstrates the concrete benefits: 131 lines eliminated, 32 independent unit tests added, and architecture prepared for future enhancements.

---

## Technology Constraints

### Platform & Framework
- **OS**: Windows 10 1809+ / Windows 11
- **Framework**: .NET 8.0
- **UI**: WinUI 3 (Windows App SDK 1.8+)
- **Architecture**: MVVM with CommunityToolkit.Mvvm

### Databases
- **Application Database**: MySQL 8.0+ (MTM_Receiving_Database)
- **ERP Database**: SQL Server (Infor Visual MTMFG) - **READ ONLY**
- **MySQL Version Constraint**: MySQL 5.7.24+ compatibility required
  - NO JSON functions, CTEs, window functions, or CHECK constraints
  - Use temporary tables and subqueries instead

### Key NuGet Packages
- CommunityToolkit.Mvvm (8.2+)
- CommunityToolkit.WinUI.UI.Controls (7.1+)
- Microsoft.Extensions.DependencyInjection (8.0+)
- Microsoft.Extensions.Hosting (8.0+)
- MySql.Data (9.0+)
- Microsoft.Data.SqlClient (5.2+)
- CsvHelper (33.0+)

### Forbidden Practices
- ❌ Direct SQL queries in C# code
- ❌ Exceptions thrown from DAO layer
- ❌ Business logic in Views or code-behind
- ❌ Service locator pattern
- ❌ Static service access (new code)
- ❌ `{Binding}` over `x:Bind`
- ❌ **ANY writes to Infor Visual database**
- ❌ MySQL 8.0+ exclusive features

---

## Development Workflow

### Before Starting Work
1. Read `.github/instructions/` files relevant to your work area
2. Review specification and plan documents
3. Check feature tasks.md for dependencies
4. Ensure database schema is up to date
5. Verify all required services are registered in DI container

### During Implementation
1. Follow task checklist sequentially
2. Mark tasks complete in tasks.md as finished
3. Use existing service patterns (refer to REUSABLE_SERVICES.md)
4. Test locally with development database
5. Update documentation if behavior changes

### Quality Gates
- ✅ All compilation errors resolved
- ✅ MVVM pattern adhered to (no logic in code-behind)
- ✅ Services registered in DI container
- ✅ Error handling uses IService_ErrorHandler
- ✅ Database operations use Model_Dao_Result pattern
- ✅ Async/await used for I/O operations
- ✅ No writes attempted to Infor Visual database
- ✅ Code follows existing patterns and styles
- ✅ User-facing changes tested in UI

### Testing Requirements
- Unit tests for service logic (ViewModels, business services)
- Integration tests for database operations (DAO layer)
- Manual UI testing for Views
- Test data must not affect production Infor Visual database

---

## Infor Visual Integration Rules

### Critical Constraint
**⚠️ INFOR VISUAL DATABASE IS STRICTLY READ ONLY - NO WRITES ALLOWED AT ANY TIME ⚠️**

### Connection Details
- **Server**: VISUAL
- **Database**: MTMFG
- **Warehouse ID**: 002 (fixed, always)
- **Default Credentials**: Username=SHOP2, Password=SHOP
- **Connection String MUST Include**: `ApplicationIntent=ReadOnly`

### Allowed Operations
- ✅ SELECT queries via stored procedures only
- ✅ SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED (performance)
- ✅ Query PURCHASE_ORDER, PURC_ORDER_LINE, PART, INVENTORY_TRANS tables
- ✅ Graceful handling of connection failures (Infor Visual may be offline)

### Forbidden Operations
- ❌ INSERT, UPDATE, DELETE, MERGE statements
- ❌ CREATE, ALTER, DROP any database objects
- ❌ Transactions that lock tables (SERIALIZABLE, REPEATABLE READ)
- ❌ Assuming Infor Visual is always available

### Schema Reference
- `PURCHASE_ORDER` → ID, VENDOR_ID, STATUS, ORDER_DATE
- `PURC_ORDER_LINE` → PURC_ORDER_ID, LINE_NO, PART_ID, ORDER_QTY
- `PART` → ID, DESCRIPTION, PRODUCT_CODE, STOCK_UM
- `INVENTORY_TRANS` → Transaction history (TYPE='R', CLASS='1' for PO receipts)

---

## Governance

### Constitution Authority
- This constitution supersedes all other development practices
- Exceptions require explicit documentation and approval
- Amendments follow semantic versioning (MAJOR.MINOR.PATCH)
- All team members must be familiar with core principles

### Amendment Process
1. Propose change with rationale and impact analysis
2. Document affected code areas
3. Update version number appropriately
4. Create migration plan if breaking change
5. Update all related instruction files

### Compliance
- Code reviews MUST verify adherence to core principles
- Pull requests MUST reference relevant principles in description
- Violations MUST be justified or corrected before merge
- Regular constitution review (quarterly)

### Runtime Guidance
- Use `.github/agents/copilot-instructions.md` for AI coding assistance
- Use `.github/instructions/` files for specific domain guidance
- Use `Documentation/REUSABLE_SERVICES.md` for service patterns
- Use `specs/` folder for feature specifications

---

**Version**: 1.1.0 | **Ratified**: 2025-12-17 | **Last Amended**: 2025-12-18

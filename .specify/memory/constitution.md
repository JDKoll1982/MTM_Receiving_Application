<!--
CONSTITUTION SYNC IMPACT REPORT
Generated: 2025-12-27

VERSION CHANGE: 1.1.0 → 1.2.0 (MINOR - Architecture Enforcement & DAO Standardization)

PRINCIPLES MODIFIED IN v1.2.0:
✅ I. MVVM Architecture - Added explicit layer separation rules and violation examples
✅ II. Database Layer Consistency - Mandated instance-based DAOs and Service→DAO delegation
✅ III. Dependency Injection Everywhere - Added DAO registration requirements

PRINCIPLES ADDED IN v1.2.0:
✅ X. Infor Visual DAO Architecture (READ-ONLY)
✅ XI. Architecture Validation & Pre-Commit Checks

PRINCIPLES FROM v1.1.0:
✅ I. MVVM Architecture (NON-NEGOTIABLE) - ENHANCED
✅ II. Database Layer Consistency - ENHANCED  
✅ III. Dependency Injection Everywhere - ENHANCED
✅ IV. Error Handling & Logging
✅ V. Security & Authentication
✅ VI. WinUI 3 Modern Practices
✅ VII. Specification-Driven Development - ENHANCED with documentation requirements
✅ VIII. Testing & Quality Assurance
✅ IX. Code Quality & Maintainability

FORBIDDEN PRACTICES ADDITIONS:
- ViewModels directly calling Dao_* classes
- ViewModels accessing Helper_Database_* helpers
- Services with direct database access (must delegate to DAOs)
- Static DAO classes (all DAOs must be instance-based with DI)
- Static factory methods in result/model classes
- Circular dependencies in class dependency graph

TEMPLATES REQUIRING UPDATES:
⚠️ .github/instructions/architecture-refactoring-guide.instructions.md - NEW (to be created)
⚠️ .github/instructions/service-dao-pattern.instructions.md - NEW (to be created)
⚠️ .github/instructions/dependency-analysis.instructions.md - NEW (to be created)
⚠️ .github/instructions/dao-instance-pattern.instructions.md - NEW (to be created)
✅ .github/instructions/mvvm-pattern.instructions.md - Update with layer separation rules
✅ .github/instructions/dao-pattern.instructions.md - Update with instance-based pattern
✅ Documentation/REUSABLE_SERVICES.md - Document Service→DAO pattern

FOLLOW-UP ITEMS:
⚠️ Refactor ReceivingModeSelectionViewModel (remove Dao_User instantiation)
⚠️ Refactor ReceivingLabelViewModel (remove Dao_ReceivingLine direct call)
⚠️ Create Dao_ReceivingLoad (instance-based)
⚠️ Create Dao_PackageTypePreference (instance-based)
⚠️ Create Dao_InforVisualPO (READ-ONLY, instance-based)
⚠️ Create Dao_InforVisualPart (READ-ONLY, instance-based)
⚠️ Refactor static DAOs to instance-based (Dao_DunnageLoad, Dao_DunnageType, etc.)
⚠️ Refactor Service_MySQL_Receiving to use Dao_ReceivingLoad
⚠️ Refactor Service_MySQL_PackagePreferences to use Dao_PackageTypePreference
⚠️ Refactor Service_InforVisual to use Infor Visual DAOs
⚠️ Update App.xaml.cs DI registration for all DAOs
⚠️ Create architecture documentation files
⚠️ Run dependency analysis to verify zero violations

ESTIMATED IMPLEMENTATION: 8-12 weeks

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

RATIONALE FOR VERSION 1.2.0 (2025-12-27):
- Addresses 4 critical architecture violations discovered via dependency analysis
- Prevents circular dependencies from static factory methods in result types
- Enforces MVVM layer separation (ViewModels SHALL NOT access DAOs directly)
- Standardizes Service→DAO delegation pattern across all services
- Mandates instance-based DAOs with DI for testability and consistency
- Elevates Infor Visual READ-ONLY constraint with dedicated DAO architecture
- Adds automated validation via pre-commit checks and dependency graph analysis
- Creates migration path for existing violations (1-sprint SLA for critical fixes)

BREAKING CHANGE ASSESSMENT: NONE (Additive amendments only)
- All amendments are clarifying or extending existing principles
- Non-compliant code has 1-sprint grace period for refactoring
- Existing compliant code requires no changes

STAKEHOLDER ALIGNMENT:
- Based on codebase dependency analysis (class-dependency-graph.dot)
- Addresses violations found in ReceivingModeSelectionViewModel, ReceivingLabelViewModel
- Aligns Service_MySQL_Dunnage pattern (Service→DAO) as canonical
- Prevents recurrence through explicit prohibition and validation
- Enforces Infor Visual constraints per production ERP protection requirements
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

**Layer Separation Rules (NON-NEGOTIABLE)**:
- ViewModels SHALL NOT directly instantiate or call DAO classes (`Dao_*`)
- ViewModels SHALL NOT access database helpers (`Helper_Database_*`)
- ViewModels SHALL NOT use connection strings or database configuration
- ALL data access MUST flow through Service layer: ViewModel → Service → DAO → Database
- Services provide business logic and abstract data access details from ViewModels

**Examples of VIOLATIONS** (Must be prevented):
```csharp
// ❌ FORBIDDEN - ViewModel directly calling DAO
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

// ❌ FORBIDDEN - ViewModel instantiating DAO
var connectionString = Helper_Database_Variables.GetConnectionString();
var dao = new Dao_User(connectionString);
var result = await dao.UpdateDefaultModeAsync(...);

// ✅ CORRECT - ViewModel calls Service
var result = await _receivingLineService.InsertLineAsync(line);
var result = await _userPreferencesService.UpdateDefaultModeAsync(...);
```

**Enforcement Mechanism**:
- Code reviews MUST verify no `Dao_*` references in ViewModel files
- Pre-commit hooks SHOULD warn on ViewModel→DAO dependencies
- Dependency analysis MUST show zero ViewModel→DAO edges in class-dependency-graph.dot

**Rationale**: MVVM ensures testability, maintainability, and clear separation between UI and logic, critical for a production WinUI 3 application. Layer separation prevents tight coupling and enables unit testing of ViewModels without database dependencies.

---

### II. Database Layer Consistency

**Model_Dao_Result Pattern (MANDATORY)**:
- ALL DAO methods MUST return `Model_Dao_Result<T>` or `Model_Dao_Result`
- NO exceptions thrown from DAO layer - return `Model_Dao_Result.Failure()` instead
- Check `result.IsSuccess` before accessing `result.Data`
- Store exceptions in `result.Exception` for logging

**DAO Architecture (MANDATORY)**:
- ALL DAOs SHALL be instance-based classes (NOT static)
- ALL DAOs SHALL receive connection string via constructor injection
- ALL DAOs SHALL be registered in DI container (`App.xaml.cs`)
- DAOs are the ONLY layer that communicates with `Helper_Database_StoredProcedure`
- Services SHALL delegate to DAOs - NO direct database access in service classes

**DAO Implementation Pattern**:
```csharp
// ✅ CORRECT - Instance-based DAO with constructor injection
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    
    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_ReceivingLoad>(
            _connectionString,
            "sp_receiving_loads_get_all",
            MapFromReader
        );
    }
    
    private static Model_ReceivingLoad MapFromReader(IDataReader reader)
    {
        // Mapping logic
    }
}

// ❌ FORBIDDEN - Static DAO (legacy pattern)
public static class Dao_DunnageLoad
{
    private static string ConnectionString => Helper_Database_Variables.GetConnectionString();
    public static async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllAsync() { }
}
```

**Service → DAO Delegation Pattern (MANDATORY)**:
```csharp
// ✅ CORRECT - Service delegates to DAO
public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
{
    private readonly Dao_DunnageType _dunnageTypeDao;
    
    public Service_MySQL_Dunnage(Dao_DunnageType dunnageTypeDao)
    {
        _dunnageTypeDao = dunnageTypeDao;
    }
    
    public async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync()
    {
        try
        {
            return await _dunnageTypeDao.GetAllAsync();
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<List<Model_DunnageType>>($"Error: {ex.Message}");
        }
    }
}

// ❌ FORBIDDEN - Service with direct database access
public class Service_MySQL_Receiving : IService_MySQL_Receiving
{
    public async Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        using var connection = new MySqlConnection(_connectionString); // ❌ Direct DB access
        await connection.OpenAsync();
        var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(...);
    }
}
```

**Stored Procedures vs Direct SQL**:
- **MySQL (mtm_receiving_application)**: NO direct SQL in C# code - ALL operations via stored procedures through DAOs.
- **Infor Visual (SQL Server)**: Direct SQL queries ARE ALLOWED in DAOs for READ ONLY operations.
- `Helper_Database_StoredProcedure` SHALL ONLY be called from DAO layer.
- Services SHALL NOT call `Helper_Database_StoredProcedure` directly.
- Parameter names in C# match stored procedure parameters (WITHOUT `p_` prefix - added automatically)
- ALL DAO methods MUST be async (`Task<Model_Dao_Result<T>>`)

**Circular Dependency Prevention**:
- Result types (Model_Dao_Result, Model_Dao_Result<T>) SHALL NOT contain static factory methods
- Use dedicated factory class `DaoResultFactory` for creating result instances
- Dependency analyzers MUST NOT show any cycles in class-dependency-graph.dot
- Static factory pattern is FORBIDDEN in model/data classes

**Factory Pattern for Result Objects**:
```csharp
// ✅ CORRECT - Factory class creates result instances
public static class DaoResultFactory
{
    public static Model_Dao_Result Failure(string message, Exception? ex = null)
    {
        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = message,
            Exception = ex,
            Severity = Enum_ErrorSeverity.Error
        };
    }
    
    public static Model_Dao_Result<T> Success<T>(T data, int affectedRows = 0)
    {
        return new Model_Dao_Result<T>
        {
            Success = true,
            Data = data,
            AffectedRows = affectedRows
        };
    }
}

// ❌ FORBIDDEN - Self-referencing static methods in model
public class Model_Dao_Result
{
    public static Model_Dao_Result Failure(string message) // ❌ Creates circular dependency
    {
        return new Model_Dao_Result { ... };
    }
}
```

**Enforcement**:
- Dependency analysis extension MUST NOT report cycles
- Code reviews MUST verify factory separation from models
- Result creation ALWAYS uses `DaoResultFactory.Failure()` or `DaoResultFactory.Success()`

**Multi-Database Support**:
- MySQL for application database (MTM_Receiving_Database)
- **SQL Server for Infor Visual database (STRICTLY READ ONLY - NO WRITES EVER)**
- Separate helpers: `Helper_Database_StoredProcedure` (MySQL)
- Connection details:
  - Infor Visual: Server=VISUAL, Database=MTMFG, Warehouse=002, User=SHOP2, Password=SHOP
  - `ApplicationIntent=ReadOnly` REQUIRED for Infor Visual connections

**Rationale**: Standardizing on instance-based DAOs enables DI, improves testability (can mock DAOs), and ensures consistent architecture. Service→DAO delegation provides clear separation of concerns. Circular dependency prevention ensures clean dependency graphs and tooling compatibility.

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

**DAO Registration (MANDATORY)**:
- ALL DAOs MUST be registered in DI container as Singletons or Transient
- DAOs receive connection string via constructor parameter
- Connection string provided by `Helper_Database_Variables.GetConnectionString()`
- Services inject DAOs via constructor

**DI Container Registration Pattern**:
```csharp
// App.xaml.cs - ConfigureServices method
private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // Connection string helper (static - acceptable for configuration)
    var connectionString = Helper_Database_Variables.GetConnectionString();
    
    // Register DAOs (Singletons for stateless data access)
    services.AddSingleton<Dao_User>(sp => new Dao_User(connectionString));
    services.AddSingleton<Dao_DunnageType>(sp => new Dao_DunnageType(connectionString));
    services.AddSingleton<Dao_DunnageLoad>(sp => new Dao_DunnageLoad(connectionString));
    services.AddSingleton<Dao_ReceivingLoad>(sp => new Dao_ReceivingLoad(connectionString));
    services.AddSingleton<Dao_InforVisualPO>(sp => new Dao_InforVisualPO(inforConnectionString));
    
    // Register Services (inject DAOs)
    services.AddSingleton<IService_MySQL_Dunnage>(sp => new Service_MySQL_Dunnage(
        sp.GetRequiredService<Dao_DunnageType>(),
        sp.GetRequiredService<Dao_DunnageLoad>(),
        sp.GetRequiredService<ILoggingService>(),
        sp.GetRequiredService<IService_ErrorHandler>()
    ));
    
    // Register ViewModels (inject Services)
    services.AddTransient<ReceivingModeSelectionViewModel>();
}
```

**Rationale**: DI enables loose coupling, testability, and runtime service replacement without code changes. DAO registration ensures consistent lifecycle management and mockability for testing.

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
- **All diagrams MUST use PlantUML** (no ASCII art)
- Database schemas use PlantUML ERD with legends
- File structures use PlantUML WBS or component diagrams
- See [markdown-documentation.instructions.md](../../.github/instructions/markdown-documentation.instructions.md)

**Feature Structure**:
```
specs/
  001-feature-name/
    spec.md               # User-facing requirements
    plan.md               # Technical approach
    tasks.md              # Implementation tasks
    research.md           # Technical decisions
    data-model.md         # Database schema (PlantUML ERD)
    contracts/            # Service interfaces
    checklists/           # Validation
```

**Architecture Enforcement Documentation**:
- `.github/instructions/architecture-refactoring-guide.instructions.md` - How to fix ViewModel→DAO violations
- `.github/instructions/service-dao-pattern.instructions.md` - Why Service→DAO delegation is mandatory
- `.github/instructions/dependency-analysis.instructions.md` - How to use dependency graph tool
- `.github/instructions/dao-instance-pattern.instructions.md` - Converting static DAOs to instance-based

**Required Documentation Updates**:
- When creating new DAOs: Document in REUSABLE_SERVICES.md
- When adding services: Update service layer documentation
- When fixing violations: Add example to refactoring guide
- When discovering new anti-patterns: Update forbidden practices list

**Rationale**: Structured approach prevents scope creep, ensures stakeholder alignment, and provides clear implementation roadmap. PlantUML diagrams are easier for AI agents to parse and produce professional visualizations for human readers. Architecture documentation prevents violation recurrence.

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

### X. Infor Visual DAO Architecture (READ-ONLY OPERATIONS)

**Infor Visual DAO Requirements**:
- ALL Infor Visual operations MUST be encapsulated in DAOs
- DAOs for Infor Visual: `Dao_InforVisualPO`, `Dao_InforVisualPart`
- Connection string MUST include `ApplicationIntent=ReadOnly`
- ONLY SELECT queries allowed - NO INSERT, UPDATE, DELETE, MERGE
- Graceful handling of connection failures (Infor Visual may be offline)

**Implementation Pattern**:
```csharp
public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPO(string connectionString)
    {
        if (!connectionString.Contains("ApplicationIntent=ReadOnly"))
            throw new InvalidOperationException("Infor Visual connection MUST be read-only");
        
        _connectionString = connectionString;
    }
    
    // ✅ ALLOWED - SELECT query for PO data
    public async Task<Model_Dao_Result<Model_InforVisualPO>> GetPOByNumberAsync(string poNumber)
    {
        string query = @"
            SELECT po.ID, po.VENDOR_ID, po.STATUS, po.ORDER_DATE
            FROM PURCHASE_ORDER po
            WHERE po.ID = @PoNumber";
        
        // Direct SQL execution for Infor Visual (READ ONLY exception)
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PoNumber", poNumber);
        
        // Execute query and map results
    }
    
    // ❌ FORBIDDEN - Any write operation
    public async Task UpdatePOStatusAsync(string poNumber, string status)
    {
        throw new InvalidOperationException("Writes to Infor Visual are STRICTLY FORBIDDEN");
    }
}
```

**Allowed Operations**:
- Query PURCHASE_ORDER, PURC_ORDER_LINE, PART, INVENTORY_TRANS tables
- SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED (performance optimization)
- Graceful fallback when Infor Visual is unavailable

**Forbidden Operations**:
- Any INSERT, UPDATE, DELETE, MERGE, or DDL statements
- Transactions that acquire locks (SERIALIZABLE, REPEATABLE READ)
- Stored procedures that modify data

**Rationale**: Infor Visual is a production ERP system. Accidental writes could corrupt critical manufacturing data. This principle elevates the existing "READ ONLY" constraint to constitutional status with explicit enforcement mechanisms.

---

### XI. Architecture Validation & Pre-Commit Checks

**Automated Dependency Analysis**:
- Dependency graph analysis MUST be run before commits
- Tool: `vscode-csharp-dependency-graph` extension (magic5644.vscode-csharp-dependency-graph)
- Generated graph: `class-dependency-graph.dot`
- ZERO circular dependencies allowed
- ZERO ViewModel→DAO dependencies allowed

**Pre-Commit Checklist**:
- [ ] No compilation errors or warnings
- [ ] Dependency graph shows no cycles
- [ ] No ViewModel files reference `Dao_*` classes
- [ ] No Service files call `Helper_Database_StoredProcedure` directly (must delegate to DAOs)
- [ ] All new DAOs are instance-based and registered in DI
- [ ] All new services have interfaces and are registered in DI
- [ ] MVVM pattern adhered to (no business logic in code-behind)
- [ ] All database operations use `Model_Dao_Result` pattern
- [ ] No writes attempted to Infor Visual database

**Violation Detection**:
```bash
# PowerShell script to detect ViewModel→DAO violations
Get-ChildItem -Path "ViewModels" -Recurse -Filter "*.cs" | 
  Select-String -Pattern "Dao_" | 
  ForEach-Object { Write-Warning "VIOLATION: $($_.Filename):$($_.LineNumber) - ViewModel references DAO" }
```

**Enforcement**:
- Pull requests MUST include dependency graph screenshot showing no violations
- Code reviews MUST verify checklist completion
- CI/CD pipeline SHOULD fail builds with architecture violations

**Rationale**: Proactive detection prevents architecture violations from entering the codebase. Automated checks reduce cognitive load on reviewers and catch violations early.

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
- ❌ Direct SQL queries in C# code (MySQL - must use stored procedures)
- ❌ Exceptions thrown from DAO layer
- ❌ Business logic in Views or code-behind
- ❌ Service locator pattern
- ❌ Static service access (new code)
- ❌ `{Binding}` over `x:Bind`
- ❌ **ANY writes to Infor Visual database**
- ❌ MySQL 8.0+ exclusive features
- ❌ ViewModels directly calling `Dao_*` classes
- ❌ ViewModels accessing `Helper_Database_*` helpers
- ❌ Services with direct database access (must delegate to DAOs)
- ❌ Static DAO classes (all DAOs must be instance-based with DI)
- ❌ Static factory methods in result/model classes (use `DaoResultFactory`)
- ❌ DAO classes NOT registered in DI container
- ❌ Connection strings hardcoded in DAOs (must use constructor injection)
- ❌ Infor Visual DAOs without `ApplicationIntent=ReadOnly` validation
- ❌ Circular dependencies in class dependency graph

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

**Architecture Violation Response Protocol**:
1. **Detection**: Automated tools or manual code review identifies violation
2. **Assessment**: Determine severity (Critical, High, Medium, Low)
3. **Remediation Plan**: Create task list with specific files and changes
4. **Implementation**: Fix violations following refactoring guides
5. **Validation**: Re-run dependency analysis to confirm resolution
6. **Documentation**: Update guides with violation examples

**Severity Levels**:
- **Critical**: ViewModel→DAO, writes to Infor Visual, circular dependencies
- **High**: Service→Database direct access, static DAOs
- **Medium**: Missing DI registration, inconsistent naming
- **Low**: Documentation gaps, non-critical style violations

**Critical Violation SLA**:
- Must be addressed within 1 sprint (2 weeks)
- Requires comprehensive refactoring plan
- Blocks new feature work until resolved

### Runtime Guidance
- Use `.github/agents/copilot-instructions.md` for AI coding assistance
- Use `.github/instructions/` files for specific domain guidance
- Use `Documentation/REUSABLE_SERVICES.md` for service patterns
- Use `specs/` folder for feature specifications

---

**Version**: 1.2.0 | **Ratified**: 2025-12-17 | **Last Amended**: 2025-12-27

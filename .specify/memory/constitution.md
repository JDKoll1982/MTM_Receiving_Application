<!--
  SYNC IMPACT REPORT
  Version Change: Initial → 1.0.0
  Rationale: MINOR bump - First formal constitution establishing governance framework

  Principles Defined:
  - I. MVVM Architecture (NON-NEGOTIABLE)
  - II. Database Layer Consistency (NON-NEGOTIABLE)
  - III. Dependency Injection (NON-NEGOTIABLE)
  - IV. Error Handling & Logging (REQUIRED)
  - V. Security & Authentication (REQUIRED)
  - VI. WinUI 3 Modern Practices (REQUIRED)
  - VII. Specification-Driven Development (REQUIRED)

  Sections Added:
  - Technology Constraints
  - Development Workflow
  - Governance

  Templates Requiring Updates:
  ✅ plan-template.md - Updated Constitution Check section with all 7 principles
  ✅ spec-template.md - Aligned with structured requirements approach
  ✅ tasks-template.md - Will align task categorization with principle domains

  Follow-up TODOs:
  - Monitor principle adherence during first 3 implementation cycles
  - Review DAO migration progress (instance-based pattern adoption)
  - Evaluate MCP tooling integration effectiveness
-->

# MTM Receiving Application Constitution

## Core Principles

### I. MVVM Architecture (NON-NEGOTIABLE)

**Strict Layer Separation**: View (XAML) → ViewModel → Service → DAO → Database

**Mandatory Rules**:

- ViewModels MUST inherit from `ViewModel_Shared_Base` or `ObservableObject`
- ViewModels MUST be `partial` classes (required for CommunityToolkit.Mvvm source generators)
- ViewModels SHALL NOT directly call DAOs (`Dao_*` classes)
- ViewModels SHALL NOT access `Helper_Database_*` classes directly
- ViewModels SHALL NOT use connection strings
- ALL data binding MUST use `x:Bind` (compile-time) over `Binding` (runtime)
- ALL data access MUST flow through Service layer
- Business logic in `.xaml.cs` code-behind files is FORBIDDEN

**Rationale**: MVVM ensures testability, maintainability, and clear separation between UI presentation and business logic. Compile-time binding (`x:Bind`) provides type safety and performance benefits. The strict layering prevents tight coupling and ensures each layer has a single, well-defined responsibility.

### II. Database Layer Consistency (NON-NEGOTIABLE)

**Instance-Based DAOs**: All Data Access Objects MUST be instance-based classes (NOT static)

**Mandatory Rules**:

- DAOs MUST accept `connectionString` in constructor
- DAOs MUST be registered in DI container as Singletons
- DAOs MUST return `Model_Dao_Result` or `Model_Dao_Result<T>`
- DAOs MUST NEVER throw exceptions (return failure results instead)
- MySQL operations MUST use stored procedures ONLY (never raw SQL in C#)
- SQL Server (Infor Visual) is READ ONLY (ApplicationIntent=ReadOnly required)
- SQL Server connections MUST use stored procedures or parameterized queries
- NO INSERT, UPDATE, DELETE operations on Infor Visual database
- All database operations MUST be async (`Task<T>` return types)

**Rationale**: Instance-based DAOs enable dependency injection, testability, and configuration flexibility. Stored procedures provide security (SQL injection protection), performance (query plan caching), and separation of data logic from application code. The Infor Visual READ ONLY constraint protects the ERP system of record from accidental data corruption.

### III. Dependency Injection (NON-NEGOTIABLE)

**Constructor Injection**: All dependencies MUST be injected via constructor

**Mandatory Rules**:

- ALL services MUST have interface definitions (`IService_*`)
- ALL services MUST be registered in `App.xaml.cs` ConfigureServices
- ViewModels MUST be registered as Transient (new instance per navigation)
- DAOs MUST be registered as Singletons (stateless, reusable)
- Shared services (logging, error handling) MUST be registered as Singletons
- Service locator pattern is FORBIDDEN (no direct `App.GetService<T>()` calls in business logic)
- Static service access is FORBIDDEN (except in App.xaml.cs initialization)

**Rationale**: Dependency injection enables loose coupling, testability (mocking dependencies), and centralized configuration. Proper lifetime management (Transient vs Singleton) prevents memory leaks and ensures correct state management.

### IV. Error Handling & Logging (REQUIRED)

**Structured Error Management**: All errors MUST be logged and handled consistently

**Mandatory Rules**:

- DAOs MUST return `Model_Dao_Result` with `Success`, `ErrorMessage`, and `Severity` properties
- DAOs MUST catch exceptions and return failure results (never propagate exceptions)
- ViewModels MUST use `IService_ErrorHandler.HandleException()` for user-facing errors
- Services MUST use `IService_LoggingUtility` for audit trails and diagnostics
- Async operations MUST set `IsBusy = true` during execution and `finally` reset to `false`
- User notifications MUST use `IService_ErrorHandler.ShowUserError()` (not raw MessageBox)
- All public API methods MUST include XML documentation comments

**Rationale**: Consistent error handling ensures users receive clear, actionable feedback. Structured logging enables troubleshooting and audit compliance. Never throwing from DAOs prevents cascade failures and enables graceful degradation.

### V. Security & Authentication (REQUIRED)

**Tiered Access Control**: Authentication adapts to workstation type

**Mandatory Rules**:

- Personal workstations: Auto-login with Windows username (30-minute timeout)
- Shared terminals: Username + 4-digit PIN authentication (15-minute timeout)
- Session management MUST track user activity (mouse, keyboard, window activation)
- New user creation MUST validate PIN uniqueness
- All authentication events MUST be logged to audit trail
- Credentials MUST NEVER be stored in plaintext (appsettings.json exceptions documented)
- Connection strings MUST use `Helper_Database_Variables.GetConnectionString()` (centralized)

**Rationale**: Tiered authentication balances security with usability. Shorter timeouts for shared terminals reduce unauthorized access risk. Comprehensive activity logging supports security auditing and compliance.

### VI. WinUI 3 Modern Practices (REQUIRED)

**Platform-Specific Patterns**: Leverage WinUI 3 and .NET 8 modern capabilities

**Mandatory Rules**:

- Use `[ObservableProperty]` attribute on private fields (not manual PropertyChanged)
- Use `[RelayCommand]` attribute for command methods (not manual ICommand implementation)
- Use `async/await` for all I/O operations (database, file, network)
- Use `ObservableCollection<T>` for data-bound collections
- Window sizing MUST use `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize()`
- Converters MUST be defined in `Module_Core/Converters/` and reused
- XAML `UpdateSourceTrigger=PropertyChanged` required for TwoWay TextBox bindings
- Braces MUST be used for all control flow statements (if, for, while)
- Accessibility modifiers MUST be explicit (no implicit `private`)

**Rationale**: CommunityToolkit.Mvvm source generators reduce boilerplate and eliminate errors. Async/await prevents UI freezing. Standardized window sizing and converters ensure consistent UX. Explicit coding standards (.editorconfig compliance) improve code clarity and reduce bugs.

### VII. Specification-Driven Development (REQUIRED)

**Speckit Workflow**: All features MUST follow structured specification process

**Mandatory Rules**:

- Features MUST have specification in `specs/[###-feature-name]/` directory
- Specifications MUST include: User scenarios, requirements, constitution check
- Implementation plans MUST verify constitutional compliance before Phase 0
- Tasks MUST be tracked in `tasks.md` with status updates
- Database schema changes MUST be documented in `data-model.md` with PlantUML diagrams
- Stored procedures MUST be idempotent (use `INSERT IGNORE`, `IF NOT EXISTS`)
- All diagrams MUST use PlantUML (NOT ASCII art)
- Feature branches MUST follow naming convention: `[###-feature-name]`

**Rationale**: Structured specifications ensure requirements are understood before implementation, reducing rework. Constitutional compliance checks prevent architectural drift. PlantUML diagrams provide parseable, version-controllable documentation that both AI agents and humans can process effectively.

## Technology Constraints

**Platform**: Windows 10/11, .NET 8.0, WinUI 3 (Windows App SDK 1.8+)

**Databases**:

- MySQL 5.7.24 (mtm_receiving_application) - Full READ/WRITE access
- SQL Server (Infor Visual - VISUAL/MTMFG) - READ ONLY (ApplicationIntent=ReadOnly)

**MySQL 5.7.24 Compatibility Constraints**:

- NO JSON functions (JSON_EXTRACT, JSON_OBJECT)
- NO Common Table Expressions (WITH clause)
- NO Window functions (ROW_NUMBER, RANK)
- NO CHECK constraints (use triggers for validation)
- NO generated columns (use triggers to populate)

**Required NuGet Packages**:

- CommunityToolkit.Mvvm (MVVM source generators)
- MySql.Data (MySQL connector)
- Microsoft.Data.SqlClient (SQL Server connector)
- xUnit + FluentAssertions (testing)

**Modules** (logical separation within monolith):

- `Module_Core` - Shared infrastructure, helpers, base classes
- `Module_Shared` - Shared ViewModels, Views, models
- `Module_Receiving` - Receiving workflow, label generation
- `Module_Dunnage` - Dunnage management
- `Module_Routing` - Routing rules, location management
- `Module_Reporting` - Report generation, scheduling
- `Module_Settings` - Application configuration, user preferences
- `Module_Volvo` - Volvo-specific integration

## Development Workflow

**Feature Implementation Sequence**:

1. Read specification from `specs/[###-feature-name]/spec.md`
2. Run constitution compliance check (verify MVVM, database, DI principles)
3. Create Models (`Models/[Module]/Model_*.cs`)
4. Create DAOs (instance-based in `Data/[Module]/Dao_*.cs`)
5. Create Service Interfaces (`Contracts/Services/IService_*.cs`)
6. Implement Services (`Services/[Module]/Service_*.cs`)
7. Create ViewModels (partial, inherits `ViewModel_Shared_Base`)
8. Create Views (XAML with `x:Bind`)
9. Register all components in DI (`App.xaml.cs`)
10. Write unit tests (`Tests/Unit/[Module]/`)
11. Update documentation if architecture changed
12. Run `dotnet build && dotnet test` before commit

**MCP Tools (Preferred)**:

- Filesystem I/O: `mcp_filesystem_*` for reading/writing/listing files
- Symbol navigation: `mcp_oraios_serena_*` for code exploration
- GitHub: `githubRemote` MCP tools (use `githubLocal` Docker-based when remote unavailable)
- UI/Web testing: `mcp_playwright_browser_*` for smoke tests

**Naming Conventions**:

- ViewModels: `ViewModel_[Module]_[Feature]`
- Views: `View_[Module]_[Feature]` or `[Feature]View`
- Services: `Service_[Purpose]` with `IService_[Purpose]`
- DAOs: `Dao_[EntityName]`
- Models: `Model_[EntityName]`
- Enums: `Enum_[Category]`
- Helpers: `Helper_[Category]_[Function]`
- Methods: PascalCase, async methods end with `Async`
- Properties: PascalCase (public), `_camelCase` (private fields)
- Constants: PascalCase in static classes (NOT UPPER_SNAKE_CASE)

**Testing Requirements**:

- Framework: xUnit with FluentAssertions
- Pattern: Arrange-Act-Assert (AAA)
- Coverage: Unit tests for all Services and DAOs
- Integration tests for database operations
- UI tests via Playwright MCP for critical workflows

## Governance

**Constitutional Authority**: This constitution supersedes all other development practices, coding guidelines, and architectural preferences. When conflicts arise between this document and other guidance (READMEs, inline comments, historical patterns), this constitution MUST take precedence.

**Amendment Process**:

1. Proposed changes MUST be documented in specification format
2. Impact analysis MUST assess affected code, templates, and workflows
3. Migration plan MUST be created for existing code violating new principles
4. Version MUST be incremented according to semantic versioning:
   - **MAJOR**: Backward-incompatible governance or principle removal/redefinition
   - **MINOR**: New principle/section added or materially expanded guidance
   - **PATCH**: Clarifications, wording, typo fixes, non-semantic refinements
5. Sync Impact Report MUST be updated as HTML comment at top of this file

**Compliance Review**:

- All PRs MUST verify constitutional compliance before merge
- Speckit `/speckit.plan` command MUST run Constitution Check during Phase 0
- Feature specifications MUST include "Constitution Check" section
- Architectural decisions MUST be justified against constitutional principles
- Complexity MUST be justified with rationale (avoid premature optimization)

**Runtime Development Guidance**:

- Use `AGENTS.md` for AI agent instructions and quick reference patterns
- Use `.github/copilot-instructions.md` for detailed Copilot coding standards
- Use `.github/instructions/*.instructions.md` for domain-specific patterns
- Use `specs/[feature]/` for feature-specific context and requirements

**Enforcement**:

- Constitutional violations MUST be documented as technical debt if shipping urgently
- Refactoring tasks MUST be created to resolve violations within 2 release cycles
- Repeated violations indicate need for tooling/automation (linters, analyzers)

**Version**: 1.0.0 | **Ratified**: 2026-01-11 | **Last Amended**: 2026-01-11

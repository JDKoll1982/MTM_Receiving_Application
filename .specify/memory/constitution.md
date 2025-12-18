# MTM Receiving Application Constitution

## Core Principles

### I. MVVM Architecture Pattern
All user interface code must follow the Model-View-ViewModel pattern with strict separation of concerns:
- **ViewModels**: Contain all business logic, inherit from BaseViewModel, use CommunityToolkit.Mvvm attributes
- **Views**: Contain only UI layout (XAML), use x:Bind for data binding (not Binding)
- **Models**: Pure data containers with no business logic, serializable, strongly-typed
- **Code-behind**: Only UI-specific logic (window initialization, control events), no business logic

### II. Data Access Object (DAO) Pattern
All database access must go through the DAO layer with consistent error handling:
- **Stored Procedures Only**: No inline SQL queries in C# code
- **Model_Dao_Result Pattern**: All DAO methods return `Model_Dao_Result<T>` or `Model_Dao_Result`
- **Async-Only**: All database operations must be async (`Task<T>`)
- **Error Handling**: Return failure results, never throw exceptions from DAO methods
- **Service Layer**: Services may call DAOs but must not contain SQL queries

### III. Dependency Injection
All services and ViewModels must use constructor injection with proper lifetime management:
- **Interfaces First**: Define interfaces in `Contracts/Services/` before implementation
- **Registration**: All services registered in `App.xaml.cs` ConfigureServices()
- **Lifetimes**: Singleton for services, Transient for ViewModels/Views
- **No Service Locator**: Use constructor injection, not static dependencies

### IV. Error Handling and Logging
Comprehensive error handling and logging infrastructure required for all features:
- **Centralized Service**: All errors flow through `IService_ErrorHandler`
- **Severity Levels**: Info, Warning, Error, Critical, Fatal (Enum_ErrorSeverity)
- **User-Friendly Messages**: Display dialogs use plain language, logs contain technical details
- **File-Based Logging**: All logs written to `%APPDATA%/MTM_Receiving_Application/Logs/`
- **Context Required**: All log entries include operation context and timestamps

### V. Async/Await Throughout
All I/O operations (database, file system, network) must be asynchronous:
- **Never Block**: No `.Result`, `.Wait()`, or `Task.Run()` for async operations
- **Propagate Async**: Methods calling async code must themselves be async
- **UI Thread Safety**: Use async/await to keep UI responsive during long operations
- **IsBusy Pattern**: ViewModels use `IsBusy` property to indicate loading states

### VI. Testing Requirements
Code quality maintained through comprehensive testing strategy:
- **Manual Tests**: Manual test procedures documented in `Tests/` folder
- **Unit Tests**: Critical business logic has unit test coverage
- **Integration Tests**: Database operations, authentication flows have integration tests
- **Test Projects**: Separate test project (`MTM_Receiving_Application.Tests`)

### VII. Database Constraints
MySQL 5.7.24 compatibility required (MAMP limitation):
- **No MySQL 8.0+ Features**: No JSON functions, CTEs, window functions, CHECK constraints
- **Stored Procedures**: All data operations through stored procedures
- **Parameter Naming**: C# uses no prefix (`"PartID"`), SQL uses `IN p_PartID`
- **OUT Parameters**: Stored procedures return `p_Status` and `p_ErrorMsg`

### VIII. Code Organization
Feature-based organization with clear ownership and discoverability:
- **By Feature**: Group related files by feature (`Receiving/`, `Authentication/`)
- **Standard Locations**: Models in `Models/`, DAOs in `Data/`, Services in `Services/`, ViewModels in `ViewModels/`, Views in `Views/`
- **Naming Conventions**: `Model_EntityName`, `Dao_EntityName`, `Service_EntityName`, `EntityNameViewModel`, `EntityNamePage`

### IX. Documentation Standards
All code and features must be documented at multiple levels:
- **XML Comments**: All public APIs have XML documentation
- **Instruction Files**: Patterns documented in `.github/instructions/`
- **Spec Documentation**: Features have spec.md, plan.md, tasks.md in `specs/`
- **README Files**: Top-level README explains setup, usage, architecture

## Technical Constraints

### Technology Stack
- **Framework**: WinUI 3 (Windows App SDK 1.8+)
- **Runtime**: .NET 8.0
- **UI Toolkit**: WinUI 3 controls only (no WinForms, WPF)
- **Database**: MySQL 5.7.24 (production) via MySql.Data provider
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.2+
- **DI Container**: Microsoft.Extensions.DependencyInjection

### Platform Requirements
- **Target OS**: Windows 10 (1809+) or Windows 11
- **Architecture**: x64 only
- **Deployment**: Single-user desktop application (not web, not mobile)

### Performance Standards
- **Database Operations**: < 500ms for single-record CRUD
- **UI Responsiveness**: No blocking operations on UI thread
- **Startup Time**: < 5 seconds from launch to main window
- **Memory**: < 200MB typical working set

## Development Workflow

### Specification-Driven Development
All features follow formal specification process:
1. **spec.md**: User stories, acceptance criteria, edge cases
2. **plan.md**: Technical approach, architecture decisions, risks
3. **tasks.md**: Implementation tasks with dependencies and testing
4. **research.md**: Design decisions, trade-offs, alternatives considered

### Code Review Requirements
- All changes must pass constitutional compliance check
- DAO methods must return Model_Dao_Result
- ViewModels must inherit from BaseViewModel
- Database operations must be async
- Error handling must use IService_ErrorHandler
- New patterns must be documented in instruction files

### Quality Gates
- Code must build without warnings
- Existing tests must pass
- New features have test coverage
- Documentation updated for public APIs
- Instruction files updated for new patterns

## Governance

### Constitution Authority
- Constitution supersedes all other coding standards and practices
- All code reviews must verify constitutional compliance
- Violations must be documented and justified
- Complexity must be justified against constitutional principles

### Amendment Process
1. Amendments require documentation of rationale
2. Impact assessment on existing codebase
3. Migration plan for non-compliant code
4. Update instruction files to reflect changes
5. Version number increment

### Compliance Verification
- Use `.github/instructions/*.instructions.md` for detailed patterns
- Reference this constitution in plan.md for all features
- Code generators must follow constitutional patterns
- Custom agents must understand and apply principles

**Version**: 1.1.0 | **Ratified**: December 18, 2025 | **Last Amended**: December 18, 2025

# Constitution Summary v1.2.0

**Last Updated**: December 27, 2025

## Core Non-Negotiable Principles

### I. MVVM Architecture
- ViewModels contain ALL business logic
- Views contain ONLY UI markup (XAML)
- **ViewModels SHALL NOT access DAOs directly**
- ALL data access flows: ViewModel → Service → DAO → Database

### II. Database Layer Consistency
- ALL DAOs must be **instance-based** (NOT static)
- ALL DAO methods return `Model_Dao_Result<T>`
- Services MUST delegate to DAOs (no direct database access)
- MySQL: Only stored procedures (NO raw SQL)
- Infor Visual: Only SELECT queries (READ ONLY)

### III. Dependency Injection Everywhere
- ALL services registered in `App.xaml.cs`
- Constructor injection only (NO service locators)
- DAOs registered as Singletons
- ViewModels registered as Transient

### IV. Error Handling & Logging
- `IService_ErrorHandler` for ALL error displays
- `ILoggingService` for ALL logging
- DAOs return failure results (NEVER throw)
- NO silent failures

### V. Security & Authentication
- Multi-tier authentication (personal/shared terminals)
- Session timeouts (30m personal, 15m shared)
- Infor Visual: **READ ONLY ACCESS ONLY**

### VI. WinUI 3 Modern Practices
- Use `x:Bind` (compile-time binding)
- CommunityToolkit.Mvvm attributes
- `partial` classes for ViewModels
- Async/await for all I/O

### VII. Specification-Driven Development
- Features start with specs
- **All diagrams use PlantUML** (no ASCII art)
- Task-based implementation
- Update docs when behavior changes

### VIII. Testing & Quality Assurance
- xUnit + Moq for testing
- 80% minimum coverage for services/ViewModels
- Integration tests for DAOs

### IX. Code Quality & Maintainability
- PascalCase naming (no UPPER_SNAKE_CASE)
- **Never block UI thread** - all I/O async
- File operations: `await File.WriteAllTextAsync()`
- Target: <500ms for database operations

### X. Infor Visual DAO Architecture
- All Infor Visual operations in DAOs
- Connection MUST include `ApplicationIntent=ReadOnly`
- **ONLY SELECT queries** - NO writes

### XI. Architecture Validation
- Dependency graph analysis before commits
- Zero ViewModel→DAO dependencies
- Zero circular dependencies
- Pre-commit checklist enforcement

## Forbidden Practices (Must Prevent)

1. ViewModels calling DAOs directly
2. ViewModels accessing `Helper_Database_*`
3. Services with direct database access
4. Static DAO classes
5. Static factory methods in model classes
6. ANY writes to Infor Visual database
7. `{Binding}` instead of `x:Bind`
8. Business logic in `.xaml.cs` code-behind
9. Throwing exceptions from DAOs
10. Direct SQL for MySQL (must use stored procedures)

## Technology Constraints

- **OS**: Windows 10 1809+ / Windows 11
- **Framework**: .NET 8.0
- **UI**: WinUI 3 (Windows App SDK 1.8+)
- **Application DB**: MySQL 8.0+
- **ERP DB**: SQL Server (Infor Visual MTMFG) - **READ ONLY**

## Key NuGet Packages

- CommunityToolkit.Mvvm (8.2+)
- CommunityToolkit.WinUI.UI.Controls (7.1+)
- Microsoft.Extensions.DependencyInjection (8.0+)
- MySql.Data (9.0+)
- Microsoft.Data.SqlClient (5.2+)

## Development Workflow

### Before Starting
1. Read relevant `.github/instructions/` files
2. Review spec and plan documents
3. Verify database schema is current
4. Check DI container registrations

### During Implementation
1. Follow task checklist
2. Use existing service patterns
3. Test with development database
4. Update documentation

### Quality Gates
- ✅ No compilation errors
- ✅ MVVM pattern adhered to
- ✅ Services registered in DI
- ✅ Error handling uses `IService_ErrorHandler`
- ✅ Database operations use `Model_Dao_Result`
- ✅ Async/await for I/O
- ✅ No writes to Infor Visual
- ✅ Code follows existing patterns

## Amendment History

- **v1.0.0** (2025-12-17): Initial ratification
- **v1.1.0** (2025-12-18): Added testing standards, code quality
- **v1.2.0** (2025-12-27): Architecture enforcement, DAO standardization, layer separation rules

# MTM Receiving Application - AI Coding Assistant Guide

## Project Context

WinUI 3 manufacturing receiving application using strict MVVM architecture with MySQL (application data) and SQL Server (Infor Visual ERP - READ ONLY). Built on .NET 8 with CommunityToolkit.Mvvm and dependency injection.

**Critical Constraint**: ⚠️ **Infor Visual database is STRICTLY READ ONLY** - no INSERT/UPDATE/DELETE operations allowed on SQL Server database.

## Architecture Overview

### MVVM Pattern (Non-Negotiable)
- **ViewModels**: All business logic, inherit from `BaseViewModel`, use `[ObservableProperty]` and `[RelayCommand]` attributes, must be `partial` classes
- **Views**: XAML only, use `x:Bind` (compile-time binding), zero business logic in code-behind
- **Models**: Pure data classes with INotifyPropertyChanged, match database schemas
- **Services**: Interface-based (`IService_*`), registered in `App.xaml.cs` DI container

### Dependency Injection
All services registered in `App.xaml.cs`:
- Singletons: `ILoggingService`, `IService_ErrorHandler`, database DAOs
- Transient: ViewModels (new instance per navigation)
- Constructor injection only - no service locators or static access

### Database Layer
**Model_Dao_Result Pattern** (mandatory for all DAOs):
```csharp
var result = await Dao_MyData.GetDataAsync();
if (result.IsSuccess) {
    // Use result.Data
} else {
    // Handle result.ErrorMessage
}
```

**MySQL Operations** (mtm_receiving_application database):
- Use `Helper_Database_StoredProcedure` for ALL operations
- Only stored procedures - no direct SQL in C# code
- Connection: `Helper_Database_Variables.GetConnectionString()`

**Infor Visual Operations** (SQL Server - READ ONLY):
- Direct SQL queries allowed for SELECT only
- Connection details: Server=VISUAL, Database=MTMFG, Warehouse=002
- Must include `ApplicationIntent=ReadOnly` in connection string
- Common queries: PO validation, part lookups, material master data

## File Organization

```
ViewModels/
  Shared/BaseViewModel.cs          # Inherit all ViewModels from this
  Receiving/*ViewModel.cs          # Feature ViewModels
Views/
  Receiving/*.xaml                 # UI markup with x:Bind
Services/
  Database/                        # DAO wrappers and services
  Receiving/                       # Business logic services
Contracts/Services/                # Service interfaces
Models/
  Receiving/                       # Data models
Data/
  Receiving/                       # DAOs (static classes)
Helpers/Database/                  # Database utilities
```

## Development Patterns

### Creating a ViewModel
```csharp
public partial class MyFeatureViewModel : BaseViewModel
{
    private readonly IMyService _service;
    
    [ObservableProperty]
    private string _myProperty = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<MyModel> _items;
    
    public MyFeatureViewModel(
        IMyService service,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _service = service;
        Items = new ObservableCollection<MyModel>();
    }
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";
            var result = await _service.GetDataAsync();
            if (result.IsSuccess) {
                Items.Clear();
                foreach (var item in result.Data) Items.Add(item);
                StatusMessage = "Loaded successfully";
            } else {
                _errorHandler.ShowUserError(result.ErrorMessage, "Load Error", nameof(LoadDataAsync));
            }
        }
        catch (Exception ex) {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, 
                nameof(LoadDataAsync), "MyFeatureViewModel");
        }
        finally { IsBusy = false; }
    }
}
```

### Creating a DAO
- File: `Data/Receiving/Dao_EntityName.cs`
- Static class with async methods
- Return `Model_Dao_Result` or `Model_Dao_Result<T>`
- Call stored procedures via `Helper_Database_StoredProcedure`
- Never throw exceptions - return failure results

### Error Handling
- Use `IService_ErrorHandler.HandleException()` for all exceptions
- Use `ILoggingService` for all logging (creates CSV logs)
- Never silent failures - always log or display errors
- DAOs return `Model_Dao_Result.Failure()` instead of throwing

## Critical Commands

### Build & Test
```powershell
dotnet build                                    # Build solution
dotnet test                                     # Run all tests
dotnet build -c Release /p:Platform=x64        # Production build
```

### Database
MySQL connection string in `Helper_Database_Variables.cs`:
```
Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=<password>;
```

### Project Structure
- Constitution: `.specify/memory/constitution.md` - **required reading**
- Instructions: `.github/instructions/*.instructions.md` - pattern guides
- Specs: `specs/001-receiving-workflow/` - feature specifications
- Documentation: `Documentation/` - business logic and workflows

## Common Pitfalls

❌ **Don't**: Write to Infor Visual database (SQL Server)  
✅ **Do**: Only SELECT queries on Infor Visual

❌ **Don't**: Use `Binding` in XAML  
✅ **Do**: Use `x:Bind` for compile-time binding

❌ **Don't**: Put business logic in View code-behind  
✅ **Do**: Keep all logic in ViewModels

❌ **Don't**: Create service instances manually  
✅ **Do**: Use constructor injection from DI container

❌ **Don't**: Throw exceptions from DAOs  
✅ **Do**: Return `Model_Dao_Result.Failure()` with error message

❌ **Don't**: Use direct SQL for MySQL operations  
✅ **Do**: Use stored procedures via `Helper_Database_StoredProcedure`

## Window & Dialog Standards

- Standard window size: 1400x900
- Use `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize()` in View constructor
- Center windows on startup with `CenterOnScreen()` from WindowHelper
- Details in `.github/instructions/window-sizing.instructions.md`

## Specification-Driven Development

Features follow Speckit workflow in `specs/001-receiving-workflow/`:
1. Read `spec.md` for feature requirements
2. Review `plan.md` for implementation approach  
3. Check `tasks.md` for task status
4. Implement according to contracts in `contracts/`
5. Update task status upon completion

## Key References

- **Constitution**: `.specify/memory/constitution.md` - Core principles and non-negotiables
- **MVVM Guide**: `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Pattern**: `.github/instructions/dao-pattern.instructions.md`
- **Services**: `Documentation/REUSABLE_SERVICES.md`
- **Infor Visual**: `specs/001-receiving-workflow/INFOR_VISUAL_INTEGRATION.md`
- **Data Model**: `specs/001-receiving-workflow/data-model.md`

## Quick Start for New Features

1. Check if spec exists in `specs/` folder
2. Register services in `App.xaml.cs` ConfigureServices
3. Create ViewModel inheriting from BaseViewModel with DI
4. Create View with x:Bind to ViewModel properties/commands
5. Create service interfaces and implementations
6. Create DAOs returning Model_Dao_Result
7. Add tests in `MTM_Receiving_Application.Tests/`
8. Verify build and run: `dotnet build && dotnet test`

## Mandatory Workflow

- **Build & Fix**: When changing any code, you are to **ALWAYS** build and fix any errors and warnings before completing the current chat session.
- **File Replacement**: To completely replace an existing file, **DO NOT** use `create_file`. You must either delete and recreate the file first, or use a console command to overwrite the data.
- **Automated Builds**: Run build commands so they do not require user interaction (e.g. pressing Enter) to retrieve output.

# Editing Brief - Module_Receiving

**Last Updated: 2025-01-15**

This guide orients AI assistants and new developers working on Module_Receiving. Read this BEFORE making any code changes to avoid common pitfalls and architectural violations.

---

## Quick Module Context

**Purpose**: Manufacturing receiving workflow—log incoming shipments, generate labels, validate against ERP.

**Architecture**: MVVM pattern with WinUI 3
- **Views**: XAML UI (10 views)
- **ViewModels**: C# logic layer (10 ViewModels)
- **Services**: Business logic (5 service implementations)
- **DAOs**: Database access (4 DAO classes)
- **Models**: Data structures (16 model classes)

**Key workflow**: User enters PO → Validates against ERP → Enters load details → Saves to MySQL → Generates CSV for labels

**Tech stack**: .NET 8, WinUI 3, MySQL (read/write), SQL Server (read-only for ERP), CsvHelper, CommunityToolkit.Mvvm

---

## Critical Rules (DO NOT VIOLATE)

### 1. MVVM Layer Separation

**✅ MUST**:
- ViewModels inherit from `ViewModel_Shared_Base`
- ViewModels are `partial` classes
- All data binding uses `x:Bind` (compile-time)
- Business logic flows: View → ViewModel → Service → DAO → Database

**❌ NEVER**:
- ViewModels calling DAOs directly
- ViewModels accessing `Helper_Database_*` classes
- Business logic in `.xaml.cs` code-behind files
- Runtime `Binding` in XAML (use `x:Bind` instead)

**Why**: Maintainability, testability, and architectural consistency

### 2. Database Access Rules

**✅ MUST**:
- MySQL: Use stored procedures ONLY (never raw SQL in C#)
- SQL Server (Infor Visual): READ ONLY - no writes allowed
- DAOs must return `Model_Dao_Result` or `Model_Dao_Result<T>`
- DAOs must NEVER throw exceptions—return failure results instead

**❌ NEVER**:
- Raw SQL queries in C# for MySQL
- Any INSERT/UPDATE/DELETE against SQL Server
- Throwing exceptions from DAO methods
- Hardcoded connection strings in code

**Why**: Data integrity, ERP safety, standardized error handling

### 3. Async Patterns

**✅ MUST**:
- All async methods end with `Async` suffix
- Use `await` for all async calls
- Handle `CancellationToken` where appropriate

**❌ NEVER**:
- Async void methods (except event handlers)
- Blocking on async calls with `.Result` or `.Wait()`
- Fire-and-forget async calls

**Why**: WinUI requires responsive UI thread

### 4. Error Handling

**✅ MUST**:
- Use `IService_ErrorHandler` for user-facing errors
- Use `IService_LoggingUtility` for diagnostic logging
- Set `IsBusy = true` during async operations
- Set `StatusMessage` to inform user of progress

**❌ NEVER**:
- Silent catch blocks (always log or handle)
- Generic exception catches without logging
- User-facing error messages in code (use error handler service)

**Why**: Consistent UX, troubleshooting capability

---

## Architectural Patterns to Preserve

### ViewModel Pattern

```csharp
// ✅ CORRECT pattern
public partial class ViewModel_Receiving_MyStep : ViewModel_Shared_Base
{
    private readonly IService_MyService _service;

    [ObservableProperty]
    private string _myProperty;

    public ViewModel_Receiving_MyStep(
        IService_MyService service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _service = service;
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
            if (result.IsSuccess)
            {
                // Handle success
            }
            else
            {
                _errorHandler.ShowUserError(result.ErrorMessage, "Load Error");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
                nameof(LoadDataAsync), nameof(ViewModel_Receiving_MyStep));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Service Pattern

```csharp
// ✅ CORRECT pattern
public class Service_MyService : IService_MyService
{
    private readonly Dao_MyDao _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_MyService(Dao_MyDao dao, IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<MyData>> GetDataAsync()
    {
        _logger.LogInfo("Fetching data...");
        return await _dao.GetDataAsync();
    }
}
```

### DAO Pattern

```csharp
// ✅ CORRECT pattern - Instance-based, no exceptions
public class Dao_MyEntity
{
    private readonly string _connectionString;

    public Dao_MyEntity(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result> SaveAsync(Model_MyEntity entity)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_Field1", entity.Field1),
                new MySqlParameter("@p_Field2", entity.Field2 ?? string.Empty)
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_MyEntity_Insert",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }
}
```

---

## Common Modifications and How to Do Them Safely

### Adding a New Field to Receiving Workflow

**Steps**:
1. **Database**: Add field to MySQL stored procedure parameters
2. **Model**: Add property to `Model_ReceivingLine` or `Model_ReceivingLoad`
3. **DAO**: Update parameter mapping in DAO method
4. **Service**: Pass new field through service layer
5. **ViewModel**: Add `[ObservableProperty]` for new field
6. **XAML**: Add UI control with `x:Bind` to ViewModel property
7. **Validation**: Add validation if field is required
8. **CSV**: Update CSV generation if field goes to labels

**Test**:
- Full wizard workflow end-to-end
- Manual entry mode
- Edit mode
- CSV output verification

### Changing Validation Logic

**Steps**:
1. Locate validation in `Service_ReceivingValidation`
2. Update validation method
3. Update error messages to be user-friendly
4. Test all validation scenarios (valid, invalid, edge cases)

**Do NOT**:
- Add validation to ViewModel (belongs in service)
- Skip unit tests for validation changes

### Adding a New Workflow Step

**Steps**:
1. Add enum value to `Enum_ReceivingWorkflowStep`
2. Create new View and ViewModel (follow naming convention)
3. Update `Service_ReceivingWorkflow` step navigation
4. Update `ViewModel_Receiving_Workflow` visibility properties
5. Add to DI registration in `App.xaml.cs`
6. Update help content

**Impact**: High—touches multiple layers

---

## Sensitive Areas (Tread Carefully)

### Session Management
**File**: `Service_SessionManager`

**Why sensitive**: Crash recovery depends on this working correctly

**Before changing**:
- Understand session lifecycle completely
- Test crash scenarios thoroughly
- Verify JSON serialization/deserialization
- Check backward compatibility with existing session files

### CSV Generation
**File**: `Service_CSVWriter`

**Why sensitive**: Label printers parse these files

**Before changing**:
- Coordinate with label system vendor
- Test both local and network path scenarios
- Verify CSV format with existing label templates
- Have rollback plan if labels fail

### Save Pipeline
**File**: `Service_ReceivingWorkflow.SaveSessionAsync`

**Why sensitive**: Critical path for data integrity

**Order matters**:
1. Session save (fast, local)
2. Local CSV (critical)
3. Database save (critical)
4. Network CSV (optional)

**Before changing**:
- Understand error handling for each step
- Test failure scenarios (network down, database timeout)
- Verify transaction boundaries
- Check that partial failures don't corrupt data

### ERP Integration
**File**: Service calling `IService_InforVisual`

**Why sensitive**: Read-only access to production ERP

**Before changing**:
- Verify queries won't lock ERP tables
- Test with realistic data volumes
- Check timeout handling
- Coordinate with ERP team if query changes

---

## Validation Checklist for Code Changes

Before submitting code:

**Architectural compliance**:
- [ ] MVVM layers properly separated
- [ ] No ViewModel→DAO calls
- [ ] All bindings use `x:Bind`
- [ ] DAOs return results (not throw exceptions)
- [ ] MySQL uses stored procedures only
- [ ] No writes to SQL Server

**Code quality**:
- [ ] Async methods end with `Async`
- [ ] Proper null handling
- [ ] Explicit access modifiers
- [ ] Curly braces used everywhere
- [ ] Follows `.editorconfig` formatting

**Error handling**:
- [ ] Try-catch in appropriate places
- [ ] IsBusy flag set correctly
- [ ] User-friendly error messages
- [ ] Logging for troubleshooting

**Testing**:
- [ ] Unit tests for new logic (if applicable)
- [ ] Manual test of affected workflow
- [ ] Verify no regressions in other modes
- [ ] CSV output validated

**Documentation**:
- [ ] Update Change-Log.md if user-facing
- [ ] Update Decisions.md if architectural choice made
- [ ] Update code comments if complex logic

---

## Where to Find Things

**Configuration**:
- Connection strings: `Helper_Database_Variables`
- CSV paths: Managed by settings module
- DI registration: `App.xaml.cs`

**Shared resources**:
- Base classes: `Module_Core` and `Module_Shared`
- Error handling: `Module_Core/Services/Service_ErrorHandler`
- Logging: `Module_Core/Services/Service_LoggingUtility`
- Database helpers: `Module_Core/Helpers/Database/`

**Documentation**:
- Architecture: `.github/copilot-instructions.md`
- Testing: `.github/instructions/testing-strategy.instructions.md`
- Module docs: This `Documentation/` folder

---

## Known Deviations (Don't "Fix" These)

These intentional deviations are documented decisions:

1. **DataGrid uses runtime Binding**: WinUI DataGrid doesn't support x:Bind for columns (WinUI limitation)
2. **Some ViewModels use service locator**: `App.GetService<T>()` used when DI not available in certain contexts
3. **Local CSV is critical, network is optional**: By design for reliability

See `Decisions.md` for full reasoning on each.

---

## When in Doubt

1. **Read the constitution**: `.github/CONSTITUTION.md` has immutable rules
2. **Check memory bank**: `memory-bank/` folder has project context
3. **Review similar code**: Find analogous feature and follow its pattern
4. **Ask before breaking conventions**: If you must violate a pattern, document why in Decisions.md

**Better to ask than break**: This module is production-critical for manufacturing operations

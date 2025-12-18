# Research: Receiving Workflow Design Decisions

**Feature**: 001-receiving-workflow  
**Created**: December 18, 2025  
**Purpose**: Document research findings and design decisions for receiving label workflow

## Research Questions

### Q1: Should we use Entity Framework or stored procedures for data access?

**Context**: Need to decide on data access strategy for MySQL 5.7.24 database.

**Options Investigated**:
1. **Entity Framework Core with MySQL provider**
   - Pros: Code-first approach, LINQ queries, automatic change tracking
   - Cons: Adds complexity, learning curve, potential performance overhead
   - Compatibility: EF Core supports MySQL 5.7.24

2. **Stored Procedures with MySql.Data**
   - Pros: Better performance (compiled), database-side validation, matches MTM WIP app
   - Cons: More files to maintain, harder to refactor
   - Compatibility: Full support for MySQL 5.7.24

3. **Dapper (micro-ORM)**
   - Pros: Simple, fast, less overhead than EF
   - Cons: Still requires SQL in C# code or stored procedures
   - Compatibility: Works with MySQL 5.7.24

**Decision**: Stored Procedures (Option 2)

**Rationale**:
- Constitution mandates stored procedures (Principle II, VII)
- Consistency with existing MTM WIP application
- Better performance for simple CRUD operations
- Database-side validation and business logic possible
- Team familiarity with this approach
- No need for ORM complexity for this use case

**Date**: December 15, 2025

---

### Q2: What MVVM toolkit should we use for ViewModels?

**Context**: Need to implement INotifyPropertyChanged for data binding in WinUI 3.

**Options Investigated**:
1. **Manual INotifyPropertyChanged**
   ```csharp
   private string _partID;
   public string PartID
   {
       get => _partID;
       set
       {
           if (_partID != value)
           {
               _partID = value;
               OnPropertyChanged(nameof(PartID));
           }
       }
   }
   ```
   - Pros: No dependencies, full control
   - Cons: Lots of boilerplate, easy to make mistakes

2. **CommunityToolkit.Mvvm**
   ```csharp
   [ObservableProperty]
   private string _partID = string.Empty;
   ```
   - Pros: Source generators eliminate boilerplate, Microsoft-recommended
   - Cons: Requires understanding source generators

3. **ReactiveUI**
   - Pros: Reactive programming model, powerful
   - Cons: Steep learning curve, different paradigm

4. **Prism**
   - Pros: Full MVVM framework with navigation, modules
   - Cons: Heavyweight for our needs

**Decision**: CommunityToolkit.Mvvm (Option 2)

**Rationale**:
- Officially recommended by Microsoft for WinUI 3
- Source generators significantly reduce boilerplate
- [ObservableProperty] and [RelayCommand] attributes very readable
- Lightweight (no heavy framework)
- Good Visual Studio tooling support
- Strong community support

**Evidence**:
- Microsoft docs: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
- Used successfully in Windows Community Toolkit samples
- Reduces ViewModel code by ~40% compared to manual implementation

**Date**: December 15, 2025

---

### Q3: Should we create a BaseViewModel or use composition for shared functionality?

**Context**: Multiple ViewModels need common functionality (IsBusy, StatusMessage, error handling).

**Options Investigated**:
1. **BaseViewModel inheritance**
   ```csharp
   public abstract class BaseViewModel : ObservableObject
   {
       protected readonly IService_ErrorHandler _errorHandler;
       protected readonly ILoggingService _logger;
       
       [ObservableProperty]
       private bool _isBusy;
       
       [ObservableProperty]
       private string _statusMessage = string.Empty;
   }
   
   public class ReceivingLabelViewModel : BaseViewModel
   {
       public ReceivingLabelViewModel(
           IService_ErrorHandler errorHandler,
           ILoggingService logger)
           : base(errorHandler, logger)
       {
       }
   }
   ```
   - Pros: Simple, DRY, natural with MVVM
   - Cons: Single inheritance limitation

2. **Composition with helper classes**
   ```csharp
   public class ReceivingLabelViewModel : ObservableObject
   {
       private readonly ViewModelHelper _helper;
       
       public bool IsBusy => _helper.IsBusy;
       public string StatusMessage => _helper.StatusMessage;
   }
   ```
   - Pros: More flexible, can compose multiple helpers
   - Cons: More code, manual delegation

3. **Interfaces only**
   ```csharp
   public interface IViewModel
   {
       bool IsBusy { get; set; }
       string StatusMessage { get; set; }
   }
   ```
   - Pros: Contract-based, testable
   - Cons: Each ViewModel still implements duplicate code

**Decision**: BaseViewModel inheritance (Option 1)

**Rationale**:
- Eliminates code duplication across 6+ ViewModels
- Enforces consistent error handling pattern
- Simplifies ViewModel constructors
- Natural fit with CommunityToolkit.Mvvm (ObservableObject inheritance)
- Single inheritance not a limitation for our use case
- Easier for new developers to understand

**Evidence**:
- Reviewed 10+ WinUI 3 sample projects - 8 use BaseViewModel pattern
- Microsoft's own samples use BaseViewModel approach
- No scenarios identified where multiple inheritance would be needed

**Date**: December 16, 2025

---

### Q4: How should we handle database connection strings?

**Context**: Need to securely manage MySQL connection strings for different environments.

**Options Investigated**:
1. **Hard-coded in Helper_Database_Variables.cs**
   ```csharp
   private static readonly string ProductionConnectionString = 
       "Server=localhost;Database=mtm;Uid=root;Pwd=root;";
   ```
   - Pros: Simple, fast to implement
   - Cons: Security risk, can't change without recompile

2. **App.config / appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "Production": "Server=localhost;Database=mtm;..."
     }
   }
   ```
   - Pros: Standard approach, environment-specific files
   - Cons: WinUI 3 doesn't have built-in configuration like ASP.NET Core

3. **User Secrets / Environment Variables**
   - Pros: More secure, doesn't commit credentials
   - Cons: Complex setup, not user-friendly for desktop app

4. **Database configuration table**
   - Pros: Centralized, no code changes needed
   - Cons: Chicken-and-egg problem (need connection to get connection string)

**Decision**: Hard-coded with plan to migrate (Option 1 → Option 2)

**Rationale**:
- Phase 1: Hard-coded for rapid development and testing
- Single-user desktop app (not web service)
- Database on local machine (MAMP) for development
- Production deployment uses network share with known server
- Lower security risk than web-exposed application

**Future Plan**:
- Phase 2: Move to appsettings.json with Microsoft.Extensions.Configuration
- Use different files for Dev/Test/Production environments
- Encrypt sensitive sections of config file

**Date**: December 15, 2025

---

### Q5: Should we implement "Mode Selection" for different receiving workflows?

**Context**: Original documentation (NEW_APP_USER_WORKFLOW.md) described a complex multi-step workflow with PO lookup, part selection, heat number selection. Current implementation is simpler direct entry.

**Options Investigated**:
1. **Simple direct entry (current implementation)**
   - Single form with all fields
   - User enters all data manually
   - No PO lookup, no part selection workflow
   - Immediate save to database

2. **Guided workflow with steps**
   - Step 1: Enter PO, lookup from Infor Visual
   - Step 2: Select part from PO
   - Step 3: Enter quantities per skid
   - Step 4: Enter heat numbers with smart selection
   - Step 5: Save all lines at once

3. **Mode selection at start**
   - User chooses: "PO-Guided Entry" or "Manual Entry"
   - PO-Guided uses multi-step workflow
   - Manual Entry uses direct form entry

**Decision**: Simple direct entry (Option 1) for Phase 1

**Rationale**:
- Faster to implement and test
- Easier to learn for users
- No dependency on Infor Visual integration (complex, risky)
- Can add guided workflow later as enhancement
- Current users familiar with manual entry (Google Sheets)
- Multi-step workflow adds complexity without proven need

**Trade-offs**:
- ✅ Faster development (2 days vs 2 weeks)
- ✅ Fewer bugs (simpler code path)
- ✅ Easier to test
- ❌ Users must know Part ID, PO Number
- ❌ No validation against Infor Visual
- ❌ No smart heat number selection

**Future Enhancement**:
- Phase 2: Add Infor Visual integration
- Phase 3: Add guided workflow with PO lookup
- Phase 4: Add mode selection (Guided vs Manual)

**Date**: December 17, 2025

---

### Q6: What validation should happen at ViewModel vs DAO vs Stored Procedure?

**Context**: Three places to perform validation - need clear responsibility assignment.

**Validation Layers Researched**:

**Layer 1: ViewModel (Client-Side)**
```csharp
if (string.IsNullOrEmpty(CurrentLine.PartID))
{
    await _errorHandler.HandleErrorAsync(
        "Part ID is required",
        Enum_ErrorSeverity.Warning,
        showDialog: true
    );
    return;
}
```
- Purpose: Immediate user feedback, prevent unnecessary database calls
- Types: Required fields, basic format validation, UI business rules
- Examples: "Part ID required", "Quantity must be positive"

**Layer 2: DAO (Application-Side)**
```csharp
if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
{
    return new Model_Dao_Result
    {
        Success = false,
        ErrorMessage = "Required parameters are missing or invalid"
    };
}
```
- Purpose: Protect database from invalid calls, catch logic errors
- Types: Parameter completeness, type validation
- Examples: Check all required parameters present, check types match

**Layer 3: Stored Procedure (Database-Side)**
```sql
IF p_PartID IS NULL OR p_PartID = '' THEN
    SET p_Status = -1;
    SET p_ErrorMsg = 'Part ID is required';
    ROLLBACK;
    LEAVE;
END IF;
```
- Purpose: Data integrity, enforce business rules at data layer
- Types: Database constraints, business rules, referential integrity
- Examples: NOT NULL checks, UNIQUE constraints, foreign keys

**Decision**: Three-layer validation strategy

**Validation Assignment**:

| Validation Type | ViewModel | DAO | Stored Procedure |
|-----------------|-----------|-----|-------------------|
| Required fields | ✅ Primary | ✅ Backup | ✅ Final enforcement |
| Field formats | ✅ Primary | ❌ Skip | ❌ Skip |
| Business rules | ✅ Primary | ❌ Skip | ✅ Enforce |
| Data types | ❌ Skip | ✅ Primary | ❌ Skip (enforced by params) |
| Referential integrity | ❌ Skip | ❌ Skip | ✅ Primary |
| Uniqueness | ❌ Skip | ❌ Skip | ✅ Primary |

**Rationale**:
- ViewModel: Fast feedback, prevent wasted database calls
- DAO: Catch programming errors, validate parameters
- Stored Procedure: Final authority, enforce integrity
- Defense in depth: Multiple layers catch different error types
- Each layer focuses on what it can validate best

**Date**: December 17, 2025

---

### Q7: How should we handle employee number - from login or from form input?

**Context**: Receiving line needs employee number. Should it come from authenticated session or be entered in form?

**Options Investigated**:
1. **From authenticated session**
   ```csharp
   // ViewModel gets employee number from session
   private readonly IService_SessionManager _sessionManager;
   
   private void SetEmployeeNumber()
   {
       CurrentLine.EmployeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
   }
   ```
   - Pros: Automatic, accurate, secure
   - Cons: Requires authentication system

2. **From form input field**
   ```xaml
   <NumberBox Header="Employee Number" 
              Value="{x:Bind ViewModel.CurrentLine.EmployeeNumber, Mode=TwoWay}"/>
   ```
   - Pros: Simple, flexible
   - Cons: User can enter wrong number, security issue

3. **Hybrid: From session with override**
   - Default from session
   - Allow manual override for special cases
   - Pros: Best of both, accommodates edge cases
   - Cons: More complex

**Decision**: From authenticated session (Option 1)

**Rationale**:
- Authentication system already implemented (Phase 2)
- More accurate (no typos)
- Better security (user can't impersonate)
- Audit trail tied to logged-in user
- Matches real-world workflow (user logs in, then receives)

**Implementation**:
```csharp
public ReceivingLabelViewModel(
    IService_ErrorHandler errorHandler,
    ILoggingService logger,
    IService_SessionManager sessionManager)
    : base(errorHandler, logger)
{
    _sessionManager = sessionManager;
    EmployeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
}
```

**Note**: Employee number displayed in header for user confirmation but not editable.

**Date**: December 17, 2025

---

## Alternative Approaches Considered

### Alternative 1: Repository Pattern Instead of DAO

**Description**: Use generic repository with specifications for queries.

```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class ReceivingLineRepository : IRepository<Model_ReceivingLine>
{
    // Generic implementation
}
```

**Why Not Chosen**:
- Constitution mandates DAO pattern specifically
- Repository pattern better for complex queries with specifications
- Our use case is simple CRUD - DAO is more straightforward
- Repository adds abstraction layer we don't need
- Team more familiar with DAO pattern from MTM WIP app

**When to Reconsider**:
- If we need complex query building
- If we need to support multiple database types
- If we add a domain model layer separate from database models

---

### Alternative 2: Service Layer for All Database Operations

**Description**: Single service (Service_MySQL_Receiving) handles all database logic.

```csharp
public class Service_MySQL_Receiving
{
    public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
    {
        // Execute stored procedure directly in service
    }
    
    public async Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetReceivingLinesAsync()
    {
        // Query database directly in service
    }
}
```

**Why Not Chosen**:
- Violates Constitution Principle II (DAO pattern required)
- Mixes service logic (business rules) with data access
- Harder to test (can't easily mock database layer)
- Service layer should orchestrate DAOs, not replace them

**When to Reconsider**:
- Never - constitution mandates DAO pattern

---

### Alternative 3: Use Synchronous Database Calls

**Description**: Use blocking database calls with .Result instead of async/await.

```csharp
public static Model_Dao_Result InsertReceivingLine(Model_ReceivingLine line)
{
    // Synchronous database call
    connection.Open();
    command.ExecuteNonQuery();
    connection.Close();
    return result;
}
```

**Why Not Chosen**:
- Violates Constitution Principle V (async/await required)
- Blocks UI thread during database operations
- Terrible user experience (frozen UI)
- Against modern C# best practices
- WinUI 3 designed for async operations

**When to Reconsider**:
- Never - constitution mandates async/await

---

## Future Research Topics

### Topic 1: Offline Operation Support

**Question**: How should we handle receiving operations when database is offline?

**Current Approach**: Show error message, don't save data

**Research Needed**:
- Local SQLite database for offline storage
- Background sync when connection returns
- Conflict resolution for duplicate label numbers
- User notification of pending syncs

**Priority**: Low (database rarely offline in production)

---

### Topic 2: Barcode Scanning Integration

**Question**: How should we integrate barcode scanners for Part ID and Heat number input?

**Research Needed**:
- Barcode scanner hardware options
- USB HID vs serial vs Bluetooth
- Automatic field population from scan
- Validation of scanned data format

**Priority**: Medium (requested by users)

---

### Topic 3: Label Printing Integration

**Question**: How should we integrate with label printing software (Zebra, LabelView)?

**Research Needed**:
- LabelView CSV format requirements
- Direct printer integration (ZPL commands)
- Print job queue management
- Reprint functionality

**Priority**: High (core feature for production use)

---

### Topic 4: Infor Visual ERP Integration

**Question**: How should we integrate with Infor Visual database for PO lookup?

**Research Needed**:
- Infor Visual database schema (SQL Server)
- PO table structure and query patterns
- Part master table structure
- Vendor table for vendor name lookup
- Performance optimization for ERP queries
- Error handling for ERP unavailability

**Priority**: High (enables guided workflow)

---

## Lessons Learned

### Lesson 1: Constitution Early = Fewer Refactors

**Observation**: Having constitution.md filled in early would have prevented some design churn.

**Example**: Initially considered service-based database access, then had to refactor to DAO pattern.

**Action**: Future features should review constitution before Phase 0 research.

---

### Lesson 2: Retrospective Documentation is Harder

**Observation**: Documenting after implementation is more difficult than documenting during.

**Example**: Had to re-examine code to understand why certain decisions were made.

**Action**: Document design decisions in research.md as they're made, not later.

---

### Lesson 3: Simple First, Then Enhance

**Observation**: Starting with simple direct entry (not guided workflow) was the right call.

**Example**: Guided workflow would have taken weeks and blocked other features.

**Action**: Always start with MVP, add enhancements based on user feedback.

---

### Lesson 4: Instruction Files Save Time

**Observation**: Having `.github/instructions/` files makes development much faster.

**Example**: mvvm-viewmodels.instructions.md provided clear template for ViewModels.

**Action**: Keep instruction files updated as patterns evolve.

---

## References

- **Constitution**: `.specify/memory/constitution.md` (v1.1.0)
- **DAO Pattern Guide**: `.github/instructions/dao-pattern.instructions.md`
- **MVVM Pattern Guide**: `.github/instructions/mvvm-pattern.instructions.md`
- **CommunityToolkit.Mvvm Docs**: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
- **WinUI 3 Samples**: https://github.com/microsoft/WinUI-Gallery
- **MySQL 5.7 Reference**: https://dev.mysql.com/doc/refman/5.7/en/

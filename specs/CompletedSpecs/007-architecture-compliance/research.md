# Architecture Compliance Research

**Feature**: 007-architecture-compliance  
**Date**: 2025-12-27  
**Phase**: 0 - Research & Decision Making

## Research Questions

This document consolidates research findings for key architectural decisions required by the Architecture Compliance Refactoring feature.

---

## 1. DAO Dependency Injection Pattern

### Research Question
Should we use **IDaoFactory pattern** or **direct DAO injection** for providing DAO instances to services?

### Options Evaluated

#### Option A: Direct DAO Constructor Injection
```csharp
public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
{
    private readonly Dao_DunnageLoad _dunnageLoadDao;
    private readonly Dao_DunnageType _dunnageTypeDao;
    
    public Service_MySQL_Dunnage(
        Dao_DunnageLoad dunnageLoadDao,
        Dao_DunnageType dunnageTypeDao,
        ILoggingService logger,
        IService_ErrorHandler errorHandler)
    {
        _dunnageLoadDao = dunnageLoadDao;
        _dunnageTypeDao = dunnageTypeDao;
    }
}
```

**Pros**:
- Explicit dependencies visible in constructor signature
- Type-safe at compile time
- No additional abstraction layer
- Standard .NET DI pattern
- Better IntelliSense and tooling support

**Cons**:
- Constructor parameter count can grow (4-8 parameters for complex services)
- Each new DAO requires service constructor modification

#### Option B: IDaoFactory Pattern
```csharp
public interface IDaoFactory
{
    Dao_DunnageLoad GetDunnageLoadDao();
    Dao_DunnageType GetDunnageTypeDao();
    Dao_ReceivingLoad GetReceivingLoadDao();
    // ... other DAOs
}

public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
{
    private readonly IDaoFactory _daoFactory;
    
    public Service_MySQL_Dunnage(
        IDaoFactory daoFactory,
        ILoggingService logger,
        IService_ErrorHandler errorHandler)
    {
        _daoFactory = daoFactory;
    }
    
    public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsAsync()
    {
        var dao = _daoFactory.GetDunnageLoadDao();
        return await dao.GetAllAsync();
    }
}
```

**Pros**:
- Fewer constructor parameters (always 3: factory + logger + errorHandler)
- Adding new DAO doesn't change service constructor
- Easier to mock in unit tests (single factory mock)

**Cons**:
- Adds abstraction layer (complexity)
- Dependencies hidden from constructor signature (less explicit)
- Factory becomes god object (knows about all DAOs)
- Violates Explicit Dependencies Principle

### Decision: Direct DAO Constructor Injection (Option A)

**Rationale**:
1. **Constitutional Alignment**: Principle III (Dependency Injection Everywhere) favors explicit dependencies
2. **Microsoft Guidance**: .NET DI documentation recommends constructor injection over factory abstraction for non-generic dependencies
3. **Parameter Count Acceptable**: Analysis shows most services use 2-4 DAOs (6-8 total constructor params with logger/errorHandler) - within acceptable range
4. **Testability**: Moq can easily mock multiple DAO parameters; no advantage to factory pattern
5. **Maintainability**: Constructor signature documents exact service dependencies (self-documenting code)

**Implementation**:
- All DAOs registered as Singletons in App.xaml.cs
- Services inject specific DAO instances via constructor
- No IDaoFactory interface created

**Mitigation for Large Constructors**:
- If service requires >5 DAOs, consider splitting into smaller services (Single Responsibility Principle)
- Use C# 12 primary constructors for cleaner syntax (if all params are readonly fields)

**Alternatives Considered**:
- Generic `IDaoFactory<T>` pattern rejected: requires complex type mapping and runtime type resolution
- Service Locator pattern rejected: constitutional violation (anti-pattern in .NET DI)

---

## 2. Infor Visual Connection String Validation

### Research Question
How should Dao_InforVisualPO and Dao_InforVisualPart validate ApplicationIntent=ReadOnly in connection string?

### Options Evaluated

#### Option A: Constructor String Parsing
```csharp
public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPO(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        
        if (!connectionString.Contains("ApplicationIntent=ReadOnly", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Infor Visual connection string MUST contain ApplicationIntent=ReadOnly. " +
                "Writing to Infor Visual ERP database is STRICTLY PROHIBITED per Constitution Principle X.");
        }
        
        _connectionString = connectionString;
    }
}
```

**Pros**:
- Simple implementation (single string.Contains check)
- Fails fast at object instantiation (before any queries)
- Clear error message referencing constitutional prohibition

**Cons**:
- String parsing is brittle (doesn't handle connection string builder syntax variations)
- False negatives if casing or spacing differs

#### Option B: SqlConnectionStringBuilder Validation
```csharp
public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPO(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        
        var builder = new SqlConnectionStringBuilder(connectionString);
        
        if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
        {
            throw new InvalidOperationException(
                "Infor Visual connection string MUST have ApplicationIntent=ReadOnly. " +
                $"Current value: {builder.ApplicationIntent}. " +
                "Writing to Infor Visual ERP database is STRICTLY PROHIBITED per Constitution Principle X.");
        }
        
        _connectionString = connectionString;
    }
}
```

**Pros**:
- Robust parsing using Microsoft.Data.SqlClient built-in parser
- Handles all connection string syntax variations
- Type-safe enum comparison (ApplicationIntent.ReadOnly)
- Better error messages (shows actual vs expected value)

**Cons**:
- Requires SqlConnectionStringBuilder parsing overhead (minimal - one-time at construction)
- Slightly more complex code

### Decision: SqlConnectionStringBuilder Validation (Option B)

**Rationale**:
1. **Robustness**: Connection strings can have multiple syntax variations (key=value; Key=Value; whitespace differences)
2. **Type Safety**: ApplicationIntent enum is more reliable than string matching
3. **Microsoft Best Practice**: Using SqlConnectionStringBuilder is the recommended approach for connection string manipulation
4. **Better Error Messages**: Shows exact misconfiguration value for debugging
5. **Low Overhead**: Validation happens once per DAO instantiation (singleton lifetime)

**Implementation**:
```csharp
using Microsoft.Data.SqlClient;

public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPO(string connectionString)
    {
        ValidateReadOnlyConnection(connectionString);
        _connectionString = connectionString;
    }
    
    private static void ValidateReadOnlyConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            
            if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. " +
                    $"Current ApplicationIntent: {builder.ApplicationIntent}. " +
                    $"Writing to Infor Visual ERP database (Server={builder.DataSource}, " +
                    $"Database={builder.InitialCatalog}) is STRICTLY PROHIBITED. " +
                    "See Constitution Principle X: Infor Visual DAO Architecture.");
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Invalid Infor Visual connection string format: {ex.Message}", ex);
        }
    }
}
```

**Additional Safeguards**:
- Same validation applied to Dao_InforVisualPart
- Helper_Database_Variables.GetInforVisualConnectionString() already includes ApplicationIntent=ReadOnly
- Double-check enforcement: constructor validation + SQL Server READ-ONLY mode
- Unit tests verify InvalidOperationException thrown for non-readonly connection strings

---

## 3. Instance-Based DAO Constructor Pattern

### Research Question
Should instance-based DAOs receive connection string via constructor parameter or retrieve it internally from Helper_Database_Variables?

### Options Evaluated

#### Option A: Constructor Parameter Injection
```csharp
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    
    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
}

// App.xaml.cs registration
services.AddSingleton(sp => new Dao_ReceivingLoad(
    Helper_Database_Variables.GetConnectionString()));
```

**Pros**:
- Explicit dependency (connection string is injectable)
- Easier to test (can pass test connection string)
- No static method coupling
- Follows pure dependency injection principles

**Cons**:
- Requires connection string passed at registration (boilerplate in App.xaml.cs)
- If connection string changes, requires DAO instance replacement

#### Option B: Internal Helper Access
```csharp
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    
    public Dao_ReceivingLoad()
    {
        _connectionString = Helper_Database_Variables.GetConnectionString();
    }
}

// App.xaml.cs registration
services.AddSingleton<Dao_ReceivingLoad>();
```

**Pros**:
- Simpler registration (no lambda factory)
- Centralized connection string management in Helper_Database_Variables
- Less boilerplate in App.xaml.cs

**Cons**:
- Couples DAO to static helper (harder to test with different connection strings)
- Hidden dependency (not visible in constructor signature)

### Decision: Constructor Parameter Injection (Option A)

**Rationale**:
1. **Testability**: Unit tests can inject test database connection strings without modifying Helper_Database_Variables
2. **Explicit Dependencies**: Constructor signature documents DAO's dependency on connection string
3. **Flexibility**: Different DAO instances can use different connection strings (e.g., read replicas, test databases)
4. **Constitutional Alignment**: Principle III (Dependency Injection Everywhere) favors explicit injection over static access
5. **Future-Proofing**: Easier to migrate to connection string configuration from appsettings.json or environment variables

**Implementation Pattern**:
```csharp
// MySQL DAO
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    
    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync()
    {
        var parameters = new Dictionary<string, object>();
        return await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_ReceivingLoad>(
            _connectionString,
            "sp_receiving_loads_get_all",
            parameters);
    }
}

// Infor Visual DAO
public class Dao_InforVisualPO
{
    private readonly string _connectionString;
    
    public Dao_InforVisualPO(string inforVisualConnectionString)
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        _connectionString = inforVisualConnectionString;
    }
    
    // ... methods
}

// App.xaml.cs - DI Registration
private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // Get connection strings from Helper (static access acceptable for configuration)
    var mySqlConnectionString = Helper_Database_Variables.GetConnectionString();
    var inforVisualConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();
    
    // Register MySQL DAOs
    services.AddSingleton(sp => new Dao_User(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_ReceivingLoad(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_ReceivingLine(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_PackageTypePreference(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_DunnageLoad(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_DunnageType(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_DunnagePart(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_DunnageSpec(mySqlConnectionString));
    services.AddSingleton(sp => new Dao_InventoriedDunnage(mySqlConnectionString));
    
    // Register Infor Visual DAOs (READ-ONLY)
    services.AddSingleton(sp => new Dao_InforVisualPO(inforVisualConnectionString));
    services.AddSingleton(sp => new Dao_InforVisualPart(inforVisualConnectionString));
    
    // Register Services (inject DAOs)
    services.AddTransient<IService_UserPreferences, Service_UserPreferences>();
    services.AddTransient<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();
    services.AddSingleton<IService_MySQL_Dunnage, Service_MySQL_Dunnage>();
    services.AddSingleton<IService_MySQL_Receiving, Service_MySQL_Receiving>();
    services.AddSingleton<IService_MySQL_PackagePreferences, Service_MySQL_PackagePreferences>();
    services.AddSingleton<IService_InforVisual, Service_InforVisual>();
}
```

**Testing Pattern**:
```csharp
[Fact]
public async Task GetAllAsync_ReturnsReceivingLoads()
{
    // Arrange - inject test connection string
    var testConnectionString = "Server=localhost;Database=test_db;...";
    var dao = new Dao_ReceivingLoad(testConnectionString);
    
    // Act
    var result = await dao.GetAllAsync();
    
    // Assert
    Assert.True(result.IsSuccess);
}
```

---

## 4. Static to Instance-Based DAO Migration Strategy

### Research Question
What is the safest approach to convert existing static DAOs (Dao_DunnageLoad, Dao_DunnageType, etc.) to instance-based without breaking existing services?

### Options Evaluated

#### Option A: Big Bang Migration
- Convert all 5 static DAOs to instance-based simultaneously
- Update all dependent services in single commit
- Update all DI registrations in single commit

**Pros**: Clean cutover, no mixed patterns in codebase
**Cons**: High risk of regression, difficult to test incrementally

#### Option B: Incremental Migration with Backward Compatibility
- Convert static DAOs to instance-based one at a time
- Keep static wrapper methods temporarily (marked [Obsolete])
- Migrate services incrementally
- Remove static wrappers in final cleanup commit

**Pros**: Lower risk, can test each DAO conversion independently
**Cons**: Temporary mixed patterns, more commits required

### Decision: Incremental Migration (Option B) with Constitutional SLA

**Rationale**:
1. **Risk Mitigation**: Converting one DAO at a time allows testing before proceeding
2. **Constitutional Compliance**: 1-sprint SLA requires working code at each step (no long-lived broken builds)
3. **Rollback Capability**: If instance-based conversion breaks tests, can revert single DAO
4. **Continuous Delivery**: Application remains functional after each commit

**Migration Sequence**:
1. **Dao_DunnageLoad** (simplest - used by Service_MySQL_Dunnage)
2. **Dao_DunnageType** (used by Service_MySQL_Dunnage)
3. **Dao_DunnagePart** (used by Service_MySQL_Dunnage)
4. **Dao_DunnageSpec** (used by Service_MySQL_Dunnage)
5. **Dao_InventoriedDunnage** (used by Service_MySQL_Dunnage)

**Step-by-Step Process** (per DAO):
```csharp
// STEP 1: Add instance-based pattern while keeping static methods
public class Dao_DunnageLoad
{
    private readonly string _connectionString;
    
    // NEW: Instance-based constructor
    public Dao_DunnageLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    // NEW: Instance method
    public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_DunnageLoad>(
            _connectionString,
            "sp_dunnage_loads_get_all",
            new Dictionary<string, object>());
    }
    
    // OLD: Static wrapper (marked obsolete)
    [Obsolete("Use instance-based Dao_DunnageLoad via DI. Static methods will be removed in v2.0.")]
    public static async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync()
    {
        var connectionString = Helper_Database_Variables.GetConnectionString();
        var dao = new Dao_DunnageLoad(connectionString);
        return await dao.GetAllAsync();
    }
}

// STEP 2: Register in App.xaml.cs
services.AddSingleton(sp => new Dao_DunnageLoad(mySqlConnectionString));

// STEP 3: Update Service_MySQL_Dunnage to inject DAO
public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
{
    private readonly Dao_DunnageLoad _dunnageLoadDao;
    
    public Service_MySQL_Dunnage(Dao_DunnageLoad dunnageLoadDao, ...)
    {
        _dunnageLoadDao = dunnageLoadDao;
    }
    
    public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsAsync()
    {
        return await _dunnageLoadDao.GetAllAsync(); // Instance method
    }
}

// STEP 4: Build and test - ensure no regressions

// STEP 5: Remove static wrapper methods (final cleanup)
```

**Verification Checklist** (per DAO):
- [ ] Instance constructor created with connectionString parameter
- [ ] All static methods converted to instance methods
- [ ] DAO registered in App.xaml.cs DI container
- [ ] All consuming services updated to inject DAO
- [ ] Unit tests pass with instance-based DAO
- [ ] Integration tests pass
- [ ] Static wrapper methods removed (final step)

---

## 5. Service Layer Business Logic vs DAO Data Access

### Research Question
What logic belongs in Services vs DAOs when creating Service_UserPreferences and Service_MySQL_ReceivingLine?

### Decision Matrix

| Logic Type | Layer | Example |
|------------|-------|---------|
| **Database Operations** | DAO | ExecuteStoredProcedureAsync, parameter mapping, Model_Dao_Result creation |
| **Business Rules** | Service | Validation (e.g., "user preference mode must be 'Package' or 'Pallet'") |
| **Error Transformation** | Service | Converting DAO failures to user-friendly error messages |
| **Logging** | Service | ILoggingService calls for audit trail |
| **Transaction Coordination** | Service | Multi-DAO operations in single transaction |
| **Caching** | Service | In-memory cache for frequently accessed data |
| **Data Transformation** | Service | DTO → Model conversion, calculated fields |
| **Result Wrapping** | DAO | Model_Dao_Result<T> creation from database results |

### Decision: Thin DAOs, Rich Services

**Rationale**:
1. **DAOs are Data Mappers**: Only responsible for database CRUD operations and Model_Dao_Result wrapping
2. **Services are Business Logic Layer**: Orchestrate DAOs, apply rules, handle errors, log activities
3. **Testability**: Business logic in services can be tested with mocked DAOs
4. **Reusability**: DAOs can be reused across multiple services without duplicating logic

**Example - Service_UserPreferences**:
```csharp
public class Service_UserPreferences : IService_UserPreferences
{
    private readonly Dao_User _userDao;
    private readonly ILoggingService _logger;
    private readonly IService_ErrorHandler _errorHandler;
    
    public Service_UserPreferences(
        Dao_User userDao,
        ILoggingService logger,
        IService_ErrorHandler errorHandler)
    {
        _userDao = userDao;
        _logger = logger;
        _errorHandler = errorHandler;
    }
    
    public async Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username)
    {
        try
        {
            // BUSINESS RULE: Username normalization
            var normalizedUsername = username?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return DaoResultFactory.Failure<Model_UserPreference>(
                    "Username cannot be empty",
                    Enum_ErrorSeverity.Medium);
            }
            
            // DELEGATE TO DAO: Database operation
            var result = await _userDao.GetLatestPreferenceAsync(normalizedUsername);
            
            // LOGGING: Audit trail
            if (result.IsSuccess)
            {
                await _logger.LogInfoAsync(
                    "UserPreferences",
                    $"Retrieved preference for user {normalizedUsername}: {result.Data.DefaultMode}");
            }
            else
            {
                await _logger.LogErrorAsync(
                    "UserPreferences",
                    $"Failed to retrieve preference for {normalizedUsername}: {result.ErrorMessage}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            // ERROR TRANSFORMATION: User-friendly message
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, 
                nameof(GetLatestUserPreferenceAsync), nameof(Service_UserPreferences));
            
            return DaoResultFactory.Failure<Model_UserPreference>(
                "An error occurred while retrieving user preferences. Please try again.",
                ex,
                Enum_ErrorSeverity.High);
        }
    }
}
```

**Example - Dao_User (Data Access Only)**:
```csharp
public class Dao_User
{
    private readonly string _connectionString;
    
    public Dao_User(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    public async Task<Model_Dao_Result<Model_UserPreference>> GetLatestPreferenceAsync(string username)
    {
        // DATA ACCESS ONLY: No business rules, no validation (Service handles that)
        var parameters = new Dictionary<string, object>
        {
            { "username", username } // Parameter passed as-is from service
        };
        
        return await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_UserPreference>(
            _connectionString,
            "sp_user_get_latest_preference",
            parameters);
    }
}
```

---

## 6. Package Dependencies

### Research Question
Are additional NuGet packages required for instance-based DAO pattern and DaoFactory?

### Analysis

**Current Packages** (from MTM_Receiving_Application.csproj):
- ✅ Microsoft.Extensions.DependencyInjection (for DI container)
- ✅ CommunityToolkit.Mvvm (for ViewModels)
- ✅ MySql.Data (for MySQL connectivity)
- ✅ Microsoft.Data.SqlClient (for SQL Server connectivity)

**Required for This Feature**:
- ✅ Microsoft.Data.SqlClient - Already referenced (needed for SqlConnectionStringBuilder in Infor Visual DAO validation)
- ✅ Microsoft.Extensions.DependencyInjection - Already referenced (needed for services.AddSingleton<>())

### Decision: No Additional Packages Required

**Rationale**:
All necessary packages are already referenced in the project. The instance-based DAO pattern uses standard .NET DI primitives without requiring additional abstraction libraries.

**Verification**:
```powershell
# Verify current packages
dotnet list package

# Expected output (relevant packages):
# Microsoft.Extensions.DependencyInjection
# Microsoft.Data.SqlClient
# MySql.Data
# CommunityToolkit.Mvvm
```

---

## Summary of Research Decisions

| Decision Area | Choice | Rationale |
|---------------|--------|-----------|
| **DAO Injection Pattern** | Direct Constructor Injection | Explicit dependencies, better tooling, constitutional alignment |
| **Infor Visual Validation** | SqlConnectionStringBuilder | Robust parsing, type-safe, better error messages |
| **DAO Constructor** | Connection String Parameter | Testability, explicit dependencies, flexibility |
| **Migration Strategy** | Incremental with Backward Compatibility | Risk mitigation, continuous delivery, 1-sprint SLA compliance |
| **Service vs DAO Logic** | Thin DAOs, Rich Services | Business logic in services, data access in DAOs |
| **Package Dependencies** | No New Packages Required | All necessary packages already referenced |

---

## Next Steps (Phase 1)

With research complete, proceed to Phase 1 deliverables:

1. **data-model.md**: Define Model_InforVisualPO, Model_InforVisualPart, Model_PackageTypePreference entities
2. **contracts/**: Document DAO method signatures and service interfaces (using .md files, not .cs files)
3. **quickstart.md**: Developer guide for implementing the refactoring following researched patterns

All unknowns from Technical Context have been resolved. Ready to proceed with Phase 1 design.

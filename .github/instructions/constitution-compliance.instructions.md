# Constitution Compliance Instructions

**Purpose**: Ensure all code adheres to the MTM Receiving Application Constitution v1.0.0  
**Applies To**: `**/*.cs`, `**/*.xaml`, `**/*.xaml.cs`  
**Reference**: [.specify/memory/constitution.md](.specify/memory/constitution.md)

---

## Overview

This document provides practical guidance for writing code that complies with the project's constitution. All developers MUST read the full constitution before contributing code.

---

## I. MVVM Architecture Compliance

### ViewModels

**MUST**:
- ✅ Inherit from `BaseViewModel` or use `ObservableObject` from CommunityToolkit.Mvvm
- ✅ Use `[ObservableProperty]` attribute for bindable properties (creates backing field automatically)
- ✅ Use `[RelayCommand]` attribute for commands (creates ICommand implementation automatically)
- ✅ Be declared as `partial` class when using source generators
- ✅ Inject dependencies via constructor
- ✅ Be registered in `App.xaml.cs` ConfigureServices as Transient

**MUST NOT**:
- ❌ Contain any UI code (no WinUI controls, no XAML manipulation)
- ❌ Use static service access (use DI constructor injection only)
- ❌ Access Views directly (use data binding and commands)

**Example**:
```csharp
public partial class MyFeatureViewModel : BaseViewModel
{
    private readonly IMyService _myService;
    
    [ObservableProperty]
    private string _myProperty;
    
    [ObservableProperty]
    private ObservableCollection<MyModel> _items;
    
    public MyFeatureViewModel(
        IMyService myService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        _myService = myService;
        Items = new ObservableCollection<MyModel>();
    }
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            var result = await _myService.GetDataAsync();
            if (result.IsSuccess)
            {
                Items.Clear();
                foreach (var item in result.Data)
                    Items.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

---

### Views (XAML)

**MUST**:
- ✅ Use `x:Bind` for all data binding (compile-time, performant)
- ✅ Bind to ViewModel properties and commands only
- ✅ Specify `Mode=TwoWay` when user input is expected
- ✅ Use `ObservableCollection` for dynamic lists

**MUST NOT**:
- ❌ Use `{Binding}` syntax (runtime binding - slower, error-prone)
- ❌ Contain business logic in code-behind
- ❌ Access ViewModels via static references

**Example**:
```xml
<Page
    x:Class="MyApp.Views.MyFeatureView"
    xmlns:viewmodels="using:MyApp.ViewModels">
    
    <Grid>
        <TextBox Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay}" />
        
        <ListView ItemsSource="{x:Bind ViewModel.Items}">
            <!-- ItemTemplate here -->
        </ListView>
        
        <Button 
            Content="Load Data" 
            Command="{x:Bind ViewModel.LoadDataCommand}" 
            IsEnabled="{x:Bind ViewModel.IsNotBusy, Mode=OneWay}" />
    </Grid>
</Page>
```

---

### Views (Code-Behind .xaml.cs)

**MUST**:
- ✅ Keep minimal - only UI-specific code
- ✅ Resolve ViewModel via constructor injection
- ✅ Set DataContext in constructor if needed for x:Bind

**MUST NOT**:
- ❌ Contain business logic
- ❌ Access database or services directly
- ❌ Manipulate data (let ViewModel handle it)

**Example**:
```csharp
public sealed partial class MyFeatureView : Page
{
    public MyFeatureViewModel ViewModel { get; }
    
    public MyFeatureView(MyFeatureViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
    
    // Only UI-specific event handlers allowed
    private void DataGrid_CellEditEnding(object sender, CellEditEndingEventArgs e)
    {
        // Notify ViewModel of edit, but don't process data here
        ViewModel.HandleCellEditCommand.Execute(e);
    }
}
```

---

### Models

**MUST**:
- ✅ Implement `INotifyPropertyChanged` (use `ObservableObject` from CommunityToolkit.Mvvm)
- ✅ Be pure data classes with properties only
- ✅ Use `[ObservableProperty]` for automatic property change notifications

**MUST NOT**:
- ❌ Contain business logic or service calls
- ❌ Reference ViewModels or Views
- ❌ Perform validation (validation goes in services or ViewModels)

**Example**:
```csharp
public partial class Model_MyEntity : ObservableObject
{
    [ObservableProperty]
    private int _id;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private DateTime _createdDate;
    
    [ObservableProperty]
    private decimal _amount;
}
```

---

## II. Database Layer Compliance

### DAO Classes

**MUST**:
- ✅ ALL methods return `Task<Model_Dao_Result<T>>` or `Task<Model_Dao_Result>`
- ✅ Be async (`async Task<...>`)
- ✅ Use stored procedures ONLY (via `Helper_Database_StoredProcedure`)
- ✅ Return `Model_Dao_Result.Failure(message, ex)` on errors
- ✅ Return `Model_Dao_Result.Success(data, message, rowsAffected)` on success

**MUST NOT**:
- ❌ Throw exceptions (catch and return failure result instead)
- ❌ Write direct SQL in C# code
- ❌ Use Entity Framework or ORMs

**Example**:
```csharp
public static async Task<Model_Dao_Result<List<MyEntity>>> GetEntitiesAsync(int userId)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            ["UserId"] = userId  // NO p_ prefix - added automatically
        };
        
        var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
            Model_Application_Variables.ConnectionString,
            "sp_GetEntitiesByUser",  // Stored procedure name
            parameters
        );
        
        if (!result.IsSuccess)
            return Model_Dao_Result<List<MyEntity>>.Failure(result.StatusMessage);
        
        var entities = new List<MyEntity>();
        foreach (DataRow row in result.Data.Rows)
        {
            entities.Add(new MyEntity
            {
                Id = Convert.ToInt32(row["Id"]),
                Name = row["Name"].ToString()
            });
        }
        
        return Model_Dao_Result<List<MyEntity>>.Success(entities, "Entities retrieved", entities.Count);
    }
    catch (MySqlException ex)
    {
        return Model_Dao_Result<List<MyEntity>>.Failure($"Database error: {ex.Message}", ex);
    }
    catch (Exception ex)
    {
        return Model_Dao_Result<List<MyEntity>>.Failure($"Unexpected error: {ex.Message}", ex);
    }
}
```

---

### Service Classes (Database Access)

**MUST**:
- ✅ Use stored procedures via Helper classes
- ✅ Provide async methods
- ✅ Handle errors gracefully (catch, log, return failure)
- ✅ For Infor Visual: Include `ApplicationIntent=ReadOnly` in connection string

**MUST NOT**:
- ❌ Write to Infor Visual database (READ ONLY)
- ❌ Use direct SQL queries
- ❌ Return generic exceptions to callers (wrap in domain-specific errors)

---

## III. Dependency Injection Compliance

### Service Registration (App.xaml.cs)

**MUST**:
- ✅ Register ALL services in `ConfigureServices` method
- ✅ Use Singleton lifetime for stateless/stateful shared services
- ✅ Use Transient lifetime for ViewModels (new instance per navigation)
- ✅ Register interface → implementation pairs

**Example**:
```csharp
.ConfigureServices((context, services) =>
{
    // Core Services (Singleton)
    services.AddSingleton<ILoggingService, LoggingUtility>();
    services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
    services.AddSingleton<IMyService, MyService>();
    
    // ViewModels (Transient)
    services.AddTransient<MyFeatureViewModel>();
    
    // Views (Transient)
    services.AddTransient<MyFeatureView>();
})
```

---

### Constructor Injection

**MUST**:
- ✅ Inject ALL dependencies via constructor
- ✅ Store injected services in `private readonly` fields
- ✅ Validate dependencies (throw ArgumentNullException if null)

**MUST NOT**:
- ❌ Use service locator pattern
- ❌ Access static service instances (except backward-compatible wrappers)
- ❌ Use `new` to instantiate services

**Example**:
```csharp
public class MyService : IMyService
{
    private readonly ILoggingService _logger;
    private readonly IMyRepository _repository;
    
    public MyService(
        ILoggingService logger,
        IMyRepository repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}
```

---

## IV. Error Handling & Logging Compliance

### Error Handling

**MUST**:
- ✅ Use `IService_ErrorHandler` for ALL exceptions
- ✅ Specify severity: Low, Medium, High, Critical, Fatal
- ✅ Log ALL errors (even if not shown to user)
- ✅ Provide user-friendly error messages

**MUST NOT**:
- ❌ Let exceptions bubble up unhandled
- ❌ Use generic error messages like "An error occurred"
- ❌ Show technical stack traces to users

**Example**:
```csharp
try
{
    await ProcessDataAsync();
}
catch (Exception ex)
{
    _errorHandler.HandleException(
        ex,
        Enum_ErrorSeverity.Medium,
        callerName: nameof(MyMethod),
        controlName: nameof(MyViewModel)
    );
}
```

---

### Logging

**MUST**:
- ✅ Use `ILoggingService` for ALL logging
- ✅ Log at appropriate levels (Debug, Information, Warning, Error, Critical)
- ✅ Include context (user, action, parameters)

**Example**:
```csharp
await _logger.LogInformationAsync($"User {userId} accessed feature X");
await _logger.LogErrorAsync($"Failed to load data for user {userId}: {ex.Message}");
```

---

## V. Critical Constraints

### Infor Visual Database (STRICTLY READ ONLY)

**MUST**:
- ✅ Include `ApplicationIntent=ReadOnly` in connection string
- ✅ Use `SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED` in stored procedures
- ✅ Query ONLY: PURCHASE_ORDER, PURC_ORDER_LINE, PART, INVENTORY_TRANS tables
- ✅ Handle connection failures gracefully (Infor Visual may be offline)

**MUST NOT**:
- ❌ Execute INSERT, UPDATE, DELETE, MERGE statements
- ❌ Create, alter, or drop database objects
- ❌ Use transactions that lock tables
- ❌ Assume Infor Visual is always available

**Connection String**:
```csharp
Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;
```

---

### MySQL 5.7.24 Compatibility

**MUST NOT**:
- ❌ Use JSON functions (`JSON_EXTRACT`, `JSON_TABLE`)
- ❌ Use CTEs (Common Table Expressions / `WITH` clause)
- ❌ Use window functions (`ROW_NUMBER()`, `RANK()`, `PARTITION BY`)
- ❌ Use `CHECK` constraints

**USE INSTEAD**:
- ✅ Temporary tables for complex queries
- ✅ Stored procedure variables
- ✅ Subqueries

---

### Async/Await

**MUST**:
- ✅ Use async/await for ALL I/O operations (database, file, network)
- ✅ Return `Task<T>` or `Task` from async methods
- ✅ Suffix async methods with `Async`
- ✅ Use `ConfigureAwait(false)` in library code

**MUST NOT**:
- ❌ Use `.Result` or `.Wait()` (causes deadlocks)
- ❌ Mix sync and async code patterns

---

## Checklist for Code Reviews

Before submitting a PR, verify:

- [ ] ViewModels: No UI code, inherit from BaseViewModel, use [ObservableProperty]/[RelayCommand]
- [ ] Views: x:Bind used, no business logic in code-behind
- [ ] Models: ObservableObject, pure data classes
- [ ] DAOs: Return Model_Dao_Result, use stored procedures, async
- [ ] Services: Registered in DI, constructor injection, interface defined
- [ ] Error Handling: IService_ErrorHandler used, no silent failures
- [ ] Logging: ILoggingService used for all logs
- [ ] Infor Visual: READ ONLY, ApplicationIntent=ReadOnly, no writes
- [ ] MySQL: 5.7.24 compatible (no CTEs, JSON functions, window functions)
- [ ] Async: All I/O operations use async/await

---

## Enforcement

- Code reviews MUST verify constitution compliance
- Pull requests MUST include constitutional checklist
- Violations MUST be justified or corrected before merge
- Use [CONSTITUTION_COMPLIANCE_CHECKLIST.md](CONSTITUTION_COMPLIANCE_CHECKLIST.md) for systematic validation

---

**Version**: 1.0.0  
**Last Updated**: 2025-12-17  
**See Also**: [.specify/memory/constitution.md](.specify/memory/constitution.md)

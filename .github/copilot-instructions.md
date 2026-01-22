---
description: 'MTM Receiving Application development guidelines - MVVM architecture, database patterns, naming conventions, and WinUI 3 best practices'
applyTo: '**/*.{cs,xaml,csproj}'
---

# MTM Receiving Application Development Guide

Manufacturing receiving operations desktop application for streamlined label generation, workflow management, and ERP integration.

**Technology Stack:**
- Framework: WinUI 3 (Windows App SDK 1.6+)
- Language: C# 12
- Platform: .NET 8
- Architecture: MVVM with CommunityToolkit.Mvvm
- Database: MySQL 8.0 (READ/WRITE), SQL Server/Infor Visual (READ ONLY)
- Testing: xUnit with FluentAssertions

## General Instructions

- Follow strict MVVM layer separation: View (XAML) → ViewModel → Service → DAO → Database
- Use dependency injection for all components
- Make ViewModels partial classes inheriting from `ViewModel_Shared_Base`
- Use `x:Bind` for all XAML bindings (never runtime `Binding`)
- DAOs are instance-based and never throw exceptions
- MySQL database access uses stored procedures only
- SQL Server (Infor Visual) is READ ONLY - no writes allowed
- All async methods end with `Async` suffix
- Handle errors through service layer, never throw from DAOs

## Naming Conventions

**Classes:**
- ViewModels: `ViewModel_<Module>_<Feature>` (e.g., `ViewModel_Receiving_Workflow`)
- Views: `View_<Module>_<Feature>` (e.g., `View_Receiving_Workflow`)
- Services: `Service_<Purpose>` with interface `IService_<Purpose>` (e.g., `IService_ReceivingWorkflow`)
- DAOs: `Dao_<EntityName>` (e.g., `Dao_ReceivingLine`)
- Models: `Model_<EntityName>` (e.g., `Model_ReceivingLine`)
- Enums: `Enum_<Category>` (e.g., `Enum_ErrorSeverity`)
- Helpers: `Helper_<Category>_<Function>` (e.g., `Helper_Database_Variables`)

**Methods:**
- PascalCase for all methods
- Async methods MUST end with `Async`: `LoadDataAsync()`, `SaveAsync()`
- DAO methods: `<Action><Entity>Async` (e.g., `InsertReceivingLineAsync`)

**Properties and Fields:**
- PascalCase for public properties
- `_camelCase` for private fields (with underscore prefix)
- Observable properties use `[ObservableProperty]` on private field

## Architecture Standards

### MVVM Layer Separation

**REQUIRED:**
- ALL ViewModels MUST inherit from `ViewModel_Shared_Base` or `ObservableObject`
- ALL ViewModels MUST be `partial` classes
- ALL data binding MUST use `x:Bind` (compile-time)
- ALL data access MUST flow through Service layer

**FORBIDDEN:**
- ViewModels SHALL NOT directly call DAOs
- ViewModels SHALL NOT access `Helper_Database_*` classes
- ViewModels SHALL NOT use connection strings
- Business logic in `.xaml.cs` code-behind files

### ViewModel Pattern

```csharp
// ✅ CORRECT - Complete ViewModel pattern
public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
{
    private readonly IService_ReceivingWorkflow _workflowService;

    [ObservableProperty]
    private string _currentStepTitle = "Receiving - Mode Selection";

    [ObservableProperty]
    private ObservableCollection<Model_Item> _items;

    public ViewModel_Receiving_Workflow(
        IService_ReceivingWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        Items = new ObservableCollection<Model_Item>();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";

            var result = await _workflowService.GetDataAsync();
            if (result.IsSuccess)
            {
                Items.Clear();
                foreach (var item in result.Data)
                {
                    Items.Add(item);
                }
                StatusMessage = $"Loaded {Items.Count} items";
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage,
                    "Load Error",
                    nameof(LoadDataAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadDataAsync),
                nameof(ViewModel_Receiving_Workflow));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

```csharp
// ❌ FORBIDDEN - ViewModel calling DAO directly
public partial class ViewModel_Bad : ViewModel_Shared_Base
{
    private async Task LoadAsync()
    {
        // NEVER DO THIS
        var result = await Dao_ReceivingLine.GetLinesAsync(loadId);
    }
}
```

### Service Layer Pattern

```csharp
// ✅ CORRECT - Service provides business logic abstraction
public interface IService_MySQL_ReceivingLine
{
    Task<Model_Dao_Result> InsertLineAsync(Model_ReceivingLine line);
    Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetLinesByLoadAsync(int loadId);
}

public class Service_MySQL_ReceivingLine : IService_MySQL_ReceivingLine
{
    private readonly Dao_ReceivingLine _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_MySQL_ReceivingLine(
        Dao_ReceivingLine dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result> InsertLineAsync(Model_ReceivingLine line)
    {
        _logger.LogInfo($"Inserting receiving line for PO: {line.PONumber}");
        return await _dao.InsertReceivingLineAsync(line);
    }
}
```

### DAO Pattern

**Instance-Based DAOs:**

```csharp
// ✅ CORRECT - Instance-based DAO with proper error handling
public class Dao_ReceivingLine
{
    private readonly string _connectionString;

    public Dao_ReceivingLine(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_Quantity", line.Quantity),
                new MySqlParameter("@p_PartID", line.PartID ?? string.Empty),
                new MySqlParameter("@p_PONumber", line.PONumber ?? string.Empty)
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Receiving_Line_Insert",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error inserting receiving line: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }
}
```

```csharp
// ❌ FORBIDDEN - Static DAO
public static class Dao_ReceivingLine
{
    private static string ConnectionString =>
        Helper_Database_Variables.GetConnectionString();
}
```

**Database Rules:**
- MySQL: Use stored procedures ONLY - never raw SQL in C#
- SQL Server (Infor Visual): READ ONLY - include `ApplicationIntent=ReadOnly` in connection string
- DAOs MUST return `Model_Dao_Result` or `Model_Dao_Result<T>`
- DAOs MUST NEVER throw exceptions - return failure results instead

### XAML Binding Pattern

```xaml
<!-- ✅ CORRECT - Using x:Bind with proper mode -->
<Page
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Workflow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="10">
            <TextBox
                Text="{x:Bind ViewModel.SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="Search..." />

            <Button
                Content="Load Data"
                Command="{x:Bind ViewModel.LoadDataCommand}"
                IsEnabled="{x:Bind ViewModel.IsBusy, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}" />

            <ListView ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_Item">
                        <TextBlock Text="{x:Bind Name}" />
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
        </StackPanel>
    </Grid>
</Page>
```

```xaml
<!-- ❌ FORBIDDEN - Runtime binding -->
<TextBox Text="{Binding MyProperty}" />

<!-- ✅ CORRECT - Compile-time binding -->
<TextBox Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay}" />
```

### Dependency Injection Registration

**Service Registration Pattern (App.xaml.cs):**

```csharp
// Singletons (shared state, stateless)
services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
services.AddSingleton<IService_LoggingUtility, Service_LoggingUtility>();

// DAOs as Singletons (stateless, reusable)
var connectionString = Helper_Database_Variables.GetConnectionString();
services.AddSingleton(sp => new Dao_ReceivingLine(connectionString));
services.AddSingleton(sp => new Dao_User(connectionString));

// Services as Singletons (business logic)
services.AddSingleton<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();

// ViewModels as Transient (new instance per navigation)
services.AddTransient<ViewModel_Receiving_Workflow>();

// Views as Transient
services.AddTransient<View_Receiving_Workflow>();
```

## Code Quality Standards

### Bracing (REQUIRED)

```csharp
// ✅ CORRECT - Always use braces
if (condition)
{
    DoSomething();
}

// ❌ FORBIDDEN - No braces
if (condition)
    DoSomething();
```

### Accessibility Modifiers (REQUIRED)

```csharp
// ✅ CORRECT - Explicit modifiers
private readonly string _connectionString;
public async Task<Model_Dao_Result> SaveAsync() { }

// ❌ FORBIDDEN - Implicit modifiers
readonly string _connectionString;
async Task<Model_Dao_Result> SaveAsync() { }
```

### Null Handling

```csharp
// ✅ CORRECT - Use null-conditional operators
var result = user?.GetPreferences();

// ✅ CORRECT - Use nullable annotations
public string? OptionalValue { get; set; }

// ✅ CORRECT - Use is null/is not null
if (value is null)
{
    return;
}

// ❌ AVOID - Use == null
if (value == null)
{
    return;
}
```

### LINQ Optimization

```csharp
// ✅ CORRECT - Use Order() for simple sorting
var sorted = items.Order();

// ❌ AVOID - OrderBy with identity selector
var sorted = items.OrderBy(x => x);
```

### Formatting

- Apply code-formatting style defined in `.editorconfig`
- Prefer file-scoped namespace declarations
- Insert newline before opening curly brace of code blocks
- Use pattern matching and switch expressions
- Use `nameof` instead of string literals for member names
- Create XML doc comments for public APIs with `<summary>`, `<param>`, `<returns>`

## Testing Standards

### Test What You Can Mock

**Decision Tree:**
- Validator (FluentValidation) → ✅ Unit Test (no dependencies)
- ViewModel with IMediator → ✅ Unit Test (mock IMediator)
- ViewModel with concrete services → ⚠️ Integration Test
- Handler with concrete DAOs → ⚠️ Integration Test
- Handler with interfaces only → ✅ Unit Test
- DAO → ⚠️ Integration Test (requires database)

### Unit Test Pattern

```csharp
using FluentAssertions;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Receiving.ViewModels;

public class ViewModel_Receiving_WorkflowTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly ViewModel_Receiving_Workflow _viewModel;

    public ViewModel_Receiving_WorkflowTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        
        _viewModel = new ViewModel_Receiving_Workflow(
            _mockMediator.Object,
            _mockErrorHandler.Object);
    }

    [Fact]
    public async Task LoadDataAsync_ShouldPopulateItems_WhenServiceReturnsSuccess()
    {
        // Arrange
        var expectedData = new List<Model_Item> { new Model_Item { Name = "Test" } };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetItemsQuery>(), default))
            .ReturnsAsync(new Model_Dao_Result<List<Model_Item>> 
            { 
                IsSuccess = true, 
                Data = expectedData 
            });

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        _viewModel.Items.Should().HaveCount(1);
        _viewModel.Items[0].Name.Should().Be("Test");
    }
}
```

### Integration Test Pattern

```csharp
using FluentAssertions;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Receiving.Integration;

[Collection("Database")]
public class Dao_ReceivingLineIntegrationTests : IAsyncLifetime
{
    private readonly Dao_ReceivingLine _dao;
    private int _testLineId;

    public Dao_ReceivingLineIntegrationTests()
    {
        var connectionString = Helper_Database_Variables.GetConnectionString();
        _dao = new Dao_ReceivingLine(connectionString);
    }

    public async Task InitializeAsync()
    {
        var testLine = new Model_ReceivingLine
        {
            PONumber = "TEST-PO-001",
            PartID = "TEST-PART",
            Quantity = 100
        };
        
        var result = await _dao.InsertReceivingLineAsync(testLine);
        _testLineId = result.Data;
    }

    public async Task DisposeAsync()
    {
        await _dao.DeleteAsync(_testLineId);
    }

    [Fact]
    public async Task GetReceivingLineAsync_ShouldReturnLine_WhenLineExists()
    {
        // Arrange & Act
        var result = await _dao.GetReceivingLineAsync(_testLineId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.PONumber.Should().Be("TEST-PO-001");
    }
}
```

**Testing Rules:**
- No Arrange/Act/Assert comments (per repo guidance)
- Use FluentAssertions for readable assertions
- Test naming: `MethodName_Should<Result>_When<Condition>`
- Integration tests use `IAsyncLifetime` for setup/cleanup
- Test data prefixed with `TEST-`

## Common Patterns

### Creating New ViewModel

**Steps:**
1. Create partial class inheriting from `ViewModel_Shared_Base`
2. Use `[ObservableProperty]` for bindable properties
3. Use `[RelayCommand]` for commands
4. Inject services (never DAOs) via constructor
5. Implement error handling with `_errorHandler`
6. Set `IsBusy = true` during async operations
7. Register in `App.xaml.cs` as Transient

### Creating New View

**Steps:**
1. Use `x:Bind` for all data binding
2. Set `Mode` (`OneWay`, `TwoWay`, `OneTime`)
3. Use `UpdateSourceTrigger=PropertyChanged` for TwoWay TextBox bindings
4. No business logic in code-behind
5. Set window size with `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, width, height)`

### Creating New Service

**Steps:**
1. Create interface in `Contracts/Services/IService_<Name>.cs`
2. Implement in module's `Services/` folder
3. Inject DAOs and dependencies via constructor
4. Add logging for key operations
5. Return `Model_Dao_Result` or appropriate types
6. Register in `App.xaml.cs`

### Creating New DAO

**Steps:**
1. Make it instance-based (never static)
2. Accept `connectionString` in constructor with null check
3. Use stored procedures via `Helper_Database_StoredProcedure`
4. Return `Model_Dao_Result` or `Model_Dao_Result<T>`
5. Never throw exceptions - return failure results
6. Use `MySqlParameter[]` for parameter mapping
7. Register as Singleton in `App.xaml.cs`

## Database Access

### MySQL (READ/WRITE)

**Use stored procedures exclusively:**

```csharp
// ✅ CORRECT - Using stored procedure
var parameters = new MySqlParameter[]
{
    new MySqlParameter("@p_PONumber", poNumber),
    new MySqlParameter("@p_Quantity", quantity)
};

var result = await Helper_Database_StoredProcedure.ExecuteAsync(
    "sp_Receiving_Line_Insert",
    parameters,
    _connectionString
);
```

```csharp
// ❌ FORBIDDEN - Raw SQL
string query = "INSERT INTO receiving_line (PONumber, Quantity) VALUES (@po, @qty)";
await connection.ExecuteAsync(query, parameters);
```

### SQL Server / Infor Visual (READ ONLY)

**Only SELECT statements allowed:**

```csharp
// ✅ CORRECT - Read-only query with ApplicationIntent
var connectionString = "Server=...;ApplicationIntent=ReadOnly;";
var result = await ExecuteAsync("SELECT * FROM VISUAL.dbo.Part WHERE PartID = @id");
```

```csharp
// ❌ FORBIDDEN - Write operations
await ExecuteAsync("UPDATE VISUAL.dbo.Part SET ...");
await ExecuteAsync("INSERT INTO VISUAL.dbo.Part ...");
await ExecuteAsync("DELETE FROM VISUAL.dbo.Part ...");
```

## Error Handling

### DAO Error Handling

```csharp
// ✅ CORRECT - Return failure result, never throw
public async Task<Model_Dao_Result> SaveAsync(Model_Entity entity)
{
    if (entity is null)
    {
        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = "Entity cannot be null",
            Severity = Enum_ErrorSeverity.Warning
        };
    }

    try
    {
        // Execute operation
        return new Model_Dao_Result { Success = true };
    }
    catch (Exception ex)
    {
        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = $"Unexpected error: {ex.Message}",
            Severity = Enum_ErrorSeverity.Error
        };
    }
}
```

```csharp
// ❌ FORBIDDEN - Throwing exceptions from DAO
public async Task<Model_Dao_Result> SaveAsync(Model_Entity entity)
{
    if (entity == null)
        throw new ArgumentNullException(nameof(entity));
}
```

### ViewModel Error Handling

```csharp
// ✅ CORRECT - Catch and handle with service
try
{
    var result = await _service.SaveAsync(item);
    if (!result.IsSuccess)
    {
        _errorHandler.ShowUserError(
            result.ErrorMessage,
            "Save Error",
            nameof(SaveAsync));
    }
}
catch (Exception ex)
{
    _errorHandler.HandleException(
        ex,
        Enum_ErrorSeverity.Medium,
        nameof(SaveAsync),
        nameof(ViewModel_MyFeature));
}
```

## Module Structure

**Modules:**
- `Module_Core` - Shared infrastructure, helpers, base classes
- `Module_Shared` - Shared ViewModels, Views, models
- `Module_Receiving` - Receiving workflow and label generation
- `Module_Dunnage` - Dunnage management
- `Module_Routing` - Routing rules
- `Module_Reporting` - Report generation
- `Module_Settings` - Configuration UI
- `Module_Volvo` - Volvo-specific integration

**Common Folders per Module:**
- `Views/` - XAML pages and windows
- `ViewModels/` - View-bound logic
- `Services/` - Business logic layer
- `Data/` - DAO implementations
- `Models/` - Data models and DTOs
- `Contracts/Services/` - Service interfaces

## Key Interfaces and Base Classes

**Base Classes:**
- `ViewModel_Shared_Base` - Base for all ViewModels with `IsBusy`, `StatusMessage`, error handling
- `ObservableObject` - CommunityToolkit.Mvvm base (when not using `ViewModel_Shared_Base`)

**Common Services:**
- `IService_ErrorHandler` - Error handling and user notifications
- `IService_LoggingUtility` - Application logging
- `IService_Dispatcher` - UI thread marshalling
- `IService_Window` - Window management

**Key Helpers:**
- `Helper_Database_Variables` - Connection string management
- `Helper_Database_StoredProcedure` - Stored procedure execution
- `WindowHelper_WindowSizeAndStartupLocation` - Window sizing

## Debugging Checklist

When debugging issues, verify:

- [ ] DI registration in `App.xaml.cs`
- [ ] ViewModel is `partial` class
- [ ] ViewModel inherits from `ViewModel_Shared_Base`
- [ ] XAML uses `x:Bind` (not `Binding`)
- [ ] No ViewModel→DAO calls (must go through Service)
- [ ] DAOs return `Model_Dao_Result` (never throw)
- [ ] MySQL uses stored procedures only
- [ ] No writes to SQL Server
- [ ] Async methods end with `Async`
- [ ] Proper error handling in ViewModels
- [ ] Check XAML binding errors in Output window

## Validation

**Build Command:**
```powershell
dotnet build MTM_Receiving_Application.sln
```

**Test Command:**
```powershell
dotnet test MTM_Receiving_Application.sln
```

**Check for Architecture Violations:**
- Search for `ViewModel` calling `Dao_` directly (forbidden)
- Search for static DAO classes (forbidden)
- Search for raw SQL in C# files (forbidden for MySQL)
- Search for `INSERT/UPDATE/DELETE` against SQL Server (forbidden)

## Additional Resources

- Project Constitution: See `.github/CONSTITUTION.md` for immutable architecture rules
- Agent Definitions: See `.github/AGENTS.md` for specialized AI agents
- Memory Bank: See `memory-bank/` folder for project context and task tracking
- Testing Guide: See `.github/instructions/testing-strategy.instructions.md`

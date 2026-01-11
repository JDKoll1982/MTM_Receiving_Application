# MTM Receiving Application - Custom Instructions for GitHub Copilot

> **Last Updated:** 2026-01-11
> **Model Recommendation:** Claude Sonnet (for complex architecture/planning), GPT-4o (for routine code generation)

---

## 📋 Project Context

**Project Name:** MTM Receiving Application
**Purpose:** A manufacturing receiving operations desktop application for streamlining label generation, workflow management, and ERP integration.

**Technology Stack:**

- **Framework:** WinUI 3 (Windows App SDK 1.6+)
- **Language:** C# 12
- **Platform:** .NET 8
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Database:** MySQL 8.0 (READ/WRITE), SQL Server/Infor Visual (READ ONLY)
- **Testing:** xUnit with FluentAssertions

**Modules:**

- `Module_Core` - Shared infrastructure, helpers, base classes
- `Module_Shared` - Shared ViewModels, Views, and models
- `Module_Receiving` - Receiving workflow and label generation
- `Module_Dunnage` - Dunnage management
- `Module_Routing` - Routing rules and location management
- `Module_Reporting` - Report generation and scheduling
- `Module_Settings` - Application and user configuration
- `Module_Volvo` - Volvo-specific integration

---

## 🏗️ Architecture Principles (Constitutional)

### I. MVVM Architecture (NON-NEGOTIABLE)

**Strict Layer Separation:**

```
View (XAML) → ViewModel → Service → DAO → Database
```

**❌ FORBIDDEN:**

- ViewModels SHALL NOT directly call DAOs (`Dao_*` classes)
- ViewModels SHALL NOT access `Helper_Database_*` classes
- ViewModels SHALL NOT use connection strings
- Business logic in `.xaml.cs` code-behind files

**✅ REQUIRED:**

- ALL ViewModels MUST inherit from `ViewModel_Shared_Base` or `ObservableObject`
- ALL ViewModels MUST be `partial` classes
- ALL data binding MUST use `x:Bind` (compile-time) over `Binding` (runtime)
- ALL data access MUST flow through Service layer

**Real Example:**

```csharp
// ✅ CORRECT - ViewModel calls Service
public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
{
    private readonly IService_ReceivingWorkflow _workflowService;

    [ObservableProperty]
    private string _currentStepTitle = "Receiving - Mode Selection";

    public ViewModel_Receiving_Workflow(
        IService_ReceivingWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }

    [RelayCommand]
    private async Task NavigateNextAsync()
    {
        await _workflowService.MoveToNextStepAsync();
    }
}

// ❌ FORBIDDEN - ViewModel directly calling DAO
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);
```

### II. Database Layer Consistency (NON-NEGOTIABLE)

**Instance-Based DAOs:**

```csharp
// ✅ CORRECT - Instance-based DAO
public class Dao_ReceivingLine
{
    private readonly string _connectionString;

    public Dao_ReceivingLine(string connectionString)
    {
        _connectionString = connectionString ??
            throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result> InsertReceivingLineAsync(
        Model_ReceivingLine line)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_Quantity", line.Quantity),
                new MySqlParameter("@p_PartID", line.PartID ?? string.Empty),
                // ... more parameters
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

// ❌ FORBIDDEN - Static DAO
public static class Dao_ReceivingLine
{
    private static string ConnectionString =>
        Helper_Database_Variables.GetConnectionString();
}
```

**Database Rules:**

- MySQL: Use stored procedures ONLY (never raw SQL in C#)
- SQL Server (Infor Visual): READ ONLY (must include `ApplicationIntent=ReadOnly`)
- DAOs MUST return `Model_Dao_Result` or `Model_Dao_Result<T>`
- DAOs MUST NEVER throw exceptions (return failure results)

### III. Service Layer Architecture

**Service Pattern:**

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

**Service Registration (App.xaml.cs):**

```csharp
// Singletons (shared state, stateless)
services.AddSingleton<ILoggingService, LoggingService>();
services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();

// DAOs as Singletons (stateless, reusable)
var connectionString = Helper_Database_Variables.GetConnectionString();
services.AddSingleton(sp => new Dao_ReceivingLine(connectionString));

// Services as Singletons or Transient (business logic)
services.AddSingleton<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();

// ViewModels as Transient (new instance per navigation)
services.AddTransient<ViewModel_Receiving_Workflow>();
```

---

## 🎨 Coding Standards

### Naming Conventions

**Classes:**

- ViewModels: `ViewModel_<Module>_<Feature>` (e.g., `ViewModel_Receiving_Workflow`)
- Views: `View_<Module>_<Feature>` or `<Feature>View` (e.g., `View_Receiving_Workflow`)
- Services: `Service_<Purpose>` with `IService_<Purpose>` (e.g., `IService_ReceivingWorkflow`)
- DAOs: `Dao_<EntityName>` (e.g., `Dao_ReceivingLine`)
- Models: `Model_<EntityName>` (e.g., `Model_ReceivingLine`)
- Enums: `Enum_<Category>` (e.g., `Enum_ErrorSeverity`)
- Helpers: `Helper_<Category>_<Function>` (e.g., `Helper_Database_Variables`)

**Methods:**

- PascalCase for all methods
- Async methods MUST end with `Async` suffix: `LoadDataAsync()`, `SaveAsync()`
- DAO methods: `<Action><Entity>Async` (e.g., `InsertReceivingLineAsync`, `GetPackagesByPoAsync`)

**Properties:**

- PascalCase for public properties
- `_camelCase` for private fields (with underscore prefix)
- Observable properties use `[ObservableProperty]` on private field

**Constants:**

- PascalCase in static classes (NOT UPPER_SNAKE_CASE)

### Code Quality (.editorconfig Compliance)

**Bracing (REQUIRED):**

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

**Accessibility Modifiers (REQUIRED):**

```csharp
// ✅ CORRECT - Explicit modifiers
private readonly string _connectionString;
public async Task<Model_Dao_Result> SaveAsync() { }

// ❌ FORBIDDEN - Implicit modifiers
readonly string _connectionString;
async Task<Model_Dao_Result> SaveAsync() { }
```

**Null Handling:**

```csharp
// ✅ CORRECT - Use null-conditional operators
var result = user?.GetPreferences();

// ✅ CORRECT - Use nullable annotations
public string? OptionalValue { get; set; }
```

**LINQ Optimization:**

```csharp
// ✅ CORRECT - Use Order() for simple sorting
var sorted = items.Order();

// ❌ AVOID - OrderBy with identity selector
var sorted = items.OrderBy(x => x);
```

---

## 🛠️ When I Ask You To

### Create a New ViewModel

**I expect you to:**

1. Make it a `partial` class
2. Inherit from `ViewModel_Shared_Base`
3. Use `[ObservableProperty]` for bindable properties
4. Use `[RelayCommand]` for commands
5. Inject dependencies via constructor
6. Call services (NEVER DAOs directly)
7. Use try-catch with `_errorHandler.HandleException()`
8. Set `IsBusy = true` during async operations
9. Register in `App.xaml.cs` as Transient

**Template:**

```csharp
public partial class ViewModel_<Module>_<Feature> : ViewModel_Shared_Base
{
    private readonly IService_<YourService> _service;

    [ObservableProperty]
    private string _myProperty = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_Item> _items;

    public ViewModel_<Module>_<Feature>(
        IService_<YourService> service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _service = service;
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

            var result = await _service.GetDataAsync();
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
                nameof(ViewModel_<Module>_<Feature>));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Create a New View (XAML)

**I expect you to:**

1. Use `x:Bind` for ALL data binding (NOT `Binding`)
2. Set appropriate `Mode` (`OneWay`, `TwoWay`, `OneTime`)
3. Use `UpdateSourceTrigger=PropertyChanged` for TwoWay bindings to TextBox
4. NO business logic in code-behind (`.xaml.cs`)
5. Set window size: `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1400, 900);`
6. Use appropriate converters from `Module_Core/Converters/`

**Template:**

```xml
<Page
    x:Class="MTM_Receiving_Application.Module_<Module>.Views.View_<Module>_<Feature>"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_<Module>.ViewModels">

    <Page.DataContext>
        <viewmodels:ViewModel_<Module>_<Feature> />
    </Page.DataContext>

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

### Implement a Service

**I expect you to:**

1. Create interface in `Contracts/Services/IService_<Name>.cs`
2. Implement in appropriate module's `Services/` folder
3. Inject DAOs and other dependencies via constructor
4. Add logging for key operations
5. Handle business logic and validation
6. Return appropriate result types (`Model_Dao_Result`, `bool`, etc.)
7. Register in `App.xaml.cs`

### Write a DAO (Data Access Object)

**I expect you to:**

1. Make it instance-based (NOT static)
2. Accept `connectionString` in constructor
3. Use stored procedures with `Helper_Database_StoredProcedure`
4. Return `Model_Dao_Result` or `Model_Dao_Result<T>`
5. NEVER throw exceptions (return failure results)
6. Use proper parameter mapping with `MySqlParameter[]`
7. Register as Singleton in `App.xaml.cs`

### Add a New Feature

**I expect you to:**

1. **Read existing documentation** (constitution, instruction files, AGENTS.md)
2. **Create Model** if needed (`Models/<Module>/Model_<Entity>.cs`)
3. **Create DAO** (Instance-based in `Data/<Module>/Dao_<Entity>.cs`)
4. **Create Service Interface** (`Contracts/Services/IService_<Feature>.cs`)
5. **Implement Service** (`Services/<Module>/Service_<Feature>.cs`)
6. **Create ViewModel** (Partial, inherits `ViewModel_Shared_Base`)
7. **Create View** (XAML with `x:Bind`)
8. **Register all in DI** (`App.xaml.cs`)
9. **Write unit tests** (`Tests/Unit/`)
10. **Update documentation** if architecture changed

### Debug an Issue

**I expect you to:**

1. **Check error logs** first (use `IService_LoggingUtility`)
2. **Verify DI registration** in `App.xaml.cs`
3. **Check XAML binding errors** (use debugger, check Output window)
4. **Verify async/await** patterns (no `async void`, proper `CancellationToken`)
5. **Check database connection** (MySQL and SQL Server)
6. **Validate stored procedures** exist and have correct parameters
7. **Review constitutional constraints** (no ViewModel→DAO, no writing to SQL Server)

### Optimize Performance

**I expect you to:**

1. **Use async/await** properly (avoid blocking calls)
2. **Batch database operations** where possible
3. **Use `ObservableCollection` efficiently** (Clear + AddRange pattern)
4. **Avoid unnecessary UI updates** (check `IsBusy` before operations)
5. **Profile with diagnostics tools** before optimizing
6. **Use LINQ `Order()` over `OrderBy(x => x)`**
7. **Consider caching** for frequently accessed data

---

## 🚫 Things to Avoid (Anti-Patterns)

### Architecture Violations

❌ **ViewModels calling DAOs directly:**

```csharp
// FORBIDDEN
var result = await Dao_User.GetUserAsync(userId);

// CORRECT
var result = await _userService.GetUserAsync(userId);
```

❌ **Static DAOs:**

```csharp
// FORBIDDEN
public static class Dao_User { }

// CORRECT
public class Dao_User
{
    public Dao_User(string connectionString) { }
}
```

❌ **Raw SQL in C# (MySQL):**

```csharp
// FORBIDDEN
string query = "INSERT INTO table VALUES (@val)";
await connection.ExecuteAsync(query, parameters);

// CORRECT
await Helper_Database_StoredProcedure.ExecuteAsync(
    "sp_insert_table", parameters, _connectionString);
```

❌ **Writing to SQL Server (Infor Visual):**

```csharp
// FORBIDDEN - SQL Server is READ ONLY
await ExecuteAsync("UPDATE VISUAL.dbo.table SET ...");

// CORRECT - Only SELECT allowed
await ExecuteAsync("SELECT * FROM VISUAL.dbo.table WHERE ...");
```

### MVVM Violations

❌ **Non-partial ViewModels:**

```csharp
// FORBIDDEN - Won't compile with CommunityToolkit.Mvvm
public class MyViewModel : ViewModel_Shared_Base { }

// CORRECT
public partial class MyViewModel : ViewModel_Shared_Base { }
```

❌ **Using `Binding` instead of `x:Bind`:**

```xml
<!-- FORBIDDEN - Runtime binding -->
<TextBox Text="{Binding MyProperty}" />

<!-- CORRECT - Compile-time binding -->
<TextBox Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay}" />
```

❌ **Business logic in code-behind:**

```csharp
// FORBIDDEN
private async void Button_Click(object sender, RoutedEventArgs e)
{
    var data = await _service.GetData();
    DataGrid.ItemsSource = data;
}

// CORRECT - Use ViewModel command
// View.xaml.cs:
public MyView()
{
    InitializeComponent();
    ViewModel = App.GetService<MyViewModel>();
}

// ViewModel:
[RelayCommand]
private async Task LoadDataAsync()
{
    var result = await _service.GetDataAsync();
    if (result.IsSuccess)
    {
        Items.Clear();
        foreach (var item in result.Data)
        {
            Items.Add(item);
        }
    }
}
```

### Error Handling Violations

❌ **DAOs throwing exceptions:**

```csharp
// FORBIDDEN
public async Task<Model_Dao_Result> SaveAsync(Model_Entity entity)
{
    if (entity == null)
        throw new ArgumentNullException(nameof(entity));
}

// CORRECT
public async Task<Model_Dao_Result> SaveAsync(Model_Entity entity)
{
    if (entity == null)
    {
        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = "Entity cannot be null",
            Severity = Enum_ErrorSeverity.Warning
        };
    }
}
```

---

## 📚 Communication Style Preferences

**When generating code:**

- Show complete, compilable examples (not pseudocode)
- Use actual class names from this project
- Include XML documentation comments for public APIs
- Reference constitutional principles when relevant

**When explaining:**

- Be concise but complete
- Reference specific files/classes from the codebase
- Explain "why" in addition to "how"
- Cite architectural constraints from constitution

**When suggesting improvements:**

- Prioritize constitutional compliance first
- Suggest incremental changes over rewrites
- Explain trade-offs clearly
- Reference similar patterns in existing code

---

## 🔗 Quick Reference

**Base Classes:**

- ViewModels: `ViewModel_Shared_Base` (in `Module_Shared/ViewModels/`)
- Models: None required (POCOs)
- Services: Interface required (`IService_<Name>`)

**Common Interfaces:**

- `IService_ErrorHandler` - Error handling and user notifications
- `IService_LoggingUtility` - Application logging
- `IService_Dispatcher` - UI thread marshalling
- `IService_Window` - Window management

**Key Helpers:**

- `Helper_Database_Variables` - Connection string management
- `Helper_Database_StoredProcedure` - Stored procedure execution
- `WindowHelper_WindowSizeAndStartupLocation` - Window sizing

**Testing:**

- Framework: xUnit
- Pattern: Arrange-Act-Assert (AAA)
- Assertions: FluentAssertions
- Unit Tests: `Tests/Unit/<Module>/`
- Integration Tests: `Tests/Integration/<Module>/`

---

## 🎯 Model Selection Guide

Use this guide to select the appropriate Copilot model for different tasks:

| Task Type                          | Recommended Model       | Rationale                    |
| ---------------------------------- | ----------------------- | ---------------------------- |
| **Complex Architecture Design**    | Claude Sonnet           | Deep reasoning, long context |
| **Refactoring Multi-File Changes** | Claude Sonnet           | Understands dependencies     |
| **New Feature Implementation**     | GPT-4o                  | Balanced speed and quality   |
| **Routine CRUD Operations**        | GPT-4o                  | Fast, reliable patterns      |
| **Debugging Logic Issues**         | Claude Sonnet           | Better problem analysis      |
| **Code Review / Analysis**         | Claude Sonnet           | Thorough evaluation          |
| **Boilerplate Generation**         | GPT-4o or GPT-3.5 Turbo | Speed matters                |
| **Documentation Writing**          | Claude Sonnet           | Better structure and clarity |

---

## ✅ Pre-Flight Checklist

Before accepting Copilot-generated code, verify:

- [ ] ViewModels are `partial` classes
- [ ] ViewModels inherit from `ViewModel_Shared_Base`
- [ ] ViewModels use `[ObservableProperty]` and `[RelayCommand]`
- [ ] Views use `x:Bind` (NOT `Binding`)
- [ ] DAOs are instance-based (NOT static)
- [ ] DAOs return `Model_Dao_Result` (never throw)
- [ ] Services registered in `App.xaml.cs`
- [ ] No ViewModel→DAO calls (must go through Service)
- [ ] MySQL uses stored procedures only
- [ ] No writes to SQL Server (Infor Visual)
- [ ] All braces present on if statements
- [ ] Accessibility modifiers explicit
- [ ] Async methods end with `Async`
- [ ] No secrets or connection strings in code

---

**For more details:**

- Constitution: `.specify/memory/constitution.md`
- Architecture: `ARCHITECTURE.md` (create if needed)
- MVVM Guide: `.github/instructions/mvvm-pattern.instructions.md`
- DAO Guide: `.github/instructions/dao-pattern.instructions.md`
- Best Practices: `docs/Copilot-BestPractices.md`

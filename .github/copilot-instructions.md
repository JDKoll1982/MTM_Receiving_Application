---
description: "Core MTM Receiving Application instructions covering architecture, validation, documentation maintenance, and how to use the categorized instruction set."
applyTo: "**/*.{cs,xaml,csproj,vb,fs,sql,md,txt,ps1,sh,bash,cmd,bat,py,js,ts,jsx,tsx,html,htm,css,scss,json,yaml,yml,xml,config,toml,ini,env,props,targets}"
---

# MTM Receiving Application Development Guide

This is the repository-level source of truth for how coding agents should behave in this workspace.
Use this file for global rules. Use category-specific files under `.github/instructions/` for the
exact language, architecture, testing, database, tooling, or documentation guidance relevant to
the current change.

## Start Here

- Read `README.md` for the project overview and repository layout.
- Read `.github/README.md` for the active AI-documentation taxonomy.
- Read this file for global rules that apply everywhere.
- Read only the instruction files that match the files you are changing or the task you are doing.
- Treat `.github/archive/` as historical reference only. Do not use archived guidance as active
    instruction unless the user explicitly asks for historical comparison.

## Non-Negotiable Rules

- Follow the MVVM flow: View → ViewModel → Service → DAO → Database.
- Do not let ViewModels call DAOs or `Helper_Database_*` classes directly.
- Use `x:Bind` in XAML. Do not introduce runtime `{Binding}`.
- Keep DAOs instance-based and return `Model_Dao_Result` or `Model_Dao_Result<T>` instead of
    throwing for expected operational failures.
- Use stored procedures for MySQL work. Do not write raw MySQL SQL in C#.
- Treat the Infor Visual SQL Server connection as read only. No `INSERT`, `UPDATE`, or `DELETE`.
- Keep business logic out of `.xaml.cs` code-behind.
- End async methods with `Async`.
- When rebuilding a full `ObservableCollection`, prefer replacing the collection instance instead
    of `Clear()` plus repeated `Add()` calls.

## Major Assumptions

If implementation would require a major assumption, pause and ask the user first with the same
chat-facing `vscode_askQuestions` approval flow described in
`.github/instructions/workflow/prompt-engineer-every-message.instructions.md`.

Examples of major assumptions:

- choosing one of several valid implementations when behavior is ambiguous
- inferring missing stored procedures, contracts, or schema details
- deciding ownership between modules, services, or DAOs without direct evidence
- changing workflow scope, navigation, or validation behavior without explicit direction

Do not create an assumption file by default. Use the chat approval flow unless the user asks for a
file artifact.

## Required Workflow

1. Read the smallest relevant instruction set for the current task.
2. Inspect the owning code path before editing.
3. Make the smallest grounded change that tests the current hypothesis.
4. Run the narrowest validation available after the first substantive edit.
5. Update documentation or metadata when the change invalidates an active source-of-truth file.

## Instruction Map

Use the category that matches your task instead of reading the whole instruction tree.

- `.github/instructions/architecture/` — MVVM, DAO, CQRS, dialogs, settings, converters, and
    WinUI design constraints
- `.github/instructions/database/` — MySQL stored procedures and Infor Visual query rules
- `.github/instructions/documentation/` — Markdown, spec slices, doc maintenance, and doc updates
- `.github/instructions/languages/` — C#, PowerShell, Python, shell, and related file-specific
    conventions
- `.github/instructions/quality/` — review rules, security, performance, code quality, comments
- `.github/instructions/testing/` — test strategy and test-writing expectations
- `.github/instructions/tooling/` — MCP, WinApp, skill authoring, and Serena subfolder guidance
- `.github/instructions/workflow/` — prompt engineering, research, agent authoring, and process
- `.github/instructions/copilotforms/` — CopilotForms export-specific workflows and handlers

Recommended starting points for common work:

- C# or XAML changes: `.github/instructions/languages/csharp.instructions.md`
- MVVM or service-layer work: `.github/instructions/architecture/mvvm-pattern.instructions.md`
- DAO changes: `.github/instructions/architecture/dao-pattern.instructions.md`
- MySQL or Infor Visual work:
    `.github/instructions/database/sql-sp-generation.instructions.md` and
    `.github/instructions/database/infor-visual-query-authoring.instructions.md`
- Tests: `.github/instructions/testing/testing-strategy.instructions.md`
- Tooling-heavy agent work: `.github/instructions/tooling/mcp-tooling.instructions.md`

## Documentation And Metadata Maintenance

- If you change active repository guidance, update the matching index or source-of-truth file.
- If you change `.github` structure, update `.github/README.md` and relevant category READMEs.
- If code changes affect CopilotForms workflow understanding, update
    `docs/CopilotForms/data/copilot-forms.config.json` or the split module metadata files.
- Prefer one authoritative file per topic. Delete, archive, or merge duplicates instead of
    maintaining parallel guidance.

## Validation Expectations

- Use the narrowest executable validation available for the changed slice.
- Prefer targeted tests or focused build/test tasks over full-solution checks when possible.
- If no executable validation exists for documentation-only work, validate by checking links,
    paths, and workspace configuration consistency.

## Related Files

- `README.md` — project overview and onboarding entry point
- `.github/README.md` — AI customization taxonomy and maintenance map
- `AGENTS.md` — repository agent contract and execution profile
- `.github/prompts/README.md` — active prompt taxonomy and usage
- `.github/instructions/README.md` — active instruction taxonomy and starting points

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

**Service Registration Pattern (add to `Infrastructure/DependencyInjection/` extension methods, NOT directly in `App.xaml.cs`):**

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

## User Interface Guidelines

### User Management Page

- For the Core Settings user management page, user card hover should show a grey underline below the card.
- Selected cards should show a blue underline, not a full-card highlight.

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
7. Register in `Infrastructure/DependencyInjection/` extension methods as Transient

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
6. Register in `Infrastructure/DependencyInjection/` extension methods

### Creating New DAO

**Steps:**

1. Make it instance-based (never static)
2. Accept `connectionString` in constructor with null check
3. Use stored procedures via `Helper_Database_StoredProcedure`
4. Return `Model_Dao_Result` or `Model_Dao_Result<T>`
5. Never throw exceptions - return failure results
6. Use `MySqlParameter[]` for parameter mapping
7. Register as Singleton in `Infrastructure/DependencyInjection/` extension methods

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
        await _errorHandler.ShowUserErrorAsync(
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

## Key Interfaces and Base_classes

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

- [ ] DI registration in `Infrastructure/DependencyInjection/` extension methods
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

## User Environment Consideration

- Avoid using terminal commands for validation as running scripts via the terminal locks up the terminal; prefer non-terminal tooling (file reads/search) for validation.

## XLSX Creation for Multi-User Access

- When creating XLSX files, implement strategies to support multi-user access.
- Use `FileShare.ReadWrite` or shared access strategies to allow concurrent edits without conflicts.

---
description: 'MTM Receiving Application development guidelines - MVVM architecture, database patterns, naming conventions, and WinUI 3 best practices'
applyTo: '**/*.{cs,xaml,csproj,vb,fs,sql,md,txt,ps1,sh,bash,cmd,bat,py,js,ts,jsx,tsx,html,htm,css,scss,json,yaml,yml,xml,config,toml,ini,env,props,targets}'
---

# MTM Receiving Application Development Guide

Manufacturing receiving operations desktop application for streamlined label generation, workflow management, and ERP integration.

## 🚨 CRITICAL ARCHITECTURE RULES - READ FIRST

### FORBIDDEN - These Will Break the System

**❌ NEVER DO THESE:**
1. **ViewModels calling DAOs directly** - MUST go through Service layer
2. **ViewModels accessing `Helper_Database_*` classes** - Use services only
3. **Static DAO classes** - All DAOs MUST be instance-based
4. **DAOs throwing exceptions** - Return `Model_Dao_Result` with error details
5. **Raw SQL in C# for MySQL** - Use stored procedures ONLY
6. **Write operations to SQL Server/Infor Visual** - READ ONLY (no INSERT/UPDATE/DELETE)
7. **Runtime `{Binding}` in XAML** - Use compile-time `{x:Bind}` only
8. **Business logic in `.xaml.cs` code-behind** - Belongs in ViewModel or Service

### REQUIRED - Every Component Must Follow

**✅ ALWAYS DO THESE:**
1. **MVVM Layer Flow:** View (XAML) → ViewModel → Service → DAO → Database
2. **ViewModels:** Partial classes inheriting from `ViewModel_Shared_Base`
3. **Services:** Interface-based with dependency injection
4. **DAOs:** Instance-based, injected via constructor, return `Model_Dao_Result`
5. **XAML Bindings:** Use `x:Bind` with explicit `Mode` (OneWay/TwoWay/OneTime)
6. **Async Methods:** All must end with `Async` suffix
7. **Error Handling:** DAOs return errors, Services handle them, ViewModels display them
8. **Database Access:** MySQL via stored procedures, SQL Server READ ONLY

### Important - Follow These Guidelines

1. **Reference Relevant Instruction Files:** Follow guidelines in `.github/instructions/` as applicable, making sure that you reference these as if the user had included them in their prompt.
- See the "Additional Resources" section below for a list of relevant instruction files.
- Reference the specific instruction files for detailed guidance on each topic.
- Follow the naming conventions and folder structures outlined in the project governance documents.
- **C# & XAML Naming:** `specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md`
- **SQL Server Naming:** `specs/Module_Receiving/03-Implementation-Blueprint/sql-naming-conventions-extended.md`
- **Database Project Setup:** `.github/instructions/database-project-integration.instructions.md`
- If you do not need to reference any instruction files for a specific task, you must explicitly state that no custom instruction files are needed for that task.

## Technology Stack

- **Framework:** WinUI 3 (Windows App SDK 1.6+)
- **Language:** C# 12
- **Platform:** .NET 8
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Database:** 
- **SQL Server (LocalDB/Express)** - Primary database for all new modules (READ/WRITE)
- **MySQL 8.0 (MAMP)** - Legacy database (transitioning to SQL Server)
- **SQL Server/Infor Visual** - ERP integration (READ ONLY)
- **Testing:** xUnit with FluentAssertions
- **DI Container:** Microsoft.Extensions.DependencyInjection

## Additional Resources

### Project Governance
- Project Constitution: See `.github/CONSTITUTION.md` for immutable architecture rules
- Agent Definitions: See `.github/AGENTS.md` for specialized AI agents
- Memory Bank: See `memory-bank/` folder for project context and task tracking

### Instruction Files
The `.github/instructions/` folder contains specialized guidance for specific scenarios. Reference these when applicable:

**Core Development:**
- `.github/instructions/csharp.instructions.md` - C# language-specific best practices
- `.github/instructions/dotnet-architecture-good-practices.instructions.md` - .NET architecture patterns
- `.github/instructions/testing-strategy.instructions.md` - Comprehensive testing guide
- `.github/instructions/code-review-generic.instructions.md` - Code review guidelines

**Code Quality:**
- `.github/instructions/object-calisthenics.instructions.md` - Code quality rules and patterns
- `.github/instructions/self-explanatory-code-commenting.instructions.md` - Commenting standards
- `.github/instructions/security-and-owask.instructions.md` - Security best practices

**Database & SQL:**
- `.github/instructions/sql-sp-generation.instructions.md` - Stored procedure generation guidelines

**Performance & Optimization:**
- `.github/instructions/performance-optimization.instructions.md` - Performance tuning guidance

**Scripting:**
- `.github/instructions/powershell.instructions.md` - PowerShell scripting standards
- `.github/instructions/powershell-scripting-ai.instructions.md` - AI-assisted PowerShell development
- `.github/instructions/powershell-pester-5.instructions.md` - PowerShell testing with Pester 5
- `.github/instructions/shell.instructions.md` - Shell scripting guidelines
- `.github/instructions/python.instructions.md` - Python development standards

**Workflow & Process:**
- `.github/instructions/memory-bank.instructions.md` - Memory bank usage and maintenance
- `.github/instructions/spec-driven-workflow-v1.instructions.md` - Specification-driven development
- `.github/instructions/update-docs-on-code-change.instructions.md` - Documentation update process
- `.github/instructions/module-doc-maintenance.instructions.md` - Module documentation standards

**AI Agent & Automation:**
- `.github/instructions/comprehensive-research.instructions.md` - Research workflow for documentation/code generation
- `.github/instructions/agents.instructions.md` - AI agent configuration and usage
- `.github/instructions/agent-skills.instructions.md` - AI agent capabilities reference
- `.github/instructions/joyride-workspace-automation.instructions.md` - Workspace automation
- `.github/instructions/serena-tools.instructions.md` - Serena tooling reference
- `.github/instructions/taming-copilot.instructions.md` - Copilot interaction patterns
- `.github/instructions/prompt.instructions.md` - Prompt engineering guidelines
- `.github/instructions/instructions.instructions.md` - Meta-instructions for instruction files

**Specialized:**
- `.github/instructions/dotnet-upgrade.instructions.md` - .NET upgrade procedures
- `.github/instructions/arrogant-code-review.instructions.md` - Assertive code review mode

## Naming Conventions

All classes follow the **5-Part Naming Standard:** `{Type}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

### **Type Guidelines:**
- **Type** = What kind of class (ViewModel, View, Service, Model, DAO, Helper, Enum)
- **Module** = Which module/domain (Receiving, Shared, Database, etc.)
- **Mode** = Context or variant (Wizard, Consolidated, EditMode, etc.)
- **CategoryType** = Why it exists / what category (Orchestration, Display, Entity, Repository, etc.)
- **DescriptiveName** = EXACTLY what it represents (MUST be clear and specific, NOT vague)

### **1. ViewModels:** `ViewModel_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode Examples:** `Wizard` (12-step), `Consolidated` (3-step), `EditMode`
- **CategoryType:** `Orchestration_`, `Display_`, `Dialog_`, `Panel_`, `Interaction_`
- **Examples (CLEAR - NOT vague):**
  - `ViewModel_Receiving_Wizard_Orchestration_MainWorkflow`
  - `ViewModel_Receiving_Wizard_Display_LoadEntryGrid`
  - `ViewModel_Receiving_Consolidated_Display_CopyPreviewPanel`
  - `ViewModel_Receiving_EditMode_Interaction_EditModeHandler`

### **2. Views:** `View_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode Examples:** `Wizard` (12-step), `Consolidated` (3-step), `EditMode`
- **CategoryType:** `Orchestration_`, `Display_`, `Dialog_`, `Panel_`
- **Examples (CLEAR - NOT vague):**
  - `View_Receiving_Wizard_Orchestration_WorkflowConsolidated`
  - `View_Receiving_Wizard_Display_LoadEntryGrid`
  - `View_Receiving_Consolidated_Dialog_CopyPreviewConfirmation`

### **3. Services:** `Service_{Module}_{Mode}_{CategoryType}_{DescriptiveName}` + `IService_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode Examples:** Can be shared across modes or mode-specific
- **CategoryType:** `Orchestration_`, `Business_`, `Infrastructure_`, `Utility_`
- **Examples (CLEAR - NOT vague):**
  - `Service_Receiving_Wizard_Orchestration_WorkflowStateMachine` / `IService_Receiving_Wizard_Orchestration_WorkflowStateMachine`
  - `Service_Receiving_Business_LoadValidation` / `IService_Receiving_Business_LoadValidation`
  - `Service_Receiving_Infrastructure_CSVExporter` / `IService_Receiving_Infrastructure_CSVExporter`

### **4. Models:** `Model_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode:** Often omitted for shared models, or included if mode-specific
- **CategoryType:** `Entity_`, `DTO_`, `Result_`, `Request_`, `Response_`
- **Examples (CLEAR - NOT vague):**
  - `Model_Receiving_Entity_WorkflowSession`
  - `Model_Receiving_Wizard_Entity_LoadDetail`
  - `Model_Receiving_Result_CopyOperationResult`
  - `Model_Receiving_DTO_ValidationError`
  - `Model_Receiving_Request_SaveWorkflowRequest`

### **5. DAOs:** `Dao_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode:** Often omitted for shared DAOs
- **CategoryType:** `Repository_`, `DataAccess_`, `Query_`
- **Examples (CLEAR - NOT vague):**
  - `Dao_Receiving_Repository_WorkflowSession`
  - `Dao_Receiving_Repository_LoadDetail`
  - `Dao_Receiving_DataAccess_CompletedTransaction`

### **6. Helpers:** `Helper_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode:** Often omitted for shared helpers
- **CategoryType:** `Database_`, `FileIO_`, `Validation_`, `Transformation_`, `Infrastructure_`
- **Examples (CLEAR - NOT vague):**
  - `Helper_Database_Infrastructure_StoredProcedureExecution`
  - `Helper_FileIO_Infrastructure_CSVWriterUtility`
  - `Helper_Validation_Business_DataFormatValidator`
  - `Helper_Transformation_Infrastructure_EntityMapper`

### **7. Enums:** `Enum_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Mode:** Often omitted for shared enums
- **CategoryType:** `Status_`, `Type_`, `State_`, `Mode_`, `Severity_`
- **Examples (CLEAR - NOT vague):**
  - `Enum_Receiving_State_WorkflowStep`
  - `Enum_Receiving_Type_CopyFieldSelection`
  - `Enum_Shared_Severity_ErrorSeverity`

### **CQRS Commands, Queries, Handlers, and Validators:**
- Pattern: `{Type}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- **Commands:** `Command_<Module>_<Mode>_<CategoryType>_<DescriptiveName>`
  - Examples:
    - `Command_Receiving_Wizard_Navigation_StartNewWorkflow`
    - `Command_Receiving_Wizard_Data_EnterOrderAndPart`
    - `Command_Receiving_Consolidated_Copy_FieldsToEmptyCells`
- **Queries:** `Query_<Module>_<Mode>_<CategoryType>_<DescriptiveName>`
  - Examples:
    - `Query_Receiving_Wizard_Get_CurrentSession`
    - `Query_Receiving_Wizard_Validate_CurrentStep`
    - `Query_Receiving_Consolidated_Preview_CopyOperation`
- **Handlers:** `Handler_<Module>_<Mode>_<CategoryType>_<DescriptiveName>`
  - Examples:
    - `Handler_Receiving_Wizard_Navigation_StartNewWorkflow`
    - `Handler_Receiving_Consolidated_Data_EnterOrderAndPart`
- **Validators:** `Validator_<Module>_<Mode>_<CategoryType>_<DescriptiveName>`
  - Examples:
    - `Validator_Receiving_Wizard_Data_EnterOrderAndPart`
    - `Validator_Receiving_Consolidated_Copy_FieldsToEmptyCells`

### **CategoryType Guidelines for CQRS:**
- `Navigation_*` = Step/workflow movement commands
- `Data_*` = Data entry or updates
- `Copy_*` = Copy or bulk operations
- `Get_*` = Data retrieval queries
- `Validate_*` = Validation queries
- `Preview_*` = Preview or simulation queries

**Methods:**
- PascalCase for all methods
- **NEW:** Methods SHOULD follow 5-part naming: `{Action}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}Async`
- Async methods MUST end with `Async`: `Load_Receiving_Wizard_Data_PONumberAsync()`, `Save_Receiving_Database_Transaction_ReceivingLineAsync()`
- **Legacy/Simple methods** MAY use short PascalCase if appropriate: `LoadDataAsync()`, `SaveAsync()` (but 5-part is preferred)
- DAO methods: Follow 5-part pattern with database action: `Insert_Receiving_Database_Record_ReceivingLineAsync()`
- See `specs/Module_Receiving/03-Implementation-Blueprint/csharp-xaml-naming-conventions-extended.md` for complete method naming guide

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
public partial class ViewModel_Receiving_Wizard_Display_PONumberEntry : ViewModel_Shared_Base
{
    private readonly IService_Receiving_Business_MySQL_ReceivingLine _receivingLineService;

    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_Receiving_Entity_ReceivingLine> _receivingLines = new();

    public ViewModel_Receiving_Wizard_Display_PONumberEntry(
        IService_Receiving_Business_MySQL_ReceivingLine receivingLineService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _receivingLineService = receivingLineService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";

            var result = await _receivingLineService.GetReceivingLineAsync(_poNumber);
            if (result.IsSuccess)
            {
                ReceivingLines.Clear();
                foreach (var line in result.Data)
                {
                    ReceivingLines.Add(line);
                }
                StatusMessage = $"Loaded {ReceivingLines.Count} lines";
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
                nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
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
public partial class ViewModel_Receiving_Wizard_Display_BadExample : ViewModel_Shared_Base
{
    private async Task LoadAsync()
    {
        // NEVER DO THIS - Direct DAO call is forbidden
        var result = await _receivingLineDao.GetLinesAsync(_poNumber);
    }
}
```

### Service Layer Pattern

```csharp
// ✅ CORRECT - Service provides business logic abstraction
public interface IService_Receiving_Business_MySQL_ReceivingLine
{
    Task<Model_Dao_Result> InsertLineAsync(Model_Receiving_Entity_ReceivingLine line);
    Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>> GetLinesByPOAsync(string poNumber);
}

public class Service_Receiving_Business_MySQL_ReceivingLine : IService_Receiving_Business_MySQL_ReceivingLine
{
    private readonly Dao_Receiving_Repository_ReceivingLine _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_Receiving_Business_MySQL_ReceivingLine(
        Dao_Receiving_Repository_ReceivingLine dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result> InsertLineAsync(Model_Receiving_Entity_ReceivingLine line)
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
public class Dao_Receiving_Repository_ReceivingLine
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_ReceivingLine(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line)
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
public static class Dao_Receiving_Repository_ReceivingLine
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
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Wizard_Display_PONumberEntry"
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
                    <DataTemplate x:DataType="models:Model_Receiving_Entity_ReceivingLine">
                        <TextBlock Text="{x:Bind PONumber}" />
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
services.AddSingleton(sp => new Dao_Receiving_Repository_ReceivingLine(connectionString));
services.AddSingleton(sp => new Dao_Receiving_Repository_ReceivingLoad(connectionString));

// Services as Singletons (business logic)
services.AddSingleton<IService_Receiving_Business_MySQL_ReceivingLine, Service_Receiving_Business_MySQL_ReceivingLine>();
services.AddSingleton<IService_Receiving_Business_MySQL_ReceivingLoad, Service_Receiving_Business_MySQL_ReceivingLoad>();

// ViewModels as Transient (new instance per navigation)
services.AddTransient<ViewModel_Receiving_Wizard_Display_PONumberEntry>();
services.AddTransient<ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();

// Views as Transient
services.AddTransient<View_Receiving_Wizard_Display_PONumberEntry>();
services.AddTransient<View_Receiving_Wizard_Orchestration_MainWorkflow>();
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

public class ViewModel_Receiving_Wizard_Display_PONumberEntryTests
{
    private readonly Mock<IService_Receiving_Business_MySQL_ReceivingLine> _mockReceivingLineService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly ViewModel_Receiving_Wizard_Display_PONumberEntry _viewModel;

    public ViewModel_Receiving_Wizard_Display_PONumberEntryTests()
    {
        _mockReceivingLineService = new Mock<IService_Receiving_Business_MySQL_ReceivingLine>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();
        
        _viewModel = new ViewModel_Receiving_Wizard_Display_PONumberEntry(
            _mockReceivingLineService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task LoadDataAsync_ShouldPopulateLines_WhenServiceReturnsSuccess()
    {
        // Arrange
        var expectedData = new List<Model_Receiving_Entity_ReceivingLine> 
        { 
            new Model_Receiving_Entity_ReceivingLine { PONumber = "PO-001" } 
        };
        _mockReceivingLineService
            .Setup(m => m.GetReceivingLineAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>> 
            { 
                IsSuccess = true, 
                Data = expectedData 
            });

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        _viewModel.ReceivingLines.Should().HaveCount(1);
        _viewModel.ReceivingLines[0].PONumber.Should().Be("PO-001");
    }
}
```

### Integration Test Pattern

```csharp
using FluentAssertions;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Receiving.Integration;

[Collection("Database")]
public class Dao_Receiving_Repository_ReceivingLineIntegrationTests : IAsyncLifetime
{
    private readonly Dao_Receiving_Repository_ReceivingLine _dao;
    private Guid _testLineId;

    public Dao_Receiving_Repository_ReceivingLineIntegrationTests()
    {
        var connectionString = Helper_Database_Variables.GetConnectionString();
        _dao = new Dao_Receiving_Repository_ReceivingLine(connectionString);
    }

    public async Task InitializeAsync()
    {
        var testLine = new Model_Receiving_Entity_ReceivingLine
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

**Example:**
```csharp
// File: ViewModel_Receiving_Wizard_Display_MyFeature.cs
public partial class ViewModel_Receiving_Wizard_Display_MyFeature : ViewModel_Shared_Base
{
    private readonly IService_Receiving_Business_MySQL_ReceivingLine _service;

    [ObservableProperty]
    private string _myProperty = string.Empty;

    public ViewModel_Receiving_Wizard_Display_MyFeature(
        IService_Receiving_Business_MySQL_ReceivingLine service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _service = service;
    }

    [RelayCommand]
    private async Task MyCommandAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var result = await _service.DoSomethingAsync();
            if (!result.IsSuccess)
                _errorHandler.ShowUserError(result.ErrorMessage, "Error", nameof(MyCommandAsync));
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(MyCommandAsync), nameof(ViewModel_Receiving_Wizard_Display_MyFeature));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Creating New View

**Steps:**
1. Use `x:Bind` for all data binding
2. Set `Mode` (`OneWay`, `TwoWay`, `OneTime`)
3. Use `UpdateSourceTrigger=PropertyChanged` for TwoWay TextBox bindings
4. No business logic in code-behind
5. Set window size with `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, width, height)`

**Example:**
```xaml
<!-- File: View_Receiving_Wizard_Display_MyFeature.xaml -->
<Page
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Wizard_Display_MyFeature"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid Padding="20">
        <StackPanel Spacing="10">
            <TextBox
                Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="Enter value..." />
            <Button
                Content="Execute"
                Command="{x:Bind ViewModel.MyCommandCommand}" />
        </StackPanel>
    </Grid>
</Page>
```

### Creating New Service

**Steps:**
1. Create interface as `IService_Receiving_Business_*` or `IService_Receiving_Infrastructure_*`
2. Implement in `Services/` folder matching the interface name
3. Inject DAOs and dependencies via constructor
4. Add logging for key operations
5. Return `Model_Dao_Result` or appropriate types
6. Register in `App.xaml.cs`

**Example:**
```csharp
// File: IService_Receiving_Business_MySQL_MyFeature.cs
public interface IService_Receiving_Business_MySQL_MyFeature
{
    Task<Model_Dao_Result<List<Model_Receiving_Entity_MyData>>> GetDataAsync();
}

// File: Service_Receiving_Business_MySQL_MyFeature.cs
public class Service_Receiving_Business_MySQL_MyFeature : IService_Receiving_Business_MySQL_MyFeature
{
    private readonly Dao_Receiving_Repository_MyData _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_Receiving_Business_MySQL_MyFeature(
        Dao_Receiving_Repository_MyData dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_MyData>>> GetDataAsync()
    {
        _logger.LogInfo("Fetching data");
        return await _dao.GetDataAsync();
    }
}
```

### Creating New DAO

**Steps:**
1. Make it instance-based (never static)
2. Accept `connectionString` in constructor with null check
3. Use stored procedures via `Helper_Database_StoredProcedure`
4. Return `Model_Dao_Result` or `Model_Dao_Result<T>`
5. Never throw exceptions - return failure results
6. Use `MySqlParameter[]` for parameter mapping
7. Register as Singleton in `App.xaml.cs`

**Example:**
```csharp
// File: Dao_Receiving_Repository_MyData.cs
public class Dao_Receiving_Repository_MyData
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_MyData(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_MyData>>> GetDataAsync()
    {
        try
        {
            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_GetMyData",
                new MySqlParameter[] { },
                _connectionString);
            
            return result.IsSuccess 
                ? new Model_Dao_Result<List<Model_Receiving_Entity_MyData>> { IsSuccess = true, Data = result.Data }
                : Model_Dao_Result<List<Model_Receiving_Entity_MyData>>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<List<Model_Receiving_Entity_MyData>>.Failure($"Error: {ex.Message}");
        }
    }
}


# MASTER PROMPT: Module_Receiving Ground-Up Rebuild with Modern Architecture

> **Version:** 2.0.0
> **Date:** January 15, 2026
> **Target:** AI Coding Assistants (GitHub Copilot, Claude, GPT-4)
> **Complexity:** Enterprise-Grade Modular Rebuild
> **Estimated Effort:** 6-8 weeks (including testing & documentation)

---

## 🎯 Mission Statement

You are tasked with rebuilding **Module_Receiving** from the ground up using modern architectural patterns, industry-standard libraries, and best practices. The goal is to create a highly modular, testable, and maintainable codebase that reduces service bloat in Module_Core while maintaining strict constitutional compliance.

This is not a simple refactor—this is a **complete architectural redesign** of the Receiving module to serve as a blueprint for modernizing the entire MTM Receiving Application.

---

## 📖 Context & Background

### Current State Analysis

**Problems to Solve:**
1. **Service Bloat in Module_Core** - Too many Receiving-specific services in shared infrastructure
2. **Tight Coupling** - ViewModels, Services, and DAOs are not properly abstracted
3. **Limited Modularity** - Module_Receiving is not truly self-contained
4. **Testing Challenges** - Hard to test due to tight coupling and lack of interfaces
5. **Maintenance Burden** - Changes to Receiving logic ripple across Module_Core

**Current Architecture:**
- **Pattern:** MVVM with CommunityToolkit.Mvvm
- **Services:** 15+ services in Module_Core (many Receiving-specific)
- **Navigation:** Custom workflow state machine in `Service_ReceivingWorkflow`
- **Validation:** Mix of ViewModel, Service, and custom validation services
- **Logging:** Custom `Service_LoggingUtility` (file-based)
- **Error Handling:** `Service_ErrorHandler` (shared across all modules)

**What Works Well:**
- ✅ Strict MVVM architecture with `partial` ViewModels
- ✅ Instance-based DAOs returning `Model_Dao_Result`
- ✅ Dependency Injection via `App.xaml.cs`
- ✅ WinUI 3 with `x:Bind` compile-time binding
- ✅ Stored procedures for all MySQL operations

**What Needs Improvement:**
- ⚠️ Service layer is monolithic (1 service = 10+ methods)
- ⚠️ Navigation is custom and tightly coupled
- ⚠️ Validation logic is scattered across multiple layers
- ⚠️ Logging lacks structured context
- ⚠️ CSV export uses custom writer (not leveraging existing libraries)

---

## 🏗️ Target Architecture

### Modern Architecture Stack

**Adopted Patterns:**
1. **CQRS (Command Query Responsibility Segregation)** via MediatR
2. **Structured Logging** via Serilog with file sinks
3. **Declarative Validation** via FluentValidation
4. **Type-Safe CSV Export** via CsvHelper
5. **Optional: Resilience Patterns** via Polly (retry policies)

**Library Selection (Research-Based):**

| Category | Library | Downloads | Justification |
|----------|---------|-----------|---------------|
| **Mediator (CQRS)** | MediatR 12.0+ | 607M+ | Industry standard, reduces service file count |
| **Logging** | Serilog 3.1+ | 2.3B+ | Structured logging, excellent ecosystem |
| **Validation** | FluentValidation 11.8+ | 741M+ | Strongly-typed, composable validation rules |
| **CSV Export** | CsvHelper 30.0+ | 34M+ | Mature, feature-rich, replaces custom writer |
| **Navigation** | Uno.Extensions.Navigation.WinUI OR Custom | 553k / N/A | ViewModel-based navigation patterns |
| **DI Scanning** | Scrutor 4.2+ | N/A | Auto-registration for handlers/validators |
| **Resilience** | Polly 8.2+ | Optional | Retry/circuit breaker for database calls |

### Architectural Layers (Post-Rebuild)

```
┌─────────────────────────────────────────────────────────────┐
│                      Module_Receiving                        │
├─────────────────────────────────────────────────────────────┤
│ Views (XAML)                                                │
│   ├─ View_Receiving_ModeSelection.xaml                     │
│   ├─ View_Receiving_POEntry.xaml                           │
│   └─ ... (7 more views)                                    │
├─────────────────────────────────────────────────────────────┤
│ ViewModels (MVVM Logic)                                     │
│   ├─ ViewModel_Receiving_ModeSelection (partial)           │
│   │    └─ Injects: IMediator, ILogger<T>                   │
│   └─ ... (8 more ViewModels)                               │
├─────────────────────────────────────────────────────────────┤
│ Handlers (CQRS Commands/Queries) - NEW                      │
│   ├─ Queries/                                               │
│   │    ├─ GetReceivingLinesQuery.cs                        │
│   │    └─ GetReceivingLinesHandler.cs                      │
│   ├─ Commands/                                              │
│   │    ├─ InsertReceivingLineCommand.cs                    │
│   │    └─ InsertReceivingLineHandler.cs                    │
│   └─ Behaviors/ (Pipelines)                                 │
│        ├─ LoggingBehavior.cs                                │
│        ├─ ValidationBehavior.cs                             │
│        └─ TransactionBehavior.cs (optional)                 │
├─────────────────────────────────────────────────────────────┤
│ Validators (FluentValidation) - NEW                         │
│   ├─ ReceivingLineValidator.cs                             │
│   ├─ ReceivingLoadValidator.cs                             │
│   └─ ReceivingSessionValidator.cs                          │
├─────────────────────────────────────────────────────────────┤
│ Services (Orchestration & Navigation)                       │
│   ├─ Service_ReceivingWorkflow.cs (navigation only)        │
│   └─ Service_CSVExport<T>.cs (using CsvHelper)             │
├─────────────────────────────────────────────────────────────┤
│ Data (Instance-Based DAOs)                                  │
│   ├─ Dao_ReceivingLine.cs                                  │
│   ├─ Dao_ReceivingLoad.cs                                  │
│   └─ Dao_PackageTypePreference.cs                          │
├─────────────────────────────────────────────────────────────┤
│ Models (Data Transfer Objects)                              │
│   ├─ Model_ReceivingLine.cs                                │
│   ├─ Model_ReceivingLoad.cs                                │
│   └─ Model_ReceivingSession.cs                             │
├─────────────────────────────────────────────────────────────┤
│ Defaults (Configuration & Presets) - NEW                    │
│   ├─ Model_DefaultPackageTypes.cs                          │
│   ├─ Model_DefaultValidationRules.cs                       │
│   └─ Model_DefaultWorkflowSettings.cs                      │
└─────────────────────────────────────────────────────────────┘
```

### Data Flow (Before vs After)

**BEFORE (Service Pattern):**
```
ViewModel → Service_MySQL_ReceivingLine (10+ methods)
            └─> Dao_ReceivingLine
                └─> MySQL sp_*
```

**AFTER (CQRS Pattern):**
```
ViewModel → IMediator.Send(Query/Command)
            └─> Handler (1 responsibility)
                ├─> Validator (FluentValidation)
                ├─> Logger (Serilog pipeline)
                └─> Dao_ReceivingLine
                    └─> MySQL sp_*
```

**Benefits:**
- ✅ Each handler is a single class with one responsibility (SRP)
- ✅ Easy to add logging/validation via pipeline behaviors
- ✅ Handler classes are highly testable (mock IMediator)
- ✅ Reduces Service file count from 1 large file to N small handlers

---

## 📋 Constitutional Constraints (NON-NEGOTIABLE)

### Critical Rules from `.specify/memory/constitution.md`

**I. MVVM Architecture**
- ❌ **FORBIDDEN:** ViewModels calling DAOs directly
- ✅ **REQUIRED:** View → ViewModel → [Mediator →] Handler → DAO → Database
- ✅ **REQUIRED:** All ViewModels MUST be `partial` classes
- ✅ **REQUIRED:** Use `[ObservableProperty]` and `[RelayCommand]` attributes

**II. Database Layer**
- ❌ **FORBIDDEN:** Raw SQL in C# code (MySQL only)
- ✅ **REQUIRED:** All MySQL operations via stored procedures
- ✅ **REQUIRED:** All DAOs return `Model_Dao_Result<T>`
- ✅ **REQUIRED:** DAOs are instance-based (registered in DI)
- ⚠️ **WARNING:** SQL Server (Infor Visual) is **READ ONLY** - `ApplicationIntent=ReadOnly`

**III. Dependency Injection**
- ✅ **REQUIRED:** All services registered in `App.xaml.cs`
- ✅ **REQUIRED:** Constructor injection for all dependencies
- ❌ **FORBIDDEN:** Service locator pattern or static service access

**IV. Error Handling**
- ✅ **REQUIRED:** Use `IService_ErrorHandler` for user-facing errors
- ✅ **REQUIRED:** Use `ILogger<T>` (Serilog) for structured logging
- ❌ **FORBIDDEN:** DAOs throwing exceptions (return failure results)

**V. WinUI 3 Standards**
- ✅ **REQUIRED:** `x:Bind` for all XAML data binding (NOT `Binding`)
- ✅ **REQUIRED:** `async/await` for all I/O operations
- ✅ **REQUIRED:** `ObservableCollection<T>` for data-bound lists
- ✅ **REQUIRED:** Window sizing via `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(1400, 900)`

**VI. Code Quality**
- ✅ **REQUIRED:** Explicit accessibility modifiers (`private`, `public`, etc.)
- ✅ **REQUIRED:** Braces for all control flow statements (`if`, `for`, `while`)
- ✅ **REQUIRED:** Async methods end with `Async` suffix
- ✅ **REQUIRED:** XML documentation comments for all public APIs

**VII. Documentation Standards**
- ✅ **REQUIRED:** All diagrams use PlantUML (no ASCII art)
- ✅ **REQUIRED:** Update README.md, ARCHITECTURE.md when behavior changes
- ✅ **REQUIRED:** Task tracking in `tasks.md` with status updates

---

## 🛠️ Implementation Strategy

### Phase 1: Foundation & Setup (Week 1)

**Objective:** Install packages, create folder structure, configure DI

**Tasks:**
1. **Install NuGet Packages**
   ```powershell
   dotnet add package MediatR
   dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
   dotnet add package Serilog
   dotnet add package Serilog.Extensions.Logging
   dotnet add package Serilog.Sinks.File
   dotnet add package FluentValidation
   dotnet add package FluentValidation.DependencyInjectionExtensions
   dotnet add package CsvHelper
   dotnet add package Scrutor # Optional
   dotnet add package Polly # Optional
   ```

2. **Create Folder Structure**
   ```
   Module_Receiving/
   ├─ Defaults/         (NEW)
   ├─ Handlers/         (NEW)
   │  ├─ Queries/
   │  ├─ Commands/
   │  └─ Behaviors/
   ├─ Validators/       (NEW)
   ├─ Data/            (Existing - verify)
   ├─ Models/          (Existing - review)
   ├─ Services/        (Reduce to navigation only)
   ├─ ViewModels/      (Refactor - inject IMediator)
   └─ Views/           (Minimal changes)
   ```

3. **Configure Serilog in `App.xaml.cs`**
   ```csharp
   // In App.xaml.cs ConfigureServices method
   Log.Logger = new LoggerConfiguration()
       .WriteTo.File(
           path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "receiving-.txt"),
           rollingInterval: RollingInterval.Day,
           outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
       .CreateLogger();
   
   services.AddLogging(builder => builder.AddSerilog(dispose: true));
   ```

4. **Register MediatR**
   ```csharp
   services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(App).Assembly));
   
   // Add pipeline behaviors
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
   services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
   ```

5. **Register FluentValidation**
   ```csharp
   services.AddValidatorsFromAssemblyContaining<ReceivingLineValidator>();
   ```

**Deliverables:**
- ✅ Packages installed
- ✅ Folder structure created
- ✅ DI configured for MediatR, Serilog, FluentValidation
- ✅ Logging outputs to `logs/receiving-YYYYMMDD.txt`

---

### Phase 2: Models & Validation (Week 1-2)

**Objective:** Review existing models, create FluentValidation validators

**Tasks:**
1. **Review Existing Models**
   - `Model_ReceivingLine.cs`
   - `Model_ReceivingLoad.cs`
   - `Model_ReceivingSession.cs`
   - `Model_PackageTypePreference.cs`
   - `Model_InforVisualPO.cs`
   - `Model_InforVisualPart.cs`

2. **Create Validators (FluentValidation)**
   ```csharp
   // Module_Receiving/Validators/ReceivingLineValidator.cs
   public class ReceivingLineValidator : AbstractValidator<Model_ReceivingLine>
   {
       public ReceivingLineValidator()
       {
           RuleFor(x => x.Quantity)
               .GreaterThan(0)
               .WithMessage("Quantity must be greater than 0");
           
           RuleFor(x => x.PartID)
               .NotEmpty()
               .WithMessage("Part ID is required");
           
           RuleFor(x => x.PONumber)
               .NotEmpty()
               .Length(1, 50)
               .WithMessage("PO Number must be 1-50 characters");
       }
   }
   ```

3. **Create Default Configuration Models**
   ```csharp
   // Module_Receiving/Defaults/Model_DefaultPackageTypes.cs
   public static class Model_DefaultPackageTypes
   {
       public static readonly List<string> StandardTypes = new()
       {
           "Standard Box",
           "Pallet",
           "Custom"
       };
   }
   ```

**Deliverables:**
- ✅ All models reviewed and documented
- ✅ FluentValidation validators for each model
- ✅ Unit tests for validators

---

### Phase 3: CQRS Handlers (Week 2-3)

**Objective:** Replace Service methods with MediatR handlers

**Migration Pattern:**

**BEFORE (Service):**
```csharp
public class Service_MySQL_ReceivingLine
{
    public async Task<Model_Dao_Result> InsertLineAsync(Model_ReceivingLine line) { }
    public async Task<Model_Dao_Result> UpdateLineAsync(Model_ReceivingLine line) { }
    public async Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetLinesByLoadAsync(int loadId) { }
    public async Task<Model_Dao_Result> DeleteLineAsync(int lineId) { }
    // ... 10+ more methods
}
```

**AFTER (CQRS Handlers):**
```csharp
// Queries/GetReceivingLinesQuery.cs
public record GetReceivingLinesQuery(int LoadId) : IRequest<Model_Dao_Result<List<Model_ReceivingLine>>>;

// Queries/GetReceivingLinesHandler.cs
public class GetReceivingLinesHandler : IRequestHandler<GetReceivingLinesQuery, Model_Dao_Result<List<Model_ReceivingLine>>>
{
    private readonly Dao_ReceivingLine _dao;
    private readonly ILogger<GetReceivingLinesHandler> _logger;
    
    public async Task<Model_Dao_Result<List<Model_ReceivingLine>>> Handle(
        GetReceivingLinesQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving receiving lines for LoadID: {LoadId}", request.LoadId);
        return await _dao.GetLinesByLoadAsync(request.LoadId);
    }
}

// Commands/InsertReceivingLineCommand.cs
public record InsertReceivingLineCommand(Model_ReceivingLine Line) : IRequest<Model_Dao_Result>;

// Commands/InsertReceivingLineHandler.cs
public class InsertReceivingLineHandler : IRequestHandler<InsertReceivingLineCommand, Model_Dao_Result>
{
    private readonly Dao_ReceivingLine _dao;
    private readonly IValidator<Model_ReceivingLine> _validator;
    private readonly ILogger<InsertReceivingLineHandler> _logger;
    
    public async Task<Model_Dao_Result> Handle(
        InsertReceivingLineCommand command, 
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command.Line, cancellationToken);
        if (!validation.IsValid)
        {
            return Model_Dao_Result.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }
        
        _logger.LogInformation("Inserting receiving line for PO: {PONumber}", command.Line.PONumber);
        return await _dao.InsertReceivingLineAsync(command.Line);
    }
}
```

**ViewModel Usage:**
```csharp
public partial class ViewModel_Receiving_Review : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    
    [RelayCommand]
    private async Task LoadLinesAsync()
    {
        var result = await _mediator.Send(new GetReceivingLinesQuery(LoadId));
        if (result.IsSuccess)
        {
            Lines.Clear();
            foreach (var line in result.Data)
                Lines.Add(line);
        }
    }
}
```

**Tasks:**
1. Create `Queries/` folder with Query and Handler pairs
2. Create `Commands/` folder with Command and Handler pairs
3. Create pipeline behaviors (Logging, Validation, optional Transaction)
4. Update ViewModels to inject `IMediator` instead of services
5. Remove old Service files (or mark as obsolete)

**Deliverables:**
- ✅ All Service methods migrated to handlers
- ✅ ViewModels updated to use IMediator
- ✅ Pipeline behaviors working (logging, validation)
- ✅ Unit tests for all handlers

---

### Phase 4: ViewModels & Navigation (Week 3-4)

**Objective:** Refactor ViewModels to use IMediator, implement navigation

**ViewModel Refactor Pattern:**

**BEFORE:**
```csharp
public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base
{
    private readonly IService_MySQL_ReceivingLine _lineService;
    private readonly IService_LoggingUtility _logger;
    
    public ViewModel_Receiving_POEntry(
        IService_MySQL_ReceivingLine lineService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _lineService = lineService;
        _logger = logger;
    }
    
    [RelayCommand]
    private async Task LoadLinesAsync()
    {
        _logger.LogInfo("Loading lines");
        var result = await _lineService.GetLinesByPOAsync(PONumber);
        // ...
    }
}
```

**AFTER:**
```csharp
public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly ILogger<ViewModel_Receiving_POEntry> _logger;
    
    public ViewModel_Receiving_POEntry(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        ILogger<ViewModel_Receiving_POEntry> logger) : base(errorHandler, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [RelayCommand]
    private async Task LoadLinesAsync()
    {
        _logger.LogInformation("Loading lines for PO: {PONumber}", PONumber);
        var result = await _mediator.Send(new GetReceivingLinesByPOQuery(PONumber));
        if (result.IsSuccess)
        {
            Lines.Clear();
            foreach (var line in result.Data)
                Lines.Add(line);
        }
        else
        {
            _errorHandler.ShowUserError(result.ErrorMessage, "Load Error", nameof(LoadLinesAsync));
        }
    }
}
```

**Navigation Strategy:**

**Option A: Keep Custom Service (Simplest)**
- Keep `Service_ReceivingWorkflow` for navigation
- Remove data access logic from it
- Focus on step management and navigation only

**Option B: Use Uno.Extensions.Navigation**
- Install `Uno.Extensions.Navigation.WinUI`
- Define routes for each view
- Use ViewModel-based navigation

**Recommendation:** Start with Option A (custom) for MVP, migrate to Option B later if needed.

**Deliverables:**
- ✅ All 9 ViewModels refactored to use IMediator
- ✅ Logging updated to use ILogger<T>
- ✅ Navigation working (custom or Uno.Extensions)
- ✅ All ViewModels registered in DI as Transient

---

### Phase 5: Services Cleanup (Week 4)

**Objective:** Remove/relocate Receiving-specific services from Module_Core

**Services to Migrate/Remove:**

1. **Service_MySQL_ReceivingLine** → ✅ Replaced by MediatR handlers
2. **Service_ReceivingValidation** → ✅ Replaced by FluentValidation validators
3. **Service_CSVWriter** → ✅ Replaced by CsvHelper-based Service_CSVExport<T>
4. **Service_LoggingUtility** → ✅ Replaced by Serilog ILogger<T>

**Services to Keep in Module_Core (Shared):**
- ✅ `IService_ErrorHandler` - Used by all modules
- ✅ `IService_Window` - Used by all modules
- ✅ `IService_Dispatcher` - Used by all modules

**Tasks:**
1. Create `Service_CSVExport<T>` using CsvHelper
   ```csharp
   public class Service_CSVExport<T> : IService_CSVExport<T>
   {
       public async Task ExportAsync(IEnumerable<T> data, string filePath)
       {
           using var writer = new StreamWriter(filePath);
           using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
           await csv.WriteRecordsAsync(data);
       }
   }
   ```

2. Remove obsolete services from Module_Core
3. Update `App.xaml.cs` DI registrations
4. Update all references to removed services

**Deliverables:**
- ✅ Receiving-specific services removed from Module_Core
- ✅ Generic CSV export service created
- ✅ All DI registrations updated
- ✅ No compilation errors

---

### Phase 6: Testing & Documentation (Week 5)

**Objective:** Achieve 80% test coverage, update documentation

**Testing Strategy:**

**Unit Tests (ViewModel):**
```csharp
[Fact]
public async Task LoadLinesCommand_Success_PopulatesItems()
{
    // Arrange
    var mockMediator = new Mock<IMediator>();
    mockMediator.Setup(m => m.Send(It.IsAny<GetReceivingLinesQuery>(), default))
        .ReturnsAsync(Model_Dao_Result.Success(new List<Model_ReceivingLine> { new() }));
    
    var vm = new ViewModel_Receiving_Review(mockMediator.Object, ...);
    
    // Act
    await vm.LoadLinesCommand.ExecuteAsync(null);
    
    // Assert
    vm.Lines.Should().HaveCount(1);
}
```

**Integration Tests (DAO):**
```csharp
[Fact]
public async Task InsertLineAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var dao = new Dao_ReceivingLine(TestConnectionString);
    var line = new Model_ReceivingLine { Quantity = 10, PartID = "P123" };
    
    // Act
    var result = await dao.InsertLineAsync(line);
    
    // Assert
    result.Success.Should().BeTrue();
}
```

**Documentation Updates:**
- ✅ Update `Module_Receiving/README.md` with new architecture
- ✅ Update `Module_Receiving/ARCHITECTURE.md` (create if not exists)
- ✅ Update `.github/copilot-instructions.md` with MediatR patterns
- ✅ Create `Module_Receiving/CHANGELOG.md`

**Deliverables:**
- ✅ 80% unit test coverage for ViewModels, Handlers, Validators
- ✅ Integration tests for all DAOs
- ✅ All documentation updated
- ✅ Code review completed

---

## 🎓 Learning Resources (For AI Assistants)

### MediatR Pattern Examples
```csharp
// Query Pattern (Read Operation)
public record GetUserQuery(int UserId) : IRequest<User>;

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _repository;
    
    public async Task<User> Handle(GetUserQuery request, CancellationToken ct)
    {
        return await _repository.GetByIdAsync(request.UserId);
    }
}

// Command Pattern (Write Operation)
public record CreateUserCommand(string Name, string Email) : IRequest<int>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _repository;
    private readonly IValidator<CreateUserCommand> _validator;
    
    public async Task<int> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);
        
        return await _repository.CreateAsync(new User 
        { 
            Name = command.Name, 
            Email = command.Email 
        });
    }
}

// Pipeline Behavior (Cross-Cutting Concern)
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}
```

### FluentValidation Examples
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 100);
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

### Serilog Structured Logging
```csharp
// Traditional logging (string interpolation)
_logger.LogInformation($"User {userId} logged in");

// Structured logging (semantic properties)
_logger.LogInformation("User {UserId} logged in", userId);

// Contextual logging
using (_logger.BeginScope("Receiving Load {LoadId}", loadId))
{
    _logger.LogInformation("Processing line {LineId}", lineId);
}
```

---

## 📊 Success Metrics

### Quantitative Goals
- ✅ **Reduce Module_Core service count by 50%** (15 → 7-8 services)
- ✅ **Increase test coverage to 80%+** for Module_Receiving
- ✅ **Reduce average service file size** from 500 lines to <100 lines (handlers)
- ✅ **Improve build time** (fewer dependencies)
- ✅ **Zero constitutional violations** in new code

### Qualitative Goals
- ✅ **Modularity:** Module_Receiving is 100% self-contained
- ✅ **Testability:** Easy to mock IMediator for ViewModel tests
- ✅ **Maintainability:** One handler = one responsibility
- ✅ **Scalability:** Easy to add new commands/queries without modifying existing code
- ✅ **Developer Experience:** Clear patterns for new features

---

## 🚨 Common Pitfalls to Avoid

### Anti-Patterns (DO NOT DO)

❌ **Directly injecting DAOs into ViewModels**
```csharp
// WRONG
public MyViewModel(Dao_User dao) { }

// CORRECT
public MyViewModel(IMediator mediator) { }
```

❌ **Creating God Handlers (multiple responsibilities)**
```csharp
// WRONG - Handler does too much
public class CreateAndUpdateUserHandler : IRequestHandler<CreateAndUpdateUserCommand, User> { }

// CORRECT - Separate handlers
public class CreateUserHandler : IRequestHandler<CreateUserCommand, int> { }
public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool> { }
```

❌ **Skipping validation in handlers**
```csharp
// WRONG - No validation
public async Task<Model_Dao_Result> Handle(InsertLineCommand command, CancellationToken ct)
{
    return await _dao.InsertAsync(command.Line);
}

// CORRECT - Validate first
public async Task<Model_Dao_Result> Handle(InsertLineCommand command, CancellationToken ct)
{
    var validation = await _validator.ValidateAsync(command.Line, ct);
    if (!validation.IsValid)
        return Model_Dao_Result.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
    
    return await _dao.InsertAsync(command.Line);
}
```

❌ **Using string interpolation for logging**
```csharp
// WRONG - No structured properties
_logger.LogInformation($"Processing {poNumber}");

// CORRECT - Structured logging
_logger.LogInformation("Processing {PONumber}", poNumber);
```

---

## 🔗 References

### Official Documentation
- **MediatR:** https://github.com/jbogard/MediatR/wiki
- **Serilog:** https://serilog.net/
- **FluentValidation:** https://docs.fluentvalidation.net/
- **CsvHelper:** https://joshclose.github.io/CsvHelper/
- **WinUI 3:** https://learn.microsoft.com/en-us/windows/apps/winui/

### Architecture Patterns
- **Clean Architecture:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **CQRS Pattern:** https://martinfowler.com/bliki/CQRS.html
- **Modular Monolith:** https://github.com/kgrzybek/modular-monolith-with-ddd

### Project-Specific Docs
- **Constitution:** `.specify/memory/constitution.md`
- **Copilot Instructions:** `.github/copilot-instructions.md`
- **MVVM Guide:** `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Guide:** `.github/instructions/dao-pattern.instructions.md`

---

## 📝 Final Checklist Before Implementation

### Pre-Implementation Validation
- [ ] All critical questions in `03_Clarification_Questions.md` answered
- [ ] Team approval on library selections (MediatR, Serilog, FluentValidation)
- [ ] NuGet package approval process completed
- [ ] Test database available for integration tests
- [ ] Development environment setup (Visual Studio 2022, .NET 8 SDK)
- [ ] Constitutional compliance review completed

### During Implementation
- [ ] Follow Phase 1-6 order strictly
- [ ] Update `tasks.md` after each task completion
- [ ] Run tests after each phase
- [ ] Document architectural decisions in ARCHITECTURE.md
- [ ] Code review after each phase

### Post-Implementation Validation
- [ ] All tests passing (80%+ coverage)
- [ ] No constitutional violations detected
- [ ] Documentation complete and accurate
- [ ] Performance benchmarks meet targets
- [ ] Code review approved by team
- [ ] Deployment plan reviewed and approved

---

## 🎯 Your Task

You are now equipped with:
1. **Context** - Understanding of current state and problems
2. **Target Architecture** - Modern stack with MediatR, Serilog, FluentValidation
3. **Constraints** - Constitutional principles to uphold
4. **Implementation Plan** - 6-phase roadmap with examples
5. **Success Criteria** - Metrics to validate completion

**Proceed with Phase 1** unless instructed otherwise. If you encounter any ambiguity or need clarification, refer to `03_Clarification_Questions.md` or ask before making assumptions.

**Good luck, and build something amazing!** 🚀

# Module_Receiving - Library Research Report

## Executive Summary

This document presents researched libraries across key categories to modernize and modularize the MTM Receiving Module. The goal is to reduce service bloat in Module_Core while maintaining constitutional compliance and architectural integrity.

---

## 1. Navigation Libraries (WinUI 3)

### Top Recommendations

#### 🥇 **Uno.Extensions.Navigation.WinUI** (553k+ downloads)

- **Repository:** <https://github.com/unoplatform/uno.extensions>
- **NuGet:** Uno.Extensions.Navigation.WinUI
- **Strengths:**
  - Built specifically for WinUI 3
  - MVVM-first navigation with ViewModel-based routing
  - Dependency injection integration
  - Supports nested navigation, modals, and flyouts
  - Active maintenance and community support
  
- **Integration Strategy:**
  - Replace custom navigation in `Service_ReceivingWorkflow.cs`
  - Maintain navigation state in ViewModels
  - Works with existing `CommunityToolkit.Mvvm` infrastructure

#### 🥈 **Mvvm.Navigation.WinUI** (5.9k downloads)

- **Repository:** <https://github.com/havendv/Mvvm.Navigation>
- **NuGet:** Mvvm.Navigation.WinUI
- **Strengths:**
  - Lightweight and focused
  - Source generator for automatic View-ViewModel binding
  - Platform-independent core (supports Uno, WPF, Avalonia)
  - Automatic DI registration
  
- **Use Case:** Ideal for greenfield or when replacing navigation completely

#### 🥉 **Singulink.UI.Navigation.WinUI** (4.5k downloads)

- **Repository:** <https://github.com/Singulink/Singulink.UI.Navigation>
- **NuGet:** Singulink.UI.Navigation.WinUI
- **Strengths:**
  - Clean API design
  - Type-safe navigation
  - Minimal dependencies
  
- **Use Case:** Best for simple, predictable navigation flows

### Alternative: Custom Lightweight Navigation

Keep existing pattern but refactor into:

- `INavigationService` interface in Module_Core/Contracts
- `NavigationService` implementation in Module_Receiving
- Register as `AddScoped<INavigationService, NavigationService>()`

---

## 2. Logging Libraries

### Top Recommendations

#### 🥇 **Serilog** (2.3B+ downloads - Industry Standard)

- **Repository:** <https://github.com/serilog/serilog>
- **NuGet:** Serilog, Serilog.Extensions.Logging, Serilog.Sinks.File
- **Strengths:**
  - Structured logging with rich context
  - Extensive sink ecosystem (file, console, database, cloud)
  - Excellent performance
  - Native .NET integration via `Microsoft.Extensions.Logging`
  - JSON output for easy parsing
  
- **Integration Strategy:**

  ```csharp
  // In App.xaml.cs ConfigureServices
  Log.Logger = new LoggerConfiguration()
      .WriteTo.File("logs/receiving-.txt", rollingInterval: RollingInterval.Day)
      .WriteTo.Debug()
      .CreateLogger();
  
  services.AddLogging(builder => builder.AddSerilog(dispose: true));
  ```

- **Migration Path:**
  - Replace `Service_LoggingUtility` with `ILogger<T>` injection
  - Keep file-based logging pattern
  - Add structured properties for better diagnostics

#### 🥈 **NLog** (503M+ downloads)

- **Repository:** <https://github.com/NLog/NLog>
- **NuGet:** NLog, NLog.Extensions.Logging
- **Strengths:**
  - Mature, proven library
  - XML configuration support
  - Flexible routing rules
  - Lower learning curve than Serilog
  
- **Use Case:** If team prefers configuration-over-code

#### 🥉 **Built-in Microsoft.Extensions.Logging** (Already in use)

- **NuGet:** Already referenced
- **Strengths:**
  - No additional dependencies
  - Direct DI integration
  - Works with all sinks
  
- **Recommendation:** Keep as abstraction layer, use Serilog as implementation

### Logging Best Practices for Module_Receiving

```csharp
public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
{
    private readonly ILogger<ViewModel_Receiving_Workflow> _logger;
    
    public ViewModel_Receiving_Workflow(
        IService_ReceivingWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        ILogger<ViewModel_Receiving_Workflow> logger) // ✅ Use generic ILogger
        : base(errorHandler, logger)
    {
        _logger = logger;
    }
    
    [RelayCommand]
    private async Task LoadAsync()
    {
        _logger.LogInformation("Loading workflow step {StepName}", CurrentStepName);
        // ... rest of method
    }
}
```

---

## 3. Error Handling & Validation Libraries

### Top Recommendations

#### 🥇 **FluentValidation** (9.6k stars, 741M+ downloads)

- **Repository:** <https://github.com/FluentValidation/FluentValidation>
- **NuGet:** FluentValidation, FluentValidation.DependencyInjectionExtensions
- **Strengths:**
  - Strongly-typed validation rules
  - Separation of validation from business logic
  - Async validation support
  - Excellent error messaging
  - Native DI integration
  
- **Integration Strategy:**

  ```csharp
  // Validators in Module_Receiving/Validators/
  public class ReceivingLineValidator : AbstractValidator<Model_ReceivingLine>
  {
      public ReceivingLineValidator()
      {
          RuleFor(x => x.Quantity).GreaterThan(0);
          RuleFor(x => x.PartID).NotEmpty();
          RuleFor(x => x.PONumber).NotEmpty().Length(1, 50);
      }
  }
  
  // In Service layer
  public class Service_MySQL_ReceivingLine
  {
      private readonly IValidator<Model_ReceivingLine> _validator;
      
      public async Task<Model_Dao_Result> InsertLineAsync(Model_ReceivingLine line)
      {
          var validation = await _validator.ValidateAsync(line);
          if (!validation.IsValid)
          {
              return Model_Dao_Result.Failure(
                  string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
          }
          // ... continue with insert
      }
  }
  ```

- **Benefits:**
  - Move validation out of `Service_ReceivingValidation.cs`
  - Reusable validators across services
  - Better error messages for users
  - Testable validation logic

#### 🥈 **Built-in Result Pattern** (Custom Implementation)

- **Example:** Current `Model_Dao_Result`
- **Strengths:**
  - Already implemented
  - No additional dependencies
  - Fits architectural constraints
  
- **Enhancement Suggestion:**

  ```csharp
  // Add to Module_Core/Models/
  public class Result<T>
  {
      public bool IsSuccess { get; }
      public T Value { get; }
      public string Error { get; }
      public static Result<T> Success(T value) => new(true, value, string.Empty);
      public static Result<T> Failure(string error) => new(false, default, error);
  }
  ```

#### 🥉 **Polly** (13k stars - Resilience Library)

- **Repository:** <https://github.com/App-vNext/Polly>
- **NuGet:** Polly
- **Strengths:**
  - Retry policies for database operations
  - Circuit breaker for external services (Infor Visual)
  - Timeout policies
  
- **Use Case:** Wrap database calls with retry logic

  ```csharp
  var retryPolicy = Policy
      .Handle<MySqlException>()
      .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  
  return await retryPolicy.ExecuteAsync(() => 
      _dao.InsertReceivingLineAsync(line));
  ```

---

## 4. CQRS / Mediator Pattern Libraries

### Top Recommendations

#### 🥇 **MediatR** (11.8k stars, 607M+ downloads)

- **Repository:** <https://github.com/jbogard/MediatR>
- **NuGet:** MediatR, MediatR.Extensions.Microsoft.DependencyInjection
- **Strengths:**
  - Industry-standard mediator implementation
  - Request/response pattern (queries)
  - Command pattern (mutations)
  - Pipeline behaviors (logging, validation, transactions)
  - Excellent for CQRS architecture
  
- **Integration Strategy (Reduces Service Files):**

  ```csharp
  // Instead of Service_MySQL_ReceivingLine.cs with 10 methods:
  
  // Create Request/Response pairs:
  public record GetReceivingLinesQuery(int LoadId) : IRequest<Model_Dao_Result<List<Model_ReceivingLine>>>;
  
  public class GetReceivingLinesHandler : IRequestHandler<GetReceivingLinesQuery, Model_Dao_Result<List<Model_ReceivingLine>>>
  {
      private readonly Dao_ReceivingLine _dao;
      
      public async Task<Model_Dao_Result<List<Model_ReceivingLine>>> Handle(
          GetReceivingLinesQuery request, CancellationToken cancellationToken)
      {
          return await _dao.GetLinesByLoadAsync(request.LoadId);
      }
  }
  
  // In ViewModel:
  public partial class ViewModel_Receiving_Review : ViewModel_Shared_Base
  {
      private readonly IMediator _mediator;
      
      [RelayCommand]
      private async Task LoadLinesAsync()
      {
          var result = await _mediator.Send(new GetReceivingLinesQuery(LoadId));
          // ... handle result
      }
  }
  ```

- **Benefits:**
  - **Dramatically reduces Service files** - Each method becomes a handler
  - Built-in pipeline for cross-cutting concerns
  - Better separation of concerns
  - Easier testing (one class per operation)
  - Aligns with Clean Architecture principles

#### 🥈 **Immediate.Handlers** (Source Generator Alternative)

- **Repository:** <https://github.com/ImmediatePlatform/Immediate.Handlers>
- **Strengths:**
  - Zero-reflection mediator using source generators
  - Better performance than MediatR
  - Simpler API
  
- **Use Case:** Performance-critical scenarios

#### Alternative: Keep Service Pattern

- Maintain existing `IService_MySQL_ReceivingLine` pattern
- Create smaller, focused services
- Group by aggregate root (Receiving Load vs Line)

---

## 5. Additional Libraries to Reduce Module_Core Bloat

### CSV Export Library

#### **CsvHelper** (34M+ downloads)

- **Repository:** <https://github.com/JoshClose/CsvHelper>
- **NuGet:** CsvHelper
- **Strengths:**
  - Feature-rich CSV reading/writing
  - Strong typing support
  - Custom mapping
  
- **Replaces:** `Service_CSVWriter.cs`
- **Integration:**

  ```csharp
  public class Service_CSVExport<T>
  {
      public async Task ExportAsync(IEnumerable<T> data, string filePath)
      {
          using var writer = new StreamWriter(filePath);
          using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
          await csv.WriteRecordsAsync(data);
      }
  }
  ```

### HTTP Client Management

#### **Refit** (10k stars)

- **Repository:** <https://github.com/reactiveui/refit>
- **NuGet:** Refit
- **Strengths:**
  - Type-safe REST API client
  - Automatic serialization
  - Integrates with `IHttpClientFactory`
  
- **Use Case:** If Infor Visual exposes REST APIs

### State Management

#### **Stateless** (5.7k stars)

- **Repository:** <https://github.com/dotnet-state-machine/stateless>
- **NuGet:** Stateless
- **Strengths:**
  - Hierarchical state machines
  - Explicit state transitions
  - Guard conditions
  
- **Replaces:** Custom workflow state in `Service_ReceivingWorkflow`
- **Example:**

  ```csharp
  var machine = new StateMachine<ReceivingStep, ReceivingTrigger>(ReceivingStep.ModeSelection);
  
  machine.Configure(ReceivingStep.ModeSelection)
      .Permit(ReceivingTrigger.Next, ReceivingStep.POEntry);
  
  machine.Configure(ReceivingStep.POEntry)
      .Permit(ReceivingTrigger.Next, ReceivingStep.WeightQuantity)
      .Permit(ReceivingTrigger.Back, ReceivingStep.ModeSelection);
  ```

### Event Aggregation

#### **EventAggregator** (Built into CommunityToolkit.Mvvm)

- Already available via `WeakReferenceMessenger`
- Use for cross-ViewModel communication
- Replaces custom event patterns

---

## 6. Dependency Injection Enhancements

### Scrutor (6.4k stars)

- **Repository:** <https://github.com/khellang/Scrutor>
- **NuGet:** Scrutor
- **Strengths:**
  - Assembly scanning for automatic registration
  - Decoration pattern support
  - Lifeti management
  
- **Use Case:**

  ```csharp
  services.Scan(scan => scan
      .FromAssemblyOf<Dao_ReceivingLine>()
      .AddClasses(classes => classes.Where(t => t.Name.StartsWith("Dao_")))
      .AsSelfWithInterfaces()
      .WithSingletonLifetime());
  ```

---

## Recommended Library Stack for Module_Receiving

### Core Libraries (High Priority)

1. **MediatR** - Reduce service file count, enable CQRS
2. **Serilog** - Structured logging, better diagnostics
3. **FluentValidation** - Separate validation concerns
4. **CsvHelper** - Replace custom CSV writer

### Supporting Libraries (Medium Priority)

5. **Uno.Extensions.Navigation** OR custom lightweight navigation
2. **Scrutor** - Simplify DI registration
3. **Polly** - Resilience for database operations

### Optional Libraries (Low Priority)

8. **Stateless** - If workflow becomes more complex
2. **Refit** - If REST APIs are added

---

## Impact Analysis: Service File Reduction

### Current Module_Core Services (Potential Candidates for Removal/Reduction)

1. ✅ **Service_LoggingUtility** → Replace with Serilog + `ILogger<T>`
2. ✅ **Service_CSVWriter** → Replace with CsvHelper
3. ✅ **Service_MySQL_ReceivingLine** → Break into MediatR handlers (10+ methods → 10+ handler classes)
4. ✅ **Service_ReceivingValidation** → Replace with FluentValidation validators
5. ⚠️ **Service_ErrorHandler** → Keep but simplify (constitutional requirement)
6. ⚠️ **Service_Window** → Keep (WinUI integration)

### Estimated Service File Reduction

- **Before:** 15+ service files in Module_Core
- **After:** 5-7 core services + distributed handlers in Module_Receiving
- **Reduction:** ~50% service file count in Module_Core

---

## Next Steps

1. **Review and Approve** library selections with team
2. **Create POC** branch with MediatR + Serilog
3. **Migrate one feature** (e.g., Load Entry) as proof of concept
4. **Document patterns** in new instruction files
5. **Gradual migration** of remaining features

---

## References

- Serilog Documentation: <https://serilog.net/>
- MediatR Wiki: <https://github.com/jbogard/MediatR/wiki>
- FluentValidation Docs: <https://docs.fluentvalidation.net/>
- WinUI 3 Navigation: <https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/>

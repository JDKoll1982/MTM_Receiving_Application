# CQRS Pipeline Flow (MediatR Behaviors)

**Feature**: Module_Volvo CQRS Modernization  
**Purpose**: Show how MediatR pipeline behaviors intercept every request/command

## MediatR Pipeline Architecture

```mermaid
flowchart TB
    subgraph ViewModel["ViewModel Layer"]
        VM["ViewModel_Volvo_ShipmentEntry<br/>(Inject IMediator)"]
    end
    
    subgraph MediatR["IMediator Pipeline"]
        direction TB
        Entry["IMediator.Send(request)"]
        
        subgraph Behaviors["Pipeline Behaviors<br/>(Order Matters!)"]
            direction TB
            B1["1️⃣ LoggingBehavior<br/>(Serilog: Log request start)"]
            B2["2️⃣ ValidationBehavior<br/>(FluentValidation)"]
            B3["3️⃣ PerformanceBehavior<br/>(Measure elapsed time)"]
            B4["4️⃣ AuditBehavior<br/>(Log who/when for commands)"]
            
            B1 --> B2
            B2 --> B3
            B3 --> B4
        end
        
        Handler["Handler<br/>(Query/Command Handler)"]
        
        Entry --> B1
        B4 --> Handler
    end
    
    subgraph HandlerLayer["Handler Layer"]
        direction LR
        QHandler["Query Handler<br/>(Read-only)"]
        CHandler["Command Handler<br/>(Write operations)"]
        
        Handler -.-> QHandler
        Handler -.-> CHandler
    end
    
    subgraph DataLayer["Data Access Layer"]
        DAO["DAO (Instance-based)<br/>Returns Model_Dao_Result"]
        DB[(MySQL Database<br/>Stored Procedures)]
        
        QHandler --> DAO
        CHandler --> DAO
        DAO --> DB
    end
    
    subgraph ResponseFlow["Response Path"]
        direction TB
        DBResp["Database Response"]
        DAOResp["Model_Dao_Result<T>"]
        HandlerResp["Handler returns result"]
        BehaviorResp["Behaviors log/audit response"]
        VMResp["ViewModel receives result"]
        
        DB --> DBResp
        DBResp --> DAOResp
        DAOResp --> HandlerResp
        HandlerResp --> BehaviorResp
        BehaviorResp --> VMResp
    end
    
    VM --> Entry
    VMResp --> VM
    
    style B2 fill:#ff6b6b,color:#fff
    style Handler fill:#4ecdc4,color:#fff
    style DAO fill:#95e1d3,color:#000
```

## Detailed Pipeline Flow

```mermaid
sequenceDiagram
    participant VM as ViewModel
    participant Med as IMediator
    participant Log as LoggingBehavior
    participant Val as ValidationBehavior
    participant Perf as PerformanceBehavior
    participant Audit as AuditBehavior
    participant Handler as CommandHandler
    participant DAO as DAO
    participant DB as Database

    Note over VM,DB: Example: CompleteShipmentCommand

    VM->>Med: Send(CompleteShipmentCommand)
    
    Note over Med: Pipeline Entry Point
    
    Med->>Log: Handle(request, next)
    activate Log
    Log->>Log: _logger.LogInformation(<br/>"Executing CompleteShipmentCommand")
    Log->>Val: next() → Continue pipeline
    deactivate Log
    
    activate Val
    Val->>Val: Get validator from DI:<br/>CompleteShipmentCommandValidator
    Val->>Val: await validator.ValidateAsync(request)
    
    alt Validation Fails
        Val-->>VM: Throw ValidationException<br/>(MediatR catches, stops pipeline)
        Note over VM: User sees error message
    else Validation Passes
        Val->>Perf: next() → Continue pipeline
        deactivate Val
    end
    
    activate Perf
    Perf->>Perf: var stopwatch = Stopwatch.StartNew()
    Perf->>Audit: next() → Continue pipeline
    
    activate Audit
    Audit->>Audit: _logger.LogInformation(<br/>"User: {UserName}, Command: CompleteShipmentCommand")
    Audit->>Handler: next() → Invoke actual handler
    
    activate Handler
    Handler->>DAO: InsertAsync(shipment)
    activate DAO
    DAO->>DB: CALL sp_volvo_shipment_insert(...)
    DB-->>DAO: ShipmentId: 123
    DAO-->>Handler: Model_Dao_Result<int>(123)
    deactivate DAO
    
    Handler->>DAO: InsertAsync(line) [loop]
    activate DAO
    DAO->>DB: CALL sp_volvo_shipment_line_insert(...)
    DB-->>DAO: LineId
    DAO-->>Handler: Success
    deactivate DAO
    
    Handler->>DAO: CompleteAsync(123, PO, Receiver)
    activate DAO
    DAO->>DB: CALL sp_volvo_shipment_complete(...)
    DB-->>DAO: Success
    DAO-->>Handler: Model_Dao_Result<bool>
    deactivate DAO
    
    Handler-->>Audit: Model_Dao_Result<int>(123)
    deactivate Handler
    
    Audit->>Audit: _logger.LogInformation(<br/>"Command completed successfully")
    Audit-->>Perf: return result
    deactivate Audit
    
    Perf->>Perf: stopwatch.Stop()<br/>var elapsed = stopwatch.ElapsedMilliseconds
    
    alt Performance Threshold Exceeded
        Perf->>Perf: _logger.LogWarning(<br/>"Slow command: {elapsed}ms")
    end
    
    Perf-->>Log: return result
    deactivate Perf
    
    Log->>Log: _logger.LogInformation(<br/>"Completed CompleteShipmentCommand")
    Log-->>Med: return result
    
    Med-->>VM: Model_Dao_Result<int>(123)
    
    Note over VM: Check result.IsSuccess<br/>Update UI accordingly
```

## Pipeline Behavior Details

### 1️⃣ LoggingBehavior (Always Runs First)

**Purpose**: Structured logging for all requests

**Implementation**:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);
        
        var response = await next();
        
        _logger.LogInformation("Handled {RequestName}", requestName);
        return response;
    }
}
```

**Logs Produced**:

```
[2026-01-16 10:30:15] INFO Handling CompleteShipmentCommand
[2026-01-16 10:30:15] INFO Handled CompleteShipmentCommand
```

### 2️⃣ ValidationBehavior (FluentValidation)

**Purpose**: Validate commands BEFORE execution

**Implementation**:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

**Validation Rules Applied**:

- `CompleteShipmentCommandValidator`
- `AddPartToShipmentCommandValidator`
- `SavePendingShipmentCommandValidator`
- (8 total validators in Module_Volvo)

**Result**:

- ✅ Valid: Pipeline continues to next behavior
- ❌ Invalid: Throws `ValidationException`, pipeline stops, ViewModel catches exception

### 3️⃣ PerformanceBehavior (Monitoring)

**Purpose**: Detect slow commands/queries (>500ms threshold)

**Implementation**:

```csharp
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int ThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var elapsed = stopwatch.ElapsedMilliseconds;
        if (elapsed > ThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Long running request: {RequestName} ({ElapsedMs}ms)", requestName, elapsed);
        }

        return response;
    }
}
```

**Logs Produced** (if slow):

```
[2026-01-16 10:30:16] WARN Long running request: CompleteShipmentCommand (1250ms)
```

### 4️⃣ AuditBehavior (Commands Only)

**Purpose**: Log WHO executed WHICH command WHEN

**Implementation**:

```csharp
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;
    private readonly IService_UserSessionManager _sessionManager;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Only audit commands (writes), not queries (reads)
        if (requestName.EndsWith("Command"))
        {
            var userName = _sessionManager.GetCurrentUser();
            _logger.LogInformation("User {UserName} executing {CommandName}", userName, requestName);
        }

        var response = await next();
        return response;
    }
}
```

**Logs Produced**:

```
[2026-01-16 10:30:15] INFO User JDoe executing CompleteShipmentCommand
```

## Pipeline Registration (App.xaml.cs)

**Dependency Injection Setup**:

```csharp
public static void ConfigureServices(IServiceCollection services)
{
    // MediatR with auto-discovery
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
        
        // Pipeline behaviors (ORDER MATTERS!)
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
    });

    // FluentValidation validators
    services.AddValidatorsFromAssembly(typeof(App).Assembly);
}
```

## Benefits of Pipeline Architecture

### ✅ Cross-Cutting Concerns Centralized

- All logging in one place
- All validation in one place
- All performance monitoring in one place
- All audit trails in one place

### ✅ Handlers Stay Focused

```csharp
// Handler ONLY does business logic
public async Task<Model_Dao_Result<int>> Handle(CompleteShipmentCommand request, CancellationToken cancellationToken)
{
    // No logging code
    // No validation code
    // No audit code
    // Just business logic!
    
    var result = await _shipmentDao.InsertAsync(...);
    return result;
}
```

### ✅ Consistent Behavior Across All Requests

- Every command gets validated
- Every command gets logged
- Every command gets audited
- Every slow operation gets flagged

### ✅ Easy to Add New Behaviors

```csharp
// Example: Add caching behavior for queries
services.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
```

## Error Handling in Pipeline

```mermaid
flowchart TD
    Start[IMediator.Send] --> Log1[LoggingBehavior]
    Log1 --> Val[ValidationBehavior]
    
    Val --> ValCheck{Valid?}
    ValCheck -->|No| ValEx[Throw ValidationException]
    ValEx --> Catch1[ViewModel catches exception]
    Catch1 --> ShowError1[Show validation error to user]
    
    ValCheck -->|Yes| Handler[Handler Execution]
    Handler --> HandlerCheck{Success?}
    
    HandlerCheck -->|DAO Failure| Return1[Return Model_Dao_Result.Failure]
    Return1 --> VMCheck1[ViewModel checks result.IsSuccess]
    VMCheck1 --> ShowError2[Show error message to user]
    
    HandlerCheck -->|Exception| HandlerEx[Catch in Handler]
    HandlerEx --> Return2[Return Model_Dao_Result.Failure with exception]
    Return2 --> VMCheck2[ViewModel checks result.IsSuccess]
    VMCheck2 --> ShowError3[Show error message + log]
    
    HandlerCheck -->|Success| Return3[Return Model_Dao_Result.Success]
    Return3 --> Log2[LoggingBehavior logs completion]
    Log2 --> VMSuccess[ViewModel processes success]
    VMSuccess --> UpdateUI[Update UI, show success]
    
    style ValEx fill:#ff6b6b,color:#fff
    style HandlerEx fill:#ff6b6b,color:#fff
    style Return3 fill:#51cf66,color:#fff
```

## Constitutional Compliance

**Principle III: CQRS + Mediator First** ✅

- All ViewModel operations go through `IMediator`
- No direct ViewModel→Service calls
- No direct ViewModel→DAO calls
- Pipeline behaviors enforce cross-cutting concerns

**Principle V: Validation & Structured Logging** ✅

- FluentValidation via `ValidationBehavior`
- Structured logging via Serilog in `LoggingBehavior`
- Performance monitoring via `PerformanceBehavior`
- Audit trails via `AuditBehavior`

## Testing the Pipeline

**Unit Test** (Test Handler in Isolation):

```csharp
[Fact]
public async Task CompleteShipmentCommandHandler_ShouldSucceed()
{
    // Arrange: Mock DAOs, skip pipeline
    var handler = new CompleteShipmentCommandHandler(_mockDao, ...);
    
    // Act: Call handler directly (no pipeline)
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

**Integration Test** (Test Full Pipeline):

```csharp
[Fact]
public async Task CompleteShipmentCommand_ShouldPassThroughPipeline()
{
    // Arrange: Real MediatR with behaviors
    var mediator = GetConfiguredMediator();
    
    // Act: Send through full pipeline
    var result = await mediator.Send(command);
    
    // Assert: Check logs, validation, audit
    _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
}
```

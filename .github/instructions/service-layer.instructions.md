# Service Layer Guidelines

**Category**: Infrastructure  
**Last Updated**: December 15, 2025  
**Applies To**: All service code in Services/ folder

## Service Architecture Principles

### Single Responsibility

Each service class should have ONE clear purpose:

- `LoggingUtility`: File-based application logging
- `Service_ErrorHandler`: Centralized error handling and user notifications
- Future services: Focus on specific business capabilities

### Dependency Injection

All services MUST:

- Accept dependencies via constructor injection
- Define corresponding interfaces in `Contracts/Services/`
- Support testability through interface abstraction

Example:

```csharp
public class Service_ErrorHandler : IService_ErrorHandler
{
    private readonly ILoggingService _loggingService;

    public Service_ErrorHandler(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }
}
```

## Error Handling Rules

### Severity Levels

Use appropriate `Enum_ErrorSeverity` levels:

- **Info**: Informational messages, no action required
- **Warning**: Potential issues, operation continues
- **Error**: Operation failed, application continues
- **Critical**: Serious issue requiring immediate attention
- **Fatal**: Application cannot continue

### Error Message Formatting

Error messages MUST be:

- **User-friendly**: Avoid technical jargon for UI dialogs
- **Descriptive**: Include context about what failed
- **Actionable**: Suggest what user can do (when applicable)

Bad: `"NullReferenceException at line 42"`  
Good: `"Unable to save receiving label. Please check that all required fields are filled and try again."`

### Exception Handling Pattern

```csharp
try
{
    // Operation
}
catch (SpecificException ex)
{
    await _errorHandler.HandleErrorAsync(
        errorMessage: "User-friendly description",
        severity: Enum_ErrorSeverity.Error,
        exception: ex,
        showDialog: true // Only if user needs to see this
    );
}
```

### When to Show Dialogs

**Show Dialog (`showDialog: true`)**:

- User-initiated actions that fail
- Critical errors affecting user workflow
- Validation errors requiring user correction

**Don't Show Dialog (`showDialog: false`)**:

- Background operations
- Automatic retries
- DAO/database layer errors (handled at higher level)
- Logging-only scenarios

## Logging Patterns

### Log Levels

Match logging to severity:

```csharp
_loggingService.LogInfo("Operation completed successfully");
_loggingService.LogWarning("Deprecated method called", context: "UserService.OldMethod");
_loggingService.LogError("Database connection failed", exception: ex);
_loggingService.LogCritical("Configuration file corrupted", exception: ex);
_loggingService.LogFatal("Out of memory", exception: ex);
```

### Contextual Logging

Always provide context:

```csharp
_loggingService.LogError(
    message: "Failed to insert receiving line",
    exception: ex,
    context: $"PartID={line.PartID}, PONumber={line.PONumber}"
);
```

### Performance Logging

Log performance metrics for slow operations:

```csharp
var stopwatch = Stopwatch.StartNew();
// ... operation ...
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 1000) // Threshold: 1 second
{
    _loggingService.LogWarning(
        $"Slow operation detected: {stopwatch.ElapsedMilliseconds}ms",
        context: "Dao_ReceivingLine.InsertReceivingLineAsync"
    );
}
```

## Service Registration

### Manual Registration (Current Pattern)

Services are instantiated and injected manually:

```csharp
var loggingService = new LoggingUtility();
var errorHandler = new Service_ErrorHandler(loggingService);

// Inject into DAOs
Dao_ReceivingLine.SetErrorHandler(errorHandler);
Dao_DunnageLine.SetErrorHandler(errorHandler);
Dao_RoutingLabel.SetErrorHandler(errorHandler);
```

### Future: DI Container (Phase 2)

When implementing MVVM with Microsoft.Extensions.DependencyInjection:

```csharp
services.AddSingleton<ILoggingService, LoggingUtility>();
services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
```

## Thread Safety

Services used across threads MUST be thread-safe:

- `LoggingUtility`: Uses `lock (_lockObject)` for file writes
- Stateless services: Naturally thread-safe
- Stateful services: Use proper synchronization

## Testing Guidelines

### Unit Tests

- Mock dependencies via interfaces
- Test each severity level
- Verify error messages and logging

### Integration Tests

- Test actual file I/O (LoggingUtility)
- Verify dialog display (Service_ErrorHandler)
- Test end-to-end error flows

## Common Patterns

### DAO Error Handling

```csharp
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

if (!result.Success)
{
    await _errorHandler.HandleDaoErrorAsync(
        result,
        operationName: "Insert Receiving Line",
        showDialog: true
    );
    return; // Don't proceed
}
```

### Multiple Error Aggregation

```csharp
var errors = new List<string>();

if (string.IsNullOrEmpty(line.PartID))
    errors.Add("Part ID is required");
if (line.Quantity <= 0)
    errors.Add("Quantity must be greater than zero");

if (errors.Any())
{
    await _errorHandler.HandleErrorAsync(
        string.Join("; ", errors),
        Enum_ErrorSeverity.Warning,
        showDialog: true
    );
    return;
}
```

## Examples

See existing implementations:

- `Services/Database/LoggingUtility.cs`
- `Services/Database/Service_ErrorHandler.cs`
- `Contracts/Services/ILoggingService.cs`
- `Contracts/Services/IService_ErrorHandler.cs`

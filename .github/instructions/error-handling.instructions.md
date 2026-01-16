# Error Handling Guidelines

**Category**: Cross-Cutting Concern  
**Last Updated**: December 15, 2025  
**Applies To**: All application code

## Error Severity Classification

### Enum_ErrorSeverity Levels

| Level | Value | When to Use | User Action | Logging |
|-------|-------|-------------|-------------|---------|
| **Info** | 0 | Successful operations, informational messages | None | Optional |
| **Warning** | 1 | Potential issues, operation continues | Be aware | Always |
| **Error** | 2 | Operation failed, app continues | Retry or adjust | Always |
| **Critical** | 3 | Serious issue affecting functionality | Contact support | Always |
| **Fatal** | 4 | Application cannot continue | Restart app | Always |

### Examples by Severity

**Info**:

```csharp
"Receiving label created successfully"
"Database connection established"
"Log files archived: 5 files"
```

**Warning**:

```csharp
"Database operation took 2.5 seconds (threshold: 1 second)"
"Vendor name not found, using default: 'Unknown'"
"Part description missing for part ID: PART-123"
```

**Error**:

```csharp
"Failed to insert receiving line: Part ID is required"
"Database connection failed: Server not responding"
"Unable to save label: Invalid PO Number"
```

**Critical**:

```csharp
"Database schema version mismatch: Expected 1.2, Found 1.0"
"Configuration file corrupted or missing"
"Multiple database operations failing consecutively"
```

**Fatal**:

```csharp
"Out of memory: Cannot allocate buffer"
"Critical system resource unavailable"
"Unrecoverable database error"
```

## Error Message Standards

### User-Facing Messages

Messages shown in dialogs MUST:

1. **Be clear and concise**: Avoid technical jargon
2. **Explain what happened**: "Unable to save receiving label"
3. **Suggest next steps**: "Please check all required fields and try again"
4. **Avoid blame**: "An error occurred" not "You entered invalid data"

**Good Examples**:

```
"Unable to connect to database. Please check your network connection and try again."
"The Part ID field is required. Please enter a valid Part ID."
"This label has already been printed. Do you want to print a duplicate?"
```

**Bad Examples**:

```
"NullReferenceException in Dao_ReceivingLine.InsertReceivingLineAsync()"
"Error: 0x80004005"
"Operation failed"
```

### Log Messages

Messages in log files SHOULD:

1. **Include technical details**: Exception types, stack traces
2. **Provide context**: PartID, PONumber, employee number
3. **Include timestamps**: Automatic in LoggingUtility
4. **Show severity**: Automatic in LoggingUtility

**Example Log Entry**:

```
[2025-12-15 16:30:45.123] [ERROR] [Dao_ReceivingLine.InsertReceivingLineAsync]
Message: Failed to insert receiving line: Part ID is required
Context: User=JDOE, Machine=DESKTOP-ABC123, PartID=, PONumber=12345
Exception: MySqlException: Column 'part_id' cannot be null
Stack Trace:
   at MySql.Data.MySqlClient.MySqlCommand.ExecuteNonQueryAsync()
   at Helper_Database_StoredProcedure.ExecuteAsync(String procedureName, MySqlParameter[] parameters)
```

## Exception Handling Patterns

### Try-Catch Hierarchy

Catch specific exceptions before general ones:

```csharp
try
{
    // Database operation
}
catch (MySqlException ex) when (ex.Number == 1062) // Duplicate key
{
    return new Model_Dao_Result
    {
        Success = false,
        ErrorMessage = "This record already exists",
        Severity = Enum_ErrorSeverity.Warning
    };
}
catch (MySqlException ex) when (IsTransientError(ex))
{
    // Retry logic handles this
    throw;
}
catch (MySqlException ex)
{
    // Other MySQL errors
    await _errorHandler.LogErrorAsync("Database error", Enum_ErrorSeverity.Error, ex);
    return CreateErrorResult(ex);
}
catch (Exception ex)
{
    // Unexpected errors
    await _errorHandler.LogErrorAsync("Unexpected error", Enum_ErrorSeverity.Critical, ex);
    return CreateErrorResult(ex);
}
```

### Never Swallow Exceptions

❌ **Bad**:

```csharp
try
{
    await SaveData();
}
catch
{
    // Silent failure
}
```

✅ **Good**:

```csharp
try
{
    await SaveData();
}
catch (Exception ex)
{
    await _errorHandler.HandleErrorAsync(
        "Failed to save data",
        Enum_ErrorSeverity.Error,
        ex,
        showDialog: true
    );
}
```

### Rethrowing Exceptions

Use `throw;` not `throw ex;` to preserve stack trace:

```csharp
try
{
    // Operation
}
catch (TransientException ex)
{
    _logger.LogWarning("Transient error, will retry", ex);
    throw; // Correct: preserves original stack trace
}
```

## Logging Requirements

### What to Log

**Always Log**:

- All exceptions (Error, Critical, Fatal)
- Database operations (with execution time)
- User actions (Info level)
- Authentication events (Info/Warning)
- Configuration changes (Info)

**Optionally Log**:

- Successful operations (Info)
- Performance metrics (Warning if slow)
- Debug information (only in Development)

### What NOT to Log

**Never Log**:

- Passwords or sensitive credentials
- Credit card numbers
- Personal Identification Numbers (SSNs, etc.)
- Full connection strings (mask passwords)

### Log File Management

- **Location**: `%APPDATA%\MTM_Receiving_Application\Logs\`
- **Format**: `app_{yyyy-MM-dd}.log`
- **Rotation**: Daily (automatic)
- **Archival**: Files older than 30 days moved to `archive/` folder
- **Cleanup**: Manual or automated based on disk space

## Dialog Display Guidelines

### When to Show ContentDialog

Show dialogs for:

- Validation errors (user needs to correct input)
- Operation failures requiring user acknowledgment
- Confirmation prompts
- Critical errors preventing workflow continuation

### When NOT to Show Dialog

Don't show dialogs for:

- Background operation failures (log only)
- Automatic retry attempts (log only)
- Info-level messages (use status bar or notifications)
- Multiple rapid errors (consolidate or throttle)

### Dialog Content Structure

```csharp
var dialog = new ContentDialog
{
    Title = "Error Title",              // Brief, clear
    Content = "Detailed explanation",   // User-friendly description
    CloseButtonText = "OK",             // Action button
    XamlRoot = App.MainWindow?.Content?.XamlRoot
};
await dialog.ShowAsync();
```

## Error Recovery Strategies

### Automatic Retry

For transient errors (network, database locks):

```csharp
// Helper_Database_StoredProcedure automatically retries:
// - 3 attempts max
// - Delays: 100ms, 200ms, 400ms
// - Only for transient error codes (1205, 1213, 2006, 2013)
```

### Graceful Degradation

When optional features fail:

```csharp
try
{
    vendorName = await LookupVendorName(partId);
}
catch (Exception ex)
{
    _logger.LogWarning("Vendor lookup failed, using default", ex);
    vendorName = "Unknown"; // Fallback value
}
```

### User Retry

For user-correctable errors:

```csharp
var dialog = new ContentDialog
{
    Title = "Save Failed",
    Content = "Unable to save label. Would you like to try again?",
    PrimaryButtonText = "Retry",
    CloseButtonText = "Cancel",
    XamlRoot = App.MainWindow?.Content?.XamlRoot
};

var result = await dialog.ShowAsync();
if (result == ContentDialogResult.Primary)
{
    await SaveLabel(); // Retry
}
```

## Testing Error Scenarios

### Required Test Cases

For each operation, test:

1. **Happy path**: Everything works
2. **Validation errors**: Required fields missing, invalid data
3. **Database unavailable**: Connection fails
4. **Transient errors**: Temporary failures that resolve
5. **Unexpected exceptions**: Null references, out of memory, etc.

### Manual Test Procedures

See `Tests/Phase1_Manual_Tests.cs` for:

- Valid data insertion
- Invalid data handling
- Database unavailable scenarios
- Error logging verification

## Examples

See existing implementations:

- `Services/Database/Service_ErrorHandler.cs` - Error handling
- `Services/Database/LoggingUtility.cs` - Logging implementation
- `Data/Receiving/Dao_ReceivingLine.cs` - DAO error patterns
- `Models/Enums/Enum_ErrorSeverity.cs` - Severity definitions

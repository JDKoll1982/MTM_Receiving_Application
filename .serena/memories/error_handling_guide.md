# Error Handling Guide

Last Updated: 2026-03-21

## Error Severity Levels

`Enum_ErrorSeverity` values (in `Module_Core/Models/Enums/`):

| Level        | Value | When to Use                           | User Action              |
| ------------ | ----- | ------------------------------------- | ------------------------ |
| **Info**     | 0     | Successful operations, informational  | None                     |
| **Low**      | 1     | Minor issue, operation continues      | Be aware                 |
| **Warning**  | 2     | Potential issue, operation continues  | Be aware                 |
| **Medium**   | 3     | Operation failed but recoverable      | Retry or adjust          |
| **Error**    | 4     | Operation failed, high severity       | Retry or contact support |
| **Critical** | 5     | Serious issue affecting functionality | Contact support          |
| **Fatal**    | 6     | Application cannot continue           | Restart app              |

## Service_ErrorHandler Usage

### In ViewModels

```csharp
[RelayCommand]
private async Task SaveAsync()
{
    try
    {
        IsBusy = true;
        StatusMessage = "Saving...";

        var result = await _service.SaveDataAsync(data);
        if (result.IsSuccess)
        {
            StatusMessage = "Saved successfully";
        }
        else
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
            nameof(MyViewModel));
    }
    finally
    {
        IsBusy = false;
    }
}
```

### In DAOs

```csharp
public async Task<Model_Dao_Result<int>> InsertAsync(Model_Entity entity)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_field", entity.Field }
        };

        return await Helper_Database_StoredProcedure.ExecuteScalarAsync<int>(
            _connectionString,
            "sp_entity_insert",
            parameters
        );
    }
    catch (Exception ex)
    {
        // NEVER throw - return failure result
        return Model_Dao_Result_Factory.Failure<int>(
            $"Error inserting entity: {ex.Message}",
            ex);
    }
}
```

## User-Facing Error Messages

### Good Messages

- Clear and concise
- Explain what happened
- Suggest next steps
- Avoid technical jargon

```
"Unable to connect to database. Please check your network connection and try again."
"The Part ID field is required. Please enter a valid Part ID."
"This label has already been printed. Do you want to print a duplicate?"
```

### Bad Messages (Avoid)

```
"NullReferenceException in Dao_ReceivingLine.InsertReceivingLineAsync()"
"Error: 0x80004005"
"Operation failed"
```

## Logging Standards

### IService_LoggingUtility Usage

Inject `IService_LoggingUtility` (not `ILoggingService`) in ViewModels and Services.

```csharp
// Log successful operations
_logger.LogInfo("Receiving line created successfully");

// Log errors with context
_logger.LogError($"Failed to insert receiving line: {ex.Message}", ex);
```

### What to Log

- ✅ All exceptions
- ✅ Database operations (with execution time)
- ✅ User actions
- ✅ Authentication events
- ✅ Configuration changes

### What NOT to Log

- ❌ Passwords or credentials
- ❌ Credit card numbers
- ❌ Full connection strings (mask passwords)

## Error Recovery Patterns

### Automatic Retry

Helper_Database_StoredProcedure automatically retries transient errors:

- 3 attempts max
- Delays: 100ms, 200ms, 400ms
- Only for transient error codes (1205, 1213, 2006, 2013)

### Graceful Degradation

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

### User Retry Dialog

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

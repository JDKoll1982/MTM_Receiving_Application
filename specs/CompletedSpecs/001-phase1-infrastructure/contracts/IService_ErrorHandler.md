# IService_ErrorHandler Contract

**Purpose**: Service contract for centralized error handling and user notification.

**Location**: Implement in `Contracts/Services/IService_ErrorHandler.cs` in your project root

## Contract Definition

```csharp
using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service contract for centralized error handling and user notification.
/// Implementations must log errors to file system and display user-friendly dialogs.
/// </summary>
public interface IService_ErrorHandler
{
    /// <summary>
    /// Handles an error by logging it and optionally displaying a dialog.
    /// </summary>
    /// <param name="errorMessage">The error message to log and display</param>
    /// <param name="severity">The severity level of the error</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="showDialog">Whether to show a user-facing dialog</param>
    /// <returns>Task for async operation</returns>
    Task HandleErrorAsync(
        string errorMessage,
        Enum_ErrorSeverity severity,
        Exception? exception = null,
        bool showDialog = true
    );

    /// <summary>
    /// Logs an error without displaying a dialog.
    /// </summary>
    /// <param name="errorMessage">The error message to log</param>
    /// <param name="severity">The severity level of the error</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <returns>Task for async operation</returns>
    Task LogErrorAsync(
        string errorMessage,
        Enum_ErrorSeverity severity,
        Exception? exception = null
    );

    /// <summary>
    /// Displays a user-friendly error dialog.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Error message to display</param>
    /// <param name="severity">Severity level (affects icon/styling)</param>
    /// <returns>Task for async operation</returns>
    Task ShowErrorDialogAsync(
        string title,
        string message,
        Enum_ErrorSeverity severity
    );

    /// <summary>
    /// Handles a Model_Dao_Result error by logging and optionally displaying.
    /// </summary>
    /// <param name="result">The DAO result containing error information</param>
    /// <param name="operationName">Name of the operation that failed</param>
    /// <param name="showDialog">Whether to show a user-facing dialog</param>
    /// <returns>Task for async operation</returns>
    Task HandleDaoErrorAsync(
        Model_Dao_Result result,
        string operationName,
        bool showDialog = true
    );
}
```

## Implementation Guidelines

**Implementation File**: `Services/Database/Service_ErrorHandler.cs`

### Key Requirements

1. **WinUI 3 Dialogs**: Use `ContentDialog` for error display (not WinForms MessageBox)
2. **Logging**: Write all errors to `%APPDATA%\MTM_Receiving_Application\Logs\`
3. **Log Format**: `[{timestamp}] [{severity}] [{operation}] {message}\n{exception}`
4. **Async Handling**: Properly await ContentDialog.ShowAsync() with XamlRoot reference

### Usage Example

```csharp
// In a DAO or ViewModel
try
{
    var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);
    if (!result.Success)
    {
        await errorHandler.HandleDaoErrorAsync(result, "InsertReceivingLine");
    }
}
catch (Exception ex)
{
    await errorHandler.HandleErrorAsync(
        "Unexpected error inserting receiving line",
        Enum_ErrorSeverity.Error,
        ex,
        showDialog: true
    );
}
```

### Dialog Display Pattern

```csharp
// WinUI 3 ContentDialog example
var dialog = new ContentDialog
{
    Title = title,
    Content = message,
    CloseButtonText = "OK",
    XamlRoot = App.MainWindow.Content.XamlRoot // Important!
};

await dialog.ShowAsync();
```

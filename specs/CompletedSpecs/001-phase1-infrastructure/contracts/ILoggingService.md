# ILoggingService Contract

**Purpose**: Service contract for application logging functionality.

**Location**: Implement in `Contracts/Services/ILoggingService.cs` in your project root

## Contract Definition

```csharp
using System;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service contract for application logging functionality.
/// Implementations must write to file system with daily rotation.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="context">Optional context information</param>
    void LogInfo(string message, string? context = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The warning message to log</param>
    /// <param name="context">Optional context information</param>
    void LogWarning(string message, string? context = null);

    /// <summary>
    /// Logs an error message with optional exception.
    /// </summary>
    /// <param name="message">The error message to log</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="context">Optional context information</param>
    void LogError(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Logs a critical error message with optional exception.
    /// </summary>
    /// <param name="message">The critical error message to log</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="context">Optional context information</param>
    void LogCritical(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Logs a fatal error message with optional exception.
    /// Fatal errors indicate the application should terminate.
    /// </summary>
    /// <param name="message">The fatal error message to log</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="context">Optional context information</param>
    void LogFatal(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Gets the path to the current log file.
    /// </summary>
    /// <returns>Full path to today's log file</returns>
    string GetCurrentLogFilePath();

    /// <summary>
    /// Ensures the log directory exists and is writable.
    /// </summary>
    /// <returns>True if log directory is accessible, false otherwise</returns>
    bool EnsureLogDirectoryExists();

    /// <summary>
    /// Archives old log files (older than specified days).
    /// </summary>
    /// <param name="daysToKeep">Number of days of logs to keep</param>
    /// <returns>Number of log files archived</returns>
    int ArchiveOldLogs(int daysToKeep = 30);
}
```

## Implementation Guidelines

**Implementation File**: `Services/Database/LoggingUtility.cs`

### Key Requirements

1. **Log Location**: `%APPDATA%\MTM_Receiving_Application\Logs\`
2. **File Naming**: `app_{yyyy-MM-dd}.log` (e.g., `app_2025-12-15.log`)
3. **Daily Rotation**: New file created automatically each day
4. **Thread Safety**: Use proper file locking for concurrent writes
5. **Exception Details**: Include full stack traces and inner exceptions

### Log Entry Format

```
[2025-12-15 10:30:45.123] [ERROR] [Dao_ReceivingLine.InsertAsync]
Message: Failed to insert receiving line for PO 12345
Context: User=DOMAIN\username, Machine=WORKSTATION
Exception: MySql.Data.MySqlClient.MySqlException: Duplicate entry '12345' for key 'PRIMARY'
   at MySql.Data.MySqlClient.MySqlConnection.Execute(...)
   at MTM_Receiving_Application.Data.Receiving.Dao_ReceivingLine.InsertAsync(...)
```

### Usage Example

```csharp
// Info logging
loggingService.LogInfo("Application started", "Version 1.0.0");

// Warning logging
loggingService.LogWarning($"PO {poNumber} not found in Infor Visual", "ReceivingViewModel");

// Error logging with exception
try
{
    await someOperation();
}
catch (Exception ex)
{
    loggingService.LogError(
        "Failed to process receiving line",
        ex,
        $"PO={poNumber}, Part={partId}"
    );
}

// Critical/Fatal logging
loggingService.LogFatal(
    "Database connection lost and retry failed",
    connectionException,
    "Service_ErrorHandler"
);
```

### Directory Initialization

```csharp
// Ensure log directory exists on application startup
public class LoggingUtility : ILoggingService
{
    private readonly string _logDirectory;
    
    public LoggingUtility()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MTM_Receiving_Application",
            "Logs"
        );
        
        EnsureLogDirectoryExists();
    }
    
    public bool EnsureLogDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

### Log Archiving

```csharp
// Archive logs older than 30 days
public int ArchiveOldLogs(int daysToKeep = 30)
{
    var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
    var archivedCount = 0;
    
    foreach (var file in Directory.GetFiles(_logDirectory, "app_*.log"))
    {
        var fileInfo = new FileInfo(file);
        if (fileInfo.LastWriteTime < cutoffDate)
        {
            // Move to archive subfolder or delete
            var archivePath = Path.Combine(_logDirectory, "archive", fileInfo.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(archivePath));
            File.Move(file, archivePath);
            archivedCount++;
        }
    }
    
    return archivedCount;
}
```

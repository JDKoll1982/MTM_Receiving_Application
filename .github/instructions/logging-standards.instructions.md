# Logging Service Standards

**Category**: Cross-Cutting Concern
**Last Updated**: December 26, 2025
**Applies To**: All Services, ViewModels, and Helpers

## Overview

The `ILoggingService` provides a standardized way to log application events, errors, and performance metrics. It handles file rotation, archiving, and formatting automatically.

## Usage Guidelines

### 1. Dependency Injection

Always inject `ILoggingService` into your classes. Do not use static loggers.

```csharp
public class MyService
{
    private readonly ILoggingService _logger;

    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }
}
```

### 2. Log Levels

| Method | Use Case | Example |
|--------|----------|---------|
| `LogInfo` | Successful operations, state changes, user actions | "Workflow started", "User logged in" |
| `LogWarning` | Non-fatal issues, performance thresholds exceeded, fallback behaviors | "Network path unavailable, using local", "Query took 2s" |
| `LogError` | Operation failures, exceptions caught but handled | "Failed to save record", "Invalid input format" |
| `LogCritical` | System instability, data corruption risks | "Database schema mismatch", "Disk full" |
| `LogFatal` | Application crash imminent | "Unhandled exception in main loop" |

### 3. Context Parameter

Always provide the `context` parameter (usually the method name or class.method) to make logs easier to trace.

```csharp
// Good
_logger.LogInfo("Starting export", "ExportService.ExportAsync");

// Bad
_logger.LogInfo("Starting export");
```

### 4. Exception Logging

When logging exceptions, pass the exception object to preserve the stack trace.

```csharp
try 
{
    // ...
}
catch (Exception ex)
{
    _logger.LogError("Failed to process file", ex, "FileService.Process");
}
```

## Log File Location

Logs are stored in: `%APPDATA%\MTM_Receiving_Application\Logs\`
Format: `Log_YYYYMMDD.txt`

## Rotation Policy

- Logs are rotated daily.
- Old logs are archived/deleted after 30 days (configurable).

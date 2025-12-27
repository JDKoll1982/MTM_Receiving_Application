# Workflow Session Persistence Standards

**Category**: Data Persistence
**Last Updated**: December 26, 2025
**Applies To**: `IService_SessionManager`, `Service_SessionManager`

## Overview

The `Service_SessionManager` is responsible for persisting the **Receiving Workflow State** to disk (JSON). This allows the application to recover the user's work in case of a crash or restart.

**Note**: Do not confuse with `IService_UserSessionManager`, which handles user login state.

## Responsibilities

1.  **Serialization**: Convert `Model_ReceivingSession` to JSON.
2.  **Storage**: Save to `%APPDATA%\MTM_Receiving_Application\session.json`.
3.  **Recovery**: Load the session from disk on startup (if requested).
4.  **Cleanup**: Delete the session file when the workflow is successfully completed.

## Implementation Pattern

```csharp
public async Task SaveSessionAsync(Model_ReceivingSession session)
{
    var json = JsonSerializer.Serialize(session, _options);
    await File.WriteAllTextAsync(_path, json);
}

public async Task<Model_ReceivingSession?> LoadSessionAsync()
{
    if (!File.Exists(_path)) return null;
    var json = await File.ReadAllTextAsync(_path);
    return JsonSerializer.Deserialize<Model_ReceivingSession>(json, _options);
}
```

## Usage

The `Service_ReceivingWorkflow` calls this service whenever the step changes or data is modified, ensuring the disk copy is always up-to-date.

## Registration
- Register as **Transient** or **Singleton**. Since it's stateless (just file I/O), Transient is fine, but Singleton is also acceptable.

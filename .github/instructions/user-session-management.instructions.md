# User Session Management Standards

**Category**: Security / State
**Last Updated**: December 26, 2025
**Applies To**: `IService_UserSessionManager`, `Service_UserSessionManager`

## Overview

The `IService_UserSessionManager` handles the current user's context, including authentication state, timeout monitoring, and user details.

## Key Responsibilities

1.  **Current User**: Stores the `Model_User` object for the currently logged-in employee.
2.  **Session Timeout**: Monitors idle time and automatically logs out the user after a configured period (e.g., 15 minutes).
3.  **Activity Tracking**: Resets the timeout timer on user interaction (mouse/keyboard).

## Usage

### Accessing Current User

```csharp
var user = _sessionManager.CurrentSession?.User;
if (user != null)
{
    string username = user.WindowsUsername;
    // ...
}
```

### Handling Timeout

The service typically exposes an event (e.g., `SessionExpired`) that the `MainWindow` or `App` subscribes to in order to show the login screen.

## Implementation Details

- **Dispatcher**: Timeout logic often interacts with the UI thread (to show login screen). Use `IDispatcherService` to marshal calls.
- **Singleton**: The session manager must be a Singleton to persist across the application lifetime.

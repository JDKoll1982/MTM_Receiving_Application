# Authentication Services Guidelines

**Category**: Authentication  
**Last Updated**: December 16, 2025  
**Applies To**: Services/Authentication/* and Contracts/Services/IService_Authentication.cs, IService_SessionManager.cs

## Purpose

Authentication services manage user identity verification, session lifecycle, and workstation detection in the MTM Receiving Application.

## Core Principles

### 1. Service Layer Architecture

Authentication functionality is split into two complementary services:

- **Service_Authentication**: Handles user validation, workstation detection, and user management
- **Service_SessionManager**: Manages active user sessions, timeout monitoring, and session lifecycle

### 2. Dependency Injection

All authentication services MUST:
- Define corresponding interfaces in `Contracts/Services/`
- Accept dependencies via constructor injection
- Be registered in `App.xaml.cs` DI container
- Use non-nullable required dependencies (no optional dependencies)

Example:
```csharp
public class Service_Authentication : IService_Authentication
{
    private readonly Dao_User _daoUser;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_Authentication(Dao_User daoUser, IService_ErrorHandler errorHandler)
    {
        _daoUser = daoUser ?? throw new ArgumentNullException(nameof(daoUser));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }
}
```

### 3. Error Handling

Authentication services follow strict error handling patterns:

**Always Handle Exceptions**:
- Wrap all DAO calls in try-catch blocks
- Use `IService_ErrorHandler` for consistent error logging
- Return result objects (never throw exceptions from service methods)
- Check for null before calling async error handler methods

```csharp
try
{
    var result = await _daoUser.GetUserByWindowsUsernameAsync(username);
    if (result.Success)
    {
        return AuthenticationResult.SuccessResult(result.Data);
    }
    return AuthenticationResult.ErrorResult(result.ErrorMessage);
}
catch (Exception ex)
{
    if (_errorHandler != null)
        await _errorHandler.HandleErrorAsync("Authentication failed", Enum_ErrorSeverity.Error, ex, false);
    return AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
}
```

**Result Objects**:
- Use strongly-typed result objects (e.g., `AuthenticationResult`, `ValidationResult`)
- Include `Success`, `ErrorMessage`, and `Data` properties
- Provide static factory methods: `SuccessResult()`, `ErrorResult()`

### 4. Progress Reporting

Authentication operations support optional progress reporting:

```csharp
public async Task<AuthenticationResult> AuthenticateByWindowsUsernameAsync(
    string windowsUsername, 
    IProgress<string>? progress = null)
{
    progress?.Report("Validating credentials...");
    // ... authentication logic
    progress?.Report($"Welcome, {user.FullName}");
}
```

**Rules**:
- Progress parameter must be optional (nullable)
- Always use null-conditional operator: `progress?.Report()`
- Provide user-friendly status messages
- Report at key milestones (start, validation, completion)

## Authentication Service Methods

### AuthenticateByWindowsUsernameAsync

Authenticates users on personal workstations using Windows username.

**Requirements**:
- Query database for user by `windows_username`
- Verify user is active (`is_active = true`)
- Log successful authentication to `user_activity_log`
- Return user object on success

**Error Cases**:
- User not found → ErrorResult("User not found")
- User inactive → ErrorResult("User account is inactive")
- Database error → Log and return generic error

### AuthenticateByPinAsync

Authenticates users on shared terminals using username + 4-digit PIN.

**Requirements**:
- Validate PIN format (4 digits, not all same)
- Query database using `sp_ValidateUserPin`
- Verify user is active
- Log successful authentication
- Track failed attempts (service does NOT implement lockout - handled by UI)

### CreateNewUserAsync

Creates new user accounts (used during first-time setup).

**Requirements**:
- Validate all required fields
- Check PIN uniqueness before creation
- Check Windows username uniqueness
- Call `sp_CreateNewUser` stored procedure
- Log user creation event
- Return new employee number on success

### ValidatePinAsync

Validates PIN format and uniqueness.

**Rules**:
- Exactly 4 digits
- Cannot be all same digit (e.g., "1111")
- Must be unique in database (unless `excludeEmployeeNumber` provided)

### DetectWorkstationTypeAsync

Determines if current workstation is personal or shared terminal.

**Logic**:
- Query `workstation_config` table by computer name
- If found → Return configured type
- If not found → Default to "personal_workstation"
- Never throw exceptions (return safe defaults)

## Session Manager Service

### Responsibility

Manages active user sessions with automatic timeout monitoring.

### Session Properties

```csharp
public class Model_UserSession
{
    public Model_User User { get; set; }
    public Model_WorkstationConfig WorkstationConfig { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public TimeSpan TimeoutDuration { get; set; }
    public string AuthenticationMethod { get; set; }
}
```

### Timeout Rules

- **Personal Workstation**: 30 minutes of inactivity
- **Shared Terminal**: 5 minutes of inactivity
- Check timeout every 60 seconds using `DispatcherTimer`
- Raise `SessionTimedOut` event when timeout detected

### Key Methods

**CreateSession**: Initializes new session with appropriate timeout
**UpdateLastActivity**: Resets activity timestamp (called on user interaction)
**StartTimeoutMonitoring**: Begins periodic timeout checks
**EndSessionAsync**: Logs session end event and cleans up

### Integration Points

**UI Integration**:
- Wire up mouse/keyboard events to call `UpdateLastActivity()`
- Subscribe to `SessionTimedOut` event in `App.xaml.cs`
- Update UI header to display current user

**App Lifecycle**:
- Start monitoring after successful authentication
- Stop monitoring on app close
- Log "session_end" event on close

## Activity Logging

All authentication events MUST be logged:

**Event Types**:
- `login_windows` - Windows username authentication
- `login_pin` - PIN authentication
- `login_failed` - Failed authentication attempt
- `user_created` - New user account created
- `session_timeout` - Session timed out
- `session_end` - User closed application

**Log Format**:
```csharp
await LogUserActivityAsync(
    eventType: "login_windows",
    username: user.WindowsUsername,
    workstationName: Environment.MachineName,
    details: $"Authentication successful for {user.FullName}"
);
```

**Rules**:
- Always log successful authentications
- Log failed attempts (but don't expose sensitive details)
- Don't block application flow if logging fails
- Use `Enum_ErrorSeverity.Info` for logging errors

## Testing Requirements

### Unit Tests

Required test coverage:

**Service_Authentication**:
- Test successful Windows username authentication
- Test successful PIN authentication
- Test failed authentication scenarios
- Test PIN validation (format, uniqueness)
- Test user creation (valid, duplicate username, duplicate PIN)
- Test workstation detection (configured, default)

**Service_SessionManager**:
- Test session creation with correct timeout durations
- Test activity timestamp updates
- Test timeout detection logic
- Test session end logging

### Integration Tests

Required scenarios:
- Full authentication flow (Windows username → session → timeout)
- Full authentication flow (PIN → session → timeout)
- Activity tracking integration with UI
- Session timeout event handling in App

## Best Practices

1. **Always use result objects** - Never throw exceptions from service methods
2. **Null-check error handler** - Use `if (_errorHandler != null)` before calling async methods
3. **Provide defaults** - Return safe defaults rather than failing (e.g., workstation detection)
4. **Log liberally** - Log all authentication events for audit trail
5. **Support progress reporting** - Enable UI feedback during long operations
6. **Validate early** - Check preconditions before calling DAO
7. **Clean error messages** - Return user-friendly error messages, log technical details

## Common Pitfalls

❌ **Don't**:
- Throw exceptions from service methods
- Use nullable error handler dependencies
- Block application flow for logging failures
- Return null from methods (use result objects)
- Hardcode timeout durations (use workstation-specific values)

✅ **Do**:
- Return strongly-typed result objects
- Require non-nullable dependencies in constructor
- Handle all exceptions gracefully
- Log authentication events consistently
- Support optional progress reporting

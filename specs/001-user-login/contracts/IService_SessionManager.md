# Service Contract: IService_SessionManager

**Purpose**: Manages active user sessions, tracks inactivity, and enforces session timeouts.

**Implementation**: `Service_SessionManager.cs`

## Interface Definition

```csharp
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for managing user sessions and enforcing session timeouts.
    /// </summary>
    public interface IService_SessionManager
    {
        /// <summary>
        /// Gets the current active user session (null if no user logged in).
        /// </summary>
        Model_UserSession? CurrentSession { get; }
        
        /// <summary>
        /// Creates a new session for the authenticated user.
        /// </summary>
        /// <param name="user">Authenticated user</param>
        /// <param name="workstationConfig">Workstation configuration</param>
        /// <param name="authenticationMethod">"windows_auto" or "pin_login"</param>
        void CreateSession(Model_User user, Model_WorkstationConfig workstationConfig, string authenticationMethod);
        
        /// <summary>
        /// Updates the last activity timestamp (called on any user interaction).
        /// </summary>
        void UpdateLastActivity();
        
        /// <summary>
        /// Starts monitoring for session timeout (begins timeout timer).
        /// </summary>
        void StartTimeoutMonitoring();
        
        /// <summary>
        /// Stops monitoring for session timeout (cleanup on app close).
        /// </summary>
        void StopTimeoutMonitoring();
        
        /// <summary>
        /// Checks if the current session has timed out.
        /// </summary>
        /// <returns>True if session has exceeded timeout duration</returns>
        bool IsSessionTimedOut();
        
        /// <summary>
        /// Clears the current session (called on app close or timeout).
        /// </summary>
        /// <param name="reason">Reason for ending session: "manual_close", "timeout"</param>
        Task EndSessionAsync(string reason);
        
        /// <summary>
        /// Event raised when session times out (application should close).
        /// </summary>
        event EventHandler<SessionTimeoutEventArgs>? SessionTimedOut;
    }
    
    /// <summary>
    /// Event args for SessionTimedOut event.
    /// </summary>
    public class SessionTimeoutEventArgs : EventArgs
    {
        public Model_UserSession Session { get; set; }
        public TimeSpan InactiveDuration { get; set; }
    }
}
```

## Method Specifications

### CurrentSession Property

**Purpose**: Provides access to active session data throughout the application

**Usage**:
```csharp
var currentUser = _sessionManager.CurrentSession?.User;
if (currentUser != null)
{
    var displayText = currentUser.DisplayName; // "John Smith (Emp #6229)"
    var employeeNumber = currentUser.EmployeeNumber; // 6229
}
```

**Lifecycle**:
- `null` from application start until authentication completes
- Set by `CreateSession()` after successful authentication
- Remains valid until `EndSessionAsync()` is called
- Cleared to `null` on session end

---

### CreateSession

**When Called**: Startup step 50% after successful authentication

**Parameters**:
- `user`: Authenticated `Model_User` object
- `workstationConfig`: Detected `Model_WorkstationConfig`
- `authenticationMethod`: "windows_auto" or "pin_login"

**Implementation**:
```csharp
public void CreateSession(Model_User user, Model_WorkstationConfig workstationConfig, string authenticationMethod)
{
    CurrentSession = new Model_UserSession
    {
        User = user,
        WorkstationName = workstationConfig.ComputerName,
        WorkstationType = workstationConfig.WorkstationType,
        AuthenticationMethod = authenticationMethod,
        LoginTimestamp = DateTime.Now,
        LastActivityTimestamp = DateTime.Now,
        TimeoutDuration = workstationConfig.IsSharedTerminal 
            ? TimeSpan.FromMinutes(15) 
            : TimeSpan.FromMinutes(30)
    };
}
```

**Post-Condition**: `CurrentSession` is not null and accessible throughout app

---

### UpdateLastActivity

**When Called**: On any user interaction event (mouse, keyboard, window focus)

**Event Bindings**:
- MainWindow.PointerMoved
- MainWindow.KeyDown
- MainWindow.GotFocus
- Any TextBox.TextChanged
- Any Button.Click

**Implementation**:
```csharp
public void UpdateLastActivity()
{
    if (CurrentSession != null)
    {
        CurrentSession.LastActivityTimestamp = DateTime.Now;
    }
}
```

**Performance**: < 1ms (in-memory update only)

**Frequency**: Could fire hundreds of times per minute (mouse movement) - must be lightweight

---

### StartTimeoutMonitoring

**When Called**: After `CreateSession()` completes (startup step 55%)

**Implementation**:
```csharp
private DispatcherTimer _timeoutTimer;

public void StartTimeoutMonitoring()
{
    _timeoutTimer = new DispatcherTimer();
    _timeoutTimer.Interval = TimeSpan.FromSeconds(60); // Check every 60 seconds
    _timeoutTimer.Tick += OnTimeoutTimerTick;
    _timeoutTimer.Start();
}

private async void OnTimeoutTimerTick(object sender, object e)
{
    if (IsSessionTimedOut())
    {
        _timeoutTimer.Stop();
        SessionTimedOut?.Invoke(this, new SessionTimeoutEventArgs
        {
            Session = CurrentSession,
            InactiveDuration = CurrentSession.TimeSinceLastActivity
        });
        await EndSessionAsync("timeout");
    }
}
```

**Timer Interval**: 60 seconds (balance between responsiveness and performance)

**Cleanup**: `StopTimeoutMonitoring()` called in App.xaml.cs OnClosed

---

### StopTimeoutMonitoring

**When Called**: Application closing (App.xaml.cs OnClosed event)

**Implementation**:
```csharp
public void StopTimeoutMonitoring()
{
    if (_timeoutTimer != null)
    {
        _timeoutTimer.Stop();
        _timeoutTimer.Tick -= OnTimeoutTimerTick;
        _timeoutTimer = null;
    }
}
```

**Purpose**: Cleanup to prevent memory leaks

---

### IsSessionTimedOut

**When Called**: Every 60 seconds by timeout timer, or manually for testing

**Logic**:
```csharp
public bool IsSessionTimedOut()
{
    if (CurrentSession == null) return false;
    
    var timeSinceActivity = DateTime.Now - CurrentSession.LastActivityTimestamp;
    return timeSinceActivity >= CurrentSession.TimeoutDuration;
}
```

**Timeout Durations**:
- Personal workstations: 30 minutes
- Shared terminals: 15 minutes

**Returns**: `true` if session has exceeded timeout, `false` otherwise

---

### EndSessionAsync

**When Called**: 
- Session timeout detected (automatic)
- Application closing (user closes window)

**Parameters**:
- `reason`: "timeout" or "manual_close"

**Implementation**:
```csharp
public async Task EndSessionAsync(string reason)
{
    if (CurrentSession == null) return;
    
    // Log session end
    var eventType = reason == "timeout" ? "session_timeout" : "session_ended";
    var details = reason == "timeout" 
        ? $"Inactivity timeout after {CurrentSession.TimeoutDuration.TotalMinutes} minutes"
        : "User closed application";
    
    await _authenticationService.LogUserActivityAsync(
        eventType,
        CurrentSession.User.WindowsUsername,
        CurrentSession.WorkstationName,
        details
    );
    
    // Clear session
    CurrentSession = null;
}
```

**Post-Condition**: `CurrentSession` is null

**Side Effect**: Activity logged in `user_activity_log` table

---

### SessionTimedOut Event

**Purpose**: Notify application that session has timed out and app should close

**Event Handler Location**: App.xaml.cs or MainWindow.xaml.cs

**Handler Implementation**:
```csharp
private async void OnSessionTimedOut(object sender, SessionTimeoutEventArgs e)
{
    // Show brief message (optional)
    var dialog = new ContentDialog
    {
        Title = "Session Timeout",
        Content = $"Your session has timed out after {e.InactiveDuration.TotalMinutes:F0} minutes of inactivity.",
        CloseButtonText = "OK",
        XamlRoot = this.Content.XamlRoot
    };
    await dialog.ShowAsync();
    
    // Close application
    Application.Current.Exit();
}
```

**Timing**: Event fires immediately when timeout detected, app closes within 2-3 seconds

## Usage Example

### App.xaml.cs Integration

```csharp
public partial class App : Application
{
    private IService_SessionManager _sessionManager;
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // ... existing code ...
        
        // Get session manager from DI container
        _sessionManager = _serviceProvider.GetRequiredService<IService_SessionManager>();
        
        // Subscribe to timeout event
        _sessionManager.SessionTimedOut += OnSessionTimedOut;
    }
    
    private async void OnSessionTimedOut(object sender, SessionTimeoutEventArgs e)
    {
        // Log details
        await _loggingService.LogInfoAsync($"Session timed out for user: {e.Session.User.WindowsUsername}");
        
        // Close application
        this.Exit();
    }
    
    protected override async void OnClosed(object sender, WindowEventArgs args)
    {
        // Cleanup
        _sessionManager.StopTimeoutMonitoring();
        await _sessionManager.EndSessionAsync("manual_close");
        
        base.OnClosed(sender, args);
    }
}
```

### MainWindow.xaml.cs Integration

```csharp
public sealed partial class MainWindow : Window
{
    private IService_SessionManager _sessionManager;
    
    public MainWindow()
    {
        this.InitializeComponent();
        
        // Get session manager
        _sessionManager = App.GetService<IService_SessionManager>();
        
        // Wire up activity tracking
        this.PointerMoved += (s, e) => _sessionManager.UpdateLastActivity();
        this.KeyDown += (s, e) => _sessionManager.UpdateLastActivity();
        this.Activated += (s, e) => _sessionManager.UpdateLastActivity();
    }
    
    private void UpdateUserDisplay()
    {
        if (_sessionManager.CurrentSession != null)
        {
            var user = _sessionManager.CurrentSession.User;
            UserDisplayTextBlock.Text = user.DisplayName; // "John Smith (Emp #6229)"
        }
    }
}
```

## State Management

### Session States

```
[No Session]
    ↓ CreateSession() called
[Active Session] (CurrentSession != null)
    ↓ User interacts with app
[Activity Updated] (LastActivityTimestamp refreshed)
    ↓ 15-30 minutes of no activity
[Timeout Detected] (IsSessionTimedOut() returns true)
    ↓ SessionTimedOut event fired
[Ending Session] (EndSessionAsync() called)
    ↓ Activity logged, CurrentSession cleared
[No Session]
```

### Concurrent Session Tracking

**Note**: This service manages a single session instance per application. Concurrent session prevention (same user, different workstation types) would require database-level tracking (out of scope for P1).

**Future Enhancement**: Add session registry table to track active sessions globally.

## Performance Requirements

| Operation | Target | Notes |
|-----------|--------|-------|
| CreateSession | < 10ms | In-memory only |
| UpdateLastActivity | < 1ms | Called frequently, must be lightweight |
| IsSessionTimedOut | < 1ms | Called every 60 seconds |
| EndSessionAsync | < 500ms | Includes database logging |
| Timer Interval | 60 seconds | Balance between responsiveness and CPU usage |

## Testing Requirements

### Unit Tests
- CreateSession initializes all properties correctly
- Timeout durations set correctly based on workstation type (30 min vs 15 min)
- UpdateLastActivity updates timestamp
- IsSessionTimedOut returns true/false correctly
- EndSessionAsync clears session and logs activity

### Integration Tests
- SessionTimedOut event fires when timeout exceeded
- Timer starts and stops correctly
- Multiple UpdateLastActivity calls don't cause issues
- Session persists across view navigation

### Mock Dependencies
- `IService_Authentication` for logging
- Mock time for testing timeout logic (inject `ITimeProvider`)

## Dependencies

- `IService_Authentication` (for `LogUserActivityAsync`)
- `Model_UserSession`, `Model_User`, `Model_WorkstationConfig` (Models)
- `DispatcherTimer` (WinUI 3)

## Contract Completion

**Status**: ✅ Complete and ready for implementation

**Review Notes**:
- Session lifecycle clearly defined
- Activity tracking pattern specified
- Timeout monitoring mechanism detailed
- Event-driven timeout notification
- Integration examples provided

# Startup Services Guidelines

**Category**: Application Lifecycle  
**Last Updated**: December 16, 2025  
**Applies To**: Services/Startup/* and Contracts/Services/IService_OnStartup_AppLifecycle.cs

## Purpose

Startup services orchestrate application initialization, authentication flows, and main window presentation in the MTM Receiving Application.

## Core Principles

### 1. Single Responsibility

The startup service (`Service_OnStartup_AppLifecycle`) has ONE clear purpose:

- Coordinate application initialization sequence
- Detect workstation type and route to appropriate authentication flow
- Create user session upon successful authentication
- Present main application window

### 2. Dependency Injection

Startup services MUST:

- Define corresponding interfaces in `Contracts/Services/`
- Accept all required services via constructor injection
- Use `IServiceProvider` to resolve transient services (e.g., Windows, Views)
- Be registered as transient or scoped (not singleton)

Example:

```csharp
public class Service_OnStartup_AppLifecycle : IService_OnStartup_AppLifecycle
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IService_Authentication _authService;
    private readonly IService_SessionManager _sessionManager;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_OnStartup_AppLifecycle(
        IServiceProvider serviceProvider,
        IService_Authentication authService,
        IService_SessionManager sessionManager,
        IService_ErrorHandler errorHandler)
    {
        _serviceProvider = serviceProvider;
        _authService = authService;
        _sessionManager = sessionManager;
        _errorHandler = errorHandler;
    }
}
```

### 3. Async Initialization

Startup must be fully asynchronous:

```csharp
public async Task StartAsync()
{
    try
    {
        // 1. Detect workstation type
        var workstationConfig = await _authService.DetectWorkstationTypeAsync();
        
        // 2. Route to authentication flow
        Model_User? authenticatedUser = null;
        if (workstationConfig.IsPersonalWorkstation)
        {
            authenticatedUser = await AuthenticateWindowsUser();
        }
        else if (workstationConfig.IsSharedTerminal)
        {
            authenticatedUser = await ShowPinLoginDialog();
        }
        
        // 3. Create session
        if (authenticatedUser != null)
        {
            _sessionManager.CreateSession(authenticatedUser, workstationConfig, "windows_auto");
            _sessionManager.StartTimeoutMonitoring();
        }
        
        // 4. Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        App.MainWindow = mainWindow;
        mainWindow.Activate();
    }
    catch (Exception ex)
    {
        await _errorHandler.HandleErrorAsync("Startup failed", Enum_ErrorSeverity.Critical, ex);
    }
}
```

## Startup Sequence

### Phase 1: Initialization (0-20%)

**Responsibilities**:

- Show splash screen (optional)
- Initialize logging services
- Load configuration
- Report progress: "Initializing services..."

**Error Handling**:

- Critical errors → Show dialog and exit
- Non-critical → Log and continue with defaults

### Phase 2: Workstation Detection (20-40%)

**Responsibilities**:

- Call `DetectWorkstationTypeAsync()`
- Determine authentication flow
- Report progress: "Detecting workstation configuration..."

**Logic**:

```csharp
var workstationConfig = await _authService.DetectWorkstationTypeAsync();
if (workstationConfig.IsPersonalWorkstation)
{
    // Personal workstation → Auto-login with Windows username
}
else if (workstationConfig.IsSharedTerminal)
{
    // Shared terminal → Show PIN login dialog
}
```

### Phase 3: Authentication (40-80%)

**Personal Workstation Flow**:

1. Get Windows username: `Environment.UserName`
2. Call `AuthenticateByWindowsUsernameAsync(windowsUser)`
3. Handle results:
   - Success → Proceed to session creation
   - User not found → Show new user creation dialog (future)
   - Error → Show error dialog with retry option

**Shared Terminal Flow**:

1. Show PIN login dialog
2. Call `AuthenticateByPinAsync(username, pin)`
3. Handle results:
   - Success → Proceed to session creation
   - Failed (< 3 attempts) → Show error, allow retry
   - Failed (3 attempts) → Show lockout message, close app
   - Error → Show error dialog

**Progress Reporting**:

- 45%: "Authenticating Windows user..."
- 60%: "Validating credentials..."
- 80%: "Welcome, [User Name]"

### Phase 4: Session Creation (80-90%)

**Responsibilities**:

- Create session: `_sessionManager.CreateSession(user, workstation, authMethod)`
- Start timeout monitoring: `_sessionManager.StartTimeoutMonitoring()`
- Log login event
- Report progress: "Creating user session..."

**Session Properties**:

- Set appropriate timeout (30 min personal, 5 min shared)
- Record authentication method ("windows_auto", "pin_manual")
- Initialize last activity timestamp

### Phase 5: Main Window Presentation (90-100%)

**Responsibilities**:

- Resolve MainWindow from DI container
- Set `App.MainWindow` property
- Activate main window
- Close splash screen (if shown)
- Report progress: "Starting application..."

**Example**:

```csharp
var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
App.MainWindow = mainWindow;
mainWindow.Activate();

// Close splash if present
_splashScreen?.Close();
```

## Error Handling

### Critical Errors

**Definition**: Errors that prevent application startup

- Database connection failure
- Missing configuration
- Unhandled exceptions in core services

**Response**:

1. Log error with `Enum_ErrorSeverity.Critical`
2. Show user-friendly error dialog
3. Close splash screen
4. Exit application gracefully

```csharp
catch (Exception ex)
{
    await _errorHandler.HandleErrorAsync(
        "Startup failed", 
        Enum_ErrorSeverity.Critical, 
        ex);
    _splashScreen?.Close();
    // Application will exit
}
```

### Non-Critical Errors

**Definition**: Errors that allow degraded functionality

- Splash screen display failure
- Non-essential service initialization failure

**Response**:

1. Log error with `Enum_ErrorSeverity.Warning`
2. Continue with default/fallback behavior
3. Don't show dialogs (would interrupt startup)

```csharp
try
{
    _splashScreen = new SplashScreenWindow();
    _splashScreen.Activate();
}
catch (Exception ex)
{
    // Log but continue without splash screen
    if (_errorHandler != null)
        await _errorHandler.HandleErrorAsync(
            "Failed to show splash screen", 
            Enum_ErrorSeverity.Warning, 
            ex, 
            showDialog: false);
}
```

## Progress Reporting

### Splash Screen Integration

If splash screen is available:

```csharp
private void UpdateSplash(double percentage, string message)
{
    _splashScreen?.DispatcherQueue.TryEnqueue(() => 
    {
        _splashScreen.UpdateProgress(percentage, message);
    });
}
```

**Progress Milestones**:

- 20%: "Initializing services..."
- 40%: "Detecting workstation configuration..."
- 45%: "Authenticating Windows user..." / "Waiting for login..."
- 50%: "Creating user session..."
- 55%: "Starting timeout monitoring..."
- 90%: "Loading application..."
- 100%: "Starting application..."

### Without Splash Screen

If splash screen fails or is not available:

- Continue startup silently
- Don't block on progress updates
- Log milestones for diagnostics

## Splash Screen Guidelines

### XAML Compilation Issues

**Common Problem**: WinUI 3 XAML compiler can be fragile with certain patterns

**Symptoms**:

- `MSB3073: XamlCompiler.exe exited with code 1`
- No specific error details in build output
- Random failures with valid XAML

**Solutions**:

1. **Simplify XAML**: Start with minimal structure
2. **Avoid namespace issues**: Keep Window classes in simple namespaces
3. **Test incrementally**: Add features one at a time
4. **Hard clean**: Delete `obj/` and `bin/` folders between attempts
5. **Fallback gracefully**: Make splash screen optional

**Template**:

```xml
<Window
    x:Class="MTM_Receiving_Application.Views.SplashScreenWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid>
        <TextBlock Text="Loading..." 
                   HorizontalAlignment="Center" 
                   VerticalAlignment="Center"/>
    </Grid>
</Window>
```

### Code-Behind Requirements

```csharp
public sealed partial class SplashScreenWindow : Window
{
    public SplashScreenViewModel ViewModel { get; }

    public SplashScreenWindow()
    {
        ViewModel = App.GetService<SplashScreenViewModel>();
        InitializeComponent();
        
        ExtendsContentIntoTitleBar = true;
        SystemBackdrop = new MicaBackdrop();
    }

    public void UpdateProgress(double percentage, string message)
    {
        ViewModel.UpdateProgress(percentage, message);
    }
}
```

### ViewModel Requirements

Must inherit from `BaseViewModel`:

```csharp
public partial class SplashScreenViewModel : BaseViewModel
{
    [ObservableProperty]
    private double _progressPercentage;

    public SplashScreenViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger) 
        : base(errorHandler, logger)
    {
        StatusMessage = "Initializing...";
    }

    public void UpdateProgress(double percentage, string message)
    {
        ProgressPercentage = percentage;
        StatusMessage = message;
    }
}
```

## Integration with App.xaml.cs

### OnLaunched Override

```csharp
protected override async void OnLaunched(LaunchActivatedEventArgs args)
{
    var startupService = _host.Services.GetRequiredService<IService_OnStartup_AppLifecycle>();
    await startupService.StartAsync();

    // Subscribe to session events
    var sessionManager = _host.Services.GetRequiredService<IService_SessionManager>();
    sessionManager.SessionTimedOut += OnSessionTimedOut;

    if (MainWindow != null)
    {
        MainWindow.Closed += OnMainWindowClosed;
    }
}
```

### Session Event Handlers

```csharp
private void OnSessionTimedOut(object? sender, SessionTimedOutEventArgs e)
{
    // Close application on timeout
    MainWindow?.Close();
}

private async void OnMainWindowClosed(object sender, WindowEventArgs args)
{
    var sessionManager = _host.Services.GetRequiredService<IService_SessionManager>();
    await sessionManager.EndSessionAsync("manual_close");
}
```

## Testing Strategies

### Manual Testing

**Personal Workstation Scenario**:

1. Launch app on personal workstation
2. Verify auto-login with Windows username
3. Verify main window shows with user info in header
4. Verify session timeout after 30 minutes of inactivity
5. Verify app closes on timeout

**Shared Terminal Scenario**:

1. Launch app on shared terminal (SHOP2, MTMDC)
2. Verify PIN login dialog appears
3. Test valid credentials → successful login
4. Test invalid credentials → error message
5. Test 3 failed attempts → lockout and close
6. Verify session timeout after 5 minutes

### Integration Testing

Create tests for:

- Successful Windows authentication flow
- Successful PIN authentication flow
- Database connection failure handling
- Session creation and timeout monitoring
- Main window presentation

### Unit Testing

Test startup service methods independently:

- Workstation detection logic
- Authentication flow routing
- Session creation
- Error handling paths

## Best Practices

1. **Make startup resilient** - Handle failures gracefully
2. **Provide progress feedback** - Keep users informed
3. **Log all milestones** - Aid troubleshooting
4. **Use async/await properly** - Don't block UI thread
5. **Validate prerequisites** - Check database connectivity early
6. **Support testing** - Expose testable methods
7. **Handle splash screen failures** - Continue without if needed

## Common Pitfalls

❌ **Don't**:

- Block UI thread during initialization
- Show multiple dialogs during startup
- Continue startup after critical errors
- Hardcode authentication flows
- Ignore splash screen display failures

✅ **Do**:

- Use fully asynchronous initialization
- Show single error dialog for critical errors
- Route authentication based on workstation type
- Make splash screen optional
- Log all startup milestones

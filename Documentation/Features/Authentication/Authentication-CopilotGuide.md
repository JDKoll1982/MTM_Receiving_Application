# Authentication - Copilot Guide

Quick reference for AI assistants working with the MTM Receiving Application authentication system.

---

## Key Classes

```csharp
IService_Authentication          // Main authentication service
Service_SessionManager           // Session timeout handling
Model_User                       // User data model
Model_UserSession                // Active session data
Model_WorkstationConfig          // Workstation type config
AuthenticationResult             // Authentication result wrapper
```

---

## Common Tasks

### 1. Authenticate User by Windows Username

```csharp
var authService = App.GetService<IService_Authentication>();
var result = await authService.AuthenticateByWindowsUsernameAsync(Environment.UserName);

if (result.IsSuccess)
{
    var user = result.User;
    // Create session, show main window
}
else
{
    // Show error or new user dialog
}
```

### 2. Authenticate User by PIN

```csharp
var authService = App.GetService<IService_Authentication>();
var result = await authService.AuthenticateByPinAsync(username, pin);

if (result.IsSuccess)
{
    var user = result.User;
    // Create session
}
else
{
    // Show error, increment failed attempts
    await authService.LogUserActivityAsync("login_failed", username, 
        Environment.MachineName, "Invalid PIN");
}
```

### 3. Create New User

```csharp
var newUser = new Model_User
{
    WindowsUsername = Environment.UserName,
    FullName = fullNameTextBox.Text,
    Pin = pinTextBox.Text,
    Department = departmentComboBox.SelectedItem.ToString(),
    Shift = shiftComboBox.SelectedItem.ToString(),
    IsActive = true
};

var authService = App.GetService<IService_Authentication>();
var result = await authService.CreateNewUserAsync(newUser, Environment.UserName);

if (result.IsSuccess)
{
    var employeeNumber = result.User.EmployeeNumber;
    // Show success message with employee number
}
```

### 4. Detect Workstation Type

```csharp
var authService = App.GetService<IService_Authentication>();
var workstationConfig = await authService.DetectWorkstationTypeAsync();

if (workstationConfig.WorkstationType == "shared_terminal")
{
    // Show PIN login dialog
}
else
{
    // Use Windows authentication
}
```

### 5. Create and Manage Session

```csharp
var sessionManager = App.GetService<IService_SessionManager>();

// Create session
var session = sessionManager.CreateSession(user, workstationConfig, "windows_auto");

// Start timeout monitoring
sessionManager.StartTimeoutMonitoring();

// Subscribe to timeout event
sessionManager.SessionTimedOut += OnSessionTimedOut;

// Update last activity (wire up to UI events)
private void UpdateActivity(object sender, EventArgs e)
{
    sessionManager?.UpdateLastActivity();
}
```

### 6. Check if Session is Timed Out

```csharp
var sessionManager = App.GetService<IService_SessionManager>();

if (sessionManager.IsSessionTimedOut())
{
    // Close application
    await sessionManager.EndSessionAsync("timeout");
    Application.Current.Exit();
}
```

### 7. Log User Activity

```csharp
var authService = App.GetService<IService_Authentication>();

await authService.LogUserActivityAsync(
    eventType: "login_success",    // Event types: login_success, login_failed, user_created, session_timeout
    username: user.WindowsUsername,
    workstationName: Environment.MachineName,
    details: $"Windows auth for {user.FullName}"
);
```

### 8. Validate PIN Before Saving

```csharp
var authService = App.GetService<IService_Authentication>();
var validation = await authService.ValidatePinAsync(pin, excludeEmployeeNumber: null);

if (!validation.IsValid)
{
    // Show error: validation.ErrorMessage
    // Examples: "PIN must be exactly 4 digits", "PIN already in use"
}
```

### 9. Get Active Departments for Dropdown

```csharp
var authService = App.GetService<IService_Authentication>();
var departments = await authService.GetActiveDepartmentsAsync();

departmentComboBox.ItemsSource = departments;
```

---

## Database Queries

### Get User by Windows Username

```sql
SELECT * FROM users WHERE windows_username = 'jsmith';
```

### Validate PIN

```sql
SELECT * FROM users 
WHERE windows_username = 'jsmith' 
  AND pin = '1234' 
  AND is_active = TRUE;
```

### Check PIN Uniqueness

```sql
SELECT COUNT(*) FROM users WHERE pin = '5678';
```

### Get Workstation Configuration

```sql
SELECT * FROM workstation_config WHERE computer_name = 'SHOP2';
```

### Add Shared Terminal

```sql
INSERT INTO workstation_config (computer_name, workstation_type, description)
VALUES ('SHOP3', 'shared_terminal', 'Shop floor terminal - Assembly');
```

### Get Recent Login Activity

```sql
SELECT event_timestamp, event_type, username, workstation_name, event_details
FROM user_activity_log
WHERE event_type IN ('login_success', 'login_failed')
ORDER BY event_timestamp DESC
LIMIT 50;
```

---

## Testing Patterns

### Mock Authentication Service

```csharp
var mockAuthService = new Mock<IService_Authentication>();
mockAuthService
    .Setup(s => s.AuthenticateByWindowsUsernameAsync(It.IsAny<string>(), null))
    .ReturnsAsync(AuthenticationResult.SuccessResult(new Model_User
    {
        EmployeeNumber = 6229,
        FullName = "John Smith",
        WindowsUsername = "jsmith",
        IsActive = true
    }));
```

### Test New User Creation

```csharp
[Fact]
public async Task CreateNewUser_ValidData_ReturnsSuccess()
{
    // Arrange
    var user = new Model_User
    {
        WindowsUsername = "jdoe",
        FullName = "Jane Doe",
        Pin = "1234",
        Department = "Receiving",
        Shift = "1st Shift"
    };
    
    var authService = new Service_Authentication(_mockDao.Object, _mockErrorHandler.Object);
    
    // Act
    var result = await authService.CreateNewUserAsync(user, "admin");
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.User.EmployeeNumber > 0);
}
```

### Test Session Timeout

```csharp
[Fact]
public void IsSessionTimedOut_After30Minutes_ReturnsTrue()
{
    // Arrange
    var sessionManager = new Service_SessionManager(_mockAuthService.Object, _mockErrorHandler.Object);
    var session = sessionManager.CreateSession(testUser, testConfig, "windows_auto");
    
    // Simulate 31 minutes passing
    session.LastActivityTime = DateTime.Now.AddMinutes(-31);
    
    // Act
    var isTimedOut = sessionManager.IsSessionTimedOut();
    
    // Assert
    Assert.True(isTimedOut);
}
```

---

## Code Generation Hints

### Generate New User Dialog

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Views.Shared.NewUserSetupDialog"
    Width="600"
    PrimaryButtonText="Create Account"
    SecondaryButtonText="Cancel">
    
    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="16" Padding="24">
            <TextBox Header="Full Name" Text="{x:Bind ViewModel.FullName, Mode=TwoWay}"/>
            <TextBox Header="Windows Username" Text="{x:Bind ViewModel.WindowsUsername}" IsReadOnly="True"/>
            <ComboBox Header="Shift" ItemsSource="{x:Bind ViewModel.Shifts}" SelectedItem="{x:Bind ViewModel.SelectedShift, Mode=TwoWay}"/>
            <PasswordBox Header="4-Digit PIN" Password="{x:Bind ViewModel.Pin, Mode=TwoWay}" MaxLength="4"/>
            <PasswordBox Header="Confirm PIN" Password="{x:Bind ViewModel.ConfirmPin, Mode=TwoWay}" MaxLength="4"/>
            <TextBox Header="Department (Optional)" Text="{x:Bind ViewModel.Department, Mode=TwoWay}"/>
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

### Generate Login Dialog

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Views.Shared.SharedTerminalLoginDialog"
    Width="400"
    PrimaryButtonText="Login"
    SecondaryButtonText="Cancel">
    
    <StackPanel Spacing="16" Padding="24">
        <TextBlock Text="Enter your credentials" Style="{StaticResource TitleTextBlockStyle}"/>
        <TextBox Header="Username" Text="{x:Bind ViewModel.Username, Mode=TwoWay}"/>
        <PasswordBox Header="PIN" Password="{x:Bind ViewModel.Pin, Mode=TwoWay}" MaxLength="4"/>
        <TextBlock Text="{x:Bind ViewModel.ErrorMessage, Mode=OneWay}" Foreground="Red"/>
    </StackPanel>
</ContentDialog>
```

---

## Configuration

### Timeout Durations

```csharp
// In Model_WorkstationConfig or startup configuration
PersonalWorkstationTimeout = TimeSpan.FromMinutes(30);
SharedTerminalTimeout = TimeSpan.FromMinutes(15);
```

### Shared Terminal Detection

```csharp
// Add to workstation_config table
var sharedTerminalNames = new[] { "SHOP2", "MTMDC" };
var sharedTerminalPatterns = new[] { "SHOP-FLOOR-*" };

// Detection logic
string computerName = Environment.MachineName;
bool isSharedTerminal = sharedTerminalNames.Contains(computerName) ||
    sharedTerminalPatterns.Any(pattern => 
        computerName.StartsWith(pattern.Replace("*", "")));
```

---

## Error Messages

### User-Friendly Error Messages

```csharp
// Authentication failures
"Invalid username or PIN. Please try again."
"User account is inactive. Contact your supervisor."
"Maximum login attempts exceeded. Application will close."

// Validation errors
"PIN must be exactly 4 numeric digits."
"PIN and Confirm PIN do not match."
"This PIN is already in use by another employee."
"Full Name is required and must be 2-100 characters."

// Database errors
"Unable to connect to database. Please check your network connection."
"Failed to create user account. Please contact IT support."
```

---

## Quick Reference: Startup Integration

```csharp
// In Service_OnStartup_AppLifecycle.StartAsync()
public async Task StartAsync()
{
    try
    {
        var authService = _serviceProvider.GetRequiredService<IService_Authentication>();
        var sessionManager = _serviceProvider.GetRequiredService<IService_SessionManager>();
        
        // 1. Detect workstation type
        var workstationConfig = await authService.DetectWorkstationTypeAsync();
        
        // 2. Authenticate based on workstation type
        Model_User? authenticatedUser = null;
        
        if (workstationConfig.WorkstationType == "personal_workstation")
        {
            var result = await authService.AuthenticateByWindowsUsernameAsync(Environment.UserName);
            if (result.IsSuccess)
            {
                authenticatedUser = result.User;
            }
            else if (result.IsNotFound)
            {
                // Show new user setup dialog
                var newUserDialog = new NewUserSetupDialog();
                if (await newUserDialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    authenticatedUser = newUserDialog.CreatedUser;
                }
            }
        }
        else // shared_terminal
        {
            var loginDialog = new SharedTerminalLoginDialog();
            if (await loginDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                authenticatedUser = loginDialog.AuthenticatedUser;
            }
        }
        
        // 3. Create session
        if (authenticatedUser != null)
        {
            sessionManager.CreateSession(authenticatedUser, workstationConfig, 
                workstationConfig.WorkstationType == "personal_workstation" ? "windows_auto" : "pin_manual");
            sessionManager.StartTimeoutMonitoring();
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

---

## Dependencies

```csharp
// Required NuGet packages
// - MySql.Data (9.4.0+)
// - CommunityToolkit.Mvvm (8.3.2+)

// Required services
IService_ErrorHandler         // Error handling
ILoggingService              // Activity logging
Dao_User                     // User database access

// Required models
Model_User
Model_UserSession
Model_WorkstationConfig
AuthenticationResult
CreateUserResult
ValidationResult
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0

# Authentication - Developer Guide

## Architecture Overview

The authentication system implements a multi-tier architecture with automatic workstation detection and dual authentication flows (Windows username vs PIN).

**Key Components:**
- `IService_Authentication` - Main authentication service
- `IService_SessionManager` - Session timeout management
- `Model_User` - User data model
- `Model_UserSession` - Active session data
- `Model_WorkstationConfig` - Workstation type configuration

---

## Authentication Flows

### 1. Personal Workstation (Windows Username Auto-Login)

**Flow:**
```
Application Start
    ↓
Detect Workstation Type (via computer name)
    ↓
Read Windows Username (Environment.UserName)
    ↓
Query Database: sp_GetUserByWindowsUsername
    ↓
┌─────────────────────────────────────┐
│ User Found?                          │
├─────────────────────────────────────┤
│ YES → Load User Profile             │
│       Create Session                │
│       Start Timeout Monitoring      │
│       Show Main Window              │
│                                     │
│ NO  → Show New User Setup Dialog    │
│       Create Account                │
│       Create Session                │
│       Show Main Window              │
└─────────────────────────────────────┘
```

**Implementation:**
```csharp
public async Task<AuthenticationResult> AuthenticateByWindowsUsernameAsync(
    string windowsUsername, 
    IProgress<string>? progress = null)
{
    progress?.Report("Validating credentials...");
    
    try
    {
        var result = await _daoUser.GetUserByWindowsUsernameAsync(windowsUsername);
        
        if (result.Success && result.Data != null)
        {
            var user = result.Data;
            
            // Verify active status
            if (!user.IsActive)
                return AuthenticationResult.ErrorResult("User account is inactive");
            
            // Log successful authentication
            await LogUserActivityAsync("login_success", user.WindowsUsername, 
                Environment.MachineName, $"Windows auth for {user.FullName}");
            
            progress?.Report($"Welcome, {user.FullName}");
            return AuthenticationResult.SuccessResult(user);
        }
        
        return AuthenticationResult.NotFoundResult("User not found");
    }
    catch (Exception ex)
    {
        if (_errorHandler != null)
            await _errorHandler.HandleErrorAsync("Authentication failed", 
                Enum_ErrorSeverity.Error, ex, false);
        return AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
    }
}
```

---

### 2. Shared Terminal (PIN Login)

**Flow:**
```
Application Start
    ↓
Detect Workstation Type (shared terminal)
    ↓
Show Login Dialog (Username + PIN)
    ↓
User Enters Credentials
    ↓
Call sp_ValidateUserPin(username, pin)
    ↓
┌─────────────────────────────────────┐
│ Valid Credentials?                   │
├─────────────────────────────────────┤
│ YES → Load User Profile             │
│       Create Session                │
│       Start Timeout Monitoring      │
│       Show Main Window              │
│                                     │
│ NO  → Increment Failed Attempts     │
│       Log Failure                   │
│       If attempts < 3: Allow Retry  │
│       If attempts = 3: Lock Out     │
└─────────────────────────────────────┘
```

**Implementation:**
```csharp
public async Task<AuthenticationResult> AuthenticateByPinAsync(
    string username, 
    string pin, 
    IProgress<string>? progress = null)
{
    progress?.Report($"Authenticating {username}...");
    
    try
    {
        // Validate PIN format
        var validation = ValidatePin(pin);
        if (!validation.IsValid)
            return AuthenticationResult.ErrorResult(validation.ErrorMessage);
        
        // Call stored procedure
        var result = await _daoUser.ValidateUserPinAsync(username, pin);
        
        if (result.Success && result.Data != null)
        {
            var user = result.Data;
            
            // Verify active status
            if (!user.IsActive)
                return AuthenticationResult.ErrorResult("User account is inactive");
            
            // Log successful authentication
            await LogUserActivityAsync("login_success", username, 
                Environment.MachineName, $"PIN auth for {user.FullName}");
            
            progress?.Report($"Welcome, {user.FullName}");
            return AuthenticationResult.SuccessResult(user);
        }
        
        // Log failed attempt
        await LogUserActivityAsync("login_failed", username, 
            Environment.MachineName, $"Invalid PIN attempt");
        
        return AuthenticationResult.ErrorResult("Invalid username or PIN");
    }
    catch (Exception ex)
    {
        if (_errorHandler != null)
            await _errorHandler.HandleErrorAsync("Authentication failed", 
                Enum_ErrorSeverity.Error, ex, false);
        return AuthenticationResult.ErrorResult($"Authentication error: {ex.Message}");
    }
}
```

---

### 3. New User Creation

**Implementation:**
```csharp
public async Task<CreateUserResult> CreateNewUserAsync(
    Model_User user, 
    string createdBy, 
    IProgress<string>? progress = null)
{
    progress?.Report("Creating user account...");
    
    try
    {
        // Validate PIN uniqueness
        var pinValidation = await ValidatePinAsync(user.Pin, excludeEmployeeNumber: null);
        if (!pinValidation.IsValid)
            return CreateUserResult.ErrorResult(pinValidation.ErrorMessage);
        
        // Call stored procedure
        var result = await _daoUser.CreateUserAsync(user, createdBy);
        
        if (result.Success)
        {
            // Log user creation
            await LogUserActivityAsync("user_created", user.WindowsUsername, 
                Environment.MachineName, 
                $"Account created for {user.FullName} by {createdBy}");
            
            progress?.Report($"Account created. Employee #{result.Data.EmployeeNumber}");
            return CreateUserResult.SuccessResult(result.Data);
        }
        
        return CreateUserResult.ErrorResult(result.ErrorMessage);
    }
    catch (Exception ex)
    {
        if (_errorHandler != null)
            await _errorHandler.HandleErrorAsync("User creation failed", 
                Enum_ErrorSeverity.Error, ex, false);
        return CreateUserResult.ErrorResult($"Error creating user: {ex.Message}");
    }
}
```

---

## Session Management

### Session Creation

```csharp
public Model_UserSession CreateSession(
    Model_User user, 
    Model_WorkstationConfig workstationConfig, 
    string authenticationMethod)
{
    var session = new Model_UserSession
    {
        User = user,
        WorkstationConfig = workstationConfig,
        LoginTime = DateTime.Now,
        LastActivityTime = DateTime.Now,
        AuthenticationMethod = authenticationMethod,
        TimeoutDuration = workstationConfig.WorkstationType == "shared_terminal" 
            ? TimeSpan.FromMinutes(15) 
            : TimeSpan.FromMinutes(30)
    };
    
    _currentSession = session;
    return session;
}
```

### Timeout Monitoring

```csharp
public void StartTimeoutMonitoring()
{
    _timeoutTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromSeconds(60) // Check every minute
    };
    
    _timeoutTimer.Tick += (sender, e) =>
    {
        if (IsSessionTimedOut())
        {
            _timeoutTimer.Stop();
            SessionTimedOut?.Invoke(this, new SessionTimedOutEventArgs
            {
                User = _currentSession.User,
                LoginTime = _currentSession.LoginTime,
                LastActivityTime = _currentSession.LastActivityTime
            });
        }
    };
    
    _timeoutTimer.Start();
}

public bool IsSessionTimedOut()
{
    if (_currentSession == null) return false;
    
    var inactiveTime = DateTime.Now - _currentSession.LastActivityTime;
    return inactiveTime >= _currentSession.TimeoutDuration;
}

public void UpdateLastActivity()
{
    if (_currentSession != null)
        _currentSession.LastActivityTime = DateTime.Now;
}
```

### Activity Tracking Integration

In `MainWindow.xaml.cs`:
```csharp
public MainWindow()
{
    InitializeComponent();
    
    // Wire up activity tracking
    this.PointerMoved += (s, e) => UpdateActivity();
    this.KeyDown += (s, e) => UpdateActivity();
    this.Activated += (s, e) => UpdateActivity();
}

private void UpdateActivity()
{
    var sessionManager = App.GetService<IService_SessionManager>();
    sessionManager?.UpdateLastActivity();
}
```

---

## Database Schema

### users Table

```sql
CREATE TABLE `users` (
    `employee_number` INT AUTO_INCREMENT PRIMARY KEY,
    `windows_username` VARCHAR(50) UNIQUE NOT NULL,
    `full_name` VARCHAR(100) NOT NULL,
    `pin` VARCHAR(4) UNIQUE NOT NULL,
    `department` VARCHAR(50) NOT NULL,
    `shift` ENUM('1st Shift', '2nd Shift', '3rd Shift') NOT NULL,
    `is_active` BOOLEAN DEFAULT TRUE,
    `visual_username` VARCHAR(50) NULL,
    `visual_password` VARCHAR(100) NULL,
    `created_date` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `created_by` VARCHAR(50) NULL,
    `modified_date` DATETIME ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_windows_username (windows_username),
    INDEX idx_pin (pin),
    INDEX idx_is_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### workstation_config Table

```sql
CREATE TABLE `workstation_config` (
    `computer_name` VARCHAR(50) PRIMARY KEY,
    `workstation_type` ENUM('personal_workstation', 'shared_terminal') NOT NULL,
    `description` VARCHAR(255) NULL,
    `timeout_minutes` INT NULL,
    `created_date` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_workstation_type (workstation_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### departments Table

```sql
CREATE TABLE `departments` (
    `department_id` INT AUTO_INCREMENT PRIMARY KEY,
    `department_name` VARCHAR(50) UNIQUE NOT NULL,
    `is_active` BOOLEAN DEFAULT TRUE,
    `sort_order` INT DEFAULT 999,
    `created_date` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_is_active (is_active),
    INDEX idx_sort_order (sort_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### user_activity_log Table

```sql
CREATE TABLE `user_activity_log` (
    `log_id` INT AUTO_INCREMENT PRIMARY KEY,
    `event_type` VARCHAR(50) NOT NULL,
    `username` VARCHAR(50) NOT NULL,
    `workstation_name` VARCHAR(50) NOT NULL,
    `event_details` TEXT NULL,
    `event_timestamp` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_event_type (event_type),
    INDEX idx_username (username),
    INDEX idx_event_timestamp (event_timestamp)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

## Stored Procedures

### sp_GetUserByWindowsUsername

```sql
DELIMITER $$

CREATE PROCEDURE `sp_GetUserByWindowsUsername`(
    IN p_windows_username VARCHAR(50),
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
    END;
    
    SELECT 
        employee_number,
        windows_username,
        full_name,
        pin,
        department,
        shift,
        is_active,
        visual_username,
        visual_password,
        created_date
    FROM users
    WHERE windows_username = p_windows_username;
    
    SET p_Status = 1;
    SET p_ErrorMsg = 'Success';
END $$

DELIMITER ;
```

### sp_ValidateUserPin

```sql
DELIMITER $$

CREATE PROCEDURE `sp_ValidateUserPin`(
    IN p_username VARCHAR(50),
    IN p_pin VARCHAR(4),
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
    END;
    
    SELECT 
        employee_number,
        windows_username,
        full_name,
        pin,
        department,
        shift,
        is_active
    FROM users
    WHERE windows_username = p_username
      AND pin = p_pin
      AND is_active = TRUE;
    
    IF FOUND_ROWS() > 0 THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'Valid credentials';
    ELSE
        SET p_Status = 0;
        SET p_ErrorMsg = 'Invalid username or PIN';
    END IF;
END $$

DELIMITER ;
```

### sp_CreateNewUser

```sql
DELIMITER $$

CREATE PROCEDURE `sp_CreateNewUser`(
    IN p_windows_username VARCHAR(50),
    IN p_full_name VARCHAR(100),
    IN p_pin VARCHAR(4),
    IN p_department VARCHAR(50),
    IN p_shift ENUM('1st Shift', '2nd Shift', '3rd Shift'),
    IN p_created_by VARCHAR(50),
    IN p_visual_username VARCHAR(50),
    IN p_visual_password VARCHAR(100),
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE v_employee_number INT;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Validate PIN uniqueness
    IF EXISTS (SELECT 1 FROM users WHERE pin = p_pin) THEN
        SET p_Status = -1;
        SET p_ErrorMsg = 'PIN already in use';
        ROLLBACK;
    ELSE
        INSERT INTO users (
            windows_username,
            full_name,
            pin,
            department,
            shift,
            created_by,
            visual_username,
            visual_password
        ) VALUES (
            p_windows_username,
            p_full_name,
            p_pin,
            p_department,
            p_shift,
            p_created_by,
            p_visual_username,
            p_visual_password
        );
        
        SET v_employee_number = LAST_INSERT_ID();
        SET p_Status = v_employee_number;
        SET p_ErrorMsg = CONCAT('User created: Employee #', v_employee_number);
        
        COMMIT;
    END IF;
END $$

DELIMITER ;
```

### sp_GetDepartments

```sql
DELIMITER $$

CREATE PROCEDURE `sp_GetDepartments`(
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
    END;
    
    SELECT 
        department_id,
        department_name,
        is_active,
        sort_order
    FROM departments
    WHERE is_active = TRUE
    ORDER BY sort_order, department_name;
    
    SET p_Status = 1;
    SET p_ErrorMsg = 'Success';
END $$

DELIMITER ;
```

### sp_LogUserActivity

```sql
DELIMITER $$

CREATE PROCEDURE `sp_LogUserActivity`(
    IN p_event_type VARCHAR(50),
    IN p_username VARCHAR(50),
    IN p_workstation_name VARCHAR(50),
    IN p_event_details TEXT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
    END;
    
    INSERT INTO user_activity_log (
        event_type,
        username,
        workstation_name,
        event_details
    ) VALUES (
        p_event_type,
        p_username,
        p_workstation_name,
        p_event_details
    );
    
    SET p_Status = 1;
    SET p_ErrorMsg = 'Activity logged';
END $$

DELIMITER ;
```

---

## Security Considerations

### PIN Storage
- **Current Implementation**: Stored in plain text (4-digit numeric)
- **Rationale**: Facility access control, not high-security data
- **Protection**: 3-attempt lockout, audit logging

### ERP Credentials
- **Storage**: Plain text in database
- **Rationale**: ERP API requires plaintext for authentication
- **Protection**: Database access controls, secure server
- **Disclosure**: User warned during account creation

### SQL Injection Prevention
- **Method**: All database access through stored procedures
- **Validation**: Input validation in service layer
- **No Inline SQL**: No dynamic SQL construction

### Session Security
- **Timeout**: Automatic logout after inactivity
- **Activity Tracking**: Mouse, keyboard, window activation
- **Audit Trail**: All events logged with timestamps

---

## API Reference

### IService_Authentication

```csharp
Task<AuthenticationResult> AuthenticateByWindowsUsernameAsync(
    string windowsUsername, 
    IProgress<string>? progress = null);

Task<AuthenticationResult> AuthenticateByPinAsync(
    string username, 
    string pin, 
    IProgress<string>? progress = null);

Task<CreateUserResult> CreateNewUserAsync(
    Model_User user, 
    string createdBy, 
    IProgress<string>? progress = null);

Task<ValidationResult> ValidatePinAsync(
    string pin, 
    int? excludeEmployeeNumber = null);

Task<Model_WorkstationConfig> DetectWorkstationTypeAsync(
    string? computerName = null);

Task<List<string>> GetActiveDepartmentsAsync();

Task LogUserActivityAsync(
    string eventType, 
    string username, 
    string workstationName, 
    string details);
```

### IService_SessionManager

```csharp
Model_UserSession CreateSession(
    Model_User user, 
    Model_WorkstationConfig workstationConfig, 
    string authenticationMethod);

void UpdateLastActivity();

void StartTimeoutMonitoring();

void StopTimeoutMonitoring();

bool IsSessionTimedOut();

Task EndSessionAsync(string reason);

event EventHandler<SessionTimedOutEventArgs>? SessionTimedOut;
```

---

## Testing

### Unit Tests

```csharp
[Fact]
public async Task AuthenticateByWindowsUsername_ValidUser_ReturnsSuccess()
{
    // Arrange
    var mockDao = new Mock<Dao_User>();
    mockDao.Setup(d => d.GetUserByWindowsUsernameAsync("jsmith"))
           .ReturnsAsync(Model_Dao_Result<Model_User>.Success(
               new Model_User { 
                   EmployeeNumber = 6229, 
                   FullName = "John Smith",
                   IsActive = true 
               }, 
               "User found", 
               1));
    
    var authService = new Service_Authentication(mockDao.Object, null);
    
    // Act
    var result = await authService.AuthenticateByWindowsUsernameAsync("jsmith");
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.User);
    Assert.Equal("John Smith", result.User.FullName);
}

[Fact]
public async Task AuthenticateByPin_InvalidCredentials_ReturnsError()
{
    // Arrange
    var mockDao = new Mock<Dao_User>();
    mockDao.Setup(d => d.ValidateUserPinAsync("jsmith", "9999"))
           .ReturnsAsync(Model_Dao_Result<Model_User>.Failure("Invalid PIN"));
    
    var authService = new Service_Authentication(mockDao.Object, null);
    
    // Act
    var result = await authService.AuthenticateByPinAsync("jsmith", "9999");
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Invalid", result.ErrorMessage);
}
```

### Integration Tests

Test against real database (test environment):
```csharp
[Fact]
public async Task FullAuthenticationFlow_PersonalWorkstation_Success()
{
    // Test complete flow: detect workstation → authenticate → create session
}

[Fact]
public async Task FullAuthenticationFlow_SharedTerminal_Success()
{
    // Test complete flow: detect workstation → show dialog → authenticate → create session
}
```

---

## Extending the System

### Adding New Authentication Method

1. Add method to `IService_Authentication`
2. Implement authentication logic
3. Create corresponding stored procedure
4. Add to startup flow in `Service_OnStartup_AppLifecycle`
5. Add tests
6. Update documentation

### Adding Biometric Authentication

```csharp
public async Task<AuthenticationResult> AuthenticateByBiometricAsync(
    string biometricToken, 
    IProgress<string>? progress = null)
{
    // Implementation
}
```

---

## Change Log

### Version 1.0.0 (December 2025)
- Initial implementation
- Windows username auto-login
- PIN login for shared terminals
- New user account creation
- Session timeout management
- Activity logging

---

**Last Updated**: December 2025  
**Version**: 1.0.0

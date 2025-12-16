# Authentication Feature Documentation

## Overview

The MTM Receiving Application implements a comprehensive multi-tier authentication system that automatically detects workstation type and provides appropriate login flows.

## Authentication Flows

### 1. Personal Workstation (Windows Username Auto-Login)

**When**: Application launched on a personal workstation (office computers, supervisor stations)

**Process**:
1. Application starts and detects workstation type
2. Automatically retrieves Windows username from `Environment.UserName`
3. Queries database for matching user record
4. If found: Authenticates automatically, loads user session, displays main window
5. If not found: Shows New User Creation Dialog (see section below)

**Implementation**:
- Detection: `Service_Authentication.DetectWorkstationTypeAsync()`
- Authentication: `Service_Authentication.AuthenticateByWindowsUsernameAsync()`
- Database: `sp_GetUserByWindowsUsername`

### 2. Shared Terminal (PIN Login)

**When**: Application launched on a shared terminal (shop floor computers: SHOP2, MTMDC, etc.)

**Process**:
1. Application starts and detects shared terminal
2. Displays PIN Login Dialog
3. User enters username and 4-digit PIN
4. Validates credentials against database
5. If valid: Authenticates, loads user session, displays main window
6. If invalid: Shows error, allows retry (max 3 attempts)
7. After 3 failed attempts: Shows lockout message for 5 seconds, then closes application

**Implementation**:
- Dialog: `Views/Shared/SharedTerminalLoginDialog.xaml`
- ViewModel: `ViewModels/Shared/SharedTerminalLoginViewModel.cs`
- Authentication: `Service_Authentication.AuthenticateByPinAsync()`
- Database: `sp_ValidateUserPin`

**Security**:
- 3-attempt lockout prevents brute force attacks
- Failed attempts logged to `user_activity_log` table for audit trail
- PINs stored in plain text (4-digit numeric) - acceptable for facility access control

### 3. New User Creation

**When**: Personal workstation user's Windows username not found in database

**Process**:
1. Windows username lookup fails
2. New User Setup Dialog appears
3. User (or supervisor) fills in required information:
   - Full Name (required)
   - Windows Username (pre-filled, read-only)
   - Department (dropdown from database, required)
   - Shift (1st/2nd/3rd, required)
   - 4-digit PIN (required, must be unique)
   - Confirm PIN (required, must match)
   - Visual/Infor ERP credentials (optional)
4. Click "Create Account" button
5. System validates:
   - All required fields completed
   - PIN is exactly 4 numeric digits
   - PIN matches confirmation
   - PIN is unique in database
6. Account created in database
7. Employee number assigned
8. Success message displayed with new employee number
9. Automatic authentication with new account
10. Main window loads

**Implementation**:
- Dialog: `Views/Shared/NewUserSetupDialog.xaml`
- ViewModel: `ViewModels/Shared/NewUserSetupViewModel.cs`
- Service: `Service_Authentication.CreateNewUserAsync()`
- Database: `sp_CreateNewUser`, `sp_GetDepartments`

**Validation**:
- Full Name: Minimum 2 characters
- PIN: Exactly 4 numeric digits, unique in system
- Department: Must select from list or enter custom
- Shift: Must select from 3 options

### 4. Optional ERP Integration

**Purpose**: Store Visual/Infor ERP credentials for future integration features

**Process**:
1. During new user account creation, user can optionally check "Configure Visual/Infor ERP Access"
2. Additional fields appear:
   - Visual/Infor Username
   - Visual/Infor Password
3. Credentials saved to database if provided
4. Used in future features (currently no ERP integration implemented)

**Security Note**:
- ERP passwords stored in **plain text** in database
- Required for ERP API integration (ERP system requires plaintext credentials)
- PasswordBox with "Peek" mode - never displayed unmasked by default
- Warning message displayed in UI about plaintext storage

## Session Management

### Session Creation

After successful authentication (any method), system creates a session:

```csharp
Model_UserSession session = new Model_UserSession
{
    User = authenticatedUser,
    WorkstationName = Environment.MachineName,
    WorkstationType = "personal_workstation" or "shared_terminal",
    AuthenticationMethod = "windows_auto" or "pin_login",
    LoginTimestamp = DateTime.Now,
    TimeoutDuration = 30 minutes (personal) or 15 minutes (shared)
};
```

### Session Timeout

**Timeout Durations**:
- Personal Workstation: 30 minutes of inactivity
- Shared Terminal: 15 minutes of inactivity

**Activity Tracking**:
Application monitors user activity:
- Mouse movement (`PointerMoved` event)
- Keyboard input (`KeyDown` event)
- Window activation (`Activated` event)

Any activity resets the inactivity timer.

**Timeout Process**:
1. Timer checks every 60 seconds if session has timed out
2. If timeout duration exceeded:
   - `SessionTimedOut` event fires
   - Application closes automatically
   - Timeout logged to `user_activity_log` table
3. User must re-authenticate to access application

**Implementation**:
- Session Model: `Models/Model_UserSession.cs`
- Session Manager: `Services/Authentication/Service_SessionManager.cs`
- Activity Tracking: `MainWindow.xaml.cs`

## Database Tables

### users
Stores employee authentication and profile data:
- `employee_number` (INT, AUTO_INCREMENT, PRIMARY KEY)
- `windows_username` (VARCHAR(50), UNIQUE, NOT NULL)
- `full_name` (VARCHAR(100), NOT NULL)
- `pin` (VARCHAR(4), UNIQUE, NOT NULL)
- `department` (VARCHAR(50), NOT NULL)
- `shift` (ENUM: '1st Shift', '2nd Shift', '3rd Shift')
- `is_active` (BOOLEAN, DEFAULT TRUE)
- `visual_username` (VARCHAR(50), NULL) - ERP username
- `visual_password` (VARCHAR(100), NULL) - ERP password (plaintext)
- `created_date` (DATETIME, DEFAULT CURRENT_TIMESTAMP)
- `created_by` (VARCHAR(50), NULL)
- `modified_date` (DATETIME, AUTO_UPDATE)

### workstation_config
Defines workstation authentication behavior:
- `computer_name` (VARCHAR(50), PRIMARY KEY) - from `Environment.MachineName`
- `workstation_type` (ENUM: 'personal_workstation', 'shared_terminal')
- `description` (VARCHAR(255), NULL)
- `timeout_minutes` (INT) - overrides default if needed

### departments
Department options for user assignment:
- `department_id` (INT, AUTO_INCREMENT, PRIMARY KEY)
- `department_name` (VARCHAR(50), UNIQUE, NOT NULL)
- `is_active` (BOOLEAN, DEFAULT TRUE)
- `sort_order` (INT, DEFAULT 999)

### user_activity_log
Audit trail for authentication events:
- `log_id` (INT, AUTO_INCREMENT, PRIMARY KEY)
- `event_type` (VARCHAR(50)) - 'login_success', 'login_failed', 'session_timeout', 'user_created'
- `username` (VARCHAR(50))
- `workstation_name` (VARCHAR(50))
- `event_details` (TEXT)
- `event_timestamp` (DATETIME, DEFAULT CURRENT_TIMESTAMP)

## Configuration

### Adding Shared Terminals

To configure a computer as a shared terminal:

```sql
INSERT INTO workstation_config (computer_name, workstation_type, description)
VALUES ('SHOP2', 'shared_terminal', 'Shop floor terminal - Receiving area');
```

### Adding Departments

To add a new department option:

```sql
INSERT INTO departments (department_name, is_active, sort_order)
VALUES ('Quality Control', TRUE, 50);
```

### Viewing Activity Logs

To query authentication activity:

```sql
-- View recent login activity
SELECT 
    event_timestamp,
    event_type,
    username,
    workstation_name,
    event_details
FROM user_activity_log
WHERE event_type IN ('login_success', 'login_failed')
ORDER BY event_timestamp DESC
LIMIT 100;

-- View session timeouts
SELECT 
    event_timestamp,
    username,
    workstation_name,
    event_details
FROM user_activity_log
WHERE event_type = 'session_timeout'
ORDER BY event_timestamp DESC;
```

## Troubleshooting

### User Cannot Log In (Windows Auth)

**Symptoms**: Windows username authentication fails on personal workstation

**Diagnosis**:
1. Check if user exists in database:
   ```sql
   SELECT * FROM users WHERE windows_username = 'DOMAIN\\username';
   ```
2. Check if workstation is configured correctly:
   ```sql
   SELECT * FROM workstation_config WHERE computer_name = 'COMPUTER_NAME';
   ```

**Solutions**:
- If user not found: Launch app, it will show New User Creation Dialog
- If workstation configured as shared terminal: Change to 'personal_workstation' in database

### PIN Login Not Working (Shared Terminal)

**Symptoms**: Valid PIN not accepted on shared terminal

**Diagnosis**:
1. Verify PIN in database:
   ```sql
   SELECT employee_number, full_name, pin FROM users WHERE pin = '1234';
   ```
2. Check if account is active:
   ```sql
   SELECT is_active FROM users WHERE pin = '1234';
   ```

**Solutions**:
- If PIN wrong: Use New User Creation on personal workstation to see assigned employee number
- If account inactive: Update database: `UPDATE users SET is_active = TRUE WHERE employee_number = XXXX;`

### Session Timing Out Too Quickly

**Symptoms**: Application closes unexpectedly during use

**Diagnosis**:
- Check timeout duration in code:
  - Personal workstation: 30 minutes
  - Shared terminal: 15 minutes
- Verify activity tracking is working (mouse/keyboard events)

**Solutions**:
- Activity tracking should reset timer on any interaction
- If timing out during active use, check for input event handling issues
- Check `user_activity_log` for 'session_timeout' events to confirm

### Database Connection Errors

**Symptoms**: Authentication fails with database error messages

**Diagnosis**:
1. Check database connection string in configuration
2. Verify MySQL service is running
3. Check network connectivity to database server
4. Verify stored procedures exist

**Solutions**:
- Ensure MySQL service running: `services.msc` → MySQL80
- Test connection with MySQL Workbench
- Verify connection string has correct server, port, username, password
- Run database schema scripts if stored procedures missing

## Security Considerations

### PIN Security
- **Storage**: PINs stored in plain text (4-digit numeric)
- **Rationale**: Physical facility access control, not high-security data
- **Protection**: 3-attempt lockout prevents brute force attacks
- **Audit**: All failed attempts logged for security monitoring

### ERP Credentials
- **Storage**: Plain text in database
- **Rationale**: ERP API requires plaintext credentials for authentication
- **Protection**: Database access controls, secure database server
- **Disclosure**: User warned during account creation about plaintext storage

### SQL Injection Prevention
- **Method**: All database access through stored procedures with parameterized queries
- **Validation**: Input validation in UI and service layers
- **No inline SQL**: No dynamic SQL construction in application code

### Session Security
- **Timeout**: Automatic logout after inactivity
- **Activity Tracking**: Comprehensive user interaction monitoring
- **Audit Trail**: All authentication events logged with timestamps

## Future Enhancements

### Planned Features
1. **ERP Integration**: Use stored Visual/Infor credentials for ERP API calls
2. **Password Reset**: Self-service PIN reset via supervisor approval
3. **Biometric Authentication**: Fingerprint/badge scanner integration
4. **Multi-Factor Authentication**: Email/SMS verification for sensitive operations
5. **Role-Based Access**: Different permission levels per department
6. **Session Handoff**: Transfer session between workstations

### Technical Debt
1. **Test Coverage**: Need xUnit test project (blocked by WinUI 3 architecture)
2. **Splash Screen**: Currently deferred, using MainWindow for startup messages
3. **Error Retry UI**: Database connection errors need retry dialog
4. **Performance Monitoring**: Add telemetry for authentication performance

## API Reference

### IService_Authentication

Main authentication service interface.

**Methods**:

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

Session management service interface.

**Methods**:

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
```

**Events**:

```csharp
event EventHandler<SessionTimedOutEventArgs>? SessionTimedOut;
```

## Change Log

### Version 1.0.0 (December 2025)
- Initial implementation
- Windows username auto-login for personal workstations
- PIN login for shared terminals
- New user account creation
- Session timeout management
- Optional ERP credential storage
- Activity logging and audit trail

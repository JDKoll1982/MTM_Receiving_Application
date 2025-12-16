# Service Contract: IService_Authentication

**Purpose**: Provides authentication and user validation services.

**Implementation**: `Service_Authentication.cs`

## Interface Definition

```csharp
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for user authentication and validation operations.
    /// </summary>
    public interface IService_Authentication
    {
        /// <summary>
        /// Authenticates a user by Windows username (automatic login for personal workstations).
        /// </summary>
        /// <param name="windowsUsername">Windows username from Environment.UserName</param>
        /// <param name="progress">Progress reporter for splash screen updates</param>
        /// <returns>Result containing authenticated user or error</returns>
        Task<Model_Dao_Result<Model_User>> AuthenticateByWindowsUsernameAsync(
            string windowsUsername,
            IProgress<(int percentage, string message)>? progress = null);
        
        /// <summary>
        /// Authenticates a user by username and PIN (shared terminal login).
        /// </summary>
        /// <param name="username">User's username (may be Windows username or employee username)</param>
        /// <param name="pin">4-digit numeric PIN</param>
        /// <param name="progress">Progress reporter for splash screen updates</param>
        /// <returns>Result containing authenticated user or error</returns>
        Task<Model_Dao_Result<Model_User>> AuthenticateByPinAsync(
            string username,
            string pin,
            IProgress<(int percentage, string message)>? progress = null);
        
        /// <summary>
        /// Creates a new user account (called from New User Setup dialog).
        /// </summary>
        /// <param name="fullName">Employee full name</param>
        /// <param name="windowsUsername">Windows username (auto-filled)</param>
        /// <param name="department">Department name</param>
        /// <param name="shift">Shift designation ("1st Shift", "2nd Shift", "3rd Shift")</param>
        /// <param name="pin">4-digit numeric PIN</param>
        /// <param name="createdBy">Windows username of account creator</param>
        /// <param name="progress">Progress reporter for splash screen updates</param>
        /// <returns>Result containing new employee number or error</returns>
        Task<Model_Dao_Result<int>> CreateNewUserAsync(
            string fullName,
            string windowsUsername,
            string department,
            string shift,
            string pin,
            string createdBy,
            IProgress<(int percentage, string message)>? progress = null);
        
        /// <summary>
        /// Validates a PIN format and uniqueness (called during new user creation).
        /// </summary>
        /// <param name="pin">4-digit numeric PIN to validate</param>
        /// <param name="excludeEmployeeNumber">Optional employee number to exclude from uniqueness check (for editing)</param>
        /// <returns>Result indicating if PIN is valid and unique</returns>
        Task<Model_Dao_Result<bool>> ValidatePinAsync(string pin, int? excludeEmployeeNumber = null);
        
        /// <summary>
        /// Detects the current workstation type (personal or shared terminal).
        /// </summary>
        /// <param name="progress">Progress reporter for splash screen updates</param>
        /// <returns>Result containing workstation configuration</returns>
        Task<Model_Dao_Result<Model_WorkstationConfig>> DetectWorkstationTypeAsync(
            IProgress<(int percentage, string message)>? progress = null);
        
        /// <summary>
        /// Retrieves list of active departments for dropdown population.
        /// </summary>
        /// <returns>Result containing list of department names</returns>
        Task<Model_Dao_Result<List<string>>> GetActiveDepartmentsAsync();
        
        /// <summary>
        /// Logs a user activity event for audit trail.
        /// </summary>
        /// <param name="eventType">Event type: login_success, login_failed, session_timeout, user_created</param>
        /// <param name="username">Username involved in event</param>
        /// <param name="workstationName">Computer name where event occurred</param>
        /// <param name="details">Additional event details</param>
        /// <returns>Result indicating success or failure of logging</returns>
        Task<Model_Dao_Result<bool>> LogUserActivityAsync(
            string eventType,
            string username,
            string workstationName,
            string details);
    }
}
```

## Method Specifications

### AuthenticateByWindowsUsernameAsync

**When Called**: Startup step 45% on personal workstations

**Progress Updates**:
- 45%: "Identifying user..."
- 46%: "Authenticating [WindowsUsername]..."
- 50%: "Loading employee profile..." (on success)

**Success Path**:
1. Calls `Dao_User.GetUserByWindowsUsernameAsync(windowsUsername)`
2. Checks `is_active = TRUE`
3. Logs success via `LogUserActivityAsync`
4. Reports progress to 50%
5. Returns user data

**Error Paths**:
- User not found → Returns error "User not found in database"
- User inactive → Returns error "User account is inactive. Contact administrator."
- Database connection error → Returns error with connection details
- Exception → Logs and returns generic error

**Performance**: < 800ms database query time

---

### AuthenticateByPinAsync

**When Called**: After user enters credentials in Shared Terminal Login Dialog

**Progress Updates**:
- 45%: "Authenticating [Username]..."
- 50%: "Loading employee profile..." (on success)

**Success Path**:
1. Validates PIN format (4 numeric digits)
2. Calls `Dao_User.ValidateUserPinAsync(username, pin)`
3. Checks `is_active = TRUE`
4. Logs success via `LogUserActivityAsync`
5. Reports progress to 50%
6. Returns user data

**Error Paths**:
- Invalid PIN format → Returns error "PIN must be exactly 4 numeric digits."
- Incorrect credentials → Returns error "Invalid username or PIN.", logs failed attempt
- User inactive → Returns error "User account is inactive. Contact administrator."
- Database error → Returns error with details

**Performance**: < 800ms database query time

---

### CreateNewUserAsync

**When Called**: User clicks "Create Account" in New User Setup Dialog

**Progress Updates**:
- 47%: "Creating employee account..."
- 48%: "Validating data..."
- 49%: "Saving to database..."
- 50%: "Employee account created. Welcome, [FullName]!"

**Validation Steps**:
1. Validate full_name (required, 2-100 chars, trim whitespace)
2. Validate department (required, not empty)
3. Validate shift (must be "1st Shift", "2nd Shift", or "3rd Shift")
4. Validate PIN via `ValidatePinAsync` (format and uniqueness)
5. Check windows_username uniqueness

**Success Path**:
1. All validation passes
2. Calls `Dao_User.CreateNewUserAsync(user, createdBy)`
3. Logs user_created event via `LogUserActivityAsync`
4. Reports progress to 50%
5. Returns new employee_number

**Error Paths**:
- Validation failure → Returns error with specific field message
- PIN already in use → Returns error "This PIN is already in use. Please choose a different PIN."
- Username already exists → Returns error "Windows username already exists in database."
- Database error → Returns error with details

**Performance**: < 2 seconds total (including validation)

---

### ValidatePinAsync

**When Called**: During new user creation form validation (on PIN field change)

**Validation Rules**:
1. Not null or empty
2. Exactly 4 characters
3. All characters are digits (0-9)
4. Unique across all users (database query)

**Success**: Returns `Model_Dao_Result<bool>` with Success = true

**Error**: Returns result with ErrorMessage describing validation failure

**Performance**: < 200ms (local validation + database uniqueness check)

---

### DetectWorkstationTypeAsync

**When Called**: Startup step 40%

**Progress Updates**:
- 40%: "Detecting workstation type..."
- 41%: "Workstation detected: [ComputerName]"

**Detection Logic**:
1. Read `Environment.MachineName`
2. Query `workstation_config` table via `Dao_User.GetSharedTerminalNamesAsync()`
3. Check if computer name exists in result list
4. If found → return configuration with workstation_type
5. If not found → default to "personal_workstation"

**Success Path**: Returns `Model_WorkstationConfig` with ComputerName and WorkstationType

**Error Path**: Database connection error → Returns error, application cannot proceed

**Performance**: < 500ms (includes database query, results cached in memory)

---

### GetActiveDepartmentsAsync

**When Called**: When opening New User Setup Dialog (populate dropdown)

**Query**: Calls `Dao_User.GetActiveDepartmentsAsync()` → `sp_GetDepartments` WHERE is_active = TRUE ORDER BY sort_order

**Success Path**: Returns list of department names (e.g., ["Receiving", "Shipping", "Production", ...])

**Error Path**: Database error → Returns error, dialog shows fallback text field only

**Performance**: < 500ms (results cached in memory, rarely changes)

---

### LogUserActivityAsync

**When Called**: After every authentication event (success, failure, creation, timeout)

**Event Types**:
- `login_success` - Successful Windows or PIN authentication
- `login_failed` - Invalid credentials, inactive account
- `session_timeout` - Inactivity timeout triggered
- `user_created` - New account created via dialog

**Success Path**: Inserts row into `user_activity_log` table via `sp_LogUserActivity`

**Error Path**: Logs error but does not block application flow (fire-and-forget pattern)

**Performance**: < 100ms (async, non-blocking)

## Error Handling

All methods return `Model_Dao_Result<T>` with:
- `Success` (bool): Operation success indicator
- `Data` (T): Result data on success
- `ErrorMessage` (string): User-friendly error message
- `ErrorDetails` (string): Technical details for logging

**Exception Handling Pattern**:
```csharp
try
{
    // Operation logic
    return Model_Dao_Result<T>.SuccessResult(data);
}
catch (MySqlException ex)
{
    await _loggingService.LogErrorAsync("Database error", ex);
    return Model_Dao_Result<T>.ErrorResult("Unable to connect to database. Please try again.");
}
catch (Exception ex)
{
    await _loggingService.LogErrorAsync("Unexpected error", ex);
    return Model_Dao_Result<T>.ErrorResult("An unexpected error occurred. Please contact IT support.");
}
```

## Testing Requirements

### Unit Tests
- PIN validation logic (format, uniqueness)
- Progress reporting calls
- Error message generation
- Null/empty input handling

### Integration Tests
- Windows authentication flow with test database
- PIN authentication with valid/invalid credentials
- New user creation with validation
- Workstation type detection with test data
- Activity logging persistence

### Mock Dependencies
- `Dao_User` for database operations
- `ILoggingService` for error logging
- `IProgress<T>` for splash screen updates

## Dependencies

- `Dao_User` (Data Access Layer)
- `ILoggingService` (Phase 1 Infrastructure)
- `Model_User`, `Model_WorkstationConfig`, `Model_Dao_Result<T>` (Models)

## Contract Completion

**Status**: ✅ Complete and ready for implementation

**Review Notes**:
- All methods have clear purpose and specifications
- Error handling patterns defined
- Performance targets specified
- Testing requirements documented

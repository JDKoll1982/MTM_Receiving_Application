# Data Model: User Authentication & Login System

**Feature**: User Authentication & Login System  
**Date**: December 15, 2025  
**Status**: Phase 1 Design

## Entity Relationship Diagram

```
┌─────────────────────────┐
│   departments           │
├─────────────────────────┤
│ PK department_id (INT)  │
│    department_name (UK) │
│    is_active            │
│    sort_order           │
│    created_date         │
└─────────────────────────┘
           │
           │ Referenced by
           │
           ▼
┌──────────────────────────┐         ┌─────────────────────────┐
│   users                  │◄────────│  user_activity_log      │
├──────────────────────────┤         ├─────────────────────────┤
│ PK employee_number (INT) │         │ PK log_id (INT)         │
│ UK windows_username      │         │ FK username             │
│ UK pin                   │         │    event_type           │
│    full_name             │         │    workstation_name     │
│    department   ─────────┼───┐     │    event_timestamp      │
│    shift                 │   │     │    details              │
│    is_active             │   │     └─────────────────────────┘
│    visual_username       │   │
│    visual_password       │   │
│    created_date          │   │
│    created_by            │   │
│    modified_date         │   │
└──────────────────────────┘   │
                               │
                               └──► (FK reference to departments.department_name)

┌─────────────────────────────┐
│   workstation_config        │
├─────────────────────────────┤
│ PK config_id (INT)          │
│ UK workstation_name         │
│    workstation_type (ENUM)  │
│    is_active                │
│    description              │
│    created_date             │
│    modified_date            │
└─────────────────────────────┘
```

## Database Tables

### Table: `users`

**Purpose**: Stores employee information for authentication and session management.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| employee_number | INT | PRIMARY KEY, AUTO_INCREMENT | Unique employee identifier used for label tracking |
| windows_username | VARCHAR(50) | UNIQUE, NOT NULL, INDEX | Windows login username (DOMAIN\username format) |
| full_name | VARCHAR(100) | NOT NULL | Employee full name for display |
| pin | VARCHAR(4) | UNIQUE, NOT NULL, INDEX | 4-digit numeric PIN for shared terminal login (plain text) |
| department | VARCHAR(50) | NOT NULL | Department assignment (references departments.department_name) |
| shift | ENUM | NOT NULL | Shift assignment: '1st Shift', '2nd Shift', '3rd Shift' |
| is_active | BOOLEAN | NOT NULL, DEFAULT TRUE, INDEX | Account active status |
| visual_username | VARCHAR(50) | NULL | Optional Visual/Infor ERP username (plain text) |
| visual_password | VARCHAR(100) | NULL | Optional Visual/Infor ERP password (plain text, masked in UI) |
| created_date | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Account creation timestamp |
| created_by | VARCHAR(50) | NULL | Windows username of account creator |
| modified_date | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP ON UPDATE | Last modification timestamp |

**Indexes**:
- PRIMARY KEY (`employee_number`)
- UNIQUE KEY (`windows_username`)
- UNIQUE KEY (`pin`)
- INDEX (`is_active`)

**Sample Data**:
```sql
INSERT INTO users VALUES
(6229, 'JSMITH', 'John Smith', '1234', 'Receiving', '1st Shift', TRUE, NULL, NULL, NOW(), 'ADMIN', NOW()),
(6230, 'MJONES', 'Mary Jones', '5678', 'Shipping', '2nd Shift', TRUE, NULL, NULL, NOW(), 'ADMIN', NOW()),
(6231, 'RBROWN', 'Robert Brown', '9012', 'Production', '1st Shift', TRUE, 'rbrown_erp', 'erp_pass123', NOW(), 'ADMIN', NOW());
```

### Table: `workstation_config`

**Purpose**: Stores workstation computer names and types for authentication flow determination.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| config_id | INT | PRIMARY KEY, AUTO_INCREMENT | Unique configuration ID |
| workstation_name | VARCHAR(50) | UNIQUE, NOT NULL | Windows computer name (e.g., SHOP2, MTMDC, SHOP-FLOOR-01) |
| workstation_type | ENUM | NOT NULL | Type: 'shared_terminal' or 'personal_workstation' |
| is_active | BOOLEAN | NOT NULL, DEFAULT TRUE | Configuration active status |
| description | VARCHAR(200) | NULL | Optional notes about workstation location/purpose |
| created_date | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Record creation timestamp |
| modified_date | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP ON UPDATE | Last modification timestamp |

**Indexes**:
- PRIMARY KEY (`config_id`)
- UNIQUE KEY (`workstation_name`)

**Sample Data**:
```sql
INSERT INTO workstation_config VALUES
(1, 'SHOP2', 'shared_terminal', TRUE, 'Shop floor terminal 2 - Receiving area', NOW(), NOW()),
(2, 'MTMDC', 'shared_terminal', TRUE, 'Main shop floor data collection terminal', NOW(), NOW()),
(3, 'SHOP-FLOOR-01', 'shared_terminal', TRUE, 'Production floor terminal 1', NOW(), NOW()),
(4, 'OFFICE-PC-025', 'personal_workstation', TRUE, 'Supervisor office workstation', NOW(), NOW());
```

### Table: `departments`

**Purpose**: Stores available departments for user assignment dropdown.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| department_id | INT | PRIMARY KEY, AUTO_INCREMENT | Unique department ID |
| department_name | VARCHAR(50) | UNIQUE, NOT NULL | Department display name (e.g., "Receiving", "Shipping") |
| is_active | BOOLEAN | NOT NULL, DEFAULT TRUE | Department active status (allows hiding without deleting) |
| sort_order | INT | NOT NULL, DEFAULT 999 | Display order in dropdown (lower numbers first) |
| created_date | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Record creation timestamp |

**Indexes**:
- PRIMARY KEY (`department_id`)
- UNIQUE KEY (`department_name`)

**Sample Data**:
```sql
INSERT INTO departments VALUES
(1, 'Receiving', TRUE, 1, NOW()),
(2, 'Shipping', TRUE, 2, NOW()),
(3, 'Production', TRUE, 3, NOW()),
(4, 'Quality Control', TRUE, 4, NOW()),
(5, 'Maintenance', TRUE, 5, NOW()),
(6, 'Administration', TRUE, 6, NOW()),
(7, 'Management', TRUE, 7, NOW());
```

### Table: `user_activity_log`

**Purpose**: Audit trail for authentication events and user actions.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| log_id | INT | PRIMARY KEY, AUTO_INCREMENT | Unique log entry ID |
| event_type | VARCHAR(50) | NOT NULL | Event type: 'login_success', 'login_failed', 'session_timeout', 'user_created', 'pin_reset' |
| username | VARCHAR(50) | NULL | Windows username or employee username involved |
| workstation_name | VARCHAR(50) | NULL | Computer name where event occurred |
| event_timestamp | DATETIME | NOT NULL, DEFAULT CURRENT_TIMESTAMP | When event occurred |
| details | TEXT | NULL | Additional context (error messages, IP addresses, etc.) |

**Indexes**:
- PRIMARY KEY (`log_id`)
- INDEX (`event_timestamp`) - for time-based queries
- INDEX (`username`) - for user activity reports
- INDEX (`event_type`) - for event type filtering

**Sample Data**:
```sql
INSERT INTO user_activity_log VALUES
(1, 'login_success', 'JSMITH', 'OFFICE-PC-025', NOW(), 'Windows authentication successful'),
(2, 'login_failed', 'UNKNOWN', 'SHOP2', NOW(), 'Invalid PIN attempt 1 of 3'),
(3, 'user_created', 'JDOE', 'OFFICE-PC-025', NOW(), 'Created by: ADMIN'),
(4, 'session_timeout', 'MJONES', 'SHOP2', NOW(), '15-minute inactivity timeout');
```

## Application Models

### Model: `Model_User`

**Purpose**: Represents a user/employee entity in the application.

```csharp
public class Model_User
{
    // Database fields
    public int EmployeeNumber { get; set; }
    public string WindowsUsername { get; set; }
    public string FullName { get; set; }
    public string Pin { get; set; }
    public string Department { get; set; }
    public string Shift { get; set; } // "1st Shift", "2nd Shift", "3rd Shift"
    public bool IsActive { get; set; }
    public string? VisualUsername { get; set; }
    public string? VisualPassword { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    
    // Computed properties
    public string DisplayName => $"{FullName} (Emp #{EmployeeNumber})";
    public bool HasErpAccess => !string.IsNullOrEmpty(VisualUsername);
}
```

### Model: `Model_UserSession`

**Purpose**: Represents the current active user session.

```csharp
public class Model_UserSession
{
    // User information
    public Model_User User { get; set; }
    
    // Session metadata
    public string WorkstationName { get; set; }
    public string WorkstationType { get; set; } // "personal_workstation" or "shared_terminal"
    public string AuthenticationMethod { get; set; } // "windows_auto" or "pin_login"
    public DateTime LoginTimestamp { get; set; }
    public DateTime LastActivityTimestamp { get; set; }
    
    // Session management
    public TimeSpan TimeoutDuration { get; set; } // 30 min or 15 min based on workstation type
    public TimeSpan TimeSinceLastActivity => DateTime.Now - LastActivityTimestamp;
    public bool IsTimedOut => TimeSinceLastActivity >= TimeoutDuration;
    
    // Methods
    public void UpdateLastActivity()
    {
        LastActivityTimestamp = DateTime.Now;
    }
    
    public void LogActivity(string eventType, string details)
    {
        // Calls sp_LogUserActivity via DAO
    }
}
```

### Model: `Model_WorkstationConfig`

**Purpose**: Workstation detection and configuration logic.

```csharp
public class Model_WorkstationConfig
{
    // Workstation information
    public string ComputerName { get; set; }
    public string WorkstationType { get; set; } // "personal_workstation" or "shared_terminal"
    public string Description { get; set; }
    
    // Detection results
    public bool IsSharedTerminal => WorkstationType == "shared_terminal";
    public bool IsPersonalWorkstation => WorkstationType == "personal_workstation";
    
    // Static detection method
    public static async Task<Model_WorkstationConfig> DetectCurrentWorkstation()
    {
        string computerName = Environment.MachineName;
        // Query workstation_config table via DAO
        // Return configuration or default to personal_workstation if not found
    }
}
```

### Model: `Model_Dao_Result<T>`

**Purpose**: Standardized database operation result (already exists in Phase 1).

```csharp
public class Model_Dao_Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorDetails { get; set; }
    
    public static Model_Dao_Result<T> SuccessResult(T data) => new() { Success = true, Data = data };
    public static Model_Dao_Result<T> ErrorResult(string message) => new() { Success = false, ErrorMessage = message };
}
```

## Data Validation Rules

### User Creation Validation

| Field | Rules | Error Message |
|-------|-------|---------------|
| FullName | Required, 2-100 characters, no leading/trailing whitespace | "Full Name is required and must be 2-100 characters." |
| WindowsUsername | Required, auto-filled, cannot be changed | (N/A - auto-filled) |
| Department | Required, must be from dropdown or "Other" text field | "Department is required." |
| Shift | Required, must be "1st Shift", "2nd Shift", or "3rd Shift" | "Shift is required." |
| Pin | Required, exactly 4 numeric digits, unique across all users | "PIN must be exactly 4 numeric digits." / "This PIN is already in use." |
| Confirm PIN | Must match Pin field exactly | "PIN and Confirm PIN do not match." |

### PIN Validation

```csharp
public static bool IsValidPin(string pin)
{
    if (string.IsNullOrEmpty(pin)) return false;
    if (pin.Length != 4) return false;
    if (!pin.All(char.IsDigit)) return false;
    return true;
}

public static async Task<bool> IsPinUnique(string pin, int? excludeEmployeeNumber = null)
{
    // Query database to check if PIN exists
    // Exclude current user if editing (excludeEmployeeNumber parameter)
}
```

### Login Validation

```csharp
public static async Task<Model_Dao_Result<Model_User>> ValidateLogin(string username, string pin)
{
    // 1. Check username exists
    // 2. Check PIN matches
    // 3. Check is_active = true
    // 4. Return user data or error
}
```

## State Transitions

### User Account Lifecycle

```
[New Employee] 
    ↓ (Windows username detected, not in database)
[New User Creation Dialog Shown]
    ↓ (Supervisor fills form, clicks Create Account)
[sp_CreateNewUser called]
    ↓ (Validation passes)
[Active User Account Created] (is_active = TRUE)
    ↓ (Can authenticate via Windows or PIN)
[Authenticated Session]
    ↓ (Inactivity timeout or manual close)
[Session Ended, Logged]
    ↓ (User can re-authenticate)
[Authenticated Session] (repeats)

Alternative paths:
- [Active User] → (Administrator deactivates) → [Inactive User] (is_active = FALSE)
- [Inactive User] → (Login attempt) → [Authentication Denied]
```

### Authentication Flow States

```
[Application Launch]
    ↓
[Step 40%: Detect Workstation Type]
    ├──→ [Personal Workstation Detected]
    │       ↓
    │   [Step 45%: Query sp_GetUserByWindowsUsername]
    │       ├──→ [User Found] → [Step 50%: Load Profile] → [Authenticated]
    │       └──→ [User Not Found] → [New User Creation Dialog] 
    │                ├──→ [Cancel] → [Application Closes]
    │                └──→ [Create Account] → [Step 50%: Load Profile] → [Authenticated]
    │
    └──→ [Shared Terminal Detected]
            ↓
        [Step 45%: Show PIN Login Dialog]
            ├──→ [Valid Credentials] → [Step 50%: Load Profile] → [Authenticated]
            ├──→ [Invalid Credentials] → [Retry (max 3 attempts)]
            │       ├──→ [Attempt < 3] → [Show Error, Clear PIN, Retry]
            │       └──→ [Attempt = 3] → [Show Lockout Error (5 sec)] → [Application Closes]
            └──→ [Cancel] → [Application Closes]
```

## Data Access Layer (DAO) Methods

### Dao_User.cs

```csharp
public class Dao_User
{
    // Authentication
    Task<Model_Dao_Result<Model_User>> GetUserByWindowsUsernameAsync(string windowsUsername);
    Task<Model_Dao_Result<Model_User>> ValidateUserPinAsync(string username, string pin);
    
    // CRUD Operations
    Task<Model_Dao_Result<int>> CreateNewUserAsync(Model_User user, string createdBy);
    Task<Model_Dao_Result<Model_User>> GetUserByEmployeeNumberAsync(int employeeNumber);
    Task<Model_Dao_Result<bool>> UpdateUserAsync(Model_User user);
    Task<Model_Dao_Result<bool>> DeactivateUserAsync(int employeeNumber);
    
    // Validation
    Task<Model_Dao_Result<bool>> IsPinUniqueAsync(string pin, int? excludeEmployeeNumber = null);
    Task<Model_Dao_Result<bool>> IsWindowsUsernameUniqueAsync(string username, int? excludeEmployeeNumber = null);
    
    // Activity Logging
    Task<Model_Dao_Result<bool>> LogUserActivityAsync(string eventType, string username, string workstationName, string details);
    
    // Configuration
    Task<Model_Dao_Result<List<string>>> GetSharedTerminalNamesAsync();
    Task<Model_Dao_Result<List<string>>> GetActiveDepartmentsAsync();
}
```

## Performance Considerations

### Database Indexes
- **Primary keys**: Clustered indexes for optimal row lookup
- **Unique keys** (windows_username, pin): Fast authentication queries
- **is_active index**: Fast filtering of active users
- **Event_timestamp index**: Fast audit log queries by date range

### Query Optimization
- Use prepared statements for all queries (prevent SQL injection)
- Limit result sets (no SELECT * in production)
- Connection pooling for database connections
- Async operations to prevent UI blocking

### Caching Strategy
- Cache `workstation_config` list in memory during startup (small dataset, rarely changes)
- Cache `departments` list in memory (small dataset, rarely changes)
- No caching of user data (security, always get fresh data from database)
- Session data stored in memory only (`Model_UserSession` instance)

## Data Migration

**Note**: This is a new feature on a greenfield database. No migration from existing data required.

**Initial Setup**:
1. Run schema script: `02_create_authentication_tables.sql`
2. Run stored procedure scripts in `Database/StoredProcedures/Authentication/`
3. Insert initial department data
4. Insert initial workstation configuration data
5. First users created via New User Creation Dialog during application usage

## Data Model Completion Checklist

- [x] All entities identified with clear purpose
- [x] Database tables defined with complete schema
- [x] Relationships and foreign keys documented
- [x] Application models (C# classes) defined
- [x] Validation rules specified
- [x] State transitions documented
- [x] DAO interface methods listed
- [x] Performance considerations addressed
- [x] Sample data provided for testing

**Status**: ✅ Ready for Phase 1 Contract Generation

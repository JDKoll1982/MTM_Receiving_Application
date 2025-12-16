# MTM Receiving Application - Startup Splash Screen System

**Document Type**: Technical Process Documentation  
**Audience**: Developers, System Architects, Technical Project Managers  
**Application**: MTM Receiving Label Application (WinUI 3 Desktop)  
**Framework**: .NET 8.0, Windows App SDK, WinUI 3  
**Last Updated**: 2025-12-15

---

## Overview

A splash screen is a temporary visual indicator displayed during application startup while critical initialization tasks complete in the background. This document describes a robust, production-ready splash screen implementation pattern that ensures:

1. **User Experience**: Users see immediate feedback instead of a frozen/unresponsive window
2. **Reliability**: All critical services initialize before the main application UI appears
3. **Error Handling**: Startup failures are caught and reported gracefully
4. **Progress Transparency**: Users see what's happening during initialization
5. **Performance**: Services initialize asynchronously without blocking the UI thread

---

## System Architecture

### Component Hierarchy

```
Application Entry Point (Main)
    ↓
Startup Orchestrator (ApplicationContext)
    ↓
├── Splash Screen (Visual Feedback)
│   └── Progress Indicator (Text + Percentage)
├── Initialization Services (Background Tasks)
│   ├── Logging Service
│   ├── Database Connectivity
│   ├── User Authentication
│   ├── Configuration Loading
│   └── Theme/UI Initialization
└── Main Application Window (After Initialization)
```

### Three-Phase Startup Model

#### Phase 1: Environment Setup (Synchronous)
**When**: Before any UI is shown  
**Duration**: < 100ms  
**Purpose**: Minimal setup to enable logging and error handling

**Activities**:
- Initialize basic framework settings
- Configure dependency injection container
- Set up initial logging capabilities
- Prepare application data directories

**Why Synchronous**: These tasks are too critical to fail and too fast to warrant async handling.

---

#### Phase 2: Splash Screen + Async Initialization (Asynchronous)
**When**: After splash screen is visible  
**Duration**: 2-10 seconds (typical)  
**Purpose**: Initialize all services required for main application

**Activities**:
- Display splash screen immediately
- Start asynchronous initialization sequence
- Update progress indicator as tasks complete
- Handle initialization errors gracefully

**Why Asynchronous**: Allows UI to remain responsive and provides progress feedback.

---

#### Phase 3: Main Application Launch (Transition)
**When**: After all initialization completes successfully  
**Duration**: < 500ms  
**Purpose**: Transition from splash to main UI

**Activities**:
- Hide splash screen
- Display main application window
- Configure window state (theme, user preferences)
- Begin normal application operation

**Why Transition**: Clean handoff from startup to runtime state.

---

## Splash Screen Visual Design

### Purpose of Each Element

**Progress Percentage (0-100%)**:
- **Purpose**: Quantitative progress indicator
- **Update Frequency**: After each major initialization task
- **Example**: "Loading database... 45%"

**Progress Message (Text)**:
- **Purpose**: Qualitative description of current activity
- **Update Frequency**: With each progress percentage update
- **Example**: "Connecting to database server..."

**Application Branding**:
- **Purpose**: Brand recognition, professional appearance
- **Elements**: 
  - MTM logo and application name: "MTM Receiving Label Application"
  - Version number from `Model_Application_Variables.Version`
  - Company name: "MTM Manufacturing"
- **Position**: Centered or top of splash screen
- **Assets**: Logo stored in `Assets/Icons/` directory

**Theme Integration**:
- **Purpose**: Visual consistency with main application
- **Elements**: Colors, fonts matching user preferences
- **Consideration**: Some users may have light/dark theme preferences

---

## Initialization Sequence (Step-by-Step)

### Detailed Startup Flow

```
Step 0: Application Launch
    ↓
    User double-clicks application icon
    Operating system loads executable
    ↓
Step 1: Entry Point Execution (Main Method)
    ↓ Duration: ~50-100ms
    Basic framework initialization
    Dependency injection container setup
    Logging service registration
    ↓
Step 2: Splash Screen Creation
    ↓ Duration: ~100-200ms
    Create splash screen window
    Display splash (user sees feedback immediately)
    Attach event handlers for initialization
    ↓
Step 3: Background Initialization (Async)
    ↓ Duration: 2-10 seconds
    ├─ [5%] Initialize Logging System (ILoggingService)
    │   Purpose: Enable error tracking throughout startup
    │   Actions: Create log directory in AppData, configure log file, set log levels
    │   Path: %AppData%/MTM_Receiving_Application/Logs/
    │   Failure Impact: CRITICAL - Cannot diagnose further issues
    │
    ├─ [10%] Initialize Error Handler (IService_ErrorHandler)
    │   Purpose: Setup centralized error handling for startup failures
    │   Actions: Register error handler, configure dialog templates
    │   Failure Impact: CRITICAL - Cannot show errors to user
    │
    ├─ [15%] Clean Up Old Logs
    │   Purpose: Remove old log files to prevent disk space issues
    │   Actions: Delete logs older than 30 days from log directory
    │   Failure Impact: LOW - Application can continue
    │
    ├─ [20%] Load Database Configuration
    │   Purpose: Read MySQL connection settings from Model_Application_Variables
    │   Actions: Load connection string, validate database credentials
    │   Configuration: Helper_Database_Variables
    │   Failure Impact: CRITICAL - Cannot connect to database without config
    │
    ├─ [30%] Verify MySQL Database Connectivity
    │   Purpose: Ensure MySQL server is reachable and database exists
    │   Actions: Test connection, verify database version, check required tables
    │   Database: MTM_Receiving_Application database on MySQL server
    │   Retry Logic: 3 attempts with 5-second delays
    │   Failure Impact: CRITICAL - Application requires database for all operations
    │
    ├─ [40%] Detect Workstation Type
    │   Purpose: Determine if this is a personal workstation or shared terminal
    │   Actions: Read Windows computer name, compare against shared terminal list
    │   Shared Terminals: SHOP2, MTMDC, SHOP-FLOOR-* pattern
    │   Failure Impact: MEDIUM - Will default to personal workstation mode
    │
    ├─ [45%] Authenticate User (Windows Username or PIN)
    │   Purpose: Identify current user and validate access
    │   Personal Workstation Flow:
    │       ├─ Read Windows username via Environment.UserName
    │       ├─ Call sp_GetUserByWindowsUsername stored procedure
    │       ├─ If found: Load user profile (employee number, name, shift, department)
    │       └─ If not found: Display new user creation dialog (pause at 45%)
    │   Shared Terminal Flow:
    │       ├─ Display login dialog over splash screen (pause at 45%)
    │       ├─ User enters username + 4-digit PIN
    │       ├─ Call sp_ValidateUserPin stored procedure
    │       ├─ If valid: Load user profile
    │       ├─ If invalid: Show error, allow up to 3 attempts
    │       └─ After 3 failures: Close application
    │   Failure Impact: CRITICAL - Cannot track which employee creates labels
    │
    ├─ [50%] Load User Profile and Session
    │   Purpose: Retrieve full employee information and initialize session
    │   Actions: Query users table, create Model_UserSession, set current user context
    │   Data Loaded: Employee number, full name, shift, department, Visual/ERP credentials
    │   Failure Impact: CRITICAL - User profile required for all label operations
    │
    ├─ [55%] Log User Activity
    │   Purpose: Record successful login for audit trail
    │   Actions: Call sp_LogUserActivity, record timestamp and workstation name
    │   Log Table: user_activity_log
    │   Failure Impact: LOW - Audit logging failure does not prevent access
    │
    ├─ [60%] Load User Preferences (Optional)
    │   Purpose: Retrieve user-specific settings if available
    │   Actions: Query user preferences table (if exists in future)
    │   Failure Impact: LOW - Default preferences used if not available
    │
    ├─ [70%] Initialize Theme System
    │   Purpose: Apply WinUI 3 theme before main window appears
    │   Actions: Configure ElementTheme (Light/Dark/Default), load custom colors
    │   Failure Impact: LOW - Default system theme used
    │
    ├─ [75%] Verify Required Database Tables
    │   Purpose: Check that all required tables and stored procedures exist
    │   Tables: users, receiving_lines, dunnage_lines, routing_labels, user_activity_log
    │   Stored Procedures: sp_GetUserByWindowsUsername, sp_ValidateUserPin, etc.
    │   Failure Impact: HIGH - Missing tables will cause runtime errors
    │
    ├─ [80%] Preload Lookup Data (Future Enhancement)
    │   Purpose: Load frequently-used lookup tables into memory
    │   Examples: Part types, customer codes, carrier information
    │   Failure Impact: MEDIUM - Performance impact if not cached
    │
    ├─ [85%] Check Visual/Infor ERP Credentials (Optional)
    │   Purpose: Verify if user has configured ERP integration
    │   Actions: Check if visual_username and visual_password are populated
    │   Result: Enable/disable ERP-integrated menu options
    │   Failure Impact: NONE - ERP integration is optional feature
    │
    ├─ [90%] Create Main Application Window
    │   Purpose: Instantiate MainWindow with NavigationView
    │   Actions: Initialize MainWindow, setup navigation menu, configure layout
    │   Navigation Items: Receiving Labels, Dunnage Labels, Routing Labels
    │   Failure Impact: CRITICAL - Application has no UI without MainWindow
    │
    ├─ [95%] Initialize ViewModels and Services
    │   Purpose: Prepare MVVM infrastructure for main application
    │   Actions: Resolve ViewModels from DI container, initialize BaseViewModel instances
    │   ViewModels: ReceivingLabelViewModel, DunnageLabelViewModel, RoutingLabelViewModel
    │   Failure Impact: CRITICAL - Application cannot function without ViewModels
    │
    └─ [100%] Finalize Startup
        Purpose: Complete transition from splash screen to main application
        Actions: Hide splash screen, show MainWindow, log total startup time
        Display: Show logged-in user's name and employee number in header
        Failure Impact: N/A - Startup complete
    ↓
Step 4: Main Application Running
    ↓
    Splash screen hidden
    Main window visible and responsive
    User can interact with application
```

---

## Progress Reporting Pattern

### How Progress Updates Work

**Progress Reporter Interface**:
```
Interface: IProgress<T>
    Method: Report(T value)
    
Where T = (int percentage, string message)
```

**Example Progress Reports**:
```
Report((5, "Initializing logging system..."))
Report((10, "Setting up error handlers..."))
Report((20, "Loading database configuration..."))
Report((30, "Connecting to MySQL database..."))
Report((40, "Detecting workstation type..."))
Report((45, "Authenticating user..."))
Report((50, "Loading employee profile..."))
Report((60, "Loading user preferences..."))
Report((70, "Applying theme..."))
Report((90, "Creating main window..."))
Report((100, "Ready to start!"))
```

**Thread Safety**:
- Progress reports may come from background threads
- Splash screen must marshal updates to UI thread
- Use thread-safe update mechanisms (InvokeRequired pattern in WinForms, Dispatcher in WPF)

**Timing Considerations**:
- Add small delays (50-100ms) between updates for visual smoothness
- Do NOT make delays too long (user experience degrades)
- Total splash time should be 2-10 seconds maximum

---

## Error Handling During Startup

### Error Severity Levels

#### Low Severity Errors
**Examples**:
- Failed to load user preferences (use defaults)
- Failed to clean up old log files
- Theme initialization failed (use default theme)

**Handling**:
1. Log the error with details
2. Use fallback/default values
3. Continue startup sequence
4. Display warning after main window loads (optional)

---

#### Medium Severity Errors
**Examples**:
- Failed to load data caches (performance impact)
- Configuration file corrupted (some settings unavailable)
- Non-critical service unavailable

**Handling**:
1. Log the error with full stack trace
2. Use fallback mechanisms
3. Continue startup with reduced functionality
4. Display informational message to user after startup
5. Consider retry logic for transient failures

---

#### High Severity Errors
**Examples**:
- Database connection timeout (but retry may succeed)
- User authentication failed (shared workstation login)
- Critical configuration missing

**Handling**:
1. Log the error with full context
2. Display user-friendly error dialog over splash screen
3. Offer retry option if applicable
4. Provide guidance for resolution
5. If retry fails 3 times, escalate to Critical

---

#### Critical (Fatal) Errors
**Examples**:
- Database server completely unreachable
- User authentication impossible (no user found, no login prompt)
- Main window creation failed

**Handling**:
1. Log the error with maximum detail
2. Display modal error dialog with clear message
3. Provide instructions for user (e.g., "Contact IT Support")
4. Close application gracefully (do NOT crash)
5. Return error code to operating system (for automation/monitoring)

**Example Error Messages**:
- "Unable to connect to MySQL database server. Please verify the server is running and network connection is available. Contact IT if problem persists."
- "User authentication failed. Windows username not found in database. Please contact your supervisor to create your user account."
- "Shared workstation login failed after 3 attempts. Application will close for security. Contact supervisor if you need PIN reset."
- "Critical initialization error: Required database tables missing. Please contact IT support with error code: ERR_STARTUP_DB_SCHEMA"

---

## Timeout and Retry Logic

### Database Connection Timeout

**Problem**: Database server may be slow to respond or temporarily unavailable.

**Solution**:
```
Maximum Timeout: 30 seconds total
Retry Attempts: 3
Retry Delay: 5 seconds between attempts

Attempt 1: Wait up to 10 seconds
    ↓ (If fails)
Attempt 2: Wait up to 10 seconds (after 5 second delay)
    ↓ (If fails)
Attempt 3: Wait up to 10 seconds (after 5 second delay)
    ↓ (If all fail)
Display Critical Error → Close Application
```

**Progress Messages**:
- Attempt 1: "Connecting to database..."
- Attempt 2: "Retrying database connection... (Attempt 2 of 3)"
- Attempt 3: "Final connection attempt... (Attempt 3 of 3)"
- Failure: "Unable to connect to database. Application will close."

---

### Service Initialization Timeout

**Problem**: Individual services may hang or take too long.

**Solution**:
```
Per-Service Timeout: 10 seconds
Total Startup Timeout: 60 seconds (safety net)

If any service exceeds 10 seconds:
    Log warning
    Continue to next service (if non-critical)
    OR abort startup (if critical)

If total startup exceeds 60 seconds:
    Display error dialog
    Close application
```

---

## User Authentication Scenarios

### Scenario 1: Personal Workstation (Automatic Login)

**Context**: Office staff, supervisors, managers with dedicated personal computers.

**Workstation Examples**: OFFICE-PC-001, SUPERVISOR-DESK-01, ADMIN-LAPTOP-05

**Flow**:
```
1. Application reads Windows username via Environment.UserName
2. Application reads computer name via Environment.MachineName
3. Check if computer name matches shared terminal pattern (SHOP2, MTMDC, SHOP-FLOOR-*)
4. Computer name does NOT match → Personal Workstation Mode
5. Call sp_GetUserByWindowsUsername with Windows username
6. If user found in database:
    ├─ Retrieve employee_number, full_name, shift, department, is_active
    ├─ Check is_active = true
    ├─ Create Model_UserSession with user information
    ├─ Call sp_LogUserActivity (event_type: 'login_success')
    └─ Continue startup with authenticated user
7. If user NOT found in database:
    ├─ Display "New User Setup" dialog (pause splash at 45%)
    ├─ Supervisor enters: full name, 4-digit PIN, shift selection
    ├─ Call sp_CreateNewUser with Windows username and entered data
    ├─ Call sp_LogUserActivity (event_type: 'user_created')
    └─ Continue startup with newly created user
8. If user found but is_active = false:
    ├─ Display error: "User account is inactive. Contact administrator."
    └─ Close application
```

**Splash Screen Progress**:
- "Detecting workstation type..." (40%)
- "Personal workstation detected" (40%)
- "Identifying user..." (45%)
- "Loading employee profile..." (50%)
- "Logged in as: [Full Name] (Employee #[number])" (55%)

**Time**: ~500-800ms (database query + profile load)

**Database Calls**:
- `sp_GetUserByWindowsUsername(@windows_username)` - Returns user profile
- `sp_CreateNewUser(...)` - Only if new user (supervisor-initiated)
- `sp_LogUserActivity(...)` - Records successful login

---

### Scenario 2: Shared Workstation (Login Required)

**Context**: Production workers, material handlers on shared shop floor terminals.

**Workstation Identifiers**: SHOP2, MTMDC, SHOP-FLOOR-01, SHOP-FLOOR-02, etc.

**Flow**:
```
1. Application reads Windows username via Environment.UserName
2. Application reads computer name via Environment.MachineName
3. Check if computer name matches shared terminal pattern:
    ├─ Exact match: "SHOP2", "MTMDC"
    ├─ Pattern match: Starts with "SHOP-FLOOR-"
    └─ Configuration file: Read from Model_WorkstationConfig.SharedTerminalNames
4. Computer name matches → Shared Terminal Mode
5. Pause splash screen at 40%
6. Display login dialog OVER splash screen (WinUI 3 ContentDialog):
    ├─ TextBox: Username (plain text)
    ├─ PasswordBox: 4-Digit PIN (masked with dots)
    ├─ Button: [Login]
    ├─ Button: [Cancel] (closes application)
    └─ Label: "Attempt X of 3" (after first failure)
7. User enters username and 4-digit PIN
8. Validate credentials:
    ├─ Call sp_ValidateUserPin(@username, @pin)
    ├─ Returns: employee_number, full_name, shift, department, is_active
9. If validation succeeds:
    ├─ Create Model_UserSession with authenticated user
    ├─ Call sp_LogUserActivity (event_type: 'login_success', workstation_name)
    ├─ Resume splash screen progress to 50%
    └─ Continue startup sequence
10. If validation fails:
    ├─ Increment failed attempt counter (max 3)
    ├─ Call sp_LogUserActivity (event_type: 'login_failed', details)
    ├─ Display error: "Invalid username or PIN. Attempt X of 3."
    ├─ If attempts < 3: Clear PIN field, allow retry
    ├─ If attempts = 3:
        ├─ Display error: "Maximum login attempts exceeded. Application closing."
        ├─ Call sp_LogUserActivity (event_type: 'login_locked_out')
        └─ Close application gracefully
11. If user clicks [Cancel]:
    └─ Close application (no error logged)
```

**Splash Screen Progress**:
- "Detecting workstation type..." (40%)
- "Shared workstation detected: [Computer Name]" (40%)
- "Waiting for user login..." (40% - paused, dialog shown)
- "Authenticating [Username]..." (45% - after login button click)
- "Loading employee profile..." (50% - after successful auth)
- "Logged in as: [Full Name] (Employee #[number])" (55%)

**Security Features**:
- **PIN Masking**: PasswordBox control masks all 4 digits as dots
- **Attempt Limiting**: Maximum 3 login attempts, then application closes
- **Audit Logging**: Every attempt (success/failure) logged with timestamp, username, workstation
- **No Credential Caching**: Credentials never stored on shared terminals
- **Session Isolation**: Previous user's data completely cleared before new login
- **Timeout**: Login dialog auto-closes after 5 minutes of inactivity (optional)

**Time**: Variable (5-30 seconds typical, depends on user input speed)

**Database Calls**:
- `sp_ValidateUserPin(@username, @pin)` - Returns user profile if valid
- `sp_LogUserActivity(...)` - Records every login attempt (success/failure/lockout)

**Error Messages**:
- Attempt 1: "Invalid username or PIN. Please try again. (Attempt 1 of 3)"
- Attempt 2: "Invalid username or PIN. Please try again. (Attempt 2 of 3)"
- Attempt 3: "Invalid username or PIN. Maximum attempts exceeded. Application will close for security."

---

### Scenario 3: New User First-Time Setup

**Context**: Windows username exists in OS but employee is not yet registered in application database.

**Triggers**:
- New employee's first day
- Existing employee receiving application access for first time
- Personal computer name changed requiring re-registration

**Flow**:
```
1. Application reads Windows username via Environment.UserName
2. Application detected as personal workstation (not shared terminal)
3. Call sp_GetUserByWindowsUsername(@windows_username)
4. Stored procedure returns NULL (user not found)
5. Pause splash screen at 45%
6. Display "New User Setup" dialog OVER splash screen (WinUI 3 ContentDialog):
    ├─ Label: "Windows Username: [detected username]" (read-only)
    ├─ TextBox: "Full Name" (e.g., "John Smith")
    ├─ PasswordBox: "4-Digit PIN" (for shared workstation access)
    ├─ PasswordBox: "Confirm PIN" (must match)
    ├─ ComboBox: "Shift" (options: 1st Shift, 2nd Shift, 3rd Shift)
    ├─ TextBox: "Department" (optional, e.g., "Receiving")
    ├─ Button: [Create Account]
    └─ Button: [Cancel] (closes application)
7. Supervisor/Administrator fills in required fields
8. Validate input:
    ├─ Full Name: Not empty, 2-100 characters
    ├─ PIN: Exactly 4 numeric digits
    ├─ Confirm PIN: Matches PIN field
    ├─ Shift: Valid selection required
    ├─ Department: Optional
9. Click [Create Account] button
10. Validation checks:
    ├─ If PIN already exists: Show error "PIN already in use by another user. Choose different PIN."
    ├─ If Windows username already exists: Show error (shouldn't happen, but safety check)
    └─ If validation passes: Continue to step 11
11. Call sp_CreateNewUser with parameters:
    ├─ @windows_username (detected from Environment.UserName)
    ├─ @full_name (from form)
    ├─ @pin (from form, hashed/encrypted before storage)
    ├─ @shift (from form)
    ├─ @department (from form)
    ├─ @created_by (current Windows username or "SYSTEM")
12. Stored procedure returns:
    ├─ New employee_number (auto-generated)
    ├─ Success status
13. If creation succeeds:
    ├─ Create Model_UserSession with new user data
    ├─ Call sp_LogUserActivity (event_type: 'user_created', details: creator info)
    ├─ Display success message: "Account created for [Full Name]. Employee #[number]"
    ├─ Resume splash screen progress to 50%
    └─ Continue startup with new user
14. If creation fails:
    ├─ Display error: "Failed to create user account. [Error details]. Contact IT."
    ├─ Log error details to application log
    └─ Close application
15. If user clicks [Cancel]:
    ├─ Display confirmation: "User account is required. Close application?"
    └─ If confirmed: Close application
```

**Splash Screen Progress**:
- "Identifying user..." (45%)
- "New user detected: [Windows Username]" (45% - paused)
- "Waiting for account creation..." (45% - dialog shown)
- "Creating user account..." (47% - after button click)
- "Loading employee profile..." (50% - after successful creation)
- "Welcome, [Full Name]! Employee #[number]" (55%)

**Security & Authorization**:
- **Who Can Create**: Supervisor or administrator must be physically present
- **Authorization Model**: Assumes physical presence = authorization (no credential check)
- **Audit Trail**: All user creations logged with Windows username of creator
- **PIN Uniqueness**: System enforces unique PINs across all users
- **Data Validation**: All inputs validated before database insert

**Time**: Variable (1-3 minutes typical, depends on data entry speed and supervisor availability)

**Database Calls**:
- `sp_GetUserByWindowsUsername(@windows_username)` - Initial check returns NULL
- `sp_CreateNewUser(...)` - Inserts new user record, returns employee_number
- `sp_LogUserActivity(...)` - Records user creation event

**Validation Rules**:
- **Full Name**: Required, 2-100 characters, alphabetic with spaces/hyphens
- **PIN**: Required, exactly 4 numeric digits (0-9 only), must be unique
- **Shift**: Required, must be one of: "1st Shift", "2nd Shift", "3rd Shift"
- **Department**: Optional, 0-50 characters
- **Windows Username**: Automatically detected, cannot be changed

**Error Messages**:
- "Full Name is required and must be 2-100 characters."
- "PIN must be exactly 4 numeric digits."
- "PIN and Confirm PIN do not match."
- "This PIN is already in use by another employee. Please choose a different PIN."
- "Failed to create user account: [Database error]. Please contact IT support."

---

## New User Creation Dialog Mockups

**Feature**: New User Setup Dialog (WinUI 3 ContentDialog)  
**Context**: Shown when Windows username not found in `users` table during startup  
**Dialog Type**: Modal ContentDialog over splash screen  
**Framework**: WinUI 3 (.NET 8.0, Windows App SDK)

---

### Mockup 1: Initial State (Dialog Display)

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application                                   [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  🆕 New Employee Setup                              [X]    │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  Windows User Not Found                                    │   │
│  │  ═══════════════════════════════════════════════════      │   │
│  │                                                            │   │
│  │  Your Windows username (JSMITH) was not found in the      │   │
│  │  receiving application database. Please complete this     │   │
│  │  form to create your employee account.                    │   │
│  │                                                            │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │  Employee Information                                 │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │  Full Name: *                                              │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │                                             │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │  (e.g., John Smith)                                        │   │
│  │                                                            │   │
│  │  Windows Username: *                                       │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ JSMITH                                      │ (auto)    │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │  (Automatically detected from Windows login)              │   │
│  │                                                            │   │
│  │  Shift: *                                                  │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ Select Shift                            ▼  │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  4-Digit PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │      │                                                  │   │
│  │  └──────┘                                                  │   │
│  │  (For shop floor terminal access)                          │   │
│  │                                                            │   │
│  │  Confirm PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │      │                                                  │   │
│  │  └──────┘                                                  │   │
│  │                                                            │   │
│  │  Department (Optional):                                    │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │                                             │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │  ℹ️ Important Notes                                   │ │   │
│  │  ├──────────────────────────────────────────────────────┤ │   │
│  │  │  • Your employee number will be auto-generated       │ │   │
│  │  │  • PIN must be unique across all employees           │ │   │
│  │  │  • Contact supervisor to modify account later        │ │   │
│  │  │  • All fields marked with * are required             │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │            [Create Account]    [Cancel]                    │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                    │
│  (Splash screen with "Waiting for account creation..." visible    │
│   in background at 45% progress)                                  │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘

Dialog Properties:
- Type: ContentDialog (WinUI 3)
- Width: 600px
- Height: Auto (based on content)
- Placement: Center of splash screen window
- IsPrimaryButtonEnabled: True (validates on click)
- IsSecondaryButtonEnabled: True
- PrimaryButtonText: "Create Account"
- SecondaryButtonText: "Cancel"
- DefaultButton: Primary
```

**WinUI 3 Controls Used**:
- `ContentDialog` - Main dialog container
- `TextBox` - Full Name, Windows Username, Department
- `PasswordBox` - PIN and Confirm PIN (masked input)
- `ComboBox` - Shift selection
- `InfoBar` - Important Notes section
- `StackPanel` - Layout container

**Key Features**:
- Clear explanation at top with detected Windows username
- Windows Username field pre-filled and read-only
- Shift dropdown with validation
- PIN fields masked by default
- Department is optional
- Important notes in InfoBar control
- Primary/Secondary button pattern

---

### Mockup 2: Shift Selection Dropdown Expanded

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application - New Employee Setup              [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  🆕 New Employee Setup                                     │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ... (header content same as above) ...                    │   │
│  │                                                            │   │
│  │  Full Name: *                                              │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ John Smith                                  │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  Windows Username: *                                       │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ JSMITH                                      │ (auto)    │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  Shift: *                                                  │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ 1st Shift                               ▲  │  ←Expanded│   │
│  │  ├────────────────────────────────────────────┤           │   │
│  │  │ 1st Shift                                  │           │   │
│  │  │ 2nd Shift                                  │           │   │
│  │  │ 3rd Shift                                  │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  4-Digit PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │ ●●●● │  ← Masked input                                 │   │
│  │  └──────┘                                                  │   │
│  │                                                            │   │
│  │  Confirm PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │ ●●●● │  ← Masked input                                 │   │
│  │  └──────┘                                                  │   │
│  │                                                            │   │
│  │  Department (Optional):                                    │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ Receiving                                   │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  ... (Important Notes section) ...                         │   │
│  │                                                            │   │
│  │            [Create Account]    [Cancel]                    │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────┘

Shift ComboBox Items:
- "1st Shift" (7:00 AM - 3:00 PM)
- "2nd Shift" (3:00 PM - 11:00 PM)
- "3rd Shift" (11:00 PM - 7:00 AM)

Note: No "Select Shift" placeholder item - first real option selected by default
```

**Key Features**:
- All fields filled with sample data
- PIN fields show masked bullets (●●●●)
- Shift dropdown showing 3 options
- Department filled but optional
- User can now click Create Account

---

### Mockup 3: Validation Error - PIN Mismatch

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application - New Employee Setup              [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  🆕 New Employee Setup                                     │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ⚠️ There are validation errors. Please correct them:     │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │ • PIN and Confirm PIN do not match                   │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │  Full Name: *                                              │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ John Smith                                  │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  Windows Username: *                                       │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ JSMITH                                      │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  Shift: *                                                  │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ 1st Shift                               ▼  │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  4-Digit PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │ ●●●● │  ← RED BORDER                                   │   │
│  │  └──────┘                                                  │   │
│  │                                                            │   │
│  │  Confirm PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │ ●●●● │  ← RED BORDER                                   │   │
│  │  └──────┘                                                  │   │
│  │  ⚠️ PINs must match                                        │   │
│  │                                                            │   │
│  │  Department (Optional):                                    │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ Receiving                                   │           │   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  ... (Important Notes) ...                                 │   │
│  │                                                            │   │
│  │            [Create Account]    [Cancel]                    │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────┘

Validation Display:
- InfoBar at top with Severity="Error" listing all validation errors
- Fields with errors show red border (Fluent Design error state)
- Inline error message below problematic fields
- Focus moves to first field with error
```

**Validation Rules**:
1. **Full Name**: Required, 2-100 characters, cannot be whitespace only
2. **Windows Username**: Auto-filled, cannot be modified
3. **Shift**: Required, must select valid option
4. **PIN**: Required, exactly 4 numeric digits, must be unique in database
5. **Confirm PIN**: Must match PIN field exactly
6. **Department**: Optional, 0-50 characters if provided

**Validation Errors Shown**:
- "Full Name is required"
- "PIN must be exactly 4 digits"
- "PIN and Confirm PIN do not match"
- "PIN is already in use by another employee"

---

### Mockup 4: Creating User (Loading State)

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application - New Employee Setup              [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  🆕 New Employee Setup                                     │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ℹ️ Creating employee account, please wait...             │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │ [█████████████████████████░░░░░░░░░░░░]  65%        │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │  Full Name: *                                              │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ John Smith                                  │ (disabled)│   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  Windows Username: *                                       │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ JSMITH                                      │ (disabled)│   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  Shift: *                                                  │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ 1st Shift                               ▼  │ (disabled)│   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │  4-Digit PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │ ●●●● │  (disabled)                                     │   │
│  │  └──────┘                                                  │   │
│  │                                                            │   │
│  │  Confirm PIN: *                                            │   │
│  │  ┌──────┐                                                  │   │
│  │  │ ●●●● │  (disabled)                                     │   │
│  │  └──────┘                                                  │   │
│  │                                                            │   │
│  │  Department (Optional):                                    │   │
│  │  ┌────────────────────────────────────────────┐           │   │
│  │  │ Receiving                                   │ (disabled)│   │
│  │  └────────────────────────────────────────────┘           │   │
│  │                                                            │   │
│  │            [Creating...]    [Cancel]                       │   │
│  │            (disabled)       (disabled)                     │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────┘

Loading Process:
1. Validate all inputs (instant)
2. Check PIN uniqueness in database (~200ms)
3. Call sp_CreateNewUser stored procedure (~300ms)
4. Generate employee number (auto-increment)
5. Log user creation activity (~100ms)
6. Success or error displayed

All controls disabled during operation to prevent double-submission.
ProgressRing or ProgressBar shows indeterminate or percentage-based progress.
```

**WinUI 3 Loading Pattern**:
- `InfoBar` with `Severity="Informational"` and progress message
- `ProgressBar` showing percentage or `ProgressRing` for indeterminate
- All input controls set `IsEnabled="False"`
- Primary and Secondary buttons disabled
- Cancel button remains enabled initially, then disabled during final DB commit

---

### Mockup 5: User Creation Success

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application - New Employee Setup              [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  ✅ Account Created Successfully                           │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ✓ Employee account created successfully!                  │   │
│  │                                                            │   │
│  │  Welcome to MTM Receiving Application, John Smith!        │   │
│  │                                                            │   │
│  │  Your account details:                                     │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │ • Employee Number: 6229                              │ │   │
│  │  │ • Full Name: John Smith                              │ │   │
│  │  │ • Windows Username: JSMITH                           │ │   │
│  │  │ • Shift: 1st Shift                                   │ │   │
│  │  │ • Department: Receiving                              │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │  The application will now continue loading...             │   │
│  │                                                            │   │
│  │  Important reminders:                                      │   │
│  │  • Use your PIN (****) for shop floor terminal access     │   │
│  │  • Your employee number appears on all labels you create  │   │
│  │  • Contact supervisor to modify account settings          │   │
│  │                                                            │   │
│  │                         [Continue]                         │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                    │
│  (Splash screen will resume at 50% after clicking Continue)       │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘

Dialog Result:
- ContentDialogResult.Primary (Continue button clicked)
- Model_UserSession created with new employee data
- Splash screen resumes: "Loading employee profile..." (50%)
- Startup sequence continues normally
```

**Behind the Scenes**:
1. User record created in `users` table
2. Employee number auto-generated (e.g., 6229)
3. PIN stored encrypted/hashed in database
4. User activity logged: `sp_LogUserActivity(event_type: 'user_created')`
5. `Model_UserSession` populated with new user data
6. Application startup continues from step 50

---

### Mockup 6: Database Error During Creation

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application - New Employee Setup              [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  ❌ Error Creating Account                                 │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ⚠️ Failed to create employee account                      │   │
│  │                                                            │   │
│  │  A database error occurred while creating your account.   │   │
│  │  This may be a temporary network or server issue.         │   │
│  │                                                            │   │
│  │  Error details:                                            │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │ Unable to connect to MySQL database server.          │ │   │
│  │  │ Connection timeout after 10 seconds.                 │ │   │
│  │  │                                                      │ │   │
│  │  │ Server: mysql.company.local:3306                     │ │   │
│  │  │ Database: mtm_receiving_app                          │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │  What to try:                                              │   │
│  │  • Check network connection is active                      │   │
│  │  • Wait a moment and click "Retry" to try again           │   │
│  │  • Click "Cancel" to close the application                │   │
│  │  • Contact IT support if problem persists                 │   │
│  │                                                            │   │
│  │              [Retry]        [Cancel]                       │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────┘

Error Handling via IService_ErrorHandler:
- Shows detailed error from Model_Dao_Result.ErrorMessage
- User-friendly explanation and guidance
- Retry option returns user to form with data preserved
- Cancel option closes application gracefully
- Error logged with full stack trace
```

**Error Types Shown**:
1. **Database Connection Error**: "Unable to connect to MySQL database server"
2. **PIN Already Exists**: "This PIN is already in use. Please choose a different 4-digit PIN."
3. **Insert Failed**: "Failed to insert user record. Database error: [details]"
4. **Timeout**: "Database operation timed out. Please try again."

---

### Mockup 7: Cancel Confirmation

```
┌────────────────────────────────────────────────────────────────────┐
│  MTM Receiving Application - New Employee Setup              [X]   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  🆕 New Employee Setup                                     │   │
│  ├────────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ┌──────────────────────────────────────────────────────┐ │   │
│  │  │  ⚠️ Confirm Application Exit                         │ │   │
│  │  ├──────────────────────────────────────────────────────┤ │   │
│  │  │                                                      │ │   │
│  │  │  Are you sure you want to cancel account creation?  │ │   │
│  │  │                                                      │ │   │
│  │  │  An employee account is required to use this        │ │   │
│  │  │  application. If you exit now, the application      │ │   │
│  │  │  will close.                                         │ │   │
│  │  │                                                      │ │   │
│  │  │  Contact your supervisor or system administrator    │ │   │
│  │  │  for assistance with account creation.              │ │   │
│  │  │                                                      │ │   │
│  │  │           [Yes, Exit]      [No, Stay]               │ │   │
│  │  │                                                      │ │   │
│  │  └──────────────────────────────────────────────────────┘ │   │
│  │                                                            │   │
│  │  ... (form content behind confirmation dialog) ...         │   │
│  │                                                            │   │
│  └────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────┘

Triggered by: Cancel button or X button in dialog
Behavior:
- Shows nested ContentDialog for confirmation
- "Yes, Exit" → ContentDialogResult.Secondary → Application closes
- "No, Stay" → Dismisses confirmation, returns to form
- User can continue filling out form if they choose to stay
```

**Implementation**:
```csharp
private async void OnCancelButtonClick(object sender, RoutedEventArgs e)
{
    var confirmDialog = new ContentDialog
    {
        Title = "Confirm Application Exit",
        Content = "Are you sure you want to cancel account creation?\n\n" +
                  "An employee account is required...",
        PrimaryButtonText = "No, Stay",
        SecondaryButtonText = "Yes, Exit",
        DefaultButton = ContentDialogButton.Primary
    };
    
    var result = await confirmDialog.ShowAsync();
    if (result == ContentDialogResult.Secondary)
    {
        // User chose to exit
        this.Hide();
        Application.Current.Exit();
    }
}
```

---

## Field Specifications

### Text Input Controls

| Field | Control Type | Width | MaxLength | Placeholder | Validation |
|-------|-------------|-------|-----------|-------------|------------|
| Full Name | TextBox | 400px | 100 | "John Smith" | Required, 2-100 chars |
| Windows Username | TextBox | 400px | 30 | Auto-filled | Required, read-only |
| Department | TextBox | 400px | 50 | "Receiving" | Optional, 0-50 chars |
| 4-Digit PIN | PasswordBox | 80px | 4 | Empty | Required, 4 numeric digits, unique |
| Confirm PIN | PasswordBox | 80px | 4 | Empty | Required, must match PIN |

### Selection Controls

**Shift ComboBox**:
- **Items**: "1st Shift", "2nd Shift", "3rd Shift"
- **Width**: 400px
- **Default**: "1st Shift" (first item selected)
- **Validation**: Required

### Button Specifications

| Button | Style | IsEnabled | Action |
|--------|-------|-----------|--------|
| Create Account | Accent (Primary) | True (validates on click) | Calls `CreateUserAsync()` |
| Cancel | Standard (Secondary) | True | Shows confirmation dialog |

---

## Keyboard Navigation & Accessibility

### Tab Order
1. Full Name
2. Windows Username (read-only, skip on Tab)
3. Shift ComboBox
4. 4-Digit PIN
5. Confirm PIN
6. Department
7. Create Account button
8. Cancel button

### Keyboard Shortcuts
- **Enter**: Activate Create Account button (if enabled)
- **Escape**: Activate Cancel button (shows confirmation)
- **Tab**: Move to next control
- **Shift+Tab**: Move to previous control

### Screen Reader Support
- Dialog announced: "New Employee Setup"
- Required fields announced with "required"
- Validation errors announced when field loses focus
- Success message announced after account creation
- All controls have appropriate AutomationProperties.Name

### High Contrast & Theme Support
- Inherits WinUI 3 theme (Light/Dark/High Contrast)
- Validation errors visible in all themes
- Focus indicators clear and visible
- Text meets WCAG AA contrast requirements

---

## Integration with Startup Sequence

### Splash Screen Context

**Before Dialog**:
```
[45%] Identifying user...
[45%] Windows username 'JSMITH' not found in database
[45%] Waiting for account creation...
```

**Dialog Shown**:
- Splash screen remains visible in background (dimmed)
- Dialog displayed centered over splash screen
- Splash screen progress bar remains at 45%

**After Success** (Create Account clicked):
```
[47%] Creating employee account...
[50%] Employee account created. Welcome, John Smith!
[50%] Loading employee profile...
[55%] Logged in as: John Smith (Employee #6229)
[60%] Loading user preferences...
```

**After Cancel** (Cancel clicked, confirmed):
```
[45%] Account creation cancelled by user
[ERROR] Application startup failed: User account required
[Application closes gracefully]
```

### Database Integration

**Stored Procedures Called**:
1. `sp_GetUserByWindowsUsername(@windows_username)` - Initial check (returns NULL)
2. `sp_CreateNewUser(...)` - Creates user record, returns employee_number
3. `sp_LogUserActivity(...)` - Logs user creation event

**Tables Modified**:
- `users` - New row inserted with employee data
- `user_activity_log` - Activity logged for audit trail

---

## Testing Scenarios

### Test Case 1: Happy Path
1. Launch app with non-existent Windows username
2. Dialog appears at 45% splash progress
3. Fill all required fields with valid data
4. Click Create Account
5. **Expected**: Success message, dialog closes, startup continues

### Test Case 2: PIN Mismatch Validation
1. Dialog appears
2. Enter PIN: 1234
3. Enter Confirm PIN: 5678
4. Click Create Account
5. **Expected**: Error InfoBar shows "PINs must match", red borders on PIN fields

### Test Case 3: PIN Already Exists
1. Dialog appears
2. Enter PIN that exists in database (e.g., 1234)
3. Fill other fields
4. Click Create Account
5. **Expected**: Error dialog "PIN already in use", return focus to PIN field

### Test Case 4: Database Connection Error
1. Dialog appears
2. Stop MySQL server
3. Fill form, click Create Account
4. **Expected**: Error dialog with connection details, Retry button enabled

### Test Case 5: Cancel and Exit
1. Dialog appears
2. Click Cancel
3. Confirmation dialog appears
4. Click "Yes, Exit"
5. **Expected**: Application closes gracefully

### Test Case 6: Cancel and Return
1. Dialog appears
2. Fill some fields
3. Click Cancel
4. Confirmation appears
5. Click "No, Stay"
6. **Expected**: Return to form with data preserved

---

## Code References

**Dialog Implementation**: To be created in `Views/Shared/NewUserSetupDialog.xaml` and `.xaml.cs`  
**ViewModel**: To be created in `ViewModels/Shared/NewUserSetupViewModel.cs`  
**User DAO**: `Data/Dao_User.cs` (to be created) - `CreateUserAsync()`, `CheckPinExistsAsync()`  
**Session Model**: `Models/Model_UserSession.cs` (to be created)  
**Startup Integration**: Integrated in `App.xaml.cs` startup sequence at 45% progress  
**Error Handling**: Uses existing `IService_ErrorHandler`

---

## WinUI 3 Implementation Notes

1. **ContentDialog Pattern**: Use WinUI 3 ContentDialog for modal dialogs
2. **PasswordBox**: Use PasswordBox control for PIN fields (automatic masking)
3. **InfoBar**: Use for validation errors and success messages
4. **ProgressBar/ProgressRing**: Show loading state during async operations
5. **Theme Integration**: Dialog inherits application theme (ElementTheme)
6. **Async/Await**: All database calls must be async to keep UI responsive
7. **Data Binding**: Use x:Bind for performance and compile-time checking
8. **MVVM Pattern**: Separate ViewModel from View for testability
9. **Resource Strings**: Externalize all text for localization support
10. **Logging**: Log all actions via ILoggingService

---

## Threading and Synchronization

### UI Thread vs Background Thread

**UI Thread** (Main Thread):
- **Purpose**: Handles all visual updates, user input
- **Runs**: Splash screen display, progress updates, main window
- **Rule**: NEVER perform long-running tasks on UI thread (causes freezing)

**Background Thread** (Worker Thread):
- **Purpose**: Performs initialization tasks
- **Runs**: Database queries, file I/O, network requests
- **Rule**: NEVER directly update UI from background thread (causes crashes)

---

### Cross-Thread Communication Pattern

**Problem**: Background thread completes task and needs to update splash screen progress.

**Solution**: Progress Reporter Pattern

```
Background Thread                          UI Thread
    │                                         │
    ├─ Complete database initialization       │
    ├─ Call: progressReporter.Report((30, "Database ready"))
    │                                         │
    │  ──────── Cross-Thread Call ─────────> │
    │                                         │
    │                              Update splash progress bar
    │                              Update splash message label
    │                                         │
    ├─ Continue to next initialization step   │
```

**Implementation Notes**:
- Progress reporter automatically marshals calls to UI thread
- No manual thread synchronization required by developer
- Built-in to most modern UI frameworks

---

## Splash Screen Lifecycle

### State Transitions

```
State 1: NOT CREATED
    ↓
    Application starts
    ↓
State 2: CREATED
    ↓
    Splash screen window instantiated
    Visual elements initialized (logo, labels, progress bar)
    ↓
State 3: VISIBLE
    ↓
    Window shown to user
    User sees splash screen
    ↓
State 4: INITIALIZING
    ↓
    Background tasks running
    Progress updates displayed
    May pause for user input (login dialogs)
    ↓
State 5: COMPLETE
    ↓
    All initialization succeeded
    Main window ready to display
    ↓
State 6: HIDDEN
    ↓
    Splash screen hidden from view
    Main window shown
    ↓
State 7: DISPOSED
    ↓
    Splash screen resources released
    Memory cleaned up
```

### Memory Management

**When to Dispose Splash Screen**:
- After main window is visible
- After splash screen has been hidden for at least 500ms (smooth transition)
- Before application exits (if startup fails)

**Resources to Release**:
- Window handle
- Graphics resources (logo images, fonts)
- Event handlers (prevent memory leaks)
- Background task references

---

## Configuration and Customization

### Splash Screen Settings

**Visual Customization**:
- Logo image path (replaceable per installation)
- Background color (theme-aware)
- Text color (theme-aware, high contrast)
- Font family and size
- Window size and position (centered on screen)

**Behavior Customization**:
- Minimum display time (e.g., 2 seconds - prevent flicker)
- Maximum display time (e.g., 60 seconds - timeout)
- Progress update frequency (e.g., every 100ms)
- Fade-in/fade-out animation (optional)

**Initialization Steps Customization**:
- Which services to initialize
- Order of initialization
- Timeout per service
- Retry logic per service
- Error handling strategy per service

---

### Application-Specific Settings

**Example Configuration File (JSON)** - Or stored in Model_Application_Variables:
```json
{
  "SplashScreen": {
    "MinimumDisplayTimeMs": 2000,
    "MaximumDisplayTimeMs": 60000,
    "FadeInDurationMs": 300,
    "FadeOutDurationMs": 300,
    "LogoPath": "Assets/Icons/MTMLogo.png",
    "ShowVersionNumber": true,
    "ShowEmployeeInfo": true
  },
  "Initialization": {
    "DatabaseConnectionTimeoutMs": 30000,
    "DatabaseRetryAttempts": 3,
    "DatabaseRetryDelayMs": 5000,
    "ServiceTimeoutMs": 10000,
    "EnableParallelInitialization": false,
    "LoginDialogTimeoutMinutes": 5
  },
  "Authentication": {
    "MaxLoginAttempts": 3,
    "SharedTerminalNames": ["SHOP2", "MTMDC"],
    "SharedTerminalPatterns": ["SHOP-FLOOR-*"],
    "RequirePinForSharedTerminals": true,
    "PinLength": 4,
    "LogAllAuthenticationAttempts": true
  },
  "Database": {
    "Server": "mysql.company.local",
    "Database": "mtm_receiving_app",
    "Port": 3306,
    "UserId": "app_user",
    "PasswordEncrypted": "[encrypted]" 
  }
}
```

**Stored in C# Code** (Model_Application_Variables):
```csharp
public class Model_Application_Variables
{
    public string ApplicationName { get; set; } = "MTM Receiving Label Application";
    public string Version { get; set; } = "1.0.0";
    public string ConnectionString { get; set; } = string.Empty;
    public string LogDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MTM_Receiving_Application",
        "Logs"
    );
    public string EnvironmentType { get; set; } = "Development";
    public List<string> SharedTerminalNames { get; set; } = new() { "SHOP2", "MTMDC" };
}
```

---

## Performance Optimization

### Parallel Initialization

**When Applicable**:
- Services are independent (no dependencies on each other)
- Services perform I/O operations (database, file, network)
- Total startup time is too long (> 10 seconds)

**Example**:
```
Sequential Initialization:
    Database (3s) → Config (2s) → User (2s) → Cache (3s) = 10 seconds total

Parallel Initialization:
    Database (3s) ┐
    Config (2s)   ├─ Execute Simultaneously → 3 seconds total (longest task)
    User (2s)     │
    Cache (3s)    ┘
```

**Considerations**:
- More complex error handling (multiple failures possible)
- Progress reporting becomes approximate
- Thread safety for shared resources (logging, error handling)
- Dependency ordering must be enforced (e.g., User requires Database)

---

### Lazy Initialization

**Concept**: Delay non-critical initialization until after main window is shown.

**Candidates for Lazy Initialization**:
- Data caches that improve performance but aren't required
- Background services (update checkers, telemetry)
- Advanced features not used immediately

**Example**:
```
Critical Startup (Show main window):
    Database ✓
    User Authentication ✓
    Core Configuration ✓
    Main Window Creation ✓
    
Lazy Initialization (After main window visible):
    ├─ Analytics Service (load in background)
    ├─ Help System (load on first F1 press)
    ├─ Advanced Reporting (load on first report request)
    └─ Plugin System (load on first plugin use)
```

**Benefits**:
- Faster perceived startup time (main window appears sooner)
- Better user experience (no waiting for unused features)
- Reduced initial memory footprint

---

## Testing and Validation

### Manual Testing Scenarios

**Test 1: Normal Startup**
1. Launch application
2. Observe splash screen appears within 500ms
3. Verify progress updates every 1-2 seconds
4. Confirm main window appears within 10 seconds
5. Verify splash screen disappears cleanly

**Expected**: Smooth progression from 0% to 100%, no errors.

---

**Test 2: Database Unavailable**
1. Stop database server
2. Launch application
3. Observe splash screen shows "Connecting to database..."
4. Verify retry attempts (3 total)
5. Confirm error dialog appears after 30 seconds
6. Verify application closes gracefully

**Expected**: Clear error message, application does not crash.

---

**Test 3: Slow Database Connection**
1. Simulate network latency (throttle connection to 1 Mbps)
2. Launch application
3. Observe splash screen progresses slowly but steadily
4. Verify timeout does not trigger prematurely
5. Confirm startup completes successfully after delay

**Expected**: Startup completes, no false timeout errors.

---

**Test 4: Shared Workstation Login**
1. Configure OS username as shared workstation identifier
2. Launch application
3. Verify splash screen pauses at 40%
4. Confirm login dialog appears over splash
5. Enter valid credentials
6. Verify startup continues and main window appears

**Expected**: Seamless login flow, no crashes.

---

**Test 5: Invalid Login Attempts**
1. Launch on shared workstation
2. Enter incorrect username/PIN
3. Verify error message displayed
4. Attempt 2 more incorrect logins
5. Confirm application closes after 3rd failure

**Expected**: Clear error messages, application closes gracefully after 3 failures.

---

**Test 6: New User Creation**
1. Launch with OS username not in database
2. Verify "New User Setup" dialog appears
3. Fill in user details
4. Submit form
5. Confirm startup continues with new user
6. Verify user exists in database

**Expected**: User created successfully, startup completes.

---

### Automated Testing

**Unit Tests**:
- Progress calculation logic (0-100% mapping)
- Error message generation
- Timeout calculation
- Retry logic

**Integration Tests**:
- Database connection with mock database
- Configuration loading with test config files
- User authentication with test user data
- Service initialization order

**End-to-End Tests**:
- Full startup sequence with real services (test environment)
- Startup with database unavailable (test failover)
- Startup with slow services (test timeout handling)

---

## Logging and Diagnostics

### What to Log During Startup

**Startup Begin**:
```
[INFO] Application startup initiated
    Version: 1.2.3
    User: johnsmith
    OS: Windows 10 Pro
    Timestamp: 2025-12-14 08:30:15.123
```

**Each Initialization Step**:
```
[INFO] Initializing logging system... (10%)
    Duration: 125ms
    Status: Success

[INFO] Connecting to database... (30%)
    Server: sql.company.local
    Database: app_prod
    Duration: 2340ms
    Status: Success
    
[WARN] Loading user preferences... (60%)
    User: johnsmith
    Duration: 450ms
    Status: Failed - Using defaults
    Reason: Preferences table empty for user
```

**Startup Complete**:
```
[INFO] Application startup complete
    Total Duration: 4.8 seconds
    Initialization Steps: 10
    Warnings: 1
    Errors: 0
    Main Window: Ready
```

**Startup Failure**:
```
[ERROR] Application startup failed
    Total Duration: 32.5 seconds
    Failed Step: Database Connection
    Retry Attempts: 3
    Error: Database server unreachable (sql.company.local:1433)
    Action: Application closing
```

---

### Diagnostic Information for Support

**Information to Include in Error Reports**:
1. Application version
2. Operating system details
3. Username and machine name
4. Startup sequence log (all steps)
5. Error message and stack trace
6. Configuration settings (sanitized - no passwords)
7. Database connection string (sanitized)
8. Timestamp of failure
9. Previous successful startup time (for comparison)

**Example Support Email**:
```
Subject: MTM Receiving App - Startup Failure - Cannot Connect to Database

Application: MTM Receiving Label Application v1.0.0
Framework: .NET 8.0, WinUI 3, Windows App SDK 1.8
User: john.smith (Windows username)
Employee: Not authenticated (startup failed before auth)
Machine: OFFICE-PC-025
OS: Windows 10 Pro (Build 19045)
Date/Time: 2025-12-15 08:30:45

Error:
    Unable to connect to MySQL database server after 3 attempts (30 seconds total).
    
Details:
    Server: mysql.company.local:3306
    Database: mtm_receiving_app
    Connection String: Server=mysql.company.local;Port=3306;Database=mtm_receiving_app;Uid=app_user;...
    Error Code: ERR_STARTUP_DB_CONNECTION_FAILED
    MySQL Error: "Unable to connect to any of the specified MySQL hosts"
    
Startup Log:
    [00:00.000] Application launched
    [00:00.085] Logging initialized (ILoggingService)
    [00:00.120] Error handler initialized (IService_ErrorHandler)
    [00:00.180] Database configuration loaded (Helper_Database_Variables)
    [00:00.250] Splash screen displayed
    [00:02.345] Database connection attempt 1 - FAILED (Timeout after 10s)
    [00:07.456] Database connection attempt 2 - FAILED (Timeout after 10s)
    [00:12.567] Database connection attempt 3 - FAILED (Timeout after 10s)
    [00:32.678] Startup failed, displaying error dialog
    [00:35.123] Application closing gracefully

Log File: %AppData%\MTM_Receiving_Application\Logs\2025-12-15.log

Please verify:
1. MySQL server is running and accessible
2. Network connectivity from OFFICE-PC-025 to mysql.company.local
3. Database 'mtm_receiving_app' exists
4. User 'app_user' has permissions
5. Firewall allows port 3306

Contact: IT Support (ext. 5555)
```

---

## Common Issues and Troubleshooting

### Issue 1: Splash Screen Flickers or Disappears Too Quickly

**Cause**: Initialization completes too fast, splash screen closes before user sees it.

**Solution**: Implement minimum display time (e.g., 2 seconds).

```
Startup completes in 800ms
Minimum display time: 2000ms
Actual display time: 2000ms (splash held open for extra 1200ms)
```

---

### Issue 2: Splash Screen Freezes at 40%

**Possible Causes**:
1. Database connection hanging (timeout not configured)
2. Service waiting for user input but dialog not displayed
3. Deadlock in initialization code

**Diagnostic Steps**:
1. Check logs for last completed step
2. Verify database server is reachable
3. Check for login dialogs hidden behind other windows
4. Review initialization code for blocking calls

**Solution**: Implement per-service timeouts and detailed logging.

---

### Issue 3: Main Window Appears with Incomplete Initialization

**Cause**: Critical initialization steps skipped or failed silently.

**Symptoms**:
- Features not working
- Data missing
- Errors appearing after startup

**Solution**: Verify all critical initialization steps have error handling and block startup on failure.

```
Critical Steps (MUST succeed):
    ✓ Database Connection
    ✓ User Authentication
    ✓ Core Configuration
    
Non-Critical Steps (CAN fail):
    ⚠ User Preferences (use defaults)
    ⚠ Data Cache (load on demand)
    ⚠ Theme (use default theme)
```

---

### Issue 4: Application Crashes During Startup

**Cause**: Unhandled exception in initialization code.

**Solution**: Wrap ALL initialization steps in try-catch blocks with proper error handling.

```
For each initialization step:
    try {
        Execute step
        Report success
    } catch (Exception ex) {
        Log error with full details
        Determine if critical:
            If critical: Display error dialog and close application
            If non-critical: Use fallback and continue
    }
```

---

### Issue 5: Splash Screen Never Closes

**Cause**: Main window creation failed or hidden behind splash screen.

**Diagnostic Steps**:
1. Check if main window was created successfully
2. Verify main window is not hidden (Visible = true)
3. Check window Z-order (splash screen should not be topmost)
4. Review main window constructor for exceptions

**Solution**: Ensure main window is explicitly shown and splash screen is explicitly hidden/disposed.

---

## Security Considerations

### Sensitive Information on Splash Screen

**What NOT to Display**:
- Database connection strings
- Passwords or encryption keys
- Internal server names or IP addresses
- Detailed error messages with system paths
- User's full permissions list

**What IS Safe to Display**:
- Application name and version
- Generic progress messages ("Loading configuration...")
- User's display name (not username)
- High-level step names
- Time remaining (approximate)

---

### Secure Login on Shared Workstations

**Best Practices**:
1. **Password Masking**: Always mask PIN/password fields
2. **Attempt Limiting**: Maximum 3 login attempts before lockout
3. **Audit Logging**: Log all login attempts (success and failure)
4. **Timeout**: Auto-close login dialog after 60 seconds of inactivity
5. **No Caching**: Never cache credentials on shared workstations
6. **Session Isolation**: Ensure previous user's data is cleared

---

## Accessibility Considerations

### Screen Reader Support

**Splash Screen Announcements**:
- Initial announcement: "Application starting"
- Progress updates: "Loading database, 30 percent complete"
- Completion: "Initialization complete, opening main window"

**Error Announcements**:
- "Error: Unable to connect to database. Retrying..."
- "Critical error: Application startup failed. Please contact support."

---

### High Contrast and Theme Support

**Visual Accessibility**:
- Respect OS high-contrast mode settings
- Ensure text is readable against background (WCAG AA compliance)
- Provide theme options (light/dark/high-contrast)
- Use large, readable fonts (minimum 12pt)

---

### Keyboard Navigation

**Login Dialog Accessibility**:
- Tab order: Username → Password → Login button → Cancel button
- Enter key submits form
- Escape key cancels dialog
- Focus visible at all times

---

## Performance Benchmarks

### Target Startup Times

**Excellent Performance**:
- Cold start: < 3 seconds (first launch after boot)
- Warm start: < 1 second (subsequent launches)
- Total initialization: < 5 seconds

**Acceptable Performance**:
- Cold start: 3-5 seconds
- Warm start: 1-2 seconds
- Total initialization: 5-10 seconds

**Poor Performance** (Requires Optimization):
- Cold start: > 5 seconds
- Warm start: > 2 seconds
- Total initialization: > 10 seconds

---

### Performance Metrics to Track

**Metrics**:
1. **Time to Splash Screen Visible**: Should be < 500ms
2. **Time to First Progress Update**: Should be < 1 second
3. **Time per Initialization Step**: Log duration of each step
4. **Total Startup Time**: Measure from launch to main window visible
5. **Memory Usage**: Track memory consumption during startup
6. **Database Query Count**: Minimize queries during startup

---

## Future Enhancements

### Progressive Web App (PWA) Splash Screens

**Consideration**: If building a web-based version, splash screen patterns differ:
- Use service workers for offline loading
- Display splash screen while service worker caches assets
- Show progress for asset download

---

### Animated Splash Screens

**Enhancement**: Add subtle animations for premium feel:
- Logo fade-in animation
- Progress bar smooth fill animation
- Pulsing "loading" indicator
- Particle effects (subtle, not distracting)

**Caution**: Keep animations lightweight to avoid delaying startup.

---

### Contextual Tips During Startup

**Enhancement**: Display helpful tips while user waits:
- "Tip: Press F1 to access the help system"
- "Did you know? You can customize shortcuts in Settings"
- "Pro tip: Use Ctrl+S to save your work"

**Benefit**: Educates users during otherwise idle wait time.

---

## Summary

### Key Takeaways

1. **User Experience First**: Splash screen provides immediate feedback, preventing "frozen" appearance.

2. **Asynchronous Initialization**: Keep UI responsive by performing initialization on background threads.

3. **Robust Error Handling**: Gracefully handle failures at each step with clear user messaging.

4. **Progress Transparency**: Show users what's happening with percentage and text updates.

5. **Security by Design**: Implement secure login flows, limit attempts, audit all actions.

6. **Performance Optimization**: Target < 5 seconds total startup time for good user experience.

7. **Accessibility**: Support screen readers, high contrast, and keyboard navigation.

8. **Comprehensive Logging**: Log every step for diagnostics and support troubleshooting.

9. **Graceful Degradation**: Use fallback values for non-critical failures, continue startup.

10. **Testability**: Design for both manual and automated testing of all startup scenarios.

---

## Document Maintenance

**When to Update This Document**:
- New initialization steps added
- Error handling strategy changes
- Performance benchmarks change
- New authentication methods added
- Accessibility requirements updated

**Review Frequency**: Quarterly or after major application releases

**Document Owner**: Development Team Lead

---

**Version**: 1.0  
**Last Reviewed**: 2025-12-15  
**Next Review**: 2026-03-15  
**For Questions**: Contact development team or refer to source code in App.xaml.cs and startup initialization classes.

**Related Documentation**:
- [User Authentication Specification](specs/001-user-login/spec.md)
- [Database Schema](Database/Schemas/)
- [MVVM Guidelines](.github/instructions/mvvm-pattern.instructions.md)
- [Dependency Injection Setup](.github/instructions/mvvm-dependency-injection.instructions.md)

**Implementation Files**:
- `App.xaml.cs` - Application entry point and dependency injection setup
- `MainWindow.xaml.cs` - Main application window
- `Models/Receiving/Model_Application_Variables.cs` - Application configuration
- `Models/Receiving/Model_UserSession.cs` - Session management (to be created)
- `Services/IService_Authentication.cs` - Authentication service interface (to be created)
- `Contracts/Services/IService_ErrorHandler.cs` - Error handling
- `Contracts/Services/ILoggingService.cs` - Logging service
- `Helpers/Database/Helper_Database_Variables.cs` - Database configuration

**Database Dependencies**:
- `users` table - Employee authentication and profile data
- `user_activity_log` table - Audit trail for all authentication events
- `sp_GetUserByWindowsUsername` - Retrieve user by Windows username
- `sp_ValidateUserPin` - Validate username + PIN for shared terminals
- `sp_CreateNewUser` - Create new user account
- `sp_LogUserActivity` - Record authentication events

---

**END OF DOCUMENTATION**
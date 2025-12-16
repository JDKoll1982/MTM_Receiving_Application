# Feature Specification: User Authentication & Login System

**Feature Branch**: `001-user-login`  
**Created**: December 15, 2025  
**Status**: Draft  
**Input**: User description: "Implement multi-tier authentication system with Windows username detection for personal workstations, PIN-based login for shared shop floor terminals, automatic new user creation workflow, and optional Visual/Infor ERP integration credentials."

## Startup Workflow Integration

**Context**: Authentication is integrated into the application's 3-phase startup sequence with splash screen progress reporting.

### Three-Phase Startup Model

**Phase 1: Environment Setup** (< 100ms)
- Initialize logging service
- Configure dependency injection container
- Load database configuration
- **Progress**: 0-15%

**Phase 2: Splash Screen + Authentication** (2-10 seconds)
- Display splash screen with MTM branding
- Connect to MySQL database (with 3 retry attempts)
- Detect workstation type (personal vs. shared terminal)
- **Authenticate user** (varies by workstation type)
- Load user profile and session
- **Progress**: 20-60%

**Phase 3: Main Application Launch** (< 500ms)
- Load user preferences and theme
- Initialize ViewModels
- Create MainWindow
- Hide splash screen, show main application
- **Progress**: 60-100%

### Authentication Integration Points

**Step 40% - Detect Workstation Type**:
- Read Windows computer name via `Environment.MachineName`
- Compare against configured shared terminal names (SHOP2, MTMDC, SHOP-FLOOR-*)
- Determine authentication flow: Auto-login or PIN-based
- Splash message: "Detecting workstation type..."

**Step 45% - Authenticate User**:
- **Personal Workstation**: Read Windows username, query `sp_GetUserByWindowsUsername`
  - If found: Load profile, continue to 50%
  - If not found: Show New User Creation dialog, pause at 45%
- **Shared Terminal**: Show PIN login dialog over splash screen, pause at 45%
  - User enters username + PIN
  - Validate via `sp_ValidateUserPin`
  - If valid: Continue to 50%
  - If invalid: Show error, allow 3 attempts, then close app
- Splash messages: "Identifying user...", "Authenticating [Username]...", "Waiting for login..."

**Step 50% - Load User Profile**:
- Create `Model_UserSession` with authenticated user data
- Store employee number, full name, shift, department
- Call `sp_LogUserActivity` (event_type: 'login_success')
- Make user info available to application context
- Splash message: "Loading employee profile...", "Logged in as: [Full Name] (Employee #[number])"

**Step 55% - Check ERP Credentials** (Optional):
- Check if `visual_username` and `visual_password` are populated
- Enable/disable ERP menu options based on availability
- Splash message: "Checking Visual/Infor ERP access..."

### Dialog Integration

**New User Creation Dialog** (WinUI 3 ContentDialog):
- **Trigger**: Windows username not found in database (step 45%)
- **Display**: Modal dialog over splash screen, centered
- **Splash State**: Paused at 45%, message: "Waiting for account creation..."
- **Fields**: Full Name, Windows Username (auto-filled), Shift, 4-Digit PIN, Confirm PIN, Department (optional)
- **Actions**: Create Account (calls `sp_CreateNewUser`) or Cancel (closes application)
- **On Success**: Resume splash at 47%, message: "Creating employee account...", continue to 50%
- **On Cancel**: Display confirmation, close application gracefully

**Shared Terminal Login Dialog** (WinUI 3 ContentDialog):
- **Trigger**: Computer name matches shared terminal pattern (step 45%)
- **Display**: Modal dialog over splash screen, centered
- **Splash State**: Paused at 45%, message: "Waiting for user login..."
- **Fields**: Username (TextBox), 4-Digit PIN (PasswordBox with masking)
- **Actions**: Login (validates credentials) or Cancel (closes application)
- **Security**: Maximum 3 attempts, application closes on 3rd failure
- **On Success**: Resume splash at 50%, message: "Loading employee profile..."
- **On Failure**: Show error, log attempt via `sp_LogUserActivity`, allow retry

### Progress Reporting

Authentication steps report progress using `IProgress<(int percentage, string message)>` pattern:

```csharp
progress.Report((40, "Detecting workstation type..."));
progress.Report((45, "Identifying user..."));
progress.Report((50, "Loading employee profile..."));
progress.Report((55, "Logged in as: John Smith (Employee #6229)"));
```

Splash screen updates UI on main thread via Dispatcher, displays progress bar and status text.

### Startup Timing Targets

- **Authentication Detection**: < 500ms (workstation type + Windows username)
- **Database Lookup**: < 800ms (user profile query)
- **Dialog Display** (if needed): Instant (< 200ms)
- **Total Authentication Time**: 1-3 seconds (automatic) or 5-30 seconds (manual login)
- **Complete Startup**: 3-8 seconds (typical), < 10 seconds (target)

### Error Handling During Startup

**Database Connection Failure** (Critical):
- Retry 3 times with 5-second delays
- Show error dialog over splash screen: "Unable to connect to MySQL database server..."
- Provide guidance: Check network, wait and retry, contact IT
- Close application gracefully if all retries fail

**User Not Found + Dialog Cancelled** (Critical):
- Show confirmation: "User account is required. Close application?"
- Log event: "Account creation cancelled by user"
- Close application gracefully

**Invalid PIN After 3 Attempts** (Critical):
- Show error: "Maximum login attempts exceeded. Application closing for security."
- Log lockout event via `sp_LogUserActivity`
- Close application automatically

**User Account Inactive** (Critical):
- Show error: "User account is inactive. Contact administrator."
- Close application (no retry option)

### Reference Documentation

Detailed splash screen implementation, mockups, and technical specifications: [SplashScreen.md](../../SplashScreen.md)

---
## Clarifications

### Session 2025-12-15

- **Q**: How should concurrent logins from the same employee across different workstations be handled? → **A**: Allow concurrent sessions across different workstation types (one personal workstation + one shared terminal simultaneously). Prevents duplicate sessions on the same workstation type. Practical for supervisors accessing from office and shop floor.

- **Q**: How should 4-digit PINs be stored in the database? → **A**: Store in plain text as VARCHAR(4). Simpler implementation suitable for the security requirements of shop floor terminals with physical access controls.

- **Q**: What should the default session timeout be? → **A**: 30 minutes for personal workstations, 15 minutes for shared terminals. Balanced approach providing security on shared equipment while minimizing interruption for office users.

- **Q**: What authorization check should be performed for new user creation? → **A**: No credential check required. Physical presence of supervisor assumed as authorization. Windows username of creator is logged via `sp_LogUserActivity` for audit trail and accountability.

- **Q**: How should shared terminal names be detected and configured? → **A**: Store in database configuration table (`workstation_config`). Allows administrators to add new shared terminals without application redeployment. Application reads this table during startup step 40%.

- **Q**: How should Visual/Infor ERP credentials be stored in the database? → **A**: Store in plain text VARCHAR (matches PIN storage approach). Allows application to retrieve credentials for ERP connections. Simpler implementation suitable for trusted environment.

- **Q**: Where should logged-in user information be displayed in the main application? → **A**: Top-right corner of NavigationView header with format "John Smith (Emp #6229)". Standard location for user identity, easily visible but not intrusive.

- **Q**: Where should the Logout button be located? → **A**: No logout button needed. Users remain logged in until application closes. Session timeout will automatically close the application. For shared terminals, workers close and relaunch the app to switch users.

- **Q**: Should database connection errors offer manual retry after automatic retries fail? → **A**: Yes. Show error dialog with "Retry" and "Exit" buttons after 3 automatic retry attempts. Gives users control to retry if network issue was temporary without requiring full application restart.

- **Q**: What should happen when session times out due to inactivity? → **A**: Close application completely. User must relaunch application to log back in. Simpler and more secure than showing re-login dialog. Consistent with "no logout button" decision.

- **Q**: Should the Department field have a default value if left blank during user creation? → **A**: Department is a required field. Users must select a department during account creation. Ensures all users have proper department assignment for reporting and organization.

- **Q**: Should Department be a dropdown or free-text field? → **A**: Dropdown (ComboBox) with predefined department options (Receiving, Shipping, Production, Quality Control, Maintenance, Administration, Management) plus "Other" option that reveals a text field for custom departments. Ensures data consistency while allowing flexibility.

- **Q**: What user actions should reset the inactivity timer for session timeout? → **A**: Any user interaction resets the timer - mouse movement, keyboard input, or window focus changes. Standard practice for inactivity detection using WinUI 3's input events.

- **Q**: Should the splash screen progress bar animate while dialogs are displayed? → **A**: Yes, show pulsing/indeterminate animation at 45% while waiting for user input. Indicates system is alive and responsive, better UX than static frozen bar.

- **Q**: When shared terminal login fails 3 times, should there be a delay before closing? → **A**: Yes, show error message for 5 seconds then auto-close application. Gives users time to read "Maximum login attempts exceeded" security message before app closes.

**Database Design Decisions** (for new MySQL database):
- **Primary Key**: `employee_number` (INT, auto-increment) - used for label tracking and foreign key relationships
- **Windows Username**: Unique key (VARCHAR) - enables fast authentication lookups on personal workstations
- **Indexes**: Created on `windows_username`, `pin`, and `is_active` for query performance
- **PIN Storage**: Plain text VARCHAR(4) with uniqueness constraint
- **Session Tracking**: Managed in application memory (`Model_UserSession`), not database table

---
## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Automatic Windows Username Login (Priority: P1)

Office staff, supervisors, and managers with assigned personal computers need seamless access to the application. When they launch the application on their personal workstation, the system automatically detects their Windows username, retrieves their user profile from the database, and grants access without requiring manual login steps.

**Why this priority**: This is the most common authentication scenario for the majority of users. It provides frictionless access for authorized personnel while maintaining security through Windows authentication. This must work first before implementing more complex shared workstation scenarios.

**Independent Test**: Can be fully tested by launching the application on a personal workstation logged in with a Windows username that exists in the user database. The application should display the splash screen with authentication progress (40-50%), automatically load the user's profile, and transition to the main screen with their personalized settings. This delivers immediate value by establishing user identity with zero manual steps.

**Splash Screen Integration**:
- Application launch → Splash screen displays at 20%
- 40%: "Detecting workstation type..." → Personal workstation detected
- 45%: "Identifying user..." → Reads Windows username, queries database
- 50%: "Loading employee profile..." → Creates session, loads user data
- 55%: "Logged in as: [Full Name] (Employee #[number])" → Shows user info
- Main window appears, splash screen fades out
- **Total Time**: 3-5 seconds from launch to main window

**Acceptance Scenarios**:

1. **Given** the application is launched on a personal workstation, **When** the Windows username exists in the user database, **Then** the splash screen shows authentication progress (40-55%) and the system automatically retrieves the user's information and displays the main application screen with their name and settings
2. **Given** the application detects the Windows username, **When** the user's profile includes employee number, name, shift, and department, **Then** all this information is made available for label creation operations
3. **Given** a user is automatically logged in, **When** they navigate to any label entry screen (Receiving, Dunnage, or Routing), **Then** their employee number is automatically associated with any labels they create
4. **Given** a user is logged in via Windows authentication, **When** they view the application header or status bar, **Then** their full name and employee number are displayed
5. **Given** the application launches and detects a Windows username, **When** that username does NOT exist in the database, **Then** the system displays a new user creation dialog (see User Story 4)

---

### User Story 2 - Shared Workstation PIN Login (Priority: P2)

Production workers and material handlers using shared shop floor terminals need to identify themselves using a username and 4-digit PIN. The system detects shared workstations by their Windows computer names (e.g., SHOP2, MTMDC) and displays a login dialog requiring both username and PIN for authentication.

**Why this priority**: Critical for shop floor operations where multiple workers share terminals throughout shifts. Must be implemented after basic authentication (P1) but before optional features. Enables individual accountability on shared equipment.

**Independent Test**: Can be fully tested by launching the application on a workstation with a designated shared computer name, observing the splash screen pause at 45% with login dialog appearing over it, entering valid username and PIN credentials, and verifying that the splash screen resumes progress and the correct user's profile loads. This delivers value by enabling secure multi-user access on shared equipment.

**Splash Screen & Dialog Integration**:
- Application launch → Splash screen displays at 20%
- 40%: "Detecting workstation type..." → Shared terminal detected (SHOP2, MTMDC, etc.)
- 45%: "Waiting for user login..." → Splash screen pauses, login dialog appears (modal ContentDialog over splash)
- Dialog shows: Username field, 4-digit PIN field (masked), Login/Cancel buttons, "Attempt X of 3" after failures
- User enters credentials, clicks Login
- 45%: "Authenticating [Username]..." → Validates via `sp_ValidateUserPin`
- If valid: 50%: "Loading employee profile..." → Splash resumes, loads user data
- If invalid: Dialog shows error, PIN field clears, retry allowed (up to 3 attempts total)
- Main window appears after successful authentication
- **Total Time**: 5-30 seconds (depends on user input speed)

**Acceptance Scenarios**:

1. **Given** the application is launched on a workstation with a shared computer name (e.g., SHOP2, MTMDC), **When** the application starts and splash screen reaches 45%, **Then** a login dialog is displayed over the splash screen requiring username and 4-digit PIN
2. **Given** the login dialog is displayed, **When** a worker enters their valid username and correct 4-digit PIN, **Then** the system retrieves their profile and grants access to the application
3. **Given** a worker enters incorrect credentials, **When** they submit the login form, **Then** an error message is displayed and they are allowed up to 3 total attempts
4. **Given** a worker has failed login 3 times, **When** the third attempt fails, **Then** the application closes automatically to prevent unauthorized access
5. **Given** a worker is entering their PIN, **When** they type digits, **Then** the PIN field displays masked characters (dots or asterisks) instead of plain text
6. **Given** multiple workers use the same terminal throughout the day, **When** each worker logs in with their credentials, **Then** their individual transaction history and settings are maintained separately
7. **Given** a worker successfully logs in on a shared terminal, **When** they finish their work, **Then** they can log out to return to the login dialog for the next worker

---

### User Story 3 - Session Timeout Management (Priority: P3)

Users remain logged in for the duration of their session until the application closes or the session times out due to inactivity. For shared terminals, workers close the application when finished, allowing the next worker to launch and authenticate. Session timeouts (30 min for personal workstations, 15 min for shared terminals) automatically close the application if no activity is detected.

**Why this priority**: Simpler than explicit logout functionality while still providing security through automatic timeout. Can be tested after core authentication flows (P1, P2) are working. Enables proper session security without additional UI complexity.

**Independent Test**: Can be fully tested by logging in, leaving the application idle for the timeout period, and verifying that the application closes automatically and logs the timeout event. This delivers value by providing automatic security without requiring users to remember to log out.

**Acceptance Scenarios**:

1. **Given** a user is logged into the application on a personal workstation, **When** the application is idle for 30 minutes, **Then** the session automatically times out, the application closes, and a timeout event is logged via `sp_LogUserActivity`
2. **Given** a user is logged into the application on a shared terminal, **When** the application is idle for 15 minutes, **Then** the session automatically times out, the application closes, and a timeout event is logged
3. **Given** a user's session has timed out, **When** they relaunch the application, **Then** they must authenticate again (automatic Windows detection for personal workstations, or PIN login for shared terminals)
4. **Given** a user on a personal workstation relaunches after timeout, **When** the Windows username is detected, **Then** they are automatically re-authenticated if still in the database
5. **Given** multiple workers need to use a shared terminal, **When** one worker finishes their work, **Then** they close the application and the next worker launches the app to authenticate with their own credentials
6. **Given** a user closes the application manually, **When** the application closes, **Then** the session ends and a logout event is logged (if needed for audit trail)

---

### User Story 4 - New User Creation on First Access (Priority: P4)

When a Windows user who is not in the database launches the application for the first time, or when a new employee needs to be registered, the system displays a quick user creation dialog. A supervisor or administrator can immediately create the user profile by entering their full name, assigning a 4-digit PIN for shared workstation access, and specifying their shift.

**Why this priority**: Streamlines employee onboarding and handles the edge case of new users, but can be implemented after core authentication flows are stable. Eliminates need for separate user administration before first use.

**Independent Test**: Can be fully tested by launching the application with a Windows username that does not exist in the database, observing the splash screen pause at 45% with "New User Setup" dialog appearing over it, filling out the form with supervisor assistance, and verifying that the splash screen resumes progress after successful account creation and the new user can immediately access the application. This delivers value by enabling instant user provisioning.

**Splash Screen & Dialog Integration**:
- Application launch → Splash screen displays at 20%
- 40%: "Detecting workstation type..." → Personal workstation detected
- 45%: "Identifying user..." → Reads Windows username, queries `sp_GetUserByWindowsUsername` → Returns NULL (not found)
- 45%: "New user detected: [Windows Username]" → Splash screen pauses
- 45%: "Waiting for account creation..." → New User Setup dialog appears (modal ContentDialog over splash)
- Dialog shows: Full Name, Windows Username (auto-filled, read-only), Shift dropdown, 4-Digit PIN, Confirm PIN, Department (optional), InfoBar with notes
- Supervisor fills form, clicks "Create Account"
- 47%: "Creating employee account..." → Calls `sp_CreateNewUser`, shows progress bar in dialog
- 50%: "Employee account created. Welcome, [Full Name]!" → Success message displayed
- 50%: "Loading employee profile..." → Splash resumes, creates session with new user
- 55%: "Welcome, [Full Name]! Employee #[number]" → Shows employee number
- Main window appears, splash screen fades out
- **Total Time**: 1-3 minutes (depends on form completion speed)

**Dialog Specifications** (WinUI 3 ContentDialog):
- **Width**: 600px, **Height**: Auto-sized based on content
- **Fields**: Full Name (TextBox, required), Windows Username (TextBox, read-only), Shift (ComboBox: 1st/2nd/3rd, required), PIN (PasswordBox, 4 digits, masked, required), Confirm PIN (PasswordBox, must match, required), Department (TextBox, optional)
- **Validation**: Inline errors with red borders, InfoBar at top for error summary
- **Buttons**: "Create Account" (Primary, Accent style) and "Cancel" (Secondary, shows confirmation)
- **Loading State**: Progress bar, disabled controls, "Creating..." button text during submission
- **Success**: Shows success InfoBar with employee details, then closes and resumes startup
- **Error**: Shows error InfoBar (e.g., "PIN already in use", "Database connection failed"), allows retry
- **Cancel Confirmation**: "Are you sure? User account is required. Application will close."

**Acceptance Scenarios**:

1. **Given** the application is launched with a Windows username, **When** that username does not exist in the user database and splash screen reaches 45%, **Then** a "New User Setup" dialog is displayed automatically over the splash screen with the Windows username pre-filled
2. **Given** the user creation dialog is displayed, **When** a supervisor enters the full name, 4-digit PIN, and shift information, **Then** the system creates a new user record and immediately grants access
3. **Given** a new user is being created, **When** the supervisor enters a PIN that is already in use by another user, **Then** an error message is displayed requesting a unique PIN
4. **Given** a new user account is created, **When** the user later logs into a shared terminal with their assigned username and PIN, **Then** they can successfully authenticate
5. **Given** a new user is created, **When** their profile is stored in the database, **Then** it includes their Windows username, full name, PIN, shift, and account creation timestamp

---

### User Story 5 - Optional Visual/Infor ERP Integration Credentials (Priority: P5)

Managers and authorized users who need access to enterprise-level manufacturing data (inventory levels, receiving records, shipping data) can configure their personal Visual/Infor ERP credentials within their user profile. This enables seamless access to ERP data from within the application without requiring separate login steps.

**Why this priority**: This is an optional enhancement for users who need ERP integration. Core label creation features work without it. Can be implemented after all authentication flows are complete.

**Independent Test**: Can be fully tested by navigating to user settings, entering Visual/Infor ERP credentials, saving them, and then attempting to access ERP-integrated features to verify the credentials are used for connection. This delivers value by enabling integrated ERP data access for authorized users.

**Acceptance Scenarios**:

1. **Given** a user is logged into the application, **When** they open User Management settings, **Then** they can enter and save their personal Visual/Infor ERP username and password
2. **Given** a user has not configured ERP credentials, **When** they attempt to access ERP-integrated features, **Then** those menu options are disabled or hidden with a message indicating credentials are required
3. **Given** a user has configured ERP credentials, **When** they access ERP-integrated features, **Then** the application uses their stored credentials to connect to the ERP system
4. **Given** a user's ERP credentials are stored, **When** they are displayed in settings, **Then** the password is masked and never shown in plain text
5. **Given** a user enters incorrect ERP credentials, **When** they attempt to access ERP features, **Then** an error message indicates authentication failed and directs them to verify their credentials
6. **Given** a user has ERP access configured, **When** they retrieve data from Visual/Infor, **Then** the application has read-only access and cannot modify ERP data

---

### Edge Cases

- What happens when the database connection is lost during Windows username lookup or PIN validation?
- How does the system detect whether a workstation is a shared terminal (SHOP2, MTMDC) versus a personal computer?
- What if a user's Windows username exists but their database record is marked as inactive or terminated?
- How does the system handle concurrent logins from the same employee on different workstations (personal computer and shared terminal simultaneously)?
- What happens if the database user table is empty on first application launch?
- How does the system handle Windows usernames with special characters, spaces, or domain prefixes (DOMAIN\username)?
- What if a shared terminal's Windows computer name changes and is no longer recognized as a shared workstation?
- How does the system handle a 4-digit PIN that is not numeric or contains fewer/more than 4 digits?
- What happens if a supervisor attempts to create a new user but enters invalid data (missing name, duplicate username, invalid shift)?
- How does the system prevent brute force PIN attempts beyond the 3-failure lockout?
- What if a user forgets their PIN for shared workstation access?
- How does the system handle Visual/Infor ERP credentials that expire or become invalid after being stored?
- What happens when attempting to access ERP features but the Visual/Infor server is unreachable?

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

**Startup & Splash Screen**:
- **FR-001**: System MUST display a splash screen immediately after application launch showing MTM branding, version number, and progress indicator
- **FR-002**: System MUST report authentication progress to splash screen using percentage (0-100%) and status messages
- **FR-003**: System MUST execute authentication between 40-60% of startup sequence progress
- **FR-004**: System MUST pause splash screen progress and display modal dialogs over the splash screen for user input (login, account creation)
- **FR-005**: System MUST resume splash screen progress after authentication dialogs are completed or dismissed
- **FR-006**: System MUST hide splash screen and display main window only after successful authentication completes
- **FR-007**: System MUST display error dialogs over splash screen for critical authentication failures before closing application
- **FR-008A**: System MUST show pulsing/indeterminate progress animation on splash screen while paused at 45% waiting for user input in authentication dialogs

**Authentication & Access Control**:
- **FR-009**: System MUST detect the Windows username when the application launches (during startup step 40%)
- **FR-010**: System MUST determine if the workstation is a personal computer or shared terminal by querying the `workstation_config` database table with the Windows computer name (during startup step 40%)
- **FR-011**: System MUST automatically authenticate users on personal workstations by validating their Windows username against the database (during startup step 45%)
- **FR-012**: System MUST display a login dialog (WinUI 3 ContentDialog) over splash screen on shared terminals requiring username and 4-digit PIN (pauses startup at 45%)
- **FR-013**: System MUST validate username and PIN combinations against the database for shared terminal authentication via `sp_ValidateUserPin` stored procedure
- **FR-014**: System MUST enforce a maximum of 3 failed login attempts, then show error message for 5 seconds before automatically closing the application on shared terminals
- **FR-015**: System MUST mask PIN entry by displaying dots or asterisks using PasswordBox control instead of plain text digits
- **FR-016**: System MUST display a new user creation dialog (WinUI 3 ContentDialog) over splash screen when a Windows username is not found in the database (pauses startup at 45%)
- **FR-017**: System MUST allow new user creation via the New User Setup dialog without credential verification (physical presence assumed), logging the Windows username of the creator via `sp_LogUserActivity` for audit purposes
- **FR-018**: System MUST require department selection during new user creation via dropdown populated from `departments` table, with "Other" option revealing text field for custom departments
- **FR-019**: System MUST validate that PINs are exactly 4 numeric digits and are unique across all users before account creation, storing PINs in plain text VARCHAR(4) format

**Session Management**:
- **FR-018**: System MUST retrieve and store user information (Windows username, full name, employee number, department, shift, PIN) upon successful authentication (during startup step 50%)
- **FR-019**: System MUST create a `Model_UserSession` object and make the logged-in user's information accessible throughout the application session
- **FR-020**: System MUST automatically associate the logged-in user's employee number with all labels they create (Receiving, Dunnage, Routing)
- **FR-021**: System MUST display the logged-in user's full name and employee number in the top-right corner of the NavigationView header with format "[Full Name] (Emp #[employee_number])" after main window loads
- **FR-022**: System MUST NOT provide a logout button or menu option (users close application to end session)
- **FR-023**: System MUST clear all session data when the application closes or session times out
- **FR-024**: System MUST allow concurrent sessions for the same user across different workstation types (one personal + one shared terminal), but prevent duplicate sessions on the same workstation type
- **FR-025**: System MUST automatically close the application after inactivity (30 minutes for personal workstations, 15 minutes for shared terminals), where any user interaction (mouse movement, keyboard input, window focus) resets the timer, logging a timeout event via `sp_LogUserActivity`

**Visual/Infor ERP Integration** (Optional):
- **FR-028**: System MUST check if user has configured Visual/Infor ERP credentials during startup step 55%
- **FR-029**: System MUST provide a user settings interface where authorized users can enter and save their Visual/Infor ERP credentials
- **FR-030**: System MUST store ERP passwords in plain text (masked in UI) and never display them unmasked in user interface
- **FR-031**: System MUST disable or hide ERP-integrated menu options for users without configured ERP credentials
- **FR-032**: System MUST use user-specific ERP credentials when connecting to Visual/Infor databases
- **FR-033**: System MUST enforce read-only access to ERP data (no modification capabilities)
- **FR-034**: System MUST show appropriate error messages when ERP credentials are invalid or the ERP server is unreachable

**Error Handling & Auditing**:
- **FR-035**: System MUST show error dialogs (WinUI 3 ContentDialog) over splash screen for invalid credentials, database connection failures, inactive accounts, or workstation detection issues
- **FR-036**: System MUST handle database connection errors gracefully during authentication with 3 automatic retry attempts (5-second delays), then display error dialog with "Retry" and "Exit" buttons allowing manual retry
- **FR-037**: System MUST display error details including server address, database name, and suggested troubleshooting steps
- **FR-038**: System MUST log all login events (successful and failed) with timestamp, username, and workstation identifier via `sp_LogUserActivity`
- **FR-039**: System MUST log all session timeouts and application closures via `sp_LogUserActivity`
- **FR-040**: System MUST log all new user creation events with timestamp and the Windows username of the creator via `sp_LogUserActivity`
- **FR-041**: System MUST log all Visual/Infor ERP connection attempts (successful and failed) via `sp_LogUserActivity`
- **FR-042**: System MUST write all authentication errors to application log file with full stack traces for debugging

### Key Entities

- **User/Employee**: Represents an employee who uses the application. Key attributes include Windows username (links to computer login), employee number (unique identifier used for label tracking), full name (display name), 4-digit PIN (for shared workstation access), department, shift designation (1st, 2nd, 3rd), active status, and optional Visual/Infor ERP credentials (username and password). This entity is stored in the database and retrieved during login.
- **Session**: Represents the current user's active session within the application. Contains the logged-in employee's complete information, authentication method (Windows auto-login or PIN), workstation identifier (computer name), login timestamp, and last activity timestamp. Makes user information accessible to all features requiring employee identification.
- **Workstation**: Represents a physical computer where the application runs. Key attributes include Windows computer name (used to determine if shared terminal or personal workstation), list of designated shared terminal names (e.g., SHOP2, MTMDC), and current session information. Used to determine which authentication flow to use.
- **Visual/ERP Integration**: Represents the optional connection to the Visual/Infor ERP system. Key attributes include user-specific credentials, connection status, read-only access permissions, and available data categories (inventory, receiving, shipping, work orders). Only applicable to users who have configured ERP access.

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: Splash screen displays within 500ms of application launch showing MTM branding and 0% progress
- **SC-002**: Users on personal workstations can access the application in under 5 seconds with automatic Windows username authentication, with splash screen showing progress from 0% to 100% and transitioning smoothly to main window
- **SC-003**: Authentication phase (steps 40-60%) completes in under 3 seconds for automatic Windows authentication
- **SC-004**: Users on shared terminals see the login dialog appear over the splash screen within 2 seconds of launch, paused at 45% progress
- **SC-005**: Splash screen progress updates are visible and smooth, updating every 200-500ms during authentication steps
- **SC-006**: Modal dialogs (login, new user creation) appear centered over the splash screen within 200ms of trigger
- **SC-007**: Splash screen remains visible throughout authentication, never freezes, and shows appropriate status messages
- **SC-008**: 100% of label creation operations correctly capture and store the logged-in user's employee number regardless of authentication method
- **SC-009**: Error dialogs displayed over splash screen clearly indicate the specific problem (invalid credentials, database connection error, inactive account, workstation detection issue) within 2 seconds of detection
- **SC-010**: The application prevents any label creation or data entry operations until a valid user is authenticated and main window is displayed
- **SC-011**: All authentication events (successful logins, failed attempts, new user creation, session timeouts, application closures) are recorded in application logs with timestamps and workstation identifiers
- **SC-012**: Failed login attempts on shared terminals are limited to 3 attempts with automatic application closure on the 3rd failure, with error message displayed over splash screen
- **SC-013**: New user creation process completes in under 2 minutes from dialog display to successful first access, with progress shown in dialog and splash screen
- **SC-014**: Users with configured Visual/Infor ERP credentials can access integrated ERP data within 10 seconds of menu selection
- **SC-015**: 100% of PINs are masked during entry (never displayed in plain text) using PasswordBox control, and ERP passwords are never displayed in user settings
- **SC-016**: Total application startup time from launch to main window display is under 10 seconds for all authentication scenarios

## Assumptions

- Employee numbers are numeric identifiers (integer format) as evidenced by existing `EmployeeNumber` properties in the codebase being of type `int`
- The application operates in a Windows desktop environment with local or network database connectivity
- The database has or will have a user/employee table structure with fields for Windows username, employee number, full name, 4-digit PIN, shift, department, active status, and optional ERP credentials
- Windows username detection is reliable and returns the currently logged-in Windows user
- Shared workstation computer names are stored in the `workstation_config` database table, allowing flexible management without application redeployment
- The application is deployed in a trusted environment where Windows authentication for personal workstations is acceptable (office and manufacturing floor)
- PIN-only authentication (without passwords) is acceptable for shared shop floor terminals due to physical security controls
- PINs are exactly 4 numeric digits stored in plain text for ease of entry on shop floor terminals (physical security controls adequate for shop floor environment)
- Only one user can be logged in per application instance at a time, and the same user can have concurrent sessions across different workstation types (one personal workstation + one shared terminal) but not duplicate sessions on the same type
- The existing `Model_Application_Variables` class will be extended or a new session management class will be created to store logged-in user information
- Visual/Infor ERP integration is optional and only configured for users who require access to ERP data
- Visual/Infor ERP credentials are user-specific (no shared accounts) and follow company IT security policies
- The Visual/Infor ERP system provides read-only database access for integration queries
- Session timeout is 30 minutes for personal workstations and 15 minutes for shared terminals to balance security with user productivity
- Administrators/supervisors have the authority to create new user accounts and reset PINs, with physical presence assumed as authorization (no credential verification required, audit logging provides accountability)
- Network connectivity to the database is generally reliable, but the application handles disconnections gracefully

## Dependencies

- **Phase 1 Infrastructure**: Complete - base MVVM architecture, services, and database helpers are in place

- **Splash Screen Components**:
  - New splash screen window/page (WinUI 3) displaying MTM logo, version, progress bar, status text
  - Progress reporting mechanism using `IProgress<(int percentage, string message)>` pattern
  - Async startup orchestrator to coordinate initialization sequence
  - ContentDialog infrastructure for modal dialogs over splash screen
  - Dispatcher/UI thread marshalling for progress updates from background threads
  - Reference: [SplashScreen.md](../../SplashScreen.md) for complete specifications

- **Database Tables**: 
  - New `users` table with fields:
    - `employee_number` (INT, PRIMARY KEY, AUTO_INCREMENT) - used for label tracking and foreign keys
    - `windows_username` (VARCHAR(50), UNIQUE, NOT NULL, INDEX) - links to Windows login
    - `full_name` (VARCHAR(100), NOT NULL) - display name
    - `pin` (VARCHAR(4), UNIQUE, NOT NULL, INDEX) - plain text for shared workstation access
    - `department` (VARCHAR(50), NOT NULL) - required department assignment
    - `shift` (ENUM('1st Shift', '2nd Shift', '3rd Shift'), NOT NULL) - shift designation
    - `is_active` (BOOLEAN, DEFAULT TRUE, INDEX) - account active status
    - `visual_username` (VARCHAR(50), NULL) - optional ERP integration username (plain text)
    - `visual_password` (VARCHAR(100), NULL) - optional ERP integration password (plain text, masked in UI)
    - `created_date` (DATETIME, DEFAULT CURRENT_TIMESTAMP) - account creation timestamp
    - `created_by` (VARCHAR(50), NULL) - Windows username of account creator for audit
    - `modified_date` (DATETIME, DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP) - last modification timestamp
  - Indexes: PRIMARY KEY (`employee_number`), UNIQUE KEY (`windows_username`), UNIQUE KEY (`pin`), INDEX (`is_active`)
  - New `workstation_config` table for shared terminal detection:
    - `config_id` (INT, PRIMARY KEY, AUTO_INCREMENT)
    - `workstation_name` (VARCHAR(50), UNIQUE, NOT NULL) - computer name (e.g., SHOP2, MTMDC)
    - `workstation_type` (ENUM('shared_terminal', 'personal_workstation'), NOT NULL)
    - `is_active` (BOOLEAN, DEFAULT TRUE)
    - `description` (VARCHAR(200), NULL) - optional notes
    - `created_date` (DATETIME, DEFAULT CURRENT_TIMESTAMP)
    - `modified_date` (DATETIME, DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP)
  - New `departments` table for department dropdown options:
    - `department_id` (INT, PRIMARY KEY, AUTO_INCREMENT)
    - `department_name` (VARCHAR(50), UNIQUE, NOT NULL) - display name in dropdown
    - `is_active` (BOOLEAN, DEFAULT TRUE) - allows hiding departments without deleting
    - `sort_order` (INT, DEFAULT 999) - controls order in dropdown
    - `created_date` (DATETIME, DEFAULT CURRENT_TIMESTAMP)
  - Initial departments: Receiving, Shipping, Production, Quality Control, Maintenance, Administration, Management
  - New `user_activity_log` table for audit trail with fields:
    - `log_id` (INT, PK, auto-increment)
    - `event_type` (VARCHAR: login_success, login_failed, logout, session_timeout, user_created, pin_reset)
    - `username` (VARCHAR, references users table)
    - `workstation_name` (VARCHAR, Windows computer name)
    - `event_timestamp` (DATETIME)
    - `details` (TEXT, additional context like error messages)
- **Stored Procedures**: 
  - `sp_GetUserByWindowsUsername` - retrieves user by Windows username for auto-login
  - `sp_ValidateUserPin` - validates username + PIN combination for shared terminals
  - `sp_CreateNewUser` - inserts new user record with validation
  - `sp_LogUserActivity` - logs authentication events (login_success, login_failed, session_timeout, user_created) for audit trail
  - `sp_GetSharedTerminalNames` - retrieves list of shared terminal computer names from workstation_config table
  - `sp_GetDepartments` - retrieves active departments ordered by sort_order for dropdown population
  - `sp_UpdateUserErpCredentials` - securely stores Visual/Infor ERP credentials
  - `sp_GetUserErpCredentials` - retrieves encrypted ERP credentials for connection
  - `sp_CheckUserActiveStatus` - verifies user account is active before granting access
  - `sp_ResetUserPin` - allows administrators to reset forgotten PINs
- **Services**: 
  - Existing `IService_ErrorHandler` for displaying login errors and authentication failures
  - Existing `ILoggingService` for audit trail and activity logging
  - New `IService_Authentication` or `IService_SessionManager` for managing user sessions
  - New `IService_WindowsIdentity` or equivalent for detecting Windows username and computer name
  - New `IService_VisualErpConnection` (optional) for managing ERP system connections
- **Models**:
  - New `Model_User` or `Model_Employee` for user data structure including Windows username, employee number, full name, PIN, shift, department, active status, and ERP credentials
  - New `Model_UserSession` for active session management including authentication method, login timestamp, workstation identifier, and last activity
  - New `Model_WorkstationConfig` for storing list of shared terminal names and workstation detection logic
  - Extension to `Model_Application_Variables` or new session management model to track current logged-in user
  - New `Model_VisualErpConnection` (optional) for ERP integration settings

- **Views & Dialogs** (WinUI 3 ContentDialog):
  - `Views/Shared/SplashScreenWindow.xaml` - Splash screen with progress bar and status text
  - `Views/Shared/SharedTerminalLoginDialog.xaml` - PIN login dialog for shared terminals
  - `Views/Shared/NewUserSetupDialog.xaml` - New user creation dialog with form fields
  - ViewModels for each dialog to handle validation, data binding, and async operations
  - See [SplashScreen.md](../../SplashScreen.md) for complete mockups and specifications

- **Configuration Files**:
  - Application settings file or database configuration table listing shared workstation names (SHOP2, MTMDC, etc.)
  - Session timeout duration configuration
  - Maximum failed login attempts configuration (default: 3)
- **Other Features**: None - this is a foundational feature that other features will depend on

## Visual Design & User Experience

### Main Window Header Design
- **User Display**: Top-right corner of NavigationView header
- **Format**: "[Full Name] (Emp #[employee_number])" - e.g., "John Smith (Emp #6229)"
- **Font**: System default, readable size (14-16pt)
- **Color**: Matches theme (high contrast in all themes)
- **No Logout Button**: Users close application to end session
- **Interaction**: Non-interactive text display (no click dropdown)

### Splash Screen Design
- **Branding**: MTM logo centered at top, "MTM Receiving Label Application" title, version number
- **Progress Bar**: Horizontal bar showing 0-100% completion with smooth animations
- **Progress Animation During Dialog Wait**: Pulsing/indeterminate animation at 45% while waiting for user input (login or new user creation)
- **Status Text**: Large, readable text below progress bar showing current step (e.g., "Authenticating user...", "Waiting for login...", "Waiting for account creation...")
- **Dimensions**: 600px × 400px (minimum), centered on screen
- **Theme**: Matches application theme (Light/Dark/High Contrast)
- **Animation**: Fade in on launch (300ms), fade out after completion (300ms)

### Shared Terminal Login Dialog
- **Layout**: Vertical stack - Title, Username field, PIN field, attempt counter, Login/Cancel buttons
- **Title**: "Shared Terminal Login" with workstation name (e.g., "SHOP2")
- **Username Field**: TextBox, plain text, placeholder: "Enter your username"
- **PIN Field**: PasswordBox, 4-digit masked input (●●●●), placeholder: "Enter 4-digit PIN"
- **Attempt Counter**: Only shown after first failure: "Attempt 2 of 3" in warning color
- **Buttons**: "Login" (Primary, Accent), "Cancel" (Secondary)
- **Dimensions**: 450px × 350px, centered over splash screen
- **Keyboard**: Enter submits, Escape cancels, Tab navigates fields

### New User Setup Dialog
- **Layout**: Vertical stack with grouped sections - Header, Employee Info fields, Important Notes, buttons
- **Header**: "New Employee Setup" title, explanation text with detected Windows username
- **Fields**: Full Name (TextBox), Windows Username (TextBox, read-only), Shift (ComboBox), PIN (PasswordBox), Confirm PIN (PasswordBox), Department (TextBox, optional)
- **Validation**: Inline errors with red borders, InfoBar at top for error summary
- **Important Notes**: InfoBar (Informational severity) with bullets about auto-generated employee number, PIN uniqueness
- **Buttons**: "Create Account" (Primary, Accent), "Cancel" (Secondary with confirmation)
- **Dimensions**: 600px × 700px, centered over splash screen
- **Loading State**: Progress bar replaces form during submission, "Creating employee account..."
- **Success State**: InfoBar (Success severity) with employee details before closing

### Error Dialogs
- **Layout**: Title, error icon (⚠️ or ❌), message text, detailed explanation, action buttons
- **Database Error**: Title: "Connection Error", icon: ⚠️, message: "Unable to connect to MySQL database after 3 attempts", details include server/database/port, buttons: "Retry" / "Exit" (manual retry option after automatic retries fail)
- **Invalid Credentials**: Title: "Authentication Failed", icon: ⚠️, message: "Invalid username or PIN", attempt counter if applicable, buttons: "Try Again" / "Cancel"
- **User Inactive**: Title: "Account Inactive", icon: ❌, message: "Your account is inactive. Contact administrator.", button: "OK" (closes app)
- **Lockout**: Title: "Maximum Attempts Exceeded", icon: ❌, message: "Maximum login attempts exceeded. Application closing for security.", countdown timer showing "Closing in 5... 4... 3...", no buttons (auto-closes after 5 seconds)
- **Dimensions**: 500px × 300px, centered over splash screen

### Accessibility Requirements
- **Screen Reader**: All dialogs and splash screen announce title and status changes
- **Keyboard Navigation**: Full keyboard support, visible focus indicators, logical tab order
- **High Contrast**: All UI elements visible in Windows High Contrast modes
- **Text Size**: All text scales appropriately with Windows display scaling (96-192 DPI)
- **Color Independence**: Errors indicated by icon and text, not just color
- **AutomationProperties**: All controls have appropriate Name, HelpText, and Role properties

### Performance & Responsiveness
- **Splash Screen Display**: < 500ms from application launch
- **Dialog Display**: < 200ms from authentication trigger
- **Progress Updates**: Every 200-500ms during initialization
- **Animation Frame Rate**: 60 FPS minimum for smooth transitions
- **UI Thread**: Never blocked for more than 100ms (all long operations on background threads)
- **Database Queries**: Timeout after 10 seconds per query, 3 retry attempts with 5-second delays

---

## Testing Considerations

### Manual Testing Scenarios

**Test 1: Personal Workstation - Existing User**
1. Launch application on personal workstation with valid Windows username
2. Observe splash screen displays, progresses through 40-50%
3. Verify automatic authentication completes without dialogs
4. Confirm main window displays with user info in header
5. **Expected**: Smooth transition, 3-5 seconds total, no errors

**Test 2: Personal Workstation - New User**
1. Launch application with Windows username not in database
2. Observe splash screen pauses at 45%, New User Setup dialog appears
3. Fill all required fields, click Create Account
4. Verify splash screen resumes, main window appears
5. **Expected**: User created successfully, under 2 minutes total

**Test 3: Shared Terminal - Valid Credentials**
1. Launch application on shared terminal (SHOP2, MTMDC)
2. Observe splash screen pauses at 45%, login dialog appears
3. Enter valid username and PIN, click Login
4. Verify splash screen resumes, main window appears
5. **Expected**: Authentication successful, 10-20 seconds total

**Test 4: Shared Terminal - Invalid Credentials (3 Attempts)**
1. Launch application on shared terminal
2. Enter incorrect PIN, observe error message
3. Retry with incorrect PIN (attempt 2)
4. Retry with incorrect PIN (attempt 3)
5. **Expected**: Error message after each attempt, application closes after 3rd failure

**Test 5: Database Connection Failure**
1. Stop MySQL database server
2. Launch application
3. Observe splash screen shows "Connecting to database...", retries 3 times
4. Verify error dialog appears over splash screen
5. **Expected**: Clear error message with retry guidance, application closes gracefully

**Test 6: Dialog Cancellation**
1. Launch application, trigger login or new user dialog
2. Click Cancel button
3. Observe confirmation dialog appears
4. Confirm cancellation
5. **Expected**: Application closes gracefully, no crash

### Automated Testing

**Unit Tests**:
- Windows username detection (Environment.UserName)
- Computer name detection and shared terminal pattern matching
- PIN validation (4 digits, numeric only, uniqueness)
- Session creation and data storage
- Error message generation

**Integration Tests**:
- Database queries (sp_GetUserByWindowsUsername, sp_ValidateUserPin, sp_CreateNewUser)
- Authentication flow with mock database
- Dialog display and data binding
- Progress reporting from background threads
- Logging via sp_LogUserActivity

**UI Tests**:
- Splash screen visibility and progress updates
- Dialog appearance over splash screen
- Keyboard navigation and accessibility
- Error dialog display and dismiss
- Theme inheritance and high contrast support

### Performance Testing

**Startup Time Benchmarks**:
- Personal workstation (existing user): Target < 5 seconds, Max 8 seconds
- Personal workstation (new user): Target < 2 minutes (depends on form completion)
- Shared terminal (valid credentials): Target < 15 seconds (depends on user input)
- Database connection retry: 30 seconds maximum (3 attempts × 10 seconds)

**Progress Reporting**:
- Splash screen updates every 200-500ms
- No UI freezes or hangs visible to user
- Smooth progress bar animation (60 FPS)

**Dialog Performance**:
- Dialog display: < 200ms from trigger
- Validation feedback: < 100ms from input
- Database submission: < 5 seconds with loading indicator

---

## Out of Scope

- Explicit logout button or menu option (users close application to end session)
- Password-based authentication for primary application access (Windows authentication and PINs are used instead)
- Multi-factor authentication (MFA) for application login
- Role-based access control (RBAC) or granular permission systems (users either have access or they don't)
- User profile editing by end users (name, shift, department changes require administrator intervention)
- Self-service employee registration (new users must be created by supervisors/administrators)
- Integration with external authentication systems (Active Directory, LDAP, OAuth) beyond Windows username detection
- Single sign-on (SSO) across multiple applications
- Biometric authentication (fingerprint, facial recognition, badge scanning)
- Email/SMS notification for login events or security alerts
- Forgot PIN recovery by end users (PIN resets must be performed by administrators)
- Password strength requirements or expiration policies for Visual/Infor ERP credentials (managed by ERP system)
- User activity tracking beyond authentication events (detailed audit trails of label creation, modifications, etc. are separate features)
- Workstation locking or screen saver integration
- Automatic computer name detection configuration UI (shared terminal names are configured via database or settings file)
- User provisioning from external HR systems or employee directories
- Geolocation or IP-based access restrictions
- Concurrent session limits beyond preventing duplicate active sessions for the same user on the same workstation type
- Visual/Infor ERP write access or data modification capabilities (read-only access only)
- Integration with ERP systems other than Visual/Infor

# Tasks: User Authentication & Login System

**Feature**: User Authentication & Login System  
**Branch**: `001-user-login`  
**Generated**: December 15, 2025  
**Status**: Ready for implementation

## Task Format Legend

```
- [ ] [TaskID] [P?] [Story?] Description with file path
```

- **[TaskID]**: Sequential task number (T001, T002, etc.)
- **[P]**: Parallelizable (can be done simultaneously with other [P] tasks)
- **[Story]**: User story label ([US1], [US2], etc.) - for user story phases only
- **Description**: Clear action with specific file path

## Implementation Strategy

**Approach**: MVP-first, incremental delivery by user story priority

1. **Setup Phase**: Database and infrastructure (blocking prerequisites)
2. **Foundational Phase**: Core models, services, and DAOs (shared across all stories)
3. **User Story Phases**: Implement each story in priority order (P1 → P5)
4. **Polish Phase**: Cross-cutting concerns, performance, documentation

Each user story is independently testable and delivers incremental value.

---

## Phase 1: Setup (Database Foundation)

**Goal**: Create database schema and stored procedures - blocking prerequisite for all user stories

**Test Criteria**: Can manually execute all stored procedures and verify table structure

### Tasks

- [ ] T001 Create database schema script in Database/Schemas/02_create_authentication_tables.sql
- [ ] T002 [P] Define users table with all fields (employee_number PK, windows_username UK, pin UK, etc.) in schema script
- [ ] T003 [P] Define workstation_config table with computer name and type in schema script
- [ ] T004 [P] Define departments table with name, sort_order, is_active in schema script
- [ ] T005 [P] Define user_activity_log table with event tracking fields in schema script
- [ ] T006 [P] Add indexes to users table (windows_username, pin, is_active) in schema script
- [ ] T007 Insert initial department data (Receiving, Shipping, Production, Quality Control, Maintenance, Administration, Management) in schema script
- [ ] T008 Insert sample workstation configuration data (SHOP2, MTMDC as shared terminals) in schema script
- [ ] T009 Create sp_GetUserByWindowsUsername stored procedure in Database/StoredProcedures/Authentication/sp_GetUserByWindowsUsername.sql
- [ ] T010 [P] Create sp_ValidateUserPin stored procedure in Database/StoredProcedures/Authentication/sp_ValidateUserPin.sql
- [ ] T011 [P] Create sp_CreateNewUser stored procedure in Database/StoredProcedures/Authentication/sp_CreateNewUser.sql
- [ ] T012 [P] Create sp_LogUserActivity stored procedure in Database/StoredProcedures/Authentication/sp_LogUserActivity.sql
- [ ] T013 [P] Create sp_GetSharedTerminalNames stored procedure in Database/StoredProcedures/Authentication/sp_GetSharedTerminalNames.sql
- [ ] T014 [P] Create sp_GetDepartments stored procedure in Database/StoredProcedures/Authentication/sp_GetDepartments.sql
- [ ] T015 Run schema script on development database and verify table creation
- [ ] T016 Deploy all stored procedures to development database and verify creation
- [ ] T017 Insert test user data (at least 3 users with different departments/shifts)
- [ ] T018 Manually test each stored procedure with test data and verify results

**Phase 1 Complete**: ✅ Database ready for application integration

---

## Phase 2: Foundational (Core Models, Services, DAOs)

**Goal**: Implement shared infrastructure used by all user stories

**Test Criteria**: Unit tests pass for all DAOs and services; can instantiate models with valid data

### Tasks - Models

- [ ] T019 [P] Create Model_User class with all database fields as properties in Models/Model_User.cs
- [ ] T020 [P] Add DisplayName computed property to Model_User (format: "John Smith (Emp #6229)")
- [ ] T021 [P] Add HasErpAccess computed property to Model_User
- [ ] T022 [P] Create Model_UserSession class with User, workstation metadata, timestamps in Models/Model_UserSession.cs
- [ ] T023 [P] Add timeout tracking properties (TimeoutDuration, TimeSinceLastActivity, IsTimedOut) to Model_UserSession
- [ ] T024 [P] Add UpdateLastActivity() method to Model_UserSession
- [ ] T025 [P] Create Model_WorkstationConfig class with workstation properties in Models/Model_WorkstationConfig.cs
- [ ] T026 [P] Add IsSharedTerminal and IsPersonalWorkstation computed properties to Model_WorkstationConfig
- [ ] T027 [P] Add static DetectCurrentWorkstation() method signature to Model_WorkstationConfig

### Tasks - Data Access Layer (DAO)

- [ ] T028 Create Dao_User class in Data/Authentication/Dao_User.cs
- [ ] T029 [P] Implement GetUserByWindowsUsernameAsync method in Dao_User using sp_GetUserByWindowsUsername
- [ ] T030 [P] Implement ValidateUserPinAsync method in Dao_User using sp_ValidateUserPin
- [ ] T031 [P] Implement CreateNewUserAsync method in Dao_User using sp_CreateNewUser
- [ ] T032 [P] Implement IsPinUniqueAsync validation method in Dao_User
- [ ] T033 [P] Implement IsWindowsUsernameUniqueAsync validation method in Dao_User
- [ ] T034 [P] Implement LogUserActivityAsync method in Dao_User using sp_LogUserActivity
- [ ] T035 [P] Implement GetSharedTerminalNamesAsync method in Dao_User using sp_GetSharedTerminalNames
- [ ] T036 [P] Implement GetActiveDepartmentsAsync method in Dao_User using sp_GetDepartments
- [ ] T037 Add proper exception handling to all Dao_User methods (MySqlException, generic Exception)
- [ ] T038 Create UserDaoTests unit test class in Tests/Unit/UserDaoTests.cs
- [ ] T039 [P] Write unit tests for GetUserByWindowsUsernameAsync (success, not found, database error)
- [ ] T040 [P] Write unit tests for ValidateUserPinAsync (valid, invalid, inactive user)
- [ ] T041 [P] Write unit tests for CreateNewUserAsync with validation scenarios
- [ ] T042 [P] Write unit tests for PIN uniqueness validation
- [ ] T043 Run all DAO unit tests and verify they pass

### Tasks - Service Contracts

- [ ] T044 [P] Create IService_Authentication interface in Contracts/Services/IService_Authentication.cs
- [ ] T045 [P] Add AuthenticateByWindowsUsernameAsync method signature to IService_Authentication
- [ ] T046 [P] Add AuthenticateByPinAsync method signature to IService_Authentication
- [ ] T047 [P] Add CreateNewUserAsync method signature to IService_Authentication
- [ ] T048 [P] Add ValidatePinAsync method signature to IService_Authentication
- [ ] T049 [P] Add DetectWorkstationTypeAsync method signature to IService_Authentication
- [ ] T050 [P] Add GetActiveDepartmentsAsync method signature to IService_Authentication
- [ ] T051 [P] Add LogUserActivityAsync method signature to IService_Authentication
- [ ] T052 [P] Create IService_SessionManager interface in Contracts/Services/IService_SessionManager.cs
- [ ] T053 [P] Add CurrentSession property to IService_SessionManager
- [ ] T054 [P] Add CreateSession, UpdateLastActivity, StartTimeoutMonitoring methods to IService_SessionManager
- [ ] T055 [P] Add IsSessionTimedOut, EndSessionAsync methods to IService_SessionManager
- [ ] T056 [P] Add SessionTimedOut event to IService_SessionManager

### Tasks - Service Implementations

- [ ] T057 Create Service_Authentication class in Services/Authentication/Service_Authentication.cs
- [ ] T058 Inject Dao_User and ILoggingService dependencies into Service_Authentication constructor
- [ ] T059 Implement AuthenticateByWindowsUsernameAsync with progress reporting in Service_Authentication
- [ ] T060 Implement AuthenticateByPinAsync with progress reporting in Service_Authentication
- [ ] T061 Implement CreateNewUserAsync with validation and progress reporting in Service_Authentication
- [ ] T062 [P] Implement ValidatePinAsync (format and uniqueness checks) in Service_Authentication
- [ ] T063 [P] Implement DetectWorkstationTypeAsync (query workstation_config table) in Service_Authentication
- [ ] T064 [P] Implement GetActiveDepartmentsAsync in Service_Authentication
- [ ] T065 [P] Implement LogUserActivityAsync wrapper in Service_Authentication
- [ ] T066 Create Service_SessionManager class in Services/Authentication/Service_SessionManager.cs
- [ ] T067 Implement CurrentSession property with private setter in Service_SessionManager
- [ ] T068 Implement CreateSession method (sets timeout duration based on workstation type) in Service_SessionManager
- [ ] T069 Implement UpdateLastActivity method (updates timestamp) in Service_SessionManager
- [ ] T070 Implement DispatcherTimer-based timeout monitoring in Service_SessionManager
- [ ] T071 Implement StartTimeoutMonitoring (starts 60-second timer) in Service_SessionManager
- [ ] T072 Implement StopTimeoutMonitoring (cleanup) in Service_SessionManager
- [ ] T073 Implement IsSessionTimedOut check logic in Service_SessionManager
- [ ] T074 Implement EndSessionAsync with activity logging in Service_SessionManager
- [ ] T075 Implement SessionTimedOut event and raise when timeout detected in Service_SessionManager
- [ ] T076 Register IService_Authentication and Service_Authentication in DI container in App.xaml.cs
- [ ] T077 Register IService_SessionManager and Service_SessionManager as singleton in DI container in App.xaml.cs
- [ ] T078 Create AuthenticationServiceTests unit test class in Tests/Unit/AuthenticationServiceTests.cs
- [ ] T079 Create SessionManagerTests unit test class in Tests/Unit/SessionManagerTests.cs
- [ ] T080 [P] Write unit tests for authentication methods (Windows username, PIN validation)
- [ ] T081 [P] Write unit tests for session timeout logic (CreateSession, UpdateLastActivity, IsTimedOut)
- [ ] T082 Run all service unit tests and verify they pass

**Phase 2 Complete**: ✅ Core infrastructure ready for user story implementation

---

## Phase 3: User Story 1 - Automatic Windows Username Login (P1)

**Goal**: Personal workstation users automatically log in with Windows username

**Story**: Personal workstation users (office staff, supervisors) should be automatically authenticated when they launch the application without entering credentials.

**Independent Test**: Launch app on personal workstation with existing Windows username in database → Automatically authenticated and MainWindow shows with user info in header.

**Test Scenarios**:
1. Personal workstation + existing user → Auto-login success
2. Personal workstation + new user → New User Creation Dialog (implemented in US4)
3. Database connection failure → Error dialog with retry option

### Tasks

- [ ] T083 [US1] Create SplashScreenWindow.xaml with MTM branding, ProgressBar, status TextBlock in Views/Shared/SplashScreenWindow.xaml
- [ ] T084 [US1] Style splash screen (600x400px, centered, no chrome/borders) in SplashScreenWindow.xaml
- [ ] T085 [US1] Create SplashScreenViewModel with ProgressPercentage and StatusMessage properties in ViewModels/Shared/SplashScreenViewModel.cs
- [ ] T086 [US1] Implement UpdateProgress method in SplashScreenViewModel
- [ ] T087 [US1] Wire up code-behind with IProgress handler and Dispatcher marshalling in SplashScreenWindow.xaml.cs
- [ ] T088 [US1] Update Service_OnStartup_AppLifecycle to show splash screen at step 20% in Services/Startup/Service_OnStartup_AppLifecycle.cs
- [ ] T089 [US1] Implement step 40% workstation detection in Service_OnStartup_AppLifecycle
- [ ] T090 [US1] Implement step 45% Windows username authentication branch in Service_OnStartup_AppLifecycle
- [ ] T091 [US1] Implement step 50% session creation on successful authentication in Service_OnStartup_AppLifecycle
- [ ] T092 [US1] Implement step 55% session timeout monitoring start in Service_OnStartup_AppLifecycle
- [ ] T093 [US1] Update MainWindow.xaml to add TextBlock in top-right NavigationView header for user display
- [ ] T094 [US1] Update MainWindowViewModel to add UserDisplayText property bound to CurrentSession in ViewModels/Shared/MainWindowViewModel.cs
- [ ] T095 [US1] Wire up activity tracking events (PointerMoved, KeyDown, Activated) in MainWindow.xaml.cs
- [ ] T096 [US1] Subscribe to SessionTimedOut event in App.xaml.cs to close application on timeout
- [ ] T097 [US1] Call EndSessionAsync in App.xaml.cs OnClosed event
- [ ] T098 [US1] Create WindowsAuthenticationFlowTests integration test in Tests/Integration/WindowsAuthenticationFlowTests.cs
- [ ] T099 [US1] Write integration test: personal workstation + existing user → auto-login success
- [ ] T100 [US1] Write integration test: personal workstation + database error → retry dialog shown
- [ ] T101 [US1] Manual test: Launch on personal workstation with existing username → verify auto-login and user header display
- [ ] T102 [US1] Manual test: Session timeout after 30 minutes → verify app closes and event logged

**User Story 1 Complete**: ✅ Personal workstation users can auto-login with Windows authentication

---

## Phase 4: User Story 2 - Shared Workstation PIN Login (P2)

**Goal**: Shared terminal users log in with username + 4-digit PIN

**Story**: Shared terminal users (shop floor workers) on communal computers (SHOP2, MTMDC, etc.) enter their username and PIN to authenticate.

**Independent Test**: Launch app on shared terminal → PIN login dialog appears → Enter valid credentials → Authenticated and MainWindow shows with user info.

**Test Scenarios**:
1. Shared terminal + valid credentials → Login success
2. Shared terminal + invalid credentials (1st attempt) → Error message, try again
3. Shared terminal + 3 failed attempts → Lockout message (5 seconds), app closes
4. Shared terminal + cancel button → App closes

### Tasks

- [ ] T103 [US2] Create SharedTerminalLoginDialog.xaml with Username TextBox, PIN PasswordBox, buttons in Views/Shared/SharedTerminalLoginDialog.xaml
- [ ] T104 [US2] Add attempt counter TextBlock (hidden initially, shown after first failure) in SharedTerminalLoginDialog.xaml
- [ ] T105 [US2] Add InfoBar for error messages in SharedTerminalLoginDialog.xaml
- [ ] T106 [US2] Style dialog as ContentDialog with Primary (Login) and Secondary (Cancel) buttons in SharedTerminalLoginDialog.xaml
- [ ] T107 [US2] Create SharedTerminalLoginViewModel with Username, Pin, AttemptCount properties in ViewModels/Shared/SharedTerminalLoginViewModel.cs
- [ ] T108 [US2] Implement LoginCommand with validation logic in SharedTerminalLoginViewModel
- [ ] T109 [US2] Call IService_Authentication.AuthenticateByPinAsync in SharedTerminalLoginViewModel
- [ ] T110 [US2] Wire up PrimaryButtonClick (Login) and CloseButtonClick (Cancel) handlers in SharedTerminalLoginDialog.xaml.cs
- [ ] T111 [US2] Implement PIN field clearing and attempt counter update on error in SharedTerminalLoginDialog.xaml.cs
- [ ] T112 [US2] Integrate dialog display at step 45% for shared terminals in Service_OnStartup_AppLifecycle.cs
- [ ] T113 [US2] Implement pulsing progress animation at 45% while waiting for login in SplashScreenViewModel.cs
- [ ] T114 [US2] Implement 3-attempt lockout logic (show error for 5 seconds, then close app) in Service_OnStartup_AppLifecycle.cs
- [ ] T115 [US2] Log failed login attempts via LogUserActivityAsync in Service_OnStartup_AppLifecycle.cs
- [ ] T116 [US2] Create PinAuthenticationFlowTests integration test in Tests/Integration/PinAuthenticationFlowTests.cs
- [ ] T117 [US2] Write integration test: shared terminal + valid credentials → login success
- [ ] T118 [US2] Write integration test: shared terminal + invalid credentials → retry allowed
- [ ] T119 [US2] Write integration test: shared terminal + 3 failures → lockout and close
- [ ] T120 [US2] Manual test: Launch on shared terminal → verify PIN dialog appears over splash screen
- [ ] T121 [US2] Manual test: Enter valid credentials → verify authentication and main window loads
- [ ] T122 [US2] Manual test: Enter invalid PIN 3 times → verify lockout message and app closes after 5 seconds

**User Story 2 Complete**: ✅ Shared terminal users can log in with PIN authentication

---

## Phase 5: User Story 3 - Session Timeout Management (P3)

**Goal**: Sessions automatically timeout after inactivity (30 min personal, 15 min shared)

**Story**: Users remain logged in during their session until the app closes or they become inactive. Automatic timeout provides security by closing the application when no activity is detected.

**Independent Test**: Log in, leave app idle for timeout period → App automatically closes and timeout event is logged.

**Test Scenarios**:
1. Personal workstation + 30 minutes inactivity → App closes, timeout logged
2. Shared terminal + 15 minutes inactivity → App closes, timeout logged
3. User interaction during idle period → Timer resets, app stays open
4. Manual app close → Session ends gracefully, logged

### Tasks

**Note**: Most timeout implementation was completed in Phase 2 (Foundational). These tasks verify and test the functionality.

- [ ] T123 [US3] Verify UpdateLastActivity is called on all MainWindow interaction events
- [ ] T124 [US3] Verify DispatcherTimer interval is set to 60 seconds in Service_SessionManager
- [ ] T125 [US3] Verify timeout durations: 30 min for personal, 15 min for shared terminals in Model_UserSession
- [ ] T126 [US3] Verify SessionTimedOut event handler in App.xaml.cs closes application
- [ ] T127 [US3] Verify EndSessionAsync logs timeout event via sp_LogUserActivity
- [ ] T128 [US3] Write unit test: IsSessionTimedOut returns true after timeout duration exceeded
- [ ] T129 [US3] Write unit test: UpdateLastActivity resets timer and IsSessionTimedOut returns false
- [ ] T130 [US3] Write integration test: Create session, wait for timeout → SessionTimedOut event fires
- [ ] T131 [US3] Manual test: Personal workstation + 30 min idle → verify app closes
- [ ] T132 [US3] Manual test: Shared terminal + 15 min idle → verify app closes
- [ ] T133 [US3] Manual test: Mouse movement during idle → verify timer resets and app stays open
- [ ] T134 [US3] Manual test: Check database user_activity_log for session_timeout event

**User Story 3 Complete**: ✅ Session timeout automatically closes app after inactivity

---

## Phase 6: User Story 4 - New User Creation on First Access (P4)

**Goal**: New users can be created via dialog when Windows username not found

**Story**: When a personal workstation user's Windows username is not in the database, a supervisor creates their account by entering full name, PIN, department, and shift information.

**Independent Test**: Launch app on personal workstation with Windows username NOT in database → New User Setup Dialog appears → Fill form, click Create Account → Account created, authenticated, main window loads.

**Test Scenarios**:
1. New Windows username → Dialog appears with username pre-filled
2. Fill all required fields, valid PIN → Account created successfully
3. Use duplicate PIN → Error message, retry
4. Cancel dialog → App closes
5. Department dropdown populates from database
6. Select "Other" department → Custom text field appears

### Tasks

- [ ] T135 [US4] Create NewUserSetupDialog.xaml with all form fields (FullName, WindowsUsername, Shift, Department, PIN, ConfirmPIN) in Views/Shared/NewUserSetupDialog.xaml
- [ ] T136 [US4] Add WindowsUsername TextBox (read-only, auto-filled) in NewUserSetupDialog.xaml
- [ ] T137 [US4] Add Department ComboBox populated from sp_GetDepartments in NewUserSetupDialog.xaml
- [ ] T138 [US4] Add "Other" option in Department ComboBox with conditional custom department TextBox in NewUserSetupDialog.xaml
- [ ] T139 [US4] Add Shift ComboBox with 3 options (1st Shift, 2nd Shift, 3rd Shift) in NewUserSetupDialog.xaml
- [ ] T140 [US4] Add PIN PasswordBox with 4-digit numeric validation in NewUserSetupDialog.xaml
- [ ] T141 [US4] Add Confirm PIN PasswordBox with match validation in NewUserSetupDialog.xaml
- [ ] T142 [US4] Add InfoBar for validation errors and success messages in NewUserSetupDialog.xaml
- [ ] T143 [US4] Add ProgressBar for loading state during account creation in NewUserSetupDialog.xaml
- [ ] T144 [US4] Style dialog with Primary (Create Account, Accent) and Secondary (Cancel) buttons in NewUserSetupDialog.xaml
- [ ] T145 [US4] Create NewUserSetupViewModel with properties for all form fields in ViewModels/Shared/NewUserSetupViewModel.cs
- [ ] T146 [US4] Add Departments ObservableCollection to NewUserSetupViewModel
- [ ] T147 [US4] Add ShowCustomDepartment boolean property to NewUserSetupViewModel
- [ ] T148 [US4] Implement LoadDepartmentsAsync method (calls IService_Authentication.GetActiveDepartmentsAsync) in NewUserSetupViewModel
- [ ] T149 [US4] Implement field validation methods (FullName, PIN format, PIN match) in NewUserSetupViewModel
- [ ] T150 [US4] Implement CreateAccountCommand with async account creation in NewUserSetupViewModel
- [ ] T151 [US4] Call IService_Authentication.CreateNewUserAsync in NewUserSetupViewModel
- [ ] T152 [US4] Wire up Department ComboBox selection to show/hide custom TextBox in NewUserSetupDialog.xaml.cs
- [ ] T153 [US4] Implement inline validation with red borders and error messages in NewUserSetupDialog.xaml.cs
- [ ] T154 [US4] Implement loading state (disable controls, show ProgressBar) during account creation in NewUserSetupDialog.xaml.cs
- [ ] T155 [US4] Implement success state (show InfoBar with employee number, then close dialog) in NewUserSetupDialog.xaml.cs
- [ ] T156 [US4] Handle cancel button with confirmation prompt in NewUserSetupDialog.xaml.cs
- [ ] T157 [US4] Integrate dialog display at step 45% when Windows username not found in Service_OnStartup_AppLifecycle.cs
- [ ] T158 [US4] Implement pulsing progress animation at 45% while dialog is open in SplashScreenViewModel.cs
- [ ] T159 [US4] Resume splash screen progress (47% → 50%) after successful account creation in Service_OnStartup_AppLifecycle.cs
- [ ] T160 [US4] Log user_created event via sp_LogUserActivity with creator's Windows username in Service_OnStartup_AppLifecycle.cs
- [ ] T161 [US4] Create NewUserCreationFlowTests integration test in Tests/Integration/NewUserCreationFlowTests.cs
- [ ] T162 [US4] Write integration test: New username → dialog appears → create account → success
- [ ] T163 [US4] Write integration test: Duplicate PIN → error message shown
- [ ] T164 [US4] Write integration test: Cancel dialog → app closes
- [ ] T165 [US4] Manual test: Launch with new Windows username → verify dialog appears with username pre-filled
- [ ] T166 [US4] Manual test: Fill all required fields → verify account created and employee number shown
- [ ] T167 [US4] Manual test: Try duplicate PIN → verify error message and retry works
- [ ] T168 [US4] Manual test: Select "Other" department → verify custom text field appears
- [ ] T169 [US4] Manual test: Cancel dialog → verify app closes gracefully
- [ ] T170 [US4] Manual test: Check database for new user record and user_created log entry

**User Story 4 Complete**: ✅ New users can be created via intuitive dialog workflow

---

## Phase 7: User Story 5 - Optional Visual/Infor ERP Integration (P5)

**Goal**: Users can optionally store Visual/Infor ERP credentials for future integrations

**Story**: Authorized users can enter and save their Visual/Infor ERP credentials in user settings. These credentials enable future ERP integration features (out of scope for this feature, but credentials stored for future use).

**Independent Test**: User opens settings → Enters ERP username and password → Saves → Credentials stored in database (plain text, masked in UI).

**Test Scenarios**:
1. User without ERP credentials → ERP-integrated features disabled/hidden
2. User enters ERP credentials → Credentials saved, menu options enabled
3. Invalid ERP credentials → Appropriate error shown (future integration feature)
4. ERP password never displayed unmasked in UI

### Tasks

**Note**: Full ERP integration is out of scope. These tasks implement credential storage UI only.

- [ ] T171 [US5] Update NewUserSetupDialog to include optional ERP credentials section (collapsed by default) in NewUserSetupDialog.xaml
- [ ] T172 [US5] Add "Configure ERP Access" checkbox to expand ERP credentials section in NewUserSetupDialog.xaml
- [ ] T173 [US5] Add Visual Username TextBox in ERP credentials section in NewUserSetupDialog.xaml
- [ ] T174 [US5] Add Visual Password PasswordBox in ERP credentials section in NewUserSetupDialog.xaml
- [ ] T175 [US5] Update NewUserSetupViewModel to include VisualUsername and VisualPassword properties in ViewModels/Shared/NewUserSetupViewModel.cs
- [ ] T176 [US5] Update CreateAccountCommand to include optional ERP credentials when saving user in NewUserSetupViewModel.cs
- [ ] T177 [US5] Verify ERP passwords are never displayed unmasked in any UI
- [ ] T178 [US5] Update startup step 55% to check for ERP credentials in Model_User in Service_OnStartup_AppLifecycle.cs
- [ ] T179 [US5] Add HasErpAccess boolean to Model_UserSession for tracking ERP capability
- [ ] T180 [US5] Manual test: Create user with ERP credentials → verify saved to database
- [ ] T181 [US5] Manual test: Create user without ERP credentials → verify NULL in database
- [ ] T182 [US5] Manual test: Verify ERP password never displayed unmasked in UI
- [ ] T183 [US5] Document ERP credential storage for future integration features

**User Story 5 Complete**: ✅ Users can optionally store ERP credentials for future integrations

---

## Phase 8: Polish & Cross-Cutting Concerns

**Goal**: Performance optimization, accessibility, comprehensive testing, documentation

**Test Criteria**: All success criteria from spec.md met; performance targets achieved; accessibility requirements satisfied

### Tasks - Performance & Optimization

- [ ] T184 Measure and verify startup time < 10 seconds (all authentication scenarios)
- [ ] T185 Measure and verify authentication phase < 3 seconds (automatic Windows login)
- [ ] T186 Measure and verify database queries < 800ms per operation
- [ ] T187 Measure and verify dialog display < 200ms from trigger
- [ ] T188 Verify splash screen progress updates every 200-500ms
- [ ] T189 Profile and optimize any performance bottlenecks identified
- [ ] T190 Verify UI thread never blocked > 100ms (all async operations correct)

### Tasks - Accessibility

- [ ] T191 Verify keyboard navigation works (Tab, Enter, Escape) in all dialogs
- [ ] T192 Add AutomationProperties.Name to all interactive controls for screen reader support
- [ ] T193 Test with Windows Narrator screen reader and fix any issues
- [ ] T194 Verify high contrast theme support (colors, borders visible)
- [ ] T195 Test text scaling from 96 DPI to 192 DPI and fix any layout issues
- [ ] T196 Verify focus indicators visible on all interactive controls

### Tasks - Error Handling & Robustness

- [ ] T197 Test database disconnection during startup → verify retry dialog with manual retry option
- [ ] T198 Test invalid database credentials → verify error message with actionable guidance
- [ ] T199 Test network timeout scenarios → verify appropriate error handling
- [ ] T200 Test corrupt database data → verify graceful degradation
- [ ] T201 Add additional logging to all critical authentication paths
- [ ] T202 Verify all error messages are user-friendly (no SQL or stack traces in UI)

### Tasks - Comprehensive Testing

- [ ] T203 Run all 6 manual test scenarios from spec.md Testing section
- [ ] T204 Verify all 16 Success Criteria from spec.md are met
- [ ] T205 Verify all 42 Functional Requirements from spec.md are implemented
- [ ] T206 Code review with focus on security (SQL injection prevention, error disclosure)
- [ ] T207 Code review with focus on performance (async/await, proper disposal)
- [ ] T208 Run all unit tests and verify > 80% code coverage
- [ ] T209 Run all integration tests and verify they pass
- [ ] T210 Test on both x64 and ARM64 platforms

### Tasks - Documentation & Cleanup

- [ ] T211 Add inline code comments to complex authentication logic
- [ ] T212 Update README.md with authentication feature overview
- [ ] T213 Create user documentation for new user creation workflow
- [ ] T214 Create admin documentation for managing shared terminal names in database
- [ ] T215 Create admin documentation for managing departments in database
- [ ] T216 Create admin documentation for querying user_activity_log for audit reports
- [ ] T217 Document session timeout values and rationale
- [ ] T218 Remove any debug logging or console output
- [ ] T219 Clean up any unused imports or commented-out code
- [ ] T220 Final code formatting pass (consistent with project style)

**Phase 8 Complete**: ✅ Feature polished, tested, and ready for production

---

## Dependencies & Execution Order

### Critical Path (Must be completed in order)

```
Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3+ (User Stories in any order)
```

### User Story Dependencies

- **US1 (Windows Auth)**: Requires Phase 1 + Phase 2
- **US2 (PIN Auth)**: Requires Phase 1 + Phase 2
- **US3 (Session Timeout)**: Built into Phase 2, just needs testing
- **US4 (New User Creation)**: Requires Phase 1 + Phase 2 + US1 (integrated into Windows auth flow)
- **US5 (ERP Credentials)**: Requires Phase 1 + Phase 2 + US4 (extends user creation)

**Recommended Order**: US1 → US2 → US4 → US3 → US5

**Parallel Opportunities**:
- Phase 1: Tasks T002-T008 (table definitions) can be done in parallel
- Phase 1: Tasks T009-T014 (stored procedures) can be done in parallel
- Phase 2: All model creation tasks (T019-T027) can be done in parallel
- Phase 2: DAO method implementations (T029-T036) can be done in parallel after T028
- Phase 2: Service contract definitions (T044-T056) can be done in parallel
- Phase 2: Unit test writing (T039-T042, T080-T081) can be done in parallel
- US2 can be implemented in parallel with US1 (different dialogs, no conflicts)
- US5 can be implemented in parallel with US3 (independent features)

---

## Success Criteria Verification

Before marking feature complete, verify all 16 Success Criteria from spec.md:

- [ ] SC-001: Splash screen displays within 500ms of application launch
- [ ] SC-002: Automatic Windows authentication on personal workstations in < 3 seconds (database query time)
- [ ] SC-003: Authentication phase (steps 40-60%) completes in under 3 seconds for automatic authentication
- [ ] SC-004: Shared Terminal Login Dialog appears within 2 seconds when workstation detected as shared
- [ ] SC-005: Splash screen progress updates occur every 200-500ms for smooth visual feedback
- [ ] SC-006: All ContentDialogs (Login, New User Setup) appear centered over splash screen within 200ms
- [ ] SC-007: Splash screen never freezes and shows pulsing animation while waiting for user input
- [ ] SC-008: New user accounts are created successfully and logged in without requiring application restart
- [ ] SC-009: 4-digit PINs are validated as unique before account creation (database constraint + pre-validation)
- [ ] SC-010: User's full name and employee number display in application header/status bar after authentication
- [ ] SC-011: All authentication events logged in application logs (user_activity_log table)
- [ ] SC-012: Session timeout closes application after configured inactivity (30 min personal, 15 min shared)
- [ ] SC-013: Login failures after 3 attempts on shared terminals result in application closure with 5-second message
- [ ] SC-014: Visual/Infor ERP passwords never displayed in plain text in any user interface
- [ ] SC-015: Database connection errors show user-friendly messages with retry option (not raw SQL errors)
- [ ] SC-016: Total application startup time (launch to main window) is under 10 seconds for all authentication scenarios

---

## Definition of Done

**Feature is complete when**:

- [x] All 220 tasks completed
- [x] All user stories independently testable and delivering value
- [x] All 16 success criteria met
- [x] All 42 functional requirements implemented
- [x] All unit tests passing (> 80% coverage)
- [x] All integration tests passing
- [x] All 6 manual test scenarios passing
- [x] Performance targets met (< 10 sec startup, < 3 sec auth)
- [x] Accessibility requirements met (keyboard, screen reader, high contrast)
- [x] Documentation complete (inline comments, README, admin docs)
- [x] Code reviewed and approved
- [x] Database deployed to development environment
- [x] Branch merged to main/development branch

---

## Task Summary

**Total Tasks**: 220  
**Setup Phase**: 18 tasks  
**Foundational Phase**: 64 tasks  
**User Story 1 (Windows Auth)**: 20 tasks  
**User Story 2 (PIN Auth)**: 20 tasks  
**User Story 3 (Session Timeout)**: 12 tasks  
**User Story 4 (New User Creation)**: 36 tasks  
**User Story 5 (ERP Credentials)**: 13 tasks  
**Polish & Cross-Cutting**: 37 tasks

**Estimated Timeline**: 12-14 days (80-96 hours)

**MVP Scope** (minimum viable product):  
Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (US1: Windows Auth) = ~40-45 tasks, delivers automatic authentication for personal workstations

---

**Generated**: December 15, 2025  
**Command**: `/speckit.tasks`  
**Ready**: ✅ Tasks ready for implementation - begin with Phase 1 (Setup)

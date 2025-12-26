# Quickstart Guide: User Authentication & Login System

**Feature**: User Authentication & Login System  
**Branch**: `002-user-login`  
**Date**: December 15, 2025

## Overview

This guide provides the step-by-step implementation sequence for the User Authentication & Login System. Follow these phases in order for a systematic build-out.

## Prerequisites

- [x] Phase 1 Infrastructure complete (MVVM structure, services, database helpers)
- [x] MySQL database server accessible
- [x] WinUI 3 development environment configured
- [x] Specification reviewed and clarified (all ambiguities resolved)
- [x] Data model and contracts reviewed

## Implementation Phases

### Phase 1: Database Foundation (Day 1)

**Goal**: Create database schema and stored procedures

**Steps**:
1. Create SQL schema script: `Database/Schemas/02_create_authentication_tables.sql`
   - `users` table with all fields and indexes
   - `workstation_config` table
   - `departments` table
   - `user_activity_log` table
   - Insert initial department data (Receiving, Shipping, Production, etc.)
   - Insert initial workstation configuration data

2. Create stored procedures in `Database/StoredProcedures/Authentication/`:
   - `sp_GetUserByWindowsUsername.sql`
   - `sp_ValidateUserPin.sql`
   - `sp_CreateNewUser.sql`
   - `sp_LogUserActivity.sql`
   - `sp_GetSharedTerminalNames.sql`
   - `sp_GetDepartments.sql`

3. Test database setup:
   - Run schema script on development database
   - Run stored procedure scripts
   - Verify tables created with correct schema
   - Insert test user data
   - Test each stored procedure manually

**Deliverables**:
- [x] Database schema script
- [x] 6 stored procedures
- [x] Test data inserted
- [x] All procedures tested manually

**Time Estimate**: 4-6 hours

---

### Phase 2: Data Models (Day 1-2)

**Goal**: Create C# model classes for data structures

**Steps**:
1. Create `Models/Model_User.cs`
   - All database fields as properties
   - DisplayName computed property
   - HasErpAccess computed property

2. Create `Models/Model_UserSession.cs`
   - User reference
   - Session metadata (timestamps, workstation info)
   - Timeout tracking properties
   - UpdateLastActivity() method

3. Create `Models/Model_WorkstationConfig.cs`
   - Workstation information properties
   - IsSharedTerminal / IsPersonalWorkstation computed properties
   - Static DetectCurrentWorkstation() method signature

4. Verify existing `Models/Model_Dao_Result.cs` from Phase 1

**Deliverables**:
- [x] Model_User.cs
- [x] Model_UserSession.cs
- [x] Model_WorkstationConfig.cs

**Time Estimate**: 2-3 hours

---

### Phase 3: Data Access Layer (Day 2)

**Goal**: Implement database access methods

**Steps**:
1. Create `Data/Authentication/Dao_User.cs`
   - Implement all methods from data-model.md DAO section
   - Use existing `Helper_Database_StoredProcedure` utilities
   - Return `Model_Dao_Result<T>` for all operations
   - Implement proper exception handling

2. Create unit tests `Tests/Unit/UserDaoTests.cs`
   - Test each DAO method with test database
   - Test error handling (connection failures, invalid data)
   - Test validation logic (PIN uniqueness, etc.)

**Deliverables**:
- [x] Dao_User.cs with all methods
- [x] Unit tests passing

**Time Estimate**: 6-8 hours

---

### Phase 4: Service Contracts & Implementations (Day 3-4)

**Goal**: Implement authentication and session management services

**Steps**:
1. Create service contracts:
   - `Contracts/Services/IService_Authentication.cs` (copy from contracts/IService_Authentication.md)
   - `Contracts/Services/IService_SessionManager.cs` (copy from contracts/IService_SessionManager.md)

2. Implement `Services/Authentication/Service_Authentication.cs`
   - All methods from IService_Authentication
   - Progress reporting integration
   - Error handling and logging
   - Inject Dao_User and ILoggingService

3. Implement `Services/Authentication/Service_SessionManager.cs`
   - All methods from IService_SessionManager
   - DispatcherTimer for timeout monitoring
   - SessionTimedOut event
   - Inject IService_Authentication

4. Register services in `App.xaml.cs`:
   ```csharp
   services.AddSingleton<IService_Authentication, Service_Authentication>();
   services.AddSingleton<IService_SessionManager, Service_SessionManager>();
   ```

5. Create unit tests:
   - `Tests/Unit/AuthenticationServiceTests.cs`
   - `Tests/Unit/SessionManagerTests.cs`
   - Mock Dao_User and ILoggingService

**Deliverables**:
- [x] IService_Authentication contract
- [x] IService_SessionManager contract
- [x] Service_Authentication implementation
- [x] Service_SessionManager implementation
- [x] Services registered in DI container
- [x] Unit tests passing

**Time Estimate**: 8-10 hours

---

### Phase 5: Splash Screen (Day 5)

**Goal**: Create splash screen with progress reporting

**Steps**:
1. Create `Views/Shared/SplashScreenWindow.xaml`:
   - MTM logo and branding
   - ProgressBar (0-100%)
   - TextBlock for status messages
   - Center on screen, no chrome/borders
   - 600x400px minimum size

2. Create `ViewModels/Shared/SplashScreenViewModel.cs`:
   - ProgressPercentage property (INotifyPropertyChanged)
   - StatusMessage property
   - UpdateProgress(int percentage, string message) method

3. Wire up code-behind `Views/Shared/SplashScreenWindow.xaml.cs`:
   - Implement IProgress<(int, string)> handler
   - Marshal updates to UI thread via Dispatcher

4. Update `Service_OnStartup_AppLifecycle.cs`:
   - Show splash screen before authentication (step 20%)
   - Pass IProgress to authentication methods
   - Hide splash screen after main window loads (step 100%)

**Deliverables**:
- [x] SplashScreenWindow.xaml
- [x] SplashScreenViewModel.cs
- [x] Splash screen displays during startup
- [x] Progress updates working

**Time Estimate**: 4-6 hours

---

### Phase 6: Shared Terminal Login Dialog (Day 6)

**Goal**: Create PIN login dialog for shared terminals

**Steps**:
1. Create `Views/Shared/SharedTerminalLoginDialog.xaml`:
   - ContentDialog with Title, Username TextBox, PIN PasswordBox
   - "Login" (Primary) and "Cancel" (Secondary) buttons
   - Attempt counter TextBlock (shown after first failure)
   - InfoBar for error messages

2. Create `ViewModels/Shared/SharedTerminalLoginViewModel.cs`:
   - Username, Pin properties (INotifyPropertyChanged)
   - AttemptCount property (0-3)
   - LoginCommand (RelayCommand)
   - Validation logic
   - Call IService_Authentication.AuthenticateByPinAsync

3. Wire up code-behind:
   - Handle PrimaryButtonClick for login
   - Handle CloseButtonClick for cancel
   - Clear PIN field on error
   - Show/hide attempt counter

4. Integrate in startup flow:
   - Show dialog at step 45% if shared terminal detected
   - Pause splash screen with pulsing animation
   - Resume splash screen on success (step 50%)
   - Close app on cancel or 3rd failure

**Deliverables**:
- [x] SharedTerminalLoginDialog.xaml
- [x] SharedTerminalLoginViewModel.cs
- [x] Dialog shows on shared terminals
- [x] PIN validation working
- [x] 3-attempt lockout working

**Time Estimate**: 6-8 hours

---

### Phase 7: New User Setup Dialog (Day 7-8)

**Goal**: Create new user creation dialog

**Steps**:
1. Create `Views/Shared/NewUserSetupDialog.xaml`:
   - ContentDialog with Title and explanation
   - TextBox: FullName (required)
   - TextBox: WindowsUsername (read-only, auto-filled)
   - ComboBox: Shift (required, 3 options)
   - ComboBox: Department (required, populated from sp_GetDepartments)
   - TextBox: Custom Department (visible only when "Other" selected)
   - PasswordBox: Pin (required, 4 digits)
   - PasswordBox: ConfirmPin (required, must match)
   - InfoBar: Validation errors and success messages
   - ProgressBar: Loading state during account creation
   - "Create Account" (Primary, Accent) and "Cancel" (Secondary) buttons

2. Create `ViewModels/Shared/NewUserSetupViewModel.cs`:
   - Properties for all fields with INotifyPropertyChanged
   - Departments collection (ObservableCollection<string>)
   - ShowCustomDepartment property (bool)
   - Validation methods for each field
   - CreateAccountCommand (RelayCommand)
   - Call IService_Authentication.GetActiveDepartmentsAsync on load
   - Call IService_Authentication.CreateNewUserAsync on submit

3. Wire up code-behind:
   - Load departments on dialog open
   - Show/hide custom department TextBox based on ComboBox selection
   - Inline validation with red borders and error messages
   - Loading state: disable controls, show ProgressBar
   - Success state: show InfoBar with employee number, then close

4. Integrate in startup flow:
   - Show dialog at step 45% if Windows username not found
   - Pause splash screen with pulsing animation
   - Resume splash screen on success (step 47% → 50%)
   - Close app on cancel (with confirmation dialog)

**Deliverables**:
- [x] NewUserSetupDialog.xaml
- [x] NewUserSetupViewModel.cs
- [x] Dialog shows for new Windows usernames
- [x] All validation working
- [x] Account creation working
- [x] Department dropdown populated

**Time Estimate**: 8-10 hours

---

### Phase 8: Authentication Integration (Day 9)

**Goal**: Wire up authentication flow in startup sequence

**Steps**:
1. Update `Service_OnStartup_AppLifecycle.cs`:
   - Step 40%: Call IService_Authentication.DetectWorkstationTypeAsync
   - Step 45%: Branch based on workstation type:
     - Personal: Call AuthenticateByWindowsUsernameAsync
       - If user found: Continue to step 50%
       - If not found: Show NewUserSetupDialog, wait for completion
     - Shared Terminal: Show SharedTerminalLoginDialog, wait for credentials
   - Step 50%: Call IService_SessionManager.CreateSession
   - Step 55%: Call IService_SessionManager.StartTimeoutMonitoring
   - Step 60%: Continue to MainWindow

2. Update `App.xaml.cs`:
   - Subscribe to IService_SessionManager.SessionTimedOut event
   - Handle timeout by closing application
   - Call EndSessionAsync in OnClosed event

3. Create integration tests:
   - `Tests/Integration/WindowsAuthenticationFlowTests.cs`
   - `Tests/Integration/PinAuthenticationFlowTests.cs`
   - `Tests/Integration/NewUserCreationFlowTests.cs`

**Deliverables**:
- [x] Startup flow updated with authentication
- [x] Personal workstation auto-login working
- [x] Shared terminal PIN login working
- [x] New user creation working
- [x] Integration tests passing

**Time Estimate**: 6-8 hours

---

### Phase 9: Main Window Updates (Day 10)

**Goal**: Display user info and track activity

**Steps**:
1. Update `Views/Shared/MainWindow.xaml`:
   - Add TextBlock in top-right corner of NavigationView header
   - Bind to user display text
   - Format: "[Full Name] (Emp #[number])"

2. Update `ViewModels/Shared/MainWindowViewModel.cs`:
   - Add UserDisplayText property
   - Get current user from IService_SessionManager.CurrentSession
   - Update property when session changes

3. Wire up activity tracking in `MainWindow.xaml.cs`:
   - Subscribe to PointerMoved event → UpdateLastActivity()
   - Subscribe to KeyDown event → UpdateLastActivity()
   - Subscribe to Activated event → UpdateLastActivity()

**Deliverables**:
- [x] User info displayed in header
- [x] Activity tracking working
- [x] Session timeout triggers app close

**Time Estimate**: 3-4 hours

---

### Phase 10: Testing & Refinement (Day 11-12)

**Goal**: Comprehensive testing and bug fixes

**Steps**:
1. Manual testing (follow spec.md test scenarios):
   - Test 1: Personal workstation - existing user
   - Test 2: Personal workstation - new user
   - Test 3: Shared terminal - valid credentials
   - Test 4: Shared terminal - invalid credentials (3 attempts)
   - Test 5: Database connection failure
   - Test 6: Dialog cancellation

2. Performance testing:
   - Measure startup time (target < 10 seconds)
   - Measure authentication phase (target < 3 seconds for auto-login)
   - Verify splash screen updates (every 200-500ms)
   - Verify dialog display (< 200ms)

3. Accessibility testing:
   - Keyboard navigation (Tab, Enter, Escape)
   - Screen reader support (AutomationProperties)
   - High contrast theme support
   - Text scaling (96-192 DPI)

4. Error handling testing:
   - Database disconnection during startup
   - Invalid database credentials
   - Network timeout scenarios
   - Corrupt database data

5. Bug fixes and polish:
   - Address any issues found during testing
   - Refine error messages for clarity
   - Optimize performance bottlenecks
   - Add additional logging where needed

**Deliverables**:
- [x] All manual test scenarios passing
- [x] Performance targets met
- [x] Accessibility requirements met
- [x] Error handling robust

**Time Estimate**: 12-16 hours

---

## Total Time Estimate

**Development**: 10-12 days (80-96 hours)  
**Testing & Refinement**: 2 days (16 hours)  
**Total**: 12-14 days

## Success Criteria Checklist

Before marking this feature complete, verify:

- [ ] All 16 Success Criteria from spec.md are met
- [ ] All 42 Functional Requirements implemented
- [ ] Database schema deployed to development environment
- [ ] All stored procedures tested and working
- [ ] Unit tests passing (> 80% code coverage)
- [ ] Integration tests passing
- [ ] Manual test scenarios all passing
- [ ] Performance targets met (< 10 sec startup)
- [ ] Accessibility requirements met
- [ ] Error handling graceful and user-friendly
- [ ] Activity logged for audit trail
- [ ] Documentation updated (README, inline comments)

## Common Pitfalls to Avoid

1. **Blocking UI Thread**: Always use async/await for database operations
2. **Null Reference Exceptions**: Check CurrentSession != null before accessing
3. **Memory Leaks**: Unsubscribe from events, stop timers on cleanup
4. **SQL Injection**: Use parameterized queries in all stored procedures
5. **Progress Not Updating**: Ensure IProgress updates marshalled to UI thread
6. **Timer Not Firing**: DispatcherTimer must be created on UI thread
7. **Dialog Z-Order**: Set XamlRoot property on ContentDialog for proper display over splash screen

## Debugging Tips

### Authentication Not Working
- Check database connection string in appsettings.json
- Verify stored procedures exist and have correct names
- Check Windows username format (may include domain: DOMAIN\username)
- Log SQL exceptions with full details

### Splash Screen Not Updating
- Verify IProgress<T> implementation marshals to Dispatcher
- Check progress percentages are sequential (40 → 45 → 50, not 50 → 45)
- Ensure progress updates not too frequent (max every 100ms)

### Session Timeout Not Working
- Verify DispatcherTimer created on UI thread
- Check TimeoutDuration set correctly (30 min vs 15 min)
- Verify UpdateLastActivity() being called on events
- Check IsSessionTimedOut() logic with debug breakpoints

### Dialog Not Showing
- Verify XamlRoot property set: `dialog.XamlRoot = this.Content.XamlRoot`
- Check ContentDialog not already open (only one at a time)
- Ensure await dialog.ShowAsync() called
- Verify MainWindow or SplashScreen has focus

## Next Steps After Completion

1. Merge feature branch to main/development branch
2. Deploy database schema to test/staging environment
3. Create user documentation for:
   - How to add new users
   - How to reset PINs
   - How to configure shared terminal names
4. Create admin documentation for:
   - Database maintenance
   - Activity log queries
   - Adding new departments
5. Plan for Phase 2 features that depend on authentication:
   - Label creation with employee tracking
   - User preferences/settings
   - Role-based access control (if needed)

## Support & Resources

- **Specification**: [spec.md](./spec.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Contracts**: [contracts/](./contracts/)
- **Research**: [research.md](./research.md)
- **WinUI 3 Docs**: https://learn.microsoft.com/en-us/windows/apps/winui/winui3/
- **MVVM Pattern**: Existing Phase 1 Infrastructure examples
- **Database Helpers**: `Helpers/Database/` folder

---

**Status**: ✅ Quickstart guide complete and ready for development

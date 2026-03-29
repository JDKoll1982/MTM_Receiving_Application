# User Identity Footprint

Last Updated: 2026-03-29

## Purpose

This document inventories the user-related fields that participate in startup authentication,
manual re-login, session startup, and privilege loading.

## User Fields

| Field                  | Source Of Truth                     | Runtime Use                                                                                | Primary Code Touchpoints                                                                                                                                                                                                                |
| ---------------------- | ----------------------------------- | ------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `EmployeeNumber`       | `auth_users.employee_number`        | Active-session identity, settings defaults, role loading, audit trails                     | `Module_Core/Models/Systems/Model_User.cs`, `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`, `Module_Core/Services/Authentication/Service_UserPrivileges.cs`, `Module_Settings.Core/Data/Dao_SettingsCoreUserRoles.cs` |
| `WindowsUsername`      | `auth_users.windows_username`       | Personal-workstation auto-login, activity logging, user creation, default-mode persistence | `Module_Core/Data/Authentication/Dao_User.cs`, `Module_Core/Services/Authentication/Service_Authentication.cs`, `Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs`                                                             |
| `FullName`             | `auth_users.full_name`              | Shell display, login confirmation, shared-terminal username fallback                       | `Module_Core/Models/Systems/Model_User.cs`, `MainWindow.xaml.cs`, `Module_Shared/ViewModels/ViewModel_Shared_SharedTerminalLogin.cs`                                                                                                    |
| `Pin`                  | `auth_users.pin`                    | Shared-terminal login and manual switch-user login                                         | `Module_Core/Services/Authentication/Service_AuthCredentialProtection.cs`, `Module_Core/Data/Authentication/Dao_User.cs`, `Database/Database_Deployment/Sql_Files/StoredProcedures/Authentication/sp_Auth_User_ValidatePin.sql`         |
| `Department`           | `auth_users.department`             | User profile display, onboarding/new-user creation, downstream reporting context           | `Module_Core/Models/Systems/Model_User.cs`, `Module_Core/Services/Authentication/Service_Authentication.cs`, `Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs`                                                                |
| `Shift`                | `auth_users.shift`                  | New-user setup, user profile storage, downstream reporting context                         | `Module_Core/Models/Systems/Model_User.cs`, `Module_Core/Services/Authentication/Service_Authentication.cs`, `Database/Database_Deployment/Sql_Files/StoredProcedures/Authentication/sp_Auth_User_Create.sql`                           |
| `IsActive`             | `auth_users.is_active`              | Login eligibility and user-management state                                                | `Module_Core/Data/Authentication/Dao_User.cs`, `Module_Settings.Core/ViewModels/ViewModel_Settings_Users.cs`                                                                                                                            |
| `VisualUsername`       | `auth_users.visual_username`        | Optional ERP integration credentials for the active user                                   | `Module_Core/Services/Authentication/Service_AuthCredentialProtection.cs`, `Module_Core/Data/Authentication/Dao_User.cs`, `Module_Core/Models/Systems/Model_User.cs`                                                                    |
| `VisualPassword`       | `auth_users.visual_password`        | Optional ERP integration credentials for the active user                                   | `Module_Core/Services/Authentication/Service_AuthCredentialProtection.cs`, `Module_Core/Data/Authentication/Dao_User.cs`, `Module_Core/Models/Systems/Model_User.cs`                                                                    |
| `DefaultReceivingMode` | `auth_users.default_receiving_mode` | Startup/receiving workflow entry behavior                                                  | `Module_Core/Models/Systems/Model_User.cs`, `Module_Receiving/Services/Service_ReceivingWorkflow.cs`, `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs`                                                                |
| `DefaultDunnageMode`   | `auth_users.default_dunnage_mode`   | Startup/dunnage workflow entry behavior                                                    | `Module_Core/Models/Systems/Model_User.cs`, `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`                                                                                                                            |

## Session Fields

| Field                                                          | Source                                                     | Runtime Use                                                               | Primary Code Touchpoints                                                                                                 |
| -------------------------------------------------------------- | ---------------------------------------------------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `WorkstationName`                                              | `Environment.MachineName` / `Model_WorkstationConfig`      | Activity logging and session timeout context                              | `Module_Core/Models/Systems/Model_UserSession.cs`, `Module_Core/Services/Authentication/Service_UserSessionManager.cs`   |
| `WorkstationType`                                              | `auth_workstation_config` via `sp_Auth_Terminal_GetShared` | Chooses Windows auto-login vs PIN/manual login flow                       | `Module_Core/Services/Authentication/Service_Authentication.cs`, `Module_Core/Models/Systems/Model_WorkstationConfig.cs` |
| `AuthenticationMethod`                                         | login workflow                                             | Distinguishes `windows_auto`, `pin_login`, and `manual_switch_user` flows | `Module_Core/Models/Systems/Model_UserSession.cs`, `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs` |
| `LoginTimestamp` / `LastActivityTimestamp` / `TimeoutDuration` | session manager                                            | Timeout monitoring and session-end audit logging                          | `Module_Core/Services/Authentication/Service_UserSessionManager.cs`                                                      |

## Privilege Fields

| Field                                     | Source Of Truth             | Runtime Use                                                            | Primary Code Touchpoints                                                                                                               |
| ----------------------------------------- | --------------------------- | ---------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| `settings_roles.role_name`                | MySQL settings role catalog | Maps role IDs to names like `User`, `Supervisor`, `Admin`, `Developer` | `Module_Settings.Core/Data/Dao_SettingsCoreRoles.cs`, `Database/Database_Deployment/Sql_Files/SeedData/02_seed_settingscore_roles.sql` |
| `settings_user_roles.user_id` / `role_id` | MySQL role assignments      | Determines effective privileges for the active session                 | `Module_Settings.Core/Data/Dao_SettingsCoreUserRoles.cs`, `Module_Core/Services/Authentication/Service_UserPrivileges.cs`              |

## Launch And Re-Login Flow Touchpoints

- App launch creates the window and then runs startup auth in `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`.
- Windows auto-login uses `AuthenticateByWindowsUsernameAsync` in `Module_Core/Services/Authentication/Service_Authentication.cs`.
- Shared-terminal and manual switch-user login use `View_Shared_SharedTerminalLoginDialog` plus `AuthenticateByPinAsync`.
- Session creation, timeout monitoring, and session end live in `Module_Core/Services/Authentication/Service_UserSessionManager.cs`.
- Session-bound privilege initialization now lives in `Module_Core/Services/Authentication/Service_UserPrivileges.cs` and is triggered by `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`.

## Review Notes

- The application uses protected PIN values at the DAO layer, not plaintext values.
- `VisualUsername` and `VisualPassword` are reversible encrypted values at rest and decrypted runtime values in `Model_User`.
- Manual switch-user login now shares the same session startup path used by app launch, which means privilege loading and settings-default initialization happen in both flows.

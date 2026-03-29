# Logout And Re-Login Impact Map

Last Updated: 2026-03-29

## Purpose

This file lists the XAML, C#, SQL, and Markdown artifacts that are involved in the logout,
manual re-login, and privilege-initialization feature set.

## XAML Files

| File                                                             | Purpose                                                | Status              |
| ---------------------------------------------------------------- | ------------------------------------------------------ | ------------------- |
| `MainWindow.xaml`                                                | Adds the top-right user menu entry point for `Log Out` | Implemented         |
| `Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml` | Manual credential dialog reused for switch-user login  | Reused              |
| `Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml`        | Remains part of first-run user creation at startup     | Existing dependency |

## C# Files

| File                                                                  | Purpose                                                                               | Status              |
| --------------------------------------------------------------------- | ------------------------------------------------------------------------------------- | ------------------- |
| `MainWindow.xaml.cs`                                                  | Handles the shell logout click and resets the shell after a successful re-login       | Implemented         |
| `Module_Shared/ViewModels/ViewModel_Shared_MainWindow.cs`             | Delegates logout and manual re-login to the new coordinator                           | Implemented         |
| `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`      | Startup login now uses the same session-initialization path used by switch-user login | Implemented         |
| `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs` | New coordinator for session startup, logout, and manual switch-user login             | Implemented         |
| `Module_Core/Services/Authentication/Service_UserPrivileges.cs`       | New session privilege loader/cache                                                    | Implemented         |
| `Module_Settings.Core/Services/SetSettingCommandHandler.cs`           | Uses initialized session privileges for settings permission checks                    | Implemented         |
| `Module_Volvo/Services/Service_VolvoAuthorization.cs`                 | Uses initialized session privileges for Volvo authorization checks                    | Implemented         |
| `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`         | Registers the new coordinator and privilege services                                  | Implemented         |
| `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`      | Updates Volvo authorization registration to use the shared privilege service          | Implemented         |
| `Module_Core/Services/Authentication/Service_UserSessionManager.cs`   | Existing session timer and logout cleanup dependency                                  | Existing dependency |
| `Module_Core/Services/Authentication/Service_Authentication.cs`       | Existing Windows/PIN authentication dependency                                        | Existing dependency |
| `Module_Core/Data/Authentication/Dao_User.cs`                         | Existing PIN hashing and user lookup dependency                                       | Existing dependency |

## SQL Files

| File                                                                                                  | Purpose                                                                  | Status              |
| ----------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ | ------------------- |
| `Database/Database_Deployment/Sql_Files/StoredProcedures/Authentication/sp_Auth_User_ValidatePin.sql` | Documents that the stored procedure receives a protected PIN value       | Updated             |
| `Database/Database_Deployment/Sql_Files/SeedData/06_seed_auth_user_bootstrap_accounts.sql`            | Seeds `admin` and `developer` users with role assignments                | Implemented         |
| `Database/Database_Deployment/Sql_Files/Schemas/02_Table_auth_users.sql`                              | Existing auth-user storage contract referenced by the new seed           | Existing dependency |
| `Database/Database_Deployment/Sql_Files/Schemas/19_Table_settings_user_roles.sql`                     | Existing role-assignment table used during privilege initialization      | Existing dependency |
| `Database/Database_Deployment/Sql_Files/SeedData/02_seed_settingscore_roles.sql`                      | Existing role catalog required for seed assignment and privilege loading | Existing dependency |

## Markdown Files

| File                                                                | Purpose                                                          | Status      |
| ------------------------------------------------------------------- | ---------------------------------------------------------------- | ----------- |
| `docs/Modules/Module_Receiving/User-Identity-Footprint.md`          | Review inventory of user-related fields and login touchpoints    | Implemented |
| `docs/Modules/Module_Receiving/Permission-Gated-Features-Review.md` | Review list of Receiving features that need permission attention | Implemented |
| `docs/Modules/Module_Receiving/Logout-ReLogin-Impact-Map.md`        | This impact map                                                  | Implemented |

## Follow-Up Candidates

- `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` for edit-mode role gating after review approval.
- `Module_Receiving/Services/Service_ReceivingWorkflow.cs` for default-mode fallback when `edit` is not authorized.
- `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs` and `Module_Receiving/Services/Service_MySQL_Receiving.cs` for destructive Receiving-role enforcement after review approval.
- `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-workflow.json` and `docs/CopilotForms/data/module-metadata/Module_Receiving/receiving-edit-mode.json` when Receiving permission policy is finalized.

# User Role Assignment Logic Verification

## Flow Diagram

```
User Creates Account
    ↓
sp_Auth_User_Create (creates user in auth_users)
    ↓
Service_Authentication.CreateNewUserAsync()
    ↓
Service_OnStartup_AppLifecycle.AssignDefaultRoleIfMissingAsync()
    ↓
Dao_SettingsCoreRoles.GetByNameAsync("User")
    → sp_SettingsCore_Roles_GetByName
    → SELECT from settings_roles WHERE role_name = "User"
    ↓
Dao_SettingsCoreUserRoles.AssignRoleAsync(employeeNumber, roleId)
    → sp_SettingsCore_UserRoles_Assign
    → INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
```

## Database Tables Involved

### auth_users
- `employee_number` INT PRIMARY KEY - User's unique identifier
- `windows_username` VARCHAR(50) - Windows login
- `full_name` VARCHAR(100)
- Created by: `sp_Auth_User_Create`

### settings_roles
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `role_name` VARCHAR(50) UNIQUE
- `description` VARCHAR(255)
- `created_at` DATETIME
- Seeded by: INSERT statement in sp_SettingsCore.sql
  - 'User', 'Supervisor', 'Admin', 'Developer'

### settings_user_roles
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `user_id` INT NOT NULL - References auth_users.employee_number
- `role_id` INT NOT NULL - References settings_roles.id
- `assigned_at` DATETIME
- UNIQUE KEY: (user_id, role_id)
- Populated by: `sp_SettingsCore_UserRoles_Assign`

## Code Components

### 1. Service_OnStartup_AppLifecycle
- Location: `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- Called when: New user created (line ~99)
- Method: `AssignDefaultRoleIfMissingAsync(int employeeNumber)`
- Logic:
  1. Check if user has any roles: `_userRolesDao.GetByUserAsync(employeeNumber)`
  2. If no roles, get "User" role: `_rolesDao.GetByNameAsync("User")`
  3. Assign role: `_userRolesDao.AssignRoleAsync(employeeNumber, roleId)`

### 2. Dao_SettingsCoreRoles
- Location: `Module_Settings.Core/Data/Dao_SettingsCoreRoles.cs`
- Method: `GetByNameAsync(string roleName)`
- Executes: `sp_SettingsCore_Roles_GetByName`
- Parameters: `p_role_name` → "User"

### 3. Dao_SettingsCoreUserRoles
- Location: `Module_Settings.Core/Data/Dao_SettingsCoreUserRoles.cs`
- Method: `AssignRoleAsync(int userId, int roleId)`
- Executes: `sp_SettingsCore_UserRoles_Assign`
- Parameters:
  - `p_user_id` → employeeNumber
  - `p_role_id` → roleId

### 4. Stored Procedures
Location: `Database/StoredProcedures/Settings/sp_SettingsCore.sql`

#### sp_SettingsCore_Roles_GetByName
```sql
SELECT id, role_name, description, created_at
FROM settings_roles
WHERE role_name = p_role_name
LIMIT 1;
```

#### sp_SettingsCore_UserRoles_Assign
```sql
IF NOT EXISTS (SELECT 1 FROM settings_user_roles 
               WHERE user_id = p_user_id AND role_id = p_role_id)
THEN
    INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
    VALUES (p_user_id, p_role_id, NOW());
END IF;
```

## Dependency Injection Registration
Location: `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` (lines 384-394)

```csharp
services.AddSingleton(_ => new Dao_SettingsCoreRoles(mySqlConnectionString));
services.AddSingleton(_ => new Dao_SettingsCoreUserRoles(mySqlConnectionString));
services.AddSingleton<IService_SettingsErrorHandler, Service_SettingsErrorHandler>();
```

## Verification Points

- [ ] Are the 'User', 'Supervisor', 'Admin', 'Developer' roles in settings_roles?
- [ ] Is Dao_SettingsCoreRoles registered in DI?
- [ ] Is Dao_SettingsCoreUserRoles registered in DI?
- [ ] Are both DAOs injected in Service_OnStartup_AppLifecycle?
- [ ] Does settings_user_roles table exist and have correct schema?
- [ ] Are the stored procedures sp_SettingsCore_Roles_GetByName and sp_SettingsCore_UserRoles_Assign deployed?
- [ ] Is AssignDefaultRoleIfMissingAsync called after user creation?

## Debugging Steps

1. Check debug output for `[RoleAssignment]` logs
2. Verify the role lookup succeeds
3. Verify the role assignment succeeds
4. Query: `SELECT * FROM settings_user_roles WHERE user_id = <employeeNumber>`

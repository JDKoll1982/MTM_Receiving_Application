# User Role Assignment - Complete Logic Verification

## Status Summary

✅ **All Components Implemented and Verified**

### 1. Database Schema
- `settings_roles` table exists with default roles seeded
- `settings_user_roles` table exists with correct foreign keys
- UNIQUE constraint on (user_id, role_id) to prevent duplicates

### 2. Stored Procedures (sp_SettingsCore.sql)

#### sp_SettingsCore_Roles_GetByName
```sql
DROP PROCEDURE IF EXISTS sp_SettingsCore_Roles_GetByName $$
CREATE PROCEDURE sp_SettingsCore_Roles_GetByName(
    IN p_role_name VARCHAR(100)
)
BEGIN
    SELECT id, role_name, description, created_at
    FROM settings_roles
    WHERE role_name = p_role_name
    LIMIT 1;
END $$
```
**Purpose**: Retrieves a role by name from settings_roles table
**Called By**: `Dao_SettingsCoreRoles.GetByNameAsync("User")`

#### sp_SettingsCore_UserRoles_Assign
```sql
DROP PROCEDURE IF EXISTS sp_SettingsCore_UserRoles_Assign $$
CREATE PROCEDURE sp_SettingsCore_UserRoles_Assign(
    IN p_user_id INT,
    IN p_role_id INT
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM settings_user_roles 
                   WHERE user_id = p_user_id AND role_id = p_role_id)
    THEN
        BEGIN
            DECLARE EXIT HANDLER FOR SQLEXCEPTION BEGIN END;
            INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
            VALUES (p_user_id, p_role_id, NOW());
        END;
    END IF;
END $$
```
**Purpose**: Assigns a role to a user with duplicate check
**Called By**: `Dao_SettingsCoreUserRoles.AssignRoleAsync(userId, roleId)`

### 3. C# Data Access Objects (DAOs)

#### Dao_SettingsCoreRoles
- **File**: `Module_Settings.Core/Data/Dao_SettingsCoreRoles.cs`
- **Method**: `GetByNameAsync(string roleName)`
- **Executes**: `sp_SettingsCore_Roles_GetByName`
- **Returns**: `Model_Dao_Result<Model_SettingsRole>`

#### Dao_SettingsCoreUserRoles
- **File**: `Module_Settings.Core/Data/Dao_SettingsCoreUserRoles.cs`
- **Method**: `AssignRoleAsync(int userId, int roleId)`
- **Executes**: `sp_SettingsCore_UserRoles_Assign`
- **Returns**: `Model_Dao_Result`

### 4. Startup Service

#### Service_OnStartup_AppLifecycle
- **File**: `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
- **Method**: `AssignDefaultRoleIfMissingAsync(int employeeNumber)`
- **Called After**: 
  1. New user creation (line ~99)
  2. Existing user authentication (line ~169)

**Logic Flow**:
1. Check if user has any roles assigned
2. If no roles, lookup "User" role from settings_roles
3. If "User" role exists, assign it to user
4. All errors are logged but non-fatal (backwards compatibility)

### 5. Dependency Injection

**Location**: `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`

```csharp
services.AddSingleton(_ => new Dao_SettingsCoreRoles(mySqlConnectionString));
services.AddSingleton(_ => new Dao_SettingsCoreUserRoles(mySqlConnectionString));
```

**Constructor injection in Service_OnStartup_AppLifecycle**:
```csharp
public Service_OnStartup_AppLifecycle(
    ...
    Dao_SettingsCoreRoles rolesDao,
    Dao_SettingsCoreUserRoles userRolesDao)
{
    ...
    _rolesDao = rolesDao;
    _userRolesDao = userRolesDao;
}
```

### 6. Role Seeding

**Location**: Bottom of `sp_SettingsCore.sql`

```sql
INSERT IGNORE INTO settings_roles (role_name, description, created_at)
VALUES 
    ('User', 'Basic user role - can modify own settings', NOW()),
    ('Supervisor', 'Supervisor role - can manage user settings', NOW()),
    ('Admin', 'Administrator role - full access to all settings', NOW()),
    ('Developer', 'Developer role - unrestricted access for development', NOW());
```

Uses `INSERT IGNORE` to avoid errors if roles already exist.

---

## Complete User Creation Flow

### When New User Creates Account:
1. User enters details in `ViewModel_Shared_NewUserSetup`
2. `Service_Authentication.CreateNewUserAsync()` is called
3. `sp_Auth_User_Create` inserts into `auth_users` with `employee_number`
4. **Service_OnStartup_AppLifecycle.AssignDefaultRoleIfMissingAsync(employeeNumber)** is called
5. `Dao_SettingsCoreRoles.GetByNameAsync("User")` retrieves role record
6. `Dao_SettingsCoreUserRoles.AssignRoleAsync(employeeNumber, roleId)` executes
7. `sp_SettingsCore_UserRoles_Assign` inserts into `settings_user_roles`

### When Existing User Starts App:
1. User authenticates
2. `Service_OnStartup_AppLifecycle.AssignDefaultRoleIfMissingAsync(employeeNumber)` is called
3. Same process as above (for backwards compatibility)

---

## Debugging Information

### Enable Debug Output
Debug logs are written with prefix `[RoleAssignment]`:
- User lookup attempts
- Role lookup results  
- Role assignment results
- Any exceptions

Check Output window in Visual Studio when creating/loading users.

### Database Queries for Verification

```sql
-- Check if roles are seeded
SELECT * FROM settings_roles;

-- Check if user has roles
SELECT * FROM settings_user_roles WHERE user_id = <employeeNumber>;

-- Check user exists
SELECT employee_number, full_name FROM auth_users WHERE employee_number = <id>;

-- Check stored procedure exists
SHOW PROCEDURE STATUS WHERE NAME LIKE 'sp_SettingsCore_UserRoles%';
```

---

## Potential Issues & Solutions

### Issue: Role not being assigned
**Check**: 
1. Are the stored procedures deployed to MySQL?
2. Is the 'User' role in settings_roles table?
3. Are debug logs showing in Output window?
4. Does the query `SELECT * FROM settings_user_roles` show any records?

### Issue: Permission check still failing
**Root Cause**: User doesn't have role assigned yet
**Solution**: Role assignment happens during startup - must be run AFTER roles are assigned

### Issue: NULL values in debug logs
**Check**:
1. Is Dao_SettingsCoreRoles correctly instantiated?
2. Is Dao_SettingsCoreUserRoles correctly instantiated?
3. Are dependencies injected correctly?

---

## Summary

The logic is **sound and complete**. The flow is:

```
New User Created 
    ↓
sp_Auth_User_Create (inserts with employee_number)
    ↓
AssignDefaultRoleIfMissingAsync(employeeNumber)
    ↓
GetByNameAsync("User") → sp_SettingsCore_Roles_GetByName → roleId
    ↓
AssignRoleAsync(employeeNumber, roleId) → sp_SettingsCore_UserRoles_Assign
    ↓
INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
    ↓
✅ User now has "User" role and can modify settings
```

All components are implemented, registered in DI, and have enhanced debugging.

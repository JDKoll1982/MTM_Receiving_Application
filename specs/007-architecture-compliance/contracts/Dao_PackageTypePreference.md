# Dao_PackageTypePreference Contract

**Type**: Data Access Object  
**Namespace**: MTM_Receiving_Application.Data.Receiving  
**Purpose**: Instance-based DAO for MySQL `package_type_preferences` table operations

---

## Class Definition

```csharp
namespace MTM_Receiving_Application.Data.Receiving;

public class Dao_PackageTypePreference
{
    private readonly string _connectionString;
    
    public Dao_PackageTypePreference(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    // Methods documented below
}
```

---

## Constructor

```csharp
public Dao_PackageTypePreference(string connectionString)
```

**Parameters**:
- `connectionString` - MySQL connection string

**Validation**:
- Throws ArgumentNullException if connectionString is null

**DI Registration** (App.xaml.cs):
```csharp
services.AddSingleton(sp => new Dao_PackageTypePreference(mySqlConnectionString));
```

---

## Methods

### GetByUserAsync

**Purpose**: Retrieve a user's package type preference.

**Signature**:
```csharp
public async Task<Model_Dao_Result<Model_PackageTypePreference>> GetByUserAsync(string username)
```

**Stored Procedure**: `sp_package_preferences_get_by_user`

**Parameters**:
- `username` (string) - Windows username or PIN-authenticated user

**Returns**: `Model_Dao_Result<Model_PackageTypePreference>`
- Success with Data: Preference found
- Success with Data = null: User has no preference (not an error)
- Failure: Database error

**Parameter Mapping**:
```csharp
var parameters = new Dictionary<string, object>
{
    { "username", username }
};
```

**SQL Logic**:
```sql
SELECT 
    preference_id AS PreferenceId,
    username AS Username,
    preferred_package_type AS PreferredPackageType,
    last_updated AS LastUpdated,
    workstation AS Workstation
FROM package_type_preferences
WHERE username = @username;
```

---

### UpsertAsync

**Purpose**: Insert new preference or update existing preference (UPSERT pattern).

**Signature**:
```csharp
public async Task<Model_Dao_Result> UpsertAsync(Model_PackageTypePreference preference)
```

**Stored Procedure**: `sp_package_preferences_upsert`

**Parameters**:
- `username` (string)
- `preferred_package_type` (string) - "Package" or "Pallet"
- `workstation` (string) - Optional workstation identifier

**Returns**: `Model_Dao_Result`
- Success: Preference inserted or updated
- Failure: Error message (e.g., invalid package type)

**Parameter Mapping**:
```csharp
var parameters = new Dictionary<string, object>
{
    { "username", preference.Username },
    { "preferred_package_type", preference.PreferredPackageType },
    { "workstation", preference.Workstation ?? string.Empty }
};

return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
    _connectionString,
    "sp_package_preferences_upsert",
    parameters);
```

**SQL Logic** (ON DUPLICATE KEY UPDATE pattern):
```sql
INSERT INTO package_type_preferences (username, preferred_package_type, workstation, last_updated)
VALUES (@username, @preferred_package_type, @workstation, NOW())
ON DUPLICATE KEY UPDATE 
    preferred_package_type = VALUES(preferred_package_type),
    workstation = VALUES(workstation),
    last_updated = NOW();
```

---

## Error Handling

```csharp
public async Task<Model_Dao_Result<Model_PackageTypePreference>> GetByUserAsync(string username)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            { "username", username }
        };
        
        var result = await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_PackageTypePreference>(
            _connectionString,
            "sp_package_preferences_get_by_user",
            parameters);
        
        // Null result is success (user has no preference)
        return result.IsSuccess
            ? DaoResultFactory.Success(result.Data?.FirstOrDefault())
            : DaoResultFactory.Failure<Model_PackageTypePreference>(result.ErrorMessage);
    }
    catch (Exception ex)
    {
        return DaoResultFactory.Failure<Model_PackageTypePreference>(
            $"Error retrieving package preference for {username}: {ex.Message}",
            ex);
    }
}
```

---

## Stored Procedures Required

### sp_package_preferences_get_by_user.sql

```sql
DROP PROCEDURE IF EXISTS sp_package_preferences_get_by_user;

DELIMITER //
CREATE PROCEDURE sp_package_preferences_get_by_user(IN p_username VARCHAR(100))
BEGIN
    SELECT 
        preference_id AS PreferenceId,
        username AS Username,
        preferred_package_type AS PreferredPackageType,
        last_updated AS LastUpdated,
        workstation AS Workstation
    FROM package_type_preferences
    WHERE username = p_username;
END //
DELIMITER ;
```

### sp_package_preferences_upsert.sql

```sql
DROP PROCEDURE IF EXISTS sp_package_preferences_upsert;

DELIMITER //
CREATE PROCEDURE sp_package_preferences_upsert(
    IN p_username VARCHAR(100),
    IN p_preferred_package_type VARCHAR(20),
    IN p_workstation VARCHAR(100)
)
BEGIN
    INSERT INTO package_type_preferences 
        (username, preferred_package_type, workstation, last_updated)
    VALUES 
        (p_username, p_preferred_package_type, p_workstation, NOW())
    ON DUPLICATE KEY UPDATE 
        preferred_package_type = VALUES(preferred_package_type),
        workstation = VALUES(workstation),
        last_updated = NOW();
END //
DELIMITER ;
```

---

## Database Schema

If the table doesn't exist, create it:

```sql
CREATE TABLE IF NOT EXISTS package_type_preferences (
    preference_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    preferred_package_type VARCHAR(20) NOT NULL DEFAULT 'Package',
    last_updated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    workstation VARCHAR(100),
    UNIQUE KEY unique_user_preference (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Note**: MySQL 5.7.24 compatible (no CHECK constraints).

---

## Testing Approach

```csharp
[Fact]
public async Task UpsertAsync_InsertsNewPreference()
{
    // Arrange
    var dao = new Dao_PackageTypePreference(TestHelper.GetTestConnectionString());
    var preference = new Model_PackageTypePreference
    {
        Username = "TEST_USER",
        PreferredPackageType = "Pallet",
        Workstation = "WS001"
    };
    
    // Act
    var result = await dao.UpsertAsync(preference);
    
    // Assert
    Assert.True(result.IsSuccess);
}

[Fact]
public async Task GetByUserAsync_ReturnsNullForNewUser()
{
    // Arrange
    var dao = new Dao_PackageTypePreference(TestHelper.GetTestConnectionString());
    
    // Act
    var result = await dao.GetByUserAsync("NONEXISTENT_USER");
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Data); // No preference = null, not failure
}
```

---

## Related Documentation

- **Constitution**: Principle II (Database Layer Consistency)
- **Service Usage**: See `IService_MySQL_PackagePreferences` for service delegation pattern

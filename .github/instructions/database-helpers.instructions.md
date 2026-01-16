# Database Helper Standards

**Category**: Data Access
**Last Updated**: December 26, 2025
**Applies To**: `Helper_Database_StoredProcedure`, `Helper_Database_Variables`

## Overview

Direct usage of `MySqlConnection` and `MySqlCommand` in DAOs is **prohibited**. All MySQL interactions must go through `Helper_Database_StoredProcedure`. This ensures consistent error handling, logging, and retry logic.

## Helper_Database_StoredProcedure

### Key Methods

1. **`ExecuteStoredProcedureAsync<T>`**
    * **Use Case**: Fetching a list of records.
    * **Returns**: `Model_Dao_Result<List<T>>`
    * **Mapping**: Automatically maps columns to properties of `T` by name.

2. **`ExecuteStoredProcedureSingleAsync<T>`**
    * **Use Case**: Fetching a single record (e.g., GetById).
    * **Returns**: `Model_Dao_Result<T>`

3. **`ExecuteNonQueryAsync`**
    * **Use Case**: Insert, Update, Delete operations.
    * **Returns**: `Model_Dao_Result` (Success/Failure).

### Usage Example

```csharp
var parameters = new Dictionary<string, object>
{
    { "p_user_id", userId },
    { "p_active", true }
};

var result = await Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync<Model_User>(
    "sp_get_users",
    parameters
);

if (result.IsSuccess)
{
    var users = result.Data;
}
```

## Parameter Naming
* **C# Side**: Use dictionary keys matching the stored procedure parameter names (usually with `p_` prefix, e.g., `p_part_id`).
* **SQL Side**: Stored procedure parameters must match the dictionary keys.

## Connection Strings
* Access via `Helper_Database_Variables.GetConnectionString()`.
* Never hardcode connection strings in DAOs.

## Error Handling

The helper automatically catches exceptions, logs them, and returns a `Model_Dao_Result` with `IsSuccess = false`. DAOs generally do not need try-catch blocks around helper calls unless specific logic is needed.

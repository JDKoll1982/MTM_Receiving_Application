# IService_UserPreferences Contract

**Type**: Service Interface  
**Namespace**: MTM_Receiving_Application.Contracts.Services  
**Purpose**: Abstraction layer providing ViewModels with access to user preference data

---

## Interface Definition

```csharp
namespace MTM_Receiving_Application.Contracts.Services;

public interface IService_UserPreferences
{
    /// <summary>
    /// Retrieves the latest receiving mode preference for a specific user
    /// </summary>
    /// <param name="username">Windows username or PIN-authenticated username</param>
    /// <returns>Model_Dao_Result containing user preference or failure</returns>
    Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username);
    
    /// <summary>
    /// Updates the user's default receiving mode preference
    /// </summary>
    /// <param name="username">Windows username or PIN-authenticated username</param>
    /// <param name="defaultMode">Default mode: "Package" or "Pallet"</param>
    /// <returns>Model_Dao_Result indicating success or failure</returns>
    Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode);
}
```

---

## Methods

### GetLatestUserPreferenceAsync

**Purpose**: Retrieve user's most recent receiving mode preference from database.

**Parameters**:
- `username` (string): User identifier (normalized to uppercase in service implementation)

**Returns**: `Task<Model_Dao_Result<Model_UserPreference>>`
- **Success**: Result.Data contains Model_UserPreference with DefaultMode property
- **Failure**: Result.ErrorMessage describes why preference could not be retrieved
- **Null Case**: If user has no preference, returns success with null Data

**Business Rules** (implemented in Service_UserPreferences):
- Username normalized to uppercase before DAO call
- Empty/null username returns immediate failure
- Logs preference retrieval for audit trail
- Transforms database errors to user-friendly messages

**Example Usage** (from ViewModel):
```csharp
var result = await _userPreferencesService.GetLatestUserPreferenceAsync(Environment.UserName);
if (result.IsSuccess && result.Data != null)
{
    SelectedMode = result.Data.DefaultMode; // "Package" or "Pallet"
}
else
{
    // Use application default (e.g., "Package")
    SelectedMode = "Package";
}
```

---

### UpdateDefaultModeAsync

**Purpose**: Persist user's default receiving mode preference to database.

**Parameters**:
- `username` (string): User identifier
- `defaultMode` (string): Must be "Package" or "Pallet"

**Returns**: `Task<Model_Dao_Result>`
- **Success**: Result.IsSuccess = true, preference updated
- **Failure**: Result.ErrorMessage describes validation error or database error

**Business Rules** (implemented in Service_UserPreferences):
- Username normalized to uppercase
- defaultMode validated: must be exactly "Package" or "Pallet" (case-insensitive comparison)
- Invalid mode returns failure without database call
- Logs preference update for audit trail
- Upserts record (inserts if new user, updates if existing)

**Example Usage** (from ViewModel):
```csharp
var result = await _userPreferencesService.UpdateDefaultModeAsync(
    Environment.UserName, 
    "Pallet");

if (result.IsSuccess)
{
    StatusMessage = "Preference saved successfully";
}
else
{
    _errorHandler.ShowUserError(result.ErrorMessage, "Save Error", nameof(SavePreference));
}
```

---

## Implementation Class: Service_UserPreferences

**Location**: `Services/Database/Service_UserPreferences.cs`

**Dependencies** (constructor injection):
- `Dao_User` - Data access for user table
- `ILoggingService` - Audit logging
- `IService_ErrorHandler` - Exception handling and user error display

**Pattern**:
```csharp
public class Service_UserPreferences : IService_UserPreferences
{
    private readonly Dao_User _userDao;
    private readonly ILoggingService _logger;
    private readonly IService_ErrorHandler _errorHandler;
    
    public Service_UserPreferences(
        Dao_User userDao,
        ILoggingService logger,
        IService_ErrorHandler errorHandler)
    {
        _userDao = userDao;
        _logger = logger;
        _errorHandler = errorHandler;
    }
    
    // ... method implementations
}
```

**DI Registration** (App.xaml.cs):
```csharp
services.AddTransient<IService_UserPreferences, Service_UserPreferences>();
```

---

## Consumers

### ReceivingModeSelectionViewModel

**Current Violation**: Directly instantiates and calls `Dao_User.UpdateDefaultModeAsync()`

**Fixed Pattern**:
```csharp
public partial class ReceivingModeSelectionViewModel : BaseViewModel
{
    private readonly IService_UserPreferences _userPreferencesService;
    
    public ReceivingModeSelectionViewModel(
        IService_UserPreferences userPreferencesService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _userPreferencesService = userPreferencesService;
    }
    
    [RelayCommand]
    private async Task SavePreferenceAsync()
    {
        var result = await _userPreferencesService.UpdateDefaultModeAsync(
            CurrentUsername, 
            SelectedMode);
        
        if (!result.IsSuccess)
        {
            _errorHandler.ShowUserError(
                result.ErrorMessage, 
                "Save Preference Error", 
                nameof(SavePreferenceAsync));
        }
    }
}
```

---

## Testing Approach

### Unit Tests
- Mock `Dao_User` to return test data
- Verify username normalization logic
- Verify defaultMode validation (rejects invalid modes)
- Verify logging calls for audit trail

### Integration Tests
- Test with real Dao_User instance against test database
- Verify upsert behavior (insert new, update existing)
- Verify null username handling
- Verify database error propagation as Model_Dao_Result.Failure

---

## Related Documentation

- **Constitution**: Principle I (MVVM Architecture - ViewModel→Service→DAO pattern)
- **Service-DAO Pattern**: `.github/instructions/service-dao-pattern.instructions.md`
- **DAO Contract**: `Dao_User` (existing DAO - verify if static or instance-based)

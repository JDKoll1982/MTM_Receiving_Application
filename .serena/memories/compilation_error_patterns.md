# Common Compilation Error Patterns & Solutions

## Issue Type 1: Observable Property Naming Conventions

**Problem:**
When using `[ObservableProperty]` with acronyms in property names, the source generator follows strict PascalCase rules.

**Examples:**
- `_poNumber` → generates `PoNumber` (NOT `PONumber`)
- `_isPOValid` → generates `IsPOValid` (INCORRECT - leads to naming inconsistency)
- `_isPoValid` → generates `IsPoValid` (CORRECT)
- `_isNonPO` → generates `IsNonPO` (INCORRECT)
- `_isNonPo` → generates `IsNonPo` (CORRECT)

**Solution:**
Use lowercase for all letters in acronyms except the first:
- ✅ `_poNumber`, `_isPoValid`, `_isNonPo`
- ❌ `_PONumber`, `_isPOValid`, `_isNonPO`

**Partial Methods:**
When implementing partial methods for property change handlers, use the GENERATED property name:
```csharp
[ObservableProperty]
private string _poNumber = string.Empty;

// Generated property: PoNumber
// Partial method must match:
partial void OnPoNumberChanged(string value) { }
```

## Issue Type 2: ViewModel Base Class Constructor

**Problem:**
`ViewModel_Shared_Base` requires THREE parameters:
1. `IService_ErrorHandler`
2. `IService_LoggingUtility`
3. `IService_Notification` ⚠️ (often forgotten)

**Solution:**
```csharp
public MyViewModel(
    IService_ErrorHandler errorHandler,
    IService_LoggingUtility logger,
    IService_Notification notificationService) 
    : base(errorHandler, logger, notificationService) // All 3 required
{
}
```

## Issue Type 3: Namespace Typos in Generated Files

**Problem:**
Search-and-replace operations can corrupt namespaces if not careful:
- `DataTransferObjects` → `DataTransferObjectss` (double s)
- `BuildTools` → `BuilDataTransferObjectsols` (concatenated incorrectly)
- `OnNavigatedTo` → `OnNavigateDataTransferObjects`

**Prevention:**
- Use whole-word matching when replacing
- Verify package references in .csproj after bulk edits
- Check generated/auto-imported code for corruption

## Issue Type 4: Error Handler API

**Current API (IService_ErrorHandler):**
```csharp
// Async methods (use in async contexts):
Task HandleErrorAsync(string errorMessage, Enum_ErrorSeverity severity, Exception? exception = null, bool showDialog = true)
Task ShowErrorDialogAsync(string title, string message, Enum_ErrorSeverity severity)
Task ShowUserErrorAsync(string message, string title, string method)

// Sync methods (compatibility):
void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
```

**Common Mistakes:**
- ❌ `ShowUserError(...)` - doesn't exist
- ❌ `Enum_Shared_Severity_ErrorSeverity` - wrong enum
- ✅ `ShowUserErrorAsync(...)` - correct async version
- ✅ `Enum_ErrorSeverity` - correct enum

**Severity Values:**
- Info, Low, Warning, Medium, Error, Critical, Fatal
- ❌ NOT `High` - use `Critical` instead

## Issue Type 5: Entity vs DTO Naming

**Pattern:**
- **Entities** (database tables): `Model_Receiving_TableEntitys_XXX`
- **DTOs** (data transfer): `Model_Receiving_DataTransferObjects_XXX`

**Examples:**
- ✅ `Model_Receiving_TableEntitys_PartType` (entity)
- ✅ `Model_Receiving_DataTransferObjects_PartDetails` (DTO)
- ❌ `Model_Receiving_Entity_PartType` (wrong pattern)

## Issue Type 6: Query/Command Naming

**Actual Pattern in Codebase:**
- Queries: `QueryRequest_Receiving_Shared_Get_XXX` or `QueryRequest_Receiving_Shared_Validate_XXX`
- Commands: `CommandRequest_Receiving_Shared_Save_XXX` or custom names like `BulkCopyFieldsCommand`

**Common Mistakes:**
- ❌ `ValidatePONumberQuery` - wrong pattern
- ✅ `QueryRequest_Receiving_Shared_Validate_PONumber` - correct
- ❌ `GetPartDetailsQuery` - wrong pattern
- ✅ `QueryRequest_Receiving_Shared_Get_PartDetails` - correct

## Issue Type 7: JSON Serialization for Workflow State

**Problem:**
`WorkflowSession.LoadDetailsJson` is a `string` (serialized JSON), not a collection.

**Solution:**
```csharp
using System.Text.Json;

// Saving:
LoadDetailsJson = JsonSerializer.Serialize(Loads.ToList())

// Loading:
var loadDetails = JsonSerializer.Deserialize<List<Model_Receiving_DataTransferObjects_LoadGridRow>>(
    result.Data.LoadDetailsJson ?? "[]");
if (loadDetails != null)
{
    foreach (var load in loadDetails) Loads.Add(load);
}
```

## Issue Type 8: GUID vs String for SessionId

**Problem:**
- Database entity uses `Guid SessionId`
- ViewModel uses `string SessionId`

**Solution:**
```csharp
// When sending to command/query expecting Guid:
SessionId = Guid.TryParse(SessionId, out var guid) ? guid : Guid.NewGuid()

// When receiving from entity:
SessionId = result.Data.SessionId.ToString()
```

## Issue Type 9: Reference Data Structure

**Problem:**
`QueryRequest_Receiving_Shared_Get_ReferenceData` returns `Model_Receiving_DataTransferObjects_ReferenceData` which contains:
- `PartTypes` (List)
- `PackageTypes` (List)
- `Locations` (List)

**Common Mistake:**
```csharp
// ❌ Wrong - trying to iterate result.Data directly
foreach (var partType in result.Data) { }

// ✅ Correct - iterate the PartTypes property
foreach (var partType in result.Data.PartTypes) { }
```

## Issue Type 10: RelayCommand Compatibility

**Problem:**
`[RelayCommand]` can only be applied to methods compatible with command signatures.

**Invalid Signatures:**
```csharp
// ❌ Method with parameters that don't return Task
private async Task MyMethodAsync() { }  // Missing return or await

// ❌ Multi-parameter methods
[RelayCommand]
private void OnCellEditEnded(LoadGridRow load, string propertyName) { }
```

**Solutions:**
```csharp
// ✅ Remove [RelayCommand] for multi-param methods
public void OnCellEditEnded(LoadGridRow load, string propertyName) { }

// ✅ Ensure async methods return Task and await or return Task.CompletedTask
[RelayCommand]
private async Task MyMethodAsync() 
{ 
    await Task.CompletedTask; // or actual async work
}
```

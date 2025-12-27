# IService_MySQL_Dunnage Contract

**Interface**: `IService_MySQL_Dunnage`  
**Implementation**: `Service_MySQL_Dunnage`  
**Location**: `Contracts/Services/IService_MySQL_Dunnage.cs`  
**Registration**: Transient (stateless)

## Interface Definition

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for dunnage database operations.
    /// Wraps DAOs with business logic validation and error handling.
    /// </summary>
    public interface IService_MySQL_Dunnage
    {
        // ==================== Type Operations (5 methods) ====================
        
        Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
        Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
        Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type);
        Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
        Task<Model_Dao_Result> DeleteTypeAsync(int typeId);

        // ==================== Spec Operations (6 methods) ====================
        
        Task<Model_Dao_Result<Model_DunnageSpec>> GetSpecsForTypeAsync(int typeId);
        Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec);
        Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec);
        Task<Model_Dao_Result> DeleteSpecAsync(int specId);
        Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId);
        Task<List<string>> GetAllSpecKeysAsync();

        // ==================== Part Operations (7 methods) ====================
        
        Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync();
        Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId);
        Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId);
        Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part);
        Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part);
        Task<Model_Dao_Result> DeletePartAsync(string partId);
        Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId);

        // ==================== Load Operations (6 methods) ====================
        
        Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads);
        Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end);
        Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync();
        Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid);
        Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load);
        Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid);

        // ==================== Inventory Operations (6 methods) ====================
        
        Task<bool> IsPartInventoriedAsync(string partId);
        Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId);
        Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync();
        Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item);
        Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId);
        Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item);

        // ==================== Impact Analysis (4 methods) ====================
        
        Task<int> GetPartCountByTypeIdAsync(int typeId);
        Task<int> GetTransactionCountByPartIdAsync(string partId);
        Task<int> GetTransactionCountByTypeIdAsync(int typeId);
        Task<int> GetPartCountBySpecKeyAsync(int typeId, string specKey);
    }
}
```

## Service Responsibilities

This service is a **thin validation layer** over DAOs. It provides:
1. **Business Logic Validation**: Fail-fast checks before DAO calls
2. **Impact Analysis**: Check dependencies before delete operations
3. **Error Wrapping**: Convert DAO errors to user-friendly messages
4. **Logging**: Audit trail for all operations
5. **User Auditing**: Inject current user from `IService_UserSessionManager` for write operations

## Method Groups

### Type Operations
**Purpose**: Manage dunnage types (Pallet, Divider, etc.)

**Validation**:
- `DeleteTypeAsync`: Check part count first via `GetPartCountByTypeIdAsync()`. If > 0, return error without calling DAO.
- `InsertTypeAsync`: Validate TypeName is not empty.
- `UpdateTypeAsync`: Validate TypeID exists.

**User Auditing**: Insert/Update/Delete methods retrieve current user via `IService_UserSessionManager.CurrentSession.User.EmployeeNumber`.

---

### Spec Operations
**Purpose**: Manage spec schemas for types

**Validation**:
- `InsertSpecAsync`: Validate `SpecSchema` is valid JSON using `System.Text.Json.JsonDocument.Parse()`.
- `UpdateSpecAsync`: Same JSON validation.
- `DeleteSpecAsync`: Check if any parts use this spec key via `GetPartCountBySpecKeyAsync()`. If > 0, warn user (don't block).
- `GetAllSpecKeysAsync`: Query all specs, parse JSON, extract keys, deduplicate, sort alphabetically.

**User Auditing**: Insert/Update/Delete operations.

---

### Part Operations
**Purpose**: Manage dunnage parts

**Validation**:
- `InsertPartAsync`: Validate PartID is unique (DAO enforces, but service can pre-check for better UX).
- `DeletePartAsync`: Check transaction count via `GetTransactionCountByPartIdAsync()`. If > 0, return error.
- `SearchPartsAsync`: Supports wildcard search with optional type filter.

**User Auditing**: Insert/Update/Delete operations.

---

### Load Operations
**Purpose**: Manage dunnage receiving transactions

**Validation**:
- `SaveLoadsAsync`: Validate loads list is not null or empty. Each load must have valid PartID.
- Batch operation - all or nothing transaction (handled by DAO/stored procedure).

**User Auditing**: All write operations.

---

### Inventory Operations
**Purpose**: Manage inventoried parts list

**Validation**:
- `RemoveFromInventoriedListAsync`: Check if part has pending transactions. If yes, warn user but allow removal.
- `IsPartInventoriedAsync`: Quick boolean check (bypasses Model_Dao_Result for performance).

**User Auditing**: Add/Remove/Update operations.

---

### Impact Analysis
**Purpose**: Support delete validation and UI warnings

**Returns**: Integer count (not Model_Dao_Result).

**Use Cases**:
- Before deleting type: "Type 'Pallet' has 15 parts. Delete anyway?"
- Before deleting part: "Part 'PALLET-001' has 50 transactions. Cannot delete."
- Before deleting spec: "5 parts use this spec key. Values will be lost."

---

## Error Handling Pattern

```csharp
public async Task<Model_Dao_Result> DeleteTypeAsync(int typeId)
{
    try
    {
        // Fail-fast validation
        int partCount = await GetPartCountByTypeIdAsync(typeId);
        if (partCount > 0) {
            return Model_Dao_Result.Failure(
                $"Cannot delete type: {partCount} parts depend on it. Delete parts first."
            );
        }

        // Get user for auditing
        var user = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;

        // Delegate to DAO
        var result = await Dao_DunnageType.DeleteAsync(typeId, user);
        
        // Wrap error with context
        if (!result.IsSuccess) {
            _logger.LogError($"Failed to delete type {typeId}: {result.ErrorMessage}");
            return Model_Dao_Result.Failure($"Delete type failed: {result.ErrorMessage}");
        }

        _logger.LogInfo($"Type {typeId} deleted by user {user}");
        return Model_Dao_Result.Success();
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(DeleteTypeAsync), nameof(Service_MySQL_Dunnage));
        return Model_Dao_Result.Failure($"Unexpected error: {ex.Message}");
    }
}
```

---

## GetAllSpecKeysAsync() Implementation Strategy

```csharp
public async Task<List<string>> GetAllSpecKeysAsync()
{
    try
    {
        // 1. Get all specs from database
        var result = await Dao_DunnageSpec.GetAllAsync();
        if (!result.IsSuccess || result.Data.Count == 0) {
            return new List<string>(); // Empty list, not error
        }

        // 2. Parse JSON schemas and extract keys
        var allKeys = new HashSet<string>();
        foreach (var spec in result.Data)
        {
            try {
                var json = JsonDocument.Parse(spec.SpecSchema);
                foreach (var property in json.RootElement.EnumerateObject()) {
                    allKeys.Add(property.Name);
                }
            }
            catch {
                // Log malformed JSON, continue
                _logger.LogWarning($"Malformed JSON in Spec {spec.SpecID}");
            }
        }

        // 3. Sort and return
        return allKeys.OrderBy(k => k).ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError($"GetAllSpecKeysAsync failed: {ex.Message}");
        return new List<string>(); // Return empty, don't fail
    }
}
```

---

## Testing Strategy

**Unit Tests** (Mock DAOs):
- Test validation logic (fail-fast before DAO calls)
- Test error wrapping (DAO errors → user-friendly messages)
- Test impact analysis queries
- Test GetAllSpecKeysAsync() JSON parsing

**Integration Tests** (Real Database):
- Test DAO delegation
- Test transaction behavior (SaveLoadsAsync batch)
- Test user auditing (current user captured)

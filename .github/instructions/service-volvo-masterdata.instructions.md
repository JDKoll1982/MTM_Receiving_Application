# Service_VolvoMasterData - Implementation Guide

**Service Name:** Service_VolvoMasterData
**Interface:** IService_VolvoMasterData
**Module:** Module_Volvo
**Purpose:** Master data management for Volvo parts, components, and assemblies

---

## Overview

Service_VolvoMasterData handles CRUD operations and business logic for the Volvo parts catalog. It manages parts, component relationships (bill of materials), and data validation for the master data tables.

---

## Architecture

### Dependencies (Constructor Injection)
```csharp
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_LoggingUtility _logger;
```

**Registration** (App.xaml.cs):
```csharp
services.AddSingleton<IService_VolvoMasterData>(sp =>
{
    var partDao = sp.GetRequiredService<Dao_VolvoPart>();
    var componentDao = sp.GetRequiredService<Dao_VolvoPartComponent>();
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    return new Service_VolvoMasterData(partDao, componentDao, logger);
});
```

---

## Core Responsibilities

### 1. Part Management
- Get all active parts
- Get part by part number (ID)
- Add new parts
- Update existing parts
- Deactivate parts (soft delete)
- Validate part data before save

### 2. Component Management
- Get components for parent part (BOM explosion)
- Add component relationships
- Update component quantities
- Remove component relationships
- Validate no circular dependencies

### 3. Data Validation
- Part number format (uppercase, no spaces)
- Quantity per skid > 0
- Description not empty
- No duplicate part numbers
- Component references valid parts

---

## Method Patterns

### GetAllActivePartsAsync
**Purpose:** Retrieve all active (is_active=1) parts for catalog/dropdown lists

**Returns:** `Model_Dao_Result<List<Model_VolvoPart>>`

**Use Cases:**
- Populate AutoSuggestBox in shipment entry
- Display parts catalog in master data management
- Export parts list for reporting

**Sorting:** Parts should be ordered by `part_number` ascending

---

### GetPartByNumberAsync
**Purpose:** Retrieve single part by part number

**Returns:** `Model_Dao_Result<Model_VolvoPart>`

**Validation:**
- ✅ partNumber not null/empty
- ✅ Part exists in database

**Error Handling:**
- Return `Success=false` with "Part not found" if not exists
- Do NOT throw exception

---

### SavePartAsync
**Purpose:** Insert new or update existing part

**Validation Rules:**
```csharp
✅ Part number required (not null/empty)
✅ Part number uppercase only
✅ Part number no spaces
✅ Description required
✅ Quantity per skid > 0
✅ No duplicate part numbers (case-insensitive)
```

**Logic:**
```csharp
if (part.Id == 0)
{
    // INSERT new part
    return await _partDao.InsertAsync(part);
}
else
{
    // UPDATE existing part
    return await _partDao.UpdateAsync(part);
}
```

**Normalization:**
- Trim whitespace from part number and description
- Convert part number to uppercase
- Set is_active = true for new parts

---

### DeactivatePartAsync
**Purpose:** Soft delete part by setting is_active=0

**Business Rule:** ⚠️ Cannot deactivate if part is used in:
- Active shipments (status='pending_po' or recent completed)
- Component relationships (as parent or component)

**Validation Flow:**
```csharp
1. Check if part used in active shipments
   - Query volvo_line_data WHERE part_number=X AND shipment.is_archived=0
2. Check if part has component relationships
   - Query volvo_part_components WHERE parent_part_number=X OR component_part_number=X
3. If any references exist → return error
4. Otherwise → set is_active=0
```

**Recommended Fix (Issue #7):**
Create stored procedure `sp_volvo_part_check_references` to perform checks atomically

---

### GetComponentsByParentAsync
**Purpose:** Get all components for a parent part (BOM)

**Returns:** `Model_Dao_Result<List<Model_VolvoPartComponent>>`

**Example:**
```
Parent: V-EMB-500
Components:
  - V-EMB-2 (Qty: 1, Qty/Skid: 1)
  - V-EMB-92 (Qty: 1, Qty/Skid: 1)
```

**Used By:** Service_Volvo.CalculateComponentExplosionAsync()

---

### AddComponentAsync
**Purpose:** Add component relationship to parent part

**Validation:**
```csharp
✅ Parent part exists and is active
✅ Component part exists and is active
✅ Quantity > 0
✅ Component qty per skid > 0
✅ No circular dependency (component cannot be ancestor of parent)
✅ No duplicate (parent+component combination must be unique)
```

**Circular Dependency Check:**
```
INVALID:
  A → B → C → A (circular)

VALID:
  A → B → C
  A → D
```

**Algorithm:**
```csharp
1. Get all components of proposed component part
2. Recursively check if parent part appears anywhere in tree
3. If found → return "Circular dependency detected"
```

---

### RemoveComponentAsync
**Purpose:** Delete component relationship

**Validation:**
- ✅ Relationship exists
- ⚠️ Optional: Check if used in recent shipments before removing

---

## Data Integrity Rules

### Part Number Standards
- **Format:** Uppercase letters, numbers, hyphens only
- **Example:** `V-EMB-500`, `V-EMB-2`
- **Validation:** `^[A-Z0-9-]+$` regex pattern
- **Auto-normalize:** Convert to uppercase on save

### Quantity Validation
- **QuantityPerSkid:** Must be > 0 (parts come in whole units)
- **Component Quantity:** Integer ≥ 1 (how many components per parent)
- **Component QuantityPerSkid:** Integer ≥ 1 (pieces per component skid)

### Soft Delete Policy
- **Never** physically delete parts (breaks historical data)
- Set `is_active=0` to hide from active catalogs
- Keep deactivated parts in database for audit trail
- Allow reactivation if needed (set is_active=1)

---

## Error Handling

### Common Error Scenarios

**"Part number already exists"**
```csharp
var existingPart = await _partDao.GetByIdAsync(part.PartNumber);
if (existingPart.Success && existingPart.Data != null && existingPart.Data.Id != part.Id)
{
    return Model_Dao_Result_Factory.Failure("Part number already exists");
}
```

**"Cannot deactivate part - used in active shipments"**
```csharp
var shipmentsResult = await _partDao.GetActiveShipmentReferencesAsync(partNumber);
if (shipmentsResult.Success && shipmentsResult.Data.Count > 0)
{
    return Model_Dao_Result_Factory.Failure(
        $"Cannot deactivate - used in {shipmentsResult.Data.Count} active shipment(s)");
}
```

**"Circular dependency detected"**
```csharp
if (await HasCircularDependencyAsync(parentPartNumber, componentPartNumber))
{
    return Model_Dao_Result_Factory.Failure(
        $"Cannot add component - circular dependency: {componentPartNumber} references {parentPartNumber}");
}
```

---

## Business Logic Examples

### Example 1: Save New Part
```csharp
var newPart = new Model_VolvoPart
{
    PartNumber = "v-emb-600", // Will be normalized to V-EMB-600
    Description = "Container Base",
    QuantityPerSkid = 44,
    IsActive = true
};

var result = await _masterDataService.SavePartAsync(newPart);

if (result.Success)
{
    int newId = result.Data; // Auto-generated ID
}
```

### Example 2: Add Component to Assembly
```csharp
var component = new Model_VolvoPartComponent
{
    ParentPartNumber = "V-EMB-500",
    ComponentPartNumber = "V-EMB-2",
    Quantity = 1, // 1 component per parent
    ComponentQuantityPerSkid = 1 // 1 piece per skid
};

var result = await _masterDataService.AddComponentAsync(component);
```

### Example 3: Deactivate Unused Part
```csharp
var result = await _masterDataService.DeactivatePartAsync("V-EMB-OLD");

if (!result.Success)
{
    // Part is referenced - cannot deactivate
    MessageBox.Show(result.ErrorMessage);
}
```

---

## Performance Considerations

### Caching Strategy
Consider caching frequently accessed data:
- **Active parts list** - Reload on part add/update/deactivate
- **Component relationships** - Cache per parent part
- **Cache invalidation** - On any master data change

### Batch Operations
For bulk imports:
```csharp
// Instead of:
foreach (var part in parts)
{
    await SavePartAsync(part); // N database calls
}

// Use transaction:
using var transaction = BeginTransaction();
foreach (var part in parts)
{
    await _partDao.InsertInTransactionAsync(part, transaction);
}
transaction.Commit();
```

---

## Testing Checklist

- [ ] Can add part with valid data
- [ ] Cannot add part with duplicate number
- [ ] Cannot add part with invalid quantity (≤0)
- [ ] Part number normalized to uppercase
- [ ] Can update existing part
- [ ] Can deactivate unused part
- [ ] Cannot deactivate part used in active shipment
- [ ] Cannot deactivate part with components
- [ ] Can add component relationship
- [ ] Cannot add component creating circular dependency
- [ ] Cannot add duplicate component relationship
- [ ] Can remove component relationship
- [ ] All errors return Model_Dao_Result with Success=false

---

## Common Issues & Solutions

### Issue: "Cannot deactivate part" even though no active shipments visible
**Cause:** Part used in archived shipments or as component
**Solution:** Check `volvo_part_components` table, implement proper reference check

### Issue: Circular dependency not detected
**Cause:** Incomplete recursive check
**Solution:** Implement breadth-first search through entire component tree

### Issue: Part number case mismatch causing duplicates
**Cause:** Database collation case-sensitive
**Solution:** Always normalize to uppercase before database operations

---

## Future Enhancements

1. **Import/Export** - CSV import for bulk part loading
2. **Part History** - Audit trail of changes (who/when)
3. **Cost Tracking** - Add cost per piece for inventory value
4. **Images** - Attach photos of parts
5. **Barcode Support** - Generate/scan barcodes for parts
6. **Supplier Info** - Track vendor part numbers and lead times

---

## Related Documentation

- [IService_VolvoMasterData Interface](../../Module_Core/Contracts/Services/IService_VolvoMasterData.cs)
- [Service_Volvo Guide](./service-volvo.instructions.md)
- [DAO Pattern Guide](./dao-pattern.instructions.md)
- [Database Schema](../../Database/Schemas/volvo_schema.sql)

---

**Version:** 1.0
**Last Updated:** January 5, 2026
**Maintained By:** Development Team

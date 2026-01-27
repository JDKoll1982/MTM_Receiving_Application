# Dunnage Type Configuration

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Dynamic Specification Fields](./dynamic-specification-fields.md), [Type Management Settings](../../Module_Settings.Dunnage/01-Settings-Categories/dunnage-type-management.md)

---

## Overview

Dunnage Type Configuration defines the rules governing the creation, modification, and lifecycle management of dunnage types. Each type represents a category of reusable packaging material (pallets, boxes, racks, etc.) with its own icon, specifications, and business rules.

---

## Type Definition

### Required Properties

**Every dunnage type MUST have:**

1. **Type Name** (string, 1-100 characters)
   - Unique identifier for the type
   - User-friendly display name
   - Examples: "Wood Pallet 48x40", "Cardboard Box - Large", "Metal Rack - Standard"

2. **Icon** (string, icon identifier)
   - Visual representation in UI
   - Selected from predefined icon library
   - Examples: 🪵 (wood pallet), 📦 (box), 🏗️ (rack)

3. **Active Status** (boolean)
   - `true` = Available for selection in workflows
   - `false` = Hidden from user selection (soft delete)

4. **Display Order** (integer, 0-999)
   - Controls sort order in type selection UI
   - Lower numbers appear first
   - Allows administrators to prioritize frequently used types

### Optional Properties

5. **Description** (string, 0-500 characters)
   - Extended description for admin reference
   - Not displayed in workflow UI
   - Helps document type purpose and usage guidelines

6. **Specification Fields** (list of spec field definitions)
   - 0-N custom fields per type
   - See [Dynamic Specification Fields](./dynamic-specification-fields.md)

7. **Associated Parts** (list of part IDs)
   - Parts configured to use this dunnage type
   - See [Part-Type Associations](./part-type-associations.md)

---

## Business Rules

### Rule 1: Type Name Uniqueness

**Definition**: No two active dunnage types can have the same name (case-insensitive).

**Validation**:
```
When creating or renaming a type:
- Check if name exists (case-insensitive comparison)
- If exists and type is active: BLOCK save, show error
- If exists and type is inactive: WARN admin, allow reactivation
```

**Error Message**:
```
"A dunnage type with the name '{TypeName}' already exists. Please use a different name."
```

**Edge Case**: See [CLARIFICATIONS.md - Edge Case 2](../CLARIFICATIONS.md#edge-case-2-duplicate-dunnage-type-names)

---

### Rule 2: Icon Assignment

**Definition**: Each type must have a valid icon identifier.

**Validation**:
```
When saving a type:
- Icon field cannot be empty
- Icon identifier must exist in predefined icon library
- If icon missing: Use default icon (📦)
- If icon invalid: Show warning, use default icon
```

**Default Icon**: 📦 (generic box icon)

**Icon Library** (predefined):
```
🪵 - Wood materials (pallets, crates)
📦 - Cardboard boxes
🏗️ - Metal structures (racks, frames)
🗃️ - Plastic containers
📋 - Generic/other
```

**Custom Icons**: See [CLARIFICATIONS.md - Edge Case 3](../CLARIFICATIONS.md#edge-case-3-type-icon-missing-or-invalid) for custom icon support decision.

---

### Rule 3: Active Status Management

**Definition**: Active status controls type visibility in workflow selection screens.

**Behavior**:

**Active = true:**
- Type appears in Type Selection (Step 1 of Guided Mode)
- Type available for Manual Entry Mode
- Type selectable in Admin Mode for editing

**Active = false:**
- Type hidden from workflow selection
- Historical data with this type remains accessible
- Type still editable in Admin Mode (can be reactivated)
- Soft delete (data preserved, not hard deleted)

**Deactivation Rules**:
```
When deactivating a type:
1. Check for active workflows using this type
2. If found: WARN admin, allow confirmation
3. On confirm: Deactivate immediately
4. Active workflows continue with cached type data
5. New workflows cannot select deactivated type
```

**Reactivation**:
```
When reactivating a type:
- Validate type still has required fields (name, icon)
- Validate spec field definitions still valid
- If valid: Reactivate immediately
- If invalid: Show validation errors, block reactivation
```

**Edge Case**: See [CLARIFICATIONS.md - Edge Case 1](../CLARIFICATIONS.md#edge-case-1-dunnage-type-deactivation-during-active-workflow)

---

### Rule 4: Display Order

**Definition**: Display Order determines type sorting in selection UI.

**Sorting Algorithm**:
```
1. Sort types by Display Order (ascending)
2. If tied, sort by Type Name (alphabetical, case-insensitive)
3. Inactive types excluded from sort (not displayed)
```

**Example**:
```
Display Order: 10 → "Wood Pallet 48x40" (shows first)
Display Order: 20 → "Cardboard Box - Large"
Display Order: 30 → "Metal Rack - Standard"
Display Order: 30 → "Plastic Tote" (tied, alphabetical order)
```

**Admin Control**:
- Drag-and-drop reordering in Admin Mode
- Manual entry of display order value
- Automatic resequencing option (gaps = 10)

---

### Rule 5: Type Deletion

**Definition**: Types can be soft-deleted (deactivated) or hard-deleted (permanently removed).

**Soft Delete (Recommended)**:
```
Action: Set Active = false
Result:
- Type hidden from workflows
- Historical data preserved
- Can be reactivated
- Audit trail maintained
```

**Hard Delete (Restricted)**:
```
Preconditions:
- Type must have zero historical transactions
- Type must have zero spec fields
- Type must have zero part associations
- Confirmation required from admin

If preconditions met:
- Permanently delete type record
- Irreversible action

If preconditions NOT met:
- Block deletion
- Show error: "Cannot delete type with existing data. Deactivate instead."
```

**Recommendation**: Always use soft delete to preserve data integrity.

**Edge Case**: See [CLARIFICATIONS.md - Edge Case 6](../CLARIFICATIONS.md#edge-case-6-spec-field-deletion-with-existing-data) for related field deletion rules.

---

### Rule 6: Type Creation Workflow

**Required Steps**:
```
1. Enter Type Name (validate uniqueness)
2. Select Icon (from library or upload custom)
3. Set Active Status (default: true)
4. Set Display Order (default: max + 10)
5. Add Description (optional)
6. Add Specification Fields (at least 1 recommended)
7. Associate Parts (optional, can be done later)
8. Save Type
```

**Validation Points**:
```
Step 1: Name uniqueness check
Step 2: Icon validity check
Step 6: Spec field validation (names, types, defaults)
Step 8: Final validation before persist
```

**Success Criteria**:
- Type saved to database
- Available in Type Selection (if Active = true)
- Spec fields available for dynamic rendering
- Audit trail entry created

---

## UI Implementation

### Type Selection - Guided Mode Step 1

**Layout**: Card-based grid

```
┌────────────┬────────────┬────────────┬───────────┐
│   🪵       │    📦      │    🏗️     │    📋    │
│            │            │            │           │
│ Wood Pallet│ Cardboard  │ Metal Rack │  Plastic  │
│  48x40     │    Box     │            │   Tote    │
│            │            │            │           │
│  [Select]  │  [Select]  │  [Select]  │ [Select]  │
└────────────┴────────────┴────────────┴───────────┘
```

**Sorting**: By Display Order (ascending), then alphabetically

**Search**: Filter by type name (real-time)

---

### Admin Mode - Type Management

**Type List View**:
```
┌──────────────────────────────────────────────────────┐
│ Dunnage Type Management                              │
│ ──────────────────────────────────────────────────  │
│                                                       │
│ [➕ Add Type]  [🔍 Search: _______________]          │
│                                                       │
│ ┌─────┬─────────────────┬──────┬────────┬─────────┐│
│ │Order│ Type Name       │ Icon │ Active │ Actions ││
│ ├─────┼─────────────────┼──────┼────────┼─────────┤│
│ │ 10  │ Wood Pallet 48x40│ 🪵  │   ✓    │ Edit|Del││
│ │ 20  │ Cardboard Box   │ 📦  │   ✓    │ Edit|Del││
│ │ 30  │ Metal Rack      │ 🏗️  │   ✓    │ Edit|Del││
│ │ 40  │ Plastic Tote    │ 📋  │        │ Edit|Del││
│ └─────┴─────────────────┴──────┴────────┴─────────┘│
│                                                       │
│ ⬆⬇ Drag to reorder                                  │
└──────────────────────────────────────────────────────┘
```

**Add/Edit Type Dialog**:
```
┌──────────────────────────────────────────────────────┐
│ Add Dunnage Type                                     │
│ ──────────────────────────────────────────────────  │
│                                                       │
│ Type Name *                                          │
│ ┌──────────────────────────────────────────────────┐│
│ │ Wood Pallet 48x40                                ││
│ └──────────────────────────────────────────────────┘│
│                                                       │
│ Icon *                                               │
│ ┌──────────────────────────────────────────────────┐│
│ │ 🪵  🪵 Wood/Lumber                            ▼ ││
│ └──────────────────────────────────────────────────┘│
│                                                       │
│ Description                                          │
│ ┌──────────────────────────────────────────────────┐│
│ │ Standard 48" x 40" wooden pallet for shipping    ││
│ │ and storage of tube assemblies.                  ││
│ └──────────────────────────────────────────────────┘│
│                                                       │
│ ☑ Active (visible in workflows)                     │
│                                                       │
│ Display Order                                        │
│ ┌──────────────────────────────────────────────────┐│
│ │ 10                                          ▲▼  ││
│ └──────────────────────────────────────────────────┘│
│                                                       │
│            [Cancel]  [Save] [Save & Add Specs]       │
└──────────────────────────────────────────────────────┘
```

---

## Integration Points

### Database Tables

**Primary Table**: `dunnage_types`
```sql
CREATE TABLE dunnage_types (
    type_id INT AUTO_INCREMENT PRIMARY KEY,
    type_name VARCHAR(100) NOT NULL UNIQUE,
    icon VARCHAR(50) NOT NULL,
    description VARCHAR(500),
    active BOOLEAN DEFAULT TRUE,
    display_order INT DEFAULT 0,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    modified_date DATETIME ON UPDATE CURRENT_TIMESTAMP,
    modified_by VARCHAR(50),
    INDEX idx_active_order (active, display_order)
);
```

**Related Tables**:
- `dunnage_specs` - Specification field definitions per type
- `dunnage_part_type_associations` - Part-type relationships
- `dunnage_loads` - Transaction data referencing type_id

---

### Service Layer

**Interface**: `IService_MySQL_Dunnage`

**Methods**:
```csharp
// Type CRUD
Task<Model_Dao_Result<List<Model_DunnageType>>> GetActiveTypesAsync();
Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
Task<Model_Dao_Result> CreateTypeAsync(Model_DunnageType type);
Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
Task<Model_Dao_Result> DeactivateTypeAsync(int typeId);
Task<Model_Dao_Result> DeleteTypeAsync(int typeId); // Hard delete

// Type-specific operations
Task<Model_Dao_Result> ValidateTypeNameUniqueAsync(string typeName, int? excludeTypeId);
Task<Model_Dao_Result> ReorderTypesAsync(List<int> typeIdsInOrder);
```

---

## Validation Examples

### Valid Type Configurations

**Example 1: Wood Pallet**
```json
{
  "type_name": "Wood Pallet 48x40",
  "icon": "🪵",
  "description": "Standard 48x40 wooden pallet",
  "active": true,
  "display_order": 10,
  "spec_fields": [
    { "name": "Condition", "type": "Dropdown", "required": true },
    { "name": "Supplier", "type": "Text", "required": true },
    { "name": "Weight Capacity", "type": "Number", "required": true }
  ]
}
```
✅ Valid - Has all required fields, unique name, valid icon

**Example 2: Cardboard Box**
```json
{
  "type_name": "Cardboard Box - Large",
  "icon": "📦",
  "active": true,
  "display_order": 20,
  "spec_fields": [
    { "name": "Size", "type": "Dropdown", "required": true }
  ]
}
```
✅ Valid - Minimal configuration, still acceptable

---

### Invalid Type Configurations

**Example 3: Duplicate Name**
```json
{
  "type_name": "Wood Pallet 48x40", // Already exists
  "icon": "🪵",
  "active": true
}
```
❌ Invalid - Duplicate name (case-insensitive check fails)  
**Error**: "A dunnage type with the name 'Wood Pallet 48x40' already exists."

**Example 4: Missing Icon**
```json
{
  "type_name": "Plastic Tote",
  "icon": "", // Empty
  "active": true
}
```
❌ Invalid - Icon required  
**Error**: "Icon is required. Please select an icon."

**Example 5: Invalid Display Order**
```json
{
  "type_name": "Metal Rack",
  "icon": "🏗️",
  "display_order": -5 // Negative
}
```
❌ Invalid - Display order must be >= 0  
**Error**: "Display Order must be between 0 and 999."

---

## Related Documentation

- [Dynamic Specification Fields](./dynamic-specification-fields.md) - Spec field system
- [Part-Type Associations](./part-type-associations.md) - Part linking
- [Type Management Settings](../../Module_Settings.Dunnage/01-Settings-Categories/dunnage-type-management.md) - Admin UI details
- [CLARIFICATIONS.md](../CLARIFICATIONS.md) - Edge cases requiring decisions

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

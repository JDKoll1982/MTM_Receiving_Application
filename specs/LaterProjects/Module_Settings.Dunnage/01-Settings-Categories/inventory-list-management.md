# Inventory List Management

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Admin Mode](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md), [Inventory Tracking Business Rule](../../Module_Dunnage/01-Business-Rules/inventory-tracking.md)

---

## Purpose

Inventory List Management allows administrators to configure frequently used dunnage type-part combinations for one-click access in workflows. This "quick-add" feature speeds up data entry for common receiving scenarios.

---

## Access

**Location**: Admin Mode → Inventory Management  
**Permission**: Administrator only  
**URL**: `/admin/dunnage/inventory` (if web-based future)

---

## Inventory List View

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Inventory List Management                      [Back to Dashboard]  [✕] Exit│
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Configure frequently used dunnage type-part combinations for quick access   │
│ in workflows. Items appear in "Add from Inventory" quick-add dialogs.       │
│                                                                              │
│ [➕ Add Item]  [🗑️ Remove Selected]  [⬆⬇ Reorder]  [Auto-Populate]         │
│                                                                              │
│ INVENTORY ITEMS (12 items)                                                  │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ┌───────────────────────────────────────────────────────────────────────┐ │
│ │⬆⬇│☑│Priority│ Type              │ Part           │ Usage │ Actions  ││ │
│ ├──┼─┼────────┼───────────────────┼────────────────┼───────┼──────────┤│ │
│ │⬍⬍│☐│   1    │ Wood Pallet 48x40 │ TUBE-A123      │  45   │ Edit|Del ││ │
│ │⬍⬍│☐│   2    │ Cardboard Box     │ TUBE-A123      │  32   │ Edit|Del ││ │
│ │⬍⬍│☐│   3    │ Wood Pallet 48x40 │ FRAME-B456     │  28   │ Edit|Del ││ │
│ │⬍⬍│☐│   4    │ Metal Rack        │ PANEL-D012     │  15   │ Edit|Del ││ │
│ │⬍⬍│☐│   5    │ Cardboard Box     │ BRACKET-E345   │  12   │ Edit|Del ││ │
│ │⬍⬍│☐│   10   │ Wood Pallet 48x40 │ HOUSING-G901   │   8   │ Edit|Del ││ │
│ │⬍⬍│☐│   15   │ Plastic Tote      │ TUBE-B456      │   5   │ Edit|Del ││ │
│ │...                                                                     ││ │
│ └───────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ ⬆⬇ Drag rows to change priority (higher priority = appears first in dialogs)│
│                                                                              │
│ Usage = Number of times this combination was received in last 90 days       │
│                                                                              │
│ ℹ Priority gaps are allowed (e.g., 1, 2, 3, 10, 15). This allows inserting │
│   new items without renumbering existing priorities.                        │
│                                                                              │
│                                                    [Close]  [Save Changes]   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Grid Features

### Columns

**⬆⬇ (Drag Handle)**:
- Drag to reorder rows
- Visual indicator during drag
- Drop to change priority

**☑ (Checkbox)**:
- Select for bulk operations
- Multi-select enabled

**Priority** (Number):
- Determines display order in workflows
- Lower number = higher priority (shows first)
- Editable inline or via edit dialog
- Gaps allowed (recommended: 1, 2, 3, 10, 15, ...)

**Type** (Read-Only):
- Dunnage type name with icon
- Click to edit item

**Part** (Read-Only):
- Part number and description
- Click to edit item

**Usage** (Read-Only):
- Count of times this combination was used
- Calculated from transaction history (last 90 days)
- Auto-updates daily
- Sortable for insight into most-used combinations

**Actions**:
- **Edit**: Opens edit dialog
- **Del**: Delete with confirmation

---

### Drag-and-Drop Reordering

**Behavior**:
```
User drags row:
→ Visual indicator shows drop position
→ On drop:
  → Recalculate priority based on position
  → Priority = avg of prev and next priorities
  → If result is integer, add 0.5 and round
  → Save to database
  → Update UI immediately
```

**Example**:
```
Initial state:
Priority 1: Wood Pallet + TUBE-A123
Priority 2: Cardboard Box + TUBE-A123
Priority 5: Wood Pallet + FRAME-B456
Priority 10: Metal Rack + PANEL-D012

User drags "Metal Rack + PANEL-D012" between priorities 1 and 2:
→ New priority = (1 + 2) / 2 = 1.5 → rounds to 2
→ Existing priority 2 shifts to 3

Result:
Priority 1: Wood Pallet + TUBE-A123
Priority 2: Metal Rack + PANEL-D012
Priority 3: Cardboard Box + TUBE-A123
Priority 5: Wood Pallet + FRAME-B456
```

---

### Auto-Populate Feature

**Purpose**: Automatically populate inventory list based on usage statistics.

**Button**: "Auto-Populate" (toolbar)

**Dialog**:
```
┌─────────────────────────────────────────────────────┐
│ Auto-Populate Inventory List                        │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Automatically add the most frequently used type-part│
│ combinations to the inventory list.                 │
│                                                      │
│ Number of items to add:                             │
│ ┌─────────────────────────────────────────────────┐│
│ │ 10                                         ▲▼  ││
│ └─────────────────────────────────────────────────┘│
│                                                      │
│ Based on transaction history:                       │
│ ● Last 30 days                                      │
│ ○ Last 90 days                                      │
│ ○ Last 12 months                                    │
│                                                      │
│ Action:                                             │
│ ● Add to existing items (append)                   │
│ ○ Replace all items (clear and populate)           │
│                                                      │
│ ☐ Only include active parts and types              │
│                                                      │
│                      [Cancel]  [Populate]           │
└─────────────────────────────────────────────────────┘
```

**Logic**:
```sql
-- Query to find top combinations
SELECT 
    type_id,
    part_id,
    COUNT(*) as usage_count
FROM dunnage_loads
WHERE created_date >= DATE_SUB(NOW(), INTERVAL 30 DAY)
  AND active = 1
GROUP BY type_id, part_id
ORDER BY usage_count DESC
LIMIT 10;
```

---

## Add/Edit Inventory Item Dialog

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Add Inventory Item                                          [?] Help  [✕]   │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ ITEM DETAILS                                                                │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Dunnage Type *                                                              │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 🪵 Wood Pallet 48x40                                              ▼ │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ Part *                                                                      │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ TUBE-A123 - Tube Assembly A123                                     ▼ │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ⚠ Part must be associated with selected type. Filter shows valid parts.    │
│                                                                              │
│ Priority (1 = highest, appears first)                                       │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 1                                                                 ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Lower numbers appear first. Gaps allowed (e.g., 1, 2, 5, 10).            │
│                                                                              │
│ PREVIEW                                                                     │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ This item will appear in workflows as:                                      │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 🪵 Wood Pallet 48x40 → TUBE-A123                              (45)   │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│                                    [Cancel]                [Add]             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Field Definitions

### Dunnage Type (Required)

**Control**: Dropdown

**Data Source**: All active dunnage types

**Display Format**: `{Icon} {Type Name}`

**Example**:
```
Dropdown options:
🪵 Wood Pallet 48x40
📦 Cardboard Box - Large
🏗️ Metal Rack - Standard
📋 Plastic Tote
```

---

### Part (Required)

**Control**: Dropdown with filtering

**Data Source**: Parts associated with selected type

**Filtering Logic**:
```
When user selects Type:
→ Query: SELECT parts WHERE part_type_associations.type_id = selected_type_id
→ Populate Part dropdown with filtered results
→ Show only parts that can use selected type
```

**Display Format**: `{Part Number} - {Description}`

**Example**:
```
If Type = "Wood Pallet 48x40":
Dropdown shows:
TUBE-A123 - Tube Assembly A123
TUBE-B456 - Tube Assembly B456
FRAME-C789 - Frame Assembly C789
(Only parts associated with Wood Pallet)
```

**Validation**:
```
If no parts associated with selected type:
    Error: "No parts are configured for this dunnage type.
            Please configure part-type associations first."
    Action: Disable Part dropdown, disable Add button
```

---

### Priority (Optional)

**Control**: Number spinner

**Default**: Auto-assigned as max(priority) + 1

**Validation**:
```
Rules:
- Integer only (1-999)
- Duplicates allowed (sorted by usage_count within same priority)
- Recommended: Use gaps (1, 2, 5, 10, 15) for future insertions

Valid Examples:
✅ 1
✅ 5
✅ 10
✅ 999

Invalid Examples:
❌ 0 (min is 1)
❌ 1000 (max is 999)
❌ 1.5 (integers only)
```

**Behavior**:
```
If priority not specified:
→ Auto-assign: SELECT MAX(priority) + 1 FROM inventory_quick_list

If priority conflicts with existing item:
→ Allowed (no uniqueness constraint)
→ Secondary sort by usage_count DESC
```

---

## Validation Rules

### Rule 1: Unique Combination

**Definition**: Each Type-Part combination can only appear once in inventory list.

**Validation**:
```
When admin adds item:
If combination (Type + Part) already exists:
    Error: "This combination already exists in inventory list:
            • 🪵 Wood Pallet 48x40 → TUBE-A123 (Priority: 1)
            
            Edit existing item or choose different combination."
    Action: Block save
```

---

### Rule 2: Valid Association

**Definition**: Part must be associated with Type (from part-type associations).

**Validation**:
```
When admin selects Type and Part:
If NOT EXISTS (
    SELECT 1 FROM part_type_associations 
    WHERE type_id = selected_type 
    AND part_id = selected_part
):
    Warning: "Part is not associated with this type.
              This may cause issues in workflows. Continue?"
    Severity: Warning (allows save with confirmation)
```

**Recommended Approach**: Filter Part dropdown to show only associated parts (prevents this warning).

---

## Usage Tracking

### Automatic Increment

**Trigger**: User selects inventory item in workflow

**Logic**:
```csharp
// In Guided Mode or Manual Entry Mode
public async Task SelectInventoryItemAsync(int inventoryId)
{
    // Increment usage count (fire-and-forget, don't block UI)
    _ = Task.Run(async () =>
    {
        await _dao.IncrementUsageCountAsync(inventoryId);
    });
    
    // Pre-fill workflow with type and part from inventory item
    await LoadTypeAndPartFromInventoryAsync(inventoryId);
}
```

**SQL**:
```sql
UPDATE inventory_quick_list 
SET usage_count = usage_count + 1,
    last_used_date = NOW()
WHERE inventory_id = @inventoryId;
```

**Rollback Behavior**:
```
If user cancels transaction before save:
→ Usage count NOT decremented
→ Intentional: Shows user interest/intent

If transaction saved successfully:
→ Usage count already incremented
→ Reflects actual receiving activity
```

---

### Usage Display

**In Admin Mode Grid**:
```
Usage column:
- Shows count for last 90 days (configurable)
- Refreshed daily via background job
- Sortable (click column header)
```

**Color Coding (Optional)**:
```
High usage (>30):   Green background
Medium usage (10-30): Yellow background
Low usage (<10):     Gray background
Zero usage (0):      Red background (consider removing)
```

---

### Usage Reports (Future)

**Report Features**:
- Top 10 most-used combinations
- Unused combinations (candidates for removal)
- Usage trends over time
- Seasonal patterns

---

## Bulk Operations

### Remove Selected

**Access**: Select rows via checkbox → Click "🗑️ Remove Selected"

**Confirmation**:
```
┌─────────────────────────────────────────────────────┐
│ Confirm Remove Items                                │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Remove 3 selected items from inventory list?        │
│                                                      │
│ Items to be removed:                                │
│ • 🪵 Wood Pallet 48x40 → TUBE-A123 (45 uses)        │
│ • 📦 Cardboard Box → TUBE-A123 (32 uses)            │
│ • 🏗️ Metal Rack → PANEL-D012 (15 uses)              │
│                                                      │
│ Users will no longer see these as quick-add options.│
│ This will not affect historical transaction data.   │
│                                                      │
│                      [Cancel]  [Remove]             │
└─────────────────────────────────────────────────────┘
```

**Behavior**:
```
On Remove confirmation:
→ DELETE FROM inventory_quick_list WHERE inventory_id IN (...)
→ Invalidate cache
→ Log audit entry
→ Refresh grid
```

---

## Workflow Integration

### Guided Mode Integration

**Location**: Guided Mode → Step 1 → "Add from Inventory" button

**Dialog Display**:
```
┌─────────────────────────────────────────────────────┐
│ Select from Inventory                               │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Frequently used combinations:                       │
│                                                      │
│ ┌─────────────────────────────────────────────────┐│
│ │ 🪵 Wood Pallet 48x40 → TUBE-A123       (45)    ││
│ │ 📦 Cardboard Box → TUBE-A123           (32)    ││
│ │ 🪵 Wood Pallet 48x40 → FRAME-B456      (28)    ││
│ │ 🏗️ Metal Rack → PANEL-D012             (15)    ││
│ │ 📦 Cardboard Box → BRACKET-E345        (12)    ││
│ │ ...                                             ││
│ └─────────────────────────────────────────────────┘│
│                                                      │
│ Click to pre-fill type and part                    │
│                                                      │
│                              [Cancel]               │
└─────────────────────────────────────────────────────┘
```

**Sorting in Dialog**:
```sql
ORDER BY priority ASC, usage_count DESC
```

**Selection Behavior**:
```
User clicks: "🪵 Wood Pallet 48x40 → TUBE-A123"

Actions:
1. Set workflow Type = "Wood Pallet 48x40"
2. Set workflow Part = "TUBE-A123"
3. Increment usage_count for this inventory item
4. Close inventory dialog
5. Navigate to Step 3 (Quantity Entry)
```

---

### Manual Entry Mode Integration

**Location**: Manual Entry Grid → Toolbar → "Add from Inventory" button

**Behavior**:
```
User clicks "Add from Inventory"
→ Shows same inventory dialog
→ User selects combination
→ New row added to grid with pre-filled Type and Part
→ User enters Quantity and Spec fields
→ Usage count incremented
```

---

## Performance Optimization

### Caching

**Strategy**:
```
Cache Duration: 15 minutes (stable data)
Cache Key: "inventory_quick_list"

Cache Contents:
- All inventory items with type and part details
- Sorted by priority ASC, usage_count DESC
- Pre-rendered display strings

Invalidation:
- On inventory CRUD operations
- Manual cache clear (admin tool)
- Expired after 15 minutes
```

---

### Database Optimization

**Indexes**:
```sql
-- Primary key
CREATE UNIQUE INDEX idx_inventory_id ON inventory_quick_list(inventory_id);

-- Unique combination constraint
CREATE UNIQUE INDEX idx_inventory_type_part ON inventory_quick_list(type_id, part_id);

-- Priority sorting
CREATE INDEX idx_inventory_priority ON inventory_quick_list(priority, usage_count DESC);

-- Type lookup
CREATE INDEX idx_inventory_type ON inventory_quick_list(type_id);

-- Part lookup
CREATE INDEX idx_inventory_part ON inventory_quick_list(part_id);
```

---

## Error Handling

### Empty Inventory List

**Workflow Dialog Display**:
```
┌─────────────────────────────────────────────────────┐
│ Select from Inventory                               │
│ ───────────────────────────────────────────────────│
│                                                      │
│       No inventory items configured.               │
│                                                      │
│       Contact your administrator to configure      │
│       frequently used dunnage combinations.        │
│                                                      │
│                              [OK]                    │
└─────────────────────────────────────────────────────┘
```

**Admin Grid Display**:
```
┌─────────────────────────────────────────────────────┐
│ No inventory items configured.                      │
│                                                      │
│ Add frequently used type-part combinations to      │
│ speed up data entry for your users.                │
│                                                      │
│ [➕ Add First Item]  [Auto-Populate]                │
└─────────────────────────────────────────────────────┘
```

---

## Related Documentation

- [Admin Mode Specification](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md)
- [Inventory Tracking Business Rule](../../Module_Dunnage/01-Business-Rules/inventory-tracking.md)
- [Guided Mode](../../Module_Dunnage/02-Workflow-Modes/001-guided-mode-specification.md)
- [Manual Entry Mode](../../Module_Dunnage/02-Workflow-Modes/002-manual-entry-mode-specification.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

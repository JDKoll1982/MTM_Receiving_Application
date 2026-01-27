# Inventory Quick-Add Lists

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Inventory Management Settings](../../Module_Settings.Dunnage/01-Settings-Categories/inventory-list-management.md), [Admin Mode](../02-Workflow-Modes/004-admin-mode-specification.md)

---

## Rule Definition

Inventory Quick-Add Lists provide administrators with the ability to configure frequently used dunnage type-part combinations for one-click access in workflows. This speeds up data entry for common receiving scenarios.

---

## Business Rule

### Rule: Inventory List Display

**Definition**: When enabled, workflows show an "Add from Inventory" button that displays pre-configured type-part combinations.

**Availability**:
- ✅ Guided Mode - Step 1 (Type Selection) - "Add from Inventory" button
- ✅ Manual Entry Mode - Grid toolbar - "Add from Inventory" button
- ❌ Edit Mode - Not applicable (modifying historical data)

**Display Order**: Items sorted by Priority (ascending), then by usage count (descending).

---

## Configuration

### Admin Configuration

**Location**: Admin Mode → Inventory Management

**Each Inventory Item Contains**:
- Dunnage Type (required)
- Part Number (required)
- Priority (optional, default: auto-assigned)
- Usage Count (system-calculated, read-only)

**Example Configuration**:
```
Priority | Type              | Part           | Usage Count
---------|-------------------|----------------|-------------
   1     | Wood Pallet 48x40 | TUBE-A123      | 45
   2     | Cardboard Box     | TUBE-A123      | 32
   3     | Wood Pallet 48x40 | FRAME-B456     | 28
   4     | Metal Rack        | PANEL-D012     | 15
```

---

## Workflow Integration

### Guided Mode Integration

**Step 1: Type Selection**

```
┌─────────────────────────────────────────────────────┐
│ Select Dunnage Type                                 │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Choose type manually:                               │
│ [🪵 Wood Pallet] [📦 Cardboard Box] [🏗️ Metal Rack]│
│                                                      │
│ OR                                                  │
│                                                      │
│ [📋 Add from Inventory]                             │
│                                                      │
│                              [Cancel]  [Next]        │
└─────────────────────────────────────────────────────┘
```

**When "Add from Inventory" Clicked**:
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
│ └─────────────────────────────────────────────────┘│
│                                                      │
│ Click to pre-fill type and part                    │
│                                                      │
│                              [Cancel]               │
└─────────────────────────────────────────────────────┘
```

**Behavior on Selection**:
```
User clicks: "🪵 Wood Pallet 48x40 → TUBE-A123"

System actions:
1. Set Type = Wood Pallet 48x40
2. Set Part = TUBE-A123
3. Close inventory dialog
4. Navigate to Step 3 (Quantity Entry)
5. Increment usage count for this inventory item
```

---

### Manual Entry Mode Integration

**Grid Toolbar**

```
┌────────────────────────────────────────────────────────┐
│ [➕ Add Row]  [📋 Add from Inventory]  [🗑️ Delete]    │
└────────────────────────────────────────────────────────┘
```

**When "Add from Inventory" Clicked**:
```
Same dialog as Guided Mode

On selection:
1. Add new row to grid
2. Pre-fill Type and Part columns
3. Focus Quantity column
4. Increment usage count
```

---

## Usage Count Tracking

### Auto-Increment Rule

**Definition**: Usage count increments every time an inventory item is used to create a dunnage load.

**Tracking Logic**:
```
When user selects inventory item:
→ Increment usage_count in database
→ Update in-memory cache

When transaction is saved:
→ Usage counts already incremented (no rollback)

When transaction is cancelled:
→ Usage counts NOT decremented (intentional - still shows interest)
```

**Rationale**: Usage count reflects both completed and attempted uses, showing which combinations users consider/need most often.

---

### Usage Count Display

**In Admin Mode**:
- Shows total usage count (all time)
- Sortable by usage count
- Color-coded: High (green), Medium (yellow), Low (gray)

**In Workflow Dialogs**:
- Shows usage count in parentheses: `(45)`
- Helps users identify most common combinations

---

## Priority vs Usage Count

**Priority** (Admin-Controlled):
- Manually set by administrator
- Determines display order
- Lower number = higher priority (displays first)
- Can override usage-based sorting

**Usage Count** (System-Calculated):
- Automatically tracked by system
- Read-only (admin cannot modify)
- Secondary sort (within same priority)
- Provides data-driven insights

**Combined Sorting**:
```
ORDER BY priority ASC, usage_count DESC

Result:
Priority 1, 45 uses  → Shows first
Priority 1, 32 uses  → Shows second
Priority 2, 100 uses → Shows third (lower priority despite higher usage)
Priority 2, 15 uses  → Shows fourth
```

---

## Validation Rules

### Rule 1: Unique Combination

**Definition**: Each Type-Part combination can only appear once in inventory list.

**Validation**:
```
When admin adds inventory item:
If combination (Type + Part) already exists:
    Error: "This combination already exists in inventory"
    Action: Block save, suggest editing existing item
```

---

### Rule 2: Valid Association

**Definition**: Type and Part must have valid association (from Part-Type Associations).

**Validation**:
```
When admin adds inventory item:
If Part is NOT associated with Type:
    Warning: "This part is not associated with this type.
              Users may see unexpected behavior. Continue?"
    Action: Allow save with confirmation (edge case support)
```

**Rationale**: Admin might pre-configure inventory before associations are finalized.

---

## Performance Optimization

### Caching Strategy

```
On app startup:
→ Load inventory list into memory cache
→ Index by type_id and part_id for fast lookup

On inventory change (admin adds/removes/reorders):
→ Invalidate cache
→ Reload from database

On workflow usage:
→ Read from cache (fast)
→ Async increment usage count (no blocking)
```

**Cache Size**: Small (<50 items typically), minimal memory impact.

---

## Database Schema

```sql
CREATE TABLE inventory_quick_list (
    inventory_id INT PRIMARY KEY AUTO_INCREMENT,
    type_id INT NOT NULL,
    part_id INT NOT NULL,
    priority INT DEFAULT 999,
    usage_count INT DEFAULT 0,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    created_by INT,
    FOREIGN KEY (type_id) REFERENCES dunnage_types(type_id),
    FOREIGN KEY (part_id) REFERENCES parts(part_id),
    UNIQUE KEY unique_type_part (type_id, part_id)
);

-- Increment usage count
UPDATE inventory_quick_list 
SET usage_count = usage_count + 1 
WHERE inventory_id = ?;
```

---

## UI/UX Considerations

### Empty State

**When no inventory items configured**:
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

**Admin sees**:
```
┌─────────────────────────────────────────────────────┐
│ Inventory List Management                           │
│ ───────────────────────────────────────────────────│
│                                                      │
│ No inventory items configured.                      │
│                                                      │
│ Add frequently used type-part combinations to      │
│ speed up data entry for your users.                │
│                                                      │
│ [➕ Add First Item]                                 │
└─────────────────────────────────────────────────────┘
```

---

### Mobile/Accessibility

**Touch-Friendly**:
- Large tap targets (minimum 44x44px)
- Clear visual hierarchy
- Icon + text labels

**Keyboard Navigation**:
- Up/Down arrows to navigate list
- Enter to select
- Esc to close dialog

**Screen Reader**:
- Proper ARIA labels
- Announced usage counts
- Clear selection feedback

---

## Related Documentation

- [Inventory Management Settings](../../Module_Settings.Dunnage/01-Settings-Categories/inventory-list-management.md)
- [Admin Mode](../02-Workflow-Modes/004-admin-mode-specification.md)
- [Guided Mode](../02-Workflow-Modes/001-guided-mode-specification.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

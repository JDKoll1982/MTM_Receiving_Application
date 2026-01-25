# Dunnage Type Management

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Admin Mode](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md), [Type Configuration Business Rule](../../Module_Dunnage/01-Business-Rules/dunnage-type-configuration.md)

---

## Purpose

Dunnage Type Management provides administrators with UI and workflows to create, configure, and manage dunnage types. Each type defines a category of dunnage container with its own icon, specifications, and part associations.

---

## Access

**Location**: Admin Mode → Type Management  
**Permission**: Administrator only  
**URL**: `/admin/dunnage/types` (if web-based future)

---

## Type List View

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Dunnage Type Management                        [Back to Dashboard]  [✕] Exit│
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ [➕ Add Type]  [🔍 Search: _______________]  [Filter: All ▼]  [⬆⬇ Reorder] │
│                                                                              │
│ CONFIGURED TYPES (8 types, 7 active)                                        │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ┌───────────────────────────────────────────────────────────────────────┐ │
│ │⬆⬇│Icon│ Type Name         │ Spec Fields │ Active │ Parts │ Actions   ││ │
│ ├──┼────┼───────────────────┼─────────────┼────────┼───────┼───────────┤│ │
│ │⬍⬍│🪵  │ Wood Pallet 48x40 │     4       │   ✓    │  12   │Edit|Specs ││ │
│ │⬍⬍│📦  │ Cardboard Box     │     3       │   ✓    │   8   │Edit|Specs ││ │
│ │⬍⬍│🏗️  │ Metal Rack        │     5       │   ✓    │   6   │Edit|Specs ││ │
│ │⬍⬍│📋  │ Plastic Tote      │     2       │   ✓    │   4   │Edit|Specs ││ │
│ │⬍⬍│🗃️  │ Plastic Crate     │     3       │        │   5   │Edit|Specs ││ │
│ │⬍⬍│🪜  │ Wooden Skid       │     4       │   ✓    │   3   │Edit|Specs ││ │
│ │⬍⬍│🏗️  │ Metal Cage        │     3       │   ✓    │   2   │Edit|Specs ││ │
│ │⬍⬍│📦  │ Custom Container  │     1       │        │   1   │Edit|Specs ││ │
│ └───────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ ⬆⬇ Drag rows to reorder display order (affects workflow type selection)    │
│                                                                              │
│ [Export Types]  [Import Types (Future)]     [Close]             [Apply]     │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Grid Features

### Columns

**Icon** (Read-Only):
- Visual identifier for type
- Emoji or image icon
- Displayed in workflows

**Type Name** (Read-Only in grid):
- Full descriptive name
- Unique identifier
- Click to edit

**Spec Fields** (Read-Only):
- Count of specification fields configured
- Click number to manage specs
- Link to Spec Field Management

**Active** (Toggle):
- Checkbox or toggle switch
- ✓ = Active (visible in workflows)
- Empty = Inactive (hidden but preserved)
- Toggle in-place without opening dialog

**Parts** (Read-Only):
- Count of parts associated with this type
- Click number to view associations
- Link to Part Association view

**Actions**:
- **Edit**: Opens type editor dialog
- **Specs**: Opens specification field management
- **Deactivate/Activate**: Toggles active status
- **Delete**: Soft delete with confirmation (admin only)

---

### Drag-and-Drop Reordering

**Behavior**:
```
User drags row:
→ Visual indicator shows drop position
→ On drop:
  → Recalculate display_order for all types
  → Save to database
  → Update UI immediately
```

**Display Order Logic**:
```
Types sorted by display_order ASC

Display Order values:
10, 20, 30, 40, ... (increments of 10)

When user drags Type at position 30 to position 15:
→ Renumber: 10, 15, 20, 30, 40, ...

When user drags to end:
→ Renumber: 10, 20, 30, ..., 80
```

**Workflow Impact**:
```
Display order determines:
1. Order in Guided Mode type selection buttons
2. Order in Manual Entry Mode type dropdown
3. Order in Admin Mode type list
```

---

### Search and Filter

**Search Box**:
```
Search by type name (case-insensitive, partial match)

Example:
User types: "pallet"
Results: "Wood Pallet 48x40", "Plastic Pallet 48x48"
```

**Filter Dropdown**:
```
Options:
- All (default)
- Active only
- Inactive only
- With spec fields
- Without spec fields
- With part associations
- Without part associations
```

---

## Add/Edit Type Dialog

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Edit Dunnage Type: Wood Pallet 48x40                            [?] Help  [✕]│
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ BASIC INFORMATION                                                           │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Type Name * (appears in workflows)                                          │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ Wood Pallet 48x40                                                     │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Name must be unique. Use descriptive names (e.g., include size).         │
│                                                                              │
│ Icon * (visual identifier)                                                  │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 🪵  Wood/Lumber                                                    ▼ │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ Icon Preview: 🪵  (This icon will appear in workflow selections)           │
│                                                                              │
│ Available Icons:                                                            │
│ 🪵 Wood  📦 Box  🏗️ Metal  📋 Tote  🗃️ Crate  🪜 Skid  🏗️ Cage  📦 Custom │
│                                                                              │
│ Description (Optional - internal notes)                                     │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ Standard 48" x 40" wooden pallet used for shipping and storage of    │  │
│ │ tube assemblies and frame components. Capacity: 2500 lbs.            │  │
│ │                                                                        │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Description not shown to users, for admin reference only.                │
│                                                                              │
│ STATUS & ORDERING                                                           │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Active (visible in workflows)                                            │
│ ℹ Uncheck to hide from users without deleting historical data.             │
│                                                                              │
│ Display Order (lower numbers appear first)                                  │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 10                                                               ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Current order: 1=Wood Pallet(10), 2=Cardboard(20), 3=Metal Rack(30)      │
│                                                                              │
│ QUICK ACTIONS                                                               │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ After saving:                                                               │
│ ☐ Manage specification fields for this type                                │
│ ☐ Associate parts with this type                                           │
│                                                                              │
│         [Cancel]  [Save]  [Save & Manage Specs]  [Save & Add Parts]         │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Field Definitions

### Type Name (Required)

**Validation**:
```
Rules:
- Required (cannot be empty)
- Unique (case-insensitive)
- Max length: 100 characters
- Allowed characters: Letters, numbers, spaces, hyphens, parentheses

Valid Examples:
✅ "Wood Pallet 48x40"
✅ "Cardboard Box - Large"
✅ "Metal Rack (Heavy Duty)"

Invalid Examples:
❌ "" (empty)
❌ "Wood Pallet 48x40" (duplicate of existing)
❌ "Type@123!" (invalid characters)
```

**Error Messages**:
```
Empty: "Type name is required."
Duplicate: "A type with this name already exists. Please use a unique name."
Invalid Characters: "Type name can only contain letters, numbers, spaces, hyphens, and parentheses."
Too Long: "Type name cannot exceed 100 characters."
```

---

### Icon (Required)

**Selection**:
```
Dropdown with icon preview:
┌────────────────────────────┐
│ 🪵  Wood/Lumber          ▼│
├────────────────────────────┤
│ 🪵  Wood/Lumber            │
│ 📦  Cardboard/Box          │
│ 🏗️  Metal/Steel            │
│ 📋  Tote/Bin               │
│ 🗃️  Crate/Container        │
│ 🪜  Skid/Platform          │
│ 🏗️  Cage/Enclosure         │
│ 📦  Custom/Other           │
└────────────────────────────┘
```

**Icon Library**:
```csharp
public static class DunnageIcons
{
    public static readonly Dictionary<string, string> AvailableIcons = new()
    {
        { "Wood", "🪵" },
        { "Box", "📦" },
        { "Metal", "🏗️" },
        { "Tote", "📋" },
        { "Crate", "🗃️" },
        { "Skid", "🪜" },
        { "Cage", "🏗️" },
        { "Custom", "📦" }
    };
}
```

**Future Enhancement**: Allow custom icon upload (image file).

---

### Description (Optional)

**Purpose**: Internal notes for administrators.

**Validation**:
```
Rules:
- Optional (can be empty)
- Max length: 500 characters
- Multiline text allowed
- Not displayed to end users
```

**Use Cases**:
- Capacity specifications
- Physical dimensions
- Usage guidelines
- Supplier information
- Internal part numbers

---

### Active Status (Boolean)

**Default**: Checked (true)

**Behavior**:
```
When Active (☑):
→ Type visible in all workflows
→ Users can select this type
→ Appears in type dropdowns and buttons

When Inactive (☐):
→ Type hidden from workflows
→ Users cannot select this type
→ Historical data preserved (not deleted)
→ Can be reactivated at any time
```

**Use Cases**:
- Temporarily disable rarely used types
- Phase out deprecated dunnage types
- Hide types during configuration updates
- Seasonal type management

---

### Display Order (Number)

**Default**: Auto-assigned (max + 10)

**Validation**:
```
Rules:
- Integer only (0-999)
- Can have gaps (recommended: increments of 10)
- Duplicates allowed (sorted alphabetically within same order)
```

**Behavior**:
```
Display Order affects:
1. Guided Mode type button order (left to right, top to bottom)
2. Manual Entry Mode dropdown order (top to bottom)
3. Admin Mode type list order (top to bottom)

Sorting:
→ ORDER BY display_order ASC, type_name ASC
```

---

## Save Actions

### Save Button

**Behavior**:
```
Validation:
→ Check required fields (Type Name, Icon)
→ Check uniqueness (Type Name)
→ Check format (Display Order 0-999)

If validation passes:
→ Save to database
→ Invalidate cache
→ Show success message
→ Close dialog
→ Refresh type list

If validation fails:
→ Highlight error fields in red
→ Show error messages below fields
→ Keep dialog open
→ Focus first error field
```

---

### Save & Manage Specs Button

**Behavior**:
```
1. Execute standard Save validation
2. If successful:
   → Save type to database
   → Close type editor dialog
   → Open Spec Field Management dialog for this type
3. If failed:
   → Show validation errors
   → Keep type editor dialog open
```

**Use Case**: Streamlined workflow for creating new type with specs in one flow.

---

### Save & Add Parts Button

**Behavior**:
```
1. Execute standard Save validation
2. If successful:
   → Save type to database
   → Close type editor dialog
   → Open Part Association dialog for this type
3. If failed:
   → Show validation errors
   → Keep type editor dialog open
```

**Use Case**: Streamlined workflow for creating new type and immediately associating parts.

---

## Quick Actions After Save

### Checkbox: Manage specification fields

**Behavior**:
```
When checkbox checked:
→ After successful save, open Spec Field Management automatically
```

### Checkbox: Associate parts

**Behavior**:
```
When checkbox checked:
→ After successful save, open Part Association Management automatically
```

**Note**: These checkboxes are alternative to "Save &..." buttons for user preference.

---

## Delete Type

### Soft Delete

**Access**: Right-click menu → Delete (or dedicated button)

**Confirmation Dialog**:
```
┌─────────────────────────────────────────────────────┐
│ Confirm Delete Type                                 │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Delete type "Wood Pallet 48x40"?                    │
│                                                      │
│ ⚠ WARNING:                                          │
│ • This type has 4 specification fields              │
│ • This type is associated with 12 parts             │
│ • 125 historical loads use this type                │
│                                                      │
│ The type will be deactivated and hidden from users. │
│ Historical data will be preserved.                  │
│                                                      │
│ This action can be undone by reactivating the type. │
│                                                      │
│                      [Cancel]  [Delete]             │
└─────────────────────────────────────────────────────┘
```

**Behavior**:
```
On Delete confirmation:
→ SET active = false
→ SET deleted_date = NOW()
→ SET deleted_by = current_user_id
→ Preserve all spec fields
→ Preserve all part associations
→ Preserve all historical load data
→ Hide from type list (unless "Show Deleted" filter enabled)
→ Log audit entry
```

**Undo Delete**:
```
Admin can reactivate by:
1. Filter: Show Deleted types
2. Select deleted type
3. Edit → Check "Active" checkbox
4. Save
```

---

## Specification Field Management

**Access**: Type list → Actions → "Specs" button

**See**: [Specification Field Configuration](./specification-field-configuration.md) for complete specification.

**Summary**:
- Manage custom data fields for selected type
- Add/Edit/Delete/Reorder spec fields
- Configure field types, validation, defaults
- Manage dropdown options

---

## Part Association Management

**Access**: Type list → Right-click → "Manage Part Associations"

**UI Layout**:
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Part Associations: Wood Pallet 48x40                                        │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Parts that can use this dunnage type:                                       │
│                                                                              │
│ ASSOCIATED PARTS (12 parts)                                                 │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ┌───────────────────────────────────────────────────────────────────────┐ │
│ │ Part Number  │ Description              │ Usage │ Actions            ││ │
│ ├──────────────┼──────────────────────────┼───────┼────────────────────┤│ │
│ │ TUBE-A123    │ Tube Assembly A123       │  45   │ [Remove Assoc]     ││ │
│ │ TUBE-B456    │ Tube Assembly B456       │  32   │ [Remove Assoc]     ││ │
│ │ FRAME-C789   │ Frame Assembly C789      │  28   │ [Remove Assoc]     ││ │
│ │ ...                                                                    ││ │
│ └───────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ [➕ Add Part Association]                                                   │
│                                                                              │
│                                                              [Close]          │
└─────────────────────────────────────────────────────────────────────────────┘
```

**See**: [Part Management](./part-management.md) for complete specification.

---

## Export Types

### Export Format

**File Type**: JSON

**Filename**: `dunnage_types_export_{timestamp}.json`

**Export Structure**:
```json
{
  "export_version": "1.0",
  "export_date": "2026-01-25T10:30:00Z",
  "exported_by": "admin@company.com",
  "type_count": 8,
  "types": [
    {
      "type_name": "Wood Pallet 48x40",
      "icon": "🪵",
      "description": "Standard 48x40 wooden pallet...",
      "active": true,
      "display_order": 10,
      "spec_fields": [
        {
          "field_name": "Condition",
          "field_type": "Dropdown",
          "required": true,
          "default_value": "Good",
          "dropdown_options": ["Excellent", "Good", "Fair", "Poor"],
          "display_order": 0
        }
      ]
    }
  ]
}
```

**Export Options Dialog**:
```
┌─────────────────────────────────────────────────────┐
│ Export Dunnage Types                                │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Export scope:                                       │
│ ☑ Active types only                                │
│ ☐ Include inactive types                           │
│ ☑ Include specification fields                     │
│ ☐ Include part associations                        │
│                                                      │
│ Export destination:                                 │
│ ● Save to file                                      │
│ ○ Copy to clipboard                                 │
│                                                      │
│                      [Cancel]  [Export]             │
└─────────────────────────────────────────────────────┘
```

---

## Import Types (Future Feature)

**Purpose**: Import types from JSON export file (future enhancement).

**See**: [Settings Architecture - Import/Export](./settings-architecture.md#configuration-importexport-future)

---

## Related Documentation

- [Admin Mode Specification](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md)
- [Type Configuration Business Rule](../../Module_Dunnage/01-Business-Rules/dunnage-type-configuration.md)
- [Specification Field Configuration](./specification-field-configuration.md)
- [Part Management](./part-management.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

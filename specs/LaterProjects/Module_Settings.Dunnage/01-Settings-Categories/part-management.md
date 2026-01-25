# Part Management

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Admin Mode](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md), [Part-Type Associations](../../Module_Dunnage/01-Business-Rules/part-type-associations.md)

---

## Purpose

Part Management provides administrators with UI and workflows to create, configure, and manage parts that can be received using dunnage containers. Each part can be associated with one or more dunnage types.

---

## Access

**Location**: Admin Mode → Part Management  
**Permission**: Administrator only  
**URL**: `/admin/dunnage/parts` (if web-based future)

---

## Part List View

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Part Management                                [Back to Dashboard]  [✕] Exit│
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ [➕ Add Part]  [🔍 Search: _______________]  [Filter: All Parts ▼]          │
│                                                                              │
│ CONFIGURED PARTS (45 parts)                                                 │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ┌───────────────────────────────────────────────────────────────────────┐ │
│ │ Part Number  │ Description              │ Assoc Types │ Active │Actions││ │
│ ├──────────────┼──────────────────────────┼─────────────┼────────┼───────┤│ │
│ │ TUBE-A123    │ Tube Assembly A123       │ 3 types     │   ✓    │Edit|A ││ │
│ │ TUBE-B456    │ Tube Assembly B456       │ 3 types     │   ✓    │Edit|A ││ │
│ │ FRAME-C789   │ Frame Assembly C789      │ 2 types     │   ✓    │Edit|A ││ │
│ │ FRAME-D012   │ Frame Assembly D012      │ 2 types     │   ✓    │Edit|A ││ │
│ │ BRACKET-E345 │ Bracket Assembly E345    │ 1 type      │   ✓    │Edit|A ││ │
│ │ PANEL-F678   │ Panel Assembly F678      │ 4 types     │   ✓    │Edit|A ││ │
│ │ HOUSING-G901 │ Housing Assembly G901    │ 2 types     │        │Edit|A ││ │
│ │ ...                                                                    ││ │
│ └───────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ [◀ Previous]  Page 1 of 5  [Next ▶]          Showing 25 of 45 parts        │
│                                                                              │
│ [Export Parts]  [Import Parts (Future)]                    [Close]          │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Grid Features

### Columns

**Part Number** (Primary Key):
- Unique identifier
- Read-only in grid, edit in dialog
- Alphanumeric (e.g., TUBE-A123)
- Click to edit

**Description** (Text):
- Full part description
- Read-only in grid, edit in dialog
- Max 200 characters
- Displayed in workflows for user clarity

**Assoc Types** (Count):
- Number of dunnage types associated with this part
- Click to manage associations
- Shows "0 types" with warning icon if no associations

**Active** (Boolean):
- Toggle checkbox
- ✓ = Active (available in workflows)
- Empty = Inactive (hidden but preserved)

**Actions**:
- **Edit**: Opens part editor dialog
- **A** (Associations): Opens type association management

---

### Search and Filter

**Search Box**:
```
Search by:
- Part Number (exact or partial)
- Description (partial match, case-insensitive)

Example:
User types: "tube"
Results: 
  TUBE-A123 - Tube Assembly A123
  TUBE-B456 - Tube Assembly B456
  CUSTOM-TUBE - Custom Tube Component
```

**Filter Dropdown**:
```
Options:
- All Parts (default)
- Active only
- Inactive only
- With associations (has at least 1 type)
- Without associations (0 types, warning state)
- Recently used (based on receiving transactions)
```

---

### Pagination

**Settings**:
- 25 parts per page (configurable)
- Previous/Next navigation
- Page number display
- Jump to page (optional)

**Performance**:
- Query only current page rows
- Total count cached for 5 minutes
- Auto-refresh on data changes

---

## Add/Edit Part Dialog

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Edit Part: TUBE-A123                                        [?] Help  [✕]   │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ PART INFORMATION                                                            │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Part Number * (unique identifier)                                           │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ TUBE-A123                                                             │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Part number must be unique. Use your company's part numbering system.    │
│                                                                              │
│ Description * (displayed to users)                                          │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ Tube Assembly A123 - Main structural tube component for chassis      │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Description helps users select the correct part in workflows.            │
│                                                                              │
│ CONFIGURATION                                                               │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Active (available for dunnage receiving)                                 │
│ ℹ Uncheck to temporarily hide from users without deleting data.            │
│                                                                              │
│ TYPE ASSOCIATIONS (3 types)                                                 │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ This part can be received using:                                           │
│ • Wood Pallet 48x40                                                         │
│ • Cardboard Box - Large                                                     │
│ • Metal Rack - Standard                                                     │
│                                                                              │
│ [Manage Associations]                                                       │
│                                                                              │
│ QUICK ACTIONS                                                               │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ After saving:                                                               │
│ ☐ Manage type associations for this part                                   │
│                                                                              │
│         [Cancel]  [Save]  [Save & Manage Associations]                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Field Definitions

### Part Number (Required)

**Validation**:
```
Rules:
- Required (cannot be empty)
- Unique (case-sensitive)
- Max length: 50 characters
- Allowed characters: Letters, numbers, hyphens, underscores
- No leading/trailing spaces

Valid Examples:
✅ "TUBE-A123"
✅ "FRAME_B456"
✅ "BRACKET-E345"
✅ "12345-PANEL"

Invalid Examples:
❌ "" (empty)
❌ "TUBE-A123" (duplicate)
❌ "PART #123" (invalid character: #)
❌ " TUBE-A123 " (leading/trailing spaces)
```

**Error Messages**:
```
Empty: "Part number is required."
Duplicate: "Part number 'TUBE-A123' already exists. Please use a unique part number."
Invalid Characters: "Part number can only contain letters, numbers, hyphens, and underscores."
Too Long: "Part number cannot exceed 50 characters."
```

---

### Description (Required)

**Validation**:
```
Rules:
- Required (cannot be empty)
- Max length: 200 characters
- Free-form text (allows all characters)
- Displayed in workflows for part selection

Recommended Format:
"[Part Type] [Part Number] - [Brief Description]"

Examples:
✅ "Tube Assembly A123 - Main structural tube"
✅ "Frame Assembly B456 - Chassis frame component"
✅ "Bracket E345 - Mounting bracket for panel assembly"
```

**Error Messages**:
```
Empty: "Description is required."
Too Long: "Description cannot exceed 200 characters. Current: 215 characters."
```

---

### Active Status (Boolean)

**Default**: Checked (true)

**Behavior**:
```
When Active (☑):
→ Part visible in all workflows
→ Users can select this part
→ Appears in part dropdowns

When Inactive (☐):
→ Part hidden from workflows
→ Users cannot select this part
→ Historical data preserved
→ Can be reactivated at any time
```

**Use Cases**:
- Obsolete parts (replaced by new part numbers)
- Seasonal parts (not currently in production)
- Parts under review (quality issues)
- Discontinued parts (but historical data needed)

---

## Type Association Management

### Access

**Methods**:
1. Part list → Actions → "A" button
2. Part editor → "Manage Associations" button
3. Part editor → Save with checkbox checked

---

### Association Dialog UI

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Manage Type Associations: TUBE-A123 - Tube Assembly A123                   │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Select which dunnage types can be used for this part:                       │
│                                                                              │
│ ┌───────────────────────────────────────────────────────────────────────┐ │
│ │ ☑ 🪵  Wood Pallet 48x40                                               ││ │
│ │ ☑ 📦  Cardboard Box - Large                                           ││ │
│ │ ☑ 🏗️  Metal Rack - Standard                                           ││ │
│ │ ☐ 📋  Plastic Tote                                                    ││ │
│ │ ☐ 🗃️  Plastic Crate                                                   ││ │
│ │ ☐ 🪜  Wooden Skid                                                     ││ │
│ │ ☐ 🏗️  Metal Cage                                                      ││ │
│ │ ☐ 📦  Custom Container                                                ││ │
│ └───────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│ ℹ Currently associated with 3 types. Users will see these options when     │
│   receiving this part.                                                      │
│                                                                              │
│ ⚠ If no types are selected, users will not be able to receive this part!   │
│                                                                              │
│ [Select All]  [Clear All]                           [Cancel]  [Save]        │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### Association Behavior

**Checkbox States**:
```
☑ Checked: Part can use this dunnage type
☐ Unchecked: Part cannot use this dunnage type
```

**Save Logic**:
```
On Save:
1. Delete all existing associations for this part
2. Insert new associations for checked types
3. Invalidate cache
4. Log audit entry
5. Refresh part list (update "Assoc Types" column)
```

**Validation**:
```
If no types selected:
    Warning: "No dunnage types selected. Users will not be able to receive this part. Continue?"
    Severity: Warning (allows save with confirmation)
```

**Workflow Impact**:
```
Guided Mode Step 1:
User selects Part: TUBE-A123
→ System loads associations: [Wood Pallet, Cardboard Box, Metal Rack]
→ Displays type selection filtered to these 3 types
→ User cannot select Plastic Tote (not associated)
```

---

### Quick Actions

**Select All Button**:
```
Action: Check all type checkboxes
Use Case: Part can use any dunnage type
```

**Clear All Button**:
```
Action: Uncheck all type checkboxes
Use Case: Start fresh or remove all associations
```

---

## Bulk Operations

### Select Multiple Parts

**Access**: Checkbox column (future enhancement)

**Operations**:
- Bulk activate/deactivate
- Bulk export
- Bulk delete (soft delete)

**UI**:
```
[Select All] [Clear Selection]  [With Selected ▼]

Dropdown options:
- Activate selected parts
- Deactivate selected parts
- Export selected parts
- Delete selected parts
```

---

## Delete Part

### Soft Delete

**Access**: Right-click menu → Delete

**Confirmation Dialog**:
```
┌─────────────────────────────────────────────────────┐
│ Confirm Delete Part                                 │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Delete part "TUBE-A123 - Tube Assembly A123"?      │
│                                                      │
│ ⚠ WARNING:                                          │
│ • This part is associated with 3 dunnage types      │
│ • 125 historical receiving transactions use part    │
│                                                      │
│ The part will be deactivated and hidden from users. │
│ Historical data will be preserved.                  │
│                                                      │
│ This action can be undone by reactivating the part. │
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
→ Preserve all type associations
→ Preserve all historical transaction data
→ Hide from part list (unless "Show Deleted" filter enabled)
→ Log audit entry
```

---

## Export Parts

### Export Format

**File Type**: JSON or CSV

**Filename**: `dunnage_parts_export_{timestamp}.json`

**JSON Export Structure**:
```json
{
  "export_version": "1.0",
  "export_date": "2026-01-25T10:30:00Z",
  "exported_by": "admin@company.com",
  "part_count": 45,
  "parts": [
    {
      "part_number": "TUBE-A123",
      "description": "Tube Assembly A123 - Main structural tube",
      "active": true,
      "associated_types": [
        "Wood Pallet 48x40",
        "Cardboard Box - Large",
        "Metal Rack - Standard"
      ]
    }
  ]
}
```

**CSV Export Structure**:
```csv
PartNumber,Description,Active,AssociatedTypes
TUBE-A123,"Tube Assembly A123 - Main structural tube",true,"Wood Pallet 48x40|Cardboard Box|Metal Rack"
TUBE-B456,"Tube Assembly B456",true,"Wood Pallet 48x40|Cardboard Box|Metal Rack"
```

---

### Export Options Dialog

```
┌─────────────────────────────────────────────────────┐
│ Export Parts                                        │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Export scope:                                       │
│ ● All parts (45 parts)                             │
│ ○ Active parts only (42 parts)                     │
│ ○ Selected parts only (3 selected)                 │
│                                                      │
│ Export format:                                      │
│ ● JSON (with associations)                         │
│ ○ CSV (simple)                                      │
│                                                      │
│ Include:                                            │
│ ☑ Type associations                                │
│ ☐ Usage statistics                                 │
│                                                      │
│ Export destination:                                 │
│ ● Save to file                                      │
│ ○ Copy to clipboard                                 │
│                                                      │
│                      [Cancel]  [Export]             │
└─────────────────────────────────────────────────────┘
```

---

## Integration Points

### Part Source

**Current Implementation**: Manual admin entry

**Future Enhancements**:
1. **ERP Integration**: Sync parts from Infor Visual or other ERP
2. **Import from CSV**: Bulk part upload
3. **API Integration**: REST API for external systems

**ERP Sync Considerations**:
```
If ERP integration enabled:
→ Part Number = ERP Part Number (read-only)
→ Description = ERP Part Description (auto-updated)
→ Active = ERP Active Status (auto-synced)
→ Type associations = Manually managed in dunnage module

Sync frequency: Nightly batch or on-demand
Conflict resolution: ERP master, dunnage associations preserved
```

---

### Workflow Usage

**Where Parts Appear**:

**Guided Mode - Step 2**:
```
Part Selection dropdown:
→ Shows all active parts
→ Filtered by selected type associations
→ Sorted alphabetically by Part Number
```

**Manual Entry Mode**:
```
Part column dropdown (per row):
→ Shows all active parts
→ Filtered by type selected in same row
→ Auto-complete enabled
```

**Edit Mode**:
```
Search filter:
→ Filter transactions by part number
→ Auto-complete from all parts (active + inactive)
```

---

## Performance Optimization

### Caching Strategy

```
Cache Duration:
- Part list (all parts): 5 minutes
- Part associations: 10 minutes
- Active parts only: 15 minutes (more stable)

Cache Invalidation:
- On part CRUD operations
- On association changes
- Manual cache clear (admin tool)
```

---

### Database Indexes

```sql
-- Primary key (unique part number)
CREATE UNIQUE INDEX idx_parts_part_number ON parts(part_number);

-- Active flag filtering
CREATE INDEX idx_parts_active ON parts(active);

-- Description search
CREATE INDEX idx_parts_description ON parts(description);

-- Association lookup
CREATE INDEX idx_part_type_assoc_part ON part_type_associations(part_id);
CREATE INDEX idx_part_type_assoc_type ON part_type_associations(type_id);
```

---

## Error Handling

### Validation Errors

**Duplicate Part Number**:
```
❌ Part number 'TUBE-A123' already exists.

Please use a unique part number or edit the existing part.
```

**No Type Associations**:
```
⚠ This part has no type associations.

Users will not be able to receive this part in workflows.
Would you like to configure associations now? [Yes] [No]
```

**Invalid Part Number Format**:
```
❌ Part number contains invalid characters.

Allowed: Letters, numbers, hyphens, underscores
Example: TUBE-A123, FRAME_B456
```

---

### Save Errors

**Database Error**:
```
❌ Failed to save part: Database error

Details: [Error message]

[Retry] [Cancel]
```

**Concurrent Modification**:
```
⚠ This part was modified by another user while you were editing.

Your changes: [Show details]
Their changes: [Show details]

[Reload] [Overwrite] [Cancel]
```

---

## Related Documentation

- [Admin Mode Specification](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md)
- [Part-Type Associations Business Rule](../../Module_Dunnage/01-Business-Rules/part-type-associations.md)
- [Dunnage Type Management](./dunnage-type-management.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

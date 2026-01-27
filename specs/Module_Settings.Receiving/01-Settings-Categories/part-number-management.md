# Part Number Management

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Purpose and Overview](../00-Core/purpose-and-overview.md), [Part Number Dynamics](../../Module_Receiving/01-Business-Rules/part-number-dynamics.md)

---

## Overview

Part Number Management provides part-specific configuration that affects how individual parts behave during receiving operations. Administrators can override automatic assignments, set custom defaults, and flag special requirements per part number.

---

## Configurable Settings Per Part

### 1. Part Type Assignment

**Purpose**: Override automatic part type detection based on part number prefix

**Data Source**: Part types are stored in a database table (`part_types` master table) and can be managed through admin settings.

**Configuration Options**:
- Dynamically loaded from `part_types` database table
- Common examples: Coils, Flat Stock, Tubing, Barstock, Nuts, Bolts, Washers, Bushings, Screws, Misc Hardware
- Additional types can be added via admin interface (future enhancement)

**Default Behavior**: Automatic assignment based on prefix rules (MMC → Coils, MMF → Flat Stock, etc.)

**Override Behavior**: Settings value takes absolute precedence over automatic detection

**UI Component**: Dropdown (populated from database table)

**Database Schema**:
```sql
CREATE TABLE part_types (
    type_id INT PRIMARY KEY IDENTITY(1,1),
    type_name NVARCHAR(50) NOT NULL UNIQUE,
    active BIT DEFAULT 1,
    display_order INT,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Part-specific type overrides stored in part_settings table
CREATE TABLE part_settings (
    part_id NVARCHAR(50) PRIMARY KEY,
    part_type_id INT FOREIGN KEY REFERENCES part_types(type_id),
    default_location NVARCHAR(20),
    quality_hold_required BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);
```

**Example**:
```
Part Number: MMC0005000
Auto-Detected Type: Coils (from MMC prefix)
Settings Override: Tubing (selected from database dropdown)
Result When Receiving: Part Type = Tubing
```

**Reactive UI Behavior**:
```
On settings page load:
1. Query part_types table for active types
2. Populate dropdown with type_name values
3. Order by display_order, then alphabetically

On filter/search:
1. Query part_types table based on search term
2. Update dropdown options reactively
3. Show only active types (WHERE active = 1)

On admin changes:
- If admin adds new part type → Appears in dropdown immediately
- If admin deactivates type → Removed from dropdown (existing assignments preserved)
```

---

### 2. Default Receiving Location

**Purpose**: Set custom default location that overrides ERP default

**Configuration Options**:
- Any valid location string (validated against Infor Visual locations if possible)
- Example locations: V-C0-01, V-C0-02, RECV, QC-HOLD

**Default Behavior**: Pull from Infor Visual default location, fallback to "RECV" if not found

**Override Behavior**: Settings value overrides ERP default

**Validation**:
- Warn if location doesn't exist in ERP
- Allow save despite warning (location may be valid but not yet in ERP)
- Real-time validation against known locations (if performance allows)

**UI Component**: Text input with auto-complete/validation

**Example**:
```
Part Number: MMC0001000
ERP Default: V-C0-01
Settings Override: V-C0-02
Result When Receiving: Location = V-C0-02 (Settings win)
```

---

### 3. Quality Hold Flag

**Purpose**: Mark parts requiring Quality Control inspection before acceptance

**Configuration Options**:
- ☐ Quality Hold Not Required (default)
- ☑ Quality Hold Required

**Default Behavior**: No Quality Hold unless flagged

**Effect When Enabled**:
- Quality Hold dialog appears when part is entered in Wizard/Manual Mode
- User must acknowledge procedures before continuing
- Audit trail records Quality Hold acknowledgment

**UI Component**: Checkbox

**Example**:
```
Part Number: MMC0003000
Quality Hold: ☑ Required
Result When Receiving:
  → Quality Hold dialog displays
  → User must acknowledge before proceeding
  → Acknowledgment logged in audit trail
```

---

### 4. Package Type Preference

**Purpose**: Set default package type for this part

**Configuration Options**:
- Skid
- Pallet
- Box
- Bundle
- Coil
- Crate
- Drum
- Other

**Default Behavior**: Auto-select based on Part Type (e.g., Coils → Skid, Flat Stock → Pallet)

**Override Behavior**: Settings value pre-selected when part is entered

**UI Component**: Dropdown selection

**Example**:
```
Part Number: MMC0001000
Part Type: Coils
Auto Package Type: Skid
Settings Override: Bundle
Result When Receiving: Package Type pre-populated as Bundle
```

---

## User Interface Design

### Main Settings View

**Layout**: Master-detail pattern

**Master Panel (Left):**
```
┌─────────────────────────────────────┐
│ Part Number Management               │
│ ─────────────────────────────────── │
│                                      │
│ 🔍 Search: [_____________] [Search] │
│                                      │
│ ┌──────────────────────────────────┐│
│ │ Part Number   │ Type    │ QH  │ ││
│ │───────────────│─────────│─────│ ││
│ │ MMC0001000    │ Coils   │  ☐  │ ││
│ │ MMC0002000    │ Coils   │  ☐  │ ││
│ │ MMC0003000    │ Coils   │  ☑  │ ││
│ │ MMC0005000    │ Tubing* │  ☐  │ ││
│ │ MMF0001000    │ Flat    │  ☐  │ ││
│ │ MMCCS00364    │ Coils   │  ☐  │ ││
│ │ ...                              ││
│ └──────────────────────────────────┘│
│                                      │
│ * = Overridden from auto-detected    │
│                                      │
│ [➕ Add New Part]  [📋 Bulk Edit]   │
└─────────────────────────────────────┘
```

**Detail Panel (Right):**
```
┌──────────────────────────────────────────────┐
│ Edit Part Configuration: MMC0001000          │
│ ──────────────────────────────────────────── │
│                                               │
│ Part Number:                                  │
│ ┌──────────────────────┐                     │
│ │ MMC0001000           │ (Read-only)         │
│ └──────────────────────┘                     │
│                                               │
│ Part Type:                                    │
│ ┌──────────────────────────────┐             │
│ │ Coils                     ▼  │             │
│ └──────────────────────────────┘             │
│ ℹ Auto-detected from prefix: Coils          │
│                                               │
│ Default Receiving Location:                   │
│ ┌──────────────────────────────┐             │
│ │ V-C0-01                       │             │
│ └──────────────────────────────┘             │
│ ℹ ERP Default: V-C0-01                       │
│ ⚠ Custom override will take precedence      │
│                                               │
│ Package Type Preference:                      │
│ ┌──────────────────────────────┐             │
│ │ Skid                      ▼  │             │
│ └──────────────────────────────┘             │
│                                               │
│ ☑ Quality Hold Required                      │
│   ┌──────────────────────────────────────┐  │
│   │ ⚠ Requires QC inspection procedures  │  │
│   │   User will be prompted during        │  │
│   │   receiving workflow                  │  │
│   └──────────────────────────────────────┘  │
│                                               │
│ Last Modified: 2026-01-20 14:35              │
│ Modified By: ADMIN (John Smith)              │
│                                               │
│         [Cancel]  [Reset]  [Save Changes]    │
└──────────────────────────────────────────────┘
```

---

## Search and Filter Capabilities

### Basic Search

**Search Fields**:
- Part Number (exact match or starts-with)
- Part Type
- Quality Hold status

**Search Behavior**:
- Real-time filtering as user types
- Case-insensitive
- Partial matching supported for part numbers

**Example Searches**:
```
Search: "MMC0001"
Results: MMC0001000, MMC0001001, MMC0001002, ...

Search: "Coils"
Results: All parts with Part Type = Coils

Search: "QH"
Results: All parts with Quality Hold enabled
```

---

### Advanced Filter

**Filter Options**:
- **Part Type**: Multi-select dropdown (dynamically populated from `part_types` database table)
- **Quality Hold**: All, Required Only, Not Required
- **Has Overrides**: Show only parts with settings overrides
- **Recently Modified**: Show parts modified in last N days

**Reactive Part Type Filter**:
```
Database-Driven Behavior:
1. On filter panel load:
   - Query: SELECT type_name FROM part_types WHERE active = 1 ORDER BY display_order
   - Populate checkboxes dynamically from query results
   
2. When admin adds new part type in admin settings:
   - New type appears in filter immediately (no page reload needed)
   - Reactive update via database change notification or polling
   
3. When admin deactivates part type:
   - Type removed from filter checkboxes
   - Existing part assignments with that type still visible in results
   
4. Filter application:
   - Query: SELECT * FROM part_settings WHERE part_type_id IN (selected_type_ids)
```

**Filter UI**:
```
┌─────────────────────────────────────┐
│ 🔧 Advanced Filters                 │
│ ─────────────────────────────────── │
│                                      │
│ Part Type: (loaded from database)   │
│ ☑ Coils    ☐ Flat Stock  ☐ Tubing  │
│ ☐ Barstock ☐ Nuts        ☐ Bolts   │
│ ☐ Washers  ☐ Bushings    ☐ Screws  │
│ ☐ Misc Hardware                     │
│ [+ types added via admin settings]  │
│                                      │
│ Quality Hold:                        │
│ ○ All  ○ Required Only  ○ Not Req   │
│                                      │
│ Show Only:                           │
│ ☐ Parts with overrides               │
│ ☐ Modified in last [30] days        │
│                                      │
│       [Clear Filters]  [Apply]       │
└─────────────────────────────────────┘
```

---

## Bulk Operations

### Bulk Edit Capability

**Supported Bulk Operations**:
1. Set Part Type for multiple parts
2. Set Default Location for multiple parts
3. Enable/Disable Quality Hold for multiple parts
4. Set Package Type Preference for multiple parts

**Bulk Edit Workflow**:
```
Step 1: Select parts (checkboxes in master list)
Step 2: Click [Bulk Edit] button
Step 3: Choose operation and value
Step 4: Preview changes
Step 5: Confirm and apply
```

**Bulk Edit UI**:
```
┌──────────────────────────────────────────────┐
│ Bulk Edit - 15 parts selected                │
│ ──────────────────────────────────────────── │
│                                               │
│ Operation:                                    │
│ ┌──────────────────────────────────────────┐ │
│ │ Set Part Type                         ▼ │ │
│ └──────────────────────────────────────────┘ │
│                                               │
│ New Value:                                    │
│ ┌──────────────────────────────────────────┐ │
│ │ Tubing                                ▼ │ │
│ └──────────────────────────────────────────┘ │
│                                               │
│ Preview:                                      │
│ ┌──────────────────────────────────────────┐ │
│ │ MMC0001000: Coils → Tubing               │ │
│ │ MMC0002000: Coils → Tubing               │ │
│ │ MMC0005000: Flat Stock → Tubing          │ │
│ │ ... (12 more)                            │ │
│ └──────────────────────────────────────────┘ │
│                                               │
│ ⚠ This will override auto-detected types     │
│                                               │
│           [Cancel]  [Apply to All]            │
└──────────────────────────────────────────────┘
```

---

## Validation Rules

### Part Number Validation

**On Entry**:
- Must follow standard format (MMC*, MMF*, MMCCS*, etc.) OR allow non-standard with warning
- Auto-standardize to uppercase and zero-padded format
- Check if part exists in ERP (warn if not found, but allow)

**On Save**:
- Part number must not be blank
- Part number must be unique (no duplicates in settings)

---

### Location Validation

**On Entry**:
- Validate against known Infor Visual locations (if possible)
- Warn if location not found: "⚠ Location not found in ERP. Save anyway?"
- Allow save despite warning

**On Save**:
- Location cannot be blank (fallback to "RECV" if empty)
- Location format: alphanumeric and hyphens only

---

### Quality Hold Validation

**On Enable**:
- No validation required (simple boolean flag)
- Warn if enabling for part with active receiving sessions?

**On Disable**:
- Warn: "⚠ Removing Quality Hold requirement. Ensure compliance before proceeding."
- Require confirmation

---

## Audit Trail

### Change Tracking

**Tracked Fields**:
- Part Number (new part additions)
- Part Type changes
- Default Location changes
- Quality Hold flag changes
- Package Type Preference changes

**Audit Record Format**:
```
┌──────────────────────────────────────────────┐
│ Change History: MMC0001000                    │
│ ──────────────────────────────────────────── │
│                                               │
│ 2026-01-25 10:30 AM - ADMIN (John Smith)    │
│ Part Type: Coils → Tubing                    │
│                                               │
│ 2026-01-20 14:35 PM - SUPER (Jane Doe)      │
│ Quality Hold: Disabled → Enabled             │
│                                               │
│ 2026-01-15 09:00 AM - ADMIN (John Smith)    │
│ Default Location: V-C0-01 → V-C0-02          │
│                                               │
│ 2026-01-10 08:15 AM - ADMIN (John Smith)    │
│ Part Created                                  │
└──────────────────────────────────────────────┘
```

---

## Database Integration

### Part Types Master Table

**Required Database Table**: `part_types`

**Purpose**: Centralized master list of all available part types used across the application.

**Schema**:
```sql
CREATE TABLE part_types (
    type_id INT PRIMARY KEY IDENTITY(1,1),
    type_name NVARCHAR(50) NOT NULL UNIQUE,
    description NVARCHAR(200),
    active BIT DEFAULT 1,
    display_order INT DEFAULT 999,
    created_at DATETIME2 DEFAULT GETDATE(),
    created_by NVARCHAR(100),
    updated_at DATETIME2 DEFAULT GETDATE(),
    updated_by NVARCHAR(100)
);

-- Seed data
INSERT INTO part_types (type_name, description, display_order) VALUES
('Coils', 'Coiled metal stock', 1),
('Flat Stock', 'Flat metal sheets/plates', 2),
('Tubing', 'Metal tubing/pipes', 3),
('Barstock', 'Bar stock material', 4),
('Nuts', 'Fastener nuts', 5),
('Bolts', 'Fastener bolts', 6),
('Washers', 'Fastener washers', 7),
('Bushings', 'Bushings and bearings', 8),
('Screws', 'Fastener screws', 9),
('Misc Hardware', 'Miscellaneous hardware', 10);
```

**Part Settings Table** (links parts to types):
```sql
CREATE TABLE part_settings (
    part_id NVARCHAR(50) PRIMARY KEY,
    part_type_id INT FOREIGN KEY REFERENCES part_types(type_id),
    default_location NVARCHAR(20),
    quality_hold_required BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    created_by NVARCHAR(100),
    updated_at DATETIME2 DEFAULT GETDATE(),
    updated_by NVARCHAR(100),
    
    INDEX IX_part_settings_type (part_type_id),
    INDEX IX_part_settings_location (default_location),
    INDEX IX_part_settings_qh (quality_hold_required)
);
```

**Admin Management** (future enhancement):
- Part types can be added/edited/deactivated through admin interface
- New types appear in all dropdowns immediately
- Deactivated types hidden from dropdowns but preserve existing assignments
- Display order can be customized per company preferences

**Reactive UI Integration**:
```csharp
// ViewModel loads part types from database
public async Task LoadPartTypesAsync()
{
    var types = await _partTypeRepository.GetActiveTypesAsync();
    PartTypes = new ObservableCollection<PartType>(types.OrderBy(t => t.DisplayOrder));
}

// Database repository
public async Task<List<PartType>> GetActiveTypesAsync()
{
    return await _context.PartTypes
        .Where(pt => pt.Active)
        .OrderBy(pt => pt.DisplayOrder)
        .ThenBy(pt => pt.TypeName)
        .ToListAsync();
}
```

---

## Integration with Receiving Workflow

### Wizard Mode Integration

**On Part Number Entry**:
1. User enters part number (e.g., MMC0001000)
2. System queries Settings for part configuration
3. System applies settings in this order:
   - Part Type: Settings override OR auto-detect from prefix
   - Default Location: Settings override OR ERP default OR "RECV" fallback
   - Package Type: Settings preference OR auto-detect from Part Type
4. If Quality Hold enabled in Settings:
   - Display Quality Hold dialog
   - Require acknowledgment
   - Log acknowledgment in session

**Example Flow**:
```
User enters: MMC0001000
Settings query finds:
  - Part Type: Tubing (override)
  - Default Location: V-C0-02 (override)
  - Quality Hold: Enabled
  - Package Type: Bundle

Result in Wizard:
  → Part Type field shows: Tubing
  → Location field shows: V-C0-02
  → Package Type field shows: Bundle
  → Quality Hold dialog appears
  → User must acknowledge before proceeding
```

---

### Manual Mode Integration

**Grid Auto-Population**:
- When user enters part number in grid row
- System auto-populates Part Type, Location, Package Type from Settings
- User can override any auto-populated value

**Bulk Part Entry**:
- If multiple rows have same part number
- Settings apply to all rows with that part number
- Quality Hold dialog appears once per unique part per session

---

### Edit Mode Integration

**Loading Historical Data**:
- Historical data loaded with original values
- Current Settings displayed as reference
- Validation uses current Settings (warn if mismatch)

**Example**:
```
Historical Transaction (2025-12-01):
  Part: MMC0001000
  Part Type: Coils (original)
  Location: V-C0-01 (original)

Current Settings (2026-01-25):
  Part Type: Tubing (new override)
  Location: V-C0-02 (new override)

Edit Mode Display:
  → Shows original values: Coils, V-C0-01
  → Warning: "⚠ Current settings differ from historical values"
  → User can keep historical or update to current
```

---

## Performance Considerations

### Caching Strategy

**In-Memory Cache**:
- Load all part settings on app startup
- Cache invalidated on settings save
- Refresh cache every N minutes (configurable)

**Query Optimization**:
- Index part_number field for fast lookups
- Use dictionary/hashmap for O(1) lookup performance

---

### Lazy Loading

**For Large Part Lists (1000+ parts)**:
- Load settings on-demand as parts are entered
- Background pre-fetch for common parts
- Virtualized grid in Settings UI

---

## Error Handling

### Configuration Errors

**Invalid Part Number**:
```
Error: "Part number format invalid. Expected format: MMC0001000"
Action: Block save, highlight invalid field
```

**Duplicate Part Number**:
```
Error: "Part MMC0001000 already configured."
Action: Prevent duplicate, suggest editing existing
```

**Network/Database Error**:
```
Error: "Unable to save settings. Database connection failed."
Action: Retry with exponential backoff, preserve user input
```

---

## Related Documentation

- [Default Part Types](../../Module_Receiving/01-Business-Rules/default-part-types.md) - Part type rules
- [Receiving Location Dynamics](../../Module_Receiving/01-Business-Rules/receiving-location-dynamics.md) - Location logic
- [Quality Hold](../../Module_Receiving/01-Business-Rules/quality-hold.md) - Quality Hold procedures
- [Settings Architecture](../00-Core/settings-architecture.md) - Technical architecture

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-25 | Initial specification created |

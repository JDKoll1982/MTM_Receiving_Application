# Specification Field Configuration

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Dynamic Specification Fields](../../Module_Dunnage/01-Business-Rules/dynamic-specification-fields.md), [Type Management](./dunnage-type-management.md)

---

## Overview

Specification Field Configuration allows administrators to define custom data fields for each dunnage type. These dynamic fields control what information users must enter when receiving dunnage, enabling unlimited flexibility without code changes.

**Key Capabilities:**
- Create custom fields per dunnage type
- Define field types (Text, Number, Dropdown, Date)
- Set required vs optional flags
- Configure default values
- Define dropdown options
- Control display order
- Apply validation rules

---

## Configurable Settings Per Type

Each dunnage type can have **0-N specification fields** configured independently.

### 1. Field Name

**Purpose**: Unique identifier and display label for the field

**Configuration**:
```
Field Name: [_____________________________]
  • 1-50 characters
  • Letters, numbers, spaces, hyphens, underscores
  • Must be unique within the type
  • Cannot start with number or special character
```

**Validation**:
- Required (cannot be empty)
- Uniqueness check (case-insensitive within type)
- Character restrictions enforced

**Examples**:
- ✅ "Condition"
- ✅ "Supplier Name"
- ✅ "Weight_Capacity"
- ✅ "Inspection-Date"
- ❌ "123Field" (starts with number)
- ❌ "Field!Name" (special character)

---

### 2. Field Type

**Purpose**: Determines the UI control and data validation

**Configuration**:
```
Field Type: [Dropdown ▼]
  Options:
    • Text
    • Number
    • Dropdown
    • Date
```

**Field Type Characteristics:**

**Text:**
- UI Control: TextBox
- Validation: Max length, regex pattern (optional)
- Storage: VARCHAR
- Example: "Supplier", "Notes", "Serial Number"

**Number:**
- UI Control: NumberBox with up/down arrows
- Validation: Min/max range, decimal places
- Storage: INT or DECIMAL
- Example: "Weight Capacity", "Count", "Temperature"

**Dropdown:**
- UI Control: ComboBox with predefined options
- Validation: Value must be in options list (unless Allow Custom enabled)
- Storage: VARCHAR
- Example: "Condition", "Size", "Color"

**Date:**
- UI Control: CalendarDatePicker
- Validation: Date format, min/max date range
- Storage: DATE or DATETIME
- Example: "Inspection Date", "Received Date", "Expiration Date"

**Changing Field Type:**
- See [CLARIFICATIONS.md - Edge Case 4](../../Module_Dunnage/CLARIFICATIONS.md#edge-case-4-spec-field-type-change-after-data-exists)
- **Recommendation**: Block type changes if historical data exists

---

### 3. Required Flag

**Purpose**: Determines if field must be filled before saving

**Configuration**:
```
☐ Required Field
  When checked:
    • Field must have value before save
    • Marked with asterisk (*) in UI
    • Validation error if empty
    • Blocks transaction save
```

**UI Impact:**
```
Guided Mode - Details Entry:
  Condition *               ← Asterisk indicates required
  ┌───────────────────────┐
  │ Good               ▼ │
  └───────────────────────┘
  ⚠ Required field (if empty)
```

**Changing Required Flag:**
- See [CLARIFICATIONS.md - Edge Case 5](../../Module_Dunnage/CLARIFICATIONS.md#edge-case-5-required-spec-field-becomes-optional-or-vice-versa)
- **Recommendation**: Forward-only application (doesn't retroactively affect historical data)

---

### 4. Default Value

**Purpose**: Pre-populate field with common value

**Configuration**:

**For Text Fields:**
```
Default Value: [_____________________________]
  • Free-form text
  • Pre-populated in new entries
  • User can override
```

**For Number Fields:**
```
Default Value: [______] ▲▼
  • Numeric value
  • Must be within min/max range (if set)
```

**For Dropdown Fields:**
```
Default Value: [Select option ▼]
  • Must be one of the dropdown options
  • Dropdown pre-selected to this value
```

**For Date Fields:**
```
Default Value: [Select ▼]
  Options:
    • Today
    • Tomorrow
    • Specific Date: [📅]
```

**When Defaults Apply:**
- ✅ New load creation in Guided Mode (Step 4: Details Entry)
- ✅ New row in Manual Entry Mode (via "Add Row" button)
- ❌ NOT applied in Edit Mode (preserves historical values)
- ❌ NOT applied when duplicating rows

---

### 5. Dropdown Options (Dropdown Fields Only)

**Purpose**: Define selectable values for dropdown fields

**Configuration**:
```
Dropdown Options (one per line):
┌─────────────────────────────┐
│ Excellent                   │
│ Good                        │
│ Fair                        │
│ Poor                        │
│ Damaged                     │
└─────────────────────────────┘

☐ Allow Custom Values
```

**Validation**:
- At least 1 option required
- Options cannot be empty
- Duplicate options not allowed (case-insensitive)
- Max 100 options per dropdown

**Allow Custom Values:**
- When checked: Users can enter values not in the list
- When unchecked: Users must select from predefined list only
- **Default**: Unchecked (strict validation)

**Changing Dropdown Options:**
- See [CLARIFICATIONS.md - Edge Case 7](../../Module_Dunnage/CLARIFICATIONS.md#edge-case-7-dropdown-spec-option-changes)
- **Recommendation**: Soft removal (mark inactive but preserve for historical data)

---

### 6. Number Field Configuration (Number Fields Only)

**Purpose**: Define valid numeric range and precision

**Configuration**:
```
Minimum Value: [______] ▲▼  (optional)
Maximum Value: [______] ▲▼  (optional)

Decimal Places: [0] ▲▼
  • 0 = Integer only
  • 1-4 = Decimal values allowed
```

**Validation Applied:**
```
If Min = 0, Max = 10000, Decimals = 0:
  - Valid: 0, 500, 2500, 10000
  - Invalid: -100 (below min)
  - Invalid: 15000 (above max)
  - Invalid: 25.5 (decimals not allowed)

If Min = 0, Max = 100, Decimals = 2:
  - Valid: 0, 25.5, 99.99, 100.00
  - Invalid: 150.5 (above max)
  - Invalid: 25.555 (too many decimals)
```

**Examples:**
- Weight Capacity: Min=0, Max=10000, Decimals=0
- Temperature: Min=-50, Max=150, Decimals=1
- Percentage: Min=0, Max=100, Decimals=2

---

### 7. Display Order

**Purpose**: Control field rendering sequence in UI

**Configuration**:
```
Display Order: [___] ▲▼
  • Integer value (0-999)
  • Lower numbers appear first
  • Ties broken by field name (alphabetical)
```

**Sorting Algorithm:**
```
1. Sort fields by Display Order (ascending)
2. If tied, sort by Field Name (alphabetical, case-insensitive)
```

**Example:**
```
Display Order: 0  → "Condition"        (shows first)
Display Order: 10 → "Supplier"
Display Order: 20 → "Inspection Date"
Display Order: 30 → "Weight Capacity"
Display Order: 30 → "Serial Number"    (tied, alphabetical)
```

**Admin Control:**
- **Drag-and-Drop**: Reorder fields visually
- **Manual Entry**: Enter display order value directly
- **Auto-Resequence**: Click button to set gaps of 10

---

## User Interface Design

### Main Settings View

**Access Path**: Admin Mode → Type Management → Select Type → "Manage Specifications"

**UI Layout:**
```
┌─────────────────────────────────────────────────────────────────────────┐
│ Manage Specifications: Wood Pallet 48x40                                │
│ ───────────────────────────────────────────────────────────────────────│
│                                                                          │
│ [➕ Add Field]  [⬆⬇ Reorder]  [🗑️ Delete Selected]                    │
│                                                                          │
│ Current Specifications (4 fields):                                      │
│                                                                          │
│ ┌──────────────────────────────────────────────────────────────────────┐│
│ │☑│Order│ Field Name        │ Type    │ Req │ Default │ Actions      ││
│ ├─┼─────┼──────────────────┼─────────┼─────┼─────────┼──────────────┤│
│ │☑│ 0   │ Condition         │ Dropdown│ ✓   │ Good    │ [Edit][Del]  ││
│ │☑│ 10  │ Supplier          │ Text    │ ✓   │         │ [Edit][Del]  ││
│ │ │ 20  │ Inspection Date   │ Date    │     │ Today   │ [Edit][Del]  ││
│ │ │ 30  │ Weight Capacity   │ Number  │ ✓   │ 2500    │ [Edit][Del]  ││
│ └──────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│ ⬆⬇ Drag rows to reorder  |  ☑ Select for bulk operations              │
│                                                                          │
│                                    [Close]                [Save Changes] │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### Add/Edit Specification Dialog

**Dialog UI:**
```
┌─────────────────────────────────────────────────────────────────────────┐
│ Add Specification Field                                                 │
│ ───────────────────────────────────────────────────────────────────────│
│                                                                          │
│ Field Name *                                                            │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Color                                                             │  │
│ └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│ Field Type *                                                            │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Dropdown                                                       ▼ │  │
│ └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│ ☑ Required Field                                                        │
│                                                                          │
│ Default Value                                                           │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Blue                                                              │  │
│ └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│ Dropdown Options (one per line) *                                       │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Red                                                               │  │
│ │ Blue                                                              │  │
│ │ Green                                                             │  │
│ │ Yellow                                                            │  │
│ │ White                                                             │  │
│ │ Black                                                             │  │
│ └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│ ☐ Allow Custom Values (users can enter values not in list)             │
│                                                                          │
│ Display Order                                                           │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ 40                                                           ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│                                  [Cancel]  [Save] [Save & Add Another]  │
└─────────────────────────────────────────────────────────────────────────┘
```

**Field Type-Specific Configuration:**

**When "Number" selected:**
```
│ Number Field Configuration                                              │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Minimum Value: [0        ] ▲▼  (optional)                        │  │
│ │ Maximum Value: [10000    ] ▲▼  (optional)                        │  │
│ │ Decimal Places: [0       ] ▲▼  (0-4)                             │  │
│ └───────────────────────────────────────────────────────────────────┘  │
```

**When "Date" selected:**
```
│ Date Field Configuration                                                │
│ ┌───────────────────────────────────────────────────────────────────┐  │
│ │ Default Value: [Today           ▼]                               │  │
│ │   Options: Today, Tomorrow, Specific Date                         │  │
│ │                                                                    │  │
│ │ Minimum Date: [📅 01/01/2020] (optional)                         │  │
│ │ Maximum Date: [📅 12/31/2030] (optional)                         │  │
│ └───────────────────────────────────────────────────────────────────┘  │
```

---

## Search and Filter Capabilities

**Field List Filtering:**
```
┌─────────────────────────────────────────────────────────────────────────┐
│ 🔍 Search Fields: [_________________________]                          │
│                                                                          │
│ Filters:                                                                │
│   Field Type: [All Types    ▼]  Required: [All ▼]                      │
└─────────────────────────────────────────────────────────────────────────┘
```

**Filter Options:**
- **Field Type**: All / Text / Number / Dropdown / Date
- **Required**: All / Required Only / Optional Only
- **Search**: Filter by field name (real-time)

---

## Bulk Operations

### Bulk Delete

**Trigger**: Select multiple fields via checkbox, click "🗑️ Delete Selected"

**Confirmation:**
```
┌─────────────────────────────────────────────────────────────────────────┐
│ Confirm Bulk Delete                                                     │
│ ───────────────────────────────────────────────────────────────────────│
│                                                                          │
│ Delete 3 selected specification fields?                                 │
│                                                                          │
│ Fields to be deleted:                                                   │
│   • Condition (Dropdown, Required)                                      │
│   • Supplier (Text, Required)                                           │
│   • Inspection Date (Date, Optional)                                    │
│                                                                          │
│ ⚠ Warning: Historical data will be preserved but fields will no         │
│   longer appear in new transactions.                                    │
│                                                                          │
│                                        [Cancel]  [Delete]                │
└─────────────────────────────────────────────────────────────────────────┘
```

**Behavior:**
- Fields marked inactive (soft delete)
- Historical data preserved
- Fields no longer rendered in workflows
- Can be "restored" by recreating with same name

---

### Bulk Reorder

**Trigger**: Click "⬆⬇ Reorder" button

**UI Mode:**
- Enable drag-and-drop mode
- Show grabber icons on each row
- Drag rows to desired position
- Click "Save Order" when done
- Display Order values auto-updated (gaps of 10)

---

## Validation Rules

### Field Name Validation

```
Rules:
- Required (not empty)
- Length: 1-50 characters
- Allowed: Letters, numbers, spaces, hyphens, underscores
- Cannot start with: Number or special character
- Must be unique within type (case-insensitive)

Examples:
✅ "Condition"
✅ "Weight_Capacity"
✅ "Inspection-Date"
❌ "" (empty)
❌ "123Field" (starts with number)
❌ "Field!Name" (special character)
❌ "Condition" (duplicate - if already exists)
```

---

### Dropdown Options Validation

```
Rules:
- At least 1 option required
- Each option: 1-100 characters
- Options cannot be empty
- No duplicate options (case-insensitive)
- Max 100 options total

Examples:
✅ ["Excellent", "Good", "Fair", "Poor"]
✅ ["Small", "Medium", "Large", "X-Large"]
❌ [] (no options)
❌ ["", "Good", "Fair"] (empty option)
❌ ["Good", "good", "GOOD"] (duplicates)
```

---

### Default Value Validation

```
Rules (vary by field type):

Text:
- Max length: 255 characters
- No special validation

Number:
- Must be valid number
- Must be within min/max range (if set)
- Must respect decimal places setting

Dropdown:
- Must be one of the dropdown options
- Case-sensitive match

Date:
- Must be valid date format
- "Today" or "Tomorrow" keywords allowed
- Must be within min/max date range (if set)
```

---

## Integration with Dunnage Workflow

### Guided Mode - Details Entry

**Dynamic Field Rendering:**
```csharp
// Pseudocode: Render spec fields in Step 4
var type = session.SelectedType;
var specFields = await _dunnageService.GetSpecFieldsAsync(type.TypeId);

foreach (var field in specFields.OrderBy(f => f.DisplayOrder))
{
    var control = RenderFieldControl(field);
    
    if (field.Required)
    {
        control.Label += " *";
        control.Validation = RequiredFieldValidator;
    }
    
    if (!string.IsNullOrEmpty(field.DefaultValue))
    {
        control.Value = field.DefaultValue;
    }
    
    detailsPanel.Children.Add(control);
}
```

**Rendering Flow:**
1. Load spec fields for selected type
2. Sort by Display Order
3. Render appropriate UI control per field type
4. Apply default values
5. Mark required fields
6. Attach validation

---

### Manual Entry Mode - Grid Columns

**Dynamic Column Creation:**
```csharp
// Pseudocode: Create grid columns from spec fields
var type = GetSelectedTypeForRow(row);
var specFields = await _dunnageService.GetSpecFieldsAsync(type.TypeId);

// Remove existing spec columns
RemoveDynamicColumns();

// Add new spec columns
foreach (var field in specFields.OrderBy(f => f.DisplayOrder))
{
    var column = new DataGridColumn
    {
        Header = field.Required ? $"{field.FieldName} *" : field.FieldName,
        CellTemplate = GetCellTemplateForFieldType(field.FieldType),
        Width = GetColumnWidth(field.FieldType)
    };
    
    dataGrid.Columns.Add(column);
}
```

---

## Performance Considerations

### Caching Spec Field Definitions

**Strategy:**
```
On App Startup:
- Load all spec field definitions into memory cache
- Index by type_id for fast lookup
- Cache expiration: 1 hour or until admin changes

On Admin Change:
- Invalidate cache immediately
- Reload spec definitions
- Notify active workflows (optional)
```

**Benefits:**
- Eliminates database query per type selection
- Faster UI rendering
- Reduced database load

---

### Lazy Loading in Grid

**For Manual Entry Mode:**
```
Grid with 100+ rows:
- Don't load all spec definitions upfront
- Load on-demand when type selected in row
- Cache loaded definitions
- Virtualize rows (only render visible rows)
```

---

## Error Handling

### Validation Errors

**Field Name Duplicate:**
```
❌ Error: A specification field named "Condition" already exists for this type.
   Please use a different name.
```

**Invalid Default Value:**
```
❌ Error: Default value "15000" exceeds maximum allowed value of 10000.
   Please enter a value between 0 and 10000.
```

**Missing Dropdown Options:**
```
❌ Error: Dropdown fields must have at least one option.
   Please add dropdown options.
```

---

### Save Errors

**Database Constraint Violation:**
```
❌ Error: Failed to save specification field. 
   Reason: Database constraint violated (duplicate field name).
   Please refresh and try again.
```

**Concurrent Modification:**
```
⚠ Warning: Another administrator modified this type's specifications.
   Your changes may overwrite theirs. Continue?
   [Cancel] [Refresh] [Save Anyway]
```

---

## Audit Trail

### Change Tracking

**Tracked Events:**
- Spec field created
- Spec field modified (what changed)
- Spec field deleted
- Display order changed
- Dropdown options modified

**Audit Log Entry:**
```json
{
  "event_type": "spec_field_modified",
  "type_id": 1,
  "field_id": 5,
  "field_name": "Condition",
  "user": "admin@company.com",
  "timestamp": "2026-01-25T10:30:00Z",
  "changes": {
    "required": {"old": false, "new": true},
    "default_value": {"old": null, "new": "Good"}
  }
}
```

---

## Related Documentation

- [Dynamic Specification Fields](../../Module_Dunnage/01-Business-Rules/dynamic-specification-fields.md) - Business rules
- [Dunnage Type Management](./dunnage-type-management.md) - Type configuration
- [Guided Mode Specification](../../Module_Dunnage/02-Workflow-Modes/001-guided-mode-specification.md) - Field rendering
- [CLARIFICATIONS.md](../../Module_Dunnage/CLARIFICATIONS.md) - Edge cases

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

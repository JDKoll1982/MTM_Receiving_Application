# Workflow Preferences

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Admin Mode](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md), [Workflow Mode Selection](../../Module_Dunnage/01-Business-Rules/workflow-mode-selection.md)

---

## Purpose

Workflow Preferences allow users to configure personal preferences for how they interact with the dunnage receiving workflow. Administrators can also set default preferences that apply to all users.

---

## Access

**User Preferences**: Main Menu → Settings → Workflow Preferences  
**Default Preferences** (Admin): Admin Mode → Advanced Settings → Workflow Defaults  
**Permission**: All users (own preferences), Administrator (defaults)

---

## User Preferences UI

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Workflow Preferences                                            [?] Help [✕]│
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Configure your personal preferences for dunnage receiving workflows.        │
│                                                                              │
│ WORKFLOW MODE                                                               │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Default workflow mode (when starting new receiving)                         │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ ● Guided Mode (3-step wizard)                                         │  │
│ │ ○ Manual Entry Mode (grid-based entry)                                │  │
│ │ ○ Ask me each time                                                    │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ You can switch modes at any time during data entry.                      │
│                                                                              │
│ GRID BEHAVIOR                                                               │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Cell edit delay (milliseconds before auto-focus)                            │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 300                                                               ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Lower = faster typing, Higher = easier mouse navigation (100-1000)       │
│                                                                              │
│ Tab key navigation                                                          │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ ● Move to next field (Type → Part → Quantity → Specs)                │  │
│ │ ○ Move to next row (insert new row after last field)                 │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ Enter key behavior (after entering Quantity)                                │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ ● Move to first spec field                                            │  │
│ │ ○ Insert new row (skip specs)                                         │  │
│ │ ○ Do nothing (stay in Quantity field)                                 │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Choose based on whether you typically enter spec fields.                 │
│                                                                              │
│ GRID DISPLAY                                                                │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Row height                                                                  │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ ● Compact (30px)                                                      │  │
│ │ ○ Standard (40px)                                                     │  │
│ │ ○ Comfortable (50px)                                                  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ Font size                                                                   │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ ● Small (11px)                                                        │  │
│ │ ○ Medium (13px)                                                       │  │
│ │ ○ Large (16px)                                                        │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ ☐ Show row numbers                                                         │
│ ☑ Highlight row on hover                                                   │
│ ☑ Show icons in Type column                                                │
│                                                                              │
│ AUTO-SAVE & VALIDATION                                                      │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Auto-save changes after each row (recommended)                           │
│ ℹ When unchecked, you must click "Save All" to commit changes.             │
│                                                                              │
│ ☑ Validate data immediately (show errors as you type)                      │
│ ℹ When unchecked, validation runs only on save.                            │
│                                                                              │
│ ☑ Confirm before deleting rows                                             │
│                                                                              │
│                      [Reset to Defaults]  [Cancel]  [Save]                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Preference Categories

### Category 1: Workflow Mode

#### Default Workflow Mode

**Control**: Radio buttons

**Options**:
1. **Guided Mode (3-step wizard)** - Default
2. **Manual Entry Mode (grid-based entry)**
3. **Ask me each time** - Shows mode selection dialog on launch

**Behavior**:
```
When user launches Dunnage module:

If preference = "Guided Mode":
→ Launch directly into Guided Mode Step 1

If preference = "Manual Entry Mode":
→ Launch directly into Manual Entry Grid

If preference = "Ask me each time":
→ Show mode selection dialog:
   ┌───────────────────────────────────────────┐
   │ Select Workflow Mode                      │
   │ ─────────────────────────────────────────│
   │                                            │
   │ How would you like to enter data?         │
   │                                            │
   │ [Guided Mode]   [Manual Entry Mode]       │
   │                                            │
   │ ☐ Remember my choice                      │
   └───────────────────────────────────────────┘
```

**Default Value**: Guided Mode (easier for new users)

---

### Category 2: Grid Behavior

#### Cell Edit Delay

**Control**: Number spinner (milliseconds)

**Range**: 100-1000 ms

**Default**: 300 ms

**Behavior**:
```
User clicks cell:
→ Wait {cell_edit_delay} ms
→ Enter edit mode automatically
→ Focus text input

Purpose:
- Lower delay (100-200): Faster for keyboard-heavy users
- Higher delay (400-600): Easier for mouse users (avoid accidental edits)
```

**Recommendation**: 300 ms (balance between speed and safety)

---

#### Tab Key Navigation

**Control**: Radio buttons

**Options**:
1. **Move to next field** (Type → Part → Quantity → Spec1 → Spec2...)
2. **Move to next row** (Type → Part → Quantity → Type[next row])

**Behavior**:
```
Option 1: Move to next field
User types Quantity, presses Tab:
→ Move to first spec field (if exists)
→ If no spec fields, move to next row Type field

Option 2: Move to next row
User types Quantity, presses Tab:
→ Skip spec fields
→ Move to next row Type field
→ Insert new row if on last row
```

**Default**: Move to next field (ensures all data captured)

---

#### Enter Key Behavior

**Control**: Radio buttons

**Options**:
1. **Move to first spec field** (if specs exist)
2. **Insert new row** (skip specs)
3. **Do nothing** (stay in Quantity field)

**Behavior**:
```
Option 1: Move to first spec field
User types Quantity, presses Enter:
→ If spec fields configured for type: Move to first spec
→ If no spec fields: Insert new row

Option 2: Insert new row
User types Quantity, presses Enter:
→ Always insert new row (skip specs)
→ Focus Type field in new row

Option 3: Do nothing
User types Quantity, presses Enter:
→ Stay in Quantity field (allow correction)
→ User must press Tab or click to move
```

**Default**: Move to first spec field (ensures complete data)

---

### Category 3: Grid Display

#### Row Height

**Control**: Radio buttons

**Options**:
1. **Compact** (30px) - More rows visible
2. **Standard** (40px) - Default
3. **Comfortable** (50px) - Easier clicking/touching

**Visual Examples**:
```
Compact (30px):
┌────────────────────────────────────┐
│Type│Part│Qty│Spec1│Spec2│         │ ← Less vertical space
├────┼────┼───┼─────┼─────┼─────────┤
│... │... │...│...  │...  │         │
└────────────────────────────────────┘

Standard (40px):
┌────────────────────────────────────┐
│ Type │ Part │ Qty │ Spec1 │ Spec2 │ ← Balanced
├──────┼──────┼─────┼───────┼───────┤
│ ...  │ ...  │ ... │ ...   │ ...   │
└────────────────────────────────────┘

Comfortable (50px):
┌────────────────────────────────────┐
│                                    │
│ Type │ Part │ Qty │ Spec1 │ Spec2 │ ← More vertical space
│                                    │
├──────┼──────┼─────┼───────┼───────┤
│ ...  │ ...  │ ... │ ...   │ ...   │
└────────────────────────────────────┘
```

**Default**: Standard (40px)

---

#### Font Size

**Control**: Radio buttons

**Options**:
1. **Small** (11px) - More data visible
2. **Medium** (13px) - Default
3. **Large** (16px) - Better readability

**Use Cases**:
- Small: High-resolution monitors, power users
- Medium: Standard desktop use
- Large: Accessibility, lower-resolution monitors, touchscreens

**Default**: Medium (13px)

---

#### Display Options

**Show Row Numbers** (Checkbox):
```
When checked:
┌───┬────────────────────────────────┐
│ # │ Type │ Part │ Qty │ ...       │
├───┼──────┼──────┼─────┼───────────┤
│ 1 │ ...  │ ...  │ ... │ ...       │
│ 2 │ ...  │ ...  │ ... │ ...       │
└───┴────────────────────────────────┘

When unchecked:
┌────────────────────────────────────┐
│ Type │ Part │ Qty │ ...           │
├──────┼──────┼─────┼───────────────┤
│ ...  │ ...  │ ... │ ...           │
└────────────────────────────────────┘
```

**Default**: Unchecked (cleaner interface)

---

**Highlight Row on Hover** (Checkbox):
```
When checked:
User hovers over row → Background changes to light blue/gray

When unchecked:
No hover effect (faster rendering on slow systems)
```

**Default**: Checked (better visual feedback)

---

**Show Icons in Type Column** (Checkbox):
```
When checked:
Type column displays: 🪵 Wood Pallet 48x40

When unchecked:
Type column displays: Wood Pallet 48x40 (text only)
```

**Default**: Checked (visual clarity)

---

### Category 4: Auto-Save & Validation

#### Auto-Save Changes

**Control**: Checkbox

**Behavior**:
```
When checked:
User completes row (moves to next row or clicks elsewhere)
→ Save row to database immediately
→ Show success indicator (green checkmark)
→ If error: Show error, keep row editable

When unchecked:
User completes row
→ Row marked as unsaved (yellow indicator)
→ Changes kept in memory only
→ User must click "Save All" to commit
→ Risk: Data loss if app crashes or closes
```

**Default**: Checked (recommended for data safety)

**Warning Message** (if user unchecks):
```
┌─────────────────────────────────────────────────────┐
│ Warning: Auto-Save Disabled                         │
│ ───────────────────────────────────────────────────│
│                                                      │
│ When auto-save is disabled, you must manually save │
│ changes using the "Save All" button.                │
│                                                      │
│ ⚠ Unsaved changes may be lost if the application   │
│   crashes or closes unexpectedly.                   │
│                                                      │
│ Are you sure you want to disable auto-save?         │
│                                                      │
│                      [Cancel]  [Disable]            │
└─────────────────────────────────────────────────────┘
```

---

#### Validate Data Immediately

**Control**: Checkbox

**Behavior**:
```
When checked (default):
User types in field
→ Validation runs on blur (field loses focus)
→ Show errors immediately (red border, error message)
→ User sees issues before moving to next field

When unchecked:
User types in field
→ No validation until save
→ All validation errors shown at save time
→ Faster for power users (no interruptions)
```

**Default**: Checked (catch errors early)

---

#### Confirm Before Deleting Rows

**Control**: Checkbox

**Behavior**:
```
When checked (default):
User clicks delete row:
→ Show confirmation dialog:
   ┌───────────────────────────────────────────┐
   │ Confirm Delete                            │
   │ ─────────────────────────────────────────│
   │                                            │
   │ Delete this load?                         │
   │ • Type: Wood Pallet 48x40                 │
   │ • Part: TUBE-A123                         │
   │ • Quantity: 100                           │
   │                                            │
   │ This action cannot be undone.             │
   │                                            │
   │              [Cancel]  [Delete]           │
   └───────────────────────────────────────────┘

When unchecked:
User clicks delete row:
→ Delete immediately without confirmation
→ Faster but riskier (accidental deletes)
```

**Default**: Checked (prevent accidental deletes)

---

## Admin Default Preferences

### Access

**Location**: Admin Mode → Advanced Settings → Workflow Defaults

**Purpose**: Set organization-wide default preferences applied to new users.

**UI Layout**:
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Workflow Default Preferences (Organization-Wide)                            │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ These settings apply to all new users. Existing users retain their personal │
│ preferences unless explicitly reset.                                         │
│                                                                              │
│ [Same preference fields as User Preferences UI]                             │
│                                                                              │
│ APPLY DEFAULTS                                                              │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ [Reset All Users to Defaults]  ⚠ Overwrites all user preferences!          │
│                                                                              │
│                                    [Cancel]  [Save Defaults]                │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### Reset All Users to Defaults

**Purpose**: Override all user preferences with current defaults.

**Confirmation**:
```
┌─────────────────────────────────────────────────────┐
│ Confirm Reset All User Preferences                  │
│ ───────────────────────────────────────────────────│
│                                                      │
│ This will reset preferences for ALL users (45 users)│
│ to the current default settings.                    │
│                                                      │
│ ⚠ WARNING:                                          │
│ • All user customizations will be lost              │
│ • Users will not be notified                        │
│ • This action cannot be undone                      │
│                                                      │
│ Use this only when rolling out major workflow      │
│ changes or standardizing across organization.       │
│                                                      │
│ Are you absolutely sure?                            │
│                                                      │
│                      [Cancel]  [Reset All]          │
└─────────────────────────────────────────────────────┘
```

---

## Reset to Defaults

### User Action

**Access**: User Preferences → "Reset to Defaults" button

**Behavior**:
```
User clicks "Reset to Defaults"
→ Show confirmation dialog
→ On confirm:
  → Load default preferences from admin settings
  → Apply to current user
  → Save to user_preferences table
  → Refresh UI
  → Show success message: "Preferences reset to organization defaults."
```

---

## Data Storage

### User Preferences Table

```sql
CREATE TABLE user_preferences (
    user_id INT,
    preference_key VARCHAR(100),
    preference_value TEXT,
    updated_date DATETIME,
    PRIMARY KEY (user_id, preference_key),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
```

**Example Rows**:
```sql
INSERT INTO user_preferences VALUES
(1, 'dunnage_default_mode', 'guided', NOW()),
(1, 'dunnage_grid_cell_delay', '300', NOW()),
(1, 'dunnage_grid_tab_nav', 'next_field', NOW()),
(1, 'dunnage_grid_enter_behavior', 'first_spec', NOW()),
(1, 'dunnage_grid_row_height', '40', NOW()),
(1, 'dunnage_grid_font_size', '13', NOW()),
(1, 'dunnage_grid_show_row_numbers', 'false', NOW()),
(1, 'dunnage_grid_highlight_hover', 'true', NOW()),
(1, 'dunnage_grid_show_icons', 'true', NOW()),
(1, 'dunnage_auto_save', 'true', NOW()),
(1, 'dunnage_validate_immediate', 'true', NOW()),
(1, 'dunnage_confirm_delete', 'true', NOW());
```

---

### System Settings Table (Defaults)

```sql
CREATE TABLE system_settings (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value TEXT,
    setting_type VARCHAR(20),
    updated_date DATETIME,
    updated_by INT,
    FOREIGN KEY (updated_by) REFERENCES users(user_id)
);
```

**Example Rows**:
```sql
INSERT INTO system_settings VALUES
('dunnage_default_mode_default', 'guided', 'string', NOW(), 1),
('dunnage_grid_cell_delay_default', '300', 'number', NOW(), 1),
...
```

---

## Preference Access Pattern

```csharp
public class Service_UserPreferences
{
    public T GetPreference<T>(int userId, string key, T systemDefault)
    {
        // Try user preference first
        var userPref = _dao.GetUserPreference(userId, key);
        if (userPref != null)
            return (T)Convert.ChangeType(userPref, typeof(T));
        
        // Fall back to system default
        var sysPref = _dao.GetSystemSetting($"{key}_default");
        if (sysPref != null)
            return (T)Convert.ChangeType(sysPref, typeof(T));
        
        // Fall back to hard-coded default
        return systemDefault;
    }
}
```

---

## Related Documentation

- [Admin Mode Specification](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md)
- [Workflow Mode Selection Business Rule](../../Module_Dunnage/01-Business-Rules/workflow-mode-selection.md)
- [Guided Mode](../../Module_Dunnage/02-Workflow-Modes/001-guided-mode-specification.md)
- [Manual Entry Mode](../../Module_Dunnage/02-Workflow-Modes/002-manual-entry-mode-specification.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

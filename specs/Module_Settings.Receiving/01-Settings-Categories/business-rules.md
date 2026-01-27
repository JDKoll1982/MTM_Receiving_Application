# Business Rules

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [CLARIFICATIONS.md](../../Module_Receiving/CLARIFICATIONS.md), [Workflow Mode Selection](../../Module_Receiving/01-Business-Rules/workflow-mode-selection.md)

---

## Purpose

Business Rules provides configuration for core workflow behaviors including auto-save policies, save destinations, workflow mode preferences, and feature toggles. These settings control HOW the receiving workflow operates and where data is persisted.

---

## Access

**Location**: Main Menu → Settings → Business Rules  
**Permission**: Administrator (system-wide settings) OR User (personal preferences)  
**Scope**: System-wide defaults + per-user overrides

---

## Settings Overview

This category contains **7 configurable business rules** that control workflow behavior and feature toggles.

**Note**: All transactions are ALWAYS saved to both database and network CSV. Local CSV export is optional.

---

## Business Rules Settings

### 1. Save Destination Configuration

#### Save to Local CSV Enabled

**Setting Key**: `Receiving.BusinessRules.SaveToCsvEnabled`  
**Purpose**: Enable/disable saving transactions to local CSV file (in addition to database and network CSV).

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (save local CSV), `false` (skip local CSV)  
**Scope**: System-wide

**Behavior**:
```
When true:
- On transaction save: Export to local CSV file
- CSV saved to: Core.Export.LocalPath + "\Receiving\"
- Filename: receiving_export_{timestamp}.csv
- Database and network CSV ALWAYS saved (regardless of this setting)

When false:
- Skip local CSV export
- Database and network CSV still saved
```

**UI Component**: Checkbox  
☑ Save to local CSV (in addition to database and network CSV)

**Related**: CSV paths configured in Module_Settings.Core

**Note**: Database and network CSV are ALWAYS saved and cannot be disabled.

---

### 2. Workflow Mode Preferences

#### Default Mode on Startup

**Setting Key**: `Receiving.BusinessRules.DefaultModeOnStartup`  
**Purpose**: Set which workflow mode loads by default.

**Data Type**: String (enum)  
**Default Value**: `"Wizard"` (12-step wizard)  
**Valid Values**: 
- `"Wizard"` - 12-step wizard mode
- `"Manual"` - Manual entry grid mode
- `"Ask"` - Prompt user to choose

**Scope**: User preference (per-user setting)

**Behavior**:
```
On application startup:
- If "Wizard": Load Wizard Mode directly
- If "Manual": Load Manual Entry Mode directly
- If "Ask": Show mode selection dialog
```

**UI Component**: Radio buttons  
Default workflow mode when starting receiving:  
● Wizard Mode (12-step guided workflow)  
○ Manual Entry Mode (grid-based entry)  
○ Ask me each time

**Related**: Edge Case 1 (mode switching allowed)

---

#### Remember Last Mode

**Setting Key**: `Receiving.BusinessRules.RememberLastMode`  
**Purpose**: Remember user's last selected mode and use it as default.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (remember), `false` (use default)  
**Scope**: User preference (per-user setting)

**Behavior**:
```
When true:
- Track last mode user selected
- On next session: Load that mode automatically
- Overrides DefaultModeOnStartup setting

When false:
- Always use DefaultModeOnStartup
- Don't remember user's last choice
```

**UI Component**: Checkbox  
☑ Remember my last selected mode (overrides default mode)

---

#### Confirm Mode Change

**Setting Key**: `Receiving.BusinessRules.ConfirmModeChange`  
**Purpose**: Show confirmation dialog when switching modes mid-workflow.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (confirm), `false` (switch immediately)  
**Scope**: User preference (per-user setting)

**Behavior**:
```
When true:
- On mode switch: Show dialog "Switch modes? Current data will migrate."
- User can cancel mode change
- Data migrated after confirmation

When false:
- Switch modes immediately
- No confirmation dialog
- Data migrated automatically
```

**UI Component**: Checkbox  
☑ Confirm before switching workflow modes (recommended to prevent accidental switches)

**Related**: Edge Case 1 (mode switching with data migration)

---

### 3. Feature Toggles

#### Auto-Fill Heat/Lot Enabled

**Setting Key**: `Receiving.BusinessRules.AutoFillHeatLotEnabled`  
**Purpose**: Enable "Auto-Fill" button to copy Heat/Lot from previous rows.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (enabled), `false` (disabled/hidden)  
**Scope**: System-wide

**Behavior**:
```
When true:
- Show "Auto-Fill" button in Heat/Lot step (Wizard Mode)
- Show "Fill Down" option in grid context menu (Manual Mode)
- Copy Heat/Lot values from rows above

When false:
- Hide Auto-Fill button
- User must enter Heat/Lot manually for each load
```

**UI Component**: Checkbox  
☑ Enable Auto-Fill Heat/Lot feature (copy from previous rows)

---

#### Save Package Type as Default

**Setting Key**: `Receiving.BusinessRules.SavePackageTypeAsDefault`  
**Purpose**: Enable checkbox to save package type preference per part.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (enabled), `false` (disabled/hidden)  
**Scope**: System-wide

**Behavior**:
```
When true:
- Show "Save as default for this part" checkbox in Package Type step
- User can create Package Type Preferences
- Next time part is received, default package type pre-selected

When false:
- Hide "Save as default" checkbox
- User cannot create preferences (always manual selection)
```

**UI Component**: Checkbox  
☑ Allow users to save package type preferences per part

**Related**: See [Package Type Preferences](./package-type-preferences.md) category

---

#### Show Review Table by Default

**Setting Key**: `Receiving.BusinessRules.ShowReviewTableByDefault`  
**Purpose**: Set default view mode for Review step (Wizard Mode).

**Data Type**: Boolean  
**Default Value**: `true` (table view)  
**Valid Values**: `true` (table), `false` (single view)  
**Scope**: User preference (per-user setting)

**Behavior**:
```
When true:
- Review step opens in Table View (all loads visible)
- User can switch to Single View if needed

When false:
- Review step opens in Single View (one load at a time)
- User can switch to Table View if needed
```

**UI Component**: Radio buttons  
Default Review step view:  
● Table View (all loads in grid)  
○ Single View (one load at a time with Previous/Next)

---

#### Allow Edit After Save

**Setting Key**: `Receiving.BusinessRules.AllowEditAfterSave`  
**Purpose**: Enable Edit Mode functionality (edit saved transactions).

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (enabled), `false` (disabled)  
**Scope**: System-wide

**Behavior**:
```
When true:
- Edit Mode available in mode selection
- Users can search and edit historical transactions
- Audit trail tracks all changes

When false:
- Edit Mode hidden/disabled
- Transactions cannot be modified after save
- Reduces risk of data tampering
```

**UI Component**: Checkbox  
☑ Allow editing saved transactions (Edit Mode)

**Warning**: Disabling this prevents correcting data entry errors after save.

**Related**: Edge Case 15 (audit trail), Edge Case 20 (concurrent editing)

**Note**: Edit Mode queries database - database saving is always enabled.

---

## User Interface Design

### Main Settings View

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Business Rules                                           [?] Help [✕] Close │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Configure workflow behaviors and feature toggles for receiving module.      │
│                                                                              │
│ SAVE DESTINATIONS                                                           │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Save to local CSV (in addition to database and network CSV)              │
│ ℹ Export transaction to CSV file on your computer                          │
│                                                                              │
│ ℹ Database and network CSV are ALWAYS saved (cannot be disabled)           │
│                                                                              │
│ WORKFLOW MODE PREFERENCES                                                   │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Default workflow mode when starting receiving:                              │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ ● Wizard Mode (12-step guided workflow)                             │   │
│ │ ○ Manual Entry Mode (grid-based entry)                              │   │
│ │ ○ Ask me each time                                                   │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│ ☑ Remember my last selected mode (overrides default mode setting above)    │
│ ☑ Confirm before switching workflow modes (prevent accidental switches)    │
│                                                                              │
│ FEATURE TOGGLES                                                             │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Enable Auto-Fill Heat/Lot feature (copy from previous rows)              │
│ ☑ Allow users to save package type preferences per part                    │
│                                                                              │
│ Default Review step view:                                                   │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ ● Table View (all loads in grid)                                     │   │
│ │ ○ Single View (one load at a time with Previous/Next)                │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│ ☑ Allow editing saved transactions (Edit Mode)                             │
│ ℹ Enables Edit Mode. Disable to prevent modification of saved data.        │
│                                                                              │
│                      [Reset to Defaults]  [Cancel]  [Save]                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Integration with Receiving Workflows

### Wizard Mode
- Uses DefaultModeOnStartup or RememberLastMode to determine if Wizard loads
- Uses ShowReviewTableByDefault for Review step view
- Applies AutoFillHeatLotEnabled for Heat/Lot step
- Applies SavePackageTypeAsDefault for Package Type step
- On save: Database and network CSV ALWAYS saved, local CSV saved if SaveToCsvEnabled = true

### Manual Entry Mode
- Uses DefaultModeOnStartup or RememberLastMode to determine if Manual loads
- Applies AutoFillHeatLotEnabled for context menu options
- Applies SavePackageTypeAsDefault for package type column
- On save: Database and network CSV ALWAYS saved, local CSV saved if SaveToCsvEnabled = true

### Edit Mode
- Only available if AllowEditAfterSave = true
- Queries database for historical transactions (database always enabled)

---

## Session Isolation Impact

**Critical**: Changes to Business Rules settings do NOT affect active sessions (Session Isolation Principle).

```
Scenario:
1. User A starts Wizard Mode session
   - Session captures: SaveToCsvEnabled = true, DefaultModeOnStartup = "Wizard"
   
2. Admin changes SaveToCsvEnabled = false globally

3. User A continues session:
   - Still uses SaveToCsvEnabled = true (session snapshot)
   - Admin's change does NOT affect User A's unsaved work
   
4. User A saves transaction and starts new session:
   - New session uses SaveToCsvEnabled = false (new snapshot)
```

See: [Session Isolation Principle](../../Module_Receiving/CLARIFICATIONS.md#session-isolation-rule)

---

## Validation Rules

**No validation rules required** - Database and network CSV saving are mandatory and cannot be disabled.

Local CSV saving is optional (SaveToCsvEnabled can be true or false).

---

## Database Schema

All business rules stored in `system_settings` or `user_preferences` table:

```sql
-- System-wide settings
INSERT INTO system_settings (category, key_name, value, data_type)
VALUES ('Receiving', 'BusinessRules.SaveToCsvEnabled', 'true', 'Boolean');

-- User-specific settings (user preference overrides system default)
INSERT INTO user_preferences (user_id, category, key_name, value, data_type)
VALUES (123, 'Receiving', 'BusinessRules.DefaultModeOnStartup', 'Manual', 'String');
```

---

## Performance Considerations

### Save Operations
```
Every transaction save performs:
- Database INSERT/UPDATE (always)
- Network CSV export (always)
- Local CSV export (if SaveToCsvEnabled = true)

Network CSV adds ~100-500ms latency if network is slow.
Database and network CSV cannot be disabled - they are critical for data integrity and sharing.
```

---

## Related Edge Cases (from CLARIFICATIONS.md)

- **Edge Case 1**: Mode switching mid-workflow → ConfirmModeChange setting
- **Edge Case 2**: Session recovery → No retention (discard on cancel)
- **Edge Case 15**: Edit Mode audit trail → AllowEditAfterSave enables tracking
- **Edge Case 16**: CSV re-export → SaveToCsvEnabled, SaveToNetworkCsvEnabled

---

## Related Documentation

- [Workflow Mode Selection](../../Module_Receiving/01-Business-Rules/workflow-mode-selection.md)
- [CSV Export Paths](../../Module_Receiving/01-Business-Rules/csv-export-paths.md)
- [Wizard Mode Specification](../../Module_Receiving/02-Workflow-Modes/001-wizard-mode-specification.md)
- [Manual Entry Mode Specification](../../Module_Receiving/02-Workflow-Modes/002-manual-entry-mode-specification.md)
- [Edit Mode Specification](../../Module_Receiving/02-Workflow-Modes/003-edit-mode-specification.md)
- [Package Type Preferences](./package-type-preferences.md)

---

**Last Updated:** 2026-01-25  
**Status:** Complete  
**Settings Count:** 7 business rules



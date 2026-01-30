# Default Values

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [CLARIFICATIONS.md](../../Module_Receiving/CLARIFICATIONS.md), [Part Number Management](./part-number-management.md)

---

## Purpose

Default Values provides system-wide default settings that pre-populate fields in receiving workflows, reducing data entry burden and ensuring consistency. These defaults apply when no part-specific or user-specific preference exists.

---

## Access

**Location**: Main Menu → Settings → Default Values  
**Permission**: Administrator (system-wide defaults)  
**Scope**: System-wide (applies to all users unless overridden)

---

## Settings Overview

This category contains **6 configurable default values** that provide smart defaults for new receiving transactions.

**Note**: Weight per package is calculated automatically (Total Weight ÷ Packages Per Load) and is not a configurable setting.

---

## Default Values Settings

### 1. Package Configuration Defaults

#### Default Package Type

**Setting Key**: `Receiving.Defaults.DefaultPackageType`  
**Purpose**: Set default package type when no part-specific preference exists.

**Data Type**: String  
**Default Value**: `"Skid"`  
**Valid Values**: Any package type name (Skid, Pallet, Box, Bundle, Crate, etc.)  
**Scope**: System-wide (fallback if no part preference)

**Behavior**:
```
On package type selection:
1. Check Part-specific preference (highest priority)
2. If no preference: Use this default value
3. User can change on-the-fly

Example:
- Part MMC0001000: Has preference "Bundle" → Use "Bundle"
- Part XYZ999: No preference → Use default "Skid"
```

**UI Component**: Dropdown (populated from available package types)  
Default package type: [Skid ▼]

**Override Hierarchy**:
```
1. Part-specific preference (from Package Type Preferences)
2. Default Package Type (this setting)
3. Hardcoded fallback: "Skid"
```

**Related**: See [Package Type Preferences](./package-type-preferences.md) for part-specific overrides

---

#### Default Packages Per Load

**Setting Key**: `Receiving.Defaults.DefaultPackagesPerLoad`  
**Purpose**: Set default number of packages per load.

**Data Type**: Integer  
**Default Value**: `1`  
**Valid Values**: `1` to `999`  
**Scope**: System-wide

**Behavior**:
```
On load entry:
- "Packages Per Load" field pre-filled with this value
- User can modify per load
- Commonly 1 for skids, 4-6 for pallets of boxes

Example:
- DefaultPackagesPerLoad = 1 → Skids typically have 1 bundle
- DefaultPackagesPerLoad = 4 → Pallets typically have 4 boxes

Weight Per Package Calculation:
- If user enters Total Weight = 200 lbs, Packages = 4
- Weight Per Package = 200 ÷ 4 = 50 lbs (auto-calculated)
```

**UI Component**: Numeric input  
Default packages per load: [____1____] ▲▼  
ℹ Typical number of packages on a single load/skid (1-999)

**Note**: Weight per package is calculated automatically, not a default setting.

---

### 2. Location Defaults

#### Default Receiving Location

**Setting Key**: `Receiving.Defaults.DefaultLocation`  
**Purpose**: Set default receiving location when no part-specific default exists.

**Data Type**: String  
**Default Value**: `"RECV"` (generic receiving location)  
**Valid Values**: Any valid Infor Visual location code  
**Scope**: System-wide (fallback if no part default)

**Behavior**:
```
On transaction save:
1. Check Part-specific default location (highest priority)
2. If no part default: Use this system default
3. User can override per transaction

Example:
- Part MMC0001000: Has default "V-C0-01" → Use "V-C0-01"
- Part XYZ999: No default → Use system default "RECV"
```

**UI Component**: Text input (validated against Infor Visual locations)  
Default receiving location: [__RECV__]  
ℹ Fallback location when no part-specific default exists

**Override Hierarchy**:
```
1. User session override (manual entry during transaction)
2. Part-specific default location (from Part Number Management)
3. Default Receiving Location (this setting)
4. Hardcoded fallback: "RECV"
```

**Related**: See [Part Number Management](./part-number-management.md) for part-specific defaults  
**Related**: Edge Case 10 (location override scope)

---

### 3. Load Number Defaults

#### Default Load Number Prefix

**Setting Key**: `Receiving.Defaults.DefaultLoadNumberPrefix`  
**Purpose**: Set prefix for auto-generated load numbers (optional).

**Data Type**: String  
**Default Value**: `""` (empty - no prefix)  
**Valid Values**: Any alphanumeric string (0-10 characters)  
**Scope**: System-wide

**Behavior**:
```
On load number generation:
- Load numbers auto-calculated as sequential integers (1, 2, 3, ...)
- If prefix set: Prepend prefix to load number

Example:
- Prefix = "" → Load numbers: 1, 2, 3, 4, 5
- Prefix = "L-" → Load numbers: L-1, L-2, L-3, L-4, L-5
- Prefix = "RCV" → Load numbers: RCV1, RCV2, RCV3, RCV4, RCV5
```

**UI Component**: Text input (max 10 characters)  
Load number prefix (optional): [________]  
ℹ Prefix for auto-generated load numbers (leave empty for numeric only)

**Examples**:
```
No prefix:
Load #1, Load #2, Load #3

With prefix "SKD-":
Load #SKD-1, Load #SKD-2, Load #SKD-3

With prefix "A":
Load #A1, Load #A2, Load #A3
```

**Related**: [Load Number Dynamics](../../Module_Receiving/01-Business-Rules/load-number-dynamics.md)  
**Related**: Edge Case 3 (load numbers not manually changeable)

---

### 4. Workflow Mode Defaults

#### Default Receiving Mode

**Setting Key**: `Receiving.Defaults.DefaultReceivingMode`  
**Purpose**: Set default workflow mode (if Business Rules setting not configured).

**Data Type**: String (enum)  
**Default Value**: `"Wizard"`  
**Valid Values**: `"Wizard"`, `"Manual"`, `"Ask"`  
**Scope**: System-wide (unless user preference overrides)

**Behavior**:
```
On application startup:
1. Check Business Rules setting: BusinessRules.RememberLastMode
   - If true: Use user's last selected mode
2. Check Business Rules setting: BusinessRules.DefaultModeOnStartup
   - If configured: Use that mode
3. Fallback: Use this default value

Example:
- DefaultReceivingMode = "Wizard" → Opens in Wizard Mode by default
- DefaultReceivingMode = "Manual" → Opens in Manual Entry Mode
- DefaultReceivingMode = "Ask" → Shows mode selection dialog
```

**UI Component**: Dropdown  
Default receiving mode: [Wizard Mode ▼]  
ℹ Wizard Mode, Manual Entry Mode, or Ask Each Time

**Note**: This setting is typically overridden by Business Rules settings. Use this as final fallback only.

**Related**: [Business Rules - Default Mode on Startup](./business-rules.md#default-mode-on-startup)

---

### 5. Unit of Measure Defaults

#### Default Unit of Measure

**Setting Key**: `Receiving.Defaults.DefaultUnitOfMeasure`  
**Purpose**: Set default unit of measure for quantity entry.

**Data Type**: String  
**Default Value**: `"LBS"` (pounds)  
**Valid Values**: `"LBS"`, `"KG"`, `"EA"` (each), `"FT"` (feet), `"M"` (meters), etc.  
**Scope**: System-wide

**Behavior**:
```
On quantity entry:
- Unit of measure field pre-filled with this value
- User can change if needed
- Typically "LBS" for weight-based receiving, "EA" for count-based

Example:
- DefaultUnitOfMeasure = "LBS" → Quantity in pounds
- DefaultUnitOfMeasure = "EA" → Quantity in pieces/count
- DefaultUnitOfMeasure = "FT" → Quantity in linear feet (tubing, etc.)
```

**UI Component**: Dropdown (populated from Infor Visual UOM codes)  
Default unit of measure: [LBS ▼]  
ℹ LBS (pounds), KG (kilograms), EA (each), FT (feet), etc.

**Integration**:
```
ERP Integration:
- Pull valid UOM codes from Infor Visual
- Validate user selection against ERP

Display:
- Quantity field: [____500____] [LBS ▼]
- User can change UOM per transaction
```

---

## User Interface Design

### Main Settings View

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Default Values                                           [?] Help [✕] Close │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Configure system-wide default values for receiving workflows.               │
│ These defaults apply when no part-specific or user preference exists.       │
│                                                                              │
│ PACKAGE CONFIGURATION DEFAULTS                                              │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Default package type (when no part preference exists)                       │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ Skid                                                             ▼   │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Options: Skid, Pallet, Box, Bundle, Crate, etc.                          │
│                                                                              │
│ Default packages per load                                                   │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ 1                                                                ▲▼  │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Typical number of packages on a single load/skid (1-999)                 │
│ ℹ Weight per package is auto-calculated (Total Weight ÷ Packages)          │
│                                                                              │
│ LOCATION DEFAULTS                                                           │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Default receiving location (fallback when no part-specific default)         │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ RECV                                                                  │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Must be a valid Infor Visual location code                               │
│                                                                              │
│ [Validate Location]                                                         │
│                                                                              │
│ LOAD NUMBER DEFAULTS                                                        │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Load number prefix (optional)                                               │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │                                                                       │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Leave empty for numeric load numbers only (e.g., 1, 2, 3)                │
│ ℹ Or enter prefix (e.g., "L-" produces L-1, L-2, L-3)                      │
│                                                                              │
│ WORKFLOW MODE DEFAULTS                                                      │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Default receiving mode (fallback if no Business Rules configured)           │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ Wizard Mode                                                      ▼   │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Options: Wizard Mode, Manual Entry Mode, Ask Each Time                   │
│ ℹ Note: Business Rules settings take precedence over this default          │
│                                                                              │
│ UNIT OF MEASURE DEFAULTS                                                    │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Default unit of measure                                                     │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ LBS (pounds)                                                     ▼   │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Options: LBS, KG, EA (each), FT (feet), M (meters), etc.                 │
│                                                                              │
│                      [Reset to Defaults]  [Cancel]  [Save]                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Integration with Receiving Workflows

### Wizard Mode - Default Value Application

**Step 1 (Mode Selection)**:
- Use DefaultReceivingMode to determine if Wizard loads

**Step 5 (Load Entry)**:
- No defaults (user enters load count manually)

**Step 6 (Quantity Entry)**:
- Use DefaultUnitOfMeasure for UOM field

**Step 8 (Package Type)**:
1. Check Package Type Preferences for current part
2. If no preference: Use DefaultPackageType
3. Pre-fill DefaultPackagesPerLoad
4. Pre-fill DefaultWeightPerPackage (if applicable)

**Step 10 (Review/Save)**:
1. Check Part Number Management for default location
2. If no part default: Use DefaultLocation
3. Apply DefaultLoadNumberPrefix to load numbers (if set)

---

### Manual Entry Mode - Default Value Application

**Row Creation**:
- Package Type column: DefaultPackageType (or part preference)
- Packages Per Load column: DefaultPackagesPerLoad
- Weight Per Package column: DefaultWeightPerPackage
- UOM: DefaultUnitOfMeasure

**On Save**:
- Location: DefaultLocation (or part default)
- Load Numbers: Apply DefaultLoadNumberPrefix

---

### Override Hierarchy (Example: Package Type)

```
Priority (Highest to Lowest):
1. User manual selection (during transaction)
   Example: User selects "Bundle" in dropdown
   
2. Part-specific preference
   Example: Part MMC0001000 → "Bundle" (from Package Type Preferences)
   
3. System default (this setting)
   Example: DefaultPackageType = "Skid"
   
4. Hardcoded fallback
   Example: "Skid" (if all above fail)

Result:
- Part MMC0001000 with user selecting nothing → Use preference "Bundle"
- Part XYZ999 (no preference) with user selecting nothing → Use default "Skid"
- Part MMC0001000 with user selecting "Crate" → Override to "Crate"
```

---

## Validation Rules

### Location Validation
```
On save of DefaultLocation:
- Query Infor Visual to verify location exists
- If not found: Warning "Location {location} not found in ERP. Save anyway?"
- Allow save (warning, not error)
```

### Load Number Prefix Validation
```
DefaultLoadNumberPrefix rules:
- Max length: 10 characters
- Alphanumeric only (A-Z, 0-9, dash, underscore)
- No spaces allowed

Invalid examples:
❌ "LOAD NUMBER #" (too long, has spaces, special char)
❌ "L " (has space)

Valid examples:
✅ "" (empty - no prefix)
✅ "L-"
✅ "SKD"
✅ "RCV_"
```

### Packages Per Load Validation
```
DefaultPackagesPerLoad:
- Must be >= 1
- Must be <= 999
- Integer only (no decimals)

If < 1: Error "Packages per load must be at least 1"
If > 999: Error "Maximum 999 packages per load"
```

---

## Session Isolation Impact

**Critical**: Changes to Default Values settings do NOT affect active sessions (Session Isolation Principle).

```
Scenario:
1. User A starts transaction
   - Session snapshot: DefaultPackageType = "Skid"
   
2. Admin changes DefaultPackageType = "Pallet" globally

3. User A continues entering loads:
   - Still sees "Skid" as default (session snapshot)
   - Admin's change does NOT affect User A's active session
   
4. User A saves transaction and starts new transaction:
   - New session uses "Pallet" as default (new snapshot)
```

See: [Session Isolation Principle](../../Module_Receiving/CLARIFICATIONS.md#session-isolation-rule)

---

## Database Schema

All default values stored in `system_settings` table:

```sql
CREATE TABLE system_settings (
    setting_id INT PRIMARY KEY IDENTITY(1,1),
    category NVARCHAR(50) NOT NULL, -- 'Receiving'
    key_name NVARCHAR(100) NOT NULL, -- 'Defaults.DefaultPackageType'
    value NVARCHAR(MAX) NOT NULL, -- 'Skid'
    data_type NVARCHAR(20) NOT NULL, -- 'String', 'Integer', 'Decimal'
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    updated_by NVARCHAR(100),
    UNIQUE (category, key_name)
);
```

---

## Related Documentation

- [Part Number Management](./part-number-management.md) - Part-specific defaults
- [Package Type Preferences](./package-type-preferences.md) - Part-package associations
- [Business Rules](./business-rules.md) - Mode preferences and behaviors
- [Load Number Dynamics](../../Module_Receiving/01-Business-Rules/load-number-dynamics.md)
- [CLARIFICATIONS.md](../../Module_Receiving/CLARIFICATIONS.md) - Edge Cases 10 (location override)

---

**Last Updated:** 2026-01-25  
**Status:** Complete  
**Settings Count:** 6 default values  
**Note:** Weight per package is calculated automatically (Total Weight ÷ Packages Per Load) - not a setting


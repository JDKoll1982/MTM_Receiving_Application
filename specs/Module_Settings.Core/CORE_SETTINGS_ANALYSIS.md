# Module_Settings.Core - Identified Cross-Module Settings

**Date:** 2026-01-25  
**Purpose:** Identify settings that should be in Module_Settings.Core (accessible to ALL modules)  
**Source Analysis:** Module_Settings.Receiving + Module_Settings.Dunnage

---

## 🎯 Overview

After analyzing both Module_Settings.Receiving and Module_Settings.Dunnage specifications, the following settings categories appear in BOTH modules and should be centralized in Module_Settings.Core to avoid duplication.

---

## ✅ Settings That Should Be Module_Settings.Core

### 1. Accessibility & User Experience Settings ⚠️ CRITICAL

**Why Core?** Accessibility features should be consistent across ALL modules, not configured separately per module.

**Settings Identified:**

#### From Module_Settings.Receiving (Accessibility category)
```
EnableKeyboardShortcuts          → Core.Accessibility.EnableKeyboardShortcuts
EnableScreenReaderMode           → Core.Accessibility.EnableScreenReaderMode
HighContrastMode                 → Core.Accessibility.HighContrastMode
LargeFontMode                    → Core.Accessibility.LargeFontMode
ShowToolTips                     → Core.Accessibility.ShowToolTips
EnableAutoFocus                  → Core.Accessibility.EnableAutoFocus
TabNavigationMode                → Core.Accessibility.TabNavigationMode
```

#### From Module_Settings.Dunnage (Workflow Preferences - Display)
```
RowHeight                        → Core.Display.GridRowHeight
FontSize                         → Core.Display.FontSize
ShowRowNumbers                   → Core.Display.ShowRowNumbers
HighlightRowOnHover              → Core.Display.HighlightRowOnHover
```

**Recommendation:**
Create `Module_Settings.Core/01-Settings-Categories/accessibility-and-display.md` specification with:
- Keyboard shortcuts (global enable/disable, per-module customization)
- Screen reader support (global setting)
- High contrast mode (global theme)
- Font sizes (global preference with module-specific overrides)
- Grid display preferences (global defaults)

**Impact on Modules:**
- Receiving: Remove Accessibility Settings from module-specific settings
- Dunnage: Move grid display preferences to Core
- Future modules: Inherit accessibility settings from Core

---

### 2. CSV Export Configuration ⚠️ CRITICAL

**Why Core?** CSV export paths should be centralized - one local path, one network path for ALL modules.

**Settings Identified:**

#### From Module_Settings.Receiving (Advanced Settings - assumed)
```
CsvExportLocalPath               → Core.Export.LocalPath
CsvExportNetworkPath             → Core.Export.NetworkPath
CreateDatedSubfolders            → Core.Export.CreateDatedSubfolders
FailOnNetworkUnavailable         → Core.Export.FailOnNetworkUnavailable
```

#### From Module_Settings.Dunnage (Advanced Settings)
```
LocalExportPath                  → Core.Export.LocalPath
NetworkExportPath                → Core.Export.NetworkPath
CreateDatedSubfolders            → Core.Export.CreateDatedSubfolders
FailIfNetworkPathUnavailable     → Core.Export.FailOnNetworkUnavailable
```

**Duplication Detected:** ✅ Both modules have IDENTICAL CSV export settings.

**Recommendation:**
Create `Module_Settings.Core/01-Settings-Categories/csv-export-configuration.md` specification with:
- Base local export path: `C:\Users\{username}\AppData\Local\MTM\` (Core)
- Base network export path: `\\SERVER\Receiving\` (Core)
- Module-specific subfolders: `{base_path}\{module_name}\` (automatic)
- Dated subfolder option (global setting)
- Network fallback behavior (global setting)

**Implementation Example:**
```
Core Setting:
  LocalPath = C:\Users\{username}\AppData\Local\MTM\
  NetworkPath = \\SERVER01\Receiving\

Module_Receiving export path:
  → C:\Users\jdoe\AppData\Local\MTM\Receiving\2026-01-25\receiving_export.csv

Module_Dunnage export path:
  → C:\Users\jdoe\AppData\Local\MTM\Dunnage\2026-01-25\dunnage_export.csv
```

**Impact on Modules:**
- Receiving: Remove CSV path settings from Advanced Settings
- Dunnage: Remove CSV path settings from Advanced Settings
- All modules: Reference Core.Export.LocalPath + module name

---

### 3. Logging & Debug Configuration ⚠️ HIGH PRIORITY

**Why Core?** Application-wide logging should be centralized, not per-module.

**Settings Identified:**

#### From Module_Settings.Dunnage (Advanced Settings - Debug & Logging)
```
LoggingLevel                     → Core.Logging.Level (Info, Debug, Trace, Error)
EnableSqlQueryLogging            → Core.Logging.EnableSqlQueryLogging
LogValidationFailures            → Core.Logging.LogValidationFailures
LogCsvExportOperations           → Core.Logging.LogCsvExportOperations (or per-module?)
LogWorkflowModeSwitches          → Core.Logging.LogWorkflowModeSwitches (or per-module?)
```

**Recommendation:**
Create `Module_Settings.Core/01-Settings-Categories/logging-and-diagnostics.md` specification with:
- Global logging level (Info, Debug, Trace, Error)
- SQL query logging (global enable/disable)
- Validation logging (global enable/disable)
- Per-module logging toggles (CSV export, workflow switches, etc.)
- Log file management (view, clear, export)

**Impact on Modules:**
- All modules: Reference Core.Logging settings
- Module-specific log categories can be toggled independently

---

### 4. Grid Performance Tuning ⚠️ MEDIUM PRIORITY

**Why Core?** Grid performance settings should be consistent across all modules.

**Settings Identified:**

#### From Module_Settings.Dunnage (Advanced Settings - Grid Performance)
```
VirtualizationThreshold          → Core.Performance.GridVirtualizationThreshold
AutoSaveDebounceDelay            → Core.Performance.AutoSaveDebounceDelay
EnableAsyncValidation            → Core.Performance.EnableAsyncValidation
CacheDropdownOptions             → Core.Performance.CacheDropdownOptions
```

#### From Module_Settings.Dunnage (Workflow Preferences)
```
CellEditDelay                    → Core.Performance.CellEditDelay
```

**Recommendation:**
Create `Module_Settings.Core/01-Settings-Categories/performance-tuning.md` specification with:
- Grid virtualization threshold (rows before virtualization)
- Auto-save debounce delay (milliseconds)
- Async validation toggle
- Dropdown caching toggle
- Cell edit delay

**Impact on Modules:**
- All modules: Use same performance settings for consistent UX
- Advanced users can tune based on hardware capabilities

---

### 5. Database Maintenance ⚠️ MEDIUM PRIORITY

**Why Core?** Database retention policies should be centralized across all modules.

**Settings Identified:**

#### From Module_Settings.Dunnage (Advanced Settings - Database Maintenance)
```
TransactionHistoryRetention      → Core.Database.TransactionRetentionDays
AuditLogRetention                → Core.Database.AuditRetentionDays (never deleted)
```

**Recommendation:**
Create `Module_Settings.Core/01-Settings-Categories/database-maintenance.md` specification with:
- Transaction history retention (days) - applies to all modules
- Audit log retention (permanent, for reference only)
- Database optimization schedule
- Database statistics viewing

**Impact on Modules:**
- All modules: Transactions archived based on Core retention policy
- Module-specific tables inherit retention rules

---

### 6. System Information ⚠️ LOW PRIORITY (Nice to Have)

**Why Core?** System-level information should be centralized.

**Settings Identified:**

#### From Module_Settings.Dunnage (Advanced Settings - System Information)
```
ApplicationVersion               → Core.System.ApplicationVersion (read-only)
DatabaseVersion                  → Core.System.DatabaseVersion (read-only)
LastDatabaseMigration            → Core.System.LastMigrationDate (read-only)
ActiveUsers24h                   → Core.System.ActiveUsers (read-only)
```

**Recommendation:**
Display in Core settings or About dialog, not per-module.

---

## ⏳ Settings That Should Remain Module-Specific

### Module_Settings.Receiving
- **Validation Rules** ✅ Module-specific (PO validation, Part validation)
- **Business Rules** ✅ Module-specific (Receiving workflow behaviors)
- **Default Values** ✅ Module-specific (Default package type, location)
- **ERP Integration** ✅ Module-specific (Receiving-specific sync)
- **Workflow Settings** ✅ Module-specific (Receiving step titles)

### Module_Settings.Dunnage
- **Dunnage Type Management** ✅ Module-specific
- **Specification Field Configuration** ✅ Module-specific
- **Part Management** ✅ Module-specific
- **Inventory List Management** ✅ Module-specific
- **Workflow Preferences** ⚠️ MIXED (some Core, some module-specific)
  - Default workflow mode → Module-specific ✅
  - Tab key behavior → Module-specific ✅
  - Enter key behavior → Module-specific ✅
  - Auto-save toggle → Module-specific ✅
  - Grid display → **Move to Core** ⚠️

---

## 📊 Impact Summary

| Setting Category | Currently In | Should Be In | Impact |
|------------------|--------------|--------------|--------|
| Accessibility & Display | Receiving, Dunnage | **Core** | HIGH - Consistency |
| CSV Export Paths | Receiving, Dunnage | **Core** | HIGH - Avoid duplication |
| Logging & Diagnostics | Dunnage | **Core** | MEDIUM - Centralized logging |
| Grid Performance | Dunnage | **Core** | MEDIUM - Consistent UX |
| Database Maintenance | Dunnage | **Core** | MEDIUM - Policy consistency |
| System Information | Dunnage | **Core** | LOW - Nice to have |

---

## 🎯 Recommended Action Plan

### Phase 1: Create Module_Settings.Core Specifications
1. ⏳ Create `Module_Settings.Core/` folder structure
2. ⏳ Create `00-Core/purpose-and-overview.md`
3. ⏳ Create `00-Core/settings-architecture.md`
4. ⏳ Create `01-Settings-Categories/accessibility-and-display.md` (HIGH)
5. ⏳ Create `01-Settings-Categories/csv-export-configuration.md` (HIGH)
6. ⏳ Create `01-Settings-Categories/logging-and-diagnostics.md` (MEDIUM)
7. ⏳ Create `01-Settings-Categories/performance-tuning.md` (MEDIUM)
8. ⏳ Create `01-Settings-Categories/database-maintenance.md` (MEDIUM)

### Phase 2: Update Module-Specific Settings
1. ⏳ Update `Module_Settings.Receiving/` - Remove Core settings
2. ⏳ Update `Module_Settings.Dunnage/` - Remove Core settings
3. ⏳ Update `Module_Settings.Receiving/COMPLETION_ROADMAP.md` - Remove Accessibility from Priority 2
4. ⏳ Update `Module_Settings.Dunnage/advanced-settings.md` - Remove CSV/Logging/DB sections

### Phase 3: Update Cross-References
1. ⏳ Update `Module_Settings.Receiving/index.md` - Link to Core settings
2. ⏳ Update `Module_Settings.Dunnage/index.md` - Link to Core settings
3. ⏳ Update `.github/copilot-instructions.md` - Document Core settings location

---

## 🔗 Integration Architecture

### Settings Hierarchy
```
Module_Settings.Core (Application-wide)
├── Accessibility & Display
│   ├── Global keyboard shortcuts
│   ├── Global screen reader mode
│   ├── Global high contrast mode
│   ├── Global font sizes
│   └── Grid display defaults
├── CSV Export Paths
│   ├── Base local path
│   ├── Base network path
│   └── Export behaviors
├── Logging & Diagnostics
│   ├── Global logging level
│   ├── SQL query logging
│   └── Module-specific log toggles
├── Performance Tuning
│   ├── Grid virtualization
│   ├── Auto-save debounce
│   └── Async validation
└── Database Maintenance
    ├── Retention policies
    └── Optimization settings

Module_Settings.Receiving (Module-specific)
├── Validation Rules (PO, Part, Quantity)
├── Business Rules (Auto-save, Mode preferences)
├── Default Values (Package type, Location)
├── ERP Integration (Sync settings)
└── Workflow Settings (Step titles, Progress)

Module_Settings.Dunnage (Module-specific)
├── Dunnage Type Management
├── Specification Field Configuration
├── Part Management
├── Inventory List Management
└── Workflow Preferences (Mode defaults, Tab behavior)
```

### Settings Access Pattern
```csharp
// Core settings (accessible to all modules)
var fontsize = await _coreSettings.GetAsync("Core.Display.FontSize");
var csvPath = await _coreSettings.GetAsync("Core.Export.LocalPath");

// Module-specific settings
var requirePO = await _receivingSettings.GetAsync("Receiving.Validation.RequirePoNumber");
var defaultType = await _dunnageSettings.GetAsync("Dunnage.Defaults.DefaultType");
```

---

## 🚨 Critical Decision Required

**Question:** Should we create Module_Settings.Core specifications NOW or defer until needed by a third module?

**Option A: Create Now (Recommended)**
- **Pros:** 
  - Avoid duplication between Receiving and Dunnage
  - Clear separation of concerns
  - Easier to add new modules (inherit Core settings)
  - Consistent UX across modules
- **Cons:** 
  - Additional upfront work
  - Need to refactor existing module specs

**Option B: Defer Until Needed**
- **Pros:** 
  - Less immediate work
  - Can focus on module-specific specs
- **Cons:** 
  - Duplicate settings in Receiving and Dunnage
  - Inconsistent accessibility/display settings
  - Harder to refactor later (3+ modules affected)

---

## ✅ Recommendation

**Create Module_Settings.Core NOW** because:
1. ✅ We already have 2 modules with duplicate settings (CSV export, grid display)
2. ✅ Accessibility settings SHOULD be application-wide, not per-module
3. ✅ Easier to refactor 2 modules now than 5+ modules later
4. ✅ Provides clear guidance for future module development
5. ✅ Improves user experience (consistent settings across modules)

**Estimated Effort:**
- Phase 1 (Create Core specs): 3-4 hours
- Phase 2 (Update module specs): 1-2 hours
- Phase 3 (Update cross-refs): 1 hour
- **Total: 5-7 hours**

---

## 📝 Next Steps

1. **Decide:** Option A (Create Core now) or Option B (Defer)?
2. **If Option A:** Follow Phase 1-3 action plan above
3. **If Option B:** Document decision and revisit when 3rd module is added

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Analysis Complete, Decision Pending  
**Stakeholders:** Development Team, Business Owner

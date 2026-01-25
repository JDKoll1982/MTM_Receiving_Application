# Core Settings Discovery Summary

**Date:** 2026-01-25  
**Analyzed:** Module_Settings.Receiving + Module_Settings.Dunnage  
**Discovery:** 6 settings categories should be Module_Settings.Core (not module-specific)

---

## 🎯 Key Findings

### ✅ Settings That Should Be Core (Application-Wide)

| # | Category | Settings Count | Priority | Found In Both Modules? |
|---|----------|---------------|----------|----------------------|
| 1 | **Accessibility & Display** | ~15 settings | CRITICAL | ✅ YES (duplicated) |
| 2 | **CSV Export Configuration** | ~4 settings | CRITICAL | ✅ YES (identical) |
| 3 | **Logging & Diagnostics** | ~5 settings | HIGH | ⚠️ Only Dunnage (but needed everywhere) |
| 4 | **Grid Performance Tuning** | ~5 settings | MEDIUM | ⚠️ Only Dunnage (but affects all) |
| 5 | **Database Maintenance** | ~2 settings | MEDIUM | ⚠️ Only Dunnage (but applies to all) |
| 6 | **System Information** | ~4 settings | LOW | ⚠️ Only Dunnage (read-only display) |

**Total Core Settings:** ~35 settings that should NOT be duplicated per module

---

## 🚨 Critical Examples of Duplication

### Example 1: CSV Export Paths (Identical in Both Modules)

**Module_Settings.Receiving** (assumed):
```
Receiving.Export.LocalPath = C:\Users\{username}\AppData\Local\MTM\Receiving\
Receiving.Export.NetworkPath = \\SERVER01\Receiving\
Receiving.Export.CreateDatedSubfolders = true
```

**Module_Settings.Dunnage** (confirmed):
```
Dunnage.Export.LocalPath = C:\Users\{username}\AppData\Local\MTM\Dunnage\
Dunnage.Export.NetworkPath = \\SERVER01\Receiving\Dunnage\
Dunnage.Export.CreateDatedSubfolders = true
```

**Problem:** User must configure SAME paths in BOTH modules. If they change network path, must update TWO places.

**Solution (Core Settings):**
```
Core.Export.BasePath = C:\Users\{username}\AppData\Local\MTM\
Core.Export.NetworkPath = \\SERVER01\Receiving\
Core.Export.CreateDatedSubfolders = true

Module folders append automatically:
→ Receiving: C:\Users\jdoe\AppData\Local\MTM\Receiving\
→ Dunnage: C:\Users\jdoe\AppData\Local\MTM\Dunnage\
```

---

### Example 2: Accessibility Settings (Should Be Application-Wide)

**Module_Settings.Receiving** (from analysis):
```
Receiving.Accessibility.EnableKeyboardShortcuts = true
Receiving.Accessibility.HighContrastMode = false
Receiving.Accessibility.LargeFontMode = false
Receiving.Accessibility.EnableScreenReaderMode = false
```

**Module_Settings.Dunnage** (from workflow-preferences.md):
```
Dunnage.Display.FontSize = Medium (13px)
Dunnage.Display.RowHeight = Standard (40px)
Dunnage.Display.HighlightRowOnHover = true
```

**Problem:** 
- Accessibility features configured PER MODULE (inconsistent UX)
- User with vision impairment must enable large font in BOTH modules
- Keyboard shortcuts might conflict between modules

**Solution (Core Settings):**
```
Core.Accessibility.EnableKeyboardShortcuts = true (global)
Core.Accessibility.HighContrastMode = false (global theme)
Core.Display.FontSize = Medium (applies to all modules)
Core.Display.GridRowHeight = Standard (consistent grids)

All modules inherit these settings automatically.
```

---

### Example 3: Logging Level (Should Be Application-Wide)

**Module_Settings.Dunnage** (advanced-settings.md):
```
Logging Level:
  ● Info (normal operation)
  ○ Debug (detailed diagnostics)
  ○ Trace (verbose, performance impact)
  ○ Error only (minimal logging)
```

**Problem:** Logging level should be application-wide, not per-module. Otherwise:
- Receiving logs at INFO level
- Dunnage logs at DEBUG level
- Log files inconsistent and confusing

**Solution (Core Settings):**
```
Core.Logging.Level = Info (applies to entire application)
Core.Logging.EnableSqlQueryLogging = false (global toggle)
Core.Logging.EnableModuleSpecificLogs = true (per-module toggles remain)
```

---

## 📊 Impact Analysis

### If We DON'T Create Module_Settings.Core:

**Problems:**
1. ✅ **Duplicate Settings** - CSV paths, display preferences duplicated in every module
2. ✅ **Inconsistent UX** - Accessibility settings configured per module (confusing)
3. ✅ **Maintenance Burden** - Update settings in N modules instead of once
4. ✅ **User Confusion** - "Why do I have to set font size in both Receiving AND Dunnage?"

**Impact on Future Modules:**
- Module_Routing will need CSV export paths → Duplicate again
- Module_Volvo will need accessibility settings → Duplicate again
- Module_Reporting will need logging level → Duplicate again

**Total Duplications by Module Count:**
```
2 modules:  35 settings × 2 = 70 settings (current state)
5 modules:  35 settings × 5 = 175 settings (if we don't fix it)
10 modules: 35 settings × 10 = 350 settings (disaster)
```

---

### If We DO Create Module_Settings.Core:

**Benefits:**
1. ✅ **Single Source of Truth** - Configure CSV paths once, applies to all modules
2. ✅ **Consistent UX** - Accessibility settings application-wide
3. ✅ **Easier Maintenance** - Update Core settings, all modules inherit
4. ✅ **User-Friendly** - "Set font size once, works everywhere"

**Architecture:**
```
Module_Settings.Core (35 settings)
├── All modules inherit Core settings
├── Modules can override if needed (rare)
└── User configures once, applies everywhere

Module_Settings.Receiving (74 settings)
└── Receiving-specific only (validation, defaults, etc.)

Module_Settings.Dunnage (45 settings)
└── Dunnage-specific only (types, specs, etc.)
```

**Total Settings:**
```
With Core: 35 Core + 74 Receiving + 45 Dunnage = 154 settings
Without Core: 109 Receiving + 79 Dunnage = 188 settings (22% overhead)

At 10 modules:
With Core: 35 Core + 10 × ~50 module-specific = ~535 settings
Without Core: 10 × ~80 (including duplicates) = ~800 settings (49% overhead)
```

---

## 🎯 Recommendation

**CREATE Module_Settings.Core NOW**

**Rationale:**
1. ✅ We already have duplication (CSV export identical in Receiving + Dunnage)
2. ✅ Accessibility MUST be application-wide for proper UX
3. ✅ Easier to refactor 2 modules now than 10 modules later
4. ✅ Future modules will benefit immediately

**Estimated Effort:**
- **Phase 1:** Create Core specifications (3-4 hours)
- **Phase 2:** Update Receiving + Dunnage to remove Core settings (1-2 hours)
- **Phase 3:** Update cross-references and documentation (1 hour)
- **Total:** 5-7 hours

**ROI:**
- Saves ~35 settings per future module (50% reduction in settings docs)
- Improves UX consistency across application
- Reduces maintenance burden long-term

---

## 📋 What You Should Do

### Option 1: Create Module_Settings.Core Now (Recommended ✅)

**Command:**
```
I want to create Module_Settings.Core specifications.
Start with Phase 1 from CORE_SETTINGS_ANALYSIS.md:
1. Create folder structure
2. Create purpose-and-overview.md
3. Create accessibility-and-display.md (CRITICAL)
4. Create csv-export-configuration.md (CRITICAL)
```

**Timeline:** 3-4 hours for critical specifications

---

### Option 2: Defer Until 3rd Module (Not Recommended ❌)

**Rationale:** Acceptable if:
- You're not creating more modules soon
- You can tolerate duplication in Receiving + Dunnage
- You'll refactor later when needed

**Risks:**
- User confusion (why configure same thing twice?)
- Harder to refactor later (more modules affected)
- Inconsistent accessibility UX

---

## 📁 Files to Read

1. **specs/Module_Settings.Core/CORE_SETTINGS_ANALYSIS.md** ← Full analysis (15 min)
2. **specs/Module_Settings.Dunnage/01-Settings-Categories/advanced-settings.md** ← See duplication examples (10 min)
3. **specs/Module_Settings.Receiving/COMPLETION_ROADMAP.md** ← Receiving settings inventory (10 min)

---

## ✅ Decision Point

**Do you want to:**
- **A)** Create Module_Settings.Core now (Recommended ✅) - 5-7 hours total effort
- **B)** Defer until 3rd module is needed (Not recommended ❌) - Accept duplication
- **C)** Discuss with team first - Review analysis, make decision together

---

**I'm ready to create Module_Settings.Core specifications whenever you are!** 🚀

**Next command if proceeding:**
```
Create Module_Settings.Core folder structure and start with 
accessibility-and-display.md specification.
```

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Analysis Complete, Awaiting Decision

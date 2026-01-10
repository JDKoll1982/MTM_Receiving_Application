---
workflow: validate-documentation
command: VD
description: Check docs against codebase, identify drift, and update outdated sections
version: 1.1.0
---

# Validate Documentation Workflow

**Command:** VD (Validate Documentation)
**Purpose:** Detect documentation drift and automatically update outdated sections
**Use Case:** Regular maintenance, after code changes, proactive quality checks

---

## Workflow Execution Steps

### Step 1: Initialize Validation

**Prompt user:**

```
🔍 Docent - Validate Documentation

Module: [await input]
Documentation path: [auto-detect or specify]
Validation scope: [Full / Specific sections]
Auto-update outdated sections? [y/n]
```

**Load existing documentation:**

- Locate `docs/workflows/{ModuleName}.md`
- Parse YAML frontmatter for metadata
- Extract `last_validated` timestamp

---

### Step 2: Check Documentation Age

**Calculate staleness:**

```
Current Date: {CurrentDate}
Last Validated: {last_validated from frontmatter}
Age: {days} days

Status:
✅ Current (< 7 days)
⚠️ Review Recommended (7-14 days)
❌ Stale (> 14 days)
```

**Alert if stale:**

```
⚠️ Documentation is {days} days old.
Recommended validation frequency: Every 14 days.
Proceeding with drift detection...
```

---

### Step 3: Component Count Validation

**Compare documented vs actual component counts:**

**From Documentation (YAML frontmatter):**

```yaml
component_counts:
  views: {documented_count}
  viewmodels: {documented_count}
  services: {documented_count}
  daos: {documented_count}
```

**From Current Codebase (file scan):**

```
Current Component Counts:
- Views: {actual_count}
- ViewModels: {actual_count}
- Services: {actual_count}
- DAOs: {actual_count}
```

**Drift Detection:**

```
Component Drift Analysis:
✅ Views: 3 documented, 3 found (no change)
❌ ViewModels: 5 documented, 6 found (+1 NEW)
❌ Services: 12 documented, 11 found (-1 REMOVED)
✅ DAOs: 4 documented, 4 found (no change)

---

### Step 3b: Self-Sufficiency Section Drift (New)

Validate that the module documentation includes a **Module Self-Sufficiency (Removal Readiness)** section and that it still matches current reality:

1. **Section exists:**
   - Find heading: `Module Self-Sufficiency (Removal Readiness)`
   - If missing: mark as ❌ DRIFT (critical)

2. **Cross-module references check (static):**
   - Re-scan repo (excluding the module folder) for `Module_{ModuleName}` and the module namespace
   - If new references exist not reflected in remediation list: mark as ⚠️ DRIFT

3. **DI/navigation/resource hooks:**
   - Ensure documented blockers include DI registrations and routing/menu entries when present

If auto-update is enabled, update the section with current findings and required remediation steps.
```

---

### Step 4: ViewModel Validation

**For each ViewModel in documentation:**

1. **Check existence:**
   - Does the documented ViewModel still exist?
   - File path: {documented_path}

2. **Property count check:**
   - Documented properties: {count}
   - Actual properties (scan for [ObservableProperty]): {count}
   - Drift: {difference}

3. **Command count check:**
   - Documented commands: {count}
   - Actual commands (scan for [RelayCommand]): {count}
   - Drift: {difference}

**Drift Report:**

```
ViewModel: {ViewModelName}
Status: ❌ DRIFT DETECTED

Properties:
- Documented: {count}
- Actual: {count}
- New: {list new properties}
- Removed: {list removed properties}

Commands:
- Documented: {count}
- Actual: {count}
- New: {list new commands}
- Removed: {list removed commands}
```

---

### Step 5: Service Validation

**For each Service in documentation:**

1. **Check existence:**
   - Service interface exists?
   - Service implementation exists?

2. **Method signature check:**
   - Compare documented method signatures
   - Scan actual service for method names
   - Detect added/removed methods

**Drift Report:**

```
Service: {ServiceName}
Status: ⚠️ CHANGES DETECTED

Methods:
- Documented: {count}
- Actual: {count}
- New Methods: {list}
- Removed Methods: {list}
- Signature Changes: {list}
```

---

### Step 6: DAO Validation

**For each DAO in documentation:**

1. **Check existence:**
   - DAO class still exists?

2. **Stored procedure mapping check:**
   - Documented SP calls: {list}
   - Actual SP calls (scan code): {list}
   - New SPs: {list}
   - Removed SPs: {list}

**Drift Report:**

```
DAO: {DaoName}
Status: ✅ CURRENT / ❌ DRIFT

Stored Procedures:
- Documented: {count}
- Actual: {count}
- New: {list}
- Removed: {list}
```

---

### Step 7: Database Schema Validation

**Query database for current schema:**

**MySQL:**

```sql
-- Check if stored procedures still exist
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name = '{proc_name}';

-- Get current parameter list
SHOW CREATE PROCEDURE {proc_name};
```

**Compare:**

- Documented parameters vs actual parameters
- Data type changes
- New/removed parameters

**Drift Report:**

```
Stored Procedure: {ProcedureName}
Status: ❌ SCHEMA DRIFT

Parameters Changed:
- p_{param}: INT → BIGINT (type changed)
- p_{param}: Removed (no longer exists)
- p_{new_param}: Added (not documented)
```

---

### Step 8: Generate Validation Report

**Compile comprehensive drift analysis:**

```markdown
# Validation Report: {ModuleName}

**Validation Date:** {CurrentDate}
**Last Validated:** {last_validated}
**Documentation Age:** {days} days

---

## Overall Status

| Component Type | Documented | Actual | Status |
|----------------|------------|--------|--------|
| Views | {count} | {count} | {✅/❌} |
| ViewModels | {count} | {count} | {✅/❌} |
| Services | {count} | {count} | {✅/❌} |
| DAOs | {count} | {count} | {✅/❌} |
| Stored Procedures | {count} | {count} | {✅/❌} |

---

## Detailed Drift Analysis

### ❌ Critical Issues ({count})

Issues that require immediate attention:

1. **{ComponentType}: {ComponentName}**
   - **Issue:** {Description}
   - **Impact:** {How this affects accuracy}
   - **Recommendation:** {Update action needed}

### ⚠️ Warnings ({count})

Non-critical changes detected:

1. **{ComponentType}: {ComponentName}**
   - **Change:** {Description}
   - **Impact:** Minor - documentation outdated but functional
   - **Recommendation:** Update when convenient

### ✅ Sections Current ({count})

These sections are accurate:
- {Section names}

---

## New Components Not Documented

**ViewModels:**
- {NewViewModelName} - {InferredPurpose}

**Services:**
- {NewServiceName} - {InferredPurpose}

**Stored Procedures:**
- {NewProcedureName} - {InferredPurpose}

---

## Removed Components Still Documented

**ViewModels:**
- {RemovedViewModelName} - No longer exists in codebase

**Services:**
- {RemovedServiceName} - Implementation removed

---

## Schema Changes Detected

**Stored Procedures Modified:**
- {ProcedureName}: Parameters changed
- {ProcedureName}: Logic updated (requires re-documentation)

**Tables Modified:**
- {TableName}: New columns added
- {TableName}: Index changes

---

## Recommended Actions

**Priority 1 - Immediate:**
1. Update {Section} - {count} new components
2. Remove obsolete {Section} - {count} removed components
3. Refresh {Section} - schema changes detected

**Priority 2 - Soon:**
1. Review {Section} - minor changes
2. Expand {Section} - new patterns discovered

**Priority 3 - Optional:**
1. Enhance {Section} - improve clarity
```

---

### Step 9: Auto-Update Sections (if user approved)

**If auto-update = Yes:**

**For each section with drift:**

1. **Re-analyze component:**
   - Run targeted analysis (like UV command for ViewModels)
   - Generate fresh documentation for that section

2. **Replace outdated section:**
   - Locate section in documentation file
   - Replace with updated content
   - Preserve section structure

3. **Update metadata:**
   - Increment component counts in frontmatter
   - Update `last_validated` timestamp
   - Add validation note

**Update Log:**

```
Updating documentation sections...

✓ Section: ViewModels - Updated {count} entries
✓ Section: Services - Added {count} new methods
✓ Section: DAOs - Removed {count} obsolete entries
✓ Section: Database Schema - Refreshed {count} procedures

Frontmatter updated:
- component_counts: Updated
- last_validated: {CurrentDate}
```

---

### Step 10: Manual Review Prompts (if auto-update = No)

**Present each drift item for user decision:**

```
Drift Item 1 of {total}:

Component: {ComponentName}
Type: {ViewModel/Service/DAO}
Issue: {Description}
Documentation Section: {SectionName}

Actions:
[U] Update this section now
[S] Skip (leave as-is)
[N] Next (decide later)
[A] Auto-update remaining

Your choice: [U/S/N/A]
```

**Process user choice:**

- U: Update immediately
- S: Skip, note in report
- N: Move to next item
- A: Auto-update rest

---

### Step 11: Update Memory

**Update memories.md:**

```markdown
| {ModuleName} | docs/workflows/{ModuleName}.md | {last_analyzed} | {CurrentDate} | ✅ Current | Validated - {drift_count} items updated |
```

**Update Validation Tracking section:**

```markdown
### Modules Pending Validation (Age > 14 days)

{Update list - remove validated modules}
```

---

### Step 12: Completion Report

**Summary message:**

```
✅ Validation complete with updates applied.

Module: {ModuleName}
Validation Date: {CurrentDate}
Documentation Age: {days} days → 0 days (refreshed)

Drift Analysis:
- Critical Issues: {count} (all resolved)
- Warnings: {count} (all addressed)
- Sections Updated: {count}
- New Components Added: {count}
- Obsolete Entries Removed: {count}

Documentation: {FilePath}
Status: ✅ Current and accurate

Validation report: {ReportPath}

Documentation integrity verified. All drifts corrected.
```

---

### Step 13: Schedule Next Validation

**Calculate next validation date:**

```
Current Date: {CurrentDate}
Recommended Frequency: 14 days
Next Validation: {CurrentDate + 14 days}

I will proactively remind you on {NextDate} when this documentation needs validation again.

(This tracking is maintained in memories.md)
```

---

**Workflow Version:** 1.0.0  
**Created:** 2026-01-08  
**Status:** Production Ready

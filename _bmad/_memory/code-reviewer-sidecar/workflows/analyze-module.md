# Workflow: Analyze Module

**Purpose:** Comprehensive code analysis to generate CODE_REVIEW.md with categorized issues

---

## Prerequisites

- Module name specified (e.g., "Module_Volvo")
- Module exists in workspace
- Agent has read access to module folder

---

## Execution Steps

### 1. Module Discovery

**Scan module structure:**

```
Module_{Name}/
├── Services/*.cs
├── Data/*.cs
├── ViewModels/*.cs
├── Views/*.xaml + *.xaml.cs
├── Models/*.cs
├── Interfaces/*.cs
├── Enums/*.cs
└── [Dialogs/Windows]
```

**Count files per category and report to user:**
"Found in Module_{Name}:

- X Services
- Y DAOs
- Z ViewModels
- N Views
- M Models"

---

### 2. Critical Issues Analysis (🔴 CRITICAL)

**Scan for:**

**SQL Injection (in DAOs):**

- Raw SQL with string concatenation
- CommandText with non-parameterized values
- Missing stored procedure usage for MySQL operations

**Path Injection:**

- File paths constructed from user input
- Missing Path.GetInvalidFileNameChars() validation
- No sanitization before Path.Combine()

**Transaction Management:**

- Multi-insert operations without MySqlTransaction
- No rollback logic on failures
- Shipment + lines without atomic writes

**Record each finding:**

```
Issue #: {auto-increment}
Severity: 🔴 CRITICAL
File: {relative path}
Method: {method name}
Lines: {start}-{end}
Description: {what's wrong}
Recommended Fix: {specific action}
```

---

### 3. Security Issues Analysis (🟡 SECURITY)

**Scan for:**

**Input Validation:**

- ReceivedSkidCount without range check (1-99)
- User input not validated before database ops
- Missing null/empty checks on required fields

**Hardcoded Values:**

- Employee numbers = empty string or hardcoded
- API keys, passwords, connection strings (warn if found)
- Look for pattern: `= "some_constant"` or `= 123`

**Authorization:**

- Service methods with no role checks
- Operations without user permission validation
- Missing audit trail (who did what)

---

### 4. Data Integrity Issues (🟠 DATA)

**Scan for:**

**Race Conditions:**

- "Only one pending allowed" checks without DB constraint
- Concurrent access to shared resources
- Missing optimistic concurrency

**Cascade Delete Protection:**

- Deactivate operations without reference checks
- Foreign key relationships not validated
- Orphaned records possible

**Duplicate Prevention:**

- AddPart without checking Parts.Any()
- No unique constraints mentioned
- Duplicate entries possible

---

### 5. Quality Issues (🔵 QUALITY)

**Scan for:**

**Magic Strings:**

- Status values like "pending_po", "completed"
- Repeated string literals
- No constants class

**Dead Code:**

- Unused methods (search for references)
- Commented-out code blocks
- FilterParts-style orphaned methods

**Code Duplication:**

- Repeated Clear() logic
- Similar validation in multiple places
- Extract to helper method candidates

**Null Checks:**

- Optional parameters without default values
- Dictionary/List access without null check
- Missing `?.` or `??` operators

---

### 6. Performance Issues (🟣 PERFORMANCE)

**Scan for:**

**N+1 Queries:**

- Loops with await _dao.GetByIdAsync()
- Individual queries instead of batch
- GetComponentsByParentAsync in loop

**Inefficient Collections:**

- ObservableCollection.Clear() + foreach Add
- LINQ Where().Count() instead of Any()
- Take(20) hardcoded without filtering first

---

### 7. Maintainability Issues (🔧 MAINTAIN)

**Scan for:**

**Missing Documentation:**

- Public methods without /// <summary>
- Complex algorithms without comments
- UpdatePartSuggestions-style methods

**Complex Validation:**

- ValidateShipment() in ViewModel (should be in Service)
- Business logic in wrong layer
- God methods > 100 lines

**Inconsistent Naming:**

- Model_Dao_Result.Success vs .IsSuccess
- Mixed conventions across files

---

### 8. Edge Cases (🟢 EDGE CASE)

**Scan for:**

**Zero/Negative Values:**

- QuantityPerSkid <= 0
- ReceivedSkidCount = 0
- Division without zero check

**Large Data:**

- CSV generation without row limit
- File operations without size check
- Memory allocation without bounds

**Missing Properties:**

- QuantityPerSkid not stored in line
- Calculated fields not persisted

---

### 9. UI Design Issues (🎨 UI DESIGN)

**Scan for:**

**Workflow Improvements:**

- Multi-step processes that could be streamlined
- Redundant user actions
- Missing shortcuts or quick actions

**Form Usability:**

- Long forms without sections
- Poor tab order
- Missing default values

---

### 10. UX Issues (👤 UX)

**Scan for:**

**User Feedback:**

- Long operations without progress indicators
- Error messages too technical
- No success confirmations

**Navigation:**

- Hard to find features
- Unclear next steps
- Missing breadcrumbs

---

### 11. Logging/Documentation (🟤 LOGGING/DOCS)

**Scan for:**

**Missing Logging:**

- Catch blocks without logger calls
- User actions not logged
- No audit trail

**Exception Details:**

- HandleErrorAsync without prior LogErrorAsync
- Generic error messages
- Stack traces not logged

---

### 12. Settings Discovery

**Hardcoded Values to Extract:**

Scan for patterns and record:

```
Setting Category: {File System Paths|Validation Rules|etc}
Setting Name: {MaxCsvLines}
Current Value: {10000}
Location: {Service_Volvo.cs:215}
Type: {Integer}
Recommended Range: {100-100000}
```

**Track:**

- File paths (Path.Combine patterns)
- Numeric constants (> 10)
- String literals in validation
- Email templates
- UI text (labels, placeholders)

---

### 13. Service Documentation Check

**For each Service class:**

1. Check if `.github/instructions/service-{name}.instructions.md` exists
2. If exists:
   - Compare service methods with documented methods
   - Flag if new methods added
   - Flag if method signatures changed
3. If not exists:
   - Mark for documentation generation

---

### 14. Generate CODE_REVIEW.md

**Use template:** `.github/templates/code-review/CODE_REVIEW.template.md`

**Structure:**

```markdown
# {Module} - Code Review Report

**Date:** {today}
**Reviewer:** Code Review Sentinel
**Scope:** All code in {Module}

---

## Implementation Plan Summary

[Tables for files to modify, methods to add/remove, database changes]

---

## Issue Location Reference

| ✓ | # | Issue | Severity | File | Method | Lines | Recommended Fix |
|---|---|-------|----------|------|--------|-------|-----------------|
| ⬜ | 1 | ... | 🔴 CRITICAL | ... | ... | ... | ... |
...
```

**Save to:** `{Module}/CODE_REVIEW.md`

---

### 15. Generate Settings Documentation

**Use template:** `.github/templates/code-review/ModuleSettings.template.md`

**Populate with discovered hardcoded values**

**Save to:** `Documentation/FutureEnhancements/Module_Settings/{Module}Settings.md`

---

### 16. Update Memories

**Record in memories.md:**

- Module analyzed
- CODE_REVIEW version (V1)
- Total issues count
- Issues by severity
- Timestamp
- Hardcoded values found
- Services needing documentation

---

### 17. Present Results to User

**Report:**

```
✅ Analysis Complete for {Module}

Generated:
- CODE_REVIEW.md ({count} issues found)
- {Module}Settings.md ({count} settings discovered)

Severity Breakdown:
🔴 CRITICAL: {count}
🟡 SECURITY: {count}
🟠 DATA: {count}
🔵 QUALITY: {count}
🟣 PERFORMANCE: {count}
🔧 MAINTAIN: {count}
🟢 EDGE CASE: {count}
🎨 UI DESIGN: {count}
👤 UX: {count}
🟤 LOGGING/DOCS: {count}

Next Steps:
1. Review CODE_REVIEW.md
2. Amend checkboxes (✅ for "do fix", ⬜ for "skip")
3. Run [F]ix command to apply checked fixes

Services needing documentation: {count}
Run [D]ocs command to generate service instruction files.
```

---

## Error Handling

**If module not found:**

- List available Module_* folders
- Ask user to confirm module name

**If module empty:**

- Report "No analyzable code found"
- Ask if user wants to analyze anyway (Views/Models only)

**If analysis fails:**

- Log error to memories.md
- Report specific failure to user
- Offer to retry or skip problematic file

---

**End of Workflow**

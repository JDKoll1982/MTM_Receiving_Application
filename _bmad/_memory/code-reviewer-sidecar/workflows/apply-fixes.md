# Workflow: Apply Fixes

**Purpose:** Execute checked fixes from CODE_REVIEW.md with build validation

---

## Prerequisites

- CODE_REVIEW_V{#}.md exists in module folder
- User has amended checkboxes (✅ = fix, ⬜ = skip)
- Module is buildable before starting

---

## Execution Steps

### 1. Load and Parse CODE_REVIEW.md

**Read file:** `{Module}/CODE_REVIEW_V{#}.md`

**Extract:**
- All rows with ✅ checkbox
- Issue number, severity, file, method, lines, fix description
- Count total fixes to apply

**Report to user:**
```
Loaded CODE_REVIEW_V{#}.md
Found {count} fixes to apply:
🔴 CRITICAL: {count}
🟡 SECURITY: {count}
... (all severities)

Estimated time: ~{count * 3} minutes
Proceed with fixes? [y/n]
```

**Wait for user confirmation**

---

### 2. Dependency Detection & Fix Ordering

**Analyze fix dependencies:**

```javascript
Dependencies detected:
- Issue #2 (stored proc) → Issue #1 (DAO update)
- Issue #11 (constants class) → Issue #5 (service usage)
- Issue #20 (validation method) → Issue #13 (ViewModel call)
```

**Build fix graph:**
1. Group fixes by file (reduces context switching)
2. Within file, order by dependencies
3. Critical fixes first (unless dependency blocks)
4. Smart grouping overrides strict severity order

**Present execution plan:**
```
Fix Execution Order:
Phase 1: Database Foundation
  - #2: Create sp_volvo_shipment_update.sql
  - #1: Update Dao_VolvoShipment to use stored proc

Phase 2: Constants & Validation
  - #11: Create VolvoShipmentStatus constants
  - #20: Add ValidateShipmentAsync to Service_Volvo

Phase 3: ViewModel Updates
  - #4: Add skid count validation
  - #5: Get employee number from session
  ...

Continue? [y/n]
```

---

### 3. Apply Fixes with Build Validation

**For each fix in execution order:**

#### Step 3.1: Announce Fix

```
[{current}/{total}] Applying fix #{issue_num}: {title}
Severity: {emoji} {name}
File: {path}
```

#### Step 3.2: Read Current File State

- Load entire file
- Locate exact section to modify
- Verify line numbers match expectations

#### Step 3.3: Apply Fix

**Based on fix type:**

**For code changes:**
- Use `replace_string_in_file` or `multi_replace_string_in_file`
- Include 3-5 lines context before/after
- Preserve whitespace and indentation

**For new files (stored procs, constants, migrations):**
- Use `create_file`
- Follow naming conventions
- Validate SQL syntax for database files

**For deletions:**
- Use `replace_string_in_file` to remove method
- Ensure no orphaned references

#### Step 3.4: Build & Validate

```powershell
dotnet build MTM_Receiving_Application.csproj
```

**Parse build output:**

**If SUCCESS:**
```
✅ Build succeeded (X.X seconds)
Fix #{issue_num} applied successfully
```

**If FAILED:**
```
❌ Build failed with {count} errors

Error: {error_message}
File: {file}
Line: {line}

Analyzing error...
```

#### Step 3.5: Handle Build Errors

**Error analysis:**
1. Read error message carefully
2. Identify error type:
   - Missing using statement
   - Syntax error
   - Undefined variable
   - Type mismatch
   - Logic error

**Auto-fix common errors:**

**Missing using:**
```csharp
// Error: "The type or namespace name 'MySqlConnection' could not be found"
// Fix: Add "using MySql.Data.MySqlClient;"
```

**Orphaned code blocks:**
```csharp
// Error: "Invalid token '}' in a member declaration"
// Fix: Remove orphaned } or add missing {
```

**Null reference:**
```csharp
// Error: "Use of possibly unassigned field '_variable'"
// Fix: Initialize variable or add null check
```

**Type mismatch:**
```csharp
// Error: "Cannot convert int to string"
// Fix: Add .ToString() or correct type
```

**Apply error fix:**
- Make correction
- Rebuild immediately
- If still fails, try different approach
- If 3 attempts fail, ask user for guidance

#### Step 3.6: Update Checkbox

**Once build succeeds:**
- Update CODE_REVIEW.md
- Change ⬜ → ✅ for issue #{num}
- Save file

---

### 4. Handle Special Fix Types

#### 4a. Stored Procedure Creation

**Path:** `Database/StoredProcedures/{Module}/sp_{name}.sql`

**Validate:**
- Check if proc already exists (grep search)
- Confirm parameters match DAO call
- Test DELIMITER syntax

**Template:**
```sql
DELIMITER $$

CREATE PROCEDURE sp_{module}_{operation}(
    IN p_param1 TYPE,
    IN p_param2 TYPE
)
BEGIN
    -- Procedure logic
    
    SELECT ROW_COUNT() AS affected_rows;
END$$

DELIMITER ;
```

#### 4b. Database Migration

**Path:** `Database/Migrations/{###}_{description}.sql`

**Auto-increment migration number:**
- List existing migrations
- Find highest number
- Increment by 1 (pad to 3 digits)

**Template:**
```sql
-- Migration: {###}_{description}
-- Created: {date}
-- Module: {module}

-- Add constraint/index/column
ALTER TABLE {table_name}
ADD CONSTRAINT {constraint_name} ...;

-- Rollback (commented)
-- ALTER TABLE {table_name} DROP CONSTRAINT {constraint_name};
```

#### 4c. Constants Class Creation

**Path:** `{Module}/Models/{Name}Status.cs`

**Template:**
```csharp
namespace MTM_Receiving_Application.{Module}.Models;

/// <summary>
/// Status constants for {entity} lifecycle
/// </summary>
public static class {Entity}Status
{
    public const string PendingPo = "pending_po";
    public const string Completed = "completed";
    // ...
}
```

#### 4d. Service Method Addition

**Validation:**
- Check if method already exists in interface
- Update interface first, then implementation
- Ensure XML docs match

**Order:**
1. Add to `IService_{Name}.cs`
2. Implement in `Service_{Name}.cs`
3. Build
4. Update calling code (ViewModel/other)

---

### 5. Progress Tracking

**After each fix:**

Update memories.md:
```
Last Fix Applied: Issue #{num} - {title}
Timestamp: {datetime}
Fixes Completed: {completed}/{total}
Build Status: Success/Failed
```

**Every 5 fixes, report progress:**
```
Progress Update:
{completed}/{total} fixes applied ({percent}%)
Estimated remaining time: ~{remaining * 3} minutes
Build: ✅ Passing
Next: Issue #{next_num} - {title}
```

---

### 6. Handle User Flags

**--skip-{type}:**
```
Flag detected: --skip-maintain
Skipping all 🔧 MAINTAIN issues
Remaining: {count} fixes
```

**--only-{type}:**
```
Flag detected: --only-critical
Applying only 🔴 CRITICAL issues
Count: {count} fixes
```

---

### 7. Completion Summary

**When all fixes applied:**

```
🎉 Fix Application Complete!

Applied: {count} fixes
Build: ✅ Passing
Time: {duration}

Severity Breakdown:
🔴 CRITICAL: {applied}/{total}
🟡 SECURITY: {applied}/{total}
... (all severities)

Skipped Issues: {count}
{List skipped issue numbers and reasons}

Updated Files:
- {file1}
- {file2}
...

Created Files:
- {file1}
- {file2}
...

Next Steps:
1. Review changes in version control
2. Test affected functionality
3. Run [V]archive command if review complete
4. Or re-run CODE_REVIEW to verify no new issues

Remaining Unchecked (⬜): {count}
{If > 0, list issue numbers and titles}
```

**Update CODE_REVIEW.md:**
- All ✅ issues have checkmarks
- All ⬜ issues remain unchecked
- Save file

**Update memories.md:**
```
Module: {name}
CODE_REVIEW Version: V{#}
Status: {completed}/{total} fixes applied
Last Session: {datetime}
Build Status: Passing
Ready for Archive: {yes/no}
```

---

## Error Recovery

**If critical error during fixes:**

1. **Stop immediately**
2. **Document state in memories.md:**
   ```
   ERROR STATE
   Last successful fix: Issue #{num}
   Failed on: Issue #{num}
   Error: {message}
   Files modified: {list}
   Build status: {status}
   ```

3. **Report to user:**
   ```
   ⚠️ Critical error encountered
   
   Last successful: Issue #{num}
   Failed on: Issue #{num}
   Error: {message}
   
   Recommend:
   1. Review file: {path}
   2. Check git diff
   3. Manual intervention may be required
   
   Fixes applied: {count}/{total}
   Safe to revert: {yes/no}
   ```

4. **Offer options:**
   - Continue with remaining fixes?
   - Revert this fix and continue?
   - Stop and allow manual fix?

---

## Build Failure Retry Logic

**Attempt limit:** 3 tries per fix

**Strategy per attempt:**
1. **First attempt:** Standard fix application
2. **Second attempt:** Alternative approach (if known)
3. **Third attempt:** Ask user for guidance

**If all attempts fail:**
- Mark fix as ❌ in memories.md
- Continue with remaining fixes
- Report failed fix in summary
- Suggest manual intervention

---

**End of Workflow**

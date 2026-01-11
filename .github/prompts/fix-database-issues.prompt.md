# Fix Database Issues - Comprehensive Remediation Plan

## Objective

Systematically fix all stored procedure, DAO, and schema issues identified in the test report while avoiding circular dependencies and ensuring each fix doesn't create new problems.

## Phase 1: Analysis & Dependency Mapping

Before making any changes, perform a thorough analysis:

### Step 1.1: Load and Parse the Report

Read #file:Database/00-Test/01-report.md and extract:

- All 31 stored procedures with OUT parameter issues
- All 15 DAO classes requiring updates
- All 5 database schema issues

### Step 1.2: Build Dependency Graph

For each issue, determine:

**Schema Issues** (Fix FIRST - these block everything else):

- Which SPs are blocked by missing columns/tables?
- What tables/columns need to be created/modified?
- Are there any circular schema dependencies?

**Stored Procedure Issues**:

- Which DAOs call this SP?
- Does this SP depend on any schema changes?
- Are there any SP→SP dependencies (one SP calling another)?

**DAO Issues**:

- Which SPs does this DAO wrap?
- Are all required SPs already fixed?
- Which Services/ViewModels call this DAO?

### Step 1.3: Determine Fix Order

Generate a fix plan following this priority:

1. **Critical Schema Fixes** (Priority: Critical)
   - Fix missing columns/tables that block multiple SPs
   - Validate schema changes don't break existing functionality

2. **High-Priority OUT Parameter SPs** (Priority: High)
   - SPs with 4+ OUT parameters
   - SPs that have no dependent DAOs (can be tested independently)

3. **Corresponding DAOs** (After their SPs are fixed)
   - Update DAOs to handle OUT parameters
   - Test DAO methods with updated SPs

4. **Medium-Priority SPs and DAOs** (Priority: Medium)
   - SPs with 2-3 OUT parameters
   - DAOs with fewer parameter mismatches

5. **Low-Priority Items** (Priority: Low)
   - SPs with minimal OUT parameters
   - DAOs affecting single SPs

### Step 1.4: Output Analysis

Before proceeding, provide:

```markdown
## Dependency Analysis Complete

### Fix Order Summary
1. Schema fixes: [list 5 critical issues]
2. Standalone SPs: [list SPs with no DAO dependencies]
3. SP→DAO pairs: [list grouped pairs that must be fixed together]
4. Complex chains: [list any SP→DAO→Service dependencies]

### Potential Conflicts Identified
- [List any circular dependencies]
- [List any blocking issues]

Proceed with fixes? (yes/no)
```

## Phase 2: Schema Fixes (Critical Priority)

For each schema issue in the Database Schema Issues table:

### Step 2.1: Investigate the Issue

1. Read the stored procedure file: `Database/StoredProcedures/{SP_SubDirectory}/{SP_Name}.sql`
2. Identify the exact missing column/table referenced
3. Determine if it's a:
   - Typo in the SP (fix SP code)
   - Missing column (ALTER TABLE)
   - Missing table (CREATE TABLE)
   - Wrong table reference (fix SP code)

### Step 2.2: Determine the Fix

For missing columns:

```sql
-- Example for sp_routing_recipient_get_all (missing 'default_department')
ALTER TABLE routing_recipients
ADD COLUMN default_department VARCHAR(100) NULL
COMMENT 'Default department for routing recipient';
```

For wrong references:

- Update the SP SQL to reference the correct column/table

### Step 2.3: Apply the Fix

1. Create migration script: `Database/Migrations/YYYYMMDD_HHMM_fix_{issue}.sql`
2. Test the migration on local DB
3. Re-run the affected SP test
4. Mark checkbox as `[x]` in #file:Database/00-Test/01-report.md

### Step 2.4: Verify

Run: `.\Database\00-Test\02-Test-StoredProcedures.ps1 -StoredProcedure {SP_Name}`

Expected: No more schema errors for this SP.

## Phase 3: Stored Procedure OUT Parameter Fixes

For each SP in the "Stored Procedures Requiring Updates" table:

### Step 3.1: Understand Current vs Expected

Example for `receiving_line_Insert`:

- Current: 10 IN parameters
- Expected: 12 total (10 IN + 2 OUT)
- Missing: 2 OUT parameters

### Step 3.2: Read the Stored Procedure

```bash
# Read the actual SP definition
Get-Content Database/StoredProcedures/{SP_SubDirectory}/{SP_Name}.sql
```

Identify:

- What are the OUT parameter names?
- What do they return (IDs, success flags, error messages)?
- Example: `OUT p_new_id INT`, `OUT p_success TINYINT`

### Step 3.3: Check if DAO Exists

Search for the DAO class listed in the table:

```csharp
// Search pattern
**/Data/**/Dao_{EntityName}.cs
```

If DAO doesn't exist yet, make a note to create it after fixing the SP.

### Step 3.4: Update Mock Data (if needed)

Edit #file:Database/00-Test/01-mock-data.json:

- Ensure IN parameters have appropriate test values
- Note: OUT parameters don't need mock values (they're outputs)

### Step 3.5: Test the SP Directly

Run the test to verify parameter count is now correct:

```powershell
.\Database\00-Test\02-Test-StoredProcedures.ps1 -StoredProcedure {SP_Name}
```

### Step 3.6: Mark Complete

Update #file:Database/00-Test/01-report.md:

- Change `| [ ] | {SP_Name}` to `| [x] | {SP_Name}`

## Phase 4: DAO Updates

For each DAO in the "DAO Files Requiring Updates" table:

### Step 4.1: Locate the DAO File

Use the "File Path" column or search:

```powershell
Get-ChildItem -Path . -Recurse -Filter "Dao_*.cs" | Where-Object { $_.Name -match "{DaoName}" }
```

Common locations:

- `Module_Core/Data/`
- `Module_Receiving/Data/`
- `Module_Routing/Data/`
- `Module_Settings/Data/`
- `Module_Dunnage/Data/`
- `Module_Volvo/Data/`

### Step 4.2: Review Related SPs

From the "Related SPs" column, identify which methods need updating.

Example for `Dao_Routing Label`:

- Related SPs: `sp_routing_label_insert`, `sp_routing_label_update`, `sp_routing_label_delete`, etc.
- Methods to update: `Insert*Async`, `Update*Async`, `Delete*Async`

### Step 4.3: Update Method Signatures

**Before:**

```csharp
public async Task<Model_Dao_Result> InsertRoutingLabelAsync(Model_RoutingLabel label)
{
    var parameters = new MySqlParameter[]
    {
        new MySqlParameter("@p_part_number", label.PartNumber),
        new MySqlParameter("@p_quantity", label.Quantity),
        // ... 8 IN parameters
    };

    return await Helper_Database_StoredProcedure.ExecuteAsync(
        "sp_routing_label_insert",
        parameters,
        _connectionString
    );
}
```

**After (with OUT parameters):**

```csharp
public async Task<Model_Dao_Result<int>> InsertRoutingLabelAsync(Model_RoutingLabel label)
{
    var parameters = new MySqlParameter[]
    {
        new MySqlParameter("@p_part_number", label.PartNumber),
        new MySqlParameter("@p_quantity", label.Quantity),
        // ... 8 IN parameters

        // Add OUT parameters
        new MySqlParameter("@p_new_label_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        },
        new MySqlParameter("@p_success", MySqlDbType.Byte)
        {
            Direction = ParameterDirection.Output
        }
    };

    var result = await Helper_Database_StoredProcedure.ExecuteAsync(
        "sp_routing_label_insert",
        parameters,
        _connectionString
    );

    if (result.Success)
    {
        var newId = parameters.FirstOrDefault(p => p.ParameterName == "@p_new_label_id")?.Value as int?;
        return Model_Dao_Result<int>.Success(newId ?? 0, result.Message);
    }

    return Model_Dao_Result<int>.Failure(result.ErrorMessage);
}
```

### Step 4.4: Update Method Callers

Search for all usages of the updated DAO method:

```csharp
// Find all calls to this method
Dao_RoutingLabel.*InsertRoutingLabelAsync
```

Update callers to handle the new return type:

```csharp
// Before
var result = await _routingLabelDao.InsertRoutingLabelAsync(label);

// After
var result = await _routingLabelDao.InsertRoutingLabelAsync(label);
if (result.Success)
{
    int newLabelId = result.Data;
    // Use the new ID...
}
```

### Step 4.5: Update Unit Tests

Find test file: `Tests/Unit/Data/**/Dao_{EntityName}_Tests.cs`

Update test assertions:

```csharp
[Fact]
public async Task InsertRoutingLabelAsync_ReturnsNewId()
{
    // Arrange
    var label = new Model_RoutingLabel { /* test data */ };

    // Act
    var result = await _dao.InsertRoutingLabelAsync(label);

    // Assert
    result.Success.Should().BeTrue();
    result.Data.Should().BeGreaterThan(0); // Check new ID
}
```

### Step 4.6: Mark Complete

Update #file:Database/00-Test/01-report.md:

- Change `| [ ] | {DAO_Class}` to `| [x] | {DAO_Class}`

## Phase 5: Integration Testing

After all fixes are applied:

### Step 5.1: Run Full SP Test Suite

```powershell
.\Database\00-Test\02-Test-StoredProcedures.ps1 -UseExecutionOrder
```

Verify:

- Parameter mismatch count = 0
- Schema broken count = 0
- Success rate > 90%

### Step 5.2: Run DAO Unit Tests

```powershell
dotnet test --filter "FullyQualifiedName~Data"
```

### Step 5.3: Run Integration Tests

```powershell
dotnet test --filter "FullyQualifiedName~Integration"
```

### Step 5.4: Generate Updated Report

```powershell
.\Database\00-Test\01-Generate-SP-TestData.ps1
```

Verify all checkboxes are marked `[x]` in the fix checklists.

## Phase 6: Documentation & Cleanup

### Step 6.1: Update CHANGELOG

Add entry:

```markdown
## [Version] - YYYY-MM-DD

### Fixed
- Fixed 31 stored procedures with OUT parameter handling
- Updated 15 DAO classes to properly handle OUT/INOUT parameters
- Resolved 5 critical database schema issues
- Improved test coverage for database layer
```

### Step 6.2: Update Architecture Docs

If new patterns were established:

- Document OUT parameter handling pattern
- Update DAO examples in `docs/architecture.md`

### Step 6.3: Commit Changes

```bash
git add Database/StoredProcedures/*/*.sql`
git add Module_*/Data/Dao_*.cs
git add Database/Schemas/*.sql
git add Database/Migrations/*.sql
git add Database/00-Test/01-report.md
git commit -m "fix: resolve stored procedure OUT parameter and schema issues

- Fixed 5 critical schema issues (missing columns/tables)
- Updated 31 stored procedures with proper OUT parameter handling
- Updated 15 DAO classes to capture and return OUT parameter values
- All database tests now passing with 100% SP parameter accuracy

Fixes #[issue-number]"
```

## Special Cases & Troubleshooting

### If DAO Doesn't Exist

Some SPs in the fix list may not have corresponding DAOs yet:

1. Check if the SP is actually used in the codebase
2. If yes, create a new DAO following the instance-based pattern
3. If no, mark as "Not Needed" in the checklist

### If Circular Dependencies Found

Example: DAO_A → SP_B → DAO_C → SP_A

Resolution:

1. Identify the cycle
2. Break the cycle by fixing the "leaf" dependency first
3. Work backwards through the chain

### If Schema Fix Breaks Existing Code

1. Revert the schema change
2. Check for existing code that relies on the old structure
3. Create a migration plan:
   - Add new column with different name
   - Update code to use new column
   - Migrate data
   - Drop old column

### If OUT Parameter Names Unknown

The SP file may not clearly document OUT parameters:

```sql
-- Look for patterns like:
SELECT @p_result = 1;
SET @out_value = LAST_INSERT_ID();
```

Common OUT parameter patterns:

- `@p_new_id` / `@out_id` - Newly inserted ID
- `@p_success` / `@out_success` - Success flag (0/1)
- `@p_error_code` / `@out_error` - Error code/message
- `@p_affected_rows` - Row count

## Success Criteria

✅ All checkboxes in #file:Database/00-Test/01-report.md marked `[x]`
✅ `02-Test-StoredProcedures.ps1` shows 0 parameter mismatches
✅ `02-Test-StoredProcedures.ps1` shows 0 schema broken errors
✅ All DAO unit tests passing
✅ All integration tests passing
✅ No compiler errors or warnings
✅ Documentation updated

## Execution Command

To use this prompt:

1. Open GitHub Copilot Chat
2. Reference this file: `@workspace /fix using #file:.github/prompts/fix-database-issues.prompt.md`
3. Follow the phase-by-phase guidance
4. Mark checkboxes in the report as you complete each item

---

**Estimated Time:** 4-8 hours depending on complexity
**Recommended Approach:** Work in small batches (5-10 items), test frequently, commit often
**Team Coordination:** If multiple developers, assign by module to avoid conflicts

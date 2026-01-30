# Guardrails - Module_Receiving

**Last Updated: 2025-01-15**

Critical safety checks and boundaries for Module_Receiving. These are hard limits that protect data integrity, system stability, and business operations.

---

## Database Safety Guardrails

### SQL Server (Infor Visual) - READ ONLY

**Rule**: NEVER write to SQL Server  
**Enforcement**: Connection string includes `ApplicationIntent=ReadOnly`

**Forbidden operations**:
```csharp
// ❌ NEVER DO THIS
await ExecuteAsync("UPDATE VISUAL.dbo.Part SET ...");
await ExecuteAsync("INSERT INTO VISUAL.dbo.PurchaseOrder ...");
await ExecuteAsync("DELETE FROM VISUAL.dbo.POLine ...");
```

**Why**: SQL Server is production ERP system. Writes could corrupt financial data, inventory, or orders.

**What to do instead**: MySQL is the receiving system of record. Read from SQL Server, write to MySQL only.

**Validation**:
- Code review must catch any SQL Server writes
- Connection string validation on startup
- Runtime check: connection string contains `ApplicationIntent=ReadOnly`

---

### MySQL Stored Procedures Only

**Rule**: No raw SQL queries in C# code  
**Enforcement**: Code review, architectural guideline

**Allowed**:
```csharp
// ✅ CORRECT
await Helper_Database_StoredProcedure.ExecuteAsync(
    "sp_Receiving_Line_Insert",
    parameters,
    connectionString
);
```

**Forbidden**:
```csharp
// ❌ NEVER DO THIS
string sql = "INSERT INTO receiving_line (PONumber, Quantity) VALUES (@po, @qty)";
await connection.ExecuteAsync(sql, parameters);
```

**Why**:
- SQL injection protection
- Centralized logic in database
- Easier to audit and maintain
- Performance tuning at database layer

**Validation**:
- Search codebase for `INSERT`, `UPDATE`, `DELETE` strings in C# files
- If found, must be in stored procedure call only

---

## Data Integrity Guardrails

### Primary Key Preservation

**Rule**: Never modify or reassign primary keys after creation  
**Affected tables**: `receiving_load`, `receiving_line`

**Why**: Other systems (Routing, Reporting) reference these IDs. Changing them breaks relationships.

**If you must delete**: Use soft delete flags instead of actual deletion where possible.

---

### PO Validation is Mandatory

**Rule**: Every receive must validate PO against ERP before saving  
**Enforcement**: Workflow service enforces this step

**Cannot bypass**:
- Direct database inserts without PO validation
- Saving without ERP confirmation
- Using fake or test PO numbers in production

**Exception**: Test/dev environments may use mock PO validation

**Validation**:
- Ensure `Service_ReceivingWorkflow.ValidatePOAsync()` is always called
- Database trigger could enforce PO existence (future enhancement)

---

### Quantity Must Be Positive

**Rule**: Quantities and weights must be > 0  
**Enforcement**: Validation service + database constraints (if exist)

**Why**: Negative or zero quantities break warehouse operations and reporting.

**Validation**:
- Check validation rules in `Service_ReceivingValidation`
- Manual testing: Try saving zero quantity (should fail)

---

## CSV File Safety

### Local CSV is Critical

**Rule**: Save operation must fail if local CSV cannot be written  
**Enforcement**: `Service_CSVWriter` checks local path first

**Why**: Local CSV is backup for database and source for label recovery.

**Acceptable failure**: Network CSV can fail (warning only)  
**Unacceptable failure**: Local CSV failure must abort save

**Validation**:
- Test save with network path unavailable (should succeed with warning)
- Test save with local path unavailable (must fail completely)

---

### CSV Format Must Match Label Templates

**Rule**: Field names and order must match label printer templates  
**Enforcement**: Manual validation with label system vendor

**Why**: Label printer parses CSV by column position/name. Changes break printing.

**Before changing CSV format**:
1. Coordinate with label system vendor
2. Update label templates first (if possible)
3. Test in non-production environment
4. Have rollback plan

**Validation**:
- Generate test CSV
- Submit to label printer test queue
- Verify labels print correctly

---

## Workflow Safety

### Session State Preservation

**Rule**: Session must save after each completed step  
**Enforcement**: `Service_SessionManager` auto-saves

**Why**: Crash recovery depends on session being current.

**Cannot skip**: Session save even if database save pending.

**Validation**:
- Complete one step, close app (force close)
- Restart, verify session recovery shows latest step

---

### Step Order is Immutable

**Rule**: Wizard steps must proceed in defined order  
**Defined in**: `Enum_ReceivingWorkflowStep`

**Why**: Business logic assumes prior steps completed (e.g., can't enter loads without PO).

**Cannot**:
- Skip steps arbitrarily
- Reorder steps without updating all workflow logic
- Allow user to jump to arbitrary step

**Exception**: Edit Mode loads existing data (bypasses wizard)

**Validation**:
- Ensure `Service_ReceivingWorkflow.CanProceedToStep()` enforces order
- Manual test: Try to skip a step

---

## Performance Guardrails

### Bulk Operations Limits

**Rule**: Manual entry grid limited to 100 rows per save  
**Why**: Prevents UI lockup and database timeout

**If more needed**: Split into multiple save operations or upgrade limit with performance testing.

**Validation**:
- Test with 100+ rows (should warn or prevent)
- Monitor save time for large batches

---

### Database Timeout is 30 Seconds

**Rule**: All database operations must complete in 30 seconds or timeout  
**Enforcement**: Connection string timeout setting

**Why**: Prevents indefinite hangs, especially during network issues.

**If operations timeout frequently**: Optimize query or increase timeout (with justification).

**Validation**:
- Monitor logs for timeout exceptions
- Test on slow network connections

---

## Security Guardrails

### No Hardcoded Credentials

**Rule**: Connection strings, passwords, API keys must come from config  
**Enforcement**: Code review

**Where credentials live**:
- Configuration files (encrypted where possible)
- Environment variables
- Secure credential store

**Never**:
```csharp
// ❌ NEVER
string connString = "Server=myserver;User=admin;Password=password123;";
```

**Validation**:
- Search code for "Password=", "User ID=", etc.
- All connection strings should come from `Helper_Database_Variables`

---

### User Authentication Required

**Rule**: Cannot use receiving module without valid user session  
**Enforcement**: Application-level authentication

**Why**: Audit trail requires knowing who performed each receive.

**Cannot bypass**: Production receiving must have user authentication.

**Validation**:
- Attempt to access receiving without logging in (should redirect to login)
- Verify user ID is saved with receiving records

---

## Testing Guardrails

### No Production Testing

**Rule**: Never test with production database  
**Enforcement**: Separate connection strings for dev/test/production

**Why**: Test data corrupts production reports and operations.

**Test environment required**:
- Dev database for development
- Test database for QA
- Production database for production only

**Validation**:
- Check connection string environment-specific
- Production config should have safeguards

---

### No Destructive Tests in Shared Environments

**Rule**: Tests that delete data must use isolated test database  
**Why**: Breaks other developers' work

**Safe test practices**:
- Use test data prefixed with "TEST-"
- Clean up after tests
- Use transactions with rollback for unit tests

**Validation**:
- Review test code for data cleanup
- Ensure test database is separate

---

## Breaking the Guardrails (Emergency Procedures)

Sometimes guardrails must be violated (e.g., emergency production fix). Follow this process:

1. **Document the need**: Why is it an emergency?
2. **Get approval**: Manager or tech lead must approve
3. **Mitigate risk**: How will you minimize impact?
4. **Execute carefully**: Double-check before running
5. **Immediate rollback plan**: Ready to undo if needed
6. **Post-incident review**: Document what happened and why
7. **Update guardrails**: Should this exception become a rule?

**Example scenario**: ERP is down, need to receive materials to keep production running.

**Possible bypass**:
- Temporarily allow receives without ERP validation
- Require supervisor approval for each PO
- Flag these receives for later ERP reconciliation

**Document in Decisions.md**: Why this was necessary and when it can/cannot be used again.

---

## Monitoring and Alerts

Set up alerts for guardrail violations:

**Critical alerts** (page immediately):
- SQL Server write attempt detected
- Local CSV path completely inaccessible for > 5 minutes
- Database connection failure for > 2 minutes

**Warning alerts** (daily summary):
- Network CSV path failures
- Session save failures
- Validation bypasses (if ever implemented)

**Audit log**:
- All guardrail violations (even allowed ones)
- Emergency procedure activations

---

## Enforcement Checklist

During code review, verify:

- [ ] No SQL Server writes
- [ ] MySQL uses stored procedures only
- [ ] DAOs don't throw exceptions
- [ ] Quantities validated as positive
- [ ] Local CSV failure aborts save
- [ ] Session saves after each step
- [ ] No hardcoded credentials
- [ ] No raw SQL in C# code
- [ ] PO validation enforced
- [ ] Proper error handling everywhere

During testing, verify:

- [ ] Can't skip workflow steps
- [ ] Can't save negative quantities
- [ ] Can't save without PO validation
- [ ] Local CSV failure prevents save
- [ ] Network CSV failure allows save
- [ ] Session recovery works
- [ ] Labels print correctly
- [ ] Database records match entered data

---

## Consequences of Guardrail Violations

| Violation | Potential Impact | Recovery Difficulty |
|-----------|------------------|---------------------|
| SQL Server write | ERP data corruption | High (may require vendor support) |
| Skip PO validation | Invalid receives in production | Medium (manual data cleanup) |
| Negative quantity | Routing failures, inventory errors | Low (data correction) |
| Local CSV failure ignored | Lost label data | Medium (manual label creation) |
| Session not saved | Lost user work on crash | Low (user re-enters) |
| CSV format change without coordination | Label printing failure | Medium (restore old CSV format) |
| Hardcoded credentials committed | Security breach | High (credential rotation) |

**Prevention is critical**: Many violations require significant manual intervention to fix.

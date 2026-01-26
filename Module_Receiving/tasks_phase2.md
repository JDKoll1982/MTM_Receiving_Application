# Phase 2: Database & DAOs - Task List

**Phase:** 2 of 8  
**Status:** 🟡 IN PROGRESS (90% complete)  
**Priority:** HIGH - Blocks all subsequent phases  
**Dependencies:** Phase 1 (Foundation) ✅ Complete

---

## 📊 **Phase 2 Overview**

**Goal:** Complete database infrastructure and Data Access Objects (DAOs) to enable CRUD operations

**Status:**
- ✅ Database Schema: 10/10 tables complete
- ✅ Stored Procedures: 29/29 complete
- ✅ Seed Data: 3/3 scripts complete
- ✅ Views: 2/2 complete
- ✅ Functions: 2/2 complete
- ⏳ DAOs: 0/6 complete ← **CURRENT FOCUS**
- ⏳ DAO Tests: 0/6 complete

**Completion:** 58/64 tasks (90%)

---

## ✅ **Completed Tasks**

### Database Schema (10 tables)
- [x] **Task 2.1:** Create `tbl_Receiving_PartType`
- [x] **Task 2.2:** Create `tbl_Receiving_PackageType`
- [x] **Task 2.3:** Create `tbl_Receiving_Location`
- [x] **Task 2.4:** Create `tbl_Receiving_Settings`
- [x] **Task 2.5:** Create `tbl_Receiving_Transaction`
- [x] **Task 2.6:** Create `tbl_Receiving_Line`
- [x] **Task 2.7:** Create `tbl_Receiving_WorkflowSession`
- [x] **Task 2.8:** Create `tbl_Receiving_PartPreference`
- [x] **Task 2.9:** Create `tbl_Receiving_AuditLog`
- [x] **Task 2.10:** Create `tbl_Receiving_CompletedTransaction`

**All constraints explicitly named with pattern:**
- `PK_Receiving_TableName` (Primary Keys)
- `FK_Receiving_TableName_Referenced` (Foreign Keys)
- `UQ_Receiving_TableName_Column` (Unique)
- `CK_Receiving_TableName_Description` (Check)
- `DF_Receiving_TableName_Column` (Default)
- `IX_Receiving_TableName_Column` (Indexes)

### Stored Procedures (29 procedures)

**Transaction (7 procedures):**
- [x] **Task 2.11:** `sp_Receiving_Transaction_Insert`
- [x] **Task 2.12:** `sp_Receiving_Transaction_Update`
- [x] **Task 2.13:** `sp_Receiving_Transaction_SelectById`
- [x] **Task 2.14:** `sp_Receiving_Transaction_SelectByPO`
- [x] **Task 2.15:** `sp_Receiving_Transaction_SelectByDateRange`
- [x] **Task 2.16:** `sp_Receiving_Transaction_Delete` (soft delete + cascade)
- [x] **Task 2.17:** `sp_Receiving_Transaction_Complete` (archive)

**Line (6 procedures):**
- [x] **Task 2.18:** `sp_Receiving_Line_Insert`
- [x] **Task 2.19:** `sp_Receiving_Line_Update`
- [x] **Task 2.20:** `sp_Receiving_Line_Delete`
- [x] **Task 2.21:** `sp_Receiving_Line_SelectById`
- [x] **Task 2.22:** `sp_Receiving_Line_SelectByTransaction`
- [x] **Task 2.23:** `sp_Receiving_Line_SelectByPO`

**WorkflowSession (4 procedures):**
- [x] **Task 2.24:** `sp_Receiving_WorkflowSession_Insert`
- [x] **Task 2.25:** `sp_Receiving_WorkflowSession_Update`
- [x] **Task 2.26:** `sp_Receiving_WorkflowSession_SelectById`
- [x] **Task 2.27:** `sp_Receiving_WorkflowSession_SelectByUser`

**Reference Data (4 procedures):**
- [x] **Task 2.28:** `sp_Receiving_PartType_SelectAll`
- [x] **Task 2.29:** `sp_Receiving_PackageType_SelectAll`
- [x] **Task 2.30:** `sp_Receiving_Location_SelectAll`
- [x] **Task 2.31:** `sp_Receiving_Location_SelectByCode`

**PartPreference (2 procedures):**
- [x] **Task 2.32:** `sp_Receiving_PartPreference_SelectByPart`
- [x] **Task 2.33:** `sp_Receiving_PartPreference_Upsert`

**Settings (2 procedures):**
- [x] **Task 2.34:** `sp_Receiving_Settings_SelectByKey`
- [x] **Task 2.35:** `sp_Receiving_Settings_Upsert`

**CompletedTransaction (2 procedures):**
- [x] **Task 2.36:** `sp_Receiving_CompletedTransaction_SelectByPO`
- [x] **Task 2.37:** `sp_Receiving_CompletedTransaction_SelectByDateRange`

**Audit (2 procedures):**
- [x] **Task 2.38:** `sp_Receiving_AuditLog_Insert`
- [x] **Task 2.39:** `sp_Receiving_AuditLog_SelectByTransaction`

### Seed Data (3 scripts)
- [x] **Task 2.40:** `SeedPartTypes.sql` (4 part types)
- [x] **Task 2.41:** `SeedPackageTypes.sql` (6 package types)
- [x] **Task 2.42:** `SeedDefaultSettings.sql` (6 default settings)

### Views & Functions
- [x] **Task 2.43:** `vw_Receiving_LineWithTransactionDetails`
- [x] **Task 2.44:** `vw_Receiving_TransactionSummary`
- [x] **Task 2.45:** `fn_Receiving_CalculateTotalWeight`
- [x] **Task 2.46:** `fn_Receiving_CalculateTotalQuantity`

### Documentation
- [x] **Task 2.47:** `DATABASE_PROJECT_SETUP.md`
- [x] **Task 2.48:** `DEPLOYMENT_GUIDE.md`
- [x] **Task 2.49:** `STORED_PROCEDURES_REFERENCE.md`
- [x] **Task 2.50:** `SQL-Server-Network-Deployment.md`

---

## ⏳ **Remaining Tasks (6 DAOs)**

### DAO Implementation - Critical Path

**Location:** `Module_Receiving/Data/`

**Pattern:** All DAOs follow this template:
```csharp
public class Dao_Receiving_Repository_[Entity]
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;
    
    public Dao_Receiving_Repository_[Entity](
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }
    
    // Methods call stored procedures via Helper_Database_StoredProcedure
    // All methods return Model_Dao_Result or Model_Dao_Result<T>
    // Never throw exceptions - return failure results
}
```

---

### Task 2.51: Dao_Receiving_Repository_Transaction

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_Transaction.cs`  
**Dependencies:** All Transaction stored procedures (tasks 2.11-2.17)  
**Estimated Time:** 2-3 hours

**Methods to Implement:**
```csharp
Task<Model_Dao_Result<string>> InsertTransactionAsync(Model_Receiving_Entity_ReceivingTransaction transaction)
Task<Model_Dao_Result> UpdateTransactionAsync(Model_Receiving_Entity_ReceivingTransaction transaction)
Task<Model_Dao_Result<Model_Receiving_Entity_ReceivingTransaction>> SelectByIdAsync(string transactionId)
Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingTransaction>>> SelectByPOAsync(string poNumber)
Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingTransaction>>> SelectByDateRangeAsync(DateTime startDate, DateTime endDate)
Task<Model_Dao_Result> DeleteAsync(string transactionId, string modifiedBy)
Task<Model_Dao_Result> CompleteAsync(string transactionId, string completedBy, string csvFilePath = null)
```

**Acceptance Criteria:**
- [ ] All 7 methods implemented
- [ ] All methods call corresponding stored procedures
- [ ] All methods return `Model_Dao_Result` pattern
- [ ] No exceptions thrown (return failure results)
- [ ] Logging for key operations
- [ ] Null parameter validation

---

### Task 2.52: Dao_Receiving_Repository_Line

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_Line.cs`  
**Dependencies:** All Line stored procedures (tasks 2.18-2.23)  
**Estimated Time:** 2-3 hours

**Methods to Implement:**
```csharp
Task<Model_Dao_Result<string>> InsertLineAsync(Model_Receiving_Entity_ReceivingLoad line)
Task<Model_Dao_Result> UpdateLineAsync(Model_Receiving_Entity_ReceivingLoad line)
Task<Model_Dao_Result> DeleteAsync(string lineId, string modifiedBy)
Task<Model_Dao_Result<Model_Receiving_Entity_ReceivingLoad>> SelectByIdAsync(string lineId)
Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLoad>>> SelectByTransactionAsync(string transactionId)
Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLoad>>> SelectByPOAsync(string poNumber)
```

**Acceptance Criteria:**
- [ ] All 6 methods implemented
- [ ] All methods call corresponding stored procedures
- [ ] All methods return `Model_Dao_Result` pattern
- [ ] No exceptions thrown
- [ ] Logging for key operations
- [ ] Null parameter validation

---

### Task 2.53: Dao_Receiving_Repository_WorkflowSession

**Priority:** P0 - CRITICAL  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_WorkflowSession.cs`  
**Dependencies:** All WorkflowSession stored procedures (tasks 2.24-2.27)  
**Estimated Time:** 1.5-2 hours

**Methods to Implement:**
```csharp
Task<Model_Dao_Result<string>> InsertSessionAsync(Model_Receiving_Entity_WorkflowSession session)
Task<Model_Dao_Result> UpdateSessionAsync(Model_Receiving_Entity_WorkflowSession session)
Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> SelectByIdAsync(string sessionId)
Task<Model_Dao_Result<List<Model_Receiving_Entity_WorkflowSession>>> SelectByUserAsync(string username)
```

**Acceptance Criteria:**
- [ ] All 4 methods implemented
- [ ] Session state JSON serialization/deserialization
- [ ] All methods call corresponding stored procedures
- [ ] All methods return `Model_Dao_Result` pattern
- [ ] No exceptions thrown

---

### Task 2.54: Dao_Receiving_Repository_PartPreference

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_PartPreference.cs`  
**Dependencies:** PartPreference stored procedures (tasks 2.32-2.33)  
**Estimated Time:** 1 hour

**Methods to Implement:**
```csharp
Task<Model_Dao_Result<Model_Receiving_Entity_PartPreference>> SelectByPartAsync(string partNumber, string scope = "System", string scopeUserId = null)
Task<Model_Dao_Result> UpsertAsync(Model_Receiving_Entity_PartPreference preference)
```

**Acceptance Criteria:**
- [ ] Both methods implemented
- [ ] Handles System and User scopes
- [ ] Upsert logic (insert or update)
- [ ] All methods return `Model_Dao_Result` pattern

---

### Task 2.55: Dao_Receiving_Repository_Settings ✅ COMPLETE

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_Settings.cs`  
**Dependencies:** Settings stored procedures (tasks 2.34-2.35)  
**Completed:** 2026-01-25

**Methods Implemented:**
```csharp
✅ Task<Model_Dao_Result<Model_Receiving_Entity_Setting>> SelectByKeyAsync(string settingKey, string scope = "System", string scopeUserId = null)
✅ Task<Model_Dao_Result> UpsertAsync(Model_Receiving_Entity_Setting setting)
```

**Acceptance Criteria:**
- [x] Both methods implemented
- [x] Type conversion (String, Integer, Boolean, Decimal)
- [x] Validation (MinValue, MaxValue, ValidValues)
- [x] All methods return `Model_Dao_Result` pattern

**Bonus:** Created supporting entity model `Model_Receiving_Entity_Setting.cs`

---

### Task 2.56: Dao_Receiving_Repository_Reference

**Priority:** P1 - HIGH  
**File:** `Module_Receiving/Data/Dao_Receiving_Repository_Reference.cs`  
**Dependencies:** Reference data stored procedures (tasks 2.28-2.31)  
**Estimated Time:** 1.5 hours

**Methods to Implement:**
```csharp
Task<Model_Dao_Result<List<Model_Receiving_Entity_PartType>>> GetPartTypesAsync()
Task<Model_Dao_Result<List<Model_Receiving_Entity_PackageType>>> GetPackageTypesAsync()
Task<Model_Dao_Result<List<Model_Receiving_Entity_Location>>> GetLocationsAsync(bool allowReceivingOnly = true)
Task<Model_Dao_Result<Model_Receiving_Entity_Location>> GetLocationByCodeAsync(string locationCode)
```

**Acceptance Criteria:**
- [ ] All 4 methods implemented
- [ ] Cached reference data (optional optimization)
- [ ] All methods return `Model_Dao_Result` pattern

---

## 🧪 **Testing Tasks (6 integration tests)**

### Task 2.57-2.62: DAO Integration Tests

**Priority:** P1 - HIGH  
**Location:** `Module_Receiving.Tests/Data/`  
**Dependencies:** All DAOs (tasks 2.51-2.56)  
**Estimated Time:** 4-6 hours total

**Files:**
- `Dao_Receiving_Repository_TransactionTests.cs`
- `Dao_Receiving_Repository_LineTests.cs`
- `Dao_Receiving_Repository_WorkflowSessionTests.cs`
- `Dao_Receiving_Repository_PartPreferenceTests.cs`
- `Dao_Receiving_Repository_SettingsTests.cs`
- `Dao_Receiving_Repository_ReferenceTests.cs`

**Test Pattern:**
```csharp
[Collection("Database")]
public class Dao_Receiving_Repository_TransactionTests : IAsyncLifetime
{
    private readonly Dao_Receiving_Repository_Transaction _dao;
    private string _testTransactionId;
    
    public async Task InitializeAsync()
    {
        // Setup test data
    }
    
    public async Task DisposeAsync()
    {
        // Cleanup test data
    }
    
    [Fact]
    public async Task InsertTransactionAsync_ShouldReturnSuccess_WhenValidData()
    {
        // Arrange, Act, Assert with FluentAssertions
    }
}
```

**Acceptance Criteria:**
- [ ] All CRUD operations tested
- [ ] Positive and negative test cases
- [ ] Uses FluentAssertions
- [ ] Cleanup after tests (IAsyncLifetime)
- [ ] Test data prefixed with "TEST-"

---

## 🔌 **DI Registration Task**

### Task 2.63: Register DAOs in DI Container

**Priority:** P0 - CRITICAL  
**File:** `App.xaml.cs`  
**Dependencies:** All DAOs (tasks 2.51-2.56)  
**Estimated Time:** 30 minutes

**Registration Pattern:**
```csharp
// In ConfigureServices method
var connectionString = Helper_Database_Variables.GetConnectionString();

// Register DAOs as Singletons (stateless, reusable)
services.AddSingleton(sp => new Dao_Receiving_Repository_Transaction(
    connectionString, 
    sp.GetRequiredService<IService_LoggingUtility>()));

services.AddSingleton(sp => new Dao_Receiving_Repository_Line(
    connectionString, 
    sp.GetRequiredService<IService_LoggingUtility>()));

// ... repeat for all 6 DAOs
```

**Acceptance Criteria:**
- [ ] All 6 DAOs registered as Singletons
- [ ] Connection string injected
- [ ] Logging service injected
- [ ] Application builds successfully
- [ ] DAOs can be resolved from container

---

## ✅ **Phase 2 Completion Criteria**

### Database Infrastructure
- [x] All 10 tables created with named constraints
- [x] All 29 stored procedures deployed
- [x] All seed data loaded (PartTypes, PackageTypes, Settings)
- [x] Views and functions created
- [x] Documentation complete

### DAOs
- [ ] All 6 DAOs implemented
- [ ] All DAOs follow instance-based pattern
- [ ] All DAOs return `Model_Dao_Result` (no exceptions)
- [ ] All DAOs registered in DI container
- [ ] Connection string configured

### Testing
- [ ] All 6 DAO integration tests created
- [ ] All tests passing
- [ ] Test data cleanup working

### Validation
- [ ] Application builds without errors
- [ ] Database connection successful
- [ ] All stored procedures callable from DAOs
- [ ] CRUD operations working end-to-end

---

## 📝 **Notes & Guidelines**

### DAO Implementation Rules
1. **No Static Classes:** All DAOs must be instance-based
2. **Constructor Injection:** Connection string and logger via constructor
3. **Error Handling:** Return `Model_Dao_Result`, never throw exceptions
4. **Stored Procedures Only:** All database access via stored procedures
5. **Logging:** Log key operations (insert, update, delete, errors)
6. **Null Checks:** Validate parameters with `ArgumentNullException.ThrowIfNull`

### Connection String Management
```csharp
// Module_Shared/Helpers/Helper_Database_Variables.cs
public static class Helper_Database_Variables
{
    public static string GetConnectionString()
    {
        // Development: LocalDB
        // Production: Network SQL Server
        return ConfigurationManager.ConnectionStrings["MTM_Receiving"].ConnectionString;
    }
}
```

### Model_Dao_Result Pattern
```csharp
// Success with data
return new Model_Dao_Result<Transaction> 
{ 
    IsSuccess = true, 
    Data = transaction 
};

// Failure with error message
return new Model_Dao_Result 
{ 
    IsSuccess = false, 
    ErrorMessage = "Transaction not found",
    Severity = Enum_ErrorSeverity.Medium
};
```

---

## 🚀 **Next Steps After Phase 2**

Once all DAOs are complete and tested:
1. **Phase 3:** Implement CQRS Handlers (18 files)
2. Update IMPLEMENTATION_PROGRESS.md to reflect 100% Phase 2 completion
3. Begin Handler implementation using DAOs

---

**Total Phase 2 Tasks:** 64  
**Completed:** 58  
**Remaining:** 6 (DAOs only)  
**Estimated Time to Complete:** 8-12 hours

# Module_Receiving Database - Stored Procedures Complete Reference

**Database:** Module_Receiving  
**Created:** 2026-01-25  
**Total Procedures:** 29  
**Version:** 1.0

---

## 📊 Overview

Complete set of stored procedures for Module_Receiving database, organized by functional area.

### Folder Structure
```
StoredProcedures/
├── Transaction/        (7 procedures)
├── Line/               (6 procedures)
├── WorkflowSession/    (4 procedures)
├── Reference/          (4 procedures)
├── PartPreference/     (2 procedures)
├── Settings/           (2 procedures)
├── CompletedTransaction/ (2 procedures)
└── Audit/              (2 procedures)
```

---

## 🔹 Transaction Procedures (7)

### **sp_Receiving_Transaction_Insert**
- **Purpose:** Create a new receiving transaction
- **Parameters:** TransactionId, PONumber, PartNumber, TotalLoads, WorkflowMode, CreatedBy
- **Returns:** Success status, ErrorMessage, TransactionId
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_Insert.sql`

### **sp_Receiving_Transaction_Update**
- **Purpose:** Update existing transaction
- **Parameters:** TransactionId, TotalWeight, TotalQuantity, Status, ModifiedBy
- **Returns:** Success status, ErrorMessage
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_Update.sql`

### **sp_Receiving_Transaction_SelectById**
- **Purpose:** Retrieve single transaction by ID
- **Parameters:** TransactionId
- **Returns:** Transaction data, Success status
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_SelectById.sql`

### **sp_Receiving_Transaction_SelectByPO**
- **Purpose:** Retrieve all transactions for a PO Number
- **Parameters:** PONumber
- **Returns:** List of transactions, Success status
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_SelectByPO.sql`

### **sp_Receiving_Transaction_SelectByDateRange**
- **Purpose:** Retrieve transactions within date range
- **Parameters:** StartDate, EndDate
- **Returns:** List of transactions, Success status
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_SelectByDateRange.sql`

### **sp_Receiving_Transaction_Delete**
- **Purpose:** Soft delete transaction and associated lines
- **Parameters:** TransactionId, ModifiedBy
- **Returns:** Success status, ErrorMessage
- **Notes:** Cascades to lines, logs to audit
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_Delete.sql`

### **sp_Receiving_Transaction_Complete**
- **Purpose:** Mark transaction as completed and archive
- **Parameters:** TransactionId, CompletedBy, CSVFilePath (optional)
- **Returns:** Success status, ErrorMessage
- **Notes:** Archives to tbl_Receiving_CompletedTransaction
- **Location:** `StoredProcedures/Transaction/sp_Receiving_Transaction_Complete.sql`

---

## 🔹 Line Procedures (6)

### **sp_Receiving_Line_Insert**
- **Purpose:** Insert a new receiving line
- **Parameters:** LineId, TransactionId, LineNumber, PONumber, PartNumber, LoadNumber, Weight, etc.
- **Returns:** Success status, ErrorMessage, LineId
- **Location:** `StoredProcedures/Line/sp_Receiving_Line_Insert.sql`

### **sp_Receiving_Line_Update**
- **Purpose:** Update existing line
- **Parameters:** LineId, Weight, HeatLot, PackageType, ReceivingLocation, ModifiedBy
- **Returns:** Success status, ErrorMessage
- **Location:** `StoredProcedures/Line/sp_Receiving_Line_Update.sql`

### **sp_Receiving_Line_Delete**
- **Purpose:** Soft delete a line
- **Parameters:** LineId, ModifiedBy
- **Returns:** Success status, ErrorMessage
- **Location:** `StoredProcedures/Line/sp_Receiving_Line_Delete.sql`

### **sp_Receiving_Line_SelectById**
- **Purpose:** Retrieve single line by ID
- **Parameters:** LineId
- **Returns:** Line data, Success status
- **Location:** `StoredProcedures/Line/sp_Receiving_Line_SelectById.sql`

### **sp_Receiving_Line_SelectByTransaction**
- **Purpose:** Retrieve all lines for a transaction
- **Parameters:** TransactionId
- **Returns:** List of lines (ordered by LineNumber), Success status
- **Location:** `StoredProcedures/Line/sp_Receiving_Line_SelectByTransaction.sql`

### **sp_Receiving_Line_SelectByPO**
- **Purpose:** Retrieve all lines for a PO Number
- **Parameters:** PONumber
- **Returns:** List of lines (ordered by date and line number), Success status
- **Location:** `StoredProcedures/Line/sp_Receiving_Line_SelectByPO.sql`

---

## 🔹 WorkflowSession Procedures (4)

### **sp_Receiving_WorkflowSession_Insert**
- **Purpose:** Create new workflow session
- **Parameters:** SessionId, WorkflowMode, PONumber, PartNumber, LoadCount, CreatedBy
- **Returns:** Success status, ErrorMessage, SessionId
- **Location:** `StoredProcedures/WorkflowSession/sp_Receiving_WorkflowSession_Insert.sql`

### **sp_Receiving_WorkflowSession_Update**
- **Purpose:** Update session state (step, validation, data)
- **Parameters:** SessionId, CurrentStep, SessionStatus, LoadDetailsJson, Step1Valid, etc.
- **Returns:** Success status, ErrorMessage
- **Location:** `StoredProcedures/WorkflowSession/sp_Receiving_WorkflowSession_Update.sql`

### **sp_Receiving_WorkflowSession_SelectById**
- **Purpose:** Retrieve session by ID
- **Parameters:** SessionId
- **Returns:** Session data including JSON details, Success status
- **Location:** `StoredProcedures/WorkflowSession/sp_Receiving_WorkflowSession_SelectById.sql`

### **sp_Receiving_WorkflowSession_SelectByUser**
- **Purpose:** Retrieve all active sessions for a user
- **Parameters:** Username
- **Returns:** List of active sessions (ordered by last activity), Success status
- **Location:** `StoredProcedures/WorkflowSession/sp_Receiving_WorkflowSession_SelectByUser.sql`

---

## 🔹 Reference Data Procedures (4)

### **sp_Receiving_PackageType_SelectAll**
- **Purpose:** Retrieve all active package types
- **Parameters:** None
- **Returns:** List of package types (ordered by SortOrder), Success status
- **Usage:** Populate dropdown lists
- **Location:** `StoredProcedures/Reference/sp_Receiving_PackageType_SelectAll.sql`

### **sp_Receiving_PartType_SelectAll**
- **Purpose:** Retrieve all active part types with measurement requirements
- **Parameters:** None
- **Returns:** List of part types with requirement flags, Success status
- **Usage:** Auto-assignment of part types, measurement field visibility
- **Location:** `StoredProcedures/Reference/sp_Receiving_PartType_SelectAll.sql`

### **sp_Receiving_Location_SelectAll**
- **Purpose:** Retrieve all active locations
- **Parameters:** AllowReceivingOnly (BIT, default=1)
- **Returns:** List of locations, Success status
- **Usage:** Populate location dropdown, filter by AllowReceiving flag
- **Location:** `StoredProcedures/Reference/sp_Receiving_Location_SelectAll.sql`

### **sp_Receiving_Location_SelectByCode**
- **Purpose:** Retrieve specific location by code
- **Parameters:** LocationCode
- **Returns:** Location details, Success status
- **Location:** `StoredProcedures/Reference/sp_Receiving_Location_SelectByCode.sql`

---

## 🔹 PartPreference Procedures (2)

### **sp_Receiving_PartPreference_SelectByPart**
- **Purpose:** Retrieve preferences for a specific part
- **Parameters:** PartNumber, Scope (System/User), ScopeUserId (optional)
- **Returns:** Preference data including part type details, Success status
- **Usage:** Auto-fill defaults when part number is entered
- **Location:** `StoredProcedures/PartPreference/sp_Receiving_PartPreference_SelectByPart.sql`

### **sp_Receiving_PartPreference_Upsert**
- **Purpose:** Insert or update part preferences
- **Parameters:** PartNumber, PartTypeId, DefaultLocation, DefaultPackageType, RequiresQualityHold, etc.
- **Returns:** Success status, ErrorMessage
- **Notes:** Updates if exists, inserts if new
- **Location:** `StoredProcedures/PartPreference/sp_Receiving_PartPreference_Upsert.sql`

---

## 🔹 Settings Procedures (2)

### **sp_Receiving_Settings_SelectByKey**
- **Purpose:** Retrieve setting value by key
- **Parameters:** SettingKey, Scope (System/User), ScopeUserId (optional)
- **Returns:** Setting details including type and valid values, Success status
- **Usage:** Application configuration retrieval
- **Location:** `StoredProcedures/Settings/sp_Receiving_Settings_SelectByKey.sql`

### **sp_Receiving_Settings_Upsert**
- **Purpose:** Insert or update application setting
- **Parameters:** SettingKey, SettingValue, SettingType, Category, ValidValues, etc.
- **Returns:** Success status, ErrorMessage
- **Notes:** Updates if exists, inserts if new
- **Location:** `StoredProcedures/Settings/sp_Receiving_Settings_Upsert.sql`

---

## 🔹 CompletedTransaction Procedures (2)

### **sp_Receiving_CompletedTransaction_SelectByPO**
- **Purpose:** Retrieve completed transactions by PO (for Edit Mode search)
- **Parameters:** PONumber
- **Returns:** List of archived transactions with modification history, Success status
- **Usage:** Edit Mode transaction lookup
- **Location:** `StoredProcedures/CompletedTransaction/sp_Receiving_CompletedTransaction_SelectByPO.sql`

### **sp_Receiving_CompletedTransaction_SelectByDateRange**
- **Purpose:** Retrieve completed transactions within date range
- **Parameters:** StartDate, EndDate
- **Returns:** List of archived transactions, Success status
- **Usage:** Reporting, historical queries
- **Location:** `StoredProcedures/CompletedTransaction/sp_Receiving_CompletedTransaction_SelectByDateRange.sql`

---

## 🔹 Audit Procedures (2)

### **sp_Receiving_AuditLog_Insert**
- **Purpose:** Insert audit log entry with field-level tracking
- **Parameters:** TableName, RecordId, Action, FieldName, OldValue, NewValue, PerformedBy, etc.
- **Returns:** Success status, ErrorMessage, AuditId
- **Usage:** Manual audit logging (automated logging happens in other SPs)
- **Location:** `StoredProcedures/Audit/sp_Receiving_AuditLog_Insert.sql`

### **sp_Receiving_AuditLog_SelectByTransaction**
- **Purpose:** Retrieve complete audit trail for a transaction
- **Parameters:** TransactionId
- **Returns:** List of all audit entries (ordered by date descending), Success status
- **Usage:** Audit reporting, change history
- **Location:** `StoredProcedures/Audit/sp_Receiving_AuditLog_SelectByTransaction.sql`

---

## 🎯 Usage Patterns

### Transaction Lifecycle
```sql
-- 1. Create transaction
EXEC sp_Receiving_Transaction_Insert @p_TransactionId, @p_PONumber, @p_PartNumber, ...

-- 2. Add lines
EXEC sp_Receiving_Line_Insert @p_LineId, @p_TransactionId, @p_LineNumber, ...

-- 3. Update transaction totals
EXEC sp_Receiving_Transaction_Update @p_TransactionId, @p_TotalWeight, ...

-- 4. Complete transaction
EXEC sp_Receiving_Transaction_Complete @p_TransactionId, @p_CompletedBy, @p_CSVFilePath
```

### Workflow Session Management
```sql
-- 1. Start session
EXEC sp_Receiving_WorkflowSession_Insert @p_SessionId, 'Wizard', ...

-- 2. Update step progression
EXEC sp_Receiving_WorkflowSession_Update @p_SessionId, @p_CurrentStep=2, @p_Step1Valid=1, ...

-- 3. Retrieve active sessions
EXEC sp_Receiving_WorkflowSession_SelectByUser @p_Username
```

### Part Preferences
```sql
-- Get part preferences (tries user scope first, fallsback to system)
EXEC sp_Receiving_PartPreference_SelectByPart @p_PartNumber, 'User', @p_UserId
-- If not found, try system
EXEC sp_Receiving_PartPreference_SelectByPart @p_PartNumber, 'System', NULL
```

---

## ✅ Deployment Checklist

- [ ] All 29 procedures created in database
- [ ] Seed data loaded (part types, package types, settings)
- [ ] DAOs created in C# to call these procedures
- [ ] Service layer configured to use DAOs
- [ ] ViewModels call services (NOT DAOs directly)
- [ ] Connection string configured in application
- [ ] Error handling tested for each procedure

---

## 📝 Notes

1. **All procedures use try/catch** with standard `IsSuccess`/`ErrorMessage` pattern
2. **Soft deletes** implemented - never hard delete records
3. **Audit logging** included in critical operations (delete, complete, update)
4. **Parameterized queries** - no SQL injection risk
5. **Transaction safety** - XACT_ABORT ON for data integrity
6. **Extended properties** on all procedures for documentation

---

## 🔗 Related Documentation

- **Tables:** `Module_Databases/Module_Receiving_Database/Tables/`
- **Migration Script:** `Scripts/Migration/001_InitialSchema_SQLCMD.sql`
- **Seed Scripts:** `Scripts/Seed/`
- **Database Setup:** `DATABASE_PROJECT_SETUP.md`
- **Deployment Guide:** `DEPLOYMENT_GUIDE.md`

---

**All stored procedures complete and ready for C# DAO integration!** 🎉

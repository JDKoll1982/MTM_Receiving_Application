# SQL Server Naming Conventions Extended

**Version:** 1.0  
**Last Updated:** 2026-01-25  
**Purpose:** Complete SQL Server database naming conventions for MTM Receiving Application with migration strategy from MySQL

**Related Documents:**
- [C# & XAML Naming Conventions](./csharp-xaml-naming-conventions-extended.md)
- [Database Project Integration](../../../.github/instructions/database-project-integration.instructions.md)

---

## 🎯 Overview

This document defines naming conventions for **SQL Server** database objects. The MTM Receiving Application is transitioning from **MySQL (MAMP)** to **SQL Server** for enhanced tooling, version control, and enterprise features.

### Migration Status

**Current State:**
- ✅ Legacy modules use MySQL (MAMP)
- ✅ New modules will use SQL Server
- ✅ Dual-database support during transition
- ✅ Migration scripts provided for each module

**Target State:**
- All modules on SQL Server
- MySQL database deprecated
- Single source of truth for schema

---

## 📐 Core Naming Principles

### 1. **Prefix-Based Organization**

**All database objects use descriptive prefixes:**

| Object Type | Prefix | Example |
|-------------|--------|---------|
| **Table** | `tbl_` | `tbl_Receiving_Line` |
| **View** | `vw_` | `vw_Receiving_TransactionSummary` |
| **Stored Procedure** | `sp_` | `sp_Receiving_Line_Insert` |
| **Function (Scalar)** | `fn_` | `fn_Receiving_CalculateTotalWeight` |
| **Function (Table-Valued)** | `tvf_` | `tvf_Receiving_GetLinesByDateRange` |
| **Index** | `IX_` | `IX_Receiving_Line_PONumber` |
| **Primary Key** | `PK_` | `PK_Receiving_Line` |
| **Foreign Key** | `FK_` | `FK_Receiving_Line_Transaction` |
| **Unique Constraint** | `UQ_` | `UQ_Receiving_Line_LineNumber` |
| **Check Constraint** | `CK_` | `CK_Receiving_Line_QuantityPositive` |
| **Default Constraint** | `DF_` | `DF_Receiving_Line_IsActive` |
| **Trigger** | `tr_` | `tr_Receiving_Line_AfterInsert` |
| **Schema** | (none) | `receiving`, `shared`, `audit` |

### 2. **Module-Based Hierarchy**

**Format:** `{Prefix}_{Module}_{EntityName}_{OptionalQualifier}`

```sql
-- Tables
tbl_Receiving_Transaction
tbl_Receiving_Line
tbl_Receiving_Load
tbl_Shared_AuditLog
tbl_Shared_ErrorLog

-- Stored Procedures
sp_Receiving_Line_Insert
sp_Receiving_Line_Update
sp_Receiving_Transaction_SelectByDateRange
sp_Shared_AuditLog_Insert

-- Views
vw_Receiving_TransactionSummary
vw_Receiving_LoadDetails
vw_Shared_ActiveUsers
```

### 3. **PascalCase for Names**

**All object names use PascalCase:**
- ✅ `tbl_Receiving_Line`
- ✅ `PONumber`, `PartNumber`, `CreatedDate`
- ❌ `tbl_receiving_line`, `po_number`, `created_date`

---

## 📊 Table Naming

### Format

```
tbl_{Module}_{EntityName}
```

### Examples

```sql
-- Module: Receiving
tbl_Receiving_Transaction       -- Header/parent table
tbl_Receiving_Line              -- Detail/child table
tbl_Receiving_Load              -- Individual load details
tbl_Receiving_CompletedTransaction
tbl_Receiving_WorkflowSession
tbl_Receiving_AuditLog

-- Module: Shared (Cross-module)
tbl_Shared_PackageType          -- Reference data
tbl_Shared_PartType             -- Reference data
tbl_Shared_User                 -- User management
tbl_Shared_ApplicationSettings

-- Module: Volvo
tbl_Volvo_Shipment
tbl_Volvo_ShipmentLine
tbl_Volvo_Part
```

### Column Naming

**Use PascalCase, descriptive names:**

```sql
CREATE TABLE [dbo].[tbl_Receiving_Line]
(
    -- Primary Key
    [LineId] CHAR(36) NOT NULL,           -- Guid as string
    
    -- Foreign Keys
    [TransactionId] CHAR(36) NOT NULL,
    
    -- Business Data
    [PONumber] NVARCHAR(50) NULL,
    [PartNumber] NVARCHAR(50) NOT NULL,
    [Quantity] INT NOT NULL,
    [Weight] DECIMAL(18, 2) NULL,
    [HeatLot] NVARCHAR(100) NULL,
    [PackageType] NVARCHAR(50) NULL,
    [PackagesPerLoad] INT NULL,
    [ReceivingLocation] NVARCHAR(100) NULL,
    
    -- Flags (BIT type)
    [IsNonPO] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields (Standard across all tables)
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    CONSTRAINT [PK_Receiving_Line] PRIMARY KEY CLUSTERED ([LineId])
);
```

**Standard Column Patterns:**

| Pattern | Type | Example | Notes |
|---------|------|---------|-------|
| **ID Columns** | `CHAR(36)` | `LineId`, `TransactionId` | Guid as string |
| **Boolean Flags** | `BIT` | `IsActive`, `IsNonPO`, `IsDeleted` | Prefix with `Is` |
| **Dates** | `DATETIME2` | `CreatedDate`, `ModifiedDate` | Use UTC |
| **Money/Decimal** | `DECIMAL(18, 2)` | `Weight`, `TotalAmount` | 18 precision, 2 scale |
| **Strings** | `NVARCHAR(n)` | `PONumber`, `PartNumber` | Unicode support |
| **Codes** | `VARCHAR(20)` | `StatusCode`, `TypeCode` | Non-unicode OK |
| **User Names** | `NVARCHAR(100)` | `CreatedBy`, `ModifiedBy` | Windows username |

**Standard Audit Columns (Required on All Tables):**

```sql
[CreatedBy] NVARCHAR(100) NOT NULL,
[CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
[ModifiedBy] NVARCHAR(100) NULL,
[ModifiedDate] DATETIME2 NULL
```

---

## 🔑 Constraint Naming

### Primary Keys

**Format:** `PK_{TableName}`

```sql
CONSTRAINT [PK_Receiving_Line] PRIMARY KEY CLUSTERED ([LineId])
CONSTRAINT [PK_Receiving_Transaction] PRIMARY KEY CLUSTERED ([TransactionId])
```

### Foreign Keys

**Format:** `FK_{ChildTable}_{ParentTable}` or `FK_{ChildTable}_{ParentTable}_{ColumnName}`

```sql
-- Simple FK
CONSTRAINT [FK_Receiving_Line_Transaction] 
    FOREIGN KEY ([TransactionId]) 
    REFERENCES [dbo].[tbl_Receiving_Transaction]([TransactionId])
    ON DELETE CASCADE

-- Multiple FKs to same parent (use column name)
CONSTRAINT [FK_Receiving_Transaction_CreatedByUser] 
    FOREIGN KEY ([CreatedBy]) 
    REFERENCES [dbo].[tbl_Shared_User]([Username])
    ON DELETE NO ACTION

CONSTRAINT [FK_Receiving_Transaction_ModifiedByUser] 
    FOREIGN KEY ([ModifiedBy]) 
    REFERENCES [dbo].[tbl_Shared_User]([Username])
    ON DELETE NO ACTION
```

### Unique Constraints

**Format:** `UQ_{TableName}_{ColumnName(s)}`

```sql
CONSTRAINT [UQ_Receiving_Line_LineNumber] 
    UNIQUE ([TransactionId], [LineNumber])

CONSTRAINT [UQ_Shared_User_Email] 
    UNIQUE ([Email])
```

### Check Constraints

**Format:** `CK_{TableName}_{ColumnName}_{Rule}`

```sql
CONSTRAINT [CK_Receiving_Line_QuantityPositive] 
    CHECK ([Quantity] > 0)

CONSTRAINT [CK_Receiving_Line_WeightPositive] 
    CHECK ([Weight] IS NULL OR [Weight] > 0)

CONSTRAINT [CK_Receiving_Transaction_StatusValid] 
    CHECK ([Status] IN ('Draft', 'Completed', 'Cancelled'))
```

### Default Constraints

**Format:** `DF_{TableName}_{ColumnName}`

```sql
CONSTRAINT [DF_Receiving_Line_IsActive] 
    DEFAULT 1 FOR [IsActive]

CONSTRAINT [DF_Receiving_Line_CreatedDate] 
    DEFAULT GETUTCDATE() FOR [CreatedDate]
```

---

## 📇 Index Naming

### Non-Clustered Indexes

**Format:** `IX_{TableName}_{ColumnName(s)}` or `IX_{TableName}_{ColumnName}_Desc` for descending

```sql
-- Single column index
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber]
    ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC);

-- Multi-column index
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber_PartNumber]
    ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC, [PartNumber] ASC);

-- Descending index (for date ranges)
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_CreatedDate_Desc]
    ON [dbo].[tbl_Receiving_Line] ([CreatedDate] DESC);

-- Covering index with INCLUDE
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber_Covering]
    ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC)
    INCLUDE ([PartNumber], [Quantity], [Weight]);
```

### Clustered Index

**Usually the primary key, but can be custom:**

```sql
-- Custom clustered index (not on PK)
CREATE CLUSTERED INDEX [CIX_Receiving_AuditLog_CreatedDate]
    ON [dbo].[tbl_Receiving_AuditLog] ([CreatedDate] ASC);
```

---

## 📜 Stored Procedure Naming

### Format

```
sp_{Module}_{EntityName}_{Action}
```

### Standard Actions

| Action | Purpose | Example |
|--------|---------|---------|
| `Insert` | Create new record | `sp_Receiving_Line_Insert` |
| `Update` | Modify existing record | `sp_Receiving_Line_Update` |
| `Delete` | Remove record | `sp_Receiving_Line_Delete` |
| `Select` | Get specific record(s) | `sp_Receiving_Line_SelectById` |
| `SelectBy{Criteria}` | Query with filter | `sp_Receiving_Line_SelectByPO` |
| `SelectAll` | Get all records | `sp_Receiving_PackageType_SelectAll` |
| `Upsert` | Insert or update | `sp_Receiving_Line_Upsert` |
| `BulkInsert` | Insert multiple | `sp_Receiving_Line_BulkInsert` |

### Examples

```sql
-- CRUD Operations
sp_Receiving_Line_Insert
sp_Receiving_Line_Update
sp_Receiving_Line_Delete
sp_Receiving_Line_SelectById
sp_Receiving_Line_SelectByPO
sp_Receiving_Line_SelectAll

-- Complex Queries
sp_Receiving_Transaction_SelectByDateRange
sp_Receiving_Transaction_SelectByUser
sp_Receiving_Transaction_SelectByStatus
sp_Receiving_Load_SelectByPartNumber

-- Business Logic
sp_Receiving_Transaction_Complete
sp_Receiving_Line_BulkCopy
sp_Receiving_Workflow_SaveSession
```

### Stored Procedure Template

```sql
-- File: StoredProcedures/ReceivingLine/sp_Receiving_Line_Insert.sql

CREATE PROCEDURE [dbo].[sp_Receiving_Line_Insert]
    @p_LineId CHAR(36),
    @p_TransactionId CHAR(36),
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50),
    @p_Quantity INT,
    @p_Weight DECIMAL(18, 2) = NULL,
    @p_HeatLot NVARCHAR(100) = NULL,
    @p_PackageType NVARCHAR(50) = NULL,
    @p_PackagesPerLoad INT = NULL,
    @p_ReceivingLocation NVARCHAR(100) = NULL,
    @p_IsNonPO BIT = 0,
    @p_CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;  -- Rollback on any error
    
    BEGIN TRY
        -- Validation
        IF @p_LineId IS NULL OR @p_LineId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'LineId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_PartNumber IS NULL OR @p_PartNumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PartNumber is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_Quantity <= 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Quantity must be greater than zero' AS ErrorMessage;
            RETURN;
        END
        
        -- Insert
        BEGIN TRANSACTION;
        
        INSERT INTO [dbo].[tbl_Receiving_Line] (
            [LineId], [TransactionId], [PONumber], [PartNumber], [Quantity],
            [Weight], [HeatLot], [PackageType], [PackagesPerLoad],
            [ReceivingLocation], [IsNonPO], [IsActive], [IsDeleted],
            [CreatedBy], [CreatedDate]
        )
        VALUES (
            @p_LineId, @p_TransactionId, @p_PONumber, @p_PartNumber, @p_Quantity,
            @p_Weight, @p_HeatLot, @p_PackageType, @p_PackagesPerLoad,
            @p_ReceivingLocation, @p_IsNonPO, 1, 0,
            @p_CreatedBy, GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
        
        -- Return success (DAOs expect IsSuccess/ErrorMessage)
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Return error (DO NOT throw - DAOs handle errors via result)
        SELECT 
            0 AS IsSuccess, 
            ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

-- Extended Property for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Inserts a new receiving line into the database. Returns IsSuccess (BIT) and ErrorMessage (NVARCHAR).',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_Insert';
GO
```

**Key Points:**
- ✅ Parameters prefixed with `@p_` for clarity
- ✅ Returns `IsSuccess` (BIT) and `ErrorMessage` (NVARCHAR)
- ✅ Uses transactions (`BEGIN TRANSACTION` / `COMMIT` / `ROLLBACK`)
- ✅ Never throws exceptions - returns error message
- ✅ Extended properties for documentation

---

## 👁️ View Naming

### Format

```
vw_{Module}_{ViewName}
```

### Examples

```sql
-- Summary/Reporting Views
vw_Receiving_TransactionSummary
vw_Receiving_LoadDetailsFull
vw_Receiving_DailySummaryByUser

-- Denormalized Views
vw_Receiving_LineWithTransactionDetails
vw_Receiving_LoadWithAllDetails

-- Filtered Views
vw_Receiving_ActiveTransactions
vw_Receiving_NonPOTransactions
vw_Shared_ActiveUsers
```

### View Template

```sql
-- File: Views/vw_Receiving_TransactionSummary.sql

CREATE VIEW [dbo].[vw_Receiving_TransactionSummary]
AS
SELECT 
    t.[TransactionId],
    t.[PONumber],
    t.[PartNumber],
    t.[TotalLoads],
    t.[TotalWeight],
    t.[WorkflowMode],
    t.[Status],
    t.[IsNonPO],
    t.[CreatedBy],
    t.[CreatedDate],
    t.[CompletedDate],
    
    -- Aggregated data from child table
    COUNT(l.[LineId]) AS LineCount,
    SUM(l.[Quantity]) AS TotalQuantity,
    AVG(l.[Weight]) AS AverageWeight
    
FROM [dbo].[tbl_Receiving_Transaction] t
LEFT JOIN [dbo].[tbl_Receiving_Line] l 
    ON t.[TransactionId] = l.[TransactionId]
    AND l.[IsActive] = 1 
    AND l.[IsDeleted] = 0
    
WHERE t.[IsActive] = 1
  AND t.[IsDeleted] = 0

GROUP BY 
    t.[TransactionId], t.[PONumber], t.[PartNumber], t.[TotalLoads],
    t.[TotalWeight], t.[WorkflowMode], t.[Status], t.[IsNonPO],
    t.[CreatedBy], t.[CreatedDate], t.[CompletedDate];
GO
```

---

## ⚙️ Function Naming

### Scalar Functions

**Format:** `fn_{Module}_{FunctionName}`

```sql
-- File: Functions/Scalar/fn_Receiving_CalculateTotalWeight.sql

CREATE FUNCTION [dbo].[fn_Receiving_CalculateTotalWeight]
(
    @TransactionId CHAR(36)
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
    DECLARE @TotalWeight DECIMAL(18, 2);
    
    SELECT @TotalWeight = SUM([Weight])
    FROM [dbo].[tbl_Receiving_Line]
    WHERE [TransactionId] = @TransactionId
      AND [IsActive] = 1
      AND [IsDeleted] = 0;
    
    RETURN ISNULL(@TotalWeight, 0);
END
GO
```

### Table-Valued Functions

**Format:** `tvf_{Module}_{FunctionName}`

```sql
-- File: Functions/TableValued/tvf_Receiving_GetLinesByDateRange.sql

CREATE FUNCTION [dbo].[tvf_Receiving_GetLinesByDateRange]
(
    @StartDate DATETIME2,
    @EndDate DATETIME2
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        [LineId],
        [TransactionId],
        [PONumber],
        [PartNumber],
        [Quantity],
        [Weight],
        [CreatedDate]
    FROM [dbo].[tbl_Receiving_Line]
    WHERE [CreatedDate] >= @StartDate
      AND [CreatedDate] <= @EndDate
      AND [IsActive] = 1
      AND [IsDeleted] = 0
);
GO
```

---

## 🔔 Trigger Naming

**Format:** `tr_{TableName}_{Timing}_{Action}`

**Timing:** `After`, `Instead`  
**Action:** `Insert`, `Update`, `Delete`

```sql
-- File: Triggers/tr_Receiving_Line_AfterInsert.sql

CREATE TRIGGER [dbo].[tr_Receiving_Line_AfterInsert]
ON [dbo].[tbl_Receiving_Line]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Log insert to audit table
    INSERT INTO [dbo].[tbl_Shared_AuditLog] (
        [AuditId], [TableName], [Action], [RecordId], 
        [PerformedBy], [PerformedDate], [Details]
    )
    SELECT 
        NEWID(), 
        'tbl_Receiving_Line', 
        'INSERT', 
        i.[LineId],
        i.[CreatedBy], 
        GETUTCDATE(),
        CONCAT('PO: ', i.[PONumber], ', Part: ', i.[PartNumber])
    FROM inserted i;
END
GO
```

---

## 🏗️ Schema Organization

**Use schemas for logical grouping:**

```sql
-- Create schemas
CREATE SCHEMA [receiving] AUTHORIZATION [dbo];
CREATE SCHEMA [volvo] AUTHORIZATION [dbo];
CREATE SCHEMA [shared] AUTHORIZATION [dbo];
CREATE SCHEMA [audit] AUTHORIZATION [dbo];

-- Use schemas in object names
CREATE TABLE [receiving].[Transaction] (...);  -- receiving.Transaction
CREATE TABLE [volvo].[Shipment] (...);         -- volvo.Shipment
CREATE TABLE [shared].[PackageType] (...);     -- shared.PackageType
CREATE TABLE [audit].[AuditLog] (...);         -- audit.AuditLog
```

**Recommendation:** For MTM Receiving Application, use `[dbo]` schema initially. Introduce custom schemas if needed for security or organization.

---

## 🔄 Migration from MySQL to SQL Server

### Syntax Translation Guide

| MySQL | SQL Server | Notes |
|-------|------------|-------|
| `AUTO_INCREMENT` | `IDENTITY(1,1)` | Auto-incrementing column |
| `TINYINT(1)` | `BIT` | Boolean values |
| `VARCHAR(n) CHARACTER SET utf8mb4` | `NVARCHAR(n)` | Unicode strings |
| `TEXT` | `NVARCHAR(MAX)` | Large text |
| `DATETIME` | `DATETIME2` | More precision |
| `` `backticks` `` | `[SquareBrackets]` | Object delimiters |
| `IF NOT EXISTS` | Schema Compare handles | DDL syntax |
| `DELIMITER $$` | (Not needed) | Batch separator is `GO` |
| `CONCAT()` | `CONCAT()` or `+` | String concatenation |
| `IFNULL(x, y)` | `ISNULL(x, y)` or `COALESCE(x, y)` | Null handling |
| `NOW()` | `GETDATE()` or `GETUTCDATE()` | Current date/time |
| `LIMIT n` | `TOP(n)` | Row limiting |

### Migration Script Template

```sql
-- File: Scripts/Migration/001_CreateReceivingTables.sql

/*
   Migration Script: Create Receiving Tables (SQL Server)
   Source: MySQL tbl_receiving_line, tbl_receiving_transaction
   Target: SQL Server tbl_Receiving_Line, tbl_Receiving_Transaction
   Date: 2026-01-25
*/

-- =============================================
-- 1. Create Parent Table: tbl_Receiving_Transaction
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Receiving_Transaction')
BEGIN
    CREATE TABLE [dbo].[tbl_Receiving_Transaction]
    (
        [TransactionId] CHAR(36) NOT NULL,
        [PONumber] NVARCHAR(50) NULL,
        [PartNumber] NVARCHAR(50) NOT NULL,
        [TotalLoads] INT NOT NULL,
        [TotalWeight] DECIMAL(18, 2) NULL,
        [WorkflowMode] NVARCHAR(50) NOT NULL,
        [WorkflowStep] NVARCHAR(50) NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [IsNonPO] BIT NOT NULL DEFAULT 0,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletedBy] NVARCHAR(100) NULL,
        [CompletedDate] DATETIME2 NULL,
        [ModifiedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME2 NULL,
        
        CONSTRAINT [PK_Receiving_Transaction] PRIMARY KEY CLUSTERED ([TransactionId])
    );
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_PONumber]
        ON [dbo].[tbl_Receiving_Transaction] ([PONumber] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_Status]
        ON [dbo].[tbl_Receiving_Transaction] ([Status] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Transaction_CreatedDate]
        ON [dbo].[tbl_Receiving_Transaction] ([CreatedDate] DESC);
    
    PRINT 'Table created: tbl_Receiving_Transaction';
END
ELSE
BEGIN
    PRINT 'Table already exists: tbl_Receiving_Transaction';
END
GO

-- =============================================
-- 2. Create Child Table: tbl_Receiving_Line
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Receiving_Line')
BEGIN
    CREATE TABLE [dbo].[tbl_Receiving_Line]
    (
        [LineId] CHAR(36) NOT NULL,
        [TransactionId] CHAR(36) NOT NULL,
        [PONumber] NVARCHAR(50) NULL,
        [PartNumber] NVARCHAR(50) NOT NULL,
        [LoadNumber] INT NOT NULL,
        [Quantity] INT NOT NULL,
        [Weight] DECIMAL(18, 2) NULL,
        [HeatLot] NVARCHAR(100) NULL,
        [PackageType] NVARCHAR(50) NULL,
        [PackagesPerLoad] INT NULL,
        [ReceivingLocation] NVARCHAR(100) NULL,
        [IsNonPO] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME2 NULL,
        
        CONSTRAINT [PK_Receiving_Line] PRIMARY KEY CLUSTERED ([LineId]),
        
        CONSTRAINT [FK_Receiving_Line_Transaction] 
            FOREIGN KEY ([TransactionId]) 
            REFERENCES [dbo].[tbl_Receiving_Transaction]([TransactionId])
            ON DELETE CASCADE,
        
        CONSTRAINT [CK_Receiving_Line_QuantityPositive] 
            CHECK ([Quantity] > 0),
        
        CONSTRAINT [CK_Receiving_Line_WeightPositive] 
            CHECK ([Weight] IS NULL OR [Weight] > 0)
    );
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Line_TransactionId]
        ON [dbo].[tbl_Receiving_Line] ([TransactionId] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber]
        ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PartNumber]
        ON [dbo].[tbl_Receiving_Line] ([PartNumber] ASC);
    
    CREATE NONCLUSTERED INDEX [IX_Receiving_Line_CreatedDate]
        ON [dbo].[tbl_Receiving_Line] ([CreatedDate] DESC);
    
    PRINT 'Table created: tbl_Receiving_Line';
END
ELSE
BEGIN
    PRINT 'Table already exists: tbl_Receiving_Line';
END
GO

-- =============================================
-- 3. Seed Reference Data (if needed)
-- =============================================

-- Example: Insert default package types
IF NOT EXISTS (SELECT * FROM [dbo].[tbl_Shared_PackageType] WHERE [PackageTypeCode] = 'SKID')
BEGIN
    INSERT INTO [dbo].[tbl_Shared_PackageType] ([PackageTypeId], [PackageTypeCode], [PackageTypeName], [IsActive])
    VALUES (NEWID(), 'SKID', 'Skid', 1);
    PRINT 'Seed data inserted: PackageType SKID';
END
GO
```

### Data Migration Script Template

```sql
-- File: Scripts/Migration/002_MigrateDataFromMySQL.sql

/*
   Data Migration Script: MySQL → SQL Server
   
   Prerequisites:
   - Linked server to MySQL configured (or use SSIS, Azure Data Factory)
   - SQL Server tables created (run 001_CreateReceivingTables.sql first)
   
   Note: This is a template. Actual migration may use:
   - SQL Server Integration Services (SSIS)
   - Azure Data Factory
   - Manual CSV export/import
   - Custom C# migration tool
*/

-- =============================================
-- Option 1: Using Linked Server
-- =============================================

-- Configure linked server to MySQL (one-time)
-- EXEC sp_addlinkedserver 
--     @server='MYSQL_SERVER', 
--     @srvproduct='MySQL',
--     @provider='MSDASQL', 
--     @datasrc='MTM_MySQL_ODBC_DSN';

-- Migrate tbl_Receiving_Transaction
INSERT INTO [dbo].[tbl_Receiving_Transaction]
SELECT 
    transaction_id,
    po_number,
    part_number,
    total_loads,
    total_weight,
    workflow_mode,
    workflow_step,
    status,
    CASE WHEN is_non_po = 1 THEN 1 ELSE 0 END,  -- TINYINT → BIT
    CASE WHEN is_completed = 1 THEN 1 ELSE 0 END,
    1,  -- IsActive (default)
    0,  -- IsDeleted (default)
    created_by,
    created_date,  -- MySQL DATETIME → SQL Server DATETIME2
    completed_by,
    completed_date,
    modified_by,
    modified_date
FROM OPENQUERY(MYSQL_SERVER, 'SELECT * FROM mtm_receiving.tbl_receiving_transaction');

PRINT CONCAT('Migrated ', @@ROWCOUNT, ' records to tbl_Receiving_Transaction');
GO

-- =============================================
-- Option 2: Using BULK INSERT from CSV
-- =============================================

-- Export from MySQL to CSV first, then:

BULK INSERT [dbo].[tbl_Receiving_Line]
FROM 'C:\Temp\receiving_line_export.csv'
WITH
(
    FIRSTROW = 2,  -- Skip header
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    TABLOCK
);

PRINT CONCAT('Bulk inserted ', @@ROWCOUNT, ' records to tbl_Receiving_Line');
GO
```

---

## 🔀 Dual-Database Support Strategy

### During Migration Period

**Application supports BOTH MySQL and SQL Server:**

```csharp
// File: Helper_Database_Infrastructure_ConnectionManagement.cs

public static class Helper_Database_Infrastructure_ConnectionManagement
{
    public static string GetConnectionString(DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.MySQL => GetMySQLConnectionString(),      // Legacy
            DatabaseType.SQLServer => GetSQLServerConnectionString(), // New
            _ => throw new ArgumentException("Invalid database type")
        };
    }
    
    private static string GetMySQLConnectionString()
    {
        // Legacy MySQL connection (MAMP)
        return "Server=localhost;Port=8889;Database=mtm_receiving;User=root;Password=root;";
    }
    
    private static string GetSQLServerConnectionString()
    {
        // New SQL Server connection
        return "Server=(localdb)\\MSSQLLocalDB;Database=MTM_Receiving;Integrated Security=true;TrustServerCertificate=true;";
    }
}

public enum DatabaseType
{
    MySQL,      // Legacy - will be deprecated
    SQLServer   // Target - all new modules
}
```

### Module-Level Database Selection

```csharp
// File: Module_Receiving/Data/Dao_Receiving_Repository_ReceivingLine.cs

public class Dao_Receiving_Repository_ReceivingLine
{
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;
    
    public Dao_Receiving_Repository_ReceivingLine(
        string connectionString,
        DatabaseType databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
    }
    
    public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line)
    {
        // Route to appropriate SP based on database type
        var spName = _databaseType == DatabaseType.SQLServer
            ? "sp_Receiving_Line_Insert"        // SQL Server SP
            : "sp_receiving_line_insert";       // MySQL SP (legacy)
        
        var parameters = BuildParameters(line, _databaseType);
        
        return await Helper_Database_StoredProcedure.ExecuteAsync(
            spName,
            parameters,
            _connectionString,
            _databaseType
        );
    }
}
```

### Configuration-Based Toggle

```json
// appsettings.json

{
  "DatabaseSettings": {
    "Receiving": {
      "DatabaseType": "SQLServer",  // "MySQL" or "SQLServer"
      "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=MTM_Receiving;Integrated Security=true;"
    },
    "Volvo": {
      "DatabaseType": "MySQL",      // Still on MySQL during migration
      "ConnectionString": "Server=localhost;Port=8889;Database=mtm_volvo;User=root;Password=root;"
    },
    "Shared": {
      "DatabaseType": "SQLServer",  // Shared tables on SQL Server
      "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=MTM_Shared;Integrated Security=true;"
    }
  }
}
```

---

## 📝 Complete Table Definition Template

```sql
-- File: Tables/tbl_Receiving_Line.sql

-- =============================================
-- Table: tbl_Receiving_Line
-- Purpose: Individual receiving line items within a transaction
-- Module: Receiving
-- Created: 2026-01-25
-- =============================================

CREATE TABLE [dbo].[tbl_Receiving_Line]
(
    -- =============================================
    -- PRIMARY KEY
    -- =============================================
    [LineId] CHAR(36) NOT NULL,
    
    -- =============================================
    -- FOREIGN KEYS
    -- =============================================
    [TransactionId] CHAR(36) NOT NULL,
    
    -- =============================================
    -- BUSINESS DATA
    -- =============================================
    [PONumber] NVARCHAR(50) NULL,
    [PartNumber] NVARCHAR(50) NOT NULL,
    [LoadNumber] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [Weight] DECIMAL(18, 2) NULL,
    [HeatLot] NVARCHAR(100) NULL,
    [PackageType] NVARCHAR(50) NULL,
    [PackagesPerLoad] INT NULL,
    [ReceivingLocation] NVARCHAR(100) NULL,
    
    -- =============================================
    -- FLAGS (Boolean indicators)
    -- =============================================
    [IsNonPO] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- =============================================
    -- AUDIT FIELDS (Required on all tables)
    -- =============================================
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- =============================================
    -- CONSTRAINTS
    -- =============================================
    
    -- Primary Key
    CONSTRAINT [PK_Receiving_Line] PRIMARY KEY CLUSTERED ([LineId]),
    
    -- Foreign Keys
    CONSTRAINT [FK_Receiving_Line_Transaction] 
        FOREIGN KEY ([TransactionId]) 
        REFERENCES [dbo].[tbl_Receiving_Transaction]([TransactionId])
        ON DELETE CASCADE,
    
    -- Check Constraints
    CONSTRAINT [CK_Receiving_Line_QuantityPositive] 
        CHECK ([Quantity] > 0),
    
    CONSTRAINT [CK_Receiving_Line_WeightPositive] 
        CHECK ([Weight] IS NULL OR [Weight] > 0),
    
    CONSTRAINT [CK_Receiving_Line_LoadNumberPositive] 
        CHECK ([LoadNumber] > 0),
    
    -- Unique Constraints
    CONSTRAINT [UQ_Receiving_Line_TransactionLoad] 
        UNIQUE ([TransactionId], [LoadNumber])
);
GO

-- =============================================
-- INDEXES
-- =============================================

-- Foreign Key Index
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_TransactionId]
    ON [dbo].[tbl_Receiving_Line] ([TransactionId] ASC);

-- Query Optimization Indexes
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber]
    ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC)
    WHERE [PONumber] IS NOT NULL;  -- Filtered index

CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PartNumber]
    ON [dbo].[tbl_Receiving_Line] ([PartNumber] ASC);

CREATE NONCLUSTERED INDEX [IX_Receiving_Line_CreatedDate]
    ON [dbo].[tbl_Receiving_Line] ([CreatedDate] DESC);

-- Covering Index for common query pattern
CREATE NONCLUSTERED INDEX [IX_Receiving_Line_PONumber_Covering]
    ON [dbo].[tbl_Receiving_Line] ([PONumber] ASC)
    INCLUDE ([PartNumber], [Quantity], [Weight], [CreatedDate])
    WHERE [IsActive] = 1 AND [IsDeleted] = 0;  -- Filtered
GO

-- =============================================
-- EXTENDED PROPERTIES (Documentation)
-- =============================================

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Individual receiving line item within a receiving transaction. Each line represents one load of material received.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique identifier for the receiving line (Guid as CHAR(36))',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'LineId';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Set to TRUE (1) for non-PO receiving (samples, returns, misc items). When TRUE, PONumber is not required.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'IsNonPO';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Soft delete flag. When TRUE (1), record is considered deleted but not physically removed.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'IsDeleted';
GO
```

---

## ✅ Compliance Checklist

**Before committing SQL Server database project:**

- [ ] All tables prefixed with `tbl_`
- [ ] All stored procedures prefixed with `sp_`
- [ ] All views prefixed with `vw_`
- [ ] All functions prefixed with `fn_` or `tvf_`
- [ ] All indexes prefixed with `IX_`
- [ ] All constraints use proper prefixes (PK_, FK_, CK_, UQ_, DF_)
- [ ] All column names use PascalCase
- [ ] All tables have audit fields (CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
- [ ] All tables have IsActive and IsDeleted flags
- [ ] All stored procedures return IsSuccess/ErrorMessage
- [ ] All stored procedures use transactions (BEGIN TRANSACTION / COMMIT)
- [ ] All stored procedures handle errors (TRY/CATCH)
- [ ] All tables have extended properties for documentation
- [ ] All indexes created for foreign keys
- [ ] All check constraints validate business rules
- [ ] Migration scripts provided for transition from MySQL

---

## 📚 Related Documentation

- [C# & XAML Naming Conventions](./csharp-xaml-naming-conventions-extended.md) - Application code naming
- [Database Project Integration](../../../.github/instructions/database-project-integration.instructions.md) - Setup guide
- [File Structure](./file-structure.md) - Project organization
- [CQRS Infrastructure](../../../Module_Core/README_CQRS_INFRASTRUCTURE.md) - CQRS patterns

---

## 🎯 Quick Reference

**Creating a new table:**
1. Name: `tbl_{Module}_{EntityName}`
2. Primary Key: `[PK_{TableName}]`
3. Include audit fields
4. Add indexes for foreign keys and common queries
5. Add extended properties for documentation
6. Create stored procedures (Insert, Update, Delete, Select)

**Creating a new stored procedure:**
1. Name: `sp_{Module}_{EntityName}_{Action}`
2. Parameters prefixed with `@p_`
3. Return `IsSuccess` (BIT) and `ErrorMessage` (NVARCHAR)`
4. Use transactions
5. Handle errors with TRY/CATCH
6. Never throw exceptions

**Migration from MySQL:**
1. Create SQL Server schema with proper naming
2. Write migration script (001_CreateTables.sql)
3. Configure dual-database support in application
4. Test with SQL Server LocalDB
5. Migrate data (SSIS, CSV, or linked server)
6. Switch module to SQL Server in appsettings.json
7. Deprecate MySQL for that module

**The database is code. Name it properly.** ✅

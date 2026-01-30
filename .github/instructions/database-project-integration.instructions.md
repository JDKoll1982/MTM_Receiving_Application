---
description: Complete guide for integrating SQL Server Database Projects with feature modules
applyTo: '**/*.{sql,sqlproj,cs,md}'
---

# Database Project Integration Guide

**Purpose:** Treat database schema as first-class code with version control, automated deployment, and build-time validation.

**When to Use:** When adding/updating a feature module that requires database schema (tables, stored procedures, views, functions).

**Related Documents:**
- [CQRS Infrastructure](../Module_Core/README_CQRS_INFRASTRUCTURE.md)
- [Module_Receiving Implementation Blueprint](../../specs/Module_Receiving/03-Implementation-Blueprint/index.md)

---

## 🎯 Quick Decision Guide

**Should I Create a Database Project for My Module?**

| Scenario | Recommendation |
|----------|----------------|
| New module with 5+ tables, 10+ stored procedures | ✅ **YES** - Start with database project |
| Existing module being rebuilt/modernized | ✅ **YES** - Migrate to database project |
| Simple module with 1-2 tables, no SPs | ⚠️ **MAYBE** - Consider future growth |
| Module uses external database (read-only) | ❌ **NO** - Database project not needed |
| Shared/Core infrastructure tables | ✅ **YES** - Separate shared database project |

**For MTM Receiving Application:** All feature modules with MySQL schema **SHOULD** use database projects.

---

## 📋 Prerequisites

### Required Tools

1. **Visual Studio 2022** (v17.8 or later)
   - Workload: "Data storage and processing"
   - Component: "SQL Server Data Tools"

2. **MySQL ODBC Driver** (for schema import)
   - Download: https://dev.mysql.com/downloads/connector/odbc/
   - Version: 8.0 or later

3. **SQL Server Database Tools (SSDT)**
   - Included with Visual Studio workload
   - Provides `.sqlproj` project type

### Optional Tools

4. **SqlPackage CLI** (for CI/CD)
   - Download: https://aka.ms/sqlpackage
   - Used for automated deployments

5. **MySQL Workbench** (for initial schema design)
   - Download: https://dev.mysql.com/downloads/workbench/

---

## 🚀 Step-by-Step Setup

### Phase 1: Create Database Project

#### 1.1 Add New Project to Solution

```
Visual Studio → Solution Explorer → Right-click Solution
→ Add → New Project
→ Search: "SQL Server Database Project"
→ Name: {ModuleName}_Database (e.g., MTM_Receiving_Database)
→ Location: {SolutionRoot}/Database/
→ Create
```

**Naming Convention:**
- Format: `{ModuleName}_Database`
- Examples:
  - `MTM_Receiving_Database`
  - `Module_Volvo_Database`
  - `Module_Dunnage_Database`
  - `MTM_Shared_Database` (for cross-module tables)

#### 1.2 Configure Project Properties

```xml
<!-- Right-click project → Properties → Project Settings -->

Target Platform: Microsoft SQL Server 2022
Default Collation: SQL_Latin1_General_CP1_CI_AS
Database Compatibility Level: 150 (SQL Server 2019)

<!-- For MySQL compatibility -->
ANSI_NULLS: ON
QUOTED_IDENTIFIER: ON
```

**Important:** Even though we use MySQL, SQL Server Database Projects provide the tooling. We'll translate syntax as needed.

#### 1.3 Create Folder Structure

```
{ModuleName}_Database/
├── Tables/
│   ├── tbl_{Module}_{EntityName}.sql
│   └── ... (one file per table)
│
├── StoredProcedures/
│   ├── {EntityName}/
│   │   ├── sp_{Module}_{Entity}_{Action}.sql
│   │   └── ... (grouped by entity)
│   └── ...
│
├── Views/
│   └── vw_{Module}_{ViewName}.sql
│
├── Functions/
│   ├── Scalar/
│   │   └── fn_{Module}_{FunctionName}.sql
│   └── TableValued/
│       └── tvf_{Module}_{FunctionName}.sql
│
├── Indexes/
│   └── IX_{TableName}_{ColumnName}.sql (if not inline with table)
│
├── Scripts/
│   ├── PreDeployment/
│   │   └── Script.PreDeployment.sql (runs before schema changes)
│   ├── PostDeployment/
│   │   └── Script.PostDeployment.sql (runs after schema changes)
│   ├── Seed/
│   │   ├── SeedPackageTypes.sql
│   │   └── SeedPartTypes.sql
│   └── Migration/
│       ├── 001_InitialSchema.sql
│       └── 002_AddNonPOSupport.sql
│
└── Security/
    └── Schemas/
        └── receiving.sql (if using custom schemas)
```

**Folder Purpose:**
- **Tables/**: Table definitions (CREATE TABLE)
- **StoredProcedures/**: SP definitions grouped by entity
- **Views/**: View definitions
- **Functions/**: Scalar and table-valued functions
- **Indexes/**: Standalone index definitions (or inline with tables)
- **Scripts/PreDeployment**: Runs BEFORE schema comparison (data backup, etc.)
- **Scripts/PostDeployment**: Runs AFTER schema comparison (seed data, etc.)
- **Scripts/Seed/**: Reference data inserts
- **Scripts/Migration/**: Custom migration logic
- **Security/**: Custom schemas, users, roles (if needed)

---

### Phase 2: Import Existing Schema (For Existing Modules)

#### 2.1 Import from MySQL Database

**Option A: Via MySQL ODBC (Recommended)**

```
1. Configure ODBC Data Source:
   - Control Panel → Administrative Tools → ODBC Data Sources (64-bit)
   - Add → MySQL ODBC 8.0 Driver
   - Name: MTM_Receiving_MySQL
   - Server: localhost (or your MySQL server)
   - Database: mtm_receiving
   - User/Password: (your credentials)
   - Test Connection

2. Import in Visual Studio:
   - Right-click database project → Import → Database
   - Data Source: .NET Framework Data Provider for ODBC
   - Connection String: DSN=MTM_Receiving_MySQL
   - Select Objects:
     ✓ Tables
     ✓ Stored Procedures
     ✓ Views
     ✓ Functions
   - Import

3. Review Imported Files:
   - Fix any syntax differences (MySQL → T-SQL)
   - Organize into folder structure
   - Rename files to match naming convention
```

**Option B: Manual Schema Creation (For New Modules)**

```
Skip import, create .sql files manually following templates below.
```

#### 2.2 Fix MySQL → T-SQL Syntax Differences

**Common Translations:**

| MySQL | T-SQL (Database Project) |
|-------|--------------------------|
| `AUTO_INCREMENT` | `IDENTITY(1,1)` |
| `TINYINT(1)` (boolean) | `BIT` |
| `VARCHAR(255) CHARACTER SET utf8mb4` | `NVARCHAR(255)` |
| `ENGINE=InnoDB` | (Remove, not needed) |
| `` `backticks` `` | `[SquareBrackets]` |
| `IF NOT EXISTS` | Use `IF NOT EXISTS` or Schema Compare |
| `DELIMITER $$` | (Remove, not needed) |

**Example Conversion:**

```sql
-- MySQL Original
CREATE TABLE `tbl_Receiving_Line` (
  `LineId` CHAR(36) NOT NULL,
  `PONumber` VARCHAR(50) CHARACTER SET utf8mb4,
  `IsActive` TINYINT(1) DEFAULT 1,
  PRIMARY KEY (`LineId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- T-SQL for Database Project
CREATE TABLE [dbo].[tbl_Receiving_Line] (
    [LineId] CHAR(36) NOT NULL PRIMARY KEY,
    [PONumber] NVARCHAR(50) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
);
```

**Stored Procedure Example:**

```sql
-- MySQL Original
DELIMITER $$
CREATE PROCEDURE sp_Receiving_Line_Insert(
    IN p_PONumber VARCHAR(50),
    IN p_PartNumber VARCHAR(50)
)
BEGIN
    INSERT INTO tbl_Receiving_Line (PONumber, PartNumber)
    VALUES (p_PONumber, p_PartNumber);
END$$
DELIMITER ;

-- T-SQL for Database Project
CREATE PROCEDURE [dbo].[sp_Receiving_Line_Insert]
    @p_PONumber NVARCHAR(50),
    @p_PartNumber NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbl_Receiving_Line (PONumber, PartNumber)
    VALUES (@p_PONumber, @p_PartNumber);
END
```

---

### Phase 3: Table Definition Templates

#### 3.1 Standard Table Template

```sql
-- File: Tables/tbl_Receiving_Line.sql

CREATE TABLE [dbo].[tbl_Receiving_Line]
(
    -- Primary Key
    [LineId] CHAR(36) NOT NULL PRIMARY KEY,
    
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
    
    -- Flags
    [IsNonPO] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Indexes (inline for simplicity)
    INDEX IX_Receiving_Line_PONumber NONCLUSTERED ([PONumber] ASC),
    INDEX IX_Receiving_Line_TransactionId NONCLUSTERED ([TransactionId] ASC),
    INDEX IX_Receiving_Line_CreatedDate NONCLUSTERED ([CreatedDate] DESC),
    
    -- Foreign Key Constraints
    CONSTRAINT FK_Receiving_Line_Transaction 
        FOREIGN KEY ([TransactionId]) 
        REFERENCES [dbo].[tbl_Receiving_Transaction]([TransactionId])
        ON DELETE CASCADE
);
GO

-- Extended Properties (Documentation)
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Individual receiving line item within a transaction', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'tbl_Receiving_Line';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Set to TRUE for non-PO receiving (samples, returns, etc.)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'tbl_Receiving_Line',
    @level2type = N'COLUMN', @level2name = N'IsNonPO';
GO
```

#### 3.2 Transaction/Header Table Template

```sql
-- File: Tables/tbl_Receiving_Transaction.sql

CREATE TABLE [dbo].[tbl_Receiving_Transaction]
(
    -- Primary Key
    [TransactionId] CHAR(36) NOT NULL PRIMARY KEY,
    
    -- Business Data
    [PONumber] NVARCHAR(50) NULL,
    [PartNumber] NVARCHAR(50) NOT NULL,
    [TotalLoads] INT NOT NULL,
    [TotalWeight] DECIMAL(18, 2) NULL,
    
    -- Workflow State
    [WorkflowMode] NVARCHAR(50) NOT NULL, -- 'Wizard', 'Manual', 'Edit'
    [WorkflowStep] NVARCHAR(50) NULL,      -- Current step if incomplete
    [Status] NVARCHAR(50) NOT NULL,        -- 'Draft', 'Completed', 'Cancelled'
    
    -- Flags
    [IsNonPO] BIT NOT NULL DEFAULT 0,
    [IsCompleted] BIT NOT NULL DEFAULT 0,
    
    -- Audit Fields
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CompletedBy] NVARCHAR(100) NULL,
    [CompletedDate] DATETIME2 NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedDate] DATETIME2 NULL,
    
    -- Indexes
    INDEX IX_Receiving_Transaction_PONumber NONCLUSTERED ([PONumber] ASC),
    INDEX IX_Receiving_Transaction_Status NONCLUSTERED ([Status] ASC),
    INDEX IX_Receiving_Transaction_CreatedDate NONCLUSTERED ([CreatedDate] DESC)
);
GO
```

---

### Phase 4: Stored Procedure Templates

#### 4.1 Insert Procedure Template

```sql
-- File: StoredProcedures/ReceivingLine/sp_Receiving_Line_Insert.sql

CREATE PROCEDURE [dbo].[sp_Receiving_Line_Insert]
    @p_LineId CHAR(36),
    @p_TransactionId CHAR(36),
    @p_PONumber NVARCHAR(50),
    @p_PartNumber NVARCHAR(50),
    @p_Quantity INT,
    @p_Weight DECIMAL(18, 2),
    @p_HeatLot NVARCHAR(100),
    @p_PackageType NVARCHAR(50),
    @p_PackagesPerLoad INT,
    @p_ReceivingLocation NVARCHAR(100),
    @p_IsNonPO BIT,
    @p_CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @p_LineId IS NULL OR @p_LineId = ''
    BEGIN
        RAISERROR('LineId is required', 16, 1);
        RETURN;
    END
    
    IF @p_PartNumber IS NULL OR @p_PartNumber = ''
    BEGIN
        RAISERROR('PartNumber is required', 16, 1);
        RETURN;
    END
    
    -- Insert
    BEGIN TRY
        INSERT INTO [dbo].[tbl_Receiving_Line] (
            LineId, TransactionId, PONumber, PartNumber, Quantity,
            Weight, HeatLot, PackageType, PackagesPerLoad,
            ReceivingLocation, IsNonPO, CreatedBy, CreatedDate
        )
        VALUES (
            @p_LineId, @p_TransactionId, @p_PONumber, @p_PartNumber, @p_Quantity,
            @p_Weight, @p_HeatLot, @p_PackageType, @p_PackagesPerLoad,
            @p_ReceivingLocation, @p_IsNonPO, @p_CreatedBy, GETUTCDATE()
        );
        
        -- Return success
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        -- Return error (DO NOT throw exception - DAOs expect result)
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO
```

#### 4.2 Select/Query Procedure Template

```sql
-- File: StoredProcedures/ReceivingLine/sp_Receiving_Line_SelectByPO.sql

CREATE PROCEDURE [dbo].[sp_Receiving_Line_SelectByPO]
    @p_PONumber NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Query
    SELECT 
        LineId,
        TransactionId,
        PONumber,
        PartNumber,
        Quantity,
        Weight,
        HeatLot,
        PackageType,
        PackagesPerLoad,
        ReceivingLocation,
        IsNonPO,
        IsActive,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate
    FROM [dbo].[tbl_Receiving_Line]
    WHERE PONumber = @p_PONumber
      AND IsActive = 1
      AND IsDeleted = 0
    ORDER BY CreatedDate DESC;
END
GO
```

#### 4.3 Update Procedure Template

```sql
-- File: StoredProcedures/ReceivingLine/sp_Receiving_Line_Update.sql

CREATE PROCEDURE [dbo].[sp_Receiving_Line_Update]
    @p_LineId CHAR(36),
    @p_Quantity INT,
    @p_Weight DECIMAL(18, 2),
    @p_HeatLot NVARCHAR(100),
    @p_ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE [dbo].[tbl_Receiving_Line]
        SET Quantity = @p_Quantity,
            Weight = @p_Weight,
            HeatLot = @p_HeatLot,
            ModifiedBy = @p_ModifiedBy,
            ModifiedDate = GETUTCDATE()
        WHERE LineId = @p_LineId;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Line not found' AS ErrorMessage;
            RETURN;
        END
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO
```

---

### Phase 5: Seed Data Scripts

#### 5.1 PostDeployment Script (Main Entry Point)

```sql
-- File: Scripts/PostDeployment/Script.PostDeployment.sql

/*
 Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed after the build script.        
 Use SQLCMD syntax to reference other scripts:
    :r .\SeedPackageTypes.sql
--------------------------------------------------------------------------------------
*/

-- Seed reference data
:r ..\Seed\SeedPackageTypes.sql
:r ..\Seed\SeedPartTypes.sql
:r ..\Seed\SeedReceivingLocations.sql

GO
```

#### 5.2 Seed Data Template

```sql
-- File: Scripts/Seed/SeedPackageTypes.sql

-- Seed Package Types (Reference Data)

IF NOT EXISTS (SELECT 1 FROM tbl_Receiving_PackageType WHERE PackageTypeCode = 'SKID')
BEGIN
    INSERT INTO tbl_Receiving_PackageType (PackageTypeId, PackageTypeCode, PackageTypeName, IsActive)
    VALUES (NEWID(), 'SKID', 'Skid', 1);
END

IF NOT EXISTS (SELECT 1 FROM tbl_Receiving_PackageType WHERE PackageTypeCode = 'BUNDLE')
BEGIN
    INSERT INTO tbl_Receiving_PackageType (PackageTypeId, PackageTypeCode, PackageTypeName, IsActive)
    VALUES (NEWID(), 'BUNDLE', 'Bundle', 1);
END

IF NOT EXISTS (SELECT 1 FROM tbl_Receiving_PackageType WHERE PackageTypeCode = 'BOX')
BEGIN
    INSERT INTO tbl_Receiving_PackageType (PackageTypeId, PackageTypeCode, PackageTypeName, IsActive)
    VALUES (NEWID(), 'BOX', 'Box', 1);
END

-- ... (repeat for all package types)

GO
```

---

### Phase 6: Build and Validate

#### 6.1 Build Database Project

```
Solution Explorer → Right-click {ModuleName}_Database → Build

Expected Output:
========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========

If Errors:
- Review error messages
- Fix SQL syntax issues
- Check table/SP dependencies
- Rebuild
```

**Common Build Errors:**

| Error | Cause | Fix |
|-------|-------|-----|
| `SQL71501: Column not found` | Typo in column name | Fix column reference |
| `SQL71006: Reference to external object` | Missing table reference | Add table .sql file |
| `SQL46010: Circular dependency` | Tables reference each other | Review FK design |
| `SQL71558: Unresolved reference` | SP references missing object | Add missing object or remove reference |

#### 6.2 Schema Validation

```
Right-click project → Schema Compare → Select Source/Target

Source: {ModuleName}_Database (project)
Target: Actual MySQL database (via ODBC)

Compare:
- Shows differences between project and database
- Green = In project, not in database
- Red = In database, not in project
- Orange = Different (schema mismatch)

Update Target:
- Generates deployment script
- Review script before applying
- Apply to synchronize database
```

---

### Phase 7: CI/CD Integration

#### 7.1 Azure DevOps Pipeline

```yaml
# azure-pipelines.yml

trigger:
  branches:
    include:
      - main
      - develop
  paths:
    include:
      - Database/MTM_Receiving_Database/**

stages:
- stage: Build
  jobs:
  - job: BuildDatabase
    pool:
      vmImage: 'windows-latest'
    
    steps:
    - task: MSBuild@1
      displayName: 'Build Database Project'
      inputs:
        solution: 'Database/MTM_Receiving_Database/MTM_Receiving_Database.sqlproj'
        configuration: 'Release'
    
    - task: PublishBuildArtifacts@1
      displayName: 'Publish DACPAC'
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'dacpac'

- stage: DeployQA
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/develop'))
  jobs:
  - deployment: DeployToQA
    environment: 'QA'
    pool:
      vmImage: 'windows-latest'
    
    strategy:
      runOnce:
        deploy:
          steps:
          - task: SqlAzureDacpacDeployment@1
            displayName: 'Deploy to QA Database'
            inputs:
              azureSubscription: 'Azure-Subscription'
              AuthenticationType: 'server'
              ServerName: 'qa-mysql-server.mysql.database.azure.com'
              DatabaseName: 'mtm_receiving_qa'
              SqlUsername: '$(QA_DB_User)'
              SqlPassword: '$(QA_DB_Password)'
              deployType: 'DacpacTask'
              DeploymentAction: 'Publish'
              DacpacFile: '$(Pipeline.Workspace)/dacpac/MTM_Receiving_Database.dacpac'
              BlockOnPossibleDataLoss: true
```

#### 7.2 GitHub Actions

```yaml
# .github/workflows/database-deploy.yml

name: Database Deploy

on:
  push:
    branches: [main, develop]
    paths:
      - 'Database/MTM_Receiving_Database/**'

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Build Database Project
      run: msbuild Database/MTM_Receiving_Database/MTM_Receiving_Database.sqlproj /p:Configuration=Release
    
    - name: Upload DACPAC
      uses: actions/upload-artifact@v3
      with:
        name: dacpac
        path: Database/MTM_Receiving_Database/bin/Release/MTM_Receiving_Database.dacpac
  
  deploy-qa:
    needs: build
    if: github.ref == 'refs/heads/develop'
    runs-on: windows-latest
    environment: QA
    
    steps:
    - name: Download DACPAC
      uses: actions/download-artifact@v3
      with:
        name: dacpac
    
    - name: Deploy to QA
      run: |
        sqlpackage.exe /Action:Publish `
          /SourceFile:MTM_Receiving_Database.dacpac `
          /TargetConnectionString:"Server=${{ secrets.QA_DB_SERVER }};Database=mtm_receiving_qa;User Id=${{ secrets.QA_DB_USER }};Password=${{ secrets.QA_DB_PASSWORD }};" `
          /p:BlockOnPossibleDataLoss=True
```

---

## 🔄 Integration with CQRS/MediatR

### DAO Pattern with Database Project

**DAOs call stored procedures from database project:**

```csharp
// File: Module_Receiving/Data/Dao_Receiving_Repository_ReceivingLine.cs

namespace MTM_Receiving_Application.Module_Receiving.Data;

public class Dao_Receiving_Repository_ReceivingLine
{
    private readonly string _connectionString;
    private readonly ILogger<Dao_Receiving_Repository_ReceivingLine> _logger;
    
    public Dao_Receiving_Repository_ReceivingLine(
        string connectionString,
        ILogger<Dao_Receiving_Repository_ReceivingLine> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }
    
    public async Task<Model_Dao_Result> InsertReceivingLineAsync(
        Model_Receiving_Entity_ReceivingLine line)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new("@p_LineId", line.LineId.ToString()),
                new("@p_TransactionId", line.TransactionId.ToString()),
                new("@p_PONumber", line.PONumber ?? string.Empty),
                new("@p_PartNumber", line.PartNumber),
                new("@p_Quantity", line.Quantity),
                new("@p_Weight", line.Weight ?? (object)DBNull.Value),
                new("@p_HeatLot", line.HeatLot ?? string.Empty),
                new("@p_PackageType", line.PackageType ?? string.Empty),
                new("@p_PackagesPerLoad", line.PackagesPerLoad ?? (object)DBNull.Value),
                new("@p_ReceivingLocation", line.ReceivingLocation ?? string.Empty),
                new("@p_IsNonPO", line.IsNonPO),
                new("@p_CreatedBy", line.CreatedBy)
            };
            
            // Call stored procedure from database project
            // SP name matches file: StoredProcedures/ReceivingLine/sp_Receiving_Line_Insert.sql
            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Receiving_Line_Insert",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting receiving line {LineId}", line.LineId);
            return Model_Dao_Result_Factory.Failure(
                $"Unexpected error inserting receiving line: {ex.Message}", 
                ex);
        }
    }
}
```

**Key Points:**
- ✅ DAO references SP by name: `"sp_Receiving_Line_Insert"`
- ✅ SP name matches database project file
- ✅ Changes to SP in database project are versioned
- ✅ Schema Compare validates DAO ↔ SP consistency

---

## 📐 Migration Strategy (Existing Modules)

### Step-by-Step Module Migration

**Goal:** Convert existing module from manual SQL scripts to database project without downtime.

#### Migration Phase 1: Setup (Week 1)

1. **Create database project** for module
2. **Import existing schema** from MySQL
3. **Organize files** into folder structure
4. **Fix syntax** (MySQL → T-SQL)
5. **Build project** (validate all SQL)
6. **Schema compare** with existing database
7. **Document differences** (if any)

**Deliverable:** Working database project that mirrors current production schema.

#### Migration Phase 2: Parallel Run (Week 2)

8. **Keep both systems**:
   - Existing: Manual scripts in repository
   - New: Database project
9. **All new changes** go in BOTH places temporarily
10. **CI/CD** builds database project (doesn't deploy yet)
11. **Test deployments** to QA environment
12. **Validate** schema consistency

**Deliverable:** Confidence that database project is production-ready.

#### Migration Phase 3: Cutover (Week 3)

13. **Deploy database project** to production (should be no-op)
14. **Remove manual scripts** from repository
15. **Update CI/CD** to deploy from database project
16. **Update documentation** to reference database project
17. **Train team** on new workflow

**Deliverable:** Module fully migrated to database project.

---

## ✅ Checklist for New Database Project

**Before First Commit:**
- [ ] Database project builds without errors
- [ ] All tables have primary keys
- [ ] All foreign keys have proper constraints
- [ ] Indexes defined for common query patterns
- [ ] Stored procedures return `IsSuccess`/`ErrorMessage`
- [ ] Seed data scripts included
- [ ] PostDeployment script references all seed scripts
- [ ] Schema Compare shows no unexpected differences
- [ ] Extended properties (descriptions) added
- [ ] File names follow naming convention

**Before First Deployment:**
- [ ] Schema Compare generated and reviewed
- [ ] Deployment script tested on QA database
- [ ] Data loss risks identified and mitigated
- [ ] Rollback plan documented
- [ ] Team notified of schema changes
- [ ] CI/CD pipeline configured
- [ ] Database backup taken

**After First Deployment:**
- [ ] Schema Compare shows synchronized state
- [ ] Application DAOs work with new schema
- [ ] Integration tests pass
- [ ] No data loss occurred
- [ ] Performance metrics unchanged
- [ ] Documentation updated

---

## 🚨 Common Pitfalls & Solutions

### Pitfall 1: "My stored procedure won't build"

**Symptom:**
```
SQL71501: Procedure has an unresolved reference to object [dbo].[tbl_Receiving_Line].[PartNumber]
```

**Cause:** Build order issue - SP references table that hasn't been "built" yet.

**Solution:**
```
1. Ensure table .sql file exists in Tables/ folder
2. Right-click table file → Include in Project (if not already)
3. Rebuild project
4. If still fails, check for circular dependencies
```

### Pitfall 2: "Schema Compare shows everything different"

**Symptom:** Every object shows as different even though schemas match.

**Cause:** Collation, encoding, or minor syntax differences.

**Solution:**
```
1. Schema Compare → Options
2. Ignore:
   ✓ Whitespace
   ✓ Comments
   ✓ Extended Properties (initially)
   ✓ Permissions (if not managing in project)
3. Re-compare
```

### Pitfall 3: "Deployment fails with data loss error"

**Symptom:**
```
Error: Deployment would result in data loss. Operation cancelled.
```

**Cause:** Dropping/altering column with data.

**Solution:**
```
1. Review deployment script
2. If acceptable data loss:
   - Schema Compare → Generate Script
   - Manually review
   - Add /p:BlockOnPossibleDataLoss=False
3. If unacceptable:
   - Write migration script in Scripts/Migration/
   - Backup data before schema change
   - Restore data after schema change
```

### Pitfall 4: "CI/CD can't connect to MySQL"

**Symptom:** Pipeline fails with connection error.

**Cause:** MySQL not supported natively by SqlPackage.

**Solution:**
```
Use custom deployment script:

1. Export DACPAC in pipeline
2. Convert T-SQL to MySQL syntax
3. Execute on MySQL using mysql CLI
4. Example: Use third-party tools like DBUp or FluentMigrator
```

**Or:** Use MySQL-compatible schema management (not SQL Server Database Project):
- **DbUp**: https://dbup.readthedocs.io/
- **FluentMigrator**: https://fluentmigrator.github.io/
- **Roundhouse**: https://github.com/chucknorris/roundhouse

**Note:** This guide uses SQL Server Database Project for **development-time benefits** (IntelliSense, refactoring, build validation). For production MySQL, you may need runtime translation.

---

## 📚 Additional Resources

**Microsoft Documentation:**
- [SQL Server Data Tools (SSDT)](https://docs.microsoft.com/en-us/sql/ssdt/sql-server-data-tools)
- [SqlPackage CLI](https://docs.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage)
- [DACPAC Files](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications)

**Project-Specific:**
- [CQRS Infrastructure](../Module_Core/README_CQRS_INFRASTRUCTURE.md)
- [Module Development Guide](../BMAD/module-agents/config/module-development-guide.md)
- [Naming Conventions Extended](../../specs/Module_Receiving/03-Implementation-Blueprint/naming-conventions-extended.md)

**Tools:**
- [MySQL ODBC Driver](https://dev.mysql.com/downloads/connector/odbc/)
- [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup)
- [Azure Data Studio](https://aka.ms/azuredatastudio)

---

## 🎓 Best Practices Summary

**DO:**
- ✅ One database project per feature module
- ✅ Use folder structure: Tables/, StoredProcedures/, Scripts/
- ✅ Follow naming conventions: `tbl_`, `sp_`, `vw_`, `fn_`
- ✅ Include seed data in PostDeployment scripts
- ✅ Add extended properties for documentation
- ✅ Version control everything
- ✅ Build database project in CI/CD
- ✅ Schema Compare before every deployment
- ✅ Test deployments on QA first

**DON'T:**
- ❌ Modify production database manually
- ❌ Skip Schema Compare step
- ❌ Ignore data loss warnings
- ❌ Mix database and application code in one project
- ❌ Store connection strings in database project
- ❌ Deploy without backup
- ❌ Use SELECT * in stored procedures
- ❌ Forget to update seed data scripts

---

## 🤝 Team Workflow

**Developer A makes schema change:**
```
1. Modify table .sql file in database project
2. Update any affected stored procedures
3. Build project (validates changes)
4. Schema Compare with local database
5. Apply changes to local database
6. Test application with new schema
7. Commit database project changes
8. Push to repository
```

**Developer B pulls changes:**
```
1. Git pull (gets database project changes)
2. Build database project
3. Schema Compare with local database
4. Review deployment script
5. Apply changes to local database
6. Continue development
```

**QA Deployment:**
```
1. CI/CD detects database project changes
2. Builds database project
3. Runs validation tests
4. Schema Compare with QA database
5. Generates deployment script
6. Deploys to QA (after approval)
7. Runs integration tests
```

**Production Deployment:**
```
1. QA approved and tested
2. Create production deployment plan
3. Backup production database
4. Schema Compare with production
5. Review deployment script
6. Execute during maintenance window
7. Validate application functionality
8. Monitor for errors
```

---

## 🎯 Next Steps

**Ready to add database project to your module?**

1. **Review this guide** in full
2. **Create database project** following Phase 1
3. **Import or create schema** following Phase 2-3
4. **Build and validate** following Phase 6
5. **Integrate with CI/CD** following Phase 7
6. **Update team** on new workflow

**Questions or issues?**
- Refer to Common Pitfalls section
- Review existing database projects (Module_Receiving_Database)
- Consult Additional Resources

**The database is now code. Treat it accordingly.** ✅

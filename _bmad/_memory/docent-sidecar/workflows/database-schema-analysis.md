---
workflow: database-schema-analysis
command: DS
description: Deep-dive analysis of database layer only
version: 1.0.0
---

# Database Schema Analysis Workflow

**Command:** DS (Database Schema)
**Purpose:** Focused database layer documentation without full module analysis
**Use Case:** Database changes, schema migrations, or deep technical reference needed

---

## Workflow Execution Steps

### Step 1: Initialize Database Analysis

**Prompt user:**

```
🗄️ Docent - Database Schema Analysis

Module: [await input]
Database focus: [MySQL / SQL Server / Both]
Include schema DDL? [y/n]
Output location: docs/database/{ModuleName}_schema.md
```

---

### Step 2: Discover Database Components

**Scan module for database references:**

1. **Find DAOs:**
   - List all `Dao_*.cs` files
   - Extract database target (MySQL vs SQL Server)

2. **Extract Stored Procedure Calls:**
   - Search for `ExecuteStoredProcedureAsync` calls
   - List procedure names referenced
   - Note parameters passed

3. **Identify Tables:**
   - From stored procedure names, infer table names
   - From model names (Model_ReceivingPackage → receiving_packages)

**Discovery Summary:**

```
Database Components Found:
- DAOs: {count}
- Stored Procedures Referenced: {count}
- Tables Inferred: {count}
```

---

### Step 3: Query Database Schemas

**For MySQL (mtm_receiving_application):**

```sql
-- For each stored procedure
SHOW CREATE PROCEDURE {procedure_name};

-- For each table
DESCRIBE {table_name};
SHOW CREATE TABLE {table_name};
SHOW INDEX FROM {table_name};
```

**For SQL Server (Infor Visual - MTMFG):**

```sql
-- Stored procedure details
SELECT 
    p.name AS procedure_name,
    par.name AS parameter_name,
    t.name AS data_type,
    par.max_length,
    par.is_output
FROM sys.procedures p
LEFT JOIN sys.parameters par ON p.object_id = par.object_id
LEFT JOIN sys.types t ON par.system_type_id = t.system_type_id
WHERE p.name = '{procedure_name}'

-- Table schema
SELECT 
    c.name AS column_name,
    t.name AS data_type,
    c.max_length,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('{table_name}')
```

---

### Step 4: Generate Stored Procedure Documentation

**For each stored procedure:**

```markdown
### {StoredProcedureName}

**Database:** {mtm_receiving_application / MTMFG}  
**Purpose:** {Infer from name and usage context}  
**Called By:** {DaoName}.{MethodName}()

#### Parameters

| Parameter | Data Type | Direction | Default | Nullable | Description |
|-----------|-----------|-----------|---------|----------|-------------|
| p_{name} | INT | IN | NULL | NO | {Purpose} |
| p_{name} | VARCHAR(50) | IN | NULL | YES | {Purpose} |
| p_{result} | INT | OUT | NULL | NO | {Return value description} |

#### Logic Summary

{High-level description of what procedure does}

**Key Operations:**
1. {Step 1 description}
2. {Step 2 description}
3. {Step 3 description}

#### Tables Accessed

| Table | Operation | Columns Affected |
|-------|-----------|------------------|
| {table_name} | SELECT | {column_list} |
| {table_name} | INSERT | {column_list} |
| {table_name} | UPDATE | {column_list} |

#### Return Value

- **Type:** {Scalar / Result Set / OUT parameter}
- **Description:** {What is returned}

#### Example Call from DAO

```csharp
var parameters = new List<MySqlParameter>
{
    new MySqlParameter("p_{name}", MySqlDbType.Int32) { Value = {value} },
    new MySqlParameter("p_{name}", MySqlDbType.VarChar) { Value = {value} }
};

var result = await Helper_Database_StoredProcedure
    .ExecuteStoredProcedureAsync("{procedure_name}", parameters);
```

#### DDL (if requested)

```sql
{SHOW CREATE PROCEDURE output}
```

```

---

### Step 5: Generate Table Documentation

**For each table:**

```markdown
### {table_name}

**Database:** {mtm_receiving_application / MTMFG}  
**Purpose:** {Description}  
**Related Models:** {C# model names}

#### Columns

| Column | Data Type | Length | Nullable | Default | Constraints | Description |
|--------|-----------|--------|----------|---------|-------------|-------------|
| id | INT | - | NO | AUTO_INCREMENT | PRIMARY KEY | Unique identifier |
| {column} | VARCHAR | 50 | YES | NULL | - | {Purpose} |
| {column} | DECIMAL | 10,2 | NO | 0.00 | - | {Purpose} |
| created_at | DATETIME | - | NO | CURRENT_TIMESTAMP | - | Record creation time |

#### Indexes

| Index Name | Type | Columns | Description |
|------------|------|---------|-------------|
| PRIMARY | PRIMARY KEY | (id) | Unique identifier |
| idx_{name} | INDEX | ({column_list}) | {Purpose} |
| uk_{name} | UNIQUE | ({column_list}) | {Uniqueness constraint} |

#### Foreign Keys

| FK Name | Column | References | On Delete | On Update |
|---------|--------|------------|-----------|-----------|
| fk_{name} | {column} | {parent_table}({column}) | CASCADE | RESTRICT |

#### Relationships

**Parent Tables (This table references):**
- {parent_table} via {column}

**Child Tables (Tables that reference this):**
- {child_table} via {column}

#### DDL (if requested)

```sql
{SHOW CREATE TABLE output}
```

```

---

### Step 6: Type Mapping Documentation

**Generate C# ↔ Database type mapping:**

```markdown
## Type Mapping Reference

### MySQL Types

| C# Type | MySQL Type | Notes |
|---------|------------|-------|
| int | INT | 32-bit integer |
| long | BIGINT | 64-bit integer |
| decimal | DECIMAL(p,s) | Precision and scale specified |
| double | DOUBLE | Floating point |
| string | VARCHAR(n) | Variable length, max n |
| string | TEXT | Large text |
| DateTime | DATETIME | Date and time |
| bool | TINYINT(1) | 0 = false, 1 = true |
| byte[] | BLOB | Binary data |
| Guid | CHAR(36) | UUID format |

### SQL Server Types

| C# Type | SQL Server Type | Notes |
|---------|-----------------|-------|
| int | INT | 32-bit integer |
| long | BIGINT | 64-bit integer |
| decimal | DECIMAL(p,s) | Precision and scale specified |
| double | FLOAT | Floating point |
| string | VARCHAR(n) | Variable length |
| string | NVARCHAR(n) | Unicode variable length |
| DateTime | DATETIME2 | High precision date/time |
| bool | BIT | 0 or 1 |
| byte[] | VARBINARY(MAX) | Binary data |
| Guid | UNIQUEIDENTIFIER | Native GUID type |
```

---

### Step 7: DAO-to-Database Mapping

**Generate complete mapping table:**

```markdown
## DAO-to-Database Component Map

| DAO Class | Method | Stored Procedure | Database | Tables Accessed |
|-----------|--------|------------------|----------|-----------------|
| {DaoName} | {MethodName} | {ProcedureName} | {MySQL/SQL Server} | {TableList} |

### Detailed Flows

#### {DaoName}

**Methods:** {count}

| Method | SP Called | Parameters | Returns | Purpose |
|--------|-----------|------------|---------|---------|
| {GetAllAsync} | {sp_name_getall} | None | `List<{Model}>` | Retrieve all records |
| {GetByIdAsync} | {sp_name_getbyid} | p_id: INT | `{Model}` | Retrieve by ID |
| {InsertAsync} | {sp_name_insert} | {param_list} | INT (new ID) | Create record |
| {UpdateAsync} | {sp_name_update} | {param_list} | VOID | Update record |
| {DeleteAsync} | {sp_name_delete} | p_id: INT | VOID | Delete record |
```

---

### Step 8: Schema Versioning

**Document schema version/migration info:**

```markdown
## Schema Version Information

**Current Schema Version:** {version from migration or DB}
**Last Migration:** {date}
**Migration Tool:** {Entity Framework / FluentMigrator / Manual}

### Recent Schema Changes

| Date | Version | Change Description | Affected Tables |
|------|---------|--------------------|-|
| {date} | {version} | {description} | {tables} |
```

---

### Step 9: Generate Database ERD

**Create Mermaid Entity-Relationship Diagram:**

```markdown
## Entity-Relationship Diagram

```mermaid
erDiagram
    {TABLE1} ||--o{ {TABLE2} : "has many"
    {TABLE1} {
        int id PK
        varchar(50) name
        datetime created_at
    }
    {TABLE2} {
        int id PK
        int {table1}_id FK
        varchar(100) description
    }
```

```

---

### Step 10: Write Documentation

**Save to:**
- `docs/database/{ModuleName}_schema.md`

**File structure:**
```markdown
---
module: {ModuleName}
database_type: {MySQL / SQL Server / Both}
last_analyzed: {CurrentDate}
schema_version: {version}
---

# {ModuleName} - Database Schema Documentation

## Overview
{Summary of database components}

## Stored Procedures ({count})
{Section 4 content}

## Tables ({count})
{Section 5 content}

## Type Mapping
{Section 6 content}

## DAO-to-Database Map
{Section 7 content}

## Schema Versioning
{Section 8 content}

## Entity-Relationship Diagram
{Section 9 content}
```

---

### Step 11: Update Memory

**Update memories.md:**

```markdown
Database schema analyzed: {ModuleName}
- Stored Procedures: {count}
- Tables: {count}
- DAOs: {count}
Documentation: docs/database/{ModuleName}_schema.md
```

---

### Step 12: Completion Message

```
✅ Database schema analysis complete.

Module: {ModuleName}
Database: {MySQL/SQL Server/Both}

Components Documented:
- Stored Procedures: {count}
- Tables: {count}
- DAO Methods: {count}
- Type Mappings: Complete
- ERD: Generated

Documentation: docs/database/{ModuleName}_schema.md

Every table mapped, every procedure documented, every relationship traced.
```

---

**Workflow Version:** 1.0.0  
**Created:** 2026-01-08  
**Status:** Production Ready

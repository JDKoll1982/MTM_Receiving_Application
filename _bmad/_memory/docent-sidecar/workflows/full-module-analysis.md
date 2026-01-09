---
workflow: full-module-analysis
command: AM
description: Generate comprehensive 7-section documentation for entire module
version: 1.0.0
---

# Full Module Analysis Workflow

**Command:** AM (Analyze Module)
**Purpose:** Generate comprehensive 7-section documentation covering all architectural layers

---

## Workflow Execution Steps

### Step 1: Initialize Analysis

**Prompt user for module information:**

```
📚 Docent - Full Module Analysis

Module to analyze: [await user input]
Output location: docs/workflows/{ModuleName}.md (default)
Custom location? [y/n]
```

**Validate module exists:**

- Check if module directory exists
- Verify it contains expected structure (Views/, ViewModels/, Services/, etc.)
- Count initial file inventory

**Status update:**

```
Module located: {ModuleName}
Beginning comprehensive analysis...

✓ File inventory scan
✓ Component discovery
✓ Dependency mapping
```

---

### Step 2: Component Discovery & Inventory

**Scan module structure systematically:**

1. **Views (XAML files):**
   - List all .xaml files
   - Extract UI control names (TextBox, Button, ListView, etc.)
   - Identify x:Bind expressions and command bindings
   - Note any runtime Binding usage (flag as deviation)

2. **ViewModels (C# files inheriting BaseViewModel):**
   - Search for `[ObservableProperty]` attributes → extract property names and FULL generic types
   - Search for `[RelayCommand]` attributes → extract command methods
   - Identify CanExecute conditions
   - Extract constructor dependencies (DI services)
   - Count properties, commands, dependencies

3. **Services (C# files implementing I*Service interfaces):**
   - List all service interfaces in Contracts/Services/
   - Find corresponding implementations
   - Extract public method signatures with complete parameter types
   - Identify DAO dependencies
   - Note async/await patterns

4. **DAOs (C# files with Dao_ prefix):**
   - List all DAO classes
   - Extract method signatures returning Model_Dao_Result
   - Identify stored procedure calls (search for ExecuteStoredProcedureAsync)
   - Map DAOs to database operations

5. **Models:**
   - Categorize by subfolder (Core, Enums, InforVisual, etc.)
   - List enums with their values
   - Identify key data models

6. **Converters:**
   - List all IValueConverter implementations
   - Note conversion purposes

**Component count summary:**

```
Module topology mapped:
- Views: {count}
- ViewModels: {count} 
- Services: {count} interfaces, {count} implementations
- DAOs: {count}
- Models: {count}
- Converters: {count}
```

---

### Step 3: Generate YAML Frontmatter

**Create structured metadata:**

```yaml
---
module_name: {ModuleName}
module_path: {ModulePath}
last_analyzed: {CurrentDate}
last_validated: {CurrentDate}
analyst: Docent v1.0.0
component_counts:
  views: {count}
  viewmodels: {count}
  services: {count}
  daos: {count}
  models: {count}
  converters: {count}
  helpers: {count}
key_workflows:
  - {workflow1}
  - {workflow2}
  - {workflow3}
integration_points:
  - {module1}
  - {module2}
---
```

---

### Step 4: Section 1 - Module Overview

**Generate overview narrative:**

```markdown
# {ModuleName} - Module Documentation

## Module Overview

### Purpose
{Infer purpose from component analysis - authentication, receiving, reporting, etc.}

### Business Value
{Describe what business process this module supports}

### Key Workflows

1. **{Workflow 1}**
   - Entry point: {ViewName}
   - Primary ViewModel: {ViewModelName}
   - Core services: {ServiceNames}
   - Database operations: {DAO → Stored Procedures}

2. **{Workflow 2}**
   ...

### Integration Points

**Dependencies (What this module uses):**
- {Module/Service dependencies}

**Dependents (What uses this module):**
- {Modules that depend on this}

**Events Published:**
- {Event names if any}

**Events Subscribed:**
- {Event subscriptions if any}

### Architecture Compliance

✅ **MTM Constraints Followed:**
- All ViewModels inherit BaseViewModel
- DAOs use stored procedures exclusively  
- XAML uses x:Bind (compile-time binding)
- Error handling via IService_ErrorHandler

⚠️ **Deviations Detected:**
{List any architectural violations found}
```

---

### Step 5: Section 2 - Mermaid Workflow Diagram

**Generate vertical flowchart with layer subgraphs:**

**Complexity check:**

- Count total nodes (UI controls + ViewModel properties/commands + Service methods + DAO methods + DB operations)
- If > 30 nodes: Create user-journey-based diagrams instead of one monolithic diagram

**Diagram structure:**

```markdown
## Mermaid Workflow Diagram

### {Primary Workflow Name}

```mermaid
flowchart TD
    subgraph UI["UI Layer - {ViewName}"]
        Control1[{ControlName}]
        Control2[{ControlName}]
    end
    
    subgraph VM["ViewModel Layer - {ViewModelName}"]
        Prop1[{PropertyName}: {Type}]
        Cmd1[{CommandName}]
    end
    
    subgraph SVC["Service Layer"]
        Method1[{ServiceName}.{MethodName}]
    end
    
    subgraph DAO["DAO Layer"]
        DaoMethod1[{DaoName}.{MethodName}]
    end
    
    subgraph DB["Database Layer"]
        SP1[({StoredProcedureName})]
        Table1[({TableName})]
    end
    
    %% Forward flow (solid arrows)
    Control1 -->|x:Bind Command| Cmd1
    Cmd1 -->|Calls| Method1
    Method1 -->|Calls| DaoMethod1
    DaoMethod1 -->|Executes| SP1
    SP1 -->|INSERT/UPDATE/SELECT| Table1
    
    %% Return flow (dotted arrows)
    Table1 -.->|Result Set| SP1
    SP1 -.->|Model_Dao_Result<T>| DaoMethod1
    DaoMethod1 -.->|Model_Dao_Result<T>| Method1
    Method1 -.->|Updates| Prop1
    Prop1 -.->|x:Bind OneWay| Control2
```

```

**If complexity > 30 nodes:**
"Diagram split into user-journey-based views for clarity. See individual workflow sections below."

---

### Step 6: Section 3 - User Interaction Lifecycle

**For each major UI workflow, create numbered step-by-step walkthrough:**

```markdown
## User Interaction Lifecycle

### Workflow: {WorkflowName}

**Forward Flow (User Action → Database):**

1. **User Action:** User {action description} in `{ControlName}`
   - Control Type: {TextBox/Button/etc.}
   - Binding: `{x:Bind ViewModel.PropertyName, Mode=TwoWay}`

2. **ViewModel Property Update:** `{PropertyName}` set to user input
   - Type: `{FullGenericType}`
   - Validation: {CanExecute conditions if applicable}

3. **Command Execution:** User triggers `{CommandName}`
   - Method: `{CommandMethodName}`
   - Parameters: {List parameters}

4. **Service Layer Call:** `{ServiceName}.{MethodName}()`
   - Business Logic: {Description of validation/transformation}
   - DAO Call: Invokes `{DaoName}.{MethodName}()`

5. **DAO Layer Execution:** `{DaoName}.{MethodName}()`
   - Stored Procedure: `{ProcedureName}`
   - Parameters: {List with types}
   - Return Type: `Model_Dao_Result<{Type}>`

6. **Database Operation:** `{StoredProcedureName}` executes
   - Operation: INSERT/UPDATE/DELETE/SELECT
   - Tables Affected: {TableNames}
   - Result: {What is returned}

**Return Flow (Database → UI Update):**

7. **Stored Procedure Returns:** Result set/scalar value
   - Maps to: `{ModelType}`

8. **DAO Processes Result:** Converts DataRow → Model
   - Returns: `Model_Dao_Result<{Type}>`
   - Success: `IsSuccess = true, Data = {object}`
   - Failure: `IsSuccess = false, ErrorMessage = {string}`

9. **Service Receives Result:** Handles DAO response
   - Success Path: Transforms data if needed, returns to ViewModel
   - Failure Path: Logs error, returns failure result

10. **ViewModel Updates:** Properties updated based on result
    - Property: `{PropertyName}` set to `{value}`
    - Binding: x:Bind automatically updates UI

11. **UI Reflects Changes:** User sees updated data
    - Control: `{ControlName}` displays new value
    - Visual: {Description of UI change}

**Error Paths:**

- **Validation Failure:** CanExecute returns false → Command disabled
- **Service Error:** Service returns failure → ViewModel shows error via `IService_ErrorHandler`
- **DAO Exception:** Caught, wrapped in Model_Dao_Result failure
- **Database Error:** Stored procedure fails → Propagates through layers as failure result
```

---

### Step 7: Section 4 - Code Inventory

**Generate comprehensive component tables:**

```markdown
## Code Inventory

### ViewModels ({count})

#### {ViewModelName}

**Properties ({count}):**

| Property Name | Type | Default Value | Binding Target | Purpose |
|---------------|------|---------------|----------------|---------|
| {Name} | `{FullGenericType}` | {Default} | {UIControl} | {Description} |

**Commands ({count}):**

| Command Name | Signature | CanExecute | Purpose |
|--------------|-----------|------------|-----|
| {Name} | `Task {MethodName}({params})` | `{CanExecuteMethod}` | {Description} |

**Dependencies:**
- `{IServiceName}` - {Purpose}
- `IService_ErrorHandler` - Error handling
- `ILoggingService` - Audit logging

**Pattern Compliance:**
✅ Inherits BaseViewModel
✅ Uses [ObservableProperty]
✅ Uses [RelayCommand]
✅ Constructor calls base(errorHandler, logger)

---

### Services ({count})

#### {ServiceName}

**Interface:** `{IServiceName}`

**Methods:**

| Method | Signature | Returns | Purpose |
|--------|-----------|---------|-----|
| {Name} | `Task<Model_Dao_Result<{T}>> {MethodName}({params})` | `Model_Dao_Result<{Type}>` | {Description} |

**Business Logic Summary:**
{Description of validation, transformation, orchestration}

**DAO Dependencies:**
- `{DaoName}` → Calls `{MethodName}()` for {purpose}

---

### DAOs ({count})

#### {DaoName}

**Database Target:** {MySQL/SQL Server}

**Methods:**

| Method | Stored Procedure | Parameters | Return Type | Purpose |
|--------|------------------|------------|-------------|-----|
| {Name} | `{ProcedureName}` | `{ParamList}` | `Model_Dao_Result<{T}>` | {Description} |

**Pattern Compliance:**
✅ Instance-based class (not static)
✅ All methods async
✅ Returns Model_Dao_Result
✅ Uses Helper_Database_StoredProcedure
✅ Never throws exceptions

---

### Models ({count})

**Core Models:**
- `{ModelName}` - {Purpose}

**Enums:**
- `{EnumName}` - Values: {ListValues}

**InforVisual Models:**
- `{ModelName}` - Maps to Infor Visual {Entity}
```

---

### Step 8: Section 5 - Database Schema Details

**Query and document database structures:**

```markdown
## Database Schema Details

### Stored Procedures ({count})

#### {StoredProcedureName}

**Database:** {mtm_receiving_application / MTMFG}
**Purpose:** {Description}

**Parameters:**

| Parameter | Data Type | Direction | Default | Description |
|-----------|-----------|-----------|---------|----------|
| {p_ParamName} | {INT/VARCHAR(50)/etc} | IN/OUT | {NULL/value} | {Purpose} |

**Logic Summary:**
{High-level description of what procedure does}

**Tables Accessed:**
- `{TableName}` - {SELECT/INSERT/UPDATE/DELETE}

**Returns:** {Scalar value / Result set / OUT parameter}

---

### Tables ({count})

#### {TableName}

**Columns:**

| Column | Data Type | Constraints | Nullable | Description |
|--------|-----------|-------------|----------|----------|
| {ColumnName} | {INT/VARCHAR(50)} | {PK/FK/UNIQUE} | {YES/NO} | {Purpose} |

**Indexes:**
- `{IndexName}` ON ({Columns}) - {UNIQUE/INDEX}

**Foreign Keys:**
- `{FKName}` REFERENCES `{ParentTable}({Column})` ON DELETE {CASCADE/RESTRICT}

---

### Type Mapping: C# ↔ Database

| C# Type | MySQL Type | SQL Server Type | Notes |
|---------|------------|-----------------|-------|
| `int` | INT | INT | |
| `string` | VARCHAR(n) | VARCHAR(n) | |
| `decimal` | DECIMAL(p,s) | DECIMAL(p,s) | |
| `DateTime` | DATETIME | DATETIME2 | |
| `bool` | TINYINT(1) | BIT | |
```

---

### Step 9: Section 6 - Module Dependencies & Integration

```markdown
## Module Dependencies & Integration

### External Dependencies

**NuGet Packages:**
- {PackageName} v{Version} - {Purpose}

**Other Modules:**
- {Module_Name} - {How used}

### Integration Map

**What {ModuleName} Calls:**
- `{ServiceName}` from {ModuleName} → {Purpose}

**What Calls {ModuleName}:**
- `{ModuleName}` → Uses `{ServiceName}` for {Purpose}

### Events

**Published:**
- `{EventName}` - Fired when {condition}

**Subscribed:**
- `{EventName}` from {Source} - Handler: {MethodName}

### Public API Surface

**Services Exposed:**
- `{IServiceName}` - Available to other modules via DI

**Models Shared:**
- `{ModelName}` - Used by {ModuleList}
```

---

### Step 10: Section 7 - Common Patterns & Code Examples

**Extract canonical implementations:**

```markdown
## Common Patterns & Code Examples

### ViewModel Command Pattern

**Standard pattern found in {count} locations:**

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;
        StatusMessage = "Loading...";
        
        var result = await _service.GetDataAsync();
        
        if (result.IsSuccess)
        {
            Items.Clear();
            foreach (var item in result.Data)
                Items.Add(item);
            StatusMessage = $"Loaded {Items.Count} items";
        }
        else
        {
            _errorHandler.ShowUserError(
                result.ErrorMessage, 
                "Load Failed", 
                nameof(LoadDataAsync)
            );
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(
            ex, 
            Enum_ErrorSeverity.Medium, 
            nameof(LoadDataAsync), 
            nameof({ViewModelName})
        );
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Service Method Pattern

**Standard pattern found in {count} services:**

```csharp
public async Task<Model_Dao_Result<int>> SaveAsync({ModelName} entity)
{
    // Validation
    if (string.IsNullOrWhiteSpace(entity.Name))
    {
        return new Model_Dao_Result<int>
        {
            IsSuccess = false,
            ErrorMessage = "Name is required"
        };
    }

    // Insert or Update
    if (entity.Id == 0)
    {
        await _logger.LogInformationAsync($"Inserting new entity: {entity.Name}");
        return await _dao.InsertAsync(entity);
    }
    else
    {
        await _logger.LogInformationAsync($"Updating entity ID {entity.Id}");
        var updateResult = await _dao.UpdateAsync(entity);
        return new Model_Dao_Result<int>
        {
            IsSuccess = updateResult.IsSuccess,
            ErrorMessage = updateResult.ErrorMessage,
            Data = entity.Id
        };
    }
}
```

### DAO Stored Procedure Call Pattern

**Standard pattern found in {count} DAOs:**

```csharp
public async Task<Model_Dao_Result<List<{ModelName}>>> GetAllAsync()
{
    try
    {
        var parameters = new List<MySqlParameter>();
        
        var result = await Helper_Database_StoredProcedure
            .ExecuteStoredProcedureAsync("{ProcedureName}", parameters);

        if (!result.IsSuccess)
        {
            return new Model_Dao_Result<List<{ModelName}>>
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        var entities = new List<{ModelName}>();
        foreach (DataRow row in result.Data.Rows)
        {
            entities.Add(MapFromDataRow(row));
        }

        return new Model_Dao_Result<List<{ModelName}>>
        {
            IsSuccess = true,
            Data = entities
        };
    }
    catch (Exception ex)
    {
        return new Model_Dao_Result<List<{ModelName}>>
        {
            IsSuccess = false,
            ErrorMessage = $"Error retrieving entities: {ex.Message}"
        };
    }
}
```

### Getting Started (Onboarding)

**To work with {ModuleName}:**

1. **Understand the workflow:** Review Section 3 (User Interaction Lifecycle)
2. **Find the entry point:** {ViewName} is the main UI
3. **Follow the pattern:** Use canonical patterns from Section 7
4. **Check constraints:** All database operations use stored procedures
5. **Test thoroughly:** Ensure MVVM separation is maintained

```

---

### Step 11: Optional Sections

**Conditional section generation:**

**Section 8: Application Settings** (if appsettings.json or Model_AppSettings found)
**Section 9: Architecture Decision Records** (if ADR files found)
**Section 10: Known Issues / Technical Debt** (if TODO/FIXME comments found)
**Section 11: Test Coverage Summary** (if test projects found)

---

### Step 12: Generate Table of Contents

**Auto-generate TOC with anchor links:**

```markdown
## Table of Contents

1. [Module Overview](#module-overview)
2. [Mermaid Workflow Diagram](#mermaid-workflow-diagram)
3. [User Interaction Lifecycle](#user-interaction-lifecycle)
4. [Code Inventory](#code-inventory)
   - [ViewModels](#viewmodels-count)
   - [Services](#services-count)
   - [DAOs](#daos-count)
   - [Models](#models-count)
5. [Database Schema Details](#database-schema-details)
6. [Module Dependencies & Integration](#module-dependencies--integration)
7. [Common Patterns & Code Examples](#common-patterns--code-examples)
{Optional sections}
```

---

### Step 13: Write Documentation File

**Save to output location:**

- Default: `docs/workflows/{ModuleName}.md`
- Custom: {UserSpecifiedPath}

**File structure:**

```
{YAML Frontmatter}

{Table of Contents}

{Section 1: Module Overview}

{Section 2: Mermaid Diagram}

{Section 3: User Interaction Lifecycle}

{Section 4: Code Inventory}

{Section 5: Database Schema}

{Section 6: Dependencies & Integration}

{Section 7: Common Patterns}

{Optional Sections}
```

---

### Step 14: Update Memory

**Add to memories.md Analyzed Modules Registry:**

```markdown
| {ModuleName} | docs/workflows/{ModuleName}.md | {CurrentDate} | {CurrentDate} | ✅ Current | Initial analysis complete |
```

**Update Discovered Patterns Library** (if new patterns found)

---

### Step 15: Completion Message

```
✅ Module topology fully documented.

Module: {ModuleName}
Components analyzed: {TotalCount}
- Views: {count}
- ViewModels: {count}
- Services: {count}
- DAOs: {count}

Documentation: {OutputPath}
Sections: 7 core + {optional} optional

Every detail documented, every connection mapped, every pattern preserved.
```

---

**Workflow Version:** 1.0.0  
**Created:** 2026-01-08  
**Status:** Production Ready

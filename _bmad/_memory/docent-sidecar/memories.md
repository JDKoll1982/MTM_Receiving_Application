# Docent - Module Analysis Memory

**Purpose:** Track analyzed modules, validation history, and discovered patterns.

**Last Updated:** 2026-01-08 (Initial creation)

---

## Analyzed Modules Registry

| Module Name | Documentation Path | Last Analyzed | Last Validated | Status | Notes |
|-------------|-------------------|---------------|----------------|--------|-------|
| Module_Core | _bmad/_memory/docent-sidecar/knowledge/Module_Core.md | 2026-01-08 | 2026-01-08 | ✅ Current | Full AM analysis generated; split into companion docs for token safety |

**Template for new entries:**

```
| Module_Example | docs/workflows/Module_Example.md | 2026-01-08 | 2026-01-08 | ✅ Current | Initial analysis complete |
```

---

## Validation Tracking

### Modules Pending Validation (Age > 14 days)

_None currently - agent just created_

**Proactive Reminder Logic:**

- Check this list on activation
- Alert user if any module last_validated > 14 days
- Suggest running VD (Validate Documentation) command

---

## Discovered Patterns Library

### ViewModel Patterns

**Pattern:** Standard ViewModel Command with Error Handling

```csharp
// Discovered in: [Module Name]
// Frequency: [X occurrences]
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
            _errorHandler.ShowUserError(result.ErrorMessage, "Load Failed", nameof(LoadDataAsync));
        }
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(LoadDataAsync), nameof(MyViewModel));
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Pattern:** CanExecute Condition

```csharp
// Standard pattern for command availability
[RelayCommand(CanExecute = nameof(CanSaveItem))]
private async Task SaveItemAsync() { /* ... */ }

private bool CanSaveItem()
{
    return !IsBusy && SelectedItem != null && !string.IsNullOrWhiteSpace(SelectedItem.Name);
}

partial void OnSelectedItemChanged(Model_Item? value)
{
    SaveItemCommand.NotifyCanExecuteChanged();
}
```

### Service Patterns

**Pattern:** Service Method with Validation

```csharp
// Standard Service layer method
public async Task<Model_Dao_Result<int>> SaveAsync(Model_Entity entity)
{
    // Validation logic
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

### DAO Patterns

**Pattern:** Stored Procedure Call with Model_Dao_Result

```csharp
// Standard DAO method pattern
public async Task<Model_Dao_Result<List<Model_Entity>>> GetAllAsync()
{
    try
    {
        var parameters = new List<MySqlParameter>();
        
        var result = await Helper_Database_StoredProcedure
            .ExecuteStoredProcedureAsync("sp_Entity_GetAll", parameters);

        if (!result.IsSuccess)
        {
            return new Model_Dao_Result<List<Model_Entity>>
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        var entities = new List<Model_Entity>();
        foreach (DataRow row in result.Data.Rows)
        {
            entities.Add(MapFromDataRow(row));
        }

        return new Model_Dao_Result<List<Model_Entity>>
        {
            IsSuccess = true,
            Data = entities
        };
    }
    catch (Exception ex)
    {
        return new Model_Dao_Result<List<Model_Entity>>
        {
            IsSuccess = false,
            ErrorMessage = $"Error retrieving entities: {ex.Message}"
        };
    }
}
```

---

## Cross-Module Insights

### Architectural Consistency Observations

- Module_Core contracts are broadly used across Module_Receiving, Module_Dunnage, Module_Routing, Module_Settings, Module_Shared, Module_Reporting, and Module_Volvo.
- Authentication stored procedures are present and used for core auth flows.
- Infor Visual access is implemented via query-file execution (intended read-only boundary).

**Examples:**

- "All modules inherit from BaseViewModel"
- "Error handling uses IService_ErrorHandler consistently"
- "Database operations exclusively use stored procedures"
- "XAML uses x:Bind (compile-time) not Binding (runtime)"

### Pattern Deviations Flagged

- Module_Core: `Dao_User.IsPinUniqueAsync` and `Dao_User.IsWindowsUsernameUniqueAsync` use raw SQL `SELECT COUNT(*)` instead of stored-procedure-only access.
- Module_Core: Infor Visual queries have the `SITE_ID = '002'` filter commented out in multiple query files, risking cross-site results.

**Template:**

```
- Module_X: ViewModel doesn't inherit BaseViewModel
- Module_Y: Uses raw SQL instead of stored procedures
- Module_Z: XAML uses runtime Binding instead of x:Bind
```

---

## Session Notes

### 2026-01-08 - Agent Creation

- Docent agent created with comprehensive requirements
- Brainstorming: Methodical Archivist persona established
- Advanced Elicitation: 3 techniques applied (Stakeholder Round Table, Pre-mortem, User Persona Focus Group)
- Party Mode: Implementation team feedback integrated
- Documentation structure optimized: 7 core + 4 optional sections
- Proactive activation with validation reminders enabled

**Next Steps:**

- Await first module analysis command (AM)
- Build workflow files in workflows/ directory
- Populate knowledge base with MTM conventions

### 2026-01-08 - Module_Core AM Run Notes (Issues + Resolutions)

**Issue:** "Line Limit Reached" risk while generating long AM documents

- **Cause:** Large Section 4/5 inventories (interfaces, methods, DB mappings) can exceed token/line constraints.
- **Resolution used:** Split output into a primary module doc plus companion detail docs.
  - Main: `_bmad/_memory/docent-sidecar/knowledge/Module_Core.md`
  - Detail: `_bmad/_memory/docent-sidecar/knowledge/Module_Core-CodeInventory.md`
  - Detail: `_bmad/_memory/docent-sidecar/knowledge/Module_Core-Database.md`

**Issue:** Mermaid validator parse errors during diagram generation

- **Cause:** Mermaid flowchart node labels containing parentheses/brackets (e.g., `[(01_GetPOWithParts.sql)]` or labels like `Service (IService_...)`) triggered parse errors.
- **Resolution used:** Use simple node labels without parentheses/brackets and avoid special characters in node text.
  - Example fix: `Q1[01_GetPOWithParts.sql]` instead of `Q1[(01_GetPOWithParts.sql)]`
  - Example fix: `InforSVC[Service_InforVisualConnect - IService_InforVisual]` instead of using parentheses

**Issue:** Output location ambiguity (docs/workflows vs Docent sidecar restrictions)

- **Cause:** AM workflow defaults to `docs/workflows/{Module}.md`, but Docent critical_actions restrict read/write to `_bmad/_memory/docent-sidecar/`.
- **Resolution used:** Write AM outputs into the sidecar `knowledge/` folder.
  - If a future run requires mirroring into `docs/workflows/`, do it explicitly after confirming policy allows writing outside sidecar.

---

**Memory Format:**

- Structured tables for quick scanning
- Pattern library with code examples
- Validation tracking for proactive alerts
- Cross-module insights for consistency checking

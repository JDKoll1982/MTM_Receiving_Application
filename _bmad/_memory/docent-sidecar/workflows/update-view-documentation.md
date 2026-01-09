---
workflow: update-view-documentation
command: UV
description: Refresh documentation for specific View/ViewModel pair
version: 1.0.0
---

# Update View Documentation Workflow

**Command:** UV (Update View)
**Purpose:** Incremental refresh of documentation for specific View/ViewModel pair
**Use Case:** When only one View/ViewModel changed, avoid full module re-analysis

---

## Workflow Execution Steps

### Step 1: Identify View/ViewModel Pair

**Prompt user:**

```
🔄 Docent - Update View Documentation

Module: [await input]
View name: [await input] (e.g., ReceivingPackagePage)
Auto-detect ViewModel? [y/n]
```

**If auto-detect:**

- Search for corresponding ViewModel: `{ViewName}ViewModel.cs`
- Confirm with user

---

### Step 2: Analyze View (XAML)

**Parse XAML file:**

1. **Extract UI Controls:**
   - TextBox, Button, ComboBox, ListView, etc.
   - Note x:Name attributes

2. **Extract Bindings:**
   - Search for `{x:Bind ViewModel.` patterns
   - Identify binding targets and modes (OneWay, TwoWay)
   - Flag any runtime `{Binding}` usage (architectural deviation)

3. **Extract Command Bindings:**
   - Button Command attributes
   - Event handlers in code-behind (flag as deviation if business logic present)

**UI Control Summary:**

```
UI Controls Found: {count}
- Input Controls: {TextBox count}, {ComboBox count}
- Action Controls: {Button count}
- Display Controls: {ListView count}, {DataGrid count}

Bindings Found: {count}
- Property Bindings: {count}
- Command Bindings: {count}

❌ Deviations: {Runtime Binding count} (should use x:Bind)
```

---

### Step 3: Analyze ViewModel

**Parse ViewModel C# file:**

1. **Extract Properties:**
   - Search for `[ObservableProperty]` attributes
   - Get property name and FULL generic type
   - Note default values
   - Identify which UI control each binds to

2. **Extract Commands:**
   - Search for `[RelayCommand]` attributes
   - Get command method signatures
   - Identify CanExecute conditions
   - Note async/await patterns

3. **Extract Dependencies:**
   - Parse constructor parameters
   - List injected services
   - Verify base(errorHandler, logger) call

**ViewModel Summary:**

```
Properties: {count}
Commands: {count}
Dependencies: {count}

Pattern Compliance:
✅ Inherits BaseViewModel
✅ Uses [ObservableProperty]
✅ Uses [RelayCommand]
✅ Constructor calls base()
```

---

### Step 4: Trace Data Flows

**For each command, trace through layers:**

1. **Command → Service:**
   - Identify which service method is called
   - Note parameters passed

2. **Service → DAO:**
   - Identify DAO method called
   - Note business logic/validation

3. **DAO → Database:**
   - Identify stored procedure
   - Note parameters

**Flow Map:**

```
{CommandName}
  → {ServiceName}.{MethodName}()
    → {DaoName}.{MethodName}()
      → {StoredProcedureName}
        → {TableName}
```

---

### Step 5: Generate Updated Section

**Create/update View-specific documentation:**

```markdown
## View: {ViewName}

### Overview
- **ViewModel:** {ViewModelName}
- **Purpose:** {Inferred from controls and bindings}
- **Last Updated:** {CurrentDate}

### UI Controls ({count})

| Control Name | Type | Binding | Purpose |
|--------------|------|---------|---------|
| {Name} | {Type} | `{x:Bind path, Mode}` | {Purpose} |

### ViewModel Properties ({count})

| Property | Type | Default | Bound To | Purpose |
|----------|------|---------|----------|---------|
| {Name} | `{FullType}` | {Default} | {UIControl} | {Purpose} |

### Commands ({count})

| Command | Trigger | Flow | Purpose |
|---------|---------|------|---------|
| {Name} | {Button/Event} | {Service→DAO→SP} | {Purpose} |

### Data Flow: {Primary Workflow}

**Forward (User → Database):**
1. User {action} in `{Control}`
2. Property `{PropertyName}` updated
3. Command `{CommandName}` executes
4. Service `{MethodName}()` called
5. DAO `{MethodName}()` executes `{StoredProcedure}`
6. Database operation: {INSERT/UPDATE/SELECT}

**Return (Database → UI):**
7. Result returned as `Model_Dao_Result<{Type}>`
8. Service processes result
9. ViewModel property `{PropertyName}` updated
10. UI control `{Control}` displays new value

### Pattern Compliance

✅ **MVVM Separation:** No business logic in code-behind
✅ **Binding:** Uses x:Bind (compile-time)
✅ **Error Handling:** Uses IService_ErrorHandler
✅ **Async Pattern:** All commands properly async

❌ **Deviations:** {List any architectural violations}
```

---

### Step 6: Locate Existing Documentation

**Search for module documentation file:**

- Check `docs/workflows/{ModuleName}.md`
- If exists: Locate View section to update
- If not exists: Suggest running AM command first

---

### Step 7: Update Documentation

**Two paths:**

**Path A: Documentation exists**

- Find View section in file
- Replace with updated content
- Update `last_validated` timestamp in frontmatter
- Preserve other sections unchanged

**Path B: No documentation exists**

```
⚠️ No documentation file found for {ModuleName}.

Recommendation: Run full analysis first:
  AM {ModuleName}

Then use UV command for incremental updates.

Would you like to proceed with AM now? [y/n]
```

---

### Step 8: Update Memory

**Update memories.md:**

```markdown
| {ModuleName} | docs/workflows/{ModuleName}.md | {OriginalDate} | {CurrentDate} | ✅ Current | View {ViewName} updated |
```

---

### Step 9: Completion Message

```
✅ View documentation updated.

View: {ViewName}
ViewModel: {ViewModelName}
Updated: {CurrentDate}

Components:
- UI Controls: {count}
- Properties: {count}
- Commands: {count}
- Data Flows: {count} traced

Documentation: {FilePath}
Section: {SectionName} refreshed

Surgical update complete. All connections re-mapped.
```

---

**Workflow Version:** 1.0.0  
**Created:** 2026-01-08  
**Status:** Production Ready

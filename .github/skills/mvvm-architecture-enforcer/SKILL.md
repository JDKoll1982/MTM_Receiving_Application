---
name: mvvm-architecture-enforcer
description: Enforces MVVM architecture rules for WinUI 3 applications in this repo. Use when creating or modifying ViewModels, Services, DAOs, or XAML bindings. Prevents ViewModels from calling DAOs or Helper_Database_* directly, requires partial ViewModels inheriting from ViewModel_Shared_Base, enforces Service layer usage, and identifies runtime {Binding} so it can be converted to {x:Bind}.
license: MIT
---

# MVVM Architecture Enforcer

This skill enforces the MTM Receiving Application architecture rules.

## When to Use This Skill

Use this skill when you are asked to:
- Create or modify a ViewModel, Service, or DAO
- Review code for architecture violations
- Update XAML bindings
- Add or refactor database access logic

## Architecture Rules (Must Follow)

### Forbidden

1. ViewModels calling DAOs directly (must go through Services)
2. ViewModels accessing `Helper_Database_*` classes
3. Static DAO classes
4. DAOs throwing exceptions
5. MySQL raw SQL in C# (stored procedures only)
6. SQL Server / Infor Visual write operations (read-only)
7. Runtime `{Binding}` in XAML (use `{x:Bind}` only)
8. Business logic in `.xaml.cs` code-behind

### Required

1. Layer flow: View (XAML) → ViewModel → Service → DAO → Database
2. ViewModels are `partial` and inherit from `ViewModel_Shared_Base`
3. Services are interface-based (`IService_*`) and injected
4. DAOs are instance-based, injected, and return `Model_Dao_Result`
5. XAML uses `{x:Bind}` with explicit `Mode`

## Quick Validation Checklist

- [ ] ViewModel is `partial`
- [ ] ViewModel inherits from `ViewModel_Shared_Base`
- [ ] ViewModel calls only Services, not DAOs or `Helper_Database_*`
- [ ] Service interface exists and is registered in DI
- [ ] DAO is instance-based, returns `Model_Dao_Result`, and does not throw
- [ ] XAML uses `{x:Bind}` (not `{Binding}`)

## How to Run the Validator

### Basic Usage (from repo root)
```powershell
.\.github\skills\mvvm-architecture-enforcer\scripts\validate-mvvm.ps1
```

### Advanced Options
```powershell
# Scan a specific directory
.\.github\skills\mvvm-architecture-enforcer\scripts\validate-mvvm.ps1 -Path "Module_Dunnage"

# Get structured output (for automation)
.\.github\skills\mvvm-architecture-enforcer\scripts\validate-mvvm.ps1 -PassThru

# Enable verbose logging
.\.github\skills\mvvm-architecture-enforcer\scripts\validate-mvvm.ps1 -Verbose
```

### Understanding the Output

**No violations:**
```
No MVVM violations found.
```

**With violations:**
```
WARNING: Found 2 MVVM violation(s).
Module_Dunnage\ViewModels\ViewModel_Example.cs: ViewModel references Dao_* directly (forbidden)
Module_Dunnage\Views\View_Example.xaml: XAML uses {Binding} outside allowed collection control context (forbidden; use {x:Bind})
```

**Violation types:**
- `ViewModel references Dao_* directly (forbidden)` - ViewModel is calling a DAO instead of a Service
- `ViewModel references Helper_Database_* (forbidden)` - ViewModel is using database helpers directly
- `ViewModel is missing 'partial class'` - ViewModel class is not declared as partial
- `XAML uses {Binding} outside allowed collection control context` - XAML is using runtime binding where compiled binding should be used

**Allowed exceptions for `{Binding}`:**
- Inside DataGrid columns and templates
- Inside ItemsControl, ItemsRepeater, ListView, GridView templates  
- ElementName bindings (e.g., `{Binding ElementName=Root, Path=...}`)
- RelativeSource bindings (e.g., `{Binding RelativeSource={RelativeSource Self}}`)

## Resource Links

- Validator script: [validate-mvvm.ps1](./scripts/validate-mvvm.ps1)
- Detailed rules: [architecture-rules.md](./references/architecture-rules.md)
- Fix examples: [common-violations.md](./references/common-violations.md)
- ViewModel scaffold: [viewmodel-template.cs](./templates/viewmodel-template.cs)
- Service scaffold: [service-template.cs](./templates/service-template.cs)

## Workflow: Audit MVVM violations

1. Identify the target files (ViewModel `.cs` and XAML `.xaml`).
2. Run [validate-mvvm.ps1](./scripts/validate-mvvm.ps1) over the repo or module.
3. Summarize violations by file.
4. Propose fixes that preserve the required layer flow.
5. Apply fixes with minimal changes.
6. Re-run the validation script.

## Workflow: Create a new ViewModel (repo-standard)

1. Start from [viewmodel-template.cs](./templates/viewmodel-template.cs).
2. Ensure the class is `partial` and inherits from `ViewModel_Shared_Base`.
3. Inject Services (never DAOs) into the constructor.
4. Use `[ObservableProperty]` for bindable state.
5. Use `[RelayCommand]` for UI actions.
6. Ensure async commands end with `Async`.
7. Ensure corresponding XAML uses `{x:Bind}` with explicit `Mode`.

## Workflow: Fix XAML binding violations

1. Search for `{Binding ...}`.
2. Convert to `{x:Bind ...}`.
3. Add explicit `Mode` (OneWay/TwoWay/OneTime).
4. For TwoWay text entry, set `UpdateSourceTrigger=PropertyChanged` when appropriate.


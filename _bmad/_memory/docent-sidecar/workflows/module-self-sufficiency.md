---
workflow: module-self-sufficiency
command: MS
description: Validate whether a module is removable (self-sufficient) and produce ALL remediation steps (including cross-module edits)
version: 1.0.0
---

# Module Self-Sufficiency Workflow

**Command:** MS (Module Self-Sufficiency)
**Purpose:** Determine if removing `Module_<X>/` breaks build/runtime, and if so, produce a complete, file-scoped remediation plan.

---

## Definition: “Self-Sufficient Module” (Removal Readiness)

A module `Module_<X>/` is **self-sufficient** if *removing the entire folder* does **not**:

1. **Break build** (no compile errors due to missing types/resources)
2. **Break runtime startup/navigation** (no crashes due to missing DI registrations, navigation routes, or XAML resources)
3. **Leave dangling integration points** (menu items, routing constants, settings screens, or shared UI that still references the removed module)

**Important nuance:** A module can be functionally “important” and still be architecturally self-sufficient. This workflow measures *removability*, not business value.

---

## Workflow Execution Steps

### Step 1: Identify Target Module

Prompt user:

```
🧩 Docent - Module Self-Sufficiency

Module folder name (e.g., Module_Receiving): [await input]
Validation mode: [Static Only / Static + Optional Build Simulation]
```

Resolve:

- `ModuleName` (e.g., `Module_Receiving`)
- `ModulePath` (e.g., `{project-root}/Module_Receiving/`)

---

### Step 2: Static Dependency Scan (Required)

Perform these scans and record concrete evidence (file + symbol + reference snippets):

#### 2.1 Cross-Module Code References

Search the entire repo (excluding the module folder) for:

- `Module_Receiving` (module folder name)
- `MTM_Receiving_Application.Module_Receiving` (namespace patterns)
- View/ViewModel types (e.g., `ViewModel_Receiving_`, `View_Receiving_`)

Classify each hit as:

- **Hard dependency (must fix):** compile-time reference to a type/resource in the module
- **Soft dependency (should fix):** string-based navigation key, feature flag, settings entry
- **Benign:** documentation-only references

#### 2.2 DI Container Registrations

Locate registrations that point into the module (commonly `App.xaml.cs`):

- `services.AddSingleton/AddTransient/AddScoped<...>`
- any `GetService<...>` usage

Determine whether removing the module will cause DI resolution failures.

#### 2.3 Navigation / Routing / Menu Registration

Search for:

- navigation routes that reference module ViewModels
- shell menus / navigation items that point to Views in the module
- any “route name → ViewModel type” dictionaries

#### 2.4 XAML Resource References

Search for:

- `ResourceDictionary` merges pointing to module XAML
- `x:Class` usages referenced from outside the module
- Styles/templates residing in the module but consumed elsewhere

#### 2.5 Shared Types & Assets

Identify models/enums/interfaces/helpers located in the module that are referenced elsewhere.
If referenced externally, the module is not self-sufficient until those types are:

- moved to `Module_Core/` or a shared module
- or replaced by an existing shared equivalent

---

### Step 3: Optional Build Simulation (Recommended)

If the user selected “Static + Optional Build Simulation”, perform a safe simulation:

- Option A (preferred): temporarily exclude the module from compilation via project configuration (if supported)
- Option B: temporarily rename folder `Module_<X>` → `Module_<X>__REMOVED` and run build
  - restore the name immediately after

Record build errors as “hard dependencies” and translate each into remediation steps.

---

## Output Format (Mandatory)

Produce this sectioned report every time:

### 1) Self-Sufficiency Verdict

- **Verdict:** ✅ Self-sufficient / ❌ Not self-sufficient / ⚠️ Inconclusive
- **Mode:** Static Only / Static + Build Simulation
- **Primary blockers:** {short list}

### 2) Break Risks (If Removed)

- **Build-time failures:** {list}
- **Runtime crash risks:** {list}
- **Functional regressions:** {list}

### 3) Dependency Findings (Evidence)

Group by dependency type:

- **DI registrations:** {file → lines/snippets}
- **Navigation/menu:** {file → references}
- **XAML/resources:** {file → references}
- **Cross-module type usage:** {file → type}

### 4) Remediation Plan (ALL STEPS)

This must be exhaustive and grouped by impacted file/module:

- **Remove/guard registrations** (e.g., in `App.xaml.cs`)
- **Remove/guard navigation routes and menu entries**
- **Move shared types/assets out of the module** (identify destination module and update namespaces)
- **Replace direct references** with shared abstractions or interfaces
- **Update documentation** to reflect new module boundaries

For each step include:

- **File** to edit
- **Exact change** (what to remove/add/replace)
- **Why** (what breaks if not done)

### 5) Re-Validation Steps

- Run `dotnet build` (x64)
- Launch app and smoke-test navigation paths that previously referenced the module

---

## Notes

- This workflow favors being *complete over confident*.
- If you cannot conclusively prove a dependency is safe, mark it ⚠️ and propose a validation action.

---
workflow: module-self-sufficiency
command: MS
module_name: Module_Receiving
analysis_date: 2026-01-10
mode: Static Only
verdict: Not self-sufficient
---

# Module Self-Sufficiency Report — Module_Receiving

## 1) Self-Sufficiency Verdict

- **Verdict:** ❌ Not self-sufficient
- **Mode:** Static Only
- **Primary blockers (hard dependencies):**
  - Root shell navigation directly references `Module_Receiving.Views.View_Receiving_Workflow`.
  - DI container registers Receiving DAOs, Services, and ViewModels.
  - Other modules (Core/Dunnage/Settings) import `MTM_Receiving_Application.Module_Receiving.Models` and/or `...Data`.

**Validation status (2026-01-10):**

- ✅ Build succeeds when `Module_Receiving/` is present.
- ❌ Build fails when `Module_Receiving/` is removed (compile-time hard dependencies confirmed).

## 2) Break Risks (If Removed)

- **Build-time failures:**
  - Missing `Module_Receiving.*` namespaces/types used in `App.xaml.cs`, `MainWindow.xaml.cs`, multiple Module_Core contracts/services/helpers, Module_Dunnage DAOs/services/ViewModels, and Module_Settings services/interfaces.
- **Runtime crash risks (even if you “stub compile”):**
  - Startup navigation attempts to navigate to the Receiving workflow view.
  - NavigationView menu item still points to a Receiving tag/route.
- **Functional regressions:**
  - All Receiving label workflow functionality disappears.
  - Any shared model types currently housed in Module_Receiving (e.g., user preferences and workflow result models) would break other workflows unless migrated.

## 3) Dependency Findings (Evidence)

### Build evidence (module removed)

When `Module_Receiving/` is absent, rebuild fails immediately with missing namespace/type errors (examples):

- `App.xaml.cs`: `CS0234` — missing `MTM_Receiving_Application.Module_Receiving` namespace.
- `Module_Core/Contracts/Services/*`: `CS0246` — missing Receiving models and workflow result types.
- `Module_Core/Services/Database/*`: `CS0246` — missing Receiving DAOs/models.
- `Module_Dunnage/*`: `CS0246` — missing shared workflow/result models sourced from Module_Receiving.

Captured output (full log):

- `_bmad/_memory/docent-sidecar/logs/devenv-rebuild-without-Module_Receiving.log`

> Note: the earlier generic `MSB3073: XamlCompiler.exe exited with code 1` symptom is consistent with upstream C# compilation failures; in the captured log, the dominant failures are C# compiler errors rather than WinUI `WMC*` XAML diagnostics.

### DI registrations

- `App.xaml.cs`
  - Registers Receiving DAOs (`Dao_ReceivingLoad`, `Dao_ReceivingLine`, `Dao_PackageTypePreference`).
  - Registers Receiving services (`IService_ReceivingValidation`, `IService_ReceivingWorkflow`, etc.).
  - Registers Receiving ViewModels (`ViewModel_Receiving_*`).

### Navigation / menu

- `MainWindow.xaml`
  - NavigationView includes a “Receiving Labels” entry with tag `ReceivingWorkflowView`.
- `MainWindow.xaml.cs`
  - Startup activation navigates to the receiving workflow page.
  - Navigation selection handler navigates to the receiving workflow page.
  - Navigation handler casts `ContentFrame.Content` to `Module_Receiving.Views.View_Receiving_Workflow`.

### Cross-module type usage

- `Module_Settings`
  - `Service_UserPreferences` and `IService_UserPreferences` import Receiving models (`Model_UserPreference`).
- `Module_Dunnage`
  - DAOs/services/ViewModels import Receiving models.
- `Module_Core`
  - Multiple service contracts import Receiving models (`IService_ReceivingWorkflow`, `IService_InforVisual`, etc.).
  - Database helper imports Receiving models (`Helper_Database_StoredProcedure`).
  - Core DAOs/services import Receiving models (`Dao_User`, `Service_MySQL_Receiving`, etc.).

## 4) Remediation Plan (ALL STEPS)

> Note: This is a *removal* remediation plan. If the goal is not removal but clearer boundaries, prefer a “shared types extraction” refactor instead.

### A) Remove/guard root navigation

- **Edit** `MainWindow.xaml`
  - Remove or conditionally hide the `NavigationViewItem` with `Tag="ReceivingWorkflowView"`.
  - **Why:** Prevents user from selecting a dead route.

- **Edit** `MainWindow.xaml.cs`
  - Remove/guard all `ContentFrame.Navigate(typeof(Module_Receiving.Views.View_Receiving_Workflow))` calls.
  - Remove/guard the `ContentFrame.Content is Module_Receiving.Views.View_Receiving_Workflow` cast block.
  - **Why:** These are compile-time dependencies and startup/runtime crash points.

### B) Remove Receiving DI registrations

- **Edit** `App.xaml.cs`
  - Remove `using MTM_Receiving_Application.Module_Receiving.*` imports.
  - Remove Receiving DAO registrations (`Dao_ReceivingLoad`, `Dao_ReceivingLine`, `Dao_PackageTypePreference`).
  - Remove Receiving service registrations (`IService_ReceivingValidation`, `IService_ReceivingWorkflow`, `IService_MySQL_ReceivingLine`, etc.).
  - Remove Receiving ViewModel registrations (`ViewModel_Receiving_*`).
  - **Why:** DI will otherwise fail at startup and compile-time types will be missing.

### C) Resolve shared-type leakage (required if keeping other modules)

- **Edit** files in `Module_Core/**`, `Module_Dunnage/**`, and `Module_Settings/**` that import `MTM_Receiving_Application.Module_Receiving.Models`.
  - **Option 1 (preferred):** Move genuinely shared models out of Module_Receiving into an appropriate shared location (typically `Module_Core/Models/...` or `Module_Shared/Models/...`), then update namespaces/usings everywhere.
    - Example candidate already used by Settings: `Model_UserPreference`.
  - **Option 2:** If the referenced models are only needed for Receiving, remove/replace those usages in other modules.
  - **Why:** Otherwise removing Module_Receiving breaks compilation across the solution.

### D) Reconcile Core contracts that depend on Receiving models

- **Edit** `Module_Core/Contracts/Services/*.cs`
  - Remove Receiving model dependencies by:
    - moving shared DTOs/results into Core/Shared, or
    - redefining contracts to use Core-owned types.
  - **Why:** Core contracts should not require a feature module to exist.

### E) Documentation updates

- Update module docs to reflect removal or the extracted shared-types boundary.

## 5) Re-Validation Steps

1. Build x64: `dotnet build /p:Platform=x64 /p:Configuration=Debug`
2. Launch app and verify:
   - App starts without navigating to Receiving.
   - Navigation menu does not expose Receiving entry.
   - Dunnage/Settings flows still work (if those modules are intended to remain).

### Last validation runs

- 2026-01-10: `Module_Receiving/` restored → `dotnet build` succeeded (x64 Debug).
- 2026-01-10: `Module_Receiving/` removed → `devenv.com /Rebuild Debug|x64` failed with CS0234/CS0246 (see log above).

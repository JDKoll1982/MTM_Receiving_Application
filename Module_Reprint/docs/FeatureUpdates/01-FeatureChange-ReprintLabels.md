# 01-FeatureChange-ReprintLabels

Last Updated: 2026-08-21

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement this feature slice end-to-end: a dedicated Reprint Labels module that lets a user
re-queue Receiving, Dunnage, and Volvo history rows back into the active label queue with
`is_reprint = 1`, replacing the reprint capability previously exposed only inside Receiving
Edit Mode.

## Feature

New top-level `Module_Reprint` with a "Reprint Labels" nav entry above Scanner, a mode-selection
landing page, and per-module history grids that reprint selected rows from history.

## Implementation Order

01

## Why This Runs First

This is a brand-new module with its own data layer, services, views, navigation, and DI
registration. It also removes the old reprint UI from Receiving Edit Mode, so the new module
must land before the removal to avoid a capability gap.

## Confirmed Decisions

- New top-level module `Module_Reprint` with `Contracts`, `Models`, `Services`, `ViewModels`,
  `Views`, and `docs`.
- New sidebar nav item "Reprint Labels" placed directly above "Scanner" (Tag `ReprintLabelsPage`).
- Landing page plus one sub-page per mode (Receiving / Dunnage / Volvo); Back returns to the
  landing page.
- Reprint means re-queueing a history row into the module's active label queue with
  `is_reprint = 1`, identical semantics to the existing Receiving reprint flow.
- Data source is history only; the active `label_data` tables are never edited on this page.
- History grid shows a far-left checkbox column; checked rows are reprinted on Save.
- Rows already queued for reprint render with a red row background and no checkbox.
- Save shows a summary dialog (X queued, Y already queued), clears checkboxes, and stays on the
  page. Duplicate rows are skipped per row; one bad row never aborts the whole batch.
- Volvo reprint uses `volvo_generated_label_history` to `volvo_generated_label_data` only
  (not the `volvo_label_data` pair).
- The reprint feature is removed from Receiving Edit Mode fully (button, command, CanExecute,
  XAML, and IsReprint row highlight). Shared SPs/DAO/service methods the new module reuses stay.
- Wiring uses dedicated reprint contracts per module consumed by `Module_Reprint`.
- Each module reuses its history query/filter shape, extended to expose an `already_queued` flag.
- Schema adds `is_reprint` and new InsertFromHistory SPs for Dunnage and Volvo via an idempotent
  migration. No role gating.

## Current Repo State

- Receiving already has `sp_Receiving_LabelData_InsertFromHistory`, `is_reprint` on
  `receiving_label_data`, `Dao_ReceivingLabelData.InsertFromHistoryAsync`, and
  `IService_MySQL_Receiving.InsertFromHistoryAsync(int)`.
- Receiving Edit Mode currently owns the only reprint UI: `ViewModel_Receiving_EditMode`
  `ReprintFromHistoryAsync` / `CanReprintFromHistory` and a Reprint button in
  `View_Receiving_EditMode.xaml`, plus `ApplyReprintRowBackground` in the code-behind.
- Dunnage and Volvo have no `is_reprint` column and no InsertFromHistory SP; they only have
  ClearToHistory archiving and history tables.
- Nav lives in `MainWindow.xaml` (Scanner item) and `MainWindow.xaml.cs` (`_navRoutes`,
  search destinations, `NavigateToRouteTagAsync`).
- DI registration lives in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  (`AddModuleServices` chain).
- Test project has unit tests under `MTM_Receiving_Application.Tests/Unit/` and SP integration
  tests under `MTM_Receiving_Application.Tests/Integration/Database/StoredProcedures/`.

## Required Outcome

A user can navigate to "Reprint Labels", pick Receiving, Dunnage, or Volvo, search/filter
history, check the rows to reprint, and click "Reprint Selected". Queued rows appear in the
module's active label queue with `is_reprint = 1`, already-queued rows are flagged and
skipped, a summary dialog reports the result, and Receiving Edit Mode no longer shows reprint
controls.

## Primary Change Areas

- Database: migration adds `is_reprint` to Dunnage and Volvo label-data tables; new
  InsertFromHistory SPs; new reprint-history SPs that return an `already_queued` flag;
  ClearToHistory SPs updated to skip reprint rows when archiving.
- Data: DAO reprint-history and InsertFromHistory methods for all three modules.
- Contracts and services: dedicated `IService_Reprint_*` contracts and implementations per
  module, plus shared reprint models and a batch-categorization helper.
- UI: `Module_Reprint` views and viewmodels (landing + shared sub-page).
- Navigation and DI: nav item, route, search terms, and `AddReprintModule` registration.
- Removal: Receiving Edit Mode reprint UI, command, and row-highlight logic.

## Files That Must Change

- `Database/Database_Deployment/Sql_Files/Migrations/10_Migration_reprint_dunnage_volvo.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelData_InsertFromHistory.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelHistory_GetForReprint.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_InsertFromHistory.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelHistory_GetForReprint.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_History_GetForReprint.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelData_ClearToHistory.sql`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_ClearToHistory.sql`
- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Module_Dunnage/Data/Dao_DunnageLabelData.cs`
- `Module_Volvo/Data/IDao_VolvoGeneratedLabelData.cs`
- `Module_Volvo/Data/Dao_VolvoGeneratedLabelData.cs`
- `Module_Receiving/Contracts/IService_MySQL_Receiving.cs`
- `Module_Receiving/Services/Service_MySQL_Receiving.cs`
- `Module_Dunnage/Contracts/IService_MySQL_Dunnage.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`
- `Module_Receiving/Contracts/IService_Reprint_Receiving.cs` (new)
- `Module_Receiving/Services/Service_Reprint_Receiving.cs` (new)
- `Module_Dunnage/Contracts/IService_Reprint_Dunnage.cs` (new)
- `Module_Dunnage/Services/Service_Reprint_Dunnage.cs` (new)
- `Module_Volvo/Contracts/IService_Reprint_Volvo.cs` (new)
- `Module_Volvo/Services/Service_Reprint_Volvo.cs` (new)
- `Module_Core/Models/Reprint/Model_ReprintHistoryRow.cs` (new)
- `Module_Core/Models/Reprint/Model_ReprintHistoryFilter.cs` (new)
- `Module_Core/Models/Reprint/Model_ReprintBatchResult.cs` (new)
- `Module_Core/Helpers/Reprint/Helper_ReprintBatch.cs` (new)
- `Module_Reprint/` (new module: models, viewmodels, views, settings keys, docs)
- `MainWindow.xaml`
- `MainWindow.xaml.cs`
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`
- `Module_Receiving/Views/View_Receiving_EditMode.xaml`
- `Module_Receiving/Views/View_Receiving_EditMode.xaml.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_Reprint/Services/Service_ReprintTests.cs` (new)
- `MTM_Receiving_Application.Tests/Unit/Module_Reprint/ViewModels/ViewModel_Reprint_ModuleBaseTests.cs` (new)
- `MTM_Receiving_Application.Tests/Integration/Database/StoredProcedures/StoredProcedureInitialCoverageIntegrationTests.cs`

## Recommended Target Shape

- One shared abstract base viewmodel `ViewModel_Reprint_ModuleBase` drives all three sub-pages:
  date range + preset buttons, Search By + search text, history grid with selectable rows,
  select-all, reprint, and back commands. Each mode subclasses it to supply its search-by
  options, module display name, history loader, and reprint executor.
- A shared `View_Reprint_ModulePage` renders the grid and footer for every mode; mode-specific
  viewmodels are constructed per navigation.
- A `Model_ReprintSelectableRow` wraps each history row with `IsSelected`, `CanSelect`, and a
  red `RowBackground` when already queued.
- A `Helper_ReprintBatch.Categorize` buckets each single-row SP result into Queued,
  AlreadyQueued, or Failed for the summary.
- Search By is persisted per mode via `IService_SettingsCoreFacade` under category `Reprint`;
  date range is not persisted and defaults to Today.

```csharp
public abstract class ViewModel_Reprint_ModuleBase : ViewModel_Shared_Base
{
    protected abstract string SearchBySettingsKey { get; }
    protected abstract string ModuleDisplayName { get; }
    protected abstract IReadOnlyList<Model_ReprintSearchByOption> BuildSearchByOptions();
    protected abstract Task<Model_Dao_Result<IReadOnlyList<Model_ReprintHistoryRow>>> LoadHistoryAsync(Model_ReprintHistoryFilter filter);
    protected abstract Task<Model_ReprintBatchResult> ExecuteReprintAsync(IReadOnlyList<string> historyIds);

    public event EventHandler? BackRequested;
    public event EventHandler? ReprintCompleted;
}
```

## Implementation Steps

1. Add the migration adding `is_reprint` to `dunnage_label_data` and
   `volvo_generated_label_data`.
2. Add `sp_Dunnage_LabelData_InsertFromHistory` and
   `sp_Volvo_GeneratedLabelData_InsertFromHistory` with duplicate reprint guards.
3. Add the three reprint-history SPs returning history rows plus an `already_queued` flag.
4. Update the two ClearToHistory SPs so reprint rows are not re-archived.
5. Add DAO reprint-history and InsertFromHistory methods for Receiving, Dunnage, and Volvo.
6. Add dedicated `IService_Reprint_*` contracts and services per module.
7. Create `Module_Reprint` models, settings keys, the shared base viewmodel, three mode
   viewmodels, the landing view, and the shared module-page view.
8. Wire navigation (nav item, route, search terms) and DI (`AddReprintModule`).
9. Remove the Receiving Edit Mode reprint UI, command, CanExecute, and row highlight.
10. Add unit tests for the reprint services and the shared base viewmodel, plus SP
    integration tests for the two new InsertFromHistory SPs.

## Task Checklist

- [ ] Migration exists and is idempotent.
- [ ] InsertFromHistory SPs exist for Dunnage and Volvo with duplicate guards.
- [ ] Three reprint-history SPs expose `already_queued`.
- [ ] ClearToHistory SPs skip reprint rows.
- [ ] DAO methods added for all three modules.
- [ ] Dedicated reprint contracts and services exist and are registered in DI.
- [ ] `Module_Reprint` landing and shared sub-page render with checkbox, red row, and footer.
- [ ] Nav item "Reprint Labels" appears above Scanner and navigates to the landing page.
- [ ] Receiving Edit Mode reprint controls are removed.
- [ ] Unit and SP integration tests added and passing.
- [ ] Docs updated (`NextVersionPatchNotes.md`, this file).

## Validation

- `dotnet build` (x64 Debug) succeeds for the app and test projects.
- `dotnet test --filter FullyQualifiedName~Module_Reprint` passes.
- Full suite shows no new failures from this feature.
- Manual: nav to Reprint Labels; each mode lists history with checkboxes; already-queued rows
  are red and uncheckable; Reprint Selected queues rows and shows the summary dialog; Receiving
  Edit Mode no longer shows a Reprint button.

## Guardrails

- Infor Visual SQL Server stays read-only.
- MySQL writes only through stored procedures; no raw MySQL SQL in C#.
- MVVM boundaries: views do not call DAOs; viewmodels call reprint services only.
- `x:Bind` only; no runtime `{Binding}`.
- Do not modify the existing Receiving Edit Mode history SPs; the new module uses its own
  reprint-history SPs.
- Do not remove shared SPs/DAO/service methods the new module reuses.

## Completion Criteria

- The new module is reachable from navigation and fully functional for Receiving, Dunnage, and
  Volvo.
- Old Receiving Edit Mode reprint UI is gone without breaking the Edit Mode screen.
- Build and tests pass with no new failures, and docs reflect the feature.

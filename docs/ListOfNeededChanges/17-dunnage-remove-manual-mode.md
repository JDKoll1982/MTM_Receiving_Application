# 17 Dunnage Remove Manual Mode

Last Updated: 2026-03-31

## Change

Completely remove Dunnage Manual Mode because it is redundant.

## Likely Files

- `Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml`
- `Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ManualEntryViewModel.cs`
- `Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml`
- `Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs`
- `Module_Dunnage/Services/Service_DunnageWorkflow.cs`
- `Module_Dunnage/Enums/Enum_DunnageWorkflowStep.cs`
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- `Module_Core/Services/Help/Service_Help.cs`
- `docs/CopilotForms/data/copilot-forms.config.json`
- `docs/CopilotForms/data/module-metadata/Module_Dunnage/dunnage-guided-workflow.json`
- `Module_Dunnage/SETTABLE_OBJECTS_REPORT.md`

## Implementation Instructions

1. Remove Manual Entry from the Dunnage mode-selection UI, including the mode card/button, default-mode checkbox, tooltip usage, and any text that frames Dunnage as Guided/Manual/Edit.
2. Remove the `SelectManualModeAsync` command path and `IsManualModeDefault` preference handling from `ViewModel_Dunnage_ModeSelectionViewModel.cs`.
3. Update `Service_DunnageWorkflow.StartWorkflowAsync()` so the Dunnage default-mode logic only routes to Guided (`TypeSelection`) or Edit (`EditMode`). Decide how to migrate any saved `manual` preference values so the user is not left with a dead default mode.
4. Remove `Enum_DunnageWorkflowStep.ManualEntry` from the Dunnage workflow enum and then update every switch/visibility map that references it, especially:
   - `ViewModel_Dunnage_WorkFlowViewModel.cs`
   - `Service_DunnageWorkflow.cs`
   - `View_Dunnage_WorkflowView.xaml`
5. Remove the Manual Entry screen from the workflow shell in `View_Dunnage_WorkflowView.xaml`, including any header/back button visibility tied to `IsManualEntryVisible`.
6. Delete `View_Dunnage_ManualEntryView.xaml`, `View_Dunnage_ManualEntryView.xaml.cs`, and `ViewModel_Dunnage_ManualEntryViewModel.cs` only after all references, registrations, and route transitions are gone.
7. Remove DI registrations for the Manual Entry view and ViewModel from `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.
8. Remove Dunnage Manual Entry help integration from `Module_Core/Services/Help/Service_Help.cs`, including step-to-help-key mapping, help entries, tooltip keys like `Tooltip.Button.QuickManualEntry`, and any `Dunnage.ManualEntry` or `Tip.Dunnage.ManualEntry` content.
9. Update Dunnage docs/metadata that still reference Manual Entry, at minimum:
   - `docs/CopilotForms/data/copilot-forms.config.json`
   - `docs/CopilotForms/data/module-metadata/Module_Dunnage/dunnage-guided-workflow.json`
   - `Module_Dunnage/SETTABLE_OBJECTS_REPORT.md`
10. Run a dead-code sweep after the functional changes are complete. Search for and remove any remaining references to:
    - `ManualEntry`
    - `SelectManualMode`
    - `IsManualModeDefault`
    - `View_Dunnage_ManualEntryView`
    - `ViewModel_Dunnage_ManualEntry`
    - `Dunnage.ManualEntry`
    - `QuickManualEntry`
11. Rebuild the project and resolve every compile error caused by the removal before considering the task finished.

## Dead Code Removal Focus

- Remove orphaned visibility properties such as `IsManualEntryVisible` if they are no longer used.
- Remove unreachable workflow branches and status messages like `Starting Manual Entry mode`.
- Remove dead help keys and dead documentation references so future tooling does not continue to advertise Manual Mode.
- Confirm no user-preference or session-state path can still produce `manual` as an active Dunnage mode without a live screen behind it.

## Acceptance Checks

- Manual Mode no longer appears anywhere in Dunnage navigation.
- The Dunnage workflow still loads and routes correctly without missing references.
- Searching the repo for Dunnage Manual Entry identifiers returns no live production references other than historical notes that were intentionally preserved.
- Build and navigation checks confirm there is no dead code path left behind for Manual Mode.

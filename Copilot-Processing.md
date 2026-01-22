# Copilot Processing

## User Request

Start work on `TASK-001-fix-build-output-issues.md`.

## Context

Refactoring code to remove `App.GetService<T>()` calls (Service Locator pattern) and switch to Constructor Injection.
Targeting .NET 8 / WinUI 3.

## Action Plan

1. **[COMPLETED]** Refactor `Module_Volvo\ViewModels\ViewModel_Volvo_Settings.cs`.
   - Injected `IService_Window` via constructor.
   - Removed `App.GetService<IService_Window>()`.
2. **[COMPLETED]** Refactor `Module_Receiving\ViewModels\ViewModel_Receiving_ModeSelection.cs`.
   - Removed useless `ClearAllUIInputs` logic that relied on `App.GetService` for transient ViewModels.
3. **[COMPLETED]** Refactor `Module_Dunnage\ViewModels\ViewModel_Dunnage_ModeSelectionViewModel.cs`.
   - Removed `App.GetService` calls in `ClearAllUIInputs`.
4. **[COMPLETED]** Refactor `Module_Dunnage\ViewModels\ViewModel_Dunnage_ReviewViewModel.cs`.
   - Removed `App.GetService` calls in `ClearUIInputsForNewEntry`.
5. **[EXECUTION]** Refactor `Module_Receiving\Services\Service_ReceivingWorkflow.cs`. (Pending)
6. **[EXECUTION]** Refactor `Module_Core\Services\Help\Service_Help.cs`. (Pending)
7. **[VERIFICATION]** Verify builds after each significant change.

## Current Phase

Executing Subtask 1.4: Refactor Services (`Service_ReceivingWorkflow` and `Service_Help`).

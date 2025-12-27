# Receiving UI Workflow Standards

**Category**: UI Patterns
**Last Updated**: December 26, 2025
**Applies To**: `ReceivingWorkflowViewModel`, `ReceivingWorkflowView`

## Overview

The Receiving UI uses a **Single View Wizard Pattern**. Instead of navigating between different Pages for each step, a single `ReceivingWorkflowView` hosts all step content, and the `ReceivingWorkflowViewModel` manages their visibility based on the current state of the `IService_ReceivingWorkflow`.

## Responsibilities

1.  **State Synchronization**: The ViewModel subscribes to `_workflowService.StepChanged`.
2.  **Visibility Management**: Based on `_workflowService.CurrentStep`, the ViewModel sets boolean properties (e.g., `IsPOEntryVisible`, `IsReviewVisible`) to `true` or `false`.
3.  **UI Orchestration**: The View binds the `Visibility` of different Grids/UserControls to these boolean properties (using `BooleanToVisibilityConverter`).

## Implementation Pattern

### ViewModel

```csharp
_workflowService.StepChanged += (s, e) => UpdateStepVisibility();

private void UpdateStepVisibility()
{
    var step = _workflowService.CurrentStep;
    
    IsModeSelectionVisible = step == WorkflowStep.ModeSelection;
    IsPOEntryVisible = step == WorkflowStep.POEntry;
    // ... set all others
    
    CurrentStepTitle = GetTitleForStep(step);
}
```

### View (XAML)

```xml
<Grid Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
    <views:ReceivingModeSelectionView />
</Grid>

<Grid Visibility="{x:Bind ViewModel.IsPOEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
    <views:POEntryView />
</Grid>
```

## Help Content
The ViewModel also updates `HelpContent` based on the current step, often using `WorkflowHelpContentGenerator`.

## Advantages
- **State Preservation**: No need to pass parameters between pages.
- **Smooth Transitions**: Instant switching between steps.
- **Shared Context**: The parent ViewModel can hold shared UI state (like a progress bar or status message).

# Receiving Workflow State Machine

**Category**: Business Logic
**Last Updated**: December 26, 2025
**Applies To**: `IService_ReceivingWorkflow`, `Service_ReceivingWorkflow`, `IService_DunnageWorkflow`

## Overview

The application uses a state machine pattern to manage the multi-step receiving wizard. This ensures data integrity and guides the user through the required process.

## Core Components

### 1. Workflow Steps (Enum)
Defined in `Models/Enums/Enum_WorkflowStep.cs` (or `Enum_DunnageWorkflowStep.cs`).
Typical flow:
`ModeSelection` -> `TypeSelection` -> `PartSelection` -> `QuantityEntry` -> `DetailsEntry` -> `Review`

### 2. Session State
The service maintains a `CurrentSession` object (e.g., `Model_ReceivingSession`) that accumulates data as the user progresses.
- **Singleton Scope**: The workflow service is registered as a Singleton to persist state across ViewModel navigations.

### 3. Navigation Methods
- `StartWorkflowAsync()`: Resets session and goes to first step.
- `AdvanceToNextStepAsync()`: Validates current step data and moves forward.
- `GoToStep(step)`: Jumps to a specific step (usually for back navigation).

### 4. Events
- `StepChanged`: Fired when `CurrentStep` changes. ViewModels subscribe to this to update UI.
- `StatusMessageRaised`: Fired to notify user of background actions (e.g., "Saving...").

## Implementation Pattern

```csharp
public async Task<Model_WorkflowStepResult> AdvanceToNextStepAsync()
{
    switch (CurrentStep)
    {
        case WorkflowStep.PartSelection:
            if (CurrentSession.SelectedPart == null) 
                return new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Select a part" };
            
            GoToStep(WorkflowStep.QuantityEntry);
            break;
        // ... other steps
    }
    return new Model_WorkflowStepResult { IsSuccess = true };
}
```

## Validation
Validation logic should be encapsulated in `IService_ReceivingValidation` or private methods within the workflow service.
- **Fail Fast**: Validate before changing state.
- **User Feedback**: Return clear error messages in `Model_WorkflowStepResult`.

## Saving
The `SaveSessionAsync()` method is the final action.
1. Validate entire session.
2. Call Database Service to persist.
3. Call CSV Service to export.
4. Clear session on success.

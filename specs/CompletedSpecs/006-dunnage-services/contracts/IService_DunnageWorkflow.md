# IService_DunnageWorkflow Contract

**Interface**: `IService_DunnageWorkflow`  
**Implementation**: `Service_DunnageWorkflow`  
**Location**: `Contracts/Services/IService_DunnageWorkflow.cs`  
**Registration**: Singleton (maintains wizard state across navigations)

## Interface Definition

```csharp
using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for managing the dunnage receiving wizard workflow state machine.
    /// Registered as singleton to maintain state across ViewModel navigations.
    /// </summary>
    public interface IService_DunnageWorkflow
    {
        /// <summary>
        /// Gets the current step in the wizard workflow.
        /// </summary>
        Enum_DunnageWorkflowStep CurrentStep { get; }

        /// <summary>
        /// Gets the current session data (selected type, part, quantity, etc.).
        /// </summary>
        Model_DunnageSession CurrentSession { get; }

        /// <summary>
        /// Event fired when the workflow step changes.
        /// ViewModels can subscribe to trigger navigation.
        /// </summary>
        event EventHandler StepChanged;

        /// <summary>
        /// Event fired when the workflow wants to display a status message.
        /// ViewModels can subscribe to show UI notifications.
        /// </summary>
        event EventHandler<string> StatusMessageRaised;

        /// <summary>
        /// Initializes a new workflow session.
        /// Sets CurrentStep to ModeSelection and creates empty session.
        /// </summary>
        /// <returns>True if initialization succeeded</returns>
        Task<bool> StartWorkflowAsync();

        /// <summary>
        /// Advances to the next step in the workflow.
        /// Validates session data before advancing (fail fast).
        /// </summary>
        /// <returns>Result indicating success and target step</returns>
        Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();

        /// <summary>
        /// Directly navigates to a specific step (for back navigation).
        /// Does not validate session data.
        /// </summary>
        /// <param name="step">Target workflow step</param>
        void GoToStep(Enum_DunnageWorkflowStep step);

        /// <summary>
        /// Saves the current session to the database and exports to CSV.
        /// Calls IService_MySQL_Dunnage.SaveLoadsAsync() and IService_DunnageCSVWriter.WriteToCSVAsync().
        /// </summary>
        /// <returns>Result indicating success, records saved, and CSV export status</returns>
        Task<Model_SaveResult> SaveSessionAsync();

        /// <summary>
        /// Clears the current session and resets to initial state.
        /// Does not change CurrentStep.
        /// </summary>
        void ClearSession();
    }
}
```

## Method Specifications

### StartWorkflowAsync()
**Pre-conditions**: None  
**Post-conditions**:
- `CurrentStep = Enum_DunnageWorkflowStep.ModeSelection`
- `CurrentSession` is initialized with empty values
- `StepChanged` event fires

**Errors**: None (always succeeds)

---

### AdvanceToNextStepAsync()
**Pre-conditions**: Valid session data for current step  
**Post-conditions** (on success):
- `CurrentStep` is incremented
- `StepChanged` event fires
- Returns `Model_WorkflowStepResult` with `IsSuccess = true` and `TargetStep` set

**Validation Rules**:
- `ModeSelection → TypeSelection`: No validation (mode is set programmatically)
- `TypeSelection → PartSelection`: `SelectedType` must not be null
- `PartSelection → QuantityEntry`: `SelectedPart` must not be null
- `QuantityEntry → DetailsEntry`: `Quantity` must be > 0
- `DetailsEntry → Review`: `PONumber` must not be empty if `SelectedType.RequiresPO = true`
- `Review → (Save)`: No auto-advance, ViewModel calls `SaveSessionAsync()`

**Errors**:
- Returns `Model_WorkflowStepResult` with `IsSuccess = false` and descriptive `ErrorMessage`

---

### GoToStep(step)
**Pre-conditions**: None  
**Post-conditions**:
- `CurrentStep` is set to `step`
- `StepChanged` event fires

**Use Case**: Back button navigation, error recovery

**Errors**: None (allows invalid transitions for flexibility)

---

### SaveSessionAsync()
**Pre-conditions**:
- `CurrentSession.PendingLoads` has at least one load
- `CurrentStep` should be `Review` (not enforced)

**Post-conditions** (on success):
- All loads in `CurrentSession.PendingLoads` are saved to database
- CSV file is written to local and network paths
- `CurrentSession.PendingLoads` is cleared
- Returns `Model_SaveResult` with success details

**Dependencies**:
- Calls `IService_MySQL_Dunnage.SaveLoadsAsync(CurrentSession.PendingLoads)`
- Calls `IService_DunnageCSVWriter.WriteToCSVAsync(CurrentSession.PendingLoads)`

**Errors**:
- Returns `Model_SaveResult` with `IsSuccess = false` if database save fails
- Returns `Model_SaveResult` with `IsSuccess = false` if local CSV write fails
- Network CSV write failure is logged but does not fail operation

---

### ClearSession()
**Pre-conditions**: None  
**Post-conditions**:
- `CurrentSession.SelectedType = null`
- `CurrentSession.SelectedPart = null`
- `CurrentSession.Quantity = 0`
- `CurrentSession.PONumber = string.Empty`
- `CurrentSession.SpecValues.Clear()`
- `CurrentSession.PendingLoads.Clear()`
- `CurrentStep` is unchanged

**Use Case**: Cancel button, session reset without restarting workflow

**Errors**: None

---

## Events

### StepChanged
**Type**: `EventHandler`  
**When Fired**:
- `StartWorkflowAsync()` completes
- `AdvanceToNextStepAsync()` succeeds
- `GoToStep()` is called

**Purpose**: Allows ViewModels to trigger navigation to corresponding page

---

### StatusMessageRaised
**Type**: `EventHandler<string>`  
**When Fired**:
- Session saved successfully ("Saved 5 loads to database")
- Validation warnings ("Network CSV write failed, but local succeeded")
- Info messages ("Session cleared")

**Purpose**: Allows ViewModels to show status bar messages

---

## Usage Example

```csharp
public partial class TypeSelectionViewModel : BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflow;

    public TypeSelectionViewModel(IService_DunnageWorkflow workflow, ...)
    {
        _workflow = workflow;
        _workflow.StepChanged += OnStepChanged;
    }

    [RelayCommand]
    private async Task SelectTypeAsync(Model_DunnageType type)
    {
        _workflow.CurrentSession.SelectedType = type;
        var result = await _workflow.AdvanceToNextStepAsync();
        if (!result.IsSuccess) {
            _errorHandler.ShowUserError(result.ErrorMessage, "Validation Error", nameof(SelectTypeAsync));
        }
        // Navigation triggered by StepChanged event
    }

    private void OnStepChanged(object? sender, EventArgs e)
    {
        if (_workflow.CurrentStep == Enum_DunnageWorkflowStep.PartSelection) {
            _navigationService.NavigateTo<PartSelectionViewModel>();
        }
    }
}
```

---

## Testing Strategy

**Unit Tests**:
- Mock `IService_MySQL_Dunnage` and `IService_DunnageCSVWriter`
- Test step transitions with valid/invalid session data
- Verify `StepChanged` event fires
- Test validation rules for each step
- Test save operation success/failure paths

**Integration Tests**:
- Full workflow from start to save
- Verify database records created
- Verify CSV files created

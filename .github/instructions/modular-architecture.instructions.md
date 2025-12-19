# Modular Architecture Guidelines (MANDATORY)

**Category**: Core Architecture Principle  
**Last Updated**: December 18, 2025  
**Applies To**: All application code, especially workflows and feature development

## Overview

Modularity is a **MANDATORY** architectural principle in the MTM Receiving Application. All features, workflows, and components must be developed with modularity as a core design constraint to minimize coupling, maximize reusability, and enable independent testing.

## Why Modularity is Mandatory

### Business Requirements
- **Rapid Feature Development**: Add new workflows without modifying existing code
- **Team Scalability**: Enable multiple developers to work independently
- **Future Extensibility**: Support conditional branching, multi-workflow scenarios
- **Maintainability**: Reduce technical debt and simplify debugging

### Technical Benefits
- **Code Reduction**: ~40% less boilerplate through base classes
- **Improved Testing**: Independent validators enable focused unit tests
- **Consistent Patterns**: Reduced learning curve for new developers
- **Easy Refactoring**: Changes isolated to single components

### Evidence
The receiving workflow Phase 1 modularization demonstrated concrete benefits:
- 131 lines of boilerplate eliminated
- 32 independent unit tests added
- 7 ViewModels refactored to consistent pattern
- Zero breaking changes during refactoring

## Core Modular Patterns

### 1. BaseStepViewModel<TStepData> Pattern (REQUIRED FOR WORKFLOWS)

**Purpose**: Eliminate duplicated navigation and event subscription logic across workflow steps.

**Implementation**:
```csharp
public partial class MyStepViewModel : BaseStepViewModel<MyStepData>
{
    public MyStepViewModel(
        IService_ReceivingWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(workflowService, errorHandler, logger) { }
    
    protected override WorkflowStep ThisStep => WorkflowStep.MyStep;
    
    protected override async Task OnNavigatedToAsync()
    {
        // Load state when step becomes active
        StepData.Value = _workflowService.GetValue();
        await base.OnNavigatedToAsync();
    }
    
    protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
    {
        // Validation integrated with navigation
        if (StepData.Value < 1)
            return Task.FromResult((false, "Value must be positive"));
        return Task.FromResult((true, string.Empty));
    }
    
    protected override Task OnBeforeAdvanceAsync()
    {
        // Save state before advancing
        _workflowService.SetValue(StepData.Value);
        return Task.CompletedTask;
    }
}
```

**Base Class Provides**:
- Automatic `StepChanged` event subscription/unsubscription
- Next/Previous/Cancel command implementations
- `OnNavigatedToAsync()` and `OnNavigatedFromAsync()` lifecycle hooks
- `ValidateStepAsync()` integration with navigation
- `OnBeforeAdvanceAsync()` hook for data persistence
- `IDisposable` implementation for cleanup

**What You Override**:
- `ThisStep` property (required) - Identifies the step
- `OnNavigatedToAsync()` (optional) - Load data when step becomes active
- `ValidateStepAsync()` (optional) - Validate before advancing
- `OnBeforeAdvanceAsync()` (optional) - Save data before advancing
- `OnNavigatedFromAsync()` (optional) - Cleanup when leaving step

**Benefits**:
- Eliminates ~20-40 lines of boilerplate per ViewModel
- Ensures consistent navigation behavior
- Centralizes validation triggering
- Simplifies testing through lifecycle hooks

### 2. Step Data DTOs (REQUIRED FOR WORKFLOWS)

**Purpose**: Define explicit, type-safe contracts between workflow steps.

**Guidelines**:
- Create one DTO class per workflow step
- Include ALL data needed by the step
- Use descriptive property names
- Initialize collections and strings with defaults
- Document each property with XML comments

**Example**:
```csharp
namespace MTM_Receiving_Application.Models.Receiving.StepData;

/// <summary>
/// Data contract for Load Entry step.
/// Specifies the number of loads/pallets to create in the session.
/// </summary>
public class LoadEntryData
{
    /// <summary>
    /// Number of loads/pallets to create (minimum: 1).
    /// </summary>
    public int NumberOfLoads { get; set; } = 1;

    /// <summary>
    /// Display string showing selected part information.
    /// </summary>
    public string SelectedPartInfo { get; set; } = string.Empty;
}
```

**Location**: `Models/Receiving/StepData/`

**Naming Convention**: `{StepName}Data` (e.g., `POEntryData`, `LoadEntryData`)

**Benefits**:
- Type-safe data transfer between steps
- Clear documentation of step requirements
- Easy to test step logic independently
- Facilitates future context-based architecture (Phase 2)

### 3. Independent Validators (REQUIRED)

**Purpose**: Extract validation logic into composable, independently testable components.

**Interface**:
```csharp
public interface IStepValidator<TInput> where TInput : class
{
    Task<ValidationResult> ValidateAsync(TInput input);
}
```

**Implementation Example**:
```csharp
public class LoadCountValidator : IStepValidator<LoadEntryData>
{
    public Task<ValidationResult> ValidateAsync(LoadEntryData input)
    {
        if (input == null)
            return Task.FromResult(ValidationResult.Failure("Load Entry data is required"));

        if (input.NumberOfLoads < 1)
            return Task.FromResult(ValidationResult.Failure("Number of loads must be at least 1"));

        return Task.FromResult(ValidationResult.Success());
    }
}
```

**ValidationResult Class**:
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new ValidationResult { IsValid = true };
    
    public static ValidationResult Failure(string message)
        => new ValidationResult { IsValid = false, ErrorMessage = message };
    
    public static ValidationResult Failure(List<string> errors)
        => new ValidationResult { IsValid = false, Errors = errors, ErrorMessage = string.Join("; ", errors) };
}
```

**Registration in DI Container**:
```csharp
// In App.xaml.cs ConfigureServices
services.AddSingleton<IStepValidator<LoadEntryData>, LoadCountValidator>();
services.AddSingleton<IStepValidator<POEntryData>, PONumberValidator>();
// ... register all validators
```

**Unit Testing**:
```csharp
[Fact]
public async void ValidateAsync_ShouldReturnSuccess_WhenNumberOfLoadsIsValid()
{
    // Arrange
    var validator = new LoadCountValidator();
    var input = new LoadEntryData { NumberOfLoads = 5 };

    // Act
    var result = await validator.ValidateAsync(input);

    // Assert
    Assert.True(result.IsValid);
}
```

**Benefits**:
- Validators independently testable (no ViewModel dependencies)
- Composable validation rules
- Reusable across different contexts
- Clear single responsibility

### 4. Service Interfaces (REQUIRED)

**Purpose**: Enable loose coupling, dependency injection, and testability.

**Guidelines**:
- ALL services MUST have an interface
- Interface defines public contract
- Implementation provides behavior
- Register in DI container with interface → implementation mapping

**Example**:
```csharp
// Interface (Contracts/Services/IService_MyFeature.cs)
public interface IService_MyFeature
{
    Task<Model_Dao_Result<MyData>> GetDataAsync(int id);
    Task<Model_Dao_Result> SaveDataAsync(MyData data);
}

// Implementation (Services/MyFeature/Service_MyFeature.cs)
public class Service_MyFeature : IService_MyFeature
{
    private readonly ILoggingService _logger;
    
    public Service_MyFeature(ILoggingService logger)
    {
        _logger = logger;
    }
    
    public async Task<Model_Dao_Result<MyData>> GetDataAsync(int id)
    {
        // Implementation
    }
}

// Registration (App.xaml.cs)
services.AddSingleton<IService_MyFeature, Service_MyFeature>();
```

## Prohibited Patterns (ANTI-PATTERNS)

### ❌ Hardcoded Switch Statements

**Bad**:
```csharp
// Workflow service with hardcoded transitions
public async Task AdvanceToNextStepAsync()
{
    switch (CurrentStep)
    {
        case WorkflowStep.POEntry:
            CurrentStep = WorkflowStep.LoadEntry;
            break;
        case WorkflowStep.LoadEntry:
            CurrentStep = WorkflowStep.WeightEntry;
            break;
        // ... 50+ more lines of switch cases
    }
}
```

**Why Bad**: Adding new steps requires modifying existing code, violates Open/Closed Principle.

**Future Solution** (Phase 2): Engine-based navigation with configuration.

### ❌ Duplicated Navigation Logic

**Bad**:
```csharp
// Each ViewModel manually subscribes and handles navigation
public MyStepViewModel(IService_ReceivingWorkflow workflowService, ...)
{
    _workflowService = workflowService;
    _workflowService.StepChanged += OnStepChanged; // Manual subscription
}

private void OnStepChanged(object? sender, EventArgs e) // Boilerplate
{
    if (_workflowService.CurrentStep == WorkflowStep.MyStep)
    {
        // Load data
    }
}
```

**Why Bad**: 7 ViewModels × 15 lines = 105 lines of duplicate code.

**Good**: Use `BaseStepViewModel<T>` which handles this automatically.

### ❌ Inline Validation in ViewModels

**Bad**:
```csharp
[RelayCommand]
private async Task ValidateAndContinueAsync()
{
    if (NumberOfLoads < 1)
    {
        await _errorHandler.HandleErrorAsync("Must have at least 1 load", ...);
        return;
    }
    if (WeightQuantity <= 0)
    {
        await _errorHandler.HandleErrorAsync("Weight must be positive", ...);
        return;
    }
    // More validation...
    await _workflowService.AdvanceToNextStepAsync();
}
```

**Why Bad**: Validation logic scattered across ViewModels, hard to test independently.

**Good**: Extract to `IStepValidator<T>` and test independently.

### ❌ Tight Coupling Between Steps

**Bad**:
```csharp
// Step A directly depends on Step B
public class StepAViewModel
{
    private readonly StepBViewModel _stepB; // Direct dependency
    
    public async Task SaveDataAsync()
    {
        _stepB.UpdateFromStepA(myData); // Tight coupling
    }
}
```

**Why Bad**: Cannot test Step A without Step B, cannot reorder steps.

**Good**: Use workflow service as intermediary with explicit data contracts.

## Modularity Checklist

Before submitting a PR with new features, verify:

- [ ] Workflow steps inherit from `BaseStepViewModel<TStepData>`
- [ ] Step data encapsulated in DTO classes
- [ ] Validation extracted into `IStepValidator<T>` implementations
- [ ] Validators registered in DI container
- [ ] Unit tests for each validator (null input, invalid data, valid data)
- [ ] No hardcoded switch statements for navigation
- [ ] No duplicated event subscription logic
- [ ] Services use interface-based DI
- [ ] No tight coupling between workflow steps
- [ ] Documentation updated with new patterns

## Migration Guide

### Migrating Existing ViewModels to Modular Pattern

1. **Create Step Data DTO**:
   - Extract all properties used by the step
   - Move to `Models/Receiving/StepData/{StepName}Data.cs`

2. **Create Validator**:
   - Extract validation logic from ViewModel
   - Implement `IStepValidator<TStepData>`
   - Add unit tests

3. **Update ViewModel**:
   - Change inheritance: `BaseViewModel` → `BaseStepViewModel<TStepData>`
   - Remove manual `StepChanged` subscription
   - Remove manual navigation commands
   - Override `ThisStep`, `OnNavigatedToAsync`, `ValidateStepAsync` as needed

4. **Register Validator**:
   - Add to `App.xaml.cs` DI container

5. **Update View**:
   - Change bindings from `ViewModel.Property` to `ViewModel.StepData.Property`
   - Remove manual Next/Previous button commands (use base class)

## Future Enhancements (Phase 2)

While Phase 1 established foundational patterns, Phase 2 would provide:

- **IWorkflowStep<TInput, TOutput>** interface for true step isolation
- **Immutable ReceivingContext** to eliminate global state
- **WorkflowEngine** to replace switch statements entirely
- **JSON configuration** for runtime workflow definition
- **StepRegistry** with reflection-based auto-discovery

These are optional but would enable:
- Multi-workflow support
- Conditional branching (skip steps based on conditions)
- A/B testing workflows
- Customer-specific workflows

## References

- **Constitution**: `.specify/memory/constitution.md` - Section X: Modular Architecture
- **Phase 1 Documentation**: `Documentation/WORKFLOW_MODULARIZATION_PHASE1_COMPLETE.md`
- **MVVM Pattern**: `.github/instructions/mvvm-pattern.instructions.md`
- **Base Class**: `ViewModels/Shared/BaseStepViewModel.cs`
- **Example Validators**: `Services/Receiving/Validators/`
- **Example Step Data**: `Models/Receiving/StepData/`

## Support

For questions about modular patterns:
- Review Phase 1 documentation
- Examine existing step ViewModels (LoadEntryViewModel, POEntryViewModel, etc.)
- Check validator test examples in `MTM_Receiving_Application.Tests/`

**Remember: Modularity is not optional. It's a constitutional requirement for all feature development.**

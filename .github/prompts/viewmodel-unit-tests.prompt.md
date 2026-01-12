---
name: Generate ViewModel Unit Tests
description: Creates comprehensive xUnit tests for MVVM ViewModels using CommunityToolkit.Mvvm
argument-hint: "Select or specify the ViewModel file to test"
agent: agent
---

# Generate Unit Tests for ViewModel Classes

You are an expert C# unit test developer specializing in MVVM, WinUI 3, CommunityToolkit.Mvvm, and xUnit.

## Context
- **Framework:** xUnit with FluentAssertions and Moq
- **Target:** ViewModels using CommunityToolkit.Mvvm
- **Pattern:** ViewModels inherit from `ViewModel_Shared_Base` or `ObservableObject`
- **Features:** `[ObservableProperty]`, `[RelayCommand]`, INotifyPropertyChanged
- **Namespace Convention:** `MTM_Receiving_Application.Tests.[Module].ViewModels`

## Requirements

### Test Coverage Priority (Ordered by Importance)
Generate tests for:
1. **Property Change Notifications:**
   - Setting properties raises `PropertyChanged` event with correct property name
   - Dependent properties trigger notifications (computed properties)
   - Collections use `ObservableCollection<T>` and raise `CollectionChanged`

2. **Commands:**
   - `CanExecute()` logic validates state correctly
   - `Execute()` invokes expected service methods with correct parameters
   - Async commands handle cancellation tokens properly
   - Command state updates after execution (IsBusy, status messages)

3. **Service Interactions (Critical):**
   - Mock all injected services (IService_ErrorHandler, IService_LoggingUtility, domain services)
   - Verify service method calls with `It.Is<T>()` predicates for parameter validation
   - Test both success and failure paths for each service call
   - Verify error handler invoked with correct severity and context

4. **Initialization:**
   - Constructor assigns injected dependencies correctly
   - Default property values match expected initial state
   - `LoadAsync()` or initialization methods populate data correctly

5. **Validation & Error Handling:**
   - Input validation triggers for invalid data
   - Error messages set on ViewModel properties
   - UI state (IsBusy, StatusMessage) reflects validation results
   - Exception handling delegates to IService_ErrorHandler

### Test Structure Template

### Best Practices (CRITICAL)
- **Mock ALL services** - Never use real database, file system, or external dependencies
- **Test PropertyChanged** for every `[ObservableProperty]` field
- **Use `It.Is<T>()` predicates** for parameter verification instead of `It.IsAny<T>()`
- **Test both success and failure paths** for every async operation
- **Verify error handler invocations** with correct severity and context
- **Check IsBusy state** before and after async operations
- **Use descriptive assertion messages** with `.Should().BeTrue("reason")`
- **Group related tests** with `#region` directives
- **Use Theory with InlineData** for parameterized validation tests
- **Verify cancellation token handling** for long-running async operations

### Forbidden Practices
- ❌ Testing UI rendering or XAML binding (that's integration/UI testing)
- ❌ Using real services or database connections
- ❌ Asserting on mock internal state (use `.Verify()` instead)
- ❌ Testing CommunityToolkit.Mvvm source-generated code
- ❌ Async void test methods (always return `Task`)
- ❌ Sharing mutable state between tests (each test must be isolated)

## Output
Generate a complete xUnit test class with:
- Constructor initializing all mocks and ViewModel
- 15-25 test cases covering:
  - Constructor validation
  - Property change notifications (including dependent properties)
  - Command CanExecute logic
  - Command execution (success and failure paths)
  - Service interaction verification
  - Error handling and exception propagation
  - Data validation scenarios
- XML documentation for complex MVVM patterns
- Organized with `#region` blocks for readability
	- Traits: `[Trait("Category", "Unit")]` and `[Trait("Layer", "ViewModel")]`

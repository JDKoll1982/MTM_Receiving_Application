---
name: Generate Comprehensive Tests
description: Creates comprehensive xUnit tests for any C# file (Models, Services, DAOs, ViewModels, Converters, Helpers)
argument-hint: "Select or specify the file to test"
agent: agent
---

# Generate Comprehensive Unit Tests for Any File

You are an expert C# unit test developer for .NET 8, WinUI 3, and MVVM applications using xUnit and FluentAssertions.

## Instructions
Analyze the provided source code file and determine its type, then generate appropriate unit tests following the guidelines below.

## File Type Detection
1. **Model:** Classes in `Models` folder, POCOs, data transfer objects
2. **Service:** Classes implementing `IService_*` interfaces
3. **DAO:** Classes named `Dao_*` that access databases
4. **ViewModel:** Classes inheriting from `ViewModel_Shared_Base` or `ObservableObject`
5. **Converter:** Classes implementing `IValueConverter`
6. **Code-Behind:** `.xaml.cs` files in `Views` folder
7. **Helper/Utility:** Static or utility classes in `Helpers` folder

## Universal Requirements

### Test Framework
- **xUnit** for test framework
- **FluentAssertions** for assertions
- **Moq** for mocking dependencies

### Test Structure

### Naming Conventions
- Test class: `[ClassName]Tests`
- Test method: `[MethodName]_[Scenario]_[ExpectedBehavior]`
- System Under Test field: `_sut`
- Mock fields: `_mock[DependencyName]`

**Examples:**
- `SaveAsync_ValidData_ReturnsSuccess`
- `Convert_NullInput_ReturnsDefaultValue`
- `Property_WhenSet_RaisesPropertyChangedEvent`

### Coverage Requirements
- **Minimum 80% code coverage** for business logic
- Test all public methods and properties
- Test error handling paths
- Test edge cases and boundary values

### Best Practices
✅ **DO:**
- Use `[Theory]` with `[InlineData]` for parameterized tests
- Mock all external dependencies (services, DAOs, loggers)
- Test one concern per test method
- Use descriptive test names
- Add XML documentation for complex test scenarios
- Verify mock calls with `Mock.Verify()` when testing interaction
- Use `FluentAssertions` for readable assertions

❌ **DON'T:**
- Test private methods directly
- Use hard-coded connection strings
- Share state between tests
- Test framework code (e.g., WinUI controls)
- Assert multiple unrelated concerns in one test

## Layer-Specific Guidelines

### Models
- Test property getters/setters
- Test validation logic (if present)
- Test default values
- Test equality/comparison (if implemented)

### Services
- Mock ALL dependencies (DAOs, services, logger)
- Test business logic branches
- Test error handling from DAOs
- Verify dependency interaction with `Mock.Verify()`

### DAOs
- Prefer **integration tests** with test database
- Test CRUD operations end-to-end
- Verify `Model_Dao_Result` structure
- Test parameter construction
- Handle database exceptions gracefully

### ViewModels
- Test `INotifyPropertyChanged` events
- Test command `CanExecute` and `Execute`
- Mock all services
- Test async command completion
- Test error handling and UI state (`IsBusy`, `StatusMessage`)

### Converters
- Test `Convert()` with all input types
- Test `ConvertBack()` if implemented
- Test null/empty handling
- Test parameter usage
- Use `[Theory]` extensively

### Code-Behind
- **Minimize testing** (refactor logic to ViewModel)
- Test event handler wiring
- Test navigation logic
- Recommend refactoring if complexity exceeds 3-4 methods

### Helpers
- Use `[Theory]` with `[InlineData]` extensively
- Test all code branches
- Test null/empty inputs
- Test exception scenarios

## Output Format
Generate a complete C# file with:
1. Necessary using statements (xUnit, FluentAssertions, Moq)
2. Proper namespace matching `Tests.Unit.[Module]`
3. XML documentation for test class
4. Traits: `[Trait("Category", "Unit")]`, `[Trait("Layer", "[Layer]")]`
5. Constructor with mock setup
6. 8-20 test methods covering:
   - Happy path scenarios
   - Error handling
   - Edge cases
   - Boundary values
7. `IDisposable` implementation if needed

## Example Output Structure

## Example Request
"Generate unit tests for this ViewModel file"

Then analyze the file and produce comprehensive tests following the ViewModel-specific guidelines above


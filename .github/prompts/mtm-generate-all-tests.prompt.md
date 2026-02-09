---
name: Generate Comprehensive Tests
description: Creates comprehensive xUnit tests for a single file or entire module with Memory Bank tracking
argument-hint: "Module name (e.g., 'Module_Core', 'Module_Receiving') or specific file path"
agent: agent
tools: ['read_file', 'create_file', 'replace_string_in_file', 'list_dir', 'get_symbols_by_name', 'get_symbols_overview', 'code_search', 'file_search']
---

# Generate Comprehensive Unit Tests for File or Module

You are an expert C# unit test developer for .NET 10, WinUI 3, and MVVM applications using xUnit and FluentAssertions with Memory Bank integration for progress tracking.

## Mission

Generate comprehensive, production-quality unit tests for either:
1. **Single File** - One source file specified by path
2. **Entire Module** - All testable files in a module (e.g., Module_Core, Module_Receiving)

Track progress in Memory Bank, create task entries for multi-file generation, and document testing patterns discovered.

## Scope & Preconditions

**When to Use:**
- User requests test generation for a module or file
- New code written without tests
- Increasing test coverage for existing code
- Establishing testing patterns for project

**Preconditions:**
- Source code files exist and are accessible
- Test project structure exists (`MTM_Receiving_Application.Tests/`)
- Memory Bank initialized (optional but recommended)

**If Test Project Missing:**
- Inform user test project structure needed
- Suggest creating test project first

## Inputs

**Required:**
- `${input:target}` - Module name (e.g., "Module_Core") OR file path (e.g., "Module_Core/Behaviors/AuditBehavior.cs")

**Optional:**
- `${input:focus}` - Specific subfolder or file type to focus on (e.g., "Behaviors", "Services", "ViewModels")
- `${input:overwrite[:false]}` - Whether to overwrite existing test files (default: false, skip existing)

## Instructions

Analyze the provided target (module or file) and determine its type, then generate appropriate unit tests following the guidelines below.

## Workflow

### Step 1: Determine Target Type (Module vs File)

**Analyze input to determine if target is module or file:**

**Module Indicators:**
- Starts with "Module_" (e.g., "Module_Core", "Module_Receiving")
- Is a directory name without file extension
- User specified "all tests for Module_X"

**File Indicators:**
- Contains file extension (.cs)
- Is a file path (e.g., "Module_Core/Behaviors/AuditBehavior.cs")
- User specified specific file name

**Action:** Use `file_search` to verify if target exists as directory or file.

### Step 2a: Single File Path (If File Target)

If target is a single file:
1. Read the source file using `get_file` or `read_file`
2. Analyze file type (see File Type Detection section)
3. Generate tests following layer-specific guidelines
4. Create test file in appropriate test project location
5. Report completion to user

**Skip to Step 3: Analyze File Type**

### Step 2b: Module Discovery (If Module Target)

If target is a module:

**2b.1: Enumerate Module Files**

Use `list_dir` recursively to find all `.cs` files in module folder:
```
Module_Core/
  ├── Behaviors/
  │   ├── AuditBehavior.cs
  │   ├── LoggingBehavior.cs
  │   └── ValidationBehavior.cs
  ├── Services/
  │   ├── Service_ErrorHandler.cs
  │   └── Service_LoggingUtility.cs
  └── Models/
      └── Model_Dao_Result.cs
```

**2b.2: Filter Testable Files**

Exclude from test generation:
- `.xaml.cs` files (minimal logic, refactor to ViewModel)
- Files in `obj/` or `bin/` directories
- Auto-generated files (*.g.cs, *.Designer.cs)
- Files already with 80%+ test coverage (if detectable)

**Apply focus filter if specified:**
- If `${input:focus}` = "Behaviors", only test files in Behaviors/ folder
- If `${input:focus}` = "Services", only test files in Services/ folder

**2b.3: Check for Existing Tests**

For each file, check if test file already exists:
- Expected test path: `MTM_Receiving_Application.Tests/{Module}/{Subfolder}/{FileName}Tests.cs`
- If exists and `overwrite` = false, skip file
- If exists and `overwrite` = true, regenerate

**2b.4: Create Task for Module Test Generation**

If testing entire module, create Memory Bank task:
- Task ID: Next available (e.g., TASK004)
- Task Name: "Generate Unit Tests for {ModuleName}"
- Subtasks: One per source file to test
- Status: In Progress

**Action:** Use create-task prompt pattern to create task entry.

**2b.5: Present Generation Plan**

Show user what will be generated:
```markdown
## Test Generation Plan: Module_Core

**Files to Test (8):**
- ✅ Behaviors/AuditBehavior.cs → AuditBehaviorTests.cs (exists, skip)
- 🆕 Behaviors/LoggingBehavior.cs → LoggingBehaviorTests.cs
- 🆕 Behaviors/ValidationBehavior.cs → ValidationBehaviorTests.cs
- ✅ Services/Service_ErrorHandler.cs → Service_ErrorHandlerTests.cs (exists, skip)
- 🆕 Services/Service_LoggingUtility.cs → Service_LoggingUtilityTests.cs
- 🆕 Models/Model_Dao_Result.cs → Model_Dao_ResultTests.cs
- 🆕 Helpers/Helper_Database_Variables.cs → Helper_Database_VariablesTests.cs
- 🆕 Helpers/Helper_Database_StoredProcedure.cs → Helper_Database_StoredProcedureTests.cs

**Summary:**
- Total Files: 8
- Existing Tests: 2 (skipped)
- New Tests: 6 (will generate)

**Memory Bank:**
- Task TASK004 created
- Progress tracking enabled

Proceed with generation? (yes/no)
```

**Wait for user confirmation before proceeding.**

### Step 3: Analyze File Type

**Analyze file type to determine testing approach:**

1. **Model:** Classes in `Models` folder, POCOs, data transfer objects
2. **Service:** Classes implementing `IService_*` interfaces
3. **DAO:** Classes named `Dao_*` that access databases
4. **ViewModel:** Classes inheriting from `ViewModel_Shared_Base` or `ObservableObject`
5. **Converter:** Classes implementing `IValueConverter`
6. **Code-Behind:** `.xaml.cs` files in `Views` folder (recommend refactoring)
7. **Helper/Utility:** Static or utility classes in `Helpers` folder
8. **Behavior:** Classes implementing `IPipelineBehavior<TRequest, TResponse>` (MediatR)
9. **Handler:** Classes implementing `IRequestHandler<TRequest, TResponse>` (MediatR CQRS)
10. **Validator:** Classes inheriting from `AbstractValidator<T>` (FluentValidation)

**Action:** Use `get_symbols_overview` to inspect class structure and inheritance.

### Step 4: Generate Tests Following Layer Guidelines

For each file (single or module batch):

**4.1: Read Source Code**

Use `read_file` or `get_file` to load complete source file.

**4.2: Identify Public API**

Use `get_symbols_by_name` to identify:
- Public classes
- Public methods
- Public properties
- Public events
- Constructors

**4.3: Generate Test File**

Follow layer-specific guidelines (see sections below) to generate comprehensive test file.

**4.4: Create Test File**

Use `create_file` to write test file to:
- Path: `MTM_Receiving_Application.Tests/{Module}/{Subfolder}/{FileName}Tests.cs`
- Ensure directory structure matches source structure

**4.5: Update Task Progress (If Module Generation)**

If generating for module with task tracking:
- Mark subtask as "Complete" in task file
- Add progress log entry
- Update completion percentage
- Use update-task prompt pattern

### Step 5: Validate Generated Tests

**5.1: Syntax Check**

Verify generated test file:
- Has all required using statements
- Compiles (check for syntax errors)
- Follows naming conventions
- Has proper namespace

**5.2: Coverage Check**

Ensure tests cover:
- All public methods (excluding properties)
- Error handling paths
- Edge cases
- Boundary values

**5.3: Pattern Adherence**

Verify tests follow MTM patterns:
- Use FluentAssertions for assertions
- Mock all dependencies
- No Arrange/Act/Assert comments (per repo standard)
- Test naming: `{Method}_Should{Result}_When{Condition}`

### Step 6: Update Memory Bank (If Applicable)

**6.1: Document New Patterns**

If new testing patterns discovered:
- Update `memory-bank/systemPatterns.md` with pattern
- Add code examples
- Document common pitfalls

**6.2: Update Progress**

Update `memory-bank/progress.md`:
- Add to "What Works" section (test coverage improved)
- Update test count/coverage statistics
- Add to "Recent Accomplishments"

**6.3: Update Task (If Module Generation)**

If task was created for module:
- Mark task as Completed
- Add summary to progress log
- Update `memory-bank/tasks/_index.md`

### Step 7: Present Results

**Single File Output:**
```markdown
## Test File Generated

**Source:** Module_Core/Behaviors/LoggingBehavior.cs  
**Test File:** MTM_Receiving_Application.Tests/Module_Core/Behaviors/LoggingBehaviorTests.cs

**Coverage:**
- 8 test methods generated
- All public methods covered
- Error handling tested
- Edge cases included

**Next Steps:**
- Review generated tests
- Run tests: `dotnet test --filter "FullyQualifiedName~LoggingBehaviorTests"`
- Adjust as needed
```

**Module Output:**
```markdown
## Module Test Generation Complete: Module_Core

**Files Generated (6):**
✅ LoggingBehaviorTests.cs - 8 tests
✅ ValidationBehaviorTests.cs - 12 tests
✅ Service_LoggingUtilityTests.cs - 15 tests
✅ Model_Dao_ResultTests.cs - 6 tests
✅ Helper_Database_VariablesTests.cs - 10 tests
✅ Helper_Database_StoredProcedureTests.cs - 18 tests

**Total Tests Generated:** 69 tests

**Memory Bank Updated:**
- ✅ Task TASK004 marked Complete
- ✅ systemPatterns.md updated with new patterns
- ✅ progress.md updated with accomplishment

**Next Steps:**
- Run all tests: `dotnet test --filter "Category=Unit"`
- Check coverage: `dotnet test --collect:"XPlat Code Coverage"`
- Review and adjust generated tests as needed
```

## File Type Detection (Detailed)

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

## Output Format

Generate a complete C# test file with:

1. **File Header:**
   ```csharp
   // <auto-generated>
   // This test file was generated by GitHub Copilot
   // Generation Date: YYYY-MM-DD
   // Source File: {SourceFilePath}
   // </auto-generated>
   ```

2. **Using Statements:**
   ```csharp
   using Xunit;
   using FluentAssertions;
   using Moq;
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   // ... other necessary usings
   ```

3. **Namespace:** `MTM_Receiving_Application.Tests.{Module}.{Subfolder}`

4. **XML Documentation:**
   ```csharp
   /// <summary>
   /// Unit tests for <see cref="{ClassName}"/>.
   /// Tests {brief description of what class does}.
   /// </summary>
   ```

5. **Test Class Attributes:**
   ```csharp
   [Trait("Category", "Unit")]
   [Trait("Layer", "{Layer}")] // e.g., "Behavior", "Service", "ViewModel"
   [Trait("Module", "{Module}")] // e.g., "Module_Core"
   ```

6. **Constructor with Mock Setup**

7. **8-20 Test Methods** covering:
   - Happy path scenarios (primary use cases)
   - Error handling (exceptions, null inputs)
   - Edge cases (empty collections, boundary values)
   - State validation (property changes, events)

8. **IDisposable Implementation** (if needed for cleanup)

## Example Output Structure

```csharp
// <auto-generated>
// This test file was generated by GitHub Copilot
// Generation Date: 2025-01-19
// Source File: Module_Core/Behaviors/LoggingBehavior.cs
// </auto-generated>

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Behaviors;

/// <summary>
/// Unit tests for <see cref="LoggingBehavior{TRequest, TResponse}"/>.
/// Tests request/response logging functionality in MediatR pipeline.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Behavior")]
[Trait("Module", "Module_Core")]
public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> _mockLogger;
    private readonly LoggingBehavior<TestRequest, TestResponse> _sut;

    public LoggingBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
        _sut = new LoggingBehavior<TestRequest, TestResponse>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldLogRequestInformation_WhenRequestIsProcessed()
    {
        // Test implementation...
    }

    // ... 7-19 more test methods
}

public record TestRequest : IRequest<TestResponse>
{
    public string Data { get; init; } = string.Empty;
}

public record TestResponse
{
    public bool Success { get; init; }
}
```

## Memory Bank Integration

### Task Creation (Module Generation)

When generating tests for entire module, create task:

**Task File:** `memory-bank/tasks/TASKXXX-generate-tests-{modulename}.md`

**Structure:**
```markdown
# TASKXXX - Generate Unit Tests for {ModuleName}

**Status:** In Progress  
**Added:** YYYY-MM-DD  
**Updated:** YYYY-MM-DD

## Original Request
Generate comprehensive unit tests for all testable files in {ModuleName}

## Thought Process
Module contains {X} testable files across {Y} categories (Behaviors, Services, etc.).
Will generate tests following MTM patterns with FluentAssertions and Moq.

## Implementation Plan
1. Analyze module structure and enumerate testable files
2. Generate tests for Behaviors (3 files)
3. Generate tests for Services (2 files)
4. Generate tests for Models (1 file)
5. Generate tests for Helpers (2 files)
6. Validate all generated tests compile
7. Update Memory Bank with patterns discovered

## Progress Tracking

**Overall Status:** In Progress - 0%

### Subtasks
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | Enumerate testable files | Not Started | YYYY-MM-DD | |
| 1.2 | Generate LoggingBehaviorTests | Not Started | YYYY-MM-DD | |
| 1.3 | Generate ValidationBehaviorTests | Not Started | YYYY-MM-DD | |
| ... | ... | ... | ... | |
```

### Pattern Documentation

After generating tests, update `memory-bank/systemPatterns.md`:

```markdown
## Testing Patterns

### MediatR Behavior Testing (Updated: YYYY-MM-DD)

**Pattern for testing IPipelineBehavior:**

[code example from generated tests]

**Key Points:**
- Test types must be public for Moq proxy generation
- Use discard parameter for CancellationToken in MediatR 14.0
- Mock ILogger with generic type matching behavior

**Common Pitfalls:**
- Private test types cause Moq errors
- Incorrect delegate signature for MediatR 14.0
```

### Progress Tracking

Update `memory-bank/progress.md`:

```markdown
## Recent Accomplishments

### YYYY-MM-DD: Module_Core Test Coverage Complete
**What was done:**
- Generated 69 unit tests across 6 files
- Coverage increased from 45% to 85%
- Documented MediatR testing patterns

**Files Created:**
- LoggingBehaviorTests.cs (8 tests)
- ValidationBehaviorTests.cs (12 tests)
- Service_LoggingUtilityTests.cs (15 tests)
- ... [list all files]

**Patterns Established:**
- MediatR behavior testing with Moq
- FluentValidation validator testing
- Service mocking patterns
```

## Quality Assurance

### Validation Checklist

**Pre-Generation:**
- [ ] Target (module or file) exists and is accessible
- [ ] Test project structure exists
- [ ] Source file(s) are valid C# code
- [ ] No existing test files (or overwrite confirmed)

**Post-Generation:**
- [ ] All test files created successfully
- [ ] Test files follow naming conventions
- [ ] All tests have proper XML documentation
- [ ] Mock objects properly initialized
- [ ] FluentAssertions used for assertions
- [ ] Test methods follow naming pattern
- [ ] Edge cases and error handling tested
- [ ] Tests compile without errors (if verifiable)

**Memory Bank (If Module):**
- [ ] Task created in memory-bank/tasks/
- [ ] Task added to _index.md
- [ ] Progress tracked with subtasks
- [ ] Patterns documented in systemPatterns.md
- [ ] Accomplishment recorded in progress.md

### Common Issues

**Issue:** Module path not found  
**Solution:** Verify module name, use `list_dir` to show available modules

**Issue:** Test file already exists  
**Solution:** Check overwrite parameter, offer to skip or regenerate

**Issue:** Cannot determine file type  
**Solution:** Analyze inheritance and interfaces, ask user for clarification

**Issue:** Too many files in module  
**Solution:** Suggest using focus filter or generating in batches

## Example Usage

### Single File Request

**User Command:**
```
Generate comprehensive tests for Module_Core/Behaviors/LoggingBehavior.cs
```

**Agent Response:**
1. "I'll generate comprehensive unit tests for LoggingBehavior."
2. [Analyzes file, identifies as MediatR Behavior]
3. [Generates 8 test methods covering all scenarios]
4. [Creates LoggingBehaviorTests.cs]
5. [Presents summary with test coverage details]

### Module Request

**User Command:**
```
Generate comprehensive tests for Module_Core
```

**Agent Response:**
1. "I'll generate tests for all testable files in Module_Core."
2. [Enumerates 8 testable files]
3. [Checks for existing tests, finds 2]
4. [Creates task TASK004 in Memory Bank]
5. [Presents generation plan]
6. **Waits for user confirmation**
7. [Upon confirmation, generates 6 test files]
8. [Updates task progress after each file]
9. [Marks task complete]
10. [Updates systemPatterns.md and progress.md]
11. [Presents comprehensive summary]

### Module Request with Focus

**User Command:**
```
Generate comprehensive tests for Module_Core - focus on Behaviors
```

**Agent Response:**
1. "I'll generate tests for Behavior files in Module_Core."
2. [Enumerates only files in Behaviors/ subfolder]
3. [Finds 3 behavior files]
4. [Creates focused task]
5. [Generates tests for 3 files]
6. [Updates Memory Bank]
7. [Presents summary focused on Behaviors]

## Reference Documentation

- Primary: `.github/copilot-instructions.md` (MTM architecture patterns)
- Testing: `.github/instructions/testing-strategy.instructions.md`
- Memory Bank: `.github/instructions/memory-bank.instructions.md`
- Patterns: `memory-bank/systemPatterns.md` (MediatR 14.0 specifics)

## Example Request

**Single File:**
"Generate unit tests for this ViewModel file" → [Analyze and generate ViewModel tests]

**Module:**
"Generate comprehensive tests for Module_Receiving" → [Enumerate, create task, generate all tests]

**Module with Focus:**
"Generate tests for Module_Core Behaviors" → [Filter to Behaviors/, generate focused tests]

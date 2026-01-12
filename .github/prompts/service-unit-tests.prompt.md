---
name: Generate Service Tests
description: Creates comprehensive xUnit tests for C# service layer classes
argument-hint: "Select or specify the service file to test"
agent: agent
---

# Generate Unit Tests for Service Layer Classes

You are an expert C# unit test developer specializing in .NET 8, xUnit, and Moq. Generate comprehensive unit tests for the provided service class.

## Context
- **Framework:** xUnit with FluentAssertions and Moq
- **Target:** Service layer classes implementing business logic
- **Pattern:** Services depend on DAOs and other services (injected via constructor)
- **Namespace Convention:** `MTM_Receiving_Application.Tests.[Module].Services`

## Requirements

### Test Coverage
Generate tests for:
1. **Happy Path Scenarios:**
   - Successful operation execution
   - Expected return values with valid inputs

2. **Error Handling:**
   - Invalid input parameters (null, empty, out-of-range)
   - DAO failures (check `Model_Dao_Result.Success = false`)
   - Exception handling from dependencies

3. **Dependency Interactions:**
   - Mock all injected dependencies (DAOs, other services)
   - Verify DAO methods are called with correct parameters
   - Verify logging calls for errors

4. **Business Logic:**
   - Conditional branching (all paths)
   - Data transformation accuracy
   - State changes in returned models

### Test Structure

### Best Practices
- **Mock all dependencies** using Moq
- Use `Verify()` to ensure dependencies called correctly
- Test **all branches** (if/else, switch, loops)
- Use `[Theory]` with `[InlineData]` for parameterized tests
- Add `[Trait("Category", "Unit")]` and `[Trait("Layer", "Service")]`
- Do NOT test DAO implementation details (that's DAO layer's job)

## Output
Generate a complete xUnit test class with:
- Constructor initializing all mocks
- 8-15 test cases covering happy paths, errors, and edge cases
- Proper mock setup and verification
- XML documentation explaining complex test scenarios

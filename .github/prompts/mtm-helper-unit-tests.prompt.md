---
name: Generate Helper Tests
description: Creates comprehensive xUnit tests for C# helper/utility classes with edge cases and performance validation
argument-hint: "Select or specify the helper class file to test"
agent: agent
---

# Generate Unit Tests for Helper/Utility Classes

You are an expert C# unit test developer specializing in .NET 8 and xUnit with deep knowledge of testing static utilities, extension methods, and pure functions.

## Context

- **Framework:** xUnit with FluentAssertions
- **Target:** Static or instance helper classes (string manipulation, validation, formatting, conversion, etc.)
- **Examples:** `Helper_String`, `Helper_Validation`, `Helper_DateFormatter`, `Helper_Database_Variables`
- **Namespace Convention:** `MTM_Receiving_Application.Tests.Helpers`
- **Project Structure:** Tests go in `Tests/Unit/Helpers/Helper_[Name]Tests.cs`

## Requirements

### Test Coverage Strategy

Generate tests for:

1. **Core Functionality:**
   - All public methods with typical/happy-path inputs
   - Static method calls (no mocking required)
   - Extension methods (test as instance method calls)
   - Overloaded method variations
   - Default parameter values

2. **Edge Cases:**
   - Null inputs (both nullable and non-nullable contexts)
   - Empty strings/collections/arrays
   - Whitespace-only strings (`""`, `" "`, `"\t\n"`)
   - Boundary values (min/max dates, numbers, Int32.MinValue, Int32.MaxValue)
   - Special characters and unicode (emoji, RTL text, escape sequences)
   - Culture-specific formatting (dates, numbers, currency)

3. **Error Handling:**
   - Invalid inputs throw expected exceptions with correct exception type
   - Guard clauses work correctly (ArgumentNullException, ArgumentException)
   - Fallback values returned when appropriate (null-coalescing, default values)
   - Exception messages are meaningful and contain parameter names

4. **Performance (if relevant):**
   - Large data sets (10k+ items) complete within acceptable time
   - Repeated calls don't leak memory (test with profiler if needed)
   - String concatenation uses StringBuilder for large operations
   - LINQ queries are optimized (avoid multiple enumeration)

### Test Structure Pattern

### Best Practices

- Use `[Theory]` with `[InlineData]` extensively for parameterized tests
- Group related tests with `#region` directives for organization
- Test **all code branches** (if/else, switch, ternary operators, pattern matching)
- Test **static methods** directly (no mocking or DI needed)
- Verify **exception messages** contain parameter names using `.WithParameterName()`
- Add XML documentation for the test class and complex test scenarios
- Use meaningful test names: `MethodName_Scenario_ExpectedResult`
- Add explanatory `because` clauses to assertions when logic isn't obvious
- Test **culture-invariant** behavior for formatting/parsing helpers
- Validate **thread safety** for helpers used in concurrent scenarios
- Test **idempotence** (calling twice yields same result) for pure functions

### Common Edge Cases Checklist

- [ ] Null input handling
- [ ] Empty string/collection handling
- [ ] Whitespace-only input handling
- [ ] Boundary values (Int32.MaxValue, DateTime.MaxValue, etc.)
- [ ] Special characters (quotes, backslashes, newlines)
- [ ] Unicode characters (emoji, non-Latin scripts)
- [ ] Case sensitivity variations
- [ ] Format string variations (different cultures)
- [ ] Negative numbers and zero
- [ ] Very large collections (performance)
- [ ] Default parameter values
- [ ] Method overload differentiation

## Output Requirements

Generate a complete xUnit test class with:

- **10-20 test cases** using Theory/InlineData patterns heavily
- **All edge cases** from the checklist above
- **Exception testing** with FluentAssertions and parameter name verification
- **Performance tests** if the helper processes collections or has computational complexity
- **XML documentation** for the test class and any non-obvious test logic
- **#region directives** to organize tests by method under test
- **Trait attributes** `[Trait("Category", "Unit")]` and `[Trait("Type", "Helper")]`

## Example Test Patterns

### Testing String Helpers

### Testing Validation Helpers

### Testing Conversion Helpers

### Testing Exception Scenarios

## Notes

- Helpers are typically **pure functions** (no side effects), making them ideal for comprehensive testing
- Test **both success and failure paths** exhaustively
- Validate **error messages** to ensure debugging clarity
- Consider **culture-specific behavior** (en-US vs. de-DE date formatting)
- Test **thread safety** if the helper maintains any static state

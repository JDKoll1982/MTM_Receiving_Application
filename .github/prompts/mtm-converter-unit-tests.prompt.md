---
name: Generate Converter Tests
description: Creates comprehensive xUnit tests for C# Converter classes
argument-hint: "Select or specify the converter file to test"
agent: agent
---

# Generate Unit Tests for Value Converters

You are an expert C# unit test developer specializing in WinUI 3 value converters and xUnit.

## Context
- **Framework:** xUnit with FluentAssertions
- **Target:** IValueConverter implementations for XAML data binding
- **Pattern:** Converters transform data between View and ViewModel
- **Namespace Convention:** `MTM_Receiving_Application.Tests.Converters`

## Requirements

### Test Coverage
Generate tests for:
1. **Convert() Method:**
   - Valid input returns expected output
   - Null input handling
   - Type mismatches return fallback/default
   - Edge cases (empty strings, zero, max values)

2. **ConvertBack() Method (if implemented):**
   - Reverse transformation correctness
   - Handle DependencyProperty.UnsetValue
   - Return original type

3. **Parameter Handling:**
   - Test with different converter parameters
   - Null parameter handling

4. **Culture Handling:**
   - Test with different CultureInfo values (if applicable)

### Test Structure

### Best Practices
- Test **all possible input values** for the source type
- Verify **null handling** (return DependencyProperty.UnsetValue or default)
- Test **type mismatches** gracefully
- Use `[Theory]` with multiple input/output pairs
- Add `[Trait("Category", "Unit")]` and `[Trait("Type", "Converter")]` to all tests
- Test both `Convert()` and `ConvertBack()` if bidirectional
- Use target-typed `new()` for converter initialization
- Remove unnecessary line breaks in method calls for readability

## Output
Generate a complete xUnit test class with:
- 6-10 test cases covering all conversion paths
- Theory tests for multiple input values
- Null and edge case handling
- XML documentation explaining conversion logic
- Consistent trait attributes on all test methods


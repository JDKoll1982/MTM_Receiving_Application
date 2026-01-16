---
name: Generate Model Tests
description: Creates comprehensive xUnit tests for C# model classes following MTM Receiving Application standards
argument-hint: "Select the model class file to test (e.g., Model_ReceivingLine.cs)"
agent: agent
---

# Generate Unit Tests for Data Models

You are an expert C# unit test developer specializing in .NET 8, xUnit, and the MTM Receiving Application architecture.

## Context

- **Project:** MTM Receiving Application
- **Framework:** xUnit with FluentAssertions
- **Target:** Data model classes (`Model_*` prefix in `Models/` folders)
- **Namespace Convention:** `MTM_Receiving_Application.Tests.Unit.[Module].Models`
- **Project Structure:** Modular architecture with `Module_Core`, `Module_Shared`, `Module_Receiving`, etc.

## Test File Naming

- **Pattern:** `[ModelName]Tests.cs`
- **Example:** `Model_ReceivingLineTests.cs` for `Model_ReceivingLine.cs`
- **Location:** `Tests/Unit/[Module]/Models/`

## Test Coverage Requirements

### 1. Property Validation

Test ALL properties for:

- **Getters/Setters:** Verify values are stored and retrieved correctly
- **Null Handling:** Test nullable properties accept null
- **Required Constraints:** Verify non-nullable properties cannot be null (compile-time enforced)
- **String Constraints:**
  - Empty string (`""`) vs. null behavior
  - Whitespace-only strings (`"   "`)
  - Maximum length if applicable
- **Numeric Constraints:**
  - Minimum/maximum values
  - Zero and negative values where applicable
  - Default initialization (0 vs. null)
- **DateTime Constraints:**
  - `DateTime.MinValue` and `DateTime.MaxValue`
  - Default value (`default(DateTime)` vs. `null`)
  - UTC vs. local time (if relevant)
- **Enum Validation:**
  - All valid enum values
  - Invalid/undefined enum values (casting test)
  - Default enum value

### 2. Constructor Testing

- **Default Constructor:** Verify all properties initialize to expected defaults
- **Parameterized Constructors:** Test all overloads with valid/invalid inputs
- **Initialization Logic:** Verify any calculated or derived properties

### 3. Object Behavior (if implemented)

- **Equality (`Equals`):**
  - Same instance returns `true`
  - Identical values return `true`
  - Different values return `false`
  - Null comparison returns `false`
  - Comparison with different type returns `false`
- **Hash Code (`GetHashCode`):**
  - Equal objects have equal hash codes
  - Consistency (multiple calls return same value)
- **ToString:**
  - Returns non-null/non-empty string
  - Contains key property values (if applicable)

### 4. Edge Cases & Boundary Testing

- **String Properties:**
  - Empty string `""`
  - Whitespace `"   "`
  - Very long strings (1000+ characters)
  - Special characters (`@`, `#`, `$`, Unicode)
  - SQL injection-like strings (validation test)
- **Numeric Properties:**
  - `int.MinValue` and `int.MaxValue`
  - `decimal.MinValue` and `decimal.MaxValue`
  - Zero, negative values
  - Precision for `decimal` types
- **DateTime Properties:**
  - Past dates (e.g., `DateTime.Now.AddYears(-100)`)
  - Future dates (e.g., `DateTime.Now.AddYears(100)`)
  - Leap year dates
- **Collections (if any):**
  - Empty collections
  - Null collections
  - Large collections (performance consideration)

## Test Class Template

## FluentAssertions Best Practices

Use these assertion methods consistently:

## Test Naming Convention

Follow this pattern:

Examples:

- `Constructor_Default_ShouldInitializeWithExpectedDefaults`
- `PartID_WhenSetToValidValue_ShouldReturnSameValue`
- `Quantity_WhenSetToNegative_ShouldStoreNegativeValue`
- `CreatedDate_Default_ShouldBeMinValue`
- `Equals_WithNullComparison_ShouldReturnFalse`

## Organization Guidelines

1. **Use Regions** to group related tests:
   - `#region Constructor Tests`
   - `#region Property Get/Set Tests`
   - `#region Edge Case Tests`
   - `#region Equality Tests`

2. **Order Tests Logically:**
   - Constructors first
   - Property tests in alphabetical order
   - Edge cases
   - Equality/ToString last

3. **Add XML Documentation:**
   - Class-level summary
   - Complex test methods with `<summary>` tags

4. **Use Traits Consistently:**

## Output Requirements

Generate a **complete, compilable test class** with:

1. ✅ Proper namespace matching module structure
2. ✅ All required using statements (xUnit, FluentAssertions, model namespace)
3. ✅ XML documentation on class
4. ✅ At least **one test per property** (get/set)
5. ✅ Edge case tests for strings, numbers, dates
6. ✅ Equality tests if model implements `Equals`/`GetHashCode`
7. ✅ Organized with regions
8. ✅ Descriptive test names following convention
9. ✅ AAA pattern in alltests
10. ✅ Traits applied to class

## Example: Complete Test Class

## Verification Checklist

Before submitting generated tests:

- [ ] All properties have at least one test
- [ ] Edge cases covered for each property type
- [ ] Tests use FluentAssertions (no `Assert.Equal`)
- [ ] Proper namespace matches module structure
- [ ] Traits applied (`[Trait("Category", "Unit")]`)
- [ ] Tests follow AAA pattern
- [ ] Test names are descriptive and follow convention
- [ ] Code compiles without errors
- [ ] XML documentation present on class
- [ ] Regions used for organization

---

**When generating tests, analyze the model class first to identify:**

1. All properties and their types
2. Any implemented interfaces (IEquatable, etc.)
3. Constructors and their parameters
4. Any custom methods or logic
5. Nullable vs. non-nullable properties

Then generate comprehensive tests covering all scenarios.

# System Patterns

**Last Updated:** 2025-01-19

## Testing Patterns (Added: 2025-01-19)

### Converter Testing Pattern

**All IValueConverter implementations follow this test structure:**

```csharp
[Trait("Category", "Unit")]
[Trait("Layer", "Converter")]
[Trait("Module", "Module_Core")]
public class Converter_[Name]Tests
{
    private readonly Converter_[Name] _sut;

    public [ConstructorName]Tests()
    {
        _sut = new Converter_[Name]();
    }

    // Test Convert method with various inputs
    [Theory]
    [InlineData(input, expected)]
    public void Convert_[Scenario]_[ExpectedBehavior](type input, type expected)
    {
        var result = _sut.Convert(input, typeof(TargetType), parameter, "en-US");
        result.Should().Be(expected);
    }

    // Test null handling
    [Fact]
    public void Convert_NullValue_Returns[DefaultOrThrows]() { }

    // Test ConvertBack if implemented
    [Fact]
    public void ConvertBack_[Scenario]_[ExpectedBehavior]() { }

    // Test ConvertBack throws if not implemented
    [Fact]
    public void ConvertBack_ThrowsNotImplementedException() 
    {
        var act = () => _sut.ConvertBack(value, typeof(Type), null, "en-US");
        act.Should().Throw<NotImplementedException>();
    }
}
```

**Key Patterns:**
- Use `[Theory]` with `[InlineData]` for multiple input scenarios
- Test both directions: Convert and ConvertBack
- Test null/empty/whitespace inputs
- Test parameter handling (e.g., "Inverse" parameters)
- Use FluentAssertions for readable assertions
- Use AAA comments for clarity in new tests
- One assertion per test when possible

### Model Testing Pattern

**Simple POCOs/DTOs:**

```csharp
[Trait("Category", "Unit")]
[Trait("Layer", "Model")]
[Trait("Module", "Module_Core")]
public class Model_[Name]Tests
{
    [Fact]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        var sut = new Model_[Name]();
        // Assert all properties have correct defaults
    }

    [Fact]
    public void Property_CanBeSet()
    {
        var sut = new Model_[Name] { Property = value };
        sut.Property.Should().Be(value);
    }

    // For models with factory methods
    [Fact]
    public void FactoryMethod_ReturnsCorrectInstance()
    {
        var result = Model_[Name].Create(params);
        result.Should().NotBeNull();
        result.Property.Should().Be(expectedValue);
    }
}
```

**Observable Models (CommunityToolkit.Mvvm):**

```csharp
[Fact]
public void PropertyChanged_FiresWhenPropertyChanges()
{
    var sut = new Model_[Name]();
    var eventFired = false;

    sut.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(Model_[Name].Property))
            eventFired = true;
    };

    sut.Property = newValue;

    eventFired.Should().BeTrue();
}
```

### Common Testing Patterns Discovered

**1. Enum Testing:**
```csharp
[Theory]
[InlineData(Enum_Type.Value1)]
[InlineData(Enum_Type.Value2)]
[InlineData(Enum_Type.Value3)]
public void Property_CanBeSetToAllEnumValues(Enum_Type value)
{
    var sut = new Model() { Property = value };
    sut.Property.Should().Be(value);
}
```

**2. Computed Property Testing:**
```csharp
[Fact]
public void ComputedProperty_ReturnsExpectedFormat()
{
    var sut = new Model 
    { 
        FirstName = "John", 
        LastName = "Doe" 
    };
    
    sut.FullName.Should().Be("John Doe");
}
```

**3. Validation Method Testing:**
```csharp
[Fact]
public void Valid_ReturnsValidResult()
{
    var result = Model_ValidationResult.Valid();
    
    result.IsValid.Should().BeTrue();
    result.ErrorMessage.Should().BeEmpty();
}

[Fact]
public void Invalid_WithNullMessage_UsesDefaultMessage()
{
    var result = Model_ValidationResult.Invalid(null);
    
    result.IsValid.Should().BeFalse();
    result.ErrorMessage.Should().Be("Validation failed.");
}
```

**4. Factory Method Testing with Argument Validation:**
```csharp
[Fact]
public void FactoryMethod_WithNullArgument_ThrowsArgumentNullException()
{
    var act = () => Model_[Name].Create(null!);
    
    act.Should().Throw<ArgumentNullException>();
}

[Fact]
public void FactoryMethod_WithEmptyString_ThrowsArgumentException()
{
    var act = () => Model_[Name].Create(string.Empty);
    
    act.Should().Throw<ArgumentException>()
        .WithMessage("*cannot be null or whitespace*");
}
```

### Testing Anti-Patterns to Avoid

**❌ DON'T:**
1. Use `result.Should().NotBeNullOrEmpty()` on `object` type (use string cast first)
2. Test implementation details (private methods, internal state)
3. Assume default DateTime values without checking source code
4. Use hardcoded culture-specific formatting (always specify culture in tests)
5. Test framework code (WinUI controls, third-party libraries)

**✅ DO:**
1. Cast `object` results to expected type before assertions
2. Test public API and contracts only
3. Verify actual default values from source code
4. Use "en-US" culture for consistent test results
5. Test your own business logic and models

### Test File Organization

**Naming Convention:**
- Test class: `[ClassName]Tests`
- Test file: `[ClassName]Tests.cs`
- Namespace: `MTM_Receiving_Application.Tests.[Module].[Subfolder]`

**File Location:**
```
MTM_Receiving_Application.Tests/
└── Module_Core/
    ├── Converters/
    │   └── Converter_BooleanToVisibilityTests.cs
    ├── Models/
    │   └── Model_Dao_ResultTests.cs
    └── Behaviors/
        └── AuditBehaviorTests.cs
```

## Architecture Overview

MTM Receiving Application follows a strict **MVVM + CQRS** architecture with clear layer separation.

## Layer Architecture

```
┌─────────────────────────────────────────┐
│           View (XAML + Code-behind)     │
│  - x:Bind for compile-time binding      │
│  - No business logic                    │
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│         ViewModel (Partial Classes)     │
│  - Inherits ViewModel_Shared_Base       │
│  - Uses IMediator for CQRS              │
│  - [ObservableProperty] attributes      │
│  - [RelayCommand] methods               │
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│   MediatR Pipeline (Global Behaviors)   │
│  - AuditBehavior (user/time logging)    │
│  - LoggingBehavior (request tracking)   │
│  - ValidationBehavior (FluentValidation)│
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│       Handlers (Query/Command)          │
│  - IRequestHandler<TRequest, TResponse> │
│  - Business logic orchestration         │
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│          Service Layer                  │
│  - Interface-based (IService_*)         │
│  - Dependency injection                 │
│  - Business logic abstraction           │
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│             DAO Layer                   │
│  - Instance-based classes               │
│  - Return Model_Dao_Result              │
│  - Never throw exceptions               │
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│            Database                     │
│  - MySQL: Stored procedures (R/W)       │
│  - SQL Server: Direct queries (R ONLY)  │
└─────────────────────────────────────────┘
```

## Key Design Patterns

### 1. MVVM Pattern
- **ViewModels** are partial classes with `[ObservableProperty]` and `[RelayCommand]`
- **Views** use `x:Bind` for compile-time type safety
- **Code-behind** contains ONLY UI-specific code (no business logic)

### 2. CQRS Pattern (MediatR)
- **Commands** - Mutations (Insert, Update, Delete)
- **Queries** - Data retrieval (no side effects)
- **Handlers** - One handler per request type
- **Behaviors** - Cross-cutting concerns (logging, validation, audit)

### 3. Dependency Injection
- **Singletons** - Services, DAOs, helpers (stateless)
- **Transient** - ViewModels, Views (per-instance state)
- **Scoped** - Not used (WinUI 3 limitation)

### 4. Repository/DAO Pattern
- **DAOs** handle database operations
- Return `Model_Dao_Result` or `Model_Dao_Result<T>`
- Never throw exceptions - return failure results

### 5. Result Pattern
```csharp
public class Model_Dao_Result
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Enum_ErrorSeverity Severity { get; set; }
}

public class Model_Dao_Result<T> : Model_Dao_Result
{
    public T Data { get; set; }
}
```

## Critical Rules (FORBIDDEN)

❌ **Never Do:**
1. ViewModels calling DAOs directly - MUST use Service layer
2. ViewModels accessing `Helper_Database_*` classes
3. Static DAO classes - ALL must be instance-based
4. DAOs throwing exceptions - Return `Model_Dao_Result`
5. Raw SQL in C# for MySQL - Use stored procedures ONLY
6. Write operations to SQL Server - READ ONLY
7. Runtime `{Binding}` in XAML - Use compile-time `{x:Bind}`
8. Business logic in `.xaml.cs` code-behind

## Testing Patterns

### Unit Tests
- Test ViewModels with mocked IMediator
- Test Validators (FluentValidation) with no dependencies
- Test Handlers with mocked DAOs (if interfaces exist)
- Use FluentAssertions for readable assertions
- No Arrange/Act/Assert comments (per repo guidance)

### Integration Tests
- Test Handlers with concrete DAOs (requires database)
- Use `IAsyncLifetime` for setup/cleanup
- Test complete workflows across layers
- Use TEST- prefix for test data

### Test Naming
- Format: `{MethodName}_Should{Result}_When{Condition}`
- Example: `Handle_ShouldLogAuditInformation_WhenRequestIsProcessed`

## MediatR 14.0 Specifics

### RequestHandlerDelegate Signature
In MediatR 14.0, `RequestHandlerDelegate<TResponse>` requires a `CancellationToken` parameter:

```csharp
// ✅ CORRECT
RequestHandlerDelegate<TestResponse> next = (cancellationToken) => 
    Task.FromResult(expectedResponse);

// Can use discard if not needed
RequestHandlerDelegate<TestResponse> next = (_) => 
    Task.FromResult(expectedResponse);

// ❌ WRONG - Does not compile
RequestHandlerDelegate<TestResponse> next = () => 
    Task.FromResult(expectedResponse);
```

### Behavior Implementation
```csharp
public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
{
    // Pre-processing
    await DoSomethingAsync();
    
    // Call next behavior or handler
    var response = await next(cancellationToken);
    
    // Post-processing
    return response;
}
```

## Common Pitfalls Solved

### Issue: Moq Cannot Create Proxy for Private Types
**Problem:** Test types marked `private` cause Moq to fail with strong-named assembly errors.

**Solution:** Mark test request/response types as `public`:
```csharp
public record TestRequest : IRequest<TestResponse>
{
    public string Data { get; init; } = string.Empty;
}
```

### Issue: RequestHandlerDelegate Signature Mismatch
**Problem:** MediatR 14.0 changed delegate signature, causing "does not take 0 arguments" errors.

**Solution:** Use discard parameter for CancellationToken:
```csharp
RequestHandlerDelegate<TestResponse> next = (_) => Task.FromResult(expectedResponse);


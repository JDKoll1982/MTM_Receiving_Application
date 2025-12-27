# Unit Testing Standards

**Category**: Quality Assurance
**Last Updated**: December 26, 2025
**Applies To**: `MTM_Receiving_Application.Tests`

## Frameworks

- **Test Framework**: xUnit
- **Mocking Framework**: Moq
- **Assertions**: xUnit Assert (`Assert.Equal`, `Assert.True`, etc.)

## Project Structure

Tests mirror the main project structure:
`MTM_Receiving_Application.Tests/Unit/Services/` -> Tests for `Services/`
`MTM_Receiving_Application.Tests/Unit/ViewModels/` -> Tests for `ViewModels/`

## Naming Conventions

- **Class**: `ClassName_Tests` (e.g., `Service_DunnageWorkflow_Tests`)
- **Method**: `MethodName_ShouldExpectedBehavior_WhenCondition`
    - Example: `AdvanceToNextStepAsync_ShouldFail_WhenTypeNotSelected`

## Mocking Pattern

1.  **Setup**: Create mocks for all dependencies in the constructor.
2.  **Inject**: Pass `mock.Object` to the class under test.
3.  **Configure**: Use `Setup()` to define behavior for specific test cases.
4.  **Verify**: Use `Verify()` to ensure dependencies were called as expected.

```csharp
public class MyService_Tests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly MyService _service;

    public MyService_Tests()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new MyService(_mockDependency.Object);
    }

    [Fact]
    public async Task DoWork_ShouldCallDependency()
    {
        // Arrange
        _mockDependency.Setup(d => d.GetData()).Returns("Success");

        // Act
        await _service.DoWork();

        // Assert
        _mockDependency.Verify(d => d.GetData(), Times.Once);
    }
}
```

## Best Practices

1.  **Isolation**: Unit tests must NOT hit the database, file system, or network. Mock these interactions.
2.  **Async**: Test methods should be `async Task` if testing async code.
3.  **Coverage**: Aim for high coverage of business logic (Services, ViewModels).
4.  **AAA Pattern**: Structure tests with Arrange, Act, Assert comments.

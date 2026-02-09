using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_DispatcherTimerWrapperTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenTimerIsNull()
    {
        // Arrange

        // Act
        var action = () => new Service_DispatcherTimerWrapper(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
}

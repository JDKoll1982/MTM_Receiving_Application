using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_DispatcherTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDispatcherQueueIsNull()
    {
        // Arrange

        // Act
        var action = () => new Service_Dispatcher(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
}

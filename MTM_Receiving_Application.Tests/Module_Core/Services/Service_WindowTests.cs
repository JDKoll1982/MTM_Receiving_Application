using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_WindowTests
{
    [Fact]
    public void GetXamlRoot_ShouldReturnNull_WhenMainWindowIsNull()
    {
        // Arrange
        var sut = new Service_Window();

        // Act
        var result = sut.GetXamlRoot();

        // Assert
        result.Should().BeNull();
    }
}

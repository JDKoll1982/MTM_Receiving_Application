using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_FocusTests
{
    private readonly Service_Focus _sut;

    public Service_FocusTests()
    {
        _sut = new Service_Focus();
    }

    [Fact]
    public void SetFocus_ShouldNotThrow_WhenControlIsNull()
    {
        // Arrange

        // Act
        var action = () => _sut.SetFocus(null!);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void SetFocusFirstInput_ShouldNotThrow_WhenContainerIsNull()
    {
        // Arrange

        // Act
        var action = () => _sut.SetFocusFirstInput(null!);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AttachFocusOnVisibility_ShouldNotThrow_WhenViewIsNull()
    {
        // Arrange

        // Act
        var action = () => _sut.AttachFocusOnVisibility(null!);

        // Assert
        action.Should().NotThrow();
    }
}

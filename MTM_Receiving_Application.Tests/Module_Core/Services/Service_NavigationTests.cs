using System;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Services.Navigation;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_NavigationTests
{
    private readonly Mock<IService_LoggingUtility> _logger;
    private readonly Service_Navigation _sut;

    public Service_NavigationTests()
    {
        _logger = new Mock<IService_LoggingUtility>();
        _sut = new Service_Navigation(_logger.Object);
    }

    [Fact]
    public void NavigateTo_ShouldReturnFalse_WhenFrameIsNull()
    {
        // Arrange

        // Act
        var result = _sut.NavigateTo((Frame)null!, typeof(object));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void NavigateTo_ShouldReturnFalse_WhenPageTypeIsNull()
    {
        // Arrange

        // Act
        var result = _sut.NavigateTo((Frame)null!, (Type)null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void NavigateTo_ShouldReturnFalse_WhenTypeNameIsInvalid()
    {
        // Arrange

        // Act
        var result = _sut.NavigateTo((Frame)null!, "Missing.Type.Name");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanGoBack_ShouldReturnFalse_WhenFrameIsNull()
    {
        // Arrange

        // Act
        var result = _sut.CanGoBack(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ClearNavigation_ShouldNotThrow_WhenFrameIsNull()
    {
        // Arrange

        // Act
        var action = () => _sut.ClearNavigation(null!);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void GoBack_ShouldNotThrow_WhenFrameIsNull()
    {
        // Arrange

        // Act
        var action = () => _sut.GoBack(null!);

        // Assert
        action.Should().NotThrow();
    }
}

using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_NotificationTests
{
    private readonly Mock<IService_Dispatcher> _dispatcher;
    private readonly Service_Notification _sut;

    public Service_NotificationTests()
    {
        _dispatcher = new Mock<IService_Dispatcher>();
        _sut = new Service_Notification(_dispatcher.Object);
    }

    [Fact]
    public void ShowStatus_ShouldSetStatusMessage_WhenCalled()
    {
        // Arrange
        var message = "Status message";

        // Act
        _sut.ShowStatus(message, InfoBarSeverity.Warning);

        // Assert
        _sut.StatusMessage.Should().Be(message);
    }

    [Fact]
    public void ShowStatus_ShouldSetStatusSeverity_WhenCalled()
    {
        // Arrange
        var severity = InfoBarSeverity.Error;

        // Act
        _sut.ShowStatus("Status", severity);

        // Assert
        _sut.StatusSeverity.Should().Be(severity);
    }

    [Fact]
    public void ShowStatus_ShouldOpenStatus_WhenCalled()
    {
        // Arrange

        // Act
        _sut.ShowStatus("Status", InfoBarSeverity.Warning);

        // Assert
        _sut.IsStatusOpen.Should().BeTrue();
    }

    [Fact]
    public void ShowStatus_ShouldNotUseDispatcher_WhenSeverityIsWarning()
    {
        // Arrange

        // Act
        _sut.ShowStatus("Status", InfoBarSeverity.Warning);

        // Assert
        _dispatcher.Verify(d => d.TryEnqueue(It.IsAny<System.Action>()), Times.Never);
    }
}

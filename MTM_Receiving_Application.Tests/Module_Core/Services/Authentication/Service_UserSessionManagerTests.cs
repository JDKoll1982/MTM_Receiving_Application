using System;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Authentication;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_UserSessionManagerTests
{
    private readonly Mock<Dao_User> _daoUser;
    private readonly Mock<IService_Dispatcher> _dispatcher;
    private readonly Service_UserSessionManager _sut;

    public Service_UserSessionManagerTests()
    {
        _daoUser = new Mock<Dao_User>("test-connection");
        _dispatcher = new Mock<IService_Dispatcher>();
        _sut = new Service_UserSessionManager(_daoUser.Object, _dispatcher.Object);
    }

    [Fact]
    public void CreateSession_ShouldThrowArgumentNullException_WhenUserIsNull()
    {
        // Arrange
        var config = new Model_WorkstationConfig("TestPC");

        // Act
        var action = () => _sut.CreateSession(null!, config, "windows");

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateSession_ShouldThrowArgumentNullException_WhenConfigIsNull()
    {
        // Arrange
        var user = new Model_User { WindowsUsername = "user", FullName = "Test User" };

        // Act
        var action = () => _sut.CreateSession(user, null!, "windows");

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StartTimeoutMonitoring_ShouldThrowInvalidOperationException_WhenNoSessionExists()
    {
        // Arrange

        // Act
        var action = () => _sut.StartTimeoutMonitoring();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartTimeoutMonitoring_ShouldStartTimer_WhenSessionExists()
    {
        // Arrange
        var user = new Model_User { WindowsUsername = "user", FullName = "Test User" };
        var config = new Model_WorkstationConfig("TestPC");
        _sut.CreateSession(user, config, "windows");

        var timer = new Mock<IService_DispatcherTimer>();
        timer.SetupProperty(t => t.Interval);
        timer.SetupProperty(t => t.IsRepeating);
        _dispatcher.Setup(d => d.CreateTimer()).Returns(timer.Object);

        // Act
        _sut.StartTimeoutMonitoring();

        // Assert
        timer.Object.Interval.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void StartTimeoutMonitoring_ShouldSetTimerRepeating_WhenSessionExists()
    {
        // Arrange
        var user = new Model_User { WindowsUsername = "user", FullName = "Test User" };
        var config = new Model_WorkstationConfig("TestPC");
        _sut.CreateSession(user, config, "windows");

        var timer = new Mock<IService_DispatcherTimer>();
        timer.SetupProperty(t => t.Interval);
        timer.SetupProperty(t => t.IsRepeating);
        _dispatcher.Setup(d => d.CreateTimer()).Returns(timer.Object);

        // Act
        _sut.StartTimeoutMonitoring();

        // Assert
        timer.Object.IsRepeating.Should().BeTrue();
    }
}

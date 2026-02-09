using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Authentication;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_AuthenticationTests
{
    private readonly Mock<Dao_User> _daoUser;
    private readonly Mock<IService_ErrorHandler> _errorHandler;
    private readonly Service_Authentication _sut;

    public Service_AuthenticationTests()
    {
        _daoUser = new Mock<Dao_User>("test-connection");
        _errorHandler = new Mock<IService_ErrorHandler>();
        _sut = new Service_Authentication(_daoUser.Object, _errorHandler.Object);
    }

    [Fact]
    public async Task AuthenticateByWindowsUsernameAsync_ShouldReturnError_WhenUsernameIsEmpty()
    {
        // Arrange

        // Act
        var result = await _sut.AuthenticateByWindowsUsernameAsync(string.Empty);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateByWindowsUsernameAsync_ShouldReturnUser_WhenDaoReturnsSuccess()
    {
        // Arrange
        var user = new Model_User { WindowsUsername = "user", FullName = "Test User" };
        var daoResult = Model_Dao_Result_Factory.Success(user);
        _daoUser.Setup(d => d.GetUserByWindowsUsernameAsync("user")).ReturnsAsync(daoResult);
        _daoUser.Setup(d => d.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

        // Act
        var result = await _sut.AuthenticateByWindowsUsernameAsync("user");

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task AuthenticateByWindowsUsernameAsync_ShouldReturnError_WhenDaoFails()
    {
        // Arrange
        var daoResult = Model_Dao_Result_Factory.Failure<Model_User>("Not found");
        _daoUser.Setup(d => d.GetUserByWindowsUsernameAsync("user")).ReturnsAsync(daoResult);

        // Act
        var result = await _sut.AuthenticateByWindowsUsernameAsync("user");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateByPinAsync_ShouldReturnError_WhenPinIsInvalid()
    {
        // Arrange

        // Act
        var result = await _sut.AuthenticateByPinAsync("user", "12");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateByPinAsync_ShouldReturnUser_WhenDaoReturnsSuccess()
    {
        // Arrange
        var user = new Model_User { WindowsUsername = "user", FullName = "Test User" };
        var daoResult = Model_Dao_Result_Factory.Success(user);
        _daoUser.Setup(d => d.ValidateUserPinAsync("user", "1234")).ReturnsAsync(daoResult);
        _daoUser.Setup(d => d.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

        // Act
        var result = await _sut.AuthenticateByPinAsync("user", "1234");

        // Assert
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task CreateNewUserAsync_ShouldReturnError_WhenWindowsUsernameMissing()
    {
        // Arrange
        var user = new Model_User { FullName = "Test User", Department = "Test", Shift = "A", Pin = "1234" };

        // Act
        var result = await _sut.CreateNewUserAsync(user, "creator");

        // Assert
        result.Success.Should().BeFalse();
    }
}

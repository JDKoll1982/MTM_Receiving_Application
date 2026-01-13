using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Authentication
{
    /// <summary>
    /// Unit tests for Service_Authentication
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Service")]
    public class Service_Authentication_Tests
    {
        private readonly Mock<Dao_User> _mockDaoUser;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Service_Authentication _sut;

        public Service_Authentication_Tests()
        {
            // Dao_User constructor requires a string, but the mock object will override virtual methods
            _mockDaoUser = new Mock<Dao_User>("Server=fake;");
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _sut = new Service_Authentication(_mockDaoUser.Object, _mockErrorHandler.Object);
        }

        [Fact]
        public async Task AuthenticateByWindowsUsernameAsync_EmptyUsername_ReturnsError_Async()
        {
            // Act
            var result = await _sut.AuthenticateByWindowsUsernameAsync("");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("required");
        }

        [Fact]
        public async Task AuthenticateByWindowsUsernameAsync_UserNotFound_ReturnsError_Async()
        {
            // Arrange
            _mockDaoUser.Setup(x => x.GetUserByWindowsUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_User>("User not found"));

            // Act
            var result = await _sut.AuthenticateByWindowsUsernameAsync("domain\\user");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("not found");
        }

        [Fact]
        public async Task AuthenticateByWindowsUsernameAsync_ValidUser_ReturnsSuccess_Async()
        {
            // Arrange
            var user = new Model_User { FullName = "Test User", WindowsUsername = "domain\\user" };
            _mockDaoUser.Setup(x => x.GetUserByWindowsUsernameAsync("domain\\user"))
                .ReturnsAsync(Model_Dao_Result_Factory.Success(user));

            _mockDaoUser.Setup(x => x.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

            // Act
            var result = await _sut.AuthenticateByWindowsUsernameAsync("domain\\user");

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().BeEquivalentTo(user);
            _mockDaoUser.Verify(x => x.LogUserActivityAsync("login_success", "domain\\user", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("", "1234")]
        [InlineData("user", "")]
        [InlineData("user", "123")] // Too short
        [InlineData("user", "12345")] // Too long
        [InlineData("user", "abcd")] // Non-numeric
        public async Task AuthenticateByPinAsync_InvalidInput_ReturnsError_Async(string username, string pin)
        {
            // Act
            var result = await _sut.AuthenticateByPinAsync(username, pin);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateByPinAsync_ValidCredentials_ReturnsSuccess_Async()
        {
            // Arrange
            var user = new Model_User { FullName = "Test User", WindowsUsername = "domain\\user" };
            _mockDaoUser.Setup(x => x.ValidateUserPinAsync("user", "1234"))
                .ReturnsAsync(Model_Dao_Result_Factory.Success(user));

             _mockDaoUser.Setup(x => x.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

            // Act
            var result = await _sut.AuthenticateByPinAsync("user", "1234");

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().Be(user);
        }

        [Fact]
        public async Task CreateNewUserAsync_InvalidData_ReturnsError_Async()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "" }; // Missing fields

            // Act
            var result = await _sut.CreateNewUserAsync(user, "admin");

            // Assert
            result.Success.Should().BeFalse();
        }

         [Fact]
        public async Task CreateNewUserAsync_ValidData_ReturnsSuccess_Async()
        {
            // Arrange
            var user = new Model_User 
            { 
                WindowsUsername = "domain\\newuser",
                FullName = "New User",
                Department = "Receiving",
                Shift = "1",
                Pin = "1234"
            };
            
            _mockDaoUser.Setup(x => x.CreateNewUserAsync(user, "admin"))
                .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(101));

             _mockDaoUser.Setup(x => x.LogUserActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

            // Act
            var result = await _sut.CreateNewUserAsync(user, "admin");

            // Assert
            result.Success.Should().BeTrue();
            result.EmployeeNumber.Should().Be(101);
        }

        [Theory]
        [InlineData("1234", true)]
        [InlineData("123", false)]
        [InlineData("abcd", false)]
        [InlineData("", false)]
        public async Task ValidatePinAsync_ValidatesCorrectly_Async(string pin, bool expectedValid)
        {
            // Act
            var result = await _sut.ValidatePinAsync(pin);

            // Assert
            result.IsValid.Should().Be(expectedValid);
        }

        [Fact]
        public async Task DetectWorkstationTypeAsync_SharedTerminal_ReturnsType_Async()
        {
            // Arrange
            var computerName = "SHARED-PC";
            _mockDaoUser.Setup(x => x.GetSharedTerminalNamesAsync())
                .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<string> { "SHARED-PC" }));

            // Act
            var result = await _sut.DetectWorkstationTypeAsync(computerName);

            // Assert
            result.WorkstationType.Should().Be("shared_terminal");
            result.ComputerName.Should().Be(computerName);
        }

         [Fact]
        public async Task DetectWorkstationTypeAsync_PersonalWorkstation_ReturnsType_Async()
        {
             // Arrange
            var computerName = "PERSONAL-PC";
            _mockDaoUser.Setup(x => x.GetSharedTerminalNamesAsync())
                .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<string> { "SHARED-PC" }));

            // Act
            var result = await _sut.DetectWorkstationTypeAsync(computerName);

            // Assert
            result.WorkstationType.Should().Be("personal_workstation");
        }
    }
}


using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;

namespace MTM_Receiving_Application.Tests.Unit
{
    public class AuthenticationServiceTests
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Service_Authentication _authService;

        public AuthenticationServiceTests()
        {
            _daoUser = new Dao_User(ConnectionString);
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _authService = new Service_Authentication(_daoUser, _mockErrorHandler.Object);
        }

        [Fact]
        public async Task AuthenticateByWindowsUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            string username = "johnk"; // Assuming exists

            // Act
            var result = await _authService.AuthenticateByWindowsUsernameAsync(username);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Equal(username, result.User.WindowsUsername);
        }

        [Fact]
        public async Task AuthenticateByPinAsync_ShouldReturnUser_WhenPinIsValid()
        {
            // Arrange
            string username = "testuser_pin_auth_" + new Random().Next(1000, 9999);
            string pin = new Random().Next(1000, 9999).ToString();
            var newUser = new Model_User
            {
                WindowsUsername = username,
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "Test User Auth",
                Department = "Receiving",
                Shift = "1st Shift",
                Pin = pin,
                IsActive = true
            };
            await _daoUser.CreateNewUserAsync(newUser, "system");

            // Act
            var result = await _authService.AuthenticateByPinAsync(username, pin);

            // Assert
            Assert.True(result.Success, $"Authentication failed: {result.ErrorMessage}");
            Assert.NotNull(result.User);
            Assert.Equal(username, result.User.WindowsUsername);
        }
    }
}

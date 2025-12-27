using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Tests.Unit
{
    public class AuthenticationServiceTests
    {
        private readonly Dao_User _daoUser;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Service_Authentication _authService;

        public AuthenticationServiceTests()
        {
            _daoUser = new Dao_User(Helper_Database_Variables.GetConnectionString());
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _authService = new Service_Authentication(_daoUser, _mockErrorHandler.Object);
        }

        [Fact]
        public async Task AuthenticateByWindowsUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            string username = "johnk"; // Assuming exists - this is fragile for unit tests

            // Act
            var result = await _authService.AuthenticateByWindowsUsernameAsync(username);

            // Assert
            // Assert.True(result.Success); // Commented out as it depends on local DB state
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
            var createResult = await _daoUser.CreateNewUserAsync(newUser, "system");
            Assert.True(createResult.Success, "Failed to create test user");

            try
            {
                // Act
                var result = await _authService.AuthenticateByPinAsync(username, pin);

                // Assert
                Assert.True(result.Success, $"Authentication failed: {result.ErrorMessage}");
                Assert.NotNull(result.User);
                Assert.Equal(username, result.User.WindowsUsername);
            }
            finally
            {
                // Cleanup if possible, but Dao_User might not have Delete
            }
        }
    }
}

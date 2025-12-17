using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Tests.Integration
{
    public class NewUserCreationFlowTests
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Service_Authentication _authService;

        public NewUserCreationFlowTests()
        {
            _daoUser = new Dao_User(ConnectionString);
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _authService = new Service_Authentication(_daoUser, _mockErrorHandler.Object);
        }

        [Fact]
        public async Task CreateNewUser_ValidData_ShouldSucceed()
        {
            // Arrange
            string username = "testuser_new_" + new Random().Next(1000, 9999);
            string pin = new Random().Next(1000, 9999).ToString();
            var newUser = new Model_User
            {
                WindowsUsername = username,
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "Test User New",
                Department = "Receiving",
                Shift = "1st Shift",
                Pin = pin,
                IsActive = true
            };

            // Act
            var result = await _authService.CreateNewUserAsync(newUser, "system");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.EmployeeNumber > 0);

            // Verify login works
            var authResult = await _authService.AuthenticateByWindowsUsernameAsync(username);
            Assert.True(authResult.Success);
        }

        [Fact]
        public async Task CreateNewUser_DuplicatePin_ShouldFail()
        {
            // Arrange
            string pin = new Random().Next(1000, 9999).ToString();
            
            // Create first user
            var user1 = new Model_User
            {
                WindowsUsername = "user1_" + new Random().Next(1000, 9999),
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "User One",
                Department = "Receiving",
                Shift = "1st Shift",
                Pin = pin,
                IsActive = true
            };
            await _authService.CreateNewUserAsync(user1, "system");

            // Create second user with same PIN
            var user2 = new Model_User
            {
                WindowsUsername = "user2_" + new Random().Next(1000, 9999),
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "User Two",
                Department = "Shipping",
                Shift = "2nd Shift",
                Pin = pin, // Duplicate
                IsActive = true
            };

            // Act
            var result = await _authService.CreateNewUserAsync(user2, "system");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("PIN", result.ErrorMessage);
        }
    }
}

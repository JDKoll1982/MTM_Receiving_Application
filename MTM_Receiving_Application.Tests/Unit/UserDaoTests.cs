using System;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Tests.Unit
{
    public class UserDaoTests
    {
        private const string ConnectionString = "Server=172.16.1.104;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;

        public UserDaoTests()
        {
            _daoUser = new Dao_User(ConnectionString);
        }

        [Fact]
        public async Task GetUserByWindowsUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            // Assuming 'johnk' exists from manual testing or seed data
            string username = "johnk";

            // Act
            var result = await _daoUser.GetUserByWindowsUsernameAsync(username);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(username, result.Data.WindowsUsername);
        }

        [Fact]
        public async Task GetUserByWindowsUsernameAsync_ShouldReturnError_WhenUserDoesNotExist()
        {
            // Arrange
            string username = "fakeuser";

            // Act
            var result = await _daoUser.GetUserByWindowsUsernameAsync(username);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("No record found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        [Fact]
        public async Task ValidateUserPinAsync_ShouldReturnUser_WhenPinIsValid()
        {
            // Arrange
            string username = "testuser_pin_" + Guid.NewGuid().ToString().Substring(0, 8);
            string pin = "";
            int attempts = 0;
            do
            {
                pin = new Random().Next(1000, 9999).ToString();
                var uniqueCheck = await _daoUser.IsPinUniqueAsync(pin);
                if (uniqueCheck.Success && uniqueCheck.Data) break;
                attempts++;
            } while (attempts < 10);

            var newUser = new Model_User
            {
                WindowsUsername = username,
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "Test User Pin",
                Department = "Receiving",
                Shift = "1st Shift",
                Pin = pin,
                IsActive = true
            };
            var createResult = await _daoUser.CreateNewUserAsync(newUser, "system");
            Assert.True(createResult.Success, $"User creation failed: {createResult.ErrorMessage}");

            // Act
            // Note: sp_ValidateUserPin checks against windows_username OR full_name
            // Ensure we are passing the exact username that was created
            var result = await _daoUser.ValidateUserPinAsync(username, pin);

            // Assert
            Assert.True(result.Success, $"Validation failed: {result.ErrorMessage}. Username: {username}, Pin: {pin}");
            Assert.NotNull(result.Data);
            Assert.Equal(username, result.Data.WindowsUsername);
        }

        [Fact]
        public async Task ValidateUserPinAsync_ShouldReturnError_WhenPinIsInvalid()
        {
            // Arrange
            string username = "johnk";
            string pin = "0000"; // Invalid PIN

            // Act
            var result = await _daoUser.ValidateUserPinAsync(username, pin);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("No record found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task IsPinUniqueAsync_ShouldReturnFalse_WhenPinExists()
        {
            // Arrange
            string pin = new Random().Next(1000, 9999).ToString();
            // Create a user with this PIN first
            var newUser = new Model_User
            {
                WindowsUsername = "testuser_unique_" + new Random().Next(1000, 9999),
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "Test User Unique",
                Department = "Receiving",
                Shift = "1st Shift",
                Pin = pin,
                IsActive = true
            };
            await _daoUser.CreateNewUserAsync(newUser, "system");

            // Act
            var result = await _daoUser.IsPinUniqueAsync(pin);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task IsPinUniqueAsync_ShouldReturnTrue_WhenPinIsNew()
        {
            // Arrange
            string pin = "9999"; // Assuming 9999 does not exist

            // Act
            var result = await _daoUser.IsPinUniqueAsync(pin);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }
    }
}

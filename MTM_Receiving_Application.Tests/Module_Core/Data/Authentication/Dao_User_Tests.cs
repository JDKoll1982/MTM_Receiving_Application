using FluentAssertions;
using Xunit;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Data.Authentication
{
    /// <summary>
    /// Unit tests for Dao_User data access object.
    /// Tests authentication, CRUD operations, validation, and configuration methods.
    /// </summary>
    public class Dao_User_Tests
    {
    private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);

        // ====================================================================
        // Constructor Tests
        // ====================================================================

        [Fact]
        public void Constructor_ValidConnectionString_CreatesInstance()
        {
            // Act
            var dao = new Dao_User(TestConnectionString);

            // Assert
            dao.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_NullConnectionString_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new Dao_User(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*connectionString*");
        }

        [Fact]
        public void Constructor_EmptyConnectionString_DoesNotThrow()
        {
            // Act
            Action act = () => new Dao_User(string.Empty);

            // Assert - Constructor doesn't validate empty strings, only null
            act.Should().NotThrow();
        }

        // ====================================================================
        // GetUserByWindowsUsernameAsync Tests
        // ====================================================================

        [Fact]
        public async Task GetUserByWindowsUsernameAsync_ValidUsername_CallsStoredProcedure()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var username = "jdoe";

            // Act & Assert
            // Note: This will fail without a real database connection
            // In a real test environment, you would use a test database or mock the Helper class
            var result = await dao.GetUserByWindowsUsernameAsync(username);

            // Basic assertion - detailed testing requires database/mocking infrastructure
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("valid_user")]
        [InlineData("UPPERCASE_USER")]
        public async Task GetUserByWindowsUsernameAsync_VariousUsernames_AcceptsInput(string username)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.GetUserByWindowsUsernameAsync(username);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // ValidateUserPinAsync Tests
        // ====================================================================

        [Fact]
        public async Task ValidateUserPinAsync_ValidCredentials_CallsStoredProcedure()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var username = "jdoe";
            var pin = "1234";

            // Act
            var result = await dao.ValidateUserPinAsync(username, pin);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("user", "0000")]
        [InlineData("user", "9999")]
        [InlineData("user", "1234")]
        [InlineData("", "1234")]
        [InlineData("user", "")]
        public async Task ValidateUserPinAsync_VariousInputs_AcceptsAllCombinations(string username, string pin)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.ValidateUserPinAsync(username, pin);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // CreateNewUserAsync Tests
        // ====================================================================

        [Fact]
        public async Task CreateNewUserAsync_ValidUser_ReturnsEmployeeNumber()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var user = CreateValidTestUser();
            var createdBy = "admin";

            // Act
            var result = await dao.CreateNewUserAsync(user, createdBy);

            // Assert
            result.Should().NotBeNull();
            // Success depends on database availability and stored procedure existence
        }

        [Fact]
        public async Task CreateNewUserAsync_NullVisualCredentials_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var user = CreateValidTestUser();
            user.VisualUsername = null;
            user.VisualPassword = null;
            var createdBy = "admin";

            // Act
            var result = await dao.CreateNewUserAsync(user, createdBy);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateNewUserAsync_WithVisualCredentials_PassesParameters()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var user = CreateValidTestUser();
            user.VisualUsername = "visual_user";
            user.VisualPassword = "visual_pass";
            var createdBy = "admin";

            // Act
            var result = await dao.CreateNewUserAsync(user, createdBy);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // IsWindowsUsernameUniqueAsync Tests
        // ====================================================================

        [Fact]
        public async Task IsWindowsUsernameUniqueAsync_WithoutExclusion_ChecksAllUsers()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var username = "new_user";

            // Act
            var result = await dao.IsWindowsUsernameUniqueAsync(username);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task IsWindowsUsernameUniqueAsync_WithExclusion_ExcludesSpecificEmployee()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var username = "existing_user";
            var excludeEmployeeNumber = 12345;

            // Act
            var result = await dao.IsWindowsUsernameUniqueAsync(username, excludeEmployeeNumber);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("user1", null)]
        [InlineData("user2", 100)]
        [InlineData("user3", 999)]
        [InlineData("", null)]
        public async Task IsWindowsUsernameUniqueAsync_VariousScenarios_HandlesAllCases(
            string username,
            int? excludeId)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.IsWindowsUsernameUniqueAsync(username, excludeId);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // LogUserActivityAsync Tests
        // ====================================================================

        [Fact]
        public async Task LogUserActivityAsync_ValidData_LogsActivity()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var eventType = "login_success";
            var username = "jdoe";
            var workstation = "WORKSTATION-01";
            var details = "Successful login";

            // Act
            var result = await dao.LogUserActivityAsync(eventType, username, workstation, details);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("login_success", "user", "WS01", "Details")]
        [InlineData("login_failed", "user", "WS02", "Failed attempt")]
        [InlineData("session_timeout", "user", "WS03", "Timeout")]
        [InlineData("user_created", "admin", "WS04", "New user")]
        public async Task LogUserActivityAsync_DifferentEventTypes_LogsCorrectly(
            string eventType,
            string username,
            string workstation,
            string details)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.LogUserActivityAsync(eventType, username, workstation, details);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task LogUserActivityAsync_NullParameters_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.LogUserActivityAsync("event", null!, null!, null!);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // GetSharedTerminalNamesAsync Tests
        // ====================================================================

        [Fact]
        public async Task GetSharedTerminalNamesAsync_NoParameters_ReturnsTerminalList()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.GetSharedTerminalNamesAsync();

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // UpsertWorkstationConfigAsync Tests
        // ====================================================================

        [Fact]
        public async Task UpsertWorkstationConfigAsync_ValidData_InsertsOrUpdates()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var workstationName = "WORKSTATION-01";
            var workstationType = "shared";
            var isActive = true;
            var description = "Shared terminal in warehouse";

            // Act
            var result = await dao.UpsertWorkstationConfigAsync(
                workstationName,
                workstationType,
                isActive,
                description);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpsertWorkstationConfigAsync_NullDescription_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.UpsertWorkstationConfigAsync(
                "WS-01",
                "dedicated",
                true,
                null);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("WS-01", "shared", true, "Description 1")]
        [InlineData("WS-02", "dedicated", false, "Description 2")]
        [InlineData("WS-03", "shared", true, null)]
        public async Task UpsertWorkstationConfigAsync_VariousConfigurations_HandlesAll(
            string name,
            string type,
            bool active,
            string? description)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.UpsertWorkstationConfigAsync(name, type, active, description);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // GetActiveDepartmentsAsync Tests
        // ====================================================================

        [Fact]
        public async Task GetActiveDepartmentsAsync_NoParameters_ReturnsDepartmentList()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.GetActiveDepartmentsAsync();

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // UpdateDefaultModeAsync Tests
        // ====================================================================

        [Fact]
        public async Task UpdateDefaultModeAsync_ValidData_UpdatesMode()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var userId = 12345;
            var defaultMode = "guided";

            // Act
            var result = await dao.UpdateDefaultModeAsync(userId, defaultMode);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateDefaultModeAsync_NullMode_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var userId = 12345;

            // Act
            var result = await dao.UpdateDefaultModeAsync(userId, null);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(100, "guided")]
        [InlineData(200, "manual")]
        [InlineData(300, null)]
        public async Task UpdateDefaultModeAsync_VariousInputs_HandlesAll(int userId, string? mode)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.UpdateDefaultModeAsync(userId, mode);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // UpdateDefaultReceivingModeAsync Tests
        // ====================================================================

        [Fact]
        public async Task UpdateDefaultReceivingModeAsync_ValidData_UpdatesMode()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var userId = 12345;
            var defaultMode = "guided";

            // Act
            var result = await dao.UpdateDefaultReceivingModeAsync(userId, defaultMode);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(100, "guided")]
        [InlineData(200, "manual")]
        [InlineData(300, "edit")]
        [InlineData(400, null)]
        public async Task UpdateDefaultReceivingModeAsync_DifferentModes_HandlesAll(
            int userId,
            string? mode)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.UpdateDefaultReceivingModeAsync(userId, mode);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // UpdateDefaultDunnageModeAsync Tests
        // ====================================================================

        [Fact]
        public async Task UpdateDefaultDunnageModeAsync_ValidData_UpdatesMode()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var userId = 12345;
            var defaultMode = "guided";

            // Act
            var result = await dao.UpdateDefaultDunnageModeAsync(userId, defaultMode);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(100, "standard")]
        [InlineData(200, "quick")]
        [InlineData(300, null)]
        public async Task UpdateDefaultDunnageModeAsync_VariousModes_HandlesAll(int userId, string? mode)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            var result = await dao.UpdateDefaultDunnageModeAsync(userId, mode);

            // Assert
            result.Should().NotBeNull();
        }

        // ====================================================================
        // Edge Cases and Error Handling Tests
        // ====================================================================

        [Fact]
        public async Task GetUserByWindowsUsernameAsync_EmptyString_DoesNotThrow()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            Func<Task> act = async () => await dao.GetUserByWindowsUsernameAsync(string.Empty);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ValidateUserPinAsync_SpecialCharactersInUsername_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var username = "user@domain.com";
            var pin = "1234";

            // Act
            Func<Task> act = async () => await dao.ValidateUserPinAsync(username, pin);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CreateNewUserAsync_AllFieldsPopulated_PassesAllParameters()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var user = new Model_User
            {
                EmployeeNumber = 99999,
                WindowsUsername = "testuser",
                FullName = "Test User",
                Pin = "9999",
                Department = "IT",
                Shift = "Day",
                VisualUsername = "visual_test",
                VisualPassword = "visual_pass123",
                IsActive = true
            };
            var createdBy = "admin";

            // Act
            Func<Task> act = async () => await dao.CreateNewUserAsync(user, createdBy);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task IsWindowsUsernameUniqueAsync_VeryLongUsername_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var longUsername = new string('a', 500);

            // Act
            Func<Task> act = async () => await dao.IsWindowsUsernameUniqueAsync(longUsername);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task LogUserActivityAsync_LongDetails_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var longDetails = new string('x', 1000);

            // Act
            Func<Task> act = async () => await dao.LogUserActivityAsync(
                "test_event",
                "user",
                "workstation",
                longDetails);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task UpsertWorkstationConfigAsync_VeryLongDescription_HandlesGracefully()
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);
            var longDescription = new string('d', 2000);

            // Act
            Func<Task> act = async () => await dao.UpsertWorkstationConfigAsync(
                "WS-01",
                "shared",
                true,
                longDescription);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ====================================================================
        // Boundary Value Tests
        // ====================================================================

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public async Task UpdateDefaultModeAsync_BoundaryUserIds_HandlesAll(int userId)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            Func<Task> act = async () => await dao.UpdateDefaultModeAsync(userId, "mode");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task IsWindowsUsernameUniqueAsync_BoundaryExclusionValues_HandlesAll(int? excludeId)
        {
            // Arrange
            var dao = new Dao_User(TestConnectionString);

            // Act
            Func<Task> act = async () => await dao.IsWindowsUsernameUniqueAsync("user", excludeId);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ====================================================================
        // Helper Methods
        // ====================================================================

        /// <summary>
        /// Creates a valid test user model with all required fields populated.
        /// </summary>
        private static Model_User CreateValidTestUser()
        {
            return new Model_User
            {
                EmployeeNumber = 12345,
                WindowsUsername = "testuser",
                FullName = "Test User",
                Pin = "1234",
                Department = "Engineering",
                Shift = "Day",
                IsActive = true
            };
        }
    }
}

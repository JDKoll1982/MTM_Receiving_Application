using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.Systems;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Tests.Integration
{
    public class WindowsAuthenticationFlowTests
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Mock<IDispatcherService> _mockDispatcherService;
        private readonly Mock<IDispatcherTimer> _mockTimer;
        private readonly Service_Authentication _authService;
        private readonly Service_UserSessionManager _sessionManager;

        public WindowsAuthenticationFlowTests()
        {
            _daoUser = new Dao_User(ConnectionString);
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _mockDispatcherService = new Mock<IDispatcherService>();
            _mockTimer = new Mock<IDispatcherTimer>();

            _mockDispatcherService.Setup(d => d.CreateTimer()).Returns(_mockTimer.Object);

            _authService = new Service_Authentication(_daoUser, _mockErrorHandler.Object);
            _sessionManager = new Service_UserSessionManager(_daoUser, _mockDispatcherService.Object);
        }

        [Fact]
        public async Task PersonalWorkstation_ExistingUser_ShouldAutoLogin()
        {
            // Arrange
            string username = "testuser_auto_" + new Random().Next(1000, 9999);
            var newUser = new Model_User
            {
                WindowsUsername = username,
                EmployeeNumber = new Random().Next(10000, 99999),
                FullName = "Test User Auto",
                Department = "Receiving",
                Shift = "1st Shift",
                Pin = new Random().Next(1000, 9999).ToString(),
                IsActive = true
            };
            await _daoUser.CreateNewUserAsync(newUser, "system");

            // Simulate Personal Workstation detection
            var workstationConfig = new Model_WorkstationConfig 
            { 
                ComputerName = "PERSONAL-PC", 
                WorkstationType = "personal_workstation" 
            };

            // Act
            // 1. Authenticate
            var authResult = await _authService.AuthenticateByWindowsUsernameAsync(username);
            
            // 2. Create Session
            Model_UserSession? session = null;
            if (authResult.Success && authResult.User != null)
            {
                session = _sessionManager.CreateSession(authResult.User, workstationConfig, "Windows");
            }

            // Assert
            Assert.True(authResult.Success);
            Assert.NotNull(authResult.User);
            Assert.Equal(username, authResult.User.WindowsUsername);
            
            Assert.NotNull(session);
            Assert.Equal(username, session.User.WindowsUsername);
            Assert.Equal("personal_workstation", session.WorkstationType);
            Assert.Equal(TimeSpan.FromMinutes(30), session.TimeoutDuration);
        }
    }
}

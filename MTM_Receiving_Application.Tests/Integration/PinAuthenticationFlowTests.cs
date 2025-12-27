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
    public class PinAuthenticationFlowTests
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Mock<IDispatcherService> _mockDispatcherService;
        private readonly Mock<IDispatcherTimer> _mockTimer;
        private readonly Service_Authentication _authService;
        private readonly Service_UserSessionManager _sessionManager;

        public PinAuthenticationFlowTests()
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
        public async Task SharedTerminal_ValidCredentials_ShouldLogin()
        {
            // Arrange
            string username = "testuser_pin_" + new Random().Next(1000, 9999);
            string pin = new Random().Next(1000, 9999).ToString();
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
            await _daoUser.CreateNewUserAsync(newUser, "system");

            var workstationConfig = new Model_WorkstationConfig 
            { 
                ComputerName = "SHARED-PC", 
                WorkstationType = "shared_terminal" 
            };

            // Act
            var authResult = await _authService.AuthenticateByPinAsync(username, pin);
            
            Model_UserSession? session = null;
            if (authResult.Success && authResult.User != null)
            {
                session = _sessionManager.CreateSession(authResult.User, workstationConfig, "PIN");
            }

            // Assert
            Assert.True(authResult.Success);
            Assert.NotNull(session);
            Assert.Equal(username, session.User.WindowsUsername);
            Assert.Equal("shared_terminal", session.WorkstationType);
            Assert.Equal(TimeSpan.FromMinutes(15), session.TimeoutDuration);
        }

        [Fact]
        public async Task SharedTerminal_InvalidCredentials_ShouldFail()
        {
            // Arrange
            string username = "testuser_pin_fail_" + new Random().Next(1000, 9999);
            string pin = "1234";
            // User not created

            // Act
            var authResult = await _authService.AuthenticateByPinAsync(username, pin);

            // Assert
            Assert.False(authResult.Success);
            Assert.Null(authResult.User);
        }
    }
}

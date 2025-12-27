using System;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.Systems;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Tests.Unit
{
    public class SessionManagerTests
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;
        private readonly Mock<IDispatcherService> _mockDispatcherService;
        private readonly Mock<IDispatcherTimer> _mockTimer;
        private readonly Service_UserSessionManager _sessionManager;

        public SessionManagerTests()
        {
            _daoUser = new Dao_User(ConnectionString);
            _mockDispatcherService = new Mock<IDispatcherService>();
            _mockTimer = new Mock<IDispatcherTimer>();

            _mockDispatcherService.Setup(d => d.CreateTimer()).Returns(_mockTimer.Object);

            _sessionManager = new Service_UserSessionManager(_daoUser, _mockDispatcherService.Object);
        }

        [Fact]
        public void CreateSession_ShouldInitializeSession()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "test", EmployeeNumber = 123 };
            var config = new Model_WorkstationConfig { ComputerName = "TEST-PC", WorkstationType = "personal_workstation" };

            // Act
            var session = _sessionManager.CreateSession(user, config, "Windows");

            // Assert
            Assert.NotNull(session);
            Assert.Equal(user, session.User);
            Assert.Equal(TimeSpan.FromMinutes(30), session.TimeoutDuration);
            Assert.NotNull(_sessionManager.CurrentSession);
        }

        [Fact]
        public void IsSessionTimedOut_ShouldReturnTrue_WhenTimeElapsed()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "test", EmployeeNumber = 123 };
            var config = new Model_WorkstationConfig { ComputerName = "TEST-PC", WorkstationType = "personal_workstation" };
            var session = _sessionManager.CreateSession(user, config, "Windows");
            
            // Manually set TimeoutDuration to a very small value to simulate timeout condition logic
            // Note: IsSessionTimedOut relies on TimeSinceLastActivity >= TimeoutDuration
            // We can't easily mock DateTime.Now inside the model without more refactoring,
            // but we can set TimeoutDuration to TimeSpan.Zero.
            session.TimeoutDuration = TimeSpan.Zero;
            
            // Act
            // Wait a tiny bit to ensure TimeSinceLastActivity > 0
            System.Threading.Thread.Sleep(10);
            bool isTimedOut = _sessionManager.IsSessionTimedOut();

            // Assert
            Assert.True(isTimedOut);
        }

        [Fact]
        public void StartTimeoutMonitoring_ShouldStartTimer()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "test", EmployeeNumber = 123 };
            var config = new Model_WorkstationConfig { ComputerName = "TEST-PC", WorkstationType = "personal_workstation" };
            _sessionManager.CreateSession(user, config, "Windows");

            // Act
            _sessionManager.StartTimeoutMonitoring();

            // Assert
            _mockDispatcherService.Verify(d => d.CreateTimer(), Times.Once);
            _mockTimer.VerifySet(t => t.Interval = It.IsAny<TimeSpan>());
            _mockTimer.VerifySet(t => t.IsRepeating = true);
            _mockTimer.Verify(t => t.Start(), Times.Once);
        }

        [Fact]
        public void StopTimeoutMonitoring_ShouldStopTimer()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "test", EmployeeNumber = 123 };
            var config = new Model_WorkstationConfig { ComputerName = "TEST-PC", WorkstationType = "personal_workstation" };
            _sessionManager.CreateSession(user, config, "Windows");
            _sessionManager.StartTimeoutMonitoring();

            // Act
            _sessionManager.StopTimeoutMonitoring();

            // Assert
            _mockTimer.Verify(t => t.Stop(), Times.Once);
        }

        [Fact]
        public void UpdateLastActivity_ShouldResetTimer()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "test", EmployeeNumber = 123 };
            var config = new Model_WorkstationConfig { ComputerName = "TEST-PC", WorkstationType = "personal_workstation" };
            var session = _sessionManager.CreateSession(user, config, "Windows");
            
            // Simulate some time passing (conceptually)
            var initialActivity = session.LastActivityTimestamp;
            System.Threading.Thread.Sleep(10); // Ensure clock ticks

            // Act
            _sessionManager.UpdateLastActivity();

            // Assert
            Assert.True(session.LastActivityTimestamp > initialActivity);
            Assert.False(_sessionManager.IsSessionTimedOut());
        }
    }
}

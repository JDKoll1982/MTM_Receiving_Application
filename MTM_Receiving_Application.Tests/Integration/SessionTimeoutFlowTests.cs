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
    public class SessionTimeoutFlowTests
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
        private readonly Dao_User _daoUser;
        private readonly Mock<IService_Dispatcher> _mockDispatcherService;
        private readonly Mock<IService_DispatcherTimer> _mockTimer;
        private readonly Service_UserSessionManager _sessionManager;

        public SessionTimeoutFlowTests()
        {
            _daoUser = new Dao_User(ConnectionString);
            _mockDispatcherService = new Mock<IService_Dispatcher>();
            _mockTimer = new Mock<IService_DispatcherTimer>();

            _mockDispatcherService.Setup(d => d.CreateTimer()).Returns(_mockTimer.Object);

            _sessionManager = new Service_UserSessionManager(_daoUser, _mockDispatcherService.Object);
        }

        [Fact]
        public void SessionTimeout_ShouldFireEvent()
        {
            // Arrange
            var user = new Model_User { WindowsUsername = "test", EmployeeNumber = 123 };
            var config = new Model_WorkstationConfig { ComputerName = "TEST-PC", WorkstationType = "personal_workstation" };
            var session = _sessionManager.CreateSession(user, config, "Windows");
            
            bool eventFired = false;
            _sessionManager.SessionTimedOut += (s, e) => eventFired = true;

            _sessionManager.StartTimeoutMonitoring();

            // Force timeout condition
            session.TimeoutDuration = TimeSpan.Zero;
            System.Threading.Thread.Sleep(10);

            // Act
            // Simulate timer tick
            _mockTimer.Raise(t => t.Tick += null, _mockTimer.Object, new object());

            // Assert
            Assert.True(eventFired);
        }
    }
}

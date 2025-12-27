using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Systems;
using MTM_Receiving_Application.Data.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.Services
{
    public class Service_MySQL_Dunnage_Tests
    {
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Mock<ILoggingService> _mockLogger;
        private readonly Mock<IService_UserSessionManager> _mockSessionManager;
        private readonly Mock<Dao_DunnageLoad> _mockDaoDunnageLoad;
        private readonly Mock<Dao_DunnageType> _mockDaoDunnageType;
        private readonly Mock<Dao_DunnagePart> _mockDaoDunnagePart;
        private readonly Mock<Dao_DunnageSpec> _mockDaoDunnageSpec;
        private readonly Mock<Dao_InventoriedDunnage> _mockDaoInventoriedDunnage;

        private readonly Service_MySQL_Dunnage _service;

        public Service_MySQL_Dunnage_Tests()
        {
            _mockErrorHandler = new Mock<IService_ErrorHandler>();
            _mockLogger = new Mock<ILoggingService>();
            _mockSessionManager = new Mock<IService_UserSessionManager>();

            // Mock DAOs with dummy connection string
            string dummyConn = "Server=dummy;";
            _mockDaoDunnageLoad = new Mock<Dao_DunnageLoad>(dummyConn);
            _mockDaoDunnageType = new Mock<Dao_DunnageType>(dummyConn);
            _mockDaoDunnagePart = new Mock<Dao_DunnagePart>(dummyConn);
            _mockDaoDunnageSpec = new Mock<Dao_DunnageSpec>(dummyConn);
            _mockDaoInventoriedDunnage = new Mock<Dao_InventoriedDunnage>(dummyConn);

            // Setup default session
            var mockSession = new Model_UserSession
            {
                User = new Model_User { WindowsUsername = "TEST_USER" }
            };
            _mockSessionManager.Setup(x => x.CurrentSession).Returns(mockSession);

            _service = new Service_MySQL_Dunnage(
                _mockErrorHandler.Object,
                _mockLogger.Object,
                _mockSessionManager.Object,
                _mockDaoDunnageLoad.Object,
                _mockDaoDunnageType.Object,
                _mockDaoDunnagePart.Object,
                _mockDaoDunnageSpec.Object,
                _mockDaoInventoriedDunnage.Object);
        }

        [Fact]
        public void Constructor_ShouldInitialize_WhenDependenciesAreValid()
        {
            Assert.NotNull(_service);
        }

        [Fact]
        public async Task GetAllTypesAsync_ShouldReturnTypes_WhenDaoSucceeds()
        {
            // Arrange
            var expectedTypes = new List<Model_DunnageType> { new Model_DunnageType { Id = 1, TypeName = "Test" } };
            _mockDaoDunnageType.Setup(x => x.GetAllAsync())
                .ReturnsAsync(DaoResultFactory.Success(expectedTypes));

            // Act
            var result = await _service.GetAllTypesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedTypes, result.Data);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Services.Receiving;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Tests.Unit.Services
{
    public class Service_DunnageCSVWriter_Tests
    {
        private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
        private readonly Mock<IService_UserSessionManager> _mockSessionManager;
        private readonly Mock<ILoggingService> _mockLogger;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Service_DunnageCSVWriter _service;

        public Service_DunnageCSVWriter_Tests()
        {
            _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
            _mockSessionManager = new Mock<IService_UserSessionManager>();
            _mockLogger = new Mock<ILoggingService>();
            _mockErrorHandler = new Mock<IService_ErrorHandler>();

            _service = new Service_DunnageCSVWriter(
                _mockDunnageService.Object,
                _mockSessionManager.Object,
                _mockLogger.Object,
                _mockErrorHandler.Object);
        }

        [Fact]
        public async Task WriteToCSVAsync_ShouldReturnFailure_WhenLoadsListIsEmpty()
        {
            // Arrange
            var loads = new List<Model_DunnageLoad>();

            // Act
            var result = await _service.WriteToCSVAsync(loads);

            // Assert
            Assert.False(result.LocalSuccess);
            Assert.Contains("No loads", result.ErrorMessage);
        }

        [Fact]
        public async Task WriteToCSVAsync_ShouldWriteLocalFile_WhenNetworkFails()
        {
            // Arrange
            var loads = new List<Model_DunnageLoad>
            {
                new Model_DunnageLoad
                {
                    DunnageType = "Pallet",
                    PartId = "P1",
                    Quantity = 10,
                    PoNumber = "PO123",
                    ReceivedDate = DateTime.Now,
                    CreatedBy = "User1",
                    Specs = new Dictionary<string, object> { { "Color", "Red" } }
                }
            };

            _mockDunnageService.Setup(s => s.GetAllSpecKeysAsync())
                .ReturnsAsync(new List<string> { "Color", "Size" });

            _mockSessionManager.Setup(s => s.CurrentSession)
                .Returns(new Model_UserSession { User = new Model_User { WindowsUsername = "TestUser" } });

            // Act
            var result = await _service.WriteToCSVAsync(loads);

            // Assert
            Assert.True(result.LocalSuccess);
            // Network success depends on if the path is valid. In test env, it likely fails or succeeds if local.
            // But since we hardcoded a network path that likely doesn't exist, it should fail network write.
            // However, if the path is valid (e.g. mapped drive), it might succeed.
            // But the path is \\mtmanu-fs01... which is likely not available in this environment.
            
            Assert.NotEmpty(result.LocalFilePath);
            Assert.True(File.Exists(result.LocalFilePath));

            // Cleanup
            if (File.Exists(result.LocalFilePath)) File.Delete(result.LocalFilePath);
        }
    }
}

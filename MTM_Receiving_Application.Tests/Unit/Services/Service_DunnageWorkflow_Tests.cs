using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Services.Receiving;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Tests.Unit.Services
{
    public class Service_DunnageWorkflow_Tests
    {
        private readonly Mock<IService_MySQL_Dunnage> _mockDunnageService;
        private readonly Mock<IService_DunnageCSVWriter> _mockCsvWriter;
        private readonly Mock<IService_UserSessionManager> _mockSessionManager;
        private readonly Mock<IService_LoggingUtility> _mockLogger;
        private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
        private readonly Service_DunnageWorkflow _service;

        public Service_DunnageWorkflow_Tests()
        {
            _mockDunnageService = new Mock<IService_MySQL_Dunnage>();
            _mockCsvWriter = new Mock<IService_DunnageCSVWriter>();
            _mockSessionManager = new Mock<IService_UserSessionManager>();
            _mockLogger = new Mock<IService_LoggingUtility>();
            _mockErrorHandler = new Mock<IService_ErrorHandler>();

            _service = new Service_DunnageWorkflow(
                _mockDunnageService.Object,
                _mockCsvWriter.Object,
                _mockSessionManager.Object,
                _mockLogger.Object,
                _mockErrorHandler.Object);
        }

        [Fact]
        public async Task StartWorkflowAsync_ShouldResetSessionAndStep()
        {
            // Act
            await _service.StartWorkflowAsync();

            // Assert
            Assert.Equal(Enum_DunnageWorkflowStep.ModeSelection, _service.CurrentStep);
            Assert.NotNull(_service.CurrentSession);
            Assert.Empty(_service.CurrentSession.Loads);
        }

        [Fact]
        public async Task AdvanceToNextStepAsync_ShouldFail_WhenTypeNotSelected()
        {
            // Arrange
            _service.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
            _service.CurrentSession.SelectedTypeId = 0;

            // Act
            var result = await _service.AdvanceToNextStepAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("select a dunnage type", result.ErrorMessage);
        }

        [Fact]
        public async Task SaveSessionAsync_ShouldCallDbAndCsv()
        {
            // Arrange
            _service.CurrentSession.SelectedPart = new Model_DunnagePart { PartId = "P1", SpecValuesDict = new Dictionary<string, object>() };
            _service.CurrentSession.Quantity = 10;
            _service.CurrentSession.SelectedTypeName = "Pallet";

            _mockSessionManager.Setup(s => s.CurrentSession)
                .Returns(new Model_UserSession { User = new Model_User { WindowsUsername = "TestUser" } });

            _mockDunnageService.Setup(s => s.SaveLoadsAsync(It.IsAny<List<Model_DunnageLoad>>()))
                .ReturnsAsync(DaoResultFactory.Success());

            _mockCsvWriter.Setup(s => s.WriteToCSVAsync(It.IsAny<List<Model_DunnageLoad>>()))
                .ReturnsAsync(new Model_CSVWriteResult { LocalSuccess = true });

            // Act
            var result = await _service.SaveSessionAsync();

            // Assert
            Assert.True(result.IsSuccess);
            _mockDunnageService.Verify(s => s.SaveLoadsAsync(It.IsAny<List<Model_DunnageLoad>>()), Times.Once);
            _mockCsvWriter.Verify(s => s.WriteToCSVAsync(It.IsAny<List<Model_DunnageLoad>>()), Times.Once);
        }
    }
}


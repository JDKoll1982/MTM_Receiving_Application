using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Database
{
    /// <summary>
    /// Unit tests for Service_ErrorHandler
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Service")]
    public class Service_ErrorHandler_Tests
    {
        private readonly Mock<IService_LoggingUtility> _mockLogger;
        private readonly Mock<IService_Window> _mockWindowService;
        private readonly Service_ErrorHandler _sut;

        public Service_ErrorHandler_Tests()
        {
            _mockLogger = new Mock<IService_LoggingUtility>();
            _mockWindowService = new Mock<IService_Window>();
            _sut = new Service_ErrorHandler(_mockLogger.Object, _mockWindowService.Object);
        }

        [Fact]
        public async Task LogErrorAsync_InfoSeverity_CallsLogInfo_Async()
        {
            // Act
            await _sut.LogErrorAsync("Test Info", Enum_ErrorSeverity.Info);

            // Assert
            _mockLogger.Verify(x => x.LogInfo("Test Info", It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task LogErrorAsync_ErrorSeverity_CallsLogError_Async()
        {
            // Arrange
            var ex = new Exception("Test Ex");

            // Act
            await _sut.LogErrorAsync("Test Error", Enum_ErrorSeverity.Error, ex);

            // Assert
            _mockLogger.Verify(x => x.LogError("Test Error", ex, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task HandleErrorAsync_ShowDialogFalse_OnlyLogs_Async()
        {
            // Act
            await _sut.HandleErrorAsync("Test", Enum_ErrorSeverity.Warning, null, false);

            // Assert
            _mockLogger.Verify(x => x.LogWarning("Test", It.IsAny<string?>()), Times.Once);
            _mockWindowService.Verify(x => x.GetXamlRoot(), Times.Never);
        }

        [Fact]
        public async Task HandleErrorAsync_ShowDialogTrue_LogsAndChecksWindow_Async()
        {
            // Arrange
            _mockWindowService.Setup(x => x.GetXamlRoot()).Returns((XamlRoot?)null); // Returrns null to avoid UI thread crash

            // Act
            await _sut.HandleErrorAsync("Test", Enum_ErrorSeverity.Error, null, true);

            // Assert
            _mockLogger.Verify(x => x.LogError("Test", null, It.IsAny<string?>()), Times.Once);
            _mockWindowService.Verify(x => x.GetXamlRoot(), Times.Once);
            // Since xamlRoot is null, we expect a warning log about it
            _mockLogger.Verify(x => x.LogWarning("Cannot show dialog - XamlRoot is null", It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task HandleDaoErrorAsync_SuccessResult_DoesNothing_Async()
        {
            // Arrange
            var result = Model_Dao_Result_Factory.Success();

            // Act
            await _sut.HandleDaoErrorAsync(result, "TestOp");

            // Assert
            _mockLogger.Verify(x => x.LogInfo(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
            _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task HandleDaoErrorAsync_FailureResult_LogsError_Async()
        {
            // Arrange
            var result = Model_Dao_Result_Factory.Failure("DB Fail", new Exception("SQL Error"));
            _mockWindowService.Setup(x => x.GetXamlRoot()).Returns((XamlRoot?)null);

            // Act
            await _sut.HandleDaoErrorAsync(result, "TestOp", true);

            // Assert
            _mockLogger.Verify(x => x.LogError(It.Is<string>(s => s.Contains("DB Fail") && s.Contains("TestOp")), It.IsAny<Exception>(), It.IsAny<string?>()), Times.Once);
        }
    }
}


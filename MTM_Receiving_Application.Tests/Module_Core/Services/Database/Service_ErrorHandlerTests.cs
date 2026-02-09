using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Database;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_ErrorHandlerTests
{
    private readonly Mock<IService_LoggingUtility> _logging;
    private readonly Mock<IService_Window> _window;
    private readonly Service_ErrorHandler _sut;

    public Service_ErrorHandlerTests()
    {
        _logging = new Mock<IService_LoggingUtility>();
        _window = new Mock<IService_Window>();
        _sut = new Service_ErrorHandler(_logging.Object, _window.Object);
    }

    [Fact]
    public async Task HandleErrorAsync_ShouldLogInfo_WhenSeverityIsInfo()
    {
        // Arrange

        // Act
        await _sut.HandleErrorAsync("Info message", Enum_ErrorSeverity.Info, showDialog: false);

        // Assert
        _logging.Verify(l => l.LogInfo("Info message"), Times.Once);
    }

    [Fact]
    public async Task HandleErrorAsync_ShouldLogWarning_WhenSeverityIsWarning()
    {
        // Arrange

        // Act
        await _sut.HandleErrorAsync("Warning message", Enum_ErrorSeverity.Warning, showDialog: false);

        // Assert
        _logging.Verify(l => l.LogWarning("Warning message"), Times.Once);
    }

    [Fact]
    public async Task HandleErrorAsync_ShouldLogError_WhenSeverityIsError()
    {
        // Arrange
        var exception = new InvalidOperationException("Failure");

        // Act
        await _sut.HandleErrorAsync("Error message", Enum_ErrorSeverity.Error, exception, showDialog: false);

        // Assert
        _logging.Verify(l => l.LogError("Error message", exception), Times.Once);
    }

    [Fact]
    public async Task HandleDaoErrorAsync_ShouldSkip_WhenResultIsSuccessful()
    {
        // Arrange
        var result = Model_Dao_Result_Factory.Success();

        // Act
        await _sut.HandleDaoErrorAsync(result, "Operation", showDialog: false);

        // Assert
        _logging.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception?>()), Times.Never);
    }
}

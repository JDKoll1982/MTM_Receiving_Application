using System.IO;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Database;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_LoggingUtilityTests
{
    [Fact]
    public void GetCurrentLogFilePath_ShouldReturnLogFilePath()
    {
        // Arrange
        var sut = new Service_LoggingUtility();

        // Act
        var path = sut.GetCurrentLogFilePath();

        // Assert
        path.Should().Contain("Logs");
    }

    [Fact]
    public void GetCurrentLogFilePath_ShouldUseAppLogPrefix()
    {
        // Arrange
        var sut = new Service_LoggingUtility();

        // Act
        var path = sut.GetCurrentLogFilePath();

        // Assert
        Path.GetFileName(path).Should().StartWith("app_");
    }

    [Fact]
    public void EnsureLogDirectoryExists_ShouldReturnTrue_WhenDirectoryIsAvailable()
    {
        // Arrange
        var sut = new Service_LoggingUtility();

        // Act
        var result = sut.EnsureLogDirectoryExists();

        // Assert
        result.Should().BeTrue();
    }
}

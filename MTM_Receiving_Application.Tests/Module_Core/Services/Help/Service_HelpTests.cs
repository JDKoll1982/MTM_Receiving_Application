using System;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Services.Help;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Help;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_HelpTests
{
    private readonly Service_Help _sut;

    public Service_HelpTests()
    {
        var window = new Mock<IService_Window>();
        var logger = new Mock<IService_LoggingUtility>();
        var dispatcher = new Mock<IService_Dispatcher>();
        var serviceProvider = new Mock<IServiceProvider>();

        _sut = new Service_Help(window.Object, logger.Object, dispatcher.Object, serviceProvider.Object);
    }

    [Fact]
    public void GetHelpContent_ShouldReturnContent_WhenKeyExists()
    {
        // Arrange

        // Act
        var content = _sut.GetHelpContent("Receiving.ModeSelection");

        // Assert
        content.Should().NotBeNull();
    }

    [Fact]
    public void SearchHelp_ShouldReturnMatches_WhenTermProvided()
    {
        // Arrange

        // Act
        var results = _sut.SearchHelp("Receiving");

        // Assert
        results.Should().NotBeEmpty();
    }
}

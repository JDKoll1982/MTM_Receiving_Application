using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_ShipRecTools_ToolSelectionTests
{
    [Fact]
    public void ActivateView_ShouldShowSharedSelectionStatus()
    {
        var notificationServiceMock = new Mock<IService_Notification>();
        var viewModel = new ViewModel_ShipRecTools_ToolSelection(
            new Mock<IService_ShipRecTools_Navigation>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            notificationServiceMock.Object
        );

        viewModel.ActivateView();

        notificationServiceMock.Verify(
            service =>
                service.ShowStatus(
                    "Choose a Ship/Rec tool to continue.",
                    MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Informational
                ),
            Times.Once
        );
    }

    [Fact]
    public void LoadTools_ShouldReplaceAllToolsCollection()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        navigationServiceMock
            .Setup(service => service.GetAllTools())
            .Returns(
                new List<Model_ToolDefinition>
                {
                    new() { ToolKey = "analysis-1", Title = "Analysis 1" },
                    new() { ToolKey = "utility-1", Title = "Utility 1" },
                }
            );

        var viewModel = new ViewModel_ShipRecTools_ToolSelection(
            navigationServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        viewModel.LoadTools();

        viewModel.AllTools.Should().HaveCount(2);
        viewModel
            .AllTools.Select(tool => tool.ToolKey)
            .Should()
            .ContainInOrder("analysis-1", "utility-1");
    }

    [Fact]
    public void SelectToolCommand_ShouldRaiseToolSelected_WhenToolKeyIsValid()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        var viewModel = new ViewModel_ShipRecTools_ToolSelection(
            navigationServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
        string? selectedToolKey = null;

        viewModel.ToolSelected += toolKey => selectedToolKey = toolKey;

        viewModel.SelectToolCommand.Execute("CustomerPullPack");

        selectedToolKey.Should().Be("CustomerPullPack");
    }

    [Fact]
    public void LoadTools_ShouldKeepWaitlistUtilityEntry_WhenProvidedByNavigationService()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        navigationServiceMock
            .Setup(service => service.GetAllTools())
            .Returns(
                new List<Model_ToolDefinition>
                {
                    new() { ToolKey = "CustomerPullPack", Title = "Customer Pull n' Pack" },
                    new()
                    {
                        ToolKey = "CustomerPullPackWaitlist",
                        Title = "Customer Pull n' Pack Waitlist",
                    },
                }
            );

        var viewModel = new ViewModel_ShipRecTools_ToolSelection(
            navigationServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        viewModel.LoadTools();

        viewModel
            .AllTools.Select(tool => tool.ToolKey)
            .Should()
            .Contain("CustomerPullPackWaitlist");
    }
}

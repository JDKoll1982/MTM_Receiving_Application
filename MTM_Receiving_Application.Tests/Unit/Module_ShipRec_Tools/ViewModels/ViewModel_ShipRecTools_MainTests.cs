using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_ShipRecTools_MainTests
{
    [Fact]
    public void NavigateToTool_ShouldRegisterHeaderBackAction_WhenToolBecomesActive()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        var headerBackServiceMock = new Mock<IService_HeaderBackNavigation>();
        navigationServiceMock
            .Setup(service => service.GetToolByKey("OutsideServiceHistory"))
            .Returns(
                new Model_ToolDefinition
                {
                    ToolKey = "OutsideServiceHistory",
                    Title = "Outside Service History",
                }
            );

        var viewModel = CreateViewModel(navigationServiceMock.Object, headerBackServiceMock.Object);

        viewModel.NavigateToTool("OutsideServiceHistory");

        viewModel.IsOutsideServiceHistoryVisible.Should().BeTrue();
        viewModel.IsToolSelectionVisible.Should().BeFalse();
        viewModel.CurrentHeaderTitle.Should().Be("Outside Service History");
        headerBackServiceMock.Verify(
            service => service.RegisterBackAction(It.IsAny<System.Func<Task>>(), "Back to Tools"),
            Times.Once
        );
    }

    [Fact]
    public void ShowToolSelectionCommand_ShouldClearHeaderBackAction()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        var headerBackServiceMock = new Mock<IService_HeaderBackNavigation>();
        navigationServiceMock
            .Setup(service => service.GetToolByKey("CustomerPullPack"))
            .Returns(
                new Model_ToolDefinition
                {
                    ToolKey = "CustomerPullPack",
                    Title = "Customer Pull n' Pack",
                }
            );

        var viewModel = CreateViewModel(navigationServiceMock.Object, headerBackServiceMock.Object);

        viewModel.NavigateToTool("CustomerPullPack");
        viewModel.ShowToolSelectionCommand.Execute(null);

        viewModel.IsToolSelectionVisible.Should().BeTrue();
        viewModel.CurrentHeaderTitle.Should().Be("Ship/Rec Tools");
        headerBackServiceMock.Verify(service => service.ClearBackAction(), Times.Once);
    }

    [Fact]
    public void NavigateToTool_ShouldShowWaitlistHost_WhenWaitlistToolIsSelected()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        var headerBackServiceMock = new Mock<IService_HeaderBackNavigation>();
        navigationServiceMock
            .Setup(service => service.GetToolByKey("CustomerPullPackWaitlist"))
            .Returns(
                new Model_ToolDefinition
                {
                    ToolKey = "CustomerPullPackWaitlist",
                    Title = "Customer Pull n' Pack Waitlist",
                }
            );

        var viewModel = CreateViewModel(navigationServiceMock.Object, headerBackServiceMock.Object);

        viewModel.NavigateToTool("CustomerPullPackWaitlist");

        viewModel.IsCustomerPullPackWaitlistVisible.Should().BeTrue();
        viewModel.IsCustomerPullPackVisible.Should().BeFalse();
        viewModel.CurrentHeaderTitle.Should().Be("Customer Pull n' Pack Waitlist");
    }

    private static ViewModel_ShipRecTools_Main CreateViewModel(
        IService_ShipRecTools_Navigation navigationService,
        IService_HeaderBackNavigation headerBackNavigation
    )
    {
        return new ViewModel_ShipRecTools_Main(
            navigationService,
            headerBackNavigation,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

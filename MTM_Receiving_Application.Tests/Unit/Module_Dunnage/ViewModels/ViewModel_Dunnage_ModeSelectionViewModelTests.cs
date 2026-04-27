using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_ModeSelectionViewModelTests
{
    [Fact]
    public void Constructor_ShouldMarkImageSearchAsDefault_WhenUserPreferenceMatches()
    {
        var session = new Model_UserSession(
            new Model_User
            {
                WindowsUsername = "DOMAIN\\user",
                DefaultDunnageMode = "image-search",
            }
        );

        var viewModel = CreateViewModel(session: session);

        viewModel.IsImageSearchModeDefault.Should().BeTrue();
        viewModel.IsGuidedModeDefault.Should().BeFalse();
        viewModel.IsEditModeDefault.Should().BeFalse();
        viewModel.IsManualModeDefault.Should().BeFalse();
    }

    [Fact]
    public async Task SetImageSearchAsDefaultAsync_ShouldPersistModeAndUpdateFlags()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            DefaultDunnageMode = "guided",
        };
        var session = new Model_UserSession(currentUser);
        var userPreferences = new Mock<IService_UserPreferences>();
        userPreferences
            .Setup(service => service.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, "image-search"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(userPreferences: userPreferences, session: session);

        await viewModel.SetImageSearchAsDefaultCommand.ExecuteAsync(true);

        currentUser.DefaultDunnageMode.Should().Be("image-search");
        viewModel.IsImageSearchModeDefault.Should().BeTrue();
        viewModel.IsGuidedModeDefault.Should().BeFalse();
        viewModel.IsEditModeDefault.Should().BeFalse();
        viewModel.StatusMessage.Should().Be("Image Search mode set as default");
        userPreferences.Verify(
            service => service.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, "image-search"),
            Times.Once
        );
    }

    [Fact]
    public async Task OpenImageSearchAsync_ShouldNavigateToImagePartSearch_WhenSessionIsClean()
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());

        var viewModel = CreateViewModel(workflow: workflow);

        await viewModel.OpenImageSearchCommand.ExecuteAsync(null);

        workflow.Verify(service => service.GoToStep(Enum_DunnageWorkflowStep.ImagePartSearch), Times.Once);
    }

    private static ViewModel_Dunnage_ModeSelection CreateViewModel(
        Mock<IService_DunnageWorkflow>? workflow = null,
        Mock<IService_UserPreferences>? userPreferences = null,
        Model_UserSession? session = null
    )
    {
        workflow ??= new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());

        userPreferences ??= new Mock<IService_UserPreferences>();
        userPreferences
            .Setup(service => service.UpdateDefaultDunnageModeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var sessionManager = new Mock<IService_UserSessionManager>();
        sessionManager.SetupGet(service => service.CurrentSession).Returns(session);

        var help = new Mock<IService_Help>();
        help.Setup(service => service.ShowHelpAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        return new ViewModel_Dunnage_ModeSelection(
            workflow.Object,
            help.Object,
            sessionManager.Object,
            userPreferences.Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
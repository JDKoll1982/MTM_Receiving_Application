using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ModeSelectionTests
{
    [Fact]
    public async Task HandleGuidedDefaultChangedAsync_ShouldIgnoreConflictingUnchecked_WhenAnotherModeIsSelected()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            DefaultReceivingMode = "manual",
        };
        var session = new Model_UserSession(currentUser);
        var userPreferences = new Mock<IService_UserPreferences>();

        var viewModel = CreateViewModel(userPreferences: userPreferences, session: session);

        await viewModel.HandleGuidedDefaultChangedAsync(false);

        currentUser.DefaultReceivingMode.Should().Be("manual");
        viewModel.IsManualModeDefault.Should().BeTrue();
        userPreferences.Verify(
            service =>
                service.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleGuidedDefaultChangedAsync_ShouldClearDefault_WhenNoOtherModeIsSelected()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = "guided",
        };
        var session = new Model_UserSession(currentUser);
        var userPreferences = new Mock<IService_UserPreferences>();
        userPreferences
            .Setup(service =>
                service.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, null)
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(userPreferences: userPreferences, session: session);

        await viewModel.HandleGuidedDefaultChangedAsync(false);

        currentUser.DefaultReceivingMode.Should().BeNull();
        viewModel.IsGuidedModeDefault.Should().BeFalse();
        viewModel.StatusMessage.Should().Be("Default mode cleared");
        userPreferences.Verify(
            service => service.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, null),
            Times.Once
        );
    }

    private static ViewModel_Receiving_ModeSelection CreateViewModel(
        Mock<IService_ReceivingWorkflow>? workflow = null,
        Mock<IService_UserPreferences>? userPreferences = null,
        Model_UserSession? session = null
    )
    {
        workflow ??= new Mock<IService_ReceivingWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_ReceivingSession());
        workflow
            .SetupGet(service => service.CurrentPart)
            .Returns(
                (MTM_Receiving_Application.Module_Core.Models.InforVisual.Model_InforVisualPart?)
                    null
            );

        userPreferences ??= new Mock<IService_UserPreferences>();
        userPreferences
            .Setup(service =>
                service.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service => service.GetBoolAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        receivingSettings
            .Setup(service =>
                service.SaveStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>())
            )
            .Returns(Task.CompletedTask);

        var sessionManager = new Mock<IService_UserSessionManager>();
        sessionManager.SetupGet(service => service.CurrentSession).Returns(session);

        var help = new Mock<IService_Help>();
        help.Setup(service => service.ShowHelpAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return new ViewModel_Receiving_ModeSelection(
            workflow.Object,
            sessionManager.Object,
            userPreferences.Object,
            help.Object,
            new Mock<IService_Window>().Object,
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ShellDefaultModeTests
{
    [Fact]
    public async Task SetDefaultMode_ShouldStoreManualOnly_WhenManualIsTicked()
    {
        var preferences = CreatePreferencesMock();
        var session = CreateSessionManager("guided");
        var viewModel = await CreateViewModelAsync(preferences, session);

        viewModel.SetDefaultMode("manual");
        await Task.Delay(25);

        preferences.Verify(
            service => service.UpdateDefaultReceivingModeAsync("test-user", "manual"),
            Times.Once
        );
        viewModel
            .ShellActions.IsManualDefault.Should()
            .BeTrue("the snapshot reflects the stored default so the boxes agree");
        viewModel.ShellActions.IsGuidedDefault.Should().BeFalse();
        viewModel
            .ShellActions.Next.Command.Should()
            .BeSameAs(
                viewModel.NextStepCommand,
                "ticking a default must not change the active page or mode"
            );
    }

    [Fact]
    public async Task ShellActions_ShouldTickEditAlone_WhenManualWasTheStoredDefaultAndEditIsTicked()
    {
        var preferences = CreatePreferencesMock();
        var session = CreateSessionManager("manual");
        var viewModel = await CreateViewModelAsync(preferences, session);

        viewModel.ShellActions.IsManualDefault.Should().BeTrue();

        viewModel.SetDefaultMode("edit");
        await Task.Delay(25);

        viewModel.ShellActions.IsEditDefault.Should().BeTrue();
        viewModel.ShellActions.IsManualDefault.Should().BeFalse();
        viewModel.ShellActions.IsGuidedDefault.Should().BeFalse();
    }

    [Fact]
    public async Task SetDefaultMode_ShouldStoreNothing_WhenTheLastTickedDefaultIsCleared()
    {
        var preferences = CreatePreferencesMock();
        var session = CreateSessionManager("edit");
        var viewModel = await CreateViewModelAsync(preferences, session);

        viewModel.SetDefaultMode(null);
        await Task.Delay(25);

        preferences.Verify(
            service => service.UpdateDefaultReceivingModeAsync("test-user", null),
            Times.Once
        );
        viewModel.ShellActions.IsEditDefault.Should().BeFalse();
        viewModel.ShellActions.IsManualDefault.Should().BeFalse();
        viewModel.ShellActions.IsGuidedDefault.Should().BeFalse();
        session.Object.CurrentSession!.User!.DefaultReceivingMode.Should().BeNull();
    }

    [Fact]
    public async Task SetDefaultMode_ShouldWriteForTheSignedInUserOnly_WhenAPreferenceIsPersisted()
    {
        var preferences = CreatePreferencesMock();
        var session = CreateSessionManager("guided");
        var viewModel = await CreateViewModelAsync(preferences, session);

        viewModel.SetDefaultMode("guided");
        await Task.Delay(25);

        preferences.Verify(
            service => service.UpdateDefaultReceivingModeAsync("test-user", "guided"),
            Times.Once
        );
        preferences.Verify(
            service =>
                service.UpdateDefaultReceivingModeAsync(
                    It.Is<string>(username => username != "test-user"),
                    It.IsAny<string?>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ShellActions_ShouldLeaveTheStoredDefaultAlone_WhenTheOverlayCommandRuns()
    {
        var preferences = CreatePreferencesMock();
        var session = CreateSessionManager("manual");
        var viewModel = await CreateViewModelAsync(preferences, session);

        viewModel.OpenLabelOverlayWindowCommand.Execute(null);
        await Task.Delay(25);

        preferences.Verify(
            service =>
                service.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never
        );
        viewModel.ShellActions.IsManualDefault.Should().BeTrue();
    }

    private static Mock<IService_UserPreferences> CreatePreferencesMock()
    {
        var preferences = new Mock<IService_UserPreferences>();
        preferences
            .Setup(service =>
                service.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        return preferences;
    }

    private static Mock<IService_UserSessionManager> CreateSessionManager(string? storedMode)
    {
        var sessionManager = new Mock<IService_UserSessionManager>();
        var session = new Model_UserSession
        {
            User = new Model_User
            {
                EmployeeNumber = 4242,
                WindowsUsername = "test-user",
                DefaultReceivingMode = storedMode,
            },
        };
        sessionManager.SetupGet(service => service.CurrentSession).Returns(session);
        return sessionManager;
    }

    private static async Task<ViewModel_Receiving_Workflow> CreateViewModelAsync(
        Mock<IService_UserPreferences> preferences,
        Mock<IService_UserSessionManager> sessionManager
    )
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);

        var dispatcher = new Mock<IService_Dispatcher>();
        dispatcher
            .Setup(service => service.TryEnqueue(It.IsAny<Action>()))
            .Returns<Action>(callback =>
            {
                callback();
                return true;
            });

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service =>
                service.FormatAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<int?>())
            )
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service =>
                service.FormatAsync(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<object?>(),
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(string.Empty);

        var registry = new Mock<IService_ViewModelRegistry>();
        registry
            .Setup(service => service.GetViewModels<ViewModel_Receiving_EditMode>())
            .Returns(Array.Empty<ViewModel_Receiving_EditMode>());

        var lotMarking = new Mock<IService_Tool_LotMarking>();
        lotMarking
            .Setup(service => service.GetEnabledPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<
                        MTM_Receiving_Application.Module_ShipRec_Tools.Models.Model_Tool_LotMarkingScope
                    >()
                )
            );

        var viewModel = new ViewModel_Receiving_Workflow(
            workflow.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            dispatcher.Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_LabelViewLauncher>().Object,
            receivingSettings.Object,
            registry.Object,
            lotMarking.Object,
            preferences.Object,
            sessionManager.Object,
            new Mock<IService_Notification>().Object
        );

        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        return viewModel;
    }
}

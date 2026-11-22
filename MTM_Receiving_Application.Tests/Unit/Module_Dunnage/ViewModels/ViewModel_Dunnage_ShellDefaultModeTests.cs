using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_ShellDefaultModeTests
{
    [Fact]
    public async Task EnterMode_ShouldNotRestartTheWorkflow_WhenTheModeIsAlreadyInForce()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.QuantityEntry);

        page.Bar.EnterMode("guided");

        page.Workflow.Verify(
            service => service.GoToStep(It.IsAny<Enum_DunnageWorkflowStep>()),
            Times.Never,
            "pressing Guided inside the guided walk must not throw the walk away"
        );
        page.ViewModel.ShellActions.Guided.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RememberModeAsDefault_ShouldStoreTheModeAndLeaveThePageAlone_WhenAModeChipIsPressedWithCtrl()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.PartSelection);

        page.Bar.RememberModeAsDefault("image-search");
        await Task.Delay(25);

        page.Preferences.Verify(
            service => service.UpdateDefaultDunnageModeAsync("test.user", "image-search"),
            Times.Once
        );
        page.Workflow.Verify(
            service => service.GoToStep(It.IsAny<Enum_DunnageWorkflowStep>()),
            Times.Never,
            "storing a default never changes the active page"
        );
        page.Bar.IsImageSearchDefault.Should().BeTrue();
        page.Bar.IsGuidedDefault.Should().BeFalse();
        page.Bar.IsEditDefault.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldTickExactlyOneDefault_WhenADifferentModeBecomesTheDefault()
    {
        var page = await CreatePageAsync(Enum_DunnageWorkflowStep.TypeSelection);

        page.Bar.RememberModeAsDefault("edit");
        await Task.Delay(25);
        TickedDefaults(page.Bar).Should().Equal("edit");

        page.Bar.RememberModeAsDefault("image-search");
        await Task.Delay(25);
        TickedDefaults(page.Bar).Should().Equal("image-search");

        page.Bar.RememberModeAsDefault("guided");
        await Task.Delay(25);
        TickedDefaults(page.Bar).Should().Equal("guided");
    }

    private static string[] TickedDefaults(ViewModel_Shared_ShellActionBar bar)
    {
        var ticked = new List<string>();

        if (bar.IsGuidedDefault == true)
        {
            ticked.Add("guided");
        }

        if (bar.IsEditDefault == true)
        {
            ticked.Add("edit");
        }

        if (bar.IsImageSearchDefault == true)
        {
            ticked.Add("image-search");
        }

        return ticked.ToArray();
    }

    private static async Task<Page> CreatePageAsync(Enum_DunnageWorkflowStep step)
    {
        var user = new Model_User { WindowsUsername = "test.user" };
        var session = new Model_UserSession(user);

        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(step);
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);

        var sessionManager = new Mock<IService_UserSessionManager>();
        sessionManager.SetupGet(service => service.CurrentSession).Returns(session);

        var preferences = new Mock<IService_UserPreferences>();
        preferences
            .Setup(service =>
                service.UpdateDefaultDunnageModeAsync(It.IsAny<string>(), It.IsAny<string?>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = new ViewModel_Dunnage_WorkFlowViewModel(
            workflow.Object,
            CreateSettingsMock().Object,
            new Mock<IService_LabelViewLauncher>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Notification>().Object,
            CreateDispatcherMock().Object,
            preferences.Object,
            new Mock<IService_Help>().Object,
            sessionManager.Object
        );

        await Task.Delay(25);

        var bar = new ViewModel_Shared_ShellActionBar();
        bar.Attach(viewModel);

        return new Page(viewModel, bar, workflow, preferences);
    }

    private sealed record Page(
        ViewModel_Dunnage_WorkFlowViewModel ViewModel,
        ViewModel_Shared_ShellActionBar Bar,
        Mock<IService_DunnageWorkflow> Workflow,
        Mock<IService_UserPreferences> Preferences
    );

    private static Mock<IService_DunnageSettings> CreateSettingsMock()
    {
        var settings = new Mock<IService_DunnageSettings>();
        settings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) =>
                    DunnageSettingsDefaults.StringDefaults.TryGetValue(key, out var value)
                        ? value
                        : string.Empty
            );

        return settings;
    }

    private static Mock<IService_Dispatcher> CreateDispatcherMock()
    {
        var dispatcher = new Mock<IService_Dispatcher>();
        dispatcher
            .Setup(service => service.TryEnqueue(It.IsAny<Action>()))
            .Returns<Action>(callback =>
            {
                callback();
                return true;
            });

        return dispatcher;
    }
}

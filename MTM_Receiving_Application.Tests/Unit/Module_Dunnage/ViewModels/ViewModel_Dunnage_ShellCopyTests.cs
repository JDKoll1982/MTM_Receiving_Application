using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_ShellCopyTests
{
    [Fact]
    public async Task ShellActions_ShouldTakeEveryLabelAndTooltipFromItsSettingsKey_WithTheVerbatimDefaultWhenUnedited()
    {
        var viewModel = await CreateViewModelAsync(CreateSettingsMock(DunnageSettingsDefaults.StringDefaults));

        var shell = viewModel.ShellActions;
        shell.Guided.Label.Should().Be("Guided");
        shell.Edit.Label.Should().Be("Edit");
        shell.ImageSearch.Label.Should().Be("Image Search");
        shell.Guided.DefaultBadge.Should().Be("Default");
        shell.Guided.DefaultTooltip.Should().Be("make Guided my default");
        shell.Edit.DefaultTooltip.Should().Be("make Edit my default");
        shell.ImageSearch.DefaultTooltip.Should().Be("make Image Search my default");
        shell.Back.Label.Should().Be("Back");
        shell.Next.Label.Should().Be("Review");
        shell.Next.Tooltip.Should().Be("Review");
        shell.ClearLabelData.Label.Should().Be("Clear All Labels");
        shell.ClearLabelData.Tooltip.Should().Be("Clear All Labels");
        shell.Help.Label.Should().Be("Help");
        shell.Help.Tooltip.Should().Be("Help");
    }

    [Fact]
    public async Task ShellActions_ShouldShowTheNewWording_WithoutARebuild_WhenAKeyIsChanged()
    {
        var settings = CreateSettingsMock(DunnageSettingsDefaults.StringDefaults);
        settings
            .Setup(service =>
                service.GetStringAsync(DunnageSettingsKeys.UiText.ShellGuided, It.IsAny<int?>())
            )
            .ReturnsAsync("Guided Wizard");
        settings
            .Setup(service =>
                service.GetStringAsync(DunnageSettingsKeys.UiText.WorkflowSave, It.IsAny<int?>())
            )
            .ReturnsAsync("Save it");
        settings
            .Setup(service =>
                service.GetStringAsync(
                    DunnageSettingsKeys.UiText.ShellImageSearch,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync("Search by photo");

        var viewModel = await CreateViewModelAsync(settings);

        viewModel.ShellActions.Guided.Label.Should().Be("Guided Wizard");
        viewModel.ShellActions.ImageSearch.Label.Should().Be("Search by photo");
        viewModel.ShellActions.Guided.AccessibilityName.Should().Be("Guided Wizard");
        viewModel.ShellActions.ImageSearch.AccessibilityName.Should().Be("Search by photo");
    }

    [Fact]
    public async Task TheSaveVerbSlot_ShouldShowTheEditedSaveKey_OnReviewAndEditMode()
    {
        var settings = CreateSettingsMock(DunnageSettingsDefaults.StringDefaults);
        settings
            .Setup(service =>
                service.GetStringAsync(DunnageSettingsKeys.UiText.WorkflowSave, It.IsAny<int?>())
            )
            .ReturnsAsync("Save it");

        var review = await CreateViewModelAsync(settings, Enum_DunnageWorkflowStep.Review);
        review.ShellActions.Next.Label.Should().Be("Save it");

        var editMode = await CreateViewModelAsync(settings, Enum_DunnageWorkflowStep.EditMode);
        editMode.ShellActions.Next.Label.Should().Be("Save it");
    }

    private static async Task<ViewModel_Dunnage_WorkFlowViewModel> CreateViewModelAsync(
        Mock<IService_DunnageSettings> settings,
        Enum_DunnageWorkflowStep step = Enum_DunnageWorkflowStep.DetailsEntry
    )
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(step);
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());
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

        var viewModel = new ViewModel_Dunnage_WorkFlowViewModel(
            workflow.Object,
            settings.Object,
            new Mock<IService_LabelViewLauncher>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Notification>().Object,
            dispatcher.Object,
            new Mock<IService_UserPreferences>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_UserSessionManager>().Object
        );

        viewModel.AttachShellActionHosts(
            new ViewModel_Dunnage_PartSelection(
                workflow.Object,
                new Mock<IService_MySQL_Dunnage>().Object,
                dispatcher.Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_UserPrivileges>().Object,
                new Mock<IService_UserSessionManager>().Object,
                settings.Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_QuantityEntry(
                workflow.Object,
                dispatcher.Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_DetailsEntry(
                workflow.Object,
                new Mock<IService_MySQL_Dunnage>().Object,
                dispatcher.Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ReceivingValidation>().Object,
                new Mock<IService_InforVisual>().Object,
                new Mock<IService_SettingsCoreFacade>().Object,
                new Mock<IService_UserSessionManager>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_Review(
                workflow.Object,
                new Mock<IService_MySQL_Dunnage>().Object,
                new Mock<IService_InforVisual>().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_Window>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new ViewModel_Dunnage_EditMode(
                new Mock<IService_MySQL_Dunnage>().Object,
                new Mock<IService_Pagination>().Object,
                workflow.Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_Window>().Object,
                new Mock<IService_InforVisual>().Object,
                new Mock<IService_Help>().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            )
        );

        await Task.Delay(25);

        return viewModel;
    }

    private static Mock<IService_DunnageSettings> CreateSettingsMock(
        IReadOnlyDictionary<string, string> values
    )
    {
        var settings = new Mock<IService_DunnageSettings>();
        settings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) => values.TryGetValue(key, out var value) ? value : string.Empty
            );

        return settings;
    }
}

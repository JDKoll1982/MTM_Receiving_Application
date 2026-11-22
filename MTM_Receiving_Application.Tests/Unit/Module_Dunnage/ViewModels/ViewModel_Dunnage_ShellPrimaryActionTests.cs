using System.Reflection;
using System.Windows.Input;
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

public sealed class ViewModel_Dunnage_ShellPrimaryActionTests
{
    [Fact]
    public async Task ShellActions_ShouldNameNoEditingToolbarCommand_OnEditMode()
    {
        var (viewModel, editMode) = await CreateEditModePageAsync();

        var shell = viewModel.ShellActions;
        shell.IsVisible.Should().BeTrue();
        shell.Next.Label.Should().Be("Save");
        shell
            .Next.Command.Should()
            .BeSameAs(editMode.SaveAllCommand, "the save verb is the strip's own Next position");

        var pageCommands = editMode
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => typeof(ICommand).IsAssignableFrom(property.PropertyType))
            .Select(property => property.GetValue(editMode))
            .OfType<ICommand>()
            .ToArray();

        pageCommands.Should().NotBeEmpty("Edit Mode keeps its whole editing toolbar");

        var slotCommands = new[] { shell.Back.Command, shell.Next.Command, shell.ClearLabelData.Command, shell.Help.Command }
            .Where(command => command is not null)
            .ToArray();

        foreach (var toolbarCommand in pageCommands.Except([editMode.SaveAllCommand]))
        {
            slotCommands
                .Should()
                .NotContain(
                    toolbarCommand,
                    "load selection, select all, remove, columns, filters and paging stay on the page"
                );
        }
    }

    private static async Task<(ViewModel_Dunnage_WorkFlowViewModel ViewModel, ViewModel_Dunnage_EditMode EditMode)> CreateEditModePageAsync()
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(Enum_DunnageWorkflowStep.EditMode);
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);

        var editMode = new ViewModel_Dunnage_EditMode(
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
        );

        var settings = new Mock<IService_DunnageSettings>();
        settings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) =>
                    DunnageSettingsDefaults.StringDefaults.TryGetValue(key, out var value)
                        ? value
                        : string.Empty
            );

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
            editMode
        );

        await Task.Delay(25);

        return (viewModel, editMode);
    }
}

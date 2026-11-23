using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ShellByReceiverTests
{
    [Fact]
    public async Task ShellActions_ShouldOfferAVisibleByReceiverChip_WhenThePageIsReceiving()
    {
        var workflow = CreateWorkflowMock(Enum_ReceivingWorkflowStep.POEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        var chip = viewModel.ShellActions.ByReceiver;
        chip.IsVisible.Should().BeTrue();
        chip.Label.Should().Be("By Receiver");
        chip.Mode.Should().Be("by-receiver");
        chip.CanBeDefault.Should().BeTrue();
        chip.Command.Should().BeSameAs(viewModel.EnterByReceiverModeCommand);
    }

    [Fact]
    public void MainWindowXaml_ShouldPlaceByReceiverChipBetweenGuidedAndManual()
    {
        var path = FindRepoFile("MainWindow.xaml");
        var content = File.ReadAllText(path);

        var guidedIndex = content.IndexOf(
            "x:Name=\"ShellGuidedButton\"",
            StringComparison.Ordinal
        );
        var byReceiverIndex = content.IndexOf(
            "x:Name=\"ShellByReceiverButton\"",
            StringComparison.Ordinal
        );
        var manualIndex = content.IndexOf(
            "x:Name=\"ShellManualButton\"",
            StringComparison.Ordinal
        );

        guidedIndex.Should().BeGreaterThanOrEqualTo(0);
        byReceiverIndex.Should().BeGreaterThanOrEqualTo(0);
        manualIndex.Should().BeGreaterThanOrEqualTo(0);
        guidedIndex.Should().BeLessThan(byReceiverIndex);
        byReceiverIndex.Should().BeLessThan(manualIndex);
    }

    private static string FindRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find {relativePath} walking up from {AppContext.BaseDirectory}"
        );
    }

    private static async Task<ViewModel_Receiving_Workflow> CreateViewModelAsync(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        return viewModel;
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock(
        Enum_ReceivingWorkflowStep step
    )
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(step);
        workflow.Setup(service => service.ResetWorkflowAsync()).Returns(Task.CompletedTask);
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);
        return workflow;
    }

    private static ViewModel_Receiving_Workflow CreateViewModel(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
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
            .ReturnsAsync(
                (string key, int? _) =>
                    key == ReceivingSettingsKeys.UiText.ShellByReceiver ? "By Receiver" : string.Empty
            );
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

        return new ViewModel_Receiving_Workflow(
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
            new Mock<IService_UserPreferences>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

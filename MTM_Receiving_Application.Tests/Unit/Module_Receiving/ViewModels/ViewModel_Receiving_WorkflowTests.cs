using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_WorkflowTests
{
    [Fact]
    public async Task Constructor_ShouldLoadClearLabelDataAvailability()
    {
        var workflow = CreateWorkflowMock();
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(true);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        viewModel.CanClearLabelData.Should().BeTrue();
    }

    [Fact]
    public async Task StepChanged_ShouldRefreshClearLabelDataAvailability()
    {
        var workflow = CreateWorkflowMock();
        Enum_ReceivingWorkflowStep currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
        var hasActiveLabelData = true;

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);
        workflow
            .Setup(service => service.HasActiveLabelDataAsync())
            .ReturnsAsync(() => hasActiveLabelData);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        viewModel.CanClearLabelData.Should().BeTrue();

        hasActiveLabelData = false;
        currentStep = Enum_ReceivingWorkflowStep.POEntry;
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        viewModel.CanClearLabelData.Should().BeFalse();
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ModeSelection);
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
            new Mock<IService_Notification>().Object
        );
    }
}

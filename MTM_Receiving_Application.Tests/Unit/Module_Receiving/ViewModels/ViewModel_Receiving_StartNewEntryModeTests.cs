using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_StartNewEntryModeTests
{
    [Fact]
    public async Task StartNewEntry_ShouldReturnToReceiverSelection_WhenTheFinishedWalkWasByReceiver()
    {
        var state = new WorkflowState
        {
            Step = Enum_ReceivingWorkflowStep.Complete,
            IsByReceiverFlow = true,
        };
        var (workflow, viewModel) = await CreateViewModelAsync(state);

        await viewModel.StartNewEntryCommand.ExecuteAsync(null);

        state.Step.Should().Be(Enum_ReceivingWorkflowStep.ReceiverSelection);
        workflow.Verify(service => service.ResetWorkflowAsync(), Times.Once);
    }

    [Fact]
    public async Task StartNewEntry_ShouldReturnToPOEntry_WhenTheFinishedWalkWasGuided()
    {
        var state = new WorkflowState
        {
            Step = Enum_ReceivingWorkflowStep.Complete,
            IsByReceiverFlow = false,
        };
        var (workflow, viewModel) = await CreateViewModelAsync(state);

        await viewModel.StartNewEntryCommand.ExecuteAsync(null);

        state.Step.Should().Be(Enum_ReceivingWorkflowStep.POEntry);
        workflow.Verify(service => service.ResetWorkflowAsync(), Times.Once);
    }

    private static async Task<(
        Mock<IService_ReceivingWorkflow> Workflow,
        ViewModel_Receiving_Workflow ViewModel
    )> CreateViewModelAsync(WorkflowState state)
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(() => state.Step);
        workflow.SetupGet(service => service.IsByReceiverFlow).Returns(() => state.IsByReceiverFlow);
        workflow
            .SetupGet(service => service.CurrentSession)
            .Returns(new Model_ReceivingSession());
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);
        workflow
            .Setup(service => service.ResetWorkflowAsync())
            .Callback(() =>
            {
                // The real service clears the flow flag and re-announces the PO step, which is
                // what used to overwrite the remembered starting point.
                state.Step = Enum_ReceivingWorkflowStep.POEntry;
                state.IsByReceiverFlow = false;
                workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
            })
            .Returns(Task.CompletedTask);
        workflow
            .Setup(service => service.GoToStep(It.IsAny<Enum_ReceivingWorkflowStep>()))
            .Callback<Enum_ReceivingWorkflowStep>(step => state.Step = step)
            .Returns<Enum_ReceivingWorkflowStep>(step =>
                Model_ReceivingWorkflowStepResult.SuccessResult(step)
            );

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

        return (workflow, viewModel);
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

        var lotMarking = new Mock<IService_Tool_LotMarking>();
        lotMarking
            .Setup(service => service.GetEnabledPartsAsync())
            .ReturnsAsync(
                MTM_Receiving_Application.Module_Core.Models.Core.Model_Dao_Result_Factory.Success(
                    new System.Collections.Generic.List<
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

    private sealed class WorkflowState
    {
        public Enum_ReceivingWorkflowStep Step { get; set; }

        public bool IsByReceiverFlow { get; set; }
    }
}

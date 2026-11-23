using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_LoadEntryByReceiverTests
{
    [Fact]
    public async Task StepChanged_ShouldPrefillLocationAndRemainingQty_WhenByReceiverWalkEntersLoadEntry()
    {
        var workflow = CreateWorkflowMock(isByReceiverFlow: true);
        workflow.SetupGet(service => service.CurrentLocation).Returns("T-00");
        workflow.SetupGet(service => service.CurrentReceiverRemainingQty).Returns(171m);

        var viewModel = CreateViewModel(workflow);

        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        viewModel.Location.Should().Be("T-00");
        viewModel.QtyRemainingOnLine.Should().Be(171m);
        viewModel.ShowQtyRemainingOnLine.Should().BeTrue();
    }

    [Fact]
    public async Task StepChanged_ShouldHideRemainingQtyReadout_WhenGuidedWalkEntersLoadEntry()
    {
        var workflow = CreateWorkflowMock(isByReceiverFlow: false);

        var viewModel = CreateViewModel(workflow);

        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        viewModel.ShowQtyRemainingOnLine.Should().BeFalse();
    }

    [Fact]
    public void QtyRemainingOnLineDisplay_ShouldRead171_WhenRemainingQtyIs171()
    {
        var viewModel = CreateViewModel(CreateWorkflowMock(isByReceiverFlow: true));

        viewModel.QtyRemainingOnLine = 171m;

        viewModel.QtyRemainingOnLineDisplay.Should().Be("171");
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock(bool isByReceiverFlow)
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.LoadEntry);
        workflow.SetupGet(service => service.CurrentPart).Returns((Model_InforVisualPart?)null);
        workflow.SetupGet(service => service.CurrentPONumber).Returns((string?)null);
        workflow.SetupGet(service => service.IsByReceiverFlow).Returns(isByReceiverFlow);
        return workflow;
    }

    private static ViewModel_Receiving_LoadEntry CreateViewModel(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        var validationService = new Mock<IService_ReceivingValidation>();
        validationService.SetupGet(service => service.UseMockLocationList).Returns(false);
        validationService.SetupGet(service => service.PresetLocations).Returns(new List<string>());

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => key);

        return new ViewModel_Receiving_LoadEntry(
            workflow.Object,
            validationService.Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_ReceivingLocationReconciliation>().Object,
            new Mock<IService_Help>().Object,
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

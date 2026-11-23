using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ByReceiverRefreshTests
{
    [Fact]
    public async Task EnteringTheMode_ShouldReadTheSourceOnce_AndRefreshWithoutLeaving()
    {
        var step = Enum_ReceivingWorkflowStep.POEntry;
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(() => step);

        var inforVisual = new Mock<IService_InforVisual>();
        var rows = new List<Model_InforVisualReceiverRow>
        {
            new() { ReceiverNumber = "TEST-RCV-001", PartNumber = "TEST-PART-001" },
            new() { ReceiverNumber = "TEST-RCV-002", PartNumber = "TEST-PART-002" },
        };
        inforVisual
            .Setup(service => service.GetRecentReceiversAsync(It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(rows));

        var viewModel = CreateViewModel(workflow, inforVisual);

        step = Enum_ReceivingWorkflowStep.ReceiverSelection;
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        inforVisual.Verify(service => service.GetRecentReceiversAsync(1000), Times.Once);
        viewModel.Rows.Should().HaveCount(2);

        viewModel.RefreshCommand.Execute(null);
        await Task.Delay(25);

        inforVisual.Verify(service => service.GetRecentReceiversAsync(1000), Times.Exactly(2));
        viewModel.Rows.Should().HaveCount(2);
        workflow.Object.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.ReceiverSelection);
        workflow.Verify(
            service => service.GoToStep(It.IsAny<Enum_ReceivingWorkflowStep>()),
            Times.Never
        );
    }

    private static ViewModel_Receiving_ReceiverSelection CreateViewModel(
        Mock<IService_ReceivingWorkflow> workflow,
        Mock<IService_InforVisual> inforVisual
    )
    {
        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service => service.GetIntAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(50);

        return new ViewModel_Receiving_ReceiverSelection(
            workflow.Object,
            inforVisual.Object,
            new Service_Pagination(),
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

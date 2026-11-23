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
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ByReceiverFailureTests
{
    [Fact]
    public async Task FailedRead_ShouldLeaveTheListFailedWithTheSourceMessage()
    {
        var workflow = CreateWorkflowMock();
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.GetRecentReceiversAsync(It.IsAny<int>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_InforVisualReceiverRow>>(
                    "TEST-FAILURE"
                )
            );

        var viewModel = CreateViewModel(workflow, inforVisual, _ => string.Empty);
        await Task.Delay(25);

        viewModel.IsFailed.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("TEST-FAILURE");
        viewModel.IsEmpty.Should().BeFalse();
        viewModel.Rows.Should().BeEmpty();
    }

    [Fact]
    public async Task Retry_ShouldRecoverTheList_WhenTheSecondReadSucceeds()
    {
        var workflow = CreateWorkflowMock();
        var rows = new List<Model_InforVisualReceiverRow>
        {
            new() { ReceiverNumber = "TEST-RCV-001", PartNumber = "TEST-PART-001" },
            new() { ReceiverNumber = "TEST-RCV-002", PartNumber = "TEST-PART-002" },
        };
        var shouldFail = true;
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.GetRecentReceiversAsync(It.IsAny<int>()))
            .ReturnsAsync(() =>
                shouldFail
                    ? new Model_Dao_Result<List<Model_InforVisualReceiverRow>>
                    {
                        Success = false,
                        ErrorMessage = string.Empty,
                    }
                    : Model_Dao_Result_Factory.Success(rows)
            );

        var viewModel = CreateViewModel(
            workflow,
            inforVisual,
            key =>
                key == ReceivingSettingsKeys.ReceiverSelection.FailureMessage
                    ? "The recent receivers could not be loaded."
                    : string.Empty
        );
        await Task.Delay(25);

        viewModel.IsFailed.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("The recent receivers could not be loaded.");

        shouldFail = false;
        viewModel.RetryCommand.Execute(null);
        await Task.Delay(25);

        viewModel.IsFailed.Should().BeFalse();
        viewModel.Rows.Should().HaveCount(2);
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ReceiverSelection);
        return workflow;
    }

    private static ViewModel_Receiving_ReceiverSelection CreateViewModel(
        Mock<IService_ReceivingWorkflow> workflow,
        Mock<IService_InforVisual> inforVisual,
        Func<string, string> getString
    )
    {
        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => getString(key));
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

using System;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ReceiverSelectionCarryTests
{
    [Fact]
    public void SelectRowCommand_ShouldCarryTheRowIntoTheWorkflowAndOpenLoadEntry()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow.SetupProperty(service => service.SelectedReceiver, (Model_InforVisualReceiverRow?)null);
        workflow.SetupProperty(service => service.CurrentPONumber, (string?)null);
        workflow.SetupProperty(service => service.CurrentPart, (Model_InforVisualPart?)null);
        workflow.SetupProperty(service => service.CurrentLocation, string.Empty);
        workflow.SetupProperty(service => service.CurrentPOVendor, (string?)null);
        workflow
            .SetupGet(service => service.SelectedReceiverKey)
            .Returns(() => workflow.Object.SelectedReceiver?.SelectionKey);

        var viewModel = CreateViewModel(workflow);

        var row = new Model_InforVisualReceiverRow
        {
            ReceiverNumber = "TEST-RCV-001",
            PONumber = "TEST-PO-001",
            POLineNumber = "2",
            PartNumber = "TEST-PART-001",
            PartDescription = "TEST PART DESCRIPTION",
            UnitOfMeasure = "LB",
            VendorName = "TEST-VENDOR",
            LocationReceivedTo = "T-00",
            QtyReceived = 10m,
            QtyOrdered = 100m,
            QtyRemaining = 90m,
        };

        viewModel.SelectRowCommand.Execute(row);

        workflow.Object.SelectedReceiver.Should().BeSameAs(row);
        workflow.Object.CurrentPONumber.Should().Be(row.PONumber);
        workflow.Object.CurrentPart!.PartID.Should().Be(row.PartNumber);
        workflow.Object.CurrentPart!.POLineNumber.Should().Be(row.POLineNumber);
        workflow.Object.CurrentLocation.Should().Be(row.LocationReceivedTo);
        workflow
            .Object.CurrentPOVendor.Should()
            .Be(
                row.VendorName,
                "the Heat/Lot page looks the vendor's custom variable column up by PO vendor name"
            );
        workflow.Object.SelectedReceiverKey.Should().Be(row.SelectionKey);
        workflow
            .Object.CurrentPart!.UnitOfMeasure.Should()
            .Be(
                "LB",
                "Load Entry records the unit of measure on the saved load, so a coil received in pounds must not be saved as EA"
            );
        workflow.Object.CurrentPart!.Description.Should().Be(row.PartDescription);
        workflow.Verify(
            service => service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry),
            Times.Once
        );
    }

    private static ViewModel_Receiving_ReceiverSelection CreateViewModel(
        Mock<IService_ReceivingWorkflow> workflow
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
            new Mock<IService_InforVisual>().Object,
            new Service_Pagination(),
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

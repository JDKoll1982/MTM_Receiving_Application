using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_LoadEntryTests
{
    [Fact]
    public void ApplyRecommendedLocationCommand_ShouldPopulateLocationAndWorkflowState()
    {
        var workflowService = new Mock<IService_ReceivingWorkflow>();
        workflowService.SetupProperty(service => service.CurrentLocation, string.Empty);

        var validationService = new Mock<IService_ReceivingValidation>();
        validationService.SetupGet(service => service.UseMockLocationList).Returns(true);
        validationService.SetupGet(service => service.PresetLocations).Returns(new List<string>());

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => key);

        var viewModel = new ViewModel_Receiving_LoadEntry(
            workflowService.Object,
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

        viewModel.ApplyRecommendedLocationCommand.Execute(
            new Model_ReceivingRecommendedLocation
            {
                WarehouseId = "002",
                LocationId = "RACK-A1",
                QuantityOnHand = 12,
                ReasonText = "Current stock suggestion",
            }
        );

        viewModel.Location.Should().Be("RACK-A1");
        workflowService.Object.CurrentLocation.Should().Be("RACK-A1");
    }
}

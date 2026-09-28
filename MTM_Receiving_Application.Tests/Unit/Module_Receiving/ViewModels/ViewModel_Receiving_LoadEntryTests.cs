using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

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

    [Fact]
    public async Task StepChanged_ShouldLoadRecommendedLocations_FromPartStock_WhenPoIsBlank()
    {
        var workflowService = new Mock<IService_ReceivingWorkflow>();
        var selectedPart = new Model_InforVisualPart
        {
            PartID = "MMC0000700",
            Description = "Mock steel coil",
            POLineNumber = "1",
        };
        workflowService.SetupGet(service => service.CurrentStep).Returns(Enum_ReceivingWorkflowStep.LoadEntry);
        workflowService.SetupGet(service => service.CurrentPart).Returns(selectedPart);
        workflowService.SetupGet(service => service.CurrentPONumber).Returns(string.Empty);
        workflowService.SetupProperty(service => service.CurrentLocation, string.Empty);

        var validationService = new Mock<IService_ReceivingValidation>();
        validationService.SetupGet(service => service.UseMockLocationList).Returns(false);
        validationService.SetupGet(service => service.PresetLocations).Returns(new List<string>());

        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service =>
                service.GetMaterialAvailabilityCurrentStockAsync(null, "MMC0000700", "002")
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualMaterialLocationRow>
                    {
                        new()
                        {
                            WarehouseCode = "002",
                            LocationId = "RECV",
                            Quantity = 5m,
                        },
                        new()
                        {
                            WarehouseCode = "002",
                            LocationId = "FG",
                            Quantity = 2m,
                        },
                        new()
                        {
                            WarehouseCode = "002",
                            LocationId = "WC",
                            Quantity = 0m,
                        },
                    }
                )
            );

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => key);

        var viewModel = new ViewModel_Receiving_LoadEntry(
            workflowService.Object,
            validationService.Object,
            inforVisualService.Object,
            new Mock<IService_ReceivingLocationReconciliation>().Object,
            new Mock<IService_Help>().Object,
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_Notification>().Object
        );

        workflowService.Raise(service => service.StepChanged += null, workflowService.Object, EventArgs.Empty);
        await Task.Delay(25);

        viewModel.RecommendedLocations.Select(location => location.LocationId).Should().Equal("RECV", "FG");
        viewModel.HasRecommendedLocations.Should().BeTrue();
    }
}

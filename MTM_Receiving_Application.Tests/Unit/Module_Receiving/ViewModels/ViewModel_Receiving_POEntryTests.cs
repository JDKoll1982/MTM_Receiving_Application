using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_POEntryTests
{
    [Fact]
    public async Task PartTextBoxLostFocusCommand_ShouldLookupPart_WhenNonPoModeAndPartIsEntered()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPartByIDAsync("MMC650"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "MMC650",
                        Description = "Mock part",
                    }
                )
            );

        var workflowService = new Mock<IService_ReceivingWorkflow>();
        workflowService.SetupProperty(service => service.CurrentPONumber, string.Empty);
        workflowService.SetupProperty(service => service.CurrentLocation, string.Empty);

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => key);
        receivingSettings
            .Setup(service =>
                service.FormatAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<int?>())
            )
            .ReturnsAsync(string.Empty);

        var qualityHoldWarning = new Mock<IService_QualityHoldWarning>();
        qualityHoldWarning
            .Setup(service => service.IsRestrictedPart(It.IsAny<string>()))
            .Returns(false);

        var viewModel = new ViewModel_Receiving_POEntry(
            inforVisualService.Object,
            workflowService.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Help>().Object,
            qualityHoldWarning.Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_AppSettings>().Object,
            receivingSettings.Object,
            new Mock<IService_Notification>().Object
        );

        viewModel.IsNonPOItem = true;
        viewModel.PartID = "MMC650";

        await viewModel.PartTextBoxLostFocusCommand.ExecuteAsync(null);

        inforVisualService.Verify(service => service.GetPartByIDAsync("MMC650"), Times.Once);
        viewModel.Parts.Should().ContainSingle(part => part.PartID == "MMC650");
    }

    [Fact]
    public async Task LoadPOCommand_ShouldPopulateUniquePartsWithOnHand_WhenPoLoads()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPOUniquePartsWithOnHandAsync("500001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(
                    new Model_InforVisualPO
                    {
                        PONumber = "500001",
                        Vendor = "Stern Steel LLC",
                        Status = "R",
                        Parts = new List<Model_InforVisualPart>
                        {
                            new()
                            {
                                PartID = "MMC0000850",
                                Description = "Coil, .312 X 14.330",
                                OnHandQty = 42,
                                Location = "RECV",
                            },
                        },
                    }
                )
            );

        var workflowService = new Mock<IService_ReceivingWorkflow>();
        workflowService.SetupProperty(service => service.CurrentPONumber, string.Empty);
        workflowService.SetupProperty(service => service.CurrentLocation, string.Empty);
        workflowService.SetupProperty(service => service.CurrentPOVendor, string.Empty);
        workflowService.SetupProperty(service => service.CurrentPOStatus, string.Empty);
        workflowService.SetupProperty(service => service.CurrentPODueDate, (DateTime?)null);

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync((string key, int? _) => key);
        receivingSettings
            .Setup(service =>
                service.FormatAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<int?>())
            )
            .ReturnsAsync(string.Empty);

        var viewModel = new ViewModel_Receiving_POEntry(
            inforVisualService.Object,
            workflowService.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_QualityHoldWarning>().Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_AppSettings>().Object,
            receivingSettings.Object,
            new Mock<IService_Notification>().Object
        );

        viewModel.PoNumber = "500001";

        await viewModel.LoadPOCommand.ExecuteAsync(null);

        inforVisualService.Verify(
            service => service.GetPOUniquePartsWithOnHandAsync("500001"),
            Times.Once
        );
        viewModel.Parts.Should().ContainSingle(part => part.PartID == "MMC0000850");
        viewModel.Parts.Single().OnHandQty.Should().Be(42);
        viewModel.Parts.Single().Location.Should().Be("RECV");
    }
}

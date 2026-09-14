using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
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

        var harness = new Harness(inforVisualService);

        harness.ViewModel.IsNonPOItem = true;
        harness.ViewModel.PartID = "MMC650";

        await harness.ViewModel.PartTextBoxLostFocusCommand.ExecuteAsync(null);

        inforVisualService.Verify(service => service.GetPartByIDAsync("MMC650"), Times.Once);
        harness.ViewModel.Parts.Should().ContainSingle(part => part.PartID == "MMC650");
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
                            new()
                            {
                                PartID = "MMF0001200",
                                Description = "Sheet, 16GA",
                                OnHandQty = 8,
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
        workflowService
            .Setup(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);

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
        viewModel.Parts.Should().HaveCount(2);
        viewModel.Parts.Should().Contain(part => part.PartID == "MMC0000850");
        viewModel.Parts.Single(part => part.PartID == "MMC0000850").OnHandQty.Should().Be(42);
        viewModel.Parts.Single(part => part.PartID == "MMC0000850").Location.Should().Be("RECV");
        workflowService.Verify(service => service.AdvanceToNextStepAsync(), Times.Never);
    }

    [Fact]
    public async Task LoadPOCommand_ShouldRejectEntry_WhenPoHasNoParts()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPOUniquePartsWithOnHandAsync("500009"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(
                    new Model_InforVisualPO { PONumber = "500009", Status = "R" }
                )
            );

        var harness = new Harness(inforVisualService);
        harness.ViewModel.PoNumber = "500009";

        await harness.ViewModel.LoadPOCommand.ExecuteAsync(null);

        harness.ViewModel.PoNumber.Should().BeEmpty();
        harness.ViewModel.Parts.Should().BeEmpty();
        harness.ViewModel.SelectedPart.Should().BeNull();
        harness.ViewModel.PoStatus.Should().BeEmpty();
        harness.ViewModel.IsPartsListVisible.Should().BeFalse();
        harness
            .RefocusRequestCount.Should()
            .Be(1, "the PO field must be cleared and refocused after rejection");
        harness
            .StatusMessages.Should()
            .ContainSingle()
            .Which.Should()
            .Be(ReceivingSettingsKeys.Messages.ErrorPoHasNoParts);
        harness
            .StatusSeverities.Should()
            .ContainSingle()
            .Which.Should()
            .Be(InfoBarSeverity.Warning);
        harness.WorkflowService.Verify(service => service.AdvanceToNextStepAsync(), Times.Never);
    }

    [Fact]
    public async Task LoadPOCommand_ShouldAutoSelectAndAdvance_WhenPoHasExactlyOneUniquePart()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPOUniquePartsWithOnHandAsync("500002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(
                    new Model_InforVisualPO
                    {
                        PONumber = "500002",
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

        var harness = new Harness(inforVisualService);
        harness.ViewModel.PoNumber = "500002";

        await harness.ViewModel.LoadPOCommand.ExecuteAsync(null);

        harness.ViewModel.SelectedPart.Should().NotBeNull();
        harness.ViewModel.SelectedPart!.PartID.Should().Be("MMC0000850");
        harness.WorkflowService.Object.CurrentPart.Should().NotBeNull();
        harness.WorkflowService.Object.CurrentPart!.PartID.Should().Be("MMC0000850");
        harness.WorkflowService.Verify(service => service.AdvanceToNextStepAsync(), Times.Once);
        harness
            .StatusMessages.Should()
            .ContainSingle()
            .Which.Should()
            .Be(ReceivingSettingsKeys.Messages.InfoPoSinglePartAutoSelected);
        harness
            .StatusSeverities.Should()
            .ContainSingle()
            .Which.Should()
            .Be(InfoBarSeverity.Informational);
        harness.RefocusRequestCount.Should().Be(0);
    }

    [Fact]
    public async Task TryAutoLoadPoAsync_ShouldLoadOnlyOnce_WhenCalledRepeatedlyForSamePo()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPOUniquePartsWithOnHandAsync("500001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(
                    new Model_InforVisualPO
                    {
                        PONumber = "500001",
                        Status = "R",
                        Parts = new List<Model_InforVisualPart>
                        {
                            new() { PartID = "MMC0000850", Description = "Coil" },
                            new() { PartID = "MMF0001200", Description = "Sheet" },
                        },
                    }
                )
            );

        var harness = new Harness(inforVisualService);
        harness.ViewModel.PoNumber = "500001";

        await harness.ViewModel.TryAutoLoadPoAsync();
        await harness.ViewModel.TryAutoLoadPoAsync();

        inforVisualService.Verify(
            service => service.GetPOUniquePartsWithOnHandAsync("500001"),
            Times.Once
        );
    }

    [Fact]
    public async Task TryAutoLoadPoAsync_ShouldDoNothing_WhenPoIsBlank()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        var harness = new Harness(inforVisualService);

        await harness.ViewModel.TryAutoLoadPoAsync();

        inforVisualService.Verify(
            service => service.GetPOUniquePartsWithOnHandAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoadPOCommand_ShouldDoNothing_WhenWorkflowIsNotOnPoEntryStep()
    {
        var inforVisualService = new Mock<IService_InforVisual>();
        var harness = new Harness(inforVisualService, Enum_ReceivingWorkflowStep.LoadEntry);
        harness.ViewModel.PoNumber = "500001";

        await harness.ViewModel.LoadPOCommand.ExecuteAsync(null);

        inforVisualService.Verify(
            service => service.GetPOUniquePartsWithOnHandAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    private sealed class Harness
    {
        public Harness(
            Mock<IService_InforVisual> inforVisualService,
            Enum_ReceivingWorkflowStep currentStep = Enum_ReceivingWorkflowStep.POEntry
        )
        {
            WorkflowService = new Mock<IService_ReceivingWorkflow>();
            WorkflowService.SetupProperty(service => service.CurrentPONumber, string.Empty);
            WorkflowService.SetupProperty(service => service.CurrentLocation, string.Empty);
            WorkflowService.SetupProperty(service => service.CurrentPOVendor, string.Empty);
            WorkflowService.SetupProperty(service => service.CurrentPOStatus, string.Empty);
            WorkflowService.SetupProperty(service => service.CurrentPODueDate, (DateTime?)null);
            WorkflowService
                .SetupProperty(service => service.CurrentPart, (Model_InforVisualPart?)null);
            WorkflowService.Setup(service => service.CurrentStep).Returns(currentStep);

            var receivingSettings = new Mock<IService_ReceivingSettings>();
            receivingSettings
                .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((string key, int? userId) => key);
            receivingSettings
                .Setup(service =>
                    service.FormatAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<int?>())
                )
                .ReturnsAsync((string key, object? arg0, int? userId) => key);
            receivingSettings
                .Setup(service =>
                    service.FormatAsync(
                        It.IsAny<string>(),
                        It.IsAny<object?>(),
                        It.IsAny<object?>(),
                        It.IsAny<int?>()
                    )
                )
                .ReturnsAsync((string key, object? arg0, object? arg1, int? userId) => key);

            var qualityHoldWarning = new Mock<IService_QualityHoldWarning>();
            qualityHoldWarning
                .Setup(service => service.IsRestrictedPart(It.IsAny<string>()))
                .Returns(false);

            Notification = new Mock<IService_Notification>();
            Notification
                .Setup(service =>
                    service.ShowStatus(It.IsAny<string>(), It.IsAny<InfoBarSeverity>())
                )
                .Callback<string, InfoBarSeverity>(
                    (message, severity) =>
                    {
                        StatusMessages.Add(message);
                        StatusSeverities.Add(severity);
                    }
                );

            ViewModel = new ViewModel_Receiving_POEntry(
                inforVisualService.Object,
                WorkflowService.Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Help>().Object,
                qualityHoldWarning.Object,
                new Mock<IService_InforVisualMockDataCatalog>().Object,
                new Mock<IService_ViewModelRegistry>().Object,
                new Mock<IService_Window>().Object,
                new Mock<IService_AppSettings>().Object,
                receivingSettings.Object,
                Notification.Object
            );

            ViewModel.PoFieldRefocusRequested += (_, _) => RefocusRequestCount++;
        }

        public Mock<IService_ReceivingWorkflow> WorkflowService { get; }

        public Mock<IService_Notification> Notification { get; }

        public ViewModel_Receiving_POEntry ViewModel { get; }

        public List<string> StatusMessages { get; } = new();

        public List<InfoBarSeverity> StatusSeverities { get; } = new();

        public int RefocusRequestCount { get; private set; }
    }
}

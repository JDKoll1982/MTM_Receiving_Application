using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_PartSelectionTests
{
    [Fact]
    public void EmptyStateText_ShouldPointAtThePoStep_WhenNoPartsHaveBeenLoaded()
    {
        var viewModel = CreateViewModel(out _, out _);

        viewModel.ShowEmptyState.Should().BeTrue();
        viewModel
            .EmptyStateText.Should()
            .Be("Load the PO number first, then come back to choose the part.");
    }

    [Fact]
    public void OnSelectedPartChanged_ShouldSetTheWorkflowPart_WhenNoPoPageIsRegistered()
    {
        var viewModel = CreateViewModel(out var workflow, out _);
        var part = new Model_InforVisualPart { PartID = "MMFTEST010" };

        viewModel.SelectedPart = part;

        workflow.Object.CurrentPart.Should().BeSameAs(part);
        viewModel.HasSelectedPart.Should().BeTrue();
    }

    [Fact]
    public void ResetToDefaults_ShouldClearTheSearchAndTheChosenPart()
    {
        var viewModel = CreateViewModel(out _, out _);
        viewModel.SearchText = "MMF";
        viewModel.SelectedPart = new Model_InforVisualPart { PartID = "MMFTEST011" };

        viewModel.ResetToDefaults();

        viewModel.SearchText.Should().BeEmpty();
        viewModel.SelectedPart.Should().BeNull();
        viewModel.HasSelectedPart.Should().BeFalse();
    }

    [Fact]
    public void HasQualityHold_ShouldFollowTheChosenPart()
    {
        var viewModel = CreateViewModel(out _, out _);

        viewModel.SelectedPart = new Model_InforVisualPart
        {
            PartID = "MMFSR0001",
            RequiresQualityHold = true,
            QualityHoldRestrictionType = "Sheet Material - Quality Hold Required",
        };

        viewModel.HasQualityHold.Should().BeTrue();
        viewModel.QualityHoldText.Should().Be("Sheet Material - Quality Hold Required");
    }

    [Fact]
    public void PoContextText_ShouldShowOnlyThePoNumber_WhenAPoIsLoaded()
    {
        var viewModel = CreateViewModel(out _, out _);

        viewModel
            .PoContextText.Should()
            .Be("PO-066868", "the stored number already carries its prefix, so no label belongs in front of it");
    }

    private static ViewModel_Receiving_PartSelection CreateViewModel(
        out Mock<IService_ReceivingWorkflow> workflow,
        out Mock<IService_ViewModelRegistry> registry)
    {
        workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupProperty(service => service.CurrentPart);
        workflow.SetupProperty(service => service.CurrentPONumber, "PO-066868");

        // No PO page is registered, which is the state a part page would be in if it were reached
        // without the PO step having built one.
        registry = new Mock<IService_ViewModelRegistry>();
        registry
            .Setup(service => service.GetViewModels<ViewModel_Receiving_POEntry>())
            .Returns([]);

        return new ViewModel_Receiving_PartSelection(
            workflow.Object,
            registry.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

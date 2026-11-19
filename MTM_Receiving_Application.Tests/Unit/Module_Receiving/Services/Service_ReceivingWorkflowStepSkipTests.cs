using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

/// <summary>
/// A PO with a single part number skips the part page. The walk then has one less stop, which only
/// works if the step machine reports the load step next and sends Back to the PO the clerk came from
/// rather than to a page that was never shown.
/// </summary>
public sealed class Service_ReceivingWorkflowStepSkipTests
{
    [Fact]
    public void SkipPartSelection_ShouldMoveToTheLoadStep_WhenThePartIsAlreadyChosen()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-500002";
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMC0000850" };

        var result = service.SkipPartSelection();

        result.Success.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.LoadEntry);
        service
            .CurrentSession.PoNumber.Should()
            .Be("PO-500002", "the load step reads the PO from the session");
    }

    [Fact]
    public void SkipPartSelection_ShouldFail_WhenNoPartIsChosen()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-500002";

        var result = service.SkipPartSelection();

        result.Success.Should().BeFalse();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.POEntry);
    }

    [Fact]
    public void SkipPartSelection_ShouldFail_WhenTheWalkIsPastThePartPage()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry);
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMC0000850" };

        var result = service.SkipPartSelection();

        result.Success.Should().BeFalse();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.LoadEntry);
    }

    [Fact]
    public void GoToPreviousStep_ShouldReturnToThePoStep_WhenThePartPageWasSkipped()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-500002";
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMC0000850" };
        service.SkipPartSelection();

        service.GoToPreviousStep();

        service
            .CurrentStep.Should()
            .Be(
                Enum_ReceivingWorkflowStep.POEntry,
                "the clerk never saw the part page, so Back cannot land on it"
            );
    }

    [Fact]
    public async Task GoToPreviousStep_ShouldReturnToThePartPage_WhenTheWalkVisitedIt()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-500001";
        service.IsPoPartListLoaded = true;
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMC0000850" };

        await service.AdvanceToNextStepAsync();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.PartSelection);

        await service.AdvanceToNextStepAsync();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.LoadEntry);

        service.GoToPreviousStep();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.PartSelection);
    }

    [Fact]
    public void GoToStep_ShouldRestoreThePartPage_WhenTheClerkReentersThePoStep()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-500002";
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMC0000850" };
        service.SkipPartSelection();

        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMF0001200" };

        service.SkipPartSelection();
        service.GoToPreviousStep();

        service
            .CurrentStep.Should()
            .Be(
                Enum_ReceivingWorkflowStep.POEntry,
                "the second PO was skipped too, so Back still belongs on the PO step"
            );
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldRefuseThePartPage_WhenThePoWasOnlyTypedIn()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-073026";

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeFalse();
        result
            .ValidationErrors.Should()
            .ContainMatch("*Load the PO first*", "the part page would open with no parts to choose");
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.POEntry);
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldOpenThePartPage_WhenThePoProducedAPartList()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPONumber = "PO-073026";
        service.IsPoPartListLoaded = true;

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.PartSelection);
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldNotNeedAPartList_WhenTheItemIsNonPo()
    {
        var service = CreateService(out _);
        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.IsNonPOItem = true;
        service.CurrentPart = new Model_InforVisualPart { PartID = "MMC0000850" };

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeTrue();
        service
            .CurrentStep.Should()
            .Be(
                Enum_ReceivingWorkflowStep.LoadEntry,
                "a non-PO receipt types its part on the PO page and has no part step"
            );
    }

    private static Service_ReceivingWorkflow CreateService(out Mock<IService_ReceivingValidation> validation)
    {
        validation = new Mock<IService_ReceivingValidation>();

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);

        return new Service_ReceivingWorkflow(
            new Mock<IService_SessionManager>().Object,
            new Mock<IService_ReceivingLabelData>().Object,
            new Mock<IService_MySQL_Receiving>().Object,
            validation.Object,
            new Mock<IService_QualityHoldWarning>().Object,
            receivingSettings.Object,
            new Mock<IService_AppSettings>().Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_UserPrivileges>().Object,
            new Mock<IService_UserPreferences>().Object
        );
    }
}

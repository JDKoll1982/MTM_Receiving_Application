using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_WorkflowTests
{
    [Fact]
    public async Task Constructor_ShouldLoadClearLabelDataAvailability()
    {
        var workflow = CreateWorkflowMock();
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(true);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        viewModel.CanClearLabelData.Should().BeTrue();
    }

    [Fact]
    public async Task StepChanged_ShouldRefreshClearLabelDataAvailability()
    {
        var workflow = CreateWorkflowMock();
        Enum_ReceivingWorkflowStep currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
        var hasActiveLabelData = true;

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);
        workflow
            .Setup(service => service.HasActiveLabelDataAsync())
            .ReturnsAsync(() => hasActiveLabelData);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        viewModel.CanClearLabelData.Should().BeTrue();

        hasActiveLabelData = false;
        currentStep = Enum_ReceivingWorkflowStep.POEntry;
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        viewModel.CanClearLabelData.Should().BeFalse();
    }

    [Fact]
    public async Task StepChanged_ShouldExposeHeaderContextSubtitle()
    {
        var workflow = CreateWorkflowMock();
        Enum_ReceivingWorkflowStep currentStep = Enum_ReceivingWorkflowStep.ModeSelection;

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        currentStep = Enum_ReceivingWorkflowStep.Review;
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

        viewModel.CurrentHeaderTitle.Should().Be("Receiving - Review & Save");
        viewModel
            .CurrentHeaderContextSubtitle.Should()
            .Be("Review every generated load before saving labels and database records.");
    }

    [Fact]
    public async Task NextStepCommand_ShouldExecuteOnThePoPage_WhenNoPartIsChosenYet()
    {
        // The part is chosen on the page after the PO, so the PO page must be free to move on
        // without one - once its part list has actually been loaded.
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow.SetupGet(service => service.CurrentPart).Returns((Model_InforVisualPart?)null);
        workflow.SetupGet(service => service.CurrentPONumber).Returns("PO-066868");
        workflow.SetupGet(service => service.IsPoPartListLoaded).Returns(true);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.NextStepCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task NextStepCommand_ShouldNotExecuteOnThePoPage_UntilThePoPartListIsLoaded()
    {
        // A typed-in PO number alone leaves the part page with nothing to choose from, so the walk
        // must not be able to reach it.
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow.SetupGet(service => service.CurrentPONumber).Returns("PO-066868");
        workflow.SetupGet(service => service.IsPoPartListLoaded).Returns(false);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.NextStepCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task NextStepCommand_ShouldNotExecute_WhenThePartStepHasNoSelectedPart()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow.SetupGet(service => service.CurrentPart).Returns((Model_InforVisualPart?)null);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.NextStepCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task NextStepCommand_ShouldExecute_WhenThePartStepHasASelectedPart()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow
            .SetupGet(service => service.CurrentPart)
            .Returns(new Model_InforVisualPart { PartID = "TEST-PART" });

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.NextStepCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task PreviousStepCommand_ShouldNotExecute_OnModeSelectionStep()
    {
        var workflow = CreateWorkflowMock();

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.PreviousStepCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task NavigationCommands_ShouldExecute_WhenBothButtonsAreOffered()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.LoadEntry);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.PreviousStepCommand.CanExecute(null).Should().BeTrue();
        viewModel.NextStepCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task HeaderStepBar_ShouldListTheWalkAndPointAtThePartStep_WhenThePartStepIsOpen()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel.IsHeaderStepBarVisible.Should().BeTrue();
        viewModel
            .HeaderStepBarSteps.Should()
            .Equal("Enter PO", "Part", "Loads", "Weight/Qty", "Heat/Lot", "Package", "Review");
        viewModel
            .HeaderStepBarIndex.Should()
            .Be(1, "the header bar is zero-based and Part is the second step");
    }

    [Fact]
    public async Task HeaderStepBar_ShouldHideItself_WhenTheModeIsNotAWalk()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ManualEntry);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        RaiseStepChanged(workflow);

        viewModel
            .IsHeaderStepBarVisible.Should()
            .BeFalse("manual entry is not part of the guided walk");
        viewModel
            .HeaderStepBarIndex.Should()
            .Be(-1);
    }

    private static void RaiseStepChanged(Mock<IService_ReceivingWorkflow> workflow)
    {
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ModeSelection);
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);

        return workflow;
    }

    private static ViewModel_Receiving_Workflow CreateViewModel(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        var dispatcher = new Mock<IService_Dispatcher>();
        dispatcher
            .Setup(service => service.TryEnqueue(It.IsAny<Action>()))
            .Returns<Action>(callback =>
            {
                callback();
                return true;
            });

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service =>
                service.FormatAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<int?>())
            )
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service =>
                service.FormatAsync(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<object?>(),
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(string.Empty);

        var registry = new Mock<IService_ViewModelRegistry>();
        registry
            .Setup(service => service.GetViewModels<ViewModel_Receiving_EditMode>())
            .Returns(Array.Empty<ViewModel_Receiving_EditMode>());

        var lotMarking = new Mock<IService_Tool_LotMarking>();
        lotMarking
            .Setup(service => service.GetEnabledPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<
                        MTM_Receiving_Application.Module_ShipRec_Tools.Models.Model_Tool_LotMarkingScope
                    >()
                )
            );

        return new ViewModel_Receiving_Workflow(
            workflow.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            dispatcher.Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_LabelViewLauncher>().Object,
            receivingSettings.Object,
            registry.Object,
            lotMarking.Object,
            new Mock<IService_Notification>().Object
        );
    }
}

using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ShellActionTests
{
    [Fact]
    public async Task ShellActions_ShouldOfferBackNextClearLabelDataAndHelpInOrder_WhenStepIsPartSelection()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow
            .SetupGet(service => service.CurrentPart)
            .Returns(new Model_InforVisualPart { PartID = "TEST-PART" });
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(true);

        var viewModel = await CreateViewModelAsync(workflow);

        var shell = viewModel.ShellActions;
        shell.Back.IsVisible.Should().BeTrue();
        shell.Next.IsVisible.Should().BeTrue();
        shell.ClearLabelData.IsVisible.Should().BeTrue();
        shell.Help.IsVisible.Should().BeTrue();
        shell
            .Next.ShowDividerAfter.Should()
            .BeFalse("the only divider in the centre group sits before Next, never after the last action");
        shell.Next.Command.Should().BeSameAs(viewModel.NextStepCommand);
        shell.Back.Command.Should().BeSameAs(viewModel.PreviousStepCommand);
        shell.ClearLabelData.Command.Should().BeSameAs(viewModel.ResetLabelDataCommand);
    }

    [Fact]
    public async Task ClearLabelDataSlot_ShouldBeEnabledAndExecutable_WhenActiveLabelDataExists()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow
            .SetupGet(service => service.CurrentPart)
            .Returns(new Model_InforVisualPart { PartID = "TEST-PART" });
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(true);

        var viewModel = await CreateViewModelAsync(workflow);

        var slot = viewModel.ShellActions.ClearLabelData;
        slot.IsEnabled.Should().BeTrue();
        slot.Command.Should().NotBeNull();
        slot
            .Command!.CanExecute(null)
            .Should()
            .BeTrue(
                "the strip binds the command with no parameter, so a command that takes a value type would report CanExecute(null) as false and leave the button disabled"
            );
    }

    [Fact]
    public async Task ClearLabelDataSlot_ShouldBeDisabled_WhenNoActiveLabelDataExists()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow
            .SetupGet(service => service.CurrentPart)
            .Returns(new Model_InforVisualPart { PartID = "TEST-PART" });
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.ClearLabelData.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ClearLabelDataSlot_ShouldCarryTheClearAllCommand_ForAShiftClick()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow
            .SetupGet(service => service.CurrentPart)
            .Returns(new Model_InforVisualPart { PartID = "TEST-PART" });
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(true);

        var viewModel = await CreateViewModelAsync(workflow);

        var slot = viewModel.ShellActions.ClearLabelData;
        slot.ShiftCommand.Should().BeSameAs(viewModel.ResetLabelDataAllCommand);
        slot
            .ShiftCommand!.CanExecute(null)
            .Should()
            .BeTrue("the strip invokes the shift variant with no parameter, as it does the plain one");
    }

    [Fact]
    public async Task ShellActions_ShouldOmitBack_WhenStepIsEnterPoNumber()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel
            .ShellActions.Back.IsVisible.Should()
            .BeFalse("the first guided step cannot go back; the control is absent, not disabled");
        viewModel.ShellActions.Next.IsVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShellActions_ShouldDisableNext_WhenThePoPartListHasNotLoaded()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow.SetupGet(service => service.CurrentPONumber).Returns("TEST-PO-1");
        workflow.SetupGet(service => service.IsPoPartListLoaded).Returns(false);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.Next.IsVisible.Should().BeTrue();
        viewModel.ShellActions.Next.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldDisableNext_WhenNoPartIsChosenOnSelectPart()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow.SetupGet(service => service.CurrentPart).Returns((Model_InforVisualPart?)null);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.Next.IsVisible.Should().BeTrue();
        viewModel.ShellActions.Next.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldDisableClearLabelData_WhenTheUserHasNoActiveLabelData()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.PartSelection);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.ClearLabelData.IsVisible.Should().BeTrue();
        viewModel.ShellActions.ClearLabelData.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldOfferClearLabelData_WhenStepIsManualEntry()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ManualEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.ClearLabelData.IsVisible.Should().BeTrue();
        viewModel.ShellActions.Back.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldOfferClearLabelData_WhenStepIsEditMode()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.EditMode);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.ClearLabelData.IsVisible.Should().BeTrue();
        viewModel.ShellActions.Back.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldCarryTheExistingShowHelpCommand_WhenHelpIsInvoked()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.LoadEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.Help.Command.Should().BeSameAs(viewModel.ShowHelpCommand);
    }

    [Fact]
    public async Task ShellActions_ShouldOfferGuidedManualEditOverlayInThatOrder_WhenThePageIsReceiving()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        var shell = viewModel.ShellActions;
        shell.ShowModeGroup.Should().BeTrue();
        shell.Guided.Mode.Should().Be("guided");
        shell.Manual.Mode.Should().Be("manual");
        shell.Edit.Mode.Should().Be("edit");
        shell.Overlay.Mode.Should().Be("overlay");
        shell.Guided.Command.Should().BeSameAs(viewModel.EnterGuidedModeCommand);
        shell.Manual.Command.Should().BeSameAs(viewModel.EnterManualModeCommand);
        shell.Edit.Command.Should().BeSameAs(viewModel.EnterEditModeCommand);
        shell.Overlay.Command.Should().BeSameAs(viewModel.OpenLabelOverlayWindowCommand);
        shell
            .Overlay.CanBeDefault.Should()
            .BeFalse("Overlay opens a window and cannot be a default mode");
    }

    [Fact]
    public async Task NextSlot_ShouldMatchNextStepCommandCanExecute_WhenThePoPartListHasNotLoaded()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow.SetupGet(service => service.CurrentPONumber).Returns("TEST-PO-1");
        workflow.SetupGet(service => service.IsPoPartListLoaded).Returns(false);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel
            .ShellActions.Next.IsEnabled.Should()
            .Be(
                viewModel.NextStepCommand.CanExecute(null),
                "the Ctrl+Right shortcut and the button must never disagree"
            );
    }

    [Fact]
    public async Task ShellActions_ShouldOfferNothing_WhenStepIsSaving()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.Saving);

        var viewModel = await CreateViewModelAsync(workflow);

        var shell = viewModel.ShellActions;
        shell.IsVisible.Should().BeFalse();
        shell.ShowModeGroup.Should().BeFalse();
        shell.Back.IsVisible.Should().BeFalse();
        shell.Next.IsVisible.Should().BeFalse();
        shell.ClearLabelData.IsVisible.Should().BeFalse();
        shell.Help.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task EnterManualModeCommand_ShouldNotResetTheWorkflow_WhenManualEntryIsAlreadyOpen()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ManualEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.EnterManualModeCommand.Execute(null);
        await Task.Delay(25);

        workflow.Verify(service => service.ResetWorkflowAsync(), Times.Never);
        workflow.Verify(
            service => service.GoToStep(It.IsAny<Enum_ReceivingWorkflowStep>()),
            Times.Never
        );
    }

    [Fact]
    public async Task EnterGuidedModeCommand_ShouldNotResetTheWorkflow_WhenAWalkStepIsAlreadyOpen()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.HeatLotEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.EnterGuidedModeCommand.Execute(null);
        await Task.Delay(25);

        workflow.Verify(service => service.ResetWorkflowAsync(), Times.Never);
    }

    [Fact]
    public async Task ShellActions_ShouldMarkManualAsTheActiveMode_WhenManualEntryIsOpen()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ManualEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.Manual.IsActive.Should().BeTrue();
        viewModel.ShellActions.Guided.IsActive.Should().BeFalse();
        viewModel.ShellActions.Edit.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldMarkGuidedAsTheActiveMode_WhenAWalkStepIsOpen()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.HeatLotEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.Guided.IsActive.Should().BeTrue();
        viewModel.ShellActions.Manual.IsActive.Should().BeFalse();
        viewModel.ShellActions.Edit.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ShellActions_ShouldMarkTheBackSlotAsBack_WhenTheStepCanGoBack()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.LoadEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel.ShellActions.Back.Kind.Should().Be(Enum_ShellActionKind.Back);
    }

    [Fact]
    public async Task ShellActions_ShouldLeaveTheBackSlotNeutral_WhenStartNewEntryOccupiesIt()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.Complete);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel
            .ShellActions.Back.Kind.Should()
            .Be(
                Enum_ShellActionKind.Plain,
                "Start New Entry is not a way out, so it must not read as one"
            );
    }

    [Fact]
    public async Task ShellActions_ShouldLeaveImageSearchHidden_WhenThePageIsReceiving()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);

        var viewModel = await CreateViewModelAsync(workflow);

        viewModel
            .ShellActions.ImageSearch.IsVisible.Should()
            .BeFalse("Image Search belongs to Non-Inventoried Labels, so Receiving leaves it hidden");
        viewModel.ShellActions.IsImageSearchDefault.Should().BeFalse();
        viewModel.ShellActions.ShowModeGroup.Should().BeTrue();
    }

    private static async Task<ViewModel_Receiving_Workflow> CreateViewModelAsync(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        return viewModel;
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
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
            new Mock<IService_UserPreferences>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

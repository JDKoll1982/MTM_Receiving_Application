using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ShellPrimaryActionTests
{
    [Fact]
    public async Task ShellActions_ShouldPutSaveAndFinishInTheNextSlot_WhenStepIsManualEntry()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ManualEntry);

        var viewModel = CreateWorkflowViewModel(workflow);
        await Task.Delay(25);

        var manualEntry = CreateManualEntryViewModel(workflow);
        await Task.Delay(25);
        manualEntry.ManualEntrySaveAndFinishText = "TEST-SAVE-AND-FINISH";

        viewModel.AttachShellActionHosts(
            manualEntry,
            CreateEditModeViewModel(workflow),
            CreateReviewViewModel(workflow)
        );
        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

        viewModel.ShellActions.Next.IsVisible.Should().BeTrue();
        viewModel.ShellActions.Next.Command.Should().BeSameAs(manualEntry.SaveCommand);
        viewModel.ShellActions.Next.Label.Should().Be("TEST-SAVE-AND-FINISH");
        viewModel
            .ShellActions.Next.ShowDividerAfter.Should()
            .BeTrue("the hosted save verb keeps the divider before Clear Label Data");
    }

    [Fact]
    public async Task ShellActions_ShouldPutSaveAndFinishInTheNextSlotAndOmitBack_WhenStepIsEditMode()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.EditMode);

        var viewModel = CreateWorkflowViewModel(workflow);
        await Task.Delay(25);

        var editMode = CreateEditModeViewModel(workflow);
        await Task.Delay(25);
        editMode.EditModeSaveAndFinishText = "TEST-SAVE-AND-FINISH-EDIT";

        viewModel.AttachShellActionHosts(
            CreateManualEntryViewModel(workflow),
            editMode,
            CreateReviewViewModel(workflow)
        );
        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

        viewModel.ShellActions.Next.IsVisible.Should().BeTrue();
        viewModel.ShellActions.Next.Command.Should().BeSameAs(editMode.SaveCommand);
        viewModel.ShellActions.Next.Label.Should().Be("TEST-SAVE-AND-FINISH-EDIT");
        viewModel
            .ShellActions.Back.IsVisible.Should()
            .BeFalse("Edit Mode is a hosted mode page and cannot go back");
    }

    [Fact]
    public async Task ShellActions_ShouldPutSaveToDatabaseInTheNextSlotAndOfferNoAddAnother_WhenStepIsReview()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.Review);

        var viewModel = CreateWorkflowViewModel(workflow);
        await Task.Delay(25);

        var review = CreateReviewViewModel(workflow);
        await Task.Delay(25);
        review.ReviewSaveToDatabaseText = "TEST-SAVE-TO-DATABASE";

        viewModel.AttachShellActionHosts(
            CreateManualEntryViewModel(workflow),
            CreateEditModeViewModel(workflow),
            review
        );
        await Task.Delay(25);
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

        var shell = viewModel.ShellActions;
        shell.Next.IsVisible.Should().BeTrue();
        shell.Next.Command.Should().BeSameAs(review.SaveCommand);
        shell.Next.Label.Should().Be("TEST-SAVE-TO-DATABASE");
        shell.Next.Label.Should().NotBe("Add Another Part/PO");
        shell.Back.IsVisible.Should().BeTrue("Review keeps the guided Back verb");
    }

    [Fact]
    public async Task ShellActions_ShouldPutStartNewEntryInTheBackSlotAndKeepOverlay_WhenStepIsComplete()
    {
        var workflow = CreateWorkflowMock();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.Complete);

        var viewModel = CreateWorkflowViewModel(workflow);
        await Task.Delay(25);
        viewModel.CompletionStartNewEntryText = "TEST-START-NEW-ENTRY";

        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

        var shell = viewModel.ShellActions;
        shell.Back.IsVisible.Should().BeTrue();
        shell.Back.Command.Should().BeSameAs(viewModel.StartNewEntryCommand);
        shell.Back.Label.Should().Be("TEST-START-NEW-ENTRY");
        shell.Next.IsVisible.Should().BeFalse("the finish page has no primary save verb");
        shell.ShowModeGroup.Should().BeTrue();
        shell.Overlay.Command.Should().BeSameAs(viewModel.OpenLabelOverlayWindowCommand);
    }

    [Theory]
    [InlineData(Enum_ReceivingWorkflowStep.ManualEntry)]
    [InlineData(Enum_ReceivingWorkflowStep.EditMode)]
    public async Task ShellActions_ShouldOfferNoRowToolbarActions_WhenStepIsManualEntryOrEditMode(
        Enum_ReceivingWorkflowStep step
    )
    {
        var workflow = CreateWorkflowMock();
        workflow.SetupGet(service => service.CurrentStep).Returns(step);

        var viewModel = CreateWorkflowViewModel(workflow);
        await Task.Delay(25);

        if (step == Enum_ReceivingWorkflowStep.ManualEntry)
        {
            var manualEntry = CreateManualEntryViewModel(workflow);
            await Task.Delay(25);
            viewModel.AttachShellActionHosts(
                manualEntry,
                CreateEditModeViewModel(workflow),
                CreateReviewViewModel(workflow)
            );
            await Task.Delay(25);
            workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

            viewModel.ShellActions.Next.Command.Should().BeSameAs(manualEntry.SaveCommand);
            viewModel.ShellActions.Next.Command.Should().NotBeSameAs(manualEntry.AddRowCommand);
            viewModel.ShellActions.Next.Command.Should().NotBeSameAs(manualEntry.RemoveRowCommand);
        }
        else
        {
            var editMode = CreateEditModeViewModel(workflow);
            await Task.Delay(25);
            viewModel.AttachShellActionHosts(
                CreateManualEntryViewModel(workflow),
                editMode,
                CreateReviewViewModel(workflow)
            );
            await Task.Delay(25);
            workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);

            viewModel.ShellActions.Next.Command.Should().BeSameAs(editMode.SaveCommand);
            viewModel.ShellActions.Next.Command.Should().NotBeSameAs(editMode.RemoveRowCommand);
            viewModel.ShellActions.Next.Command.Should().NotBeSameAs(editMode.ApplyBatchCommand);
        }
    }

    private static Mock<IService_ReceivingWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.POEntry);
        workflow
            .SetupGet(service => service.CurrentSession)
            .Returns(new Model_ReceivingSession());
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);
        return workflow;
    }

    private static ViewModel_Receiving_Workflow CreateWorkflowViewModel(
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

        var registry = new Mock<IService_ViewModelRegistry>();
        registry
            .Setup(service => service.GetViewModels<ViewModel_Receiving_EditMode>())
            .Returns(Array.Empty<ViewModel_Receiving_EditMode>());

        return new ViewModel_Receiving_Workflow(
            workflow.Object,
            CreateErrorHandlerMock().Object,
            new Mock<IService_LoggingUtility>().Object,
            dispatcher.Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_LabelViewLauncher>().Object,
            CreateReceivingSettingsMock().Object,
            registry.Object,
            CreateLotMarkingMock().Object,
            new Mock<IService_UserPreferences>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static ViewModel_Receiving_ManualEntry CreateManualEntryViewModel(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        var validation = new Mock<IService_ReceivingValidation>();
        validation
            .SetupGet(service => service.PresetLocations)
            .Returns(Array.Empty<string>());

        return new ViewModel_Receiving_ManualEntry(
            workflow.Object,
            validation.Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_QualityHoldWarning>().Object,
            CreateErrorHandlerMock().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Help>().Object,
            CreateReceivingSettingsMock().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static ViewModel_Receiving_EditMode CreateEditModeViewModel(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        return new ViewModel_Receiving_EditMode(
            workflow.Object,
            new Mock<IService_MySQL_Receiving>().Object,
            new Mock<IService_ReceivingLabelData>().Object,
            new Mock<IService_Pagination>().Object,
            CreateErrorHandlerMock().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Help>().Object,
            CreateReceivingSettingsMock().Object,
            new Mock<IService_Notification>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_UserPrivileges>().Object,
            new Mock<IService_ViewModelRegistry>().Object
        );
    }

    private static ViewModel_Receiving_Review CreateReviewViewModel(
        Mock<IService_ReceivingWorkflow> workflow
    )
    {
        return new ViewModel_Receiving_Review(
            workflow.Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_Window>().Object,
            CreateReceivingSettingsMock().Object,
            CreateLotMarkingMock().Object,
            CreateErrorHandlerMock().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Mock<IService_ReceivingSettings> CreateReceivingSettingsMock()
    {
        var settings = new Mock<IService_ReceivingSettings>();
        settings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        settings
            .Setup(service => service.GetBoolAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        settings
            .Setup(service => service.GetIntAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(0);
        settings
            .Setup(service =>
                service.FormatAsync(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<int?>())
            )
            .ReturnsAsync(string.Empty);
        settings
            .Setup(service =>
                service.FormatAsync(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<object?>(),
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(string.Empty);
        return settings;
    }

    private static Mock<IService_ErrorHandler> CreateErrorHandlerMock()
    {
        var errorHandler = new Mock<IService_ErrorHandler>();
        errorHandler
            .Setup(service =>
                service.HandleErrorAsync(
                    It.IsAny<string>(),
                    It.IsAny<Enum_ErrorSeverity>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<bool>()
                )
            )
            .Returns(Task.CompletedTask);
        errorHandler
            .Setup(service =>
                service.ShowErrorDialogAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Enum_ErrorSeverity>()
                )
            )
            .Returns(Task.CompletedTask);
        errorHandler
            .Setup(service =>
                service.ShowUserErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
            )
            .Returns(Task.CompletedTask);
        return errorHandler;
    }

    private static Mock<IService_Tool_LotMarking> CreateLotMarkingMock()
    {
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
        return lotMarking;
    }
}

using FluentAssertions;
using Material.Icons;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_WorkFlowViewModelTests
{
    [Fact]
    public async Task Constructor_ShouldStartWorkflowAsync()
    {
        var workflow = CreateWorkflowMock();

        _ = CreateViewModel(workflow);
        await Task.Delay(25);

        workflow.Verify(service => service.StartWorkflowAsync(), Times.Once);
    }

    [Fact]
    public async Task StepChanged_ShouldShowImageSearchAndUpdateHeaderTitle()
    {
        var workflow = CreateWorkflowMock();
        Enum_DunnageWorkflowStep currentStep = Enum_DunnageWorkflowStep.ModeSelection;
        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        currentStep = Enum_DunnageWorkflowStep.ImagePartSearch;
        workflow.Raise(
            service => service.StepChanged += null,
            workflow.Object,
            System.EventArgs.Empty
        );

        viewModel.IsImagePartSearchVisible.Should().BeTrue();
        viewModel.IsModeSelectionVisible.Should().BeFalse();
        viewModel.CurrentHeaderTitle.Should().Be("Dunnage - Search Parts by Image");
        viewModel.CurrentHeaderContextTitle.Should().BeNull();
    }

    [Fact]
    public async Task StepChanged_ShouldExposePartSelectionHeaderContext()
    {
        var workflow = CreateWorkflowMock();
        Enum_DunnageWorkflowStep currentStep = Enum_DunnageWorkflowStep.ModeSelection;
        var session = new Model_DunnageSession
        {
            SelectedTypeName = "Bags",
            SelectedType = new Model_DunnageType { Icon = "PackageVariantClosed" },
        };

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);
        workflow.SetupGet(service => service.CurrentSession).Returns(session);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        currentStep = Enum_DunnageWorkflowStep.PartSelection;
        workflow.Raise(
            service => service.StepChanged += null,
            workflow.Object,
            System.EventArgs.Empty
        );

        viewModel.IsPartSelectionVisible.Should().BeTrue();
        viewModel.CurrentHeaderTitle.Should().Be("Dunnage - Select Part");
        viewModel.CurrentHeaderContextTitle.Should().Be("Select Part - Bags");
        viewModel
            .CurrentHeaderContextSubtitle.Should()
            .Be(
                "Use the search function to quickly find parts. Parts with a green badge are tracked in inventory."
            );
        viewModel.CurrentHeaderContextIconKind.Should().Be(MaterialIconKind.PackageVariantClosed);
    }

    [Fact]
    public async Task StepChanged_ShouldExposeQuantityEntryHeaderContext()
    {
        var workflow = CreateWorkflowMock();
        Enum_DunnageWorkflowStep currentStep = Enum_DunnageWorkflowStep.ModeSelection;

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        currentStep = Enum_DunnageWorkflowStep.QuantityEntry;
        workflow.Raise(
            service => service.StepChanged += null,
            workflow.Object,
            System.EventArgs.Empty
        );

        viewModel.IsQuantityEntryVisible.Should().BeTrue();
        viewModel.CurrentHeaderContextTitle.Should().Be("Enter Loads");
        viewModel
            .CurrentHeaderContextSubtitle.Should()
            .Be("Set the number of loads and enter the quantity for each load.");
        viewModel.CurrentHeaderContextIconKind.Should().BeNull();
    }

    [Fact]
    public async Task StepChanged_ShouldExposeDetailsEntryHeaderContext()
    {
        var workflow = CreateWorkflowMock();
        Enum_DunnageWorkflowStep currentStep = Enum_DunnageWorkflowStep.ModeSelection;

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        currentStep = Enum_DunnageWorkflowStep.DetailsEntry;
        workflow.Raise(
            service => service.StepChanged += null,
            workflow.Object,
            System.EventArgs.Empty
        );

        viewModel.IsDetailsEntryVisible.Should().BeTrue();
        viewModel.CurrentHeaderContextTitle.Should().Be("Enter Details");
        viewModel
            .CurrentHeaderContextSubtitle.Should()
            .Be(
                "Enter the PO number, confirm the location, and review per-load details before saving."
            );
        viewModel.CurrentHeaderContextIconKind.Should().BeNull();
    }

    [Fact]
    public async Task ReturnToModeSelectionAsync_ShouldClearSessionAndNavigate_WhenNothingIsUnsaved()
    {
        var workflow = CreateWorkflowMock();
        workflow.Setup(service => service.HasUnsavedData()).Returns(false);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);

        await viewModel.ReturnToModeSelectionCommand.ExecuteAsync(null);

        workflow.Verify(service => service.ClearSession(), Times.Once);
        workflow.Verify(
            service => service.GoToStep(Enum_DunnageWorkflowStep.ModeSelection),
            Times.Once
        );
    }

    [Fact]
    public async Task NavigationLockChanged_ShouldUpdateCanNavigate()
    {
        var workflow = CreateWorkflowMock();
        var isLocked = false;
        workflow.SetupGet(service => service.IsNavigationLocked).Returns(() => isLocked);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        viewModel.CanNavigate.Should().BeTrue();

        isLocked = true;
        workflow.Raise(
            service => service.NavigationLockChanged += null,
            workflow.Object,
            System.EventArgs.Empty
        );

        viewModel.CanNavigate.Should().BeFalse();
    }

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
        Enum_DunnageWorkflowStep currentStep = Enum_DunnageWorkflowStep.ModeSelection;
        var hasActiveLabelData = true;

        workflow.SetupGet(service => service.CurrentStep).Returns(() => currentStep);
        workflow
            .Setup(service => service.HasActiveLabelDataAsync())
            .ReturnsAsync(() => hasActiveLabelData);

        var viewModel = CreateViewModel(workflow);
        await Task.Delay(25);
        viewModel.CanClearLabelData.Should().BeTrue();

        hasActiveLabelData = false;
        currentStep = Enum_DunnageWorkflowStep.TypeSelection;
        workflow.Raise(
            service => service.StepChanged += null,
            workflow.Object,
            System.EventArgs.Empty
        );
        await Task.Delay(25);

        viewModel.CanClearLabelData.Should().BeFalse();
    }

    private static Mock<IService_DunnageWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());
        workflow.Setup(service => service.StartWorkflowAsync()).ReturnsAsync(true);
        workflow.Setup(service => service.HasActiveLabelDataAsync()).ReturnsAsync(false);
        workflow.Setup(service => service.HasUnsavedData()).Returns(false);

        return workflow;
    }

    private static ViewModel_Dunnage_WorkFlowViewModel CreateViewModel(
        Mock<IService_DunnageWorkflow> workflow
    )
    {
        return new ViewModel_Dunnage_WorkFlowViewModel(
            workflow.Object,
            new Mock<IService_DunnageSettings>().Object,
            new Mock<IService_LabelViewLauncher>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

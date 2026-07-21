using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_ImagePartSearchDialogTests
{
    [Fact]
    public async Task LoadPartsAsync_ShouldOnlyDisplayPartsWithImages_InPartIdOrder()
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetAllPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnagePart>
                    {
                        new()
                        {
                            PartId = "PART-200",
                            DunnageTypeName = "Bins",
                            ImagePath = "Images/part200.png",
                        },
                        new()
                        {
                            PartId = "PART-100",
                            DunnageTypeName = "Bags",
                            ImagePath = "Images/part100.png",
                        },
                        new()
                        {
                            PartId = "PART-300",
                            DunnageTypeName = "Wrap",
                            ImagePath = string.Empty,
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(dunnageService: dunnageService);

        await viewModel.LoadPartsCommand.ExecuteAsync(null);

        viewModel.DisplayedParts.Select(part => part.PartId).Should().Equal("PART-100", "PART-200");
        viewModel.HasNoResults.Should().BeFalse();
    }

    [Fact]
    public async Task Constructor_ShouldRefreshParts_WhenImageSearchIsAlreadyTheCurrentStep()
    {
        var loadedParts = new List<Model_DunnagePart>
        {
            new()
            {
                PartId = "PART-100",
                DunnageTypeName = "Bags",
                ImagePath = "Images/part100.png",
            },
        };

        var loadTriggered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetAllPartsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(loadedParts))
            .Callback(() => loadTriggered.TrySetResult(true));

        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(new Model_DunnageSession());
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_DunnageWorkflowStep.ImagePartSearch);

        var viewModel = CreateViewModel(dunnageService, workflow);

        await loadTriggered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Yield();

        viewModel.DisplayedParts.Select(part => part.PartId).Should().Equal("PART-100");
        dunnageService.Verify(service => service.GetAllPartsAsync(), Times.Once);
    }

    [Fact]
    public async Task FilterText_ShouldFilterDisplayedParts_ByPartTypeOrLocation()
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetAllPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnagePart>
                    {
                        new()
                        {
                            PartId = "PART-100",
                            DunnageTypeName = "Bags",
                            HomeLocation = "A-01",
                            ImagePath = "Images/part100.png",
                        },
                        new()
                        {
                            PartId = "PART-200",
                            DunnageTypeName = "Shrink Wrap",
                            HomeLocation = "B-09",
                            ImagePath = "Images/part200.png",
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(dunnageService: dunnageService);
        await viewModel.LoadPartsCommand.ExecuteAsync(null);

        viewModel.FilterText = "wrap";

        viewModel.DisplayedParts.Should().ContainSingle(part => part.PartId == "PART-200");

        viewModel.FilterText = "A-01";

        viewModel.DisplayedParts.Should().ContainSingle(part => part.PartId == "PART-100");
    }

    [Fact]
    public async Task SelectPartAsync_ShouldPopulateSessionAndNavigateToPartSelection()
    {
        var session = new Model_DunnageSession();
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(session);

        var selectedType = new Model_DunnageType { Id = 8, TypeName = "Shrink Wrap" };

        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetTypeByIdAsync(8))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(selectedType));

        var viewModel = CreateViewModel(dunnageService, workflow, session);
        var part = new Model_DunnagePart
        {
            PartId = "PART-500",
            TypeId = 8,
            DunnageTypeName = "Shrink Wrap",
        };

        await viewModel.SelectPartCommand.ExecuteAsync(part);

        session.SelectedPart.Should().Be(part);
        session.SelectedTypeId.Should().Be(8);
        session.SelectedTypeName.Should().Be("Shrink Wrap");
        session.SelectedType.Should().Be(selectedType);
        workflow.Verify(
            service => service.GoToStep(Enum_DunnageWorkflowStep.PartSelection),
            Times.Once
        );
    }

    private static ViewModel_Dunnage_ImagePartSearchDialog CreateViewModel(
        Mock<IService_MySQL_Dunnage>? dunnageService = null,
        Mock<IService_DunnageWorkflow>? workflow = null,
        Model_DunnageSession? workflowSession = null
    )
    {
        if (dunnageService is null)
        {
            dunnageService = new Mock<IService_MySQL_Dunnage>();
            dunnageService
                .Setup(service => service.GetAllPartsAsync())
                .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_DunnagePart>()));
        }

        workflow ??= new Mock<IService_DunnageWorkflow>();
        workflow
            .SetupGet(service => service.CurrentSession)
            .Returns(workflowSession ?? new Model_DunnageSession());

        return new ViewModel_Dunnage_ImagePartSearchDialog(
            dunnageService.Object,
            workflow.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

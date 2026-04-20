using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_ReviewViewModelTests
{
    [Fact]
    public async Task LoadSessionLoadsAsync_ShouldNormalizeCurrentLoadPoAndShowVendor_WhenPoExists()
    {
        var workflowService = new Mock<IService_DunnageWorkflow>();
        workflowService.SetupGet(service => service.CurrentSession).Returns(
            new Model_DunnageSession
            {
                Loads = new ObservableCollection<Model_DunnageLoad>
                {
                    new()
                    {
                        PartId = "DUN-100",
                        PoNumber = "62450",
                    },
                },
            }
        );

        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPOWithPartsAsync("PO-062450"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(
                    new Model_InforVisualPO
                    {
                        PONumber = "PO-062450",
                        Vendor = "Precision Plating Inc.",
                    }
                )
            );

        var viewModel = CreateViewModel(workflowService.Object, inforVisualService.Object);

        await viewModel.LoadSessionLoadsAsync();

        viewModel.CurrentLoad.Should().NotBeNull();
        viewModel.CurrentLoad!.PoNumber.Should().Be("PO-062450");
        viewModel.CurrentVendorName.Should().Be("Precision Plating Inc.");
        viewModel.CurrentVendorErrorMessage.Should().BeEmpty();
        viewModel.HasCurrentVendorState.Should().BeTrue();
    }

    [Fact]
    public async Task LoadSessionLoadsAsync_ShouldShowNotFoundMessage_WhenPoFormatIsValidButPoDoesNotExist()
    {
        var workflowService = new Mock<IService_DunnageWorkflow>();
        workflowService.SetupGet(service => service.CurrentSession).Returns(
            new Model_DunnageSession
            {
                Loads = new ObservableCollection<Model_DunnageLoad>
                {
                    new()
                    {
                        PartId = "DUN-100",
                        PoNumber = "PO-066866",
                    },
                },
            }
        );

        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.GetPOWithPartsAsync("PO-066866"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(null));

        var viewModel = CreateViewModel(workflowService.Object, inforVisualService.Object);

        await viewModel.LoadSessionLoadsAsync();

        viewModel.CurrentVendorName.Should().BeEmpty();
        viewModel.CurrentVendorErrorMessage.Should().Be("PO-066866 was not found in Infor Visual.");
        viewModel.HasCurrentVendorState.Should().BeTrue();
    }

    [Fact]
    public async Task LoadSessionLoadsAsync_ShouldHideVendorState_WhenPoIsMalformed()
    {
        var workflowService = new Mock<IService_DunnageWorkflow>();
        workflowService.SetupGet(service => service.CurrentSession).Returns(
            new Model_DunnageSession
            {
                Loads = new ObservableCollection<Model_DunnageLoad>
                {
                    new()
                    {
                        PartId = "DUN-100",
                        PoNumber = "Nothing Entered",
                    },
                },
            }
        );

        var inforVisualService = new Mock<IService_InforVisual>();
        var viewModel = CreateViewModel(workflowService.Object, inforVisualService.Object);

        await viewModel.LoadSessionLoadsAsync();

        viewModel.CurrentVendorName.Should().BeEmpty();
        viewModel.CurrentVendorErrorMessage.Should().BeEmpty();
        viewModel.HasCurrentVendorState.Should().BeFalse();
        inforVisualService.Verify(service => service.GetPOWithPartsAsync(It.IsAny<string>()), Times.Never);
    }

    private static ViewModel_Dunnage_Review CreateViewModel(
        IService_DunnageWorkflow workflowService,
        IService_InforVisual inforVisualService
    )
    {
        return new ViewModel_Dunnage_Review(
            workflowService,
            new Mock<IService_MySQL_Dunnage>().Object,
            inforVisualService,
            new Mock<IService_Help>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

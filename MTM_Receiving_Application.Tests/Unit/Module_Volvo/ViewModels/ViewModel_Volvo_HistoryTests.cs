using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.ViewModels;

public sealed class ViewModel_Volvo_HistoryTests
{
    [Fact]
    public void EditAndDeleteCommands_ShouldOnlyEnableForCompletedArchivedSelection()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedShipment = new Model_VolvoShipment
        {
            Id = 1,
            ShipmentNumber = 1001,
            IsArchived = true,
            Status = VolvoShipmentStatus.PendingPo,
        };

        viewModel.EditCommand.CanExecute(null).Should().BeFalse();
        viewModel.DeleteCommand.CanExecute(null).Should().BeFalse();

        viewModel.SelectedShipment = new Model_VolvoShipment
        {
            Id = 2,
            ShipmentNumber = 1002,
            IsArchived = true,
            Status = VolvoShipmentStatus.Completed,
        };

        viewModel.EditCommand.CanExecute(null).Should().BeTrue();
        viewModel.DeleteCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task LoadRecentShipmentsAsync_ShouldReplaceHistory_WhenLatestResultsDiffer()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .SetupSequence(mediator =>
                mediator.Send(It.IsAny<GetRecentShipmentsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_VolvoShipment>
                    {
                        new() { Id = 1, ShipmentNumber = 1001 },
                        new() { Id = 2, ShipmentNumber = 1002 },
                    }
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_VolvoShipment>
                    {
                        new() { Id = 3, ShipmentNumber = 2001 },
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock);

        await viewModel.LoadRecentShipmentsCommand.ExecuteAsync(null);
        await viewModel.LoadRecentShipmentsCommand.ExecuteAsync(null);

        viewModel.History.Should().ContainSingle();
        viewModel.History[0].Id.Should().Be(3);
        viewModel.History[0].ShipmentNumber.Should().Be(2001);
    }

    private static ViewModel_Volvo_History CreateViewModel(Mock<IMediator>? mediatorMock = null)
    {
        return new ViewModel_Volvo_History(
            (mediatorMock ?? new Mock<IMediator>()).Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

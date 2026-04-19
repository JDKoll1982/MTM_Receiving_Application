using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.ViewModels;

public sealed class ViewModel_Volvo_HistoryTests
{
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

        var viewModel = new ViewModel_Volvo_History(
            mediatorMock.Object,
            new Mock<IService_InforVisual>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        await viewModel.LoadRecentShipmentsCommand.ExecuteAsync(null);
        await viewModel.LoadRecentShipmentsCommand.ExecuteAsync(null);

        viewModel.History.Should().ContainSingle();
        viewModel.History[0].Id.Should().Be(3);
        viewModel.History[0].ShipmentNumber.Should().Be(2001);
    }
}

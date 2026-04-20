using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Handlers;

public sealed class DeleteShipmentHistoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteCompletedArchivedShipment_WhenAuthorized()
    {
        var historyDaoMock = new Mock<IDao_VolvoLabelHistory>();
        historyDaoMock
            .Setup(dao => dao.GetArchivedShipmentByIdAsync(42))
            .ReturnsAsync(
                new Model_Dao_Result<Model_VolvoShipment?>
                {
                    Success = true,
                    Data = new Model_VolvoShipment
                    {
                        Id = 42,
                        IsArchived = true,
                        Status = VolvoShipmentStatus.Completed,
                    },
                }
            );
        historyDaoMock
            .Setup(dao => dao.DeleteArchivedShipmentAsync(42))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var authServiceMock = new Mock<IService_VolvoAuthorization>();
        authServiceMock
            .Setup(service => service.CanManageShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var handler = new DeleteShipmentHistoryCommandHandler(
            historyDaoMock.Object,
            authServiceMock.Object
        );

        var result = await handler.Handle(
            new DeleteShipmentHistoryCommand { ShipmentId = 42 },
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        historyDaoMock.Verify(dao => dao.DeleteArchivedShipmentAsync(42), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectArchivedShipment_WhenStatusIsNotCompleted()
    {
        var historyDaoMock = new Mock<IDao_VolvoLabelHistory>();
        historyDaoMock
            .Setup(dao => dao.GetArchivedShipmentByIdAsync(9))
            .ReturnsAsync(
                new Model_Dao_Result<Model_VolvoShipment?>
                {
                    Success = true,
                    Data = new Model_VolvoShipment
                    {
                        Id = 9,
                        IsArchived = true,
                        Status = VolvoShipmentStatus.PendingPo,
                    },
                }
            );

        var authServiceMock = new Mock<IService_VolvoAuthorization>();
        authServiceMock
            .Setup(service => service.CanManageShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var handler = new DeleteShipmentHistoryCommandHandler(
            historyDaoMock.Object,
            authServiceMock.Object
        );

        var result = await handler.Handle(
            new DeleteShipmentHistoryCommand { ShipmentId = 9 },
            CancellationToken.None
        );

        result.IsSuccess.Should().BeFalse();
        result
            .ErrorMessage.Should()
            .Be("Only completed archived shipments can be deleted from history");
        historyDaoMock.Verify(dao => dao.DeleteArchivedShipmentAsync(It.IsAny<int>()), Times.Never);
    }
}

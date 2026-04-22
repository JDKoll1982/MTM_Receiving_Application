using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Handlers;

public sealed class ClearLabelDataCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMovedRowCount_WhenArchiveSucceeds()
    {
        var daoMock = new Mock<IDao_VolvoGeneratedLabelData>();
        daoMock
            .Setup(dao => dao.ClearToHistoryAsync("johnk"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(5));

        var authMock = new Mock<IService_VolvoAuthorization>();
        authMock
            .Setup(service => service.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var handler = new ClearLabelDataCommandHandler(daoMock.Object, authMock.Object);

        var result = await handler.Handle(
            new ClearLabelDataCommand { ArchivedBy = "johnk" },
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAuthorizationFails()
    {
        var daoMock = new Mock<IDao_VolvoGeneratedLabelData>();
        var authMock = new Mock<IService_VolvoAuthorization>();
        authMock
            .Setup(service => service.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Failure("Denied"));

        var handler = new ClearLabelDataCommandHandler(daoMock.Object, authMock.Object);

        var result = await handler.Handle(new ClearLabelDataCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        daoMock.Verify(dao => dao.ClearToHistoryAsync(It.IsAny<string>()), Times.Never);
    }
}

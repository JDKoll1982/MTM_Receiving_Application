using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Handlers;

public sealed class ClearLabelDataCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnMovedRowCount_WhenArchiveSucceeds()
    {
        var daoMock = new Mock<IDao_VolvoGeneratedLabelData>();
        daoMock
            .Setup(dao => dao.ClearToHistoryAsync("johnk", 6229, false))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(5));

        var authMock = new Mock<IService_VolvoAuthorization>();
        authMock
            .Setup(service => service.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var sessionManagerMock = new Mock<IService_UserSessionManager>();
        sessionManagerMock
            .SetupGet(service => service.CurrentSession)
            .Returns(
                new MTM_Receiving_Application.Module_Core.Models.Systems.Model_UserSession(
                    new MTM_Receiving_Application.Module_Core.Models.Systems.Model_User
                    {
                        EmployeeNumber = 6229,
                    }
                )
            );

        var userPrivilegesMock = new Mock<IService_UserPrivileges>();

        var handler = new ClearLabelDataCommandHandler(
            daoMock.Object,
            authMock.Object,
            sessionManagerMock.Object,
            userPrivilegesMock.Object
        );

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

        var handler = new ClearLabelDataCommandHandler(
            daoMock.Object,
            authMock.Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_UserPrivileges>().Object
        );

        var result = await handler.Handle(new ClearLabelDataCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        daoMock.Verify(
            dao => dao.ClearToHistoryAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFullClearRequestedByNonAdmin()
    {
        var daoMock = new Mock<IDao_VolvoGeneratedLabelData>();
        var authMock = new Mock<IService_VolvoAuthorization>();
        authMock
            .Setup(service => service.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var sessionManagerMock = new Mock<IService_UserSessionManager>();
        sessionManagerMock
            .SetupGet(service => service.CurrentSession)
            .Returns(
                new MTM_Receiving_Application.Module_Core.Models.Systems.Model_UserSession(
                    new MTM_Receiving_Application.Module_Core.Models.Systems.Model_User
                    {
                        EmployeeNumber = 6229,
                    }
                )
            );

        var userPrivilegesMock = new Mock<IService_UserPrivileges>();
        userPrivilegesMock.SetupGet(service => service.IsInitialized).Returns(true);
        userPrivilegesMock.SetupGet(service => service.CurrentUserId).Returns(6229);
        userPrivilegesMock
            .Setup(service => service.HasAnyRole("Admin", "Developer"))
            .Returns(false);

        var handler = new ClearLabelDataCommandHandler(
            daoMock.Object,
            authMock.Object,
            sessionManagerMock.Object,
            userPrivilegesMock.Object
        );

        var result = await handler.Handle(
            new ClearLabelDataCommand { ClearAllRows = true },
            CancellationToken.None
        );

        result.IsSuccess.Should().BeFalse();
        result
            .ErrorMessage.Should()
            .Be("Only Admin or Developer users can clear all Volvo generated label rows.");
        daoMock.Verify(
            dao => dao.ClearToHistoryAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()),
            Times.Never
        );
    }
}

using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Command_CustomerPullPackUpdateStatusHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAssignOwner_WhenStatusChangesToAccepted()
    {
        var existingEntry = CreateEntry();
        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>("Server=localhost;");
        daoMock
            .Setup(dao => dao.GetByIdAsync("WL-1"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(existingEntry));
        daoMock
            .Setup(dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()))
            .ReturnsAsync(
                (Model_CustomerPullPack_WaitlistEntry entry) =>
                    Model_Dao_Result_Factory.Success(entry)
            );

        var handler = new Command_CustomerPullPackUpdateStatusHandler(daoMock.Object);

        var result = await handler.Handle(
            new Command_CustomerPullPackUpdateStatus(
                "WL-1",
                Enum_CustomerPullPackWaitlistStatus.Accepted,
                "handler1",
                "Handler One"
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data!.CurrentOwnerUserId.Should().Be("handler1");
        result.Data.CurrentStatus.Should().Be(Enum_CustomerPullPackWaitlistStatus.Accepted);
    }

    [Fact]
    public async Task Handle_ShouldRetainOwner_WhenStatusChangesToProblem()
    {
        var existingEntry = CreateEntry();
        existingEntry.CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Accepted;
        existingEntry.CurrentOwnerUserId = "handler1";
        existingEntry.CurrentOwnerDisplayName = "Handler One";

        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>("Server=localhost;");
        daoMock
            .Setup(dao => dao.GetByIdAsync("WL-1"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(existingEntry));
        daoMock
            .Setup(dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()))
            .ReturnsAsync(
                (Model_CustomerPullPack_WaitlistEntry entry) =>
                    Model_Dao_Result_Factory.Success(entry)
            );

        var handler = new Command_CustomerPullPackUpdateStatusHandler(daoMock.Object);

        var result = await handler.Handle(
            new Command_CustomerPullPackUpdateStatus(
                "WL-1",
                Enum_CustomerPullPackWaitlistStatus.Problem,
                "handler1",
                "Handler One",
                Enum_CustomerPullPackProblemReason.IncorrectQty,
                "Wrong quantity in location"
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data!.CurrentOwnerUserId.Should().Be("handler1");
        result.Data.ProblemReason.Should().Be(Enum_CustomerPullPackProblemReason.IncorrectQty);
    }

    [Fact]
    public async Task Handle_ShouldRejectProblemWithoutReasonOrNote()
    {
        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>("Server=localhost;");
        daoMock
            .Setup(dao => dao.GetByIdAsync("WL-1"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(CreateEntry()));

        var handler = new Command_CustomerPullPackUpdateStatusHandler(daoMock.Object);

        var result = await handler.Handle(
            new Command_CustomerPullPackUpdateStatus(
                "WL-1",
                Enum_CustomerPullPackWaitlistStatus.Problem,
                "handler1",
                "Handler One"
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Problem status requires");
    }

    private static Model_CustomerPullPack_WaitlistEntry CreateEntry()
    {
        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = "WL-1",
            SourceLineKey = "LINE-1",
            CustomerId = "VOLVO",
            CustomerName = "Volvo Group",
            CustomerOrderId = "CO-1001",
            ParentPartId = "PART-100",
            RequestedQuantity = 10,
            RequestedByUserId = "jkoll",
            RequestedByDisplayName = "John Koll",
            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested,
        };
    }
}

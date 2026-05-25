using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Command_CustomerPullPackBatchUpsertHandlerTests
{
    [Fact]
    public async void Handle_ShouldUpsertEveryEntry_WhenBatchIsValid()
    {
        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>(CreateConnectionString());
        daoMock
            .SetupSequence(dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_WaitlistEntry { WaitlistId = "WL-1001" }
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_WaitlistEntry { WaitlistId = "WL-1002" }
                )
            );

        var handler = new Command_CustomerPullPackBatchUpsertHandler(daoMock.Object);

        var result = await handler.Handle(
            new Command_CustomerPullPackBatchUpsert([CreateEntry("LINE-1"), CreateEntry("LINE-2")]),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        daoMock.Verify(
            dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async void Handle_ShouldFlagLocationReview_WhenNoLocationsWereSelected()
    {
        Model_CustomerPullPack_WaitlistEntry? capturedEntry = null;
        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>(CreateConnectionString());
        daoMock
            .Setup(dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()))
            .Callback<Model_CustomerPullPack_WaitlistEntry>(entry => capturedEntry = entry)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_CustomerPullPack_WaitlistEntry { WaitlistId = "WL-2001" }
                )
            );

        var handler = new Command_CustomerPullPackBatchUpsertHandler(daoMock.Object);
        var entry = CreateEntry("LINE-3");
        entry.SelectedLocations.Clear();
        entry.RequesterContextNote = "No location available on the report.";

        await handler.Handle(
            new Command_CustomerPullPackBatchUpsert([entry]),
            CancellationToken.None
        );

        capturedEntry.Should().NotBeNull();
        capturedEntry!.LocationReviewFlag.Should().BeTrue();
        capturedEntry.SelectedLocations.Should().BeEmpty();
    }

    [Fact]
    public async void Handle_ShouldReturnDuplicateConflict_WhenDaoSignalsExistingOpenWaitlist()
    {
        var duplicateEntry = CreateEntry("LINE-4");
        duplicateEntry.WaitlistId = "WL-EXISTING";

        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>(CreateConnectionString());
        daoMock
            .Setup(dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()))
            .ReturnsAsync(
                new Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
                {
                    Success = false,
                    Data = duplicateEntry,
                    ErrorMessage = "An open waitlist item already exists for this report line.",
                    Severity = Enum_ErrorSeverity.Warning,
                    ReturnValue = "WL-EXISTING",
                }
            );

        var handler = new Command_CustomerPullPackBatchUpsertHandler(daoMock.Object);

        var result = await handler.Handle(
            new Command_CustomerPullPackBatchUpsert([CreateEntry("LINE-4")]),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeFalse();
        result
            .ErrorMessage.Should()
            .Be("An open waitlist item already exists for this report line.");
        result.ReturnValue.Should().Be("WL-EXISTING");
        result.Data.Should().ContainSingle();
        result.Data![0].WaitlistId.Should().Be("WL-EXISTING");
    }

    [Fact]
    public async void Handle_ShouldPreserveOwnerAndHandlerFields_WhenExistingItemWasAlreadyAccepted()
    {
        Model_CustomerPullPack_WaitlistEntry? capturedEntry = null;
        var existingAcceptedEntry = CreateEntry("LINE-5");
        existingAcceptedEntry.WaitlistId = "WL-ACCEPTED";
        existingAcceptedEntry.CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Accepted;
        existingAcceptedEntry.CurrentOwnerUserId = "handler1";
        existingAcceptedEntry.CurrentOwnerDisplayName = "Handler One";
        existingAcceptedEntry.HandlerNote = "Handle with forklift.";
        existingAcceptedEntry.SelectedLocations = ["SUB-LOCKED"];
        existingAcceptedEntry.RequestedQuantity = 11;

        var daoMock = new Mock<Dao_CustomerPullPackWaitlist>(CreateConnectionString());
        daoMock
            .Setup(dao => dao.GetByIdAsync("WL-ACCEPTED"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(existingAcceptedEntry));
        daoMock
            .Setup(dao => dao.UpsertAsync(It.IsAny<Model_CustomerPullPack_WaitlistEntry>()))
            .Callback<Model_CustomerPullPack_WaitlistEntry>(entry => capturedEntry = entry)
            .ReturnsAsync(Model_Dao_Result_Factory.Success(existingAcceptedEntry));

        var handler = new Command_CustomerPullPackBatchUpsertHandler(daoMock.Object);
        var changedEntry = CreateEntry("LINE-5");
        changedEntry.WaitlistId = "WL-ACCEPTED";
        changedEntry.CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested;
        changedEntry.CurrentOwnerUserId = string.Empty;
        changedEntry.CurrentOwnerDisplayName = string.Empty;
        changedEntry.HandlerNote = string.Empty;
        changedEntry.SelectedLocations = ["SUB-NEW"];
        changedEntry.RequestedQuantity = 99;
        changedEntry.RequesterContextNote = "Updated requester note.";

        var result = await handler.Handle(
            new Command_CustomerPullPackBatchUpsert([changedEntry]),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        capturedEntry.Should().NotBeNull();
        capturedEntry!.CurrentStatus.Should().Be(Enum_CustomerPullPackWaitlistStatus.Accepted);
        capturedEntry.CurrentOwnerUserId.Should().Be("handler1");
        capturedEntry.CurrentOwnerDisplayName.Should().Be("Handler One");
        capturedEntry.HandlerNote.Should().Be("Handle with forklift.");
        capturedEntry.SelectedLocations.Should().Equal(["SUB-LOCKED"]);
        capturedEntry.RequestedQuantity.Should().Be(11);
        capturedEntry.RequesterContextNote.Should().Be("Updated requester note.");
    }

    private static Model_CustomerPullPack_WaitlistEntry CreateEntry(string sourceLineKey)
    {
        return new Model_CustomerPullPack_WaitlistEntry
        {
            SourceLineKey = sourceLineKey,
            CustomerId = "VOLVO",
            CustomerName = "Volvo Group",
            CustomerOrderId = "CO-1001",
            ParentPartId = "PART-100",
            RequestedQuantity = 24,
            SelectedLocations = ["SUB-01"],
            RequestedByUserId = "jkoll",
            RequestedByDisplayName = "John Koll",
            RequesterContextNote = "Pull before lunch.",
            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested,
            LastUpdatedByUserId = "jkoll",
        };
    }

    private static string CreateConnectionString()
    {
        return "Server=MYSQL;Database=mtm_receiving_application;Uid=root;Pwd=test;";
    }
}

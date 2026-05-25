using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Tests.Integration.Module_ShipRec_Tools;

public sealed class Dao_CustomerPullPackWaitlistIntegrationTests
{
    [Fact]
    public async Task UpsertAsync_ThenGetByIdAsync_ShouldRoundTripWaitlistEntry_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dao = new Dao_CustomerPullPackWaitlist(connectionString);
        var testSuffix = Guid.NewGuid().ToString("N");
        var entry = CreateEntry(testSuffix);

        try
        {
            var upsertResult = await dao.UpsertAsync(entry);

            upsertResult.IsSuccess.Should().BeTrue();
            upsertResult.Data.Should().NotBeNull();
            upsertResult.Data!.WaitlistId.Should().NotBeNullOrWhiteSpace();
            entry.WaitlistId = upsertResult.Data.WaitlistId;

            var savedId = upsertResult.Data.WaitlistId;
            var loadResult = await dao.GetByIdAsync(savedId);

            loadResult.IsSuccess.Should().BeTrue();
            loadResult.Data.Should().NotBeNull();
            loadResult.Data!.WaitlistId.Should().Be(savedId);
            loadResult.Data.SourceLineKey.Should().Be(entry.SourceLineKey);
            loadResult.Data.RequesterContextNote.Should().Be(entry.RequesterContextNote);
            loadResult.Data.SelectedLocations.Should().BeEquivalentTo(entry.SelectedLocations);
        }
        finally
        {
            await CompleteIfCreatedAsync(dao, entry);
        }
    }

    [Fact]
    public async Task UpsertAsync_ShouldReturnDuplicateConflict_WhenOpenEntryAlreadyExistsAndIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dao = new Dao_CustomerPullPackWaitlist(connectionString);
        var testSuffix = Guid.NewGuid().ToString("N");
        var originalEntry = CreateEntry(testSuffix);
        var duplicateEntry = CreateEntry(testSuffix);
        duplicateEntry.RequesterContextNote = "Duplicate attempt.";
        duplicateEntry.LastUpdatedByUserId = "dup-user";

        try
        {
            var originalResult = await dao.UpsertAsync(originalEntry);
            originalResult.IsSuccess.Should().BeTrue();
            originalResult.Data.Should().NotBeNull();
            originalEntry.WaitlistId = originalResult.Data!.WaitlistId;

            var duplicateResult = await dao.UpsertAsync(duplicateEntry);

            duplicateResult.IsSuccess.Should().BeFalse();
            duplicateResult.ReturnValue.Should().NotBeNull();
            duplicateResult.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            duplicateResult.Data.Should().NotBeNull();
            duplicateResult.Data!.WaitlistId.Should().Be(originalResult.Data!.WaitlistId);
        }
        finally
        {
            await CompleteIfCreatedAsync(dao, originalEntry);
        }
    }

    private static string? TryGetIntegrationConnectionString()
    {
        return Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("MTM_MYSQL_CONNECTION_STRING");
    }

    private static Model_CustomerPullPack_WaitlistEntry CreateEntry(string testSuffix)
    {
        return new Model_CustomerPullPack_WaitlistEntry
        {
            SourceLineKey = $"TEST-CPP-LINE-{testSuffix}",
            CustomerId = "TEST-CUSTOMER",
            CustomerName = "Test Customer",
            CustomerOrderId = $"TEST-ORDER-{testSuffix}",
            ParentPartId = $"TEST-PART-{testSuffix}",
            RequestedQuantity = 5,
            SelectedLocations = [$"TEST-LOC-{testSuffix}"],
            RequestedByUserId = "test.user",
            RequestedByDisplayName = "Test User",
            RequesterContextNote = "Integration test waitlist entry.",
            CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Requested,
            LastUpdatedByUserId = "test.user",
        };
    }

    private static async Task CompleteIfCreatedAsync(
        Dao_CustomerPullPackWaitlist dao,
        Model_CustomerPullPack_WaitlistEntry entry
    )
    {
        if (string.IsNullOrWhiteSpace(entry.WaitlistId))
        {
            return;
        }

        entry.CurrentStatus = Enum_CustomerPullPackWaitlistStatus.Completed;
        entry.CompletionUserId = "test.user";
        entry.CompletionTimestamp = DateTime.UtcNow;
        entry.LastUpdatedByUserId = "test.user";
        await dao.UpsertAsync(entry);
    }
}

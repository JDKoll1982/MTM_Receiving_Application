using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Data;

public sealed class Dao_ScannerBatchSessionTests
{
    [Fact]
    public async Task UpsertSessionAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var dao = CreateDao();

        var result = await dao.UpsertSessionAsync(new Model_ScannerBatchSession
        {
            SessionId = Guid.NewGuid(),
            OwnerUserId = string.Empty,
            SessionName = "Night Shift",
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Owner user id is required.");
    }

    [Fact]
    public void MapSession_ShouldMapPersistedFields()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(string));
        table.Columns.Add("user_id", typeof(string));
        table.Columns.Add("profile_id", typeof(string));
        table.Columns.Add("session_name", typeof(string));
        table.Columns.Add("status", typeof(string));
        table.Columns.Add("stop_requested", typeof(bool));
        table.Columns.Add("sent_count", typeof(int));
        table.Columns.Add("failed_count", typeof(int));
        table.Columns.Add("waiting_count", typeof(int));
        table.Columns.Add("last_message", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));
        table.Columns.Add("updated_at", typeof(DateTime));

        var sessionId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        table.Rows.Add(
            sessionId.ToString(),
            "user-1",
            profileId.ToString(),
            "Night Shift",
            "Running",
            true,
            3,
            1,
            2,
            "Validation warning",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow
        );

        using var reader = table.CreateDataReader();
        reader.Read();

        var mapMethod = typeof(Dao_ScannerBatchSession).GetMethod(
            "MapSession",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        mapMethod.Should().NotBeNull();
        var mapped = mapMethod!.Invoke(null, [reader]);
        var model = mapped.Should().BeOfType<Model_ScannerBatchSession>().Subject;

        model.SessionId.Should().Be(sessionId);
        model.ActiveProfileId.Should().Be(profileId);
        model.OwnerUserId.Should().Be("user-1");
        model.SessionName.Should().Be("Night Shift");
        model.Status.Should().Be(Enum_ScannerSessionStatus.Running);
        model.StopRequested.Should().BeTrue();
        model.SentItems.Should().Be(3);
        model.FailedItems.Should().Be(1);
        model.WaitingItems.Should().Be(2);
    }

    private static Dao_ScannerBatchSession CreateDao()
    {
        return new Dao_ScannerBatchSession("Server=localhost;Database=test;Uid=test;Pwd=test;");
    }
}

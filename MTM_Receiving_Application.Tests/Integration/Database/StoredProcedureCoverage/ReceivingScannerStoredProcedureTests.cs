using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the scanner procedures end to end: a profile is created, made the default, then the
/// session that uses it is saved with one item and finally cleared.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class ReceivingScannerStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private string _profileId = string.Empty;
    private string _sessionId = string.Empty;
    private string _userId = string.Empty;

    public ReceivingScannerStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        _userId = UniqueToken("TEST-SCAN-USER-");
        _profileId = Guid.NewGuid().ToString();
        _sessionId = Guid.NewGuid().ToString();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Database.ConnectionString is null)
        {
            return;
        }

        await ExecuteSqlAsync(
            "DELETE FROM receiving_scanner_item WHERE session_id = @sessionId;",
            new MySqlParameter("@sessionId", _sessionId));
        await ExecuteSqlAsync(
            "DELETE FROM receiving_scanner_session WHERE user_id = @userId;",
            new MySqlParameter("@userId", _userId));
        await ExecuteSqlAsync(
            "DELETE FROM receiving_scanner_profile WHERE user_id = @userId;",
            new MySqlParameter("@userId", _userId));
    }

    [SkippableFact]
    public async Task ScannerProfile_ShouldUpsertPromoteAndDelete()
    {
        SkipWhenUnavailable();

        await UpsertProfileAsync("TEST-Profile-A", isDefault: 0);

        var first = await QueryAsync(
            "sp_Receiving_ScannerProfile_GetByUser",
            Parameter("p_user_id", _userId));
        var firstRow = first.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        firstRow["profile_name"].ToString().Should().Be("TEST-Profile-A");
        ToInt(firstRow["is_default"]).Should().Be(0);
        firstRow["app_window_title"].ToString().Should().Be("TEST-Window");

        await UpsertProfileAsync("TEST-Profile-A", isDefault: 1);

        var promoted = await QueryAsync(
            "sp_Receiving_ScannerProfile_GetByUser",
            Parameter("p_user_id", _userId));
        ToInt(promoted.Rows[0]["is_default"]).Should().Be(1);

        await ExecuteProcedureAsync(
            "sp_Receiving_ScannerProfile_SetDefault",
            Parameter("p_id", _profileId),
            Parameter("p_user_id", _userId));

        await ExecuteProcedureAsync(
            "sp_Receiving_ScannerProfile_Delete",
            Parameter("p_id", _profileId),
            Parameter("p_user_id", _userId));

        var afterDelete = await QueryAsync(
            "sp_Receiving_ScannerProfile_GetByUser",
            Parameter("p_user_id", _userId));
        afterDelete.Rows.Count.Should().Be(0);
    }

    [SkippableFact]
    public async Task ScannerSessionAndItems_ShouldPersistThenClearTheList()
    {
        SkipWhenUnavailable();

        await UpsertProfileAsync("TEST-Profile-B", isDefault: 1);

        await ExecuteProcedureAsync(
            "sp_Receiving_ScannerSession_Upsert",
            Parameter("p_id", _sessionId),
            Parameter("p_user_id", _userId),
            Parameter("p_profile_id", _profileId),
            Parameter("p_session_name", "TEST-Session"),
            Parameter("p_status", "Ready"),
            Parameter("p_stop_requested", 0),
            Parameter("p_last_message", "TEST-MESSAGE"));

        var sessions = await QueryAsync(
            "sp_Receiving_ScannerSession_GetByUser",
            Parameter("p_user_id", _userId));
        var session = sessions.Rows.Cast<System.Data.DataRow>()
            .Should().ContainSingle(candidate => candidate["id"].ToString() == _sessionId).Subject;
        session["session_name"].ToString().Should().Be("TEST-Session");

        await UpsertItemAsync(itemOrder: 1, status: "Waiting");
        await UpsertItemAsync(itemOrder: 2, status: "Sent");

        var items = await QueryAsync(
            "sp_Receiving_ScannerItem_GetBySession",
            Parameter("p_session_id", _sessionId));
        var itemRows = items.Rows.Cast<System.Data.DataRow>().ToList();
        itemRows.Should().HaveCount(2);
        itemRows.Select(row => ToInt(row["item_order"])).Should().Equal(1, 2);
        itemRows[1]["status"].ToString().Should().Be("Sent");

        await ExecuteProcedureByPositionAsync(
            "sp_Receiving_ScannerItem_Upsert",
            Parameter("p_session_id", _sessionId),
            Parameter("p_item_order", 2),
            Parameter("p_part_id", "TEST-PART-SCAN"),
            Parameter("p_from_warehouse_id", "002"),
            Parameter("p_from_location_id", "TEST-FROM"),
            Parameter("p_to_warehouse_id", "002"),
            Parameter("p_to_location_id", "TEST-TO"),
            Parameter("p_quantity", 3m),
            Parameter("p_payload_json", null),
            Parameter("p_validation_state", "Valid"),
            Parameter("p_validation_notes", "TEST-NOTES"),
            Parameter("p_status", "Failed"),
            Parameter("p_failure_code", "E-TEST"),
            Parameter("p_failure_message", "TEST-FAILURE"));

        var updated = await QueryAsync(
            "sp_Receiving_ScannerItem_GetBySession",
            Parameter("p_session_id", _sessionId));
        var updatedRow = updated.Rows.Cast<System.Data.DataRow>()
            .Should().ContainSingle(candidate => ToInt(candidate["item_order"]) == 2).Subject;
        updatedRow["status"].ToString().Should().Be("Failed");
        updatedRow["failure_code"].ToString().Should().Be("E-TEST");

        await ExecuteProcedureAsync(
            "sp_Receiving_ScannerItem_DeleteBySession",
            Parameter("p_session_id", _sessionId));

        var cleared = await QueryAsync(
            "sp_Receiving_ScannerItem_GetBySession",
            Parameter("p_session_id", _sessionId));
        cleared.Rows.Count.Should().Be(0);
    }

    private async Task UpsertProfileAsync(string profileName, int isDefault)
    {
        await ExecuteProcedureAsync(
            "sp_Receiving_ScannerProfile_Upsert",
            Parameter("p_id", _profileId),
            Parameter("p_user_id", _userId),
            Parameter("p_profile_name", profileName),
            Parameter("p_is_default", isDefault),
            Parameter("p_target_executable_name", "VMINVENT.exe"),
            Parameter("p_app_window_title", "TEST-Window"),
            Parameter("p_target_child_window_title", "Inventory Transfers"),
            Parameter("p_app_window_class", null),
            Parameter("p_require_exact_title_match", 0),
            Parameter("p_from_warehouse_default", "002"),
            Parameter("p_to_warehouse_default", "002"),
            Parameter("p_activation_delay_ms", 250),
            Parameter("p_delay_between_fields_ms", 100),
            Parameter("p_pause_after_item_ms", 200),
            Parameter("p_popup_timeout_ms", 3000),
            Parameter("p_popup_close_timeout_ms", 1200),
            Parameter("p_send_shortcut_chord", "Ctrl+Alt+M"),
            Parameter("p_stop_shortcut_chord", "Ctrl+Alt+N"),
            Parameter("p_allow_advanced_timing", 0));
    }

    private async Task UpsertItemAsync(int itemOrder, string status)
    {
        await ExecuteProcedureByPositionAsync(
            "sp_Receiving_ScannerItem_Upsert",
            Parameter("p_session_id", _sessionId),
            Parameter("p_item_order", itemOrder),
            Parameter("p_part_id", "TEST-PART-SCAN"),
            Parameter("p_from_warehouse_id", "002"),
            Parameter("p_from_location_id", "TEST-FROM"),
            Parameter("p_to_warehouse_id", "002"),
            Parameter("p_to_location_id", "TEST-TO"),
            Parameter("p_quantity", 1m),
            Parameter("p_payload_json", null),
            Parameter("p_validation_state", "NotValidated"),
            Parameter("p_validation_notes", null),
            Parameter("p_status", status),
            Parameter("p_failure_code", null),
            Parameter("p_failure_message", null));
    }
}

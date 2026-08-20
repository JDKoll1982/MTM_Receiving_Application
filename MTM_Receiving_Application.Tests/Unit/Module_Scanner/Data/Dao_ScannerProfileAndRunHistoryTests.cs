using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Data;

public sealed class Dao_ScannerProfileAndRunHistoryTests
{
    [Fact]
    public async Task UpsertProfileAsync_ShouldFail_WhenAppWindowTitleMissing()
    {
        var dao = new Dao_ScannerProfile("Server=172.16.1.104;Database=test;Uid=test;Pwd=test;");

        var result = await dao.UpsertProfileAsync(new Model_ScannerProfile
        {
            ProfileId = Guid.NewGuid(),
            OwnerUserId = "user-1",
            ProfileName = "Default",
            AppWindowTitle = string.Empty,
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("App window title is required.");
    }

    [Fact]
    public async Task InsertRunItemAsync_ShouldFail_WhenSessionItemIdMissing()
    {
        var dao = new Dao_ScannerRunHistory("Server=172.16.1.104;Database=test;Uid=test;Pwd=test;");

        var result = await dao.InsertRunItemAsync(new Model_ScannerRunItem
        {
            RunId = Guid.NewGuid(),
            SessionItemId = null,
            SequenceNumber = 1,
            PartId = "MMC0000850",
            FromWarehouse = "002",
            ToWarehouse = "002",
            Quantity = "1",
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Session item id is required.");
    }

    [Fact]
    public void MapProfile_ShouldMapTargetAndWarehouseDefaults()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(string));
        table.Columns.Add("user_id", typeof(string));
        table.Columns.Add("profile_name", typeof(string));
        table.Columns.Add("is_default", typeof(bool));
        table.Columns.Add("target_executable_name", typeof(string));
        table.Columns.Add("app_window_title", typeof(string));
        table.Columns.Add("target_child_window_title", typeof(string));
        table.Columns.Add("app_window_class", typeof(string));
        table.Columns.Add("require_exact_title_match", typeof(bool));
        table.Columns.Add("from_warehouse_default", typeof(string));
        table.Columns.Add("to_warehouse_default", typeof(string));
        table.Columns.Add("activation_delay_ms", typeof(int));
        table.Columns.Add("delay_between_fields_ms", typeof(int));
        table.Columns.Add("pause_after_item_ms", typeof(int));
        table.Columns.Add("popup_timeout_ms", typeof(int));
        table.Columns.Add("popup_close_timeout_ms", typeof(int));
        table.Columns.Add("send_shortcut_chord", typeof(string));
        table.Columns.Add("stop_shortcut_chord", typeof(string));
        table.Columns.Add("allow_advanced_timing", typeof(bool));
        table.Columns.Add("created_at", typeof(DateTime));
        table.Columns.Add("updated_at", typeof(DateTime));

        var profileId = Guid.NewGuid();
        table.Rows.Add(
            profileId.ToString(),
            "user-1",
            "Default",
            true,
            "VMINVENT.exe",
            "Inventory Transfers",
            "Inventory Transfers",
            "SomeClass",
            false,
            "002",
            "002",
            250,
            100,
            200,
            3000,
            1200,
            "Ctrl+Alt+M",
            "Ctrl+Alt+N",
            false,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        using var reader = table.CreateDataReader();
        reader.Read();

        var mapMethod = typeof(Dao_ScannerProfile).GetMethod(
            "MapProfile",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        mapMethod.Should().NotBeNull();
        var mapped = mapMethod!.Invoke(null, [reader]);
        var model = mapped.Should().BeOfType<Model_ScannerProfile>().Subject;

        model.ProfileId.Should().Be(profileId);
        model.TargetExecutableName.Should().Be("VMINVENT.exe");
        model.AppWindowTitle.Should().Be("Inventory Transfers");
        model.FromWarehouseDefault.Should().Be("002");
        model.ToWarehouseDefault.Should().Be("002");
    }
}

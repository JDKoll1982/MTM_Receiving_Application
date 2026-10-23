using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Data;

public sealed class Dao_ReceivingLoadTests
{
    [Fact]
    public void MapRowToLoad_ShouldPreserveHistoryRecordId_WhenGuidIsMissing()
    {
        var row = CreateRow(42, DBNull.Value);

        var result = InvokeMapRowToLoad(row);

        result.HistoryRecordID.Should().Be(42);
        result.LoadID.Should().Be(Guid.Empty);
    }

    [Fact]
    public void MapRowToLoad_ShouldPreserveBothPersistedIdentifiers_WhenGuidExists()
    {
        var expectedGuid = Guid.NewGuid();
        var row = CreateRow(7, expectedGuid.ToString());

        var result = InvokeMapRowToLoad(row);

        result.HistoryRecordID.Should().Be(7);
        result.LoadID.Should().Be(expectedGuid);
    }

    [Fact]
    public void MapRowToLoad_ShouldMapCreatedAt_WhenColumnIsPresent()
    {
        var savedAt = new DateTime(2026, 10, 23, 9, 14, 7);
        var row = CreateRow(11, Guid.NewGuid().ToString(), savedAt);

        var result = InvokeMapRowToLoad(row);

        result.CreatedAt.Should().Be(savedAt);
    }

    [Fact]
    public void MapRowToLoad_ShouldLeaveCreatedAtNull_WhenColumnIsMissing()
    {
        var row = CreateRow(12, DBNull.Value);

        var result = InvokeMapRowToLoad(row);

        result.CreatedAt.Should().BeNull();
    }

    private static Model_ReceivingLoad InvokeMapRowToLoad(DataRow row)
    {
        var dao = new Dao_ReceivingLoad("Server=172.16.1.104;Database=test;");
        var methodInfo = typeof(Dao_ReceivingLoad).GetMethod(
            "MapRowToLoad",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(dao, new object[] { row });
        return result.Should().BeOfType<Model_ReceivingLoad>().Subject;
    }

    private static DataRow CreateRow(int id, object loadGuid, DateTime? createdAt = null)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("load_guid", typeof(string));

        if (createdAt.HasValue)
        {
            table.Columns.Add("created_at", typeof(DateTime));
        }

        var row = table.NewRow();
        row["id"] = id;
        row["load_guid"] = loadGuid;
        if (createdAt.HasValue)
        {
            row["created_at"] = createdAt.Value;
        }
        table.Rows.Add(row);

        return row;
    }
}

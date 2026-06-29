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

    private static DataRow CreateRow(int id, object loadGuid)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("load_guid", typeof(string));

        var row = table.NewRow();
        row["id"] = id;
        row["load_guid"] = loadGuid;
        table.Rows.Add(row);

        return row;
    }
}

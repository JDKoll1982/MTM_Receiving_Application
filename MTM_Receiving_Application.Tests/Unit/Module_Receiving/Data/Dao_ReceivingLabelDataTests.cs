using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Data;

public sealed class Dao_ReceivingLabelDataTests
{
    [Fact]
    public void MapRowToLoad_ShouldPreserveLabelDataRecordId_WhenGuidIsMissing()
    {
        var row = CreateRow(15, DBNull.Value);

        var result = InvokeMapRowToLoad(row);

        result.LabelDataRecordID.Should().Be(15);
        result.LoadID.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void MapRowToLoad_ShouldPreserveBothQueueIdentifiers_WhenGuidExists()
    {
        var expectedGuid = Guid.NewGuid();
        var row = CreateRow(27, expectedGuid.ToString());

        var result = InvokeMapRowToLoad(row);

        result.LabelDataRecordID.Should().Be(27);
        result.LoadID.Should().Be(expectedGuid);
    }

    private static Model_ReceivingLoad InvokeMapRowToLoad(DataRow row)
    {
        var dao = new Dao_ReceivingLabelData("Server=172.16.1.104;Database=test;");
        var methodInfo = typeof(Dao_ReceivingLabelData).GetMethod(
            "MapRowToLoad",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(null, new object[] { row });
        return result.Should().BeOfType<Model_ReceivingLoad>().Subject;
    }

    private static DataRow CreateRow(int id, object loadGuid)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("load_id", typeof(string));

        var row = table.NewRow();
        row["id"] = id;
        row["load_id"] = loadGuid;
        table.Rows.Add(row);

        return row;
    }
}

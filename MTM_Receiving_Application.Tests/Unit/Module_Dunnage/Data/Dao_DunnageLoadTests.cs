using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Data;

public sealed class Dao_DunnageLoadTests
{
    [Fact]
    public void MapFromReader_ShouldPreservePartSkidFieldsFromHistory()
    {
        using var reader = CreateHistoryReader();
        reader.Read().Should().BeTrue();

        var dao = new Dao_DunnageLoad("Server=172.16.1.104;Database=test;");
        var methodInfo = typeof(Dao_DunnageLoad).GetMethod(
            "MapFromReader",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(dao, new object[] { reader });
        var load = result.Should().BeOfType<Model_DunnageLoad>().Subject;

        load.PartSkidSequence.Should().Be(4);
        load.PartSkidTotal.Should().Be(7);
        load.LabelNumber.Should().Be("LBL-44");
        load.CreatedBy.Should().Be("archiver");
        load.Udc1.Should().Be("Blue");
    }

    private static IDataReader CreateHistoryReader()
    {
        var table = new DataTable();
        table.Columns.Add("load_uuid", typeof(Guid));
        table.Columns.Add("part_id", typeof(string));
        table.Columns.Add("type_id", typeof(int));
        table.Columns.Add("type_name", typeof(string));
        table.Columns.Add("type_icon", typeof(string));
        table.Columns.Add("quantity", typeof(decimal));
        table.Columns.Add("po_number", typeof(string));
        table.Columns.Add("received_date", typeof(DateTime));
        table.Columns.Add("created_by", typeof(string));
        table.Columns.Add("created_date", typeof(DateTime));
        table.Columns.Add("modified_by", typeof(string));
        table.Columns.Add("modified_date", typeof(DateTime));
        table.Columns.Add("location", typeof(string));
        table.Columns.Add("label_number", typeof(string));
        table.Columns.Add("part_skid_sequence", typeof(int));
        table.Columns.Add("part_skid_total", typeof(int));
        table.Columns.Add("udc1", typeof(string));
        table.Columns.Add("udc2", typeof(string));
        table.Columns.Add("udc3", typeof(string));
        table.Columns.Add("udc4", typeof(string));
        table.Columns.Add("udc5", typeof(string));
        table.Columns.Add("udc6", typeof(string));
        table.Columns.Add("udc7", typeof(string));
        table.Columns.Add("udc8", typeof(string));
        table.Columns.Add("udc9", typeof(string));
        table.Columns.Add("udc10", typeof(string));

        table.Rows.Add(
            Guid.Parse("00000000-0000-0000-0000-000000000044"),
            "PART-44",
            3,
            "Plastic",
            "Help",
            6.25m,
            "PO-44",
            new DateTime(2026, 4, 1, 8, 0, 0),
            "archiver",
            new DateTime(2026, 4, 1, 8, 5, 0),
            DBNull.Value,
            DBNull.Value,
            "RECV",
            "LBL-44",
            4,
            7,
            "Blue",
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value,
            DBNull.Value
        );

        return table.CreateDataReader();
    }
}

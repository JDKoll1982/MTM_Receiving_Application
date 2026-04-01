using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Data;

public sealed class Dao_DunnageLabelDataTests
{
    [Fact]
    public void OrderLoadsAndAssignPartSkidCounters_ShouldAssignSequenceAndTotalsPerPart()
    {
        var firstA = new Model_DunnageLoad
        {
            PartId = "A100",
            LoadNumber = 2,
            LoadUuid = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        };
        var secondA = new Model_DunnageLoad
        {
            PartId = "a100",
            LoadNumber = 1,
            LoadUuid = Guid.Parse("00000000-0000-0000-0000-000000000003"),
        };
        var onlyB = new Model_DunnageLoad
        {
            PartId = "B200",
            LoadNumber = 1,
            LoadUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        };

        var orderedLoads = InvokeOrderLoadsAndAssignPartSkidCounters([firstA, secondA, onlyB]);

        orderedLoads.Select(load => load.PartId).Should().Equal("a100", "A100", "B200");
        secondA.PartSkidSequence.Should().Be(1);
        secondA.PartSkidTotal.Should().Be(2);
        firstA.PartSkidSequence.Should().Be(2);
        firstA.PartSkidTotal.Should().Be(2);
        onlyB.PartSkidSequence.Should().Be(1);
        onlyB.PartSkidTotal.Should().Be(1);
    }

    [Fact]
    public void MapFromReader_ShouldPreservePartSkidFields()
    {
        using var reader = CreateActiveLabelReader();
        reader.Read().Should().BeTrue();

        var methodInfo = typeof(Dao_DunnageLabelData).GetMethod(
            "MapFromReader",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(null, new object[] { reader });
        var load = result.Should().BeOfType<Model_DunnageLoad>().Subject;

        load.QueueRowId.Should().Be(9);
        load.PartSkidSequence.Should().Be(2);
        load.PartSkidTotal.Should().Be(5);
        load.LabelNumber.Should().Be("LBL-9");
    }

    private static List<Model_DunnageLoad> InvokeOrderLoadsAndAssignPartSkidCounters(
        List<Model_DunnageLoad> loads
    )
    {
        var methodInfo = typeof(Dao_DunnageLabelData).GetMethod(
            "OrderLoadsAndAssignPartSkidCounters",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(null, new object[] { loads });
        return result.Should().BeOfType<List<Model_DunnageLoad>>().Subject;
    }

    private static IDataReader CreateActiveLabelReader()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("load_uuid", typeof(Guid));
        table.Columns.Add("part_id", typeof(string));
        table.Columns.Add("dunnage_type_id", typeof(int));
        table.Columns.Add("dunnage_type_name", typeof(string));
        table.Columns.Add("dunnage_type_icon", typeof(string));
        table.Columns.Add("quantity", typeof(decimal));
        table.Columns.Add("po_number", typeof(string));
        table.Columns.Add("received_date", typeof(DateTime));
        table.Columns.Add("user_id", typeof(string));
        table.Columns.Add("location", typeof(string));
        table.Columns.Add("label_number", typeof(string));
        table.Columns.Add("part_skid_sequence", typeof(int));
        table.Columns.Add("part_skid_total", typeof(int));
        table.Columns.Add("specs_json", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));

        table.Rows.Add(
            9,
            Guid.Parse("00000000-0000-0000-0000-000000000009"),
            "PART-9",
            4,
            "Corrugated",
            "PackageVariantClosed",
            10.5m,
            "PO-9",
            new DateTime(2026, 4, 1, 12, 0, 0),
            "tester",
            "RECV",
            "LBL-9",
            2,
            5,
            "{\"Width\":\"10\"}",
            new DateTime(2026, 4, 1, 12, 5, 0)
        );

        return table.CreateDataReader();
    }
}
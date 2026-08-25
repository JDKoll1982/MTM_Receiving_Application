using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Data;

public sealed class Dao_DunnageCustomFieldTests
{
    [Fact]
    public void MapFromReader_ShouldPopulateFullCustomFieldDefinition()
    {
        using var reader = CreateReader();
        reader.Read().Should().BeTrue();

        var dao = new Dao_DunnageCustomField("Server=172.16.1.104;Database=test;");
        var methodInfo = typeof(Dao_DunnageCustomField).GetMethod(
            "MapFromReader",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(dao, new object[] { reader });
        var field = result.Should().BeOfType<Model_CustomFieldDefinition>().Subject;

        field.Id.Should().Be(7);
        field.DunnageTypeId.Should().Be(12);
        field.FieldName.Should().Be("Weight (lbs)");
        field.FieldType.Should().Be("Number");
        field.DisplayOrder.Should().Be(3);
        field.IsRequired.Should().BeTrue();
        field.Unit.Should().Be("lbs");
        field.MinValue.Should().Be(1m);
        field.MaxValue.Should().Be(50m);
        field.DefaultValue.Should().Be("10");
        field.ValidationRules.Should().Be("{\"min\":1}");
        field.CreatedDate.Should().Be(new DateTime(2026, 4, 1, 10, 30, 0));
        field.CreatedBy.Should().Be("tester");
    }

    private static IDataReader CreateReader()
    {
        var table = new DataTable();
        // Column names mirror the exact casing returned by sp_Dunnage_CustomFields_GetByType.
        table.Columns.Add("ID", typeof(int));
        table.Columns.Add("DunnageTypeID", typeof(int));
        table.Columns.Add("FieldName", typeof(string));
        table.Columns.Add("FieldType", typeof(string));
        table.Columns.Add("DisplayOrder", typeof(int));
        table.Columns.Add("IsRequired", typeof(bool));
        table.Columns.Add("Unit", typeof(string));
        table.Columns.Add("MinValue", typeof(decimal));
        table.Columns.Add("MaxValue", typeof(decimal));
        table.Columns.Add("DefaultValue", typeof(string));
        table.Columns.Add("ValidationRules", typeof(string));
        table.Columns.Add("CreatedDate", typeof(DateTime));
        table.Columns.Add("CreatedBy", typeof(string));

        table.Rows.Add(
            7,
            12,
            "Weight (lbs)",
            "Number",
            3,
            true,
            "lbs",
            1m,
            50m,
            "10",
            "{\"min\":1}",
            new DateTime(2026, 4, 1, 10, 30, 0),
            "tester"
        );

        return table.CreateDataReader();
    }
}

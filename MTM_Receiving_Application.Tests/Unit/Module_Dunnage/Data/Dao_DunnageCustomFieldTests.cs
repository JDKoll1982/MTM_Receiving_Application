using System;
using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Data;

public sealed class Dao_DunnageCustomFieldTests
{
    [Fact]
    public void MapFromReader_ShouldPopulateFullCustomFieldDefinition()
    {
        using var reader = CreateReader();
        reader.Read().Should().BeTrue();

        var dao = new Dao_DunnageCustomField("Server=localhost;Database=test;");
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
        field.DatabaseColumnName.Should().Be("weight_lbs");
        field.FieldType.Should().Be("Number");
        field.DisplayOrder.Should().Be(3);
        field.IsRequired.Should().BeTrue();
        field.ValidationRules.Should().Be("{\"min\":1}");
        field.CreatedDate.Should().Be(new DateTime(2026, 4, 1, 10, 30, 0));
        field.CreatedBy.Should().Be("tester");
    }

    private static IDataReader CreateReader()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("dunnagetypeid", typeof(int));
        table.Columns.Add("field_name", typeof(string));
        table.Columns.Add("databasecolumnname", typeof(string));
        table.Columns.Add("field_type", typeof(string));
        table.Columns.Add("display_order", typeof(int));
        table.Columns.Add("is_required", typeof(bool));
        table.Columns.Add("validationrules", typeof(string));
        table.Columns.Add("createddate", typeof(DateTime));
        table.Columns.Add("createdby", typeof(string));

        table.Rows.Add(
            7,
            12,
            "Weight (lbs)",
            "weight_lbs",
            "Number",
            3,
            true,
            "{\"min\":1}",
            new DateTime(2026, 4, 1, 10, 30, 0),
            "tester"
        );

        return table.CreateDataReader();
    }
}

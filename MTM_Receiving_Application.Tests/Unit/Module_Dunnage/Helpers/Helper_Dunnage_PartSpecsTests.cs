using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Helpers;

public sealed class Helper_Dunnage_PartSpecsTests
{
    [Fact]
    public void ExtractUdc_ShouldReturnAllTenSlots_FromPart()
    {
        var part = new Model_DunnagePart();
        part.SetUdcValue(1, "Alpha");
        part.SetUdcValue(10, "Omega");

        var values = Helper_Dunnage_PartSpecs.ExtractUdc(part);

        values.Should().HaveCount(10);
        values[0].Should().Be("Alpha");
        values[9].Should().Be("Omega");
    }

    [Fact]
    public void CreateDefaultUdcValues_ShouldFillDefaultsBySlot()
    {
        var fields = new List<Model_CustomFieldDefinition>
        {
            new() { DisplayOrder = 2, DefaultValue = "Tall" },
            new() { DisplayOrder = 5, DefaultValue = "48" },
            new() { DisplayOrder = 99, DefaultValue = "ignored" },
        };

        var values = Helper_Dunnage_PartSpecs.CreateDefaultUdcValues(fields);

        values.Should().HaveCount(10);
        values[1].Should().Be("Tall");
        values[4].Should().Be("48");
        values[0].Should().BeNull();
        values[9].Should().BeNull();
    }

    [Fact]
    public void BuildLabeledPairs_ShouldReturnFieldNameValues_OrderedBySlot()
    {
        var fields = new List<Model_CustomFieldDefinition>
        {
            new() { FieldName = "Width", DisplayOrder = 2 },
            new() { FieldName = "Length", DisplayOrder = 1 },
            new() { FieldName = "Empty", DisplayOrder = 3 },
        };
        var values = new string?[10];
        values[1] = "10";
        values[0] = "48";

        var pairs = Helper_Dunnage_PartSpecs.BuildLabeledPairs(fields, values);

        pairs.Select(pair => pair.Key).Should().Equal("Length", "Width");
        pairs.Select(pair => pair.Value).Should().Equal("48", "10");
    }

    [Fact]
    public void CreateSpecItem_ShouldMapDefinitionToDialogRow()
    {
        var field = new Model_CustomFieldDefinition
        {
            FieldName = "Height",
            FieldType = "Number",
            IsRequired = true,
            Unit = "in",
            MinValue = 1m,
            MaxValue = 9m,
            Choices = new List<string> { "A", "B" },
        };

        var specItem = Helper_Dunnage_PartSpecs.CreateSpecItem(field);

        specItem.Name.Should().Be("Height");
        specItem.DataType.Should().Be("Number");
        specItem.IsRequired.Should().BeTrue();
        specItem.Unit.Should().Be("in");
        specItem.MinValue.Should().Be(1.0);
        specItem.MaxValue.Should().Be(9.0);
        specItem.Choices.Should().Equal("A", "B");
    }

    [Fact]
    public void CreateDefinition_ShouldMapDialogRowToDefinition_WithDisplayOrder()
    {
        var specItem = new Model_SpecItem
        {
            Name = "Edge Guard",
            DataType = "Choices",
            IsRequired = true,
            Choices = new List<string> { "Yes", "No" },
        };

        var field = Helper_Dunnage_PartSpecs.CreateDefinition(specItem, displayOrder: 4);

        field.FieldName.Should().Be("Edge Guard");
        field.FieldType.Should().Be("Choices");
        field.IsRequired.Should().BeTrue();
        field.DisplayOrder.Should().Be(4);
        field.Choices.Should().Equal("Yes", "No");
    }

    [Fact]
    public void CreateSpecInput_ShouldMapDefinitionToInput_WithValue()
    {
        var field = new Model_CustomFieldDefinition
        {
            FieldName = "Stackable",
            FieldType = "Boolean",
            IsRequired = true,
            Choices = new List<string>(),
        };

        var input = Helper_Dunnage_PartSpecs.CreateSpecInput(field, value: true);

        input.SpecName.Should().Be("Stackable");
        input.SpecType.Should().Be("boolean");
        input.IsRequired.Should().BeTrue();
        input.Value.Should().Be(true);
    }
}

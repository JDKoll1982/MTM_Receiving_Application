using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Models;

public class Model_SpecItemTests
{
    [Fact]
    public void Description_ShouldIncludeChoices_WhenSpecTypeIsChoices()
    {
        var specItem = new Model_SpecItem
        {
            Name = "Type",
            DataType = "Choices",
            IsRequired = true,
            Choices = new List<string> { "Tall", "Short" },
        };

        specItem.Description.Should().Be("Choices (Choices: Tall, Short)");
    }

    [Fact]
    public void CreateDefinition_ShouldRoundTripSpecItemToCustomField()
    {
        var specItem = new Model_SpecItem
        {
            Name = "Type",
            DataType = "Choices",
            IsRequired = true,
            Choices = new List<string> { "Tall", "Short" },
        };

        var definition = Helper_Dunnage_PartSpecs.CreateDefinition(specItem, displayOrder: 2);

        definition.FieldName.Should().Be("Type");
        definition.FieldType.Should().Be("Choices");
        definition.IsRequired.Should().BeTrue();
        definition.DisplayOrder.Should().Be(2);
        definition.Choices.Should().Equal("Tall", "Short");
    }

    [Fact]
    public void CreateSpecItem_ShouldRoundTripCustomFieldToSpecItem()
    {
        var field = new Model_CustomFieldDefinition
        {
            FieldName = "Type",
            FieldType = "Choices",
            IsRequired = true,
            Unit = "units",
            MinValue = 1m,
            MaxValue = 9m,
            Choices = new List<string> { "Tall", "Short" },
        };

        var specItem = Helper_Dunnage_PartSpecs.CreateSpecItem(field);

        specItem.Name.Should().Be("Type");
        specItem.DataType.Should().Be("Choices");
        specItem.IsRequired.Should().BeTrue();
        specItem.Unit.Should().Be("units");
        specItem.MinValue.Should().Be(1.0);
        specItem.MaxValue.Should().Be(9.0);
        specItem.Choices.Should().Equal("Tall", "Short");
    }
}

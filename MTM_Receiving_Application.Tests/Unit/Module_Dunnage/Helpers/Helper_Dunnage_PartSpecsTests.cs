using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Helpers;

public sealed class Helper_Dunnage_PartSpecsTests
{
    [Fact]
    public void BuildCombinedSpecPayload_ShouldKeepConfiguredValuesAndAddDefinitionObjects()
    {
        var configuredValues = new Dictionary<string, object?>
        {
            ["Length"] = 48,
            ["Material"] = "HDPE",
        };

        var payload = Helper_Dunnage_PartSpecs.BuildCombinedSpecPayload(
            configuredValues,
            [
                new Model_SpecItem
                {
                    Name = "Edge Guard",
                    DataType = "Choices",
                    IsRequired = true,
                    Choices = ["Yes", "No"],
                },
            ],
            "Handle carefully"
        );

        payload["Length"].Should().Be(48);
        payload["Material"].Should().Be("HDPE");
        payload["Notes"].Should().Be("Handle carefully");
        payload["Edge Guard"].Should().BeOfType<SpecDefinition>();

        var definition = (SpecDefinition)payload["Edge Guard"]!;
        definition.DataType.Should().Be("Choices");
        definition.Required.Should().BeTrue();
        definition.Choices.Should().Equal("Yes", "No");
    }

    [Fact]
    public void BuildRuntimeValues_ShouldMergeConfiguredValuesAndDefinitionDefaults()
    {
        var configuredValues = new Dictionary<string, object>
        {
            ["Length"] = 48,
            ["Notes"] = "Keep dry",
        };

        var runtimeValues = Helper_Dunnage_PartSpecs.BuildRuntimeValues(
            configuredValues,
            new Dictionary<string, SpecDefinition>
            {
                ["Edge Guard"] = new() { DataType = "Choices" },
                ["Stackable"] = new() { DataType = "Boolean" },
            }
        );

        runtimeValues.Should().ContainKey("Length");
        runtimeValues.Should().ContainKey("Edge Guard");
        runtimeValues.Should().ContainKey("Stackable");
        runtimeValues.Should().NotContainKey("Notes");
        runtimeValues["Edge Guard"].Should().Be(string.Empty);
        runtimeValues["Stackable"].Should().Be(false);
    }

    [Fact]
    public void TryGetSpecDefinition_ShouldRecognizeSerializedDefinitionObject()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "type": "Number",
              "required": true,
              "unit": "in"
            }
            """
        );

        var result = Helper_Dunnage_PartSpecs.TryGetSpecDefinition(
            document.RootElement,
            out var definition
        );

        result.Should().BeTrue();
        definition.DataType.Should().Be("Number");
        definition.Required.Should().BeTrue();
        definition.Unit.Should().Be("in");
    }
}

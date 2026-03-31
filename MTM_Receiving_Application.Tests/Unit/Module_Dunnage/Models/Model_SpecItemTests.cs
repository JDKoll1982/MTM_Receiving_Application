using System.Collections.Generic;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Models;
using Xunit;

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
    public void SpecDefinition_ShouldRoundTripChoicesArray()
    {
        var definition = new SpecDefinition
        {
            DataType = "Choices",
            Required = true,
            Choices = new List<string> { "Tall", "Short" },
        };

        definition.Choices.Should().Equal("Tall", "Short");
        definition.Type.Should().Be("Choices");
    }
}


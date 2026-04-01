using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Models;

public sealed class Model_CustomFieldDefinitionTests
{
    [Theory]
    [InlineData("Weight (lbs)", "weight_lbs")]
    [InlineData("  Part Length  ", "part_length")]
    [InlineData("Width/Height+Depth", "width_height_depth")]
    [InlineData("Already__Clean", "already_clean")]
    [InlineData("", "")]
    public void BuildDatabaseColumnName_ShouldNormalizeFieldNames(
        string fieldName,
        string expectedColumnName
    )
    {
        var result = Model_CustomFieldDefinition.BuildDatabaseColumnName(fieldName);

        result.Should().Be(expectedColumnName);
    }
}

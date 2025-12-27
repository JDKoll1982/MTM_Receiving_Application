using System;
using System.Collections.Generic;
using Xunit;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.Models.Dunnage;

public class Model_DunnageSpec_Tests
{
    [Fact]
    public void SpecValue_SetJson_ShouldDeserializeToDictionary()
    {
        // Arrange
        var model = new Model_DunnageSpec();
        string json = "{\"Length\": 48, \"Width\": 40, \"Material\": \"Wood\"}";

        // Act
        model.SpecValue = json;

        // Assert
        Assert.NotNull(model.SpecsDefinition);
        Assert.Equal(3, model.SpecsDefinition.Count);
        Assert.True(model.SpecsDefinition.ContainsKey("Length"));
        Assert.True(model.SpecsDefinition.ContainsKey("Width"));
        Assert.True(model.SpecsDefinition.ContainsKey("Material"));
        
        // Note: System.Text.Json deserializes numbers as JsonElement by default when type is object
        // We just verify keys exist for now, or check string values
        Assert.Equal("Wood", model.SpecsDefinition["Material"].ToString());
    }

    [Fact]
    public void SpecValue_SetInvalidJson_ShouldSetEmptyDictionary()
    {
        // Arrange
        var model = new Model_DunnageSpec();
        string invalidJson = "{ Invalid JSON }";

        // Act
        model.SpecValue = invalidJson;

        // Assert
        Assert.NotNull(model.SpecsDefinition);
        Assert.Empty(model.SpecsDefinition);
    }

    [Fact]
    public void SpecValue_SetNull_ShouldSetEmptyDictionary()
    {
        // Arrange
        var model = new Model_DunnageSpec();
        
        // Act
        model.SpecValue = null!; // Testing null handling

        // Assert
        Assert.NotNull(model.SpecsDefinition);
        Assert.Empty(model.SpecsDefinition);
    }

    [Fact]
    public void CreatedDate_ShouldDefaultToNow()
    {
        // Arrange
        var model = new Model_DunnageSpec();

        // Assert
        Assert.NotEqual(DateTime.MinValue, model.CreatedDate);
        Assert.True((DateTime.Now - model.CreatedDate).TotalSeconds < 1);
    }
}

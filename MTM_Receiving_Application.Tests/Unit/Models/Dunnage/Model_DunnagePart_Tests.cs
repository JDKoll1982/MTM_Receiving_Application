using System;
using System.Collections.Generic;
using Xunit;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.Models.Dunnage;

public class Model_DunnagePart_Tests
{
    [Fact]
    public void SpecValues_SetJson_ShouldDeserializeToDictionary()
    {
        // Arrange
        var model = new Model_DunnagePart();
        string json = "{\"Color\": \"Blue\", \"Weight\": 15.5}";

        // Act
        model.SpecValues = json;

        // Assert
        Assert.NotNull(model.SpecValuesDict);
        Assert.Equal(2, model.SpecValuesDict.Count);
        Assert.True(model.SpecValuesDict.ContainsKey("Color"));
        Assert.True(model.SpecValuesDict.ContainsKey("Weight"));
        Assert.Equal("Blue", model.SpecValuesDict["Color"].ToString());
    }

    [Fact]
    public void SpecValues_SetInvalidJson_ShouldSetEmptyDictionary()
    {
        // Arrange
        var model = new Model_DunnagePart();
        string invalidJson = "Not JSON";

        // Act
        model.SpecValues = invalidJson;

        // Assert
        Assert.NotNull(model.SpecValuesDict);
        Assert.Empty(model.SpecValuesDict);
    }

    [Fact]
    public void CreatedDate_ShouldDefaultToNow()
    {
        // Arrange
        var model = new Model_DunnagePart();

        // Assert
        Assert.NotEqual(DateTime.MinValue, model.CreatedDate);
        Assert.True((DateTime.Now - model.CreatedDate).TotalSeconds < 1);
    }
}

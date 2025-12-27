using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xunit;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.Models.Dunnage;

public class Model_DunnageSession_Tests
{
    [Fact]
    public void HasLoads_ShouldBeFalse_WhenCollectionIsEmpty()
    {
        // Arrange
        var session = new Model_DunnageSession();

        // Assert
        Assert.False(session.HasLoads);
    }

    [Fact]
    public void HasLoads_ShouldBeTrue_WhenItemAdded()
    {
        // Arrange
        var session = new Model_DunnageSession();
        bool propertyChangedFired = false;
        session.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(Model_DunnageSession.HasLoads))
                propertyChangedFired = true;
        };

        // Act
        session.Loads.Add(new Model_DunnageLoad());

        // Assert
        Assert.True(session.HasLoads);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void HasLoads_ShouldBeFalse_WhenItemsCleared()
    {
        // Arrange
        var session = new Model_DunnageSession();
        session.Loads.Add(new Model_DunnageLoad());
        bool propertyChangedFired = false;
        
        // Reset flag after add
        session.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(Model_DunnageSession.HasLoads))
                propertyChangedFired = true;
        };

        // Act
        session.Loads.Clear();

        // Assert
        Assert.False(session.HasLoads);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void ReplacingCollection_ShouldUpdateHasLoadsAndSubscribers()
    {
        // Arrange
        var session = new Model_DunnageSession();
        var newCollection = new ObservableCollection<Model_DunnageLoad>
        {
            new Model_DunnageLoad()
        };

        // Act
        session.Loads = newCollection;

        // Assert
        Assert.True(session.HasLoads);
        
        // Verify subscription persists on new collection
        bool propertyChangedFired = false;
        session.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(Model_DunnageSession.HasLoads))
                propertyChangedFired = true;
        };

        session.Loads.Clear();
        Assert.False(session.HasLoads);
        Assert.True(propertyChangedFired);
    }
}

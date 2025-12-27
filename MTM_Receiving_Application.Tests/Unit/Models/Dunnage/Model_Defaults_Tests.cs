using System;
using Xunit;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Tests.Unit.Models.Dunnage;

public class Model_Defaults_Tests
{
    [Fact]
    public void Model_DunnageType_CreatedDate_ShouldDefaultToNow()
    {
        var model = new Model_DunnageType();
        Assert.NotEqual(DateTime.MinValue, model.CreatedDate);
        Assert.True((DateTime.Now - model.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void Model_DunnageLoad_Dates_ShouldDefaultToNow()
    {
        var model = new Model_DunnageLoad();
        
        Assert.NotEqual(DateTime.MinValue, model.CreatedDate);
        Assert.True((DateTime.Now - model.CreatedDate).TotalSeconds < 1);

        Assert.NotEqual(DateTime.MinValue, model.ReceivedDate);
        Assert.True((DateTime.Now - model.ReceivedDate).TotalSeconds < 1);
    }

    [Fact]
    public void Model_InventoriedDunnage_CreatedDate_ShouldDefaultToNow()
    {
        var model = new Model_InventoriedDunnage();
        Assert.NotEqual(DateTime.MinValue, model.CreatedDate);
        Assert.True((DateTime.Now - model.CreatedDate).TotalSeconds < 1);
    }
}

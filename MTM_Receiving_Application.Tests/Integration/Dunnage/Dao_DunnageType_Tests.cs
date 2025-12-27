using System;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using System.Diagnostics;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_DunnageType_Tests
{
    [Fact]
    public async Task FullCrudLifecycle_ShouldWork()
    {
        // Arrange
        string typeName = $"TestType_{Guid.NewGuid().ToString().Substring(0, 8)}";
        string user = "TestUser";
        int newId = 0;

        // Act & Assert - Insert
        var stopwatch = Stopwatch.StartNew();
        var insertResult = await Dao_DunnageType.InsertAsync(typeName, user);
        stopwatch.Stop();
        
        Assert.True(insertResult.Success, $"Insert failed: {insertResult.ErrorMessage}");
        Assert.True(insertResult.Data > 0);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Insert took too long: {stopwatch.ElapsedMilliseconds}ms");
        newId = insertResult.Data;

        // Act & Assert - GetById
        stopwatch.Restart();
        var getResult = await Dao_DunnageType.GetByIdAsync(newId);
        stopwatch.Stop();

        Assert.True(getResult.Success);
        Assert.NotNull(getResult.Data);
        Assert.Equal(typeName, getResult.Data.TypeName);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetById took too long: {stopwatch.ElapsedMilliseconds}ms");

        // Act & Assert - Update
        string updatedName = typeName + "_Updated";
        stopwatch.Restart();
        var updateResult = await Dao_DunnageType.UpdateAsync(newId, updatedName, user);
        stopwatch.Stop();

        Assert.True(updateResult.Success);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Update took too long: {stopwatch.ElapsedMilliseconds}ms");

        var verifyUpdate = await Dao_DunnageType.GetByIdAsync(newId);
        Assert.NotNull(verifyUpdate.Data);
        Assert.Equal(updatedName, verifyUpdate.Data.TypeName);

        // Act & Assert - GetAll
        stopwatch.Restart();
        var getAllResult = await Dao_DunnageType.GetAllAsync();
        stopwatch.Stop();

        Assert.True(getAllResult.Success);
        Assert.NotNull(getAllResult.Data);
        Assert.Contains(getAllResult.Data, t => t.Id == newId && t.TypeName == updatedName);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetAll took too long: {stopwatch.ElapsedMilliseconds}ms");

        // Act & Assert - Delete
        stopwatch.Restart();
        var deleteResult = await Dao_DunnageType.DeleteAsync(newId);
        stopwatch.Stop();

        Assert.True(deleteResult.Success);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Delete took too long: {stopwatch.ElapsedMilliseconds}ms");

        var verifyDelete = await Dao_DunnageType.GetByIdAsync(newId);
        Assert.False(verifyDelete.Success); // Should fail or return null data depending on implementation
    }

    [Fact]
    public async Task CountParts_ShouldReturnZero_ForNewType()
    {
        // Arrange
        string typeName = $"TestType_Count_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var insertResult = await Dao_DunnageType.InsertAsync(typeName, "TestUser");
        int typeId = insertResult.Data;

        try
        {
            // Act
            var countResult = await Dao_DunnageType.CountPartsAsync(typeId);

            // Assert
            Assert.True(countResult.Success);
            Assert.Equal(0, countResult.Data);
        }
        finally
        {
            // Cleanup
            await Dao_DunnageType.DeleteAsync(typeId);
        }
    }
}

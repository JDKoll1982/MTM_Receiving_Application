using System;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using System.Diagnostics;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_InventoriedDunnage_Tests
{
    [Fact]
    public async Task FullCrudLifecycle_ShouldWork()
    {
        // Arrange - Create Type and Part
        string typeName = $"TestType_Inv_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await Dao_DunnageType.InsertAsync(typeName, "TestUser");
        int typeId = typeResult.Data;

        string partId = $"PART-INV-{Guid.NewGuid().ToString().Substring(0, 8)}";
        await Dao_DunnagePart.InsertAsync(partId, typeId, "{}", "TestUser");

        try
        {
            string inventoryMethod = "Manual";
            string notes = "Check daily";
            string user = "TestUser";
            int newId = 0;

            // Act & Assert - Insert
            var stopwatch = Stopwatch.StartNew();
            var insertResult = await Dao_InventoriedDunnage.InsertAsync(partId, inventoryMethod, notes, user);
            stopwatch.Stop();

            Assert.True(insertResult.Success, $"Insert failed: {insertResult.ErrorMessage}");
            Assert.True(insertResult.Data > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Insert took too long: {stopwatch.ElapsedMilliseconds}ms");
            newId = insertResult.Data;

            // Act & Assert - Check
            stopwatch.Restart();
            var checkResult = await Dao_InventoriedDunnage.CheckAsync(partId);
            stopwatch.Stop();

            Assert.True(checkResult.Success);
            Assert.True(checkResult.Data);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Check took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetByPart
            stopwatch.Restart();
            var getResult = await Dao_InventoriedDunnage.GetByPartAsync(partId);
            stopwatch.Stop();

            Assert.True(getResult.Success);
            Assert.NotNull(getResult.Data);
            Assert.Equal(partId, getResult.Data.PartId);
            Assert.Equal(inventoryMethod, getResult.Data.InventoryMethod);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetByPart took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetAll
            stopwatch.Restart();
            var getAllResult = await Dao_InventoriedDunnage.GetAllAsync();
            stopwatch.Stop();

            Assert.True(getAllResult.Success);
            Assert.NotNull(getAllResult.Data);
            Assert.Contains(getAllResult.Data, i => i.Id == newId);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetAll took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Update
            string updatedMethod = "API";
            stopwatch.Restart();
            var updateResult = await Dao_InventoriedDunnage.UpdateAsync(newId, updatedMethod, notes, user);
            stopwatch.Stop();

            Assert.True(updateResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Update took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyUpdate = await Dao_InventoriedDunnage.GetByPartAsync(partId);
            Assert.NotNull(verifyUpdate.Data);
            Assert.Equal(updatedMethod, verifyUpdate.Data.InventoryMethod);

            // Act & Assert - Delete
            stopwatch.Restart();
            var deleteResult = await Dao_InventoriedDunnage.DeleteAsync(newId);
            stopwatch.Stop();

            Assert.True(deleteResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Delete took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyDelete = await Dao_InventoriedDunnage.CheckAsync(partId);
            Assert.False(verifyDelete.Data);
        }
        finally
        {
            // Cleanup
            await Dao_DunnagePart.DeleteAsync(await GetPartId(partId));
            await Dao_DunnageType.DeleteAsync(typeId);
        }
    }

    private async Task<int> GetPartId(string partId)
    {
        var part = await Dao_DunnagePart.GetByIdAsync(partId);
        Assert.NotNull(part.Data);
        return part.Data.Id;
    }
}

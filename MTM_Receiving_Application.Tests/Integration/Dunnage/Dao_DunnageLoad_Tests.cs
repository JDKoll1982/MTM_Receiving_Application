using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using System.Diagnostics;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_DunnageLoad_Tests
{
    [Fact]
    public async Task FullCrudLifecycle_ShouldWork()
    {
        // Arrange - Create Type and Part
        string typeName = $"TestType_Load_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await Dao_DunnageType.InsertAsync(typeName, "TestUser");
        int typeId = typeResult.Data;

        string partId = $"PART-LOAD-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var partResult = await Dao_DunnagePart.InsertAsync(partId, typeId, "{}", "TestUser");
        Assert.True(partResult.Success, $"Part insert failed: {partResult.ErrorMessage}");

        try
        {
            Guid loadUuid = Guid.NewGuid();
            decimal quantity = 50.5m;
            string user = "TestUser";

            // Act & Assert - Insert
            var stopwatch = Stopwatch.StartNew();
            var insertResult = await Dao_DunnageLoad.InsertAsync(loadUuid, partId, quantity, user);
            stopwatch.Stop();

            Assert.True(insertResult.Success, $"Insert failed: {insertResult.ErrorMessage}");
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Insert took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetById
            stopwatch.Restart();
            var getResult = await Dao_DunnageLoad.GetByIdAsync(loadUuid);
            stopwatch.Stop();

            Assert.True(getResult.Success, $"GetById failed: {getResult.ErrorMessage}");
            Assert.NotNull(getResult.Data);
            Assert.Equal(loadUuid, getResult.Data.LoadUuid);
            Assert.Equal(partId, getResult.Data.PartId);
            Assert.Equal(quantity, getResult.Data.Quantity);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetById took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetAll
            stopwatch.Restart();
            var getAllResult = await Dao_DunnageLoad.GetAllAsync();
            stopwatch.Stop();

            Assert.True(getAllResult.Success);
            Assert.NotNull(getAllResult.Data);
            Assert.Contains(getAllResult.Data, l => l.LoadUuid == loadUuid);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetAll took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetByDateRange
            stopwatch.Restart();
            var getRangeResult = await Dao_DunnageLoad.GetByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(1));
            stopwatch.Stop();

            Assert.True(getRangeResult.Success);
            Assert.NotNull(getRangeResult.Data);
            Assert.Contains(getRangeResult.Data, l => l.LoadUuid == loadUuid);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetByDateRange took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Update
            decimal updatedQuantity = 75.0m;
            stopwatch.Restart();
            var updateResult = await Dao_DunnageLoad.UpdateAsync(loadUuid, updatedQuantity, user);
            stopwatch.Stop();

            Assert.True(updateResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Update took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyUpdate = await Dao_DunnageLoad.GetByIdAsync(loadUuid);
            Assert.NotNull(verifyUpdate.Data);
            Assert.Equal(updatedQuantity, verifyUpdate.Data.Quantity);

            // Act & Assert - Delete
            stopwatch.Restart();
            var deleteResult = await Dao_DunnageLoad.DeleteAsync(loadUuid);
            stopwatch.Stop();

            Assert.True(deleteResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Delete took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyDelete = await Dao_DunnageLoad.GetByIdAsync(loadUuid);
            Assert.False(verifyDelete.Success);
        }
        finally
        {
            // Cleanup
            await Dao_DunnagePart.DeleteAsync(await GetPartId(partId));
            await Dao_DunnageType.DeleteAsync(typeId);
        }
    }

    [Fact]
    public async Task InsertBatch_ShouldWork()
    {
        // Arrange
        string typeName = $"TestType_Batch_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await Dao_DunnageType.InsertAsync(typeName, "TestUser");
        int typeId = typeResult.Data;

        string partId = $"PART-BATCH-{Guid.NewGuid().ToString().Substring(0, 8)}";
        await Dao_DunnagePart.InsertAsync(partId, typeId, "{}", "TestUser");

        try
        {
            var loads = new List<Model_DunnageLoad>
            {
                new Model_DunnageLoad { LoadUuid = Guid.NewGuid(), PartId = partId, Quantity = 10 },
                new Model_DunnageLoad { LoadUuid = Guid.NewGuid(), PartId = partId, Quantity = 20 }
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var batchResult = await Dao_DunnageLoad.InsertBatchAsync(loads, "TestUser");
            stopwatch.Stop();

            // Assert
            Assert.True(batchResult.Success, $"Batch insert failed: {batchResult.ErrorMessage}");
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Batch insert took too long: {stopwatch.ElapsedMilliseconds}ms");

            var allLoads = await Dao_DunnageLoad.GetAllAsync();
            Assert.NotNull(allLoads.Data);
            Assert.Contains(allLoads.Data, l => l.LoadUuid == loads[0].LoadUuid);
            Assert.Contains(allLoads.Data, l => l.LoadUuid == loads[1].LoadUuid);

            // Cleanup loads
            foreach (var load in loads)
            {
                await Dao_DunnageLoad.DeleteAsync(load.LoadUuid);
            }
        }
        finally
        {
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


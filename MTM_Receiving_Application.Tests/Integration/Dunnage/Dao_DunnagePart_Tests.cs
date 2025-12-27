using System;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using System.Diagnostics;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_DunnagePart_Tests
{
    [Fact]
    public async Task FullCrudLifecycle_ShouldWork()
    {
        // Arrange - Create a Type first
        string typeName = $"TestType_Part_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await Dao_DunnageType.InsertAsync(typeName, "TestUser");
        Assert.True(typeResult.Success, "Failed to create prerequisite Type");
        int typeId = typeResult.Data;

        try
        {
            string partId = $"PART-{Guid.NewGuid().ToString().Substring(0, 8)}";
            string specValues = "{\"length\": 120, \"width\": 100}";
            string user = "TestUser";
            int newId = 0;

            // Act & Assert - Insert
            var stopwatch = Stopwatch.StartNew();
            var insertResult = await Dao_DunnagePart.InsertAsync(partId, typeId, specValues, user);
            stopwatch.Stop();

            Assert.True(insertResult.Success, $"Insert failed: {insertResult.ErrorMessage}");
            Assert.True(insertResult.Data > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Insert took too long: {stopwatch.ElapsedMilliseconds}ms");
            newId = insertResult.Data;

            // Act & Assert - GetById
            stopwatch.Restart();
            var getResult = await Dao_DunnagePart.GetByIdAsync(partId);
            stopwatch.Stop();

            Assert.True(getResult.Success);
            Assert.NotNull(getResult.Data);
            Assert.Equal(partId, getResult.Data.PartId);
            Assert.Equal(typeId, getResult.Data.TypeId);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetById took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetByType
            stopwatch.Restart();
            var getByTypeResult = await Dao_DunnagePart.GetByTypeAsync(typeId);
            stopwatch.Stop();

            Assert.True(getByTypeResult.Success);
            Assert.NotNull(getByTypeResult.Data);
            Assert.Contains(getByTypeResult.Data, p => p.Id == newId);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetByType took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Update
            string updatedSpecValues = "{\"length\": 120, \"width\": 100, \"height\": 15}";
            stopwatch.Restart();
            var updateResult = await Dao_DunnagePart.UpdateAsync(newId, updatedSpecValues, user);
            stopwatch.Stop();

            Assert.True(updateResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Update took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyUpdate = await Dao_DunnagePart.GetByIdAsync(partId);
            Assert.NotNull(verifyUpdate.Data);
            Assert.Contains("height", verifyUpdate.Data.SpecValues);

            // Act & Assert - Search (SC-006 < 200ms)
            stopwatch.Restart();
            var searchResult = await Dao_DunnagePart.SearchAsync(partId.Substring(0, 5));
            stopwatch.Stop();

            Assert.True(searchResult.Success);
            Assert.NotNull(searchResult.Data);
            Assert.Contains(searchResult.Data, p => p.Id == newId);
            Assert.True(stopwatch.ElapsedMilliseconds < 200, $"Search took too long: {stopwatch.ElapsedMilliseconds}ms (Limit 200ms)");

            // Act & Assert - CountTransactions (Should be 0)
            stopwatch.Restart();
            var countResult = await Dao_DunnagePart.CountTransactionsAsync(partId);
            stopwatch.Stop();

            Assert.True(countResult.Success);
            Assert.Equal(0, countResult.Data);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"CountTransactions took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Delete
            stopwatch.Restart();
            var deleteResult = await Dao_DunnagePart.DeleteAsync(newId);
            stopwatch.Stop();

            Assert.True(deleteResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Delete took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyDelete = await Dao_DunnagePart.GetByIdAsync(partId);
            Assert.False(verifyDelete.Success);
        }
        finally
        {
            // Cleanup Type
            await Dao_DunnageType.DeleteAsync(typeId);
        }
    }
}

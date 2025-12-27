using System;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Helpers.Database;
using System.Diagnostics;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_DunnagePart_Tests
{
    private readonly Dao_DunnagePart _daoPart;
    private readonly Dao_DunnageType _daoType;

    public Dao_DunnagePart_Tests()
    {
        string conn = Helper_Database_Variables.GetConnectionString();
        _daoPart = new Dao_DunnagePart(conn);
        _daoType = new Dao_DunnageType(conn);
    }

    [Fact]
    public async Task FullCrudLifecycle_ShouldWork()
    {
        // Arrange - Create a Type first
        string typeName = $"TestType_Part_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await _daoType.InsertAsync(typeName, "TestUser");
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
            var insertResult = await _daoPart.InsertAsync(partId, typeId, specValues, user);
            stopwatch.Stop();

            Assert.True(insertResult.Success, $"Insert failed: {insertResult.ErrorMessage}");
            Assert.True(insertResult.Data > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Insert took too long: {stopwatch.ElapsedMilliseconds}ms");
            newId = insertResult.Data;

            // Act & Assert - GetById
            stopwatch.Restart();
            var getResult = await _daoPart.GetByIdAsync(partId);
            stopwatch.Stop();

            Assert.True(getResult.Success);
            Assert.NotNull(getResult.Data);
            Assert.Equal(partId, getResult.Data.PartId);
            Assert.Equal(typeId, getResult.Data.TypeId);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetById took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetByType
            stopwatch.Restart();
            var getByTypeResult = await _daoPart.GetByTypeAsync(typeId);
            stopwatch.Stop();

            Assert.True(getByTypeResult.Success);
            Assert.NotNull(getByTypeResult.Data);
            Assert.Contains(getByTypeResult.Data, p => p.Id == newId);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetByType took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Update
            string updatedSpecValues = "{\"length\": 120, \"width\": 100, \"height\": 15}";
            stopwatch.Restart();
            var updateResult = await _daoPart.UpdateAsync(newId, updatedSpecValues, user);
            stopwatch.Stop();

            Assert.True(updateResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Update took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyUpdate = await _daoPart.GetByIdAsync(partId);
            Assert.NotNull(verifyUpdate.Data);
            Assert.Contains("height", verifyUpdate.Data.SpecValues);

            // Act & Assert - Search (SC-006 < 200ms)
            stopwatch.Restart();
            var searchResult = await _daoPart.SearchAsync(partId.Substring(0, 5));
            stopwatch.Stop();

            Assert.True(searchResult.Success);
            Assert.NotNull(searchResult.Data);
            Assert.Contains(searchResult.Data, p => p.Id == newId);
            Assert.True(stopwatch.ElapsedMilliseconds < 200, $"Search took too long: {stopwatch.ElapsedMilliseconds}ms (Limit 200ms)");

            // Act & Assert - CountTransactions (Should be 0)
            stopwatch.Restart();
            var countResult = await _daoPart.CountTransactionsAsync(partId);
            stopwatch.Stop();

            Assert.True(countResult.Success);
            Assert.Equal(0, countResult.Data);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"CountTransactions took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Delete
            stopwatch.Restart();
            var deleteResult = await _daoPart.DeleteAsync(newId);
            stopwatch.Stop();

            Assert.True(deleteResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Delete took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyDelete = await _daoPart.GetByIdAsync(partId);
            Assert.False(verifyDelete.Success);
        }
        finally
        {
            // Cleanup Type
            await _daoType.DeleteAsync(typeId);
        }
    }
}

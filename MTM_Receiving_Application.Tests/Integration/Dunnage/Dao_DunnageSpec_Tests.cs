using System;
using System.Threading.Tasks;
using Xunit;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Helpers.Database;
using System.Diagnostics;

namespace MTM_Receiving_Application.Tests.Integration.Dunnage;

public class Dao_DunnageSpec_Tests
{
    private readonly Dao_DunnageSpec _daoSpec;
    private readonly Dao_DunnageType _daoType;

    public Dao_DunnageSpec_Tests()
    {
        string conn = Helper_Database_Variables.GetConnectionString();
        _daoSpec = new Dao_DunnageSpec(conn);
        _daoType = new Dao_DunnageType(conn);
    }

    [Fact]
    public async Task FullCrudLifecycle_ShouldWork()
    {
        // Arrange - Create a Type first
        string typeName = $"TestType_Spec_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await _daoType.InsertAsync(typeName, "TestUser");
        Assert.True(typeResult.Success, "Failed to create prerequisite Type");
        int typeId = typeResult.Data;

        try
        {
            string specKey = "material";
            string specValue = "{\"type\": \"string\", \"options\": [\"wood\", \"plastic\"]}";
            string user = "TestUser";
            int newId = 0;

            // Act & Assert - Insert
            var stopwatch = Stopwatch.StartNew();
            var insertResult = await _daoSpec.InsertAsync(typeId, specKey, specValue, user);
            stopwatch.Stop();

            Assert.True(insertResult.Success, $"Insert failed: {insertResult.ErrorMessage}");
            Assert.True(insertResult.Data > 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Insert took too long: {stopwatch.ElapsedMilliseconds}ms");
            newId = insertResult.Data;

            // Act & Assert - GetById
            stopwatch.Restart();
            var getResult = await _daoSpec.GetByIdAsync(newId);
            stopwatch.Stop();

            Assert.True(getResult.Success);
            Assert.NotNull(getResult.Data);
            Assert.Equal(specKey, getResult.Data.SpecKey);
            // Note: JSON formatting might change slightly (whitespace), so exact string match might be fragile.
            // But for simple JSON it should be fine or we can parse it.
            // For now, let's check it contains key elements.
            Assert.Contains("wood", getResult.Data.SpecValue);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetById took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - GetByType
            stopwatch.Restart();
            var getByTypeResult = await _daoSpec.GetByTypeAsync(typeId);
            stopwatch.Stop();

            Assert.True(getByTypeResult.Success);
            Assert.NotNull(getByTypeResult.Data);
            Assert.Contains(getByTypeResult.Data, s => s.Id == newId);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"GetByType took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Update
            string updatedValue = "{\"type\": \"string\", \"options\": [\"wood\", \"plastic\", \"metal\"]}";
            stopwatch.Restart();
            var updateResult = await _daoSpec.UpdateAsync(newId, updatedValue, user);
            stopwatch.Stop();

            Assert.True(updateResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Update took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyUpdate = await _daoSpec.GetByIdAsync(newId);
            Assert.NotNull(verifyUpdate.Data);
            Assert.Contains("metal", verifyUpdate.Data.SpecValue);

            // Act & Assert - CountPartsUsingSpec (Should be 0 as no parts yet)
            stopwatch.Restart();
            var countResult = await _daoSpec.CountPartsUsingSpecAsync(typeId, specKey);
            stopwatch.Stop();

            Assert.True(countResult.Success);
            Assert.Equal(0, countResult.Data);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"CountPartsUsingSpec took too long: {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - DeleteById
            stopwatch.Restart();
            var deleteResult = await _daoSpec.DeleteByIdAsync(newId);
            stopwatch.Stop();

            Assert.True(deleteResult.Success);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"DeleteById took too long: {stopwatch.ElapsedMilliseconds}ms");

            var verifyDelete = await _daoSpec.GetByIdAsync(newId);
            Assert.False(verifyDelete.Success);
        }
        finally
        {
            // Cleanup Type
            await _daoType.DeleteAsync(typeId);
        }
    }

    [Fact]
    public async Task DeleteByType_ShouldRemoveAllSpecs()
    {
        // Arrange
        string typeName = $"TestType_DelAll_{Guid.NewGuid().ToString().Substring(0, 8)}";
        var typeResult = await _daoType.InsertAsync(typeName, "TestUser");
        int typeId = typeResult.Data;

        try
        {
            await _daoSpec.InsertAsync(typeId, "spec1", "{}", "TestUser");
            await _daoSpec.InsertAsync(typeId, "spec2", "{}", "TestUser");

            var specs = await _daoSpec.GetByTypeAsync(typeId);
            Assert.NotNull(specs.Data);
            Assert.Equal(2, specs.Data.Count);

            // Act
            var deleteResult = await _daoSpec.DeleteByTypeAsync(typeId);

            // Assert
            Assert.True(deleteResult.Success);

            var verifyDelete = await _daoSpec.GetByTypeAsync(typeId);
            Assert.NotNull(verifyDelete.Data);
            Assert.Empty(verifyDelete.Data);
        }
        finally
        {
            await _daoType.DeleteAsync(typeId);
        }
    }
}

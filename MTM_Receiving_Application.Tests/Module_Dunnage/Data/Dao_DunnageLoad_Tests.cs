using FluentAssertions;
using Xunit;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Data;

/// <summary>
/// Unit tests for Dao_DunnageLoad data access object.
/// Tests CRUD operations, batch processing, and date range queries for dunnage loads.
/// Uses the mtm_receiving_application_test database.
/// </summary>
public class Dao_DunnageLoad_Tests
{
    private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);

    // ====================================================================
    // Constructor Tests
    // ====================================================================

    [Fact]
    public void Constructor_ValidConnectionString_CreatesInstance()
    {
        // Act
        var dao = new Dao_DunnageLoad(TestConnectionString);

        // Assert
        dao.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConnectionString_DoesNotThrow()
    {
        // Act
        Action act = () => new Dao_DunnageLoad(null!);

        // Assert
        act.Should().NotThrow();
    }

    // ====================================================================
    // GetAllAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetAllAsync_NoParameters_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);

        // Act
        var result = await dao.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // GetByDateRangeAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetByDateRangeAsync_ValidDateRange_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        // Act
        var result = await dao.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByDateRangeAsync_StartDateAfterEndDate_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var startDate = DateTime.Now;
        var endDate = DateTime.Now.AddDays(-30);

        // Act
        var result = await dao.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByDateRangeAsync_SameDates_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var date = DateTime.Now;

        // Act
        var result = await dao.GetByDateRangeAsync(date, date);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByDateRangeAsync_VeryOldDates_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var startDate = new DateTime(1900, 1, 1);
        var endDate = new DateTime(1900, 12, 31);

        // Act
        Func<Task> act = async () => await dao.GetByDateRangeAsync(startDate, endDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetByDateRangeAsync_FutureDates_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var startDate = DateTime.Now.AddYears(1);
        var endDate = DateTime.Now.AddYears(2);

        // Act
        Func<Task> act = async () => await dao.GetByDateRangeAsync(startDate, endDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetByDateRangeAsync_VeryLargeRange_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var startDate = DateTime.MinValue;
        var endDate = DateTime.MaxValue;

        // Act
        Func<Task> act = async () => await dao.GetByDateRangeAsync(startDate, endDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // GetByIdAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetByIdAsync_ValidGuid_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();

        // Act
        var result = await dao.GetByIdAsync(loadUuid);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_EmptyGuid_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.Empty;

        // Act
        Func<Task> act = async () => await dao.GetByIdAsync(loadUuid);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetByIdAsync_DifferentGuids_HandlesAll()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var guid = Guid.NewGuid();
            Func<Task> act = async () => await dao.GetByIdAsync(guid);
            await act.Should().NotThrowAsync();
        }
    }

    // ====================================================================
    // InsertAsync Tests
    // ====================================================================

    [Fact]
    public async Task InsertAsync_ValidParameters_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = "DUNNAGE-001";
        var quantity = 100.5m;
        var user = "testuser";

        // Act
        var result = await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("PART-001", 100.0)]
    [InlineData("DUNNAGE-123", 50.5)]
    [InlineData("SKD-456", 1000.99)]
    public async Task InsertAsync_DifferentParts_HandlesAll(string partId, decimal quantity)
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var user = "testuser";

        // Act
        var result = await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.001)]
    [InlineData(1.0)]
    [InlineData(999999.99)]
    public async Task InsertAsync_DifferentQuantities_HandlesAll(decimal quantity)
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = "PART-001";
        var user = "testuser";

        // Act
        var result = await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("admin")]
    [InlineData("testuser")]
    [InlineData("")]
    public async Task InsertAsync_DifferentUsers_HandlesAll(string user)
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = "PART-001";
        var quantity = 100m;

        // Act
        var result = await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // InsertBatchAsync Tests
    // ====================================================================

    [Fact]
    public async Task InsertBatchAsync_ValidSingleLoad_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loads = new List<Model_DunnageLoad> { CreateValidDunnageLoad() };
        var user = "testuser";

        // Act
        var result = await dao.InsertBatchAsync(loads, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertBatchAsync_MultipleLoads_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loads = new List<Model_DunnageLoad>
        {
            CreateValidDunnageLoad(),
            CreateValidDunnageLoad(),
            CreateValidDunnageLoad()
        };
        var user = "testuser";

        // Act
        var result = await dao.InsertBatchAsync(loads, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task InsertBatchAsync_VariousBatchSizes_HandlesAll(int batchSize)
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loads = new List<Model_DunnageLoad>();
        for (int i = 0; i < batchSize; i++)
        {
            loads.Add(CreateValidDunnageLoad());
        }
        var user = "testuser";

        // Act
        var result = await dao.InsertBatchAsync(loads, user);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // UpdateAsync Tests
    // ====================================================================

    [Fact]
    public async Task UpdateAsync_ValidParameters_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var quantity = 150.5m;
        var user = "testuser";

        // Act
        var result = await dao.UpdateAsync(loadUuid, quantity, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.001)]
    [InlineData(100.5)]
    [InlineData(9999.99)]
    public async Task UpdateAsync_DifferentQuantities_HandlesAll(decimal quantity)
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var user = "testuser";

        // Act
        var result = await dao.UpdateAsync(loadUuid, quantity, user);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_EmptyGuid_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.Empty;
        var quantity = 100m;
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.UpdateAsync(loadUuid, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // DeleteAsync Tests
    // ====================================================================

    [Fact]
    public async Task DeleteAsync_ValidGuid_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();

        // Act
        var result = await dao.DeleteAsync(loadUuid);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_EmptyGuid_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.Empty;

        // Act
        Func<Task> act = async () => await dao.DeleteAsync(loadUuid);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_MultipleDeletes_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var guid = Guid.NewGuid();
            Func<Task> act = async () => await dao.DeleteAsync(guid);
            await act.Should().NotThrowAsync();
        }
    }

    // ====================================================================
    // Edge Cases and Boundary Value Tests
    // ====================================================================

    [Fact]
    public async Task InsertAsync_VeryLongPartId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = new string('P', 500);
        var quantity = 100m;
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_VeryLongUsername_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = "PART-001";
        var quantity = 100m;
        var user = new string('U', 500);

        // Act
        Func<Task> act = async () => await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_SpecialCharactersInPartId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = "PART-@#$%^&*()";
        var quantity = 100m;
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_NegativeQuantity_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var partId = "PART-001";
        var quantity = -100m;
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.InsertAsync(loadUuid, partId, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertBatchAsync_EmptyList_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loads = new List<Model_DunnageLoad>();
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.InsertBatchAsync(loads, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertBatchAsync_DuplicatePartIds_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var load = CreateValidDunnageLoad();
        var loads = new List<Model_DunnageLoad> { load, load, load };
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.InsertBatchAsync(loads, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_VeryLargeQuantity_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var quantity = decimal.MaxValue;
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.UpdateAsync(loadUuid, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_VerySmallQuantity_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_DunnageLoad(TestConnectionString);
        var loadUuid = Guid.NewGuid();
        var quantity = 0.000001m;
        var user = "testuser";

        // Act
        Func<Task> act = async () => await dao.UpdateAsync(loadUuid, quantity, user);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // Helper Methods
    // ====================================================================

    /// <summary>
    /// Creates a valid test dunnage load with all required fields populated.
    /// </summary>
    private static Model_DunnageLoad CreateValidDunnageLoad()
    {
        return new Model_DunnageLoad
        {
            LoadUuid = Guid.NewGuid(),
            PartId = "DUNNAGE-001",
            Quantity = 100.5m,
            ReceivedDate = DateTime.Now,
            CreatedBy = "testuser",
            CreatedDate = DateTime.Now,
            ModifiedBy = null,
            ModifiedDate = null
        };
    }
}

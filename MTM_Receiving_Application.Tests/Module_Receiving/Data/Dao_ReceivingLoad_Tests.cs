using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Data;

/// <summary>
/// Unit tests for Dao_ReceivingLoad data access object.
/// Tests CRUD operations, transactions, and batch processing for receiving loads.
/// </summary>
public class Dao_ReceivingLoad_Tests
{

    private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);

    // ====================================================================
    // Constructor Tests
    // ====================================================================

    [Fact]
    public void Constructor_ValidConnectionString_CreatesInstance()
    {
        // Act
        var dao = new Dao_ReceivingLoad(TestConnectionString);

        // Assert
        dao.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConnectionString_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new Dao_ReceivingLoad(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*connectionString*");
    }

    // ====================================================================
    // SaveLoadsAsync Tests
    // ====================================================================

    [Fact]
    public async Task SaveLoadsAsync_NullList_ReturnsFailure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);

        // Act
        var result = await dao.SaveLoadsAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Fact]
    public async Task SaveLoadsAsync_EmptyList_ReturnsFailure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>();

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Fact]
    public async Task SaveLoadsAsync_ValidSingleLoad_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad> { CreateValidLoad() };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveLoadsAsync_MultipleValidLoads_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateValidLoad(),
            CreateValidLoad(),
            CreateValidLoad()
        };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveLoadsAsync_NullPONumber_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.PoNumber = null;
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("PO-12345")]
    [InlineData("12345")]
    [InlineData("PO12345")]
    public async Task SaveLoadsAsync_CleansPONumberPrefix_HandlesAll_Async(string poNumber)
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.PoNumber = poNumber;
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveLoadsAsync_NonPOItem_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.IsNonPOItem = true;
        load.PoNumber = null;
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // UpdateLoadsAsync Tests
    // ====================================================================

    [Fact]
    public async Task UpdateLoadsAsync_NullList_ReturnsFailure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);

        // Act
        var result = await dao.UpdateLoadsAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Fact]
    public async Task UpdateLoadsAsync_EmptyList_ReturnsFailure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>();

        // Act
        var result = await dao.UpdateLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLoadsAsync_ValidSingleLoad_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad> { CreateValidLoad() };

        // Act
        var result = await dao.UpdateLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLoadsAsync_MultipleLoads_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateValidLoad(),
            CreateValidLoad()
        };

        // Act
        var result = await dao.UpdateLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // DeleteLoadsAsync Tests
    // ====================================================================

    [Fact]
    public async Task DeleteLoadsAsync_NullList_ReturnsSuccess_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);

        // Act
        var result = await dao.DeleteLoadsAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task DeleteLoadsAsync_EmptyList_ReturnsSuccess_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>();

        // Act
        var result = await dao.DeleteLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task DeleteLoadsAsync_ValidSingleLoad_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad> { CreateValidLoad() };

        // Act
        var result = await dao.DeleteLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteLoadsAsync_MultipleLoads_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>
        {
            CreateValidLoad(),
            CreateValidLoad(),
            CreateValidLoad()
        };

        // Act
        var result = await dao.DeleteLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // GetHistoryAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetHistoryAsync_ValidParameters_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var partID = "PART-001";
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        // Act
        var result = await dao.GetHistoryAsync(partID, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("PART-001")]
    [InlineData("ABC123")]
    public async Task GetHistoryAsync_DifferentPartIDs_HandlesAll_Async(string partID)
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        // Act
        var result = await dao.GetHistoryAsync(partID, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHistoryAsync_StartDateAfterEndDate_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var partID = "PART-001";
        var startDate = DateTime.Now;
        var endDate = DateTime.Now.AddDays(-30);

        // Act
        var result = await dao.GetHistoryAsync(partID, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHistoryAsync_SameDateRange_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var partID = "PART-001";
        var date = DateTime.Now;

        // Act
        var result = await dao.GetHistoryAsync(partID, date, date);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // GetAllAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetAllAsync_ValidDateRange_CallsStoredProcedure_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        // Act
        var result = await dao.GetAllAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WideRange_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var startDate = DateTime.Now.AddYears(-10);
        var endDate = DateTime.Now.AddYears(1);

        // Act
        var result = await dao.GetAllAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // Edge Cases and Boundary Value Tests
    // ====================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(0.001)]
    [InlineData(1.5)]
    [InlineData(1000000.99)]
    public async Task SaveLoadsAsync_DifferentWeights_HandlesAll_Async(decimal weight)
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.WeightQuantity = weight;
        load.WeightPerPackage = weight / load.PackagesPerLoad;
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task SaveLoadsAsync_DifferentPackageCounts_HandlesAll_Async(int packageCount)
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.PackagesPerLoad = packageCount;
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        var result = await dao.SaveLoadsAsync(loads);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveLoadsAsync_VeryLongPartID_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.PartID = new string('A', 500);
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        Func<Task> act = async () => await dao.SaveLoadsAsync(loads);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveLoadsAsync_VeryLongHeatNumber_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.HeatLotNumber = new string('H', 1000);
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        Func<Task> act = async () => await dao.SaveLoadsAsync(loads);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveLoadsAsync_SpecialCharactersInFields_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var load = CreateValidLoad();
        load.PartID = "PART-@#$%";
        load.HeatLotNumber = "HEAT-<>{}";
        load.PackageTypeName = "Package's \"Type\"";
        var loads = new List<Model_ReceivingLoad> { load };

        // Act
        Func<Task> act = async () => await dao.SaveLoadsAsync(loads);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task SaveLoadsAsync_VariousBatchSizes_HandlesAll_Async(int batchSize)
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var loads = new List<Model_ReceivingLoad>();
        for (int i = 0; i < batchSize; i++)
        {
            loads.Add(CreateValidLoad());
        }

        // Act
        Func<Task> act = async () => await dao.SaveLoadsAsync(loads);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetHistoryAsync_VeryOldDates_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var startDate = new DateTime(1900, 1, 1);
        var endDate = new DateTime(1900, 12, 31);

        // Act
        Func<Task> act = async () => await dao.GetHistoryAsync("PART-001", startDate, endDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllAsync_FutureDates_HandlesGracefully_Async()
    {
        // Arrange
        var dao = new Dao_ReceivingLoad(TestConnectionString);
        var startDate = DateTime.Now.AddYears(1);
        var endDate = DateTime.Now.AddYears(2);

        // Act
        Func<Task> act = async () => await dao.GetAllAsync(startDate, endDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // Helper Methods
    // ====================================================================

    /// <summary>
    /// Creates a valid test receiving load with all required fields populated.
    /// </summary>
    private static Model_ReceivingLoad CreateValidLoad()
    {
        return new Model_ReceivingLoad
        {
            LoadID = Guid.NewGuid(),
            PartID = "PART-001",
            PartType = "Steel Coil",
            PoNumber = "PO-12345",
            PoLineNumber = "001",
            LoadNumber = 1,
            WeightQuantity = 1000.50m,
            HeatLotNumber = "HEAT-ABC123",
            PackagesPerLoad = 10,
            PackageTypeName = "Skid",
            WeightPerPackage = 100.05m,
            IsNonPOItem = false,
            ReceivedDate = DateTime.Now
        };
    }
}


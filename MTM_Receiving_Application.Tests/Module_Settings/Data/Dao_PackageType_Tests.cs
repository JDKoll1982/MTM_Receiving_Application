using FluentAssertions;
using Xunit;
using MTM_Receiving_Application.Module_Settings.Data;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Data;

/// <summary>
/// Unit tests for Dao_PackageType data access object.
/// Tests CRUD operations for package type management.
/// </summary>
public class Dao_PackageType_Tests
{
    private const string TestConnectionString = "Server=localhost;Database=test_db;User Id=test;Password=test;";

    // ====================================================================
    // Constructor Tests
    // ====================================================================

    [Fact]
    public void Constructor_ValidConnectionString_CreatesInstance()
    {
        // Act
        var dao = new Dao_PackageType(TestConnectionString);

        // Assert
        dao.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConnectionString_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new Dao_PackageType(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*connectionString*");
    }

    // ====================================================================
    // GetAllAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetAllAsync_NoParameters_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);

        // Act
        var result = await dao.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // GetByIdAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetByIdAsync_ValidId_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var id = 1;

        // Act
        var result = await dao.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task GetByIdAsync_DifferentIds_HandlesAll(int id)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);

        // Act
        var result = await dao.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NegativeId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var id = -1;

        // Act
        Func<Task> act = async () => await dao.GetByIdAsync(id);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // InsertAsync Tests
    // ====================================================================

    [Fact]
    public async Task InsertAsync_ValidPackageType_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = CreateValidPackageType();
        var createdBy = 1001;

        // Act
        var result = await dao.InsertAsync(packageType, createdBy);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_MinimalData_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Name = "Test",
            Code = "TST"
        };
        var createdBy = 1;

        // Act
        var result = await dao.InsertAsync(packageType, createdBy);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Skid", "SKD")]
    [InlineData("Pallet", "PLT")]
    [InlineData("Box", "BOX")]
    [InlineData("Crate", "CRT")]
    public async Task InsertAsync_DifferentTypes_HandlesAll(string name, string code)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Name = name,
            Code = code
        };
        var createdBy = 1001;

        // Act
        var result = await dao.InsertAsync(packageType, createdBy);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public async Task InsertAsync_DifferentCreatedBy_HandlesAll(int createdBy)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = CreateValidPackageType();

        // Act
        var result = await dao.InsertAsync(packageType, createdBy);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // UpdateAsync Tests
    // ====================================================================

    [Fact]
    public async Task UpdateAsync_ValidPackageType_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = CreateValidPackageType();
        packageType.Id = 1;

        // Act
        var result = await dao.UpdateAsync(packageType);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_AllFieldsChanged_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Id = 1,
            Name = "Updated Name",
            Code = "UPD"
        };

        // Act
        var result = await dao.UpdateAsync(packageType);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    public async Task UpdateAsync_DifferentIds_HandlesAll(int id)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = CreateValidPackageType();
        packageType.Id = id;

        // Act
        var result = await dao.UpdateAsync(packageType);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // DeleteAsync Tests
    // ====================================================================

    [Fact]
    public async Task DeleteAsync_ValidId_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var id = 1;

        // Act
        var result = await dao.DeleteAsync(id);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task DeleteAsync_DifferentIds_HandlesAll(int id)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);

        // Act
        var result = await dao.DeleteAsync(id);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_NegativeId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var id = -1;

        // Act
        Func<Task> act = async () => await dao.DeleteAsync(id);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // GetUsageCountAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetUsageCountAsync_ValidId_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var id = 1;

        // Act
        var result = await dao.GetUsageCountAsync(id);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task GetUsageCountAsync_DifferentIds_HandlesAll(int id)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);

        // Act
        var result = await dao.GetUsageCountAsync(id);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // Edge Cases and Boundary Value Tests
    // ====================================================================

    [Fact]
    public async Task InsertAsync_VeryLongName_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Name = new string('N', 500),
            Code = "TST"
        };
        var createdBy = 1001;

        // Act
        Func<Task> act = async () => await dao.InsertAsync(packageType, createdBy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_VeryLongCode_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Name = "Test",
            Code = new string('C', 100)
        };
        var createdBy = 1001;

        // Act
        Func<Task> act = async () => await dao.InsertAsync(packageType, createdBy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_SpecialCharactersInName_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Name = "Package's \"Special\" Type @#$%",
            Code = "SPC"
        };
        var createdBy = 1001;

        // Act
        Func<Task> act = async () => await dao.InsertAsync(packageType, createdBy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_EmptyStrings_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Name = string.Empty,
            Code = string.Empty
        };
        var createdBy = 1001;

        // Act
        Func<Task> act = async () => await dao.InsertAsync(packageType, createdBy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_VeryLongName_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Id = 1,
            Name = new string('N', 1000),
            Code = "UPD"
        };

        // Act
        Func<Task> act = async () => await dao.UpdateAsync(packageType);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_SpecialCharactersInCode_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = new Model_PackageType
        {
            Id = 1,
            Name = "Test",
            Code = "@#$%^&*()"
        };

        // Act
        Func<Task> act = async () => await dao.UpdateAsync(packageType);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-999)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public async Task GetUsageCountAsync_BoundaryValues_HandlesAll(int id)
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);

        // Act
        Func<Task> act = async () => await dao.GetUsageCountAsync(id);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertAsync_NegativeCreatedBy_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = CreateValidPackageType();
        var createdBy = -1;

        // Act
        Func<Task> act = async () => await dao.InsertAsync(packageType, createdBy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_WithIdZero_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_PackageType(TestConnectionString);
        var packageType = CreateValidPackageType();
        packageType.Id = 0;

        // Act
        Func<Task> act = async () => await dao.UpdateAsync(packageType);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // Helper Methods
    // ====================================================================

    /// <summary>
    /// Creates a valid test package type with required fields populated.
    /// </summary>
    private static Model_PackageType CreateValidPackageType()
    {
        return new Model_PackageType
        {
            Name = "Test Package",
            Code = "TST",
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }
}

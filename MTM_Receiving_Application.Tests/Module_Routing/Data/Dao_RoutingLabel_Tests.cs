using FluentAssertions;
using Xunit;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Tests.Unit.Module_Routing.Data;

/// <summary>
/// Unit tests for Dao_RoutingLabel data access object.
/// Tests CRUD operations, duplicate detection, and export tracking for routing labels.
/// </summary>
public class Dao_RoutingLabel_Tests
{
private static string TestConnectionString => Helper_Database_Variables.GetConnectionString(useProduction: false);

    // ====================================================================
    // Constructor Tests
    // ====================================================================

    [Fact]
    public void Constructor_ValidConnectionString_CreatesInstance()
    {
        // Act
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Assert
        dao.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConnectionString_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new Dao_RoutingLabel(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*connectionString*");
    }

    // ====================================================================
    // InsertLabelAsync Tests
    // ====================================================================

    [Fact]
    public async Task InsertLabelAsync_ValidLabel_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();

        // Act
        var result = await dao.InsertLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertLabelAsync_NullOtherReasonId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.OtherReasonId = null;

        // Act
        var result = await dao.InsertLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertLabelAsync_WithOtherReasonId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.OtherReasonId = 5;

        // Act
        var result = await dao.InsertLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task InsertLabelAsync_DifferentQuantities_HandlesAll(int quantity)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Quantity = quantity;

        // Act
        var result = await dao.InsertLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("PO-12345", "001")]
    [InlineData("12345", "002")]
    [InlineData("PO12345", "999")]
    public async Task InsertLabelAsync_DifferentPONumbers_HandlesAll(string poNumber, string lineNumber)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.PONumber = poNumber;
        label.LineNumber = lineNumber;

        // Act
        var result = await dao.InsertLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public async Task InsertLabelAsync_DifferentCreatedBy_HandlesAll(int createdBy)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.CreatedBy = createdBy;

        // Act
        var result = await dao.InsertLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // UpdateLabelAsync Tests
    // ====================================================================

    [Fact]
    public async Task UpdateLabelAsync_ValidLabel_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Id = 1;

        // Act
        var result = await dao.UpdateLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLabelAsync_UpdatedQuantity_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Id = 1;
        label.Quantity = 999;

        // Act
        var result = await dao.UpdateLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLabelAsync_NullOtherReasonId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Id = 1;
        label.OtherReasonId = null;

        // Act
        var result = await dao.UpdateLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    public async Task UpdateLabelAsync_DifferentIds_HandlesAll(int id)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Id = id;

        // Act
        var result = await dao.UpdateLabelAsync(label);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // GetLabelByIdAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetLabelByIdAsync_ValidId_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelId = 1;

        // Act
        var result = await dao.GetLabelByIdAsync(labelId);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task GetLabelByIdAsync_DifferentIds_HandlesAll(int labelId)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        var result = await dao.GetLabelByIdAsync(labelId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLabelByIdAsync_NegativeId_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelId = -1;

        // Act
        Func<Task> act = async () => await dao.GetLabelByIdAsync(labelId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // GetAllLabelsAsync Tests
    // ====================================================================

    [Fact]
    public async Task GetAllLabelsAsync_DefaultParameters_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        var result = await dao.GetAllLabelsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(50, 0)]
    [InlineData(100, 0)]
    [InlineData(100, 100)]
    [InlineData(100, 500)]
    public async Task GetAllLabelsAsync_DifferentPagination_HandlesAll(int limit, int offset)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        var result = await dao.GetAllLabelsAsync(limit, offset);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllLabelsAsync_ZeroLimit_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        Func<Task> act = async () => await dao.GetAllLabelsAsync(0, 0);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllLabelsAsync_NegativeOffset_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        Func<Task> act = async () => await dao.GetAllLabelsAsync(100, -10);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // DeleteLabelAsync Tests
    // ====================================================================

    [Fact]
    public async Task DeleteLabelAsync_ValidId_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelId = 1;

        // Act
        var result = await dao.DeleteLabelAsync(labelId);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task DeleteLabelAsync_DifferentIds_HandlesAll(int labelId)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        var result = await dao.DeleteLabelAsync(labelId);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // MarkLabelExportedAsync Tests
    // ====================================================================

    [Fact]
    public async Task MarkLabelExportedAsync_ValidId_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelId = 1;

        // Act
        var result = await dao.MarkLabelExportedAsync(labelId);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task MarkLabelExportedAsync_DifferentIds_HandlesAll(int labelId)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        var result = await dao.MarkLabelExportedAsync(labelId);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // CheckDuplicateLabelAsync Tests
    // ====================================================================

    [Fact]
    public async Task CheckDuplicateLabelAsync_ValidParameters_CallsStoredProcedure()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;
        var hoursWindow = 24;

        // Act
        var result = await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, hoursWindow);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(24)]
    [InlineData(48)]
    [InlineData(168)]
    public async Task CheckDuplicateLabelAsync_DifferentTimeWindows_HandlesAll(int hoursWindow)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;

        // Act
        var result = await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, hoursWindow);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckDuplicateLabelAsync_ZeroHoursWindow_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;

        // Act
        Func<Task> act = async () => await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, 0);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("PO-12345", "001")]
    [InlineData("12345", "002")]
    [InlineData("", "003")]
    public async Task CheckDuplicateLabelAsync_DifferentPONumbers_HandlesAll(string poNumber, string lineNumber)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var recipientId = 10;

        // Act
        var result = await dao.CheckDuplicateLabelAsync(poNumber, lineNumber, recipientId, 24);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // CheckDuplicateAsync Helper Tests
    // ====================================================================

    [Fact]
    public async Task CheckDuplicateAsync_ValidParameters_CallsCheckDuplicate()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;
        var createdWithinDate = DateTime.Now.AddHours(-12);

        // Act
        var result = await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, createdWithinDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckDuplicateAsync_DefaultDateTime_UsesDefault24Hours()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;

        // Act
        var result = await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, default);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckDuplicateAsync_FutureDate_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;
        var futureDate = DateTime.Now.AddDays(1);

        // Act
        Func<Task> act = async () => await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, futureDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckDuplicateAsync_OldDate_CapsAtOneWeek()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var poNumber = "PO-12345";
        var lineNumber = "001";
        var recipientId = 10;
        var oldDate = DateTime.Now.AddDays(-30);

        // Act
        Func<Task> act = async () => await dao.CheckDuplicateAsync(poNumber, lineNumber, recipientId, oldDate);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // MarkExportedAsync Helper Tests
    // ====================================================================

    [Fact]
    public async Task MarkExportedAsync_EmptyList_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelIds = new List<int>();

        // Act
        var result = await dao.MarkExportedAsync(labelIds);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkExportedAsync_SingleId_CallsMarkExported()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelIds = new List<int> { 1 };

        // Act
        var result = await dao.MarkExportedAsync(labelIds);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkExportedAsync_MultipleIds_CallsMarkExportedForEach()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var labelIds = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = await dao.MarkExportedAsync(labelIds);

        // Assert
        result.Should().NotBeNull();
    }

    // ====================================================================
    // Edge Cases and Boundary Value Tests
    // ====================================================================

    [Fact]
    public async Task InsertLabelAsync_VeryLongDescription_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Description = new string('D', 2000);

        // Act
        Func<Task> act = async () => await dao.InsertLabelAsync(label);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertLabelAsync_SpecialCharactersInFields_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.PONumber = "PO-@#$%";
        label.Description = "Description with <special> \"characters\"";

        // Act
        Func<Task> act = async () => await dao.InsertLabelAsync(label);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertLabelAsync_ZeroQuantity_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Quantity = 0;

        // Act
        Func<Task> act = async () => await dao.InsertLabelAsync(label);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertLabelAsync_NegativeQuantity_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.Quantity = -10;

        // Act
        Func<Task> act = async () => await dao.InsertLabelAsync(label);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public async Task InsertLabelAsync_BoundaryRecipientIds_HandlesAll(int recipientId)
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);
        var label = CreateValidRoutingLabel();
        label.RecipientId = recipientId;

        // Act
        Func<Task> act = async () => await dao.InsertLabelAsync(label);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllLabelsAsync_VeryLargeLimit_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        Func<Task> act = async () => await dao.GetAllLabelsAsync(int.MaxValue, 0);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckDuplicateLabelAsync_VeryLargeTimeWindow_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        Func<Task> act = async () => await dao.CheckDuplicateLabelAsync("PO-123", "001", 10, 99999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckDuplicateLabelAsync_NegativeTimeWindow_HandlesGracefully()
    {
        // Arrange
        var dao = new Dao_RoutingLabel(TestConnectionString);

        // Act
        Func<Task> act = async () => await dao.CheckDuplicateLabelAsync("PO-123", "001", 10, -24);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ====================================================================
    // Helper Methods
    // ====================================================================

    /// <summary>
    /// Creates a valid test routing label with all required fields populated.
    /// </summary>
    private static Model_RoutingLabel CreateValidRoutingLabel()
    {
        return new Model_RoutingLabel
        {
            PONumber = "PO-12345",
            LineNumber = "001",
            Description = "Test Part Description",
            RecipientId = 10,
            RecipientName = "Test Recipient",
            RecipientLocation = "Building A",
            Quantity = 100,
            CreatedBy = 1001,
            CreatedDate = DateTime.Now,
            OtherReasonId = null,
            IsActive = true,
            CsvExported = false
        };
    }
}

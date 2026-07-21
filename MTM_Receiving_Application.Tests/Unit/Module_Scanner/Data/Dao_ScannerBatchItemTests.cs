using System.Data;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Data;

public sealed class Dao_ScannerBatchItemTests
{
    [Fact]
    public async Task UpsertItemAsync_ShouldFail_WhenQuantityIsInvalid()
    {
        var dao = CreateDao();

        var result = await dao.UpsertItemAsync(new Model_ScannerBatchItem
        {
            SessionId = Guid.NewGuid(),
            SequenceNumber = 1,
            PayloadPartId = "MMC0000850",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B-01",
            PayloadQuantity = "BAD",
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Quantity is invalid.");
    }

    [Fact]
    public void MapItem_ShouldMapPersistedTransferFields()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(long));
        table.Columns.Add("session_id", typeof(string));
        table.Columns.Add("item_order", typeof(int));
        table.Columns.Add("part_id", typeof(string));
        table.Columns.Add("from_warehouse_id", typeof(string));
        table.Columns.Add("from_location_id", typeof(string));
        table.Columns.Add("to_warehouse_id", typeof(string));
        table.Columns.Add("to_location_id", typeof(string));
        table.Columns.Add("quantity", typeof(decimal));
        table.Columns.Add("validation_state", typeof(string));
        table.Columns.Add("validation_notes", typeof(string));
        table.Columns.Add("status", typeof(string));
        table.Columns.Add("failure_code", typeof(string));
        table.Columns.Add("failure_message", typeof(string));
        table.Columns.Add("updated_at", typeof(DateTime));

        var sessionId = Guid.NewGuid();
        table.Rows.Add(
            14L,
            sessionId.ToString(),
            2,
            "MMC0000850",
            "002",
            "A-01",
            "002",
            "B-01",
            25.75m,
            "Valid",
            "Match found",
            "Sent",
            DBNull.Value,
            DBNull.Value,
            DateTime.UtcNow
        );

        using var reader = table.CreateDataReader();
        reader.Read();

        var mapMethod = typeof(Dao_ScannerBatchItem).GetMethod(
            "MapItem",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        mapMethod.Should().NotBeNull();
        var mapped = mapMethod!.Invoke(null, [reader]);
        var model = mapped.Should().BeOfType<Model_ScannerBatchItem>().Subject;

        model.SessionItemId.Should().Be(14L);
        model.SessionId.Should().Be(sessionId);
        model.SequenceNumber.Should().Be(2);
        model.PayloadPartId.Should().Be("MMC0000850");
        model.PayloadFromWarehouse.Should().Be("002");
        model.PayloadFromLocation.Should().Be("A-01");
        model.PayloadToWarehouse.Should().Be("002");
        model.PayloadToLocation.Should().Be("B-01");
        model.PayloadQuantity.Should().Be("25.75");
        model.ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
        model.ExecutionState.Should().Be(Enum_ScannerExecutionState.Sent);
    }

    private static Dao_ScannerBatchItem CreateDao()
    {
        return new Dao_ScannerBatchItem("Server=localhost;Database=test;Uid=test;Pwd=test;");
    }
}

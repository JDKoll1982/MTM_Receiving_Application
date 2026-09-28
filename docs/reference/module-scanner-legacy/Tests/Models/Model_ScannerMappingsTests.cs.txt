using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Models;

public sealed class Model_ScannerMappingsTests
{
    [Fact]
    public void ToRunItem_ShouldMapPayloadFieldsToRunItem()
    {
        var item = new Model_ScannerBatchItem
        {
            SessionId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            SequenceNumber = 3,
            PayloadPartId = "MMCCS00740",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B-01",
            PayloadQuantity = "125",
            ValidationState = Enum_ScannerValidationState.Valid,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };

        var runItem = item.ToRunItem(Guid.NewGuid());

        runItem.SessionId.Should().Be(item.SessionId);
        runItem.ItemId.Should().Be(item.ItemId);
        runItem.SequenceNumber.Should().Be(3);
        runItem.PartId.Should().Be("MMCCS00740");
        runItem.FromWarehouse.Should().Be("002");
        runItem.FromLocation.Should().Be("A-01");
        runItem.ToWarehouse.Should().Be("002");
        runItem.ToLocation.Should().Be("B-01");
        runItem.Quantity.Should().Be("125");
        runItem.ValidationState.Should().Be(Enum_ScannerValidationState.Valid);
    }

    [Fact]
    public void RecalculateItemCounters_ShouldReflectExecutionStates()
    {
        var session = new Model_ScannerBatchSession();
        session.Items.Add(new Model_ScannerBatchItem { ExecutionState = Enum_ScannerExecutionState.Sent });
        session.Items.Add(new Model_ScannerBatchItem { ExecutionState = Enum_ScannerExecutionState.Failed });
        session.Items.Add(new Model_ScannerBatchItem { ExecutionState = Enum_ScannerExecutionState.Waiting });
        session.Items.Add(new Model_ScannerBatchItem { ExecutionState = Enum_ScannerExecutionState.Waiting });

        session.RecalculateItemCounters();

        session.TotalItems.Should().Be(4);
        session.SentItems.Should().Be(1);
        session.FailedItems.Should().Be(1);
        session.WaitingItems.Should().Be(2);
    }

    [Fact]
    public void ToRunSnapshot_ShouldProjectOrderedItemsAndSummaryCounters()
    {
        var session = new Model_ScannerBatchSession
        {
            OwnerUserId = "user-1",
            OwnerDisplayName = "Operator A",
            ActiveProfileId = Guid.NewGuid(),
            Status = Enum_ScannerSessionStatus.Running,
            LastSendStartedUtc = DateTime.UtcNow,
        };

        session.Items.Add(new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 2,
            PayloadPartId = "PART-B",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A-02",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B-02",
            PayloadQuantity = "1",
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        });

        session.Items.Add(new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = session.SessionId,
            SequenceNumber = 1,
            PayloadPartId = "PART-A",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "B-01",
            PayloadQuantity = "1",
            ExecutionState = Enum_ScannerExecutionState.Sent,
            SentUtc = DateTime.UtcNow,
        });

        var snapshot = session.ToRunSnapshot();

        snapshot.TotalItems.Should().Be(2);
        snapshot.SentItems.Should().Be(1);
        snapshot.WaitingItems.Should().Be(1);
        snapshot.Items.Should().HaveCount(2);
        snapshot.Items[0].PartId.Should().Be("PART-A");
        snapshot.Items[1].PartId.Should().Be("PART-B");
    }
}

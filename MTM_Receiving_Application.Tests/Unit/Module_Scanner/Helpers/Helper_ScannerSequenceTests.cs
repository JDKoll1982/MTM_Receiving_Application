using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Helpers;

public sealed class Helper_ScannerSequenceTests
{
    private static Model_ScannerBatchItem CreateItem()
    {
        return new Model_ScannerBatchItem
        {
            PayloadPartId = "MMCCS00740",
            PayloadQuantity = "5",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
            PayloadToLocation = "R-05",
        };
    }

    [Fact]
    public void BuildFieldValues_ShouldEmitSixOrderedFields()
    {
        var values = Helper_ScannerSequence.BuildFieldValues(CreateItem());

        values.Should().ContainInOrder(
            "MMCCS00740",
            "5",
            "002",
            "V-A1-01",
            "002",
            "R-05"
        );
    }

    [Fact]
    public void BuildFieldSequence_ShouldEndWithNoTrailingTab()
    {
        var sequence = Helper_ScannerSequence.BuildFieldSequence(CreateItem());

        sequence.Should().HaveCount(6);
        sequence[^1].TabsAfter.Should().Be(0);
        sequence[0].Value.Should().Be("MMCCS00740");
    }

    [Fact]
    public void IsEligibleForSend_ShouldBeTrue_WhenWaitingAndValid()
    {
        var item = CreateItem();
        item.ExecutionState = Enum_ScannerExecutionState.Waiting;
        item.ValidationState = Enum_ScannerValidationState.Valid;

        Helper_ScannerSequence.IsEligibleForSend(item).Should().BeTrue();
    }

    [Theory]
    [InlineData(Enum_ScannerExecutionState.Sent)]
    [InlineData(Enum_ScannerExecutionState.Failed)]
    [InlineData(Enum_ScannerExecutionState.Sending)]
    public void IsEligibleForSend_ShouldBeFalse_WhenNotWaiting(Enum_ScannerExecutionState state)
    {
        var item = CreateItem();
        item.ExecutionState = state;
        item.ValidationState = Enum_ScannerValidationState.Valid;

        Helper_ScannerSequence.IsEligibleForSend(item).Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForSend_ShouldBeFalse_WhenInvalid()
    {
        var item = CreateItem();
        item.ExecutionState = Enum_ScannerExecutionState.Waiting;
        item.ValidationState = Enum_ScannerValidationState.Invalid;

        Helper_ScannerSequence.IsEligibleForSend(item).Should().BeFalse();
    }

    [Fact]
    public void FindNextEligible_ShouldReturnLowestSequenceWaitingValidItem()
    {
        var skipped = CreateItem();
        skipped.SequenceNumber = 1;
        skipped.ValidationState = Enum_ScannerValidationState.Invalid;

        var ready = CreateItem();
        ready.SequenceNumber = 2;
        ready.ValidationState = Enum_ScannerValidationState.Valid;

        var next = Helper_ScannerSequence.FindNextEligible(new[] { skipped, ready });

        next.Should().BeSameAs(ready);
    }

    [Fact]
    public void FindNextEligible_ShouldReturnNull_WhenNoneReady()
    {
        var item = CreateItem();
        item.ValidationState = Enum_ScannerValidationState.Invalid;

        Helper_ScannerSequence.FindNextEligible(new[] { item }).Should().BeNull();
    }

    [Theory]
    [InlineData("VMINVENT", "VMINVENT.exe", true)]
    [InlineData("vminvent", "VMINVENT.exe", true)]
    [InlineData("explorer", "VMINVENT.exe", false)]
    [InlineData(null, "VMINVENT.exe", false)]
    public void IsTargetProcess_ShouldMatchByName(string? foreground, string target, bool expected)
    {
        Helper_ScannerSequence.IsTargetProcess(foreground, target).Should().Be(expected);
    }

    [Fact]
    public void IsTargetTitle_ShouldRequireExactMatch_WhenRequired()
    {
        Helper_ScannerSequence.IsTargetTitle("Inventory Transfers", "Inventory Transfers", true)
            .Should().BeTrue();
        Helper_ScannerSequence.IsTargetTitle("Inventory Transfers - 002", "Inventory Transfers", true)
            .Should().BeFalse();
        Helper_ScannerSequence.IsTargetTitle("Inventory Transfers - 002", "Inventory Transfers", false)
            .Should().BeTrue();
    }
}

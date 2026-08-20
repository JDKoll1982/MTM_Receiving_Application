using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Helpers;

public sealed class Helper_ScannerSequenceTests
{
    [Fact]
    public void BuildFieldValues_ShouldReturnFieldsInScreenOrder()
    {
        var item = new Model_ScannerBatchItem
        {
            PayloadPartId = "ABC-123",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A1",
            PayloadToWarehouse = "003",
            PayloadToLocation = "B2",
            PayloadQuantity = "5",
        };

        var values = Helper_ScannerSequence.BuildFieldValues(item);

        // VMINVENT "Inventory Transfers" tab path: part id, quantity, from warehouse,
        // from location, to warehouse, to location.
        values.Should().Equal("ABC-123", "5", "002", "A1", "003", "B2");
    }

    [Fact]
    public void BuildFieldSequence_ShouldReturnTabCountsBetweenFields()
    {
        var item = new Model_ScannerBatchItem
        {
            PayloadPartId = "ABC-123",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "A1",
            PayloadToWarehouse = "003",
            PayloadToLocation = "B2",
            PayloadQuantity = "5",
        };

        var sequence = Helper_ScannerSequence.BuildFieldSequence(item);

        sequence.Should().Equal(
            ("ABC-123", 1),
            ("5", 2),
            ("002", 1),
            ("A1", 5),
            ("003", 1),
            ("B2", 0)
        );
    }

    [Theory]
    [InlineData(Enum_ScannerExecutionState.Waiting, Enum_ScannerValidationState.Valid, true)]
    [InlineData(Enum_ScannerExecutionState.Waiting, Enum_ScannerValidationState.NotValidated, false)]
    [InlineData(Enum_ScannerExecutionState.Waiting, Enum_ScannerValidationState.Invalid, false)]
    [InlineData(Enum_ScannerExecutionState.Sent, Enum_ScannerValidationState.Valid, false)]
    [InlineData(Enum_ScannerExecutionState.Failed, Enum_ScannerValidationState.Valid, false)]
    public void IsEligibleForSend_ShouldMatch_WhenStateCombination(Enum_ScannerExecutionState execution, Enum_ScannerValidationState validation, bool expected)
    {
        var item = new Model_ScannerBatchItem
        {
            ExecutionState = execution,
            ValidationState = validation,
        };

        Helper_ScannerSequence.IsEligibleForSend(item).Should().Be(expected);
    }

    [Fact]
    public void FindNextEligible_ShouldReturnFirstBySequenceNumber()
    {
        var items = new[]
        {
            new Model_ScannerBatchItem { SequenceNumber = 1, ExecutionState = Enum_ScannerExecutionState.Sent, ValidationState = Enum_ScannerValidationState.Valid },
            new Model_ScannerBatchItem { SequenceNumber = 2, ExecutionState = Enum_ScannerExecutionState.Waiting, ValidationState = Enum_ScannerValidationState.Valid },
            new Model_ScannerBatchItem { SequenceNumber = 3, ExecutionState = Enum_ScannerExecutionState.Waiting, ValidationState = Enum_ScannerValidationState.Invalid },
            new Model_ScannerBatchItem { SequenceNumber = 4, ExecutionState = Enum_ScannerExecutionState.Waiting, ValidationState = Enum_ScannerValidationState.Valid },
        };

        var next = Helper_ScannerSequence.FindNextEligible(items);

        next.Should().NotBeNull();
        next!.SequenceNumber.Should().Be(2);
    }

    [Fact]
    public void FindNextEligible_ShouldReturnNull_WhenNoneAreReady()
    {
        var items = new[]
        {
            new Model_ScannerBatchItem { ExecutionState = Enum_ScannerExecutionState.Sent, ValidationState = Enum_ScannerValidationState.Valid },
            new Model_ScannerBatchItem { ExecutionState = Enum_ScannerExecutionState.Waiting, ValidationState = Enum_ScannerValidationState.NotValidated },
        };

        Helper_ScannerSequence.FindNextEligible(items).Should().BeNull();
    }

    [Theory]
    [InlineData("VMINVENT.exe", "vminvent")]
    [InlineData("VMINVENT", "vminvent")]
    [InlineData("vminvent.EXE", "vminvent")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void NormalizeProcessName_ShouldStripExeAndLowercase(string? input, string expected)
    {
        Helper_ScannerSequence.NormalizeProcessName(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("vminvent", "VMINVENT.exe", true)]
    [InlineData("vminvent", "VMINVENT", true)]
    [InlineData("notepad", "VMINVENT.exe", false)]
    [InlineData("vminvent", null, true)] // No target configured means no process check
    [InlineData("vminvent", "", true)]
    public void IsTargetProcess_ShouldMatch_WhenProcessNameAligns(string foreground, string? target, bool expected)
    {
        Helper_ScannerSequence.IsTargetProcess(foreground, target).Should().Be(expected);
    }

    [Theory]
    [InlineData("Inventory Transfers", "Inventory Transfers", true, true)]
    [InlineData("Inventory Transfers - XYZ", "Inventory Transfers", false, true)]
    [InlineData("Inventory Transfers - XYZ", "Inventory Transfers", true, false)]
    [InlineData("", "Inventory Transfers", false, false)]
    [InlineData("Anything", null, false, true)] // No title configured means no title check
    public void IsTargetTitle_ShouldRespectExactFlag(
        string? foreground,
        string? expected,
        bool requireExact,
        bool expectedResult
    )
    {
        Helper_ScannerSequence
            .IsTargetTitle(foreground, expected, requireExact)
            .Should()
            .Be(expectedResult);
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Models;

/// <summary>
/// Covers the change notifications the Workbench Status column depends on. The status button is
/// bound with <c>Mode=OneWay</c>, so the row must raise <c>StatusIsValid</c> and <c>StatusText</c>
/// after a To / Qty edit and after validation applies its result — otherwise the icon and its
/// tooltip keep showing the old state.
/// </summary>
public sealed class Model_ScannerBatchItemTests
{
    private static (Model_ScannerBatchItem Item, List<string> Raised) CreateItemWithRecorder()
    {
        var item = new Model_ScannerBatchItem
        {
            PayloadPartId = "MMC0000650",
            PayloadFromWarehouse = "002",
            PayloadFromLocation = "V-A1-01",
            PayloadToWarehouse = "002",
        };

        var raised = new List<string>();
        ((INotifyPropertyChanged)item).PropertyChanged += (_, args) =>
            raised.Add(args.PropertyName ?? string.Empty);

        return (item, raised);
    }

    [Fact]
    public void ApplyValidationResult_ShouldRaiseStatusProperties_WhenRowBecomesValid()
    {
        var (item, raised) = CreateItemWithRecorder();
        item.PayloadToLocation = "V-A0-01";
        item.PayloadQuantity = "25";
        raised.Clear();

        item.ApplyValidationResult(
            new Model_ScannerItemValidationResult
            {
                State = Enum_ScannerValidationState.Valid,
                Message = "Validation passed.",
            }
        );

        raised.Should().Contain(nameof(Model_ScannerBatchItem.StatusIsValid));
        raised.Should().Contain(nameof(Model_ScannerBatchItem.StatusText));
        item.StatusIsValid.Should().BeTrue();
        item.StatusText.Should().Be("✅ Ready!");
    }

    [Fact]
    public void ApplyValidationResult_ShouldRaiseStatusIsValid_WhenRowRevertsToInvalid()
    {
        var (item, raised) = CreateItemWithRecorder();
        item.PayloadToLocation = "V-A0-01";
        item.ApplyValidationResult(
            new Model_ScannerItemValidationResult { State = Enum_ScannerValidationState.Valid }
        );
        raised.Clear();

        item.ApplyValidationResult(
            new Model_ScannerItemValidationResult
            {
                State = Enum_ScannerValidationState.Invalid,
                Message = "Quantity must be a positive number.",
            }
        );

        raised.Should().Contain(nameof(Model_ScannerBatchItem.StatusIsValid));
        item.StatusIsValid.Should().BeFalse();

        // No friendly mapping covers this wording, so the validator's own message is shown.
        item.StatusText.Should().Be("! Quantity must be a positive number.");
    }

    [Fact]
    public void PayloadToLocation_ShouldRaiseStatusText_WhenDestinationIsCleared()
    {
        var (item, raised) = CreateItemWithRecorder();
        item.PayloadToLocation = "V-A0-01";
        raised.Clear();

        item.PayloadToLocation = string.Empty;

        raised.Should().Contain(nameof(Model_ScannerBatchItem.StatusText));
        item.StatusText.Should().Be("⏳ Enter destination");
    }

    [Fact]
    public void PayloadQuantity_ShouldRaiseStatusText_WhenQuantityExceedsOnHand()
    {
        var (item, raised) = CreateItemWithRecorder();
        item.PayloadToLocation = "V-A0-01";
        item.MaxQuantity = 25m;
        item.ValidationState = Enum_ScannerValidationState.Invalid;
        raised.Clear();

        item.PayloadQuantity = "30";

        raised.Should().Contain(nameof(Model_ScannerBatchItem.StatusText));
        item.StatusText.Should().Be("! Qty too High");
    }

    [Fact]
    public void PayloadQuantity_ShouldRaiseStatusText_WhenQuantityIsNotANumber()
    {
        var (item, raised) = CreateItemWithRecorder();
        item.PayloadToLocation = "V-A0-01";
        item.MaxQuantity = 25m;
        item.ValidationState = Enum_ScannerValidationState.Invalid;
        raised.Clear();

        // A committed but unresolved expression reaches the row as text; the status must say so
        // instead of keeping the previous quantity message.
        item.PayloadQuantity = "5+";

        raised.Should().Contain(nameof(Model_ScannerBatchItem.StatusText));
        item.StatusText.Should().Be("! Qty must be a number");
    }
}

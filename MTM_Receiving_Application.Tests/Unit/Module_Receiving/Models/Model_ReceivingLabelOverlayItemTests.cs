using System;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models;

/// <summary>
/// Covers the row-level parts of the Label Overlay lists: which columns a row shows, whether a date falls
/// inside a tab's range, and that the cut-out tab works on copies so its ticks cannot arm an overlay tag.
/// </summary>
public sealed class Model_ReceivingLabelOverlayItemTests
{
    private static Model_ReceivingLabelOverlayItem BuildRow() =>
        new()
        {
            PartId = "TEST-PART-1",
            Description = "TEST description",
            HeatLotNumber = "TEST-LOT-9",
            LabelNumber = 3,
            ReceivedDate = new DateTime(2026, 3, 10),
            ShapeCode = "⭐",
            ColorCode = "RED",
            MarkingText = "Red Star",
        };

    [Fact]
    public void SetVisibleColumns_ShouldHideShapeAndColor_WhenThoseColumnsAreTurnedOff()
    {
        var row = BuildRow();

        row.SetVisibleColumns([Model_Tool_LotMarkingColumnCatalog.Parts]);

        row.ShowPart.Should().BeTrue();
        row.ShowShape.Should().BeFalse();
        row.ShowColor.Should().BeFalse();
        row.ShowHeat.Should().BeFalse();
        row.ShowReceived.Should().BeFalse();
    }

    [Fact]
    public void SetVisibleColumns_ShouldShowEveryColumn_WhenNoColumnIsChosen()
    {
        var row = BuildRow();

        row.SetVisibleColumns([]);

        row.ShowPart.Should().BeTrue();
        row.ShowHeat.Should().BeTrue();
        row.ShowShape.Should().BeTrue();
        row.ShowColor.Should().BeTrue();
        row.ShowReceived.Should().BeTrue();
    }

    [Fact]
    public void ShowShape_ShouldBeFalse_WhenTheLotHasNoMarking()
    {
        var row = new Model_ReceivingLabelOverlayItem { PartId = "TEST-PART-2" };

        row.SetVisibleColumns(Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys);

        row.HasMarking.Should().BeFalse();
        row.ShowShape.Should().BeFalse();
        row.ShowColor.Should().BeFalse();
        row.ShowPart.Should().BeTrue();
    }

    [Fact]
    public void ShapeNameAndColorName_ShouldBeWords_WhenTheCodesAreFromThePalette()
    {
        var row = BuildRow();

        row.ShapeName.Should().NotBeNullOrWhiteSpace();
        row.ColorName.Should().NotBeNullOrWhiteSpace();
        row.ShapeName.Should().NotBe(row.ColorName);
    }

    [Fact]
    public void CopyForCutOut_ShouldCarryEverything_AndItsOwnTick()
    {
        var row = BuildRow();

        var copy = row.CopyForCutOut(isSelected: true);

        copy.SelectionKey.Should().Be(row.SelectionKey);
        copy.PartId.Should().Be(row.PartId);
        copy.HeatLotNumber.Should().Be(row.HeatLotNumber);
        copy.LabelNumber.Should().Be(row.LabelNumber);
        copy.ReceivedDate.Should().Be(row.ReceivedDate);
        copy.MarkingText.Should().Be(row.MarkingText);
        copy.IsSelected.Should().BeTrue();
        row.IsSelected.Should().BeFalse("the overlay tag keeps its own tick");
    }

    [Fact]
    public void SelectionKey_ShouldDiffer_WhenTwoLoadsShareALabelNumber()
    {
        var first = BuildRow();
        var second = new Model_ReceivingLabelOverlayItem
        {
            PartId = first.PartId,
            HeatLotNumber = "TEST-LOT-10",
            LabelNumber = first.LabelNumber,
        };

        first.SelectionKey.Should().NotBe(second.SelectionKey);
    }

    [Theory]
    [InlineData("2026-03-10", "2026-03-01", "2026-03-31", true)]
    [InlineData("2026-03-10", "2026-03-11", null, false)]
    [InlineData("2026-03-10", null, "2026-03-09", false)]
    [InlineData("2026-03-10", null, null, true)]
    public void IncludesDate_ShouldMatch_WhenTheDayIsInsideTheRange(
        string day,
        string? from,
        string? to,
        bool expected
    )
    {
        var filter = new Model_LabelOverlayListFilter(
            new Model_Tool_LotMarkingFilter(
                from is null ? null : DateTime.Parse(from),
                to is null ? null : DateTime.Parse(to),
                Enum_LotMarkingSortField.LastReceiptDate,
                IsDescending: true
            ),
            Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys
        );

        filter.IncludesDate(DateTime.Parse(day)).Should().Be(expected);
    }

    [Fact]
    public void IncludesDate_ShouldMatchNothing_WhenTheRangeRunsBackwards()
    {
        var filter = new Model_LabelOverlayListFilter(
            new Model_Tool_LotMarkingFilter(
                new DateTime(2026, 3, 31),
                new DateTime(2026, 3, 1),
                Enum_LotMarkingSortField.LastReceiptDate,
                IsDescending: true
            ),
            Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys
        );

        filter.IsBackwardsRange.Should().BeTrue();
        filter.IncludesDate(new DateTime(2026, 3, 10)).Should().BeFalse();
    }

    [Fact]
    public void Default_ShouldIncludeEveryColumnAndNoDateLimit()
    {
        var filter = Model_LabelOverlayListFilter.Default;

        filter.HasDateRange.Should().BeFalse();
        Model_Tool_LotMarkingColumnCatalog
            .All.Should()
            .OnlyContain(column => filter.IsColumnVisible(column.Key));
    }
}

using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Helpers;

public sealed class Helper_DunnageNoLabelPartsTests
{
    [Fact]
    public void Split_ShouldSendEveryLoadToLabel_WhenTheNoLabelListIsEmpty()
    {
        var loads = new[] { Load("TEST-A"), Load("TEST-B") };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(loads, []);

        needsLabel.Should().BeEquivalentTo(loads);
        noLabel.Should().BeEmpty();
    }

    [Fact]
    public void Split_ShouldSendOnlyMatchingLoadsToHistory_WhenTheListFlagsSomeParts()
    {
        var loads = new[] { Load("TEST-A"), Load("TEST-B"), Load("TEST-C") };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(loads, ["TEST-B"]);

        needsLabel.Select(load => load.PartId).Should().Equal("TEST-A", "TEST-C");
        noLabel.Select(load => load.PartId).Should().Equal("TEST-B");
    }

    [Fact]
    public void Split_ShouldMatchPartNumbers_WhenCaseAndPaddingDiffer()
    {
        var loads = new[] { Load("test-a"), Load(" TEST-B "), Load("TEST-C") };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(
            loads,
            [" Test-A", "test-b "]
        );

        needsLabel.Select(load => load.PartId).Should().Equal("TEST-C");
        noLabel.Select(load => load.PartId).Should().Equal("test-a", " TEST-B ");
    }

    [Fact]
    public void Split_ShouldNotFlagAnything_WhenTheListOnlyHoldsBlanks()
    {
        var loads = new[] { Load("TEST-A"), Load(string.Empty) };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(loads, ["  ", ""]);

        needsLabel.Should().HaveCount(2);
        noLabel.Should().BeEmpty();
    }

    [Fact]
    public void Split_ShouldReturnTwoEmptyLists_WhenThereAreNoLoads()
    {
        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split([], ["TEST-A"]);

        needsLabel.Should().BeEmpty();
        noLabel.Should().BeEmpty();
    }

    [Fact]
    public void Split_ShouldReturnTwoEmptyLists_WhenLoadsAreNull()
    {
        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(null, ["TEST-A"]);

        needsLabel.Should().BeEmpty();
        noLabel.Should().BeEmpty();
    }

    [Fact]
    public void Split_ShouldKeepEveryLoadNeedingALabel_WhenTheListIsNull()
    {
        var loads = new[] { Load("TEST-A"), Load("TEST-B") };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(loads, null);

        needsLabel.Should().HaveCount(2);
        noLabel.Should().BeEmpty();
    }

    [Fact]
    public void Split_ShouldSendEveryLoadToLabel_WhenNothingIsFlagged()
    {
        var loads = new[] { Load("TEST-A") };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(loads, ["TEST-Z"]);

        needsLabel.Should().ContainSingle().Which.PartId.Should().Be("TEST-A");
        noLabel.Should().BeEmpty();
    }

    [Fact]
    public void Split_ShouldSendEveryLoadToHistory_WhenAllPartsAreFlagged()
    {
        var loads = new[] { Load("TEST-A"), Load("TEST-B") };

        var (needsLabel, noLabel) = Helper_DunnageNoLabelParts.Split(
            loads,
            ["TEST-A", "TEST-B"]
        );

        needsLabel.Should().BeEmpty();
        noLabel.Should().HaveCount(2);
    }

    private static Model_DunnageLoad Load(string partId) => new() { PartId = partId };
}

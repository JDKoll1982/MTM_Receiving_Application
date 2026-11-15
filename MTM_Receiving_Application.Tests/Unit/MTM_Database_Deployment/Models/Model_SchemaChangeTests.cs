using FluentAssertions;
using MTM_Database_Deployment.Models;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Models;

/// <summary>
/// The removal and confirmation flags decide whether a deploy can reach the destructive branch,
/// so they are pinned here: anything additive must never require confirmation.
/// </summary>
public sealed class Model_SchemaChangeTests
{
    [Theory]
    [InlineData(Enum_ChangeKind.ExtraTable)]
    [InlineData(Enum_ChangeKind.ExtraColumn)]
    [InlineData(Enum_ChangeKind.ExtraIndex)]
    [InlineData(Enum_ChangeKind.ExtraObject)]
    public void IsRemoval_ShouldBeTrue_WhenChangeDeletesAnExistingObject(Enum_ChangeKind kind)
    {
        CreateChange(kind).IsRemoval.Should().BeTrue();
    }

    [Theory]
    [InlineData(Enum_ChangeKind.CreateTable)]
    [InlineData(Enum_ChangeKind.AddColumn)]
    [InlineData(Enum_ChangeKind.AlterColumn)]
    [InlineData(Enum_ChangeKind.AddIndex)]
    [InlineData(Enum_ChangeKind.AddForeignKey)]
    [InlineData(Enum_ChangeKind.UpdateView)]
    [InlineData(Enum_ChangeKind.UpdateProcedure)]
    [InlineData(Enum_ChangeKind.UpdateTrigger)]
    [InlineData(Enum_ChangeKind.RunSeed)]
    public void IsRemoval_ShouldBeFalse_WhenChangeOnlyAddsOrReplaces(Enum_ChangeKind kind)
    {
        CreateChange(kind).IsRemoval.Should().BeFalse();
    }

    [Fact]
    public void RequiresConfirmation_ShouldBeFalse_WhenChangeIsAddColumn()
    {
        CreateChange(Enum_ChangeKind.AddColumn).RequiresConfirmation.Should().BeFalse();
    }

    [Fact]
    public void RequiresConfirmation_ShouldBeTrue_WhenChangeIsRebuildTable()
    {
        CreateChange(Enum_ChangeKind.RebuildTable).RequiresConfirmation.Should().BeTrue();
    }

    [Fact]
    public void DisplayText_ShouldAppendDetail_WhenDetailIsPresent()
    {
        var change = new Model_SchemaChange
        {
            Kind = Enum_ChangeKind.AddColumn,
            Target = "dunnage_types.icon",
            Detail = "varchar(50)"
        };

        change.DisplayText.Should().Be("Add column: dunnage_types.icon - varchar(50)");
    }

    [Fact]
    public void DisplayText_ShouldOmitSeparator_WhenDetailIsBlank()
    {
        var change = new Model_SchemaChange
        {
            Kind = Enum_ChangeKind.ExtraTable,
            Target = "legacy_table"
        };

        change.DisplayText.Should().Be("Extra table: legacy_table");
    }

    private static Model_SchemaChange CreateChange(Enum_ChangeKind kind) =>
        new() { Kind = kind, Target = "sample" };
}

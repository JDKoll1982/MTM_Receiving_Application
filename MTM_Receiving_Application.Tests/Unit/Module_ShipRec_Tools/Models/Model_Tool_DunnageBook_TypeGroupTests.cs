using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Models;

public sealed class Model_Tool_DunnageBook_TypeGroupTests
{
    private static Model_Tool_DunnageBook_TypeGroup CreateGroupWithTwoEntries()
    {
        var group = new Model_Tool_DunnageBook_TypeGroup { TypeId = 1, TypeName = "Pallets" };
        group.Entries.Add(CreateEntry(group, "PLT-001"));
        group.Entries.Add(CreateEntry(group, "PLT-002"));
        return group;
    }

    private static Model_Tool_DunnageBook_Entry CreateEntry(
        Model_Tool_DunnageBook_TypeGroup group,
        string partId
    ) =>
        new(new Model_DunnagePart { PartId = partId, TypeId = 1, DunnageTypeName = "Pallets" })
        {
            Group = group,
        };

    [Fact]
    public void IsTypeSelected_ShouldPropagateToAllEntries()
    {
        var group = CreateGroupWithTwoEntries();

        group.IsTypeSelected = true;

        group.Entries.Should().OnlyContain(entry => entry.IsSelected);
    }

    [Fact]
    public void EntrySelectionChange_ShouldUpdateGroupIsTypeSelected()
    {
        var group = CreateGroupWithTwoEntries();

        group.Entries[0].IsSelected = true;

        group.IsTypeSelected.Should().BeFalse();

        group.Entries[1].IsSelected = true;

        group.IsTypeSelected.Should().BeTrue();
        group.SelectedCount.Should().Be(2);
    }
}

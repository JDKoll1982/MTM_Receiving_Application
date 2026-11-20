using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One grid group: the findings that share a part and a received date (FR-043).
/// The view binds its header to <see cref="Key"/> and folds the group shut through
/// <see cref="ToggleCommand"/>.
/// </summary>
public sealed class Model_ReconciliationFindingGroup
    : ObservableCollection<Model_ReconciliationFinding>
{
    /// <summary>Creates a group with its header text and its folded state.</summary>
    /// <param name="key">The header text, which is the part and its received date.</param>
    /// <param name="isCollapsed">True when the group's lines are hidden.</param>
    public Model_ReconciliationFindingGroup(string key, bool isCollapsed = false)
    {
        Key = key;
        IsCollapsed = isCollapsed;
    }

    /// <summary>The header text the grid shows above the group.</summary>
    public string Key { get; }

    /// <summary>True when the group is folded shut, so its lines are not listed.</summary>
    public bool IsCollapsed { get; }

    /// <summary>The command that folds this group open or shut.</summary>
    public ICommand? ToggleCommand { get; init; }

    /// <summary>The chevron the header draws for the current state.</summary>
    public string ToggleGlyph => IsCollapsed ? "\uE76C" : "\uE70D";

    /// <summary>The header's screen-reader name, which states what a click will do.</summary>
    public string ToggleAutomationName => IsCollapsed ? "Expand group" : "Collapse group";
}

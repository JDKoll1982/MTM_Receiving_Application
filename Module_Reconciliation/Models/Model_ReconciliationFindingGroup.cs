using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One grid group: the findings that share a part and a received date (FR-043).
/// The view binds its header to <see cref="Key"/>.
/// </summary>
public sealed class Model_ReconciliationFindingGroup
    : ObservableCollection<Model_ReconciliationFinding>
{
    /// <summary>Creates a group with its header text.</summary>
    /// <param name="key">The header text, which is the part and its received date.</param>
    public Model_ReconciliationFindingGroup(string key)
    {
        Key = key;
    }

    /// <summary>The header text the grid shows above the group.</summary>
    public string Key { get; }
}

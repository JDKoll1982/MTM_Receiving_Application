using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One collapsible location section on the Material Availability Board when the board is loaded
/// from a location range: the location header stays visible while its part cards expand or
/// collapse together.
/// </summary>
public partial class Model_Tool_MaterialAvailabilityLocationGroup : ObservableObject
{
    /// <summary>Location ID shown in the section header.</summary>
    public string LocationId { get; init; } = string.Empty;

    /// <summary>Parts found in this location, in board order.</summary>
    public ObservableCollection<Model_Tool_MaterialAvailabilityCard> Cards { get; } = [];

    /// <summary>
    /// True while the section is expanded. Bound two-way by the section's expander so the
    /// operator's expand/collapse choice survives a card being added or the board refreshing.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>Header caption, for example "V-D0-02  •  17 part(s)".</summary>
    public string LocationHeaderText => $"{LocationId}  •  {Cards.Count} part(s)";
}

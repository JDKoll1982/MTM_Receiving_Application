using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Runtime settings controlling which work-order detail fields are shown in the UI and print output.
/// </summary>
public sealed class Model_Tool_MaterialAvailabilityFieldSettings
{
    public HashSet<string> UiVisibleFieldIds { get; set; } =
        new(System.StringComparer.OrdinalIgnoreCase);

    public HashSet<string> PrintVisibleFieldIds { get; set; } =
        new(System.StringComparer.OrdinalIgnoreCase);

    public bool IsShowAllChipEnabled { get; set; } = true;
}

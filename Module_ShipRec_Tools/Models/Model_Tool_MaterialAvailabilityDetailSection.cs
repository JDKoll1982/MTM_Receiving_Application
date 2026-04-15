using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Groups work-order detail fields into one modal card section.
/// </summary>
public sealed class Model_Tool_MaterialAvailabilityDetailSection
{
    public string Title { get; set; } = string.Empty;

    public List<Model_Tool_MaterialAvailabilityDetailField> Fields { get; set; } = [];
}

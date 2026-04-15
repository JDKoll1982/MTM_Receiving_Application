namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One labeled work-order detail field presented inside the work-order details modal.
/// </summary>
public sealed class Model_Tool_MaterialAvailabilityDetailField
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool IsLogicOnly { get; set; }

    public bool IsVisibleInUi { get; set; }

    public bool IsVisibleInPrint { get; set; }
}

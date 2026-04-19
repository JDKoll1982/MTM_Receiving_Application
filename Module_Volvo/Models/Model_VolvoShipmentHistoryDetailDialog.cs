namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Prepared dialog state for the shipment history detail preview.
/// </summary>
public class Model_VolvoShipmentHistoryDetailDialog
{
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string DialogTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted detail text rendered in the dialog body.
    /// </summary>
    public string DetailText { get; set; } = string.Empty;
}

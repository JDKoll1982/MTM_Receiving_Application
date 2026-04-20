namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Prepared window state for the shipment history detail preview.
/// </summary>
public class Model_VolvoShipmentHistoryDetailDialog
{
    /// <summary>
    /// Gets or sets the window title.
    /// </summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shipment header shown in the window.
    /// </summary>
    public Model_VolvoShipment Shipment { get; set; } = new();

    /// <summary>
    /// Gets or sets the read-only shipment lines shown as cards.
    /// </summary>
    public System.Collections.Generic.List<Model_VolvoShipmentLine> Lines { get; set; } = new();
}

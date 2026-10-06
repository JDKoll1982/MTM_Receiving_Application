namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Raw read-only warehouse-location row returned from Infor Visual. Used when the application
/// needs the list of locations that exist in a range rather than the stock held at them.
/// </summary>
public class Model_InforVisualLocationRow
{
    public string LocationId { get; set; } = string.Empty;

    public string WarehouseCode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

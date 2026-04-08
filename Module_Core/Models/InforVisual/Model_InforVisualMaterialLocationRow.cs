namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Raw read-only current-stock row for one part/location combination returned from Infor Visual.
/// </summary>
public class Model_InforVisualMaterialLocationRow
{
    public string PartId { get; set; } = string.Empty;

    public string PartDescription { get; set; } = string.Empty;

    public string WarehouseCode { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal CommittedQuantity { get; set; }
}

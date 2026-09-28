namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Raw Infor Visual row returned for PO line binary/spec search candidates.
/// </summary>
public class Model_InforVisualPOLineSpecSearchRow
{
    public string PONumber { get; set; } = string.Empty;

    public int POLineNumber { get; set; }

    public string PartId { get; set; } = string.Empty;

    public string VendorId { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string VendorPartId { get; set; } = string.Empty;

    public decimal QtyOrdered { get; set; }

    public decimal TotalQtyReceived { get; set; }

    public string PoStatus { get; set; } = string.Empty;

    public string BinaryType { get; set; } = string.Empty;

    public string SpecText { get; set; } = string.Empty;

    public string SupplementalText { get; set; } = string.Empty;
}

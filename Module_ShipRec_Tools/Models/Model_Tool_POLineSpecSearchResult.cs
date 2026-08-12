namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Ranked PO line spec search result projected for the ShipRec tool UI.
/// </summary>
public class Model_Tool_POLineSpecSearchResult
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

    public string SpecExcerpt { get; set; } = string.Empty;

    public string SpecText { get; set; } = string.Empty;

    public int MatchScore { get; set; }

    public string BinaryType { get; set; } = string.Empty;
}

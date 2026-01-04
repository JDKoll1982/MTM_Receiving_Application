namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents a reason code for non-PO packages (po_number='OTHER')
/// Maps to routing_other_reasons database table
/// </summary>
public class Model_RoutingOtherReason
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Short reason code (e.g., "RETURNED", "SAMPLE")
    /// </summary>
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// User-facing description (e.g., "Returned Item")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Active status (0=inactive, 1=active)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order for dropdown display (lower = first)
    /// </summary>
    public int DisplayOrder { get; set; } = 999;
}

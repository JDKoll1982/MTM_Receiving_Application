using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Structured email data for PO requisition
/// Separates email content into logical sections for display and formatting
/// </summary>
public class Model_VolvoEmailData
{
    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Greeting text
    /// </summary>
    public string Greeting { get; set; } = string.Empty;

    /// <summary>
    /// Main message body
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// List of discrepancy line items (empty if no discrepancies)
    /// </summary>
    public List<DiscrepancyLineItem> Discrepancies { get; set; } = new();

    /// <summary>
    /// Requested parts/quantities after component explosion
    /// </summary>
    public Dictionary<string, int> RequestedLines { get; set; } = new();

    /// <summary>
    /// Additional notes (optional)
    /// </summary>
    public string? AdditionalNotes { get; set; }

    /// <summary>
    /// Email signature
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Represents a discrepancy line item for table display
    /// </summary>
    public class DiscrepancyLineItem
    {
        public string PartNumber { get; set; } = string.Empty;
        public int PacklistQty { get; set; }
        public int ReceivedQty { get; set; }
        public int Difference { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}

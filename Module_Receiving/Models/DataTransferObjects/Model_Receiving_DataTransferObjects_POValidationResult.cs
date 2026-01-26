using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;

/// <summary>
/// DataTransferObjects containing PO number validation result
/// Used in Wizard Mode Step 1 for PO validation
/// </summary>
public class Model_Receiving_DataTransferObjects_POValidationResult
{
    /// <summary>
    /// Whether the PO number is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Standardized/formatted PO number
    /// </summary>
    public string? FormattedPONumber { get; set; }

    /// <summary>
    /// Whether PO exists in ERP system (future: query Infor Visual)
    /// </summary>
    public bool ExistsInERP { get; set; }

    /// <summary>
    /// Whether PO has already been received (duplicate check)
    /// </summary>
    public bool AlreadyReceived { get; set; }

    /// <summary>
    /// Warning messages (non-blocking)
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

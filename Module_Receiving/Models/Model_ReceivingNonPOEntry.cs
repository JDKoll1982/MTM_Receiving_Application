using System;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents a saved reusable non-PO reference entry for Receiving guided workflow.
/// </summary>
public class Model_ReceivingNonPOEntry
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UseCount { get; set; }
}

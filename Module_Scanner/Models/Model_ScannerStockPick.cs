namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Result of the Step 6b-a1-a stock-location modal: a chosen source location, the
/// operator-entered transfer quantity, and the number of transaction lines it should
/// create in the main flow (default 1, never below 1).
/// </summary>
public sealed class Model_ScannerStockPick
{
    /// <summary>Selected part (location search mode); empty for part-number mode.</summary>
    public string PartId { get; set; } = string.Empty;

    /// <summary>Selected source location (part-number mode); empty for location mode.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>On-hand quantity of the source location (used as the per-row max).</summary>
    public decimal OnHand { get; set; }

    public string Quantity { get; set; } = string.Empty;

    public int TransactionCount { get; set; } = 1;
}

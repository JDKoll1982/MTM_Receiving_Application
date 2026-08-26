namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Result of the Step 6b-a1-a stock-location modal: the chosen source location and the
/// operator-entered transfer quantity (bounded to 0..location on-hand).
/// </summary>
public sealed class Model_ScannerStockPick
{
    public string Location { get; set; } = string.Empty;

    public string Quantity { get; set; } = string.Empty;
}

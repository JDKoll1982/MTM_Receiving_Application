using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// One destination row in the Advanced bulk-move dialog Step 2. Each row maps to a single
/// Items To Send line: the part moves from the shared From location to the entered To location.
/// </summary>
public sealed partial class Model_ScannerBulkMoveDestination : ObservableObject
{
    public string PartId { get; set; } = string.Empty;

    public string PartDescription { get; set; } = string.Empty;

    public string FromWarehouse { get; set; } = string.Empty;

    public string FromLocation { get; set; } = string.Empty;

    /// <summary>Destination location entered by the operator.</summary>
    [ObservableProperty]
    private string _toLocation = string.Empty;

    /// <summary>Numeric quantity bound to the Step 2 NumberBox.</summary>
    [ObservableProperty]
    private double _quantityNumber;

    /// <summary>
    /// Quantity to move on this row as the string payload used by
    /// <c>Model_ScannerBatchItem.PayloadQuantity</c>.
    /// </summary>
    public string Quantity =>
        QuantityNumber > 0
            ? QuantityNumber.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;
}

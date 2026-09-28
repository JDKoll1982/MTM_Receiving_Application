using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// A selectable part row in the Advanced bulk-move dialog Step 1 (parts at a From location).
/// </summary>
public sealed partial class Model_ScannerBulkMovePart : ObservableObject
{
    public string PartId { get; set; } = string.Empty;

    public string PartDescription { get; set; } = string.Empty;

    public string WarehouseCode { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    /// <summary>On-hand quantity for this part at the source location.</summary>
    public decimal Quantity { get; set; }

    /// <summary>True when the operator checked this part to move it.</summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// How many destination rows to create for this part. Defaults to 1; when greater than 1
    /// the part is split into that many destination rows in Step 2.
    /// </summary>
    [ObservableProperty]
    private int _entryCount = 1;
}

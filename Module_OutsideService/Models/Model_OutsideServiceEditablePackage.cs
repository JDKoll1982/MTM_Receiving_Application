namespace MTM_Receiving_Application.Module_OutsideService.Models;

/// <summary>
/// Editable package row used by the Outside Service setup screen.
/// </summary>
public class Model_OutsideServiceEditablePackage
{
    /// <summary>
    /// Gets or sets the 1-based package sequence.
    /// </summary>
    public int PackageSequence { get; set; }

    /// <summary>
    /// Gets or sets the quantity entered for the package row.
    /// </summary>
    public double PackageQuantity { get; set; }

    /// <summary>
    /// Gets the display label for the package row.
    /// </summary>
    public string PackageLabel => $"Package {PackageSequence}";
}

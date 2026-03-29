namespace MTM_Receiving_Application.Module_OutsideService.Models;

/// <summary>
/// Represents one physical package quantity row on an Outside Service line.
/// </summary>
public class Model_OutsideServiceRequestPackage
{
    /// <summary>
    /// Gets or sets the package record identifier.
    /// </summary>
    public int OutsideServiceRequestPackageId { get; set; }

    /// <summary>
    /// Gets or sets the parent line identifier.
    /// </summary>
    public int OutsideServiceRequestLineId { get; set; }

    /// <summary>
    /// Gets or sets the 1-based package sequence.
    /// </summary>
    public int PackageSequence { get; set; }

    /// <summary>
    /// Gets or sets the quantity stored in this package.
    /// </summary>
    public decimal PackageQuantity { get; set; }
}

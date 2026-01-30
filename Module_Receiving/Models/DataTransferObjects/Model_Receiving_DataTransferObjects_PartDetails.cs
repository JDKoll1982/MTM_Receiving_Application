using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;

/// <summary>
/// DataTransferObjects containing enriched part details with preferences and type information
/// Used in Wizard Mode Step 1 when selecting a part
/// </summary>
public class Model_Receiving_DataTransferObjects_PartDetails
{
    /// <summary>
    /// Part number/ID
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Part description from part master.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Full part type entity object.
    /// </summary>
    public Model_Receiving_TableEntitys_PartType? PartType { get; set; }

    /// <summary>
    /// Part type ID
    /// </summary>
    public int? PartTypeId { get; set; }

    /// <summary>
    /// Part type name (Coil, Flat Bar, etc.)
    /// </summary>
    public string? PartTypeName { get; set; }

    /// <summary>
    /// Part type code
    /// </summary>
    public string? PartTypeCode { get; set; }

    /// <summary>
    /// Default receiving location for this part (alias for DefaultReceivingLocation).
    /// </summary>
    public string? DefaultLocation { get; set; }

    /// <summary>
    /// Default receiving location for this part.
    /// </summary>
    public string? DefaultReceivingLocation { get; set; }

    /// <summary>
    /// Default package type for this part.
    /// </summary>
    public string? DefaultPackageType { get; set; }

    /// <summary>
    /// Default number of packages per load.
    /// </summary>
    public int? DefaultPackagesPerLoad { get; set; }

    /// <summary>
    /// Whether this part requires quality hold.
    /// </summary>
    public bool RequiresQualityHold { get; set; }

    /// <summary>
    /// Quality hold procedure (if applicable).
    /// </summary>
    public string? QualityHoldProcedure { get; set; }

    /// <summary>
    /// Whether part type requires diameter measurement.
    /// </summary>
    public bool RequiresDiameter { get; set; }

    /// <summary>
    /// Whether part type requires width measurement.
    /// </summary>
    public bool RequiresWidth { get; set; }

    /// <summary>
    /// Whether part type requires length measurement.
    /// </summary>
    public bool RequiresLength { get; set; }

    /// <summary>
    /// Whether part type requires thickness measurement.
    /// </summary>
    public bool RequiresThickness { get; set; }

    /// <summary>
    /// Whether part type requires weight measurement.
    /// </summary>
    public bool RequiresWeight { get; set; }
}

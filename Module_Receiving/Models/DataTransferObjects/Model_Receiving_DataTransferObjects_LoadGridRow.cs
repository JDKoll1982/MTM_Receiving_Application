namespace MTM_Receiving_Application.Module_Receiving.Models.DTOs;

/// <summary>
/// Data Transfer Object for representing a single row in the Load Details Grid (Step 2)
/// Used for DataGrid binding and bulk operations
/// </summary>
public class Model_Receiving_DataTransferObjects_LoadGridRow
{
    /// <summary>
    /// Load number (1-based index)
    /// </summary>
    public int LoadNumber { get; set; }

    /// <summary>
    /// Part number/ID
    /// </summary>
    public string PartId { get; set; } = string.Empty;

    /// <summary>
    /// Weight or quantity for this load
    /// </summary>
    public decimal? WeightOrQuantity { get; set; }

    /// <summary>
    /// Unit of measure (LBS, KG, EA)
    /// </summary>
    public string UnitOfMeasure { get; set; } = "LBS";

    /// <summary>
    /// Heat lot number
    /// </summary>
    public string? HeatLot { get; set; }

    /// <summary>
    /// Package type (Skid, Pallet, Box, etc.)
    /// </summary>
    public string? PackageType { get; set; }

    /// <summary>
    /// Number of packages in this load
    /// </summary>
    public int? PackagesPerLoad { get; set; }

    /// <summary>
    /// Weight per individual package
    /// </summary>
    public decimal? WeightPerPackage { get; set; }

    /// <summary>
    /// Receiving location
    /// </summary>
    public string? ReceivingLocation { get; set; }

    /// <summary>
    /// Whether this field was auto-filled via bulk copy
    /// Used for clear auto-filled operations
    /// </summary>
    public bool IsAutoFilled { get; set; }

    /// <summary>
    /// Whether this row has validation errors
    /// </summary>
    public bool HasErrors { get; set; }

    /// <summary>
    /// Validation error message (if any)
    /// </summary>
    public string? ErrorMessage { get; set; }
}

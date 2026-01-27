namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Defines the 10 standard part types based on part number prefixes
/// </summary>
public enum Enum_Receiving_Type_PartType
{
    /// <summary>
    /// Coil - Prefix: MMC
    /// </summary>
    Coil = 1,

    /// <summary>
    /// Flat-stock - Prefix: MMF
    /// </summary>
    FlatStock = 2,

    /// <summary>
    /// Customer Supplied Coil - Prefix: MMCCS
    /// </summary>
    CustomerSuppliedCoil = 3,

    /// <summary>
    /// Customer Supplied Flat-stock - Prefix: MMFCS
    /// </summary>
    CustomerSuppliedFlatStock = 4,

    /// <summary>
    /// Special Requiements Coil - Prefix: MMCSR
    /// </summary>
    /// <remarks>
    /// Special Requirements parts may have unique handling or documentation needs.
    /// /remarks>
    SpecialRequirementsCoil = 5,

    /// <summary>
    /// Special Requirements Flat-stock - Prefix: MMFSR
    /// </summary>
    /// <remarks>
    /// Special Requirements parts may have unique handling or documentation needs.
    /// /remarks>
    SpecialRequirementsFlatStock = 6,

    /// <summary>
    /// Other/Unknown - Default when prefix doesn't match
    /// </summary>
    Other = 7
}

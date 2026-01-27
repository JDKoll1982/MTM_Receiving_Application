namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Defines which individual fields can be copied in bulk copy operations (Step 2: Load Details).
/// Multiple fields can be selected for a single copy operation.
/// </summary>
public enum Enum_Receiving_CopyType_FieldSelection
{
    /// <summary>
    /// Heat/Lot number field.
    /// </summary>
    HeatLot = 1,

    /// <summary>
    /// Package Type field (Skid, Pallet, Box, etc.).
    /// </summary>
    PackageType = 2,

    /// <summary>
    /// Packages Per Load field (count of packages).
    /// </summary>
    PackagesPerLoad = 3,

    /// <summary>
    /// Receiving Location field (Dock A, RECV, etc.).
    /// </summary>
    ReceivingLocation = 4
}


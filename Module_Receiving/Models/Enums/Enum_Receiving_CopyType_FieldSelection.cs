namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Defines which fields to copy in bulk copy operations (Step 2: Load Details)
/// </summary>
public enum Enum_Receiving_CopyType_FieldSelection
{
    /// <summary>
    /// Copy all fields to empty cells only
    /// </summary>
    AllFields = 1,

    /// <summary>
    /// Copy only Weight/Quantity field to empty cells
    /// </summary>
    WeightQuantityOnly = 2,

    /// <summary>
    /// Copy only Heat Lot field to empty cells
    /// </summary>
    HeatLotOnly = 3,

    /// <summary>
    /// Copy only Package Type field to empty cells
    /// </summary>
    PackageTypeOnly = 4,

    /// <summary>
    /// Copy only Packages Per Load field to empty cells
    /// </summary>
    PackagesPerLoadOnly = 5
}

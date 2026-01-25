namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Specifies which fields to copy in bulk copy operations.
/// Used by CopyToLoadsCommand and ClearAutoFilledDataCommand.
/// </summary>
public enum CopyFields
{
    /// <summary>
    /// Copy all load detail fields (Weight, Heat Lot, Package Type, Packages Per Load).
    /// </summary>
    AllFields,

    /// <summary>
    /// Copy only the Weight or Quantity field.
    /// </summary>
    WeightOnly,

    /// <summary>
    /// Copy only the Heat Lot field.
    /// </summary>
    HeatLotOnly,

    /// <summary>
    /// Copy only the Package Type field.
    /// </summary>
    PackageTypeOnly,

    /// <summary>
    /// Copy only the Packages Per Load field.
    /// </summary>
    PackagesPerLoadOnly
}

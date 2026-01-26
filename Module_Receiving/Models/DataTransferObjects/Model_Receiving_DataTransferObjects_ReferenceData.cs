using System.Collections.Generic;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;

/// <summary>
/// DataTransferObjects containing all reference data for the Receiving module
/// Used for populating dropdowns, comboboxes, and lookups
/// </summary>
public class Model_Receiving_DataTransferObjects_ReferenceData
{
    /// <summary>
    /// Available part types (Coil, Flat Bar, Round, etc.)
    /// </summary>
    public List<Model_Receiving_TableEntitys_PartType> PartTypes { get; set; } = new();

    /// <summary>
    /// Available package types (Skid, Pallet, Box, etc.)
    /// </summary>
    public List<Model_Receiving_TableEntitys_PackageType> PackageTypes { get; set; } = new();

    /// <summary>
    /// Available receiving locations
    /// </summary>
    public List<Model_Receiving_TableEntitys_Location> Locations { get; set; } = new();
}

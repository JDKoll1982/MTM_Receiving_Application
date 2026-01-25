using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Preview of a copy operation showing what will be copied vs preserved.
/// Used to display a confirmation dialog before executing the copy.
/// </summary>
public class Model_Receiving_DTO_CopyPreview
{
    /// <summary>
    /// Number of cells that will be copied from source to target.
    /// </summary>
    public int CellsToBeCopied { get; init; }
    
    /// <summary>
    /// Number of cells that will be preserved (not overwritten) due to existing user data.
    /// </summary>
    public int CellsToBePreserved { get; init; }
    
    /// <summary>
    /// Dictionary mapping load numbers to field names that have conflicting data.
    /// Key: Load number, Value: List of field names with existing data that will be preserved.
    /// Example: { 3: ["HeatLot", "PackageType"], 5: ["WeightOrQuantity"] }
    /// </summary>
    public Dictionary<int, List<string>> LoadsWithConflicts { get; init; } = new();
}

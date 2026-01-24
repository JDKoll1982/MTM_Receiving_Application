namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Result of a bulk copy operation showing which cells were copied vs preserved.
/// Helps users understand the impact of copy operations on existing data.
/// </summary>
public class CopyOperationResult
{
    /// <summary>
    /// Load number used as the source for the copy operation.
    /// </summary>
    public int SourceLoadNumber { get; init; }
    
    /// <summary>
    /// Total number of target loads affected by the copy operation.
    /// </summary>
    public int TotalTargetLoads { get; init; }
    
    /// <summary>
    /// Number of cells that were copied from source to target.
    /// </summary>
    public int CellsCopied { get; init; }
    
    /// <summary>
    /// Number of cells that were preserved (not overwritten) because they contained user-entered data.
    /// </summary>
    public int CellsPreserved { get; init; }
    
    /// <summary>
    /// List of load numbers that had user-entered data preserved during the copy operation.
    /// </summary>
    public List<int> LoadsWithPreservedData { get; init; } = new();
    
    /// <summary>
    /// True if the copy operation completed successfully.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// User-friendly message describing the copy operation result.
    /// Example: "Copied to 10 rows. Preserved 3 rows with existing data."
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

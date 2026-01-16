using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Service interface for CSV export of dunnage data with dynamic column generation
/// EXTENDED for dynamic spec columns (spec 010-dunnage-complete)
/// </summary>
public interface IService_DunnageCSVWriter
{
    // ============================================
    // EXISTING METHOD (from spec 006-dunnage-services)
    // ============================================

    /// <summary>
    /// Write dunnage loads to CSV file (wizard workflow export)
    /// Uses fixed columns based on selected type's specs
    /// </summary>
    /// <param name="loads">List of loads to export</param>
    /// <param name="typeName">Dunnage type name for filename</param>
    /// <returns>File paths (local and network) and success status</returns>
    public Task<Model_CSVWriteResult> WriteToCsvAsync(List<Model_DunnageLoad> loads, string typeName);

    /// <summary>
    /// Write dunnage loads to CSV file (backward compatibility)
    /// </summary>
    /// <param name="loads"></param>
    public Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads);

    // ============================================
    // NEW METHODS (spec 010-dunnage-complete)
    // ============================================

    /// <summary>
    /// Write dunnage loads to CSV with dynamic columns for all spec keys
    /// Used for Manual Entry and Edit Mode exports (all types in one file)
    /// </summary>
    /// <param name="loads">List of loads to export</param>
    /// <param name="allSpecKeys">Union of all spec keys across all types (from GetAllSpecKeysAsync)</param>
    /// <param name="filename">Optional custom filename (default: DunnageData_{timestamp}.csv)</param>
    /// <returns>CSV write result with local/network paths and success status</returns>
    /// <remarks>
    /// Generates columns: ID, PartID, DunnageType, Quantity, PONumber, ReceivedDate, UserId, Location, LabelNumber, [Dynamic Spec Columns]
    /// Blank cells for specs not applicable to a load's type
    /// RFC 4180 compliant (CsvHelper escaping)
    /// Dual-path write: local (%APPDATA%) always succeeds, network (\\MTMDC\) best-effort
    /// </remarks>
    public Task<Model_CSVWriteResult> WriteDynamicCsvAsync(
        List<Model_DunnageLoad> loads,
        List<string> allSpecKeys,
        string? filename = null);

    /// <summary>
    /// Export selected loads from DataGrid (Manual Entry or Edit Mode)
    /// Includes dynamic spec columns based on types in selection
    /// </summary>
    /// <param name="selectedLoads">Loads selected in DataGrid</param>
    /// <param name="includeAllSpecColumns">If true, includes all spec keys across all types. If false, only keys used by selected loads' types.</param>
    /// <returns>CSV write result</returns>
    public Task<Model_CSVWriteResult> ExportSelectedLoadsAsync(
        List<Model_DunnageLoad> selectedLoads,
        bool includeAllSpecColumns = false);

    /// <summary>
    /// Validate network path availability (for dual-path writing)
    /// </summary>
    /// <param name="timeout">Timeout in seconds for reachability check</param>
    /// <returns>True if network path reachable, false otherwise</returns>
    public Task<bool> IsNetworkPathAvailableAsync(int timeout = 3);

    /// <summary>
    /// Get local CSV file path for current user
    /// </summary>
    /// <param name="filename">Filename (without path)</param>
    /// <returns>Full local path (%APPDATA%\MTM_Receiving_Application\{filename})</returns>
    public string GetLocalCsvPath(string filename);

    /// <summary>
    /// Get network CSV file path for current user
    /// </summary>
    /// <param name="filename">Filename (without path)</param>
    /// <returns>Full network path (\\MTMDC\DunnageData\{username}\{filename})</returns>
    public string GetNetworkCsvPath(string filename);

    /// <summary>
    /// Clears the contents of all CSV files created by the application from local and network paths
    /// </summary>
    /// <param name="filenamePattern">Optional filename pattern to match (e.g., "DunnageData_*.csv"). If null, clears all CSV files.</param>
    /// <returns>CSV clear result with counts of cleared files and any errors</returns>
    public Task<Model_CSVDeleteResult> ClearCSVFilesAsync(string? filenamePattern = null);
}

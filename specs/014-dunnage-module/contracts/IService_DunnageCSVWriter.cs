using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service for writing dunnage data to CSV files for LabelView 2022 import.
/// </summary>
public interface IService_DunnageCSVWriter
{
    /// <summary>
    /// Writes dunnage loads to CSV file in LabelView-compatible format.
    /// </summary>
    /// <param name="loads">List of dunnage loads to export</param>
    /// <param name="filePath">Target file path (optional, uses default if not provided)</param>
    /// <returns>DAO result with file path of generated CSV</returns>
    Task<Model_Dao_Result<string>> WriteLoadsToCSVAsync(
        List<Model_DunnageLoad> loads,
        string? filePath = null);

    /// <summary>
    /// Gets the default CSV export file path for dunnage labels.
    /// </summary>
    /// <returns>Default file path</returns>
    string GetDefaultFilePath();
}


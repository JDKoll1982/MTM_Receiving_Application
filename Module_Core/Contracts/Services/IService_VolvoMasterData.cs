using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service interface for managing Volvo parts master data
/// </summary>
public interface IService_VolvoMasterData
{
    /// <summary>
    /// Gets all parts from the master catalog
    /// </summary>
    /// <param name="includeInactive">Include deactivated parts</param>
    /// <returns>List of parts</returns>
    Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets a specific part by part number
    /// </summary>
    /// <param name="partNumber">Part number to retrieve</param>
    /// <returns>Part details or null if not found</returns>
    Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber);

    /// <summary>
    /// Adds a new part to the master catalog
    /// </summary>
    /// <param name="part">Part to add</param>
    /// <param name="components">Optional component relationships</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> AddPartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null);

    /// <summary>
    /// Updates an existing part in the master catalog
    /// </summary>
    /// <param name="part">Part with updated values</param>
    /// <param name="components">Updated component relationships (replaces existing)</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> UpdatePartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null);

    /// <summary>
    /// Deactivates a part (soft delete - preserves historical integrity)
    /// </summary>
    /// <param name="partNumber">Part number to deactivate</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> DeactivatePartAsync(string partNumber);

    /// <summary>
    /// Imports parts from CSV file
    /// </summary>
    /// <param name="csvFilePath">Path to CSV file</param>
    /// <returns>Import summary with new/updated/unchanged counts</returns>
    Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportCsvAsync(string csvFilePath);

    /// <summary>
    /// Exports all parts to CSV file
    /// </summary>
    /// <param name="csvFilePath">Path where CSV should be saved</param>
    /// <param name="includeInactive">Include deactivated parts</param>
    /// <returns>Success result with file path</returns>
    Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false);
}

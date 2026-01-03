using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Volvo;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service interface for Volvo parts master data management
/// Handles CRUD operations for parts catalog, component relationships, and CSV import/export
/// </summary>
public interface IService_VolvoMasterData
{
    /// <summary>
    /// Gets all Volvo parts (active and optionally inactive)
    /// Used for populating dropdown lists in shipment entry
    /// </summary>
    /// <param name="includeInactive">If true, includes deactivated parts</param>
    /// <returns>List of all parts</returns>
    Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets part by part number
    /// </summary>
    /// <param name="partNumber">Part number (e.g., V-EMB-2)</param>
    /// <returns>Part if found, null otherwise</returns>
    Task<Model_Dao_Result<Model_VolvoPart?>> GetPartAsync(string partNumber);

    /// <summary>
    /// Gets all components for a parent part
    /// Used for component explosion calculations
    /// </summary>
    /// <param name="parentPartNumber">Parent part number</param>
    /// <returns>List of component relationships</returns>
    Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(string parentPartNumber);

    /// <summary>
    /// Adds new Volvo part with optional components
    /// Validates unique part number, required fields
    /// </summary>
    /// <param name="part">Part to add</param>
    /// <param name="components">Optional list of component relationships</param>
    /// <returns>Created part</returns>
    Task<Model_Dao_Result<Model_VolvoPart>> AddPartAsync(
        Model_VolvoPart part,
        List<Model_VolvoPartComponent>? components = null);

    /// <summary>
    /// Updates existing Volvo part (quantity and components only, part number cannot change)
    /// Shows warning that changes will NOT affect existing shipment records (historical integrity)
    /// </summary>
    /// <param name="part">Updated part</param>
    /// <param name="components">Updated component relationships (replaces all existing)</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> UpdatePartAsync(
        Model_VolvoPart part,
        List<Model_VolvoPartComponent>? components = null);

    /// <summary>
    /// Deactivates a part (soft delete)
    /// Part is hidden from dropdown lists but remains in database for historical data integrity
    /// </summary>
    /// <param name="partNumber">Part number to deactivate</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> DeactivatePartAsync(string partNumber);

    /// <summary>
    /// Reactivates a previously deactivated part
    /// </summary>
    /// <param name="partNumber">Part number to reactivate</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> ReactivatePartAsync(string partNumber);

    /// <summary>
    /// Imports parts from CSV file (DataSheet.csv format)
    /// Validates CSV structure, shows preview of changes (new/updated/unchanged) before import
    /// </summary>
    /// <param name="csvFilePath">Path to CSV file</param>
    /// <returns>Import summary with counts of new/updated/unchanged parts</returns>
    Task<Model_Dao_Result<Model_VolvoImportSummary>> ImportCsvAsync(string csvFilePath);

    /// <summary>
    /// Exports all parts to CSV file (DataSheet.csv format)
    /// Used for backup and data migration
    /// </summary>
    /// <param name="filePath">Target file path</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> ExportCsvAsync(string filePath);
}

/// <summary>
/// Summary of CSV import operation
/// </summary>
public class Model_VolvoImportSummary
{
    public int NewParts { get; set; }
    public int UpdatedParts { get; set; }
    public int UnchangedParts { get; set; }
    public List<string> Errors { get; set; } = new();
}


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service interface for core routing label operations: creation, editing, CSV export, validation
/// </summary>
public interface IRoutingService
{
    /// <summary>
    /// Creates a new routing label, saves to database and exports to CSV
    /// </summary>
    /// <param name="label">Label data to create</param>
    /// <returns>Result with created label ID or error message</returns>
    public Task<Model_Dao_Result<int>> CreateLabelAsync(Model_RoutingLabel label);

    /// <summary>
    /// Updates an existing routing label (Edit Mode), logs changes to history
    /// </summary>
    /// <param name="label">Updated label data (must include ID)</param>
    /// <param name="editedByEmployeeNumber">Employee number making the edit</param>
    /// <returns>Result indicating success or failure</returns>
    public Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label, int editedByEmployeeNumber);

    /// <summary>
    /// Retrieves a single label by ID
    /// </summary>
    /// <param name="labelId">Label ID to retrieve</param>
    /// <returns>Result with label data or error message</returns>
    public Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId);

    /// <summary>
    /// Retrieves all labels for Edit Mode grid (paginated, sorted by date DESC)
    /// </summary>
    /// <param name="limit">Number of records to return</param>
    /// <param name="offset">Number of records to skip</param>
    /// <returns>Result with list of labels or error message</returns>
    public Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0);

    /// <summary>
    /// Checks if a duplicate label exists (same PO, Line, Recipient, Date)
    /// </summary>
    /// <param name="poNumber">PO number</param>
    /// <param name="lineNumber">Line number</param>
    /// <param name="recipientId">Recipient ID</param>
    /// <param name="createdDate">Date to check</param>
    /// <returns>Result with duplicate check data (exists flag + existing label ID if found)</returns>
    public Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateLabelAsync(
        string poNumber,
        string lineNumber,
        int recipientId,
        DateTime createdDate);

    /// <summary>
    /// Exports a single label to CSV file (async with retry)
    /// </summary>
    /// <param name="label">Label to export</param>
    /// <returns>Result indicating CSV export success or failure</returns>
    public Task<Model_Dao_Result> ExportLabelToCsvAsync(Model_RoutingLabel label);

    /// <summary>
    /// Regenerates CSV entry for a label (Reprint functionality in Edit Mode)
    /// </summary>
    /// <param name="labelId">Label ID to regenerate CSV for</param>
    /// <returns>Result indicating CSV regeneration success or failure</returns>
    public Task<Model_Dao_Result> RegenerateLabelCsvAsync(int labelId);

    /// <summary>
    /// Resets (clears) the CSV file (with confirmation required)
    /// </summary>
    /// <returns>Result indicating CSV reset success or failure</returns>
    public Task<Model_Dao_Result> ResetCsvFileAsync();

    /// <summary>
    /// Validates label data before creation (business rules)
    /// </summary>
    /// <param name="label">Label to validate</param>
    /// <returns>Validation result (IsSuccess=true if valid, ErrorMessage if invalid)</returns>
    public Model_Dao_Result ValidateLabel(Model_RoutingLabel label);
}

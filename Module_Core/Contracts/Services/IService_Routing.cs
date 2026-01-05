using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for managing routing label entry, queue, and CSV export.
/// Handles label creation, numbering, and LabelView-compatible CSV generation.
/// </summary>
public interface IService_Routing
{
    /// <summary>
    /// Adds a new routing label to the current queue.
    /// Auto-increments label number if not provided.
    /// </summary>
    /// <param name="label">Routing label to add</param>
    /// <returns>DAO result with created label ID</returns>
    public Task<Model_Dao_Result<int>> AddLabelAsync(Model_Routing_Label label);

    /// <summary>
    /// Gets all current/today labels (not archived).
    /// </summary>
    /// <returns>DAO result with list of current labels</returns>
    public Task<Model_Dao_Result<List<Model_Routing_Label>>> GetTodayLabelsAsync();

    /// <summary>
    /// Duplicates a label (copies all fields, increments label number).
    /// </summary>
    /// <param name="labelId">ID of label to duplicate</param>
    /// <returns>DAO result with new label ID</returns>
    public Task<Model_Dao_Result<int>> DuplicateLabelAsync(int labelId);

    /// <summary>
    /// Gets the next label number for the current session.
    /// </summary>
    /// <returns>DAO result with next label number</returns>
    public Task<Model_Dao_Result<int>> GetNextLabelNumberAsync();

    /// <summary>
    /// Exports labels to CSV file for LabelView 2022 import.
    /// Uses template: "Expo - Mini UPS Label ver. 1.0"
    /// </summary>
    /// <param name="labels">List of labels to export</param>
    /// <param name="filePath">Target file path (optional, uses default if not provided)</param>
    /// <returns>DAO result with file path of generated CSV</returns>
    public Task<Model_Dao_Result<string>> ExportToCSVAsync(
        List<Model_Routing_Label> labels,
        string? filePath = null);

    /// <summary>
    /// Updates an existing routing label.
    /// </summary>
    /// <param name="label">Updated label</param>
    /// <returns>DAO result</returns>
    public Task<Model_Dao_Result> UpdateLabelAsync(Model_Routing_Label label);

    /// <summary>
    /// Deletes a routing label from the queue.
    /// </summary>
    /// <param name="labelId">ID of label to delete</param>
    /// <returns>DAO result</returns>
    public Task<Model_Dao_Result> DeleteLabelAsync(int labelId);
}



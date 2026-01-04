using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for routing label history archival and retrieval.
/// Handles moving labels from "Today" to "History" and retrieving archived labels.
/// </summary>
public interface IService_Routing_History
{
    /// <summary>
    /// Archives all current/today labels to history.
    /// Moves labels from routing_labels (is_archived=0) to routing_labels_history (is_archived=1).
    /// Clears current labels after archival.
    /// </summary>
    /// <returns>DAO result with count of archived labels</returns>
    Task<Model_Dao_Result<int>> ArchiveTodayToHistoryAsync();

    /// <summary>
    /// Gets archived labels filtered by date range.
    /// Labels are sorted by date descending and grouped by date.
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <returns>DAO result with list of archived labels</returns>
    Task<Model_Dao_Result<List<Model_Routing_Label>>> GetHistoryAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets all archived labels (no date filter).
    /// </summary>
    /// <returns>DAO result with list of all archived labels</returns>
    Task<Model_Dao_Result<List<Model_Routing_Label>>> GetAllHistoryAsync();

    /// <summary>
    /// Exports history to CSV file.
    /// </summary>
    /// <param name="labels">List of labels to export</param>
    /// <param name="filePath">Target file path</param>
    /// <returns>DAO result</returns>
    Task<Model_Dao_Result> ExportHistoryToCSVAsync(
        List<Model_Routing_Label> labels,
        string filePath);
}



using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Models.Reporting;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service interface for End-of-Day reporting across all modules
/// Filters history data by date range, normalizes PO numbers, and exports to CSV/email format
/// Reference: EndOfDayEmail.js and AppScript.js from Google Sheets routing label system
/// </summary>
public interface IService_Reporting
{
    /// <summary>
    /// Retrieves Receiving history data filtered by date range
    /// Includes: PO Number (normalized), Part Number, Description, Quantity, Weight, Heat/Lot, Date
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <returns>DAO result containing list of report rows</returns>
    Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Retrieves Dunnage history data filtered by date range
    /// Includes: Type, Part, Specs (concatenated), Quantity, Date
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <returns>DAO result containing list of report rows</returns>
    Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Retrieves Routing history data filtered by date range
    /// Includes: Deliver To, Department, Package Description, PO Number, Employee, Date
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <returns>DAO result containing list of report rows</returns>
    Task<Model_Dao_Result<List<Model_ReportRow>>> GetRoutingHistoryAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Retrieves Volvo history data filtered by date range
    /// Includes: Shipment Date, Shipment Number, PO Number, Receiver Number, Status, Date
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <returns>DAO result containing list of report rows</returns>
    Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Checks availability of data for each module in the specified date range
    /// Returns dictionary of module name â†’ record count
    /// Used to enable/disable module checkboxes in UI
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <returns>DAO result containing dictionary of module names and record counts</returns>
    Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Exports report data to CSV file matching MiniUPSLabel.csv structure
    /// </summary>
    /// <param name="data">Filtered report rows</param>
    /// <param name="moduleName">Module name for filename (Receiving, Dunnage, Routing, Volvo)</param>
    /// <returns>DAO result with file path of generated CSV</returns>
    Task<Model_Dao_Result<string>> ExportToCSVAsync(
        List<Model_ReportRow> data,
        string moduleName);

    /// <summary>
    /// Formats report data as email body with alternating row colors grouped by date
    /// Matches Google Sheets colorHistory() function behavior
    /// </summary>
    /// <param name="data">Filtered report rows</param>
    /// <param name="applyDateGrouping">Apply alternating colors by date group</param>
    /// <returns>DAO result containing HTML-formatted string for email body</returns>
    Task<Model_Dao_Result<string>> FormatForEmailAsync(
        List<Model_ReportRow> data,
        bool applyDateGrouping = true);

    /// <summary>
    /// Normalizes PO number to standard format (PO-063150 or PO-063150B)
    /// Matches EndOfDayEmail.js normalizePO() algorithm
    /// Examples:
    /// - "63150" â†’ "PO-063150"
    /// - "063150B" â†’ "PO-063150B"
    /// - "Customer Supplied" â†’ "Customer Supplied" (pass through)
    /// - "" â†’ "No PO"
    /// - "1234" â†’ "Validate PO" (too short)
    /// </summary>
    /// <param name="poNumber">Raw PO number to normalize</param>
    /// <returns>Normalized PO number</returns>
    string NormalizePONumber(string? poNumber);
}



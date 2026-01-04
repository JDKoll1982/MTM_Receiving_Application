using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Models.Volvo;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service interface for Volvo dunnage requisition workflow
/// Handles shipment entry, component explosion calculation, label generation, email formatting, and PO completion
/// </summary>
public interface IService_Volvo
{
    /// <summary>
    /// Calculates component explosion for a list of parts with skid counts
    /// Aggregates duplicate components across all parts in the shipment
    /// Returns dictionary of part number â†’ total piece count
    /// </summary>
    /// <param name="shipmentLines">List of parts with received skid counts</param>
    /// <returns>Dictionary with part number as key and calculated piece count as value</returns>
    Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> shipmentLines);

    /// <summary>
    /// Generates CSV file for LabelView 2022 import
    /// CSV format matches LabelView requirements with Material ID, Quantity, Employee, Date, Time, Receiver, Notes
    /// File is created in export folder and remains until shipment is archived
    /// </summary>
    /// <param name="shipment">Shipment header with date, employee, notes</param>
    /// <param name="requestedLines">Aggregated requested lines (part number â†’ piece count)</param>
    /// <returns>File path of generated CSV file</returns>
    Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(
        Model_VolvoShipment shipment,
        Dictionary<string, int> requestedLines);

    /// <summary>
    /// Formats email text for purchasing department
    /// Includes greeting (from user settings), discrepancy notice (if any), discrepancies table, and requested lines table
    /// User can edit greeting and discrepancy message, but tables are read-only
    /// </summary>
    /// <param name="shipment">Shipment header</param>
    /// <param name="shipmentLines">List of parts with discrepancy data</param>
    /// <param name="requestedLines">Aggregated requested lines</param>
    /// <param name="customGreeting">User-editable greeting line (default from settings)</param>
    /// <param name="customDiscrepancyMessage">User-editable discrepancy message</param>
    /// <returns>Formatted plain text email ready for copy/paste</returns>
    Task<Model_Dao_Result<string>> FormatEmailTextAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> shipmentLines,
        Dictionary<string, int> requestedLines,
        string? customGreeting = null,
        string? customDiscrepancyMessage = null);

    /// <summary>
    /// Saves shipment as "Pending PO" status
    /// Inserts shipment header and lines, generates shipment number, enforces only one pending PO at a time
    /// </summary>
    /// <param name="shipment">Shipment header (date, employee, notes)</param>
    /// <param name="shipmentLines">List of parts with skid counts and discrepancy data</param>
    /// <returns>Created shipment with generated ID and shipment number</returns>
    Task<Model_Dao_Result<Model_VolvoShipment>> SaveShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> shipmentLines);

    /// <summary>
    /// Gets pending shipment if one exists
    /// Only one pending PO shipment can exist at a time (enforced by application logic)
    /// </summary>
    /// <returns>Pending shipment with lines, or null if none exists</returns>
    Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync();

    /// <summary>
    /// Completes shipment by entering PO and Receiver numbers
    /// Updates status to 'completed', archives to history, clears CSV file content (not delete)
    /// </summary>
    /// <param name="shipmentId">Shipment ID to complete</param>
    /// <param name="poNumber">PO number from purchasing department</param>
    /// <param name="receiverNumber">Receiver number from Infor Visual</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> CompleteShipmentAsync(
        int shipmentId,
        string poNumber,
        string receiverNumber);

    /// <summary>
    /// Updates existing shipment (for history editing)
    /// Regenerates CSV file with warning to user
    /// </summary>
    /// <param name="shipment">Updated shipment header</param>
    /// <param name="shipmentLines">Updated shipment lines</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> UpdateShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> shipmentLines);

    /// <summary>
    /// Gets shipment history with filtering
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <param name="status">Status filter ('pending_po', 'completed', or 'all')</param>
    /// <returns>List of shipments matching filter criteria</returns>
    Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all");

    /// <summary>
    /// Gets shipment by ID with all lines
    /// </summary>
    /// <param name="shipmentId">Shipment ID</param>
    /// <returns>Shipment with lines</returns>
    Task<Model_Dao_Result<Model_VolvoShipment?>> GetShipmentByIdAsync(int shipmentId);

    /// <summary>
    /// Exports history to CSV file
    /// </summary>
    /// <param name="shipments">List of shipments to export</param>
    /// <param name="filePath">Target file path</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> ExportHistoryToCsvAsync(
        List<Model_VolvoShipment> shipments,
        string filePath);

    /// <summary>
    /// Clears CSV file content (keeps file for LabelView compatibility)
    /// </summary>
    /// <param name="csvFilePath">Path to CSV file</param>
    /// <returns>Success result</returns>
    Task<Model_Dao_Result> ClearCsvFileAsync(string csvFilePath);
}



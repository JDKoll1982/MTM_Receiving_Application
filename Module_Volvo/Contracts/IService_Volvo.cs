using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Contracts;

/// <summary>
/// Service interface for Volvo dunnage requisition business logic.
/// Handles component explosion, label generation, email formatting, and shipment management.
/// </summary>
public interface IService_Volvo
{
    /// <summary>
    /// Calculates component explosion and aggregates piece counts for all parts in shipment
    /// Example: If V-EMB-500 (3 skids) includes components V-EMB-2 and V-EMB-92,
    /// result includes V-EMB-500: 264 pcs + V-EMB-2: 3 pcs + V-EMB-92: 3 pcs
    /// </summary>
    /// <param name="lines">List of shipment lines with part numbers and skid counts</param>
    /// <returns>Dictionary of part numbers to total piece counts (includes parent parts + components)</returns>
    public Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> lines);

    /// <summary>
    /// [STUB] Generates labels for label printing.
    /// TODO: Implement database-backed label generation.
    /// Previously saved to: %APPDATA%\MTM_Receiving_Application\Volvo\Labels\Shipment_[ID]_[Date] files
    /// </summary>
    /// <param name="shipmentId">Shipment ID to generate labels for</param>
    /// <returns>File path where labels were written</returns>
    public Task<Model_Dao_Result<string>> GenerateLabelAsync(int shipmentId);

    /// <summary>
    /// Formats email text for PO requisition (with discrepancy notice if applicable)
    /// Includes: Greeting, discrepancy table (if any), requested lines, signature
    /// </summary>
    /// <param name="shipment">Shipment header</param>
    /// <param name="lines">Shipment lines with discrepancy data</param>
    /// <param name="requestedLines">Aggregated component explosion results</param>
    /// <returns>Formatted email text ready to copy to Outlook</returns>
    public Task<string> FormatEmailTextAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int>? requestedLines = null);

    /// <summary>
    /// Formats email data for PO requisition (structured for preview and HTML export)
    /// </summary>
    /// <param name="shipment">Shipment header</param>
    /// <param name="lines">Shipment lines with discrepancy data</param>
    /// <param name="requestedLines">Aggregated component explosion results</param>
    /// <returns>Structured email data</returns>
    public Task<Model_VolvoEmailData> FormatEmailDataAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int>? requestedLines = null);

    /// <summary>
    /// Converts structured email data to HTML format with tables for Outlook paste
    /// </summary>
    /// <param name="emailData">Structured email data</param>
    /// <returns>HTML formatted email</returns>
    public string FormatEmailAsHtml(Model_VolvoEmailData emailData);

    /// <summary>
    /// Validates shipment data before save
    /// Centralized validation logic to ensure data integrity
    /// </summary>
    /// <param name="shipment">Shipment header to validate</param>
    /// <param name="lines">Shipment lines to validate</param>
    /// <returns>Validation result with error message if failed</returns>
    public Task<Model_Dao_Result> ValidateShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines);

    /// <summary>
    /// Saves shipment and lines with status='pending_po'
    /// Validates: Only one pending shipment allowed at a time
    /// </summary>
    /// <param name="shipment">Shipment header data</param>
    /// <param name="lines">Shipment line items</param>
    /// <param name="overwriteExisting">If true, deletes any existing pending shipment before saving</param>
    /// <returns>Tuple of (ShipmentId, ShipmentNumber) for the saved shipment</returns>
    public Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        bool overwriteExisting = false);

    /// <summary>
    /// Gets pending shipment if one exists (status='pending_po')
    /// </summary>
    /// <returns>Pending shipment or null if none exists</returns>
    public Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync();

    /// <summary>
    /// Gets pending shipment with all line items
    /// </summary>
    /// <returns>Tuple of (shipment, lines) or null if no pending shipment</returns>
    public Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync();

    /// <summary>
    /// Completes shipment with PO and Receiver numbers
    /// Sets status='completed' and is_archived=1
    /// </summary>
    /// <param name="shipmentId">Shipment ID to complete</param>
    /// <param name="poNumber">Purchase order number from purchasing department</param>
    /// <param name="receiverNumber">Receiver number from Infor Visual</param>
    /// <returns>Success/failure result</returns>
    public Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber);

    /// <summary>
    /// Gets all active Volvo parts for dropdown population
    /// </summary>
    /// <returns>List of active parts (is_active=1)</returns>
    public Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync();

    /// <summary>
    /// Gets shipment history with filtering
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <param name="status">Status filter ('pending_po', 'completed', or 'all')
    /// </param>
    /// <returns>List of shipments matching filters</returns>
    public Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all");

    /// <summary>
    /// Gets all shipment lines for a specific shipment
    /// </summary>
    /// <param name="shipmentId">Shipment ID</param>
    /// <returns>List of shipment lines</returns>
    public Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetShipmentLinesAsync(int shipmentId);

    /// <summary>
    /// Updates an existing shipment and its lines
    /// Regenerates labels if applicable
    /// </summary>
    /// <param name="shipment">Updated shipment data</param>
    /// <param name="lines">Updated shipment lines</param>
    /// <returns>Success/failure result</returns>
    public Task<Model_Dao_Result> UpdateShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines);

}

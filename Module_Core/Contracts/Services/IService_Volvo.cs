using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service interface for Volvo dunnage requisition business logic
/// Handles component explosion, CSV generation, email formatting, and shipment management
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
    Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> lines);

    /// <summary>
    /// Generates CSV file for LabelView 2022 label printing
    /// Format: Material,Quantity,Employee,Date,Time,Receiver,Notes
    /// Saved to: %APPDATA%\MTM_Receiving_Application\Volvo\Labels\Shipment_[ID]_[Date].csv
    /// </summary>
    /// <param name="shipmentId">Shipment ID to generate labels for</param>
    /// <returns>File path where CSV was written</returns>
    Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId);

    /// <summary>
    /// Formats email text for PO requisition (with discrepancy notice if applicable)
    /// Includes: Greeting, discrepancy table (if any), requested lines, signature
    /// </summary>
    /// <param name="shipment">Shipment header</param>
    /// <param name="lines">Shipment lines with discrepancy data</param>
    /// <param name="requestedLines">Aggregated component explosion results</param>
    /// <returns>Formatted email text ready to copy to Outlook</returns>
    Task<string> FormatEmailTextAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int> requestedLines);

    /// <summary>
    /// Saves shipment and lines with status='pending_po'
    /// Validates: Only one pending shipment allowed at a time
    /// </summary>
    /// <param name="shipment">Shipment header data</param>
    /// <param name="lines">Shipment line items</param>
    /// <returns>Tuple of (ShipmentId, ShipmentNumber) for the saved shipment</returns>
    Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines);

    /// <summary>
    /// Gets pending shipment if one exists (status='pending_po')
    /// </summary>
    /// <returns>Pending shipment or null if none exists</returns>
    Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync();

    /// <summary>
    /// Gets pending shipment with all line items
    /// </summary>
    /// <returns>Tuple of (shipment, lines) or null if no pending shipment</returns>
    Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync();

    /// <summary>
    /// Completes shipment with PO and Receiver numbers
    /// Sets status='completed' and is_archived=1
    /// </summary>
    /// <param name="shipmentId">Shipment ID to complete</param>
    /// <param name="poNumber">Purchase order number from purchasing department</param>
    /// <param name="receiverNumber">Receiver number from Infor Visual</param>
    /// <returns>Success/failure result</returns>
    Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber);

    /// <summary>
    /// Gets all active Volvo parts for dropdown population
    /// </summary>
    /// <returns>List of active parts (is_active=1)</returns>
    Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync();

    /// <summary>
    /// Gets shipment history with filtering
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <param name="status">Status filter ('pending_po', 'completed', or 'all')</param>
    /// <returns>List of shipments matching filters</returns>
    Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetShipmentHistoryAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all");
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Data;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Service for Volvo dunnage requisition business logic
/// Handles component explosion, CSV generation, email formatting, and shipment management
/// </summary>
public class Service_Volvo : IService_Volvo
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;
    private readonly IService_LoggingUtility _logger;

    public Service_Volvo(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        IService_LoggingUtility logger)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates component explosion and aggregates piece counts for all parts in shipment
    /// Algorithm:
    /// 1. For each line, get parent part quantity per skid
    /// 2. Calculate parent pieces: skidCount × qtyPerSkid
    /// 3. Get components for parent part
    /// 4. For each component, calculate pieces: skidCount × componentQty × componentQtyPerSkid
    /// 5. Aggregate duplicates across all lines
    /// </summary>
    public async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> lines)
    {
        try
        {
            await _logger.LogInformationAsync("Calculating component explosion for shipment lines");

            var aggregatedPieces = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                // Get parent part details
                var partResult = await _partDao.GetByIdAsync(line.PartNumber);
                if (!partResult.Success || partResult.Data == null)
                {
                    return new Model_Dao_Result<Dictionary<string, int>>
                    {
                        Success = false,
                        ErrorMessage = $"Part {line.PartNumber} not found in master data",
                        Severity = Enum_ErrorSeverity.Error
                    };
                }

                var parentPart = partResult.Data;

                // Add parent part pieces
                int parentPieces = line.ReceivedSkidCount * parentPart.QuantityPerSkid;
                if (aggregatedPieces.ContainsKey(line.PartNumber))
                {
                    aggregatedPieces[line.PartNumber] += parentPieces;
                }
                else
                {
                    aggregatedPieces[line.PartNumber] = parentPieces;
                }

                // Get and add component pieces
                var componentsResult = await _componentDao.GetByParentPartAsync(line.PartNumber);
                if (componentsResult.Success && componentsResult.Data != null)
                {
                    foreach (var component in componentsResult.Data)
                    {
                        // Component pieces = skidCount × componentQty × componentQtyPerSkid
                        int componentPieces = line.ReceivedSkidCount * component.Quantity * component.ComponentQuantityPerSkid;

                        if (aggregatedPieces.ContainsKey(component.ComponentPartNumber))
                        {
                            aggregatedPieces[component.ComponentPartNumber] += componentPieces;
                        }
                        else
                        {
                            aggregatedPieces[component.ComponentPartNumber] = componentPieces;
                        }
                    }
                }
            }

            await _logger.LogInformationAsync($"Component explosion complete: {aggregatedPieces.Count} unique parts");

            return new Model_Dao_Result<Dictionary<string, int>>
            {
                Success = true,
                Data = aggregatedPieces
            };
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error calculating component explosion: {ex.Message}", ex);
            return new Model_Dao_Result<Dictionary<string, int>>
            {
                Success = false,
                ErrorMessage = $"Error calculating component explosion: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Generates CSV file for LabelView 2022 label printing
    /// Format: Material,Quantity,Employee,Date,Time,Receiver,Notes
    /// </summary>
    public async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId)
    {
        try
        {
            await _logger.LogInformationAsync($"Generating label CSV for shipment {shipmentId}");

            // Get shipment details
            var shipmentResult = await _shipmentDao.GetByIdAsync(shipmentId);
            if (!shipmentResult.Success || shipmentResult.Data == null)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "Shipment not found",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            var shipment = shipmentResult.Data;

            // Get shipment lines
            var linesResult = await _lineDao.GetByShipmentIdAsync(shipmentId);
            if (!linesResult.Success || linesResult.Data == null)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve shipment lines",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            var lines = linesResult.Data;

            // Calculate component explosion
            var explosionResult = await CalculateComponentExplosionAsync(lines);
            if (!explosionResult.Success || explosionResult.Data == null)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = explosionResult.ErrorMessage,
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            var aggregatedPieces = explosionResult.Data;

            // Create CSV directory
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string csvDirectory = Path.Combine(appDataPath, "MTM_Receiving_Application", "Volvo", "Labels");
            Directory.CreateDirectory(csvDirectory);

            // Generate CSV file path
            string dateStr = shipment.ShipmentDate.ToString("yyyyMMdd");
            string fileName = $"Shipment_{shipmentId}_{dateStr}.csv";
            string filePath = Path.Combine(csvDirectory, fileName);

            // Build CSV content
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Material,Quantity,Employee,Date,Time,Receiver,Notes");

            string dateFormatted = shipment.ShipmentDate.ToString("MM/dd/yyyy");
            string timeFormatted = DateTime.Now.ToString("HH:mm:ss");

            foreach (var kvp in aggregatedPieces.OrderBy(x => x.Key))
            {
                // Format: Material,Quantity,Employee,Date,Time,Receiver,Notes
                // Receiver field is empty for Volvo parts (per spec - hide PO field for V-EMB- parts)
                csvContent.AppendLine($"{kvp.Key},{kvp.Value},{shipment.EmployeeNumber},{dateFormatted},{timeFormatted},,");
            }

            // Write CSV file
            await File.WriteAllTextAsync(filePath, csvContent.ToString());

            await _logger.LogInformationAsync($"Label CSV generated: {filePath}");

            return new Model_Dao_Result<string>
            {
                Success = true,
                Data = filePath
            };
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error generating label CSV: {ex.Message}", ex);
            return new Model_Dao_Result<string>
            {
                Success = false,
                ErrorMessage = $"Error generating label CSV: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Formats email text for PO requisition (with discrepancy notice if applicable)
    /// </summary>
    public async Task<string> FormatEmailTextAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int> requestedLines)
    {
        var emailText = new StringBuilder();

        // Subject line (for user reference)
        emailText.AppendLine($"Subject: PO Requisition - Volvo Dunnage - {shipment.ShipmentDate:MM/dd/yyyy} Shipment #{shipment.ShipmentNumber}");
        emailText.AppendLine();

        // Greeting
        emailText.AppendLine("Good morning,");
        emailText.AppendLine();
        emailText.AppendLine($"Please create a PO for the following Volvo dunnage received on {shipment.ShipmentDate:MM/dd/yyyy}:");
        emailText.AppendLine();

        // Discrepancy section (if any)
        var discrepancies = lines.Where(l => l.HasDiscrepancy).ToList();
        if (discrepancies.Any())
        {
            emailText.AppendLine("**DISCREPANCIES NOTED**");
            emailText.AppendLine();
            emailText.AppendLine("Part Number\tPacklist Qty\tReceived Qty\tDifference\tNote");
            emailText.AppendLine(new string('-', 80));

            foreach (var line in discrepancies)
            {
                int difference = line.ReceivedSkidCount - (line.ExpectedSkidCount ?? 0);
                string diffStr = difference > 0 ? $"+{difference}" : difference.ToString();
                emailText.AppendLine($"{line.PartNumber}\t{line.ExpectedSkidCount}\t{line.ReceivedSkidCount}\t{diffStr}\t{line.DiscrepancyNote ?? ""}");
            }

            emailText.AppendLine();
        }

        // Requested lines section
        emailText.AppendLine("Requested Lines:");
        emailText.AppendLine();
        emailText.AppendLine("Part Number\tQuantity (pcs)");
        emailText.AppendLine(new string('-', 40));

        foreach (var kvp in requestedLines.OrderBy(x => x.Key))
        {
            emailText.AppendLine($"{kvp.Key}\t{kvp.Value}");
        }

        emailText.AppendLine();

        // Notes section (if any)
        if (!string.IsNullOrWhiteSpace(shipment.Notes))
        {
            emailText.AppendLine("Additional Notes:");
            emailText.AppendLine(shipment.Notes);
            emailText.AppendLine();
        }

        // Signature
        emailText.AppendLine("Thank you,");
        emailText.AppendLine($"Employee #{shipment.EmployeeNumber}");

        await _logger.LogInformationAsync("Email text formatted");

        return emailText.ToString();
    }

    /// <summary>
    /// Saves shipment and lines with status='pending_po'
    /// Validates: Only one pending shipment allowed at a time
    /// </summary>
    public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines)
    {
        try
        {
            await _logger.LogInformationAsync("Saving Volvo shipment as pending PO");

            // Validate: Only one pending shipment allowed
            var existingPendingResult = await _shipmentDao.GetPendingAsync();
            if (existingPendingResult.Success && existingPendingResult.Data != null)
            {
                return new Model_Dao_Result<(int, int)>
                {
                    Success = false,
                    ErrorMessage = $"A pending shipment already exists (Shipment #{existingPendingResult.Data.ShipmentNumber} from {existingPendingResult.Data.ShipmentDate:MM/dd/yyyy}). Please complete it before creating a new one.",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            // Validate: At least one line required
            if (lines == null || lines.Count == 0)
            {
                return new Model_Dao_Result<(int, int)>
                {
                    Success = false,
                    ErrorMessage = "At least one part line is required",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            // Calculate and store piece counts for each line
            foreach (var line in lines)
            {
                var partResult = await _partDao.GetByIdAsync(line.PartNumber);
                if (!partResult.Success || partResult.Data == null)
                {
                    return new Model_Dao_Result<(int, int)>
                    {
                        Success = false,
                        ErrorMessage = $"Part {line.PartNumber} not found",
                        Severity = Enum_ErrorSeverity.Error
                    };
                }

                // Calculate parent part pieces only (components handled separately)
                var parentPart = partResult.Data;
                line.CalculatedPieceCount = line.ReceivedSkidCount * parentPart.QuantityPerSkid;
            }

            // Insert shipment
            var insertResult = await _shipmentDao.InsertAsync(shipment);
            if (!insertResult.Success)
            {
                return new Model_Dao_Result<(int, int)>
                {
                    Success = false,
                    ErrorMessage = insertResult.ErrorMessage,
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            int shipmentId = insertResult.Data.ShipmentId;
            int shipmentNumber = insertResult.Data.ShipmentNumber;

            // Insert lines
            foreach (var line in lines)
            {
                line.ShipmentId = shipmentId;
                var lineResult = await _lineDao.InsertAsync(line);
                if (!lineResult.Success)
                {
                    await _logger.LogErrorAsync($"Failed to insert shipment line: {lineResult.ErrorMessage}");
                    // Note: In production, consider transaction rollback here
                    return new Model_Dao_Result<(int, int)>
                    {
                        Success = false,
                        ErrorMessage = $"Failed to save shipment lines: {lineResult.ErrorMessage}",
                        Severity = Enum_ErrorSeverity.Error
                    };
                }
            }

            await _logger.LogInformationAsync($"Shipment saved successfully: ID={shipmentId}, Number={shipmentNumber}");

            return new Model_Dao_Result<(int, int)>
            {
                Success = true,
                Data = (shipmentId, shipmentNumber),
                AffectedRows = 1 + lines.Count
            };
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
            return new Model_Dao_Result<(int, int)>
            {
                Success = false,
                ErrorMessage = $"Error saving shipment: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Gets pending shipment if one exists
    /// </summary>
    public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync()
    {
        return await _shipmentDao.GetPendingAsync();
    }

    /// <summary>
    /// Gets pending shipment with all line items
    /// </summary>
    public async Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync()
    {
        try
        {
            var shipmentResult = await _shipmentDao.GetPendingAsync();
            if (!shipmentResult.Success)
            {
                return new Model_Dao_Result<(Model_VolvoShipment?, List<Model_VolvoShipmentLine>)>
                {
                    Success = false,
                    ErrorMessage = shipmentResult.ErrorMessage,
                    Severity = shipmentResult.Severity
                };
            }

            if (shipmentResult.Data == null)
            {
                return new Model_Dao_Result<(Model_VolvoShipment?, List<Model_VolvoShipmentLine>)>
                {
                    Success = true,
                    Data = (null, new List<Model_VolvoShipmentLine>())
                };
            }

            var linesResult = await _lineDao.GetByShipmentIdAsync(shipmentResult.Data.Id);
            if (!linesResult.Success)
            {
                return new Model_Dao_Result<(Model_VolvoShipment?, List<Model_VolvoShipmentLine>)>
                {
                    Success = false,
                    ErrorMessage = linesResult.ErrorMessage,
                    Severity = linesResult.Severity
                };
            }

            return new Model_Dao_Result<(Model_VolvoShipment?, List<Model_VolvoShipmentLine>)>
            {
                Success = true,
                Data = (shipmentResult.Data, linesResult.Data ?? new List<Model_VolvoShipmentLine>())
            };
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting pending shipment with lines: {ex.Message}", ex);
            return new Model_Dao_Result<(Model_VolvoShipment?, List<Model_VolvoShipmentLine>)>
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Completes shipment with PO and Receiver numbers
    /// </summary>
    public async Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Completing shipment {shipmentId} with PO={poNumber}, Receiver={receiverNumber}");

            // Validate inputs
            if (string.IsNullOrWhiteSpace(poNumber))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "PO Number is required",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            if (string.IsNullOrWhiteSpace(receiverNumber))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Receiver Number is required",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await _shipmentDao.CompleteAsync(shipmentId, poNumber, receiverNumber);

            if (result.Success)
            {
                await _logger.LogInformationAsync($"Shipment {shipmentId} completed successfully");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Error completing shipment: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Gets all active Volvo parts for dropdown population
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync()
    {
        return await _partDao.GetAllAsync(includeInactive: false);
    }

    /// <summary>
    /// Gets shipment history with filtering
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetShipmentHistoryAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all")
    {
        return await _shipmentDao.GetHistoryAsync(startDate, endDate, status);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
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
    private readonly IService_VolvoAuthorization _authService;

    public Service_Volvo(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        IService_LoggingUtility logger,
        IService_VolvoAuthorization authService)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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
    /// <param name="lines"></param>
    public async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        List<Model_VolvoShipmentLine> lines)
    {
        try
        {
            await _logger.LogInfoAsync("Calculating component explosion for shipment lines");

            var aggregatedPieces = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                // Get parent part details
                var partResult = await _partDao.GetByIdAsync(line.PartNumber);
                if (!partResult.IsSuccess || partResult.Data == null)
                {
                    return new Model_Dao_Result<Dictionary<string, int>>
                    {
                        Success = false,
                        ErrorMessage = $"Part {line.PartNumber} not found in master data",
                        Severity = Enum_ErrorSeverity.Error
                    };
                }

                var parentPart = partResult.Data;

                // Validate quantity per skid
                if (parentPart.QuantityPerSkid <= 0)
                {
                    return new Model_Dao_Result<Dictionary<string, int>>
                    {
                        Success = false,
                        ErrorMessage = $"Part {line.PartNumber} has invalid QuantityPerSkid: {parentPart.QuantityPerSkid} (must be > 0)",
                        Severity = Enum_ErrorSeverity.Error
                    };
                }

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
                if (componentsResult.IsSuccess && componentsResult.Data != null)
                {
                    foreach (var component in componentsResult.Data)
                    {
                        // Validate component quantities
                        if (component.Quantity <= 0 || component.ComponentQuantityPerSkid <= 0)
                        {
                            await _logger.LogWarningAsync(
                                $"Skipping component {component.ComponentPartNumber} with invalid quantity: " +
                                $"ComponentQty={component.Quantity}, QtyPerSkid={component.ComponentQuantityPerSkid}");
                            continue;
                        }

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

            await _logger.LogInfoAsync($"Component explosion complete: {aggregatedPieces.Count} unique parts");

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
    /// <param name="shipmentId"></param>
    public async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId)
    {
        try
        {
            // Authorization check
            var authResult = await _authService.CanGenerateLabelsAsync();
            if (!authResult.IsSuccess)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "You are not authorized to generate labels",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            // Validate shipmentId (prevent file path injection)
            if (shipmentId <= 0)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "Invalid shipment ID",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            await _logger.LogInfoAsync($"Generating label CSV for shipment {shipmentId}");

            // Get shipment details
            var shipmentResult = await _shipmentDao.GetByIdAsync(shipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
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
            if (!linesResult.IsSuccess || linesResult.Data == null)
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
            if (!explosionResult.IsSuccess || explosionResult.Data == null)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = explosionResult.ErrorMessage,
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            var aggregatedPieces = explosionResult.Data;

            // Sanity check: prevent extremely large CSV files
            const int MaxCsvLines = 10000;
            if (aggregatedPieces.Count > MaxCsvLines)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = $"CSV generation failed: Too many parts ({aggregatedPieces.Count} parts exceeds maximum of {MaxCsvLines})",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            // Create CSV directory
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string csvDirectory = Path.Combine(appDataPath, "MTM_Receiving_Application", "Volvo", "Labels");
            Directory.CreateDirectory(csvDirectory);

            // Use fixed filename Volvo_Labels.csv (overwrites previous)
            string fileName = "Volvo_Labels.csv";
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

            await _logger.LogInfoAsync($"Label CSV generated: {filePath}");

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
    /// <param name="shipment"></param>
    /// <param name="lines"></param>
    /// <param name="requestedLines"></param>
    public async Task<string> FormatEmailTextAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int>? requestedLines = null)
    {
        // Null guard for requestedLines
        requestedLines ??= new Dictionary<string, int>();

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
        if (discrepancies.Count > 0)
        {
            emailText.AppendLine("**DISCREPANCIES NOTED**");
            emailText.AppendLine();
            emailText.AppendLine("Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote");
            emailText.AppendLine(new string('-', 80));

            foreach (var line in discrepancies)
            {
                int expectedPieces = line.ExpectedPieceCount ?? 0;
                int receivedPieces = line.CalculatedPieceCount;
                int difference = receivedPieces - expectedPieces;
                string diffStr = difference > 0 ? $"+{difference}" : difference.ToString();
                emailText.AppendLine($"{line.PartNumber}\t{expectedPieces}\t{receivedPieces}\t{diffStr}\t{line.DiscrepancyNote ?? ""}");
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

        await _logger.LogInfoAsync("Email text formatted");

        return emailText.ToString();
    }

    /// <summary>
    /// Formats email data for PO requisition (structured for display and HTML conversion)
    /// </summary>
    /// <param name="shipment"></param>
    /// <param name="lines"></param>
    /// <param name="requestedLines"></param>
    public async Task<Model_VolvoEmailData> FormatEmailDataAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int>? requestedLines = null)
    {
        // Null guard for requestedLines
        requestedLines ??= new Dictionary<string, int>();

        var emailData = new Model_VolvoEmailData
        {
            Subject = $"PO Requisition - Volvo Dunnage - {shipment.ShipmentDate:MM/dd/yyyy} Shipment #{shipment.ShipmentNumber}",
            Greeting = "Good morning,",
            Message = $"Please create a PO for the following Volvo dunnage received on {shipment.ShipmentDate:MM/dd/yyyy}:",
            RequestedLines = requestedLines,
            AdditionalNotes = string.IsNullOrWhiteSpace(shipment.Notes) ? null : shipment.Notes,
            Signature = string.Empty
        };

        // Build discrepancy list
        var discrepancies = lines.Where(l => l.HasDiscrepancy).ToList();
        foreach (var line in discrepancies)
        {
            int expectedPieces = line.ExpectedPieceCount ?? 0;
            int receivedPieces = line.CalculatedPieceCount;
            int difference = receivedPieces - expectedPieces;
            emailData.Discrepancies.Add(new Model_VolvoEmailData.DiscrepancyLineItem
            {
                PartNumber = line.PartNumber,
                PacklistQty = expectedPieces,
                ReceivedQty = receivedPieces,
                Difference = difference,
                Note = line.DiscrepancyNote ?? string.Empty
            });
        }

        await _logger.LogInfoAsync("Email data formatted");
        return emailData;
    }

    /// <summary>
    /// Converts structured email data to HTML format with tables for Outlook paste
    /// </summary>
    /// <param name="emailData"></param>
    public string FormatEmailAsHtml(Model_VolvoEmailData emailData)
    {
        var html = new StringBuilder();

        html.AppendLine("<html>");
        html.AppendLine("<body style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");

        // Greeting
        html.AppendLine($"<p>{emailData.Greeting}</p>");
        html.AppendLine($"<p>{emailData.Message}</p>");

        // Discrepancy table
        if (emailData.Discrepancies.Count > 0)
        {
            html.AppendLine("<p><strong>**DISCREPANCIES NOTED**</strong></p>");
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
            html.AppendLine("<th>Part Number</th>");
            html.AppendLine("<th>Packlist Qty</th>");
            html.AppendLine("<th>Received Qty</th>");
            html.AppendLine("<th>Difference</th>");
            html.AppendLine("<th>Note</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var disc in emailData.Discrepancies)
            {
                string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{disc.PartNumber}</td>");
                html.AppendLine($"<td>{disc.PacklistQty}</td>");
                html.AppendLine($"<td>{disc.ReceivedQty}</td>");
                html.AppendLine($"<td>{diffStr}</td>");
                html.AppendLine($"<td>{disc.Note}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("<br/>");
        }

        // Requested Lines table
        html.AppendLine("<p><strong>Requested Lines:</strong></p>");
        html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
        html.AppendLine("<thead>");
        html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
        html.AppendLine("<th>Part Number</th>");
        html.AppendLine("<th>Quantity (pcs)</th>");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");

        foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{kvp.Key}</td>");
            html.AppendLine($"<td>{kvp.Value}</td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("<br/>");

        // Notes
        if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
        {
            html.AppendLine("<p><strong>Additional Notes:</strong></p>");
            html.AppendLine($"<p>{emailData.AdditionalNotes}</p>");
        }

        // Signature
        html.AppendLine($"<p>{emailData.Signature.Replace("\\n", "<br/>")}</p>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    /// <summary>
    /// Validates shipment data before save
    /// Centralized validation logic for data integrity
    /// </summary>
    /// <param name="shipment"></param>
    /// <param name="lines"></param>
    public async Task<Model_Dao_Result> ValidateShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines)
    {
        // Validate: At least one line required
        if (lines == null || lines.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure("At least one part line is required");
        }

        // Validate: All lines have valid data
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.PartNumber))
            {
                return Model_Dao_Result_Factory.Failure("All parts must have a part number");
            }

            if (line.ReceivedSkidCount < 1 || line.ReceivedSkidCount > 99)
            {
                return Model_Dao_Result_Factory.Failure(
                    $"Part {line.PartNumber}: skid count must be between 1 and 99");
            }

            if (line.HasDiscrepancy && !line.ExpectedSkidCount.HasValue)
            {
                return Model_Dao_Result_Factory.Failure(
                    $"Part {line.PartNumber} has discrepancy but no expected skid count");
            }
        }

        // Validate: Shipment date not in future
        if (shipment.ShipmentDate > DateTime.Now.AddDays(1))
        {
            return Model_Dao_Result_Factory.Failure("Shipment date cannot be in the future");
        }

        await _logger.LogInfoAsync("Shipment validation passed");
        return Model_Dao_Result_Factory.Success();
    }

    /// <summary>
    /// Saves shipment and lines with status='pending_po'
    /// Validates: Only one pending shipment allowed at a time
    /// </summary>
    /// <param name="shipment"></param>
    /// <param name="lines"></param>
    /// <param name="overwriteExisting">If true, deletes any existing pending shipment before saving</param>
    public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        bool overwriteExisting = false)
    {
        try
        {
            // Authorization check
            var authResult = await _authService.CanManageShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return new Model_Dao_Result<(int, int)>
                {
                    Success = false,
                    ErrorMessage = "You are not authorized to manage shipments",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            await _logger.LogInfoAsync("Saving Volvo shipment as pending PO");

            // Check for existing pending shipment
            var existingPendingResult = await _shipmentDao.GetPendingAsync();
            if (existingPendingResult.IsSuccess && existingPendingResult.Data != null)
            {
                if (overwriteExisting)
                {
                    // Delete the existing pending shipment
                    await _logger.LogInfoAsync($"Deleting existing pending shipment #{existingPendingResult.Data.ShipmentNumber}");
                    var deleteResult = await _shipmentDao.DeleteAsync(existingPendingResult.Data.Id);
                    if (!deleteResult.IsSuccess)
                    {
                        return new Model_Dao_Result<(int, int)>
                        {
                            Success = false,
                            ErrorMessage = $"Failed to delete existing shipment: {deleteResult.ErrorMessage}",
                            Severity = Enum_ErrorSeverity.Error
                        };
                    }
                }
                else
                {
                    // Return the existing shipment info so the UI can prompt for overwrite
                    return new Model_Dao_Result<(int, int)>
                    {
                        Success = false,
                        ErrorMessage = $"PENDING_EXISTS|{existingPendingResult.Data.ShipmentNumber}|{existingPendingResult.Data.ShipmentDate:MM/dd/yyyy}",
                        Data = (existingPendingResult.Data.Id, existingPendingResult.Data.ShipmentNumber),
                        Severity = Enum_ErrorSeverity.Warning
                    };
                }
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
                if (!partResult.IsSuccess || partResult.Data == null)
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
            if (!insertResult.IsSuccess)
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

            await _logger.LogInfoAsync($"Starting line insertion transaction for shipment {shipmentId}, {lines.Count} lines to insert");

            // Insert lines within transaction for data integrity
            await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                int lineIndex = 0;
                foreach (var line in lines)
                {
                    lineIndex++;
                    line.ShipmentId = shipmentId;

                    await _logger.LogInfoAsync($"Inserting line {lineIndex}/{lines.Count}: " +
                        $"Part={line.PartNumber}, " +
                        $"Skids={line.ReceivedSkidCount}, " +
                        $"Pieces={line.CalculatedPieceCount}, " +
                        $"HasDiscrepancy={line.HasDiscrepancy}, " +
                        $"ExpectedSkids={line.ExpectedSkidCount?.ToString() ?? "NULL"}, " +
                        $"Note={(!string.IsNullOrEmpty(line.DiscrepancyNote) ? "PROVIDED" : "NULL")}");

                    var parameters = new Dictionary<string, object>
                    {
                        { "shipment_id", line.ShipmentId },
                        { "part_number", line.PartNumber },
                        { "received_skid_count", line.ReceivedSkidCount },
                        { "calculated_piece_count", line.CalculatedPieceCount },
                        { "has_discrepancy", line.HasDiscrepancy ? 1 : 0 },
                        { "expected_skid_count", line.ExpectedSkidCount ?? (object)DBNull.Value },
                        { "discrepancy_note", line.DiscrepancyNote ?? (object)DBNull.Value }
                    };

                    await _logger.LogInfoAsync($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");

                    var lineResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                        connection,
                        (MySqlTransaction)transaction,
                        "sp_Volvo_ShipmentLine_Insert",
                        parameters);

                    if (!lineResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        await _logger.LogErrorAsync($"Failed to insert line {lineIndex} for part {line.PartNumber}");
                        await _logger.LogErrorAsync($"Error: {lineResult.ErrorMessage}");
                        if (lineResult.Exception != null)
                        {
                            await _logger.LogErrorAsync($"Exception Type: {lineResult.Exception.GetType().Name}");
                            await _logger.LogErrorAsync($"Exception Message: {lineResult.Exception.Message}");
                            if (lineResult.Exception.InnerException != null)
                            {
                                await _logger.LogErrorAsync($"Inner Exception: {lineResult.Exception.InnerException.Message}");
                            }
                        }
                        return new Model_Dao_Result<(int, int)>
                        {
                            Success = false,
                            ErrorMessage = $"Failed to insert line for part {line.PartNumber}: {lineResult.ErrorMessage}",
                            Severity = Enum_ErrorSeverity.Error
                        };
                    }

                    await _logger.LogInfoAsync($"Line {lineIndex} inserted successfully");
                }

                await transaction.CommitAsync();
                await _logger.LogInfoAsync($"Transaction committed: Shipment {shipmentId} saved with {lines.Count} lines");

                return new Model_Dao_Result<(int, int)>
                {
                    Success = true,
                    Data = (shipmentId, shipmentNumber)
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logger.LogErrorAsync($"Transaction failed, rolled back: {ex.Message}", ex);
                return new Model_Dao_Result<(int, int)>
                {
                    Success = false,
                    ErrorMessage = $"Transaction failed: {ex.Message}",
                    Severity = Enum_ErrorSeverity.Error
                };
            }
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
        var result = await _shipmentDao.GetPendingAsync();
        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<Model_VolvoShipment?>(result.ErrorMessage);
        }
        return Model_Dao_Result_Factory.Success<Model_VolvoShipment?>(result.Data);
    }

    /// <summary>
    /// Gets pending shipment with all line items
    /// </summary>
    public async Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync()
    {
        try
        {
            var shipmentResult = await _shipmentDao.GetPendingAsync();
            if (!shipmentResult.IsSuccess)
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
            if (!linesResult.IsSuccess)
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
    /// <param name="shipmentId"></param>
    /// <param name="poNumber"></param>
    /// <param name="receiverNumber"></param>
    public async Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber)
    {
        try
        {
            // Authorization check
            var authResult = await _authService.CanCompleteShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "You are not authorized to complete shipments",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            await _logger.LogInfoAsync($"Completing shipment {shipmentId} with PO={poNumber}, Receiver={receiverNumber}");

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

            if (result.IsSuccess)
            {
                await _logger.LogInfoAsync($"Shipment {shipmentId} completed successfully");
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
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="status"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all")
    {
        return await _shipmentDao.GetHistoryAsync(startDate, endDate, status);
    }

    /// <summary>
    /// Gets all shipment lines for a specific shipment
    /// </summary>
    /// <param name="shipmentId"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetShipmentLinesAsync(int shipmentId)
    {
        return await _lineDao.GetByShipmentIdAsync(shipmentId);
    }

    /// <summary>
    /// Updates an existing shipment and its lines
    /// Regenerates CSV if applicable
    /// </summary>
    /// <param name="shipment"></param>
    /// <param name="lines"></param>
    public async Task<Model_Dao_Result> UpdateShipmentAsync(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines)
    {
        try
        {
            // Update shipment header
            var shipmentResult = await _shipmentDao.UpdateAsync(shipment);
            if (!shipmentResult.IsSuccess)
            {
                return shipmentResult;
            }

            // Delete existing lines
            var existingLines = await _lineDao.GetByShipmentIdAsync(shipment.Id);
            if (existingLines.IsSuccess && existingLines.Data != null)
            {
                foreach (var line in existingLines.Data)
                {
                    await _lineDao.DeleteAsync(line.Id);
                }
            }

            // Insert updated lines
            foreach (var line in lines)
            {
                line.ShipmentId = shipment.Id;
                var lineResult = await _lineDao.InsertAsync(line);
                if (!lineResult.IsSuccess)
                {
                    await _logger.LogErrorAsync(
                        $"Failed to insert line: {lineResult.ErrorMessage}",
                        null,
                        nameof(UpdateShipmentAsync));
                }
            }

            // Regenerate CSV if completed
            if (shipment.Status == "completed" && !string.IsNullOrEmpty(shipment.PONumber))
            {
                await GenerateLabelCsvAsync(shipment.Id);
            }

            return Model_Dao_Result_Factory.Success("Shipment updated successfully");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error updating shipment: {ex.Message}",
                ex,
                nameof(UpdateShipmentAsync));
            return Model_Dao_Result_Factory.Failure($"Error updating shipment: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports shipment history to CSV format
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="status"></param>
    public async Task<Model_Dao_Result<string>> ExportHistoryToCsvAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all")
    {
        try
        {
            var historyResult = await GetHistoryAsync(startDate, endDate, status);
            if (!historyResult.IsSuccess || historyResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<string>("Failed to retrieve history data");
            }

            var csv = new StringBuilder();
            csv.AppendLine("ShipmentNumber,Date,PONumber,ReceiverNumber,Status,EmployeeNumber,Notes");

            foreach (var shipment in historyResult.Data)
            {
                csv.AppendLine($"{shipment.ShipmentNumber}," +
                              $"{shipment.ShipmentDate:yyyy-MM-dd}," +
                              $"{EscapeCsv(shipment.PONumber)}," +
                              $"{EscapeCsv(shipment.ReceiverNumber)}," +
                              $"{shipment.Status}," +
                              $"{shipment.EmployeeNumber}," +
                              $"{EscapeCsv(shipment.Notes)}");
            }

            return Model_Dao_Result_Factory.Success(csv.ToString());
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error exporting history: {ex.Message}",
                ex,
                nameof(ExportHistoryToCsvAsync));
            return Model_Dao_Result_Factory.Failure<string>($"Error exporting history: {ex.Message}");
        }
    }

    /// <summary>
    /// Escapes CSV values (wraps in quotes if contains comma, quote, or newline)
    /// </summary>
    /// <param name="value"></param>
    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Helpers;

/// <summary>
/// Helper methods for Volvo shipment calculations and CSV generation.
/// </summary>
public static class Helper_VolvoShipmentCalculations
{
    /// <summary>
    /// Calculates component explosion and aggregates piece counts for shipment lines.
    /// </summary>
    /// <param name="partDao"></param>
    /// <param name="componentDao"></param>
    /// <param name="lines"></param>
    /// <param name="logger"></param>
    public static async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        List<Model_VolvoShipmentLine> lines,
        IService_LoggingUtility? logger = null)
    {
        try
        {
            if (lines == null || lines.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>("No shipment lines provided");
            }

            var aggregatedPieces = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                var partResult = await partDao.GetByIdAsync(line.PartNumber);
                if (!partResult.IsSuccess || partResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                        $"Part {line.PartNumber} not found in master data");
                }

                var parentPart = partResult.Data;

                if (parentPart.QuantityPerSkid <= 0)
                {
                    return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                        $"Part {line.PartNumber} has invalid QuantityPerSkid: {parentPart.QuantityPerSkid} (must be > 0)");
                }

                var parentPieces = line.ReceivedSkidCount * parentPart.QuantityPerSkid;
                if (aggregatedPieces.ContainsKey(line.PartNumber))
                {
                    aggregatedPieces[line.PartNumber] += parentPieces;
                }
                else
                {
                    aggregatedPieces[line.PartNumber] = parentPieces;
                }

                var componentsResult = await componentDao.GetByParentPartAsync(line.PartNumber);
                if (componentsResult.IsSuccess && componentsResult.Data != null)
                {
                    foreach (var component in componentsResult.Data)
                    {
                        if (component.Quantity <= 0 || component.ComponentQuantityPerSkid <= 0)
                        {
                            if (logger != null)
                            {
                                await logger.LogWarningAsync(
                                    $"Skipping component {component.ComponentPartNumber} with invalid quantity: " +
                                    $"ComponentQty={component.Quantity}, QtyPerSkid={component.ComponentQuantityPerSkid}");
                            }
                            continue;
                        }

                        var componentPieces = line.ReceivedSkidCount * component.Quantity * component.ComponentQuantityPerSkid;

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

            return Model_Dao_Result_Factory.Success(aggregatedPieces);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                $"Error calculating component explosion: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates CSV label file for a shipment and returns the file path.
    /// </summary>
    /// <param name="shipmentDao"></param>
    /// <param name="lineDao"></param>
    /// <param name="partDao"></param>
    /// <param name="componentDao"></param>
    /// <param name="authService"></param>
    /// <param name="logger"></param>
    /// <param name="shipmentId"></param>
    public static async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        IService_VolvoAuthorization authService,
        IService_LoggingUtility logger,
        int shipmentId)
    {
        try
        {
            var authResult = await authService.CanGenerateLabelsAsync();
            if (!authResult.IsSuccess)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "You are not authorized to generate labels",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            if (shipmentId <= 0)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "Invalid shipment ID",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            await logger.LogInfoAsync($"Generating label CSV for shipment {shipmentId}");

            var shipmentResult = await shipmentDao.GetByIdAsync(shipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "Shipment not found",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            var linesResult = await lineDao.GetByShipmentIdAsync(shipmentId);
            if (!linesResult.IsSuccess || linesResult.Data == null)
            {
                return new Model_Dao_Result<string>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve shipment lines",
                    Severity = Enum_ErrorSeverity.Error
                };
            }

            var explosionResult = await CalculateComponentExplosionAsync(partDao, componentDao, linesResult.Data, logger);
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

            var shipment = shipmentResult.Data;

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string csvDirectory = Path.Combine(appDataPath, "MTM_Receiving_Application", "Volvo", "Labels");
            Directory.CreateDirectory(csvDirectory);

            string fileName = "Volvo_Labels.csv"; // TODO(SpreadsheetRemoval): Legacy filename reference - update when MySQL export implemented
            string filePath = Path.Combine(csvDirectory, fileName);

            var csvContent = new StringBuilder();
            csvContent.AppendLine("Material,Quantity,Employee,Date,Time,Receiver,Notes");

            string dateFormatted = shipment.ShipmentDate.ToString("MM/dd/yyyy");
            string timeFormatted = DateTime.Now.ToString("HH:mm:ss");

            foreach (var kvp in aggregatedPieces.OrderBy(x => x.Key))
            {
                csvContent.AppendLine($"{kvp.Key},{kvp.Value},{shipment.EmployeeNumber},{dateFormatted},{timeFormatted},,");
            }

            await File.WriteAllTextAsync(filePath, csvContent.ToString());

            await logger.LogInfoAsync($"Label CSV generated: {filePath}");

            return new Model_Dao_Result<string>
            {
                Success = true,
                Data = filePath
            };
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync($"Error generating label CSV: {ex.Message}", ex);
            return new Model_Dao_Result<string>
            {
                Success = false,
                ErrorMessage = $"Error generating label CSV: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }
}

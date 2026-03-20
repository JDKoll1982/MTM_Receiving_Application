using System;
using System.Collections.Generic;
using System.Linq;
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
/// Helper methods for Volvo shipment calculations and label generation.
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
    public static async Task<
        Model_Dao_Result<Dictionary<string, int>>
    > CalculateComponentExplosionAsync(
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        List<Model_VolvoShipmentLine> lines,
        IService_LoggingUtility? logger = null
    )
    {
        try
        {
            if (lines == null || lines.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                    "No shipment lines provided"
                );
            }

            var aggregatedPieces = new Dictionary<string, int>();

            foreach (var line in lines)
            {
                var partResult = await partDao.GetByIdAsync(line.PartNumber);
                if (!partResult.IsSuccess || partResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                        $"Part {line.PartNumber} not found in master data"
                    );
                }

                var parentPart = partResult.Data;

                if (parentPart.QuantityPerSkid <= 0)
                {
                    return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                        $"Part {line.PartNumber} has invalid QuantityPerSkid: {parentPart.QuantityPerSkid} (must be > 0)"
                    );
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
                                    $"Skipping component {component.ComponentPartNumber} with invalid quantity: "
                                        + $"ComponentQty={component.Quantity}, QtyPerSkid={component.ComponentQuantityPerSkid}"
                                );
                            }
                            continue;
                        }

                        var componentPieces =
                            line.ReceivedSkidCount
                            * component.Quantity
                            * component.ComponentQuantityPerSkid;

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
                $"Error calculating component explosion: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Generates label data for a shipment by loading the shipment and its lines from the
    /// database, running the component explosion, and returning a human-readable summary.
    /// All data lives in the database (volvo_label_data / volvo_line_data); no files are written.
    /// </summary>
    /// <param name="shipmentDao">DAO for shipment header data.</param>
    /// <param name="lineDao">DAO for shipment line data.</param>
    /// <param name="partDao">DAO for part data.</param>
    /// <param name="componentDao">DAO for part component data.</param>
    /// <param name="authService">Authorization service to verify label generation rights.</param>
    /// <param name="logger">Logging utility.</param>
    /// <param name="shipmentId">Unique identifier of the shipment to process.</param>
    public static async Task<Model_Dao_Result<string>> GenerateLabelAsync(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        IService_VolvoAuthorization authService,
        IService_LoggingUtility logger,
        int shipmentId
    )
    {
        try
        {
            var authResult = await authService.CanGenerateLabelsAsync();
            if (!authResult.Success)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    authResult.ErrorMessage ?? "You are not authorized to generate labels."
                );
            }

            var shipmentResult = await shipmentDao.GetByIdAsync(shipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    shipmentResult.ErrorMessage ?? "Shipment not found"
                );
            }

            var linesResult = await lineDao.GetByShipmentIdAsync(shipmentId);
            if (!linesResult.IsSuccess || linesResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    linesResult.ErrorMessage ?? "Failed to retrieve shipment lines"
                );
            }

            if (linesResult.Data.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    "Shipment has no lines to generate labels for"
                );
            }

            var explosionResult = await CalculateComponentExplosionAsync(
                partDao,
                componentDao,
                linesResult.Data,
                logger
            );

            if (!explosionResult.IsSuccess || explosionResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    explosionResult.ErrorMessage ?? "Component explosion failed"
                );
            }

            var shipment = shipmentResult.Data;
            var aggregatedPieces = explosionResult.Data;
            int totalPieces = aggregatedPieces.Values.Sum();

            var summary =
                $"Shipment #{shipment.ShipmentNumber} ({shipment.ShipmentDate:MM/dd/yyyy}): "
                + $"{aggregatedPieces.Count} part(s), {totalPieces:N0} total pieces";

            await logger.LogInfoAsync(
                $"Label data verified for shipment {shipmentId}: {aggregatedPieces.Count} parts, {totalPieces} pieces"
            );

            return Model_Dao_Result_Factory.Success(summary);
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync(
                $"Error generating labels for shipment {shipmentId}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<string>(
                $"Error generating labels: {ex.Message}",
                ex
            );
        }
    }
}

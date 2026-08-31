using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Service for the Delivery Schedule tool. Thin facade over
/// <see cref="IService_InforVisual.GetDeliveryScheduleLinesAsync"/> that maps the
/// raw Infor Visual rows to presentation models for the schedule grid.
/// </summary>
public class Service_Tool_DeliverySchedule : IService_Tool_DeliverySchedule
{
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_DeliverySchedule(
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_DeliveryScheduleLine>>> SearchAsync(
        Model_InforVisualDeliveryScheduleFilter filter
    )
    {
        var result = await _inforVisual.GetDeliveryScheduleLinesAsync(filter);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"DeliverySchedule: failed to load grid. {result.ErrorMessage}",
                result.Exception
            );
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_DeliveryScheduleLine>>(
                result.ErrorMessage,
                result.Exception
            );
        }

        var rows = MapRows(result.Data ?? []);
        _logger.LogInfo($"DeliverySchedule: returning {rows.Count} grid rows.");
        return Model_Dao_Result_Factory.Success(rows);
    }

    private static List<Model_Tool_DeliveryScheduleLine> MapRows(
        IReadOnlyList<Model_InforVisualDeliveryScheduleLine> source
    )
    {
        var rows = new List<Model_Tool_DeliveryScheduleLine>(source.Count);
        foreach (var line in source)
        {
            rows.Add(
                new Model_Tool_DeliveryScheduleLine
                {
                    PoNumber = line.PoNumber,
                    VendorName = line.VendorName,
                    PoDesiredDate = line.PoDesiredDate,
                    PoPromiseDate = line.PoPromiseDate,
                    OrderDate = line.OrderDate,
                    Carrier = line.Carrier,
                    PartNumber = line.PartNumber,
                    OrderQty = line.OrderQty,
                    ReceivedQty = line.ReceivedQty,
                    RemainingQty = line.RemainingQty,
                    LineDesiredDate = line.LineDesiredDate,
                    LinePromiseDate = line.LinePromiseDate,
                    PoStatus = line.PoStatus,
                    LineStatus = line.LineStatus,
                    ReceivedBy = line.ReceivedBy,
                    DueDate = line.DueDate,
                    Category = line.Category,
                    DeliveryState = line.DeliveryState,
                    PoState = line.PoState,
                }
            );
        }

        return rows;
    }
}

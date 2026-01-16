using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for SavePendingShipmentCommand - saves shipment as pending for later resumption.
/// </summary>
public class SavePendingShipmentCommandHandler : IRequestHandler<SavePendingShipmentCommand, Model_Dao_Result<int>>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;

    public SavePendingShipmentCommandHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
    }

    public async Task<Model_Dao_Result<int>> Handle(SavePendingShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create shipment model
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = request.ShipmentDate.DateTime,
                ShipmentNumber = request.ShipmentNumber,
                Notes = request.Notes,
                Status = "Pending",
                EmployeeNumber = Environment.UserName // TODO: Get from session/auth
            };

            Model_Dao_Result<(int ShipmentId, int ShipmentNumber)> insertResult;

            if (request.ShipmentId.HasValue && request.ShipmentId.Value > 0)
            {
                // Update existing pending shipment
                shipment.Id = request.ShipmentId.Value;
                var updateResult = await _shipmentDao.UpdateAsync(shipment);
                
                if (!updateResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(updateResult.ErrorMessage);
                }

                insertResult = new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
                {
                    Success = true,
                    Data = (request.ShipmentId.Value, request.ShipmentNumber)
                };
            }
            else
            {
                // Insert new pending shipment
                insertResult = await _shipmentDao.InsertAsync(shipment);

                if (!insertResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(insertResult.ErrorMessage);
                }
            }

            var shipmentId = insertResult.Data.ShipmentId;

            // Insert shipment lines
            foreach (var partDto in request.Parts)
            {
                var line = new Model_VolvoShipmentLine
                {
                    ShipmentId = shipmentId,
                    PartNumber = partDto.PartNumber,
                    ReceivedSkidCount = partDto.ReceivedSkidCount,
                    ExpectedSkidCount = partDto.ExpectedSkidCount,
                    HasDiscrepancy = partDto.HasDiscrepancy,
                    DiscrepancyNote = partDto.DiscrepancyNote ?? string.Empty
                };

                var lineResult = await _lineDao.InsertAsync(line);
                if (!lineResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        $"Failed to insert line for part {partDto.PartNumber}: {lineResult.ErrorMessage}");
                }
            }

            return new Model_Dao_Result<int>
            {
                Success = true,
                Data = shipmentId
            };
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Unexpected error saving pending shipment: {ex.Message}", ex);
        }
    }
}

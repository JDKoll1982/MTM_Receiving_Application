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
    private readonly Dao_VolvoPart _partDao;

    public SavePendingShipmentCommandHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
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

            Model_Dao_Result<(int ShipmentId, int ShipmentNumber)> saveResult;

            if (request.ShipmentId.HasValue && request.ShipmentId.Value > 0)
            {
                // Update existing pending shipment by ID
                shipment.Id = request.ShipmentId.Value;
                var updateResult = await _shipmentDao.UpdateAsync(shipment);

                if (!updateResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(updateResult.ErrorMessage);
                }

                saveResult = new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
                {
                    Success = true,
                    Data = (request.ShipmentId.Value, request.ShipmentNumber)
                };
            }
            else
            {
                // If a pending shipment already exists, update it instead of inserting a new one
                var pendingResult = await _shipmentDao.GetPendingAsync();
                if (!pendingResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(pendingResult.ErrorMessage);
                }

                if (pendingResult.Data != null)
                {
                    shipment.Id = pendingResult.Data.Id;
                    var updateResult = await _shipmentDao.UpdateAsync(shipment);

                    if (!updateResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure<int>(updateResult.ErrorMessage);
                    }

                    saveResult = new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
                    {
                        Success = true,
                        Data = (pendingResult.Data.Id, pendingResult.Data.ShipmentNumber)
                    };
                }
                else
                {
                    // Insert new pending shipment
                    saveResult = await _shipmentDao.InsertAsync(shipment);

                    if (!saveResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure<int>(saveResult.ErrorMessage);
                    }
                }
            }

            var shipmentId = saveResult.Data.ShipmentId;

            // Replace existing lines if shipment already existed
            var existingLinesResult = await _lineDao.GetByShipmentIdAsync(shipmentId);
            if (!existingLinesResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(existingLinesResult.ErrorMessage);
            }

            foreach (var existingLine in existingLinesResult.Data ?? Enumerable.Empty<Model_VolvoShipmentLine>())
            {
                var deleteResult = await _lineDao.DeleteAsync(existingLine.Id);
                if (!deleteResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        $"Failed to delete line {existingLine.Id}: {deleteResult.ErrorMessage}");
                }
            }

            // Insert shipment lines
            foreach (var partDto in request.Parts)
            {
                var partResult = await _partDao.GetByIdAsync(partDto.PartNumber);
                if (!partResult.IsSuccess || partResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        partResult.ErrorMessage ?? $"Part '{partDto.PartNumber}' not found in master data");
                }

                var quantityPerSkid = partResult.Data.QuantityPerSkid;
                var calculatedPieceCount = quantityPerSkid * partDto.ReceivedSkidCount;

                var line = new Model_VolvoShipmentLine
                {
                    ShipmentId = shipmentId,
                    PartNumber = partDto.PartNumber,
                    QuantityPerSkid = quantityPerSkid,
                    ReceivedSkidCount = partDto.ReceivedSkidCount,
                    CalculatedPieceCount = calculatedPieceCount,
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

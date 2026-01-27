using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for CompleteShipmentCommand - finalizes shipment, generates labels, sends email.
/// </summary>
public class CompleteShipmentCommandHandler : IRequestHandler<CompleteShipmentCommand, Model_Dao_Result<int>>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly Dao_VolvoPart _partDao;
    private readonly IService_VolvoAuthorization _authService;

    public CompleteShipmentCommandHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        IService_VolvoAuthorization authService)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<Model_Dao_Result<int>> Handle(CompleteShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Authorization check
            var authResult = await _authService.CanCompleteShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    "You are not authorized to complete shipments");
            }

            // If a pending shipment exists, complete it instead of inserting a new one
            var pendingResult = await _shipmentDao.GetPendingAsync();
            if (!pendingResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(pendingResult.ErrorMessage);
            }

            if (pendingResult.Data != null)
            {
                var pendingShipment = pendingResult.Data;

                // Update notes if changed
                if (!string.Equals(pendingShipment.Notes ?? string.Empty, request.Notes ?? string.Empty, StringComparison.Ordinal))
                {
                    var updateResult = await _shipmentDao.UpdateAsync(new Model_VolvoShipment
                    {
                        Id = pendingShipment.Id,
                        Notes = request.Notes
                    });

                    if (!updateResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure<int>(updateResult.ErrorMessage);
                    }
                }

                // Replace existing lines with current request parts
                var existingLinesResult = await _lineDao.GetByShipmentIdAsync(pendingShipment.Id);
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

                foreach (var partDataTransferObjects in request.Parts)
                {
                    var partResult = await _partDao.GetByIdAsync(partDataTransferObjects.PartNumber);
                    if (!partResult.IsSuccess || partResult.Data == null)
                    {
                        return Model_Dao_Result_Factory.Failure<int>(
                            partResult.ErrorMessage ?? $"Part '{partDataTransferObjects.PartNumber}' not found in master data");
                    }

                    var quantityPerSkid = partResult.Data.QuantityPerSkid;
                    var calculatedPieceCount = quantityPerSkid * partDataTransferObjects.ReceivedSkidCount;

                    var line = new Model_VolvoShipmentLine
                    {
                        ShipmentId = pendingShipment.Id,
                        PartNumber = partDataTransferObjects.PartNumber,
                        QuantityPerSkid = quantityPerSkid,
                        ReceivedSkidCount = partDataTransferObjects.ReceivedSkidCount,
                        CalculatedPieceCount = calculatedPieceCount,
                        ExpectedSkidCount = partDataTransferObjects.ExpectedSkidCount,
                        HasDiscrepancy = partDataTransferObjects.HasDiscrepancy,
                        DiscrepancyNote = partDataTransferObjects.DiscrepancyNote ?? string.Empty
                    };

                    var lineResult = await _lineDao.InsertAsync(line);
                    if (!lineResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure<int>(
                            $"Failed to insert line for part {partDataTransferObjects.PartNumber}: {lineResult.ErrorMessage}");
                    }
                }


                var completeResult = await _shipmentDao.CompleteAsync(
                    pendingShipment.Id,
                    request.PONumber,
                    request.ReceiverNumber);

                if (!completeResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(completeResult.ErrorMessage);
                }

                return new Model_Dao_Result<int> { Success = true, Data = pendingShipment.Id };
            }

            // No pending shipment exists: insert then complete
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = request.ShipmentDate.DateTime,
                ShipmentNumber = request.ShipmentNumber,
                Notes = request.Notes,
                Status = "Completed",
                PONumber = request.PONumber,
                ReceiverNumber = request.ReceiverNumber,
                EmployeeNumber = Environment.UserName
            };

            var insertResult = await _shipmentDao.InsertAsync(shipment);
            if (!insertResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(insertResult.ErrorMessage);
            }

            var shipmentId = insertResult.Data.ShipmentId;

            foreach (var partDataTransferObjects in request.Parts)
            {
                var partResult = await _partDao.GetByIdAsync(partDataTransferObjects.PartNumber);
                if (!partResult.IsSuccess || partResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        partResult.ErrorMessage ?? $"Part '{partDataTransferObjects.PartNumber}' not found in master data");
                }

                var quantityPerSkid = partResult.Data.QuantityPerSkid;
                var calculatedPieceCount = quantityPerSkid * partDataTransferObjects.ReceivedSkidCount;

                var line = new Model_VolvoShipmentLine
                {
                    ShipmentId = shipmentId,
                    PartNumber = partDataTransferObjects.PartNumber,
                    QuantityPerSkid = quantityPerSkid,
                    ReceivedSkidCount = partDataTransferObjects.ReceivedSkidCount,
                    CalculatedPieceCount = calculatedPieceCount,
                    ExpectedSkidCount = partDataTransferObjects.ExpectedSkidCount,
                    HasDiscrepancy = partDataTransferObjects.HasDiscrepancy,
                    DiscrepancyNote = partDataTransferObjects.DiscrepancyNote ?? string.Empty
                };

                var lineResult = await _lineDao.InsertAsync(line);
                if (!lineResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        $"Failed to insert line for part {partDataTransferObjects.PartNumber}: {lineResult.ErrorMessage}");
                }
            }

            var completeInsertResult = await _shipmentDao.CompleteAsync(shipmentId, request.PONumber, request.ReceiverNumber);
            if (!completeInsertResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(completeInsertResult.ErrorMessage);
            }

            return new Model_Dao_Result<int> { Success = true, Data = shipmentId };
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Unexpected error completing shipment: {ex.Message}", ex);
        }
    }
}

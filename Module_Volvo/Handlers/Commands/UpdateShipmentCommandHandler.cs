using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for UpdateShipmentCommand - updates shipment header and lines.
/// </summary>
public class UpdateShipmentCommandHandler : IRequestHandler<UpdateShipmentCommand, Model_Dao_Result>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;
    private readonly IService_VolvoAuthorization _authService;
    private readonly IService_LoggingUtility _logger;

    public UpdateShipmentCommandHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        IService_VolvoAuthorization authService,
        IService_LoggingUtility logger)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result> Handle(UpdateShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure(
                    shipmentResult.ErrorMessage ?? "Shipment not found");
            }

            var shipment = shipmentResult.Data;
            shipment.ShipmentDate = request.ShipmentDate.DateTime;
            shipment.Notes = request.Notes;
            shipment.PONumber = request.PONumber;
            shipment.ReceiverNumber = request.ReceiverNumber;

            var updateResult = await _shipmentDao.UpdateAsync(shipment);
            if (!updateResult.IsSuccess)
            {
                return updateResult;
            }

            var existingLines = await _lineDao.GetByShipmentIdAsync(shipment.Id);
            if (existingLines.IsSuccess && existingLines.Data != null)
            {
                foreach (var line in existingLines.Data)
                {
                    var deleteResult = await _lineDao.DeleteAsync(line.Id);
                    if (!deleteResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure(
                            deleteResult.ErrorMessage ?? $"Failed to delete line {line.Id}");
                    }
                }
            }

            var newLines = new List<Model_VolvoShipmentLine>();
            foreach (var part in request.Parts)
            {
                var partResult = await _partDao.GetByIdAsync(part.PartNumber);
                if (!partResult.IsSuccess || partResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure(
                        partResult.ErrorMessage ?? $"Part '{part.PartNumber}' not found in master data");
                }

                var quantityPerSkid = partResult.Data.QuantityPerSkid;

                var line = new Model_VolvoShipmentLine
                {
                    ShipmentId = shipment.Id,
                    PartNumber = part.PartNumber,
                    QuantityPerSkid = quantityPerSkid,
                    ReceivedSkidCount = part.ReceivedSkidCount,
                    CalculatedPieceCount = quantityPerSkid * part.ReceivedSkidCount,
                    ExpectedSkidCount = part.ExpectedSkidCount,
                    HasDiscrepancy = part.HasDiscrepancy,
                    DiscrepancyNote = part.DiscrepancyNote
                };

                newLines.Add(line);
            }

            foreach (var line in newLines)
            {
                var insertResult = await _lineDao.InsertAsync(line);
                if (!insertResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(
                        insertResult.ErrorMessage ?? $"Failed to insert line for part {line.PartNumber}");
                }
            }

            if (shipment.Status == VolvoShipmentStatus.Completed && !string.IsNullOrWhiteSpace(shipment.PONumber))
            {
                await Helper_VolvoShipmentCalculations.GenerateLabelAsync(
                    _shipmentDao,
                    _lineDao,
                    _partDao,
                    _componentDao,
                    _authService,
                    _logger,
                    shipment.Id);
            }

            return Model_Dao_Result_Factory.Success("Shipment updated successfully");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Unexpected error updating shipment: {ex.Message}", ex);
        }
    }
}

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
    private readonly IService_VolvoAuthorization _authService;

    public CompleteShipmentCommandHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        IService_VolvoAuthorization authService)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
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

            // Create shipment model
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = request.ShipmentDate.DateTime,
                ShipmentNumber = request.ShipmentNumber,
                Notes = request.Notes,
                Status = "Completed",
                PONumber = request.PONumber,
                ReceiverNumber = request.ReceiverNumber,
                EmployeeNumber = Environment.UserName // TODO: Get from session/auth
            };

            // Insert shipment
            var insertResult = await _shipmentDao.InsertAsync(shipment);

            if (!insertResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(insertResult.ErrorMessage);
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

            // Complete shipment (updates status, adds PO/Receiver)
            var completeResult = await _shipmentDao.CompleteAsync(shipmentId, request.PONumber, request.ReceiverNumber);

            if (!completeResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(completeResult.ErrorMessage);
            }

            // TODO: Generate labels and send email (will be handled by ViewModel after successful completion)
            // The ViewModel will call GenerateLabelCsvQuery and FormatEmailDataQuery

            return new Model_Dao_Result<int>
            {
                Success = true,
                Data = shipmentId
            };
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Unexpected error completing shipment: {ex.Message}", ex);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for GetShipmentDetailQuery - retrieves shipment header and lines.
/// </summary>
public class GetShipmentDetailQueryHandler
    : IRequestHandler<GetShipmentDetailQuery, Model_Dao_Result<ShipmentDetail>>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly IDao_VolvoLabelHistory _historyDao;

    public GetShipmentDetailQueryHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        IDao_VolvoLabelHistory historyDao
    )
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _historyDao = historyDao ?? throw new ArgumentNullException(nameof(historyDao));
    }

    public async Task<Model_Dao_Result<ShipmentDetail>> Handle(
        GetShipmentDetailQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            Model_Dao_Result<Model_VolvoShipment?> shipmentResult;
            Model_Dao_Result<List<Model_VolvoShipmentLine>> linesResult;

            if (request.IsArchived)
            {
                shipmentResult = await _historyDao.GetArchivedShipmentByIdAsync(request.ShipmentId);
                if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure<ShipmentDetail>(
                        shipmentResult.ErrorMessage ?? "Archived shipment not found"
                    );
                }

                linesResult = await _historyDao.GetArchivedLinesByShipmentHistoryIdAsync(
                    request.ShipmentId
                );
            }
            else
            {
                shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
                if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure<ShipmentDetail>(
                        shipmentResult.ErrorMessage ?? "Shipment not found"
                    );
                }

                linesResult = await _lineDao.GetByShipmentIdAsync(request.ShipmentId);
            }

            if (!linesResult.IsSuccess || linesResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<ShipmentDetail>(
                    linesResult.ErrorMessage ?? "Failed to retrieve shipment lines"
                );
            }

            var detail = new ShipmentDetail
            {
                Shipment = shipmentResult.Data,
                Lines = linesResult.Data,
            };

            return Model_Dao_Result_Factory.Success(detail);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<ShipmentDetail>(
                $"Unexpected error retrieving shipment detail: {ex.Message}",
                ex
            );
        }
    }
}

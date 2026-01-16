using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for GetPendingShipmentQuery - retrieves pending shipment for current user.
/// </summary>
public class GetPendingShipmentQueryHandler : IRequestHandler<GetPendingShipmentQuery, Model_Dao_Result<Model_VolvoShipment>>
{
    private readonly Dao_VolvoShipment _shipmentDao;

    public GetPendingShipmentQueryHandler(Dao_VolvoShipment shipmentDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
    }

    public async Task<Model_Dao_Result<Model_VolvoShipment>> Handle(GetPendingShipmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get pending shipment (currently no user filtering in DAO, future enhancement)
            return await _shipmentDao.GetPendingAsync();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_VolvoShipment>(
                $"Unexpected error retrieving pending shipment: {ex.Message}", ex);
        }
    }
}

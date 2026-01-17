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
/// Handler for GetRecentShipmentsQuery - retrieves recent shipment history.
/// </summary>
public class GetRecentShipmentsQueryHandler : IRequestHandler<GetRecentShipmentsQuery, Model_Dao_Result<List<Model_VolvoShipment>>>
{
    private readonly Dao_VolvoShipment _shipmentDao;

    public GetRecentShipmentsQueryHandler(Dao_VolvoShipment shipmentDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> Handle(GetRecentShipmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var days = request.Days <= 0 ? 30 : request.Days;
            var startDate = DateTime.Now.AddDays(-days).Date;
            var endDate = DateTime.Now.Date;

            return await _shipmentDao.GetHistoryAsync(startDate, endDate, "all");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoShipment>>(
                $"Unexpected error retrieving recent shipments: {ex.Message}", ex);
        }
    }
}

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
/// Handler for GetShipmentHistoryQuery - retrieves shipment history with filters.
/// </summary>
public class GetShipmentHistoryQueryHandler : IRequestHandler<GetShipmentHistoryQuery, Model_Dao_Result<List<Model_VolvoShipment>>>
{
    private readonly Dao_VolvoShipment _shipmentDao;

    public GetShipmentHistoryQueryHandler(Dao_VolvoShipment shipmentDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> Handle(GetShipmentHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var startDate = (request.StartDate ?? DateTimeOffset.Now.AddDays(-30)).DateTime.Date;
            var endDate = (request.EndDate ?? DateTimeOffset.Now).DateTime.Date;
            var statusFilter = NormalizeStatus(request.StatusFilter);

            return await _shipmentDao.GetHistoryAsync(startDate, endDate, statusFilter);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoShipment>>(
                $"Unexpected error retrieving shipment history: {ex.Message}", ex);
        }
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "all";
        }

        if (status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return "all";
        }

        if (status.Equals("Pending PO", StringComparison.OrdinalIgnoreCase))
        {
            return VolvoShipmentStatus.PendingPo;
        }

        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            return VolvoShipmentStatus.Completed;
        }

        return status;
    }
}

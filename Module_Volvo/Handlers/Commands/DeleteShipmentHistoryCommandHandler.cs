using System;
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
/// Deletes a completed archived Volvo shipment header and its archived lines.
/// </summary>
public class DeleteShipmentHistoryCommandHandler
    : IRequestHandler<DeleteShipmentHistoryCommand, Model_Dao_Result>
{
    private readonly IDao_VolvoLabelHistory _historyDao;
    private readonly IService_VolvoAuthorization _authService;

    public DeleteShipmentHistoryCommandHandler(
        IDao_VolvoLabelHistory historyDao,
        IService_VolvoAuthorization authService
    )
    {
        _historyDao = historyDao ?? throw new ArgumentNullException(nameof(historyDao));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<Model_Dao_Result> Handle(
        DeleteShipmentHistoryCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var authResult = await _authService.CanManageShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure(
                    "You are not authorized to delete shipment history"
                );
            }

            var shipmentResult = await _historyDao.GetArchivedShipmentByIdAsync(request.ShipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure(
                    shipmentResult.ErrorMessage ?? "Archived shipment not found"
                );
            }

            var normalizedStatus = VolvoShipmentStatus.NormalizeStorageValue(
                shipmentResult.Data.Status
            );
            if (
                !shipmentResult.Data.IsArchived
                || normalizedStatus != VolvoShipmentStatus.Completed
            )
            {
                return Model_Dao_Result_Factory.Failure(
                    "Only completed archived shipments can be deleted from history"
                );
            }

            return await _historyDao.DeleteArchivedShipmentAsync(request.ShipmentId);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Unexpected error deleting shipment history: {ex.Message}",
                ex
            );
        }
    }
}

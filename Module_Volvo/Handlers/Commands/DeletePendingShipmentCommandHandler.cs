using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Deletes a pending Volvo shipment header and its lines.
/// </summary>
public class DeletePendingShipmentCommandHandler
    : IRequestHandler<DeletePendingShipmentCommand, Model_Dao_Result>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly IService_VolvoAuthorization _authService;

    public DeletePendingShipmentCommandHandler(
        Dao_VolvoShipment shipmentDao,
        IService_VolvoAuthorization authService
    )
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<Model_Dao_Result> Handle(
        DeletePendingShipmentCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var authResult = await _authService.CanManageShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure(
                    "You are not authorized to delete pending shipments"
                );
            }

            return await _shipmentDao.DeleteAsync(request.ShipmentId);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Unexpected error deleting pending shipment: {ex.Message}",
                ex
            );
        }
    }
}

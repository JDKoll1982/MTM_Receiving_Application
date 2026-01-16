using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for GetInitialShipmentDataQuery - returns current date and next shipment number.
/// </summary>
public class GetInitialShipmentDataQueryHandler : IRequestHandler<GetInitialShipmentDataQuery, Model_Dao_Result<InitialShipmentData>>
{
    private readonly Dao_VolvoShipment _shipmentDao;

    public GetInitialShipmentDataQueryHandler(Dao_VolvoShipment shipmentDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
    }

    public async Task<Model_Dao_Result<InitialShipmentData>> Handle(GetInitialShipmentDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get next shipment number from database
            var nextNumberResult = await _shipmentDao.GetNextShipmentNumberAsync();

            if (!nextNumberResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<InitialShipmentData>(
                    nextNumberResult.ErrorMessage ?? "Failed to retrieve next shipment number");
            }

            var data = new InitialShipmentData
            {
                CurrentDate = DateTimeOffset.Now,
                NextShipmentNumber = nextNumberResult.Data
            };

            return Model_Dao_Result_Factory.Success(data);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<InitialShipmentData>(
                $"Unexpected error getting initial shipment data: {ex.Message}", ex);
        }
    }
}

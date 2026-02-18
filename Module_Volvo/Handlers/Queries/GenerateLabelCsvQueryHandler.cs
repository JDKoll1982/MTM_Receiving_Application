using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Helpers;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// [STUB] Handler for label generation - generates labels for shipment.
/// TODO: Implement database-backed label generation.
/// </summary>
public class GenerateLabelQueryHandler : IRequestHandler<GenerateLabelQuery, Model_Dao_Result<string>>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;
    private readonly IService_VolvoAuthorization _authService;
    private readonly IService_LoggingUtility _logger;

    public GenerateLabelQueryHandler(
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

    public async Task<Model_Dao_Result<string>> Handle(GenerateLabelQuery request, CancellationToken cancellationToken)
    {
        return await Helper_VolvoShipmentCalculations.GenerateLabelAsync(
            _shipmentDao,
            _lineDao,
            _partDao,
            _componentDao,
            _authService,
            _logger,
            request.ShipmentId);
    }
}

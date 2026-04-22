using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Rebuilds the generated Volvo label queue rows for one shipment from its current saved lines.
/// </summary>
public record SyncVolvoGeneratedLabelDataCommand : IRequest<Model_Dao_Result<int>>
{
    public int ShipmentId { get; init; }
}

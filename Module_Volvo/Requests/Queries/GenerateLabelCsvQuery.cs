using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to generate CSV label file for a shipment.
/// </summary>
public record GenerateLabelCsvQuery : IRequest<Model_Dao_Result<string>>
{
    /// <summary>
    /// Shipment ID to generate labels for.
    /// </summary>
    public int ShipmentId { get; init; }
}

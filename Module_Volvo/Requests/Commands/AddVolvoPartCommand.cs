using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to add a new Volvo part to master data.
/// </summary>
public record AddVolvoPartCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Part number to add.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>
    /// Quantity per skid.
    /// </summary>
    public int QuantityPerSkid { get; init; }
}

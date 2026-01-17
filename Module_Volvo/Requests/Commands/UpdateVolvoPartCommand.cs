using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to update an existing Volvo part.
/// </summary>
public record UpdateVolvoPartCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Part number to update.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>
    /// Updated quantity per skid.
    /// </summary>
    public int QuantityPerSkid { get; init; }
}

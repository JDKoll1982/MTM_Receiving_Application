using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to activate a Volvo part.
/// </summary>
public record ActivateVolvoPartCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Part number to activate.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;
}

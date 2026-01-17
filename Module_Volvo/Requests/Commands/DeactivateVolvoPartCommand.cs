using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to deactivate a Volvo part.
/// </summary>
public record DeactivateVolvoPartCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Part number to deactivate.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;
}

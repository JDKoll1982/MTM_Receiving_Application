using FluentValidation;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Clears queue ownership so another handler can take the line.
/// </summary>
/// <param name="WaitlistId"></param>
/// <param name="CurrentUserId"></param>
/// <param name="CurrentUserDisplayName"></param>
public sealed record Command_CustomerPullPackUnassignOwner(
    string WaitlistId,
    string CurrentUserId,
    string CurrentUserDisplayName
) : IRequest<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>>;

/// <summary>
/// Validates explicit owner-unassignment requests.
/// </summary>
public sealed class Command_CustomerPullPackUnassignOwnerValidator
    : AbstractValidator<Command_CustomerPullPackUnassignOwner>
{
    public Command_CustomerPullPackUnassignOwnerValidator()
    {
        RuleFor(command => command.WaitlistId).NotEmpty();
        RuleFor(command => command.CurrentUserId).NotEmpty();
        RuleFor(command => command.CurrentUserDisplayName).NotEmpty();
    }
}

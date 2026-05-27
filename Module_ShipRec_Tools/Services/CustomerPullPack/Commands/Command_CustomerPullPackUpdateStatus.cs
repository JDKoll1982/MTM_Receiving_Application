using FluentValidation;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Updates the current status, owner assignment, and handler notes for a queue item.
/// </summary>
public sealed record Command_CustomerPullPackUpdateStatus(
    string WaitlistId,
    Enum_CustomerPullPackWaitlistStatus NewStatus,
    string CurrentUserId,
    string CurrentUserDisplayName,
    Enum_CustomerPullPackProblemReason ProblemReason = Enum_CustomerPullPackProblemReason.None,
    string HandlerNote = ""
) : IRequest<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>>;

/// <summary>
/// Validates queue status updates.
/// </summary>
public sealed class Command_CustomerPullPackUpdateStatusValidator
    : AbstractValidator<Command_CustomerPullPackUpdateStatus>
{
    public Command_CustomerPullPackUpdateStatusValidator()
    {
        RuleFor(command => command.WaitlistId).NotEmpty();
        RuleFor(command => command.CurrentUserId).NotEmpty();
        RuleFor(command => command.CurrentUserDisplayName).NotEmpty();

        RuleFor(command => command)
            .Must(command =>
                command.NewStatus != Enum_CustomerPullPackWaitlistStatus.Problem
                || command.ProblemReason != Enum_CustomerPullPackProblemReason.None
                || string.IsNullOrWhiteSpace(command.HandlerNote) is false
            )
            .WithMessage("Problem status requires a preset reason or a handler note.");
    }
}

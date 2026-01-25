using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Navigate to a specific step in the workflow.
    /// Validates current step before allowing navigation forward.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="TargetStep">The target step number (1, 2, or 3)</param>
    /// <param name="IsEditMode">Whether navigating in edit mode from review screen</param>
    public record Command_Receiving_Wizard_Navigation_GoToStep(
        Guid SessionId,
        int TargetStep,
        bool IsEditMode = false
    ) : IRequest<Result>;
}

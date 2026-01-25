using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Initialize a new receiving workflow session.
    /// </summary>
    /// <param name="Mode">The workflow mode: "Guided" or "Manual"</param>
    public record Command_Receiving_Wizard_Navigation_StartNewWorkflow(
        string Mode
    ) : IRequest<Result<Guid>>;
}

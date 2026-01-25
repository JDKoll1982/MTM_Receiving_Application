using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Queries
{
    /// <summary>
    /// Retrieve current workflow session state.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    public record Query_Receiving_Wizard_Get_CurrentSession(
        Guid SessionId
    ) : IRequest<Result<ReceivingWorkflowSession>>;
}

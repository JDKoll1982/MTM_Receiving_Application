using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Queries
{
    /// <summary>
    /// Retrieve all load details for a session.
    /// Returns loads ordered by LoadNumber.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    public record Query_Receiving_Wizard_Get_AllLoadDetails(
        Guid SessionId
    ) : IRequest<Result<List<Model_Receiving_Entity_LoadDetail>>>;
}

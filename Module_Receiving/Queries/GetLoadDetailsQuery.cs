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
    public record GetLoadDetailsQuery(
        Guid SessionId
    ) : IRequest<Result<List<LoadDetail>>>;
}

using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Change which load is the copy source.
    /// Updates session.CopySourceLoadNumber.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="NewSourceLoadNumber">The new source load number (1 to LoadCount)</param>
    public record ChangeCopySourceCommand(
        Guid SessionId,
        int NewSourceLoadNumber
    ) : IRequest<Result>;
}

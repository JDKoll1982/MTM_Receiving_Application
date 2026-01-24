using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Force overwrite occupied cells (power user feature).
    /// Requires explicit user confirmation. Logs overwrite operations for audit.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="SourceLoadNumber">The load number to copy from</param>
    /// <param name="TargetLoadNumbers">The loads to overwrite (must be specified)</param>
    /// <param name="FieldsToOverwrite">Which fields to overwrite</param>
    /// <param name="Confirmed">User confirmation flag (must be true to execute)</param>
    public record ForceOverwriteCommand(
        Guid SessionId,
        int SourceLoadNumber,
        List<int> TargetLoadNumbers,
        CopyFields FieldsToOverwrite,
        bool Confirmed
    ) : IRequest<Result<CopyOperationResult>>;
}

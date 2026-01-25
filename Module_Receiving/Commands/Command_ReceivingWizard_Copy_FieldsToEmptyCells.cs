using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Copy data from source load to target loads (empty cells only).
    /// Never overwrites existing data. Sets auto-fill flags for copied cells.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="SourceLoadNumber">The load number to copy from</param>
    /// <param name="TargetLoadNumbers">The loads to copy to (empty list = all loads except source)</param>
    /// <param name="FieldsToCopy">Which fields to copy</param>
    public record Command_Receiving_Wizard_Copy_FieldsToEmptyCells(
        Guid SessionId,
        int SourceLoadNumber,
        List<int> TargetLoadNumbers,
        CopyFields FieldsToCopy
    ) : IRequest<Result<CopyOperationResult>>;
}

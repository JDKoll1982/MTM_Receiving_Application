using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Queries
{
    /// <summary>
    /// Preview what a copy operation will do.
    /// Shows cells to be copied, preserved, and conflicts.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="SourceLoadNumber">The load number to copy from</param>
    /// <param name="TargetLoadNumbers">The loads to copy to</param>
    /// <param name="FieldsToCopy">Which fields to copy</param>
    public record Query_ReceivingWizard_Preview_CopyOperation(
        Guid SessionId,
        int SourceLoadNumber,
        List<int> TargetLoadNumbers,
        CopyFields FieldsToCopy
    ) : IRequest<Result<CopyPreview>>;
}

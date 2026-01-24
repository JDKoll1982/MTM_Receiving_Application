using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Clear auto-filled data from loads.
    /// Only clears fields where auto-fill flag is true. Never clears manually entered data.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="TargetLoadNumbers">The loads to clear (empty list = all loads)</param>
    /// <param name="FieldsToClear">Which fields to clear</param>
    public record ClearAutoFilledDataCommand(
        Guid SessionId,
        List<int> TargetLoadNumbers,
        CopyFields FieldsToClear
    ) : IRequest<Result>;
}

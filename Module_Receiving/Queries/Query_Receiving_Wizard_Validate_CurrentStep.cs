using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Queries
{
    /// <summary>
    /// Get validation status for current or target step.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="Step">The step number to validate (1, 2, or 3)</param>
    public record Query_Receiving_Wizard_Validate_CurrentStep(
        Guid SessionId,
        int Step
    ) : IRequest<Result<ValidationStatus>>;

    /// <summary>
    /// Represents the validation status of a workflow step.
    /// </summary>
    /// <param name="IsValid">Whether the step is valid</param>
    /// <param name="Errors">List of validation errors</param>
    /// <param name="ErrorCount">Number of errors</param>
    /// <param name="WarningCount">Number of warnings</param>
    public record ValidationStatus(
        bool IsValid,
        List<ValidationError> Errors,
        int ErrorCount,
        int WarningCount
    );
}

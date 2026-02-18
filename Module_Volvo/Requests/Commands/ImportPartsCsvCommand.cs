using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// [STUB] Command to bulk import parts data.
/// TODO: Implement database import operation.
/// </summary>
public record ImportPartsCommand : IRequest<Model_Dao_Result<ImportPartsResult>>
{
    /// <summary>
    /// Full file path to data file containing part data.
    /// </summary>
    public string FilePath { get; init; } = string.Empty;
}

/// <summary>
/// Result of import operation with success/failure counts and error details.
/// </summary>
public record ImportPartsResult
{
    /// <summary>
    /// Number of parts successfully imported.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of parts that failed validation or import.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// List of error messages for failed imports (includes row number and reason).
    /// </summary>
    public List<string> Errors { get; init; } = new();
}
